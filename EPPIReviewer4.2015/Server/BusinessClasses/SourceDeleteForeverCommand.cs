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
using Csla.DataPortalClient;
using System.Threading;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class SourceDeleteForeverCommand : CommandBase<SourceDeleteForeverCommand>
    {
    public SourceDeleteForeverCommand(){}

        public SourceDeleteForeverCommand(int SourceId)
        {
            _SourceId = SourceId;
        }
        private int _SourceId;
        public int SourceId
        {
            get { return _SourceId; }
        }

        private string _Result;
        public string Result
        {
            get { return _Result; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_SourceId", _SourceId);
            info.AddValue("_Result", _Result);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _SourceId = info.GetValue<int>("_SourceId");
            _Result = info.GetValue<string>("_Result");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int Rid = ri.ReviewId;
            System.Threading.Tasks.Task.Run(() => FireAndForgetExcecuteCommand(Rid, ri.UserId));//fire and forget. We don't wait to see what happens.
            //st_SourceDeleteForever will resume any previously interrupted source deletion that didn't finish, for whatever reason (most likely: timeout or app pool refresh).

            if (SourceId > 0)
            {
                //we triggered this to actually delete a source (other option is to make sure an interrupted source deletion gets a chance to resume, if necessary)
                //so we'll wait a bit to see if the deletion ends in 30s
                for (int count = 0; count < 3; count++)
                {
                    System.Threading.Thread.Sleep(10 * 1000);//wait 10s
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_SourceDeleteForeverIsRunning", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@revID", Rid));
                            command.Parameters.Add(new SqlParameter("@result", System.Data.SqlDbType.Int));
                            command.Parameters["@result"].Direction = System.Data.ParameterDirection.Output;
                            command.ExecuteNonQuery();
                            int? res = command.Parameters["@result"].Value as int?;

                            if (res != null && res != 0)
                            {
                                _Result = "Deletion running for SourceId: " + res.ToString();
                            }
                            else
                            {//must have finished already!
                                _Result = "No deletion is running";
                                count = 100;//end the loop!
                            }
                        }
                        connection.Close();
                    }
                }
            }
            else _Result = "Task fired and forgotten - not checking if a deletion is running";
        }
        private void FireAndForgetExcecuteCommand(int revID, int ContactId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SourceDeleteForever", connection))
                {
                    //st_SourceDeleteForever can take a long time. Does deletion in batches of 400 items, stopping 4s between batches.
                    //this is because all deletions include multiple tables and are wrapped in a transaction (no partial deletions are possible)
                    //consequence is that SP "locks" lots of tables, thus, between batches we stop to let other queries execute.
                    //Deleting rows from TB_ITEM is slow because of all "CASCADE on delete" foreign keys on ITEM_ID.
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@srcID", _SourceId));
                    command.Parameters.Add(new SqlParameter("@revID", revID));
                    command.Parameters.Add(new SqlParameter("@contactID", ContactId));
                    command.Parameters.Add(new SqlParameter("@result", System.Data.SqlDbType.Int));
                    command.Parameters["@result"].Direction = System.Data.ParameterDirection.Output;
                    command.CommandTimeout = 600;//ten minutes
                    command.ExecuteNonQuery();
                    //for debugging: get the result value. We don't do anything with it, but it's useful to check if it's what we'd expect...
                    //possible values are:
                    //0: SP was triggered _without_ a SourceId(null or zero) and no Job was running for this review.
                    //1: SP was triggered with a SourceId and the execution ended correctly.
                    //-1: Check on currently running JOB found a job active and still running(last activity was less than 10m ago).
                    //-2: SP was triggered with a SourceId, but the check on currently running JOB found a job that needed resuming, so SP resumed the older job and ignored the SourceId supplied.
                    //-10: (should be impossible to trigger) SP reached the "active" deletion portion, but doesn't have a SourceId to work on (either supplied or retrieved from `TB_REVIEW_JOB`) so can't continue and is stopping instead.
                    //-11: we have a SourceId to act on, but it either doesn't belong to the review or it isn't already marked as deleted.
                    //-12: an exception occurred(either in the batches or the final deletions).Error should appear in the "job message" field in `TB_REVIEW_JOB`.

                    //Debug: put a breakpoint here to "see" the result value
                    int? result = command.Parameters["@result"].Value as int?;
                }
                connection.Close();
            }
        }
#endif
    }
}
