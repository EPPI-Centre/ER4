using EPPIDataServices.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text;

namespace PDF_hashing
{
    internal class Program
    {
        private static ILogger<Program>? _logger = null;
        private static SQLHelper SqlHelper = null;
        private static int MillisecondsToSleep = 1; //100ms appears to be good - means app sleeps 800ms per second in dev
        private static int MaxDocsToProcess = 0; //set in config. If left out or set to zero, then we'll process all docs
        private static string blobConnection = "";
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            SetConfigurableValues(configuration);
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            //Silly Microsoft does not provide a log-to-file facility, so have to go for Serilog...
            //requires Serilog.AspNetCore package.

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(CreateLogFileName()).ReadFrom.Configuration(configuration)
                .CreateLogger();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<ILogger<Program>>();
            _logger = serviceProvider.GetService<ILogger<Program>>();
            SqlHelper = new SQLHelper(configuration, _logger);

            Console.WriteLine("App starting.");
            ShowSettings();
            StartupChoice();
        }
        private static void StartupChoice()
        {
            ConsoleKey[] choices = { ConsoleKey.D1, ConsoleKey.NumPad1, //1: indexes 0,1
                ConsoleKey.D2, ConsoleKey.NumPad2, //2: indexes 2,3
                ConsoleKey.S, //S for settings: index 4
                ConsoleKey.Q //the letter Q - make sure this is always the last option
            };
            ConsoleKey answer = ConsoleKey.Spacebar;//just to give it a value!
            ShowStartupChoices();
            int counter = 0;
            while (!choices.Contains(answer))
            {
                counter++;
                ConsoleKeyInfo r = Console.ReadKey(true);
                answer = r.Key;
                Console.WriteLine("You pressed: " + ((char)answer) + " (answer N." + counter.ToString() + ")");
                int indexOf = Array.IndexOf(choices, answer);
                if (indexOf == 0 || indexOf == 1) {
                    //do hashing;
                    DoHashing();
                }
                else if (indexOf == 2 || indexOf == 3)
                {
                    //do migration;
                    DoDocsMigration();
                }
                else if (indexOf == 4)
                {
                    //change settings
                    answer = ConsoleKey.Spacebar;//prevent quitting!
                    DoChangeSettings();
                }
                else if (indexOf == choices.Length - 1)
                {
                    //quit
                    Log.Warning("Quitting.");
                    Console.WriteLine("Quitting.");
                }
                else if (counter >= 20)
                {
                    Log.Warning("Too many (invalid?) choices (" + counter.ToString() + "), quitting.");
                    Console.WriteLine("Too many invalid choices (" + counter.ToString() + "), quitting.");
                    break;
                }
            }

        }
        private static void ShowSettings()
        {
            Console.WriteLine("Current settings:");
            Console.WriteLine("- Process up to " + MaxDocsToProcess.ToString() + " docs");
            Console.WriteLine("- Pause for " + MillisecondsToSleep.ToString() + " milliseconds between docs.");
            Console.WriteLine("- Blob Connection string: ");
            Console.WriteLine("\t\"" + blobConnection + "\"");
            Console.WriteLine("");
            Log.Warning("Current settings:");
            Log.Warning("- Process up to " + MaxDocsToProcess.ToString() + " docs");
            Log.Warning("- Pause for " + MillisecondsToSleep.ToString() + " milliseconds between docs.");
            Log.Warning("- Blob Connection string: ");
            Log.Warning("\t\"[redacted]\"");
        }
        private static void ShowStartupChoices()
        {
            Console.WriteLine("What do you want to do?");
            Console.WriteLine("(1) Start hashing routine");
            Console.WriteLine("(2) Start migration of binaries to blob");
            Console.WriteLine("(S) Change Settings");
            Console.WriteLine("(Q) Quit without doing anything");
            Console.WriteLine("Press the corresponding keys (this is not case-sensitive)");
        }
        private static void DoHashing()
        {
            Log.Warning("DoHashing is starting.");
            Console.WriteLine("DoHashing is starting.");
            int counter = 0; long currentid = 0;
            while ((counter < MaxDocsToProcess || MaxDocsToProcess == 0)
                && currentid != -1)
            {
                currentid = HashNextDoc(currentid);
                counter++;
                Thread.Sleep(MillisecondsToSleep);
            }
            Console.WriteLine("");
            Log.Warning("Processed: " + (counter -1).ToString() + " docs.");
            Console.WriteLine("Processed: " + (counter - 1).ToString() + " docs.");
            if (counter >= MaxDocsToProcess && MaxDocsToProcess != 0)
            {
                Log.Warning("Processing ends: reached MaxDocsToProcess");
                Console.WriteLine("Processing ends: reached MaxDocsToProcess");
            }
            else if (currentid == -1)
            {
                Log.Warning("Processing ends: no more docs");
                Console.WriteLine("Processing ends: no more docs");
            }
            else
            {
                Log.Warning("Processing ends: unkown reason");
                Console.WriteLine("Processing ends: unkown reason");
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
                    if (reader == null)
                    {
                        Log.Error("FAIL: could not fetch next doc to hash. Aborting.");
                        return -1;
                    }
                    else if (reader.Read())// && ItemIDs.Count < 5000)
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

                    //SQL to get the same hash from truncated txt is:
                    //DATALENGTH(SUBSTRING(DOCUMENT_TEXT, 0, 4001)) dunno why, but that's how it looks


                    if (text.Length > 200) hashed = HashString(text);
                    else
                    {
                        //0x1C209ADD594DF6B37167F1F668D582D1F37658F7
                        hashed = "0x0000000000000000000000000000000000000000";
                    }
                    CMD = "UPDATE TB_ITEM_DOCUMENT set TXT_HASH = CONVERT(varbinary(20), '"
                                    + hashed
                                    + "', 1) WHERE ITEM_DOCUMENT_ID =" + id.ToString();
                    
                    //debug: adding the line below makes every call fail, used to check for error handling
                    //CMD += Environment.NewLine + "WAITFOR DELAY '00:00:31'";
                    int res = SqlHelper.ExecuteNonQueryNonSP(conn, CMD);
                    if (res == -2) //we didn't save this hash and error should be investigated
                    {//we will however continue processing
                        Log.Error("DID NOT SAVE HASH for: " + id.ToString() + ". Hash is: " + hashed + " Text starts with: " + (text.Length > 20 ? text.Substring(0, 20) : text));
                        Log.Error("SQL Command was: " + Environment.NewLine + CMD + "");
                        Console.WriteLine("DID NOT SAVE HASH for: " + id.ToString() + ".");
                    }
                    else
                    {
                        Log.Information("hashed " + id.ToString() + ". Hash is: " + hashed + " Text starts with: " + (text.Length > 20 ? text.Substring(0, 20) : text));
                        Console.Write(id.ToString() + ".");
                    }
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

        private static void DoDocsMigration()
        {
            Log.Warning("DoDocsMigration is starting.");
            Console.WriteLine("DoDocsMigration is starting");


            Log.Warning("DoDocsMigration has ended.");
            Console.WriteLine("DoDocsMigration has ended");
        }

        private static void DoChangeSettings()
        {
            bool SomeSettingsDidChange = false;
            Log.Warning("Changing settings...");
            Console.WriteLine("");
            Console.WriteLine("Changing settings...");
            Console.WriteLine("Setting: process up to " + MaxDocsToProcess.ToString() + " docs");
            Console.WriteLine("(Zero means process all docs) ");
            Console.WriteLine("Press \"C\" to change this, any other key to keep this value");
            ConsoleKeyInfo answer = Console.ReadKey(true);
            if (answer.Key == ConsoleKey.C)
            {
                Console.WriteLine("Setting: process up to docs, changing value");
                Console.WriteLine("New value must be a positive integer, please type the new value and press \"Enter\"");
                string? newSettingStr = Console.ReadLine();
                int newval = -1;
                if (!int.TryParse(newSettingStr, out newval) || newval < 0)
                {
                    Console.WriteLine("Invalid answer, returning to main menu.");
                    Console.WriteLine("");
                    Console.WriteLine("");
                    ShowStartupChoices();
                    return;
                }
                else
                {
                    Console.WriteLine("Valid answer, MaxDocsToProcess set to: " + newval.ToString() + ".");
                    Console.WriteLine("");
                    Console.WriteLine("");
                    MaxDocsToProcess = newval;
                    SomeSettingsDidChange = true;
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Setting: pause for " + MillisecondsToSleep.ToString() + " milliseconds between docs.");
            Console.WriteLine("(Zero means going at max speed) ");
            Console.WriteLine("Press \"C\" to change this, any other key to keep this value");
            answer = Console.ReadKey(true);
            if (answer.Key == ConsoleKey.C)
            {
                Console.WriteLine("Setting: milliseconds pause between docs, changing value");
                Console.WriteLine("New value must be a positive integer, please type the new value and press \"Enter\"");
                string? newSettingStr = Console.ReadLine();
                int newval = -1;
                if (!int.TryParse(newSettingStr, out newval) || newval < 0)
                {
                    Console.WriteLine("Invalid answer, returning to main menu.");
                    Console.WriteLine("");
                    Console.WriteLine("");
                    if (SomeSettingsDidChange)
                    {
                        Console.WriteLine("Changed settings!");
                        Log.Warning("Changed settings!");
                        ShowSettings();
                        Console.WriteLine("");
                        Console.WriteLine("");
                    }
                    ShowStartupChoices();
                    return;
                }
                else
                {
                    Console.WriteLine("Valid answer, MillisecondsToSleep set to: " + newval.ToString() + ".");
                    Console.WriteLine("");
                    Console.WriteLine("");
                    MillisecondsToSleep = newval;
                    SomeSettingsDidChange = true;
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Setting, blob connection string:");
            Console.WriteLine("\t\"" + blobConnection +"\"");
            Console.WriteLine("(Only used for migrating docs, but must be valid if used!!!) ");
            Console.WriteLine("Press \"C\" to change this, any other key to keep this value");
            answer = Console.ReadKey(true);
            if (answer.Key == ConsoleKey.C)
            {
                Console.WriteLine("Setting: blob connection string");
                Console.WriteLine("New value must be valid, please type the new value and press \"Enter\"");
                string? newSettingStr = Console.ReadLine();
                if(newSettingStr == null || newSettingStr.Length < 20) 
                {
                    Console.WriteLine("Invalid answer, returning to main menu.");
                    Console.WriteLine("");
                    Console.WriteLine(""); 
                    if (SomeSettingsDidChange)
                    {
                        Console.WriteLine("Changed settings!");
                        Log.Warning("Changed settings!");
                        ShowSettings();
                        Console.WriteLine("");
                        Console.WriteLine("");
                    }
                    ShowStartupChoices();
                    return;
                }
                else
                {
                    Console.WriteLine("Valid answer, blobConnection set to:");
                    Console.WriteLine("\t\"" + newSettingStr + "\"");
                    Console.WriteLine("");
                    Console.WriteLine("");
                    blobConnection = newSettingStr;
                    SomeSettingsDidChange = true;
                }
            }
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Extiting \"Change Settings\"");
            if (SomeSettingsDidChange)
            {
                Console.WriteLine("");
                Console.WriteLine("Changed settings!");
                Log.Warning("Changed settings!");
                ShowSettings();
            }
            Console.WriteLine("");
            Console.WriteLine("");
            ShowStartupChoices();
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

            var blobConn = configuration["AppSettings:blobConnection"];
            if (blobConn != null)
            {
                blobConnection = blobConn;
            }
        }
    }
}
