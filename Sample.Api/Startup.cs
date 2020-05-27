using System;
using MassTransit;
using MassTransit.Definition;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sample.Components.Consumers;
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

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMassTransit(cfg =>
            {
                // adds the consumer of a contract/message as SCOPED 
                // cfg.AddConsumer<SubmitOrderConsumer>();

                // in-memory version of masstransit runtime
                // MassTransit has an in-memory transport but Mediator does not even use transport so it really is fast.
                // ~= 650K messages/sec
                // cfg.AddMediator();
                
                services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                cfg.AddBus(context => Bus.Factory.CreateUsingRabbitMq(configure =>
                {
                    configure.ConfigureEndpoints(context.Container);
                }));
                // this replaces Mediator, also adds health checks
                services.AddMassTransitHostedService();

                // adds a client that knows how to send a request
                cfg.AddRequestClient<SubmitOrderCommand>(
                    new Uri($"exchange:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}")); 
                cfg.AddRequestClient<CheckOrderRequestedEvent>();
                
            });

            services.AddOpenApiDocument(settings => settings.Title = "Sample Api" );
            
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
