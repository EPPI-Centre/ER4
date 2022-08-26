using ERxWebClient2.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
//using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
//using System.Web.Http;

namespace ERxWebClient2.Services
{
	public sealed class ZoteroService : IZoteroService, IDisposable
	{
		public UriBuilder GetCollectionsUri;
		public UriBuilder GetItemsUri;
		private string baseUrl = "https://api.zotero.org";
		private IHttpClientProvider _httpProvider;
		//private AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

		// use singleton pattern here
		private static ZoteroService instance = null;
		private static readonly object padlock = new object();
		private const string TargetResultsHeader = "Total-Results";

		private ZoteroService()
		{

		}

		public static ZoteroService Instance
		{
			get
			{
				lock (padlock)
				{
					if (instance == null)
					{
						instance = new ZoteroService();
					}
					return instance;
				}
			}
		}

		public void SetZoteroServiceHttpProvider(IHttpClientProvider httpProvider)
		{
			//_retryPolicy = Policy
			//.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)			
			//.WaitAndRetryAsync(new[]
			//	{
			//		TimeSpan.FromSeconds(1),
			//		TimeSpan.FromSeconds(5),
			//		TimeSpan.FromSeconds(10)
			//	}, (exception, timeSpan, retryCount, context) =>
			//	{
			//		Console.Write("RETRYING - " + DateTime.Now.Second);
			//	});

			_httpProvider = httpProvider;
		}

		public async Task<HttpResponseMessage> GetTokenOauth(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			return response;
		}
		public async Task<string> DoGetReq(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return json;
		}


		public async Task<List<T>> GetCollections<T>(string requestUri)
		{
			var response =  await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			string json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<T>>(json);
		}

		public async Task<List<T>> GetPagedCollections<T>(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var TotalResultsHeader = response.Headers.FirstOrDefault(x => x.Key == TargetResultsHeader);
			var totalNumberOfItems = Convert.ToInt64(TotalResultsHeader.Value.FirstOrDefault());
			string json = await response.Content.ReadAsStringAsync();
			var listedItems = JsonConvert.DeserializeObject<List<T>>(json);
			int count = 0;
			long start = 25;
			long limit = 25;
			if (totalNumberOfItems > 25)
			{
				while (count < (totalNumberOfItems / 25))
				{
					var nextPagedRequest = requestUri + $"&start={start}";
					var pagedResponse = await _httpProvider.GetAsync(nextPagedRequest);
					json = await pagedResponse.Content.ReadAsStringAsync();
					var pagedItems = JsonConvert.DeserializeObject<List<T>>(json);
					listedItems.AddRange(pagedItems);
					start += 25;
					limit = totalNumberOfItems - start;
					count++;
				}
			}
			return listedItems;
		}

		public async Task<List<object>> GetItems(string requestUri)
		{
			GetItemsUri = new UriBuilder($"{baseUrl}/users/475425/collections/9KH9TNSJ/items");
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<object>>(json); ;
		}

		public async Task<HttpResponseMessage> CollectionPost(string payload, string requestUri)
		{
			HttpContent exampleCollection = new StringContent(payload, Encoding.UTF8, "application/json");

			var response = await _httpProvider.PostAsync(requestUri, exampleCollection);
			response.EnsureSuccessStatusCode();
			return response;
		}

		public async Task<HttpResponseMessage> CreateItem(string payload, string requestUri)
		{
			HttpContent exampleItem = new StringContent(payload, Encoding.UTF8, "application/json");

			var response = await _httpProvider.PostAsync(requestUri, exampleItem);
			response.EnsureSuccessStatusCode();
			return response;
		}

		public async Task<HttpResponseMessage> DeleteCollection(string requestUri)
		{
			var response = await _httpProvider.DeleteAsync(requestUri);
			response.EnsureSuccessStatusCode();
			return response;
		}

		public async Task<HttpResponseMessage> UpdateCollection<T>(string payload, string requestUri)
		{
			HttpContent exampleCollection = new StringContent(payload, Encoding.UTF8, "application/json");
			var response = await _httpProvider.PutAsync(requestUri, exampleCollection);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return response;
		}

		public async Task<HttpResponseMessage> UpdateZoteroItem<T>(string payload, string requestUri)
		{
			HttpContent exampleCollection = new StringContent(payload, Encoding.UTF8, "application/json");
			var response = await _httpProvider.PutAsync(requestUri, exampleCollection);
			response.EnsureSuccessStatusCode();
			return response;
		}

		public void Dispose()
		{
			_httpProvider?.Dispose();
		}

		public async Task<string> GetUserPermissions(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return json;
		}

		public async Task<JObject> GetItem(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JObject>(json); ;
		}

		public async Task<Collection> GetCollectionItem(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<Collection>(json); ;
		}

		public async Task<JObject> GetDocument(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JObject>(json); ;
		}

		public async Task<ApiKey> GetApiKey(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			var key = JsonConvert.DeserializeObject<ApiKey>(json);
			return key;
		}

		public async Task<bool> DeleteApiKey(string requestUri)
		{
			var response = await _httpProvider.DeleteAsync(requestUri);
			response.EnsureSuccessStatusCode();
			if (response.StatusCode == HttpStatusCode.NoContent)
			{ return true; }
			else
			{
				return false;
			}
		}

		public async Task<JArray> GetDocumentArray(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JArray>(json); ;
		}

		public async Task<HttpResponseMessage> GetDocumentHeader(string requestUri)
		{
			var response = await _httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			return response;
		}

		public async Task<JObject> POSTAuth(string payload, string requestUri)
		{
			HttpContent examplePDF = new StringContent(payload, Encoding.UTF8, "application/json");
			var response = await _httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JObject>(json);
		}

		public async Task<string> POSTOAuth(string payload, string requestUri)
		{
			HttpContent examplePDF = new StringContent(payload, Encoding.UTF8, "application/json");
			var response = await _httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return json;
		}

		public async Task<string> POSTForm(IEnumerable<KeyValuePair<string, string>> payload, string requestUri)
		{
			HttpContent examplePDF = new FormUrlEncodedContent(payload);
			var response = await _httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return json;
		}

		public async Task<string> POSTDocument(string payload, string requestUri)
		{
			HttpContent examplePDF = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
			var response = await _httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return json;
		}


		public async Task<JObject> POSTJDocument(string payload, string requestUri)
		{
			HttpContent examplePDF = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
			var response = await _httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JObject>(json);
		}
		public async Task<JObject> POSTFile(string payload, string requestUri, string contentType)
		{
			HttpContent examplePDF = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
			var response = await _httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JObject>(json);
		}

		//public async Task<MultipartMemoryStreamProvider> POSTFormDocument(string payload, string requestUri)
		//{
		//	HttpContent examplePDF = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
		//	var response = await _httpProvider.PostAsync(requestUri, examplePDF);
		//	response.EnsureSuccessStatusCode();
		//	var provider = new MultipartMemoryStreamProvider();
		//	var res = await response.Content.ReadAsMultipartAsync(provider);
		//	return res;
		//}

		public async Task<JObject> POSTFormMultiPart(IEnumerable<KeyValuePair<string, string>> payload, string requestUri)
		{
			HttpContent examplePDF = new FormUrlEncodedContent(payload);
			var response = await _httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JObject>(json);
		}

	}
}
