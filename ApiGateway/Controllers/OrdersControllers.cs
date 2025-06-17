using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("orders")]
    [Tags("Orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _ordersServiceUrl;

        public OrdersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _ordersServiceUrl = Environment.GetEnvironmentVariable("ORDERS_SERVICE_URL") ?? "http://orders-service:8080";
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var client = _httpClientFactory.CreateClient("OrdersService");
            var downstreamUrl = $"{_ordersServiceUrl}/api/orders";

            var response = await ProxyRequest(client, downstreamUrl, HttpMethod.Post, request);
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] Guid userId)
        {
            var client = _httpClientFactory.CreateClient("OrdersService");
            var downstreamUrl = $"{_ordersServiceUrl}/api/orders?userId={userId}";

            var response = await ProxyRequest(client, downstreamUrl, HttpMethod.Get, null);
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpGet("{orderId}/status")]
        public async Task<IActionResult> GetOrderStatus(string orderId)
        {
            var client = _httpClientFactory.CreateClient("OrdersService");
            var downstreamUrl = $"{_ordersServiceUrl}/api/orders/{orderId}/status";

            var response = await ProxyRequest(client, downstreamUrl, HttpMethod.Get, null);
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        private async Task<HttpResponseMessage> ProxyRequest(HttpClient client, string url, HttpMethod method, object content)
        {
            var request = new HttpRequestMessage(method, url);

            if (content != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(content);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }            

            foreach (var header in Request.Headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && request.Content != null)
                {
                    request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);
        }
    }

    public record CreateOrderRequest(Guid userId, decimal amount);
}
