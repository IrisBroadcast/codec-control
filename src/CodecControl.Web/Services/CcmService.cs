using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.Interfaces;
using LazyCache;
using NLog;

namespace CodecControl.Web.Services
{
    public class CcmService : ICcmService
    {
        // TODO: Listen to CCM hub and reload list when necessary

        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly int _reloadIntervalInSeconds;
        private readonly ICcmRepository _ccmRepository;
        private readonly IAppCache _cache;

        public CcmService(ApplicationSettings appSettings, ICcmRepository ccmRepository, IAppCache cache)
        {
            log.Info("CCMService constructor");
            //_serviceScope = serviceProvider.CreateScope();
            _ccmRepository = ccmRepository;
            _cache = cache;
            _reloadIntervalInSeconds = appSettings.CcmCodecInformationReloadInterval;
        }

        public async Task<CodecInformation> GetCodecInformationBySipAddress(string sipAddress)
        {
            return (await GetCodecInformationList()).FirstOrDefault(c => c.SipAddress == sipAddress);

        }

        public async Task<List<CodecInformation>> GetCodecInformationList()
        {
            var list = await _cache.GetOrAddAsync(
                CacheKeys.Codecinformationlist,
                () => _ccmRepository.GetCodecInformationList(),
                DateTime.Now.AddSeconds(_reloadIntervalInSeconds));
            return list;
        }

    }
}