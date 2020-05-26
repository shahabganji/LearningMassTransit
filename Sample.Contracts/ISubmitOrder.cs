using System;

namespace Sample.Contracts
{
    public interface ISubmitOrder
    {
        public Guid OrderId { get;}
        public DateTime Timestamp { get;  }
        public string Customer { get;  }
    }

    public interface IOrderSubmissionAccepted
    {
        public Guid OrderId { get;  }
        public DateTime Timestamp { get;  }
        public string Customer { get; }
    }
    
    public interface IOrderSubmissionRejected 
    {
        public Guid OrderId { get;  }
        public DateTime Timestamp { get;  }
        public string Customer { get; }
        public string Reason { get; set; }
    }

    
}
