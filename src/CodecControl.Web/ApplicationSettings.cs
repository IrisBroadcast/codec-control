using System;

namespace CodecControl.Web
{
    public class ApplicationSettings
    {
        public string Version { get; set; }
        public string CcmHost { get; set; }
        public int CcmCodecInformationReloadInterval { get; set; } = 60;
        public string CorsPolicyOrigins { get; set; }
        public Uri CcmHostUri => new Uri(CcmHost);
    }
} 