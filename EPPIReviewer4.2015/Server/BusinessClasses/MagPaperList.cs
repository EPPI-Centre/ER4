using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Configuration;
//using Csla.Validation;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.IO;
using System.Net;
#endif

namespace BusinessLibrary.BusinessClasses
{

    public class PaperMakes
    {
        public string AAAuId { get; set; } // NB AUTHORS AREN'T USED AT THE MOMENT - WILL NEED FIXING SO THAT THEY ARE LISTS if / when we use MAKES for full bibliographic info
        public string AADAuN { get; set; }
        public string CC { get; set; }
        public DateTime D { get; set; }
        public string DN { get; set; }
        public string DOI { get; set; }
        public Int64 FFId { get; set; }
        public string FFN { get; set; }
        public string FP { get; set; }
        public string I { get; set; }
        public string IA { get; set; }
        public Int64 Id { get; set; }
        public Int64 JJId { get; set; }
        public string JJN { get; set; }
        public string LP { get; set; }
        public string PB { get; set; }
        public string Pt { get; set; }
        public List<Int64> RId { get; set; }
        public List<string> S { get; set; }
        public string Ti { get; set; }
        public string V { get; set; }
        public Int32 Y { get; set; }
        //public IList<string> Roles { get; set; }
    }

    

    [Serializable]
    public class MagPaperList : DynamicBindingListBase<MagPaper>, System.ComponentModel.IPagedCollectionView, INotifyPropertyChanged
    {
        public static void GetMagPaperList(MagPaperListSelectionCriteria selectionCriteria, EventHandler<DataPortalResult<MagPaperList>> handler)
        {
            DataPortal<MagPaperList> dp = new DataPortal<MagPaperList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(selectionCriteria);
        }


#if SILVERLIGHT
        public MagPaperList() { }
#else
        private MagPaperList() { }
#endif
        public string AllIds()
        {
            string retval = "";
            foreach (MagPaper mp in this)
            {
                if (retval == "")
                {
                    retval = mp.PaperId.ToString();
                }
                else
                {
                    retval += mp.PaperId.ToString();
                }
            }
            return retval;
        }

        public void ResetSelected()
        {
            foreach (MagPaper p in this)
            {
                p.IsSelected = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public bool MoveToFirstPage()
        {
            return this.MoveToPage(0);
        }

        public bool MoveToLastPage()
        {
            return (((this.TotalItemCount != -1) && (this.PageSize > 0)) && this.MoveToPage(this.PageCount - 1));
        }

        public bool MoveToNextPage()
        {
            return MoveToPage(_pageIndex + 1);
        }

        public bool MoveToPreviousPage() { return true; }

        private bool RaisePageChanging(int newPageIndex)
        {
            EventHandler<PageChangingEventArgs> handler = this.PageChanging;
            if (handler != null)
            {
                PageChangingEventArgs pageChangingEventArgs = new PageChangingEventArgs(newPageIndex);
                //handler(this, pageChangingEventArgs);
                return pageChangingEventArgs.Cancel;
            }

            return false;
        }

        public bool MoveToPage(int pageIndex)
        {
            if (pageIndex < -1)
            {
                return false;
            }
            if ((pageIndex == -1) && (this.PageSize > 0))
            {
                return false;
            }
            if ((pageIndex >= this.PageCount) || (this._pageIndex == pageIndex))
            {
                return false;
            }
            return false;
        }

        public bool CanChangePage { get { return true; ; } }

        public event EventHandler<EventArgs> PageChanged;

        public event EventHandler<System.ComponentModel.PageChangingEventArgs> PageChanging;

        [Newtonsoft.Json.JsonProperty]
        public int PageCount
        {
            get
            {
                if (this._pageSize <= 0)
                {
                    return 0;
                }
                return Math.Max(1, (int)Math.Ceiling(((double)this.ItemCount) / ((double)this._pageSize)));

            }
        }

        private int _totalItemCount;
        [Newtonsoft.Json.JsonProperty]
        public int TotalItemCount
        {
            get
            {
                return _totalItemCount;
            }
            set
            {
                if (_totalItemCount != value)
                {
                    _totalItemCount = value;
                }
            }
        }


        private int _pageSize;//= 700;
        [Newtonsoft.Json.JsonProperty]
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (_pageSize != value && value >= 1)
                {
                    _pageSize = value;
                    //NotifyPropertyChanged("PageSize"); // for implementing INotifyPropertyChanged
                }
            }
        }

