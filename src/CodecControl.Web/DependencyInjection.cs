using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.SR.BaresipRest;
using CodecControl.Data.Database;
using CodecControl.Web.AudioStatus;
using CodecControl.Web.Interfaces;
using CodecControl.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CodecControl.Web
{
    public static class DependencyInjection
    {
        public static void ConfigureDepencencyInjection(this IServiceCollection services)
        {
            services.AddTransient<CcmDbContext>();
            services.AddSingleton<ICcmService, CcmService>();
            services.AddSingleton<SocketPool>();
            services.AddSingleton<AudioStatusUpdater>();
            services.AddTransient<SocketProxy>();
            services.AddTransient<IkusNetApi>();
            services.AddTransient<BaresipRestApi>();
        }
    }
}