
namespace ERxWebClient2.Services
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly HttpClient _httpClient;
        private IConfiguration _configuration;

        public HttpClientProvider(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;           
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return SendRequestWithRetry(requestUri, HttpMethod.Get);
            //return _httpClient.GetAsync(requestUri);
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return _httpClient.PostAsync(requestUri, content);
        }

        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            return _httpClient.PutAsync(requestUri, content);
        }

        public Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return _httpClient.DeleteAsync(requestUri);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private async Task<HttpResponseMessage> SendRequestWithRetry(string requestUri, HttpMethod httpMethod, HttpContent content = null)
        {
            HttpResponseMessage response;
            var sendCount = 0;
            var configuration = _configuration.GetSection("ZoteroSettings");
            var minIntervalFactor = Convert.ToDouble(configuration["zotero_request_minimum_interval_factor"]);
            var maxRetries = Convert.ToDouble(configuration["zotero_request_retries"]);
            do
            {
                if (sendCount > 0) {
                    var timeToDelayRetry = (int)Math.Pow(minIntervalFactor, 
                        sendCount);
                    await Task.Delay(1000*timeToDelayRetry); 
                }
                var request = new HttpRequestMessage(httpMethod, new Uri(requestUri));
                if (content != null) request.Content = content;
                response = await _httpClient.SendAsync(request);
                sendCount++;
            }
            while (sendCount < maxRetries && response.StatusCode == System.Net.HttpStatusCode.NotFound);

            return response;
        }
    }
}
