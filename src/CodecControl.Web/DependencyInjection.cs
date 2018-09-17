using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.SR.BaresipRest;
using CodecControl.Data.Database;
using CodecControl.Web.AudioStatus;
using CodecControl.Web.CCM;
using Microsoft.Extensions.DependencyInjection;

namespace CodecControl.Web
{
    public static class DependencyInjection
    {
        public static void ConfigureDepencencyInjection(this IServiceCollection services)
        {
            services.AddSingleton<CcmService>();
            services.AddTransient<ICcmRepository, CcmDbRepository>();
            //services.AddTransient<ICcmRepository, CcmApiRepository>();
            services.AddTransient<CcmDbContext>();

            services.AddSingleton<SocketPool>();
            services.AddSingleton<AudioStatusUpdater>();
            services.AddTransient<SocketProxy>();
            services.AddTransient<IkusNetApi>();
            services.AddTransient<BaresipRestApi>();
        }
    }
}