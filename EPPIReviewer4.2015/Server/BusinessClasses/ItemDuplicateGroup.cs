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

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using static BusinessLibrary.BusinessClasses.ItemDuplicateReadOnlyGroupList;
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
                return GetProperty(IsEditableProperty);
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
            //BusinessRules.AddRule(new IsNotInRole(AuthorizationActions.EditObject, "ReadOnlyUser" )); 
        }
        public bool ShowScores()
        {
            ItemDuplicateGroupMember mb = getMaster();
            if (mb != null && mb.ItemId == OriginalMasterID) return true;
            return false;
        }
#if SILVERLIGHT
        public ItemDuplicateGroup() { }
        
#else
        public ItemDuplicateGroup() 
        {
            Members = new MobileList<ItemDuplicateGroupMember>();
        }

#endif


#if !SILVERLIGHT
        int RevID;
        protected override void DataPortal_Update()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RevID = ri.ReviewId;
            bool toSave = false;
            Exception LastCaughtException = null;
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
#if !OLDDEDUP
                //if group has a non-original master and at least one member requires "saving changes"
                if (!ShowScores() && Members.Find(o => o.IsDirty) != null)//recalculate scores, since now we can!
                {// we do this now, as we'll then save individual members...
                    ReScore();
                }
#endif
                List<ItemDuplicateGroupMember> goneWrong = new List<ItemDuplicateGroupMember>();
                foreach (ItemDuplicateGroupMember o in this.Members)
                {
                    if (o.IsDirty)
                    {
                        toSave = true;
                        try
                        {
                            ItemDuplicateGroupMember oo = o.Save(true);
                        }
                        catch (Exception e)
                        {//we should log it in DB, so that it will work both in ER4 and ER-Web.
                            goneWrong.Add(o);//we keep track of what member(s) didn't save properly...
                            this.LogException(e, ri.UserId, o, true);
                        }
                    }
                }
                if (goneWrong.Count > 0)
                {
                    //if something went wrong and an exception was triggered, we'll try saving the changes for the members where saving failed (once)
                    System.Threading.Thread.Sleep(200);//wait a tiny bit, to give some time to SQL to "catch up"...
                    foreach (ItemDuplicateGroupMember o in goneWrong)
                    {
                        try
                        {
                            ItemDuplicateGroupMember oo = o.Save(true);
                        }
                        catch (Exception e)
                        {
                            //we should log it, in DB, so that it will work both in ER4 and ER-Web.
                            this.LogException(e, ri.UserId, o, false);
                            LastCaughtException = e;
                        }
                    }
                }
                if (toSave)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                        {
                            //fake exception for testing...
                            //uncomment code below, put a breakpoint and "drag" execution into the exception rising block, to test how the system reacts...

                            //if (1 == DateTime.Now.Ticks)
                            //{
                            //    Exception e = new Exception("this is fake, deliberately triggered(TB_ITEM_REV1)");
                            //    throw e;
                            //}
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
                    catch (Exception e) {
                        LogException(e, ri.UserId, null, true);
                        System.Threading.Thread.Sleep(200);
                        try
                        {
                            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                            {
                                //fake exception for testing...
                                //uncomment code below, put a breakpoint and "drag" execution into the exception rising block, to test how the system reacts...

                                //if (1 == DateTime.Now.Ticks)
                                //{
                                //    Exception e2 = new Exception("this is fake, deliberately triggered(TB_ITEM_REV2)");
                                //    throw e2;
                                //}
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
                        catch (Exception ee)
                        {
                            LogException(ee, ri.UserId, null, false);
                            LastCaughtException = ee;
                        }
                    }
                    
                }
            }
            if (toSave)
            {//since what happens to groups is complex: st_ItemDuplicateUpdateTbItemReview looks at the whole group and normalises what happens to tb_item_review
                //it seems safer to simply reload the whole group before sending it back, alternatively we could replicate some of the the data collection in st_ItemDuplicateUpdateTbItemReview
                // and in this method to re-load the group data directly from/in st_ItemDuplicateUpdateTbItemReview, performance wise, this alternative way is probably better than what follows.
                //System.Threading.Thread.Sleep(2000);
                if (LastCaughtException != null)
                {
                    //we get in here _only_ if an exception was thrown, caught and not compensated for, so we throw it "after" processing all that we could process.
                    //this ensures the client gets a chance to signal the problem to the user.
                    //if an exception was thrown but we could retry successfully, then we don't need the user to know.
                    throw LastCaughtException;
                }
                Members.Clear(); 
                DataPortal_Fetch(new SingleCriteria<ItemDuplicateGroup, int>(this.GroupID));
                
            }
        }
        private void LogException(Exception e, int ContactId, ItemDuplicateGroupMember member, bool firstAttempt)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_LogReviewJob", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewId", RevID));
                    command.Parameters.Add(new SqlParameter("@ContactId", ContactId));
                    command.Parameters.Add(new SqlParameter("@Success", false));
                    command.Parameters.Add(new SqlParameter("@JobType", "Save Changes to Duplicates group"));
                    command.Parameters.Add(new SqlParameter("@CurrentState", "Failure"));
                    string msg = "";
                    if (member != null)
                    {
                        msg = "Exception thrown when saving changes to group member"
                                    + (firstAttempt ? " (1st try)" : " (2nd try)") + ". Group ID: " + this.GroupID
                                    + ". Group Member ID: " + member.ItemDuplicateId + " (ItemId: " + member.ItemId + "). "
                                    + "IsChecked: " + member.IsChecked.ToString() + "; "
                                    + "IsDuplicate: " + member.IsDuplicate.ToString() + "; "
                                    + "IsMaster: " + member.IsMaster.ToString() + ". "
                                    + "ERROR: " + e.Message;
                    } 
                    else
                    {
                        msg = "Exception thrown when applying group changes to TB_ITEM_REVIEW"
                                    + (firstAttempt ? " (1st try)" : " (2nd try)") + ". Group ID: " + this.GroupID
                                    + ". "
                                    + "ERROR: " + e.Message;
                    }
                    command.Parameters.Add(new SqlParameter("@JobMessage", msg));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        private void ReScore()
        {
            //3 phases: (1) get item details for all members. (2) recalculate. (3) save data (includes lying, as we'll update also the OriginalItemID field).

            //fetch data
            string IDs = "";
            ItemDuplicateGroupMember MasterMember = this.getMaster();
            if (MasterMember == null) return;//can't do anything sensible without it...
            foreach (ItemDuplicateGroupMember m in this.Members)
            {
                IDs += m.ItemId + ",";
            }
            IDs = IDs.TrimEnd(',');
            ItemComparison MasterComparator = new ItemComparison();
            List<ItemComparison> rescoreGroup = new List<ItemComparison>();
            using (SqlConnection conn = new SqlConnection(DataConnection.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("st_ItemDuplicatesGetGroupMembersForScoring", conn))
                {
                    
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@REVIEW_ID", RevID));
                    cmd.Parameters.Add(new SqlParameter("@ITEM_IDS", IDs));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(cmd.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            ItemComparison member = new ItemComparison(reader, 0);
                            if (member.ITEM_ID != MasterMember.ItemId) rescoreGroup.Add(member);
                            else
                            {//this is our new master
                                MasterComparator = member;
                            }
                        }
                    }
                }
                if (MasterComparator.ITEM_ID == 0 || rescoreGroup.Count < 1) return;//something didn't add up and we can't do anything...
                //(2) rescore all;
                Comparator comparer = new Comparator();
                foreach (ItemComparison m in rescoreGroup)
                {
                    ItemDuplicateGroupMember gm = this.Members.Find(found => m.ITEM_ID == found.ItemId);
                    if (gm != null)
                    {
                        gm.SimilarityScore = comparer.CompareItems(MasterComparator, m);
                    }
                }
                MasterMember.SimilarityScore = 1;//always for the master member!
                //(3) update group Master: we'll save item members elsewhere, but we need to change the "ORIGINAL_MASTER_ID" field of this group so that new scores will show.
                using (SqlCommand cmd = new SqlCommand("st_ItemDuplicatesGetGroupChangeOriginalMasterId", conn))
                {
                    long MasterID = this.getMaster().ItemId;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@REVIEW_ID", RevID));
                    cmd.Parameters.Add(new SqlParameter("@GROUP_ID", GroupID));
                    cmd.Parameters.Add(new SqlParameter("@NEWMASTER_ID", MasterComparator.ITEM_ID));
                    cmd.ExecuteNonQuery();
                }

            }
            
        }

        protected void DataPortal_Fetch(SingleCriteria<ItemDuplicateGroup, int> criteria)
        {
            
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupMembers", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@groupID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@ReviewID", ri.ReviewId)); 
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
                LoadProperty(IsEditableProperty, ri.HasWriteRights());
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
