using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Data;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemList : DynamicBindingListBase<Item>, System.ComponentModel.IPagedCollectionView, INotifyPropertyChanged
    {

        #region Implementing IPagedCollectionView
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

        private int _pageSize ;//= 700;
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
                    NotifyPropertyChanged("PageSize");
                }
            }
        }

        private int _pageIndex;
        public int PageIndex
        {
            get
            {
                return _pageIndex;
            }
        }

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

        #endregion

        public bool HasSavedHandler = false;

        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info)
        {
            base.OnSetState(info);
            // Paging properties
            _pageIndex = info.GetValue<int>("_pageIndex");
            _totalItemCount = info.GetValue<int>("_totalItemCount");
            _pageSize = info.GetValue<int>("_pageSize");
            _isPageChanging = info.GetValue<bool>("_isPageChanging");
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info)
        {
            base.OnGetState(info);
            // Paging properties
            info.AddValue("_pageIndex", _pageIndex);
            info.AddValue("_totalItemCount", _totalItemCount);
            info.AddValue("_pageSize", _pageSize);
            info.AddValue("_isPageChanging", _isPageChanging);
        }

        // METHOD THAT CALLS ALL VARIATIONS IN ITEM LISTS
        public static void GetItemList(SelectionCriteria selectionCriteria, EventHandler<DataPortalResult<ItemList>> handler)
        {
            DataPortal<ItemList> dp = new DataPortal<ItemList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(selectionCriteria);
        }

        // Lists Ids in the format needed to be used in a udf in the database
        public string ListIds()
        {
            string returnList = "";
            foreach(Item currentItem in this)
            {
                if (returnList == "")
                {
                    returnList = currentItem.ItemId.ToString();
                }
                else
                {
                    returnList += "," + currentItem.ItemId.ToString();
                }
            }
            return returnList;
        }

        public string SelectedIds()
        {
            string returnList = "";
            foreach (Item currentItem in this)
            {
                if (currentItem.IsSelected)
                {
                    if (returnList == "")
                    {
                        returnList = currentItem.ItemId.ToString();
                    }
                    else
                    {
                        returnList += "," + currentItem.ItemId.ToString();
                    }
                }
            }
            return returnList;
        }


#if SILVERLIGHT
        public ItemList() { }

    protected override void AddNewCore()
    {
        Add(Item.NewItem());
    }

#else
        private ItemList() { }
#endif


#if SILVERLIGHT
       
