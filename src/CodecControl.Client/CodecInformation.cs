using System;
using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.SR.BaresipRest;
using Newtonsoft.Json;

namespace CodecControl.Client
{
    public class CodecInformation
    {
        public string SipAddress { get; set; }
        public string Ip { get; set; }
        public string Api { get; set; }
        public string GpoNames { get; set; }
        public int NrOfInputs { get; set; }
        public int NrOfGpos { get; set; }

        [JsonIgnore]
        public Type CodecApiType
        {
            get
            {
                if (Enum.TryParse(Api, true, out CodecApiTypes apiType))
                {
                    switch (apiType)
                    {
                        case CodecApiTypes.IkusNet:
                            return typeof(IkusNetApi);
                        case CodecApiTypes.IkusNetSt:
                            return typeof(IkusNetStApi);
                        case CodecApiTypes.BareSipRest:
                            return typeof(BaresipRestApi);
                        default:
                            // TODO: Log as warning
                            return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }
}