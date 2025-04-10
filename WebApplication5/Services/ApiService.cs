using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebApplication5.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string _token;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            var baseAddress = _configuration["ExternalApi:BaseAddress"];
            if (!string.IsNullOrEmpty(baseAddress))
                _httpClient.BaseAddress = new Uri(baseAddress);
            else
                throw new InvalidOperationException("Base address is not configured.");
        }

        private async Task EnsureTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token) && DateTime.Now < _tokenExpiry) return;

            var requestBody = new
            {
                username = _configuration["ExternalApi:Username"],
                password = _configuration["ExternalApi:Password"]
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var loginEndpoint = _configuration["ExternalApi:Endpoints:Login"];
            var response = await _httpClient.PostAsync(loginEndpoint, jsonContent);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Login failed: {response.StatusCode}");

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);
            _token = jsonDoc.RootElement.GetProperty("token").GetString();
            _tokenExpiry = DateTime.Now.AddHours(1);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        public async Task<string> GetDataAsync(string endpoint)
        {
            await EnsureTokenAsync();

            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed request: {response.StatusCode}");

            return await response.Content.ReadAsStringAsync();
        }
    }
}
