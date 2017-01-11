using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using System.Text.RegularExpressions;


#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Net;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ImportURLsCommand : CommandBase<ImportURLsCommand>
    {
#if SILVERLIGHT
        
        public ImportURLsCommand() 
        {
            URLsDic = new MobileDictionary<long, string>();
        }
#else
        protected ImportURLsCommand()
        {
            
        }
#endif
        public bool AddLine(string Line)
        {
            string[] splitted = Line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Int64 ItemID;
            if (splitted == null || splitted.Length < 2) return false;
            if (!Int64.TryParse(splitted[0], out ItemID)) return false;
            if (ItemID < 1) return false;
            try
            {
                URLsDic.Add(ItemID, splitted[1]);
            }
            catch { return false; }
            return true;
        }
        public int Count
        {
            get
            {
                if (URLsDic == null) return -1; //shouldn't happen!
                else return URLsDic.Count;
            }
        }
        public override string ToString()
        {
            string result = "";
            if (URLsDic != null)
            {
                foreach (KeyValuePair<Int64, string> el in URLsDic)
                {
                    result += el.Key.ToString() + " " + el.Value + Environment.NewLine;
                }
                result = result.Trim();
            }
            return result;
        }
        private static PropertyInfo<MobileDictionary<Int64, string>> URLsDicProperty = RegisterProperty<MobileDictionary<Int64, string>>(new PropertyInfo<MobileDictionary<Int64, string>>("URLsDic", "URLsDic"));
        private MobileDictionary<Int64, string> URLsDic
        {
            get { return ReadProperty(URLsDicProperty); }
            set { LoadProperty(URLsDicProperty, value); }
        }
        public static PropertyInfo<string> ResultProperty = RegisterProperty<string>(new PropertyInfo<string>("Result", "Result"));
        public string Result
        {
            get { return ReadProperty(ResultProperty); }
        }
#if !SILVERLIGHT
        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int rid = ri.ReviewId;
            MobileDictionary<Int64, string> failed = new MobileDictionary<long, string>();//used to report about failures
            LoadProperty(ResultProperty, "Success");//will change to report at which point it failed if an exception is raised
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                foreach (KeyValuePair<Int64, string> el in URLsDic)
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand("st_ItemURLSet", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@Rid", rid));
                            command.Parameters.Add(new SqlParameter("@ItemID", el.Key));
                            command.Parameters.Add(new SqlParameter("@Contact", ri.Name));
                            command.Parameters.Add(new SqlParameter("@URL", el.Value));
                            command.Parameters.Add(new SqlParameter("@Result", System.Data.SqlDbType.Int));
                            command.Parameters[4].Value = -1;
                            command.Parameters[4].Direction = System.Data.ParameterDirection.Output;
                            command.ExecuteNonQuery();
                            if ((int)command.Parameters[4].Value < 1)
                            {//-1 for all failures
                                failed.Add(el.Key, el.Value);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (Result.IndexOf("Error") == -1) LoadProperty(ResultProperty, "Error for item(s): ");
                        if (Result != "Error for item(s): ") LoadProperty(ResultProperty, Result + ", " + el.Key.ToString());
                        else LoadProperty(ResultProperty, Result + el.Key.ToString());
                        //if (!failed.ContainsKey(el.Key)) failed.Add(el.Key, el.Value);
                    }
                }
                
            }
            if (failed.Count > 0)
            {//update didn't work for all items
                URLsDic = failed;
            }
            else
            {// if all is well, command comes back empty!
                URLsDic.Clear();
            }
        }
#endif
    }
}
