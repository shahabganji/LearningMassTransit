using System;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using Sample.Contracts.Events;

namespace Sample.Components.StateMachines.Activities
{
    public sealed class CustomerDeletedActivity 
        : Activity<OrderState,CustomerAccountClosedEvent>
    {
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
            Console.WriteLine("Execute order cancelled activity: {0} for customer {1}" , 
                context.Instance.CorrelationId,
                context.Data.CustomerNumber);
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, CustomerAccountClosedEvent, TException> context, Behavior<OrderState, CustomerAccountClosedEvent> next) where TException : Exception 
            => next.Faulted(context);
    }
}
