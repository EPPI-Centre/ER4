using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ERxWebClient2.Services
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly HttpClient _httpClient;

        public HttpClientProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return _httpClient.GetAsync(requestUri);
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
    }
}
