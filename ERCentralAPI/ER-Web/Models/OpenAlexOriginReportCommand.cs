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
using Csla.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Humanizer;




#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class OpenAlexOriginReportCommand : CommandBase<OpenAlexOriginReportCommand>
    {
        public OpenAlexOriginReportCommand() { }

        private Int64 _attId;
        private OaOriginSummary _Summary  = new OaOriginSummary();

        public List<MagAutoUpdateRun> MagAutoUpdateRunList = new List<MagAutoUpdateRun>();
        public List<int> MagAutoUpdateRunListCounts = new List<int>();
        public List<MagRelatedPapersRun> MagRelatedSearches = new List<MagRelatedPapersRun>();
        public List<int> MagRelatedSearchesCounts = new List<int>();
        public List<MagSearch> MagTextSearches = new List<MagSearch>();
        public List<int> MagTextSearchesCounts = new List<int>();
        public List<OaOriginReportItem> Items = new List<OaOriginReportItem>();

        public OpenAlexOriginReportCommand(Int64 attId)
        {
            _attId = attId;
        }

        public OaOriginSummary Summary
        {
            get { return _Summary; }
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_attId", _attId);
            //info.AddValue("_numCodings", _numCodings);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _attId = info.GetValue<Int64>("_attId");
            //_numCodings = info.GetValue<int>("_numCodings");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            List<MagSearch> allMagTextSearches = new List<MagSearch>();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OpenAlexOriginReport", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ReviewId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@attributeid", _attId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        //first reader is one summary line
                        if (reader.Read()) 
                        {
                            _Summary = new OaOriginSummary(reader);
                        }
                        reader.NextResult();
                        //second reader is the list of auto update runs found
                        while (reader.Read()) 
                        {
                            MagAutoUpdateRunList.Add(MagAutoUpdateRun.GetMagAutoUpdateRun(reader));
                            MagAutoUpdateRunListCounts.Add(reader.GetInt32("ITEMS_COUNT"));
                        }
                        reader.NextResult();
                        //3rd reader is the list of Related searches found
                        while (reader.Read()) 
                        { 
                            MagRelatedSearches.Add(MagRelatedPapersRun.GetMagRelatedPapersRun(reader));
                            MagRelatedSearchesCounts.Add(reader.GetInt32("ITEMS_COUNT"));
                        }
                        reader.NextResult();
                        //4th reader contains all TB_MAG_SEARCH records that might contain our papers
                        while (reader.Read())
                        {
                            allMagTextSearches.Add(MagSearch.GetMagSearchWithIds(reader));//we'll use and reduce this list later
                        }
                        reader.NextResult();
                        //5th and last reader contains all items (sometimes multiple row per item)
                        Int64 previousItemId = 0, currentItemId = 0, PaperId = 0;
                        OaOriginReportItem tmpItem = new OaOriginReportItem();
                        while (reader.Read())
                        {
                            currentItemId = reader.GetInt64("item_id");
                            if (previousItemId != currentItemId)
                            {
                                tmpItem = new OaOriginReportItem(reader);
                                Items.Add(tmpItem);
                                previousItemId = currentItemId;
                            }
                            PaperId = reader.GetInt64("PaperId");
                            if (PaperId > 0 && !tmpItem.OpenAlexPaperId.Contains(PaperId))
                            {
                                tmpItem.OpenAlexPaperId.Add(PaperId);
                            }
                            if (reader.GetBoolean("IsInAU") == true)
                            {
                                tmpItem.AutoUpdateResults.Add(reader.GetInt32("MAG_AUTO_UPDATE_RUN_ID"));
                            }
                            if (reader.GetBoolean("IsInRS") == true)
                            {
                                tmpItem.RelatedSearches.Add(reader.GetInt32("MAG_RELATED_RUN_ID"));
                            }
                        }
                    }
                }
                connection.Close();
            }
            ProcessTextSearches(allMagTextSearches);
            Summary.RecomputeFinalSummaryFigures(Items);
        }
        /// <summary>
        /// to keep the SQL fast enough, we process IDs for text searches here
        ///aims:
        ///- remove text searches that do not contain the Items we care about
        ///- count match items to searches
        ///process:
        ///1. represent searches as an ID paired with a list of paperIDs
        ///2. find our Items in the lists of paperIDs to see where they appear
        ///3. put data in the properties we'll return
        /// </summary>
        /// <param name="allMagTextSearches"></param>
        private void ProcessTextSearches(List<MagSearch> allMagTextSearches)
        {
            //1. represent searches as an ID paired with a list of paperIDs
            List<KeyValuePair<int, List<long>>> SearchesAndPaperIds = new List<KeyValuePair<int, List<long>>>();
            foreach(MagSearch search in allMagTextSearches)
            {
                List<long> PaperIds = new List<long>();
                string[] splitted = search.SearchIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                long tId;
                foreach (string pid in splitted)
                {
                    if (long.TryParse(pid, out tId)) PaperIds.Add(tId);
                }
                SearchesAndPaperIds.Add(new KeyValuePair<int, List<long>>(search.MagSearchId, PaperIds));
            }
            //2. find our Items in the lists of paperIDs to see where they appear
            List<int> SearchIdsToKeep = new List<int>();
            foreach(OaOriginReportItem item in Items)
            {
                foreach(long Pid in item.OpenAlexPaperId)
                {
                    List<KeyValuePair<int,List<long>>> found = SearchesAndPaperIds.FindAll(f => f.Value.Contains(Pid));
                    foreach (KeyValuePair<int, List<long>> fS in found)
                    {
                        if (!SearchIdsToKeep.Contains(fS.Key))
                        {
                            SearchIdsToKeep.Add(fS.Key);
                            MagSearch? toAdd = allMagTextSearches.Find(f => f.MagSearchId == fS.Key);
                            if (toAdd != null)
                            {
                                allMagTextSearches.Remove(toAdd);
                                MagTextSearches.Add(toAdd);//3. put data in the properties we'll return
                            }
                            MagTextSearchesCounts.Add(1);//3. put data in the properties we'll return
                        }
                        else
                        {
                            int Index = MagTextSearches.FindIndex(f => f.MagSearchId == fS.Key);
                            if (Index != -1) MagTextSearchesCounts[Index]++;//3. put data in the properties we'll return
                        }
                        if (!item.TextSearches.Contains(fS.Key))
                            item.TextSearches.Add(fS.Key); //3.put data in the properties we'll return
                    }
                }
            }
        }

