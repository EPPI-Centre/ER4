using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;

/// <summary>
/// Summary description for Utils
/// </summary>
public class Utils
{
    public Utils()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static string ConnectionString
    {
        get
        {
            string name = Environment.MachineName;
            //if (name.ToLower() == "epi2" | name.ToLower() == "ssru30") // to run on live database locally
            if (name.ToLower() == "epi2")// | name.ToLower() == "ssru30")
                return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ER4ConnectionString"].ConnectionString;
            else if (name.ToLower() == "epi3")// | name.ToLower() == "ssru30")
                return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ER4ConnectionStringAz"].ConnectionString;
            else if (name.ToLower() == "silvie2")
                return "Data Source=SILVIE2;Initial Catalog=Reviewer;Integrated Security=True";
            else if (name.ToLower() == "bk-epi")// | name.ToLower() == "ssru38")
                return "Data Source=db-epi;Initial Catalog=TestReviewer;Integrated Security=True";
            else if (name.ToLower() == "ssrulap41")
                return "Data Source=SSRULAP41\\LAP2008;Initial Catalog=Reviewer;Integrated Security=True";
            else
                return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ER4ConnectionString1"].ConnectionString;
        }
    }

    public static string AdmConnectionString
    {
        get
        {
            string name = Environment.MachineName;
            //if (name.ToLower() == "epi2" | name.ToLower() == "ssru30") // to run on live database locally
            if (name.ToLower() == "epi2")// | name.ToLower() == "ssru30")
                return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ADMConnectionString"].ConnectionString;
            else if (name.ToLower() == "epi3")// | name.ToLower() == "ssru30")
                return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ADMConnectionStringAz"].ConnectionString;
            else if (name.ToLower() == "silvie2")
                return "Data Source=SILVIE2;Initial Catalog=ReviewerAdmin;Integrated Security=True";
            else if (name.ToLower() == "bk-epi")// | name.ToLower() == "ssru38")
                return "Data Source=db-epi;Initial Catalog=TestReviewerAdmin;Integrated Security=True";
            else if (name.ToLower() == "ssrulap41")
                return "Data Source=SSRULAP41\\LAP2008;Initial Catalog=ReviewerAdmin;Integrated Security=True";
            else
                return System.Configuration.ConfigurationManager.ConnectionStrings["WcfHostWeb.Properties.Settings.ADMConnectionString1"].ConnectionString;
        }
    }
    public static string WPMServerUrl
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["WPMServerUrl"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["WPMServerUrl"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static string UCLWPMServerUrl
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["UCLWPMServerUrl"];
            if (tmp != null) return tmp;
            else return "";
            //return System.Configuration.ConfigurationManager.AppSettings["UCLWPMServerUrl"];
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["UCLWPMServerUrl"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static string WPMCallBackURL
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["WPMCallBackURL"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["WPMCallBackURL"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    //style.css
    public static string WPMstyle
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["WPMstyle"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["WPMstyle"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    //clientID
    public static string WPMclientID
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["WPMclientID"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["WPMclientID"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static string UCLWPMclientID
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["UCLWPMclientID"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["UCLWPMclientID"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static string ProxyURL
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["ProxyURL"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["ProxyURL"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static bool USEproxyOUT
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["USEproxyOUT"];
            if (tmp != null && tmp == "1") return true;
            else return false;
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["USEproxyOUT"];
            //    if (customSetting != null && customSetting.Value == "1")
            //        return true;
            //}
            //return false;
        }
    }
    public static bool USEproxyIN
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["USEproxyIN"];
            if (tmp != null && tmp == "1") return true;
            else return false;
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["USEproxyIN"];
            //    if (customSetting != null && customSetting.Value == "1")
            //        return true;
            //}
            //return false;
        }
    }
    //pathwayid
    public static string WPMpathwayid
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["WPMpathwayid"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["WPMpathwayid"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static string UCLWPMpathwayid
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["UCLWPMpathwayid"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["UCLWPMpathwayid"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    //departmentid
    public static string WPMdepartmentid
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["WPMdepartmentid"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["WPMdepartmentid"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static string UCLWPMdepartmentid
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["UCLWPMdepartmentid"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["UCLWPMdepartmentid"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    //live
    public static string WPMisLive
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["WPMisLive"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["WPMisLive"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static string UCLWPMisLive
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["UCLWPMisLive"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["UCLWPMisLive"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static string UCLWPMsharedS
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["UCLWPMsharedS"];
            if (tmp != null) return tmp;
            else return "";
        }
    }
    public static string SMTP
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["SMTP"];
            if (tmp != null) return tmp;
            else return "";
            //System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/ER4Manager_1");
            //if (rootWebConfig1.AppSettings.Settings.Count > 0)
            //{
            //    System.Configuration.KeyValueConfigurationElement customSetting = rootWebConfig1.AppSettings.Settings["SMTP"];
            //    if (customSetting != null)
            //        return customSetting.Value;
            //}
            //return "";
        }
    }
    public static string EPPIVisUrl
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["EPPIVisUrl"];
            if (tmp != null) return tmp;
            else return "https://eppi.ioe.ac.uk/eppi-vis/";
        }
    }
    public static string SMTPUser
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["SMTPUser"];
            if (tmp != null) return tmp;
            else return "";
        }
    }
    public static string SMTPAuthentic
    {
        get
        {
            string tmp = System.Configuration.ConfigurationManager.AppSettings["SMTPAuthentic"];
            if (tmp != null) return tmp;
            else return "";
        }
    }
    private static SmtpClient smtpClient()
    {
        //from https://docs.microsoft.com/en-us/answers/questions/400152/authentication-failed-because-the-remote-party-has.html
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
        SmtpClient smtp = new SmtpClient(SMTP);
        smtp.UseDefaultCredentials = false;
        System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential(SMTPUser, SMTPAuthentic);
        smtp.Credentials = SMTPUserInfo;
        smtp.EnableSsl = true; smtp.Port = 587;
        return smtp;
    }
    public static string EmailFrom
    {
        get
        {
            return "EPPISupport@ucl.ac.uk";
        }
    }
    public static string getMD5Hash(string input)
    {

        System.Security.Cryptography.MD5CryptoServiceProvider CryptoService;
        CryptoService = new System.Security.Cryptography.MD5CryptoServiceProvider();
        string sharedS = "***REMOVED***";//***REMOVED***
        byte[] InputBytes = System.Text.Encoding.Default.GetBytes(input + sharedS);
        InputBytes = CryptoService.ComputeHash(InputBytes);
        string s1 = BitConverter.ToString(InputBytes).Replace("-", ""), s2 = BitConverter.ToString(InputBytes);
        return BitConverter.ToString(InputBytes).Replace("-", "").ToLower();
    }
    public static string getMD5HashUCL(string input)
    {

        System.Security.Cryptography.MD5CryptoServiceProvider CryptoService;
        CryptoService = new System.Security.Cryptography.MD5CryptoServiceProvider();
        string sharedS = UCLWPMsharedS;
        byte[] InputBytes = System.Text.Encoding.Default.GetBytes(input + sharedS);
        InputBytes = CryptoService.ComputeHash(InputBytes);
        string s1 = BitConverter.ToString(InputBytes).Replace("-", ""), s2 = BitConverter.ToString(InputBytes);
        return BitConverter.ToString(InputBytes).Replace("-", "").ToLower();
    }




    public static string GetSessionString(string key)
    {
        if (HttpContext.Current.Session[key] == null)
        {
            return null;
        }
        else
        {
            return (string)(HttpContext.Current.Session[key]);
        }
    }

    public static bool SetSessionString(string key, string value)
    {
        if (key != "")
        {
            HttpContext.Current.Session[key] = value;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsNumeric(object Expression)
    {
        // Variable to collect the Return value of the TryParse method.
        bool isNum;

        // Define variable to collect out parameter of the TryParse method. If the conversion fails, the out parameter is zero.
        double retNum;

        // The TryParse method converts a string in a specified style and culture-specific format to its double-precision floating point number equivalent.
        // The TryParse method does not generate an exception if the conversion fails. If the conversion passes, True is returned. If it does not, False is returned.
        isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
        return isNum;
    }


    public static string Login(string userid, string password, string ip_address, bool isAdmDB)
    {
        //Utils.SetSessionString("DB", DB);

        IDataReader reader = Utils.GetReader(isAdmDB, "st_ContactLogin",
            userid, password, ip_address);
        // Now check to see if the login was passed
        string result = null;

        if (reader.Read())
        {
            Utils.SetSessionString("Contact_ID", reader["CONTACT_ID"].ToString());
            Utils.SetSessionString("IsAdm", reader["IS_SITE_ADMIN"].ToString());
            result = reader["CONTACT_ID"].ToString();
            Utils.SetSessionString("ContactName", reader["CONTACT_NAME"].ToString());
            if (reader["IsSLA"].ToString() != "0")
                Utils.SetSessionString("IsSiteLicenseAdm", "1");
            if (reader["IsOA"].ToString() != "0")
                Utils.SetSessionString("IsOrganisationAdm", "1");
        }
        else
        {
            Utils.SetSessionString("DB", "");
        }
        reader.Close();
        return result;
    }

    public static SqlDataReader ReturnReader(string sql, bool isAdmDB)
    {
        //SqlConnection myConnection = new SqlConnection(ConfigurationManager.AppSettings[Utils.GetSessionString("DB")]);
        SqlConnection myConnection;
        if (isAdmDB) myConnection = new SqlConnection(Utils.AdmConnectionString);
        else myConnection = new SqlConnection(Utils.ConnectionString);
        SqlCommand myCommand = new SqlCommand(sql, myConnection);
        myCommand.CommandType = CommandType.Text;
        myConnection.Open();
        SqlDataReader result = myCommand.ExecuteReader(CommandBehavior.CloseConnection);
        return result;
    }

    public static DataSet ReturnDataSet(string sql)
    {
        SqlConnection myConnection = new SqlConnection(ConfigurationManager.AppSettings[Utils.GetSessionString("DB")]);
        SqlDataAdapter sda = new SqlDataAdapter(sql, myConnection);
        DataSet result = new DataSet();
        sda.Fill(result);
        return result;
    }

    public static void ExecuteQuery(string SQL, bool isAdmDB)
    {
        //SqlConnection myConnection = new SqlConnection(ConfigurationManager.AppSettings[Utils.GetSessionString("DB")]);
        SqlConnection myConnection;
        if (isAdmDB) myConnection = new SqlConnection(Utils.AdmConnectionString);
        else myConnection = new SqlConnection(Utils.ConnectionString);
        SqlCommand myCommand = new SqlCommand(SQL, myConnection);
        myCommand.CommandType = CommandType.Text;
        myConnection.Open();
        myCommand.ExecuteNonQuery();
        myConnection.Close();
    }

    /* ***
     * 
     *		THIS SECTION CONTAINS GENERIC ROUTINES TO ACCESS THE DATABASE THROUGH MICROSOFT'S ENTERPRISE LIBRARY
     *		DATA ACCESS APPLICATION BLOCK - now recoded to bypass enterprise library
     * 
     * */
    public static void SendErrorEmail(Exception error, string spName, params object[] spParams)
    {
        if (HttpContext.Current.Request.UserHostAddress.ToString() != "127.0.0.1")
        {
            try
            {
                //we have a new (05/08) mail account now, so error messages will be sent also to that address
                //string mailTo = "j.thomas@ioe.ac.uk";
                //string mailCC = "j.brunton@ioe.ac.uk, EPPIsupport@ioe.ac.uk";
               
                MailMessage msg = new MailMessage();
                msg.To.Add(EmailFrom);
                //msg.CC.Add(mailCC);

                msg.From = new MailAddress(EmailFrom);

                msg.Subject = "ER4 Manager: database error report";
                msg.Body = error.Message + "<br><br>" + error.Source + "<br><br>" +
                    error.StackTrace + "<br><br>" + error.InnerException;
                msg.Body = "Reviewer: " + GetSessionString("Reviewer") + "<br><br>" +
                    "Database: " + GetSessionString("Database") + "<br><br>" +
                    "Review: " + GetSessionString("Review") + "<br><br>" +
                    "Request: " + HttpContext.Current.Request.RawUrl + "<br><br>" +
                    "IP address: " + HttpContext.Current.Request.UserHostAddress.ToString() + "<br><br>" +
                    msg.Body.Replace(Environment.NewLine, "<br>");
                msg.Body += "<br>Name of statement: " + spName + "<br>";
                msg.Body += "<br>Params: ";
                for (int i = 0; i < spParams.Length; i++)
                {
                    msg.Body += spParams[i].ToString() + "<br>";
                }
                msg.IsBodyHtml = true;
                // the institute changed this number messages weren't getting sent
                // We need to know when this number changes
                // SmtpClient smtp = new SmtpClient("144.82.31.3");
                //SmtpClient smtp = new SmtpClient("144.82.35.189");
                SmtpClient smtp = smtpClient();
                if (GetSessionString("Reviewer") == "")
                {
                    // do nothing - just someone who's not logged in
                }
                else
                {
                    smtp.Send(msg);
                }
            }
            catch
            {
                // message fails: c'est la vie
            }
        }
    }

    /// <summary>
    /// The session variable DB is set when someone logs in.  If the variable is absent the user is
    /// redirected back to the login page.
    /// </summary>
    /// <param name="pServer"></param>
    /// <returns></returns>
    public static string GetDatabaseName(HttpServerUtility pServer)
    {
        if ((Utils.GetSessionString("DB") == "") ||
            (Utils.GetSessionString("DB") == null))
        {
            pServer.Transfer("login.aspx?reason=NoDatabase");
        }
        return Utils.GetSessionString("DB");
    }

    /// <summary>
    /// Returns an sqldatareader containing the results of a sp_sproc_columns call.
    /// </summary>
    /// <param name="StoredProcedureName"></param>
    /// <param name="myConnection"></param>
    /// <returns></returns>
    public static SqlDataReader GetParameters(string StoredProcedureName,
        SqlConnection myConnection)
    {
        SqlCommand myCommand = new SqlCommand("execute sp_sproc_columns " + StoredProcedureName, myConnection);
        myCommand.CommandType = CommandType.Text;

        try
        {
            // myConnection.Open();
            SqlDataReader result = myCommand.ExecuteReader();

            return result;
        }
        catch (Exception e)
        {
            SendErrorEmail(e, StoredProcedureName, "Error in GetParameters");
            return null;
        }
    }

    public static SqlDataReader GetParameters1(string StoredProcedureName,
     SqlConnection myConnection)
    {
        SqlCommand myCommand = new SqlCommand("execute sp_sproc_columns " + StoredProcedureName, myConnection);
        myCommand.CommandType = CommandType.Text;

        try
        {
            // myConnection.Open();
            SqlDataReader result = myCommand.ExecuteReader();

            return result;
        }
        catch (Exception e)
        {
            SendErrorEmail(e, StoredProcedureName, "Error in GetParameters");
            return null;
        }
    }

    public static SqlDbType GetSqlDbType(string paramType)
    {
        switch (paramType.ToLower())
        {
            case ("bigint"):
                return SqlDbType.BigInt;
            case ("binary"):
                return SqlDbType.Binary;
            case ("bit"):
                return SqlDbType.Bit;
            case ("char"):
                return SqlDbType.Char;
            case ("datetime"):
                return SqlDbType.DateTime;
            case ("decimal"):
                return SqlDbType.Decimal;
            case ("float"):
                return SqlDbType.Float;
            case ("image"):
                return SqlDbType.Image;
            case ("int"):
                return SqlDbType.Int;
            case ("money"):
                return SqlDbType.Money;
            case ("nchar"):
                return SqlDbType.NChar;
            case ("ntext"):
                return SqlDbType.NText;
            case ("nvarchar"):
                return SqlDbType.NVarChar;
            case ("real"):
                return SqlDbType.Real;
            case ("smalldatetime"):
                return SqlDbType.SmallDateTime;
            case ("smallint"):
                return SqlDbType.SmallInt;
            case ("smallmoney"):
                return SqlDbType.SmallMoney;
            case ("text"):
                return SqlDbType.Text;
            case ("timestamp"):
                return SqlDbType.Timestamp;
            case ("tinyint"):
                return SqlDbType.TinyInt;
            case ("uniqueidentifier"):
                return SqlDbType.UniqueIdentifier;
            case ("varbinary"):
                return SqlDbType.VarBinary;
            case ("varchar"):
                return SqlDbType.VarChar;
            case ("variant"):
                return SqlDbType.Variant;
            default:
                return SqlDbType.Variant;
        }
    }

    public static ParameterDirection GetParamDirection(string paramDirection)
    {
        switch (paramDirection)
        {
            case "1":
                return ParameterDirection.Input;
            case "2":
                return ParameterDirection.Output;
            default:
                return ParameterDirection.ReturnValue;
        }
    }

    /// <summary>
    /// Returns an idatareader from a stored procedure (spName could also be an sql statement)
    /// </summary>
    /// <param name="pServer">Always enter 'Server' here</param>
    /// <param name="spName">Name of the stored procedure</param>
    /// <param name="spParams">Parameters: ALWAYS IN THE CORRECT ORDER</param></param>
    /// <returns></returns>
    public static IDataReader GetReader(bool isAdmDB, string spName, params object[] spParams)
    {
        //string DatabaseName = GetDatabaseName(pServer);

        //SqlConnection myConnection = new SqlConnection(ConfigurationSettings.AppSettings[DatabaseName]);
        SqlConnection myConnection;
        if (isAdmDB) myConnection = new SqlConnection(Utils.AdmConnectionString);
        else myConnection = new SqlConnection(Utils.ConnectionString);
        myConnection.Open();
        SqlCommand myCommand = new SqlCommand(spName, myConnection);
        myCommand.CommandType = CommandType.StoredProcedure;
        myCommand.CommandTimeout = 60;
        int i = 0;
        if (spParams.Length > 0) // there are parameters that need creating
        {
            SqlDataReader sdrParams = GetParameters1(spName, myConnection);
            while (sdrParams.Read())
            {
                // the first record is the ReturnValue from the stored procedure - we don't create this parameter,
                // so need to make sure we skip it - hence this line:
                if (GetParamDirection(sdrParams["COLUMN_TYPE"].ToString()) != ParameterDirection.ReturnValue)
                {
                    SqlParameter param = new SqlParameter(sdrParams["COLUMN_NAME"].ToString(),
                        GetSqlDbType(sdrParams["TYPE_NAME"].ToString()),
                        Convert.ToInt32(sdrParams["LENGTH"].ToString()));
                    param.Direction = GetParamDirection(sdrParams["COLUMN_TYPE"].ToString());
                    if (spParams[i] != null)
                    {
                        param.Value = spParams[i].ToString();
                    }
                    else
                    {
                        param.Value = null;
                    }
                    i++;
                    myCommand.Parameters.Add(param);

                }
            }
            sdrParams.Close();
        }
        try
        {
            SqlDataReader result = myCommand.ExecuteReader(CommandBehavior.CloseConnection);
            return result;
        }
        catch (Exception e)
        {
            SendErrorEmail(e, spName, spParams);
            return null;
        }
    }


    public static IDataReader GetReader1(bool isAdmDB, string spName, SqlConnection myConnection, params object[] spParams)
    {
        //string DatabaseName = GetDatabaseName(pServer);

        //SqlConnection myConnection = new SqlConnection(ConfigurationSettings.AppSettings[DatabaseName]);
        //SqlConnection myConnection;
        if (isAdmDB) myConnection.ConnectionString = Utils.AdmConnectionString;
        else myConnection.ConnectionString = Utils.ConnectionString;

        myConnection.Open();
        SqlCommand myCommand = new SqlCommand(spName, myConnection);
        myCommand.CommandType = CommandType.StoredProcedure;
        myCommand.CommandTimeout = 60;
        int i = 0;
        if (spParams.Length > 0) // there are parameters that need creating
        {
            SqlDataReader sdrParams = GetParameters1(spName, myConnection);
            while (sdrParams.Read())
            {
                // the first record is the ReturnValue from the stored procedure - we don't create this parameter,
                // so need to make sure we skip it - hence this line:
                if (GetParamDirection(sdrParams["COLUMN_TYPE"].ToString()) != ParameterDirection.ReturnValue)
                {
                    SqlParameter param = new SqlParameter(sdrParams["COLUMN_NAME"].ToString(),
                        GetSqlDbType(sdrParams["TYPE_NAME"].ToString()),
                        Convert.ToInt32(sdrParams["LENGTH"].ToString()));
                    param.Direction = GetParamDirection(sdrParams["COLUMN_TYPE"].ToString());
                    if (spParams[i] != null)
                    {
                        param.Value = spParams[i];
                    }
                    else
                    {
                        param.Value = null;
                    }
                    i++;
                    myCommand.Parameters.Add(param);

                }
            }
            sdrParams.Close();
        }
        try
        {
            SqlDataReader result = myCommand.ExecuteReader(CommandBehavior.CloseConnection);
            return result;
        }
        catch (Exception e)
        {
            SendErrorEmail(e, spName, spParams);
            return null;
        }
    }



    public static IDataReader GetReaderFromQuery(HttpServerUtility pServer, string sql)
    {
        string DatabaseName = GetDatabaseName(pServer);

        //SqlConnection myConnection = new SqlConnection(ConfigurationSettings.AppSettings[DatabaseName]);
        SqlConnection myConnection = new SqlConnection(ConfigurationManager.AppSettings[DatabaseName]);
        myConnection.Open();
        SqlCommand myCommand = new SqlCommand(sql, myConnection);
        myCommand.CommandTimeout = 60;

        myCommand.CommandType = CommandType.Text;
        try
        {
            SqlDataReader result = myCommand.ExecuteReader(CommandBehavior.CloseConnection);
            return result;
        }
        catch (Exception e)
        {
            SendErrorEmail(e, "GetReaderFromQuery", sql);
            pServer.Transfer("error.aspx?error=" + e.Message);
            return null;
        }
    }

    public static DataSet GetDataSet(HttpServerUtility pServer, string sql)
    {
        string DatabaseName = GetDatabaseName(pServer);

        //SqlConnection myConnection = new SqlConnection(ConfigurationSettings.AppSettings[DatabaseName]);
        SqlConnection myConnection = new SqlConnection(ConfigurationManager.AppSettings[DatabaseName]);
        myConnection.Open();
        SqlDataAdapter sda = new SqlDataAdapter(sql, myConnection);
        sda.SelectCommand.CommandTimeout = 60;

        try
        {
            DataSet result = new DataSet();
            sda.Fill(result);
            return result;
        }
        catch (Exception e)
        {
            SendErrorEmail(e, "Function - GetDataSet", sql);
            pServer.Transfer("error.aspx?error=" + e.Message);
            return null;
        }
    }





    /// <summary>
    /// Executes a stored procedure.
    /// Parameters have to be in the correct order!
    /// </summary>
    /// <param name="pServer">Always enter 'Server' here</param>
    /// <param name="spName">Name of stored procedure</param>
    /// <param name="spParams">Values of parameters IN ORDER THEY APPEAR IN STORED PROCEDURE</param>
    /// <returns></returns>
    public static int ExecuteSP(bool isAdmDB, HttpServerUtility pServer, string spName, params object[] spParams)
    {
        //string DatabaseName = GetDatabaseName(pServer);

        //SqlConnection myConnection = new SqlConnection(ConfigurationSettings.AppSettings[DatabaseName]);
        SqlConnection myConnection;
        //myConnection = new SqlConnection(ConfigurationManager.AppSettings[DatabaseName]);
        if (isAdmDB) myConnection = new SqlConnection(Utils.AdmConnectionString);
        else myConnection = new SqlConnection(Utils.ConnectionString);
        myConnection.Open();

        SqlCommand myCommand = new SqlCommand(spName, myConnection);

        myCommand.CommandType = CommandType.StoredProcedure;
        myCommand.CommandTimeout = 60;

        int i = 0;
        if (spParams.Length > 0) // there are parameters that need creating
        {
            SqlDataReader sdrParams = GetParameters(spName, myConnection);
            while (sdrParams.Read())
            {
                if (GetParamDirection(sdrParams["COLUMN_TYPE"].ToString()) != ParameterDirection.ReturnValue)
                {
                    SqlParameter param = new SqlParameter(sdrParams["COLUMN_NAME"].ToString(),
                        GetSqlDbType(sdrParams["TYPE_NAME"].ToString()),
                        Convert.ToInt32(sdrParams["LENGTH"].ToString()));
                    param.Direction = GetParamDirection(sdrParams["COLUMN_TYPE"].ToString());
                    if (spParams[i] != null)
                    {
                        param.Value = spParams[i].ToString();
                    }
                    else
                    {
                        param.Value = null;
                    }
                    i++;
                    myCommand.Parameters.Add(param);
                }
            }
            sdrParams.Close();
        }
        try
        {
            //myConnection.Open();
            myCommand.ExecuteNonQuery();
            myConnection.Close();
            myConnection.Dispose();
        }
        catch (Exception e)
        {
            SendErrorEmail(e, spName, spParams);
            pServer.Transfer("error.aspx?error=" + e.Message);
        }

        return -1;
    }

    /// <summary>
    /// Example:
    /// SqlParameter[] paramList = new SqlParameter[2];
    /// paramList[0] = new SqlParameter("@WEB_DB", SqlDbType.NVarChar, 255, ParameterDirection.Input,
    ///     true, 0, 0, null, DataRowVersion.Default, tbWebDB.Text);
    /// paramList[1] = new SqlParameter("@WEBDB_ID", SqlDbType.BigInt, 8, ParameterDirection.Output,
    ///     true, 0, 0, null, DataRowVersion.Default, "");
    /// Utils.ExecuteSPWithReturnValues(Server, "st_REVIEW_WEBDB_CREATE", paramList);
    /// Server.Transfer("WebDB_Create.aspx?WebDBID=" + paramList[1].Value.ToString());
    /// </summary>
    /// <param name="pServer"></param>
    /// <param name="spName"></param>
    /// <param name="spParams"></param>
    /// <returns></returns>
    public static int ExecuteSPWithReturnValues(bool isAdmDB, HttpServerUtility pServer, string spName,
        params SqlParameter[] spParams)
    {
        //string DatabaseName = GetDatabaseName(pServer);

        //SqlConnection myConnection = new SqlConnection(ConfigurationSettings.AppSettings[DatabaseName]);
        //SqlConnection myConnection = new SqlConnection(ConfigurationManager.AppSettings[DatabaseName]);
        //myConnection.Open();


        //string DatabaseName = GetDatabaseName(pServer);

        //SqlConnection myConnection = new SqlConnection(ConfigurationSettings.AppSettings[DatabaseName]);
        SqlConnection myConnection;
        //myConnection = new SqlConnection(ConfigurationManager.AppSettings[DatabaseName]);
        if (isAdmDB) myConnection = new SqlConnection(Utils.AdmConnectionString);
        else myConnection = new SqlConnection(Utils.ConnectionString);
        myConnection.Open();



        SqlCommand myCommand = new SqlCommand(spName, myConnection);

        myCommand.CommandType = CommandType.StoredProcedure;
        myCommand.CommandTimeout = 60;

        for (int i = 0; i < spParams.Length; i++)
        {
            myCommand.Parameters.Add(spParams[i]);
        }
        try
        {
            //myConnection.Open();
            myCommand.ExecuteNonQuery();
            myConnection.Close();
            myConnection.Dispose();
        }
        catch (Exception e)
        {
            SendErrorEmail(e, spName, spParams);
            pServer.Transfer("error.aspx?error=" + e.Message);
        }
        return -1;
    }
    /*
    public static string ForgottenPmailOld(string mailTo, string stCont, string passwD, string stAdditional)
    //function to send the forgotten password to a forgetful user and 
    //to alert eppisupport that someone is trying to use the forgotten password feature
    //stAdditional is added at the beginning of the message, used to add info for the admin copy of the message
    {
        //part of this code was originally in reviewers.aspx.cs
        string mailFrom = "EPPIsupport@ioe.ac.uk";
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);

        msg.From = new MailAddress(mailFrom);

        //msg.To = e.Item.Cells[2].Text;
        //msg.From = "eppi.edinfo@ioe.ac.uk";
        msg.Subject = "EPPI-Reviewer: password information";
        msg.IsBodyHtml = true;
        //msg.Body = "&gt;&gt;This is an automatically generated message&lt;&lt; <br>";
        msg.Body = "EPPI-Reviewer: Software for research synthesis";
        msg.Body += "<br><br>";
        msg.Body += "Dear " + stCont;
        msg.Body += "<br><br>";
        msg.Body += stAdditional + " " + stCont + " has requested a password reminder from EPPI-Reviewer 4.0.<BR>";
        msg.Body += "The password is: <b>" + passwD + "</b><br>";


        msg.Body += "<br><br>";
        msg.Body += "The login page for EPPI-Reviewer 4 can be found at:";
        msg.Body += "<a href='http://eppi.ioe.ac.uk/eppireviewer4/'>http://eppi.ioe.ac.uk/eppireviewer4/</a>";
        msg.Body += "<br><br>";
        msg.Body += "On the EPPI-Reviewer 4 gateway (<a href='http://eppi.ioe.ac.uk/cms/er4'>http://eppi.ioe.ac.uk/cms/er4</a>) can be found details about your trial account, the user manual, the support forum, ";
        msg.Body += "and links to instructional videos on our YouTube channel <a href='http://www.youtube.com/user/eppireviewer4'>http://www.youtube.com/user/eppireviewer4</a>.";
        msg.Body += "<br><br>";
        msg.Body += "EPPI-Reviewer 4 is developed and maintained by the EPPI-Centre of the Institute of Education at the University of London, UK. ";
        msg.Body += "To find out more about the work of the EPPI-Centre please visit our website <a href='http://eppi.ioe.ac.uk/'>eppi.ioe.ac.uk</a>.";
        msg.Body += "<br>";
        msg.Body += "<br><br>In case this message is unexpected, please don't hesitate to contact our support staff:<br>";
        msg.Body += "<a href='mailto:EPPIsupport@ioe.ac.uk'>EPPIsupport@ioe.ac.uk</a>";
        msg.Body += "<br><br>";
        //msg.Body += "&gt;&gt;This is an automatically generated message&lt;&lt;";



        //msg.Body += "In case this message is unexpected, please don't hesitate to contact our support staff:<br>";
        //msg.Body += "<a href='mailto:EPPIsupport@ioe.ac.uk'>EPPIsupport@ioe.ac.uk</a>";
        //msg.BodyFormat = System.Web.Mail.MailFormat.Text;
        //SmtpMail.SmtpServer = "144.82.31.3";


        // the institute changed this number messages weren't getting sent
        // We need to know when this number changes
        // SmtpClient smtp = new SmtpClient("144.82.31.3");
        //SmtpClient smtp = new SmtpClient("144.82.35.189");
        //to make this work more reliably, let's use the server name, with no DNS extension
        //in this way we take advantage of windows name resolution routines, that will try completing
        //the DNS names with the suffix of the present domain. This should let the system work even if the
        //domain name is changed
        SmtpClient smtp = new SmtpClient(SMTP);
        // next line had inst in the credentials. It doesn't appear to be necessary. (JB 14092011)
        //System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential(@"inst\EPPIsupport", "xyz");
        System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential("EPPIsupport@ioe.ac.uk", "xyz");
        smtp.UseDefaultCredentials = false;
        smtp.Credentials = SMTPUserInfo;
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR:</B><br>" + ex.ToString();
        }
    }
    */

    public static string ForgottenPmail(string mailTo, string stCont, string LinkUI, string CID, string BaseUrl, string stAdditional)
    //function to send the forgotten password to a forgetful user and 
    //to alert eppisupport that someone is trying to use the forgotten password feature
    {
        string emailID = "2"; // this is based on the values in the database
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);
        if (mailTo != EmailFrom)
        {
            msg.Bcc.Add(EmailFrom);
        }
        msg.From = new MailAddress(EmailFrom);

        msg.Subject = "EPPI-Reviewer: password reset";
        msg.IsBodyHtml = true;


        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
        if (idr.Read())
        {
            msg.Body = idr["EMAIL_MESSAGE"].ToString();
        }
        idr.Close();

        msg.Body = msg.Body.Replace("FullNameHere", stCont);
        
        string queStr = BaseUrl + "LinkCheck.aspx?LUID=" + LinkUI + "&CID=" + CID;
        msg.Body = msg.Body.Replace("linkURLhere", queStr);

        if (stAdditional != "")
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", stAdditional);
        }
        else
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", "");
        }


        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR:</B><br>" + ex.ToString();
        }
    }

    public static string ForgottenUsernameMail(string mailTo, string stCont, string UserName, string stAdditional)
    //function to send the forgotten password to a forgetful user and 
    //to alert eppisupport that someone is trying to use the forgotten password feature
    {
        string emailID = "6"; // this is based on the values in the database
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);
        if (mailTo != EmailFrom)
        {
            msg.Bcc.Add(EmailFrom);
        }
        msg.From = new MailAddress(EmailFrom);

        msg.Subject = "EPPI-Reviewer: username reminder";
        msg.IsBodyHtml = true;


        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
        if (idr.Read())
        {
            msg.Body = idr["EMAIL_MESSAGE"].ToString();
        }
        idr.Close();

        msg.Body = msg.Body.Replace("FullNameHere", stCont);
        msg.Body = msg.Body.Replace("UsernameHere", UserName);

        if (stAdditional != "")
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", stAdditional);
        }
        else
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", "");
        }


        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR:</B><br>" + ex.ToString();
        }
    }

    public static string InviteEmail(string mailTo, string inviteeName, string reviewName, string inviterName,
        string inviterEmail, string emailAccountMsg)
    //function to send an invitation email to a user who has been invited into a review 
    {
        string emailID = "3"; // this is based on the values in the database
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);

        msg.From = new MailAddress(EmailFrom);

        msg.Subject = "EPPI-Reviewer: Review invitation";
        msg.IsBodyHtml = true;

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
        if (idr.Read())
        {
            msg.Body = idr["EMAIL_MESSAGE"].ToString();
        }
        idr.Close();

        msg.Body = msg.Body.Replace("InviteeNameHere", inviteeName);
        msg.Body = msg.Body.Replace("InviterNameHere", inviterName);
        msg.Body = msg.Body.Replace("ReviewNameHere", reviewName);

        if (emailAccountMsg != "")
        {
            msg.Body = msg.Body.Replace("ProblemWithAccountMsgHere", emailAccountMsg);
        }
        else
        {
            msg.Body = msg.Body.Replace("ProblemWithAccountMsgHere", "");
        }


        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR IN SENDING THE EMAIL.</B>";
        }
    }

    public static string OutstandingFeeEmail(string mailTo, string contactName, string outstandingFee)
    //function to let someone know there is an outstanding fee for previous unpaid account and/or review extensions 
    {
        string emailID = "11"; // this is based on the values in the database
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);

        msg.From = new MailAddress("eppisupport@ucl.ac.uk");

        msg.Subject = "EPPI-Reviewer: Outstanding fee";
        msg.IsBodyHtml = true;

        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
        if (idr.Read())
        {
            msg.Body = idr["EMAIL_MESSAGE"].ToString();
        }
        idr.Close();

        msg.Body = msg.Body.Replace("ShowFeeHere", "&#163;" + outstandingFee + " (GBP)");
        msg.Body = msg.Body.Replace("FullNameHere", contactName);

        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR IN SENDING THE EMAIL.</B>";
        }
    }


    public static string VerifyAccountEmail(string mailTo, string newUser, string userName, string LinkUI, string CID, string BaseUrl, string stAdditional)
    {
        string emailID = "7"; // this is based on the values in the database
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);
        //if (mailTo != mailFrom)
        //{
        //    msg.Bcc.Add(mailFrom);
        //}
        msg.From = new MailAddress(EmailFrom);

        msg.Subject = "EPPI-Reviewer: Account Activation";
        msg.IsBodyHtml = true;


        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
        if (idr.Read())
        {
            msg.Body = idr["EMAIL_MESSAGE"].ToString();
        }
        idr.Close();

        msg.Body = msg.Body.Replace("FullNameHere", newUser);

        string queStr = BaseUrl + "LinkCheck.aspx?LUID=" + LinkUI + "&CID=" + CID;
        msg.Body = msg.Body.Replace("linkURLhere", queStr);

        if (stAdditional != "")
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", stAdditional);
        }
        else
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", "");
        }


        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR:</B><br>" + ex.ToString();
        }
    }

    public static string WelcomeEmail(string mailTo, string newUser, string userName, string exDate, string stAdditional)
    {
        string emailID = "1"; // this is based on the values in the database
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);
        //if (mailTo != mailFrom)
        //{
        //    msg.Bcc.Add(mailFrom);
        //}
        msg.From = new MailAddress(EmailFrom);

        msg.Subject = "Welcome to EPPI Reviewer";
        msg.IsBodyHtml = true;


        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
        if (idr.Read())
        {
            msg.Body = idr["EMAIL_MESSAGE"].ToString();
        }
        idr.Close();

        msg.Body = msg.Body.Replace("FullNameHere", newUser);
        msg.Body = msg.Body.Replace("UsernameHere", userName);
        msg.Body = msg.Body.Replace("ExpiryDateHere", exDate);

        if (stAdditional != "")
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", stAdditional);
        }
        else
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", "");
        }


        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR:</B><br>" + ex.ToString();
        }
    }

    public static string NewUserEmail(string mailTo, string subject, string message)
    {
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);

        msg.From = new MailAddress(EmailFrom);
        msg.CC.Add(new MailAddress(EmailFrom));
        //msg.Bcc.Add(new MailAddress(inviterEmail));

        msg.Subject = subject;
        msg.IsBodyHtml = true;
        msg.Body = message;

        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR IN SENDING THE EMAIL.</B>";
        }
    }
    
    public static string GhostUserActivationEmail(string mailTo, string Fullname, string PurcharserName, string LinkUI, string CID, string BaseUrl, string stAdditional)
    {
        string emailID = "4"; // this is based on the values in the database
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);

        //if (mailTo != mailFrom)
        //{
        //    msg.Bcc.Add(mailFrom);
        //}
        msg.From = new MailAddress(EmailFrom);

        msg.Subject = "EPPI-Reviewer: Account Activation (On behalf of: " + PurcharserName + ")";
        msg.IsBodyHtml = true;


        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
        if (idr.Read())
        {
            msg.Body = idr["EMAIL_MESSAGE"].ToString();
        }
        idr.Close();

        msg.Body = msg.Body.Replace("FullNameHere", Fullname);
        msg.Body = msg.Body.Replace("PurcharserNameHere", PurcharserName); 

        string queStr = BaseUrl + "LinkCheck.aspx?LUID=" + LinkUI + "&CID=" + CID;
        msg.Body = msg.Body.Replace("linkURLhere", queStr);

        if (stAdditional != "")
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", stAdditional);
        }
        else
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", "");
        }


        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR:</B><br>" + ex.ToString();
        }
    }
    public static string GhostCreditTransferEmail(string mailTo, string Fullname, string PurcharserName, string Months, string NewDate, string stAdditional)
    {
        string emailID = "8"; // this is based on the values in the database
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);

        //if (mailTo != mailFrom)
        //{
        //    msg.Bcc.Add(mailFrom);
        //}
        msg.From = new MailAddress(EmailFrom);

        msg.Subject = "EPPI-Reviewer: Account Extension (On behalf of: " + PurcharserName + ")";
        msg.IsBodyHtml = true;


        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
        if (idr.Read())
        {
            msg.Body = idr["EMAIL_MESSAGE"].ToString();
        }
        idr.Close();

        msg.Body = msg.Body.Replace("FullNameHere", Fullname);
        msg.Body = msg.Body.Replace("PurcharserNameHere", PurcharserName);
        msg.Body = msg.Body.Replace("MonthsCreditHere", Months);

        if (NewDate == "")
        {//we are adding credit to an account that is still not active, can't tell the expiry date
            msg.Body = msg.Body.Replace(", as a result your account is now valid until <b>NewDateHere</b> (dd/mm/yyyy)", "");
        }
        else
        {
            msg.Body = msg.Body.Replace("NewDateHere", NewDate);
        }
        

        if (stAdditional != "")
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", stAdditional);
        }
        else
        {
            msg.Body = msg.Body.Replace("AdminMessageHere", "");
        }


        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR:</B><br>" + ex.ToString();
        }
    }
    /*
    public static string ReviewCreationErrorEmailOld(string mailTo, string newUser, string errorMessage,
        string passWord)
    {
        string mailFrom = "EPPIsupport@ioe.ac.uk";
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);

        msg.From = new MailAddress(mailFrom);
        //msg.Bcc.Add(new MailAddress(inviterEmail));

        msg.Subject = "EPPI-Reviewer: Error - Example review creation failure";
        msg.IsBodyHtml = true;


        msg.Body = "EPPI-Reviewer: Software for research synthesis";
        msg.Body += "<br><br>";

        msg.Body += "An example review being created for ContactID " + newUser + "  has generated an error message<br>";
        msg.Body += "The error message is: " + errorMessage;
        msg.Body += "<br> (Note: This message has only been sent to EPPISupport)";

        msg.Body += "<br><br>";
        msg.Body += "EPPI-Reviewer 4 is developed and maintained by the EPPI-Centre of the Institute of Education at the University of London, UK. ";
        msg.Body += "To find out more about the work of the EPPI-Centre please visit our website <a href='http://eppi.ioe.ac.uk/'>eppi.ioe.ac.uk</a>.";
        msg.Body += "<br>";
        msg.Body += "<br><br>In case this message is unexpected, please don't hesitate to contact our support staff:<br>";
        msg.Body += "<a href='mailto:EPPIsupport@ioe.ac.uk'>EPPIsupport@ioe.ac.uk</a>";
        msg.Body += "<br><br>";

        SmtpClient smtp = new SmtpClient(SMTP);
        System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential("EPPIsupport@ioe.ac.uk", "xyz");
        smtp.UseDefaultCredentials = false;
        smtp.Credentials = SMTPUserInfo;
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR IN SENDING THE EMAIL.</B>";
        }
    }
    */
    public static string ReviewCreationErrorEmail(string mailTo, string contactID, string errorMessage)
    {
        string emailID = "5"; // this is based on the values in the database
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);

        msg.From = new MailAddress(EmailFrom);
        //msg.Bcc.Add(new MailAddress(inviterEmail));

        msg.Subject = "EPPI-Reviewer: Error - Example review creation failure";
        msg.IsBodyHtml = true;


        bool isAdmDB = true;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_EmailGet", emailID);
        if (idr.Read())
        {
            msg.Body = idr["EMAIL_MESSAGE"].ToString();
        }
        idr.Close();

        msg.Body = msg.Body.Replace("ContactIDHere", contactID);
        msg.Body = msg.Body.Replace("ErrorMessageHere", errorMessage);

        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR IN SENDING THE EMAIL.</B>";
        }
    }


    public static string EmailNewsletter(string mailTo, string subject, string message)
    {
        MailMessage msg = new MailMessage();
        msg.To.Add(mailTo);

        msg.From = new MailAddress(EmailFrom);
        //msg.CC.Add(new MailAddress("EPPIsupport@ioe.ac.uk"));
        //msg.Bcc.Add(new MailAddress(inviterEmail));

        msg.Subject = subject;
        msg.IsBodyHtml = true;
        msg.Body = message;

        SmtpClient smtp = smtpClient();
        try
        {
            smtp.Send(msg);
            return "The email was sent successfully";
        }
        catch (Exception ex)
        {
            //NOTE: this is useful info when using the function from an admin account, 
            //will allow the admin to know why the email could not be sent.
            //OTOH is VERY DANGEROUS to include this in the forgotten password page: it's acessible to everyone,
            //hence other code will be included in that page to handle this situation
            return "<B>THERE WAS AN ERROR IN SENDING THE EMAIL.</B>";
        }
    }
    public static string CreateLink(int CID, string Reason, string ccEmail, HttpServerUtility Server)
    {
        SqlParameter[] paramListMakeLink = new SqlParameter[4];
        paramListMakeLink[0] = new SqlParameter("@TYPE", Reason);
        paramListMakeLink[1] = new SqlParameter("@CID", CID);
        paramListMakeLink[2] = new SqlParameter("@CC_EMAIL", ccEmail == "" ? null : ccEmail);
        paramListMakeLink[3] = new SqlParameter("@UID", SqlDbType.UniqueIdentifier);
        paramListMakeLink[3].Direction = ParameterDirection.Output;
        Utils.ExecuteSPWithReturnValues(true, Server, "st_CheckLinkCreate", paramListMakeLink);
        return paramListMakeLink[3].Value.ToString();
    }
    public static string FixURLForHTTPS(string BaseUrl)
    {
        string name = Environment.MachineName;
        if (name.ToLower() == "epi2" && BaseUrl.ToLower().IndexOf("https://") < 0)
        {//system is live, use https!
            BaseUrl = BaseUrl.ToLower().Replace("http://", "https://");
        }
        return BaseUrl;
    }
    public static string buildExampleReview(int contactID, HttpServerUtility Server)
    {
        string exampleReviewResult = "Success";
        bool isAdmDB = true;
        int sourceReviewID = 0;
        IDataReader idr = Utils.GetReader(isAdmDB, "st_ManagementSettings");
        if (idr.Read()) // it exists
        {
            sourceReviewID = int.Parse(idr["EXAMPLE_NON_SHAREABLE_REVIEW_ID"].ToString());
        }
        idr.Close();
        string CIDstr = contactID.ToString();
        if (CIDstr == null || CIDstr == "")
        {
            return "There was a problem in copying the example review to your account. System didn't recognise you.";
        }
        // Step 01: create new review, populate TB_REVIEW, TB_REVIEW_CONTACT, TB_CONTACT_REVIEW_ROLE 
        //  and return new REVIEW_ID
        int destinationReviewID = 0;
        string errorMessage = "0";

        SqlParameter[] paramList01 = new SqlParameter[3];
        paramList01[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
            true, 0, 0, null, DataRowVersion.Default, CIDstr);
        paramList01[1] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        paramList01[2] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
            true, 0, 0, null, DataRowVersion.Default, "");
        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep01", paramList01);
        if (paramList01[2].Value.ToString() != "0")
        {
            errorMessage = "Step 01 error - " + paramList01[2].Value.ToString();
        }
        else
        {
            destinationReviewID = int.Parse(paramList01[1].Value.ToString());
            // keep going          
            // Step 03: copy the items by populating TB_ITEM, TB_ITEM_REVIEW, TB_ITEM_AUTHOR, TB_ITEM_DOCUMENT, 
            //  TB_ITEM_REVIEW
            errorMessage = "0";

            SqlParameter[] paramList03 = new SqlParameter[4];
            paramList03[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, CIDstr);
            paramList03[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, sourceReviewID);
            paramList03[2] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, destinationReviewID);
            paramList03[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep03", paramList03);
            if (paramList03[3].Value.ToString() != "0")
            {
                errorMessage = "Step 03 error - " + paramList03[3].Value.ToString();
            }
            else
            {
                // keep going
                // Step 05: copy the duplicate data from TB_ITEM_DUPLICATE_GROUP, TB_ITEM_DUPLICATE_GROUP_MEMBERS
                errorMessage = "0";
                SqlParameter[] paramList05 = new SqlParameter[4];
                paramList05[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, CIDstr);
                paramList05[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, sourceReviewID);
                paramList05[2] = new SqlParameter("@NEW_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                    true, 0, 0, null, DataRowVersion.Default, destinationReviewID);
                paramList05[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                    true, 0, 0, null, DataRowVersion.Default, "");
                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep05", paramList05);
                if (paramList05[3].Value.ToString() != "0")
                {
                    errorMessage = "Step 05 error - " + paramList05[3].Value.ToString();
                }
                else
                {
                    // keep going
                    // Step 07: gather up the set_id's in the the example review 
                    errorMessage = "0";
                    DataTable dtSets7 = new DataTable();
                    System.Data.DataRow newrow7;
                    dtSets7.Columns.Add(new DataColumn("SOURCE_SET_ID", typeof(string)));
                    idr = Utils.GetReader(isAdmDB, "st_CopyReviewStep07", sourceReviewID);
                    while (idr.Read())
                    {
                        newrow7 = dtSets7.NewRow();
                        newrow7["SOURCE_SET_ID"] = idr["SET_ID"].ToString();
                        dtSets7.Rows.Add(newrow7);
                    }
                    idr.Close();


                    // Step 09: copy the code sets one at a time (to avoid timeouts)
                    for (int i = 0; i < dtSets7.Rows.Count; i++)
                    {
                        string test = dtSets7.Rows[i]["SOURCE_SET_ID"].ToString();
                        SqlParameter[] paramList09 = new SqlParameter[5];
                        paramList09[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, CIDstr);
                        paramList09[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, sourceReviewID);
                        paramList09[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, destinationReviewID);
                        paramList09[3] = new SqlParameter("@SOURCE_SET_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                            true, 0, 0, null, DataRowVersion.Default, dtSets7.Rows[i]["SOURCE_SET_ID"].ToString());
                        paramList09[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                            true, 0, 0, null, DataRowVersion.Default, "");
                        Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep09", paramList09);
                        if (paramList09[4].Value.ToString() != "0")
                        {
                            errorMessage = "Step 09 error - " + paramList09[4].Value.ToString();
                            i = dtSets7.Rows.Count; // stop the loop
                        }
                    }
                    if (errorMessage != "0")
                    {
                        // go no further
                    }
                    else
                    {
                        // keep going
                        // Step 11: copy the data for each set
                        errorMessage = "0";
                        for (int i = 0; i < dtSets7.Rows.Count; i++)
                        {
                            string test = dtSets7.Rows[i]["SOURCE_SET_ID"].ToString();
                            SqlParameter[] paramList11 = new SqlParameter[5];
                            paramList11[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, CIDstr);
                            paramList11[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, sourceReviewID);
                            paramList11[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, destinationReviewID);
                            paramList11[3] = new SqlParameter("@SOURCE_SET_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, dtSets7.Rows[i]["SOURCE_SET_ID"].ToString());
                            paramList11[4] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                                true, 0, 0, null, DataRowVersion.Default, "");
                            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep11", paramList11);
                            if (paramList11[4].Value.ToString() != "0")
                            {
                                errorMessage = "Step 11 error - " + paramList11[4].Value.ToString();
                                i = dtSets7.Rows.Count; // stop the loop
                            }
                        }
                        if (errorMessage != "0")
                        {
                            // go no further
                        }
                        else
                        {
                            // keep going
                            // Step 13: work assignments, diagrams, searches
                            errorMessage = "0";
                            SqlParameter[] paramList13 = new SqlParameter[4];
                            paramList13[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, CIDstr);
                            paramList13[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, sourceReviewID);
                            paramList13[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                true, 0, 0, null, DataRowVersion.Default, destinationReviewID);
                            paramList13[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                                true, 0, 0, null, DataRowVersion.Default, "");
                            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep13", paramList13);
                            if (paramList13[3].Value.ToString() != "0")
                            {
                                errorMessage = "Step 13 error - " + paramList13[3].Value.ToString();
                            }
                            else
                            {
                                // keep going
                                // Step 15: reports, meta-analysis
                                errorMessage = "0";
                                SqlParameter[] paramList15 = new SqlParameter[4];
                                paramList15[0] = new SqlParameter("@CONTACT_ID", SqlDbType.Int, 8, ParameterDirection.Input,
                                    true, 0, 0, null, DataRowVersion.Default, CIDstr);
                                paramList15[1] = new SqlParameter("@SOURCE_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                    true, 0, 0, null, DataRowVersion.Default, sourceReviewID);
                                paramList15[2] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                    true, 0, 0, null, DataRowVersion.Default, destinationReviewID);
                                paramList15[3] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                                    true, 0, 0, null, DataRowVersion.Default, "");
                                Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStep15", paramList15);
                                if (paramList15[3].Value.ToString() != "0")
                                {
                                    errorMessage = "Step 15 error - " + paramList15[3].Value.ToString();
                                }
                                else
                                {
                                    // keep going
                                    // clean up the data (i.e. remove OLD_ATTRIBUTE_ID, OLD_GUIDELINE_ID, OLD_ITEM_ID)
                                    errorMessage = "0";

                                    SqlParameter[] paramListCleanup = new SqlParameter[2];
                                    paramListCleanup[0] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                                        true, 0, 0, null, DataRowVersion.Default, destinationReviewID);
                                    paramListCleanup[1] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                                        true, 0, 0, null, DataRowVersion.Default, "");
                                    Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStepCleanup", paramListCleanup);
                                    if (paramListCleanup[1].Value.ToString() != "0")
                                    {
                                        errorMessage = "Step cleanup error - " + paramListCleanup[1].Value.ToString();
                                    }
                                    //else
                                    //{
                                    //    errorMessage = "An email with your password has been sent to your email address and an example " +
                                    //        "review has been created under your name. If you do not receive the email please contact us at EPPIsupport@ioe.ac.uk";
                                    //}
                                }
                            }
                        }
                    }
                }
            }
        }
        if (errorMessage != "0")
        {
            // there has been a problem so run the cleanup
            SqlParameter[] paramListCleanup = new SqlParameter[2];
            paramListCleanup[0] = new SqlParameter("@DESTINATION_REVIEW_ID", SqlDbType.BigInt, 8, ParameterDirection.Input,
                true, 0, 0, null, DataRowVersion.Default, destinationReviewID);
            paramListCleanup[1] = new SqlParameter("@RESULT", SqlDbType.NVarChar, 50, ParameterDirection.Output,
                true, 0, 0, null, DataRowVersion.Default, "");
            Utils.ExecuteSPWithReturnValues(isAdmDB, Server, "st_CopyReviewStepCleanup", paramListCleanup);

            // send an email to EPPI-Support saying there is a problem with the review creation
            Utils.ReviewCreationErrorEmail(EmailFrom, contactID.ToString(), errorMessage);
            exampleReviewResult = "There was a problem in copying the example review to your account. EPPISupport has been notified.";

        }
        return exampleReviewResult;
    }
}
