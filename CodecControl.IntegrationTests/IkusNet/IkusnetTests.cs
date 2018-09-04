using Xunit;
using CodecControl.Client;

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
        
        
    
        private ICodecManager GetCodecManager()
        {
            return new CodecManager();
        }

        [Fact]
        public void Ikusnet_GetLineStatus()
        {
            var manager = GetCodecManager();
            var lineStatus = manager.GetLineStatusAsync(CodecInformation, 0);
            Assert.NotNull(lineStatus);
        }

    }
}