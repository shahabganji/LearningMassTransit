using System;

namespace Sample.Contracts.Events
{
    public interface OrderAcceptedEvent
    {
        public Guid OrderId { get; }
        public DateTime Timestamp { get;  }
    }
}