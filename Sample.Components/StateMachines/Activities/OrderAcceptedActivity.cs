using System;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using Sample.Contracts.Events;

namespace Sample.Components.StateMachines.Activities
{
    public sealed class OrderAcceptedActivity 
        : Activity<OrderState,OrderAcceptedEvent>
    {
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
            Console.WriteLine("Execute order accepted activity: {0}" , 
                context.Data.OrderId);
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, OrderAcceptedEvent, TException> context, Behavior<OrderState, OrderAcceptedEvent> next) where TException : Exception 
            => next.Faulted(context);
    }
}
