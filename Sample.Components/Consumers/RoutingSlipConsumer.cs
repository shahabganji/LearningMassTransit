using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;

namespace Sample.Components.Consumers
{
    public sealed class RoutingSlipConsumer : 
        IConsumer<RoutingSlipCompleted>,
        IConsumer<RoutingSlipFaulted>,
        IConsumer<RoutingSlipActivityCompleted>
    {
        private readonly ILogger<RoutingSlipConsumer> _logger;

        public RoutingSlipConsumer(ILogger<RoutingSlipConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<RoutingSlipCompleted> context)
        {
            if(_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information, "Routing Slip Completed, {TrackingNumber}" , 
                    context.Message.TrackingNumber);

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoutingSlipActivityCompleted> context)
        {
            if(_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information, "Routing Slip ACTIVITY Completed, {TrackingNumber} {ActivityName}" , 
                    context.Message.TrackingNumber , context.Message.ActivityName);

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoutingSlipFaulted> context)
        {
            if(_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information, "Routing Slip FAULTED Completed, {TrackingNumber} {ExceptionInfo}" , 
                    context.Message.TrackingNumber , context.Message.ActivityExceptions.FirstOrDefault());

            return Task.CompletedTask;
        }
    }
}
