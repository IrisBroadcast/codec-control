#region copyright
/*
 * Copyright (c) 2018 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
 #endregion

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