using System;

namespace Sample.Contracts
{
    public interface SubmitOrderCommand
    {
        public Guid OrderId { get;}
        public DateTime Timestamp { get;  }
        public string Customer { get;  }
    }
}
