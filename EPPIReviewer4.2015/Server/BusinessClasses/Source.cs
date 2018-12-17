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

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class Source : BusinessBase<Source>
    {
    public Source() { }
#region properties
        public static readonly PropertyInfo<string> Source_NameProperty = RegisterProperty<string>(new PropertyInfo<string>("Source_Name", "Source_Name"));
        public string Source_Name
        {
            get
            {
                return GetProperty(Source_NameProperty);
            }
            set
            {
                if (value.Length > 255) SetProperty(Source_NameProperty, value.Substring(0, 255));
                else SetProperty(Source_NameProperty, value);
            }
        }
        public static readonly PropertyInfo<DateTime> DateOfSerachProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("DateOfSerach", "DateOfSerach"));
        public DateTime DateOfSerach
        {
            get
            {
                return GetProperty(DateOfSerachProperty);
            }
            set
            {
                if (value == null) SetProperty(DateOfSerachProperty, DateTime.Now);
                else SetProperty(DateOfSerachProperty, value);
            }
        }
        public static readonly PropertyInfo<DateTime> DateOfImportProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("DateOfImport", "DateOfImport"));
        public DateTime DateOfImport
        {
            get
            {
                return GetProperty(DateOfImportProperty);
            }
            set
            {
                if (value == null) SetProperty(DateOfImportProperty, DateTime.Now);
                else SetProperty(DateOfImportProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SourceDataBaseProperty = RegisterProperty<string>(new PropertyInfo<string>("SourceDataBase", "SourceDataBase"));
        public string SourceDataBase
        {
            get
            {
                return GetProperty(SourceDataBaseProperty);
            }
            set
            {
                if (value.Length > 200) SetProperty(SourceDataBaseProperty, value.Substring(0, 200));
                else SetProperty(SourceDataBaseProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SearchDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchDescription", "SearchDescription"));
        public string SearchDescription
        {
            get
            {
                return GetProperty(SearchDescriptionProperty);
            }
            set
            {
                if (value.Length > 4000) SetProperty(SearchDescriptionProperty, value.Substring(0, 4000));
                else SetProperty(SearchDescriptionProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SearchStringProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchString", "SearchString"));
        public string SearchString
        {
            get
            {
                return GetProperty(SearchStringProperty);
            }
            set
            {
                SetProperty(SearchStringProperty, value);
            }
        }
        public static readonly PropertyInfo<string> NotesProperty = RegisterProperty<string>(new PropertyInfo<string>("Notes", "Notes"));
        public string Notes
        {
            get
            {
                return GetProperty(NotesProperty);
            }
            set
            {
                if (value.Length > 4000) SetProperty(NotesProperty, value.Substring(0, 4000));
                else SetProperty(NotesProperty, value);
            }
        }
        public static readonly PropertyInfo<string> ImportFilterProperty = RegisterProperty<string>(new PropertyInfo<string>("ImportFilter", "ImportFilter"));
        public string ImportFilter
        {
            get
            {
                return GetProperty(ImportFilterProperty);
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
        public static readonly PropertyInfo<int> Source_IDProperty = RegisterProperty<int>(new PropertyInfo<int>("Source_ID", "Source_ID"));
        public int Source_ID
        {
            get
            {
                return GetProperty(Source_IDProperty);
            }
        }
        public static readonly PropertyInfo<bool> IsFlagDeletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsFlagDeleted", "IsFlagDeleted"));
        public bool IsFlagDeleted
        {
            get
            {
                return GetProperty(IsFlagDeletedProperty);
            }
        }
        
        public static readonly PropertyInfo<int> CodesProperty = RegisterProperty<int>(new PropertyInfo<int>("Codes", "Codes"));
        public int Codes
        {
            get
            {
                return GetProperty(CodesProperty);
            }
        }
        public static readonly PropertyInfo<int> InductiveCodesProperty = RegisterProperty<int>(new PropertyInfo<int>("InductiveCodes", "InductiveCodes"));
        public int InductiveCodes
        {
            get
            {
                return GetProperty(InductiveCodesProperty);
            }
        }
        public static readonly PropertyInfo<int> AttachedFilesProperty = RegisterProperty<int>(new PropertyInfo<int>("AttachedFiles", "AttachedFiles"));
        public int AttachedFiles
        {
            get
            {
                return GetProperty(AttachedFilesProperty);
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
        public static readonly PropertyInfo<int> isMasterOfProperty = RegisterProperty<int>(new PropertyInfo<int>("isMasterOf", "isMasterOf"));
        public int isMasterOf
        {
            get
            {
                return GetProperty(isMasterOfProperty);
            }
        }
        public static readonly PropertyInfo<int> OutcomesProperty = RegisterProperty<int>(new PropertyInfo<int>("Outcomes", "Outcomes"));
        public int Outcomes
        {
            get
            {
                return GetProperty(OutcomesProperty);
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyItemAttribute), canRead);
        //    //AuthorizationRules.AllowRead(ItemAttributeIdProperty, canRead);
        //}
#endregion
#if !SILVERLIGHT
        protected override void DataPortal_Update()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (!ri.IsAuthenticated) return;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_Source_Update", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@s_ID", Source_ID));
                    command.Parameters.Add(new SqlParameter("@sDB", SourceDataBase));
                    command.Parameters.Add(new SqlParameter("@Name", Source_Name));
                    command.Parameters.Add(new SqlParameter("@DoS", DateOfSerach));
                    command.Parameters.Add(new SqlParameter("@DoI", DateOfImport));
                    command.Parameters.Add(new SqlParameter("@Descr", SearchDescription));
                    command.Parameters.Add(new SqlParameter("@s_Str", SearchString));
                    command.Parameters.Add(new SqlParameter("@Notes", Notes));
                    command.ExecuteNonQuery();
                }
            }
        }
        protected void DataPortal_Fetch(SingleCriteria<Source, int> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int RevID = ri.ReviewId;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SourceDetails", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@revID", System.Data.SqlDbType.Int);
                    command.Parameters["@revID"].Value = RevID;
                    command.Parameters.Add("@sourceID", System.Data.SqlDbType.Int);
                    command.Parameters["@sourceID"].Value = criteria.Value;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Child_Fetch(reader);
                        }
                    }
                }
                connection.Close();
            }
        }
        public static Source GetSource(SafeDataReader reader)
        {
            return DataPortal.FetchChild<Source>(reader);
        }

        private void Child_Fetch(SafeDataReader reader)
        {//CODES	IDUCTIVE_CODES	Attached Files	DUPLICATES	isMasterOf
            LoadProperty<string>(Source_NameProperty, reader.GetString("Source_Name"));
            LoadProperty<int>(Total_ItemsProperty, reader.GetInt32("Total_Items"));
            LoadProperty<int>(Deleted_ItemsProperty, reader.GetInt32("Deleted_Items"));
            LoadProperty<int>(Source_IDProperty, reader.GetInt32("Source_ID"));
            LoadProperty<bool>(IsFlagDeletedProperty, reader.GetBoolean("IS_DELETED"));
            LoadProperty<DateTime>(DateOfSerachProperty, reader.GetDateTime("DATE_OF_SEARCH"));
            LoadProperty<DateTime>(DateOfImportProperty, reader.GetDateTime("DATE_OF_IMPORT"));
            LoadProperty<string>(SourceDataBaseProperty, reader.GetString("SOURCE_DATABASE"));
            LoadProperty<string>(SearchDescriptionProperty, reader.GetString("SEARCH_DESCRIPTION"));
            LoadProperty<string>(SearchStringProperty, reader.GetString("SEARCH_STRING"));
            LoadProperty<string>(NotesProperty, reader.GetString("NOTES"));
            LoadProperty<string>(ImportFilterProperty, reader.GetString("IMPORT_FILTER"));
            LoadProperty<int>(CodesProperty, reader.GetInt32("CODES"));
            LoadProperty<int>(InductiveCodesProperty, reader.GetInt32("IDUCTIVE_CODES"));
            LoadProperty<int>(AttachedFilesProperty, reader.GetInt32("Attached Files"));
            LoadProperty<int>(DuplicatesProperty, reader.GetInt32("DUPLICATES"));
            LoadProperty<int>(isMasterOfProperty, reader.GetInt32("isMasterOf"));
            LoadProperty<int>(OutcomesProperty, reader.GetInt32("OUTCOMES"));
        }


#endif
    }
}
