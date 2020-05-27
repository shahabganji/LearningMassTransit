using System;

namespace Sample.Contracts.Events
{
    public interface CheckOrderRequestedEvent
    {
        public Guid OrderId { get; }
    }
}
