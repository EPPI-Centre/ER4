using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using Csla.DataPortalClient;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ERxWebClient2.Controllers;

//using Csla.Configuration;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Linq;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ZoteroReviewCollection : BusinessBase<ZoteroReviewCollection>
    {
        public static void GetZoteroReviewCollection(EventHandler<DataPortalResult<ZoteroReviewCollection>> handler)
        {
            DataPortal<ZoteroReviewCollection> dp = new DataPortal<ZoteroReviewCollection>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public ZoteroReviewCollection() { }

        
#else
        public ZoteroReviewCollection() { }
#endif


        public static readonly PropertyInfo<string> CollectionKeyProperty = RegisterProperty<string>(new PropertyInfo<string>("CollectionKey", "CollectionKey", ""));
  
        public string CollectionKey
        {
            get
            {
                return GetProperty(CollectionKeyProperty);
            }
            set
            {
                SetProperty(CollectionKeyProperty, value);
            }
        }


        public static readonly PropertyInfo<string> LibraryIDProperty = RegisterProperty<string>(new PropertyInfo<string>("LibraryID", "LibraryID", ""));
        public string LibraryID
        {
            get
            {
                return GetProperty(LibraryIDProperty);
            }
            set
            {
                SetProperty(LibraryIDProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ApiKeyProperty = RegisterProperty<string>(new PropertyInfo<string>("ApiKey", "ApiKey", ""));
        public string ApiKey
        {
            get
            {
                return GetProperty(ApiKeyProperty);
            }
            set
            {
                SetProperty(ApiKeyProperty, value);
            }
        }

        public static readonly PropertyInfo<long> REVIEW_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("REVIEW_ID", "REVIEW_ID", 0m));
        public long REVIEW_ID
        {
            get
            {
                return GetProperty(REVIEW_IDProperty);
            }
            set
            {
                SetProperty(REVIEW_IDProperty, value);
            }
        }

        public static readonly PropertyInfo<int> USER_IDProperty = RegisterProperty<int>(new PropertyInfo<int>("USER_ID", "USER_ID", 0));
        public int USER_ID
        {
            get
            {
                return GetProperty(USER_IDProperty);
            }
            set
            {
                SetProperty(USER_IDProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ParentCollectionProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentCollection", "ParentCollection", ""));
        public string ParentCollection
        {
            get
            {
                return GetProperty(ParentCollectionProperty);
            }
            set
            {
                SetProperty(ParentCollectionProperty, value);
            }
        }


        public static readonly PropertyInfo<string> CollectionNameProperty = RegisterProperty<string>(new PropertyInfo<string>("CollectionName", "CollectionName", ""));
        public string CollectionName
        {
            get
            {
                return GetProperty(CollectionNameProperty);
            }
            set
            {
                SetProperty(CollectionNameProperty, value);
            }
        }

        public static readonly PropertyInfo<long> VersionProperty = RegisterProperty<long>(new PropertyInfo<long>("Version", "Version", 0m));
        public long Version
        {
            get
            {
                return GetProperty(VersionProperty);
            }
            set
            {
                SetProperty(VersionProperty, value);
            }
        }

        public static readonly PropertyInfo<DateTime> DateCreatedProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("DateCreated", "DateCreated", DateTime.Now));
        public DateTime DateCreated
        {
            get
            {
                return GetProperty(DateCreatedProperty);
            }
            set
            {
                SetProperty(DateCreatedProperty, value);
            }
        }

        public static readonly PropertyInfo<int> GroupBeingSyncedProperty = RegisterProperty<int>(new PropertyInfo<int>("GroupBeingSynced", "GroupBeingSynced", 0));
        public int GroupBeingSynced
        {
            get
            {
                return GetProperty(GroupBeingSyncedProperty);
            }
            set
            {
                SetProperty(GroupBeingSyncedProperty, value);
            }
        }



#if !SILVERLIGHT



        protected void DataPortal_Fetch(SingleCriteria<ZoteroReviewCollection, long> criteria)
		{
			using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("st_ZoteroReviewConnection", connection))
				{
					command.CommandType = System.Data.CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@ReviewID", criteria.Value));
					using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
					{
						if (reader.Read())
						{
							LoadProperty<string>(CollectionKeyProperty, reader.GetString("CollectionKey"));
							LoadProperty<string>(LibraryIDProperty, reader.GetString("LibraryID"));
							LoadProperty<string>(ApiKeyProperty, reader.GetString("ApiKey"));
							LoadProperty<long>(REVIEW_IDProperty, reader.GetInt64("ReviewId"));
							LoadProperty<int>(USER_IDProperty, reader.GetInt32("UserId"));
							LoadProperty<string>(ParentCollectionProperty, reader.GetString("ParentCollection"));
							LoadProperty<string>(CollectionNameProperty, reader.GetString("CollectionName"));
							LoadProperty<long>(VersionProperty, reader.GetInt64("Version"));
							LoadProperty<DateTime>(DateCreatedProperty, reader.GetDateTime("DateCreated"));
                            LoadProperty<int>(GroupBeingSyncedProperty, reader.GetInt32("GroupBeingSynced"));

                            MarkOld();
						}
					}
				}
				connection.Close();
			}
		}

		protected override void DataPortal_Insert()
        {
            AddNew();
        }

        private void AddNew()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroCollectionCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CollectionKey", ReadProperty(CollectionKeyProperty)));
                    command.Parameters.Add(new SqlParameter("@LibraryID", ReadProperty(LibraryIDProperty)));
                    command.Parameters.Add(new SqlParameter("@ApiKey", ReadProperty(ApiKeyProperty)));
                    command.Parameters.Add(new SqlParameter("@USER_ID", ReadProperty(USER_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(REVIEW_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@ParentCollection", ReadProperty(ParentCollectionProperty)));
                    command.Parameters.Add(new SqlParameter("@CollectionName", ReadProperty(CollectionNameProperty)));
                    command.Parameters.Add(new SqlParameter("@Version", ReadProperty(VersionProperty)));
                    command.Parameters.Add(new SqlParameter("@DateCreated", ReadProperty(DateCreatedProperty)));
                    command.Parameters.Add(new SqlParameter("@GroupBeingSynced", ReadProperty(GroupBeingSyncedProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public class UserDetails
        {
            public int userId { get; set; }

            public long reviewId { get; set; }
        }

        protected override void DataPortal_Update()
        {

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroCollectionUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@LibraryID", ReadProperty(LibraryIDProperty)));
                    command.Parameters.Add(new SqlParameter("@ApiKey", ReadProperty(ApiKeyProperty)));
                    command.Parameters.Add(new SqlParameter("@USER_ID", ReadProperty(USER_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(REVIEW_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@GroupBeingSynced", ReadProperty(GroupBeingSyncedProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroCollectionDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@USER_ID", ReadProperty(USER_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(REVIEW_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@LibraryID", ReadProperty(LibraryIDProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static ZoteroReviewCollection GetZoteroReviewCollection(SafeDataReader reader)
        {
            ZoteroReviewCollection returnValue = new ZoteroReviewCollection();
            returnValue.LoadProperty<string>(CollectionKeyProperty, reader.GetString("CollectionKey"));
            returnValue.LoadProperty<string>(LibraryIDProperty, reader.GetString("LibraryID"));
            returnValue.LoadProperty<string>(ApiKeyProperty, reader.GetString("ApiKey"));
            returnValue.LoadProperty<long>(REVIEW_IDProperty, reader.GetInt64("ReviewId"));
            returnValue.LoadProperty<int>(USER_IDProperty, reader.GetInt32("UserId"));
            returnValue.LoadProperty<string>(ParentCollectionProperty, reader.GetString("ParentCollection"));
            returnValue.LoadProperty<string>(CollectionNameProperty, reader.GetString("CollectionName"));
            returnValue.LoadProperty<long>(VersionProperty, reader.GetInt64("Version"));
            returnValue.LoadProperty<DateTime>(DateCreatedProperty, reader.GetDateTime("DateCreated"));
            returnValue.LoadProperty<int>(GroupBeingSyncedProperty, reader.GetInt32("GroupBeingSynced"));

            returnValue.MarkOld();
            return returnValue;
        }
#endif
    }
}