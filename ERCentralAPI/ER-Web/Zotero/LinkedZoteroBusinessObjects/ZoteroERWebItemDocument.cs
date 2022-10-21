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

        


        public static readonly PropertyInfo<string> documenT_TITLEProperty = RegisterProperty<string>(new PropertyInfo<string>("documenT_TITLE", "documenT_TITLE", ""));
        public string documenT_TITLE
        {
            get
            {
                return GetProperty(documenT_TITLEProperty);
            }
            set
            {
                SetProperty(documenT_TITLEProperty, value);
            }
        }

        public string Extension
        {
            get
            {
                string t = GetProperty(documenT_TITLEProperty);
                int i = t.LastIndexOf('.');
                if (i> -1)
                {
                    return t.Substring(i+1);
                }
                return "";
            }
        }

		

		public static readonly PropertyInfo<ZoteroERWebReviewItem.ErWebState> SyncStateProperty = RegisterProperty<ZoteroERWebReviewItem.ErWebState>(new PropertyInfo<ZoteroERWebReviewItem.ErWebState>("SyncState", "SyncState", ZoteroERWebReviewItem.ErWebState.notSet));
		public ZoteroERWebReviewItem.ErWebState SyncState
		{
			get
			{
				return GetProperty(SyncStateProperty);
			}
			set
			{
				SetProperty(SyncStateProperty, value);
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
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ZoteroItemDocumentCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@DocZoteroKey", ReadProperty(DocZoteroKeyProperty)));
                    command.Parameters.Add(new SqlParameter("@ItemDocumentId", ReadProperty(ItemDocumentIdProperty)));
                    command.ExecuteNonQuery();

                }
                connection.Close();
            }
        }


#endif
    }
}