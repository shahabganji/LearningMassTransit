using System;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using Microsoft.Extensions.Logging;
using Sample.Contracts.Events;

namespace Sample.Components.StateMachines.Activities
{
    public sealed class OrderAcceptedActivity 
        : Activity<OrderState,OrderAcceptedEvent>
    {
        private readonly ILogger<OrderAcceptedActivity> _logger;

        public OrderAcceptedActivity(ILogger<OrderAcceptedActivity> logger)
        {
            _logger = logger;
        }
        public void Probe(ProbeContext context)
        {
            context.CreateScope("order-accepted");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(
            BehaviorContext<OrderState, OrderAcceptedEvent> context,
            Behavior<OrderState, OrderAcceptedEvent> next)
        {
            _logger.LogInformation("Execute order accepted activity: {OrderId}" , 
                context.Data.OrderId);
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, OrderAcceptedEvent, TException> context, Behavior<OrderState, OrderAcceptedEvent> next) where TException : Exception 
            => next.Faulted(context);
    }
}
