using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("accounts")]
    [Tags("Accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _paymentsServiceUrl;

        public AccountsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _paymentsServiceUrl = Environment.GetEnvironmentVariable("PAYMENTS_SERVICE_URL") ?? "http://payments-service:8080";
        }

        /// <summary>
        /// Create an account.
        /// </summary>
        /// <param name="request">Account creation details.</param>
        /// <returns>Created account ID.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var client = _httpClientFactory.CreateClient("PaymentsService");
            var downstreamUrl = $"{_paymentsServiceUrl}/api/accounts";

            var response = await ProxyRequest(client, downstreamUrl, HttpMethod.Post, request);
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Top up an account.
        /// </summary>
        /// <param name="accountId">Account ID.</param>
        /// <param name="request">Top-up amount.</param>
        /// <returns>No content.</returns>
        [HttpPost("{accountId}/topup")]
        public async Task<IActionResult> TopUpAccount(string accountId, [FromBody] TopUpAccountRequest request)
        {
            var client = _httpClientFactory.CreateClient("PaymentsService");
            var downstreamUrl = $"{_paymentsServiceUrl}/api/accounts/{accountId}/topup";

            var response = await ProxyRequest(client, downstreamUrl, HttpMethod.Post, request);
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Get the balance of an account.
        /// </summary>
        /// <param name="accountId">Account ID.</param>
        /// <returns>Account balance.</returns>
        [HttpGet("{accountId}/balance")]
        public async Task<IActionResult> GetAccountBalance(string accountId)
        {
            var client = _httpClientFactory.CreateClient("PaymentsService");
            var downstreamUrl = $"{_paymentsServiceUrl}/api/accounts/{accountId}/balance";

            var response = await ProxyRequest(client, downstreamUrl, HttpMethod.Get, null);
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Proxy helper method.
        /// </summary>
        private async Task<HttpResponseMessage> ProxyRequest(HttpClient client, string url, HttpMethod method, object content)
        {
            var request = new HttpRequestMessage(method, url);

            if (content != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(content);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            // Copy headers except Host
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

    /// <summary>
    /// DTO for creating an account.
    /// </summary>
    public record CreateAccountRequest(Guid UserId);

    /// <summary>
    /// DTO for topping up an account.
    /// </summary>
    public record TopUpAccountRequest(decimal Amount);
}
