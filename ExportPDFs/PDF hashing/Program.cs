using EPPIDataServices.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PDF_hashing
{
    internal class Program
    {
        private static ILogger<Program>? _logger = null;
        private static SQLHelper SqlHelper = null;
        private static int MillisecondsToSleep = 1; //100ms appears to be good - means app sleeps 800ms per second in dev
        private static int MaxDocsToProcess = 0;
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(CreateLogFileName())
                .CreateLogger();
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<ILogger<Program>>();
            _logger = serviceProvider.GetService<ILogger<Program>>();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            SetConfigurableValues(configuration);
            SqlHelper = new SQLHelper(configuration, _logger);
            int counter = 0; long currentid = 0;
            while ((counter < MaxDocsToProcess || MaxDocsToProcess == 0)
                && currentid != -1)
            {
                currentid = HashNextDoc(currentid);
                counter++;
                Thread.Sleep(MillisecondsToSleep);
            }
        }
        private static long HashNextDoc(long lastDocid = 0)
        {
            string que = @"SELECT TOP(1) ITEM_DOCUMENT_ID, DOCUMENT_TEXT from TB_ITEM_DOCUMENT where ITEM_DOCUMENT_ID > "+ lastDocid.ToString() 
                + " AND TXT_HASH is null order by ITEM_DOCUMENT_ID";
            long id = -1;
            //"Select distinct(r.ITEM_ID) from TB_ITEM_DOCUMENT d
            //            inner join TB_ITEM_REVIEW r on d.ITEM_ID = r.ITEM_ID and d.DOCUMENT_TEXT = 'Error: could not find/load an appropriate filter!'
            //             AND DOCUMENT_EXTENSION = '.pdf' AND REVIEW_ID = " + RevId;// + " AND IS_INCLUDED = 1 and IS_DELETED = 0 and MASTER_ITEM_ID is null";
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                string text = "";
                string CMD = "";
                string hashed = "";
                using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, que))
                {
                    if (reader.Read())// && ItemIDs.Count < 5000)
                    {
                        id = (Int64)reader["ITEM_DOCUMENT_ID"];
                        text = (string)reader["DOCUMENT_TEXT"];
                    }
                }
                if (text != null && id > -1)
                {
                    if (text.Length > 4000) text = text.Substring(0, 4000);
                    //has to be, because in SQL HASHBYTES('SHA1', @txt) needs @txt to be 8000 bytes max
                    //IN SQL this will be something like:
                    //if DATALENGTH(@txt) > 8000 -- 2 bytes per nvarchar character
                    //BEGIN
                    //  Set @toHash = SUBSTRING(@txt, 0, 4000)
                    //END
                    //ELSE
                    //BEGIN
                    //    Set @toHash = @txt
                    //END
                    if (text.Length > 200) hashed = HashString(text);
                    else
                    {
                        //0x1C209ADD594DF6B37167F1F668D582D1F37658F7
                        hashed = "0x0000000000000000000000000000000000000000";
                    }
                    CMD = "UPDATE TB_ITEM_DOCUMENT set TXT_HASH = CONVERT(varbinary(20), '"
                                    + hashed
                                    + "', 1) WHERE ITEM_DOCUMENT_ID =" + id.ToString();
                    SqlHelper.ExecuteNonQueryNonSP(conn, CMD);
                    Log.Information("hashed " + id.ToString() + ". Hash is: " + hashed + " Text starts with: " + (text.Length > 20 ? text.Substring(0,20) : text));
                }
                
            }
            return id;
        }
        private static string HashString(string input)
        {
            string res = "";
            //var sha1 = new System.Security.Cryptography.SHA1.;
            byte[] plaintextBytes = Encoding.Unicode.GetBytes(input);
            byte[]? hashBytes = System.Security.Cryptography.SHA1.HashData(plaintextBytes);
                
            if (hashBytes != null)
            {
                System.Text.StringBuilder s = new System.Text.StringBuilder();
                s.Append("0x");
                foreach (byte b in hashBytes)
                {
                    s.Append(b.ToString("x2").ToUpper());
                }
                //res = System.Convert.ToBase64String(hashBytes);
                res = s.ToString();
            }
            return res;
        }
        private static string CreateLogFileName()
        {
            DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
            string LogFilename = logDir.FullName + @"\" + "PDFhashing-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            if (!System.IO.File.Exists(LogFilename))
            {
                using (FileStream fs = System.IO.File.Create(LogFilename))
                {
                    fs.Close();
                }
            }
            return LogFilename;
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            //Action<ILoggingBuilder> tester = new Action<ILoggingBuilder>(configure => configure.AddConsole());
            //Action<ILoggingBuilder> tester2 = new Action<ILoggingBuilder>(configure => configure.AddSerilog());

            services.AddLogging(configure => configure.AddConsole()
                    ).AddLogging(configure => configure.AddSerilog());

        }
        private static void SetConfigurableValues(IConfigurationRoot configuration)
        {
            var MS = configuration["AppSettings:MillisecondsToSleep"];
            if (MS != null)
            {
                int Msint;
                if (int.TryParse(MS, out Msint))
                {
                    MillisecondsToSleep = Msint;
                }
            }
            var Md = configuration["AppSettings:MaxDocsToProcess"];
            if (Md != null)
            {
                int Msint;
                if (int.TryParse(Md, out Msint))
                {
                    MaxDocsToProcess = Msint;
                }
            }
            
        }
    }
}
