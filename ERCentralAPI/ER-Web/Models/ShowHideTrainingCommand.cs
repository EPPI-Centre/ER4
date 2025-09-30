using Csla;
using BusinessLibrary.Security;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ShowHideTrainingCommand : CommandBase<ShowHideTrainingCommand>
    {
        public ShowHideTrainingCommand() { }

        private string _RecordsToHide = "";
        private string _RecordsToShow = "";

        public ShowHideTrainingCommand(string recordsToHide, string recordsToShow)
        {
            _RecordsToHide = recordsToHide;
            _RecordsToShow = recordsToShow;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_RecordsToHide", _RecordsToHide);
            info.AddValue("_RecordsToShow", _RecordsToShow);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _RecordsToHide = info.GetValue<string>("_RecordsToHide");
            _RecordsToShow = info.GetValue<string>("_RecordsToShow");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingShowHideRecords", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@RecordsToHide", _RecordsToHide));
                    command.Parameters.Add(new SqlParameter("@RecordsToShow", _RecordsToShow));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
