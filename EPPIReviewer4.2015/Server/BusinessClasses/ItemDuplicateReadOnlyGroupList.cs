using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;

#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using AuthorsHandling;
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

        private void DataPortal_Fetch(SingleCriteria<ItemDuplicateReadOnlyGroupList, bool> criteria)
        {
            IsReadOnly = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupCheckOngoing", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@revID", System.Data.SqlDbType.Int);
                    command.Parameters["@revID"].Value = ri.ReviewId;
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
                if (criteria.Value == true)
                {
                    FindNewDuplicates(connection);
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
                    command.Parameters.Add(new SqlParameter("@RevID", ri.ReviewId));
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

        protected void FindNewDuplicates(SqlConnection connection)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            SetShortSearchText(ri.ReviewId);
            FindNewDuplicatesNewVersion(ri.ReviewId);
        }

        private void AddDuplicate(ItemComparison MasterItem, ItemComparison matchedItem, double matchingScore)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                if (MasterItem.IS_MASTER == 1)
                {
                    using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupAddAddionalItem", connection))
                    {
                        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@revID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@MasterID", MasterItem.ITEM_ID));
                        command.Parameters.Add(new SqlParameter("@NewDuplicateItemID", matchedItem.ITEM_ID));
                        command.Parameters.Add(new SqlParameter("@Score", matchingScore));
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (SqlCommand command = new SqlCommand("st_ItemDuplicateGroupCreateNew", connection))
                    {
                        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@revID", ri.ReviewId));
                        command.Parameters.Add(new SqlParameter("@Score", matchingScore));
                        command.Parameters.Add(new SqlParameter("@MasterID", MasterItem.ITEM_ID));
                        command.Parameters.Add(new SqlParameter("@MemberId", matchedItem.ITEM_ID));
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
        }

        private void SetShortSearchText(int ReviewId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicateGetTitles", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
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

        private void FindNewDuplicatesNewVersion(int ReviewId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemDuplicatesGetCandidatesOnSearchText", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        string currentSearchText = "";
                        List<ItemComparison> CurrentGroup = new List<ItemComparison>();
                        while (reader.Read())
                        {
                            if (currentSearchText != reader["SearchText"].ToString() && CurrentGroup.Count > 0)
                            {
                                MakeComparisons(CurrentGroup, 1);
                                CurrentGroup.Clear();
                                currentSearchText = reader["SearchText"].ToString();
                            }
                            currentSearchText = reader["SearchText"].ToString();
                            ItemComparison newItem = ReadItemComparison(reader, 0);
                            if (!AlreadyInGroup(CurrentGroup, newItem))
                            {
                                CurrentGroup.Add(newItem);
                            }
                            newItem = ReadItemComparison(reader, 1);
                            if (!AlreadyInGroup(CurrentGroup, newItem))
                            {
                                CurrentGroup.Add(newItem);
                            }
                        }
                        if (CurrentGroup.Count > 0)
                        {
                            MakeComparisons(CurrentGroup, 1); // for the last group to be read before the reader didn't read any more
                        }
                    }
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
                        double matchingScore = CompareItems(group[0], group[i]);
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
                            double matchingScore = CompareItems(existingMaster, ic);
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
                        }
                    }
                    foreach (ItemComparison ic in group)
                    {
                        if (ic.ITEM_ID != candidateMaster.ITEM_ID && ic.IS_MASTER == 0)
                        {
                            double matchingScore = CompareItems(candidateMaster, ic);
                            if (matchingScore > 0.7)
                            {
                                AddDuplicate(candidateMaster, ic, matchingScore);
                                candidateMaster.IS_MASTER = 1; // i.e. after the first time, we are adding to an existing group in AddDuplicate
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
            // we only do this once (no empirical basis, so can adjust upwards if necessary)
            if (unmatched.Count > 1 && iteration < 10)
            {
                MakeComparisons(unmatched, iteration + 1); // only one lot of recursion
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

        private Double CompareItems(ItemComparison ic1, ItemComparison ic2)
        {
            double ret = 0;
            double titleSimilarity = MagPaperItemMatch.HaBoLevenshtein(ic1.TITLE, ic2.TITLE);

            // if titles don't reach threshold return without further tests
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
                MagPaperItemMatch.HaBoLevenshtein(ic1.ABSTRACT, ic2.ABSTRACT) > 90 &&
                MagPaperItemMatch.HaBoLevenshtein(ic1.PARENT_TITLE, ic2.PARENT_TITLE) > 90)
            {
                return 0.98;
            }

            // Title and journal similarity > 80 plus exact matches on 4 other fields where present
            if (titleSimilarity > 80 &&

                // similarity on journals > 80 where both fields present
                ((ic1.PARENT_TITLE.Length > 5 && ic2.PARENT_TITLE.Length > 5 &&
                MagPaperItemMatch.HaBoLevenshtein(ic1.PARENT_TITLE, ic2.PARENT_TITLE) > 80) ||
                (ic1.PARENT_TITLE.Length <=5 || ic2.PARENT_TITLE.Length <= 5)) &&

                // matches on DOI where both fields present
                ((ic1.DOI.Length > 5 && ic2.DOI.Length > 5 &&
                CompareDOIs(ic1.DOI, ic2.DOI) == true) ||
                (ic1.DOI.Length <=5 || ic2.DOI.Length <=5)) &&

                // exact match on year where both fields present
                ((ic1.YEAR != "" && ic2.YEAR != "" && ic1.YEAR == ic2.YEAR) ||
                (ic1.YEAR == "" || ic2.YEAR == "")) &&

                // exact match on issue where both fields present
                ((ic1.ISSUE != "" && ic2.ISSUE != "" && ic1.ISSUE == ic2.ISSUE) ||
                (ic1.ISSUE == "" || ic2.ISSUE == "")) &&

                // exact match on start page where both fields present
                ((ic1.GetFirstPage() != "" && ic2.GetFirstPage() != "" && ic1.GetFirstPage() == ic2.GetFirstPage()) ||
                (ic1.GetFirstPage() == "" || ic2.GetFirstPage() == "")))
            {
                return 0.97;
            }

            // If none of the above, just calculate the basic similarity score
            int volumeMatch = ic1.VOLUME == ic2.VOLUME ? 1 : 0;
            int pageMatch = ic1.GetFirstPage() == ic2.GetFirstPage() ? 1 : 0;
            int yearMatch = ic1.YEAR == ic2.YEAR ? 1 : 0;
            double journalJaro = ic1.PARENT_TITLE != null ? MagPaperItemMatch.HaBoLevenshtein(ic1.PARENT_TITLE, ic2.PARENT_TITLE) : 0;
            //double allAuthorsJaro = MagPaperItemMatch.Jaro(reader["AUTHORS"].ToString().Replace(",", " "),
            //readerCandidates["AUTHORS"].ToString().Replace(",", " "));
            double allAuthorsCompare = CompareAuthors(ic1, ic2);
            ret = ((titleSimilarity / 100 * 1.75) +
                (volumeMatch * 0.5) +
                (pageMatch * 0.5) +
                (yearMatch * 0.5) +
                (journalJaro / 100 * 1) +
                (CompareAuthors(ic1, ic2) * 1.25)) / 5.5;

            // Final sanity checks to prevent auto-matches on questionable records

            // if pages and year are present, but don't match, this needs manual check
            if ((ic1.PAGES != "" && ic2.PAGES != "" && ic1.GetFirstPage() != ic2.GetFirstPage()) &&
                (ic1.YEAR != "" && ic2.YEAR != "" && ic1.YEAR != ic2.YEAR))
                ret = Math.Min(ret, 0.75);

            // check for erratum
            if ((ic1.TITLE.IndexOf("erratum") > -1 && ic2.TITLE.IndexOf("erratum") < 0) ||
                (ic2.TITLE.IndexOf("erratum") > -1 && ic1.TITLE.IndexOf("erratum") < 0))
                ret = Math.Min(ret, 0.75);
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
            int totalAuthors = ic1.GetAuthors().Count;
            double lastWithLast = 0;
            double firstWithLast = 0;
            foreach (AutH author in ic1.GetAuthors())
            {
                foreach (AutH auth2 in ic2.GetAuthors())
                {
                    if (author.LastName.Length > 2 && MagMakesHelpers.CleanText(author.LastName) ==
                        MagMakesHelpers.CleanText(auth2.LastName))
                    {
                        lastWithLast++;
                        break;
                    }
                }
            }
            foreach (AutH author in ic1.GetAuthors())
            {
                foreach (AutH auth2 in ic2.GetAuthors())
                {
                    if (author.LastName.Length > 2 && MagMakesHelpers.CleanText(author.LastName) ==
                        MagMakesHelpers.CleanText(auth2.FirstName))
                    {
                        firstWithLast++;
                        break;
                    }
                }
            }
            ret = Math.Max(lastWithLast / totalAuthors, firstWithLast / totalAuthors);
            return ret > 0.3 ? ret : 0;
        }

        private class ItemComparison
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
            public double MatchingScore { get; set; }

            public AutorsList GetAuthors()
            {
                string[] authors = AUTHORS.Split(';');
                AutorsList alist = new AutorsList();
                for (int x = 0; x < authors.Count(); x++)
                {
                    if (authors[x] != " " && authors[x] != "")
                        alist.Add(NormaliseAuth.singleAuth(authors[x], x + 1, 0));
                }
                return alist;
            }
            public string GetFirstPage()
            {
                if (PAGES == "")
                    return "";
                int i = PAGES.IndexOf("-");
                if (i == -1)
                    return PAGES;
                else
                    return PAGES.Substring(0, i);
            }
        }

        private ItemComparison ReadItemComparison(SafeDataReader reader, int index)
        {
            ItemComparison ic = new ItemComparison();
            ic.ITEM_ID = index == 0 ? reader.GetInt64("ITEM_ID") : reader.GetInt64("ITEM_ID2");
            ic.AUTHORS = index == 0 ? reader.GetString("AUTHORS") : reader.GetString("AUTHORS2");
            ic.TITLE = index == 0 ? reader.GetString("TITLE") : reader.GetString("TITLE2");
            if (ic.TITLE.IndexOf("Erratum") == -1 )
                ic.TITLE = MagMakesHelpers.RemoveTextInParentheses(ic.TITLE);
            ic.TITLE = MagMakesHelpers.CleanText(ic.TITLE);
            ic.PARENT_TITLE = index == 0 ? MagMakesHelpers.CleanText(reader.GetString("PARENT_TITLE").Replace("&", "and")) :
                MagMakesHelpers.CleanText(reader.GetString("PARENT_TITLE2").Replace("&", "and"));
            ic.YEAR = index == 0 ? reader.GetString("YEAR") : reader.GetString("YEAR2");
            ic.VOLUME = index == 0 ? reader.GetString("VOLUME") : reader.GetString("VOLUME2");
            ic.PAGES = index == 0 ? reader.GetString("PAGES") : reader.GetString("PAGES2");
            ic.ISSUE = index == 0 ? reader.GetString("ISSUE") : reader.GetString("ISSUE2");
            ic.DOI = index == 0 ? reader.GetString("DOI") : reader.GetString("DOI2");
            ic.ABSTRACT = index == 0 ? reader.GetString("ABSTRACT") : reader.GetString("ABSTRACT2");
            ic.HAS_CODES = index == 0 ? reader.GetInt32("HAS_CODES") : reader.GetInt32("HAS_CODES2");
            ic.IS_MASTER = index == 0 ? reader.GetInt32("IS_MASTER") : reader.GetInt32("IS_MASTER2");

            return ic;
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
            //: base(type)
        {
            LoadProperty(GroupIdProperty, GroupId);
            LoadProperty(ItemIdsProperty, "0");
        }

        public GroupListSelectionCriteria(Type type, string ItemIds)
            //: base(type)
        {

            LoadProperty(ItemIdsProperty, ItemIds);
            LoadProperty(GroupIdProperty, 0);

        }
#if !SILVERLIGHT 
        public GroupListSelectionCriteria(){}
#endif
    }
}
