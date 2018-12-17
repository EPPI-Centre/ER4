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
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemDuplicateDirtyGroup: BusinessBase<ItemDuplicateDirtyGroup>
    {

        public static void GetItemDuplicateList(string IDs, EventHandler<DataPortalResult<ItemDuplicateDirtyGroup>> handler)
        {
            DataPortal<ItemDuplicateDirtyGroup> dp = new DataPortal<ItemDuplicateDirtyGroup>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemDuplicateDirtyGroup, string>(IDs));
        }
        public ItemDuplicateDirtyGroupMember getMaster()
        {
            foreach (ItemDuplicateDirtyGroupMember gm in Members)
            {
                if (gm.IsMaster) return gm;
            }
            return null;
        }
        public bool setMaster(Int64 ID)
        {
            bool canDo = false;
            foreach (ItemDuplicateDirtyGroupMember m in Members)
            {
                if (m.ItemId == ID && m.IsAvailable)
                {
                    canDo = true;
                    break;
                }
            }
            if (!canDo) return false;
            int chk = 0;
            foreach  (ItemDuplicateDirtyGroupMember m in Members)
            {
                if (m.ItemId == ID && m.IsAvailable)
                {
                    m.IsMaster = true;
                    chk++;
                }
                else if (m.IsMaster)
                {
                    m.IsMaster = false;
                    chk++;
                }
                if (chk == 2) return true;
            }
            return false;
        }
        public bool IsUsable
        {
            get
            {
                int countM = 0;
                int countValidMembers = 0;
                foreach (ItemDuplicateDirtyGroupMember m in Members)
                {
                    if (m.IsMaster) countM++;
                    if (m.IsAvailable) countValidMembers++;
                }
                if (countM == 1 && countValidMembers > 1) return true;
                return false;
            }
        }
        private static PropertyInfo<MobileList<ItemDuplicateDirtyGroupMember>> MembersProperty = RegisterProperty<MobileList<ItemDuplicateDirtyGroupMember>>(new PropertyInfo<MobileList<ItemDuplicateDirtyGroupMember>>("Members", "Members"));
        public MobileList<ItemDuplicateDirtyGroupMember> Members
        {
            get
            {
                return GetProperty(MembersProperty);
            }
            set
            {
                SetProperty(MembersProperty, value);
            }
        }
       
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ItemSet), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ItemSet), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ItemSet), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ItemSet), canRead);
        //    string[] denyEditSave = new string[] { "ReadOnlyUser" };
        //    AuthorizationRules.DenyEdit(typeof(ItemDuplicateGroup), denyEditSave);
        //}
#if SILVERLIGHT
        public ItemDuplicateDirtyGroup() { }
        
#else
        private ItemDuplicateDirtyGroup() 
        {
            Members = new MobileList<ItemDuplicateDirtyGroupMember>();
        }
        
#endif


#if !SILVERLIGHT




        public void DataPortal_Fetch(SingleCriteria<ItemDuplicateDirtyGroup, string> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int justToCheck = ri.ReviewId;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateDirtyGroupMembers", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IDs", criteria.Value);
                    command.Parameters.AddWithValue("@RevID", justToCheck);
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Members.Add(ItemDuplicateDirtyGroupMember.GetItemDuplicate(reader));
                        }
                        //reader.NextResult();
                        //reader.Read();
                        //LoadProperty<long>(OriginalMasterIDProperty, reader.GetInt64("ORIGINAL_MASTER_ID"));
                    }
                }
                foreach (ItemDuplicateDirtyGroupMember m in Members)
                {
                    if (m.IsAvailable)
                    {
                        m.IsMaster = true;
                        break;
                    }
                }
                MarkNew();
                connection.Close();
                //LoadProperty(GroupIDProperty, criteria.Value);
            }
        }
        protected override void DataPortal_Update()
        {
            DataPortal_Insert();
        }
        protected override void DataPortal_Insert()
        {
            if (Members == null || Members.Count < 1) return;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int justToCheck = ri.ReviewId;
            string IDs = "";
            Int64 MasterID = 0;
            foreach (ItemDuplicateDirtyGroupMember iddg in Members)
            {
                if (iddg.IsMaster) MasterID = iddg.ItemId;
                else if(iddg.IsAvailable) IDs += iddg.ItemId.ToString() + ",";
            }
            if (MasterID == 0) return;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupAddNew", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IDs", IDs);
                    command.Parameters.AddWithValue("@MasterID", MasterID);
                    command.Parameters.AddWithValue("@RevID", justToCheck);
                    command.ExecuteNonQuery();
                    //using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    //{
                    //    while (reader.Read())
                    //    {
                    //        Members.Add(ItemDuplicateDirtyGroupMember.GetItemDuplicate(reader, 0));
                    //    }
                    //    //reader.NextResult();
                    //    //reader.Read();
                    //    //LoadProperty<long>(OriginalMasterIDProperty, reader.GetInt64("ORIGINAL_MASTER_ID"));
                    //}
                }
                //MarkNew();
                connection.Close();
                //LoadProperty(GroupIDProperty, criteria.Value);
            }
        }

#endif
    }
}
