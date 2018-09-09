using System;
using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.SR.BaresipRest;

namespace CodecControl.Client
{
    public class CodecInformation
    {
        public string SipAddress { get; set; }
        public string Ip { get; set; }
        public string Api { get; set; }
        public string GpoNames { get; set; }
        public int NrOfInputs { get; set; }

        public Type CodecApiType
        {
            get
            {
                Enum.TryParse(Api, true, out CodecApiTypes apiType);

                switch (apiType)
                {
                    case CodecApiTypes.IkusNet:
                        return typeof(IkusNetApi);
                    case CodecApiTypes.IkusNetST:
                        return typeof(IkusNetBaseApi);
                    case CodecApiTypes.BareSipRest:
                        return typeof(BaresipRestApi);
                    default:
                        // TODO: Log as warning
                        return null;
                }
            }
        }
    }
}