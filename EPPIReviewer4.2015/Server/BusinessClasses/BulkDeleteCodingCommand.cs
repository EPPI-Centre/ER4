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
    public class BulkDeleteCodingCommand : CommandBase<BulkDeleteCodingCommand>
    {

        public BulkDeleteCodingCommand() { }
        public BulkDeleteCodingCommand(int setId, int reviewerId, bool isPreview)
        {
            _SetId = setId;
            _ReviewerId = reviewerId;
            _IsPreview = isPreview;
        }

        private bool _IsPreview;
        public bool IsPreview
        {
            get { return _IsPreview; }
        }
        private int _SetId;
        public int SetId
        {
            get { return _SetId; }
        }
        private int _ReviewerId;
        public int ReviewerId
        {
            get { return _ReviewerId; }
        }
        private int _TotalItemsAffected = 0;
        public int TotalItemsAffected
        {
            get { return _TotalItemsAffected; }
        }
        private int _CompletedCodingToBeDeleted = 0;
        public int CompletedCodingToBeDeleted
        {
            get { return _CompletedCodingToBeDeleted; }
        }
        private int _IncompletedCodingToBeDeleted = 0;
        public int IncompletedCodingToBeDeleted
        {
            get { return _IncompletedCodingToBeDeleted; }
        }
        private int _HiddenIncompletedCodingToBeDeleted = 0;
        public int HiddenIncompletedCodingToBeDeleted
        {
            get { return _HiddenIncompletedCodingToBeDeleted; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_SetId", _SetId);
            info.AddValue("_ReviewerId", _ReviewerId);
            info.AddValue("_TotalItemsAffected", _TotalItemsAffected);
            info.AddValue("_IncompletedCodingToBeDeleted", _IncompletedCodingToBeDeleted);
            info.AddValue("_CompletedCodingToBeDeleted", _CompletedCodingToBeDeleted);
            info.AddValue("_IsPreview", _IsPreview);
            info.AddValue("_HiddenIncompletedCodingToBeDeleted", _HiddenIncompletedCodingToBeDeleted); 
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _SetId = info.GetValue<int>("_SetId");
            _ReviewerId = info.GetValue<int>("_ReviewerId");
            _TotalItemsAffected = info.GetValue<int>("_TotalItemsAffected"); 
            _CompletedCodingToBeDeleted = info.GetValue<int>("_CompletedCodingToBeDeleted");
            _IncompletedCodingToBeDeleted = info.GetValue<int>("_IncompletedCodingToBeDeleted");
            _HiddenIncompletedCodingToBeDeleted = info.GetValue<int>("_HiddenIncompletedCodingToBeDeleted"); 
            _IsPreview = info.GetValue<bool>("_IsPreview");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                //Thread.Sleep(2000);
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemSetBulkDelete", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    
                    command.CommandTimeout = 120;
                    command.Parameters.Add(new SqlParameter("@SET_ID", SetId));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@DELETE_CODING_OF_CONTACT_ID", ReviewerId));
                    if (IsPreview)
                    {
                        command.CommandText = "st_ItemSetBulkDeletePreview";
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@Affected", System.Data.SqlDbType.Int));
                        command.Parameters["@Affected"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                    }
                    if (IsPreview)
                    {
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                _TotalItemsAffected = reader.GetInt32("TotItems");
                                _CompletedCodingToBeDeleted = reader.GetInt32("Completed");
                                _HiddenIncompletedCodingToBeDeleted = reader.GetInt32("AdditionalIncomplete");
                                _IncompletedCodingToBeDeleted = TotalItemsAffected - CompletedCodingToBeDeleted - HiddenIncompletedCodingToBeDeleted;
                            }
                        }
                    }
                    else
                    {
                        _TotalItemsAffected = (int)command.Parameters["@Affected"].Value;
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}
