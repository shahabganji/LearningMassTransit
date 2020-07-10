using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Sample.Platform
{
    public sealed class SampleConsumer  :IConsumer<SampleCommand>
    {
        private readonly ILogger<SampleConsumer> _logger;

        public SampleConsumer(ILogger<SampleConsumer> logger)
        {
            _logger = logger;
        }
        
        public Task Consume(ConsumeContext<SampleCommand> context)
        {
            _logger.LogDebug("By your command: {Command}" , context.Message.Command);
            return Task.CompletedTask;
        }
    }
}
