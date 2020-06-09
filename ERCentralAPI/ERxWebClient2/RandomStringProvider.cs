using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ERxWebClient2
{
    public class RandomStringProvider
    {

        private const string RandomStringUri =

                "https://www.random.org/strings/?num=1&len=8&digits=on&upperalpha=on&loweralpha=on&unique=on&format=plain&rnd=new";



        private readonly HttpClient _httpClient;


        public RandomStringProvider()

        {

            _httpClient = new HttpClient();

        }


        public async Task UpdateString(CancellationToken cancellationToken)
        {
            try

            {

                var response = await _httpClient.GetAsync(RandomStringUri, cancellationToken);

                if (response.IsSuccessStatusCode)

                {

                    RandomString = await response.Content.ReadAsStringAsync();

                }

            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex);

            }

        }


        public string RandomString { get; private set; } = string.Empty;

    }
}