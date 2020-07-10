using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Courier.Contracts;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.MongoDbIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sample.Components.Batch.Consumers;
using Sample.Components.Consumers;
using Sample.Components.CourierActivities;
using Sample.Components.StateMachines;
using Sample.Components.StateMachines.Activities;
using Serilog;
using Warehouse.Contracts.Commands;
using Activity = System.Diagnostics.Activity;

namespace Sample.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            var isService = Debugger.IsAttached || args.Contains("--console");

            var host = CreateHostBuilder(args);

            await host.Build().RunAsync();
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
                    // services.AddOpenTelemetry(configurator =>
                    // {

                    //     configurator.UseZipkin(builder =>
                    //     {
                    //         builder.ServiceName = "mt-sample-service";
                    //         builder.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                    //     });
                    //
                    //     configurator.AddDependencyCollector();
                    // });

                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

                    // services.AddScoped<RoutingSlipCompletedBatchConsumer>();

                    services.AddMassTransit(
                        cfg =>
                        {
                            cfg.SetKebabCaseEndpointNameFormatter();
                            
                            cfg.AddConsumersFromNamespaceContaining<RoutingSlipCompletedBatchConsumer>();

                            cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                            // adds RoutingSlip activities
                            cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();

                            cfg.AddRequestClient<AllocateInventoryCommand>();

                            cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(
                                    typeof(OrderStateSagaDefinition))
                                .MongoDbRepository(r =>
                                {
                                    r.Connection = "mongodb://127.0.0.1";
                                    r.DatabaseName = "orderdb";
                                });

                            cfg.ConfigureBus();
                        });

                    services.AddHostedService<MassTransitConsoleWorker>();
                    services.AddScoped<CustomerAccountClosedActivity>();
                    services.AddScoped<OrderAcceptedActivity>();
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
                        busFactoryConfigurator.Host("localhost", "sample.api" , hostConfigurator => {});

                        // // we could set a consumer here by using ReceiveEndpoint,
                        // // or use the extension method of AddConsumer* on IServiceCollectionConfigurator
                        // // it is also possible to configure endpoint by xDefinitions, e.g. SagaDefinition<T>
                        // // NOT adding a receive endpoint name, publishes the message to all Consumer instances
                        // // Broadcasting Pub/Sub, not Competitive Consumer, the latter will be available if 
                        // // Receive endpoint has a name 
                        // busFactoryConfigurator.ReceiveEndpoint(host, e =>
                        // {
                        //     e.UseRetry(r=>r.Exponential(3,TimeSpan.FromMilliseconds(300),TimeSpan.FromMilliseconds(2000),
                        //         TimeSpan.FromMilliseconds(100)));
                        //     e.UseInMemoryOutbox();
                        //     e.Consumer(
                        //         () => new SubmitOrderConsumer(context.Container.GetService<ILogger<SubmitOrderConsumer>>()));
                        // });

                        // creates queues, sagas and etc.
                        busFactoryConfigurator.ConfigureEndpoints(context);

                        // busFactoryConfigurator.ReceiveEndpoint(
                        //     KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipCompletedBatchConsumer>(),
                        //     endpoint =>
                        //     {
                        //         // this should be at least the number of MessageLimit,
                        //         // otherwise we always get a time limit  
                        //         endpoint.PrefetchCount = 20;
                        //
                        //         endpoint.Batch<RoutingSlipCompleted>(b =>
                        //         {
                        //             b.MessageLimit = 10;
                        //             b.TimeLimit = TimeSpan.FromSeconds(5);
                        //
                        //             b.Consumer<RoutingSlipCompletedBatchConsumer, RoutingSlipCompleted>(
                        //                 context.Container);
                        //         });
                        //     });
                    }));


            return configurator;
        }
    }
}
