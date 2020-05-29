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
    public class ItemDuplicateReadOnlyGroupList : ReadOnlyListBase<ItemDuplicateReadOnlyGroupList, ItemDuplicateReadOnlyGroup>
    {

        public ItemDuplicateReadOnlyGroupList() { }
        public int CompletedCount
        {
            get
            {
                int res = 0;
                if (this.Count > 0)
                {
                    foreach (ItemDuplicateReadOnlyGroup G in this)
                    {
                        if (G.IsComplete) res++;
                    }
                }
                return res;
            }
        }
        public static void getItemDuplicateReadOnlyGroupList(bool getNew, EventHandler<DataPortalResult<ItemDuplicateReadOnlyGroupList>> handler)
        {
            DataPortal<ItemDuplicateReadOnlyGroupList> dp = new DataPortal<ItemDuplicateReadOnlyGroupList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(getNew));
        }
        public static void GetGroupsList(EventHandler<DataPortalResult<ItemDuplicateReadOnlyGroupList>> handler)
        {
            DataPortal<ItemDuplicateReadOnlyGroupList> dp = new DataPortal<ItemDuplicateReadOnlyGroupList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(false));
        }
#if !SILVERLIGHT

        private void DataPortal_Fetch(SingleCriteria<ItemDuplicateReadOnlyGroupList, bool> criteria)
        {
            IsReadOnly = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupCheckOngoing", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@revID", System.Data.SqlDbType.Int);
                    command.Parameters["@revID"].Value = ri.ReviewId;
                    command.Parameters.Add("@RETURN_VALUE", System.Data.SqlDbType.Int);
                    command.Parameters["@RETURN_VALUE"].Direction = System.Data.ParameterDirection.ReturnValue;
                    command.CommandTimeout = 300;
                    command.ExecuteNonQuery();
                    if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "-2")
                    {
                        this.Clear();
                        throw new DataPortalException("Execution still Running", this);
                    }
                    else if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "-3")
                    {//we'll see if this happens frequently
                        this.Clear();
                        throw new DataPortalException("Previous Execution failed." + Environment.NewLine
                            + "Please try to \"Get new duplicates\" once more." + Environment.NewLine
                            + "If this message appears again, please contact EPPI-reviewer Support Staff.", this);
                    }
                }
                if (criteria.Value == true)
                {
                    FindNewDuplicates(connection);
                    //if (this.Count == 1)
                    //{
                    //    //RaiseListChangedEvents = true;
                    //    this.Clear();

                    //    throw new DataPortalException("Execution still Running", this);
                    //}
                }
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemDuplicateReadOnlyGroup.GetItemDuplicateReadOnlyGroup(reader));
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            //RaiseListChangedEvents = true;
        }

        protected void FindNewDuplicates(SqlConnection connection)
        {
            // At the moment, this doesn't find duplicates at all - but it will soon!
            using (SqlCommand command = new SqlCommand("st_ItemDuplicateFindNew", connection))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@revID", ri.ReviewId));
                command.CommandTimeout = 300;
                //string r = "";
                try
                {
                    command.ExecuteNonQuery();
                    //using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    //{

                    //    while (reader.Read())
                    //    {
                    //        if (reader[0] != null) r += reader[0].ToString() + Environment.NewLine;
                    //    }
                    //    r += "!!";
                    //}
                }
                catch (Exception ex)
                {
                    //throw exception that will be interpreted as a timeout and handled client side
                    throw new DataPortalException("Execution still Running", this);
                }
            }
        }
        private void DataPortal_Fetch(GroupListSelectionCriteria criteria)
        {
            IsReadOnly = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupListSearch", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ItemIDs", criteria.ItemIds));
                    command.Parameters.Add(new SqlParameter("@GroupID", criteria.GroupId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemDuplicateReadOnlyGroup.GetItemDuplicateReadOnlyGroup(reader));
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

#endif
    }
    [Serializable]
    public class GroupListSelectionCriteria : Csla.CriteriaBase<GroupListSelectionCriteria>
    {
        private static PropertyInfo<int> GroupIdProperty = RegisterProperty<int>(typeof(GroupListSelectionCriteria), new PropertyInfo<int>("GroupId", "GroupId"));
        public int GroupId
        {
            get { return ReadProperty(GroupIdProperty); }
        }
        private static PropertyInfo<string> ItemIdsProperty = RegisterProperty<string>(typeof(GroupListSelectionCriteria), new PropertyInfo<string>("ItemIds", "ItemIds"));
        public string ItemIds
        {
            get { return ReadProperty(ItemIdsProperty); }
        }

        public GroupListSelectionCriteria(Type type, int GroupId)
        //: base(type)
        {
            LoadProperty(GroupIdProperty, GroupId);
            LoadProperty(ItemIdsProperty, "0");
        }

        public GroupListSelectionCriteria(Type type, string ItemIds)
        //: base(type)
        {

            LoadProperty(ItemIdsProperty, ItemIds);
            LoadProperty(GroupIdProperty, 0);

        }
#if !SILVERLIGHT 
        public GroupListSelectionCriteria() { }
#endif
    }
}
