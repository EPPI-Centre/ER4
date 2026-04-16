
namespace ER_Web.Services
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly HttpClient _httpClient;
        private IConfiguration _configuration;
        private static int _GlobalBackoffDelay = 0;//in Seconds. Code should access the Property, not this backing field.
        private static readonly object padlock4BackoffProperty = new object();

        private static int GlobalBackoffDelay
        {
            get
            {
                lock (padlock4BackoffProperty)
                {
                    return _GlobalBackoffDelay;
                }
            }
            set
            {
                lock (padlock4BackoffProperty)
                {
                    _GlobalBackoffDelay = value;
                }
            }
        }
        private async Task BackoffEnforcer()
        {
            if (GlobalBackoffDelay > 0)
            {
                await Task.Delay(1000 * GlobalBackoffDelay);
                GlobalBackoffDelay = 0;
            }
        }
        private void CheckForBackoffSignals(HttpResponseMessage? res)
        {
            if (res != null)
            {
                string backoffSt = "";
                int backoffInt;
                if (res.Headers.Contains("Backoff"))
                {
                    backoffSt = res.Headers.GetValues("Backoff").First();
                }
                else if (res.Headers.RetryAfter != null)
                {
                    backoffSt = res.Headers.RetryAfter.ToString();
                }
                if (backoffSt != "" && int.TryParse(backoffSt, out backoffInt))
                {
                    GlobalBackoffDelay = backoffInt;
                }
            }
        }

        public HttpClientProvider(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;           
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            var res = await SendRequestWithRetry(requestUri, HttpMethod.Get);
            return res;
            //return _httpClient.GetAsync(requestUri);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            await BackoffEnforcer();
            var res = await _httpClient.PostAsync(requestUri, content);
            CheckForBackoffSignals(res);
            return res;
        }

        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            await BackoffEnforcer();
            var res = await _httpClient.PutAsync(requestUri, content);
            CheckForBackoffSignals(res);
            return res;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            await BackoffEnforcer();
            var res = await _httpClient.DeleteAsync(requestUri);
            CheckForBackoffSignals(res);
            return res;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private async Task<HttpResponseMessage> SendRequestWithRetry(string requestUri, HttpMethod httpMethod, HttpContent content = null)
        {
            await BackoffEnforcer();
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
                    await Task.Delay(GlobalBackoffDelay > timeToDelayRetry? 1000*GlobalBackoffDelay : 1000*timeToDelayRetry);
                }
                var request = new HttpRequestMessage(httpMethod, new Uri(requestUri));
                if (content != null) request.Content = content;
                response = await _httpClient.SendAsync(request);
                //uncomment code below for testing purposes only!
                //Random aaa = new Random();
                //var bbb = aaa.Next(1, 10);
                //if (bbb > 5) response.StatusCode = System.Net.HttpStatusCode.NotFound;
                CheckForBackoffSignals(response);
                sendCount++;
            }
            while (sendCount < maxRetries && (response.StatusCode == System.Net.HttpStatusCode.NotFound
                                                || response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
                                                || response.StatusCode == System.Net.HttpStatusCode.InternalServerError
                                                ));

            return response;
        }
    }
}