        private int _pageIndex;
        [Newtonsoft.Json.JsonProperty]
        public int PageIndex
        {
            get
            {
                return _pageIndex;
            }
        }

        [Newtonsoft.Json.JsonProperty]
        public int ItemCount
        {
            get
            {
                return TotalItemCount;
            }
            set
            {
                TotalItemCount = value;
            }
        }

        private bool _isPageChanging;

        public bool IsPageChanging
        {
            get { return _isPageChanging; }
            private set
            {
                if (_isPageChanging != value)
                {
                    _isPageChanging = value;
                }
            }
        }


        private Int64 _FieldOfStudyId;
        public Int64 FieldOfStudyId
        {
            get
            {
                return _FieldOfStudyId;
            }
            set
            {
                _FieldOfStudyId = value;
            }
        }

        private Int64 _PaperId;
        public Int64 PaperId
        {
            get
            {
                return _PaperId;
            }
            set
            {
                _PaperId = value;
            }
        }

        private Int64 _AuthorId;
        public Int64 AuthorId
        {
            get
            {
                return _AuthorId;
            }
            set
            {
                _AuthorId = value;
            }
        }

        private int _MagRelatedRunId;
        public int MagRelatedRunId
        {
            get
            {
                return _MagRelatedRunId;
            }
            set
            {
                _MagRelatedRunId = value;
            }
        }

        private string _PaperIds;
        public string PaperIds
        {
            get
            {
                return _PaperIds;
            }
            set
            {
                _PaperIds = value;
            }
        }

        private string _IncludedOrExcluded;
        public string IncludedOrExcluded
        {
            get
            {
                return _IncludedOrExcluded;
            }
            set
            {
                _IncludedOrExcluded = value;
            }
        }

        private string _AttributeIds;
        public string AttributeIds
        {
            get
            {
                return _AttributeIds;
            }
            set
            {
                _AttributeIds = value;
            }
        }

        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info)
        {
            base.OnSetState(info);
            // Paging properties
            _pageIndex = info.GetValue<int>("_pageIndex");
            _totalItemCount = info.GetValue<int>("_totalItemCount");
            _pageSize = info.GetValue<int>("_pageSize");
            _isPageChanging = info.GetValue<bool>("_isPageChanging");
            _FieldOfStudyId = info.GetValue<Int64>("_FieldOfStudyId");
            _PaperId = info.GetValue<Int64>("_PaperId");
            _AuthorId = info.GetValue<Int64>("_AuthorId");
            _MagRelatedRunId = info.GetValue<int>("_MagRelatedRunId");
            _PaperIds = info.GetValue<string>("_PaperIds");
            _IncludedOrExcluded = info.GetValue<string>("_IncludedOrExcluded");
            _AttributeIds = info.GetValue<string>("_AttributeIds");
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info)
        {
            base.OnGetState(info);
            // Paging properties
            info.AddValue("_pageIndex", _pageIndex);
            info.AddValue("_totalItemCount", _totalItemCount);
            info.AddValue("_pageSize", _pageSize);
            info.AddValue("_isPageChanging", _isPageChanging);
            info.AddValue("_FieldOfStudyId", _FieldOfStudyId);
            info.AddValue("_PaperId", _PaperId);
            info.AddValue("_AuthorId", _AuthorId);
            info.AddValue("_MagRelatedRunId", _MagRelatedRunId);
            info.AddValue("_PaperIds", _PaperIds);
            info.AddValue("_IncludedOrExcluded", _IncludedOrExcluded);
            info.AddValue("_AttributeIds", _AttributeIds);
        }


#if SILVERLIGHT
       
#else

        private class MakesResponse
        {
            public string expr { get; set; }
            public List<PaperMakes> entities { get; set; }
        }
        

