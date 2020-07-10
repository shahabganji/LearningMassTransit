using System;
using System.Threading.Tasks;
using MassTransit;
using static System.Console;

namespace Sample.Platform
{
    public interface SampleCommand
    {
        Guid Id { get; set; }
        string Command { get; set; }
    }
}


namespace Sample.Platform.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bus = Bus.Factory.CreateUsingRabbitMq();

            await bus.StartAsync();

            while (true)
            {
                Write("Enter a command or leave it blank to quit: ");

                var command = ReadLine();
                
                if( string.IsNullOrEmpty(command) ) break;

                await bus.Publish<SampleCommand>(new { InVar.Id , Command = command });

            }

            await bus.StopAsync();
        }
    }
}
