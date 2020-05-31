using System;
using System.Data;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using Warehouse.Contracts;
using Warehouse.Contracts.Responses;

namespace Sample.Components.CourierActivities
{
    public sealed class AllocateInventoryActivity
        : IActivity<AllocateInventoryArguments, AllocateInventoryLog>
    {
        private readonly IRequestClient<AllocateInventory> _client;

        public AllocateInventoryActivity(IRequestClient<AllocateInventory> client)
        {
            _client = client;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<AllocateInventoryArguments> context)
        {
            var orderId = context.Arguments.OrderId;

            var itemNumber = context.Arguments.ItemNumber;
            if( string.IsNullOrEmpty(itemNumber) )
                throw  new NoNullAllowedException($"{nameof(itemNumber)}");
            
            var quantity = context.Arguments.Quantity;
            if( quantity <= 0 )
                throw new ArgumentOutOfRangeException($"{nameof(quantity)}");

            var allocationId = NewId.NextGuid();
            
            var response = await _client.GetResponse<InventoryAllocatedResponse>(new
            {
                AllocationId = allocationId,
                ItemNumber = itemNumber,
                Quantity = quantity
            });
            
            // this publishes RoutingSlipCompleted event, thus IConsumer<RoutingSlipCompleted> can consume the message,
            // in this case, RoutingSlipConsumer
            return context.Completed<AllocateInventoryLog>(new {response.Message.AllocationId });
        }

        public async Task<CompensationResult> Compensate(CompensateContext<AllocateInventoryLog> context)
        {
            await context.Publish<AllocationReleaseRequested>(new
            {
                context.Log.AllocationId,
                Reason = "Order faulted"
            });

            return context.Compensated();
        }
    }
}
