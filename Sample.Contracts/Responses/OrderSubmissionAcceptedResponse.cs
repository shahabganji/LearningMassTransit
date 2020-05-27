using System;

namespace Sample.Contracts.Responses
{
    public interface OrderSubmissionAcceptedResponse
    {
        public Guid OrderId { get;  }
        public DateTime Timestamp { get;  }
        public string Customer { get; }
    }
}
