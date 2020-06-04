using System;

namespace Sample.Contracts.Events
{
    public interface OrderFulfillmentFaulted
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
    }
}
