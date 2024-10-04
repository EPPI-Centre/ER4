using EPPIDataServices.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace CopyReviewData
{
    internal partial class Program
    {
        static Serilog.ILogger? _logger;
        private static SQLHelper? SqlHelper = null;
        private static int SourceRevId;
        private static int DestinationRevId;
        static void Main(string[] args)
        {
            string loggerfilename = CreateLogFileName();
            Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices(services =>
                {
                }).UseSerilog((ctx, lc) => lc.WriteTo.File(loggerfilename).ReadFrom.Configuration(ctx.Configuration))
                .Build();
            _logger = Log.Logger;
            var MSlogger = new Serilog.Extensions.Logging.SerilogLoggerFactory(_logger).CreateLogger<Program>();
            SqlHelper = new SQLHelper(configuration, MSlogger);
            LogLine("");
            LogLine("");
            LogLine("CopyReviewData Is Starting!");
            MainDecision();
            LogLine("END.");
        }
        private static void MainDecision()
        {
            Console.WriteLine("What to do? Options are: \"MapCodes(1)\" or \"End(2)\".");
            Console.WriteLine("[You can type the text OR the number to pick one]");
            Console.WriteLine("[Answering \"Q\" (not case sensitive) to Any prompt will immediately Quit]");
            string? answer = GetAnswer();
            if (answer == "MapCodes" || answer == "1" )
            {
                DoMapCodes();
            }
            else if (answer == null || answer == "")
            {
                MainDecision();
            }
            else if (answer == "End" || answer == "2")
            {
                LogLine("User asked to End.");
            }
        }
        private static void DoMapCodes() 
        {
            LogLine("Doing MapCodes...");
            Console.WriteLine("Do You Want to Map all codes between two reviews? [Y/N]");
            Console.WriteLine("Alternative is CustomMapping, where you specify what Coding tools to look at.");
            string? answer = GetAnswer();
            if (answer != null && answer.ToLower() == "y") { MapCodesBetween2ReviewsGetInput(); }
            else if (answer != null && answer.ToLower() == "n") { DoCustomMapping(); }
            else DoMapCodes();
        }
        private static void MapCodesBetween2ReviewsGetInput()
        {
            LogLine("Doing MapCodesBetween2Reviews...");
            Console.WriteLine("Please enter the ReviewId for the SOURCE (contains ancestor tools) review");
            string? answer = GetAnswer();
            int srcId;
            if (!int.TryParse(answer, out srcId))
            {
                MapCodesBetween2ReviewsGetInput();
                return;
            }
            SourceRevId = srcId; 
            Console.WriteLine("Please enter the ReviewId for the DESTINATION (contains offspring tools) review");
            answer = GetAnswer();
            int dstId;
            if (!int.TryParse(answer, out dstId))
            {
                MapCodesBetween2ReviewsGetInput();
                return;
            }
            DestinationRevId = dstId;
            LogLine("Will map all codes from review: " + SourceRevId + " to review: " + DestinationRevId);
            try
            {
                DoMapCodesBetween2Reviews();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (_logger != null) _logger.LogException(ex);
            }
        }
        
        private static void DoCustomMapping()
        {
            LogLine("Doing MapCodesBetween2Reviews...[not implemented!]");
        }
        private static string? GetAnswer()
        {
            string? answer = Console.ReadLine();
            if (answer == "Q" || answer == "q")
            {
                LogLine("User Interrupted with 'Q'");
                LogLine("END.");
                Environment.Exit(0);
                return null;
            }
            else return answer;
        }
        private static string CreateLogFileName()
        {
            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
            string LogFilename = logDir.FullName + @"\" + "CopyReviewData-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            if (!System.IO.File.Exists(LogFilename)) {
                using (FileStream fs = System.IO.File.Create(LogFilename))
                {
                    fs.Close();
                }
            }
            return LogFilename;
        }

        private static void LogLine(string line)
        {
            Console.WriteLine(line);
            if (_logger != null) _logger.Information(line);
        }
    }
}
