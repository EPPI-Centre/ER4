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

        public MagItemPaperInsertCommand(string PaperIds, string SourceOfIds, int MagRelatedRunId)
        {
            _PaperIds = PaperIds;
            _SourceOfIds = SourceOfIds;
            _MagRelatedRunId = MagRelatedRunId;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_PaperIds", _PaperIds);
            info.AddValue("_NImported", _NImported);
            info.AddValue("_SourceOfIds", _SourceOfIds);
            info.AddValue("_MagRelatedRunId", _MagRelatedRunId);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _PaperIds = info.GetValue<string>("_PaperIds");
            _NImported = info.GetValue<int>("_NImported");
            _SourceOfIds = info.GetValue<string>("_SourceOfIds");
            _MagRelatedRunId = info.GetValue<int>("_MagRelatedRunId");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                string AllIDsToSearch = "";
                if (_SourceOfIds == "RelatedPapersSearch") // if not, then the list of ids has been sent from the browser
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

                //Guid GuidJob = Guid.NewGuid();
                TextInfo myTI = new CultureInfo("en-GB", false).TextInfo;

                //DataTable dtItems = new DataTable("Items");
                //dtItems.Columns.Add("id", typeof(Int64));
                //dtItems.Columns.Add("year", typeof(string));
                //dtItems.Columns.Add("journal", typeof(string));
                //dtItems.Columns.Add("volume", typeof(string));
                //dtItems.Columns.Add("issue", typeof(string));
                //dtItems.Columns.Add("pages", typeof(string));
                //dtItems.Columns.Add("title", typeof(string));
                //dtItems.Columns.Add("doi", typeof(string));
                //dtItems.Columns.Add("abstract", typeof(string));
                //dtItems.Columns.Add("SearchText", typeof(string));
                //dtItems.Columns.Add("GUID_JOB");

                //DataTable dtAuthors = new DataTable("Authors");
                //dtAuthors.Columns.Add("id", typeof(Int64));
                //dtAuthors.Columns.Add("GUID_JOB");
                //dtAuthors.Columns.Add("first", typeof(string));
                //dtAuthors.Columns.Add("second", typeof(string));
                //dtAuthors.Columns.Add("last", typeof(string));
                //dtAuthors.Columns.Add("rank", typeof(int));
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

                foreach (string PaperId in AllIDsToSearch.Split(new char[] { ','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    MagPaper mp = MagPaper.GetMagPaperFromMakes(Convert.ToInt64(PaperId), null);
                    if (mp.PaperId > 0)
                    {
                        ItemIncomingData tItem = ItemIncomingData.NewItem();
                        tItem.AuthorsLi = new AuthorsHandling.AutorsList();
                        tItem.pAuthorsLi= new AuthorsHandling.AutorsList();
                        string[] authors = mp.Authors.Split(',');
                        for (int x = 0; x < authors.Count(); x++)
                        {
                            //AutH author = NormaliseAuth.singleAuth(authors[x], 0, 0);
                            AutH author = NormaliseAuth.singleAuth(authors[x], x + 1, 0);
                            if (author != null)
                            {
                                tItem.AuthorsLi.Add(author);
                                //DataRow newAuthor = dtAuthors.NewRow();
                                //newAuthor["id"] = mp.PaperId;
                                //newAuthor["GUID_JOB"] = GuidJob.ToString();
                                //newAuthor["first"] = author.LastName; // looks odd, but order is reversed in a MAG list
                                //newAuthor["second"] = author.MiddleName == "" ? author.MiddleName : author.FirstName;
                                //newAuthor["last"] = author.MiddleName != "" ? author.MiddleName : author.FirstName;
                                //newAuthor["rank"] = x + 1;
                                //dtAuthors.Rows.Add(newAuthor);
                            }
                        }
                        tItem.OldItemId = mp.PaperId.ToString();

                        if (mp.Year >= 0) tItem.Year= mp.Year.ToString();
                        if (mp.Journal != null) tItem.Parent_title = mp.Journal != null ? myTI.ToTitleCase(mp.Journal) : "";
                        if (mp.Volume != null) tItem.Volume = mp.Volume;
                        if (mp.Issue != null) tItem.Issue = mp.Issue;
                        if ((mp.FirstPage != null && mp.FirstPage.Length > 0) 
                            ||
                            (mp.LastPage != null && mp.LastPage.Length > 0)) tItem.Pages= mp.FirstPage + "-" + mp.LastPage;
                        if (mp.OriginalTitle != null) tItem.Title = mp.OriginalTitle;
                        if (mp.DOI != null) tItem.DOI = mp.DOI;
                        if (mp.Abstract != null) tItem.Abstract = mp.Abstract;
                        tItem.SearchText = Item.ToShortSearchText(mp.PaperTitle);
                        if (mp.URLs != null && mp.URLs.Length > 0)
                        {
                            string[] urls = mp.URLs.Split(';');
                            if (urls.Length > 0) tItem.Url = urls[0];
                        }
                        if (mp.Publisher != null) tItem.Publisher = mp.Publisher;
                        tItem.MAGManualFalseMatch = false;
                        tItem.MAGManualTrueMatch = false;
                        tItem.MAGMatchScore = 1.0;
                        incomingList.IncomingItems.Add(tItem);
                        //DataRow dr = dtItems.NewRow();
                        //dr["id"] = mp.PaperId;
                        //dr["year"] = mp.Year;
                        //dr["journal"] = mp.Journal != null ? myTI.ToTitleCase(mp.Journal) : "";
                        //dr["volume"] = mp.Volume;
                        //dr["issue"] = mp.Issue;
                        //dr["pages"] = mp.FirstPage + "-" + mp.LastPage;
                        //dr["title"] = mp.OriginalTitle;
                        //dr["doi"] = mp.DOI;
                        //dr["abstract"] = mp.Abstract;
                        //dr["GUID_JOB"] = GuidJob.ToString();
                        //dr["SearchText"] = Item.ToShortSearchText(mp.PaperTitle);
                        //dtItems.Rows.Add(dr);
                    }
                }

                //using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                //{
                //    sbc.DestinationTableName = "TB_MAG_ITEM_PAPER_INSERT_TEMP";
                //    sbc.BatchSize = 1000;
                //    sbc.WriteToServer(dtItems);
                //}
                //using (SqlBulkCopy sbc = new SqlBulkCopy(connection))
                //{
                //    sbc.DestinationTableName = "TB_MAG_ITEM_PAPER_INSERT_AUTHORS_TEMP";
                //    sbc.BatchSize = 1000;
                //    sbc.WriteToServer(dtAuthors);
                //}
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
