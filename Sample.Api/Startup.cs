using System;
using MassTransit;
using MassTransit.Definition;
using MassTransit.PrometheusIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Sample.Contracts.Commands;
using Sample.Contracts.Events;

namespace Sample.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddOpenTelemetry(builder =>
            // {
            //     builder.UseZipkin(o =>
            //     {
            //         o.ServiceName = "sample-api";
            //         o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
            //     });
            //
            //     builder.AddRequestCollector();
            //     
            //     // sets sampler
            //     //b.SetSampler(new HealthRequestsSampler(Samplers.AlwaysSample));
            //
            //     // sets resource
            //     //b.SetResource(new Resource(new Dictionary<string, string>() {
            //     //    { "service.name", "BackEndApp" },
            //     //    { "deploymentTenantId", "kubecon-demo-surface" } }));
            //
            //     // set the FlightID from the distributed context
            //     //b.AddProcessorPipeline(pipelineBuilder => pipelineBuilder.AddProcessor(_ => new FlightIDProperties()));
            //     
            // });

            // this replaces Mediator, also adds health checks
            services.AddMassTransitHostedService();
            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(cfg =>
            {
                // adds the consumer of a contract/message as SCOPED 
                // cfg.AddConsumer<SubmitOrderConsumer>();

                // in-memory version of masstransit runtime
                // MassTransit has an in-memory transport but Mediator does not even use transport so it really is fast.
                // ~= 650K messages/sec
                // cfg.AddMediator();

                cfg.AddBus(context => Bus.Factory.CreateUsingRabbitMq(configure =>
                {
                    configure.Host("localhost", "sample.api");
                    configure.ConfigureEndpoints(context);
                    
                    configure.UsePrometheusMetrics(serviceName:"mt-sample-api");
                }));

                // adds a client that knows how to send a request to an endpoint, this endpoint should be the same 
                // as the endpoint set for the consumer, either manually or by formatters
                cfg.AddRequestClient<SubmitOrderCommand>(new Uri("queue:submit-order"));
                // new Uri($"exchange:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}"));

                cfg.AddRequestClient<CheckOrderRequestedEvent>();
            });

            services.AddOpenApiDocument(settings => settings.Title = "Sample Api");

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseHttpMetrics();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics("/metrics/all");
                // endpoints.MapMetrics("/metrics/mt");
            });
        }
    }
}
