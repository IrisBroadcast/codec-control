using CodecControl.Web;
using CodecControl.Web.Services;
using Xunit;

namespace CodecControl.IntegrationTests.Baresip
{
    public class CcmIntegrationTests
    {
        [Fact(Skip = "Integration test")]
        public void should_get_list_of_controllable_codecs_from_ccm()
        {
            var appSettings = new ApplicationSettings
            {
                CcmHost = "http://uccm.sr.se"
            };
            var sut = new CcmService(appSettings);

            var list = sut.CodecInformationList;
            Assert.NotNull(list);
            Assert.False(string.IsNullOrEmpty(list[0].SipAddress));
        }
    }
}