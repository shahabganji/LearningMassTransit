using System;

namespace Sample.Contracts.Events
{
    public interface CustomerAccountClosedEvent
    {
        Guid CustomerId { get; }
        string CustomerNumber { get; }
    }
}
