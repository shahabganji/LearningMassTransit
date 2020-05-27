using System;

namespace Sample.Contracts
{
    public interface CheckOrderEvent
    {
        public Guid OrderId { get; }
    }

    public interface OrderNotFound
    {
        public Guid OrderId { get; }
    }
    

    public interface OrderStatus
    {
        public Guid OrderId { get; set; }
        public string State { get; set; }
    }
}
