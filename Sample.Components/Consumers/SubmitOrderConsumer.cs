using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;

namespace Sample.Components.Consumers
{
    // todo: add a cache decorator on top of this
    public sealed class SubmitOrderConsumer : IConsumer<ISubmitOrder>
    {
        private readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ISubmitOrder> context)
        {
            _logger.LogDebug(
                "SubmitOrderConsumer, {CustomerNumber}", context.Message.Customer);
            if (context.Message.Customer.Contains("TEST"))
            {
                if (context.ResponseAddress != null)
                    await context.RespondAsync<IOrderSubmissionRejected>(new
                    {
                        InVar.Timestamp,
                        context.Message.Customer,
                        context.Message.OrderId,
                        Reason = $"Test Customer cannot submit order: {context.Message.Customer}"
                    });

                return;
            }

            if (context.RequestId != null)
                await context.RespondAsync<IOrderSubmissionAccepted>(new
                {
                    InVar.Timestamp,
                    context.Message.Customer,
                    context.Message.OrderId
                }).ConfigureAwait(false);
        }
    }
}
