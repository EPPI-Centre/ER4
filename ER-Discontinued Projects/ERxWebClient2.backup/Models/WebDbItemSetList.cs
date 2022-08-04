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

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    //public class ItemSetList : BusinessListBase<ItemSetList, ItemSet>
    public class WebDbItemSetList : ReadOnlyBindingListBase<WebDbItemSetList, WebDbItemSet>
    {

        public WebDbItemSetList() { }

        internal static WebDbItemSetList NewItemSetList()
        {
            return new WebDbItemSetList();
        }

        internal WebDbItemSet getItemSet(Int64 ItemSetID)
        {
            foreach (WebDbItemSet IS in this)
            {
                if (IS.ItemSetId == ItemSetID) return IS;
            }
            return null;
        }
        private bool ContainsThisSet(int SetID)
        {
            foreach (WebDbItemSet IS in this)
            {
                if (IS.SetId == SetID) return true;
            }
            return false;
        }

        public void AddFullTextData(ItemAttributesAllFullTextDetailsList FTDL)
        {
            //adds a list of full text details into the right itemSet and ItemAttribute if possible;
            foreach (ItemAttributeFullTextDetails ftd in FTDL)
            {
                WebDbItemSet set = this.getItemSet(ftd.ItemSetId);
                if (set == null) continue;
                ReadOnlyItemAttribute roia = set.GetItemAttributeFromIAID(ftd.ItemAttributeId);
                if (roia == null) continue;
                ItemAttributeFullTextDetails oldElement = roia.ItemAttributeFullTextList.GetItemAttributeFullTextDetails(ftd.IsFromPDF, ftd.ItemAttributeTextId);
                if (oldElement != null)
                {// to make sure we don't add the same "line" if it's already there
                    roia.ItemAttributeFullTextList.Remove(oldElement);
                }
                roia.ItemAttributeFullTextList.Add(ftd);
            }

        }
#if SILVERLIGHT
    
#else
        protected void DataPortal_Fetch(WebDbItemSetListSelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();

                //getting the list with full text details but limited to completed codes (for fast'n dirty coding reports)
                using (SqlCommand command = new SqlCommand("st_WebDbItemSetDataList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.ItemId));
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WebDbId", criteria.WebDbId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(WebDbItemSet.GetItemSet(reader, criteria.WebDbId));
                        }
                        reader.NextResult();//second reader gets all full text details in one go
                        AddFullTextData(ItemAttributesAllFullTextDetailsList.BuildList(reader));
                    }
                }

                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

#endif

    }

    [Serializable]
    public class WebDbItemSetListSelectionCriteria : Csla.CriteriaBase<WebDbItemSetListSelectionCriteria>
    {
        private static PropertyInfo<int> WebDbIdProperty = RegisterProperty<int>( new PropertyInfo<int>("WebDbId", "WebDbId"));
        public int WebDbId
        {
            get { return ReadProperty(WebDbIdProperty); }
        }
        //private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(typeof(ItemSetSelectionCriteria), new PropertyInfo<int>("SetId", "SetId"));
        //public int SetId
        //{
        //    get { return ReadProperty(SetIdProperty); }
        //}
        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>( new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get { return ReadProperty(ItemIdProperty); }
        }

        public WebDbItemSetListSelectionCriteria(int WebDbId, 
            //int SetID, 
            Int64 ItemID)
        //: base(type)
        {
            LoadProperty(WebDbIdProperty, WebDbId);
            //LoadProperty(SetIdProperty, SetID);
            LoadProperty(ItemIdProperty, ItemID);
        }

        public WebDbItemSetListSelectionCriteria() { }

    }
}
