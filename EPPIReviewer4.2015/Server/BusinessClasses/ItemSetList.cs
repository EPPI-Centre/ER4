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
    public class ItemSetList : DynamicBindingListBase<ItemSet>
    {
        public static void GetItemSetList(Int64 itemId, EventHandler<DataPortalResult<ItemSetList>> handler)
        {
            DataPortal<ItemSetList> dp = new DataPortal<ItemSetList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ItemSetList, Int64>(itemId));
        }

        

#if SILVERLIGHT
        public ItemSetList() { }
        private List<ItemSet> _SetsVisibleToUser;
        public List<ItemSet> SetsVisibleToUser
        {
            get
            {
                if (_SetsVisibleToUser == null)
                {//we need to populate this!
                    _SetsVisibleToUser = new List<ItemSet>();
                    List<ItemSet> temp = new List<ItemSet>();
                    foreach (ItemSet el in this)
                    {
                        if (el.IsCompleted)
                            _SetsVisibleToUser.Add(el);
                        else
                        {
                            temp.Add(el);
                        }
                    }
                    bool IsFound = false;
                    BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
                    foreach (ItemSet el in temp)
                    {
                        if (ri.UserId == el.ContactId)
                        {//the uncompleted set belongs to the current user
                            IsFound = false;
                            foreach (ItemSet completed in _SetsVisibleToUser)
                            {
                                if (completed.SetId == el.SetId)
                                {
                                    IsFound = true;
                                    break; // the current list does not already include this set (e.g. there is no other completed version)
                                }
                            }
                            if (!IsFound)
                            {//add the uncompleted set
                                _SetsVisibleToUser.Add(el);
                            }
                        }
                        
                    }
                }
                return _SetsVisibleToUser;
            }
        }
#else
        private ItemSetList() { }
#endif

        internal static ItemSetList NewItemSetList()
        {
            return new ItemSetList();
        }

        internal ItemSet getItemSet(Int64 ItemSetID)
        {
            foreach (ItemSet IS in this)
            {
                if (IS.ItemSetId == ItemSetID) return IS;
            }
            return null;
        }
        private bool ContainsThisSet(int SetID)
        {
            foreach (ItemSet IS in this)
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
                ItemSet set = this.getItemSet(ftd.ItemSetId);
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
        protected void DataPortal_Fetch(SingleCriteria<ItemSetList, Int64> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                if (criteria.Value > 0)//getting the list of all item_sets (regardless of completion) without full text details, this is used in dialogCoding
                {
                    using (SqlCommand command = new SqlCommand("st_ItemSetDataListAll", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        //command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                Add(ItemSet.GetItemSet(reader));
                            }
                        }
                    }
                }
                else //getting the list with full text details but limited to completed codes (for fast'n dirty coding reports)
                {
                    using (SqlCommand command = new SqlCommand("st_ItemSetDataList", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                        //command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", -criteria.Value));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                Add(ItemSet.GetItemSet(reader));
                            }
                            reader.NextResult();//second reader gets all full text details in one go
                            AddFullTextData(ItemAttributesAllFullTextDetailsList.BuildList(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

#endif

    }
}
