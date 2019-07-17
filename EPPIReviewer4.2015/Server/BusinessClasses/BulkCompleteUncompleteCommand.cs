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
    public class BulkCompleteUncompleteCommand : CommandBase<BulkCompleteUncompleteCommand>
    {
#if SILVERLIGHT
        public BulkCompleteUncompleteCommand(){}
#else
        public BulkCompleteUncompleteCommand() { }
#endif

        private Int64 _AttributeId;
        public Int64 AttributeId
        {
            get { return _AttributeId; }
        }

        private bool _IsCompleting;
        public bool IsCompleting
        {
            get { return _IsCompleting; }
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
        private int _PotentiallyAffectedItems;
        public int PotentiallyAffectedItems
        {
            get { return _PotentiallyAffectedItems; }
        }
        private int _AffectedItems;
        public int AffectedItems
        {
            get { return _AffectedItems; }
        }


        public BulkCompleteUncompleteCommand(Int64 attributeId, bool isCompleting, int setId, bool isPreview)
        {
            _AttributeId = attributeId;
            _IsCompleting = isCompleting;
            _SetId = setId;
            _IsPreview = isPreview;
        }
        public BulkCompleteUncompleteCommand(Int64 attributeId, bool isCompleting, int setId, int reviewerId, bool isPreview)
        {
            _AttributeId = attributeId;
            _IsCompleting = isCompleting;
            _SetId = setId;
            _ReviewerId = reviewerId;
            _IsPreview = isPreview;
        }
        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_AttributeId", _AttributeId);
            info.AddValue("_IsCompleting", _IsCompleting);
            info.AddValue("_SetId", _SetId);
            info.AddValue("_ReviewerId", _ReviewerId);
            info.AddValue("_AffectedItems", _AffectedItems);
            info.AddValue("_PotentiallyAffectedItems", _PotentiallyAffectedItems);
            info.AddValue("_IsPreview", _IsPreview);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _AttributeId = info.GetValue<Int64>("_AttributeId");
            _IsCompleting = info.GetValue<bool>("_IsCompleting");
            _SetId = info.GetValue<int>("_SetId");
            _ReviewerId = info.GetValue<int>("_ReviewerId");
            _PotentiallyAffectedItems = info.GetValue<int>("_PotentiallyAffectedItems");
            _AffectedItems = info.GetValue<int>("_AffectedItems");
            _IsPreview = info.GetValue<bool>("_IsPreview");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                //Thread.Sleep(2000);
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemSetBulkCompleteOnAttribute", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    
                    command.CommandTimeout = 120;
                    command.Parameters.Add(new SqlParameter("@SET_ID", SetId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", AttributeId));
                    command.Parameters.Add(new SqlParameter("@COMPLETE", IsCompleting));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReviewerId));
                    if (IsPreview)
                    {
                        command.CommandText = "st_ItemSetBulkCompleteOnAttributePreview";
                        command.Parameters.Add(new SqlParameter("@PotentiallyAffected", System.Data.SqlDbType.Int));
                        command.Parameters["@PotentiallyAffected"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(new SqlParameter("@WouldBeAffected", System.Data.SqlDbType.Int));
                        command.Parameters["@WouldBeAffected"].Direction = System.Data.ParameterDirection.Output;
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter("@Affected", System.Data.SqlDbType.Int));
                        command.Parameters["@Affected"].Direction = System.Data.ParameterDirection.Output;
                    }
                    command.ExecuteNonQuery();
                    if (IsPreview)
                    {
                        _PotentiallyAffectedItems = (int)command.Parameters["@PotentiallyAffected"].Value;
                        _AffectedItems = (int)command.Parameters["@WouldBeAffected"].Value;
                    }
                    else
                    {
                        _AffectedItems = (int)command.Parameters["@Affected"].Value;
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}
