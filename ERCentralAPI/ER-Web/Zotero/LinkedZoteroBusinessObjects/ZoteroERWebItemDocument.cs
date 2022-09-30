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

#if SILVERLIGHT
       public ZoteroERWebItemDocument() { }

        
#else
        public ZoteroERWebItemDocument(long item_Document_Id, string doc_Zotero_Key, DocumentSyncState documentSyncState) {

            Item_Document_Id = item_Document_Id;
            Doc_Zotero_Key = doc_Zotero_Key;
            SyncState = documentSyncState;
        }
#endif

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

        public static readonly PropertyInfo<int> DocumentSyncStateProperty = RegisterProperty<int>(new PropertyInfo<int>("DocumentSyncState", "DocumentSyncState", 0));
        public int SyncState
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

    //SERGIO: was trying to this with parameter because of the compilation error
    //internal class ZoteroERWebItemDocumentSelectionCriteria
    //{
    //    private Type type;
    //    private long itemDocumentId;
    //    private string docZoteroKey;

    //    public ZoteroERWebItemDocumentSelectionCriteria(Type type, long itemDocumentId, string docZoteroKey)
    //    {
    //        LoadProperty(ComparisonIdProperty, comparisonId);
    //        LoadProperty(ParentAttributeIdProperty, parentAttributeId);
    //        LoadProperty(SetIdProperty, setId);
    //    }
    //}
}