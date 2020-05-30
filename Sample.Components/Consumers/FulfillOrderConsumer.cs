using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Definition;
using Sample.Contracts.Commands;

namespace Sample.Components.Consumers
{
    public sealed class FulfillOrderConsumer
        : IConsumer<FulfillOrderCommand>
    {
        // here we create the routing slip 
        public async Task Consume(ConsumeContext<FulfillOrderCommand> context)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            const string allocateInventory = "AllocateInventory";
            var queue = $"queue:{KebabCaseEndpointNameFormatter.Instance.SanitizeName(allocateInventory)}_execute";

            builder.AddActivity(allocateInventory, new Uri(queue), new
            {
                ItemNumber = "Item123",
                Quantity = 10.0m
            });

            builder.AddVariable("OrderId", context.Message.OrderId);

            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }
}
