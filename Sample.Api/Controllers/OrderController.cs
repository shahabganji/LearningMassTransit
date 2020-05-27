using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Definition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Components.Consumers;
using Sample.Contracts.Commands;
using Sample.Contracts.Events;
using Sample.Contracts.Responses;

namespace Sample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IRequestClient<SubmitOrderCommand> _submitOrderClient;
        private readonly IRequestClient<CheckOrderRequestedEvent> _checkOrderClient;
        private readonly ISendEndpointProvider _endpointProvider;

        public OrderController(
            ILogger<OrderController> logger,
            IRequestClient<SubmitOrderCommand> submitOrderClient,
            IRequestClient<CheckOrderRequestedEvent> checkOrderClient,
            ISendEndpointProvider endpointProvider)
        {
            _logger = logger;
            _submitOrderClient = submitOrderClient;
            _checkOrderClient = checkOrderClient;
            _endpointProvider = endpointProvider;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OrderNotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var (status,notFound) = await _checkOrderClient.GetResponse<OrderStatusResponse,OrderNotFoundResponse>(new
            {
                OrderId = id
            });

            var response = status.IsCompletedSuccessfully
                ? (IActionResult) Ok((await status).Message)
                : NotFound((await notFound).Message);
            
            return  response;
        }
        
        [HttpPost("{id}/{customerNumber}")]
        [ProducesResponseType(typeof(OrderSubmissionAcceptedResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(OrderSubmissionRejectedResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(Guid id, string customerNumber)
        {
            // the default timeout is 30 sec
            var (accepted, rejected) =
                await _submitOrderClient.GetResponse<OrderSubmissionAcceptedResponse, OrderSubmissionRejectedResponse>(
                    new
                    {
                        OrderId = id,
                        Customer = customerNumber,
                        InVar.Timestamp
                    });

            if (accepted.IsCompletedSuccessfully)
            {
                var response = await accepted;
                return Accepted(response.Message);
            }

            var rejectedResponse = await rejected;
            return BadRequest(rejectedResponse.Message);
        }

        [HttpPut("{id}/{customerNumber}")]
        [ProducesResponseType( StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Put(Guid id, string customerNumber)
        {
            var endpoint = await _endpointProvider.GetSendEndpoint(
                new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Consumer<SubmitOrderConsumer>()}"));
                // new Uri($"queue:submit-order"));

                await endpoint.Send<SubmitOrderCommand>(new
            {
                OrderId = id,
                Customer = customerNumber,
                InVar.Timestamp
            });

            return Accepted();
        }
    }
}
