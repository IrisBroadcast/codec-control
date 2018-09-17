﻿using System.Threading.Tasks;
using CodecControl.Data.Database;
using CodecControl.IntegrationTests.Helpers;
using CodecControl.Web;
using CodecControl.Web.CCM;
using LazyCache;
using LazyCache.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CodecControl.IntegrationTests
{
    public class CcmIntegrationTests
    {
        //[RunnableInDebugOnlyFact]
        //public async Task should_get_list_of_controllable_codecs_from_ccm()
        //{
        //    var appSettings = new ApplicationSettings
        //    {
        //        CcmHost = "http://uccm.sr.se"
        //    };
        //    var connectionString = "";
        //    var serviceProvider = new ServiceCollection()
        //        .AddDbContext<CcmDbContext>(options =>
        //        {
        //            options.UseMySql(connectionString);
        //        })
        //        .AddTransient<CcmDbContext>()
        //        .BuildServiceProvider();

        //    var sut = new CcmService(appSettings, serviceProvider, new MockCachingService()) )

        //    var list = await sut.GetCodecInformationList();
        //    Assert.NotNull(list);
        //    Assert.False(string.IsNullOrEmpty(list[0].SipAddress));
        //}
    }
}