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

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagItemPaperInsertCommand : CommandBase<MagItemPaperInsertCommand>
    {

#if SILVERLIGHT
    public MagItemPaperInsertCommand(){}
#else
        public MagItemPaperInsertCommand() { }
#endif

        private string _PaperIds;
        private int _NImported;
        private string _SourceOfIds;
        private int _MagRelatedRunId;
        private string _MagSearchDescription;
        private string _MagSearchText;
        private int _MagAutoUpdateRunId;
        private string _OrderBy;
        private double _AutoUpdateScore;
        private double _StudyTypeClassifierScore;
        private double _UserClassifierScore;
        private int _TopN;
        private string _FilterJournal;
        private string _FilterDOI;
        private string _FilterURL;

        public int NImported
        {
            get
            {
                return _NImported;
            }
            set
            {
                _NImported = value;
            }
        }

        public MagItemPaperInsertCommand(string PaperIds, string SourceOfIds, int MagRelatedRunId,
             int MagAutoUpdateRunId, string OrderBy, double AutoUpdateScore, double StudyTypeClassifierScore, double UserClassifierScore,
            int TopN, string FilterJournal, string FilterDOI, string FilterURL, string MagSearchText = "", string MagSearchDesc = "")
        {
            _PaperIds = PaperIds;
            _SourceOfIds = SourceOfIds;
            _MagRelatedRunId = MagRelatedRunId;
            _MagSearchDescription = MagSearchDesc;
            _MagSearchText = MagSearchText;
            _MagAutoUpdateRunId = MagAutoUpdateRunId;
            _OrderBy = OrderBy;
            _AutoUpdateScore = AutoUpdateScore;
            _StudyTypeClassifierScore = StudyTypeClassifierScore;
            _UserClassifierScore = UserClassifierScore;
            _TopN = TopN;
            _FilterJournal = FilterJournal;
            _FilterDOI = FilterDOI;
            _FilterURL = FilterURL;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_PaperIds", _PaperIds);
            info.AddValue("_NImported", _NImported);
            info.AddValue("_SourceOfIds", _SourceOfIds);
            info.AddValue("_MagRelatedRunId", _MagRelatedRunId);
            info.AddValue("_MagSearchDescription", _MagSearchDescription);
            info.AddValue("_MagSearchText", _MagSearchText);
            info.AddValue("_MagAutoUpdateRunId", _MagAutoUpdateRunId);
            info.AddValue("_OrderBy", _OrderBy);
            info.AddValue("_AutoUpdateScore", _AutoUpdateScore);
            info.AddValue("_StudyTypeClassifierScore", _StudyTypeClassifierScore);
            info.AddValue("_UserClassifierScore", _UserClassifierScore);
            info.AddValue("_TopN", _TopN);
            info.AddValue("_FilterJournal", _FilterJournal);
            info.AddValue("_FilterDOI", _FilterDOI);
            info.AddValue("_FilterURL", _FilterURL);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _PaperIds = info.GetValue<string>("_PaperIds");
            _NImported = info.GetValue<int>("_NImported");
            _SourceOfIds = info.GetValue<string>("_SourceOfIds");
            _MagRelatedRunId = info.GetValue<int>("_MagRelatedRunId");
            _MagSearchDescription = info.GetValue<string>("_MagSearchDescription");
            _MagSearchText = info.GetValue<string>("_MagSearchText");
            _MagAutoUpdateRunId = info.GetValue<int>("_MagAutoUpdateRunId");
            _OrderBy = info.GetValue<string>("_OrderBy");
            _AutoUpdateScore = info.GetValue<double>("_AutoUpdateScore");
            _StudyTypeClassifierScore = info.GetValue<double>("_StudyTypeClassifierScore");
            _UserClassifierScore = info.GetValue<double>("_UserClassifierScore");
            _TopN = info.GetValue<int>("_TopN");
            _FilterJournal = info.GetValue<string>("_FilterJournal");
            _FilterDOI = info.GetValue<string>("_FilterDOI");
            _FilterURL = info.GetValue<string>("_FilterURL");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            List<string> AlreadyUsedPaperIds = new List<string>();
            List<string> AllIDsToSearch = new List<string>();
            IncomingItemsList incomingList = IncomingItemsList.NewIncomingItemsList();
            incomingList.SourceDB = "Microsoft Academic Graph";
            incomingList.HasMAGScores = true;
            incomingList.IsFirst = true; incomingList.IsLast = true;
            incomingList.IncomingItems = new MobileList<ItemIncomingData>();

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                // First query populates a list of PaperIds that are already in the review (we don't want to import duplicates)
                using (SqlCommand command = new SqlCommand("st_MagGetCurrentlyUsedPaperIds", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            AlreadyUsedPaperIds.Add(reader["PaperId"].ToString());
                        }
                    }
                }

                // There are currently 4 types of import: RelatedPapersSearch, AutoUpdateRun, SelectedPapers and MagSearchResults.
                // All produce the 'incominglist' object, which is then saved in the normal way.
                // The first two come from lists of IDs (either in database or from client). We'll deal with these three first.

                // 1. Related papers search - here we have a list of IDs in the database - need to retrieve and then ensure we aren't already using any in this review
                if (_SourceOfIds == "RelatedPapersSearch") 
                {
                    incomingList.SourceName = "Automated search: " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToLongTimeString();
                    using (SqlCommand command = new SqlCommand("st_MagItemMagRelatedPaperInsert", connection))
                    {
                        command.CommandTimeout = 2000; // 2000 secs = about 2 hours? (JT - not sure why we have the long timeout?)
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", _MagRelatedRunId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                string currentPaperId = reader["PaperId"].ToString();
                                if (!AlreadyUsedPaperIds.Exists(element => element == currentPaperId))
                                {
                                    AllIDsToSearch.Add(currentPaperId);
                                }
                            }
                        }
                    }
                }
                // 2. AutoUpdateRun - here we also have a list of IDs in the database - need to retrieve and then ensure we aren't already using any in this review
                if (_SourceOfIds == "AutoUpdateRun")
                {
                    incomingList.SourceName = "Auto-update imported on: " + DateTime.Now.ToShortDateString() +
                        ". Top: " + this._TopN.ToString() + " ordered by " + _OrderBy + " with thresholds: AutoUpdate: " +
                        _AutoUpdateScore.ToString() + ", Study type classifier: " + _StudyTypeClassifierScore.ToString() +
                        ", user built classifier: " + _UserClassifierScore.ToString();
                    using (SqlCommand command = new SqlCommand("st_MagAutoUpdateRunResults", connection))
                    {
                        command.CommandTimeout = 2000; // 2000 secs = about 2 hours? (JT - not sure why we have the long timeout?)
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MagAutoUpdateRunId", _MagAutoUpdateRunId));
                        command.Parameters.Add(new SqlParameter("@OrderBy", _OrderBy));
                        command.Parameters.Add(new SqlParameter("@AutoUpdateScore", _AutoUpdateScore));
                        command.Parameters.Add(new SqlParameter("@StudyTypeClassifierScore", _StudyTypeClassifierScore));
                        command.Parameters.Add(new SqlParameter("@UserClassifierScore", _UserClassifierScore));
                        command.Parameters.Add(new SqlParameter("@TopN", _TopN));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                string currentPaperId = reader["PaperId"].ToString();
                                if (!AlreadyUsedPaperIds.Exists(element => element == currentPaperId))
                                {
                                    AllIDsToSearch.Add(currentPaperId);
                                }
                            }
                        }
                    }
                }
                // 3. Selected papers - we have a list of IDs from the client, so just need to check they aren't in the DB already
                if (_SourceOfIds == "SelectedPapers")
                {
                    foreach (string PaperId in _PaperIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!AlreadyUsedPaperIds.Exists(element => element == PaperId))
                        {
                            AllIDsToSearch.Add(PaperId);
                        }
                    }
                    incomingList.SourceName = "Selected items from MAG on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToLongTimeString();
                    incomingList.SearchStr = _PaperIds;
                }
                // then we look up the list of IDs from 1, 2 and 3 in MAKES. Doing in batches of 100 as this is much quicker than one at a time
                int count = 0;
                while (count < AllIDsToSearch.Count)
                {
                    string query = "";
                    for (int i = count; i < AllIDsToSearch.Count && i < count + 100; i++)
                    {
                        if (query == "")
                        {
                            query = "Id=" + AllIDsToSearch[i].ToString();
                        }
                        else
                        {
                            query += ",Id=" + AllIDsToSearch[i].ToString();
                        }
                    }
                    MagMakesHelpers.PaperMakesResponse resp = MagMakesHelpers.EvaluateExpressionNoPagingWithCount("OR(" + query + ")", "100");

                    foreach (MagMakesHelpers.PaperMakes pm in resp.entities)
                    {
                        MagPaper mp = MagPaper.GetMagPaperFromPaperMakes(pm, null);
                        if (mp.PaperId > 0 && PaperPassesFilters(mp))
                        {
                            incomingList.IncomingItems.Add(GetIncomingItemFromMagPaper(mp));
                        }
                    }
                    count += 100;
                }

                // 4. The final type of import is from a search. This is different, as we have the MAKES query to run and just
                // need to cycle through its pages to get the data. There are TWO types of search result: one that imports
                // everything from a search; and the other, that imports only new IDs from the last deployed MAKES / MAG.
                List<string> IDsFilteredList = new List<string>();
                if (_SourceOfIds == "MagSearchResults" || _SourceOfIds == "MagSearchResultsLatestMAG")
                {
                    incomingList.SourceName = _MagSearchDescription;
                    incomingList.SearchStr = _MagSearchText;
                    MagCurrentInfo currentMAGInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("LIVE");
                    int totalHits = 0;
                    int numPages = 1;
                    // Get the total from current MAKES - don't rely on the number in the search, as it may change between MAKES versions
                    MagMakesHelpers.MakesCalcHistogramResponse resp = MagMakesHelpers.CalcHistoramCount(_MagSearchText);
                    foreach (MagMakesHelpers.histograms hs in resp.histograms)
                    {
                        if (hs.attribute == "Id")
                        {
                            totalHits = hs.total_count;
                            break;
                        }
                    }
                    if (totalHits > 100)
                    {
                        numPages = (totalHits / 100) + 1;
                    }
                    // We already have a query for MAKES, so just need to run through each page
                    for (int c = 0; c < numPages; c++)
                    {
                        MagMakesHelpers.PaperMakesResponse res = MagMakesHelpers.EvaluateExpressionWithPaging(_MagSearchText, "100", (c * 100).ToString());
                        if (_SourceOfIds == "MagSearchResults") // all items not in the review just go into the incomingList
                        {
                            foreach (MagMakesHelpers.PaperMakes pm in res.entities)
                            {
                                // Only import those that aren't already in the review
                                if (!AlreadyUsedPaperIds.Exists(element => element == pm.Id.ToString()))
                                {
                                    MagPaper mp = MagPaper.GetMagPaperFromPaperMakes(pm, null);

                                    if (mp.PaperId > 0 && PaperPassesFilters(mp))
                                    {
                                        incomingList.IncomingItems.Add(GetIncomingItemFromMagPaper(mp));
                                    }
                                }
                            }
                        }
                        else // need to filter to only those PaperIds in the last version of MAG
                        {
                            string IDs = "";
                            string IDsFiltered = "";
                            IDsFilteredList.Clear();
                            foreach (MagMakesHelpers.PaperMakes pm in res.entities)
                            {
                                IDs += pm.Id.ToString() + ",";
                            }
                            IDs.TrimEnd(',');
                            using (SqlCommand command = new SqlCommand("st_MagSearchFilterToLatest", connection))
                            {
                                command.CommandTimeout = 500; // should make this a nice long time
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@MagVersion", currentMAGInfo.MagFolder));
                                command.Parameters.Add(new SqlParameter("@IDs", IDs));
                                using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                                {
                                    while (reader.Read())
                                    {
                                        IDsFilteredList.Add(reader["PaperId"].ToString());
                                    }
                                }
                            }
                            if (IDsFilteredList.Count > 0)
                            {
                                foreach (MagMakesHelpers.PaperMakes pm in res.entities)
                                {
                                    // Only import those that are in latest MAG and not in review
                                    if (IDsFilteredList.Exists(element => element == pm.Id.ToString()) &&
                                        !AlreadyUsedPaperIds.Exists(element => element == pm.Id.ToString()))
                                    {
                                        MagPaper mp = MagPaper.GetMagPaperFromPaperMakes(pm, null);
                                        if (mp.PaperId > 0 && PaperPassesFilters(mp))
                                        {
                                            incomingList.IncomingItems.Add(GetIncomingItemFromMagPaper(mp));
                                        }
                                    }
                                }
                            }
                        }
                        // this is a practical limit for what the importing routine can cope with
                        if (incomingList.IncomingItems.Count > 20000)
                        {
                            break;
                        }
                    }
                }

                incomingList.buildShortTitles();
                _NImported = incomingList.IncomingItems.Count;
                incomingList = incomingList.Save();
                if (_MagRelatedRunId > 0)
                {
                    using (SqlCommand command = new SqlCommand("st_MagRelatedRun_Update", connection))
                    {
                        command.CommandTimeout = 500; // should make this a nice long time
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", _MagRelatedRunId));//Imported
                        command.Parameters.Add(new SqlParameter("@STATUS", "Imported"));
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            
            }
        }

        private ItemIncomingData GetIncomingItemFromMagPaper(MagPaper mp)
        {
            TextInfo myTI = new CultureInfo("en-GB", false).TextInfo;
            ItemIncomingData tItem = ItemIncomingData.NewItem();
            tItem.AuthorsLi = new AuthorsHandling.AutorsList();
            tItem.pAuthorsLi = new AuthorsHandling.AutorsList();
            string[] authors = mp.Authors.Split(',');
            for (int x = 0; x < authors.Count(); x++)
            {
                AutH author = NormaliseAuth.singleAuth(authors[x], x + 1, 0, true);
                if (author != null)
                {
                    tItem.AuthorsLi.Add(author);
                }
            }
            tItem.OldItemId = mp.PaperId.ToString();

            if (mp.Year >= 0) tItem.Year = mp.Year.ToString();
            if (mp.Journal != null) tItem.Parent_title = mp.Journal != null ? myTI.ToTitleCase(mp.Journal) : "";
            if (mp.Volume != null) tItem.Volume = mp.Volume;
            if (mp.Issue != null) tItem.Issue = mp.Issue;
            if ((mp.FirstPage != null && mp.FirstPage.Length > 0)
                ||
                (mp.LastPage != null && mp.LastPage.Length > 0)) tItem.Pages = mp.FirstPage + "-" + mp.LastPage;
            if (mp.OriginalTitle != null) tItem.Title = mp.OriginalTitle;
            if (mp.DOI != null) tItem.DOI = mp.DOI;
            if (mp.Abstract != null) tItem.Abstract = mp.Abstract;
            tItem.SearchText = Item.ToShortSearchText(mp.PaperTitle);
            //if (mp.URLs != null && mp.URLs.Length > 0)
            //{
            //    string[] urls = mp.URLs.Split(';');
            //    if (urls.Length > 0) tItem.Url = urls[0];
            //}
            tItem.Url = "https://academic.microsoft.com/paper/" + mp.PaperId.ToString();
            if (mp.Publisher != null) tItem.Publisher = mp.Publisher;
            tItem.MAGManualFalseMatch = false;
            tItem.MAGManualTrueMatch = false;
            tItem.MAGMatchScore = 1.0;

            return tItem;
        }

        private bool PaperPassesFilters(MagPaper mp)
        {
            if (mp.Journal == null || mp.DOI == null || mp.URLs == null)
            {
                return true;
            }
            if (!doFilter(mp.Journal.ToLower(), _FilterJournal))
            {
                return false;
            }
            if (!doFilter(mp.DOI.ToLower(), _FilterDOI))
            {
                return false;
            }
            if (!doFilter(mp.URLs.ToLower(), _FilterURL))
            {
                return false;
            }
            return true;
        }

        private bool doFilter(string field, string filter)
        {
            if (filter != "" && filter.Length > 3)
            {
                string[] filters = filter.ToLower().Split(',');
                foreach (string s in filters)
                {
                    if (field.Contains(s))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void OldVersion() // main difference is this looks up existing IDs in the database, rather than on the webserver. Also looks up items on MAKES one by one so is slower
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                string AllIDsToSearch = "";
                if (_SourceOfIds == "RelatedPapersSearch") // if not, then the list of ids has been sent from the browser or from the MagSearchText
                {
                    using (SqlCommand command = new SqlCommand("st_MagItemMagRelatedPaperInsert", connection))
                    {
                        command.CommandTimeout = 2000; // 2000 secs = about 2 hours?
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", _MagRelatedRunId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                _PaperIds += _PaperIds == "" ? reader["PaperId"].ToString() : "," + reader["PaperId"].ToString();
                                AllIDsToSearch = _PaperIds;
                            }
                        }
                    }
                }
                else
                {//we need to check if the selected paper ID already belong into the current review.
                    string sourceIds = _PaperIds;
                    Regex regex = new Regex(",");
                    while (sourceIds != "")
                    {
                        int maxChars = 4000;
                        string IdsToCheck = "";
                        if (sourceIds.Length > maxChars)
                        {
                            MatchCollection allSplits = regex.Matches(sourceIds);
                            int splitIndex = 0;
                            for (int cnt = 0; cnt < allSplits.Count; cnt++)
                            {
                                Match m = allSplits[cnt];
                                if (m.Index > maxChars)
                                {//OK, we'll take the previous match...
                                    splitIndex = allSplits[cnt - 1].Index;
                                    break;
                                }
                            }
                            IdsToCheck = sourceIds.Substring(0, splitIndex);
                            sourceIds = sourceIds.Replace(IdsToCheck, "").Substring(1);//otherwise we start with a comma...
                        }
                        else
                        {
                            IdsToCheck = sourceIds;
                            sourceIds = "";
                        }
                        using (SqlCommand command = new SqlCommand("st_MagItemPaperInsertAvoidDuplicates", connection))
                        {
                            //command.CommandTimeout = 500; 
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@MAG_IDs", IdsToCheck));
                            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                while (reader.Read())
                                {
                                    AllIDsToSearch += AllIDsToSearch == "" ? reader["PaperId"].ToString() : "," + reader["PaperId"].ToString();
                                }
                            }
                        }
                    }

                }
                IncomingItemsList incomingList = IncomingItemsList.NewIncomingItemsList();

                if (_SourceOfIds == "RelatedPapersSearch")
                {
                    incomingList.SourceName = "Automated search: " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToLongTimeString();
                }
                else
                {
                    incomingList.SourceName = "Selected items from MAG on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToLongTimeString();
                    incomingList.SearchStr = _PaperIds;
                }
                incomingList.SourceDB = "Microsoft Academic Graph";
                incomingList.HasMAGScores = true;
                incomingList.IsFirst = true; incomingList.IsLast = true;
                incomingList.IncomingItems = new MobileList<ItemIncomingData>();

                foreach (string PaperId in AllIDsToSearch.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    MagPaper mp = MagPaper.GetMagPaperFromMakes(Convert.ToInt64(PaperId), null);
                    if (mp.PaperId > 0)
                    {
                        incomingList.IncomingItems.Add(GetIncomingItemFromMagPaper(mp));
                    }
                }

                incomingList.buildShortTitles();
                _NImported = incomingList.IncomingItems.Count;
                incomingList = incomingList.Save();
                if (_MagRelatedRunId > 0)
                {
                    using (SqlCommand command = new SqlCommand("st_MagRelatedRun_Update", connection))
                    {
                        command.CommandTimeout = 500; // should make this a nice long time
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", _MagRelatedRunId));//Imported
                        command.Parameters.Add(new SqlParameter("@STATUS", "Imported"));
                        command.ExecuteNonQuery();

                    }
                }
                connection.Close();

            }
        }

#endif


    }
}
