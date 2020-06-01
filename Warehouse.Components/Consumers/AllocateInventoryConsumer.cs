using System.Threading.Tasks;
using MassTransit;
using Warehouse.Contracts.Commands;
using Warehouse.Contracts.Events;
using Warehouse.Contracts.Responses;

namespace Warehouse.Components.Consumers
{
    public class AllocateInventoryConsumer 
        : IConsumer<AllocateInventoryCommand>
    {
        public async Task Consume(ConsumeContext<AllocateInventoryCommand> context)
        {
            await Task.Delay(200);

            // starts the allocation saga
            await context.Publish<AllocationCreatedEvent>(new
            {
                context.Message.AllocationId,
                HoldDuration = 8000 // probably read from a configuration, in milliseconds
            });
            
            await context.RespondAsync<InventoryAllocatedResponse>(new
            {
                context.Message.AllocationId,
                context.Message.ItemNumber,
                context.Message.Quantity
            });
        }
    }
}
