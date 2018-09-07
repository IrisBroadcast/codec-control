using System;

namespace CodecControl.Web
{
    public class ApplicationSettings
    {
        public string CcmHost { get; set; }

        public Uri CcmHostUri => new Uri(CcmHost);

        public int CcmCodecInformationReloadInterval { get; set; } = 60;
    }
}