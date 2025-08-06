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
using static ERxWebClient2.Controllers.ClassifierController;
using ERxWebClient2.Controllers;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ScreeningFromSearchCommand : CommandBase<ScreeningFromSearchCommand>
    {

        public ScreeningFromSearchCommand() { }


        private int _SearchId;
        private int _CodeSetId;
        private Int64 _TriggeringItemId;
        private bool _CreateNew = false;
        private string _Result = "";


        public ScreeningFromSearchCommand(int searchId, int codesetId, Int64 itemId, bool createNew)
        {
           _SearchId = searchId;
            _CodeSetId = codesetId;
            _TriggeringItemId = itemId;
            _CreateNew = createNew;
        }
        public string Result
        {
            get { return _Result; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_SearchId", _SearchId);
            info.AddValue("_CodeSetId", _CodeSetId);
            info.AddValue("_TriggeringItemId", _TriggeringItemId);
            info.AddValue("_CreateNew", _CreateNew);
            info.AddValue("_Result", _Result);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _SearchId = info.GetValue<int>("_SearchId");
            _CodeSetId = info.GetValue<int>("_CodeSetId");
            _TriggeringItemId = info.GetValue<Int64>("_TriggeringItemId");
            _CreateNew = info.GetValue<bool>("_CreateNew");
            _Result = info.GetValue<string>("_Result");
        }

#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                if (_CreateNew)
                {
                    using (SqlCommand command = new SqlCommand("st_ScreeningCreate_List_FromSearch", connection))
                    {
                        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command.Parameters.Add(new SqlParameter("@SEARCH_ID", _SearchId));
                        command.Parameters.Add(new SqlParameter("@CODE_SET_ID", _CodeSetId));
                        command.Parameters.Add(new SqlParameter("@NEW_TRAINING_FS_ID", System.Data.SqlDbType.Int));
                        command.Parameters["@NEW_TRAINING_FS_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();

                        int NewId = Convert.ToInt32(command.Parameters["@NEW_TRAINING_FS_ID"].Value);
                        if (NewId == -1)
                        {
                            _Result = "Search not found, or not suitable";
                        }
                        else
                        {
                            _Result = "Done";
                        }
                    }
                }
                else
                {
                    using (SqlCommand command = new SqlCommand("st_TrainingFromSearchRenewCounts", connection))
                    {
                        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command.Parameters.Add(new SqlParameter("@CODE_SET_ID", _CodeSetId));
                        command.Parameters.Add(new SqlParameter("@TRIGGERING_ITEM_ID", _TriggeringItemId));
                        command.Parameters.Add(new SqlParameter("@NEW_TRAINING_FS_ID", System.Data.SqlDbType.Int));
                        command.Parameters["@NEW_TRAINING_FS_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        int NewId = Convert.ToInt32(command.Parameters["@NEW_TRAINING_FS_ID"].Value);
                        if (NewId == -1)
                        {
                            _Result = "Failed, mismatched data";
                        }
                        else if (NewId == -2)
                        {
                            _Result = "Unnecessary";
                        }
                        else
                        {
                            _Result = "Done";
                        }
                    }

                }
                connection.Close();
            }
        }

#endif
    }
}
