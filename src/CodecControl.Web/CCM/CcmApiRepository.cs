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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CodecControl.Client;
using Newtonsoft.Json;
using NLog;

namespace CodecControl.Web.CCM
{
    public class CcmApiRepository
    {
        private readonly ApplicationSettings _applicationSettings;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public CcmApiRepository(ApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }

        public async Task<CodecInformation> GetCodecInformation(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return null;
            }

            using (new TimeMeasurer($"Load codec information for {sipAddress} from CCM"))
            {
                Uri uri = null;
                try
                {
                    var client = new HttpClient();

                    uri = new Uri(_applicationSettings.CcmHostUri, "api/codecinformation?sipaddress=" + sipAddress);

                    var response = await client.GetAsync(uri);

                    if (!response.IsSuccessStatusCode)
                    {
                        log.Warn("Failed to retrieve codec information from CCM");
                        return null;
                    }

                    string stringData = await response.Content.ReadAsStringAsync();
                    var codecInformation = JsonConvert.DeserializeObject<CodecInformation>(stringData);
                    return codecInformation;
                }
                catch (Exception ex)
                {
                    log.Error(ex, $"Exception when retrieving codec information from CCM ({uri?.AbsoluteUri})");
                    return null;
                }
            }
        }
    }
}