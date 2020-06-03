using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Sample.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Open Telemetry
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            var serilogConfiguration = new ConfigurationBuilder()
                .AddJsonFile("serilog.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(serilogConfiguration)
                .CreateLogger();

            try
            {
                Log.Information("Starting up the Sample.Api application");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application, Sample.Api, start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("serilog.json", false, true)
                        .AddJsonFile($"serilog.{context.HostingEnvironment.EnvironmentName}.json", true, true);
                })
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration);
                });
    }
}
