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
    public class RobotOpenAiTaskReadOnlyList : ReadOnlyListBase<RobotOpenAiTaskReadOnlyList, RobotOpenAiTaskReadOnly>
    {
        public RobotOpenAiTaskReadOnlyList() 
        {
        }
        

#if !SILVERLIGHT

        private void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            int rid = ri.ReviewId;
            int cid = ri.UserId;
            bool IsSiteAdmin = ri.IsSiteAdmin;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_RobotApiTopQueuedJobs", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ROBOT_NAME", "OpenAI GPT4"));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        Child_Fetch(reader, rid, cid, IsSiteAdmin);
                    }
                }
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
        private void DataPortal_Fetch(RobotOpenAiTaskCriteria crit)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            int rid = ri.ReviewId;
            int cid = ri.UserId;
            bool IsSiteAdmin = ri.IsSiteAdmin;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_RobotApiPastJobs", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ROBOT_NAME", "OpenAI GPT4"));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", rid));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", cid));
                    if (ri.IsSiteAdmin)
                    {
                        command.Parameters.Add(new SqlParameter("@isSiteAdmin", true));
                    }
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        Child_Fetch(reader, rid, cid, IsSiteAdmin);
                        reader.NextResult();
                        while (reader.Read())
                        {
                            int CallId = reader.GetInt32("ROBOT_API_CALL_ID");
                            RobotOpenAiTaskReadOnly target = this.FirstOrDefault(f => f.RobotApiCallId == CallId);
                            if (target != null)
                            {
                                target.Errors.Add(RobotOpenAiTaskError.Child_FetchError(reader));
                            }
                        }
                    }
                }
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
        private void Child_Fetch(SafeDataReader reader, int rid, int cid, bool IsSiteAdmin)
        {
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            while (reader.Read())
            {
                Add(DataPortal.FetchChild<RobotOpenAiTaskReadOnly>(reader, !IsSiteAdmin, rid, cid));
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
#endif
    }
}
