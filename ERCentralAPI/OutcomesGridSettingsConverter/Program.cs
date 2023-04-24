// See https://aka.ms/new-console-template for more information
using BusinessLibrary.Data;
using EPPIDataServices.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
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

foreach (string Arg in args)
{
    if (Arg.ToLower() == "saveresults") SaveResults = true;
    else if (Arg.ToLower().StartsWith("revid:")) 
    { 
        int revid;
        if (int.TryParse(Arg.Substring(6), out revid)) JustThisReview = revid;
    }
}

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
    private static int JustThisReview = 0;
    private static bool SaveResults = false;
    public static void DoWork()
    {
        string SQLget = @"select META_ANALYSIS_TITLE, META_ANALYSIS_ID, REVIEW_ID, len(grid_settings) as length,
                            cast(cast(cast(GRID_SETTINGS as varchar(max)) as xml).query('data(/RawData/SerializationString)') as XML).value('.[1]','nvarchar(max)' ) as decoded,
                            cast(cast(GRID_SETTINGS as varchar(max)) as xml).query('data(/RawData/SerializationString)') as val,
                            GRID_SETTINGS
                            from TB_META_ANALYSIS where GRID_SETTINGS is not null";
        if (JustThisReview > 0) SQLget += " AND REVIEW_ID = " + JustThisReview.ToString();
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
        List<ParsedValues> ToSave = new List<ParsedValues>();
        foreach (IncomingXLM setting in inList)
        {
            ParsedValues parsed = ProcessSettings(setting);
            if (parsed.HasData()) ToSave.Add(parsed);
        }
        Logger.Debug("Found " + ToSave.Count + " MAs with settings to convert/save.");
        if (SaveResults)
        {
            Logger.Debug("Saving results...");
            using (SqlConnection connection = new SqlConnection(SqlHelper.ER4DB))
            {
                foreach (ParsedValues parsed in ToSave)
                {
                    //1. Sort values, per MA, if any
                    if (parsed.SortByColName != "")
                    {
                        string SQLcmd = "UPDATE TB_META_ANALYSIS set SORTED_FIELD = '" + parsed.SortByColName
                            + "', SORT_DIRECTION = " + (parsed.SortByDirection ? "1" : "0")
                            + Environment.NewLine + "WHERE META_ANALYSIS_ID = " + parsed.MaId.ToString();
                        int SqlRes = SqlHelper.ExecuteNonQueryNonSP(connection, SQLcmd);
                        if (SqlRes < 0)
                        {
                            Logger.Error(Environment.NewLine);
                            Logger.Error("Error saving sort vals for MA: " + parsed.MaId.ToString());
                            Logger.Error(Environment.NewLine);
                        }
                    }
                    foreach(SingleFilterDescriptor SFD in parsed.Filters)
                    {
                        string SQLcmd = "DELETE from TB_META_ANALYSIS_FILTER_SETTING WHERE META_ANALYSIS_ID = " + parsed.MaId.ToString()
                            + " AND COLUMN_NAME = '" + SFD.ColumnUniqueName + "'";
                        int SqlRes = SqlHelper.ExecuteNonQueryNonSP(connection, SQLcmd);
                        if (SqlRes < 0)
                        {
                            Logger.Error(Environment.NewLine);
                            Logger.Error("Error deleting existing filter settings for MA: " + parsed.MaId.ToString() + " colname: " + SFD.ColumnUniqueName);
                            Logger.Error(Environment.NewLine);
                            continue;
                        }
                        SQLcmd = "INSERT INTO TB_META_ANALYSIS_FILTER_SETTING (META_ANALYSIS_ID ,COLUMN_NAME ,SELECTED_VALUES ,FILTER_1_VALUE ,FILTER_1_OPERATOR ,FILTER_1_CASE_SENSITIVE"
                            + ",FIELD_FILTER_LOGICAL_OPERATOR ,FILTER_2_VALUE ,FILTER_2_OPERATOR ,FILTER_2_CASE_SENSITIVE) VALUES (";
                        SQLcmd += parsed.MaId.ToString() + ", '"
                            + SFD.ColumnUniqueName + "', '"
                            + SFD.SelectedDistinctValues + "', '"
                            + SFD.Filter1Val + "', '"
                            + SFD.Filter1Operator + "', "
                            + (SFD.Filter1CaseSensitive ? "1" : "0") + ", '"
                            + SFD.FiltersLogicalOperator + "', '"
                            + SFD.Filter2Val + "', '"
                            + SFD.Filter2Operator + "', "
                            + (SFD.Filter2CaseSensitive ? "1" : "0") + ")";
                        SqlRes = SqlHelper.ExecuteNonQueryNonSP(connection, SQLcmd);
                        if (SqlRes < 0)
                        {
                            Logger.Error(Environment.NewLine);
                            Logger.Error("Error Inserting filter settings for MA: " + parsed.MaId.ToString() + " colname: " + SFD.ColumnUniqueName);
                            Logger.Error(Environment.NewLine);
                        }
                    }
                }
            }
        }
        
    }
    public static ParsedValues ProcessSettings(IncomingXLM setting)
    {
        ParsedValues res = new ParsedValues(setting.MaId);
        XElement? EntryNode = setting.XMLtoParse.Descendants("R").Descendants().Where(f=> f.Name == "RV" 
        && f.Attribute("IsRoot") != null 
        && f.Attribute("IsRoot").Value == "true").FirstOrDefault();
        if (EntryNode == null) return res;

        XElement? FilterDescriptors = EntryNode.Descendants().Where(f => f.Name == "PD"
        && f.Attribute("PN") != null
        && f.Attribute("PN").Value == "FilterDescriptors").FirstOrDefault();
        if (FilterDescriptors != null)
        {
            XElement? FilterDescriptorsListRV = setting.XMLtoParse.Descendants("R").Descendants("RV").Where(f =>f.Attribute("Key") != null
            && f.Attribute("Key").Value == FilterDescriptors.Attribute("RK").Value).FirstOrDefault();
            if (FilterDescriptorsListRV != null)
            {
                IEnumerable<XElement> FilterDescriptorsList = FilterDescriptorsListRV.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                        && f.Attribute("PN").Value == "FilterDescriptors[]");
                foreach (XElement FilterDescriptorPD in FilterDescriptorsList)
                {
                    XElement? FilterDescriptorRV = setting.XMLtoParse.Descendants("R").Descendants("RV").Where(f => f.Attribute("Key") != null
                        && FilterDescriptorPD.Attribute("RK") != null
                        && f.Attribute("Key").Value == FilterDescriptorPD.Attribute("RK").Value).FirstOrDefault();
                    if (FilterDescriptorRV != null)
                    {
                        ProcessSingleFilter(res, setting.XMLtoParse, FilterDescriptorRV);
                    }
                }
                Logger.Debug("Found one FilterDescriptor!");
            }
        }


        XElement? SortDescriptorElRoot = EntryNode.Descendants().Where(f => f.Name == "PD"
        && f.Attribute("PN") != null
        && f.Attribute("PN").Value == "SortDescriptors").FirstOrDefault();
        if (SortDescriptorElRoot != null && SortDescriptorElRoot.Attribute("RK").Value != null)
        {

            XElement? SortDescriptorEl1 = setting.XMLtoParse.Descendants("R").Descendants("RV").Where(f => f.Attribute("Key") != null
            && f.Attribute("Key").Value == SortDescriptorElRoot.Attribute("RK").Value).FirstOrDefault();
            if (SortDescriptorEl1 != null)
            {
                XElement? SortDescriptorEl2 = SortDescriptorEl1.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                            && f.Attribute("PN").Value == "SortDescriptors[]").FirstOrDefault();
                if (SortDescriptorEl2 != null && SortDescriptorEl2.Attribute("RK") != null)
                {
                    XElement? SortDescriptorEl3 = setting.XMLtoParse.Descendants("R").Descendants("RV").Where(f => f.Attribute("Key") != null
                    && f.Attribute("Key").Value == SortDescriptorEl2.Attribute("RK").Value).FirstOrDefault();

                    if (SortDescriptorEl3 != null)
                    {
                        XElement? ColumnUniqueNamePD = SortDescriptorEl3.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                && f.Attribute("PN").Value == "ColumnUniqueName").FirstOrDefault();
                        int VKint;
                        if (ColumnUniqueNamePD != null && ColumnUniqueNamePD.Attribute("VK") != null
                            && int.TryParse(ColumnUniqueNamePD.Attribute("VK").Value, out VKint)
                            && VKint > 0)
                        {
                            XElement? ColumnUniqueNamePV = setting.XMLtoParse.Descendants("P").Descendants("PV")
                                .Where(f => f.Attribute("Key") != null && f.Attribute("Key").Value == VKint.ToString())
                                .FirstOrDefault();
                            if (ColumnUniqueNamePV != null)
                            {
                                XElement? SortValueElement = ColumnUniqueNamePV.Descendants("Value").FirstOrDefault();
                                if (SortValueElement != null && SortValueElement.Value != "")
                                {
                                    res.SortByColName = SortValueElement.Value;
                                    
                                    if (SortValueElement.Value.StartsWith("aa")
                                        || SortValueElement.Value.StartsWith("aq")
                                        || SortValueElement.Value.StartsWith("occ")
                                        ) res.SortByColName = SortValueElement.Value;
                                    else if (SortValueElement.Value == "ESColumn") res.SortByColName = "ES";
                                    else if (SortValueElement.Value == "SEESColumn") res.SortByColName = "SEES";
                                    else if (SortValueElement.Value == "titleColumn") res.SortByColName = "ShortTitle";
                                    else if (SortValueElement.Value == "DescColumn") res.SortByColName = "Title";
                                    else if (SortValueElement.Value == "TimepointColumn") res.SortByColName = "TimepointDisplayValue";
                                    else if (SortValueElement.Value == "OutcomeTypeName") res.SortByColName = "OutcomeTypeName";
                                    else if (SortValueElement.Value == "OutcomeColumn") res.SortByColName = "OutcomeText";
                                    else if (SortValueElement.Value == "InterventionColumn") res.SortByColName = "InterventionText";
                                    else if (SortValueElement.Value == "ComparisonColumn") res.SortByColName = "ControlText";
                                    else if (SortValueElement.Value == "Arm1Column") res.SortByColName = "grp1ArmName";
                                    else if (SortValueElement.Value == "Arm2Column") res.SortByColName = "grp2ArmName";
                                    VKint = -1;
                                    XElement? SortDirectionPD = SortDescriptorEl3.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                    && f.Attribute("PN").Value == "SortDirection").FirstOrDefault();
                                    if (SortDirectionPD != null && SortDirectionPD.Attribute("VK") != null
                                    && int.TryParse(SortDirectionPD.Attribute("VK").Value, out VKint)
                                    && VKint > 0)
                                    {
                                        XElement? SortDirectionPV = setting.XMLtoParse.Descendants("P").Descendants("PV")
                                        .Where(f => f.Attribute("Key") != null && f.Attribute("Key").Value == VKint.ToString())
                                        .FirstOrDefault();
                                        if (SortDirectionPV != null)
                                        {
                                            XElement? SortDirValueElement = SortDirectionPV.Descendants("Value").FirstOrDefault();
                                            if (SortDirValueElement != null)
                                            {
                                                if (SortDirValueElement.Value == "Ascending") res.SortByDirection = true;
                                                else if (SortDirValueElement.Value == "Descending") res.SortByDirection = false;
                                            }
                                            if (res.SortByColName != "")
                                            {
                                                Logger.Debug("Found one sort descriptor!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return res;
    }
    public static void ProcessSingleFilter(ParsedValues container, XDocument XMLtoParse, XElement SingleFilterRootEl)
    {
        SingleFilterDescriptor res = new SingleFilterDescriptor();

        //1. Column name, will return if we can't find this data
        XElement? ColumnUniqueNamePD = SingleFilterRootEl.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                && f.Attribute("PN").Value == "ColumnUniqueName").FirstOrDefault();
        if (ColumnUniqueNamePD == null) return;
        int VKint;
        if (ColumnUniqueNamePD != null && ColumnUniqueNamePD.Attribute("VK") != null
            && int.TryParse(ColumnUniqueNamePD.Attribute("VK").Value, out VKint)
            && VKint > 0)
        {
            XElement? ColumnUniqueNamePV = XMLtoParse.Descendants("P").Descendants("PV")
                                .Where(f => f.Attribute("Key") != null && f.Attribute("Key").Value == VKint.ToString())
                                .FirstOrDefault();
            if (ColumnUniqueNamePV == null) return;
            XElement? ColumnUniqueNameValueElement = ColumnUniqueNamePV.Descendants("Value").FirstOrDefault();
            if (ColumnUniqueNameValueElement == null || ColumnUniqueNameValueElement.Value == "") return;
            res.ColumnUniqueName = ColumnUniqueNameValueElement.Value;
        }
        else return;

        //2. "distinct values" (that's the tickable vals offered by the filter dialog)
        XElement? SelectedDistinctValuesPD = SingleFilterRootEl.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                && f.Attribute("PN").Value == "SelectedDistinctValues").FirstOrDefault();
        if (SelectedDistinctValuesPD != null && SelectedDistinctValuesPD.Attribute("RK") != null)
        {
            XElement? SelectedDistinctValuesRV = XMLtoParse.Descendants("R").Descendants("RV").Where(f => f.Attribute("Key") != null
            && f.Attribute("Key").Value == SelectedDistinctValuesPD.Attribute("RK").Value).FirstOrDefault();
            if (SelectedDistinctValuesRV != null)
            {
                IEnumerable<XElement> SelectedDistinctValueList = SelectedDistinctValuesRV.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                        && f.Attribute("PN").Value == "SelectedDistinctValues");
                string selectedValsString = "";
                foreach (XElement SelectedDistinctValuePD in SelectedDistinctValueList)
                {
                    VKint = -1;
                    if (SelectedDistinctValuePD != null && SelectedDistinctValuePD.Attribute("VK") != null
                        && int.TryParse(SelectedDistinctValuePD.Attribute("VK").Value, out VKint)
                        && VKint > 0)
                    {
                        XElement? SelectedDistinctValuePV = XMLtoParse.Descendants("P").Descendants("PV")
                                            .Where(f => f.Attribute("Key") != null && f.Attribute("Key").Value == VKint.ToString())
                                            .FirstOrDefault();
                        if (SelectedDistinctValuePV != null)
                        {
                            XElement? SelectedDistinctValueElement = SelectedDistinctValuePV.Descendants("Value").FirstOrDefault();
                            if (SelectedDistinctValueElement != null && SelectedDistinctValueElement.Value != "")
                                selectedValsString += SelectedDistinctValueElement.Value + "{¬}";
                        }
                    }
                }
                if (selectedValsString != "") selectedValsString = selectedValsString.Substring(0, selectedValsString.Length - 3);
                if (selectedValsString != "") res.SelectedDistinctValues = selectedValsString;
            }
        }
        //3. Filter1 and connected vals
        XElement? Filter1PD = SingleFilterRootEl.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                && f.Attribute("PN").Value == "Filter1").FirstOrDefault();
        if (Filter1PD != null && Filter1PD.Attribute("RK") != null && Filter1PD.Attribute("RK").Value != "0")
        {
            ProcessFields1Or2(XMLtoParse, res, Filter1PD.Attribute("RK").Value, true);
        }
        //4. FieldFilterLogicalOperator (combines filter1 and filter2)
        XElement? FieldFilterLogicalOperatorPD = SingleFilterRootEl.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                && f.Attribute("PN").Value == "FieldFilterLogicalOperator").FirstOrDefault();
        VKint = -1;
        if (FieldFilterLogicalOperatorPD != null && FieldFilterLogicalOperatorPD.Attribute("VK") != null
            && int.TryParse(FieldFilterLogicalOperatorPD.Attribute("VK").Value, out VKint)
            && VKint > 0)
        {
            XElement? FieldFilterLogicalOperatorValuePV = XMLtoParse.Descendants("P").Descendants("PV")
                                .Where(f => f.Attribute("Key") != null && f.Attribute("Key").Value == VKint.ToString())
                                .FirstOrDefault();
            if (FieldFilterLogicalOperatorValuePV != null)
            {
                XElement? FieldFilterLogicalOperatorValueElement = FieldFilterLogicalOperatorValuePV.Descendants("Value").FirstOrDefault();
                if (FieldFilterLogicalOperatorValueElement != null && FieldFilterLogicalOperatorValueElement.Value != "")
                    res.FiltersLogicalOperator = FieldFilterLogicalOperatorValueElement.Value;
            }
        }

        //5. Filter2 and connected vals
        XElement? Filter2PD = SingleFilterRootEl.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                && f.Attribute("PN").Value == "Filter2").FirstOrDefault();
        if (Filter2PD != null && Filter2PD.Attribute("RK") != null && Filter2PD.Attribute("RK").Value != "0")
        {
            ProcessFields1Or2(XMLtoParse, res, Filter2PD.Attribute("RK").Value, false);
        }

        if (res.HasData()) container.Filters.Add(res);
    }
    private static void ProcessFields1Or2(XDocument XMLtoParse, SingleFilterDescriptor res, string RKval, bool isFilter1)
    {
        XElement? FilterRV = XMLtoParse.Descendants("R").Descendants("RV").Where(f => f.Attribute("Key") != null
            && f.Attribute("Key").Value == RKval).FirstOrDefault();
        if (FilterRV == null) return;
        string Operator = "", FilterValueStr = "";
        bool IsCaseSensitiveRes = false;
        //A: Operator
        XElement? OperatorPD = FilterRV.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                && f.Attribute("PN").Value == "Operator").FirstOrDefault();
        if (OperatorPD == null) return;
        int VKint;
        if (OperatorPD != null && OperatorPD.Attribute("VK") != null
                        && int.TryParse(OperatorPD.Attribute("VK").Value, out VKint)
                        && VKint > 0)
        {
            XElement? OperatorValuePV = XMLtoParse.Descendants("P").Descendants("PV")
                                            .Where(f => f.Attribute("Key") != null && f.Attribute("Key").Value == VKint.ToString())
                                            .FirstOrDefault();
            if (OperatorValuePV != null)
            {
                XElement? OperatorElement = OperatorValuePV.Descendants("Value").FirstOrDefault();
                if (OperatorElement != null && OperatorElement.Value != "") Operator = OperatorElement.Value;
            }
        }
        //B: Value
        XElement? ValuePD = FilterRV.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                && f.Attribute("PN").Value == "Value").FirstOrDefault();
        if (ValuePD == null) return;
        VKint = -1;
        if (ValuePD != null && ValuePD.Attribute("VK") != null
                        && int.TryParse(ValuePD.Attribute("VK").Value, out VKint)
                        && VKint > 0)
        {
            XElement? ValuePV = XMLtoParse.Descendants("P").Descendants("PV")
                                            .Where(f => f.Attribute("Key") != null && f.Attribute("Key").Value == VKint.ToString())
                                            .FirstOrDefault();
            if (ValuePV != null)
            {
                XElement? ValueElement = ValuePV.Descendants("Value").FirstOrDefault();
                if (ValueElement != null && ValueElement.Value != "") FilterValueStr = ValueElement.Value;
            }
        }
        //C: IsCaseSensitive (bool)
        XElement? IsCaseSensitivePD = FilterRV.Descendants("D").Descendants("PD").Where(f => f.Attribute("PN") != null
                                && f.Attribute("PN").Value == "IsCaseSensitive").FirstOrDefault();
        if (IsCaseSensitivePD == null) return;
        VKint = -1;
        if (IsCaseSensitivePD != null && IsCaseSensitivePD.Attribute("VK") != null
                        && int.TryParse(IsCaseSensitivePD.Attribute("VK").Value, out VKint)
                        && VKint > 0)
        {
            XElement? IsCaseSensitivePV = XMLtoParse.Descendants("P").Descendants("PV")
                                            .Where(f => f.Attribute("Key") != null && f.Attribute("Key").Value == VKint.ToString())
                                            .FirstOrDefault();
            if (IsCaseSensitivePV != null)
            {
                XElement? ValueElement = IsCaseSensitivePV.Descendants("Value").FirstOrDefault();
                if (ValueElement != null && ValueElement.Value == "true") IsCaseSensitiveRes = true;
            }
        }
        if (FilterValueStr != "" && Operator != "")
        {
            if (isFilter1)
            {
                res.Filter1CaseSensitive = IsCaseSensitiveRes;
                res.Filter1Val = FilterValueStr;
                res.Filter1Operator = Operator;
            }
            else
            {
                res.Filter2CaseSensitive = IsCaseSensitiveRes;
                res.Filter2Val = FilterValueStr;
                res.Filter2Operator = Operator;
            }
        }
    }
}

