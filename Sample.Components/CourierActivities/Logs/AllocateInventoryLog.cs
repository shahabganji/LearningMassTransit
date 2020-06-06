using System;

namespace Sample.Components.CourierActivities.Logs
{
    public interface AllocateInventoryLog
    {
        Guid AllocationId { get; }
    }
}
