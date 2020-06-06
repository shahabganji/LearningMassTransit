using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts.Commands;

namespace Sample.Components.Consumers
{
    public sealed class FulfillOrderCommandFaultedConsumer :
        IConsumer<Fault<FulfillOrderCommand>>
    {
        private readonly ILogger<FulfillOrderCommandFaultedConsumer> _logger;

        public FulfillOrderCommandFaultedConsumer(ILogger<FulfillOrderCommandFaultedConsumer> logger)
            => _logger = logger;

        public Task Consume(ConsumeContext<Fault<FulfillOrderCommand>> context)
        {
            _logger.LogInformation("Fulfill order command faulted due to {Reason}",
                context.Message.Exceptions.FirstOrDefault()?.Message);

            return Task.CompletedTask;
        }
    }
}
