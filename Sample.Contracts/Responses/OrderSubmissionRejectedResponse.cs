using System;

namespace Sample.Contracts.Responses
{
    public interface OrderSubmissionRejectedResponse 
    {
        public Guid OrderId { get;  }
        public DateTime Timestamp { get;  }
        public string Customer { get; }
        public string Reason { get; set; }
    }
}
