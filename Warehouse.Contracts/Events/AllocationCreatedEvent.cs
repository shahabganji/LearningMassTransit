using System;

namespace Warehouse.Contracts.Events
{
    public interface AllocationCreatedEvent
    {
        Guid AllocationId { get; }
        TimeSpan HoldDuration { get; }
    }
}
