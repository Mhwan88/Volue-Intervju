using System.Net.Http.Headers;
using JsonConverter= System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VolueEnergyTrader.Models;

namespace VolueEnergyTrader.Controllers
{
    public class OutputBidPacketAPiFetcher
    {
        private readonly HttpClient _client;
        private readonly string _customerId = "TestCustomer";
        private readonly string _apiKey = Environment.GetEnvironmentVariable("APIKEY");
        private readonly string _forDate = "2024-02-03";
        private readonly string _market = "FCR-D-D1";
        private readonly string _country = "Sweden";
        private readonly string _endPoint = File.ReadAllText("Volue.endpoint");
        private readonly ILogger<OutputBidPacketAPiFetcher> _logger;


        public OutputBidPacketAPiFetcher(ILogger<OutputBidPacketAPiFetcher> logger)
        {
            _client = new HttpClient();
            _logger = logger;
        }

        // Method for fetching API values
        public async Task<OutputBidPacketApiModel> FetchBidResultsAsync()
        {
            try
            {
                // Construct the URI for the API call using the UriBuilder
                var uri = new UriBuilder("https://vmsn-app-planner3test.azurewebsites.net/status/market/bid-result");

                // Cuild the query string with parameters
                uri.Query = $"CustomerId={_customerId}&ForDate={_forDate}&Market={_market}&Country={_country}";

                // Get request with the query parameters 
                var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri);

                // Add API key to header for authorization
                request.Headers.Add("ApiKey", _apiKey);

                // Send the HTTP request async 
                var response = await _client.SendAsync(request);

                // Check errors in request
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        $"Request failed with status code: {response.StatusCode} and reason: {response.ReasonPhrase}");
                }

                // Read respons as a string
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Convert the JSON respons into an instance of OutBidPacketApiModel
                var result = JsonConvert.DeserializeObject<OutputBidPacketApiModel>(jsonResponse);

                if (result == null)
                {
                    _logger.LogWarning("No response from the server.");
                    return null;
                }

                _logger.LogInformation($"Received response: {result}");

                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError($"An error occurred while fetching bid results: {ex.Message}");
                throw; 
            }
        }
    }
}