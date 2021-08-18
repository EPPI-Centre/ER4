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
using BusinessLibrary.BusinessClasses.ImportItems;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
//using BusinessLibrary.eFetchPubmed;
using System.Xml.Linq;
using System.Net;
using System.IO;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class PubMedSearch : BusinessBase<PubMedSearch>
    {
        public PubMedSearch(){}
        
        public static void GetPubMedSearch(string QueStr, EventHandler<DataPortalResult<PubMedSearch>> handler)
        {
            DataPortal<PubMedSearch> dp = new DataPortal<PubMedSearch>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<PubMedSearch, string>(QueStr));
        }
        public static readonly PropertyInfo<string> QueryStrProperty = RegisterProperty<string>(new PropertyInfo<string>("QueryStr", "QueryStr"));
        public string QueryStr
        {
            get
            {
                return GetProperty(QueryStrProperty);
            }
            set
            {
                SetProperty(QueryStrProperty, value);
            }
        }
        public static readonly PropertyInfo<string> WebEnvProperty = RegisterProperty<string>(new PropertyInfo<string>("WebEnv", "WebEnv"));
        public string WebEnv
        {
            get
            {
                return GetProperty(WebEnvProperty);
            }
            set
            {
                SetProperty(WebEnvProperty, value);
            }
        }
        
        public static readonly PropertyInfo<int> QueMaxProperty = RegisterProperty<int>(new PropertyInfo<int>("QueMax", "QueMax"));
        public int QueMax
        {
            get
            {
                return GetProperty(QueMaxProperty);
            }
            set
            {
                SetProperty(QueMaxProperty, value);
            }
        }
        public static readonly PropertyInfo<int> showStartProperty = RegisterProperty<int>(new PropertyInfo<int>("showStart", "showStart"));
        public int showStart
        {
            get
            {
                return GetProperty(showStartProperty);
            }
            set
            {
                SetProperty(showStartProperty, value);
            }
        }
        public static readonly PropertyInfo<int> showEndProperty = RegisterProperty<int>(new PropertyInfo<int>("showEnd", "showEnd"));
        public int showEnd
        {
            get
            {
                return GetProperty(showEndProperty);
            }
            set
            {
                SetProperty(showEndProperty, value);
            }
        }
        public static readonly PropertyInfo<int> saveStartProperty = RegisterProperty<int>(new PropertyInfo<int>("saveStart", "saveStart"));
        public int saveStart
        {
            get
            {
                return GetProperty(saveStartProperty);
            }
            set
            {
                SetProperty(saveStartProperty, value);
            }
        }
        public static readonly PropertyInfo<int> saveEndProperty = RegisterProperty<int>(new PropertyInfo<int>("saveEnd", "saveEnd"));
        public int saveEnd
        {
            get
            {
                return GetProperty(saveEndProperty);
            }
            set
            {
                SetProperty(saveEndProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SummaryProperty = RegisterProperty<string>(new PropertyInfo<string>("Summary", "Summary"));
        public string Summary
        {
            get
            {
                return GetProperty(SummaryProperty);
            }
            set
            {
                SetProperty(SummaryProperty, value);
            }
        }
        public static readonly PropertyInfo<MobileList<string>> SavedIndexesProperty = RegisterProperty<MobileList<string>>(new PropertyInfo<MobileList<string>>("SavedIndexes", "SavedIndexes"));
        public MobileList<string> SavedIndexes
        {
            get
            {
                return GetProperty(SavedIndexesProperty);
            }
            set
            {
                SetProperty(SavedIndexesProperty, value);
            }
        }
        public static readonly PropertyInfo<IncomingItemsList> ItemsListProperty = RegisterProperty<IncomingItemsList>(new PropertyInfo<IncomingItemsList>("ItemsList", "ItemsList"));
        public IncomingItemsList ItemsList
        {
            get { return GetProperty(ItemsListProperty); }
            set { SetProperty(ItemsListProperty, value); }
        }
        public static readonly PropertyInfo<int> QueryKeyProperty = RegisterProperty<int>(new PropertyInfo<int>("QueryKey", "QueryKey"));
        public int QueryKey
        {
            get
            {
                return GetProperty(QueryKeyProperty);
            }
            set
            {
                SetProperty(QueryKeyProperty, value);
            }
        }

#if !SILVERLIGHT
        protected void DataPortal_Fetch(SingleCriteria<PubMedSearch, string> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (!ri.IsAuthenticated) return;
            //first go: do the search, get QueryKey, NewWebEnv, QueMax
            using (WebClient webc = new WebClient())
            {
                //tool=EppiReviewer4&email=eppisupport@ioe.ac.uk&db=pubmed&term=pain sleep disorders&usehistory=y
                nvcoll.Clear();
                nvcoll.Add("tool", "EppiReviewer4");
                nvcoll.Add("email", "eppisupport@ioe.ac.uk");
                nvcoll.Add("db", "pubmed");
                nvcoll.Add("usehistory", "y");
                nvcoll.Add("term", criteria.Value);
                string response = "";

                try
                {
                    byte[] responseArray = webc.UploadValues(SearchAddress, "POST", nvcoll);
                    response = Encoding.ASCII.GetString(responseArray);
                }
                catch (WebException we)
                {//if request is unsuccessful, we get an error inside the WebException
                    WebResponse wr = we.Response;
                    if (wr == null)
                    {
                        throw new Exception("WebResponse is null: " + we.Message + Environment.NewLine
                            + (we.InnerException != null && we.InnerException.Message != null ? we.InnerException.Message : ""));

                    }
                    using (var reader = new StreamReader(wr.GetResponseStream()))
                    {
                        if (reader == null)
                        {
                            throw new Exception("WebResponse reader is null: " + we.Message + Environment.NewLine
                            + (we.InnerException != null && we.InnerException.Message != null ? we.InnerException.Message : ""));
                        }
                        response = reader.ReadToEnd();

                        webc.Dispose();
                    }
                    return;
                }
                if (response != "")
                {
                    XElement xResponse = XElement.Parse(response);
                    XElement webenvElement = xResponse.Element("WebEnv");
                    WebEnv = webenvElement.Value;
                    QueryStr = criteria.Value;
                    XElement QueMaxElement = xResponse.Element("Count");
                    QueMax = Convert.ToInt32(QueMaxElement.Value);
                    XElement QueKeyElement = xResponse.Element("QueryKey");
                    QueryKey = Convert.ToInt32(QueKeyElement.Value);
                }
            }
            if (QueMax == 0)
            {
                Summary = "Search in PubMed for \""
                        + (criteria.Value.Length > 200 ? "[...long query...]" : criteria.Value)
                        + "\" returned no Items.\r\nPlease revise your Search String";
            }
            else
            {//fetch some results to show to the user
                showStart = 1;
                showEnd = showStart + 39;
                if (ItemsList == null)
                {
                    ItemsList = IncomingItemsList.NewIncomingItemsList();
                }
                else
                {
                    ItemsList.IncomingItems.Clear();
                }
                ItemsList.SearchStr = criteria.Value;
                ItemsList.SourceDB = "PubMed";
                ItemsList.DateOfSearch = DateTime.Now;
                ItemsList.SourceName = "PubMed Search on " + DateTime.Now.ToShortDateString();
                FetchResults();
                NormalSummary();
                //Summary = "Search in PubMed for \""
                //        + (criteria.Value.Length > 200 ? "[...long query...]" : criteria.Value)
                //        + "\" returned approximately " + QueMax + " Items.\r\n";
                //Summary += "Displaying first " + (showEnd + 1) + " Items.\r\n";
            }

        }
        private System.Collections.Specialized.NameValueCollection nvcoll = new System.Collections.Specialized.NameValueCollection();
        private string SearchAddress = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi";
        private string FetchAddress = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi";

        private void FetchResults()
        {
            string response = "";
            bool toSave = false;
            using (WebClient webc = new WebClient())
            {
                nvcoll.Clear();
                nvcoll.Add("tool", "EppiReviewer4");
                nvcoll.Add("email", "eppisupport@ioe.ac.uk");
                nvcoll.Add("db", "pubmed");
                nvcoll.Add("usehistory", "y");
                nvcoll.Add("WebEnv", WebEnv);
                nvcoll.Add("retmode", "text");
                nvcoll.Add("rettype", "medline");
                nvcoll.Add("query_key", QueryKey.ToString());
                if (showEnd != 0 && showStart <= showEnd && saveEnd == 0 && saveStart == 0)
                {
                    nvcoll.Add("retstart", (showStart - 1).ToString() );
                    if (showEnd > QueMax) showEnd = QueMax;
                    nvcoll.Add("retmax", (showEnd - showStart + 1).ToString());
                }
                else
                {
                    nvcoll.Add("retstart", (saveStart - 1).ToString());
                    if (saveEnd > QueMax) saveEnd = QueMax;
                    nvcoll.Add("retmax", (saveEnd - saveStart + 1).ToString());
                    toSave = true;
                }
                try
                {
                    byte[] responseArray = webc.UploadValues(FetchAddress, "POST", nvcoll);
                    response = Encoding.ASCII.GetString(responseArray);
                }
                catch (WebException we)
                {//if request is unsuccessful, we get an error inside the WebException
                    WebResponse wr = we.Response;
                    if (wr == null)
                    {
                        throw new Exception("WebResponse is null: " + we.Message + Environment.NewLine
                            + (we.InnerException != null && we.InnerException.Message != null ? we.InnerException.Message : ""));

                    }
                    using (var reader = new StreamReader(wr.GetResponseStream()))
                    {
                        if (reader == null)
                        {
                            throw new Exception("WebResponse reader is null: " + we.Message + Environment.NewLine
                            + (we.InnerException != null && we.InnerException.Message != null ? we.InnerException.Message : ""));
                        }
                        response = reader.ReadToEnd();

                        webc.Dispose();
                    }
                    return;
                }
            }
            if (response != "")
            {
                ParseThis(response);
            }
            if (toSave && ItemsList != null && ItemsList.IncomingItems.Count > 0)
            {
                ItemsList.Saved += new EventHandler<SavedEventArgs>(ItemsList_Saved);
                IncomingItemsList throwaway = ItemsList.Save();
            }
            else
            {
                NormalSummary();
            }
        }
        private void NormalSummary()
        {
            Summary = "Search in PubMed for \"" + ItemsList.SearchStr + "\" returned " + QueMax + " Items.\r\n";
            Summary += "Displaying Items from N°" + (showStart) + " to N°" + (showEnd) + ".\r\n";
        }
        private void ParseThis(string TxtFileContent)
        {
            BusinessLibrary.BusinessClasses.ReadOnlyImportFilterRule inRules =  ReadOnlyImportFilterRule.NewReadOnlyImportFilterRule();

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("select * from TB_IMPORT_FILTER where IMPORT_FILTER_NAME = 'PubMed'", connection))
                {
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            inRules = (ReadOnlyImportFilterRule.GetReadOnlyImportFilterRule(reader));
                        }
                    }
                }
                connection.Close();
            }
            FilterRules rules = new FilterRules();
            foreach (BusinessLibrary.BusinessClasses.TypeRules tprs in inRules.typeRules)
            {
                rules.typeRules.Add(new BusinessLibrary.BusinessClasses.ImportItems.TypeRules(tprs.Type_ID, tprs.RuleName, tprs.RuleRegexSt));
            }
            foreach (KeyValuePair<int, string> kvp in inRules.typesMap)
            {
                rules.AddTypeDef(kvp.Key, kvp.Value);
            }
            rules.Abstract_Set(inRules.Abstract);
            rules.author_Set(inRules.Author);
            rules.Availability_Set(inRules.Availability);
            rules.City_Set(inRules.City);
            rules.date_Set(inRules.Date);
            rules.DefaultTypeCode = inRules.DefaultTypeCode;
            rules.Edition_Set(inRules.Edition);
            rules.EndPage_Set(inRules.EndPage);
            rules.Institution_Set(inRules.Institution);
            rules.Issue_Set(inRules.Issue);
            rules.month_Set(inRules.Month);
            rules.Notes_Set(inRules.Notes);
            rules.OldItemID_Set(inRules.OldItemId);
            rules.Pages_Set(inRules.Pages);
            rules.pAuthor_Set(inRules.ParentAuthor);
            rules.pTitle_Set(inRules.pTitle);
            rules.Publisher_Set(inRules.Publisher);
            rules.shortTitle_Set(inRules.shortTitle);
            rules.StandardN_Set(inRules.StandardN);
            rules.startOfNewField_Set(inRules.StartOfNewField);
            rules.startOfNewRec_Set(inRules.StartOfNewRec);
            rules.StartPage_Set(inRules.StartPage);
            rules.title_Set(inRules.Title);
            rules.typeField_Set(inRules.typeField);
            rules.Url_Set(inRules.Url);
            rules.Volume_Set(inRules.Volume);
            rules.DOI_Set(inRules.DOI);
            rules.Keywords_Set(inRules.Keywords);
            ItemsList.IncomingItems = ImportRefs.Imp(TxtFileContent, rules);
            ItemsList.buildShortTitles();
        }
        protected void OldDataPortal_Fetch(SingleCriteria<PubMedSearch, string> criteria)
        {
            //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            //if (!ri.IsAuthenticated) return;
            //if (this.QueMax == 0) DP_F_GetNew(criteria);
            ////else DP_F_ReturnOtherItems(criteria);
            
        }
        protected void DP_F_GetNew(SingleCriteria<PubMedSearch, string> criteria)
        {
            //ItemsList = ImportItems.IncomingItemsList.NewIncomingItemsList();
            //ItemsList.IncomingItems = new MobileList<ItemIncomingData>();
            //ItemsList.SearchStr = criteria.Value;
            //ItemsList.SourceDB = "PubMed";
            //ItemsList.DateOfSearch = DateTime.Now;

            //ItemsList.SourceName = "PubMed Search on " + DateTime.Now.ToShortDateString();
            //eUtils.eSearchResult res0 = new BusinessLibrary.eUtils.eSearchResult();
            //// STEP #1: search in PubMed 
            //try
            //{
            //    eUtils.eUtilsService serv = new eUtils.eUtilsService();
            //    // call NCBI ESearch utility
            //    // NOTE: search term should be URL encoded
            //    eUtils.eSearchRequest req = new eUtils.eSearchRequest();
            //    req.db = "pubmed";

            //    req.term = System.Web.HttpUtility.UrlEncode(criteria.Value);
            //    req.usehistory = "y";
            //    req.sort = "PublicationDate";
            //    req.email = "EPPISupport@ioe.ac.uk";
            //    req.tool = "EppiReviewer4";
            //    //req.RetMax = "20000";
            //    //req.rettype = "count";
                
            //    res0 = serv.run_eSearch(req);
            //    // store WebEnv & QueryKey for use in eFetch
            //    if (res0.ERROR != null)
            //        throw new Exception(res0.ERROR);
            //    SavedIndexes = new MobileList<string>();
            //    //SavedIndexes.AddRange(res0.IdList);

            //    WebEnv = res0.WebEnv;
            //    QueryStr = res0.QueryKey;
            //    QueMax = Convert.ToInt32( res0.Count);
            //    //Id = res0.IdList[0]; 
                
            //}
            //catch (Exception eee)
            //{
            //    Summary += eee.Message;
            //    return;
            //}
            //// STEP #2: fetch records from pubmed starting from record
            //showStart = 1;
            //showEnd = QueMax > 200 ? 200 : QueMax;
            
            ////Summary += "QueryKey: " + QueryStr + "\r\n\r\n";
            //if (QueMax == 0)
            //{
            //    Summary = "Search in PubMed for \""
            //            + (criteria.Value.Length > 200 ? "[...long query...]" : criteria.Value)
            //            + "\" returned no Items.\r\nPlease revise your Search String";
            //}
            //else
            //{
            //    DP_F_ReturnOtherItems();
            //    if (Summary == null || Summary == "")
            //    {
            //        Summary = "Search in PubMed for \""
            //            + (criteria.Value.Length > 200 ? "[...long query...]" : criteria.Value)
            //            + "\" returned approximately " + QueMax + " Items.\r\n";
            //        Summary += "Dislpaying first " + showEnd + " Items.\r\n";
            //    }
            //}
            //this.MarkNew();
            
        }
        protected void DP_F_ReturnOtherItems()
        {
            //if (QueMax == 0) return;
            //try
            //{
            //    eFetchPubmed.eFetchPubmedService serv = new eFetchPubmed.eFetchPubmedService();
            //    // call NCBI EFetch utility
            //    ItemsList.IncomingItems.Clear();
            //    //retStart = (currEnd + 1);

            //    //ItemsList.SourceID = (e.NewObject as IncomingItemsList).SourceID;
            //    //if (retStart >= QueMax)
            //    //{
            //    //    Summary = "No More Items to Fetch";
            //    //    return;
            //    //}
            //    int aCount;
            //    string[] dateArr;
            //    string tDate, sPubl;
            //    eFetchPubmed.PaginationType pages;
            //    eFetchPubmed.ArticleType tArt;
            //    eFetchPubmed.PubmedDataType pmData;
            //    eFetchPubmed.eFetchRequest req = new eFetchPubmed.eFetchRequest();
            //    req.WebEnv = WebEnv;
            //    req.query_key = QueryStr;
            //    //req.id = SavedIndexes[0];

            //    req.retstart = (showStart - 1).ToString();
            //    req.retmax = (showEnd - showStart + 1).ToString();
            //    req.email = "EPPISupport@ioe.ac.uk";
            //    req.tool = "EppiReviewer4";
            //    req.rettype = "medline";
            //    eFetchPubmed.eFetchResult res = serv.run_eFetch(req);
            //    req.tool = "";
            //    //// results output
            //    //PubmedArticleType PubmedArticleT;
            //    //for (int i = 0; i < res.PubmedArticleSet.GetLength(0); i++)
            //    //{

            //    //    //if (!(res.PubmedArticleSet[i].MedlineCitation.Article.Items[0] is eFetchPubmed.JournalType))
            //    //    //{ 
            //    //    //    continue;
            //    //    //}
            //    //    ItemIncomingData Itm = ImportItems.ItemIncomingData.NewItem();
            //    //    object obj = res.PubmedArticleSet[i];

            //    //    if (obj.GetType() == typeof(PubmedArticleType))
            //    //    {
            //    //        PubmedArticleT = obj as PubmedArticleType;
            //    //    }
            //    //    else
            //    //        continue;
            //    //    pmData = PubmedArticleT.PubmedData;

            //    //    //pmData = res.PubmedArticleSet[i].
            //    //    //res.PubmedArticleSet[i].PubmedData;
            //    //    foreach (eFetchPubmed.ArticleIdType idType in pmData.ArticleIdList)
            //    //    {
            //    //        if (idType.IdType == eFetchPubmed.ArticleIdTypeIdType.pubmed)
            //    //        {
            //    //            Itm.Url = @"http://www.ncbi.nlm.nih.gov/pubmed/" + idType.Value;
            //    //        }
            //    //        if (idType.IdType == eFetchPubmed.ArticleIdTypeIdType.doi)
            //    //        {
            //    //            Itm.DOI = idType.Value;
            //    //        }
            //    //    }
            //    //    tArt = PubmedArticleT.MedlineCitation.Article;
            //    //    eFetchPubmed.JournalType artIt = tArt.Journal as eFetchPubmed.JournalType;
            //    //    //hunt for the date
            //    //    if (tArt.ArticleDate == null)
            //    //    {
            //    //        if (artIt.JournalIssue.PubDate.Items != null && artIt.JournalIssue.PubDate.Items.Length == 1)
            //    //        {
            //    //            dateArr = ImportItems.ImportRefs.getDate(artIt.JournalIssue.PubDate.Items[0]);
            //    //            Itm.Year = dateArr[0];
            //    //            Itm.Month = dateArr[1];
            //    //        }
            //    //        else if (artIt.JournalIssue.PubDate.Items != null && artIt.JournalIssue.PubDate.Items.Length > 1)
            //    //        {
            //    //            tDate = artIt.JournalIssue.PubDate.Items[0] + " " + artIt.JournalIssue.PubDate.Items[1];
            //    //            dateArr = ImportItems.ImportRefs.getDate(tDate);
            //    //            Itm.Year = dateArr[0];
            //    //            Itm.Month = dateArr[1];
            //    //        }
            //    //    }
            //    //    else
            //    //    {
            //    //        tDate = "";
            //    //        if (tArt.ArticleDate[0].Year != null)
            //    //            tDate = tArt.ArticleDate[0].Year;
            //    //        if (tArt.ArticleDate[0].Month != null)
            //    //            tDate += " " + tArt.ArticleDate[0].Month;
            //    //        dateArr = ImportItems.ImportRefs.getDate(tDate);
            //    //        Itm.Year = dateArr[0];
            //    //        Itm.Month = dateArr[1];
            //    //    }
            //    //    if (Itm.Title != null) Itm.Title = tArt.ArticleTitle.Value;
            //    //    if (Itm.Parent_title != null) Itm.Parent_title = artIt.Title;
            //    //    if (artIt.JournalIssue != null)
            //    //    {
            //    //        if (artIt.JournalIssue.Issue != null) Itm.Issue = artIt.JournalIssue.Issue;
            //    //        if (artIt.JournalIssue.Volume != null) Itm.Volume = artIt.JournalIssue.Volume;
            //    //    }
            //    //    if (artIt.ISSN != null) Itm.Standard_number = "ISSN:" + artIt.ISSN.Value;
            //    //    if (tArt.Abstract != null)
            //    //    {
            //    //        if (tArt.Abstract.AbstractText != null)
            //    //        {
            //    //            Itm.Abstract = "";
            //    //            foreach (eFetchPubmed.AbstractTextType partA in tArt.Abstract.AbstractText)
            //    //            {
            //    //                if (partA.Label != null && partA.Label != "")
            //    //                {
            //    //                    Itm.Abstract += partA.Label.ToUpper() + ":" + Environment.NewLine;
            //    //                }
            //    //                Itm.Abstract += partA.Value + Environment.NewLine;
            //    //            }
            //    //            Itm.Abstract = Itm.Abstract.Trim();
            //    //        }
            //    //        if (tArt.Abstract.CopyrightInformation != null)
            //    //        {
            //    //            sPubl = System.Text.RegularExpressions.Regex.Replace(tArt.Abstract.CopyrightInformation,
            //    //                    @"((\(c\)) |Copyright |Copyright (\(c\)) |^|Copyright © )\d\d\d\d", "");
            //    //            sPubl = System.Text.RegularExpressions.Regex.Replace(sPubl, @", Inc. ", " ");
            //    //            sPubl = System.Text.RegularExpressions.Regex.Replace(sPubl, @",Inc.|, Inc.", "");

            //    //            Itm.Publisher = sPubl.Trim();
            //    //        }
            //    //    }
            //    //    if (tArt.Affiliation != null)
            //    //        Itm.Institution = tArt.Affiliation;
            //    //    if (PubmedArticleT.MedlineCitation.MedlineJournalInfo != null
            //    //        && PubmedArticleT.MedlineCitation.MedlineJournalInfo.Country != null)
            //    //        Itm.Country = PubmedArticleT.MedlineCitation.MedlineJournalInfo.Country;

            //    //    //second tricky hunt: pages!!

            //    //    if (tArt.Items != null && tArt.Items[0] is eFetchPubmed.PaginationType)
            //    //    {
            //    //        pages = tArt.Items[0] as eFetchPubmed.PaginationType;
            //    //        Itm.Pages = "";
            //    //        for (int pp = 0; pp < pages.Items.Length; pp++)
            //    //        {
            //    //            if (pages.ItemsElementName[pp] == BusinessLibrary.eFetchPubmed.ItemsChoiceType1.MedlinePgn)
            //    //            {
            //    //                Itm.Pages = pages.Items[pp];
            //    //                break;
            //    //            }
            //    //            else if (pages.ItemsElementName[pp] == BusinessLibrary.eFetchPubmed.ItemsChoiceType1.StartPage)
            //    //                Itm.Pages = pages.Items[pp] + "-" + Itm.Pages;
            //    //            else if (pages.ItemsElementName[pp] == BusinessLibrary.eFetchPubmed.ItemsChoiceType1.EndPage)
            //    //                Itm.Pages += pages.Items[pp];
            //    //        }
            //    //    }
            //    //    Itm.pAuthorsLi = new AuthorsHandling.AutorsList();
            //    //    Itm.AuthorsLi = new AuthorsHandling.AutorsList();
            //    //    aCount = 0;
            //    //    if (tArt.AuthorList != null && tArt.AuthorList.Author != null)
            //    //    {
            //    //        while (aCount < PubmedArticleT.MedlineCitation.Article.AuthorList.Author.Length & aCount < 256)
            //    //        {
            //    //            eFetchPubmed.AuthorType rAu = PubmedArticleT.MedlineCitation.Article.AuthorList.Author[aCount];
            //    //            AuthorsHandling.AutH taut;
            //    //            switch (rAu.Items.GetLength(0))
            //    //            {
            //    //                case 1:
            //    //                    taut = AuthorsHandling.AutH.NewAutH(
            //    //                    rAu.Items[0].ToString(), aCount, 0);
            //    //                    break;
            //    //                case 2:
            //    //                    taut = AuthorsHandling.AutH.NewAutH(
            //    //                    rAu.Items[0].ToString(), rAu.Items[1].ToString(), aCount, 0);
            //    //                    break;
            //    //                case 4:
            //    //                    taut = AuthorsHandling.AutH.NewAutH(
            //    //                    rAu.Items[0].ToString(), rAu.Items[1].ToString(), rAu.Items[2] + " " + rAu.Items[3], aCount, 0);
            //    //                    break;
            //    //                default:
            //    //                    taut = AuthorsHandling.AutH.NewAutH(
            //    //                        rAu.Items[0].ToString(), rAu.Items[1].ToString(), rAu.Items[2].ToString().Substring(1), aCount, 0);
            //    //                    break;
            //    //            }
            //    //            Itm.AuthorsLi.Add(taut);
            //    //            aCount++;

            //    //        }
            //    //    }
            //    //    //finally hunt for keywords
            //    //    if (PubmedArticleT.MedlineCitation.KeywordList != null)
            //    //    {
            //    //        object ob = PubmedArticleT.MedlineCitation.KeywordList;
            //    //        ob = PubmedArticleT.MedlineCitation.KeywordList.ToString();
            //    //    }
            //    //    if (PubmedArticleT != null && PubmedArticleT.MedlineCitation != null && PubmedArticleT.MedlineCitation.MeshHeadingList != null)
            //    //    {
            //    //        foreach (MeshHeadingType mht in PubmedArticleT.MedlineCitation.MeshHeadingList)
            //    //        {
            //    //            sPubl = "";
            //    //            if (mht.DescriptorName != null && mht.DescriptorName.Value != null && mht.DescriptorName.Value.Length > 0)
            //    //            {
            //    //                sPubl = mht.DescriptorName.Value;
            //    //            }
            //    //            if (mht.QualifierName != null)
            //    //            {
            //    //                foreach (QualifierNameType qual in mht.QualifierName)
            //    //                {
            //    //                    if (qual.Value != null && qual.Value.Length > 0)
            //    //                    {
            //    //                        if (sPubl.Length > 0) sPubl += "/";
            //    //                        if (qual.MajorTopicYN == QualifierNameTypeMajorTopicYN.Y) sPubl += "*";
            //    //                        sPubl += qual.Value;
            //    //                    }
            //    //                }
            //    //            }
            //    //            Itm.Keywords += sPubl + Environment.NewLine;
            //    //        }
            //    //        Itm.Keywords = Itm.Keywords.Trim();
            //    //    }

            //    //    //Itm.Url = @"http://www.ncbi.nlm.nih.gov/pubmed/" + SavedIndexes[showStart + i - 1];
            //    //    //_Result  += "Title: " + res.PubmedArticleSet[i].MedlineCitation.Article.ArticleTitle + "\r\n";
            //    //    //_Result  += "Abstract: " + res.PubmedArticleSet[i].MedlineCitation.Article.Abstract.AbstractText + "\r\n";
            //    //    //_Result  += "--------------------------\r\n\r\n";
            //    //    ItemsList.IncomingItems.Add(Itm);
            //    //    //ItemsList.FilterID = 4;
            //    //}
            //    //currStart = retStart;
            //    //currEnd = retEnd;
            //    Summary = "Search in PubMed for \"" + ItemsList.SearchStr + "\" returned " + QueMax + " Items.\r\n";
            //    Summary += "Dislpaying Items from N° " + showStart + " to N° " + showEnd + ".\r\n";
            //}
            //catch (Exception eee)
            //{
            //    Summary += "ERROR: PubMed replied \"" + eee.Message + "\"";
            //}
        }
        protected override void DataPortal_Insert()
        {
            FetchResults();
            ////SavedIndexes =  new MobileList<int>();
            ////for (int i = 0; i < currEnd - currStart; i++)
            ////{
            ////    SavedIndexes.Add(i);
            ////}
            //if (saveEnd == 0 && saveStart == 0)
            //{
            //    //send back more results
            //    DP_F_ReturnOtherItems();
            //}
            //else if (showEnd == 0 && showStart == 0)
            //{
            //    //get the requested results & save
            //    showEnd = saveEnd;
            //    showStart = saveStart;
            //    DP_F_ReturnOtherItems();
            //    ItemsList.buildShortTitles();
            //    ItemsList.Saved += new EventHandler<SavedEventArgs>(ItemsList_Saved);
            //    ItemsList.Save();
            //}
        }

        void ItemsList_Saved(object sender, SavedEventArgs e)
        {
            ItemsList = IncomingItemsList.NewIncomingItemsList();
            if ((e.NewObject as IncomingItemsList).SearchStr == "")
            {
                Summary = "Nothing was saved: items requested could not be fetched from PubMed";
            }
            else
            {
                Summary = "Source Saved";
            }
            /* try
            {
                eFetchPubmed.eFetchPubmedService serv = new eFetchPubmed.eFetchPubmedService();
                // call NCBI EFetch utility
                int aCount;
                eFetchPubmed.eFetchRequest req = new eFetchPubmed.eFetchRequest();
                req.WebEnv = WebEnv;
                req.query_key = QueryStr;
                //retStart = (currEnd + 1);
                ItemsList.IncomingItems.Clear();
                ItemsList.SourceID = (e.NewObject as IncomingItemsList).SourceID;
                if (retStart >= QueMax)
                {
                    Summary = "No More Items to Fetch";
                    return;
                }
                req.retstart = retStart.ToString();
                retEnd = (QueMax > currEnd + 201 ? currEnd + 201 : QueMax);
                req.retmax = (retEnd - retStart).ToString();
                req.email = "EPPISupport@ioe.ac.uk";
                req.tool = "EppiReviewer4(alpha)";
                eFetchPubmed.eFetchResult res = serv.run_eFetch(req);
                // results output
                for (int i = 0; i < retEnd - retStart; i++)
                {
                    ItemIncomingData Itm = ImportItems.ItemIncomingData.NewItem();
                    Itm.Title = res.PubmedArticleSet[i].MedlineCitation.Article.ArticleTitle;
                    Itm.pAuthorsLi = new MobileList<AuthorsHandling.AutH>();
                    Itm.AuthorsLi = new MobileList<AuthorsHandling.AutH>();
                    aCount = 0;
                    while (aCount < res.PubmedArticleSet[i].MedlineCitation.Article.AuthorList.Author.Length)
                    {
                        eFetchPubmed.AuthorType rAu = res.PubmedArticleSet[i].MedlineCitation.Article.AuthorList.Author[aCount];
                        AuthorsHandling.AutH taut;
                        switch (rAu.Items.GetLength(0))
                        {
                            case 1:
                                taut = AuthorsHandling.AutH.NewAutH(
                                rAu.Items[0], aCount, 0);
                                break;
                            case 2:
                                taut = AuthorsHandling.AutH.NewAutH(
                                rAu.Items[0], rAu.Items[1], aCount, 0);
                                break;
                            case 4:
                                taut = AuthorsHandling.AutH.NewAutH(
                                rAu.Items[0], rAu.Items[1], rAu.Items[2] + " " + rAu.Items[3], aCount, 0);
                                break;
                            default:
                                taut = AuthorsHandling.AutH.NewAutH(
                                    rAu.Items[0], rAu.Items[1], rAu.Items[2].Substring(1), aCount, 0);
                                break;
                        }
                        Itm.AuthorsLi.Add(taut);
                        aCount++;

                    }

                    //_Result  += "Title: " + res.PubmedArticleSet[i].MedlineCitation.Article.ArticleTitle + "\r\n";
                    //_Result  += "Abstract: " + res.PubmedArticleSet[i].MedlineCitation.Article.Abstract.AbstractText + "\r\n";
                    //_Result  += "--------------------------\r\n\r\n";
                    ItemsList.IncomingItems.Add(Itm);
                    //ItemsList.FilterID = 4;
                }
                currStart = retStart;
                currEnd = retEnd;
            }
            catch (Exception eee)
            {
                Summary += eee.ToString();
            }*/
        }
        protected override void DataPortal_Update()
        {
            //for (int i = currStart; i < currEnd - currStart; i++)
            //{
            //    SavedIndexes.Add(i);
            //}
            //ItemsList.Saved += new EventHandler<SavedEventArgs>(ItemsList_Saved);
            //ItemsList.Save();
            DataPortal_Insert();
        }
