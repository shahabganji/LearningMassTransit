using System;

namespace Sample.Contracts
{
    public interface OrderSubmissionRejectedResponse 
    {
        public Guid OrderId { get;  }
        public DateTime Timestamp { get;  }
        public string Customer { get; }
        public string Reason { get; set; }
    }
}