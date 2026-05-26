using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

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
                })
                .AddCookie("VawgAuthentication", config =>
                {
                    config.Cookie.Name = "WebDbErLoginCookieVawg";
                    config.LoginPath = "/Vawg/Login";
                });
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 20,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));
                options.AddPolicy("costly", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 1,
                            QueueLimit = 0,
                            Window = TimeSpan.FromSeconds(10)
                        }));
                options.OnRejected = async (context, cancellationToken) =>
                {
                    // Custom rejection handling logic
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers["Retry-After"] = "60";

                    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);

                    // Optional logging
                    Logger.LogError("Rate limit exceeded for IP: {IpAddress}",
                        context.HttpContext.Connection.RemoteIpAddress);
                };
            });

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
            app.UseRateLimiter();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
