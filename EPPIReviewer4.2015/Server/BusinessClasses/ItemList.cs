using System;
using System.ComponentModel;
using System.Collections.Generic;
#if !CSLA_NETCORE
using System.Windows.Data;
#endif
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;

#endif

#if CSLA_NETCORE
namespace System.ComponentModel
{
    //
    // Summary:
    //     Defines methods and properties that a collection view implements to provide paging
    //     capabilities to a collection.
    public interface IPagedCollectionView
    {
        //
        // Summary:
        //     Gets a value that indicates whether the System.ComponentModel.IPagedCollectionView.PageIndex
        //     value can change.
        //
        // Returns:
        //     true if the System.ComponentModel.IPagedCollectionView.PageIndex value can change;
        //     otherwise, false.
        bool CanChangePage { get; }
        //
        // Summary:
        //     Gets a value that indicates whether the page index is changing.
        //
        // Returns:
        //     true if the page index is changing; otherwise, false.
        bool IsPageChanging { get; }
        //
        // Summary:
        //     Gets the number of known items in the view before paging is applied.
        //
        // Returns:
        //     The number of known items in the view before paging is applied.
        int ItemCount { get; }
        //
        // Summary:
        //     Gets the zero-based index of the current page.
        //
        // Returns:
        //     The zero-based index of the current page.
        int PageIndex { get; }
        //
        // Summary:
        //     Gets or sets the number of items to display on a page.
        //
        // Returns:
        //     The number of items to display on a page.
        int PageSize { get; set; }
        //
        // Summary:
        //     Gets the total number of items in the view before paging is applied.
        //
        // Returns:
        //     The total number of items in the view before paging is applied, or -1 if the
        //     total number is unknown.
        int TotalItemCount { get; }

        //
        // Summary:
        //     When implementing this interface, raise this event after the System.ComponentModel.IPagedCollectionView.PageIndex
        //     has changed.
        event EventHandler<EventArgs> PageChanged;
        //
        // Summary:
        //     When implementing this interface, raise this event before changing the System.ComponentModel.IPagedCollectionView.PageIndex.
        //     The event handler can cancel this event.
        event EventHandler<PageChangingEventArgs> PageChanging;

        //
        // Summary:
        //     Sets the first page as the current page.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToFirstPage();
        //
        // Summary:
        //     Sets the last page as the current page.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToLastPage();
        //
        // Summary:
        //     Moves to the page after the current page.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToNextPage();
        //
        // Summary:
        //     Moves to the page at the specified index.
        //
        // Parameters:
        //   pageIndex:
        //     The index of the page to move to.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToPage(int pageIndex);
        //
        // Summary:
        //     Moves to the page before the current page.
        //
        // Returns:
        //     true if the operation was successful; otherwise, false.
        bool MoveToPreviousPage();
    }
    //
    // Summary:
    //     Provides data for the System.ComponentModel.IPagedCollectionView.PageChanging
    //     event.
    public sealed class PageChangingEventArgs : CancelEventArgs
    {
        private int _page = 1;
        public PageChangingEventArgs(int newPageIndex)
        {
            _page = newPageIndex;
        }
        public int NewPageIndex
        {
            get { return _page; }
        }
    }
}
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


        private int _pageSize ;//= 700;
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
                    NotifyPropertyChanged("PageSize");
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
        public ItemList() { }
#endif


#if SILVERLIGHT
       
