using System;

namespace Warehouse.Contracts.Responses
{
    public interface InventoryAllocatedResponse
    {
        Guid AllocationId { get; }
        
        string ItemNumber { get; }
        decimal Quantity { get; }
    }
}
