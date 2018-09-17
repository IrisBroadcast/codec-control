using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;

namespace CodecControl.Web.Services
{
    public class CcmService : ICcmService, IDisposable
    {
        
        // TODO: Listen to CCM hub and reload list when necessary

        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly int _reloadIntervalInSeconds;
        private readonly ApplicationSettings _appSettings;
        private List<CodecInformation> _codecInformationList;
        private DateTime _discardTime;
        private IServiceScope _serviceScope;
        
        public CcmService(ApplicationSettings appSettings, IServiceProvider serviceProvider)
        {
            log.Info("CCMService constructor");
            _serviceScope = serviceProvider.CreateScope();
            _appSettings = appSettings;
            _reloadIntervalInSeconds = appSettings.CcmCodecInformationReloadInterval;
            _discardTime = DateTime.Now;
        }

        public List<CodecInformation> CodecInformationList
        {
            get
            {
                // TODO: Discard old list only when new list has been successfully downloaded from CCM.
                if (DateTime.Now > _discardTime)
                {
                    log.Info("Discarding current CCM list");
                    _codecInformationList = null;
                }
                return _codecInformationList ?? (_codecInformationList = LoadCodecInformationListAsync().Result);
            }
        }

        private async Task<List<CodecInformation>> LoadCodecInformationListAsync()
        {
            using (new TimeMeasurer("Load codec information from CCM"))
            {
                Uri uri = null;
                try
                {
                    log.Info("Loading codec information from CCM");
                    var client = new HttpClient();

                    uri = new Uri(_appSettings.CcmHostUri, "api/codecinformation");

                    var response = await client.GetAsync(uri);

                    if (!response.IsSuccessStatusCode)
                    {
                        log.Warn("Failed to retrieve codec information list from CCM");
                        return new List<CodecInformation>();
                    }

                    string stringData = await response.Content.ReadAsStringAsync();
                    var codecInformationList = JsonConvert.DeserializeObject<List<CodecInformation>>(stringData);
                    log.Info($"Found information for #{codecInformationList.Count} codecs in CCM list");

                    _discardTime = DateTime.Now.AddSeconds(_reloadIntervalInSeconds);
                    return codecInformationList;
                }
                catch (Exception ex)
                {
                    log.Error(ex, $"Exception when retrieving codec information from CCM ({uri?.AbsoluteUri})");
                    return new List<CodecInformation>();
                }
            }
        }

      
        public CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            return CodecInformationList.FirstOrDefault(c => c.SipAddress == sipAddress);

        }

        public void Dispose()
        {
            _serviceScope?.Dispose();
        }
    }
}