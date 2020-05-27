using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Components.Consumers;
using Sample.Components.StateMachines;

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
                            // cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();

                            cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                                .RedisRepository(); 
                            
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
                        // we could set a consumer here by using ReceiveEndpoint,
                        // or use the extension method of AddConsumer* on IServiceCollectionConfigurator 
                        busFactoryConfigurator.ReceiveEndpoint("submit-order", e =>
                        {
                            e.UseRetry(r=>r.Exponential(3,TimeSpan.FromMilliseconds(300),TimeSpan.FromMilliseconds(2000),
                                TimeSpan.FromMilliseconds(100)));
                            e.UseInMemoryOutbox();
                            e.Consumer(
                                () => new SubmitOrderConsumer(context.Container.GetService<ILogger<SubmitOrderConsumer>>()));
                        });
                        // creates queues, sagas and etc.
                        busFactoryConfigurator.ConfigureEndpoints(context.Container);
                    }));


            return configurator;
        }
    }
}
