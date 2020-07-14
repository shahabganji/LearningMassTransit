using System;
using System.Net.Http;
using System.Threading.Tasks;
using MassTransit;
using static System.Console;
using static System.String;

namespace Sample.Console.BatchProducer
{
    class Program
    {
        private const string BaseUrl = "https://localhost:6001";
        private static readonly HttpClient Client = new HttpClient();

        static async Task Main(string[] args)
        {
            string response;
            do
            {
                await Out.WriteAsync("Enter number of messages, or leave it blank to exit. # ");
                response = await In.ReadLineAsync();

                if (IsNullOrEmpty(response)) break; // exit the program

                var validNumberOfMessages = int.TryParse(response, out var messages);

                if (!validNumberOfMessages)
                {
                    await Out.WriteLineAsync("Please enter a valid number");
                    continue;
                }

                for (var index = 0; index < messages; index++)
                {
                    var shouldBeFaulted = index % 4 == 0;

                    var orderId = NewId.NextGuid();
                    var customerNumber = shouldBeFaulted ? "INVALIDCustomer" : "Customer";
                    const string paymentCardNumber = "4000-1234-1234-1234";

                    await Out.WriteLineAsync($"Order: {orderId} STATUS: " +
                                             (shouldBeFaulted ? "Faulted" : "Completed"));


                    var createRequest = new HttpRequestMessage(HttpMethod.Post,
                        new Uri($"{BaseUrl}/Order/{orderId}/{customerNumber}?paymentCardNumber={paymentCardNumber}"));
                    // Creat new Order
                    await Client.SendAsync(createRequest);

                    var acceptRequest = new HttpRequestMessage(HttpMethod.Put,
                        new Uri($"{BaseUrl}/Order/{orderId}/accept"));
                    // Accept the Order
                    await Client.SendAsync(acceptRequest);
                }

                await Out.WriteLineAsync("");
            } while (!IsNullOrEmpty(response));
        }
    }
}