        protected void DataPortal_Fetch(MagPaperListSelectionCriteria selectionCriteria)
        {
            /*** OLD VERSION - USES LOCAL MAG RATHER THANK MAKES
             
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            PageSize = selectionCriteria.PageSize;
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                using (SqlCommand command = SpecifyListCommand(connection, selectionCriteria, ri))
                {
                    command.CommandTimeout = 500; // a bit longer, as some of these lists are long!
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@PageNo", selectionCriteria.PageNumber + 1));
                    command.Parameters.Add(new SqlParameter("@RowsPerPage", selectionCriteria.PageSize));
                    command.Parameters.Add(new SqlParameter("@Total", 0));
                    command.Parameters["@Total"].Direction = System.Data.ParameterDirection.Output;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(MagPaper.GetMagPaper(reader));
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _pageIndex = selectionCriteria.PageNumber;
                            _totalItemCount = reader.GetInt32("@Total");
                        }
                    }
                }
                connection.Close();
            }
            */

            /* 
             * VERSION 2 - USING MAKES API (VERSION 1 OF MAKES DOESN'T INCLUDE AUTHORS, ABSTRACTS, JOURNALS ETC, SO SWITCHING TO AZURE SEARCH LOOKUP ***
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            PageSize = selectionCriteria.PageSize;
            string Ids = "";
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                using (SqlCommand command = SpecifyListPaperIdsCommand(connection, selectionCriteria, ri))
                {
                    command.CommandTimeout = 500; // a bit longer, as some of these lists are long!
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@PageNo", selectionCriteria.PageNumber + 1));
                    command.Parameters.Add(new SqlParameter("@RowsPerPage", selectionCriteria.PageSize));
                    command.Parameters.Add(new SqlParameter("@Total", 0));
                    command.Parameters["@Total"].Direction = System.Data.ParameterDirection.Output;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            if (Ids == "")
                            {
                                Ids = "Id=" + reader["PaperId"].ToString();
                            }
                            else
                            {
                                Ids += ",Id=" + reader["PaperId"].ToString();
                            }
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _pageIndex = selectionCriteria.PageNumber;
                            _totalItemCount = reader.GetInt32("@Total");
                        }
                    }
                }
                connection.Close();
            }

            if (Ids != "")
            {
                string responseText = "";
                WebRequest request = WebRequest.Create(URL + "?expr=Or(" + Ids + ")&attributes=AAAuId,AADAuN,CC,D,DN,DOI,FP,I,JJId,JJN,LP,PB,RId,S,Ti,V,Y&count=15");
                WebResponse response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    responseText = reader.ReadToEnd();
                }
                response.Close();

                var respJson = JsonConvert.DeserializeObject<MakesResponse>(responseText);

                foreach (PaperMakes pm in respJson.entities)
                {
                    Add(MagPaper.GetMagPaper(pm));
                }
            }
            */

            // new hybrid system: SQL where necessary, but AzSearch and MAKES where possible

            string originalListType = selectionCriteria.ListType;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            PageSize = selectionCriteria.PageSize;
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                if (selectionCriteria.ListType == "CitationsList")
                {
                    selectionCriteria.ListType = "PaperListById"; // put the IDs into this list and then match up below to get the items in the review info
                    if (selectionCriteria.MagPaperId != 0)
                    {
                        string responseText = "";
                        WebRequest request = WebRequest.Create(ConfigurationManager.AppSettings["AzureMAKESBaseURL"] + @"?expr=Id=" + selectionCriteria.MagPaperId.ToString() +
                            "&attributes=RId");
                        WebResponse response = request.GetResponse();
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(dataStream);
                            responseText = reader.ReadToEnd();
                        }
                        response.Close();

                        var respJson = JsonConvert.DeserializeObject<MakesResponse>(responseText);
                        selectionCriteria.PaperIds = "";
                        if (respJson.entities.Count == 1 && respJson.entities[0].RId != null)
                        {
                            foreach (Int64 RId in respJson.entities[0].RId)
                            {
                                selectionCriteria.PaperIds += selectionCriteria.PaperIds == "" ? RId.ToString() : "," + RId.ToString();
                            }
                        }
                    }
                }

