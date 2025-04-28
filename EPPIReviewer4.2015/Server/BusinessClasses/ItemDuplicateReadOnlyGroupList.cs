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

#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using AuthorsHandling;
using System.Threading;
#if !CSLA_NETCORE
using System.Web.Hosting;
#endif
using System.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemDuplicateReadOnlyGroupList : ReadOnlyListBase<ItemDuplicateReadOnlyGroupList, ItemDuplicateReadOnlyGroup>
    {

        public ItemDuplicateReadOnlyGroupList() { }
        public int CompletedCount
        {
            get
            {
                int res = 0;
                if (this.Count > 0)
                {
                    foreach (ItemDuplicateReadOnlyGroup G in this)
                    {
                        if (G.IsComplete) res++;
                    }
                }
                return res;
            }
        }
        public static void getItemDuplicateReadOnlyGroupList(bool getNew, EventHandler<DataPortalResult<ItemDuplicateReadOnlyGroupList>> handler)
        {
            DataPortal<ItemDuplicateReadOnlyGroupList> dp = new DataPortal<ItemDuplicateReadOnlyGroupList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(getNew));
        }
        public static void GetGroupsList(EventHandler<DataPortalResult<ItemDuplicateReadOnlyGroupList>> handler)
        {
            DataPortal<ItemDuplicateReadOnlyGroupList> dp = new DataPortal<ItemDuplicateReadOnlyGroupList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemDuplicateReadOnlyGroupList, bool>(false));
        }
#if !SILVERLIGHT
        private int _RevId = 0;
        private bool CachedCriteriaValue = false ;
        private int _Cid = 0;
        //private int instance = DateTime.Now.Millisecond;
        Comparator comparator;
        private DataTable ResultsCache = null;
        private void DataPortal_Fetch(SingleCriteria<ItemDuplicateReadOnlyGroupList, bool> criteria)
        {
            IsReadOnly = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            _RevId = ri.ReviewId;
            _Cid = ri.UserId;
            RaiseListChangedEvents = false;
            CachedCriteriaValue = criteria.Value;//might change in CheckOngoing(...)
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                CheckOngoing(connection, true, true, criteria.Value);//this will insert a new "dedup running" line in TB_REVIEW_JOB if:
                //dedup isn't already running, AND:
                //1. criteria.Value is true (user asked to run "get new dups")
                //or
                //2. Check found that previous execution failed, instructing the code below to "get new dups" even if the user didn't ask for this.
                if (CachedCriteriaValue == true)
                {
#if OLDDEDUP
                    FindNewDuplicatesOld(connection);
#else
                    FindNewDuplicates(connection);
#endif
                    //if (this.Count == 1)
                    //{
                    //    //RaiseListChangedEvents = true;
                    //    this.Clear();

                    //    throw new DataPortalException("Execution still Running", this);
                    //}
                }
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevID", _RevId));
                    command.CommandTimeout = 45;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemDuplicateReadOnlyGroup.GetItemDuplicateReadOnlyGroup(reader));
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            //RaiseListChangedEvents = true;
        }

        protected void FindNewDuplicatesOld(SqlConnection connection)
        {
            // At the moment, this doesn't find duplicates at all - but it will soon!
            using (SqlCommand command = new SqlCommand("st_ItemDuplicateFindNew", connection))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@revID", _RevId));
                command.CommandTimeout = 300;
                //string r = "";
                try
                {
                    command.ExecuteNonQuery();
                    //using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    //{

                    //    while (reader.Read())
                    //    {
                    //        if (reader[0] != null) r += reader[0].ToString() + Environment.NewLine;
                    //    }
                    //    r += "!!";
                    //}
                }
                catch (Exception ex)
                {
                    //throw exception that will be interpreted as a timeout and handled client side
                    throw new DataPortalException("Execution still Running", this);
                }
            }
        }


        protected void FindNewDuplicates(SqlConnection connection)
        {
            
            bool StillRunning = true;
            SetShortSearchText();
            //line above might take time, so let's check again that noone else triggered this...
            //CheckOngoing(connection, true, true);//shouldrestart = true: don't throw an exception if it should restart, we are (re/)starting...
            
#if CSLA_NETCORE
            System.Threading.Tasks.Task.Run(() => FindNewDuplicatesNewVersion());
#else
            //see: https://codingcanvas.com/using-hostingenvironment-queuebackgroundworkitem-to-run-background-tasks-in-asp-net/
            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => FindNewDuplicatesNewVersion(cancellationToken));
#endif
            

            //we now  want to wait about 3m to keep the user waiting...
            //we check if it's still running every "sleeptime" for up to 10 times in total.
            int counter = 0;
            int sleepTime = 15 * 1000;//in Milliseconds
            while (StillRunning && counter < 9)
            {
                Thread.Sleep(sleepTime);
                //passing once=false so not to throw an exception if we think it's still running for real.
                //passing shouldrestart=false so not to try re-triggering a "get new duplicates" routine as we just did it...
                StillRunning = CheckOngoing(connection, false, false);//passing once=false so not to throw an exception if we think it's still running for real.
                counter++;
            }
            if (StillRunning)
            {
                Thread.Sleep(sleepTime);
                StillRunning = CheckOngoing(connection, true, false);//this is the last check, will throw an exception if it's still running.
            }
            
        }
        private bool CheckOngoing(SqlConnection AlreadyOpenConnection, bool once = true, bool shouldRestart = false, bool userAskedToGetNewDuplicates = false)
        {
#if OLDDEDUP
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupCheckOngoing", AlreadyOpenConnection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@revID", System.Data.SqlDbType.Int);
                    command.Parameters["@revID"].Value = _RevId;
                    command.Parameters.Add("@RETURN_VALUE", System.Data.SqlDbType.Int);
                    command.Parameters["@RETURN_VALUE"].Direction = System.Data.ParameterDirection.ReturnValue;
                    command.CommandTimeout = 300;
                    command.ExecuteNonQuery();
                    if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "-2")
                    {
                        this.Clear();
                        throw new DataPortalException("Execution still Running", this);
                    }
                    else if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "-3")
                    {//we'll see if this happens frequently
                        this.Clear();
                        throw new DataPortalException("Previous Execution failed." + Environment.NewLine
                            + "Please try to \"Get new duplicates\" once more." + Environment.NewLine
                            + "If this message appears again, please contact EPPI-reviewer Support Staff.", this);
                    }
                }
            
