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
using System.ComponentModel;


#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Xml.Linq;

#endif


namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ArchieReviewUndoCheckoutCommand : ArchieCommand<ArchieReviewUndoCheckoutCommand>
    {
        public ArchieReviewUndoCheckoutCommand() 
        {
            registerProperties();
        }
#if SILVERLIGHT
        public ArchieReviewUndoCheckoutCommand(string archieReviewID) 
        {
            registerProperties();
            LoadProperty(ArchieReviewIDProperty, archieReviewID);
        }

#endif
        public static readonly PropertyInfo<string> ArchieReviewIDProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieReviewID", "ArchieReviewID"));
        public string ArchieReviewID
        {
            get { return ReadProperty(ArchieReviewIDProperty); }
            set { LoadProperty(ArchieReviewIDProperty, value); }
        }
        public static readonly PropertyInfo<string> ResultProperty = RegisterProperty<string>(new PropertyInfo<string>("Result", "Result"));
        public string Result
        {
            get { return ReadProperty(ResultProperty); }
            set { LoadProperty(ResultProperty, value); }
        }
        public static readonly PropertyInfo<int> ReviewIDProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewID", "ReviewID"));
        public int ReviewID
        {
            get { return ReadProperty(ReviewIDProperty); }
            set { LoadProperty(ReviewIDProperty, value); }
        }
#if !SILVERLIGHT
        protected override void DataPortal_Execute()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (ArchieCode == null || ArchieCode.Length < 64)
            {
                Identity = ArchieIdentity.GetArchieIdentity(ri, true);
            }
            else
            {
                Identity = ArchieIdentity.GetArchieIdentity(ArchieCode, ArchieState);
            }
            if (!Identity.IsAuthenticated)
            {//send object back to re-authenticate in Archie
                Result = "Not Done: user not authenticated in Archie"; //see Identity for details
                return;
            }
            //find the review via st_ArchieReviewFindFromArchieID, if there is none
            //check out review, create review, add user
            //if review exists and user isn't in it, add user
            //return
            bool ContactInReview = false;
            bool IsCheckedOutHere = false;
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("myRole", "Author");
            pars.Add("published", "false");
            XDocument reviews = Identity.GetXMLQuery("rest/reviews", pars);
            ReadOnlyArchieReview roar = ReadOnlyArchieReview.GetReadOnlyReview(Identity);
            foreach (XElement el in reviews.Elements().Elements("review"))
            {
                roar = ReadOnlyArchieReview.GetReadOnlyReview(el, Identity);
                if (roar.ArchieReviewId == ArchieReviewID)
                {
                    break;
                }
                else
                {
                    roar = null;
                }
            }
            if (roar == null)
            {
                Result = "Not Done: User doesn't have access to this review in Archie";
                return;
            }

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ArchieReviewFindFromArchieID", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@A_ID", ArchieReviewID));
                    command.Parameters.Add(new SqlParameter("@CID", ri.UserId));

                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            ContactInReview = reader.GetInt32("CONTACT_IS_IN_REVIEW") == 1;
                            IsCheckedOutHere = reader.GetBoolean("IS_CHECKEDOUT_HERE") == true;
                            LoadProperty(ReviewIDProperty, reader.GetInt32("REVIEW_ID"));
                        }
                    }
                }
            }
            
            if (IsCheckedOutHere)
            {
                //check out review from Archie
                string ArchResult = Identity.UndoCheckOutReview(ArchieReviewID);
                if (ArchResult == "Done")
                {
                    Result = ArchResult;
                    //mark review as not checked out here
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("st_ArchieReviewMarkAsCheckedInOut", connection))
                        {//this also adds the user to the review (as reviewAdmin)
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@RID", ReviewID));
                            command.Parameters.Add(new SqlParameter("@CID", ri.UserId));
                            command.Parameters.Add(new SqlParameter("@ARID", roar.ArchieReviewId));
                            command.Parameters.Add(new SqlParameter("@ARCD", roar.ArchieReviewCD));
                            command.Parameters.Add(new SqlParameter("@IS_CHECKEDOUT_HERE", false));
                            command.Parameters.Add(new SqlParameter("@RES", System.Data.SqlDbType.Int));
                            command.Parameters["@RES"].Direction = System.Data.ParameterDirection.Output;
                            command.ExecuteNonQuery();
                            int localUndoCheckoutMark;
                            int.TryParse(command.Parameters["@RES"].Value.ToString(), out localUndoCheckoutMark);
                            if (localUndoCheckoutMark == -1)
                            {
                                Result = "Error: local record could not be updated. Review could not be found.";
                            }
                            else if (localUndoCheckoutMark == -2)
                            {
                                Result = "Error: local record could not be updated.";
                            }
                            else if (localUndoCheckoutMark == 1)
                            {
                                Result = "Done";
                            }
                            else 
                            {
                                Result = "Unspecified Error (updating local record), please contact EPPISupport@ioe.ac.uk";
                            }
                        }
                        connection.Close();
                    }
                    return;
                }
                else
                {// not sure should we do something different??
                    Result = ArchResult;
                    return;
                }

            }
            else
            {//to do
                Result = "Not Done: review is not checked out";
                return;
            }
        }


#endif
    }
}