                if (selectionCriteria.ListType == "CitedByList")
                {
                    selectionCriteria.ListType = "PaperListById"; // put the IDs into this list and then match up below to get the items in the review info
                    if (selectionCriteria.MagPaperId != 0)
                    {
                        string responseText = "";
                        WebRequest request = WebRequest.Create(ConfigurationManager.AppSettings["AzureMAKESBaseURL"] + "?expr=RId=" + selectionCriteria.MagPaperId.ToString() +
                            "&attributes=Id&count=100");
                        WebResponse response = request.GetResponse();
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(dataStream);
                            responseText = reader.ReadToEnd();
                        }
                        response.Close();

                        var respJson = JsonConvert.DeserializeObject<MakesResponse>(responseText);
                        selectionCriteria.PaperIds = "";
                        if (respJson.entities.Count > 0)
                        {
                            foreach (PaperMakes pm in respJson.entities)
                            {
                                selectionCriteria.PaperIds += selectionCriteria.PaperIds == "" ? pm.Id.ToString() : "," + pm.Id.ToString();
                            }
                        }
                    }
                }

                using (SqlCommand command = SpecifyListPaperIdsCommand(connection, selectionCriteria, ri))
                {
                    command.CommandTimeout = 500; // a bit longer, as some of these lists are long!
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@PageNo", selectionCriteria.PageNumber + 1));
                    command.Parameters.Add(new SqlParameter("@RowsPerPage", selectionCriteria.PageSize));
                    command.Parameters.Add(new SqlParameter("@Total", 0));
                    command.Parameters["@Total"].Direction = System.Data.ParameterDirection.Output;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            PaperAzureSearch pas = MagPaperAzureSearch.GetPaperAzureSearch(reader["PaperId"].ToString());
                            if (pas.id > 0)
                            {
                                Add(MagPaper.GetMagPaper(pas, reader));
                            }
                            else
                            {
                                //Add(MagPaper.GetMagPaper(pas, reader));
                            }
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _pageIndex = selectionCriteria.PageNumber;
                            _totalItemCount = reader.GetInt32("@Total");
                        }
                    }
                }
                connection.Close();
                if (originalListType == "CitedByList" || originalListType == "CitationsList")
                {
                    selectionCriteria.ListType = originalListType;
                    selectionCriteria.PaperIds = "";
                }
            }

            RaiseListChangedEvents = true;
        }

        private SqlCommand SpecifyListPaperIdsCommand(SqlConnection connection, MagPaperListSelectionCriteria criteria, ReviewerIdentity ri)
        {
            SqlCommand command = null;
            switch (criteria.ListType)
            {
                case "ReviewMatchedPapers":
                    command = new SqlCommand("st_ReviewMatchedPapersIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@INCLUDED", criteria.Included));
                    this.IncludedOrExcluded = criteria.Included;
                    this.PaperIds = ""; // probably unnecessary, but just in case...
                    break;
                case "ReviewMatchedPapersWithThisCode":
                    command = new SqlCommand("st_ReviewMatchedPapersWithThisCodeIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_IDS", criteria.AttributeIds));
                    this.AttributeIds = criteria.AttributeIds;
                    this.PaperIds = ""; // probably unnecessary, but just in case...
                    break;
                case "ItemMatchedPapersList":
                    command = new SqlCommand("st_ItemMatchedPapersIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.ITEM_ID));
                    break;
                case "CitationsList":
                    command = new SqlCommand("st_PaperCitationsIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.MagPaperId));
                    this.PaperId = criteria.MagPaperId; // storing this in the object helps with paging
                    break;
                case "CitedByList":
                    command = new SqlCommand("st_PaperCitedByIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.MagPaperId));
                    this.PaperId = criteria.MagPaperId;
                    break;
                case "RecommendationsList":
                    command = new SqlCommand("st_PaperRecommendationsIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.MagPaperId));
                    this.PaperId = criteria.MagPaperId;
                    break;
                case "RecommendedByList":
                    command = new SqlCommand("st_PaperRecommendedByIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.MagPaperId));
                    this.PaperId = criteria.MagPaperId;
                    this.PaperIds = "";
                    break;
                case "PaperFieldsOfStudyList":
                    command = new SqlCommand("st_FieldOfStudyPapersIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@FieldOfStudyId", criteria.FieldOfStudyId));
                    this.FieldOfStudyId = criteria.FieldOfStudyId;
                    break;
                case "AuthorPaperList":
                    // NOT IMPLEMENTED YET - CAN GET A LIST OF PAPERS BY GIVEN AUTHOR VIA AZURE SEARCH STORED PROC: st_AuthorPapersIds
                    //command = new SqlCommand("st_AuthorPapers", connection);
                    //command.CommandType = System.Data.CommandType.StoredProcedure;
                    //command.Parameters.Add(new SqlParameter("@AuthorId", criteria.AuthorId));
                    //this.AuthorId = criteria.AuthorId;
                    break;
                case "MagRelatedPapersRunList":
                    command = new SqlCommand("st_MagRelatedPapersListIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", criteria.MagRelatedRunId));
                    this.MagRelatedRunId = criteria.MagRelatedRunId;
                    this.PaperIds = "";
                    this.AttributeIds = "";
                    break;
                case "PaperListById":
                    command = new SqlCommand("st_PaperListByIdIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperIds", criteria.PaperIds));
                    this.PaperIds = criteria.PaperIds;
                    this._PaperId = 0;
                    break;
            }
            return command;
        }

        private SqlCommand SpecifyListCommand(SqlConnection connection, MagPaperListSelectionCriteria criteria, ReviewerIdentity ri)
        {
            SqlCommand command = null;
            /*
            switch (criteria.ListType)
            {
                case "ReviewMatchedPapers":
                    command = new SqlCommand("st_ReviewMatchedPapers", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@INCLUDED", criteria.Included));
                    this.IncludedOrExcluded = criteria.Included;
                    this.PaperIds = ""; // probably unnecessary, but just in case...
                    break;
                case "ReviewMatchedPapersWithThisCode":
                    command = new SqlCommand("st_ReviewMatchedPapersWithThisCode", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_IDS", criteria.AttributeIds));
                    this.AttributeIds = criteria.AttributeIds;
                    this.PaperIds = ""; // probably unnecessary, but just in case...
                    break;
                case "ItemMatchedPapersList":
                    command = new SqlCommand("st_ItemMatchedPapers", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.ITEM_ID));
                    break;
                case "CitationsList":
                    command = new SqlCommand("st_PaperCitations", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.MagPaperId));
                    this.PaperId = criteria.MagPaperId; // storing this in the object helps with paging
                    break;
                case "CitedByList":
                    command = new SqlCommand("st_PaperCitedBy", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.MagPaperId));
                    this.PaperId = criteria.MagPaperId;
                    break;
                case "RecommendationsList":
                    command = new SqlCommand("st_PaperRecommendations", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.MagPaperId));
                    this.PaperId = criteria.MagPaperId;
                    break;
                case "RecommendedByList":
                    command = new SqlCommand("st_PaperRecommendedBy", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.MagPaperId));
                    this.PaperId = criteria.MagPaperId;
                    this.PaperIds = "";
                    break;
                case "PaperFieldsOfStudyList":
                    command = new SqlCommand("st_FieldOfStudyPapers", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@FieldOfStudyId", criteria.FieldOfStudyId));
                    this.FieldOfStudyId = criteria.FieldOfStudyId;
                    break;
                case "AuthorPaperList":
                    command = new SqlCommand("st_AuthorPapers", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@AuthorId", criteria.AuthorId));
                    this.AuthorId = criteria.AuthorId;
                    break;
                case "MagRelatedPapersRunList":
                    command = new SqlCommand("st_MagRelatedPapersList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_RELATED_RUN_ID", criteria.MagRelatedRunId));
                    this.MagRelatedRunId = criteria.MagRelatedRunId;
                    this.PaperIds = "";
                    this.AttributeIds = "";
                    break;
                case "PaperListById":
                    command = new SqlCommand("st_PaperListById", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperIds", criteria.PaperIds));
                    this.PaperIds = criteria.PaperIds;
                    this._PaperId = 0;
                    break;
            }*/
            return command;
            
        }



