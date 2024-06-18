using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WebDatabasesMVC
{
    public class Program
    {
        public static SQLHelper SqlHelper;
        public static void Main(string[] args)
        {
            //Log.Logger = new LoggerConfiguration()
            //    .WriteTo.File(CreateLogFileName())
            //    .MinimumLevel.Error()
            //    .CreateLogger();
            Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string loggerfilename = CreateLogFileName();
            var ihb = Host.CreateDefaultBuilder(args);
            ihb.UseSerilog((ctx, lc) => lc
            .WriteTo.File(loggerfilename).ReadFrom.Configuration(ctx.Configuration)
            );
            return ihb.ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
        private static string CreateLogFileName()
        {
            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
            string LogFilename = logDir.FullName + @"\" + "WebDatabasesMVC-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            //if (!System.IO.File.Exists(LogFilename)) System.IO.File.Create(LogFilename);
            return LogFilename;
        }
    }
}
