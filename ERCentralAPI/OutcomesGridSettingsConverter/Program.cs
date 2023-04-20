// See https://aka.ms/new-console-template for more information
using BusinessLibrary.Data;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;

Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();
var builder = WebApplication.CreateBuilder(args);

//add the file logger
string loggerfilename = CreateLogFileName();
//Silly Microsoft does not provide a log-to-file facility, so have to go for Serilog...
//requires Serilog.AspNetCore package.

builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.File(loggerfilename).ReadFrom.Configuration(ctx.Configuration)
        );
_Logger = Log.Logger;
var MSlogger = new Serilog.Extensions.Logging.SerilogLoggerFactory(Program.Logger).CreateLogger<Program>();
var app = builder.Build();
SqlHelper = new SQLHelper(builder.Configuration, MSlogger);
DataConnection.DataConnectionConfigure(SqlHelper);
DoWork();


public partial class Program
{
    //public static SQLHelper? SqlHelper {  get; private set; }
    private static string CreateLogFileName()
    {
        DirectoryInfo logDir = System.IO.Directory.CreateDirectory("LogFiles");
        string LogFilename = logDir.FullName + @"\" + "OutcomesGridSettingsConverter-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
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
    protected static Serilog.ILogger? _Logger;

    //for CSLA objects that don't understand dependency injection, we keep an instance of the logger here, used when we spin a long-lasting thread
    //and we opt for file-system logging of errors, instead of writing to the DB (which we do, on occasion)
    //this is naughty, but it's the best I could think of, given the DI absence in old versions of CSLA
    public static Serilog.ILogger? Logger { get { return _Logger; } }
    public static SQLHelper? SqlHelper;
    public static void DoWork()
    {
        string SQLget = @"select META_ANALYSIS_TITLE, META_ANALYSIS_ID, REVIEW_ID, len(grid_settings) as length,
                            cast(cast(cast(GRID_SETTINGS as varchar(max)) as xml).query('data(/RawData/SerializationString)') as XML).value('.[1]','nvarchar(max)' ) as decoded,
                            cast(cast(GRID_SETTINGS as varchar(max)) as xml).query('data(/RawData/SerializationString)') as val,
                            GRID_SETTINGS
                            from TB_META_ANALYSIS where GRID_SETTINGS is not null";
        List<IncomingXLM> inList = new List<IncomingXLM>();
        using (SqlConnection connection = new SqlConnection(SqlHelper.ER4DB))
        {
            var reader = SqlHelper.ExecuteQueryNonSP(connection, SQLget);
            int line = 0;
            while (reader.Read())
            {
                XDocument xdoc = XDocument.Parse((string)reader["decoded"]);
                
                inList.Add(new IncomingXLM()
                {
                    MaId = (int)reader["META_ANALYSIS_ID"],
                    XMLtoParse = xdoc
                }) ; 

                line++;
                Logger.Debug("Line: " + line.ToString() + " MA Id: " + inList[inList.Count - 1].MaId.ToString());
            }
        }
        foreach (IncomingXLM setting in inList)
        {
            ProcessSettings(setting);
        }
    }
    public static void ProcessSettings(IncomingXLM setting)
    {
        XElement? EntryNode = setting.XMLtoParse.Descendants("R").Descendants().Where(f=> f.Name == "RV" 
        && f.Attribute("IsRoot") != null 
        && f.Attribute("IsRoot").Value == "true").FirstOrDefault();
        if (EntryNode == null) return;

        XElement? FilterDescriptors = EntryNode.Descendants().Where(f => f.Name == "PD"
        && f.Attribute("PN") != null
        && f.Attribute("PN").Value == "FilterDescriptors").FirstOrDefault();
        if (FilterDescriptors != null)
        {
            XElement? FilterDescriptor1 = setting.XMLtoParse.Descendants("R").Descendants().Where(f => f.Name == "RV"
            && f.Attribute("Key") != null
            && f.Attribute("Key").Value == FilterDescriptors.Attribute("RK").Value).FirstOrDefault();
            if (FilterDescriptor1 != null)
            {
                Logger.Debug("Found one FilterDescriptor!");
            }
        }


        XElement? SortDescriptorElRoot = EntryNode.Descendants().Where(f => f.Name == "PD"
        && f.Attribute("PN") != null
        && f.Attribute("PN").Value == "SortDescriptors").FirstOrDefault();
        if (SortDescriptorElRoot != null)
        {

            XElement? SortDescriptorEl1 = setting.XMLtoParse.Descendants("R").Descendants().Where(f => f.Name == "RV"
            && f.Attribute("Key") != null
            && f.Attribute("Key").Value == SortDescriptorElRoot.Attribute("RK").Value).FirstOrDefault();
            if (SortDescriptorEl1 != null)
            {
                XElement? ColumnUniqueNamePD = SortDescriptorEl1.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                            && f.Attribute("PN").Value == "ColumnUniqueName").FirstOrDefault();
                int VKint;
                if (ColumnUniqueNamePD != null && ColumnUniqueNamePD.Attribute("VK") != null 
                    && int.TryParse(ColumnUniqueNamePD.Attribute("VK").Value, out VKint)
                    && VKint > 0)
                {
                    XElement? ColumnUniqueNamePV = setting.XMLtoParse.Descendants("R").Descendants("PV")
                        .Where(f => f.Attribute("Key") != null && f.Attribute("Key").Value == VKint.ToString())
                        .FirstOrDefault();
                    if (ColumnUniqueNamePV != null)
                    {
                        
                        Logger.Debug("Found one sort descriptor!");
                    }
                }
            }
        }
    }
}

public class IncomingXLM
{
    public int MaId { get; set; }
    public XDocument XMLtoParse { get; set; }
}