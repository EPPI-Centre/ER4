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
    public class ReadOnlyTemplateReviewList : ReadOnlyListBase<ReadOnlyTemplateReviewList, ReadOnlyTemplateReview>
    {
        public ReadOnlyTemplateReviewList() { }

        public static void GetReviewTemplates(EventHandler<DataPortalResult<ReadOnlyTemplateReviewList>> handler)
        {
            DataPortal<ReadOnlyTemplateReviewList> dp = new DataPortal<ReadOnlyTemplateReviewList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }
        public ReadOnlyTemplateReview GetTemplateReview(int Rid)
        {
            foreach (ReadOnlyTemplateReview ror in this)
            {
                if (ror.TemplateId == Rid )
                {
                    return ror;
                }
            }
            return null;
        }
#if !SILVERLIGHT

        private void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int currentRid = ri.ReviewId;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TemplateReviewList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {//the template root
                            Add(ReadOnlyTemplateReview.GetReadOnlyTemplateReview
                                (
                                    reader.GetInt32("TEMPLATE_REVIEW_ID")
                                    , reader.GetString("TEMPLATE_NAME")
                                    , reader.GetString("TEMPLATE_DESCRIPTION")
                                )
                               );
                        }
                        reader.NextResult();//get the sets
                        ReadOnlyTemplateReview rotr = this[0];
                        while (reader.Read())
                        {
                            if (reader.GetInt32("TEMPLATE_REVIEW_ID") != rotr.TemplateId)
                            {
                                rotr = GetTemplateReview(reader.GetInt32("TEMPLATE_REVIEW_ID"));
                            }
                            rotr.ReviewSetIds.Add(reader.GetInt32("REVIEW_SET_ID"));
                        }
                    }
                }
                connection.Close();
            }
            Add(ReadOnlyTemplateReview.GetReadOnlyTemplateReview
                                (
                                    1000
                                    , "Manually pick from Public codesets..."
                                    , "This option allows to pick and choose codesets manually from the list of public codesets."
                                )
                               );
            Add(ReadOnlyTemplateReview.GetReadOnlyTemplateReview
                                (
                                    2000
                                    , "Manually pick from your own codesets..."
                                    , "This option allows to pick and choose codesets manually from the list codesets present in the reviews where you have administrative rights."
                                )
                               );
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

#endif
    }
}
