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

namespace Klasifiki
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();

        }
        internal IConfigurationRoot configuration;
        internal string EPPIApiUrl;
        internal string EPPIApiClientSecret;
        internal string EPPIApiClientName;
        public SQLHelper SqlHelper;
        internal EPPILogger Logger;
        void GetAppSettings()
        {
            System.IO.Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\Tmpfiles");
            Logger = new EPPILogger(true);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
            try
            {
                //            EPPIApiUrl": "http://localhost/eridentityprovider",
                //            "EPPIApiClientSecret": "ClientSecret4Devs",
                //"EPPIApiClientName
                EPPIApiUrl = configuration["AppSettings:EPPIApiUrl"];
                if (EPPIApiUrl == null || EPPIApiUrl == "")
                    throw new Exception("ERROR: could not get value for FTPBaselineFolder, please check appsettings.json file.");
                EPPIApiClientSecret = configuration["AppSettings:EPPIApiClientSecret"];
                if (EPPIApiClientSecret == null || EPPIApiClientSecret == "")
                    throw new Exception("ERROR: could not get value for FTPUpdatesFolder, please check appsettings.json file.");
                EPPIApiClientName = configuration["AppSettings:EPPIApiClientName"];
                if (EPPIApiClientName == null || EPPIApiClientName == "")
                    throw new Exception("ERROR: could not get value for FTPUpdatesFolder, please check appsettings.json file.");
                SqlHelper = new SQLHelper(configuration, Logger);
                if (SqlHelper == null || SqlHelper.DataServiceDB == "")
                    throw new Exception("ERROR: could not get value for DatabaseName, please check appsettings.json file.");
            }
            catch (Exception e)
            {
                Logger.LogMessageLine("");
                Logger.LogMessageLine("Error reading config file, details are:");
                Logger.LogMessageLine(e.Message);
                Logger.LogMessageLine("Aborting...");
                Logger.LogMessageLine("");
                System.Environment.Exit(0);
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
