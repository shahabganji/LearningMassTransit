using System;

namespace Sample.Contracts.Events
{
    public interface CheckOrderEvent
    {
        public Guid OrderId { get; }
    }
}
