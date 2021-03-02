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
using Csla.DataPortalClient;
using System.Threading;
using System.Threading.Tasks;
using Csla.Data;
using System.IO;
using Newtonsoft.Json.Linq;





#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Configuration;
using System.Data;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using AuthorsHandling;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ImportJsonCommand : CommandBase<ImportJsonCommand>
    {

#if SILVERLIGHT
    public ImportJsonCommand(){}
#else
        public ImportJsonCommand() { }
#endif
        private string _path;
        private string _message;

        private static PropertyInfo<ReviewSetsList> ReviewSetsProperty = RegisterProperty<ReviewSetsList>(new PropertyInfo<ReviewSetsList>("ReviewSets", "ReviewSets"));
        public ReviewSetsList ReviewSets
        {
            get { return ReadProperty(ReviewSetsProperty); }
            set { LoadProperty(ReviewSetsProperty, value); }
        }

        public string ReturnMessage
        {
            get { return _message; }
        }

        public ImportJsonCommand(string Path, string Message)
        {
            _path = Path;
            _message = Message;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_abstract", _path);
            info.AddValue("_message", _message);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _path = info.GetValue<string>("_abstract");
            _message = info.GetValue<string>("_message");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            _message = "codesets, items, codes";
            Rootobject ro = null;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            try
            {
                ro = JsonConvert.DeserializeObject<Rootobject>(File.ReadAllText(_path));
            }
            catch
            {
                _message = "deserialize error";
            }
            if (_message.Contains("codesets"))
            {
                AddCodeSets(ro, ri.ReviewId, ri.UserId);
            }
            if (_message.Contains("items"))
            {
                AddItems(ro, ri.ReviewId, ri.UserId);
            }
            if (_message.Contains("codes"))
            {
                AddCodes(ro, ri.ReviewId, ri.UserId);
            }
            //}
            //catch
            //{
            //    _message = "deserialize error";
            //}
            //bool result = Task.Run(() => Do(ri.ReviewId, ri.UserId)).GetAwaiter().GetResult();
        }

        //**********************************************************************************************************
        // This whole 'originalId' is confusing in this context! It is referring to the field 'original...Id' NOT
        // the set ID of the 'original' code set...

        // This section is about creating codesets and codes

        private bool CodeSetUsingOriginalId;

        private void AddCodeSets(Rootobject ro, int ReviewId, int ContactId)
        {
            foreach(Codeset CurrentCodeSet in ro.CodeSets)
            {
                ReviewSet rs = FindSetBySetId(CurrentCodeSet);
                if (rs == null) // it's not already in the review - we have to create it
                {
                    rs = CreateNewCodeSet(CurrentCodeSet, ReviewId);
                    CodeSetUsingOriginalId = true;
                    if (rs == null)
                    {
                        _message = "Could not create code set";
                        return;
                    }
                }
                this.ReviewSets.Add(rs);
                if (rs != null)
                {
                    foreach (Attributeslist nested in CurrentCodeSet.Attributes.AttributesList)
                    {
                        AddNestedAttributes(rs, null, nested, ContactId, 0);
                    }
                }
            }
        }

        private AttributeSet AddNestedAttributes(ReviewSet rs, AttributeSet aSet, Attributeslist a, int ContactId, int order)
        {
            AttributeSet NewAttributeSet = FindAttributeByAttributeId(a, rs);
            if (NewAttributeSet == null)
            {
                NewAttributeSet = CreateNewAttributeSet(rs, a, ContactId, order, aSet);
                a.AttributeId = NewAttributeSet.AttributeId;
                if (aSet == null)
                {
                    rs.Attributes.Add(NewAttributeSet);
                }
                else
                {
                    aSet.Attributes.Add(NewAttributeSet);
                }
            }
            if (NewAttributeSet != null)
            {
                int n = 0;
                if (a.Attributes != null)
                {
                    foreach (Attributeslist nested in a.Attributes.AttributesList)
                    {
                        AddNestedAttributes(rs, NewAttributeSet, nested, ContactId, n);
                        n++;
                    }
                }
            }
            return NewAttributeSet;
        }

        private ReviewSet FindSetBySetId(Codeset cs)
        {
            foreach (ReviewSet rs in this.ReviewSets)
            {
                if (rs.SetId == cs.SetId)
                {
                    CodeSetUsingOriginalId = false;
                    return rs;
                }
            }
            foreach (ReviewSet rs in this.ReviewSets)
            {
                if (rs.OriginalSetId == cs.SetId)
                {
                    CodeSetUsingOriginalId = true;
                    return rs;
                }
            }
            
            return null;
        }

        private AttributeSet FindAttributeByAttributeId(Attributeslist alist, ReviewSet rs)
        {
            AttributeSet aset = null;
            if (CodeSetUsingOriginalId == false) // using the field OriginalSetId
            {
                aset = rs.GetAttributeSetFromOriginalAttributeId(alist.OriginalAttributeID);
            }
            else
            {
                aset = rs.GetAttributeSetFromAttributeId(alist.AttributeId);
            }
            return aset;
        }

        private ReviewSet CreateNewCodeSet(Codeset cs, int ReviewId)
        {
            ReviewSet rs = null;
            int NewReviewSetId = 0;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@SET_TYPE_ID", 3)); // NOT PRESENT IN JSON
                    command.Parameters.Add(new SqlParameter("@ALLOW_CODING_EDITS", true));
                    command.Parameters.Add(new SqlParameter("@SET_NAME", cs.SetName));
                    command.Parameters.Add(new SqlParameter("@CODING_IS_FINAL", true));
                    command.Parameters.Add(new SqlParameter("@SET_ORDER", 0));
                    command.Parameters.Add(new SqlParameter("@SET_DESCRIPTION", cs.SetDescription));
                    command.Parameters.Add(new SqlParameter("@ORIGINAL_SET_ID", cs.OriginalSetId));

                    SqlParameter par = new SqlParameter("@NEW_REVIEW_SET_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_REVIEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;

                    SqlParameter par2 = new SqlParameter("@NEW_SET_ID", System.Data.SqlDbType.Int);
                    par2.Value = 0;
                    command.Parameters.Add(par2);
                    command.Parameters["@NEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    NewReviewSetId = Convert.ToInt32(command.Parameters["@NEW_REVIEW_SET_ID"].Value);
                    connection.Close();
                }
            }
            if (NewReviewSetId != 0)
            {
                rs = ReviewSet.GetReviewSet(NewReviewSetId);
            }
            return rs;
        }

        private AttributeSet CreateNewAttributeSet(ReviewSet rs, Attributeslist a, int ContactId, int order, AttributeSet aset)
        {
            AttributeSet NewAttributeSet = AttributeSet.NewAttributeSet();
            NewAttributeSet.AttributeDescription = a.AttributeDescription;
            NewAttributeSet.AttributeName = a.AttributeName;
            NewAttributeSet.AttributeOrder = order;
            NewAttributeSet.AttributeSetDescription = a.AttributeSetDescription;
            NewAttributeSet.AttributeSetId = rs.SetId;
            NewAttributeSet.AttributeType = a.AttributeType;
            NewAttributeSet.AttributeTypeId = a.AttributeType == "Not selectable (no checkbox)" ? 1 : 2; // NO AttributeTypeId in json file??
            NewAttributeSet.ContactId = ContactId;
            NewAttributeSet.ExtType = a.ExtType;
            NewAttributeSet.ExtURL = a.ExtURL;
            NewAttributeSet.OriginalAttributeID = a.AttributeId;
            NewAttributeSet.ParentAttributeId = aset == null ? 0 : aset.AttributeId;
            NewAttributeSet.SetId = rs.SetId;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSetInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SET_ID", rs.SetId));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", aset == null ? 0 : aset.AttributeId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", a.AttributeType == "Not selectable (no checkbox)" ? 1 : 2)); // NO AttributeTypeId in json file??
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_DESC", a.AttributeSetDescription));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", order));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_NAME", a.AttributeName));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_DESC", a.AttributeDescription));
                    command.Parameters.Add(new SqlParameter("@Ext_URL", a.ExtURL));
                    command.Parameters.Add(new SqlParameter("@Ext_Type", a.ExtType));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command.Parameters.Add(new SqlParameter("@ORIGINAL_ATTRIBUTE_ID", a.AttributeId));
                    command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_SET_ID", 0));
                    command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_ID", 0));
                    command.Parameters["@NEW_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    NewAttributeSet.AttributeSetId = Convert.ToInt64(command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Value);
                    NewAttributeSet.AttributeId = Convert.ToInt64(command.Parameters["@NEW_ATTRIBUTE_ID"].Value);
                }
                connection.Close();
            }
            return NewAttributeSet;
        }


        

        // ***********************************************************************************************************
        // Adding items

        private void AddItems(Rootobject ro, int ReviewId, int ContactId)
        {
            foreach (Reference r in ro.References)
            {
                Item i = null;
                using (SqlConnection connection3 = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection3.Open();
                    using (SqlCommand command3 = new SqlCommand("st_Item", connection3))
                    {
                        command3.CommandType = System.Data.CommandType.StoredProcedure;
                        command3.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command3.Parameters.Add(new SqlParameter("@ITEM_ID", r.ItemId));
                        using (Csla.Data.SafeDataReader reader3 = new Csla.Data.SafeDataReader(command3.ExecuteReader()))
                        {
                            if (reader3.Read())
                                i = Item.GetItem(reader3);
                        }
                    }
                    connection3.Close();
                }
                if (i == null)
                {
                    SaveReference(ReviewId, ContactId, r);
                }
            }
        }

        public void SaveReference(int ReviewId, int ContactId, Reference r)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", 0));
                    command.Parameters.Add(new SqlParameter("@TITLE", r.Title));
                    command.Parameters.Add(new SqlParameter("@TYPE_ID", 14)); // NOT IN THE JSON FILE. Could either add to the json, or add more here...
                    command.Parameters.Add(new SqlParameter("@PARENT_TITLE", r.ParentTitle));
                    command.Parameters.Add(new SqlParameter("@SHORT_TITLE", r.ShortTitle));
                    command.Parameters.Add(new SqlParameter("@DATE_CREATED", r.DateCreated));
                    command.Parameters.Add(new SqlParameter("@CREATED_BY", r.CreatedBy));
                    command.Parameters.Add(new SqlParameter("@DATE_EDITED", r.DateEdited.Ticks > new DateTime(1900, 1, 1).Ticks ?
                        r.DateEdited : new DateTime(1900, 1, 1)));
                    command.Parameters.Add(new SqlParameter("@EDITED_BY", r.EditedBy));
                    command.Parameters.Add(new SqlParameter("@YEAR", r.Year));
                    command.Parameters.Add(new SqlParameter("@MONTH", r.Month));
                    command.Parameters.Add(new SqlParameter("@STANDARD_NUMBER", r.StandardNumber));
                    command.Parameters.Add(new SqlParameter("@CITY", r.City));
                    command.Parameters.Add(new SqlParameter("@COUNTRY", r.Country));
                    command.Parameters.Add(new SqlParameter("@PUBLISHER", r.Publisher));
                    command.Parameters.Add(new SqlParameter("@INSTITUTION", r.Institution));
                    command.Parameters.Add(new SqlParameter("@VOLUME", r.Volume));
                    command.Parameters.Add(new SqlParameter("@PAGES", r.Pages));
                    command.Parameters.Add(new SqlParameter("@EDITION", r.Edition));
                    command.Parameters.Add(new SqlParameter("@ISSUE", r.Issue));
                    command.Parameters.Add(new SqlParameter("@IS_LOCAL", true));
                    command.Parameters.Add(new SqlParameter("@AVAILABILITY", r.Availability));
                    command.Parameters.Add(new SqlParameter("@URL", r.URL));
                    command.Parameters.Add(new SqlParameter("@COMMENTS", r.Comments));
                    command.Parameters.Add(new SqlParameter("@ABSTRACT", r.Abstract));
                    command.Parameters.Add(new SqlParameter("@DOI", r.DOI));
                    command.Parameters.Add(new SqlParameter("@KEYWORDS", r.Keywords));
                    command.Parameters.Add(new SqlParameter("@IS_INCLUDED", true));
                    command.Parameters.Add(new SqlParameter("@OLD_ITEM_ID", r.ItemId));
                    command.Parameters["@ITEM_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));

                    command.ExecuteNonQuery();
                    r.ItemId = Convert.ToInt64(command.Parameters["@ITEM_ID"].Value);

                    SaveAuthors(connection, r);
                }
                connection.Close();
            }
        }

        protected void SaveAuthors(SqlConnection connection, Reference r)
        {
            MobileList<AutH> AuthorLi = new MobileList<AutH>();
            if (r.Authors != null && r.Authors.Length > 0) AuthorLi.AddRange(NormaliseAuth.processField(r.Authors, 0));
            if (r.ParentAuthors != null && r.ParentAuthors.Length > 0) AuthorLi.AddRange(NormaliseAuth.processField(r.ParentAuthors, 1));
            using (SqlCommand command = new SqlCommand("st_ItemAuthorDelete", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@ITEM_ID", r.ItemId));
                command.ExecuteNonQuery();
            }
            foreach (AutH a in AuthorLi)
            {
                if (a.LastName == "") continue;
                using (SqlCommand command = new SqlCommand("st_ItemAuthorUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", r.ItemId));
                    command.Parameters.Add(new SqlParameter("@RANK", a.Rank));
                    command.Parameters.Add(new SqlParameter("@ROLE", a.Role));
                    command.Parameters.Add(new SqlParameter("@LAST", a.LastName));
                    command.Parameters.Add(new SqlParameter("@FIRST", a.FirstName));
                    command.Parameters.Add(new SqlParameter("@SECOND", a.MiddleName));
                    command.ExecuteNonQuery();
                }
            }
            SaveArms(connection, r);
        }

        protected void SaveArms(SqlConnection connection, Reference r)
        {
            int armIndex = 0;
            Dictionary<int, int> arms = new Dictionary<int, int>();
            if (r.Codes != null)
            {
                foreach (Code c in r.Codes)
                {
                    if (c.ArmId != 0)
                    {
                        int ArmId = 0;
                        if (arms.TryGetValue(c.ArmId, out ArmId))
                        {
                            c.ArmId = ArmId;
                        }
                        else
                        {
                            using (SqlCommand command = new SqlCommand("st_ItemArmCreate", connection))
                            {
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@ITEM_ID", r.ItemId));
                                command.Parameters.Add(new SqlParameter("@ARM_NAME", c.ArmTitle));
                                command.Parameters.Add(new SqlParameter("@ORDERING", armIndex));
                                command.Parameters.Add(new SqlParameter("@NEW_ITEM_ARM_ID", 0));
                                command.Parameters["@NEW_ITEM_ARM_ID"].Direction = System.Data.ParameterDirection.Output;
                                command.ExecuteNonQuery();
                                arms.Add(c.ArmId, Convert.ToInt32(command.Parameters["@NEW_ITEM_ARM_ID"].Value));
                                c.ArmId = Convert.ToInt32(command.Parameters["@NEW_ITEM_ARM_ID"].Value);
                                armIndex++;
                            }
                        }
                    }
                }
            }
        }

        // *****************************************************************************************
        // *************** adding item_attributes

        private void AddCodes(Rootobject ro, int ReviewId, int ContactId)
        {
            foreach (Reference r in ro.References)
            {
                Item i = null;
                // this isn't really necessary, but is a double-check that the specified item exists in this review
                // the item IDs will have been set in the Rootobject in the previous step
                using (SqlConnection connection3 = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection3.Open();
                    using (SqlCommand command3 = new SqlCommand("st_Item", connection3))
                    {
                        command3.CommandType = System.Data.CommandType.StoredProcedure;
                        command3.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command3.Parameters.Add(new SqlParameter("@ITEM_ID", r.ItemId));
                        using (Csla.Data.SafeDataReader reader3 = new Csla.Data.SafeDataReader(command3.ExecuteReader()))
                        {
                            if (reader3.Read())
                                i = Item.GetItem(reader3);
                        }
                    }
                    connection3.Close();
                } 
                if (i != null)
                {
                    if (r.Codes != null)
                    {
                        foreach (Code c in r.Codes)
                        {
                            SaveAttribute(c, ReviewId, ContactId, r.ItemId);
                        }
                    }
                }
            }
        }

        private Int64 SaveAttribute(Code c, int ReviewId, int ContactId, Int64 ItemId)
        {
            Int64 ItemAttributeId = 0;
            AttributeSet aset = this.ReviewSets.GetAttributeSetFromOriginalAttributeId(c.AttributeId);
            if (aset != null)
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ItemAttributeInsert", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                        command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", c.AdditionalText));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", aset.AttributeId));
                        command.Parameters.Add(new SqlParameter("@SET_ID", aset.SetId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", ItemId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID",  c.ArmId == 0 ? (object)DBNull.Value :c.ArmId));
                        command.Parameters.Add(new SqlParameter("@NEW_ITEM_ATTRIBUTE_ID", 0));
                        command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                        command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        if (command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value != DBNull.Value)
                        {
                            ItemAttributeId = (Int64)command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value;
                        }
                        Int64 _itemSetId = (Int64)command.Parameters["@NEW_ITEM_SET_ID"].Value;
                    }
                    connection.Close();
                }
            }
            return ItemAttributeId;
        }
        

