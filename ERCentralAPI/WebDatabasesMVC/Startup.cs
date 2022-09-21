using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebDatabasesMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }
        public static ILogger<Program> Logger { get; private set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("CookieAuthentication")
                .AddCookie("CookieAuthentication", config =>
                {
                    config.Cookie.Name = "WebDbErLoginCookie";
                    config.LoginPath = "/Login";
                })
                .AddCookie("FairAuthentication", config =>
                {
                    config.Cookie.Name = "WebDbErLoginCookieF";
                    config.LoginPath = "/Fair/Login";
                }); ;
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {//this is needed to allow serialising CSLA child objects:
                //they all have a "Parent" field which creates a reference loop.
                options.SerializerSettings.CheckAdditionalContent = true;

                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Program> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            Logger = logger;
            Program.SqlHelper = new SQLHelper((IConfigurationRoot)Startup.Configuration, logger);
            BusinessLibrary.Data.DataConnection.DataConnectionConfigure(Program.SqlHelper);
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
