using System;

namespace Sample.Contracts.Events
{
    public interface OrderSubmittedEvent
    {
        public Guid OrderId { get; }
        public DateTime Timestamp { get;  }
        public string CustomerName { get; set; }
    }
}
