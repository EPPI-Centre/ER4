using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using Csla.Rules;
using Csla.Rules.CommonRules;
//using Csla.Validation;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemDuplicateGroup : BusinessBase<ItemDuplicateGroup>
    {

        public static void GetItemDuplicateList(int GroupID, EventHandler<DataPortalResult<ItemDuplicateGroup>> handler)
        {
            DataPortal<ItemDuplicateGroup> dp = new DataPortal<ItemDuplicateGroup>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemDuplicateGroup, int>(GroupID));
        }
        public ItemDuplicateGroupMember getMaster()
        {
            foreach (ItemDuplicateGroupMember gm in Members)
            {
                if (gm.IsMaster) return gm;
            }
            return null;
        }
        public bool isComplete()
        {
            if (!IsEditable) return true;
            foreach (ItemDuplicateGroupMember mb in Members)
            {
                if (!mb.IsChecked) return false;
            }
            return true;
        }
        
        public readonly static PropertyInfo<int> GroupIDProperty = RegisterProperty<int>(new PropertyInfo<int>("GroupID", "GroupID"));
        public int GroupID
        {
            get
            {
                return GetProperty(GroupIDProperty);
            }
            //set
            //{
            //    SetProperty(GroupIDProperty, value);
            //}
        }
        public readonly static PropertyInfo<long> OriginalMasterIDProperty = RegisterProperty<long>(new PropertyInfo<long>("OriginalMasterID", "OriginalMasterID"));
        public long OriginalMasterID
        {
            get
            {
                return GetProperty(OriginalMasterIDProperty);
            }
        }
        public readonly static PropertyInfo<bool> IsEditableProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsEditable", "IsEditable"));
        public bool IsEditable
        {
            get
            {
                return GetProperty(IsEditableProperty) && 
                    Csla.Rules.BusinessRules.HasPermission( AuthorizationActions.EditObject, this);
                    // Csla.Security.AuthorizationRules.CanEditObject(this.GetType());
            }
            set
            {
                SetProperty(IsEditableProperty, value);
            }
        }
        public readonly static PropertyInfo<int> AddGroupIDProperty = RegisterProperty<int>(new PropertyInfo<int>("AddGroupID", "AddGroupID"));
        public int AddGroupID
        {
            get
            {
                return GetProperty(AddGroupIDProperty);
            }
            set
            {
                SetProperty(AddGroupIDProperty, value);
            }
        }
        public readonly static PropertyInfo<MobileList<long>> AddItemsProperty = RegisterProperty<MobileList<long>>(new PropertyInfo<MobileList<long>>("AddItemID", "AddItemID"));
        public MobileList<long> AddItems
        {
            get
            {
                return GetProperty(AddItemsProperty);
            }
            set
            {
                SetProperty(AddItemsProperty, value);
            }
        }
        public readonly static PropertyInfo<long> RemoveItemIDProperty = RegisterProperty<long>(new PropertyInfo<long>("RemoveItemID", "RemoveItemID"));
        public long RemoveItemID
        {
            get
            {
                return GetProperty(RemoveItemIDProperty);
            }
            set
            {
                SetProperty(RemoveItemIDProperty, value);
            }
        }
        public readonly static PropertyInfo<MobileList<ItemDuplicateGroupMember>> MembersProperty = RegisterProperty<MobileList<ItemDuplicateGroupMember>>(new PropertyInfo<MobileList<ItemDuplicateGroupMember>>("Members", "Members"));
        public MobileList<ItemDuplicateGroupMember> Members
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
        public readonly static PropertyInfo<MobileList<ItemDuplicateManualGroupMember>> ManualMembersProperty = RegisterProperty<MobileList<ItemDuplicateManualGroupMember>>(new PropertyInfo<MobileList<ItemDuplicateManualGroupMember>>("ManualMembers", "ManualMembers"));
        public MobileList<ItemDuplicateManualGroupMember> ManualMembers
        {
            get
            {
                return GetProperty(ManualMembersProperty);
            }
            set
            {
                SetProperty(ManualMembersProperty, value);
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

        protected override void AddBusinessRules()
        {
            BusinessRules.AddRule(new IsNotInRole(AuthorizationActions.EditObject, "ReadOnlyUser" )); 
        }
#if SILVERLIGHT
        public ItemDuplicateGroup() { }
        public bool ShowScores()
        {
            ItemDuplicateGroupMember mb = getMaster();
            if (mb != null && mb.ItemId == OriginalMasterID) return true;
            return false;
        }
        
#else
        public ItemDuplicateGroup() 
        {
            Members = new MobileList<ItemDuplicateGroupMember>();
        }
        
#endif


#if !SILVERLIGHT
        
        protected override void DataPortal_Update()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int RevID = ri.ReviewId;
            bool toSave = false; 
            if (AddGroupID != 0)
            {
                toSave = true;
                //run update SP & reset input parameter
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupManualMerge", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add("@SourceGroupID", System.Data.SqlDbType.Int);
                        command.Parameters["@SourceGroupID"].Value = AddGroupID;
                        AddGroupID = 0;
                        command.Parameters.Add("@MasterID", System.Data.SqlDbType.Int);
                        command.Parameters["@MasterID"].Value = this.getMaster().ItemId;
                        command.Parameters.Add("@RevID", System.Data.SqlDbType.Int);
                        command.Parameters["@RevID"].Value = RevID;
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                //refetch manual members
                //GetManualMembers();
            }
            else if (AddItems != null && AddItems.Count != 0)
            {
                toSave = true;
                //run update SP & reset input parameter
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    long masterID = this.getMaster().ItemId;
                    connection.Open();
                    foreach (long AddItemID in AddItems)
                    {
                        using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupManualAddItem", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add("@NewDuplicateItemID", System.Data.SqlDbType.BigInt);
                            command.Parameters["@NewDuplicateItemID"].Value = AddItemID;

                            command.Parameters.Add("@MasterID", System.Data.SqlDbType.Int);
                            command.Parameters["@MasterID"].Value = masterID;
                            command.Parameters.Add("@RevID", System.Data.SqlDbType.Int);
                            command.Parameters["@RevID"].Value = RevID;
                            command.ExecuteNonQuery();
                        }
                    }
                    AddItems.Clear();
                    connection.Close();
                }
                //refetch manual members
                //GetManualMembers();
            }
            else if ( RemoveItemID  != 0)
            {
                toSave = true;
                //run update SP & reset input parameter
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupManualRemoveItem", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add("@DuplicateItemID", System.Data.SqlDbType.BigInt);
                        command.Parameters["@DuplicateItemID"].Value = RemoveItemID;
                        RemoveItemID = 0;
                        command.Parameters.Add("@RevID", System.Data.SqlDbType.Int);
                        command.Parameters["@RevID"].Value = RevID;
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                //refetch manual members
                //GetManualMembers();
            }
            else
            {
                
                foreach (ItemDuplicateGroupMember o in this.Members)
                {
                    if (o.IsDirty)
                    {
                        toSave = true;
                        ItemDuplicateGroupMember oo = o.Save(true);
                    }
                }
                if (toSave)
                {
                    
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_ItemDuplicateUpdateTbItemReview", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add("@groupID", System.Data.SqlDbType.Int);
                            command.Parameters["@groupID"].Value = GroupID;
                            command.ExecuteNonQuery();
                        }
                        connection.Close();
                    }
                    
                }
            }
            if (toSave)
            {//since what happens to groups is complex: st_ItemDuplicateUpdateTbItemReview looks at the whole group and normalises what happens to tb_item_review
                //it seems safer to simply reload the whole group before sending it back, alternatively we could replicate some of the the data collection in st_ItemDuplicateUpdateTbItemReview
                // and in this method to re-load the group data directly from/in st_ItemDuplicateUpdateTbItemReview, performance wise, this alternative way is probably better than what follows.
                //System.Threading.Thread.Sleep(2000);
                Members.Clear(); 
                DataPortal_Fetch(new SingleCriteria<ItemDuplicateGroup, int>(this.GroupID));
            }
        }


        protected void DataPortal_Fetch(SingleCriteria<ItemDuplicateGroup, int> criteria)
        {
            
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int justToCheck = ri.ReviewId;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupMembers", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@groupID", System.Data.SqlDbType.Int);
                    command.Parameters["@groupID"].Value = criteria.Value;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Members.Add(ItemDuplicateGroupMember.GetItemDuplicate(reader, criteria.Value));
                        }
                        reader.NextResult();
                        reader.Read();
                        LoadProperty<long>(OriginalMasterIDProperty, reader.GetInt64("ORIGINAL_MASTER_ID"));
                    }
                }
                connection.Close();
                LoadProperty(GroupIDProperty, criteria.Value);
                GetManualMembers();
            }
        }
        protected void GetManualMembers()
        {
            if (ManualMembers != null) ManualMembers.Clear();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupManualMembers", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@groupID", System.Data.SqlDbType.Int);
                    command.Parameters["@groupID"].Value = GroupID;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            if (ManualMembers == null) ManualMembers = new MobileList<ItemDuplicateManualGroupMember>();
                            ManualMembers.Add(ItemDuplicateManualGroupMember.GetItemDuplicate(reader, GroupID));   
                            //.Add(ItemDuplicateGroupMember.GetItemDuplicate(reader, GroupID));
                        }
                    }
                }
                connection.Close();
            }
            if (this.getMaster() == null)
                Console.WriteLine(this.GroupID);
            IsEditable = this.getMaster().IsEditable;
        }
        protected override void DataPortal_DeleteSelf()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int justToCheck = ri.ReviewId;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateDeleteGroup", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@GroupID", System.Data.SqlDbType.Int);
                    command.Parameters["@GroupID"].Value = this.GroupID;
                    command.Parameters.Add("@ReviewID", System.Data.SqlDbType.Int);
                    command.Parameters["@ReviewID"].Value = justToCheck;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
#endif



    }
}
