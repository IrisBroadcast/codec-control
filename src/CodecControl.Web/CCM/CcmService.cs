using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.Cache;
using LazyCache;
using NLog;

namespace CodecControl.Web.CCM
{
    public class CcmService
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly int _reloadIntervalInSeconds;
        private readonly CcmApiRepository _ccmRepository;
        private readonly IAppCache _cache;

        public CcmService(ApplicationSettings appSettings, CcmApiRepository ccmRepository, IAppCache cache)
        {
            log.Info("CCMService constructor");
            _ccmRepository = ccmRepository;
            _cache = cache;
            _reloadIntervalInSeconds = appSettings.CcmCodecInformationReloadInterval;
        }

        public async Task<CodecInformation> GetCodecInformationBySipAddress(string sipAddress)
        {
            var ci = await _cache.GetOrAdd(
                CacheKeys.CodecInformation(sipAddress),
                () => _ccmRepository.GetCodecInformation(sipAddress),
                DateTime.Now.AddSeconds(_reloadIntervalInSeconds));
            return ci;
        }

    }
}