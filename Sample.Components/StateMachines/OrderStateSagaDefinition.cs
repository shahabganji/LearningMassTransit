using GreenPipes;
using MassTransit;
using MassTransit.Definition;

namespace Sample.Components.StateMachines
{
    public class OrderStateSagaDefinition
        : SagaDefinition<OrderState>
    {
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
            ISagaConfigurator<OrderState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 5000, 10000));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}