public class IncomingXLM
{
    public int MaId { get; set; }
    public XDocument XMLtoParse { get; set; }
}

public class ParsedValues
{
    public ParsedValues(int maId)
    {
        MaId = maId;
    }
    public int MaId { get; set; }
    public string SortByColName { get; set; } = "";
    public bool SortByDirection { get; set; } = true;

    public List<SingleFilterDescriptor> Filters { get; set; } = new List<SingleFilterDescriptor>();
    public bool HasData()
    {
        if (SortByColName != "") return true;
        else if (Filters.FindAll(f => f.HasData() == true).Any()) return true;
        return false;
    }
}
public class SingleFilterDescriptor
{
    public string ColumnUniqueName { get; set; } = "";
    public string SelectedDistinctValues { get; set; } = "";
    public string Filter1Val { get; set; } = "";
    public string Filter1Operator { get; set; } = "";
    public bool Filter1CaseSensitive { get; set; } = false;
    public string FiltersLogicalOperator { get; set; } = "And";
    public string Filter2Val { get; set; } = "";
    public string Filter2Operator { get; set; } = "";
    public bool Filter2CaseSensitive { get; set; } = false;

    public bool HasData()
    {
        if (ColumnUniqueName == "") return false;
        else if (SelectedDistinctValues != "") return true;
        else if (Filter1Val != "" && Filter1Operator != "") return true;
        else if (Filter2Val != "" && Filter2Operator != "") return true;
        return false;
    }

}