#endif

        public class Rootobject
        {
            public Codeset[] CodeSets { get; set; }
            public Reference[] References { get; set; }
        }

        public class Codeset
        {
            public string SetName { get; set; }
            public int ReviewSetId { get; set; }
            public int SetId { get; set; }
            public Settype SetType { get; set; }
            public string SetDescription { get; set; }
            public Attributes Attributes { get; set; }
            public int OriginalSetId { get; set; }
        }

        public class Settype
        {
            public string SetTypeName { get; set; }
            public string SetTypeDescription { get; set; }
        }

        public class Attributes
        {
            public Attributeslist[] AttributesList { get; set; }
        }

        public class Attributeslist
        {
            public int AttributeSetId { get; set; }
            public Int64 AttributeId { get; set; }
            public int OriginalAttributeID { get; set; }
            public string AttributeSetDescription { get; set; }
            public string AttributeType { get; set; }
            public string AttributeName { get; set; }
            public string AttributeDescription { get; set; }
            public string ExtURL { get; set; }
            public string ExtType { get; set; }
            public Attributes Attributes { get; set; }
        }

        public class Reference
        {
            public string QuickCitation { get; set; }
            public Int64 ItemId { get; set; }
            public string Title { get; set; }
            public string ParentTitle { get; set; }
            public string ShortTitle { get; set; }
            public DateTime DateCreated { get; set; }
            public string CreatedBy { get; set; }
            public DateTime DateEdited { get; set; }
            public string EditedBy { get; set; }
            public string Year { get; set; }
            public string Month { get; set; }
            public string StandardNumber { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string Publisher { get; set; }
            public string Institution { get; set; }
            public string Volume { get; set; }
            public string Pages { get; set; }
            public string Edition { get; set; }
            public string Issue { get; set; }
            public string Availability { get; set; }
            public string URL { get; set; }
            public string OldItemId { get; set; }
            public string Abstract { get; set; }
            public string Comments { get; set; }
            public string TypeName { get; set; }
            public string Authors { get; set; }
            public string ParentAuthors { get; set; }
            public string DOI { get; set; }
            public string Keywords { get; set; }
            public string ItemStatus { get; set; }
            public string ItemStatusTooltip { get; set; }
            public Code[] Codes { get; set; }
        }

        public class Code
        {
            public int AttributeId { get; set; }
            public string AdditionalText { get; set; }
            public int ArmId { get; set; }
            public string ArmTitle { get; set; }
            public Itemattributefulltextdetail[] ItemAttributeFullTextDetails { get; set; }
        }

        public class Itemattributefulltextdetail
        {
            public int ItemDocumentId { get; set; }
            public int TextFrom { get; set; }
            public int TextTo { get; set; }
            public string Text { get; set; }
            public bool IsFromPDF { get; set; }
            public string DocTitle { get; set; }
            public string ItemArm { get; set; }
        }

    }
}
