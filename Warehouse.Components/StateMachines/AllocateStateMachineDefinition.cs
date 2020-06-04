using GreenPipes;
using MassTransit;
using MassTransit.Definition;

namespace Warehouse.Components.StateMachines
{
    public sealed class AllocateStateMachineDefinition
        : SagaDefinition<AllocationState>
    {
        public AllocateStateMachineDefinition()
        {
            this.ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
            ISagaConfigurator<AllocationState> sagaConfigurator)
        {
            // to resolve the duplicate key error, AllocationCreated, ReleaseRequested
            endpointConfigurator.UseMessageRetry(configurator => configurator.Interval(3, 1000));
            
            // this holds the messages until the persistence part of those messages completed
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
