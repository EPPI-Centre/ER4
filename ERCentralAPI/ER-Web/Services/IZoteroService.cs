using ERxWebClient2.Controllers;
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Services
{
    public interface IZoteroService    {
        Task<JObject> POSTFormMultiPart(IEnumerable<KeyValuePair<string, string>> payload, string requestUri, IHttpClientProvider httpProvider);

        Task<string> POSTJDocument(string payload, string requestUri, IHttpClientProvider httpProvider);

        Task<string> POSTDocument(string payload, string requestUri, IHttpClientProvider httpProvider);

        Task<HttpResponseMessage> GetDocumentHeader(string requestUri, IHttpClientProvider httpProvider);

        Task<bool> DeleteApiKey(string requestUri, IHttpClientProvider httpProvider);

        Task<Collection> GetCollectionItem(string requestUri, IHttpClientProvider httpProvider);

        Task<JObject> GetItem(string requestUri, IHttpClientProvider httpProvider);

        Task<string> GetUserPermissions(string requestUri, IHttpClientProvider httpProvider);


        Task<HttpResponseMessage> CreateItem(string payload, string requestUri, IHttpClientProvider httpProvider);

        Task<List<T>> GetPagedCollections<T>(string requestUri, IHttpClientProvider httpProvider);

        Task<List<T>> GetCollections<T>(string requestUri, IHttpClientProvider httpProvider);

        Task<string> DoGetReq(string requestUri, IHttpClientProvider httpProvider); // TODO this is not named well

    }
}