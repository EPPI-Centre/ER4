using ERxWebClient2.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace ER_Web.Services
{
	public sealed class ZoteroService : IZoteroService
    {
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
			var response = await httpProvider.GetAsync(requestUri);
			response.EnsureSuccessStatusCode();
			string json = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<T>>(json);
		}

		public async Task<List<T>> GetPagedCollections<T>(string requestUri, IHttpClientProvider httpProvider)
		{
			//var APIwatch = new System.Diagnostics.Stopwatch();
			//var ParseWatch = new System.Diagnostics.Stopwatch();
			long batchSize = 100;
			long start = batchSize;

			//APIwatch.Start();
			var response = await httpProvider.GetAsync(requestUri + $"&start=0&limit={batchSize}");
			response.EnsureSuccessStatusCode();
			var TotalResultsHeader = response.Headers.FirstOrDefault(x => x.Key == TargetResultsHeader);
			var totalNumberOfItems = Convert.ToInt64(TotalResultsHeader.Value.FirstOrDefault());
			string json = await response.Content.ReadAsStringAsync();
			//APIwatch.Stop();
			//ParseWatch.Start();
			var listedItems = JsonConvert.DeserializeObject<List<T>>(json);
			//ParseWatch.Stop();
			int count = 1;
			if (totalNumberOfItems > batchSize)
			{
				int totPages = (int)Math.Ceiling((double)(totalNumberOfItems / batchSize));
                //while (listedItems.Count < totalNumberOfItems)
                while (count <= totPages)//we already got the first page!
				{
                    var nextPagedRequest = requestUri + $"&start={start}&limit={batchSize}";
					//APIwatch.Start();
					var pagedResponse = await httpProvider.GetAsync(nextPagedRequest);
					json = await pagedResponse.Content.ReadAsStringAsync();
					pagedResponse.EnsureSuccessStatusCode();
					//APIwatch.Stop();
					//ParseWatch.Start();
					var pagedItems = JsonConvert.DeserializeObject<List<T>>(json);
					//ParseWatch.Stop();
					listedItems.AddRange(pagedItems);
					start += batchSize;
					//limit = totalNumberOfItems - start;
					count++;
				}
			}
			//var APItime = APIwatch.ElapsedMilliseconds / 1000;
			//var Parsetime = ParseWatch.ElapsedMilliseconds / 1000;
			//System.Diagnostics.Debug.WriteLine("APItime: " + APItime.ToString());
			//System.Diagnostics.Debug.WriteLine("Parsetime: " + Parsetime.ToString());
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
        public async Task<HttpResponseMessage> UpdatePartialItems(string payload, string requestUri, IHttpClientProvider httpProvider)
        {
            HttpContent exampleItem = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await httpProvider.PostAsync(requestUri, exampleItem);
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

		public async Task<string> POSTJDocument(string payload, string requestUri, IHttpClientProvider httpProvider)
		{
			HttpContent examplePDF = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
			var response = await httpProvider.PostAsync(requestUri, examplePDF);
			response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
            //var json = await response.Content.ReadAsStringAsync();
            //return JsonConvert.DeserializeObject<JObject>(json);
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
