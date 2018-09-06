using CodecControl.Client.Prodys.IkusNet;
using CodecControl.Web.Controllers;
using CodecControl.Web.Interfaces;
using CodecControl.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodecControl.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ApplicationSettings appSettings = new ApplicationSettings();
            Configuration.GetSection("Application").Bind(appSettings);
            services.AddSingleton(appSettings);

            // Dependency injection
            services.AddSingleton<ICcmService, CcmService>();
            services.AddSingleton<SocketPool>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
