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
using BusinessLibrary.Security;

#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlySource : ReadOnlyBase<ReadOnlySource>
    {
        public ReadOnlySource() { }

        // only gets name and id at the moment to save server resources (used in dialog coding to display the source name for an item)
        public static void GetItemReadOnlySource(Int64 ItemId, EventHandler<DataPortalResult<ReadOnlySource>> handler)
        {
            DataPortal<ReadOnlySource> dp = new DataPortal<ReadOnlySource>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ReadOnlySource, Int64>(ItemId));
        }

        public static readonly PropertyInfo<string> Source_NameProperty = RegisterProperty<string>(new PropertyInfo<string>("Source_Name", "Source_Name"));
        public string Source_Name
        {
            get
            {
                return GetProperty(Source_NameProperty);
            }
        }

        public static readonly PropertyInfo<int> Total_ItemsProperty = RegisterProperty<int>(new PropertyInfo<int>("Total_Items", "Total_Items"));
        public int Total_Items
        {
            get
            {
                return GetProperty(Total_ItemsProperty);
            }
        }

        public static readonly PropertyInfo<int> Deleted_ItemsProperty = RegisterProperty<int>(new PropertyInfo<int>("Deleted_Items", "Deleted_Items"));
        public int Deleted_Items
        {
            get
            {
                return GetProperty(Deleted_ItemsProperty);
            }
        }
        public static readonly PropertyInfo<int> DuplicatesProperty = RegisterProperty<int>(new PropertyInfo<int>("Duplicates", "Duplicates"));
        public int Duplicates
        {
            get
            {
                return GetProperty(DuplicatesProperty);
            }
        }
        public static readonly PropertyInfo<int> Source_IDProperty = RegisterProperty<int>(new PropertyInfo<int>("Source_ID", "Source_ID"));
        public int Source_ID
        {
            get
            {
                return GetProperty(Source_IDProperty);
            }
        }
        public static readonly PropertyInfo<bool> IsDeletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsDeleted", "IsDeleted"));
        public bool IsDeleted
        {
            get
            {
                return GetProperty(IsDeletedProperty);
            }
        }
        public static readonly PropertyInfo<bool> IsBeingDeletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsBeingDeleted", "IsBeingDeleted", false));
        public bool IsBeingDeleted
        {
            get
            {
                return GetProperty(IsBeingDeletedProperty);
            }
        }
        public void MarkAsBeingDeleted()
        {
            LoadProperty<bool>(IsBeingDeletedProperty, true);
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyItemAttribute), canRead);
        //    //AuthorizationRules.AllowRead(ItemAttributeIdProperty, canRead);
        //}

#if !SILVERLIGHT

        public static ReadOnlySource GetReadOnlySource(SafeDataReader reader)
        {
            return DataPortal.FetchChild<ReadOnlySource>(reader);
        }

        protected void DataPortal_Fetch(SingleCriteria<ReadOnlySource, Int64> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemSourceDetails", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            LoadProperty<string>(Source_NameProperty, reader.GetString("Source_Name"));
                            LoadProperty<int>(Source_IDProperty, reader.GetInt32("Source_ID"));
                        }
                    }
                }
                connection.Close();
            }
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            LoadProperty<string>(Source_NameProperty, reader.GetString("Source_Name"));
            LoadProperty<int>(Total_ItemsProperty, reader.GetInt32("Total_Items"));
            LoadProperty<int>(Deleted_ItemsProperty, reader.GetInt32("Deleted_Items"));
            LoadProperty<int>(Source_IDProperty, reader.GetInt32("Source_ID"));
            LoadProperty<bool>(IsDeletedProperty, reader.GetInt32("IS_DELETED") == 1 ? true : false);
            LoadProperty<int>(DuplicatesProperty, reader.GetInt32("Duplicates"));
            LoadProperty<bool>(IsBeingDeletedProperty, false);
        }


#endif
    }
}
