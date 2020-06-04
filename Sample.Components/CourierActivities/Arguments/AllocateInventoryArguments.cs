using System;

namespace Sample.Components.CourierActivities.Arguments
{
    public interface AllocateInventoryArguments
    {
        Guid OrderId { get; }
        
        string ItemNumber { get; }
        decimal Quantity { get; }
    }
}
