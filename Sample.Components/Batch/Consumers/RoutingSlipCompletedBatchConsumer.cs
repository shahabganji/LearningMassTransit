using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;

namespace Sample.Components.Batch.Consumers
{
    public sealed class RoutingSlipCompletedBatchConsumer
        : IConsumer<Batch<RoutingSlipCompleted>>
    {
        private readonly ILogger<RoutingSlipCompletedBatchConsumer> _logger;

        public RoutingSlipCompletedBatchConsumer(ILogger<RoutingSlipCompletedBatchConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Batch<RoutingSlipCompleted>> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information, "Routing Slips Completed, {TrackingNumbers}",
                    string.Join(", ", context.Message.Select(m => m.Message.TrackingNumber)));
            return Task.CompletedTask;
        }
    }
}
