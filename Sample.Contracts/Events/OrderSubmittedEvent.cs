using System;

namespace Sample.Contracts
{
    public interface OrderSubmittedEvent
    {
        public Guid OrderId { get; }
        public DateTime Timestamp { get;  }
        public string CustomerName { get; set; }
    }
}
