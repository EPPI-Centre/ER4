using ERxWebClient2.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace ERxWebClient2.Services
{
	public sealed class ZoteroService : IZoteroService
	{
		public UriBuilder GetCollectionsUri;
		public UriBuilder GetItemsUri;		
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

		public async Task<string> DoGetReq(string requestUri, IHttpClientProvider httpProvider)
		{
			var response = await httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return json;
		}


		public async Task<List<T>> GetCollections<T>(string requestUri, IHttpClientProvider httpProvider)
		{
			var response =  await httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			string json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<T>>(json);
		}

		public async Task<List<T>> GetPagedCollections<T>(string requestUri, IHttpClientProvider httpProvider)
		{
			var response = await httpProvider.GetAsync(requestUri);
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
					var pagedResponse = await httpProvider.GetAsync(nextPagedRequest);
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


		public async Task<HttpResponseMessage> CreateItem(string payload, string requestUri, IHttpClientProvider httpProvider)
		{
			HttpContent exampleItem = new StringContent(payload, Encoding.UTF8, "application/json");

			var response = await httpProvider.PostAsync(requestUri, exampleItem);
			response.EnsureSuccessStatusCode();
			return response;
		}

		public async Task<HttpResponseMessage> UpdateItem(string payload, string requestUri, IHttpClientProvider httpProvider)
		{
			HttpContent exampleItem = new StringContent(payload, Encoding.UTF8, "application/json");

			var response = await httpProvider.PutAsync(requestUri, exampleItem);
			response.EnsureSuccessStatusCode();
			return response;
		}


		public async Task<string> GetUserPermissions(string requestUri, IHttpClientProvider httpProvider)
		{
			var response = await httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return json;
		}

		public async Task<JObject> GetItem(string requestUri, IHttpClientProvider httpProvider)
		{
			var response = await httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JObject>(json); ;
		}

		public async Task<Collection> GetCollectionItem(string requestUri, IHttpClientProvider httpProvider)
		{
			var response = await httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<Collection>(json); ;
		}

		public async Task<bool> DeleteApiKey(string requestUri, IHttpClientProvider httpProvider)
		{
			var response = await httpProvider.DeleteAsync(requestUri);
			response.EnsureSuccessStatusCode();
			if (response.StatusCode == HttpStatusCode.NoContent)
			{ return true; }
			else
			{
				return false;
			}
		}


		public async Task<HttpResponseMessage> GetDocumentHeader(string requestUri, IHttpClientProvider httpProvider)
		{
				var response = await httpProvider.GetAsync(requestUri);
				response.EnsureSuccessStatusCode();
				return response;
		}

		public async Task<string> POSTDocument(string payload, string requestUri, IHttpClientProvider httpProvider)
		{
			HttpContent examplePDF = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
			var response = await httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return json;
		}

		public async Task<JObject> POSTJDocument(string payload, string requestUri, IHttpClientProvider httpProvider)
		{
			HttpContent examplePDF = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
			var response = await httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JObject>(json);
		}

		public async Task<JObject> POSTFormMultiPart(IEnumerable<KeyValuePair<string, string>> payload, string requestUri, IHttpClientProvider httpProvider)
		{
			HttpContent examplePDF = new FormUrlEncodedContent(payload);
			var response = await httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
			var json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<JObject>(json);
		}

	}
}
