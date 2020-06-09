using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EPPIDataServices.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace ERxWebClient2
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Program> logger)
        {
            Configuration = configuration;
            Logger = logger;
            Program.SqlHelper = new SQLHelper((IConfigurationRoot)configuration, logger);
        }

        //makes the logger available within CSLA objects, used in TrainingRunCommand, list may grow...
        public static ILogger<Program> Logger { get; private set; }
        public static IConfiguration Configuration { get; private set; }
        public static IncomingEthTxService Signaller { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                //.AddAuthorization()
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(3),
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["AppSettings:EPPIApiUrl"],
                        ValidAudience = Configuration["AppSettings:EPPIApiClientName"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AppSettings:EPPIApiClientSecret"]))
                    };
                });

            services.AddSingleton(Configuration);
            services.AddSingleton<IHostedService, IncomingEthTxService>();
            //services.AddHostedService<IncomingEthTxService>();
            services.AddMvc().AddJsonOptions(options =>
            {//this is needed to allow serialising CSLA child objects:
                //they all have a "Parent" field which creates a reference loop.
                options.SerializerSettings.CheckAdditionalContent = true;

                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {

                    HotModuleReplacement = true,
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseAuthentication();
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings required for the PDF viewer.
            provider.Mappings[".res"] = "application/octet-stream";
            provider.Mappings[".pexe"] = "application/x-pnacl";
            provider.Mappings[".nmf"] = "application/octet-stream";
            provider.Mappings[".mem"] = "application/octet-stream";
            provider.Mappings[".wasm"] = "application/wasm";
            app.UseStaticFiles(new StaticFileOptions {
                ContentTypeProvider = provider
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
            var serviceProvider = app.ApplicationServices;
            var hostingEnv = serviceProvider.GetService<IHostedService>();
            Signaller = (IncomingEthTxService)hostingEnv;
        }
    }
    //public abstract class BackgroundService : IHostedService, IDisposable
    //{
    //    protected readonly IServiceScopeFactory _scopeFactory;
    //    private Task _executingTask;
    //    public readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
    //    public  ILogger<IHostedService> Logger { get; private set; }
    //    public CancellationToken GlobalCancellationToken;

    //    public BackgroundService(ILogger<IHostedService> logger)
    //    {
    //        Logger = logger;
    //    }

    //    protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

    //    public virtual Task StartAsync(CancellationToken cancellationToken)
    //    {
    //        // Store the task we're executing
    //        _executingTask = ExecuteAsync(_stoppingCts.Token);
    //        Console.WriteLine("[BackgroundService] Service is Running");
    //        this.Logger.LogWarning("starting...");
    //        this.Logger.LogWarning("starting...");
    //        // If the task is completed then return it,
    //        // this will bubble cancellation and failure to the caller
    //        if (_executingTask.IsCompleted)
    //        {
    //            return _executingTask;
    //        }

    //        // Otherwise it's running
    //        return Task.CompletedTask;
    //    }

    //    public virtual async Task StopAsync(CancellationToken cancellationToken)
    //    {
    //        this.Logger.LogWarning("Trying to stop...");
    //        Console.WriteLine("[BackgroundService] Service is Stopping: " + cancellationToken.IsCancellationRequested.ToString());
    //        //CancellationToken newC = new CancellationToken(true);
    //        //Console.WriteLine("[BackgroundService] Service is Stopping: " + newC.IsCancellationRequested.ToString());
    //        //this.GlobalCancellationToken = newC;
    //        // Stop called without start
    //        if (_executingTask == null)
    //        {
    //            return;
    //        }

    //        try
    //        {
    //            // Signal cancellation to the executing method
    //            _stoppingCts.Cancel();
    //            this.Logger.LogWarning("Trying to stop _stoppingCts...");
    //            Console.WriteLine("[BackgroundService] Service is Stopping: _stoppingCts " + cancellationToken.IsCancellationRequested.ToString());
    //            //Thread.Sleep(50 * 1000);//50s
    //        }
    //        finally
    //        {
    //            // Wait until the task completes or the stop token triggers
    //            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite,
    //                                                          cancellationToken));
    //        }
    //    }

    //    public virtual void Dispose()
    //    {
    //        _stoppingCts.Cancel();
    //    }
    //}

    public abstract class HostedService : IHostedService
    {
        // Example untested base class code kindly provided by David Fowler: https://gist.github.com/davidfowl/a7dd5064d9dcf35b6eae1a7953d615e3

        private Task _executingTask;
        private CancellationTokenSource _cts;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Store the task we're executing
            _executingTask = ExecuteAsync(_cts.Token);

            // If the task is completed then return it, otherwise it's running
            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            // Signal cancellation to the executing method
            _cts.Cancel();

            // Wait until the task completes or the stop token triggers
            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

            // Throw if cancellation triggered
            cancellationToken.ThrowIfCancellationRequested();
        }

        // Derived classes should override this and execute a long running method until 
        // cancellation is requested
        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }


    public class IncomingEthTxService : HostedService
    {
        public ILogger<IHostedService> Logger { get; private set; }
        public IncomingEthTxService(ILogger<IHostedService> logger): base()// : base(logger)
        {
            Logger = logger;
        }
        public List<Task> queued = new List<Task>();
        public List<Task> running = new List<Task>();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //GlobalCancellationToken = stoppingToken;
            while (!stoppingToken.IsCancellationRequested)
            {
                
                Console.WriteLine("[IncomingEthTxService] Service is Running");

                // Run something
                //while (queued.Count > 0)
                //{
                //    Task q = queued[0];
                //    queued.RemoveRange(0, 1);
                //    q.Start();
                //    running.Add(q);
                //}    
                
                await Task.Delay(5*1000, stoppingToken);
                //for (int i = 0; i < running.Count; i++)
                //{
                //    if (running[i].Status != TaskStatus.Running)
                //    {
                //        running.RemoveAt(i);
                //    }
                //}
            }
            if (stoppingToken.IsCancellationRequested)
            {
                
                Console.WriteLine("[IncomingEthTxService] received stop signal");
                Logger.LogCritical("[IncomingEthTxService] received stop signal");
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[IncomingEthTxService] in StopAsync1, " + stoppingToken.IsCancellationRequested);
            await base.StopAsync(stoppingToken);
            // Run your graceful clean-up actions
            Console.WriteLine("[IncomingEthTxService] in StopAsync2, " + stoppingToken.IsCancellationRequested);// + " " + _stoppingCts.Token.IsCancellationRequested);
            Thread.Sleep(50 * 1000);//50s
            Logger.LogCritical("[IncomingEthTxService] in StopAsync, " + stoppingToken.IsCancellationRequested);
        }
    }
}