#else
            using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupCheckOngoingLog", AlreadyOpenConnection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add("@revID", System.Data.SqlDbType.Int);
                command.Parameters["@revID"].Value = _RevId;
                command.Parameters.Add(new SqlParameter("@CONTACT_ID", _Cid));
                command.Parameters.Add(new SqlParameter("@GettingNewDuplicates", userAskedToGetNewDuplicates));
                command.Parameters.Add("@RETURN_VALUE", System.Data.SqlDbType.Int);
                command.Parameters["@RETURN_VALUE"].Direction = System.Data.ParameterDirection.ReturnValue;
                command.ExecuteNonQuery();
                if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "-2")
                {
                    this.Clear();
                    if (once) throw new DataPortalException("Execution still Running", this);
                    else return true;
                }
                else if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "-3")
                {//we could start "get new duplicates" straight away, or tell the user to do it...
                    if (shouldRestart)
                    {//we want to trigger "GetNewDuplicates" silently. This happens when we're checking if a getnewduplicates is running at the very start.
                        CachedCriteriaValue = true;
                    }
                    else
                    {//this happened _after_ the first check, so something is wrong and we might as well let the user know.
                        this.Clear();
                        throw new DataPortalException("Previous Execution failed." + Environment.NewLine
                            + "Please try to \"Get new duplicates\" once more." + Environment.NewLine
                            + "If this message appears again, please contact EPPI-reviewer Support Staff.", this);
                    }
                }
                else if (command.Parameters["@RETURN_VALUE"].Value.ToString() == "-4")
                {//we'll see if this happens frequently: 10 attempts in a row failed!
                    this.Clear();
                    throw new DataPortalException("Multiple previous executions failed!" + Environment.NewLine
                        + "This indicates there is a problem with this review." + Environment.NewLine
                        + "Please contact EPPI-Reviewer Support Staff.", this);
                }
            }
