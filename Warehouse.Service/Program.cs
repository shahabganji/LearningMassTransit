using System;
using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.MongoDbIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;
using Warehouse.Components.Consumers;
using Warehouse.Components.StateMachines;

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
                .ConfigureLogging((context, builder) =>
                {
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile("serilog.json", false, true)
                        .AddJsonFile($"serilog.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                        .Build();
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();
                    builder.AddSerilog(logger, dispose: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

                    services.AddMassTransit(
                        cfg =>
                        {
                            cfg.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();

                            cfg.AddSagaStateMachine<AllocationStateMachine, AllocationState>(
                                    typeof(AllocateStateMachineDefinition))
                                .MongoDbRepository(r =>
                                {
                                    r.Connection = "mongodb://127.0.0.1";
                                    r.DatabaseName = "allocations";
                                });

                            cfg.ConfigureBus();
                        });

                    services.AddHostedService<MassTransitWarehouseConsoleWorker>();
                });

            return host;
        }
    }

    public static class SampleServiceExtensions
    {
        public static IServiceCollectionBusConfigurator ConfigureBus(this IServiceCollectionBusConfigurator configurator)
        {
            configurator.AddBus(context =>
                Bus.Factory.CreateUsingRabbitMq(
                    busFactoryConfigurator =>
                    {
                        busFactoryConfigurator.Host("localhost",
                            hostConfigurator => { });

                        // when using scheduler this line should be added
                        busFactoryConfigurator.UseMessageScheduler(new Uri("queue:quartz-scheduler"));

                        busFactoryConfigurator.ConfigureEndpoints(context);
                    }));


            return configurator;
        }
    }
}
