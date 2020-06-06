using System;
using System.Diagnostics.CodeAnalysis;
using Automatonymous;
using MassTransit;
using Sample.Components.StateMachines.Activities;
using Sample.Contracts.Commands;
using Sample.Contracts.Events;
using Sample.Contracts.Responses;

namespace Sample.Components.StateMachines
{
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    public sealed class OrderStateMachine
        : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);
            
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => FulfillmentFaulted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => FulfillmentCompleted, x => x.CorrelateById(m => m.Message.OrderId));
            
            Event(() => FulfillOrderCommandFaulted, x => x.CorrelateById(m => m.Message.Message.OrderId));
            
            Event(() => AccountClosed,
                x => x.CorrelateBy((saga, context) => saga.CustomerNumber == context.Message.CustomerNumber));
            Event(() => OrderStatusRequested,
                x =>
                {
                    x.CorrelateById(m => m.Message.OrderId);
                    x.OnMissingInstance(a => a.ExecuteAsync(async context =>
                    {
                        await context.RespondAsync<OrderNotFoundResponse>(new {context.Message.OrderId});
                    }));
                });
            
            Initially(
                When(OrderSubmitted)
                    // copies the customer name from data/message/event to the instance of the saga
                    .Then(context =>
                    {
                        if (context.Data.Timestamp.HasValue)
                            context.Instance.SubmitDate = context.Data.Timestamp;
                        if (!string.IsNullOrEmpty(context.Data.CustomerNumber))
                            context.Instance.CustomerNumber = context.Data.CustomerNumber;

                        context.Instance.PaymentCardNumber = context.Data.PaymentCardNumber;
                        
                        context.Instance.Updated = DateTime.UtcNow;
                    })
                    .TransitionTo(Submitted)
            );

            During(Submitted,
                Ignore(OrderSubmitted),
                When(AccountClosed)
                    .Activity(x => x.OfType<CustomerAccountClosedActivity>())
                    .TransitionTo(Cancelled),
                When(OrderAccepted)
                    .Activity(x => x.OfType<OrderAcceptedActivity>())
                    .TransitionTo(Accepted));

            During(Accepted, 
                When(FulfillmentFaulted)
                    .TransitionTo(Faulted),
                When(FulfillmentCompleted)
                    .TransitionTo(Completed),
                When( FulfillOrderCommandFaulted)
                    .TransitionTo(Faulted));
            
            DuringAny(
                When(OrderStatusRequested)
                    .RespondAsync(x => x.Init<OrderStatusResponse>(new
                    {
                        OrderId = x.Instance.CorrelationId,
                        State = x.Instance.CurrentState
                    })));

            // during any event, except initial & final,
            // if an OrderSubmitted event fired then do the following
            DuringAny(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        context.Instance.SubmitDate ??= context.Data.Timestamp;
                        context.Instance.CustomerNumber ??= context.Data.CustomerNumber;
                    }));
        }

        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Cancelled { get; private set;}
        public State Faulted { get;private set; }
        public State Completed { get;private set; }

        public Event<CustomerAccountClosedEvent> AccountClosed { get; private set; }
        public Event<OrderSubmittedEvent> OrderSubmitted { get;private set; }
        public Event<OrderAcceptedEvent> OrderAccepted { get; private set; }
        public Event<CheckOrderRequestedEvent> OrderStatusRequested { get; private set;}
        public Event<OrderFulfillmentFaulted> FulfillmentFaulted { get; private set;}
        public Event<OrderFulfillmentCompleted> FulfillmentCompleted { get; private set;}
        
        public Event<Fault<FulfillOrderCommand>> FulfillOrderCommandFaulted { get; private set; }
    }
}
