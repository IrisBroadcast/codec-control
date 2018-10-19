using System.IO;
using CodecControl.Web.Hub;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

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

            services.ConfigureDepencencyInjection();

            services.AddDirectoryBrowser();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .AllowAnyOrigin();
            }));

            services.AddLazyCache();
            services.AddSignalR();
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

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Serve log files as static files
            var logFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logFiles");
            Directory.CreateDirectory(logFolder);

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(logFolder),
                RequestPath = "/log",
                EnableDirectoryBrowsing = true,
                StaticFileOptions =
                {
                    ContentTypeProvider = new FileExtensionContentTypeProvider { Mappings = {[".log"] = "text/plain"}},
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
                    }
                }
            });

            app.UseCors("CorsPolicy");

            app.UseSignalR(routes =>
            {
                routes.MapHub<AudioStatusHub>("/audiostatusHub");
            });

            app.UseMvc();
        }
    }
}
