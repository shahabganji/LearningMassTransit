using System;

namespace Sample.Contracts.Commands
{
    public interface FulfillOrderCommand
    {
        Guid OrderId { get; }
    }
}
