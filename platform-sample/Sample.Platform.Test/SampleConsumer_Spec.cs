using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Sample.Platform.Test
{
    public class UnitTest1
    {
        [Fact]
        public async Task Should_consume_the_message()
        {
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer(() => new SampleConsumer(NullLogger<SampleConsumer>.Instance));

            await harness.Start();

            try
            {
                await harness.InputQueueSendEndpoint.Send<SampleCommand>(
                    new {Id = NewId.NextGuid(), Command = "Test"});

                var consumed = harness.Consumed.Select<SampleCommand>().Any();
                
                Assert.True(consumed);

            }
            finally
            {
                await harness.Stop();
            }

        }
    }
}
