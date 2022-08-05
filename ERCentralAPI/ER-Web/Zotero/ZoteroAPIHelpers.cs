using ERxWebClient2.Controllers;
using ERxWebClient2.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ERxWebClient2.Zotero
{

    public class SQLReviewItem
    {
        public long ITEM_ID { get; set; }

        public int REVIEW_ID { get; set; }
        public int TYPE_ID { get; set; }
        public string Authors { get; set; }
        public string Title { get; set; }
        public string DATE_CREATED { get; set; }
        public string DATE_EDITED { get; set; }

        public SQLReviewItem(long rITEM_ID, int rReviewID, int rTYPE_ID, string rAuthors, string rTitle, string rDATE_CREATED, string rDATE_EDITED)
        {
            this.ITEM_ID = rITEM_ID;
            this.REVIEW_ID = rReviewID;
            this.TYPE_ID = rTYPE_ID;
            this.Authors = rAuthors;
            this.Title = rTitle;
            this.DATE_CREATED = rDATE_CREATED;
            this.DATE_EDITED = rDATE_EDITED;
        }
    }

    public class SQLZoteroReviewItem
    {
        public string KEY { get; set; }
        public long LIBRARY_ID { get; set; }
        public long ITEM_ID { get; set; }
        public int REVIEW_ID { get; set; }

        public long VERSION { get; set; }

        public string LAST_MODIFIED { get; set; }


        public SQLZoteroReviewItem(string rKey, long rLibraryID, long rItemID, int rReviewID, long rVersion, string rLastModified)
        {
            this.KEY = rKey;
            this.LIBRARY_ID = rLibraryID;
            this.ITEM_ID = rItemID;
            this.REVIEW_ID = rReviewID;
            this.VERSION = rVersion;
            this.LAST_MODIFIED = rLastModified;
        }
    }

    public static class ZoteroAPIHelpers
    {
        public static async Task InsertDataTableintoSQLTableusingSQLBulkCopyAsync()
        {
            DataTable libraryData = await GetLibraryData();

            using (SqlConnection sqlconnection = new SqlConnection(@"Data Source=DESKTOP-PPK08OI; 
                    Initial Catalog=Reviewer; Integrated Security=SSPI;"))
            {
                sqlconnection.Open();

                var libkey = libraryData.Rows[0]["LibraryKey"];
                var libraryId = libraryData.Rows[0]["LibraryId"];
                var reviewId = libraryData.Rows[0]["ReviewId"];

                // table exists already
                SqlCommand cmd = new SqlCommand("INSERT INTO TB_Review_Library_Zotero (LibraryKey, LibraryId, ReviewId) VALUES('" + libkey + "', " + libraryId + "," + reviewId + ");", sqlconnection);
                cmd.ExecuteNonQuery();
                sqlconnection.Close();
            }
        }

        public static object GetPropertyValue(object SourceData, string propName)
        {
            return SourceData.GetType().GetProperty(propName).GetValue(SourceData, null);
        }

        public static List<SQLReviewItem> GetCurrentReviewItems()
        {

            var Authors = "First Item authors";
            var Review_ID = 7;
            var ITEM_ID = 1122;
            var Title = "first item";
            var TYPE_ID = 1;
            var DATE_CREATED = "19/08/2020";
            var DATE_EDITED = "19/01/2021";

            var firstItem = new SQLReviewItem(ITEM_ID, Review_ID, TYPE_ID, Authors, Title, DATE_CREATED, DATE_EDITED);


            //var secondItem = new ReviewItem();

            //secondItem.Authors = "secondItem authors";
            //secondItem.ITEM_ID = 2;
            //secondItem.Title = "secondItem";
            //secondItem.TYPE_ID = 2;
            //secondItem.DATE_CREATED = "19/08/2021";
            //secondItem.DATE_EDITED = "19/08/2021";

            var reviewItems = new List<SQLReviewItem>();
            reviewItems.Add(firstItem);
            //reviewItems.Add(secondItem);
            return reviewItems;
        }

        public static List<SQLZoteroReviewItem> GetZoteroReviewItems()
        {
            var Key = "CollectionKey";
            var LibraryID = 1234;
            var ItemID = 1122;
            var ReviewID = 7;
            var version = 22;
            var lastModified = "10/2/2021";

            var firstItem = new SQLZoteroReviewItem(Key, LibraryID, ItemID, ReviewID, version, lastModified);

            var reviewItems = new List<SQLZoteroReviewItem>();
            reviewItems.Add(firstItem);
            return reviewItems;
        }

        public static UriBuilder GetCollectionsUri;
        public static UriBuilder GetItemsUri;
        private static string baseUrl = "https://api.zotero.org";
		private static ZoteroService _zoteroService;

		private static async Task<DataTable> GetLibraryData()
        {
            // this needs to get the data from Zotero where the library already exists
            //then just get the fields we need
            // get actual data here for practise
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl),
            };
            httpClient.DefaultRequestHeaders.Add("Zotero-API-Version", "3");
            httpClient.DefaultRequestHeaders.Add("Zotero-API-Key", "OHGNAiEP08rRKmZBgUNWsNrn");
            GetCollectionsUri = new UriBuilder($"{baseUrl}/users/8317548/collections");
            HttpClientProvider httpClientProvider = new HttpClientProvider(httpClient);
            _zoteroService.SetZoteroServiceHttpProvider(httpClientProvider);

            var collections = await _zoteroService.GetCollections<Collection>(GetCollectionsUri.ToString());
            var dataLibraryOne = collections[0].data;
            var collectionKey = dataLibraryOne.key;
            var libraryId = collections[0].library.id;

            DataTable reviewZoteroTable = new DataTable();
            reviewZoteroTable.TableName = "TB_Review_Library_Zotero";
            reviewZoteroTable.Columns.Add(new DataColumn("LibraryKey", typeof(String)));
            reviewZoteroTable.Columns.Add(new DataColumn("LibraryId", typeof(long)));
            reviewZoteroTable.Columns.Add(new DataColumn("ReviewId", typeof(long)));

            DataRow dataRow = reviewZoteroTable.NewRow();
            dataRow["LibraryKey"] = collectionKey;
            dataRow["LibraryId"] = libraryId;
            dataRow["ReviewId"] = 7;

            reviewZoteroTable.Rows.Add(dataRow);

            return reviewZoteroTable;
        }

        public static async Task<bool> GetLibraryExists()
        {
            int libraryId = 0;
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl),
            };
            httpClient.DefaultRequestHeaders.Add("Zotero-API-Version", "3");
            httpClient.DefaultRequestHeaders.Add("Zotero-API-Key", "OHGNAiEP08rRKmZBgUNWsNrn");
            GetCollectionsUri = new UriBuilder($"{baseUrl}/users/8317548/collections");
            HttpClientProvider httpClientProvider = new HttpClientProvider(httpClient);
            _zoteroService.SetZoteroServiceHttpProvider(httpClientProvider);

            var collections = await _zoteroService.GetCollections<Collection>(GetCollectionsUri.ToString());
            var dataLibraryOne = collections[0].data;
            var collectionKey = dataLibraryOne.key;
            libraryId = collections[0].library.id;

            if (libraryId > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static string GetMD5HashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string GetMD5HashFromStream(Stream stream)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(stream);
            stream.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
