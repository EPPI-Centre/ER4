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
    public class ItemAttributesAllFullTextDetailsList : DynamicBindingListBase<ItemAttributeFullTextDetails>
    {

    public ItemAttributesAllFullTextDetailsList() { }

    //internal ItemAttributeFullTextDetails getItemSet(Int64 ItemSetID)
    //{
    //    foreach (ItemSet IS in this)
    //    {
    //        if (IS.ItemSetId == ItemSetID) return IS;
    //    }
    //    return null;
    //}
#if !SILVERLIGHT
        internal static ItemAttributesAllFullTextDetailsList BuildList(Csla.Data.SafeDataReader reader)
        {
            ItemAttributesAllFullTextDetailsList result = new ItemAttributesAllFullTextDetailsList();
            while (reader.Read())
            {//build list 
                result.Add(ItemAttributeFullTextDetails.GetItemAttributeFullTextDetails(reader));
            }
            return result;
        }
        

        private void DataPortal_Fetch(SingleCriteria<ItemAttributesAllFullTextDetailsList, Int64> criteria)
        {//get all details based on item_id Only
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemAttributesAllFullTextDetailsList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    if (criteria.Value < 0)
                    {//if item id < 0 this is because we want to get only the FullTextCoding visible to the user.
                        command.CommandText = "st_ItemAttributesContactFullTextDetailsList";
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", -criteria.Value));
                    }
                    else command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    //command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ItemAttributeFullTextDetails.GetItemAttributeFullTextDetails(reader));
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
