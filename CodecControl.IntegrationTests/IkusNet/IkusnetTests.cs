using Xunit;
using CodecControl.Client;
using CodecControl.Client.Prodys.IkusNet;

namespace CodecControl.IntegrationTests
{
   
    public class IkusnetTests
    {
        

        private static readonly CodecInformation CodecInformation = new CodecInformation
        {
            SipAddress = "sto-s17-01@acip.example.com",
            Api = "IkusNet",
            Ip = "192.0.2.30" // sto-s17-01
        };
        

        [Fact]
        public void Ikusnet_GetLineStatus()
        {
            var codecApi = new IkusNetApi();
            var lineStatus = codecApi.GetLineStatusAsync(CodecInformation.Ip, 0);
            Assert.NotNull(lineStatus);
        }

    }
}