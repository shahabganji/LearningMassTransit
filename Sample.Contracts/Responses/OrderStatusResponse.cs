using System;

namespace Sample.Contracts.Responses
{
    public interface OrderStatusResponse
    {
        public Guid OrderId { get; set; }
        public string State { get; set; }
    }
}