#endif
    }
    [Serializable]
    public class ContinueCriteria : Csla.CriteriaBase<ContinueCriteria>
    {
        /*public static readonly PropertyInfo<bool> OnlyIncludedProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("OnlyIncluded", "OnlyIncluded"));
        public bool OnlyIncluded
        {
            get { return ReadProperty(OnlyIncludedProperty); }
        }

        public static readonly PropertyInfo<bool> ShowDeletedProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("ShowDeleted", "ShowDeleted"));
        public bool ShowDeleted
        {
            get { return ReadProperty(ShowDeletedProperty); }
        }

        public static readonly PropertyInfo<int> SourceIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("SourceId", "SourceId"));
        public Int64 SourceId
        {
            get { return ReadProperty(SourceIdProperty); }
        }

        public static readonly PropertyInfo<string> AttributeSetIdListProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("AttributeSetIdList", "AttributeSetIdList"));
        public string AttributeSetIdList
        {
            get { return ReadProperty(AttributeSetIdListProperty); }
        }

        public ContinueCriteria(Type type, bool onlyIncluded, bool showDeleted, int sourceId, string attributeSetIdList)
            : base(type)
        {
            LoadProperty(OnlyIncludedProperty, onlyIncluded);
            LoadProperty(ShowDeletedProperty, showDeleted);
            LoadProperty(SourceIdProperty, sourceId);
            LoadProperty(AttributeSetIdListProperty, attributeSetIdList);
        }

        public ContinueCriteria() { }
         */ 

    }
}
