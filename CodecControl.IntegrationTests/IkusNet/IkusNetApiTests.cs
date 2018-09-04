using System.Threading.Tasks;
using Xunit;
using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.Models;

namespace CodecControl.IntegrationTests
{
    public class IkusNetApiTests
    {
        private string _hostAddress;

        public IkusNetApiTests()
        {
            _hostAddress = "192.0.2.237";
        }

        [Fact]
        public async Task GetDeviceName()
        {
            var sut = new IkusNetApi();
            var deviceName = await sut.GetDeviceNameAsync(_hostAddress);
            Assert.Equal("MTU 25", deviceName);
        }

        [Fact]
        public async Task GetInputLevel()
        {
            var sut = new IkusNetApi();

            await sut.SetInputGainLevelAsync(_hostAddress, 0, 6);

            var level = await sut.GetInputGainLevelAsync(_hostAddress, 0);
            Assert.Equal(6, level);

            await sut.SetInputGainLevelAsync(_hostAddress, 0, 4);

            level = await sut.GetInputGainLevelAsync(_hostAddress, 0);
            Assert.Equal(4, level);

        }

        [Fact]
        public async Task GetLineStatus()
        {
            var sut = new IkusNetApi();

            LineStatus lineStatus = await sut.GetLineStatusAsync(_hostAddress, 0);
            Assert.Equal("", lineStatus.RemoteAddress);
            Assert.Equal(LineStatusCode.NoPhysicalLine, lineStatus.StatusCode);
            Assert.Equal(DisconnectReason.None, lineStatus.DisconnectReason);
        }

        //[Fact(Skip = "To avoid unintentional calling")]
        //public async Task Call()
        //{
        //    var sut = new IkusNetApi();

        //    var address = "sto-s17-01@acip.example.com";
        //    var profile = "Studio";

        //    bool result = await sut.CallAsync(_hostAddress, address, profile);
        //    Assert.True(result);
        //}

        //[Fact(Skip="To avoid unintentional hangup")]
        //public async Task Hangup()
        //{
        //    var sut = new IkusNetApi();
        //    bool result = await sut.HangUpAsync(_hostAddress);
        //    Assert.True(result);
        //}
    }
}