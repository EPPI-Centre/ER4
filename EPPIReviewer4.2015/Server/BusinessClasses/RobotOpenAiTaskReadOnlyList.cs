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
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_RobotApiTopQueuedJobs", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ROBOT_NAME", "OpenAI GPT4"));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        Child_Fetch(reader, ri);
                    }
                }
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
        private void Child_Fetch(SafeDataReader reader, ReviewerIdentity ri)
        {
            int rid = ri.ReviewId;
            int cid = ri.UserId;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            while (reader.Read())
            {
                Add(DataPortal.FetchChild<RobotOpenAiTaskReadOnly>(reader, true, rid, cid));
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
#endif
    }
}
