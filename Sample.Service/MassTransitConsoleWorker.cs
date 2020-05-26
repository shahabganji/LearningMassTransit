using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sample.Service
{
    public class MassTransitConsoleWorker : BackgroundService
    {
        private readonly IBusControl _bus;
        private readonly ILogger<MassTransitConsoleWorker> _logger;

        public MassTransitConsoleWorker(
            IBusControl bus,
            ILogger<MassTransitConsoleWorker> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bus.StartAsync(stoppingToken).ConfigureAwait(false);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _bus.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
