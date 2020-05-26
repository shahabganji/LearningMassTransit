using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace Sample.Service
{
    public class MassTransitConsoleWorker : BackgroundService
    {
        private readonly IBusControl _bus;
        public MassTransitConsoleWorker(IBusControl bus)
            => _bus = bus;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bus.StartAsync(stoppingToken).ConfigureAwait(false);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _bus.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
