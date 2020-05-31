using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using Microsoft.Extensions.Logging;
using Sample.Contracts.Commands;
using Sample.Contracts.Events;
using Sample.Contracts.Responses;

namespace Sample.Components.Consumers
{
    // todo: add a cache decorator on top of this
    public sealed class SubmitOrderConsumer : IConsumer<SubmitOrderCommand>
    {
        private readonly ILogger<SubmitOrderConsumer> _logger;
        
        public SubmitOrderConsumer(){}
        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SubmitOrderCommand> context)
        {
            _logger?.LogDebug("SubmitOrderConsumer, {CustomerNumber}",
                context.Message.Customer);
            if (context.Message.Customer.Contains("TEST"))
            {
                if (context.ResponseAddress != null)
                    await context.RespondAsync<OrderSubmissionRejectedResponse>(new
                    {
                        InVar.Timestamp,
                        context.Message.Customer,
                        context.Message.OrderId,
                        Reason = $"Test Customer cannot submit order: {context.Message.Customer}"
                    });

                return;
            }
            
            await context.Publish<OrderSubmittedEvent>(new
            {
                context.Message.OrderId,
                InVar.Timestamp,
                CustomerNumber = context.Message.Customer
            });

            if (context.RequestId != null)
                await context.RespondAsync<OrderSubmissionAcceptedResponse>(new
                {
                    InVar.Timestamp,
                    context.Message.Customer,
                    context.Message.OrderId
                }).ConfigureAwait(false);
        }
    }

    public class SubmitOrderConsumerDefinition
        : ConsumerDefinition<SubmitOrderConsumer>
    {
        // public SubmitOrderConsumerDefinition()
        // {
        //     EndpointDefinition = new ConsumerEndpointDefinition<SubmitOrderConsumer>(new EndpointSettings<IEndpointDefinition<SubmitOrderConsumer>>
        //     {
        //         IsTemporary = true, 
        //         Name =$"{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}{Guid.NewGuid()}" ,
        //     });
        // }
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseInMemoryOutbox();
            endpointConfigurator.UseMessageRetry(r => r.Interval(3,1000));
        }
    }
}
