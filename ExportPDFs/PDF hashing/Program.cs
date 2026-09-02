using EPPIDataServices.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text;

namespace PDF_hashing
{
    internal partial class Program
    {
        private static ILogger<Program>? _logger = null;
        private static SQLHelper SqlHelper = null;
        private static int MillisecondsToSleep = 1; //100ms appears to be good - means app sleeps 800ms per second in dev
        private static int MaxDocsToProcess = 0; //set in config. If left out or set to zero, then we'll process all docs
        private static string blobConnection = "";
        private static bool AddHostNameToBlobFiles = true;
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
            Log.Warning("App starting.");
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
            Console.WriteLine("- Add hostname to Blob files: " + AddHostNameToBlobFiles.ToString());
            Console.WriteLine("");
            Log.Warning("Current settings:");
            Log.Warning("- Process up to " + MaxDocsToProcess.ToString() + " docs");
            Log.Warning("- Pause for " + MillisecondsToSleep.ToString() + " milliseconds between docs.");
            Log.Warning("- Blob Connection string: ");
            Log.Warning("\t\"[redacted]\"");
            Log.Warning("- Add hostname to Blob files: " + AddHostNameToBlobFiles.ToString());
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
            Console.WriteLine("Add hostname to Blob files: " + AddHostNameToBlobFiles.ToString() + ".");
            Console.WriteLine("(Needs to be true in dev, will keep it false in production) ");
            Console.WriteLine("Press \"C\" to change this, any other key to keep this value");
            answer = Console.ReadKey(true);
            if (answer.Key == ConsoleKey.C)
            {
                Console.WriteLine("Add hostname to Blob files, changing value, current value is: " + AddHostNameToBlobFiles.ToString());
                Console.WriteLine("Press \"Y\" for 'true' or \"N\" for 'false'.");
                Console.WriteLine("[Any other key will be seen as invalid and will keep the current val.]");
                ConsoleKeyInfo answer2 = Console.ReadKey(true);
                
                if (answer2.Key != ConsoleKey.Y && answer2.Key != ConsoleKey.N)
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
                    bool choice = true;
                    if (answer2.Key == ConsoleKey.N) choice = false;
                    Console.WriteLine("Valid answer, AddHostNameToBlobFiles set to: " + choice.ToString() + ".");
                    Console.WriteLine("");
                    Console.WriteLine("");
                    if (AddHostNameToBlobFiles != choice) SomeSettingsDidChange = true;
                    AddHostNameToBlobFiles = choice;
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
            AddHostNameToBlobFiles = configuration.GetValue<bool>("AppSettings:AddHostNameToBlobFiles", true);//defauts to true!
        }
    }
}
