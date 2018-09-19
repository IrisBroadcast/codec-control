using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.Interfaces;
using Newtonsoft.Json;
using NLog;

namespace CodecControl.Web.CCM
{
    public class CcmApiRepository : ICcmRepository
    {
        private readonly ApplicationSettings _applicationSettings;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public CcmApiRepository(ApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }

        public async Task<List<CodecInformation>> GetCodecInformationList()
        {
            using (new TimeMeasurer("Load codec information from CCM"))
            {
                Uri uri = null;
                try 
                {
                    log.Info("Loading codec information from CCM");
                    var client = new HttpClient();

                    uri = new Uri(_applicationSettings.CcmHostUri, "api/codecinformation");

                    var response = await client.GetAsync(uri);

                    if (!response.IsSuccessStatusCode)
                    {
                        log.Warn("Failed to retrieve codec information list from CCM");
                        return new List<CodecInformation>();
                    }

                    string stringData = await response.Content.ReadAsStringAsync();
                    var codecInformationList = JsonConvert.DeserializeObject<List<CodecInformation>>(stringData);
                    log.Info($"Found information for #{codecInformationList.Count} codecs in CCM list");

                    return codecInformationList;
                }
                catch (Exception ex)
                {
                    log.Error(ex, $"Exception when retrieving codec information from CCM ({uri?.AbsoluteUri})");
                    return new List<CodecInformation>();
                }
            }
        }

    }
}