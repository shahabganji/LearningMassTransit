using System;

namespace Sample.Contracts
{
    public interface OrderSubmissionAcceptedResponse
    {
        public Guid OrderId { get;  }
        public DateTime Timestamp { get;  }
        public string Customer { get; }
    }
}