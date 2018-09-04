using System;
using CodecControl.Client;

namespace CodecControl.Web.Controllers
{
    public class CodecApiFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CodecApiFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICodecApi CreateCodecApi(Type codecApiType)
        {
            return codecApiType != null ? _serviceProvider.GetService(codecApiType) as ICodecApi : null;
        }

    }
}