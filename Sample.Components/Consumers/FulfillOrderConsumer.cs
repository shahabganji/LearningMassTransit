using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
using MassTransit.Definition;
using Sample.Contracts.Commands;
using Sample.Contracts.Events;

namespace Sample.Components.Consumers
{
    public sealed class FulfillOrderConsumer
        : IConsumer<FulfillOrderCommand>
    {
        private static readonly Random Randomizer = new Random();
        
        // here we create the routing slip 
        public async Task Consume(ConsumeContext<FulfillOrderCommand> context)
        {
            if( context.Message.CustomerNumber.StartsWith("INVALID") )
                throw new InvalidOperationException("We tried, but the customer is invalid");
            
            if( context.Message.CustomerNumber.StartsWith("MAYBE") && Randomizer.Next(100) > 50 )
                throw  new ApplicationException("We randomly exploded, so sad, much tear!");
            
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            const string allocateInventory = "AllocateInventory";
            var queue = $"queue:{KebabCaseEndpointNameFormatter.Instance.SanitizeName(allocateInventory)}_execute";

            builder.AddActivity(allocateInventory, new Uri(queue), new
            {
                ItemNumber = "Item123",
                Quantity = 10.0m
            });

            builder.AddActivity("PaymentActivity",
                new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.SanitizeName("payment")}_execute"),
                new
                {
                    Amount = 99.95m,
                    CardNumber =
                        context.Message.PaymentCardNumber ?? "5999-1234-5678-9012" // pertain the previous behavior
                });

            await builder.AddSubscription(context.SourceAddress,
                RoutingSlipEvents.Faulted | RoutingSlipEvents.Supplemental,
                RoutingSlipEventContents.None,
                x => x.Send<OrderFulfillmentFaulted>(new
                {
                    context.Message.OrderId, InVar.Timestamp
                }));
            
            await builder.AddSubscription(context.SourceAddress,
                RoutingSlipEvents.Completed | RoutingSlipEvents.Supplemental,
                RoutingSlipEventContents.None,
                x => x.Send<OrderFulfillmentCompleted>(new
                {
                    context.Message.OrderId, InVar.Timestamp
                }));

            // implicit setting the OrderId property on all activities that have that value in their argument type 
            builder.AddVariable("OrderId", context.Message.OrderId);

            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }
}
