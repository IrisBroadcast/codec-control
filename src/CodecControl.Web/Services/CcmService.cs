using System;
using System.Collections.Generic;
using System.Linq;
using CodecControl.Client;
using CodecControl.Web.Interfaces;

namespace CodecControl.Web.Services
{
    public class CcmService : ICcmService
    {
        // TODO: Listen to CCM hub and reload list when necessary

        private List<CodecInformation> _codecInformationList;
        private DateTime discardTime;
        private readonly int _reloadIntervalInSeconds = 60;

        public List<CodecInformation> CodecInformationList
        {
            get
            {
                if (DateTime.Now > discardTime)
                {
                    _codecInformationList = null;
                }
                return _codecInformationList ?? (_codecInformationList = LoadCodecInformationList());
            }
        }

        private List<CodecInformation> LoadCodecInformationList()
        {
            // TODO: Connect to CCM and retrieve codec information
            var codecInformationList = new List<CodecInformation>();

            discardTime = DateTime.Now.AddSeconds(_reloadIntervalInSeconds);
            return codecInformationList;
        }

        public CcmService()
        {
            discardTime = DateTime.Now;
        }

        public CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            return CodecInformationList.FirstOrDefault(c => c.SipAddress == sipAddress);

        }
    }
}