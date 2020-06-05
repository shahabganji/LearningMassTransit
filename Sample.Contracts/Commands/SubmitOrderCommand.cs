using System;

namespace Sample.Contracts.Commands
{
    public interface SubmitOrderCommand
    {
         Guid OrderId { get;}
         DateTime Timestamp { get;  }
        string Customer { get;  }
        
         string PaymentCardNumber { get; }
    }
}
