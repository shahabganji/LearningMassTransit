using System;

namespace Sample.Contracts.Events
{
    public interface OrderStatus
    {
        public Guid OrderId { get; set; }
        public string State { get; set; }
    }
}