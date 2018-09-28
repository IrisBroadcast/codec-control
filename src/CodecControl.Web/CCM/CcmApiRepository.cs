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