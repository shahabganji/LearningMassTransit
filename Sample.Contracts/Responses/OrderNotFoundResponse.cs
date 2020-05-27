using System;

namespace Sample.Contracts.Responses
{
    public interface OrderNotFoundResponse
    {
        public Guid OrderId { get; }
    }
}
