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
        public MagPaperList() { }
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

        private int _MagAutoUpdateRunId;
        public int MagAutoUpdateRunId
        {
            get
            {
                return _MagAutoUpdateRunId;
            }
            set
            {
                _MagAutoUpdateRunId = value;
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

        private string _MagSearchText;
        public string MagSearchText
        {
            get
            {
                return _MagSearchText;
            }
            set
            {
                _MagSearchText = value;
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
            _MagSearchText = info.GetValue<string>("_MagSearchText");
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
            info.AddValue("_MagSearchText", _MagSearchText);
        }


#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch(MagPaperListSelectionCriteria selectionCriteria)
        {
             // There are two types of list: one where we look up the items in SQL first, and one where the list comes from MAKES, and
             // we do an SQL lookup to match up IDs.
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            PageSize = selectionCriteria.PageSize;
            _pageIndex = selectionCriteria.PageNumber;
            string Ids = "";
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                // These lists start with an SQL query, and we then grab paper info from MAKES
                if (selectionCriteria.ListType == "ReviewMatchedPapers" ||
                    selectionCriteria.ListType == "ReviewMatchedPapersWithThisCode" ||
                    selectionCriteria.ListType == "ItemMatchedPapersList" ||
                    selectionCriteria.ListType == "MagRelatedPapersRunList" ||
                    selectionCriteria.ListType == "MagAutoUpdateRunPapersList")
                {
                    using (SqlCommand command = SpecifyListPaperIdsCommand(connection, selectionCriteria, ri))
                    {
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                        command.Parameters.Add(new SqlParameter("@PageNo", selectionCriteria.PageNumber + 1));
                        command.Parameters.Add(new SqlParameter("@RowsPerPage", selectionCriteria.PageSize));
                        command.Parameters.Add(new SqlParameter("@Total", 0));
                        command.Parameters["@Total"].Direction = System.Data.ParameterDirection.Output;
                        
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                MagPaper newRow = MagPaper.GetMagPaperFromMakes(reader.GetInt64("PaperId"), reader);
                                if (selectionCriteria.ListType == "MagRelatedPapersRunList" ||
                                    selectionCriteria.ListType == "MagAutoUpdateRunPapersList")
                                {
                                    newRow.SimilarityScore = reader.GetDouble("SimilarityScore");
                                }
                                Add(newRow);
                            }
                            reader.NextResult();
                            if (reader.Read())
                            {
                                _pageIndex = selectionCriteria.PageNumber;
                                _totalItemCount = reader.GetInt32("@Total");
                            }
                        }
                    }
                }
                else // i.e. these list types: CitedByList, RecommendationsList, RecommendedByList, PaperFieldsOfStudyList, MagSearchResultsList
                     // AuthorPaperList, PaperListById (selected items).
                    //  These query MAKES for the list of PaperIds and then we look up in our database to see whether they are in the review.
                {
                    MagMakesHelpers.PaperMakes mpParent = null;
                    MagMakesHelpers.FieldOfStudyMakes fosParent = null;
                    string queryOffset = (PageIndex * _pageSize).ToString();

                    string searchString = "";
                    switch (selectionCriteria.ListType)
                    {
                        case "PaperFieldsOfStudyList":
                            searchString = "Composite(F.FId=" + selectionCriteria.FieldOfStudyId.ToString() + ")";                          
                            if (selectionCriteria.DateFrom != "" && selectionCriteria.DateTo != "")
                            {
                                searchString = "AND(" + searchString + ",D=['" + selectionCriteria.DateFrom + "','" +
                                    selectionCriteria.DateTo + "'])";
                            }
                            else
                            {
                                if (selectionCriteria.DateFrom != "")
                                {
                                    searchString = "AND(" + searchString + ",D>='" + selectionCriteria.DateFrom + "')";
                                }
                                else
                                {
                                    if (selectionCriteria.DateTo != "") // though think this is always true here
                                    {
                                        searchString = "AND(" + searchString + ",D<='" + selectionCriteria.DateTo + "')";
                                    }
                                }
                            }
                            this.FieldOfStudyId = selectionCriteria.FieldOfStudyId;
                            /*
                            fosParent = MagMakesHelpers.EvaluateSingleFieldOfStudyId(selectionCriteria.FieldOfStudyId.ToString());
                            if (fosParent != null)
                            {
                                _totalItemCount = fosParent.CC;
                            }
                            */
                            // replaced the above with this, so that the count is correct even with date filters
                            MagMakesHelpers.MakesCalcHistogramResponse resp = MagMakesHelpers.CalcHistoramCount(searchString);
                            if (resp != null)
                            {
                                foreach (MagMakesHelpers.histograms hs in resp.histograms)
                                {
                                    if (hs.attribute == "Id")
                                    {
                                        _totalItemCount = hs.total_count;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                _totalItemCount = 0;
                            }
                            break;
                        case "CitedByList":
                            searchString = "RId=" + selectionCriteria.MagPaperId.ToString();
                            this.PaperIds = selectionCriteria.PaperIds;
                            this.PaperId = selectionCriteria.MagPaperId;
                            mpParent = MagMakesHelpers.GetPaperMakesFromMakes(selectionCriteria.MagPaperId);
                            if (mpParent != null)
                            {
                                _totalItemCount = mpParent.CC;
                            }
                            break;
                        case "CitationsList":
                            mpParent = MagMakesHelpers.GetPaperMakesFromMakes(selectionCriteria.MagPaperId);
                            string ids = "";
                            if (mpParent != null && mpParent.RId != null)
                            {
                                _totalItemCount = mpParent.RId.Count;

                                for(int i = _pageIndex * selectionCriteria.PageSize; i < mpParent.RId.Count && i <
                                    ((PageIndex * selectionCriteria.PageSize) + selectionCriteria.PageSize); i++)
                                {
                                    ids += ids == "" ? mpParent.RId[i].ToString() : "," + mpParent.RId[i].ToString();
                                }
                            }
                            else
                            {
                                _totalItemCount = 0;
                            }
                            searchString = "OR(Id=" + ids.Replace(",", ", Id=") + ")";
                            this.PaperIds = selectionCriteria.PaperIds;
                            this.PaperId = selectionCriteria.MagPaperId;
                            queryOffset = "0";
                            
                            break;
                        case "PaperListById":
                            searchString = "OR(Id=" + selectionCriteria.PaperIds.Replace(",", ", Id=") + ")";
                            if (selectionCriteria.PaperIds != "")
                            {
                                _totalItemCount = selectionCriteria.PaperIds.ToCharArray().Count(c => c == ',') + 1;
                            }
                            else
                            {
                                _totalItemCount = 0;
                                searchString = "";
                            }
                            break;
                        case "MagSearchResultsList":
                            searchString = selectionCriteria.MagSearchText;
                            MagSearchText = searchString;
                            resp = MagMakesHelpers.CalcHistoramCount(searchString);
                            foreach (MagMakesHelpers.histograms hs in resp.histograms)
                            {
                                if (hs.attribute == "Id")
                                {
                                    _totalItemCount = hs.total_count;
                                    break;
                                }
                            }
                            break;
                        case "RecommendationsList": // recommendations not currently in MAKES :(
                            //searchString = "Composite(RId=" + selectionCriteria.MagPaperId.ToString() + ")";
                            break;
                        case "RecommendedByList":
                            //searchString = "Composite(RId=" + selectionCriteria.MagPaperId.ToString() + ")";
                            break;
                    }

                    if (searchString != "" && _totalItemCount > 0)
                    {
                        MagMakesHelpers.PaperMakesResponse pmr = MagMakesHelpers.EvaluateExpressionWithPaging(searchString, selectionCriteria.PageSize.ToString(),
                            queryOffset);

                        if (pmr.entities != null && pmr.entities.Count > 0)
                        {
                            foreach (MagMakesHelpers.PaperMakes pm in pmr.entities)
                            {
                                MagPaper mp = MagPaper.GetMagPaperFromPaperMakes(pm, null);
                                if (mp != null)
                                {
                                    this.Add(mp);
                                }
                            }
                        }
                        CheckIfPapersAlreadyInReview();
                    }
                } // end IF for the set of list types that start with a MAKES search

                if (selectionCriteria.ListType == "PaperFieldsOfStudyList")
                {
                    _pageIndex = selectionCriteria.PageNumber;
                }
                connection.Close();
            }

            RaiseListChangedEvents = true;
        }

        private SqlCommand SpecifyListPaperIdsCommand(SqlConnection connection, MagPaperListSelectionCriteria criteria, ReviewerIdentity ri)
        {
            SqlCommand command = null;
            switch (criteria.ListType)
            {
                case "ReviewMatchedPapers":
                    command = new SqlCommand("st_MagReviewMatchedPapersIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@INCLUDED", criteria.Included));
                    this.IncludedOrExcluded = criteria.Included;
                    this.PaperIds = ""; // probably unnecessary, but just in case...
                    break;
                case "ReviewMatchedPapersWithThisCode":
                    command = new SqlCommand("st_MagReviewMatchedPapersWithThisCodeIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_IDS", criteria.AttributeIds));
                    this.AttributeIds = criteria.AttributeIds;
                    this.PaperIds = ""; // probably unnecessary, but just in case...
                    break;
                case "ItemMatchedPapersList":
                    command = new SqlCommand("st_MagItemMatchedPapersIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.ITEM_ID));
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
                    command = new SqlCommand("st_MagPaperListByIdIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperIds", criteria.PaperIds));
                    this.PaperIds = criteria.PaperIds;
                    this._PaperId = 0;
                    break;
                case "MagAutoUpdateRunPapersList":
                    command = new SqlCommand("st_MagAutoUpdateRunListIds", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagAutoUpdateRunId", criteria.MagAutoUpdateRunId));
                    command.Parameters.Add(new SqlParameter("@OrderBy", criteria.AutoUpdateOrderBy));
                    command.Parameters.Add(new SqlParameter("@AutoUpdateScore", criteria.AutoUpdateAutoUpdateScore));
                    command.Parameters.Add(new SqlParameter("@StudyTypeClassifierScore", criteria.AutoUpdateStudyTypeClassifierScore));
                    command.Parameters.Add(new SqlParameter("@UserClassifierScore", criteria.AutoUpdateUserClassifierScore));
                    command.Parameters.Add(new SqlParameter("@AutoupdateUserTopN", criteria.AutoUpdateUserTopN));
                    this.MagAutoUpdateRunId = criteria.MagAutoUpdateRunId;
                    this.PaperIds = "";
                    this.AttributeIds = "";
                    break;
            }
            return command;
        }

        private void CheckIfPapersAlreadyInReview()
        {
            if (this.Count == 0)
            {
                return;
            }
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                string ids = "";
                foreach (MagPaper mp in this)
                {
                    if (ids == "")
                    {
                        ids = mp.PaperId.ToString();
                    }
                    else
                    {
                        ids += "," + mp.PaperId.ToString();
                    }
                }
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagPaperListByIdIds", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperIds", ids));
                    command.Parameters.Add(new SqlParameter("@PageNo", 1));
                    command.Parameters.Add(new SqlParameter("@RowsPerPage", PageSize));
                    command.Parameters.Add(new SqlParameter("@Total", 0));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters["@Total"].Direction = System.Data.ParameterDirection.Output;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            foreach (MagPaper mp in this)
                            {
                                if (mp.PaperId == reader.GetInt64("PaperId"))
                                {
                                    mp.LinkedITEM_ID = reader.GetInt64("ITEM_ID");
                                    mp.ManualTrueMatch = reader.GetBoolean("ManualTrueMatch");
                                    mp.ManualFalseMatch = reader.GetBoolean("ManualFalseMatch");
                                    mp.AutoMatchScore = reader.GetDouble("AutoMatchScore");
                                }
                            }
                        }
                    }
                }
                connection.Close();
            }
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

        public static readonly PropertyInfo<int> MagAutoUpdateRunIdProperty = RegisterProperty<int>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<int>("MagAutoUpdateRunId", "MagAutoUpdateRunId", 0));
        public int MagAutoUpdateRunId
        {
            get { return ReadProperty(MagAutoUpdateRunIdProperty); }
            set
            {
                SetProperty(MagAutoUpdateRunIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> AutoUpdateOrderByProperty = RegisterProperty<string>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<string>("AutoUpdateOrderBy", "AutoUpdateOrderBy", "AutoUpdate"));
        public string AutoUpdateOrderBy
        {
            get { return ReadProperty(AutoUpdateOrderByProperty); }
            set
            {
                SetProperty(AutoUpdateOrderByProperty, value);
            }
        }

        public static readonly PropertyInfo<double> AutoUpdateAutoUpdateScoreProperty = RegisterProperty<double>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<double>("AutoUpdateAutoUpdateScore", "AutoUpdateAutoUpdateScore"));
        public double AutoUpdateAutoUpdateScore
        {
            get { return ReadProperty(AutoUpdateAutoUpdateScoreProperty); }
            set
            {
                SetProperty(AutoUpdateAutoUpdateScoreProperty, value);
            }
        }

        public static readonly PropertyInfo<double> AutoUpdateStudyTypeClassifierScoreProperty = RegisterProperty<double>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<double>("AutoUpdateStudyTypeClassifierScore", "AutoUpdateStudyTypeClassifierScore"));
        public double AutoUpdateStudyTypeClassifierScore
        {
            get { return ReadProperty(AutoUpdateStudyTypeClassifierScoreProperty); }
            set
            {
                SetProperty(AutoUpdateStudyTypeClassifierScoreProperty, value);
            }
        }

        public static readonly PropertyInfo<double> AutoUpdateUserClassifierScoreProperty = RegisterProperty<double>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<double>("AutoUpdateUserClassifierScore", "AutoUpdateUserClassifierScore"));
        public double AutoUpdateUserClassifierScore
        {
            get { return ReadProperty(AutoUpdateUserClassifierScoreProperty); }
            set
            {
                SetProperty(AutoUpdateUserClassifierScoreProperty, value);
            }
        }

        public static readonly PropertyInfo<int> AutoUpdateUserTopNProperty = RegisterProperty<int>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<int>("AutoUpdateUserTopN", "AutoUpdateUserTopN"));
        public int AutoUpdateUserTopN
        {
            get { return ReadProperty(AutoUpdateUserTopNProperty); }
            set
            {
                SetProperty(AutoUpdateUserTopNProperty, value);
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

        public static readonly PropertyInfo<string> DateFromProperty = RegisterProperty<string>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<string>("DateFrom", "DateFrom", ""));
        public string DateFrom
        {
            get { return ReadProperty(DateFromProperty); }
            set
            {
                SetProperty(DateFromProperty, value);
            }
        }

        public static readonly PropertyInfo<string> DateToProperty = RegisterProperty<string>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<string>("DateTo", "DateTo", ""));
        public string DateTo
        {
            get { return ReadProperty(DateToProperty); }
            set
            {
                SetProperty(DateToProperty, value);
            }
        }

        public static readonly PropertyInfo<string> MagSearchTextProperty = RegisterProperty<string>(typeof(MagPaperListSelectionCriteria), new PropertyInfo<string>("MagSearchText", "MagSearchText", ""));
        public string MagSearchText
        {
            get { return ReadProperty(MagSearchTextProperty); }
            set
            {
                SetProperty(MagSearchTextProperty, value);
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
