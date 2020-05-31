using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using MassTransit;
using MassTransit.Testing;
using Sample.Components.Consumers;
using Sample.Contracts.Commands;
using Sample.Contracts.Events;
using Sample.Contracts.Responses;
using Xunit;

namespace Sample.Components.Test
{
    public sealed class When_an_order_request_is_consumed
    {
        
        [Fact]
        public async Task Should_respond_with_rejected_if_test()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(3)};
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                // use this for request/response scenarios
                var requestClient = await harness.ConnectRequestClient<SubmitOrderCommand>();
                
                var response = await requestClient.GetResponse<OrderSubmissionRejectedResponse>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    Customer = "TEST1234"
                });


                response.Message.OrderId.IsSameOrEqualTo(orderId);
                consumer.Consumed.Select<SubmitOrderCommand>().Any().Should().BeTrue();
                harness.Sent.Select<OrderSubmissionRejectedResponse>().Any().Should().BeTrue();
            }
            finally
            {
                await harness.Stop();
            }
        }

        
        [Fact]
        public async Task Should_respond_with_acceptance_if_ok()
        {
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();

                // use this for request/response scenarios
                var requestClient = await harness.ConnectRequestClient<SubmitOrderCommand>();
                
                var response = await requestClient.GetResponse<OrderSubmissionAcceptedResponse>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    Customer = "1234"
                });


                response.Message.OrderId.IsSameOrEqualTo(orderId);
                consumer.Consumed.Select<SubmitOrderCommand>().Any().Should().BeTrue();
                harness.Sent.Select<OrderSubmissionAcceptedResponse>().Any().Should().BeTrue();
                // harness.Published.Select<OrderSubmittedEvent>().Any().Should().BeTrue();
            }
            finally
            {
                await harness.Stop();
            }
        }
        
        [Fact]
        public async Task Should_consume_submit_order_command()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(3)};
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                
                // use this to send commands not request/response
                await harness.InputQueueSendEndpoint.Send<SubmitOrderCommand>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    Customer = "1234"
                });

                consumer.Consumed.Select<SubmitOrderCommand>().Any().Should().BeTrue();
                
                harness.Sent.Select<OrderSubmissionAcceptedResponse>().Any().Should().BeFalse();
                harness.Sent.Select<OrderSubmissionRejectedResponse>().Any().Should().BeFalse();
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task Should_publish_order_submitted_event()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(3)};
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                await harness.InputQueueSendEndpoint.Send<SubmitOrderCommand>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    Customer = "1234"
                });

                harness.Published.Select<OrderSubmittedEvent>().Any().Should().BeTrue();
                
            }
            finally
            {
                await harness.Stop();
            }
        }
        
        [Fact]
        public async Task Should_not_publish_order_submitted_event_when_rejected()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(3)};
            var consumer = harness.Consumer<SubmitOrderConsumer>();

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                await harness.InputQueueSendEndpoint.Send<SubmitOrderCommand>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    Customer = "TEST1234"
                });

                harness.Published.Select<OrderSubmittedEvent>().Any().Should().BeFalse();
                
            }
            finally
            {
                await harness.Stop();
            }
        }
        
    }
}