#else

        int retryCount = 0;//used to avoid endless loop in try-catch-(manual)retry
        protected void DataPortal_Fetch(SelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                PageSize = criteria.PageSize;
                try
                {
                    using (SqlCommand command = SpecifyListCommand(connection, criteria, ri))
                    {
                        command.Parameters.Add(new SqlParameter("@PageNum", criteria.PageNumber + 1));
                        command.Parameters.Add(new SqlParameter("@PerPage", _pageSize));
                        command.Parameters.Add(new SqlParameter("@CurrentPage", 0));
                        command.Parameters.Add(new SqlParameter("@TotalPages", 0));
                        command.Parameters.Add(new SqlParameter("@TotalRows", 0));
                        command.Parameters["@CurrentPage"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters["@TotalPages"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters["@TotalRows"].Direction = System.Data.ParameterDirection.Output;
                        //command.CommandTimeout = 1;//used to test timeouts
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            List<long> tIds = new List<long>();
                            while (reader.Read())
                            {
                                Item tmp = Item.GetItem(reader);
                                if (!tIds.Contains(tmp.ItemId))
                                {
                                    tIds.Add(tmp.ItemId);
                                    Add(tmp);
                                }
                            }
                            reader.NextResult();
                            if (reader.Read())
                            {
                                _pageIndex = reader.GetInt32("@CurrentPage") - 1;
                                _totalItemCount = reader.GetInt32("@TotalRows");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (retryCount < 1 && e.Message.ToLower().Contains("timeout"))
                    {
                        retryCount++;
                        string sql = "select i.item_id, IS_INCLUDED, IS_DELETED from tb_item i inner join TB_ITEM_REVIEW ir on i.ITEM_ID = ir.ITEM_ID and REVIEW_ID = @rid";
                        using (SqlCommand commandEx = new SqlCommand(sql, connection))
                        {
                            commandEx.Parameters.Add(new SqlParameter("@rid", System.Data.SqlDbType.Int));
                            commandEx.Parameters["@rid"].Value = ri.ReviewId;
                            commandEx.CommandTimeout = 240;
                            commandEx.ExecuteReader();
                            this.DataPortal_Fetch(criteria);
                        }
                    }
                    else 
                        //in case we already tried the workaround above, avoid an endless loop and return original problem to client
                        throw e;
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }
        
        private SqlCommand SpecifyListCommand(SqlConnection connection, SelectionCriteria criteria, ReviewerIdentity ri)
        {
            SqlCommand command = null;
            switch (criteria.ListType)
            {
                case "StandardItemList":
                    command = new SqlCommand("st_ItemList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@SHOW_INCLUDED", criteria.OnlyIncluded));
                    command.Parameters.Add(new SqlParameter("@SHOW_DELETED", criteria.ShowDeleted));
                    command.Parameters.Add(new SqlParameter("@SOURCE_ID", criteria.SourceId));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID_LIST", criteria.AttributeSetIdList));
                    break;

                case "ItemListWithoutAttributes":
                    command = new SqlCommand("st_ItemListWithoutAttributes", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@SHOW_INCLUDED", criteria.OnlyIncluded));
                    command.Parameters.Add(new SqlParameter("@SHOW_DELETED", criteria.ShowDeleted));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID_LIST", criteria.AttributeSetIdList));
                    break;

                case "GetItemSearchList":
                    command = new SqlCommand("st_ItemSearchList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@SEARCH_ID", criteria.SearchId));
                    break;

                case "GetItemWorkAllocationList":
                    command = new SqlCommand("st_ItemWorkAllocationList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@WORK_ALLOCATION_ID", criteria.WorkAllocationId)); // actually, work allocation id
                    command.Parameters.Add(new SqlParameter("@WHICH_FILTER", "ALL"));
                    break;

                case "GetItemWorkAllocationListStarted":
                    command = new SqlCommand("st_ItemWorkAllocationList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@WORK_ALLOCATION_ID", criteria.WorkAllocationId)); // actually, work allocation id
                    command.Parameters.Add(new SqlParameter("@WHICH_FILTER", "STARTED"));
                    break;

                case "GetItemWorkAllocationListRemaining":
                    command = new SqlCommand("st_ItemWorkAllocationList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@WORK_ALLOCATION_ID", criteria.WorkAllocationId)); // actually, work allocation id
                    command.Parameters.Add(new SqlParameter("@WHICH_FILTER", "REMAINING"));
                    break;

                //start comparison lists
                case "ComparisonAgree1vs2":
                case "ComparisonAgree1vs3":
                case "ComparisonAgree2vs3":
                case "ComparisonDisagree1vs2":
                case "ComparisonDisagree1vs3":
                case "ComparisonDisagree2vs3":
                    command = new SqlCommand("st_ItemComparisonList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@COMPARISON_ID", criteria.ComparisonId)); // actually, comparison id
                    command.Parameters.Add(new SqlParameter("@LIST_WHAT", criteria.ListType));
                    break;
                //end comparison lists

                //start screening comparison lists
                case "ComparisonAgree1vs2Sc":
                case "ComparisonAgree1vs3Sc":
                case "ComparisonAgree2vs3Sc":
                case "ComparisonDisagree1vs2Sc":
                case "ComparisonDisagree1vs3Sc":
                case "ComparisonDisagree2vs3Sc":
                    command = new SqlCommand("st_ItemComparisonScreeningList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@COMPARISON_ID", criteria.ComparisonId)); // actually, comparison id
                    command.Parameters.Add(new SqlParameter("@LIST_WHAT", criteria.ListType));
                    break;
                //end screening comparison lists

                case "ReviewerCodingCompleted":
                    command = new SqlCommand("st_ItemReviewerCodingList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", criteria.ContactId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", criteria.SetId));
                    command.Parameters.Add(new SqlParameter("@COMPLETED", true));
                    break;

                case "ReviewerCodingIncomplete":
                    command = new SqlCommand("st_ItemReviewerCodingListUncomplete", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", criteria.ContactId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", criteria.SetId));
                    //command.Parameters.Add(new SqlParameter("@COMPLETED", false));
                    break;

                case "CrosstabsList":
                    command = new SqlCommand("st_ItemCrosstabsList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@XSET_ID", criteria.XAxisSetId)); // x axis set id
                    command.Parameters.Add(new SqlParameter("@YSET_ID", criteria.YAxisSetId)); // y axis set id
                    command.Parameters.Add(new SqlParameter("@FILTER_SET_ID", criteria.FilterSetId)); // filter attribute id
                    command.Parameters.Add(new SqlParameter("@XATTRIBUTE_ID", criteria.XAxisAttributeId)); // x axis attribute id
                    command.Parameters.Add(new SqlParameter("@YATTRIBUTE_ID", criteria.YAxisAttributeId)); // y axis attribute id
                    command.Parameters.Add(new SqlParameter("@FILTER_ATTRIBUTE_ID", criteria.FilterAttributeId)); // filter attribute id
                    break;
                case "FrequencyNoneOfTheAbove":
                    command = new SqlCommand("st_ItemListFrequencyNoneOfTheAbove", connection);
                    //@ATTRIBUTE_ID BIGINT = null,
                    //@SET_ID BIGINT,
                    //@IS_INCLUDED BIT,
                    //@FILTER_ATTRIBUTE_ID BIGINT,
                    //@REVIEW_ID INT,
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", criteria.SetId)); // x axis set id
                    command.Parameters.Add(new SqlParameter("@IS_INCLUDED", criteria.OnlyIncluded)); // filter attribute id
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", criteria.XAxisAttributeId)); // x axis attribute id
                    command.Parameters.Add(new SqlParameter("@FILTER_ATTRIBUTE_ID", criteria.FilterAttributeId)); // filter attribute id
                    break;
                case "FrequencyWithFilter":
                    command = new SqlCommand("st_ItemListFrequencyWithFilter", connection);
                    //@ATTRIBUTE_ID BIGINT = null,
                    //@SET_ID BIGINT,
                    //@IS_INCLUDED BIT,
                    //@FILTER_ATTRIBUTE_ID BIGINT,
                    //@REVIEW_ID INT,
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", criteria.SetId)); // x axis set id
                    command.Parameters.Add(new SqlParameter("@IS_INCLUDED", criteria.OnlyIncluded)); // filter attribute id
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", criteria.XAxisAttributeId)); // x axis attribute id
                    command.Parameters.Add(new SqlParameter("@FILTER_ATTRIBUTE_ID", criteria.FilterAttributeId)); // filter attribute id
                    break;
                default:
                    break;
            }
            return command;
        }
