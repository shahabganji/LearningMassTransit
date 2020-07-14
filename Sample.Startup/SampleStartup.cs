using System;
using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Platform.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sample.Components.Batch.Consumers;
using Sample.Components.Consumers;
using Sample.Components.CourierActivities;
using Sample.Components.StateMachines;
using Sample.Components.StateMachines.Activities;
using Warehouse.Contracts.Commands;

namespace Sample.Startup
{
    public class SampleStartup : IPlatformStartup
    {
        public void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator, IServiceCollection services)
        {
            // I guess one is required
            configurator.SetKebabCaseEndpointNameFormatter();
            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

            configurator.AddConsumer<RoutingSlipCompletedBatchConsumer>(
                x => x.Options<BatchOptions>(b => b.SetMessageLimit(10).SetTimeLimit(s: 5)));

            configurator.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
            // adds RoutingSlip activities
            configurator.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();

            configurator.AddRequestClient<AllocateInventoryCommand>();

            configurator.AddSagaStateMachine<OrderStateMachine, OrderState>(
                    typeof(OrderStateSagaDefinition))
                .MongoDbRepository(r => 
                {
                    r.Connection = "mongodb://mongo";
                    r.DatabaseName = "orderdb";
                });

            services.AddScoped<CustomerAccountClosedActivity>();
            services.AddScoped<OrderAcceptedActivity>();
        }

        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator,
            IBusRegistrationContext context) where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
            // configurator.ConfigureEndpoints(context);
        }
    }
}
