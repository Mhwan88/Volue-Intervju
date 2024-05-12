using System.Net.Http.Headers;
using JsonConverter= System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VolueEnergyTrading.Controller
{
    public class BidResultController
    {
        private readonly HttpClient _client;
        private readonly string _customerId = "TestCustomer";
        private readonly string _apiKey = File.ReadAllText("secret.key");
        private readonly string _forDate = "2024-02-03";
        private readonly string _market = "FCR-D-D1";
        private readonly string _country = "Sweden";
        private readonly string _endPoint = File.ReadAllText("Volue.endpoint");
        
        public BidResultController()
        {
            _client = new HttpClient();
        }

        
        public async Task<JObject> FetchBidResultsAsync()
        {
            
            var uri = new UriBuilder("https://vmsn-app-planner3test.azurewebsites.net/status/market/bid-result");
            uri.Query = $"CustomerId={_customerId}&ForDate={_forDate}&Market={_market}&Country={_country}";
            
            
            
            var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri);
            request.Headers.Add("ApiKey", _apiKey);
            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Request failed with status code: {response.StatusCode} and reason: {response.ReasonPhrase}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

            if (result == null)
            {
                Console.WriteLine("No response");
                return [];
            }

            Console.WriteLine($"this is the result {result}");
            
            
            return result;
        }
    }
}