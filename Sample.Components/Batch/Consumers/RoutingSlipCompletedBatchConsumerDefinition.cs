using System;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Courier.Contracts;
using MassTransit.Definition;

namespace Sample.Components.Batch.Consumers
{
    // public sealed class RoutingSlipCompletedBatchConsumerDefinition
    //     : ConsumerDefinition<RoutingSlipCompletedBatchConsumer>
    // {
    //     public RoutingSlipCompletedBatchConsumerDefinition()
    //     {
    //         this.ConcurrentMessageLimit = 20;
    //     }
    //
    //     protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<RoutingSlipCompletedBatchConsumer> consumerConfigurator)
    //     {
    //         endpointConfigurator.Batch<RoutingSlipCompleted>(b =>
    //         {
    //             b.MessageLimit = 11;
    //             b.TimeLimit = TimeSpan.FromSeconds(5);
    //         });
    //     }
    // }
}
