using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Warehouse.Service
{
    public class MassTransitWarehouseConsoleWorker : BackgroundService
    {
        private readonly IBusControl _busControl;
        
        public MassTransitWarehouseConsoleWorker(IBusControl busControl)
         => _busControl = busControl;
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => _busControl.StartAsync(stoppingToken);

        public override Task StopAsync(CancellationToken cancellationToken) => _busControl.StopAsync(cancellationToken);
    }
}
