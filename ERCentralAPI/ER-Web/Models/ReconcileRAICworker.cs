using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Csla;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration.KeyPerFile;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography.Xml;


namespace BusinessLibrary.BusinessClasses
{
    public class ReconcileRAICworker
    {
        public static void FindAndDoWorkFromUITrigger(int ScreeningCodeSetId, int ReviewId, int ContactId, long TriggeringItemId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingNextItem", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command.Parameters.Add(new SqlParameter("@TRAINING_CODE_SET_ID", ScreeningCodeSetId));
                    command.Parameters.Add(new SqlParameter("@SIMULATE", 1));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            if (reader.NextResult())//only happens for "auto reconcile: retain all include codes (raic)"
                            {//under RAIC, items get unlocked ONLY when user changes item or requests an item (also in "simluate" mode) and get reconciled via the below
                                GetAndDoWork(reader, ReviewId, ContactId, TriggeringItemId);
                            }
                        }
                    }
                }
            }
        }
        public static void GetAndDoWork(Csla.Data.SafeDataReader reader, int reviewId, int contactId, long triggeringItemId = 0)
        {
            List<KeyValuePair<long, int>> itemsTocheck = new List<KeyValuePair<long, int>>();
            while (reader.Read())
            {
                itemsTocheck.Add(new KeyValuePair<long, int>(reader.GetInt64("ItemId"), reader.GetInt32("CONTACT_ID")));
            }
            if (triggeringItemId > 0 && itemsTocheck.FindIndex(f=> f.Key == triggeringItemId) == -1)
            {//the triggering item should be checked too, but doesn't appear in current screening lists, so we add it manually, which means it will be auto-reconciled
                itemsTocheck.Add(new KeyValuePair<long, int>(triggeringItemId, contactId));
            }
            if (itemsTocheck.Count > 0)
            {
                ReconcileRAICworker rec = new ReconcileRAICworker(itemsTocheck, reviewId, contactId);
                Task.Run(() => rec.DoRAICreconcileWork());
            }
        }
        public static void TrainingLockTheseItems(List<long> ItemsToWorkOn, int ReviewId, int ContactId)
        {
            DataTable InputTable = new DataTable();
            InputTable.Columns.Add(new DataColumn("ItemId", Int64.MaxValue.GetType()));
            InputTable.Columns.Add(new DataColumn("CONTACT_ID", int.MaxValue.GetType()));
            foreach (long ItemId in ItemsToWorkOn)
            {
                DataRow dr = InputTable.NewRow();
                dr["ItemId"] = ItemId;
                dr["CONTACT_ID"] = ContactId;
                InputTable.Rows.Add(dr);
            }
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingLockTheseItems", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    SqlParameter tvpParam = command.Parameters.AddWithValue("@ItemsToLock", InputTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.ITEMS_CONTACT_INPUT_TB";
                    command.ExecuteNonQuery();
                }
            }
        }
        private int ReviewID;
        private int ContactId;
        private List<KeyValuePair<Int64, int>> ItemsToWorkOn;
        public ReconcileRAICworker(List<KeyValuePair<long, int>> itemsToWorkOn, int reviewId, int contactId)
        {
            ReviewID = reviewId;
            ContactId = contactId;
            ItemsToWorkOn = itemsToWorkOn;
        }
        public void DoRAICreconcileWork()
        {
            try
            {
                int DummyUserId = 0;
                int ScreeningSetId = 0; int NofPeoplePerItem = 0; int ReviewSetId = 0;
                bool UserIsInThisReview = false;
                //get the ContactId for the dummy user and more;
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("st_GetAutoReconcileDataForRAIC", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ReviewId", ReviewID));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                DummyUserId = reader.GetInt32("DUMMY_ID");
                                UserIsInThisReview = reader.GetBoolean("DUMMY_IS_IN_REVIEW");
                                ScreeningSetId = reader.GetInt32("SCREENING_CODE_SET_ID");
                                NofPeoplePerItem = reader.GetInt32("SCREENING_N_PEOPLE");
                                ReviewSetId = reader.GetInt32("REVIEW_SET_ID");
                            }
                        }
                    }
                }
                //get the ReviewSet, so that we know what include codes are there
                ReviewSet reviewSet = ReviewSet.GetReviewSet(ReviewSetId);
                List<AttributeSet> IncludeCodes = reviewSet.Attributes.ToList().FindAll(f => f.AttributeType == "Include");
                List<AttributeSet> ExcludeCodes = reviewSet.Attributes.ToList().FindAll(f => f.AttributeType == "Exclude");


                foreach (KeyValuePair<Int64, int> kvp in ItemsToWorkOn)
                {
                    
                    DataPortal<ItemSetList> dp = new DataPortal<ItemSetList>();
                    ItemSetList AllItemSets = dp.Fetch(new SingleCriteria<ItemSetList, Int64>(kvp.Key));
                    List<ItemSet> ItemSets = AllItemSets.ToList().FindAll(f => f.SetId == ScreeningSetId);

                    //check what we're doing: is an existing coding already complete?? If so, do nothing
                    if (ItemSets.Find(f=> f.IsCompleted == true) != null)
                    {
                        ErrorLogSink("");
                        ErrorLogSink("AutoReconcile - Retain All Include Codes:");
                        ErrorLogSink("Skipping item: " + kvp.Key.ToString() + " in review: " + ReviewID.ToString() +", item has completed coding.");
                        ErrorLogSink("");
                        continue;
                    }


                    List<ReadOnlyItemAttribute> IncludeCodesToAdd = new List<ReadOnlyItemAttribute>();
                    List<ReadOnlyItemAttribute> ExcludeCodesToAdd = new List<ReadOnlyItemAttribute>();
                    List<int> PeopleWhoCodedThisitem = new List<int>();
                    foreach (ItemSet iSet in ItemSets)
                    {
                        if (!PeopleWhoCodedThisitem.Contains(iSet.ContactId))
                        {
                            PeopleWhoCodedThisitem.Add(iSet.ContactId);
                        }

                        List<ReadOnlyItemAttribute> IncludeCodesInThisItem = iSet.ItemAttributes.ToList().FindAll(f => IncludeCodes.Find(f1 => f1.AttributeId == f.AttributeId) != null);
                        foreach (ReadOnlyItemAttribute roia in IncludeCodesInThisItem)
                        {
                            if (IncludeCodesToAdd.Find(f => roia.AttributeId == f.AttributeId) == null) IncludeCodesToAdd.Add(roia);
                        }
                        if (IncludeCodesToAdd.Count == 0)
                        {
                            List<ReadOnlyItemAttribute> ExcludeCodesInThisItem = iSet.ItemAttributes.ToList().FindAll(f => ExcludeCodes.Find(f1 => f1.AttributeId == f.AttributeId) != null);
                            foreach (ReadOnlyItemAttribute roia in ExcludeCodesInThisItem)
                            {
                                if (ExcludeCodesToAdd.Find(f=> roia.AttributeId == f.AttributeId) == null) ExcludeCodesToAdd.Add(roia);
                            }
                        }
                    }
                    if (PeopleWhoCodedThisitem.Count >= NofPeoplePerItem)
                    {
                        //we need to create the coding in the name of the dummy user
                        //we use SPs rather that BOs because BOs will be tied to a given review/user match
                        //but we NEED to specify the user and don't want things to fail if current user changes review before we finish
                        
                        //check: do we have something to add???
                        if (IncludeCodesToAdd.Count + ExcludeCodesToAdd.Count < 1)
                        {
                            ErrorLogSink("");
                            ErrorLogSink("AutoReconcile - Retain All Include Codes:");
                            ErrorLogSink("Skipping item: " + kvp.Key.ToString() + " in review: " + ReviewID.ToString() + ", got no codes to complete.");
                            ErrorLogSink("");
                        }
                        
                        Int64 ItemSetId = 0;
                        if (UserIsInThisReview == false)
                        {//add user to review
                            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand("st_ReviewAddContact", connection))
                                {//this adds the user to the review as 'RegularUser'
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", DummyUserId));
                                    command.Parameters.Add(new SqlParameter("@old_review_id", ""));
                                    command.ExecuteNonQuery();
                                }
                                // make the user "Coding only"
                                using (SqlCommand command = new SqlCommand("st_ReviewRoleUpdateByContactID", connection))
                                {//this adds the user to the review as 'RegularUser'
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", DummyUserId));
                                    command.Parameters.Add(new SqlParameter("@ROLE_NAME", "Coding only"));
                                    command.ExecuteNonQuery();
                                }
                                connection.Close();
                            }
                            UserIsInThisReview = true;
                        }
                        using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                        {
                            connection.Open();
                            List<ReadOnlyItemAttribute> codesToAdd = IncludeCodesToAdd;
                            if (codesToAdd.Count == 0) codesToAdd = ExcludeCodesToAdd;
                            foreach (ReadOnlyItemAttribute roia in codesToAdd)
                            {//add all the include codes we found
                                using (SqlCommand command = new SqlCommand("st_ItemAttributeInsert", connection))
                                {
                                    command.CommandType = System.Data.CommandType.StoredProcedure;

                                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", DummyUserId));
                                    command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", roia.AdditionalText));
                                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", roia.AttributeId));
                                    command.Parameters.Add(new SqlParameter("@SET_ID", ScreeningSetId));
                                    command.Parameters.Add(new SqlParameter("@ITEM_ID", kvp.Key));
                                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                                    command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", roia.ArmId == 0 ? (object)DBNull.Value : roia.ArmId));
                                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_ATTRIBUTE_ID", 0));
                                    command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                                    command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output;

                                    command.ExecuteNonQuery();
                                    if (ItemSetId == 0) ItemSetId = (Int64)command.Parameters["@NEW_ITEM_SET_ID"].Value;
                                }
                            }
                            if (ItemSetId > 0)
                            {//complete and lock the coding for the dummy user
                                using (SqlCommand command = new SqlCommand("st_ItemSetComplete", connection))
                                {
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", ItemSetId));
                                    command.Parameters.Add(new SqlParameter("@COMPLETE", true));
                                    command.Parameters.Add(new SqlParameter("@IS_LOCKED", true));
                                    command.Parameters.Add(new SqlParameter("@SUCCESSFUL", true));
                                    command.Parameters["@SUCCESSFUL"].Direction = System.Data.ParameterDirection.Output;
                                    command.ExecuteNonQuery();
                                }
                            }
                            connection.Close();
                        }
                        
                    }
                }
                //unclock the items we just evaluated
                DataTable InputTable = new DataTable();
                InputTable.Columns.Add(new DataColumn("ItemId", Int64.MaxValue.GetType()));
                InputTable.Columns.Add(new DataColumn("CONTACT_ID", int.MaxValue.GetType()));
                foreach (KeyValuePair<Int64, int> kvp in ItemsToWorkOn)
                {
                    DataRow dr = InputTable.NewRow();
                    dr["ItemId"] = kvp.Key;
                    dr["CONTACT_ID"] = kvp.Value;
                    InputTable.Rows.Add(dr);
                }
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("st_TrainingUnlockTheseItems", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewID));
                        SqlParameter tvpParam = command.Parameters.AddWithValue("@ItemsToUnlock", InputTable);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "dbo.ITEMS_CONTACT_INPUT_TB";
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch(Exception ex)
            {
                ErrorLogSink(ex.Message);
                if (ex.StackTrace != null) ErrorLogSink(ex.StackTrace);
            }
        }
        private void ErrorLogSink(string Message)
        {
            try
            {
                if (Program.Logger != null) Program.Logger.Error(Message);
            }
            catch { }
        }
    }
}
