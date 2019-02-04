using Csla;
using System;
using Csla.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ClassifierCreateCodesCommand : CommandBase<ClassifierCreateCodesCommand>
    {

        public ClassifierCreateCodesCommand(int searchID, string searchName, Int64 destAttributeID, int destSetID)
        {
            _SearchID = searchID;
            _ParentAttributeID = destAttributeID;
            _SetID = destSetID;
            _SearchName = searchName;
        }
        public ClassifierCreateCodesCommand() { }
        
        private int _SearchID;
        private Int64 _ParentAttributeID;
        private int _SetID;
        private string _SearchName;

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_SearchID", _SearchID);
            info.AddValue("_ParentAttributeID", _ParentAttributeID);
            info.AddValue("_SetID", _SetID);
            info.AddValue("_SearchName", _SearchName);
            
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _SearchID = info.GetValue<int>("_SearchID");
            _ParentAttributeID = info.GetValue<Int64>("_ParentAttributeID");
            _SetID = info.GetValue<int>("_SetID");
            _SearchName = info.GetValue<string>("_SearchName");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();

                using (SqlCommand command = new SqlCommand("st_ItemAttributeBulkAssignCodesFromMLsearchResults", connection))
                {

                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SearchID", _SearchID));
                    command.Parameters.Add(new SqlParameter("@revID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ParentAttributeID", _ParentAttributeID));
                    command.Parameters.Add(new SqlParameter("@SetID", _SetID));
                    command.Parameters.Add(new SqlParameter("@SearchName", _SearchName));
                    command.Parameters.Add(new SqlParameter("@ContactID", ri.UserId));
                    command.CommandTimeout = 60; //can take time for large batches!
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif
    }
}
