using BusinessLibrary.Data;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Builder;

try
{
    //Apparently this gets automatically "swapped out" when we call UseSerilog(...)
    Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();
    var builder = WebApplication.CreateBuilder(args);
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
    SqlHelper = new SQLHelper(builder.Configuration, MSlogger);

    // Add services to the container.

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
    //builder.Services.AddAuthorization(opt =>
    //{
    //    opt.
    //});
    var app = builder.Build();

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
partial class Program
{
    public static SQLHelper? SqlHelper {  get; private set; }
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

}

//namespace BusinessLibrary.Data
//{
//    public static class DataConnection
//    {
//        private static string _ConnectionString = "";
//        private static string _AdmConnectionString = "";
//        public static void SetConnectionString(string val)
//        {
//            _ConnectionString = val;
//        }
//        public static void SetAdmConnectionString(string val)
//        {
//            _AdmConnectionString = val;
//        }
//        public static string ConnectionString
//        {
//            get
//            {
//                return _ConnectionString;
//            }
//        }

//        public static string AdmConnectionString
//        {
//            get
//            {
//                return _AdmConnectionString;
//            }
//        }


//    }
//}
