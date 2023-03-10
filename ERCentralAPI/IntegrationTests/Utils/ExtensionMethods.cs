using Azure;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace IntegrationTests.Utils
{
    public static class ExtensionMethods
    {
        public static  Task<T?> GetAndDeserialize<T>(this HttpClient client, string requestUri)
        {
            //var response = await client.GetAsync(requestUri);
            //response.EnsureSuccessStatusCode();
            //var result = await response.Content.ReadAsStringAsync();
            //return JsonConvert.DeserializeObject<T>(result);
            return client.GetFromJsonAsync<T>(requestUri);
        }
        public static async Task<JsonNode?> GetAndDeserialize(this HttpClient client, string requestUri)
        {
            var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonNode.Parse(result);
            //return client.GetFromJsonAsync<T>(requestUri);
        }
        public static async Task<T?> PostAndDeserialize<T>(this HttpClient client, string requestUri, object Values)
        {
            var resp = await client.PostAsJsonAsync(requestUri, Values);
            resp.EnsureSuccessStatusCode();
            var result = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }
        public static async Task<JsonNode?> PostAndDeserialize(this HttpClient client, string requestUri, object Values)
        {
            StringContent strContent = MakeHttpContent(Values);
            var response = await client.PostAsync(requestUri, strContent);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonNode.Parse(result);
            //return client.GetFromJsonAsync<T>(requestUri);
        }
        public static async Task<JsonNode?> PostAndDeserializeWithoutDevolvedErrorHandling(this HttpClient client, string requestUri, object Values)
        {
            StringContent strContent = MakeHttpContent(Values);
            var response = await client.PostAsync(requestUri, strContent);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonNode.Parse(result);
            } 
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new Exception(result);
            }
            //return client.GetFromJsonAsync<T>(requestUri);
        }

        public static StringContent MakeHttpContent(this object inputObj)
        {
            var jsonString = JsonConvert.SerializeObject(inputObj);
            StringContent strContent = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            return strContent;
        }
    }
}