#endif

    }

    // used to define the parameters for the query.
    [Serializable]
    public class SelectionCriteria : BusinessBase //Csla.CriteriaBase
    {
        private static PropertyInfo<bool> OnlyIncludedProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("OnlyIncluded", "OnlyIncluded", true));
        public bool OnlyIncluded
        {
            get { return ReadProperty(OnlyIncludedProperty); }
            set
            {
                SetProperty(OnlyIncludedProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowDeletedProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("ShowDeleted", "ShowDeleted", false));
        public bool ShowDeleted
        {
            get { return ReadProperty(ShowDeletedProperty); }
            set
            {
                SetProperty(ShowDeletedProperty, value);
            }
        }

        private static PropertyInfo<int> SourceIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("SourceId", "SourceId", 0));
        public int SourceId
        {
            get { return ReadProperty(SourceIdProperty); }
            set
            {
                SetProperty(SourceIdProperty, value);
            }
        }

        private static PropertyInfo<int> SearchIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("SearchId", "SearchId", 0));
        public int SearchId
        {
            get { return ReadProperty(SearchIdProperty); }
            set
            {
                SetProperty(SearchIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> XAxisSetIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("XAxisSetId", "XAxisSetId"));
        public Int64 XAxisSetId
        {
            get { return ReadProperty(XAxisSetIdProperty); }
            set
            {
                SetProperty(XAxisSetIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> XAxisAttributeIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("XAxisAttributeId", "XAxisAttributeId"));
        public Int64 XAxisAttributeId
        {
            get { return ReadProperty(XAxisAttributeIdProperty); }
            set
            {
                SetProperty(XAxisAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> YAxisSetIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("YAxisSetId", "YAxisSetId"));
        public Int64 YAxisSetId
        {
            get { return ReadProperty(YAxisSetIdProperty); }
            set
            {
                SetProperty(YAxisSetIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> YAxisAttributeIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("YAxisAttributeId", "YAxisAttributeId"));
        public Int64 YAxisAttributeId
        {
            get { return ReadProperty(YAxisAttributeIdProperty); }
            set
            {
                SetProperty(YAxisAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> FilterSetIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("FilterSetId", "FilterSetId"));
        public Int64 FilterSetId
        {
            get { return ReadProperty(FilterSetIdProperty); }
            set
            {
                SetProperty(FilterSetIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> FilterAttributeIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("FilterAttributeId", "FilterAttributeId"));
        public Int64 FilterAttributeId
        {
            get { return ReadProperty(FilterAttributeIdProperty); }
            set
            {
                SetProperty(FilterAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeSetIdListProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("AttributeSetIdList", "AttributeSetIdList", string.Empty));
        public string AttributeSetIdList
        {
            get { return ReadProperty(AttributeSetIdListProperty); }
            set
            {
                SetProperty(AttributeSetIdListProperty, value);
            }
        }

        private static PropertyInfo<string> ListTypeProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("ListType", "ListType", "StandardItemList"));
        public string ListType
        {
            get { return ReadProperty(ListTypeProperty); }
            set
            {
                SetProperty(ListTypeProperty, value);
            }
        }

        private static PropertyInfo<int> PageNumberProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("PageNumber", "PageNumber", 0));
        public int PageNumber
        {
            get { return ReadProperty(PageNumberProperty); }
            set
            {
                SetProperty(PageNumberProperty, value);
            }
        }

        private static PropertyInfo<int> PageSizeProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("PageSize", "PageSize", 1));
        public int PageSize
        {
            get { return ReadProperty(PageSizeProperty); }
            set
            {
                SetProperty(PageSizeProperty, value);
            }
        }

        private static PropertyInfo<int> WorkAllocationIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("WorkAllocationId", "WorkAllocationId", 0));
        public int WorkAllocationId
        {
            get { return ReadProperty(WorkAllocationIdProperty); }
            set
            {
                SetProperty(WorkAllocationIdProperty, value);
            }
        }

        private static PropertyInfo<int> ComparisonIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("ComparisonId", "ComparisonId", 0));
        public int ComparisonId
        {
            get { return ReadProperty(ComparisonIdProperty); }
            set
            {
                SetProperty(ComparisonIdProperty, value);
            }
        }

        private static PropertyInfo<string> DescriptionProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("Description", "Description", string.Empty));
        public string Description
        {
            get { return ReadProperty(DescriptionProperty); }
            set
            {
                SetProperty(DescriptionProperty, value);
            }
        }

        private static PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("ContactId", "ContactId", 0));
        public int ContactId
        {
            get { return ReadProperty(ContactIdProperty); }
            set
            {
                SetProperty(ContactIdProperty, value);
            }
        }

        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("SetId", "SetId", 0));
        public int SetId
        {
            get { return ReadProperty(SetIdProperty); }
            set
            {
                SetProperty(SetIdProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowInfoColumnProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("ShowInfoColumn", "ShowInfoColumn", false));
        public bool ShowInfoColumn
        {
            get { return ReadProperty(ShowInfoColumnProperty); }
            set
            {
                SetProperty(ShowInfoColumnProperty, value);
            }
        }

        private static PropertyInfo<bool> ShowScoreColumnProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("ShowScoreColumn", "ShowScoreColumn", false));
        public bool ShowScoreColumn
        {
            get { return ReadProperty(ShowScoreColumnProperty); }
            set
            {
                SetProperty(ShowScoreColumnProperty, value);
            }
        }

        public SelectionCriteria() { }

    }
}
