using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Sample.Contracts.Events;

namespace Sample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class CustomerController : Controller
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public CustomerController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpDelete("{id}/{customerNumber}")]
        public async Task<IActionResult> Delete(Guid id, string customerNumber)
        {
            await _publishEndpoint.Publish<CustomerAccountClosedEvent>(new
            {
                CustomerId = id,
                CustomerNumber = customerNumber
            });

            return Ok();
        }
    }
}
