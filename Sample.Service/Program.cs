using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sample.Components.Consumers;

namespace Sample.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var isService = Debugger.IsAttached || args.Contains("--console");

            var host = CreateHostBuilder(args);

            await host.Build().RunAsync();
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
                            cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                            cfg.ConfigureBus();
                        });


                    services.AddHostedService<MassTransitConsoleWorker>();
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
                        // creates queues, sagas and etc.
                        busFactoryConfigurator.ConfigureEndpoints(context.Container);
                    }));


            return configurator;
        }
    }
}
