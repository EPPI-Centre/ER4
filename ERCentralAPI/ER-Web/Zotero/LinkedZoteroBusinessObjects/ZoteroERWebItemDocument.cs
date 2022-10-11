using Csla;
//using Csla.Configuration;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ZoteroERWebItemDocument : BusinessBase<ZoteroERWebItemDocument>
    {
        public ZoteroERWebItemDocument()
        {
                
        }

        public ZoteroERWebItemDocument(long item_Document_Id, string doc_Zotero_Key) {

            ItemDocumentId = item_Document_Id;
            DocZoteroKey = doc_Zotero_Key;
        }

        public static readonly PropertyInfo<long> ItemDocumentIdProperty = RegisterProperty<long>(new PropertyInfo<long>("ItemDocumentId", "ItemDocumentId", 0m));
        public long ItemDocumentId
        {
            get
            {
                return GetProperty(ItemDocumentIdProperty);
            }
            set
            {
                SetProperty(ItemDocumentIdProperty, value);
            }
        }


        public static readonly PropertyInfo<string> DocZoteroKeyProperty = RegisterProperty<string>(new PropertyInfo<string>("DocZoteroKey", "DocZoteroKey", ""));
        public string DocZoteroKey
        {
            get
            {
                return GetProperty(DocZoteroKeyProperty);
            }
            set
            {
                SetProperty(DocZoteroKeyProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ParentItemProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentItem", "ParentItem", ""));
        public string ParentItem
        {
            get
            {
                return GetProperty(ParentItemProperty);
            }
            set
            {
                SetProperty(ParentItemProperty, value);
            }
        }


        public static readonly PropertyInfo<DateTime> LAST_MODIFIEDProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("LAST_MODIFIED", "LAST_MODIFIED", DateTime.Now));
        public DateTime LAST_MODIFIED
        {
            get
            {
                return GetProperty(LAST_MODIFIEDProperty);
            }
            set
            {
                SetProperty(LAST_MODIFIEDProperty, value);
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

        public static readonly PropertyInfo<string> SimpleTextProperty = RegisterProperty<string>(new PropertyInfo<string>("SimpleText", "SimpleText", ""));
        public string SimpleText
        {
            get
            {
                return GetProperty(SimpleTextProperty);
            }
            set
            {
                SetProperty(SimpleTextProperty, value);
            }
        }

        public static readonly PropertyInfo<string> FileNameProperty = RegisterProperty<string>(new PropertyInfo<string>("FileName", "FileName", ""));
        public string FileName
        {
            get
            {
                return GetProperty(FileNameProperty);
            }
            set
            {
                SetProperty(FileNameProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ExtensionProperty = RegisterProperty<string>(new PropertyInfo<string>("Extension", "Extension", ""));
        public string Extension
        {
            get
            {
                return GetProperty(ExtensionProperty);
            }
            set
            {
                SetProperty(ExtensionProperty, value);
            }
        }


#if !SILVERLIGHT


        protected void DataPortal_Fetch(SingleCriteria<ZoteroERWebItemDocument, string> criteria)
        {
            //using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //{
            //    connection.Open();
            //    using (SqlCommand command = new SqlCommand("st_ItemInERWebANDZotero", connection))
            //    {
            //        command.CommandType = System.Data.CommandType.StoredProcedure;
            //        command.Parameters.Add(new SqlParameter("@ItemReviewId", criteria.Value));
            //        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
            //        {
            //            if (reader.Read())
            //            {
            //                LoadProperty<string>(ItemKeyProperty, reader.GetString("ItemKey"));
            //                MarkOld();
            //            }
            //        }
            //    }
            //    connection.Close();
            //}
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
                using (SqlCommand command = new SqlCommand("st_ZoteroItemDocumentCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ParentItem", ReadProperty(ParentItemProperty)));
                    command.Parameters.Add(new SqlParameter("@DocZoteroKey", ReadProperty(DocZoteroKeyProperty)));
                    command.Parameters.Add(new SqlParameter("@ItemDocumentId", ReadProperty(ItemDocumentIdProperty)));
                    command.Parameters.Add(new SqlParameter("@LAST_MODIFIED", ReadProperty(LAST_MODIFIEDProperty)));
                    command.Parameters.Add(new SqlParameter("@Version", ReadProperty(VersionProperty)));
                    command.Parameters.Add(new SqlParameter("@SimpleText", ReadProperty(SimpleTextProperty)));
                    command.Parameters.Add(new SqlParameter("@FileName", ReadProperty(FileNameProperty)));
                    command.Parameters.Add(new SqlParameter("@Extension", ReadProperty(ExtensionProperty)));
                    command.ExecuteNonQuery();

                }
                connection.Close();
            }
        }
#endif
    }
}