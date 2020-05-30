using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Warehouse.Components;

namespace Warehouse.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                    
                    services.AddMassTransit(
                        cfg =>
                        {
                            cfg.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();
                            
                            cfg.ConfigureBus();
                        });

                    services.AddHostedService<MassTransitWarehouseConsoleWorker>();
                });

            return host;
        }
    }
    
    public static class SampleServiceExtensions
    {
        public static IServiceCollectionConfigurator ConfigureBus(this IServiceCollectionConfigurator configurator)
        {
            configurator.AddBus(context =>
                Bus.Factory.CreateUsingRabbitMq(
                    busFactoryConfigurator =>
                    {
                        var host = busFactoryConfigurator.Host("localhost", "sample.api");
                        
                        busFactoryConfigurator.ConfigureEndpoints(context);
                    }));


            return configurator;
        }
    }
}
