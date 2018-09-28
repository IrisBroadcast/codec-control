using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodecControl.Client.Exceptions;
using Newtonsoft.Json;

namespace CodecControl.Client.SR.BaresipRest
{
    public class HttpService
    {
        public static async Task<T> GetWithBaresipResponseAsync<T>(Uri url) where T : BaresipResponse
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(4);
                var response = await client.GetAsync(url);
                return await ParseResponse<T>(response);
            }
        }

        public static async Task<T> PostWithBaresipResponseAsync<T>(Uri url, object data = null) where T : BaresipResponse
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(4);

                StringContent content;
                if (data != null)
                {
                    var s = JsonConvert.SerializeObject(data);
                    content = new StringContent(s, Encoding.UTF8, "application/json");
                }
                else
                {
                    content = null;
                }

                var response = await client.PostAsync(url, content);
                return await ParseResponse<T>(response);
            }
        }

        private static async Task<T> ParseResponse<T>(HttpResponseMessage response) where T : BaresipResponse
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new CodecInvocationException($"Request to {response.RequestMessage.RequestUri} returned HTTP status {response.StatusCode}");
            }

            string jsonString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<T>(jsonString);

            if (content == null)
            {
                throw new CodecInvocationException($"Response from {response.RequestMessage.RequestUri} was invalid");
            }

            if (!content.Success)
            {
                throw new CodecInvocationException($"Response from {response.RequestMessage.RequestUri} indicates operation was unsuccessful");
            }

            return content;
        }

    }
}