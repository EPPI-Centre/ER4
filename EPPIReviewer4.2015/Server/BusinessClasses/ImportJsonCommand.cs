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
using BusinessLibrary.BusinessClasses.ImportItems;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ImportJsonCommand : CommandBase<ImportJsonCommand>
    {


        public ImportJsonCommand() { }

        private string _path = "";
        private string _message = "";
        private string _ReportContent = "";
        private string _ImportWhat = "";
        private string _SourceName = "";

        public static readonly PropertyInfo<ReviewSetsList> ReviewSetsProperty = RegisterProperty<ReviewSetsList>(new PropertyInfo<ReviewSetsList>("ReviewSets", "ReviewSets"));
        public ReviewSetsList ReviewSets
        {
            get { return ReadProperty(ReviewSetsProperty); }
            set { LoadProperty(ReviewSetsProperty, value); }
        }

        public string ReturnMessage
        {
            get { return _message; }
        }

        public ImportJsonCommand(string Path, string ImportWhat)
        {
            _path = Path;
            _ImportWhat = ImportWhat;
        }
        public ImportJsonCommand(string JSONReport, string ImportWhat, string SourceName)
        {
            _ReportContent = JSONReport;
            _ImportWhat = ImportWhat;
            _SourceName = SourceName;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_abstract", _path);
            info.AddValue("_message", _message);
            info.AddValue("_ReportContent", _ReportContent);
            info.AddValue("_ImportWhat", _ImportWhat);
            info.AddValue("_SourceName", _SourceName); 
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, StateMode mode)
        {
            _path = info.GetValue<string>("_abstract");
            _message = info.GetValue<string>("_message");
            _ReportContent = info.GetValue<string>("_ReportContent");
            _ImportWhat = info.GetValue<string>("_ImportWhat");
            _SourceName = info.GetValue<string>("_SourceName");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            if (_ImportWhat == "") _ImportWhat = "codesets, items, codes";
            Rootobject ro = null;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            try
            {
                if (_path != "")
                ro = JsonConvert.DeserializeObject<Rootobject>(File.ReadAllText(_path), new JsonSerializerSettings
                {
                    DateFormatString = "dd/MM/yyyy"
                });
                else if (_ReportContent != "")
                {
                    ro = JsonConvert.DeserializeObject<Rootobject>(_ReportContent, new JsonSerializerSettings
                    {
                        DateFormatString = "dd/MM/yyyy"
                    });
                }
                else
                {
                    _message = "Error: no data";
                    return;
                }
            }
            catch (Exception e)
            {
                _message = "Error: deserialize failed";
                return;
            }
            if (_ImportWhat.Contains("codesets"))
            {
                if (ReviewSets == null)
                {
                    DataPortal<ReviewSetsList> dp = new DataPortal<ReviewSetsList>();
                    this.ReviewSets = dp.Fetch();//this allows to import more items, AFTER having imported the coding tools in a previous import
                    if (ReviewSets == null) this.ReviewSets = new ReviewSetsList();
                }
                AddCodeSets(ro, ri.ReviewId, ri.UserId);
            }
            if (_ImportWhat.Contains("items"))
            {
                AddItems(ro, ri.ReviewId, ri.UserId);
            }
            if (_ImportWhat.Contains("codes"))
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
            foreach (Codeset CurrentCodeSet in ro.CodeSets)
            {
                ReviewSet rs = FindSetBySetId(CurrentCodeSet.SetId);
                if (rs == null) // it's not already in the review - we have to create it
                {
                    rs = CreateNewCodeSet(CurrentCodeSet, ReviewId);
                    CodeSetUsingOriginalId = true;
                    if (rs == null)
                    {
                        _message = "Could not create code set";
                        return;
                    }
                    this.ReviewSets.Add(rs);
                }
                if (rs != null && CurrentCodeSet.Attributes != null)
                {
                    int c = 0;
                    foreach (Attributeslist nested in CurrentCodeSet.Attributes.AttributesList)
                    {
                        AddNestedAttributes(rs, null, nested, ContactId, c);
                        c++;
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
                if (a.Attributes != null && a.Attributes.AttributesList != null)
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

        private ReviewSet FindSetBySetId(int SetId)
        {
            foreach (ReviewSet rs in this.ReviewSets)
            {
                if (rs.SetId == SetId)
                {
                    CodeSetUsingOriginalId = false;
                    return rs;
                }
            }
            foreach (ReviewSet rs in this.ReviewSets)
            {
                if (rs.OriginalSetId == SetId)
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
            if (CodeSetUsingOriginalId == true) // using the field OriginalSetId
            {
                aset = rs.GetAttributeSetFromOriginalAttributeId(alist.AttributeId);
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
                    if (cs.SetType != null && cs.SetType.SetTypeId > 0) command.Parameters.Add(new SqlParameter("@SET_TYPE_ID", cs.SetType.SetTypeId));
                    else command.Parameters.Add(new SqlParameter("@SET_TYPE_ID", 3)); // NOT PRESENT IN JSON
                    command.Parameters.Add(new SqlParameter("@ALLOW_CODING_EDITS", true));
                    command.Parameters.Add(new SqlParameter("@SET_NAME", cs.SetName));
                    command.Parameters.Add(new SqlParameter("@CODING_IS_FINAL", true));
                    command.Parameters.Add(new SqlParameter("@SET_ORDER", 0));
                    command.Parameters.Add(new SqlParameter("@SET_DESCRIPTION", cs.SetDescription));
                    command.Parameters.Add(new SqlParameter("@ORIGINAL_SET_ID", cs.SetId));

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
            if (a.AttributeTypeId > 0) NewAttributeSet.AttributeTypeId = a.AttributeTypeId;
            else NewAttributeSet.AttributeTypeId = a.AttributeType == "Not selectable (no checkbox)" ? 1 : 2; // NO AttributeTypeId in older json files
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
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", NewAttributeSet.AttributeTypeId)); 
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
        private IncomingItemsList IncomingItems = new IncomingItemsList();
        private ItemTypeNVL ItemTypes = null;
        private void AddItems(Rootobject ro, int ReviewId, int ContactId)
        {
            IncomingItems = IncomingItemsList.NewIncomingItemsList();
            IncomingItems.SourceName = _SourceName;
            IncomingItems.IncomingItems = new MobileList<ItemIncomingData>();
            IncomingItems.IsIncluded = true;
            IncomingItems.RetainItemsList = true;
            ItemTypeNVLFactory factory = new ItemTypeNVLFactory();
            ItemTypes = factory.FetchItemTypeNVL();
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
                    //SaveReference(ReviewId, ContactId, r);
                    AddReference(r);
                }
            }
            if (IncomingItems.IncomingItems != null && IncomingItems.IncomingItems.Count > 0)
            {
                int batchSize = 2000;
                if (IncomingItems.IncomingItems.Count > batchSize) SaveItemsInBatches(IncomingItems, batchSize);
                else IncomingItems = IncomingItems.Save();
                int index = 0;
                foreach (ItemIncomingData iid in IncomingItems.IncomingItems)
                {
                    //we rely on the two lists being in identical order!
                    ro.References[index].ItemId = iid.NewItemId;//the new item ID is used to build arms!
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        SaveArms(connection, ro.References[index]);
                    }
                    index++;
                }
                //SelectionCriteria crit = new SelectionCriteria();
                //crit.SourceId = IncomingItems.SourceID;
                //crit.PageSize = 4000;
                //crit.PageNumber = 1;
                //ItemList imported = new ItemList();
                //ItemList importedPage = DataPortal.Fetch<ItemList>(crit);
                //if (importedPage != null) imported.AddRange(importedPage.ToList());
                //if (importedPage != null && importedPage.PageCount > 1)
                //{
                //    while (crit.PageNumber <= importedPage.PageCount)
                //    {
                //        crit.PageNumber++;
                //        importedPage = DataPortal.Fetch<ItemList>(crit);
                //        if (importedPage != null) imported.AddRange(importedPage.ToList());
                //    }
                //}
                
            }
        }
        private void SaveItemsInBatches(IncomingItemsList BigList, int batchSize)
        {
            
            int BatchesCount = (int)Math.Ceiling((double)BigList.IncomingItems.Count / batchSize);
            int SourceId = 0;
            MobileList<ItemIncomingData> AllSavedItems = new MobileList<ItemIncomingData>();
            for (int i = 0; i < BatchesCount; i++)
            {
                IncomingItemsList SmallList = new IncomingItemsList();
                SmallList.IncomingItems = new MobileList<ItemIncomingData>();
                SmallList.IsFirst = false;
                SmallList.IsLast = false;
                SmallList.SourceName = BigList.SourceName;
                SmallList.IsIncluded = true;
                SmallList.RetainItemsList = true;
                SmallList.SourceID = SourceId;
                if (i == 0)
                {//1st batch
                    SmallList.IsFirst = true;
                    SmallList.IncomingItems.AddRange(BigList.IncomingItems.GetRange(0, batchSize));
                }
                else if (i == BatchesCount - 1)
                {//last batch
                    SmallList.IncomingItems.AddRange(BigList.IncomingItems.GetRange(i * batchSize, BigList.IncomingItems.Count - i * batchSize));
                    SmallList.IsLast = true;
                }
                else
                {//interim batches
                    SmallList.IncomingItems.AddRange(BigList.IncomingItems.GetRange(i * batchSize, batchSize));
                }
                SmallList = SmallList.Save();
                if (i == 0)
                { //we created the SourceRecord, so now we want to re-use it!
                    SourceId = SmallList.SourceID;
                }
                AllSavedItems.AddRange(SmallList.IncomingItems);
            }
            BigList.IncomingItems = AllSavedItems;
        }

        private void AddReference(Reference r)
        {
            ItemIncomingData itm = new ItemIncomingData();
            itm.Title = r.Title;
            //if (r.TypeName == "") { r.TypeName = "Journal, Article"; }
            var type = this.ItemTypes.FirstOrDefault(t => t.Value == r.TypeName);
            if (type != null && type.Key > 0)
            {
                itm.TypeId = type.Key;
            }
            else
            {
                type = this.ItemTypes.FirstOrDefault(t => t.Value == "Journal, Article");
                if (type != null && type.Key > 0)
                {
                    itm.TypeId = type.Key;
                }
                else if (ItemTypes.Count > 0 && ItemTypes[ItemTypes.Count - 1].Key > 0)
                {
                    itm.TypeId = ItemTypes[ItemTypes.Count - 1].Key;
                }
                else throw new Exception("Can't import items because there are no valid Item Types in the DB");
            }
            itm.Parent_title = r.ParentTitle;
            itm.Short_title = r.ShortTitle;
            itm.DateEdited = r.DateEdited.Ticks > new DateTime(1900, 1, 1).Ticks ? r.DateEdited : new DateTime(1900, 1, 1);
            itm.Year = r.Year;
            itm.Month = r.Month;
            itm.Standard_number = r.StandardNumber;
            itm.City = r.City; 
            itm.Country = r.Country;
            itm.Publisher = r.Publisher;
            itm.Institution = r.Institution;
            itm.Volume = r.Volume;
            itm.Pages = r.Pages;
            itm.Issue = r.Issue;
            itm.Availability = r.Availability;
            itm.Url = r.URL;
            itm.Comments = r.Comments;
            itm.Abstract = r.Abstract;
            itm.DOI = r.DOI;
            itm.Keywords = r.Keywords;
            itm.AuthorsLi = new AutorsList();
            itm.pAuthorsLi = new MobileList<AutH>();
            if (r.Authors != null && r.Authors.Length > 0) itm.AuthorsLi.AddRange(NormaliseAuth.processField(r.Authors, 0));
            if (r.ParentAuthors != null && r.ParentAuthors.Length > 0) itm.AuthorsLi.AddRange(NormaliseAuth.processField(r.ParentAuthors, 1));
            if (r.ItemId > 0)
            {
                itm.OldItemId = r.ItemId.ToString();
                if (r.OldItemId.Trim() != "") itm.Comments = "Older ID: " + r.OldItemId.Trim() + Environment.NewLine + itm.Comments;
            }
            else if (r.OldItemId != null && r.OldItemId != "")
            {
                itm.OldItemId = r.OldItemId; 
            }
            if (itm.Short_title == "") itm.buildShortTitle();
            this.IncomingItems.IncomingItems.Add(itm);
        }

        /// <summary>
        /// deprecated - doesn't allow to produce a source!
        /// </summary>
        /// <param name="ReviewId"></param>
        /// <param name="ContactId"></param>
        /// <param name="r"></param>
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

                    if (r.Outcomes != null)
                    {
                        foreach (Outcome o in r.Outcomes)
                        {
                            SaveOutcome(o, ReviewId, ContactId, r.ItemId);
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
                        command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", c.ArmId == 0 ? (object)DBNull.Value : c.ArmId));
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

        // **************************************************************************************************************************
        // ********************************************** adding outcomes ***********************************************************

        private void SaveOutcome(Outcome o, int ReviewId, int ContactId, Int64 ItemId)
        {
            //commented out: we try something a bit better below
            //// If there is > 1 codeset in the json, we don't know where to 'tie' the outcomes, so don't try to import them
            //if (ReviewSets.Count != 1)
            //{
            //    return;
            //}

            //this can still be wrong, when we have multiple "Standard" tools imported: we could end up trying to add outcomes to the wrong tool.
            ReviewSet rs = ReviewSets.FirstOrDefault(f=>f.SetTypeId == 3);
            if (rs == null) return;
            Int64 NewOutcomeId = 0;
            Int64 ItemSetId = 0;
            if (rs != null)
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command3 = new SqlCommand("st_ItemSetGetCompleted", connection))
                    {
                        command3.CommandType = System.Data.CommandType.StoredProcedure;
                        command3.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command3.Parameters.Add(new SqlParameter("@ITEM_ID", ItemId));
                        command3.Parameters.Add(new SqlParameter("@SET_ID", rs.SetId));
                        using (Csla.Data.SafeDataReader reader3 = new Csla.Data.SafeDataReader(command3.ExecuteReader()))
                        {
                            if (reader3.Read())
                                ItemSetId = Convert.ToInt64(reader3["ITEM_SET_ID"].ToString());
                        }
                    }

                    if (ItemSetId > 0)
                    {
                        AttributeSet InterventionId = rs.GetAttributeSetFromOriginalAttributeId(o.ItemAttributeIdIntervention);
                        AttributeSet ControlId = rs.GetAttributeSetFromOriginalAttributeId(o.ItemAttributeIdControl);
                        AttributeSet OutcomeId = rs.GetAttributeSetFromOriginalAttributeId(o.ItemAttributeIdOutcome);
                        using (SqlCommand command = new SqlCommand("st_OutcomeItemInsert", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", ItemSetId));
                            command.Parameters.Add(new SqlParameter("@OUTCOME_TYPE_ID", o.OutcomeTypeId));
                            command.Parameters.Add(new SqlParameter("@OUTCOME_TITLE", o.Title));
                            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_INTERVENTION", InterventionId == null ? 0 : InterventionId.AttributeId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_CONTROL", ControlId == null ? 0 : ControlId.AttributeId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_OUTCOME", OutcomeId == null ? 0 : OutcomeId.AttributeId));
                            command.Parameters.Add(new SqlParameter("@OUTCOME_DESCRIPTION", o.OutcomeDescription));
                            command.Parameters.Add(new SqlParameter("@DATA1", o.Data1));
                            command.Parameters.Add(new SqlParameter("@DATA2", o.Data2));
                            command.Parameters.Add(new SqlParameter("@DATA3", o.Data3));
                            command.Parameters.Add(new SqlParameter("@DATA4", o.Data4));
                            command.Parameters.Add(new SqlParameter("@DATA5", o.Data5));
                            command.Parameters.Add(new SqlParameter("@DATA6", o.Data6));
                            command.Parameters.Add(new SqlParameter("@DATA7", o.Data7));
                            command.Parameters.Add(new SqlParameter("@DATA8", o.Data8));
                            command.Parameters.Add(new SqlParameter("@DATA9", o.Data9));
                            command.Parameters.Add(new SqlParameter("@DATA10", o.Data10));
                            command.Parameters.Add(new SqlParameter("@DATA11", o.Data11));
                            command.Parameters.Add(new SqlParameter("@DATA12", o.Data12));
                            command.Parameters.Add(new SqlParameter("@DATA13", o.Data13));
                            command.Parameters.Add(new SqlParameter("@DATA14", o.Data14));
                            command.Parameters.Add(new SqlParameter("@ITEM_TIMEPOINT_ID", o.ItemTimepointId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID_GRP1", o.ItemArmIdGrp1));
                            command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID_GRP2", o.ItemArmIdGrp2));
                            command.Parameters.Add(new SqlParameter("@NEW_OUTCOME_ID", 0));
                            command.Parameters["@NEW_OUTCOME_ID"].Direction = System.Data.ParameterDirection.Output;
                            command.ExecuteNonQuery();
                            if (command.Parameters["@NEW_OUTCOME_ID"].Value != DBNull.Value)
                            {
                                NewOutcomeId = (Int64)command.Parameters["@NEW_OUTCOME_ID"].Value;
                            }
                        }

                        if (NewOutcomeId > 0)
                        {
                            string attributes = "";
                            if (o.OutcomeCodes != null && o.OutcomeCodes.OutcomeItemAttributesList != null)
                            {
                                foreach (Outcomeitemattributeslist a in o.OutcomeCodes.OutcomeItemAttributesList)
                                {
                                    if (attributes == "")
                                    {
                                        AttributeSet tmp = rs.GetAttributeSetFromOriginalAttributeId(a.AttributeId);
                                        if (tmp != null) attributes = tmp.AttributeId.ToString();
                                    }
                                    else
                                    {
                                        AttributeSet tmp = rs.GetAttributeSetFromOriginalAttributeId(a.AttributeId);
                                        if (tmp != null) attributes += "," + tmp.AttributeId.ToString();
                                    }
                                }
                                using (SqlCommand command = new SqlCommand("st_OutcomeItemAttributesSave", connection))
                                {
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.Parameters.Add(new SqlParameter("@OUTCOME_ID", NewOutcomeId));
                                    command.Parameters.Add(new SqlParameter("@ATTRIBUTES", attributes));
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    connection.Close();
                }
            }
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
            public int SetTypeId { get; set; }
        }

        public class Attributes
        {
            public Attributeslist[] AttributesList { get; set; }
        }

        public class Attributeslist
        {
            public Int64 AttributeSetId { get; set; }
            public Int64 AttributeId { get; set; }
            public Int64 OriginalAttributeID { get; set; }
            public string AttributeSetDescription { get; set; }
            public string AttributeType { get; set; }
            public int AttributeTypeId { get; set; } = 0;
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
            public Outcome[] Outcomes { get; set; }
        }

        public class Code
        {
            public Int64 AttributeId { get; set; }
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

        public class Outcome
        {
            public int OutcomeId { get; set; }
            public int ItemSetId { get; set; }
            public int OutcomeTypeId { get; set; }
            public string OutcomeTypeName { get; set; }
            public int ItemAttributeIdIntervention { get; set; }
            public int ItemAttributeIdControl { get; set; }
            public int ItemAttributeIdOutcome { get; set; }
            public string Title { get; set; }
            public string ShortTitle { get; set; }
            public string OutcomeDescription { get; set; }
            public float Data1 { get; set; }
            public float Data2 { get; set; }
            public float Data3 { get; set; }
            public float Data4 { get; set; }
            public float Data5 { get; set; }
            public float Data6 { get; set; }
            public float Data7 { get; set; }
            public float Data8 { get; set; }
            public float Data9 { get; set; }
            public float Data10 { get; set; }
            public float Data11 { get; set; }
            public float Data12 { get; set; }
            public float Data13 { get; set; }
            public float Data14 { get; set; }
            public string InterventionText { get; set; }
            public string ControlText { get; set; }
            public string OutcomeText { get; set; }
            public int ItemTimepointId { get; set; }
            public string ItemTimepointMetric { get; set; }
            public string ItemTimepointValue { get; set; }
            public int ItemArmIdGrp1 { get; set; }
            public int ItemArmIdGrp2 { get; set; }
            public string TimepointDisplayValue { get; set; }
            public string grp1ArmName { get; set; }
            public string grp2ArmName { get; set; }
            public Outcomecodes OutcomeCodes { get; set; }
            public float feWeight { get; set; }
            public float reWeight { get; set; }
            public float SMD { get; set; }
            public object SESMD { get; set; }
            public float R { get; set; }
            public float SER { get; set; }
            public float OddsRatio { get; set; }
            public float SEOddsRatio { get; set; }
            public float RiskRatio { get; set; }
            public float SERiskRatio { get; set; }
            public object CIUpperSMD { get; set; }
            public object CILowerSMD { get; set; }
            public float CIUpperR { get; set; }
            public float CILowerR { get; set; }
            public float CIUpperOddsRatio { get; set; }
            public float CILowerOddsRatio { get; set; }
            public float CIUpperRiskRatio { get; set; }
            public float CILowerRiskRatio { get; set; }
            public float CIUpperRiskDifference { get; set; }
            public float CILowerRiskDifference { get; set; }
            public float CIUpperPetoOddsRatio { get; set; }
            public float CILowerPetoOddsRatio { get; set; }
            public object CIUpperMeanDifference { get; set; }
            public object CILowerMeanDifference { get; set; }
            public float RiskDifference { get; set; }
            public float SERiskDifference { get; set; }
            public float MeanDifference { get; set; }
            public object SEMeanDifference { get; set; }
            public float PetoOR { get; set; }
            public float SEPetoOR { get; set; }
            public float ES { get; set; }
            public object SEES { get; set; }
            public int NRows { get; set; }
            public object CILower { get; set; }
            public object CIUpper { get; set; }
            public string ESDesc { get; set; }
            public string SEDesc { get; set; }
            public string Data1Desc { get; set; }
            public string Data2Desc { get; set; }
            public string Data3Desc { get; set; }
            public string Data4Desc { get; set; }
            public string Data5Desc { get; set; }
            public string Data6Desc { get; set; }
            public string Data7Desc { get; set; }
            public string Data8Desc { get; set; }
            public string Data9Desc { get; set; }
            public string Data10Desc { get; set; }
            public string Data11Desc { get; set; }
            public string Data12Desc { get; set; }
            public string Data13Desc { get; set; }
            public string Data14Desc { get; set; }
        }

        public class Outcomecodes
        {
            public Outcomeitemattributeslist[] OutcomeItemAttributesList { get; set; }
        }

        public class Outcomeitemattributeslist
        {
            public int OutcomeItemAttributeId { get; set; }
            public int OutcomeId { get; set; }
            public int AttributeId { get; set; }
            public string AdditionalText { get; set; }
            public string AttributeName { get; set; }
        }




    }
}