#endif
            return false;
        }

        //private List<MatchedItems> LocalCache = new List<MatchedItems>();
        private void AddDuplicate(ItemComparison MasterItem, ItemComparison matchedItem, double matchingScore)
        {
            DataRow dr = ResultsCache.NewRow();
            dr["MASTER_ID"] = MasterItem.ITEM_ID;
            dr["member_id"] = matchedItem.ITEM_ID;
            dr["IsNew"] = MasterItem.IS_MASTER == 0;//this tells us that it is ALREADY a master of an existing group!
            dr["score"] = matchingScore;
            ResultsCache.Rows.Add(dr);
            //MatchedItems match = new MatchedItems(MasterItem, matchedItem, matchingScore);
            //LocalCache.Add(match);
            //if (LocalCache.Count >= 5) FlushCache();
            //using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //{
            //    connection.Open();


            //        //MatchedItems current = LocalCache[0];
            //        //ItemComparison MasterItem = current.MasterItem;
            //        //ItemComparison matchedItem = current.matchedItem;
            //        //double matchingScore = current.matchingScore;
            //        if (MasterItem.IS_MASTER == 1)// || current.MasterItem == LastMaster)
            //        {
            //            using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupAddAddionalItem", connection))
            //            {
            //                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //                command.CommandType = System.Data.CommandType.StoredProcedure;
            //                command.Parameters.Add(new SqlParameter("@revID", _RevId));
            //                command.Parameters.Add(new SqlParameter("@MasterID", MasterItem.ITEM_ID));
            //                command.Parameters.Add(new SqlParameter("@NewDuplicateItemID", matchedItem.ITEM_ID));
            //                command.Parameters.Add(new SqlParameter("@Score", matchingScore));
            //                //command.ExecuteNonQuery();
            //            }
            //        }
            //        else
            //        {
            //            //if (current.MasterItem != LastMaster) LastMaster = current.MasterItem;//when adding a new group, from here on, we'll add additional items (block above)
            //            using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupCreateNew", connection))
            //            {
            //                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //                command.CommandType = System.Data.CommandType.StoredProcedure;
            //                command.Parameters.Add(new SqlParameter("@revID", _RevId));
            //                command.Parameters.Add(new SqlParameter("@Score", matchingScore));
            //                command.Parameters.Add(new SqlParameter("@MasterID", MasterItem.ITEM_ID));
            //                command.Parameters.Add(new SqlParameter("@MemberId", matchedItem.ITEM_ID));
            //                //command.ExecuteNonQuery();
            //            }
            //        }
            //        //LocalCache.RemoveRange(0, 1);


            //    connection.Close();
            //}
        }
        //ItemComparison LastMaster = new ItemComparison();
        
        private void FlushCache()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupCreateFromERNativeResults", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@revID", _RevId));
                    SqlParameter tvpParam = command.Parameters.AddWithValue("@input", ResultsCache);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.CREATE_GROUPS_INPUT_TB";
                    command.ExecuteNonQuery();
                }
                ResultsCache.Rows.Clear();
            }


            //using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            //{
            //    connection.Open();
                
            //    while (LocalCache.Count > 0)
            //    {
            //        MatchedItems current = LocalCache[0];
            //        //ItemComparison MasterItem = current.MasterItem;
            //        //ItemComparison matchedItem = current.matchedItem;
            //        //double matchingScore = current.matchingScore;
            //        if (current.MasterItem.IS_MASTER == 1 || current.MasterItem == LastMaster)
            //        {
            //            using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupAddAddionalItem", connection))
            //            {
            //                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //                command.CommandType = System.Data.CommandType.StoredProcedure;
            //                command.Parameters.Add(new SqlParameter("@revID", _RevId));
            //                command.Parameters.Add(new SqlParameter("@MasterID", current.MasterItem.ITEM_ID));
            //                command.Parameters.Add(new SqlParameter("@NewDuplicateItemID", current.matchedItem.ITEM_ID));
            //                command.Parameters.Add(new SqlParameter("@Score", current.matchingScore));
            //                command.ExecuteNonQuery();
            //            }
            //        }
            //        else
            //        {
            //            if (current.MasterItem != LastMaster) LastMaster = current.MasterItem;//when adding a new group, from here on, we'll add additional items (block above)
            //            using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupCreateNew", connection))
            //            {
            //                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //                command.CommandType = System.Data.CommandType.StoredProcedure;
            //                command.Parameters.Add(new SqlParameter("@revID", _RevId));
            //                command.Parameters.Add(new SqlParameter("@Score", current.matchingScore));
            //                command.Parameters.Add(new SqlParameter("@MasterID", current.MasterItem.ITEM_ID));
            //                command.Parameters.Add(new SqlParameter("@MemberId", current.matchedItem.ITEM_ID));
            //                command.ExecuteNonQuery();
            //            }
            //        }
            //        LocalCache.RemoveRange(0, 1);
            //    }
                
            //    connection.Close();
            //}
        }
        private void SetShortSearchText()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGetTitles", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", _RevId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            string searchText = Item.ToShortSearchText(reader["TITLE"].ToString());
                            Int64 ItemId = reader.GetInt64("ITEM_ID");
                            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
                            {
                                connection2.Open();
                                using (SqlCommand commandsave = new SqlCommand("st_ItemDuplicateSaveShortSearchText", connection2))
                                {
                                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                                    commandsave.CommandType = System.Data.CommandType.StoredProcedure;
                                    commandsave.Parameters.Add(new SqlParameter("@SearchText", searchText));
                                    commandsave.Parameters.Add(new SqlParameter("@ITEM_ID", ItemId));
                                    commandsave.ExecuteNonQuery();
                                }
                                connection2.Close();
                            }
                        }
                    }
                }
                connection.Close();
            }
        }

        
        private void FindNewDuplicatesNewVersion(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ResultsCache == null) PerpareResultsCache();//initialises and perpares the columns...
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                comparator = new Comparator();
                connection.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("st_ItemDuplicatesGetCandidatesOnSearchText", connection))
                    {
                        //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", _RevId));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", _Cid));
                        command.CommandTimeout = 240; //8 times the usual 30s...
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            string currentSearchText = "";
                            List<ItemComparison> CurrentGroup = new List<ItemComparison>();
                            int counter = 0;//used when debugging
                            while (reader.Read())
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    throw new Exception("Cancel request was received");
                                }
                                counter++;
                                string tmp = reader["SearchText"].ToString();
                                if (currentSearchText != tmp && CurrentGroup.Count > 0)
                                {
                                    if (CurrentGroup.Count > 0) MakeComparisons(CurrentGroup, 1);
                                    //will go and save current batch of results. We do it here as we're starting to look at a new set items, possibly to be into their own group
                                    if (ResultsCache.Rows.Count > 800) FlushCache(); 
                                    CurrentGroup.Clear();
                                    //currentSearchText = tmp;
                                }
                                currentSearchText = tmp;
                                ItemComparison newItem = new ItemComparison(reader, 0);
                                if (!AlreadyInGroup(CurrentGroup, newItem))
                                {
                                    CurrentGroup.Add(newItem);
                                }
                                newItem = new ItemComparison(reader, 1);
                                if (!AlreadyInGroup(CurrentGroup, newItem))
                                {
                                    CurrentGroup.Add(newItem);
                                }
                            }
                            if (CurrentGroup.Count > 0)
                            {
                                MakeComparisons(CurrentGroup, 1); // for the last group to be read before the reader didn't read any more
                                if (ResultsCache.Rows.Count > 0) FlushCache();//save last batch
                            }
                            //debug purposes: comment out in production
                            //MagMakesHelpers.CleanText("nothing");
                        }

                    }
                }
                catch (Exception e)
                {
                    using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupsMarkAsDoneChecking", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@revID", _RevId));
                        command.Parameters.Add(new SqlParameter("@state", "Failed"));
                        command.Parameters.Add(new SqlParameter("@message", e.Message));
                        command.ExecuteNonQuery();
                    }
                    this.Clear();
                    throw new DataPortalException("Execution failed with message:" + Environment.NewLine
                        + e.Message + Environment.NewLine
                        + "This indicates there is a problem with this review." + Environment.NewLine
                        + "If this message appears again, please contact EPPI-reviewer Support Staff.", this);

                }
                //FlushCache();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupsMarkAsDoneChecking", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@revID", _RevId));
                    command.Parameters.Add(new SqlParameter("@state", "Ended"));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        private void MakeComparisons(List<ItemComparison> group, int iteration)
        {
            List<ItemComparison> unmatched = new List<ItemComparison>();

            // if the first item is already a Master item, we add everything else into that group
            // UNLESS it's already a master - in which case we leave it alone
            if (group[0].IS_MASTER == 1)
            {
                for (int i = 1; i < group.Count; i++)
                {
                    if (group[i].IS_MASTER == 0)
                    {
                        double matchingScore = comparator.CompareItems(group[0], group[i]);
                        if (matchingScore > 0.7)
                        {
                            AddDuplicate(group[0], group[i], matchingScore);
                        }
                        else
                        {
                            unmatched.Add(group[i]);
                        }
                    }
                }
            }
            else
            {
                // The first item isn't already a master item. Can we find an existing master further down the list?
                ItemComparison existingMaster = null;
                foreach (ItemComparison ic in group)
                {
                    if (ic.IS_MASTER == 1)
                    {
                        existingMaster = ic;
                        break;
                    }
                }
                if (existingMaster != null)
                {
                    foreach (ItemComparison ic in group)
                    {
                        if (ic.ITEM_ID != existingMaster.ITEM_ID && ic.IS_MASTER == 0)
                        {
                            double matchingScore = comparator.CompareItems(existingMaster, ic);
                            if (matchingScore > 0.7)
                            {
                                AddDuplicate(existingMaster, ic, matchingScore);
                            }
                            else
                            {
                                unmatched.Add(ic);
                            }
                        }
                    }
                }
                else
                {
                    // this is an entirely new group
                    // we go for the lowest item id to be master, unless another item 'has codes'
                    // (if the first item 'has codes' then we stick with it
                    ItemComparison candidateMaster = group[0];
                    foreach (ItemComparison ic in group)
                    {
                        if (ic.HAS_CODES == 1)
                        {
                            candidateMaster = ic;
                            break;//we found the oldest member with codes, stopping here is perfect
                        }
                    }
                    foreach (ItemComparison ic in group)
                    {
                        if (ic.ITEM_ID != candidateMaster.ITEM_ID && ic.IS_MASTER == 0)
                        {
                            double matchingScore = comparator.CompareItems(candidateMaster, ic);
                            if (matchingScore > 0.7)
                            {
                                AddDuplicate(candidateMaster, ic, matchingScore);
                                //candidateMaster.IS_MASTER = 1; // i.e. after the first time, we are adding to an existing group in AddDuplicate
                            }
                            else
                            {
                                unmatched.Add(ic);
                            }

                        }
                    }
                }
            }
            // if we've put some items in the unmatched group, we circulate round again to see if they
            // make a distinct group of their own
            // we do this 9 times. I've not seen a situation where more than a couple is needed though
            if (unmatched.Count > 1 && iteration < 10)
            {
                MakeComparisons(unmatched, iteration + 1);
            }
        }

        private bool AlreadyInGroup(List<ItemComparison> CurrentGroup, ItemComparison it)
        {
            bool ret = false;
            foreach (ItemComparison ic in CurrentGroup)
            {
                if (ic.ITEM_ID == it.ITEM_ID)
                    return true;
            }
            return ret;
        }

        private void PerpareResultsCache()
        {
            ResultsCache = new DataTable();
            ResultsCache.Columns.Add(new DataColumn("MASTER_ID", Int64.MaxValue.GetType()));
            ResultsCache.Columns.Add(new DataColumn("member_id", Int64.MaxValue.GetType()));
            ResultsCache.Columns.Add(new DataColumn("IsNew", false.GetType()));
            ResultsCache.Columns.Add(new DataColumn("score", double.MaxValue.GetType()));
        }
        

        private void DataPortal_Fetch(GroupListSelectionCriteria criteria)
        {
            IsReadOnly = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupListSearch", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ItemIDs", criteria.ItemIds));
                    command.Parameters.Add(new SqlParameter("@GroupID", criteria.GroupId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemDuplicateReadOnlyGroup.GetItemDuplicateReadOnlyGroup(reader));
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

        internal class ItemComparison
        {
            public Int64 ITEM_ID { get; set; }
            public string AUTHORS { get; set; }
            public string TITLE { get; set; }
            public string PARENT_TITLE { get; set; }
            public string YEAR { get; set; }
            public string VOLUME { get; set; }
            public string PAGES { get; set; }
            public string ISSUE { get; set; }
            public string DOI { get; set; }
            public string ABSTRACT { get; set; }
            public Int32 HAS_CODES { get; set; }
            public Int32 IS_MASTER { get; set; }
            public Int32 TYPE_ID { get; set; }
            
            public double MatchingScore { get; set; }
            public ItemComparison() { }
            public ItemComparison (SafeDataReader reader, int index)
            {
                if (index == 0)
                {
                    ITEM_ID = reader.GetInt64("ITEM_ID");
                    AUTHORS = reader.GetString("AUTHORS");
                    TITLE = reader.GetString("TITLE");
                    PARENT_TITLE = MagMakesHelpers.CleanText(reader.GetString("PARENT_TITLE").Replace("&", "and"), true);
                    YEAR = reader.GetString("YEAR");
                    VOLUME = reader.GetString("VOLUME");
                    PAGES = reader.GetString("PAGES");
                    ISSUE = reader.GetString("ISSUE");
                    DOI = reader.GetString("DOI").ToUpper().Replace("HTTPS://DX.DOI.ORG/", "").Replace("HTTPS://DOI.ORG/", "").Replace("HTTP://DX.DOI.ORG/", "").Replace("HTTP://DOI.ORG/", "").Replace("[DOI]", "").TrimEnd('.').Trim();
                    ABSTRACT = reader.GetString("ABSTRACT");
                    HAS_CODES = reader.GetInt32("HAS_CODES");
                    IS_MASTER = reader.GetInt32("IS_MASTER");
                    TYPE_ID = reader.GetInt32("TYPE_ID");
                }
                else
                {
                    ITEM_ID = reader.GetInt64("ITEM_ID2");
                    AUTHORS = reader.GetString("AUTHORS2");
                    TITLE = reader.GetString("TITLE2");
                    PARENT_TITLE = MagMakesHelpers.CleanText(reader.GetString("PARENT_TITLE2").Replace("&", "and"), true);
                    YEAR = reader.GetString("YEAR2");
                    VOLUME = reader.GetString("VOLUME2");
                    PAGES = reader.GetString("PAGES2");
                    ISSUE = reader.GetString("ISSUE2");
                    DOI = reader.GetString("DOI2").ToUpper().Replace("HTTPS://DX.DOI.ORG/", "").Replace("HTTPS://DOI.ORG/", "").Replace("HTTP://DX.DOI.ORG/", "").Replace("HTTP://DOI.ORG/", "").Replace("[DOI]", "").TrimEnd('.').Trim();
                    ABSTRACT = reader.GetString("ABSTRACT2");
                    HAS_CODES = reader.GetInt32("HAS_CODES2");
                    IS_MASTER = reader.GetInt32("IS_MASTER2");
                    TYPE_ID = reader.GetInt32("TYPE_ID2");
                }
                //line below was: if (TITLE.IndexOf("Erratum") == -1)
                if (!Comparator.ErratumRegex.IsMatch(TITLE))
                    TITLE = MagMakesHelpers.RemoveTextInParentheses(TITLE);
                TITLE = MagMakesHelpers.CleanText(TITLE, true);
            }

            public ItemComparison(MagMakesHelpers.OaPaper pm)
            {
                ITEM_ID = Convert.ToInt64(pm.id.Replace("https://openalex.org/W", ""));
                AUTHORS = MagMakesHelpers.getAuthors(pm.authorships);
                TITLE = pm.title;
                PARENT_TITLE = pm.primary_location != null && pm.primary_location.source != null && pm.primary_location.source.display_name != null ? pm.primary_location.source.display_name : "";
                PARENT_TITLE = MagMakesHelpers.CleanText(PARENT_TITLE.Replace("&", "and"));
                YEAR = DateTime.Parse(pm.publication_date).Year.ToString();
                VOLUME = pm.biblio != null ? pm.biblio.volume : "";
                PAGES = pm.biblio != null ? pm.biblio.first_page + "-" + pm.biblio.last_page : "";
                ISSUE = pm.biblio != null ? pm.biblio.issue : "";
                DOI = pm.doi != null ? pm.doi.ToUpper().Replace("HTTPS://DX.DOI.ORG/", "").Replace("HTTPS://DOI.ORG/", "").Replace("HTTP://DX.DOI.ORG/", "").Replace("HTTP://DOI.ORG/", "").Replace("[DOI]", "").TrimEnd('.').Trim() : "";
                ABSTRACT = MagMakesHelpers.ReconstructInvertedAbstract(pm.abstract_inverted_index);
                HAS_CODES = 0;
                IS_MASTER = 0;
                TYPE_ID = MagMakesHelpers.GetErEquivalentPubTypeFromOa(pm.type);
                if (TITLE != null && !Comparator.ErratumRegex.IsMatch(TITLE))
                    TITLE = MagMakesHelpers.RemoveTextInParentheses(TITLE);
                TITLE = MagMakesHelpers.CleanText(TITLE);
            }

            public ItemComparison(Item i)
            {
                ITEM_ID = i.ItemId;
                AUTHORS = i.Authors;
                TITLE = i.Title;
                PARENT_TITLE = MagMakesHelpers.CleanText(i.ParentTitle.Replace("&", "and"));
                YEAR = i.Year;
                VOLUME = i.Volume;
                PAGES = i.Pages;
                ISSUE = i.Issue;
                DOI = i.DOI != null ? i.DOI.ToUpper().Replace("HTTPS://DX.DOI.ORG/", "").Replace("HTTPS://DOI.ORG/", "").Replace("HTTP://DX.DOI.ORG/", "").Replace("HTTP://DOI.ORG/", "").Replace("[DOI]", "").TrimEnd('.').Trim() : "";
                ABSTRACT = i.Abstract;
                HAS_CODES = 0;
                IS_MASTER = 0;
                TYPE_ID = i.TypeId;
                //line below was: if (TITLE.IndexOf("Erratum") == -1)
                if (!Comparator.ErratumRegex.IsMatch(TITLE))
                    TITLE = MagMakesHelpers.RemoveTextInParentheses(TITLE);
                TITLE = MagMakesHelpers.CleanText(TITLE);
            }

            private AutorsList _AutorsList = null;
            public AutorsList GetAuthors()
            {
                if (_AutorsList == null)
                {
                    AutorsList alist = new AutorsList();
                    if (AUTHORS.Length > 0)
                    {
                        string[] authors = AUTHORS.Split(';');
                        for (int x = 0; x < authors.Count(); x++)
                        {
                            if (authors[x] != " " && authors[x] != "")
                            {
                                //SG change 22/06/2022
                                //make sure we're not evaluating authors that have too little data, we demand to have at least 2 chars between first, middle and last names.
                                AutH autH = NormaliseAuth.singleAuth(authors[x], x + 1, 0);
                                //if (autH != null && (autH.FirstName.Length > 0 || autH.LastName.Length > 0 || autH.MiddleName.Length > 0))
                                if (autH != null && (autH.FirstName.Length + autH.LastName.Length + autH.MiddleName.Length > 1))
                                {
                                    alist.Add(autH);
                                }
                            }
                        }
                    }
                    this._AutorsList = alist;
                }
                return _AutorsList;
            }
            private string _FirstPage = null;
            public string GetFirstPage()
            { 
                if (_FirstPage == null)
                {
                    if (PAGES == "")
                        _FirstPage = "";
                    int i = PAGES.IndexOf("-");
                    if (i == -1)
                        _FirstPage = PAGES;
                    else
                        _FirstPage = PAGES.Substring(0, i);
                }
                return _FirstPage;
            }
        }
        internal class Comparator
        {
            public static readonly System.Text.RegularExpressions.Regex ErratumRegex = new System.Text.RegularExpressions.Regex("(E|e)rratum|(c|C)orrection|(c|C)orrigendum|(e|E)rrata");
            //little tricks to ensure costly operations are done only once
            private void ClearPrivateComparisonValues()
            {
                _ptitleLev = -1.1;
                _ptitleJaro = -1.1;
            }
            private double _ptitleLev = -1.1;
            private double ptitleLev(ItemComparison ic1, ItemComparison ic2)
            {
                if (_ptitleLev == -1.1)
                {
                    _ptitleLev = MagPaperItemMatch.HaBoLevenshtein(ic1.PARENT_TITLE, ic2.PARENT_TITLE);
                }
                return _ptitleLev;
            }
            private double _ptitleJaro = -1.1;
            private double ptitleJaro(ItemComparison ic1, ItemComparison ic2)
            {
                if (_ptitleJaro == -1.1)
                {
                    _ptitleJaro = MagPaperItemMatch.Jaro(ic1.PARENT_TITLE, ic2.PARENT_TITLE);
                }
                return _ptitleJaro;
            }
            
            public Double CompareItems(ItemComparison ic1, ItemComparison ic2)
            {
                ClearPrivateComparisonValues();
                double ret = 0;
                double titleSimilarity = MagPaperItemMatch.HaBoLevenshtein(ic1.TITLE, ic2.TITLE);

                //if titles don't reach threshold return without further tests
                //if (titleSimilarity < 80)
                //return 0;

                // essentially identical in all respects
                // not parsing authors, as this takes more time
                if (ic1.TITLE == ic2.TITLE &&
                    ic1.ABSTRACT == ic2.ABSTRACT &&
                    ic1.PARENT_TITLE == ic2.PARENT_TITLE &&
                    ic1.PAGES == ic2.PAGES &&
                    ic1.DOI == ic2.DOI &&
                    ic1.VOLUME == ic2.VOLUME &&
                    ic1.ISSUE == ic2.ISSUE &&
                    ic1.YEAR == ic2.YEAR &&
                    (MagMakesHelpers.CleanText(ic1.AUTHORS) == MagMakesHelpers.CleanText(ic2.AUTHORS)))
                {
                    return 1;
                }

                // Exact matches on DOI, Volume, first page and title > 90 similar
                if (titleSimilarity > 90 &&
                    ic1.DOI != "" && ic2.DOI != "" &&
                    ic1.VOLUME != "" && ic2.VOLUME != "" &&
                    ic1.GetFirstPage() != "" && ic2.GetFirstPage() != "" &&
                    ic1.DOI == ic2.DOI &&
                    ic1.VOLUME == ic2.VOLUME &&
                    ic1.GetFirstPage() == ic2.GetFirstPage())
                {
                    return 0.99;
                }

                // almost exact matches on title, abstract and journal
                // all fields MUST be present with meaningful length
                if (ic1.TITLE != "" && ic2.TITLE != "" &&
                    titleSimilarity > 90 &&
                    ic1.ABSTRACT.Length > 50 && ic2.ABSTRACT.Length > 50 &&
                    ic1.PARENT_TITLE.Length > 5 && ic2.PARENT_TITLE.Length > 5 && titleSimilarity > 90 &&
                    ptitleLev(ic1, ic2) > 90 &&
                    MagPaperItemMatch.HaBoLevenshtein(ic1.ABSTRACT, ic2.ABSTRACT) > 97) //07/10/2021: up from 90 to account for "correction" references, which might have almost identical abstract to what they are correcting...
                {
                    return 0.98;
                }

                // Title and journal similarity > 80 plus exact matches on 4 other fields where present
                if (
                    titleSimilarity > 80 &&
                    // need to be both journal articles
                    ic1.TYPE_ID == 14 && ic2.TYPE_ID == 14 &&

                    //Journal (parent title) fields must be present in both refs and be a decent match
                    ic1.PARENT_TITLE.Length > 1 && ic2.PARENT_TITLE.Length > 1 //if we have 0 or 1 chars in the journal field, we don't trust it
                    && ptitleLev(ic1, ic2) > 80

                    //DOI, test is passed if they match OR if DOI data is missing in one/both refs 
                    //(we reject this match only if there is a DOI mismatch, not on the basis of missing DOIs)
                    && 
                    (
                        ic1.DOI == ic2.DOI || (ic1.DOI == "" || ic2.DOI == "")
                    )

                    //ISSUE and YEAR clauses are passed if they match or data is missing in both references
                    && ic1.YEAR == ic2.YEAR //if data is missing in BOTH refs, the match still happens
                    && ic1.ISSUE == ic2.ISSUE //if data is missing in BOTH refs, the match still happens

                    //"FIRST PAGE", must be present in both refs and an exact match
                    && (ic1.GetFirstPage() != "" && ic1.GetFirstPage() == ic2.GetFirstPage())
                   )
                {
                    return 0.97;
                }

                double AuthorComparison = CompareAuthors(ic1, ic2);
                // Exact match on DOI, high match on titles and authors
                if (titleSimilarity > 95 &&
                    ic1.DOI != "" && ic2.DOI != "" &&
                    ic1.DOI == ic2.DOI &&
                    AuthorComparison > 0.7)
                {
                    return 0.96;
                }
                
                int volumeMatch = ic1.VOLUME == ic2.VOLUME ? 1 : 0;
                int pageMatch = ic1.GetFirstPage() == ic2.GetFirstPage() ? 1 : 0;
                int yearMatch = ic1.YEAR == ic2.YEAR ? 1 : 0;
                int doiMatch = ic1.DOI.Length > 8 && ic1.DOI == ic2.DOI ? 1 : 0;
                double journalJaro = ic1.PARENT_TITLE != null ? ptitleLev(ic1, ic2) : 0;
                //double allAuthorsJaro = MagPaperItemMatch.Jaro(reader["AUTHORS"].ToString().Replace(",", " "),
                //readerCandidates["AUTHORS"].ToString().Replace(",", " "));
                //double allAuthorsCompare = CompareAuthors(ic1, ic2);

                // If none of the above, just calculate the basic similarity score. One includes boost for exact match on DOI
                if (doiMatch == 1)
                {
                    ret = ((titleSimilarity / 100 * 1.87) + //(titleSimilarity / 100 * 1.75) +
                        (volumeMatch * 0.5) +
                        (pageMatch * 0.66) + //(pageMatch * 0.5)
                        (yearMatch * 0.5) +
                        (journalJaro / 100 * 1) +
                        (doiMatch) +
                        (AuthorComparison * 1)) / 6.53; //(AuthorComparison * 1.25)
                }
                else
                {
                    // if no doi match, then the standard similarity algorithm applies
                    ret = ((titleSimilarity / 100 * 1.87) + //(titleSimilarity / 100 * 1.75) +
                        (volumeMatch * 0.5) +
                        (pageMatch * 0.66) + //(pageMatch * 0.5)
                        (yearMatch * 0.5) +
                        (journalJaro / 100 * 1) +
                        (AuthorComparison * 1)) / 5.53; //(AuthorComparison * 1.25)
                }

                // Final sanity checks to prevent auto-matches on questionable records

                // if pages and year are present, but don't match, this needs manual check
                if ((ic1.PAGES != "" && ic2.PAGES != "" && ic1.GetFirstPage() != ic2.GetFirstPage()) &&
                    (ic1.YEAR != "" && ic2.YEAR != "" && ic1.YEAR != ic2.YEAR))
                    ret = Math.Min(ret, 0.75);

                // check for erratum
                bool ic1HasErratum = Comparator.ErratumRegex.IsMatch(ic1.TITLE);
                bool ic2HasErratum = Comparator.ErratumRegex.IsMatch(ic2.TITLE);
                if ((ic1HasErratum && !ic2HasErratum)
                    || (ic2HasErratum && !ic1HasErratum))
                    ret = Math.Min(ret, 0.75);
                //OLD version below, when we only looked for "erratum". New regex will match 4 labels for the same effect
                //if ((ic1.TITLE.IndexOf("erratum") > -1 && ic2.TITLE.IndexOf("erratum") < 0) ||
                //    (ic2.TITLE.IndexOf("erratum") > -1 && ic1.TITLE.IndexOf("erratum") < 0))
                //    ret = Math.Min(ret, 0.75);

                // if first page numbers different AND journal different then can't be an auto-match
                if ((ic1.PARENT_TITLE != "" && ic2.PARENT_TITLE != "") &&
                    ptitleLev(ic1, ic2) < 75 &&
                    (ic1.PAGES != "" && ic2.PAGES != "") &&
                    ic1.GetFirstPage() != ic2.GetFirstPage())
                {
                    ret = Math.Min(ret, 0.75);
                }
                // if an item is a report and titles don't match exactly then we flag as low confidence
                if ((ic1.PARENT_TITLE == "" || ic2.PARENT_TITLE == "") &&
                    (ic1.PAGES == "" || ic2.PAGES == "") &&
                    (ic1.VOLUME == "" || ic2.VOLUME == "") &&
                    (titleSimilarity != 100))
                {
                    ret = Math.Min(ret, 0.77);
                }

                return ret;
            }

            private bool CompareDOIs(string doi1, string doi2)
            {
                // compare using substrings to take account of whether or not
                // the 'http' bit is included in one but not the other
                if (doi1.Length > doi2.Length)
                {
                    if (doi1.IndexOf(doi2) > 0)
                        return true;
                }
                else
                {
                    if (doi2.IndexOf(doi1) > 0)
                        return true;
                }
                return false;
            }
            private double CompareAuthors(ItemComparison ic1, ItemComparison ic2)
            {
                double ret = 0;
                int totalAuthors = 0;
                if (ic1.GetAuthors() != null) totalAuthors = ic1.GetAuthors().Count;
                else return 0.0;//can't compare authors if one ref has no authors...
                int MaxAuthors = totalAuthors;
                //Added by SG on 22/06/2022: if number of authors differs between the two refs, but the 2 refs _are_ duplicates,
                //then usually we want to consider the smaller value (of "total authors"), because the higher number is likely to be due to:
                //1. Authors parsed wrong, which happens when some authors have many names, or when org names pollute the authors field.
                //2. One ref truncates authors with "et al." (or similar) which happens for refs with many authors, sometimes.
                //In both cases we should consider the smaller number, as otherwise we add "penalty" to bad parsing or the truncation
                //Moreover, this trick helps to ensure we'll get Symmetric results (i.e. comparing RefA with RefB returns the same vaue of comparing RefB with RefA)
                if (ic2.GetAuthors() == null)
                {
                    return 0.0; //ditto, nothing to compare
                }
                else if (ic2.GetAuthors().Count < totalAuthors)
                {
                    totalAuthors = ic2.GetAuthors().Count;
                    //given that ic2 has less authors than ic1 we also want to make sure ic2 goes in the "outer" foreach cycle below,
                    //as this cycle defines the "maximum" number of successful matches and we don't want this value to be more than our totalAuthors measure;
                    //otherwise we can't guarantee that results will be simmetric, because N of authors would influence how many chanches to "match" we give to a pair of refs
                    ItemComparison tmp = ic1;
                    ic1 = ic2;
                    ic2 = tmp;
                } else
                {
                    MaxAuthors = ic2.GetAuthors().Count;//it's more or equal to n of auth. in ic1
                }

                double lastWithLast = 0;
                double firstWithLast = 0;
                foreach (AutH author in ic1.GetAuthors())
                {
                    foreach (AutH auth2 in ic2.GetAuthors())
                    {
                        double compareResult = 0;
                        if (author.LastName.Length > 1 && author.LastName == auth2.LastName) compareResult = 1;
                        else compareResult = doCompareAuthors(MagMakesHelpers.CleanText(author.LastName, true), MagMakesHelpers.CleanText(auth2.LastName, true));
                        if (compareResult == 0)
                        {//Added by SG 22/06/2022
                            //This accounts for parsing problems, when either "first" or "last" (name) fields are filled with an intial.
                            //If this happens in one or the other reference (not both), and one of the other ref _also_ swapped "last" and "first" names,
                            //then the given author _will fail_ to match, even when it IS the same author, unless we add the line below (and similar, in the 2nd cycle below).
                            compareResult = doCompareAuthors(MagMakesHelpers.CleanText(author.LastName, true), MagMakesHelpers.CleanText(auth2.FullName, true));
                            if (compareResult == 0)
                            {
                                compareResult = doCompareAuthors(MagMakesHelpers.CleanText(author.FullName, true), MagMakesHelpers.CleanText(auth2.LastName, true));
                            }
                        }
                        if (compareResult > 0)
                        {
                            lastWithLast += compareResult;
                            break;
                        }
                    }
                }
                if (lastWithLast <= totalAuthors)//no need to compare last with first if authors match perfectly already
                {
                    foreach (AutH author in ic1.GetAuthors())
                    {
                        foreach (AutH auth2 in ic2.GetAuthors())
                        {
                            double compareResult = 0;
                            if (author.LastName.Length > 1 && author.LastName == auth2.FirstName) compareResult = 1;
                            else compareResult = doCompareAuthors(MagMakesHelpers.CleanText(author.LastName, true), MagMakesHelpers.CleanText(auth2.FirstName, true));
                            if (compareResult == 0)
                            {//Added by SG 22/06/2022
                                //see above! Mirror image of the addition in the previous cycle.
                                compareResult = doCompareAuthors(MagMakesHelpers.CleanText(author.FullName, true), MagMakesHelpers.CleanText(auth2.FirstName, true));
                                if (compareResult == 0)
                                {
                                    compareResult = doCompareAuthors(MagMakesHelpers.CleanText(author.FirstName, true), MagMakesHelpers.CleanText(auth2.FullName, true));
                                }
                            }
                            if (compareResult > 0)
                            {
                                firstWithLast += compareResult;
                                break;
                            }
                        }
                    }
                }
                ret = Math.Max(lastWithLast / totalAuthors, firstWithLast / totalAuthors);
                if (ret > 1) ret = 1;//just in case "cycling" found more matches than the total we could expect (might, if the cycles have a bug!)

                //finally, we adjust in case we had fewer authors in one ref
                if (MaxAuthors != totalAuthors)
                {
                    //baseMaxScore goes down proportionally depending on authors count difference between the 2 refs
                    //it's 1 when author counts are equal, approaching 0 as the difference increases
                    //as such, we could use it to account for the missing (by definition) matches, but we don't want to
                    //because sometimes missing some authors doesn't mean much.
                    double baseMaxScore = 1 - (((double)MaxAuthors - totalAuthors) / MaxAuthors);
                    double adjustedBaseMaxScore = baseMaxScore + ((1 - baseMaxScore) * 0.33);
                    //thus adjustedBaseMaxScore increases by "one third of (1-baseMaxScore)". So, it gets closer to 1, but never quite there.
                    //overall, we are "guessing" that there is a 0.33 probability that a single missing author in one of the refs doesn't mean they are not duplicates.

                    ret = ret * adjustedBaseMaxScore;//thus, somewhat less.
                    //EXAMPLE: ref1 has 10 authors, ref2 has 9 authors.
                    //All authors in ref2 match. Without _any_ adjustment, score would be 1
                    //adjusting with baseMaxScore would give us 0.9, meaning that the "missing" author
                    //penalises as much as an author that did exist (both refs had 10 authors) and does not match, which is not right.
                    //instead we still penalise, but not as much as a genuine author mismatch
                    //returning 0.933 instead of 0.9
                }

                return ret > 0.3 ? ret : 0;
            }
            private double doCompareAuthors(string a1, string a2)
            {
                if (a1.Length < 2 || a2.Length < 2)
                    return 0;
                if (a1.Length > a2.Length)
                {
                    if (a1.IndexOf(a2) > -1)
                        return 1;
                    else
                        return 0;
                }
                if (a2.IndexOf(a1) > -1)
                    return 1;
                else
                    return 0;
            }


        }
#endif
    }



    [Serializable]
    public class GroupListSelectionCriteria : Csla.CriteriaBase<GroupListSelectionCriteria>
    {
        private static PropertyInfo<int> GroupIdProperty = RegisterProperty<int>(typeof(GroupListSelectionCriteria), new PropertyInfo<int>("GroupId", "GroupId"));
        public int GroupId
        {
            get { return ReadProperty(GroupIdProperty); }
        }
        private static PropertyInfo<string> ItemIdsProperty = RegisterProperty<string>(typeof(GroupListSelectionCriteria), new PropertyInfo<string>("ItemIds", "ItemIds"));
        public string ItemIds
        {
            get { return ReadProperty(ItemIdsProperty); }
        }

        public GroupListSelectionCriteria(Type type, int GroupId)
        {
            LoadProperty(GroupIdProperty, GroupId);
            LoadProperty(ItemIdsProperty, "0");
        }

        public GroupListSelectionCriteria(Type type, string ItemIds)
        {
            LoadProperty(ItemIdsProperty, ItemIds);
            LoadProperty(GroupIdProperty, 0);
        }
        public GroupListSelectionCriteria(int GroupId, string ItemIds)
        {
            if (ItemIds == "" || ItemIds == null)
            {
                LoadProperty(GroupIdProperty, GroupId);
                LoadProperty(ItemIdsProperty, "0");
            } else if (GroupId < 1) {
                LoadProperty(ItemIdsProperty, ItemIds);
                LoadProperty(GroupIdProperty, 0);
            }
            else
            {
                LoadProperty(GroupIdProperty, GroupId);
                LoadProperty(ItemIdsProperty, ItemIds);
            }
        }

#if !SILVERLIGHT 
        public GroupListSelectionCriteria() { }
#endif
    }
}