#endif



            }

    // used to define the parameters for the query.
    [Serializable]
    public class MagPaperListSelectionCriteria : BusinessBase //Csla.CriteriaBase
    {
        public MagPaperListSelectionCriteria() { }
        public static readonly PropertyInfo<Int64> MagPaperIdProperty = RegisterProperty<Int64>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<Int64>("MagPaperId", "MagPaperId"));
        public Int64 MagPaperId
        {
            get { return ReadProperty(MagPaperIdProperty); }
            set
            {
                SetProperty(MagPaperIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> ITEM_IDProperty = RegisterProperty<Int64>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<Int64>("ITEM_ID", "ITEM_ID"));
        public Int64 ITEM_ID
        {
            get { return ReadProperty(ITEM_IDProperty); }
            set
            {
                SetProperty(ITEM_IDProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ListTypeProperty = RegisterProperty<string>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<string>("ListType", "ListType", ""));
        public string ListType
        {
            get { return ReadProperty(ListTypeProperty); }
            set
            {
                SetProperty(ListTypeProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> FieldOfStudyIdProperty = RegisterProperty<Int64>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<Int64>("FieldOfStudyId", "FieldOfStudyId"));
        public Int64 FieldOfStudyId
        {
            get { return ReadProperty(FieldOfStudyIdProperty); }
            set
            {
                SetProperty(FieldOfStudyIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> AuthorIdProperty = RegisterProperty<Int64>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<Int64>("AuthorId", "AuthorId"));
        public Int64 AuthorId
        {
            get { return ReadProperty(AuthorIdProperty); }
            set
            {
                SetProperty(AuthorIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> MagRelatedRunIdProperty = RegisterProperty<int>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<int>("MagRelatedRunId", "MagRelatedRunId", 0));
        public int MagRelatedRunId
        {
            get { return ReadProperty(MagRelatedRunIdProperty); }
            set
            {
                SetProperty(MagRelatedRunIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> PaperIdsProperty = RegisterProperty<string>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<string>("PaperIds", "PaperIds", ""));
        public string PaperIds
        {
            get { return ReadProperty(PaperIdsProperty); }
            set
            {
                SetProperty(PaperIdsProperty, value);
            }
        }

        public static readonly PropertyInfo<string> AttributeIdsProperty = RegisterProperty<string>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<string>("AttributeIds", "AttributeIds", ""));
        public string AttributeIds
        {
            get { return ReadProperty(AttributeIdsProperty); }
            set
            {
                SetProperty(AttributeIdsProperty, value);
            }
        }

        public static readonly PropertyInfo<string> IncludedProperty = RegisterProperty<string>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<string>("Included", "Included", string.Empty));
        public string Included
        {
            get { return ReadProperty(IncludedProperty); }
            set
            {
                SetProperty(IncludedProperty, value);
            }
        }

        public static readonly PropertyInfo<int> PageNumberProperty = RegisterProperty<int>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<int>("PageNumber", "PageNumber", 0));
        public int PageNumber
        {
            get { return ReadProperty(PageNumberProperty); }
            set
            {
                SetProperty(PageNumberProperty, value);
            }
        }

        public static readonly PropertyInfo<int> PageSizeProperty = RegisterProperty<int>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<int>("PageSize", "PageSize", 1));
        public int PageSize
        {
            get { return ReadProperty(PageSizeProperty); }
            set
            {
                SetProperty(PageSizeProperty, value);
            }
        }

        public static readonly PropertyInfo<int> NumResultsProperty = RegisterProperty<int>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<int>("NumResults", "NumResults", 1));
        public int NumResults
        {
            get { return ReadProperty(NumResultsProperty); }
            set
            {
                SetProperty(NumResultsProperty, value);
            }
        }
    }
}
