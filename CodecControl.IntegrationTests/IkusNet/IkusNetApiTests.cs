#region copyright
/*
 * Copyright (c) 2018 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
 #endregion

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