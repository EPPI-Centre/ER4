using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Authentication;
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
            //Rate Limiting: first, get the values we want from Configuration
            var RateLimitingSettings = Configuration.GetSection("RateLimiting")?.GetChildren();
            List<RateLimiterFixedWindowPolicySetting> allSettings = new List<RateLimiterFixedWindowPolicySetting>();
            foreach (var setting in RateLimitingSettings)
            {
                RateLimiterFixedWindowPolicySetting tempOptions = new RateLimiterFixedWindowPolicySetting();
                setting.Bind(tempOptions);
                allSettings.Add(tempOptions);
            }
            services.AddRateLimiter(options => configureRateLimits(options, allSettings));

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

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        private void configureRateLimits(RateLimiterOptions options, List<RateLimiterFixedWindowPolicySetting> allSettings)
        {
            foreach (RateLimiterFixedWindowPolicySetting rlf in allSettings)
            {
                if (rlf.PolicyName == "Global")
                {
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = rlf.PermitLimit,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(rlf.WindowInSeconds)
                    }));
                }
                else
                {
                    options.AddPolicy(rlf.PolicyName, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = rlf.PermitLimit,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(rlf.WindowInSeconds)
                    }));
                }
            }
            if (allSettings.Count > 0)
            {
                options.OnRejected = async (context, cancellationToken) =>
                {
                    //var pp = context.HttpContext.Features.Get<Microsoft.AspNetCore.Authentication.IAuthenticateResultFeature>()?
                    //          .AuthenticateResult?.Properties?.GetTokenValue("access_token")?.ToString();
                    //var p1 = context.HttpContext.Features.Get<Microsoft.AspNetCore.Authentication.IAuthenticateResultFeature>();
                    //var p2 = p1?.AuthenticateResult;
                    //var p3 = p2?.Properties;
                    //var p4 = p3?.GetTokenValue("access_token");
                    string policyName = "Default policy";
                    string path = context.HttpContext.Request.Path;
                    EnableRateLimitingAttribute? pol = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>();
                    if (pol != null) {
                        policyName = pol.PolicyName;
                    }
                    string IpAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();
                    TimeSpan retryAfter = TimeSpan.FromSeconds(-1);
                    object whatshere;
                    bool gotit = context.Lease.TryGetMetadata("RETRY_AFTER", out whatshere);
                    if (gotit)
                    {
                        retryAfter = (TimeSpan)whatshere;
                    }
                    if (retryAfter == TimeSpan.FromSeconds(-1))
                    {//got to give a generic response
                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please wait a few seconds and try again.", cancellationToken);
                        Logger.LogError($"Rate limit exceeded for IP: {IpAddress}"
                            + Environment.NewLine + $"On path: {path}");
                    }
                    else
                    {//we know all we'd like to know!
                        string secondsStr = retryAfter.TotalSeconds.ToString();
                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        context.HttpContext.Response.Headers.RetryAfter = secondsStr;

                        await context.HttpContext.Response.WriteAsync($"Rate limit exceeded. Please try again after {secondsStr} seconds.", cancellationToken);

                        Logger.LogError($"Rate limit exceeded for IP: {IpAddress}"
                            + Environment.NewLine + $"On Policy: {policyName} and path: {path}"
                            + Environment.NewLine + $"Retry after: {secondsStr} seconds." + Environment.NewLine);
                    }
                };
            }
        }
        private class RateLimiterFixedWindowPolicySetting
        {
            public string PolicyName { get; set; }
            public int WindowInSeconds { get; set; }
            public int PermitLimit { get; set; }
        }
    }
}
