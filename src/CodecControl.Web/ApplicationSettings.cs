using System;

namespace CodecControl.Web
{
    public class ApplicationSettings
    {
        public string CcmHost { get; set; }
        public int CcmCodecInformationReloadInterval { get; set; } = 60;
        public string [] CorsPolicyOrigins { get; set; }
        public string AuthenticatedUserName { get; set; }
        public string AuthenticatedPassword { get; set; }
        public string ReleaseDate { get; set; }
        public string Version { get; set; }

        public Uri CcmHostUri => new Uri(CcmHost);

    }
} 