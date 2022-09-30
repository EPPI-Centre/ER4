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

        public ZoteroERWebItemDocument(long item_Document_Id,string documentTitle, string doc_Zotero_Key, DocumentSyncState documentSyncState) {

            Item_Document_Id = item_Document_Id;
            DOCUMENT_TITLE = documentTitle;
            Doc_Zotero_Key = doc_Zotero_Key;
            SyncState = documentSyncState;
        }

        public static readonly PropertyInfo<long> Item_Document_IdProperty = RegisterProperty<long>(new PropertyInfo<long>("Item_Document_Id", "Item_Document_Id", 0m));
        public long Item_Document_Id
        {
            get
            {
                return GetProperty(Item_Document_IdProperty);
            }
            set
            {
                SetProperty(Item_Document_IdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> DOCUMENT_TITLEProperty = RegisterProperty<string>(new PropertyInfo<string>("DOCUMENT_TITLE", "DOCUMENT_TITLE", ""));
        public string DOCUMENT_TITLE
        {
            get
            {
                return GetProperty(DOCUMENT_TITLEProperty);
            }
            set
            {
                SetProperty(DOCUMENT_TITLEProperty, value);
            }
        }

        public static readonly PropertyInfo<string> Doc_Zotero_KeyProperty = RegisterProperty<string>(new PropertyInfo<string>("Doc_Zotero_Key", "Doc_Zotero_Key", ""));
        public string Doc_Zotero_Key
        {
            get
            {
                return GetProperty(Doc_Zotero_KeyProperty);
            }
            set
            {
                SetProperty(Doc_Zotero_KeyProperty, value);
            }
        }

        public static readonly PropertyInfo<DocumentSyncState> DocumentSyncStateProperty = RegisterProperty<DocumentSyncState>(new PropertyInfo<DocumentSyncState>("DocumentSyncState", "DocumentSyncState", DocumentSyncState.existsOnlyOnER));
        public DocumentSyncState SyncState
        {
            get
            {
                return GetProperty(DocumentSyncStateProperty);
            }
            set
            {
                SetProperty(DocumentSyncStateProperty, value);
            }
        }

        public enum DocumentSyncState
        {
            existsOnlyOnER,
            existsOnlyOnZotero,
            upToDate
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
#endif
    }
}