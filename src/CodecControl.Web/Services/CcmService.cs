using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.Interfaces;
using Newtonsoft.Json;

namespace CodecControl.Web.Services
{
    public class CcmService : ICcmService
    {
        private readonly ApplicationSettings _appSettings;
        // TODO: Listen to CCM hub and reload list when necessary

        private List<CodecInformation> _codecInformationList;
        private DateTime _discardTime;
        private readonly int _reloadIntervalInSeconds = 60;

        public CcmService(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
            _discardTime = DateTime.Now;
        }

        public List<CodecInformation> CodecInformationList
        {
            get
            {
                if (DateTime.Now > _discardTime)
                {
                    _codecInformationList = null;
                }
                return _codecInformationList ?? (_codecInformationList = LoadCodecInformationListAsync().Result);
            }
        }

        private async Task<List<CodecInformation>> LoadCodecInformationListAsync()
        {
            // TODO: Connect to CCM and retrieve codec information
            var client = new HttpClient();

            var uri = new Uri(_appSettings.CcmHostUri, "api/codecinformation");

            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                List<CodecInformation> codecInformationList = JsonConvert.DeserializeObject<List<CodecInformation>>(stringData);

                _discardTime = DateTime.Now.AddSeconds(_reloadIntervalInSeconds);
                return codecInformationList;
            }
            else
            {
                return new List<CodecInformation>();
            }
        }

      
        public CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            return CodecInformationList.FirstOrDefault(c => c.SipAddress == sipAddress);

        }
    }
}