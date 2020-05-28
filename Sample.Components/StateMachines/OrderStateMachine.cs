using System;
using Automatonymous;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using MassTransit.RedisIntegration;
using Sample.Contracts.Events;
using Sample.Contracts.Responses;

namespace Sample.Components.StateMachines
{
    public sealed class OrderStateMachine
        : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => OrderSubmitted,
                x => x.CorrelateById(m => m.Message.OrderId));
            
            Event(() => OrderStatusRequested,
                x =>
                {
                    x.CorrelateById(m => m.Message.OrderId);
                    x.OnMissingInstance(a => a.ExecuteAsync(async context =>
                    {
                        await context.RespondAsync<OrderNotFoundResponse>(new {context.Message.OrderId});
                    }));
                });

            InstanceState(x => x.CurrentState);

            Initially(
                When(OrderSubmitted)
                    // copies the customer name from data/message/event to the instance of the saga
                    .Then(context =>
                    {
                        context.Instance.SubmitDate = context.Data.Timestamp;
                        context.Instance.CustomerNumber = context.Data.CustomerName;
                        context.Instance.Updated = DateTime.UtcNow;
                    })
                    .TransitionTo(Submitted)
            );
            
            // will say why this is significant later on.
            During(Submitted, Ignore(OrderSubmitted));

            DuringAny(When(OrderStatusRequested)
                .RespondAsync(x=>x.Init<OrderStatusResponse>(new
                {
                    OrderId = x.Instance.CorrelationId,
                    State = x.Instance.CurrentState
                })));
            
            // during any event, except initial & final,
            // if an OrderSubmitted event fired then do the following
            DuringAny(When(OrderSubmitted).Then(context =>
            {
                context.Instance.SubmitDate ??= context.Data.Timestamp;
                context.Instance.CustomerNumber ??= context.Data.CustomerName;
            }));
        }

        public State Submitted { get; set; }

        public Event<OrderSubmittedEvent> OrderSubmitted { get; private set; }
        public Event<CheckOrderRequestedEvent> OrderStatusRequested { get; private set; }
    }

    public class OrderState
        : SagaStateMachineInstance
            , IVersionedSaga
    {
        public int Version { get; set; }
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } // current state of the saga

        // data to be stored
        public string CustomerNumber { get; set; }
        public DateTime Updated { get; set; }
        public DateTime? SubmitDate { get; set; }
    }

    public class OrderStateSagaDefinition
        : SagaDefinition<OrderState>
    {
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500,5000,10000));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
    
}
