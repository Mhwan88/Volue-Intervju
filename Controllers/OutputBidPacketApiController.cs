using System.Net.Http.Headers;
using JsonConverter= System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VolueEnergyTrader.Models;

namespace VolueEnergyTrader.Controllers
{
    public class OutputBidPacketApiController
    {
        private readonly HttpClient _client;
        private readonly string _customerId = "TestCustomer";
        private readonly string _apiKey = File.ReadAllText("secret.key");
        private readonly string _forDate = "2024-02-03";
        private readonly string _market = "FCR-D-D1";
        private readonly string _country = "Sweden";
        private readonly string _endPoint = File.ReadAllText("Volue.endpoint");
        private readonly ILogger<OutputBidPacketApiController> _logger;  

        
        public OutputBidPacketApiController(ILogger<OutputBidPacketApiController> logger)
        {
            _client = new HttpClient();
            _logger = logger;
        }

        
        public async Task<OutputBidPacketApiModel> FetchBidResultsAsync()
        {
            
            var uri = new UriBuilder("https://vmsn-app-planner3test.azurewebsites.net/status/market/bid-result");
            uri.Query = $"CustomerId={_customerId}&ForDate={_forDate}&Market={_market}&Country={_country}";
            
            
            
            var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri);
            request.Headers.Add("ApiKey", _apiKey);
            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Request failed with status code: {response.StatusCode} and reason: {response.ReasonPhrase}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<OutputBidPacketApiModel>(jsonResponse);

            if (result == null)
            {
                _logger.LogWarning("No response from the server.");
            }

            _logger.LogInformation($"Received response: {result}");
            
            return result;
        }
    }
}