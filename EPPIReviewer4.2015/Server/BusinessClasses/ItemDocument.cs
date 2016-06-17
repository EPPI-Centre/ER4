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
using Csla.DataPortalClient;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemDocument : BusinessBase<ItemDocument>
    {
#if SILVERLIGHT
    public ItemDocument() { }

        
#else
        private ItemDocument() { }
#endif

        public override string ToString()
        {
            return Title;
        }

        private static PropertyInfo<Int64> ItemDocumentIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemDocumentId", "ItemDocumentId"));
        public Int64 ItemDocumentId
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

        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId 
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
            set
            {
                SetProperty(ItemIdProperty, value);
            }
        }

        private static PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle", "ShortTitle", string.Empty));
        public string ShortTitle
        {
            get
            {
                return GetProperty(ShortTitleProperty);
            }
            set
            {
                SetProperty(ShortTitleProperty, value);
            }
        }

        private static PropertyInfo<string> ExtensionProperty = RegisterProperty<string>(new PropertyInfo<string>("Extension", "Extension", string.Empty));
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

        private static PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
            set
            {
                SetProperty(TitleProperty, value);
            }
        }

        private static PropertyInfo<string> TextProperty = RegisterProperty<string>(new PropertyInfo<string>("Text", "Text", string.Empty));
        public string Text
        {
            get
            {
                return GetProperty(TextProperty);
            }
            set
            {
                SetProperty(TextProperty, value);
            }
        }

        private static PropertyInfo<bool> BinaryExistsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("BinaryExists", "BinaryExists"));
        public bool BinaryExists
        {
            get
            {
                return GetProperty(BinaryExistsProperty);
            }
            set
            {
                SetProperty(BinaryExistsProperty, value);
            }
        }

        private static PropertyInfo<int> TextFromProperty = RegisterProperty<int>(new PropertyInfo<int>("TextFrom", "TextFrom"));
        public int TextFrom
        {
            get
            {
                return GetProperty(TextFromProperty);
            }
            set
            {
                SetProperty(TextFromProperty, value);
            }
        }

        private static PropertyInfo<int> TextToProperty = RegisterProperty<int>(new PropertyInfo<int>("TextTo", "TextTo"));
        public int TextTo
        {
            get
            {
                return GetProperty(TextToProperty);
            }
            set
            {
                SetProperty(TextToProperty, value);
            }
        }
        private static PropertyInfo<byte[]> FreeNotesStreamProperty = RegisterProperty<byte[]>(new PropertyInfo<byte[]>("FreeNotesStream", "FreeNotesStream"));
         //<summary>
         //get and set the notes for Silverdox in the Silverdox Byte[] format, this field uses the same underlying data as FreeNotesXML, and is what Silverdox uses to load and save notes on client side.
         // </summary>
        public byte[] FreeNotesStream
        {
            get
            {
                return GetProperty(FreeNotesStreamProperty);
            }
            set
            {
                SetProperty(FreeNotesStreamProperty, value);
            }
        }
         //<summary>
         //get and set the notes for Silverdox in the Silverdox XML format, this field uses the same underlying data as FreeNotesStream, and is what gets saved on the server.
         //</summary>
        public string FreeNotesXML
        {
            get
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                return encoder.GetString(FreeNotesStream, 0, FreeNotesStream.Length);
            }
            set
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                byte[] data = encoder.GetBytes(value);
                this.LoadProperty<byte[]>(FreeNotesStreamProperty, data);
                this.MarkDirty();
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ItemDocument), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ItemDocument), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ItemDocument), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ItemDocument), canRead);

        //    //AuthorizationRules.AllowRead(ItemDocumentIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(TitleProperty, canRead);

        //    //AuthorizationRules.AllowWrite(TitleProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    /*
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_TYPE_ID", 3)); // All set to review specific keywords atm
                    command.Parameters.Add(new SqlParameter("@ALLOW_CODING_EDITS", ReadProperty(AllowCodingEditsProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_NAME", ReadProperty(SetNameProperty)));
                    command.Parameters.Add(new SqlParameter("@CODING_IS_FINAL", ReadProperty(CodingIsFinalProperty)));

                    command.Parameters.Add(new SqlParameter("@NEW_REVIEW_SET_ID", 0));
                    command.Parameters["@NEW_REVIEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_SET_ID", 0));
                    command.Parameters["@NEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ItemDocumentIdProperty, command.Parameters["@NEW_REVIEW_SET_ID"].Value);
                    LoadProperty(SetIdProperty, command.Parameters["@NEW_SET_ID"].Value);
                    */
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_DOCUMENT_ID", ReadProperty(ItemDocumentIdProperty)));
                    command.Parameters.Add(new SqlParameter("@DOCUMENT_TITLE", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@DOCUMENT_FREE_NOTES", FreeNotesXML));
                    
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocumentDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", ReadProperty(ItemDocumentIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        protected void DataPortal_Fetch(SingleCriteria<ItemDocument, Int64> criteria) // used to return a specific item
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDocument", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<Int64>(ItemDocumentIdProperty, reader.GetInt64("ITEM_DOCUMENT_ID"));
                            LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static ItemDocument GetItemDocument(SafeDataReader reader)
        {
            ItemDocument returnValue = new ItemDocument();
            returnValue.LoadProperty<Int64>(ItemDocumentIdProperty, reader.GetInt64("ITEM_DOCUMENT_ID"));
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("DOCUMENT_TITLE"));
            returnValue.LoadProperty<string>(ExtensionProperty, reader.GetString("DOCUMENT_EXTENSION"));
            returnValue.LoadProperty<string>(TextProperty, reader.GetString("DOCUMENT_TEXT"));
            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.LoadProperty<bool>(BinaryExistsProperty, reader.GetString("DOC_BINARY") == "False" ? false : true);
            returnValue.LoadProperty<int>(TextFromProperty, 0);
            returnValue.LoadProperty<int>(TextToProperty, 0);
            returnValue.FreeNotesXML = reader.GetString("DOCUMENT_FREE_NOTES");
            
            //comment this:
            //const string illegalXmlChars = @"[\uDE00-\uDEFF]";
            //System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(illegalXmlChars);
            //System.Text.RegularExpressions.Match match = regex.Match(returnValue.Text);
            //int ind = match.Index;
            //regex = new System.Text.RegularExpressions.Regex("[\uDC00-\uDFFF]");
            // match = regex.Match(returnValue.Text);
            // int inde = match.Index;
            //string aaa = returnValue.Text.Substring(ind -10, 20);
            //string aaab = returnValue.Text.Substring(inde - 10, 20);
            //if (regex.IsMatch(returnValue.Text))
            //{
            //    returnValue.Text = regex.Replace(returnValue.Text, "¬!¬");
            //}
            //end of commented block
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
