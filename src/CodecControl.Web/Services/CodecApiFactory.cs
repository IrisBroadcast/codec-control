using System;
using CodecControl.Client;
using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.SR.BaresipRest;
using CodecControl.Web.Interfaces;

namespace CodecControl.Web.Controllers
{
    public class CodecApiFactory
    {
        private readonly ICcmService _ccmService;
        private readonly IServiceProvider _serviceProvider;

        public CodecApiFactory(ICcmService ccmService, IServiceProvider serviceProvider)
        {
            _ccmService = ccmService;
            _serviceProvider = serviceProvider;
        }

        public ICodecApi CreateCodecApi(Type codecApiType)
        {
            return codecApiType != null ? _serviceProvider.GetService(codecApiType) as ICodecApi : null;
        }

    }
}