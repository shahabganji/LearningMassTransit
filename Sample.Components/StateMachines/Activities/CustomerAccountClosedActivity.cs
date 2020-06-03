using System;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using Sample.Contracts.Events;

namespace Sample.Components.StateMachines.Activities
{
    public sealed class CustomerAccountClosedActivity
        : Activity<OrderState, CustomerAccountClosedEvent>
    {
        // private readonly ILogger<CustomerAccountClosedActivity> _logger;

        // public CustomerAccountClosedActivity(ILogger<CustomerAccountClosedActivity> logger)
        // {
        //     _logger = logger;
        // }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("order-cancelled");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(
            BehaviorContext<OrderState, CustomerAccountClosedEvent> context,
            Behavior<OrderState, CustomerAccountClosedEvent> next)
        {
            // _logger
            //     .LogInformation("Execute order cancelled activity: {OrderId} for customer {CustomerNumber}" , 
            // context.Instance.CorrelationId,
            // context.Data.CustomerNumber);
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(
            BehaviorExceptionContext<OrderState, CustomerAccountClosedEvent, TException> context,
            Behavior<OrderState, CustomerAccountClosedEvent> next) where TException : Exception
            => next.Faulted(context);
    }
}