#else
        //method used to create an itemList as a child object
        public static ItemList GetItemList(SelectionCriteria criteria)
        {
            return DataPortal.Fetch<ItemList>(criteria);
        }
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
                    if (criteria.ListType != "WebDbWithWithoutCodes")
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
                    else
                    {
#if WEBDB
                        List<MiniItem> items = new List<MiniItem>();
                        Dictionary<long, string> Wcodes = new Dictionary<long, string>();
                        Dictionary<long, string> WOcodes = new Dictionary<long, string>();
                        using (SqlCommand command = SpecifyListCommand(connection, criteria, ri))
                        {
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                while (reader.Read())
                                {
                                    Wcodes.Add(reader.GetInt64("ATTRIBUTE_ID"), reader.GetString("ATTRIBUTE_NAME"));
                                }
                                reader.NextResult();
                                while (reader.Read())
                                {
                                    WOcodes.Add(reader.GetInt64("ATTRIBUTE_ID"), reader.GetString("ATTRIBUTE_NAME"));
                                }
                                reader.NextResult();
                                while (reader.Read())
                                {
                                    MiniItem mit = new MiniItem(reader.GetInt64("ItemId"));
                                    string[] tmp = reader.GetString("With_atts").Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    string[] tmp2 = reader.GetString("reject").Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string s in tmp)
                                    {
                                        mit.Attributes.Add(long.Parse(s));
                                    }
                                    foreach (string s in tmp2)
                                    {
                                        mit.Attributes2.Add(long.Parse(s));
                                    }
                                    mit.ShortTitle = reader.GetString("SHORT_TITLE");
                                    items.Add(mit);
                                }
                            }
                        }

                        //easy win: if we have "without" codes, remove all items with anything in the "reject" field (Attributes2)
                        if (criteria.WithOutAttributesIdsList != null && criteria.WithOutAttributesIdsList != "")
                            items = items.FindAll(f => f.Attributes2.Count == 0);
                        foreach (KeyValuePair<long, string> kvp in Wcodes)
                        {//remove if item doesn't contain this code
                            items = items.FindAll(f => f.Attributes.Contains(kvp.Key));
                        }
                        //we now have the list of items IDs

                        //sort them by short-tile/ID.
                        items.Sort();
                        //take just the wanted page
                        
                        _totalItemCount = items.Count;
                        _pageSize = criteria.PageSize;
                        //int len = Math.Min(100, items.Count);
                        _pageIndex = Math.Min(PageCount, criteria.PageNumber);
                        
                        if (_pageIndex == PageCount -1)
                        {//last page
                            items = items.GetRange(_pageIndex * PageSize, _totalItemCount - (_pageIndex * PageSize));
                        }
                        else
                        {//a full page
                            items = items.GetRange(_pageIndex * _pageSize, _pageSize);
                        }

                        string Ids = "";
                        foreach (MiniItem itm in items)
                        {
                            Ids += itm.ItemId.ToString() + ",";
                        }
                        Ids = Ids.TrimEnd(',');
                        //st_WebDbItemListFromIDs
                        using (SqlCommand command = new SqlCommand("st_WebDbItemListFromIDs", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                            command.Parameters.Add(new SqlParameter("@Items", Ids));
                            command.Parameters.Add(new SqlParameter("@webDbId", criteria.WebDbId));
                            List<long> tIds = new List<long>();
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                while (reader.Read())
                                {
                                    Item tmp = Item.GetItem(reader);
                                    if (!tIds.Contains(tmp.ItemId))
                                    {
                                        tIds.Add(tmp.ItemId);
                                        Add(tmp);
                                    }
                                }
                            }
                        }
