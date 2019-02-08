using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Client.Models;
using CodecControl.Client.Prodys.IkusNet;
using Xunit;

namespace CodecControl.IntegrationTests.IkusNet
{
    public class IkusNetApiTests
    {
        //private string _hostAddress;

        private CodecInformation CodecInformation => new CodecInformation()
        {
            SipAddress = "mtu-25@contrib.sr.se",
            Api = "IkusNet",
            Ip = "134.25.127.236"
        };

        public IkusNetApiTests()
        {
            //_hostAddress = "192.0.2.237";
        }

        [Fact]
        public async Task GetDeviceName()
        {
            var sut = new IkusNetApi(new SocketPool());
            var deviceName = await sut.GetDeviceNameAsync(CodecInformation.Ip);
            Assert.Equal("MTU 25", deviceName);
        }

        [Fact]
        public async Task GetInputLevel()
        {
            var sut = new IkusNetApi(new SocketPool());

            await sut.SetInputGainLevelAsync(CodecInformation.Ip, 0, 6);

            var level = await sut.GetInputGainLevelAsync(CodecInformation.Ip, 0);
            Assert.Equal(6, level);

            await sut.SetInputGainLevelAsync(CodecInformation.Ip, 0, 4);

            level = await sut.GetInputGainLevelAsync(CodecInformation.Ip, 0);
            Assert.Equal(4, level);

        }

        [Fact]
        public async Task GetLineStatus()
        {
            var sut = new IkusNetApi(new SocketPool());

            LineStatus lineStatus = await sut.GetLineStatusAsync(CodecInformation.Ip);
            Assert.Equal(LineStatusCode.NoPhysicalLine, lineStatus.StatusCode);
            Assert.Equal(DisconnectReason.None, lineStatus.DisconnectReason);
        }

        //[Fact(Skip = "To avoid unintentional calling")]
        //public async Task Call()
        //{
        //    var sut = new IkusNetApi();

        //    var address = "sto-s17-01@acip.example.com";
        //    var profile = "Studio";
        //    var whichCodec = "Program";

        //    bool result = await sut.CallAsync(_hostAddress, address, profile, whichCodec);
        //    Assert.True(result);
        //}

        //[Fact(Skip="To avoid unintentional hangup")]
        //public async Task Hangup()
        //{
        //    var sut = new IkusNetApi();
        //    bool result = await sut.HangUpAsync(_hostAddress);
        //    Assert.True(result);
        //}


        [Fact]
        public async Task Ikusnet_GetLineStatus()
        {
            var codecApi = new IkusNetApi(new SocketPool());
            var lineStatus = await codecApi.GetLineStatusAsync(CodecInformation.Ip);
            Assert.NotNull(lineStatus);
        }

    }
}