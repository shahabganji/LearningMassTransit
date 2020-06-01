using System;

namespace Warehouse.Contracts.Events
{
    public interface AllocationReleaseRequestedEvent
    {
        Guid AllocationId { get; }
        string Reason { get; }
    }
}
