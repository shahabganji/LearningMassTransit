using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MassTransit.Definition;
using MassTransit.Testing;
using Sample.Components.Consumers;
using Sample.Components.StateMachines;
using Sample.Contracts.Commands;
using Sample.Contracts.Events;
using Sample.Contracts.Responses;
using Xunit;

namespace Sample.Components.Test
{
    public sealed class Submitting_an_order
    {
        [Fact]
        public async Task Should_create_a_state_instance()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(3)};
            var orderStateMachine = new OrderStateMachine();
            var orderSaga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                await harness.Bus.Publish<OrderSubmittedEvent>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerName = "1234"
                });

                orderSaga.Created
                    .Select(s => s.CorrelationId == orderId)
                    .Any()
                    .Should().BeTrue();

                var instanceId = await orderSaga.Exists(orderId, s => s.Submitted);
                instanceId.Should().NotBeNull();

                var instance = orderSaga.Sagas.Contains(orderId);
                instance.CustomerNumber.Should().Be("1234");
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task Should_respond_to_status_check_requested_event()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(3)};
            var orderStateMachine = new OrderStateMachine();
            var orderSaga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                await harness.Bus.Publish<OrderSubmittedEvent>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerName = "1234"
                });

                orderSaga.Created
                    .Select(s => s.CorrelationId == orderId)
                    .Any()
                    .Should().BeTrue();

                var instanceId = await orderSaga.Exists(orderId, s => s.Submitted);
                instanceId.Should().NotBeNull();

                var requestClient = await harness.ConnectRequestClient<CheckOrderRequestedEvent>();

                var response = await requestClient.GetResponse<OrderStatusResponse>(new
                {
                    OrderId = orderId
                });

                orderSaga.Consumed.Select<CheckOrderRequestedEvent>().Any().Should().BeTrue();
                response.Message.State.Should().Be(orderStateMachine.Submitted.Name);

            }
            finally
            {
                await harness.Stop();
            }
        }
        
        [Fact]
        public async Task Should_cancel_order_when_account_closed()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(3)};
            var orderStateMachine = new OrderStateMachine();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                var customer = "12345";
                await harness.Bus.Publish<OrderSubmittedEvent>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerName  = customer
                });

                saga.Created.Select(x => x.CorrelationId == orderId).Any().Should().BeTrue();
                
                var instance = await saga.Exists(orderId, x => x.Submitted);
                instance.Should().NotBeNull();

                await harness.Bus.Publish<CustomerAccountClosedEvent>(new
                {
                    CustomerId = NewId.NextGuid(),
                    CustomerNumber = customer
                });

                instance = await saga.Exists(orderId, x => x.Cancelled);
                instance.Should().NotBeNull();
            }
            finally
            {
                await harness.Stop();
            }
        }
        
        [Fact]
        public async Task Should_accept_an_order()
        {
            var harness = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(3)};
            var orderStateMachine = new OrderStateMachine();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);

            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                var customer = "12345";
                await harness.Bus.Publish<OrderSubmittedEvent>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerName  = customer
                });

                saga.Created.Select(x => x.CorrelationId == orderId).Any().Should().BeTrue();
                
                var instance = await saga.Exists(orderId, x => x.Submitted);
                instance.Should().NotBeNull();

                await harness.Bus.Publish<OrderAcceptedEvent>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                });

                instance = await saga.Exists(orderId, x => x.Accepted);
                instance.Should().NotBeNull();
            }
            finally
            {
                await harness.Stop();
            }
        }

    }
    
}
