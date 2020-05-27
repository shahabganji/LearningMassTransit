using System;

namespace Sample.Contracts.Commands
{
    public interface SubmitOrderCommand
    {
        public Guid OrderId { get;}
        public DateTime Timestamp { get;  }
        public string Customer { get;  }
    }
}
