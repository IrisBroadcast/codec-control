using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Client.SR.BaresipRest;
using CodecControl.Web.CCM;
using CodecControl.Web.Hub;
using Microsoft.Extensions.DependencyInjection;

namespace CodecControl.Web
{
    public static class DependencyInjection
    {
        public static void ConfigureDepencencyInjection(this IServiceCollection services)
        {
            services.AddSingleton<CcmService>();
            services.AddTransient<CcmApiRepository>();

            services.AddSingleton<SocketPool>();
            services.AddSingleton<AudioStatusUpdater>();
            services.AddTransient<SocketProxy>();
            services.AddTransient<IkusNetApi>();
            services.AddTransient<BaresipRestApi>();
        }
    }
}