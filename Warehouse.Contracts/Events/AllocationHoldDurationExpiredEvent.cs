using System;

namespace Warehouse.Contracts.Events
{
    public interface AllocationHoldDurationExpiredEvent
    {
        Guid AllocationId { get; }
    }
}
