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
using System.Globalization;
using BusinessLibrary.BusinessClasses.ImportItems;
using System.Text.RegularExpressions;


#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using AuthorsHandling;
using BusinessLibrary.Security;
using System.Data;
#if !CSLA_NETCORE
using System.Web.Hosting;
#endif

#endif


namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagImportFieldsOfStudyCommand : CommandBase<MagImportFieldsOfStudyCommand>
    {

#if SILVERLIGHT
    public MagImportFieldsOfStudyCommand(){}
#else
        public MagImportFieldsOfStudyCommand() { }
#endif

        private string _itemList;
        private Int64 _attributeIdFilter;
        private string _attributeList;
        private int _reviewSetIndex;
        private int _reviewSetId;
        private string _returnMessage;
        private int _numTopics;
        private int _minItems;
        private string _method;

        public string ReturnMessage
        {
            get
            {
                return _returnMessage;
            }
            set
            {
                _returnMessage = value;
            }
        }

        public MagImportFieldsOfStudyCommand(string itemList, Int64 attributeIdFilter, int reviewSetIndex, int ReviewSetId, int NumTopics, int MinItems, string method, string attributeList)
        {
            _itemList = itemList;
            _attributeIdFilter = attributeIdFilter;
            _attributeList = attributeList;
            _reviewSetIndex = reviewSetIndex;
            _reviewSetId = ReviewSetId;
            _numTopics = NumTopics;
            _minItems = MinItems;
            _method = method;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_itemList", _itemList);
            info.AddValue("_attributeList", _attributeList);
            info.AddValue("_attributeIdFilter", _attributeIdFilter);
            info.AddValue("_reviewSetIndex", _reviewSetIndex);
            info.AddValue("_reviewSetId", _reviewSetId);
            info.AddValue("_returnMessage", _returnMessage);
            info.AddValue("_numTopics", _numTopics);
            info.AddValue("_minItems", _minItems);
            info.AddValue("_method", _method);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _itemList = info.GetValue<string>("_itemList");
            _attributeIdFilter = info.GetValue<Int64>("_attributeIdFilter");
            _attributeList = info.GetValue<string>("_attributeList");
            _reviewSetIndex = info.GetValue<int>("_reviewSetIndex");
            _reviewSetId = info.GetValue<int>("_reviewSetId");
            _returnMessage = info.GetValue<string>("_returnMessage");
            _numTopics = info.GetValue<int>("_numTopics");
            _minItems = info.GetValue<int>("_minItems");
            _method = info.GetValue<string>("_method");
        }


#if !SILVERLIGHT

        class PaperData
        {
            public Int64 PaperId { get; set; }
            public Int64 FoS { get; set; }
        }
        class ItemData
        {
            public Int64 PaperId { get; set; }
            public Int64 ItemId { get; set; }
        }


        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (_method == "UseNLP")
            {
#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => doUseNLP(ri.ReviewId, ri.UserId));
#else
                ReturnMessage = "Process running (can take 10+ minutes)";
                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => doUseNLP(ri.ReviewId, ri.UserId, cancellationToken));
#endif
                return;
            }

            if (_itemList == "")
            {
                doCreateCodeSet();
                return;
            }

            
            
            List<ItemData> ItemsData = new List<ItemData>();

            // Get a list of the item ids that we need to get topics for
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                // 1. populate a list of PaperIds that are already in the review (we don't want to import duplicates)
                using (SqlCommand command = new SqlCommand("st_MagGetPaperIdsForFoSImport", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_IDS", _itemList));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", _attributeIdFilter));
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", _reviewSetId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            ItemsData.Add(new ItemData { ItemId = Convert.ToInt64(reader["ITEM_ID"].ToString()),
                                PaperId = Convert.ToInt64(reader["PaperId"].ToString())
                            });
                        }
                    }
                }
                connection.Close();
            }
            if (ItemsData.Count == 0)
            {
                ReturnMessage = "No items for which to obtain topics";
                return;
            }
            if (ItemsData.Count > 20000)
            {
                ReturnMessage = "Too many items to get topics for at once";
                return;
            }

            // 2. get review set to put topics in (create if it doesn't already exist)
            ReviewSet rs = null;
            if (_reviewSetId == 0)
            {
                _reviewSetId = CreateNewReviewSet(ri.ReviewId);
            }
            
            rs = ReviewSet.GetReviewSet(_reviewSetId);
            if (rs == null)
            {
                ReturnMessage = "Error: could not get code set";
                return;
            }
            if (rs.SetTypeId == 5 || rs.SetTypeId == 6)
            {
                ReturnMessage = "Error: can't add topics to screening or admin code sets";
                return;
            }

            // 3. Run through the list of PaperIds, hit MAKES and get the topics
            int count = 0;
            int numAdded = 0;
            List<PaperData> PapersData = new List<PaperData>();
            while (count < ItemsData.Count)
            {
                string query = "";
                for (int i = count; i < ItemsData.Count && i < count + 100; i++)
                {
                    if (query == "")
                    {
                        query = "Id=" + ItemsData[i].PaperId.ToString();
                    }
                    else
                    {
                        query += ",Id=" + ItemsData[i].PaperId.ToString();
                    }
                }
                
                MagMakesHelpers.PaperMakesResponse resp = MagMakesHelpers.EvaluateExpressionNoPagingWithCount("OR(" + query + ")", "100");
                foreach (MagMakesHelpers.PaperMakes pm in resp.entities)
                {
                    MagPaper mp = MagPaper.GetMagPaperFromPaperMakes(pm, null);
                    if (mp.PaperId > 0 && mp.FieldsOfStudy != null && mp.FieldsOfStudy != "")
                    {
                        foreach (string f in mp.FieldsOfStudy.Split(','))
                        {
                            PapersData.Add(new PaperData{ FoS =  Convert.ToInt64(f), PaperId = mp.PaperId });
                        }
                        numAdded++;
                    }

                }
                count += 100;
            }

            int enough = 0;
            var result = PapersData.GroupBy(x => x.FoS).ToDictionary(x => x.Key, x => x.Count()).OrderByDescending(x => x.Value);
            foreach (KeyValuePair<Int64, int> entry in result)
            {
                if (entry.Value >= _minItems && isValidFos(entry.Key) && enough <= _numTopics)
                {
                    GetOrCreateAttribute(rs, entry.Key.ToString(), ri.UserId, ri.ReviewId);
                    enough++;
                }
            }
            foreach (PaperData pd in PapersData)
            {
                AttributeSet aset = rs.GetAttributeSetFromOriginalAttributeId(pd.FoS);
                if (aset != null)
                {
                    var items = from i in ItemsData where i.PaperId == pd.PaperId select i;
                    foreach (ItemData id in items)
                    {
                        SaveAttribute(aset.AttributeId, ri.ReviewId, ri.UserId, id.ItemId, aset.SetId);
                    }
                }
            }
       
            ReturnMessage = "Successful. Items processed: " + numAdded.ToString();
        }

        private int CreateNewReviewSet(int ReviewId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@SET_TYPE_ID", 3)); // Standard
                    command.Parameters.Add(new SqlParameter("@ALLOW_CODING_EDITS", 1));
                    command.Parameters.Add(new SqlParameter("@SET_NAME", "OpenAlex Topics"));
                    command.Parameters.Add(new SqlParameter("@CODING_IS_FINAL", 1));
                    command.Parameters.Add(new SqlParameter("@SET_ORDER", _reviewSetIndex));
                    command.Parameters.Add(new SqlParameter("@SET_DESCRIPTION", "Topics auto-generated from OpenAlex"));
                    command.Parameters.Add(new SqlParameter("@ORIGINAL_SET_ID", 0));

                    SqlParameter par = new SqlParameter("@NEW_REVIEW_SET_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_REVIEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;

                    SqlParameter par2 = new SqlParameter("@NEW_SET_ID", System.Data.SqlDbType.Int);
                    par2.Value = 0;
                    command.Parameters.Add(par2);
                    command.Parameters["@NEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    _reviewSetId = Convert.ToInt32(command.Parameters["@NEW_REVIEW_SET_ID"].Value);
                }
                connection.Close();
            }
            return _reviewSetId;
        }

        
        private bool isValidFos(Int64 fos)
        {
            MagMakesHelpers.FieldOfStudyMakes check = MagMakesHelpers.EvaluateSingleFieldOfStudyId(fos.ToString());
            if (check != null)
            {
                if (check.FL > 0 && check.FP == null)
                    return false;
                else
                    return true;
            }
            return false;
        }

        // recursive function to create the tree of topics (where necessary)
        private AttributeSet GetOrCreateAttribute(ReviewSet rs, string FoSId, int ContactId, int ReviewId)
        {
            AttributeSet aset = rs.GetAttributeSetFromOriginalAttributeId(Convert.ToInt64(FoSId));
            if (aset != null)
                return aset;

            // need to create it. However, also need to create parents (probably)

            // first, get the MAKES record of our current attribute for creation
            MagMakesHelpers.FieldOfStudyMakes fos = MagMakesHelpers.EvaluateSingleFieldOfStudyId(FoSId);
            if (fos != null)
            {
                // next, check to see if it's a root node. If it is, we create and return it
                if (fos.FL == 0 || fos.FP == null) // shouldn't have to check for null, but some non FL topics have null parents??!
                {
                    aset = new AttributeSet
                    {
                        SetId = rs.SetId,
                        ParentAttributeId = 0,
                        AttributeTypeId = 2,
                        AttributeSetDescription = "",
                        AttributeOrder = rs.Attributes.Count,
                        AttributeName = fos.DFN,
                        AttributeDescription = "",
                        ExtURL = "https://explore.openalex.org/C" + fos.Id,
                        ExtType = "OpenAlex",
                        ContactId = ContactId,
                        OriginalAttributeID = fos.Id
                    };
                    AttributeSet.CreateNew(aset);
                    rs.Attributes.Add(aset);
                    return aset;
                }
                
                // it's not a root node, therefore we have to check for a parent    
                if (fos.FP != null)
                {
                    AttributeSet parentAttribute = null;
                                    
                    foreach (MagMakesHelpers.FieldOfStudyRelationshipMakes p in fos.FP)
                    {
                        parentAttribute = rs.GetAttributeSetFromOriginalAttributeId(p.FId);
                        if (parentAttribute != null)
                            break;
                    }
                    if (parentAttribute == null)
                    {
                        parentAttribute = GetOrCreateAttribute(rs, fos.FP[0].FId.ToString(), ContactId, ReviewId);
                    }

                    if (parentAttribute != null)
                    {
                        aset = new AttributeSet
                        {
                            SetId = rs.SetId,
                            ParentAttributeId = parentAttribute.AttributeId,
                            AttributeTypeId = 2,
                            AttributeSetDescription = "",
                            AttributeOrder = parentAttribute.Attributes.Count,
                            AttributeName = fos.DFN,
                            AttributeDescription = "",
                            ExtURL = "https://explore.openalex.org/C" + fos.Id,
                            ExtType = "OpenAlex",
                            ContactId = ContactId,
                            OriginalAttributeID = fos.Id
                        };
                        AttributeSet.CreateNew(aset);
                        parentAttribute.Attributes.Add(aset);
                        return aset;
                    }
                }
            }
            return null; // will only get here if the MAKES call fails
        }

        private Int64 SaveAttribute(Int64 AttributeId, int ReviewId, int ContactId, Int64 ItemId, int SetId)
        {
            Int64 ItemAttributeId = 0;
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ItemAttributeInsert", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                        command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", ""));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", AttributeId));
                        command.Parameters.Add(new SqlParameter("@SET_ID", SetId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", ItemId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", (object)DBNull.Value));
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

        private void doCreateCodeSet()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            ReviewSet rs = null;
            if (_reviewSetId == 0)
            {
                _reviewSetId = CreateNewReviewSet(ri.ReviewId);
            }

            rs = ReviewSet.GetReviewSet(_reviewSetId);
            if (rs == null)
            {
                ReturnMessage = "Error: could not get code set for new codes";
                return;
            }

            string [] FosIds = _attributeList.Replace("\n\r", "¬").Replace("r\n", "¬").Replace("\n", "¬").Replace("\r", "¬").Replace(",", "¬").Split('¬');
            Int64 testInt64 = 0;
            int count = 0;
            foreach (string s in FosIds)
            {
                if (s.Trim() != "" && Int64.TryParse(s, out testInt64))
                {
                    if (isValidFos(testInt64))
                    {
                        GetOrCreateAttribute(rs, s.Trim(), ri.UserId, ri.ReviewId);
                    }
                }
            }
        }
        private async void doUseNLP(int ReviewId, int ContactId, CancellationToken cancellationToken = default(CancellationToken))
        {
            string BatchGuid = Guid.NewGuid().ToString();
            int rowcount = 0;

            ReviewSet rs = ReviewSet.GetReviewSet(_reviewSetId);
            if (rs == null)
            {
                ReturnMessage = "Error: could not get code set";
                return;
            }
            if (rs.SetTypeId == 5 || rs.SetTypeId == 6)
            {
                ReturnMessage = "Error: can't add topics to screening or admin code sets";
                return;
            }

            // 1) write the data to the Azure SQL database
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ClassifierGetClassificationDataToSQL", connection))
                {
                    command.CommandTimeout = 6000; // 10 minutes - if there are tens of thousands of items it can take a while
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID_CLASSIFY_TO", _attributeIdFilter));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_LIST", _itemList));
                    command.Parameters.Add(new SqlParameter("@SOURCE_ID", 0));
                    command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                    command.Parameters.Add(new SqlParameter("@ContactId", ContactId));
                    command.Parameters.Add(new SqlParameter("@MachineName", TrainingRunCommand.NameBase));
                    command.Parameters.Add(new SqlParameter("@ROWCOUNT", 0));
                    command.Parameters["@ROWCOUNT"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    rowcount = Convert.ToInt32(command.Parameters["@ROWCOUNT"].Value);
                }
            }
            if (rowcount == 0)
            {
                ReturnMessage = "Zero rows to classify!";
                return;
            }

            // 2) Call the datafactory process that will take the above data and assign OpenAlex topics to all the records

#if (!CSLA_NETCORE)
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"BatchGuid", BatchGuid }
            };
            DataFactoryHelper.RunDataFactoryProcess("Fair wikipedia categories", parameters, true, ContactId, cancellationToken);
#endif

            // 3) Grab the classifications from the Azure SQL table and insert into tb_item_attribute

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("st_ClassifierGetClassificationScores", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {

                            AttributeSet aset = rs.GetAttributeSetFromAttributeName(reader["PredictedLabel"].ToString());
                            if (aset != null)
                            {
                                SaveAttribute(aset.AttributeId, ReviewId, ContactId, Convert.ToInt64(reader["ITEM_ID"].ToString()), aset.SetId);
                            }
                            
                        }
                    }
                }
                connection.Close();
            }

            // 4) clean up the Azure DB - deleting the item data and scores

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ClassifierCleanUpBatch", connection))
                {
                    command.CommandTimeout = 6000; // 10 minutes - if there are tens of thousands of items it can take a while
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@BatchGuid", BatchGuid));
                    command.ExecuteNonQuery();
                }
            }

            ReturnMessage = "Categories processed successfully";

        }



#endif


    }
}
