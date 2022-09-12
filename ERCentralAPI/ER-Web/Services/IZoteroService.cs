using ERxWebClient2.Controllers;
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Services
{
    public interface IZoteroService    {
        Task<JObject> POSTFormMultiPart(IEnumerable<KeyValuePair<string, string>> payload, string requestUri);

        Task<JObject> POSTJDocument(string payload, string requestUri);

        Task<string> POSTDocument(string payload, string requestUri);

        Task<string> POSTOAuth(string payload, string requestUri);

        Task<HttpResponseMessage> GetDocumentHeader(string requestUri);

        Task<bool> DeleteApiKey(string requestUri);

        Task<Collection> GetCollectionItem(string requestUri);

        Task<JObject> GetItem(string requestUri);

        Task<string> GetUserPermissions(string requestUri);

        Task<HttpResponseMessage> UpdateZoteroItem<T>(string payload, string requestUri);

        Task<HttpResponseMessage> CreateItem(string payload, string requestUri);

        Task<List<T>> GetPagedCollections<T>(string requestUri);

        Task<List<T>> GetCollections<T>(string requestUri);

        Task<string> DoGetReq(string requestUri); // TODO this is not named well

        void SetZoteroServiceHttpProvider(IHttpClientProvider httpProvider);
    }
}