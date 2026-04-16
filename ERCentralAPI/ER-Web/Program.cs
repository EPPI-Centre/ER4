using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Data;
using EPPIDataServices.Helpers;
using ER_Web.Services;
using ERxWebClient2.Zotero;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Data.SqlClient;
using System.Text;

try
{
    //Apparently this gets automatically "swapped out" when we call UseSerilog(...)
    Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();
    var builder = WebApplication.CreateBuilder(args);

    //ensure our long-running IHostedServices have enough time to shutdown - most BOs will have a 30s timeout for SQL calls, so we set it to more than that
    //it's unclear whether by default the ShutdownTimeout is 5 seconds or 90s!!
    builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(45));
#if DEBUG
    //we allow CORS from localhost *only* when debugging
    var clientURL = builder.Configuration["AppSettings:clientURL"];
    if (clientURL != null)
    {
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins(clientURL)
                    .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
        });
    }
#endif
    //add the file logger
    string loggerfilename = CreateLogFileName();
    //Silly Microsoft does not provide a log-to-file facility, so have to go for Serilog...
    //requires Serilog.AspNetCore package.

    builder.Host.UseSerilog((ctx, lc) => lc
            .WriteTo.File(loggerfilename).ReadFrom.Configuration(ctx.Configuration)
            );
    _Logger = Log.Logger;


    //set values in AzureSettings, which is available to CSLA business objects (version we currently use doesn't understand dependency injection)
    BusinessLibrary.BusinessClasses.AzureSettings.SetValues(builder.Configuration);

    //do the connection strings trick (as above, more or less...)
    //see: https://stackoverflow.com/questions/65443870/can-a-serilog-ilogger-be-converted-to-a-microsoft-extensions-logging-ilogger
    //we need a Microsoft.Extensions.Logger.Ilogger object for our SQLHelper, so we can then log SQL errors if/where we bypass BOs and talk to the DB directly.
    // the SqlHelper class will make sure our connection strings are available to BOs also.
    var MSlogger = new Serilog.Extensions.Logging.SerilogLoggerFactory(_Logger).CreateLogger<Program>();

    builder.Services.AddHttpClient("zoteroApi");

    // Add services to the container.
    builder.Services.AddSingleton<ZoteroConcurrentDictionary>();
    //builder.Services.AddSingleton(SqlHelper);
    builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
    {//this is needed to allow serialising CSLA child objects:
     //they all have a "Parent" field which creates a reference loop.
        options.SerializerSettings.CheckAdditionalContent = true;
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(3),
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AppSettings:EPPIApiUrl"],
            ValidAudience = builder.Configuration["AppSettings:EPPIApiClientName"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:EPPIApiClientSecret"]))
        };
        options.SaveToken = true;
    });

    builder.Services
        .Configure<HostOptions>(
            (options) =>
            {
                //Service Behavior in case of exceptions - defautls to StopHost
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                //Host will try to wait 30 seconds before stopping the service. 
                options.ShutdownTimeout = TimeSpan.FromSeconds(30);
            })
        .AddHostedService<ER_Web.Services.GracefulShutdownGuardianService>().AddHostedService<RobotOpenAiHostedService>();
    //line above: order is important. Having GracefulShutdownGuardianService first, means it's the last to stop, which is ideal:
    //This service has one role: to keep ER alive while long-lasting-tasks are shutting down. This is done via two mechanisms:
    //1. Makes Program.CancelToken.IsCancellationRequested flip to true, so that fire-and-forget tasks in LongLastingFireAndForgetCommands can notice and stop gracefully
    //2. Monitor TB_REVIEW_JOB for fired and forgotten tasks - GracefulShutdownGuardianService will only finish "stopping" when either:
    //2.1 No review_jobs report themselves as "running" in TB_REVIEW_JOB
    //2.2 28 seconds passed since we first checked for running jobs.
    //IOW, GracefulShutdownGuardianService only stops when everything else has stopped gracefully, or when it's too late and we're giving up.
    //thus, since pt.1 happens immediately (inside OnStopping()), we then ALSO wait for RobotOpenAiHostedService to stop, and only then check for fired and forgotten long lasting tasks
    //however Program.CancelToken.IsCancellationRequested flipped to true immediately, so all fired and forgotten tasks had the best chance to stop gracefully
    //this matters, because RobotOpenAiHostedService executes RobotOpenAICommand which has a long-ish lasting task (calling GPT API) that monitors Program.CancelToken, but doesn't log to TB_REVIEW_JOB
    //so, in this way any RobotOpenAICommand will be cancelled as soon as possible, allowing RobotOpenAiHostedService to stop gracefully, and also giving any other long lasting task
    //their best chance to stop concurrently. GracefulShutdownGuardianService will stop last, fulfilling it's general role, which is .

    var app = builder.Build();
    var SqlHelper = new SQLHelper(builder.Configuration, MSlogger);
    DataConnection.DataConnectionConfigure(SqlHelper);

    //the following command could be used to log "streamlined request data", whatever that means... Disabled for now, could be useful if we could use it to log request data along with exceptions.
    //app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    var provider = new FileExtensionContentTypeProvider();
    // Add new mappings required for the PDF viewer.
    provider.Mappings[".res"] = "application/octet-stream";
    provider.Mappings[".pexe"] = "application/x-pnacl";
    provider.Mappings[".nmf"] = "application/octet-stream";
    provider.Mappings[".mem"] = "application/octet-stream";
    provider.Mappings[".wasm"] = "application/wasm";
    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = provider
    });
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");

    app.MapFallbackToFile("index.html");
    app.UseRouting();
#if DEBUG
    app.UseCors();
#endif

    app.UseAuthentication();
    app.UseAuthorization();

    Csla.SmartDate.SetDefaultFormatString("dd/MM/yyyy");

    LongLastingTaskResumer resumer = new LongLastingTaskResumer(_Logger);
    resumer.ResumePausedJobs();
    //StartRobots();
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Exception in App root");
}
finally
{
    //probably not needed at all, but just in case...
    if (_Logger != null)
    {
        Log.Information("Shutting down!");
        var Idisp = _Logger as IDisposable;
        if (Idisp != null) Idisp.Dispose();
    }
}
public partial class Program
{
    //public static SQLHelper? SqlHelper {  get; private set; }
    private static string CreateLogFileName()
    {
        DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
        string LogFilename = logDir.FullName + @"\" + "ERxWebClient2-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
        if (!System.IO.File.Exists(LogFilename))
        {
            using (FileStream fs = System.IO.File.Create(LogFilename))
            {
                fs.Close();
            }
        }
        //System.IO.File.Create(LogFilename);
        return LogFilename;
    }
    private  static  Serilog.ILogger? _Logger;

    //for CSLA objects that don't understand dependency injection, we keep an instance of the logger here, used when we spin a long-lasting thread
    //and we opt for file-system logging of errors, instead of writing to the DB (which we do, on occasion)
    //this is naughty, but it's the best I could think of, given the DI absence in old versions of CSLA
    public static Serilog.ILogger? Logger { get { return _Logger; } }

    public static bool AppIsShuttingDown { 
        get { return Program.TokenSource.Token.IsCancellationRequested; }
    }

    public static CancellationTokenSource TokenSource { 
        get;
        private set; 
    } = new CancellationTokenSource();


}

