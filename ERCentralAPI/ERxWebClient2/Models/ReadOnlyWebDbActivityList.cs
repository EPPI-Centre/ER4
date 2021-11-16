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

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyWebDbActivityList : ReadOnlyListBase<ReadOnlyWebDbActivityList, ReadOnlyWebDbActivity>
    {

        // st_Eppi_Vis_Get_Log
        // Input: @WEBDB_ID int, @FROM datetime, @UNTIL datetime, @TYPE nvarchar(255)
        // Output: tv_webdb_log_identity int, tv_created datetime, tv_log_type nvarchar(25), tv_details nvarchar(max)

        public ReadOnlyWebDbActivityList() { }





#if !SILVERLIGHT

        private void DataPortal_Fetch(ReadOnlyWebDbActivityListSelectionCrit criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            int currentRid = ri.ReviewId;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_Eppi_Vis_Get_Log", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", criteria.WebDBID));
                    command.Parameters.Add(new SqlParameter("@FROM", criteria.From));
                    command.Parameters.Add(new SqlParameter("@UNTIL", criteria.Until));
                    command.Parameters.Add(new SqlParameter("@TYPE", criteria.LogType));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlyWebDbActivity.GetReadOnlyWebDBActivity(reader));
                        }
                    }
                }
                connection.Close();
            }

        }

#endif
    }

    [Serializable]
    public class ReadOnlyWebDbActivityListSelectionCrit : Csla.CriteriaBase<ReadOnlyWebDbActivityListSelectionCrit>
    {
        private static PropertyInfo<int> WebDBIDProperty = RegisterProperty<int>(typeof(ReadOnlyWebDbActivityListSelectionCrit), new PropertyInfo<int>("WebDBID", "WebDBID"));
        public int WebDBID
        {
            get { return ReadProperty(WebDBIDProperty); }
        }
        private static PropertyInfo<DateTime> FromProperty = RegisterProperty<DateTime>(typeof(ReadOnlyWebDbActivityListSelectionCrit), new PropertyInfo<DateTime>("From", "From"));
        public DateTime From
        {
            get { return ReadProperty(FromProperty); }
        }
        private static PropertyInfo<DateTime> UntilProperty = RegisterProperty<DateTime>(typeof(ReadOnlyWebDbActivityListSelectionCrit), new PropertyInfo<DateTime>("Until", "Until"));
        public DateTime Until
        {
            get { return ReadProperty(UntilProperty); }
        }
        private static PropertyInfo<string> LogTypeProperty = RegisterProperty<string>(typeof(ReadOnlyWebDbActivityListSelectionCrit), new PropertyInfo<string>("LogType", "LogType"));
        public string LogType
        {
            get { return ReadProperty(LogTypeProperty); }
        }

        public ReadOnlyWebDbActivityListSelectionCrit(int webDBID, DateTime from, DateTime until, string logType)
        {
            LoadProperty(WebDBIDProperty, webDBID);
            LoadProperty(FromProperty, from);
            LoadProperty(UntilProperty, until);
            LoadProperty(LogTypeProperty, logType);
        }

        public ReadOnlyWebDbActivityListSelectionCrit() { }
    }

}
