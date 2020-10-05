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
    public class WebDbReviewSetsList : DynamicBindingListBase<WebDbReviewSet>
    {

        public WebDbReviewSetsList() { }
        public WebDbReviewSet GetWebDbReviewSet(int SetId)
        {
            WebDbReviewSet returnValue;
            foreach (WebDbReviewSet reviewSet in this)
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
            foreach (WebDbReviewSet rs in this)
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
            foreach (WebDbReviewSet rs in this)
            {
                returnValue = rs.GetAttributeSetFromAttributeId(AttributeId);
                if (returnValue != null)
                {
                    return returnValue;
                }
            }
            return returnValue;
        }


        

        public int AttributeSetCount()
        {
            int retVal = 0;
            foreach (WebDbReviewSet rs in this)
            {
                retVal += rs.AttributesCount();
            }
            return retVal;
        }



#if !SILVERLIGHT
    

        protected void DataPortal_Fetch(SingleCriteria<WebDbReviewSetsList, int> criteria)//contains the WebDbId
        {
            RaiseListChangedEvents = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbGetCodesets", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            WebDbReviewSet reviewSet = WebDbReviewSet.GetWebDbReviewSet(reader);
                            WebDbReviewSet.ReviewSetFromDBCommonPart(reviewSet);
                            Add(reviewSet);
                        }
                    }
                }
                connection.Close();
                //we get the list of set types and plug them into each set in the review
                ReadOnlySetTypeList RoSTL = DataPortal.Fetch<ReadOnlySetTypeList>();
                foreach (WebDbReviewSet res in this)
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
