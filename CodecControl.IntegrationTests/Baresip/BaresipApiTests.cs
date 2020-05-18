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
using CodecControl.Client.Models;
using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.SR.BaresipRest;
using Xunit;

namespace CodecControl.IntegrationTests.Baresip
{
    public class BaresipApiTests
    {
        private readonly string _ip;

        public BaresipApiTests()
        {
            _ip = "134.25.127.231";

        }

        [Fact]
        public async Task IsAvailable()
        {
            var sut = new BaresipRestApi();
            var success = await sut.CheckIfAvailableAsync(_ip);
            Assert.True(success);
        }

        [Fact]
        public async Task SetInputEnabled()
        {
            var sut = new BaresipRestApi();
            var success = await sut.SetInputEnabledAsync(_ip, 0, true);
            Assert.True(success);
        }

        [Fact]
        public async Task GetInputLevel()
        {
            var sut = new IkusNetApi(new ProdysSocketPool());

            await sut.SetInputGainLevelAsync(_ip, 0, 6);

            var level = await sut.GetInputGainLevelAsync(_ip, 0);
            Assert.Equal(6, level);

            await sut.SetInputGainLevelAsync(_ip, 0, 4);

            level = await sut.GetInputGainLevelAsync(_ip, 0);
            Assert.Equal(4, level);

        }

        [Fact]
        public async Task GetLineStatus()
        {
            var sut = new IkusNetApi(new ProdysSocketPool());

            LineStatus lineStatus = await sut.GetLineStatusAsync(_ip);
            Assert.Equal(LineStatusCode.NoPhysicalLine, lineStatus.StatusCode);
            Assert.Equal(DisconnectReason.None, lineStatus.DisconnectReason);
        }

        [Fact(Skip = "To avoid unintentional calling")]
        public async Task Call()
        {
            var sut = new IkusNetApi(new ProdysSocketPool());

            var callee = "sto-s17-01@acip.example.com";
            var profileName = "Studio";
            var deviceEncoder = "Program";

            bool result = await sut.CallAsync(_ip, callee, profileName, deviceEncoder);
            Assert.True(result);
        }

        [Fact(Skip = "To avoid unintentional hangup")]
        public async Task Hangup()
        {
            var sut = new IkusNetApi(new ProdysSocketPool());
            bool result = await sut.HangUpAsync(_ip);
            Assert.True(result);
        }
    }
}
