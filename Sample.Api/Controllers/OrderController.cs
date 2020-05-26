using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Contracts;

namespace Sample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IRequestClient<ISubmitOrder> _submitOrderRequestClient;

        public OrderController(ILogger<OrderController> logger, IRequestClient<ISubmitOrder> submitOrderRequestClient)
        {
            _logger = logger;
            _submitOrderRequestClient = submitOrderRequestClient;
        }

        [HttpPost("{id}/{customerNumber}")]
        [ProducesResponseType(typeof(IOrderSubmissionAccepted), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(IOrderSubmissionRejected), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(Guid id, string customerNumber)
        {
            // the default timeout is 30 sec
            var (accepted, rejected) =
                await _submitOrderRequestClient.GetResponse<IOrderSubmissionAccepted, IOrderSubmissionRejected>(
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
    }
}
