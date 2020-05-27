using System;

namespace Sample.Contracts.Responses
{
    public interface OrderNotFound
    {
        public Guid OrderId { get; }
    }
}
