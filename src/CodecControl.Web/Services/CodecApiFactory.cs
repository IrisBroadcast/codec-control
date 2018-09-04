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

        //public ICodecApi GetCodecApi(string sipAddress)
        //{
        //    var codecInfo = GetCodecInformationBySipAddress(sipAddress);
        //    var codecApi = CreateCodecApi(codecInfo);
        //    return codecApi;
        //}

        //public CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        //{
        //    var codecInformation = _ccmService.GetCodecInformationBySipAddress(sipAddress);

        //    if (codecInformation == null)
        //    {
        //        // TODO: Log as warning
        //        return null;
        //    }

        //    return string.IsNullOrEmpty(codecInformation.Api) ? null : codecInformation;
        //}

        public ICodecApi CreateCodecApi(CodecInformation codecInformation)
        {
            switch (codecInformation.Api)
            {
                case "IkusNet":
                    return (ICodecApi)_serviceProvider.GetService(typeof(IkusNetApi));
                case "BaresipRest":
                    return (ICodecApi)_serviceProvider.GetService(typeof(BaresipRestApi));
                default:
                    // TODO: Log as warning
                    return null;
            }
        }

    }
}