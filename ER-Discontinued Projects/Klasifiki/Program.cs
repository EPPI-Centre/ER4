using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;
using Serilog;
using Microsoft.AspNetCore.Builder;

namespace Klasifiki
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
            string LogFilename = logDir.FullName + @"\" + "Klasifiki-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            //if (!System.IO.File.Exists(LogFilename)) System.IO.File.Create(LogFilename);
            return LogFilename;
        }
        public static void Main(string[] args)
        {
            // Check the setting to setup for SQL logging entries or not
            if (Convert.ToBoolean(Configuration["AppSettings:UseDatabaseLogging"]))
            {
                // This is a serilog configuration
                Log.Logger = new LoggerConfiguration().WriteTo.File(CreateLogFileName())
               .ReadFrom.Configuration(Configuration)
               .CreateLogger();

                WebHost.CreateDefaultBuilder(args)
                  .UseStartup<Startup>()
                  .UseConfiguration(Configuration)
                  .UseSerilog()
                  .Build().Run();
            }
            else
            {
                //Without logging to the Datbase
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File(CreateLogFileName())
                    .MinimumLevel.Error()
                    .CreateLogger();
               

                BuildWebHost(args).Run();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
           WebHost.CreateDefaultBuilder(args)
                  .UseStartup<Startup>()
                    .UseSerilog()//!!!!!!!!!!!!!!!!!!!
                  .UseConfiguration(Configuration)
                    .Build();


        //internal IConfigurationRoot configuration;

        internal static IdentityServer4Client IdentityServerClient;
        public static SQLHelper SqlHelper;

        //internal static EPPILogger Logger;

        //void GetAppSettings()
        //{
        //    System.IO.Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Tmpfiles");
        //    Logger = new EPPILogger(true);
        //    var builder = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        //    configuration = builder.Build();
        //    try
        //    {
        //        EPPIApiUrl = configuration["AppSettings:EPPIApiUrl"];
        //        if (EPPIApiUrl == null || EPPIApiUrl == "")
        //            throw new Exception("ERROR: could not get value for FTPBaselineFolder, please check appsettings.json file.");
        //        EPPIApiClientSecret = configuration["AppSettings:EPPIApiClientSecret"];
        //        if (EPPIApiClientSecret == null || EPPIApiClientSecret == "")
        //            throw new Exception("ERROR: could not get value for FTPUpdatesFolder, please check appsettings.json file.");
        //        EPPIApiClientName = configuration["AppSettings:EPPIApiClientName"];
        //        if (EPPIApiClientName == null || EPPIApiClientName == "")
        //            throw new Exception("ERROR: could not get value for FTPUpdatesFolder, please check appsettings.json file.");
        //        SqlHelper = new SQLHelper(configuration, Logger);
        //        if (SqlHelper == null || SqlHelper.DataServiceDB == "")
        //            throw new Exception("ERROR: could not get value for DatabaseName, please check appsettings.json file.");
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.LogMessageLine("");
        //        Logger.LogMessageLine("Error reading config file, details are:");
        //        Logger.LogMessageLine(e.Message);
        //        Logger.LogMessageLine("Aborting...");
        //        Logger.LogMessageLine("");
        //        System.Environment.Exit(0);
        //    }
        //}


    }
}
