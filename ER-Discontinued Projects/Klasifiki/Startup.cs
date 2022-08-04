using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Serilog.Sinks.File;
using System.Collections.Concurrent;

namespace Klasifiki
{
    public class Startup
    {

        public Startup(IConfiguration configuration, ILogger<Program> logger)
        {
            Configuration = configuration;
            Program.SqlHelper = new SQLHelper((IConfigurationRoot)configuration, logger);
            Program.IdentityServerClient = new IdentityServer4Client(configuration);
        }
        
        
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {   
            services.AddMvc();
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //.AddJwtBearer(options =>
            //{
            //    // base-address of your identityserver
            //    options.Authority = "https://demo.identityserver.io";

            //    // name of the API resource
            //    options.Audience = "api1";
            //});
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            string authority = Configuration["AppSettings:EPPIApiUrl"];
            string clientID = Configuration["AppSettings:EPPIApiClientName"];
            string clientSecret = Configuration["AppSettings:EPPIApiClientSecret"];

            
            services
                .AddAuthorization(options =>
                {
                    options.AddPolicy("Authenticated", new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        //.RequireClaim("role", "Administrators")
                        .Build());
                })
                .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;// "Cookies";
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                    options.Authority = authority;
                    options.RequireHttpsMetadata = false;

                    options.ClientId = clientID;
                    options.ClientSecret = clientSecret;
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Add("KlasifikiAPI");
                    options.Scope.Add("offline_access");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            app.UseAuthentication();
            
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseResponseCompression();
            //loggerFactory.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfigurationPubMed
            //{
            //    LogLevel = LogLevel.Error
            //}));
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "Default",                                          // Route name
                "{controller}/{action}/{id?}",                           // URL with parameters
                new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
                );
            });
        }

        
    }
}
