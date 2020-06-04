using System;

namespace Sample.Components.CourierActivities.Arguments
{
    public interface PaymentArguments
    {
        Guid OrderId { get; }
        decimal Amount { get; }
        
        string CardNumber { get; }
    }
}
