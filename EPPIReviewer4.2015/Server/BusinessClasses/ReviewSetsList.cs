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
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReviewSetsList : DynamicBindingListBase<ReviewSet>
    {

        public static void GetReviewSetsList(EventHandler<DataPortalResult<ReviewSetsList>> handler)
        {
            DataPortal<ReviewSetsList> dp = new DataPortal<ReviewSetsList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

        public static void GetReviewSetsList4Copy(bool GetPrivateSets, EventHandler<DataPortalResult<ReviewSetsList>> handler)
        {//used to get the list of sets that a given user can copy into the current review
            DataPortal<ReviewSetsList> dp = new DataPortal<ReviewSetsList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ReviewSetsList, bool>(GetPrivateSets)); 
        }

        public ReviewSet GetReviewSet(int SetId)
        {
            ReviewSet returnValue;
            foreach (ReviewSet reviewSet in this)
            {
                returnValue = reviewSet;
                if (returnValue.SetId == SetId)
                    return returnValue;
            }
            return null;
        }

        public AttributeSet GetAttributeSet(Int64 AttributeSetId)
        {
            AttributeSet returnValue = null;
            foreach (ReviewSet rs in this)
            {
                returnValue = rs.GetAttributeSet(AttributeSetId);
                if (returnValue != null)
                {
                    return returnValue;
                }
            }
            return returnValue;
        }

        public AttributeSet GetAttributeSetFromAttributeId(Int64 AttributeId)
        {
            AttributeSet returnValue = null;
            foreach (ReviewSet rs in this)
            {
                returnValue = rs.GetAttributeSetFromAttributeId(AttributeId);
                if (returnValue != null)
                {
                    return returnValue;
                }
            }
            return returnValue;
        }

        // JT commented out 07/06/2018 - doesn't look like this is ever called??
        //public void SetItemData(ItemSetList data)
        //{
        //    //List<ItemSet> list = 
        //       object o1 =  data.Cast<List<ItemSet>>();
        //    object o2 = data.Cast<ItemSet>();//as List<ItemSet>;
            
        //}
        //

        public void SetItemData(List<ItemSet> data, Int64 CurrentItemArmId)
        {
            ClearItemData();
            foreach (ItemSet itemSet in data)
            {
                ReviewSet rs = this.GetReviewSet(itemSet.SetId);
                if (rs != null)
                {
                    // JT deleted all the code that was here and moved to SetItemSetData below
                    SetItemSetData(itemSet, CurrentItemArmId);
                }
            }
        }

        public void ClearItemData()
        {
            foreach (ReviewSet rs in this)
            {
                rs.ClearItemData();
            }
        }
        public void SetItemSetData(ItemSet itemSet, Int64 CurrentItemArmId)
        {
            ReviewSet rs = this.GetReviewSet(itemSet.SetId);
            if (rs != null)
            {
                rs.ItemSetContactId = itemSet.ContactId;
                rs.ItemSetContactName = itemSet.ContactName;
                rs.ItemSetId = itemSet.ItemSetId;
                rs.ItemSetIsCompleted = itemSet.IsCompleted;
                rs.ItemSetIsLocked = itemSet.IsLocked;
                rs.ItemSetSetId = itemSet.SetId;
                if (itemSet.ItemAttributes != null)
                {
                    foreach (ReadOnlyItemAttribute itemAttribute in itemSet.ItemAttributes)
                    {
                        if (itemAttribute.ArmId == CurrentItemArmId)
                        {
                            AttributeSet attributeSet = rs.GetAttributeSet(itemAttribute.AttributeSetId);
                            if (attributeSet != null)
                            {
                                ItemAttributeData itemData = new ItemAttributeData();
                                itemData.ItemAttributeTextList = itemAttribute.ItemAttributeTextList;
                                itemData.ItemAttributeId = itemAttribute.ItemAttributeId;
                                itemData.AttributeId = itemAttribute.AttributeId;
                                itemData.AttributeSetId = itemAttribute.AttributeSetId;
                                itemData.ItemId = itemAttribute.ItemId;
                                itemData.ItemSetId = itemAttribute.ItemSetId;
                                itemData.SetId = attributeSet.SetId;
                                itemData.ArmId = itemAttribute.ArmId;
                                itemData.AdditionalText = itemAttribute.AdditionalText;
                                itemData.ItemAttributeTextList = itemAttribute.ItemAttributeTextList;
                                //itemData.ItemContactId = itemAttribute.ContactId;
                                //itemData.IsSelected = true; Default is true.
                                attributeSet.IsSelected = true;
                                attributeSet.ItemData = itemData;
                            }
                        }
                    }
                }


                // JT rationalising - this function does exactly the same as the one above! Just with an individual reviewset. So we should
                // use the same code! Delete below once it's working

                //rs.ClearItemData();
                //rs.ItemSetContactId = itemSet.ContactId;
                //rs.ItemSetContactName = itemSet.ContactName;
                //rs.ItemSetId = itemSet.ItemSetId;
                //rs.ItemSetIsCompleted = itemSet.IsCompleted;
                //rs.ItemSetIsLocked = itemSet.IsLocked;
                //rs.ItemSetSetId = itemSet.SetId;
                //if (itemSet.ItemAttributes != null)
                //{
                //    foreach (ReadOnlyItemAttribute itemAttribute in itemSet.ItemAttributes)
                //    {
                //        AttributeSet attributeSet = rs.GetAttributeSet(itemAttribute.AttributeSetId);
                //        if (attributeSet != null)
                //        {
                //            ItemAttributeData itemData = new ItemAttributeData();
                //            itemData.ItemAttributeTextList = itemAttribute.ItemAttributeTextList;
                //            itemData.ItemAttributeId = itemAttribute.ItemAttributeId;
                //            itemData.ItemId = itemAttribute.ItemId;
                //            itemData.ItemSetId = itemAttribute.ItemSetId;
                //            itemData.SetId = attributeSet.SetId;
                //            itemData.AdditionalText = itemAttribute.AdditionalText;
                //            itemData.ItemAttributeTextList = itemAttribute.ItemAttributeTextList;
                //            //itemData.ItemContactId = itemAttribute.ContactId;
                //            //itemData.IsSelected = true; Default is true.
                //            attributeSet.IsSelected = true;
                //            attributeSet.ItemData = itemData;
                //        }
                //    }
                //}
            }
        }
        
        public string RemoveReviewSet(ReviewSet rs)
        {
            if (this.IndexOf(rs) == -1)
            {
                return "Error: code set not in list. Please go to the 'My info' tab and reload your review.";
            }
            for (int i = this.IndexOf(rs) + 1; i < this.Count; i++)
            {
                this[i].SetIsNew();
                this[i].SetOrder = this[i].SetOrder - 1;
                this[i].SetIsOld();
            }
            rs.SetIsNew(); // marking the code set as 'new' means that its removal doesn't get sent to the dataportal
            Remove(rs);
            rs.SetIsOld();
            if (this.IndexOf(rs) == -1)
            {
                return "";
            }
            else
            {
                return "Error: code set could not be removed from list. Please go to the 'My info' tab and reload your review.";
            }
        }

        public string MoveReviewSet(ReviewSet rs, int position)
        {
            if (this.IndexOf(rs) == -1)
            {
                return "Error: code set not in list. Please go to the 'My info' tab and reload your review.";
            }
            
            //AttributeSet hostAttribute = attribute.HostAttribute; // as remove (next line) deletes this value
            RemoveReviewSet(rs);
            for (int i = position; i < this.Count; i++)
            {
                this[i].SetIsNew();
                this[i].SetOrder = this[i].SetOrder + 1;
                this[i].SetIsOld();
            }
            rs.SetIsNew();
            this.Insert(position, rs);
            rs.SetOrder = position;
            rs.SetIsOld();
            return "";
        }

        private bool _loadingAttributes;
        public bool LoadingAttributes
        {
            get
            {
                return _loadingAttributes;
            }
            set
            {
                _loadingAttributes = value;
            }
        }

        public int AttributeSetCount()
        {
            int retVal = 0;
            foreach (ReviewSet rs in this)
            {
                retVal += rs.AttributesCount();
            }
            return retVal;
        }



        public ReviewSetsList() { }
#if SILVERLIGHT
    protected override void AddNewCore()
    {
        Add(ReviewSet.NewReviewSet());
    }
#endif

#if !SILVERLIGHT
    

        protected void DataPortal_Fetch()
        {
            RaiseListChangedEvents = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSets", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // more secure using server stored object
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            ReviewSet reviewSet = ReviewSet.GetReviewSet(reader);
                            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
                            {
                                connection2.Open();
                                using (SqlCommand command2 = new SqlCommand("st_AttributeSet", connection2))
                                {
                                    
                                    command2.CommandType = System.Data.CommandType.StoredProcedure;
                                    command2.Parameters.Add(new SqlParameter("@SET_ID", reviewSet.SetId));
                                    command2.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", 0));
                                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command2.ExecuteReader()))
                                    {
                                        while (reader2.Read())
                                        {
                                             
                                            AttributeSet newAttributeSet = AttributeSet.GetAttributeSet(reader2, reviewSet.TempMaxDepth);
                                            reviewSet.Attributes.Add(newAttributeSet);
                                        }
                                        reader2.Close();
                                    }
                                }
                                connection2.Close();
                            }
                            Add(reviewSet);
                        }
                    }
                }
                connection.Close();
                //we get the list of set types and plug them into each set in the review
                ReadOnlySetTypeList RoSTL = DataPortal.Fetch<ReadOnlySetTypeList>();
                foreach (ReviewSet res in this)
                {
                    foreach (ReadOnlySetType rost in RoSTL)
                    {
                        if (res.SetTypeId == rost.SetTypeId)
                        {
                            res.SetType = rost;
                            break;
                        }
                    }
                }
            }
            RaiseListChangedEvents = true;
        }
        private void DataPortal_Fetch(SingleCriteria<ReviewSetsList, bool> criteria)
        {//criteria is true if getting only private reviews
            RaiseListChangedEvents = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetsForCopy", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@PRIVATE_SETS", criteria.Value ? 1 : 0));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            ReviewSet reviewSet = ReviewSet.GetReviewSet(reader);
                            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
                            {
                                connection2.Open();
                                using (SqlCommand command2 = new SqlCommand("st_AttributeSet", connection2))
                                {

                                    command2.CommandType = System.Data.CommandType.StoredProcedure;
                                    command2.Parameters.Add(new SqlParameter("@SET_ID", reviewSet.SetId));
                                    command2.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", 0));
                                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command2.ExecuteReader()))
                                    {
                                        while (reader2.Read())
                                        {

                                            AttributeSet newAttributeSet = AttributeSet.GetAttributeSet(reader2, reviewSet.TempMaxDepth);
                                            reviewSet.Attributes.Add(newAttributeSet);
                                        }
                                        reader2.Close();
                                    }
                                }
                                connection2.Close();
                            }
                            Add(reviewSet);
                        }
                    }
                }
                connection.Close();
                //we get the list of set types and plug them into each set in the review
                ReadOnlySetTypeList RoSTL = DataPortal.Fetch<ReadOnlySetTypeList>();
                foreach (ReviewSet res in this)
                {
                    foreach (ReadOnlySetType rost in RoSTL)
                    {
                        if (res.SetTypeId == rost.SetTypeId)
                        {
                            res.SetType = rost;
                            break;
                        }
                    }
                }
            }
            RaiseListChangedEvents = true;
        }
#endif

    }
}
