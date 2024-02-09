
using Csla;
using Newtonsoft.Json;
using BusinessLibrary.Security;
#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ZoteroReviewConnection : BusinessBase<ZoteroReviewConnection>
    {
        public ZoteroReviewConnection() { }

        public static readonly PropertyInfo<int> ZoteroConnectionIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ZoteroConnectionId", "ZoteroConnectionId"));
        public int ZoteroConnectionId
        {
            get
            {
                return GetProperty(ZoteroConnectionIdProperty);
            }
            set
            {
                SetProperty(ZoteroConnectionIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> LibraryIdProperty = RegisterProperty<string>(new PropertyInfo<string>("LibraryID", "LibraryID", ""));
        public string LibraryId
        {
            get
            {
                return GetProperty(LibraryIdProperty);
            }
            set
            {
                SetProperty(LibraryIdProperty, value);
            }
        }
#if !SILVERLIGHT
        public static readonly PropertyInfo<string> ApiKeyProperty = RegisterProperty<string>(new PropertyInfo<string>("ApiKey", "ApiKey", ""));

        //ApiKey doesn't leave the server side!!
        [JsonIgnore]
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
#endif
        public static readonly PropertyInfo<int> REVIEW_IDProperty = RegisterProperty<int>(new PropertyInfo<int>("REVIEW_ID", "REVIEW_ID", 0m));
        public int REVIEW_ID
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
        public int ErUserId
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
        public static readonly PropertyInfo<int> ZoteroUserIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ZoteroUserId", "ZoteroUserId", 0));
        public int ZoteroUserId
        {
            get
            {
                return GetProperty(ZoteroUserIdProperty);
            }
            set
            {
                SetProperty(ZoteroUserIdProperty, value);
            }
        }
        public string Status
        {
            get
            {
                if (ZoteroConnectionId < 1 || ApiKey == "") return "No API Key";
                else //API key exists...
                {
                    if (LibraryId == "") return "No Group Library";
                    else return "OK";
                }
            }
        }
#if !SILVERLIGHT
        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroReviewConnection", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewID", ri.ReviewId));

                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {

                            LoadProperty<string>(LibraryIdProperty, reader.GetString("LibraryID"));
                            ApiKey = reader.GetString("ApiKey");
                            LoadProperty<int>(ZoteroUserIdProperty, reader.GetInt32("ZoteroUserId"));
                            LoadProperty<int>(REVIEW_IDProperty, reader.GetInt32("ReviewId"));
                            LoadProperty<int>(USER_IDProperty, reader.GetInt32("UserId"));
                            LoadProperty<int>(ZoteroConnectionIdProperty, reader.GetInt32("ZOTERO_CONNECTION_ID"));
                            MarkOld();
                        }
                    }
                }
                connection.Close();
            }
        }
        protected override void DataPortal_Insert()
        {
            int localREVIEW_ID, localCONTACT_ID;
            if (Csla.ApplicationContext.User == null || Csla.ApplicationContext.User.Identity as ReviewerIdentity == null)
            {//we reached this point from the oAuth callback done by Zotero, so we can't use the authenticated user to find the right data,
                //we need to use the values supplied inside the object
                localREVIEW_ID = REVIEW_ID;
                localCONTACT_ID = ErUserId;
            }
            else
            {//got here from an Angular client call, we know who the user is for sure, so we'll use the revieweIdentity object, which is safer
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                localREVIEW_ID = ri.ReviewId;
                localCONTACT_ID = ri.UserId;
            }
            
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroConnectionCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@LibraryID", LibraryId == "" ? DBNull.Value : LibraryId));
                    command.Parameters.Add(new SqlParameter("@ZoteroUserId", ZoteroUserId));
                    command.Parameters.Add(new SqlParameter("@ApiKey", ApiKey));
                    command.Parameters.Add(new SqlParameter("@USER_ID", localCONTACT_ID));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", localREVIEW_ID));
                    command.Parameters.Add(new SqlParameter("@ZOTERO_CONNECTION_ID", System.Data.SqlDbType.Int));
                    command.Parameters["@ZOTERO_CONNECTION_ID"].Value = 0;
                    command.Parameters["@ZOTERO_CONNECTION_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ZoteroUserIdProperty, command.Parameters["@ZOTERO_CONNECTION_ID"].Value);
                }
                connection.Close();
            }
        }
        protected override void DataPortal_Update()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroConnectionUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@LibraryID", LibraryId == "" ? DBNull.Value : LibraryId));
                    command.Parameters.Add(new SqlParameter("@ZoteroUserId", ZoteroUserId));
                    command.Parameters.Add(new SqlParameter("@ApiKey", ApiKey));
                    command.Parameters.Add(new SqlParameter("@USER_ID", ReadProperty(USER_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        protected override void DataPortal_DeleteSelf()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroConnectionDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ApiKey", ApiKey));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
#endif
    }
}
