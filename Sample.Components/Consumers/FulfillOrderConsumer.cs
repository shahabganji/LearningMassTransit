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
            
            builder.AddActivity("Payment",
                new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.SanitizeName("payment")}_execute"),
                new
                {
                    Amount = 99.95m , 
                    CardNumber = "5999-1234-5678-9012"
                });

            // implicit setting the OrderId property on all activities that have that value in their argument type 
            builder.AddVariable("OrderId", context.Message.OrderId);

            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }
}
