using System.Threading.Tasks;
using MassTransit;
using Warehouse.Contracts;
using Warehouse.Contracts.Responses;

namespace Warehouse.Components.Consumers
{
    public class AllocateInventoryConsumer 
        : IConsumer<AllocateInventory>
    {
        public async Task Consume(ConsumeContext<AllocateInventory> context)
        {
            await Task.Delay(200);
            
            await context.RespondAsync<InventoryAllocatedResponse>(new
            {
                context.Message.AllocationId,
                context.Message.ItemNumber,
                context.Message.Quantity
            });
        }
    }
}
