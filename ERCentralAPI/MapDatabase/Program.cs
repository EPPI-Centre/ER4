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

namespace MapDatabase
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
            string LogFilename = logDir.FullName + @"\" + "MapDatabase-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            return LogFilename;
        }


        public static void Main(string[] args)
        {
            //Without logging to the Datbase
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(CreateLogFileName())
                .MinimumLevel.Error()
                .CreateLogger();


            BuildWebHost(args).Run();

        }

        public static IWebHost BuildWebHost(string[] args) =>
           WebHost.CreateDefaultBuilder(args)
                  .UseStartup<Startup>()
                    .UseSerilog()//!!!!!!!!!!!!!!!!!!!
                  .UseConfiguration(Configuration)
                    .Build();



        //internal static IdentityServer4Client IdentityServerClient;
        public static SQLHelper SqlHelper;




        /*public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();*/
    }
}