#endif
    }
    public class OaOriginReportItem
    {
        public Int64 ItemId { get; private set; }
        public List<Int64> OpenAlexPaperId { get; private set; } = new List<Int64>();
        public string Title { get; private set; } = string.Empty;
        public string ShortTitle { get; private set; } = string.Empty;
        public string SourceName { get; private set; } = string.Empty;

        public List<int> RelatedSearches { get; private set; } = new List<int>();
        public List<int> TextSearches { get; private set; } = new List<int>();

        public List<int> AutoUpdateResults { get; private set; } = new List<int>();
        public OaOriginReportItem() 
        {
            ItemId = 0;
        }
        public OaOriginReportItem(SafeDataReader reader) 
        {
            ItemId = reader.GetInt64("item_id");
            long tmp = reader.GetInt64("PaperId");
            if (tmp > 0) OpenAlexPaperId.Add(tmp);
            Title = reader.GetString("TITLE");
            ShortTitle = reader.GetString("SHORT_TITLE");
            SourceName = reader.GetString("SOURCE_NAME");
        }
    }
    public class OaOriginSummary
    {
        public int TotalItems { get; private set; } = 0;
        public int Matched { get; private set; } = 0;
        public int NotMatched { get; private set; } = 0;
        public int InAutoUpdateResults { get; private set; } = 0;
        public int InRelatedSearches { get; private set; } = 0;
        public int InTextSearches { get; private set; } = 0;
        public int InBothAuAndRs { get; private set; } = 0;
        public int InBothAuAndTs { get; private set; } = 0;
        public int InBothTsAndRs { get; private set; } = 0;
        public int InAll3 { get; private set; } = 0;
        public int OtherMatched { get; private set; } = 0;
        public OaOriginSummary() { }
        public OaOriginSummary(SafeDataReader reader) 
        {
            TotalItems = reader.GetInt32("Computed Total");
            Matched = reader.GetInt32("Matched");
            NotMatched = reader.GetInt32("Not Matched");
            InAutoUpdateResults = reader.GetInt32("In AutoUpdate results");
            InRelatedSearches = reader.GetInt32("In Related Searches");
            InBothAuAndRs = reader.GetInt32("in both");
            OtherMatched = reader.GetInt32("Other");
        }
        /// <summary>
        /// We need to recompute (and since we can, check) figures because:
        /// figures from TextSearches are not returned by SQL as they are not-known at that point!
        /// So we'll check the figures we have, and compute what's missing here.
        /// </summary>
        /// <param name="Items"></param>
        /// <exception cref="Exception"></exception>
        public void RecomputeFinalSummaryFigures(List<OaOriginReportItem> Items)
        {

            List<long> InAu = new List<long>();
            List<long> InRs = new List<long>();
            List<long> InTs = new List<long>();
            int localInBothAuAndRs = 0;
            int localInBothAuAndTs = 0;
            int localInBothTsAndRs = 0;
            int localInAll3 = 0;
            List<OaOriginReportItem> tList = Items.FindAll(f => f.AutoUpdateResults.Count > 0);
            foreach (OaOriginReportItem itm in tList)
            {
                if (!InAu.Contains(itm.ItemId)) InAu.Add(itm.ItemId);
            }
            if (InAutoUpdateResults != InAu.Count) throw new Exception("Report figures are inconsistent, error found in number of Items in AutoUpdate results");
            tList = Items.FindAll(f => f.RelatedSearches.Count > 0);
            foreach (OaOriginReportItem itm in tList)
            {
                if (!InRs.Contains(itm.ItemId)) InRs.Add(itm.ItemId);
            }
            if (InRelatedSearches != InRs.Count) throw new Exception("Report figures are inconsistent, error found in number of Items in Related Searches");
            tList = Items.FindAll(f => f.TextSearches.Count > 0);
            foreach (OaOriginReportItem itm in tList)//now we complicate things. We add Ids to InTs and ALSO check if they appear also in the other two lists, in the same loop
            {
                if (!InTs.Contains(itm.ItemId))
                {
                    InTs.Add(itm.ItemId);
                    if (InAu.Contains(itm.ItemId))
                    {
                        localInBothAuAndTs++;//this item is in both text searches and auto updates
                        if (InRs.Contains(itm.ItemId))
                        {
                            localInBothTsAndRs++;//this item is in both text searches and related searches
                            localInAll3++;//but it's also Au, so it's in all 3!
                        }
                    }
                    else
                    {
                        if (InRs.Contains(itm.ItemId)) localInBothTsAndRs++;//this item is in both text searches and related searches (but not in auto updates)
                    }
                }
            }
            InTextSearches = InTs.Count;
            //we've found all items that are in both (Au & Ts), both (Rs & Ts) and "all 3", so we only need to check what's in both (Au & Rs)
            foreach (long itId in InAu)
            {
                if (InRs.Contains(itId)) localInBothAuAndRs++;
            }

            if (localInBothAuAndRs != InBothAuAndRs) throw new Exception("Report figures are inconsistent, error found in number of Items in both AU and RS results");
            InBothAuAndTs = localInBothAuAndTs;
            InBothTsAndRs = localInBothTsAndRs;
            InAll3 = localInAll3;
            int remainingMatchedCount = Items.FindAll(f => f.OpenAlexPaperId.Count > 0 
                                                        && f.AutoUpdateResults.Count == 0
                                                        && f.RelatedSearches.Count == 0
                                                        && f.TextSearches.Count == 0
                                                        ).Count;
            OtherMatched = remainingMatchedCount;
        }
    }
}