#endif
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

                case "MagMatchesNeedingChecking":
                    command = new SqlCommand("st_ItemListMaybeMagMatches", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@SHOW_INCLUDED", criteria.OnlyIncluded));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID_LIST", criteria.AttributeSetIdList));
                    break;

                case "MagMatchesNotMatched":
                    command = new SqlCommand("st_ItemListMagNoMatches", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@SHOW_INCLUDED", criteria.OnlyIncluded));
                    break;

                case "MagMatchesMatched":
                    command = new SqlCommand("st_ItemListMagMatches", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@SHOW_INCLUDED", criteria.OnlyIncluded));
                    break;

                case "MagSimulationTP":
                    command = new SqlCommand("st_ItemListMagSimulationTPFN", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    //command.Parameters.Add(new SqlParameter("@SHOW_INCLUDED", criteria.OnlyIncluded));
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", criteria.MagSimulationId));
                    command.Parameters.Add(new SqlParameter("@FOUND", true));
                    break;

                case "MagSimulationFN":
                    command = new SqlCommand("st_ItemListMagSimulationTPFN", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    //command.Parameters.Add(new SqlParameter("@SHOW_INCLUDED", criteria.OnlyIncluded));
                    command.Parameters.Add(new SqlParameter("@MAG_SIMULATION_ID", criteria.MagSimulationId));
                    command.Parameters.Add(new SqlParameter("@FOUND", false));
                    break;
#if WEBDB //little trick to avoid making this field add to routine costs in ER4 and ER-Web 
                //this section contains special lists used only by WebDbs
                case "WebDbWithThisCode":
                    command = new SqlCommand("st_WebDbWithThisCode", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@included", criteria.OnlyIncluded));
                    command.Parameters.Add(new SqlParameter("@attributeId", criteria.FilterAttributeId));
                    command.Parameters.Add(new SqlParameter("@WebDbId", criteria.WebDbId));
                    break;
                case "WebDbAllItems":
                    command = new SqlCommand("st_WebDbAllItems", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId)); // use the stored value so that noone can list items out of a review they aren't properly authenticated on
                    command.Parameters.Add(new SqlParameter("@included", criteria.OnlyIncluded));
                    command.Parameters.Add(new SqlParameter("@WebDbId", criteria.WebDbId));
                    break;
                case "WebDbFrequencyNoneOfTheAbove":
                    command = new SqlCommand("st_WebDbItemListFrequencyNoneOfTheAbove", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WebDbId", criteria.WebDbId));
                    command.Parameters.Add(new SqlParameter("@SetId", criteria.SetId)); // x axis set id
                    command.Parameters.Add(new SqlParameter("@included", criteria.OnlyIncluded)); // filter attribute id
                    command.Parameters.Add(new SqlParameter("@ParentAttributeId", criteria.XAxisAttributeId)); // x axis attribute id
                    command.Parameters.Add(new SqlParameter("@FilterAttributeId", criteria.FilterAttributeId)); // filter attribute id
                    break;
                case "WebDbWithWithoutCodes":
                    command = new SqlCommand("st_WebDbItemListWithWithoutCodes", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WebDbId", criteria.WebDbId));
                    command.Parameters.Add(new SqlParameter("@WithAttributesIds", criteria.WithAttributesIds));
                    command.Parameters.Add(new SqlParameter("@WithSetIdsList", criteria.WithSetIdsList));
                    command.Parameters.Add(new SqlParameter("@included", criteria.OnlyIncluded)); 
                    command.Parameters.Add(new SqlParameter("@WithOutAttributesIdsList", criteria.WithOutAttributesIdsList)); 
                    command.Parameters.Add(new SqlParameter("@WithOutSetIdsList", criteria.WithOutSetIdsList)); 
                    break;
                case "WebDbSearch":
                    string que = criteria.SearchString;
                    if (criteria.SearchString.Trim().Contains(' ') && 
                            (
                            criteria.SearchWhat == "TitleAbstract" ||
                            criteria.SearchWhat == "Title" ||
                            criteria.SearchWhat == "Abstract" ||
                            criteria.SearchWhat == "AdditionalText" 
                            )
                        )
                    {//in these cases, SQL uses the 'CONTAINSTABLE' construct, so we need to parse the search text first.
                        FullTextSearch fts = new FullTextSearch(criteria.SearchString.Trim());
                        que = fts.NormalForm;
                    }
                    command = new SqlCommand("st_WebDbSearchFreeText", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WebDbId", criteria.WebDbId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TEXT", que));
                    command.Parameters.Add(new SqlParameter("@SEARCH_WHAT", criteria.SearchWhat));
                    command.Parameters.Add(new SqlParameter("@included", criteria.OnlyIncluded));
                    break;
#endif
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
        public SelectionCriteria() { }
        public static readonly PropertyInfo<bool> OnlyIncludedProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("OnlyIncluded", "OnlyIncluded", true));
        public bool OnlyIncluded
        {
            get { return ReadProperty(OnlyIncludedProperty); }
            set
            {
                SetProperty(OnlyIncludedProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> ShowDeletedProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("ShowDeleted", "ShowDeleted", false));
        public bool ShowDeleted
        {
            get { return ReadProperty(ShowDeletedProperty); }
            set
            {
                SetProperty(ShowDeletedProperty, value);
            }
        }

        public static readonly PropertyInfo<int> SourceIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("SourceId", "SourceId", 0));
        public int SourceId
        {
            get { return ReadProperty(SourceIdProperty); }
            set
            {
                SetProperty(SourceIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> SearchIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("SearchId", "SearchId", 0));
        public int SearchId
        {
            get { return ReadProperty(SearchIdProperty); }
            set
            {
                SetProperty(SearchIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> XAxisSetIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("XAxisSetId", "XAxisSetId"));
        public Int64 XAxisSetId
        {
            get { return ReadProperty(XAxisSetIdProperty); }
            set
            {
                SetProperty(XAxisSetIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> XAxisAttributeIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("XAxisAttributeId", "XAxisAttributeId"));
        public Int64 XAxisAttributeId
        {
            get { return ReadProperty(XAxisAttributeIdProperty); }
            set
            {
                SetProperty(XAxisAttributeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> YAxisSetIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("YAxisSetId", "YAxisSetId"));
        public Int64 YAxisSetId
        {
            get { return ReadProperty(YAxisSetIdProperty); }
            set
            {
                SetProperty(YAxisSetIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> YAxisAttributeIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("YAxisAttributeId", "YAxisAttributeId"));
        public Int64 YAxisAttributeId
        {
            get { return ReadProperty(YAxisAttributeIdProperty); }
            set
            {
                SetProperty(YAxisAttributeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> FilterSetIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("FilterSetId", "FilterSetId"));
        public Int64 FilterSetId
        {
            get { return ReadProperty(FilterSetIdProperty); }
            set
            {
                SetProperty(FilterSetIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> FilterAttributeIdProperty = RegisterProperty<Int64>(typeof(SelectionCriteria), new PropertyInfo<Int64>("FilterAttributeId", "FilterAttributeId"));
        public Int64 FilterAttributeId
        {
            get { return ReadProperty(FilterAttributeIdProperty); }
            set
            {
                SetProperty(FilterAttributeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> AttributeSetIdListProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("AttributeSetIdList", "AttributeSetIdList", string.Empty));
        public string AttributeSetIdList
        {
            get { return ReadProperty(AttributeSetIdListProperty); }
            set
            {
                SetProperty(AttributeSetIdListProperty, value);
            }
        }

        public static readonly PropertyInfo<int> MagSimulationIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("MagSimulationId", "MagSimulationId", 0));
        public int MagSimulationId
        {
            get { return ReadProperty(MagSimulationIdProperty); }
            set
            {
                SetProperty(MagSimulationIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ListTypeProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("ListType", "ListType", "StandardItemList"));
        public string ListType
        {
            get { return ReadProperty(ListTypeProperty); }
            set
            {
                SetProperty(ListTypeProperty, value);
            }
        }

        public static readonly PropertyInfo<int> PageNumberProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("PageNumber", "PageNumber", 0));
        public int PageNumber
        {
            get { return ReadProperty(PageNumberProperty); }
            set
            {
                SetProperty(PageNumberProperty, value);
            }
        }

        public static readonly PropertyInfo<int> PageSizeProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("PageSize", "PageSize", 1));
        public int PageSize
        {
            get { return ReadProperty(PageSizeProperty); }
            set
            {
                SetProperty(PageSizeProperty, value);
            }
        }

        public static readonly PropertyInfo<int> WorkAllocationIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("WorkAllocationId", "WorkAllocationId", 0));
        public int WorkAllocationId
        {
            get { return ReadProperty(WorkAllocationIdProperty); }
            set
            {
                SetProperty(WorkAllocationIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> ComparisonIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("ComparisonId", "ComparisonId", 0));
        public int ComparisonId
        {
            get { return ReadProperty(ComparisonIdProperty); }
            set
            {
                SetProperty(ComparisonIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> DescriptionProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("Description", "Description", string.Empty));
        public string Description
        {
            get { return ReadProperty(DescriptionProperty); }
            set
            {
                SetProperty(DescriptionProperty, value);
            }
        }

        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("ContactId", "ContactId", 0));
        public int ContactId
        {
            get { return ReadProperty(ContactIdProperty); }
            set
            {
                SetProperty(ContactIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> SetIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("SetId", "SetId", 0));
        public int SetId
        {
            get { return ReadProperty(SetIdProperty); }
            set
            {
                SetProperty(SetIdProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> ShowInfoColumnProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("ShowInfoColumn", "ShowInfoColumn", false));
        public bool ShowInfoColumn
        {
            get { return ReadProperty(ShowInfoColumnProperty); }
            set
            {
                SetProperty(ShowInfoColumnProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> ShowScoreColumnProperty = RegisterProperty<bool>(typeof(SelectionCriteria), new PropertyInfo<bool>("ShowScoreColumn", "ShowScoreColumn", false));
        public bool ShowScoreColumn
        {
            get { return ReadProperty(ShowScoreColumnProperty); }
            set
            {
                SetProperty(ShowScoreColumnProperty, value);
            }
        }
#if WEBDB //little trick to avoid making this field add to routine costs in ER4 and ER-Web
        public static readonly PropertyInfo<int> WebDbIdProperty = RegisterProperty<int>(typeof(SelectionCriteria), new PropertyInfo<int>("WebDbId", "WebDbId", 0));
        public int WebDbId
        {
            get { return ReadProperty(WebDbIdProperty); }
            set
            {
                SetProperty(WebDbIdProperty, value);
            }
        }
        public static readonly PropertyInfo<string> WithAttributesIdsListProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("WithAttributesIds", "WithAttributesIds", string.Empty));
        public string WithAttributesIds
        {
            get { return ReadProperty(WithAttributesIdsListProperty); }
            set
            {
                SetProperty(WithAttributesIdsListProperty, value);
            }
        }
        public static readonly PropertyInfo<string> WithSetIdsListProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("WithSetIdsList", "WithSetIdsList", string.Empty));
        public string WithSetIdsList
        {
            get { return ReadProperty(WithSetIdsListProperty); }
            set
            {
                SetProperty(WithSetIdsListProperty, value);
            }
        }
        public static readonly PropertyInfo<string> WithOutAttributesIdsListProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("WithOutAttributesIdsList", "WithOutAttributesIdsList", string.Empty));
        public string WithOutAttributesIdsList
        {
            get { return ReadProperty(WithOutAttributesIdsListProperty); }
            set
            {
                SetProperty(WithOutAttributesIdsListProperty, value);
            }
        }
        public static readonly PropertyInfo<string> WithOutSetIdsListProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("WithOutSetIdsList", "WithOutSetIdsList", string.Empty));
        public string WithOutSetIdsList
        {
            get { return ReadProperty(WithOutSetIdsListProperty); }
            set
            {
                SetProperty(WithOutSetIdsListProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SearchWhatProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("SearchWhat", "SearchWhat", string.Empty));
        public string SearchWhat
        {
            get { return ReadProperty(SearchWhatProperty); }
            set
            {
                SetProperty(SearchWhatProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SearchStringProperty = RegisterProperty<string>(typeof(SelectionCriteria), new PropertyInfo<string>("SearchString", "SearchString", string.Empty));
        public string SearchString
        {
            get { return ReadProperty(SearchStringProperty); }
            set
            {
                SetProperty(SearchStringProperty, value);
            }
        }
#endif
    }
}
