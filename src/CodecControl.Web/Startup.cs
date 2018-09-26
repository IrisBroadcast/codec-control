﻿using System.IO;
using CodecControl.Data.Database;
using CodecControl.Web.Hub;
using CodecControl.Web.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
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

            var connectionString = Configuration.GetConnectionString("CcmDatabase");
            services.AddDbContext<CcmDbContext>(options =>
            {
                options.UseMySql(connectionString);
            });

            services.ConfigureDepencencyInjection();

            services.AddDirectoryBrowser();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    //.WithOrigins(appSettings.CorsPolicyOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .AllowAnyOrigin()
                    ;
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
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logFiles")),
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
