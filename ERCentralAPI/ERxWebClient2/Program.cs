using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ERxWebClient2
{

    public class Program
    {

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
             .Build();

        private static string CreateLogFileName()
        {
            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
            string LogFilename = logDir.FullName + @"\" + "ERxWebClient2-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            //if (!System.IO.File.Exists(LogFilename)) System.IO.File.Create(LogFilename);
            return LogFilename;
        }

        public static void Main(string[] args)
        {

            //Without logging to the Datbase
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(CreateLogFileName())
                //.MinimumLevel.Error()
                .CreateLogger();

            BuildWebHost(args).Run();
        }

        public static SQLHelper SqlHelper;
        //internal static EPPILogger Logger;

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(Configuration)
                //.ConfigureLogging((hostingContext, logging) =>
                //{
                //    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                //    logging.AddConsole();
                //    logging.AddDebug();
                //})
                .UseShutdownTimeout(TimeSpan.FromSeconds(2))
                .UseStartup<Startup>()
                .UseSerilog()//!!!!!!!!!!!!!!!!!!!
                .Build();

       
    }

}
