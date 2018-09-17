using CodecControl.Data.Database;
using CodecControl.IntegrationTests.Helpers;
using CodecControl.Web;
using CodecControl.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CodecControl.IntegrationTests
{
    public class CcmIntegrationTests
    {
        [RunnableInDebugOnlyFact]
        public void should_get_list_of_controllable_codecs_from_ccm()
        {
            var appSettings = new ApplicationSettings
            {
                CcmHost = "http://uccm.sr.se"
            };
            var connectionString = "";
            var serviceProvider = new ServiceCollection()
                .AddDbContext<CcmDbContext>(options =>
                {
                    options.UseMySql(connectionString);
                })
                .AddTransient<CcmDbContext>()
                .BuildServiceProvider();

            var sut = new CcmService(appSettings, serviceProvider);

            var list = sut.CodecInformationList;
            Assert.NotNull(list);
            Assert.False(string.IsNullOrEmpty(list[0].SipAddress));
        }
    }
}