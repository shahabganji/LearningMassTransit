using System;

namespace Warehouse.Contracts.Commands
{
    public interface AllocateInventoryCommand
    {
        Guid AllocationId { get; }
        
        string ItemNumber { get; }
        decimal Quantity { get; }
    }
}
