using System;
using Csla;
using Csla.Core;


#if (!SILVERLIGHT)
using Csla.Data;
using BusinessLibrary.Security;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using System.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class QuickCodingReportData : ReadOnlyBase<QuickCodingReportData>
    {

#if SILVERLIGHT
    public ReadOnlyReview() { }
#else
        public QuickCodingReportData()
        {
            LoadProperty<ItemList>(ItemsProperty, new ItemList());
            LoadProperty<ItemSetList>(ItemSetsProperty, new ItemSetList());
        }
#endif
        public static readonly PropertyInfo<ItemList> ItemsProperty = RegisterProperty(new PropertyInfo<ItemList>("Items", "Items"));
        public ItemList Items
        {
            get
            {
                return GetProperty(ItemsProperty);
            }
        }
        public static readonly PropertyInfo<ItemSetList> ItemSetsProperty = RegisterProperty(new PropertyInfo<ItemSetList>("ItemSets", "ItemSets", new ItemSetList()));
        public ItemSetList ItemSets
        {
            get
            {
                return GetProperty(ItemSetsProperty);
            }
        }
        public int pageSize
        {
            get { return Items.PageSize; }
        }
        public int pageCount
        {
            get { return Items.PageCount; }
        }
        public int pageIndex
        {
            get { return Items.PageIndex; }
        }
        public int totalItemCount
        {
            get { return Items.TotalItemCount; }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyReview), canRead);
        //    //AuthorizationRules.AllowRead(ReviewNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //}

#if !SILVERLIGHT

        private DataTable InputTable = new DataTable();

        protected void DataPortal_Fetch(QuickCodingReportDataSelectionCriteria criteria)
        {
            LoadProperty(ItemsProperty, ItemList.GetItemList(criteria.ItemsSelectionCriteria));
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            PerpareInputTable();
            if (InputTable.Rows.Count == 0) return;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                
                using (SqlCommand command = new SqlCommand("st_QuickCodingReportCodingData", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@revID", ri.ReviewId));
                    SqlParameter tvpParam = command.Parameters.AddWithValue("@input", InputTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.ITEMS_INPUT_TB";
                    command.Parameters.Add(new SqlParameter("@SetIds", criteria.SetIds));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            ItemSets.Add(ItemSet.GetItemSet(reader));//this gets also itemAttributes, Arms coding and Outcomes...

                        }
                        reader.NextResult();//second reader gets all full text details in one go
                        ItemSets.AddFullTextData(ItemAttributesAllFullTextDetailsList.BuildList(reader));
                    }
                }
                
            }
        }
        private void PerpareInputTable()
        {
            InputTable.Columns.Add(new DataColumn("ItemId", Int64.MaxValue.GetType()));
            foreach (Item itm in this.Items)
            {
                DataRow dr = InputTable.NewRow();
                dr["ItemId"] = itm.ItemId;
                InputTable.Rows.Add(dr);
            }
        }
        private void Coding_Fetch(SafeDataReader reader)
        {

        }

#endif
    }
    // used to define the parameters for the query.
    [Serializable]
    public class QuickCodingReportDataSelectionCriteria : BusinessBase
    {
        public QuickCodingReportDataSelectionCriteria() { }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_ItemsSelectionCriteria", _ItemsSelectionCriteria);
            info.AddValue("_SetIds", _SetIds);

        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _ItemsSelectionCriteria = info.GetValue<SelectionCriteria>("_ItemsSelectionCriteria");
            _SetIds = info.GetValue<string>("_SetIds");
        }

        //public static readonly PropertyInfo<SelectionCriteria> ItemsSelectionCriteriaProperty = RegisterProperty<SelectionCriteria>(typeof(SelectionCriteria), new PropertyInfo<SelectionCriteria>("ItemsSelectionCriteria", "ItemsSelectionCriteria"));
        private SelectionCriteria _ItemsSelectionCriteria;
        public SelectionCriteria ItemsSelectionCriteria
        {
            get { return _ItemsSelectionCriteria; }
            set
            {
                _ItemsSelectionCriteria = value;
            }
        }
        private string _SetIds;
        public string SetIds
        {
            get { return _SetIds; }
            set
            {
                _SetIds = value;
            }
        }
    }
}
