using System.Threading.Tasks;
using MassTransit;
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
                CustomerNumber = context.Message.Customer,
                context.Message.PaymentCardNumber
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
}
