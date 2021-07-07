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
    public class ReadOnlyArchieReview : ArchieReadOnlyBase<ReadOnlyArchieReview>
    {
        public ReadOnlyArchieReview() { }

        
        public static readonly PropertyInfo<Security.ArchieIdentity> IdentityProperty = RegisterProperty<Security.ArchieIdentity>(new PropertyInfo<Security.ArchieIdentity>("Identity", "Identity"));
        public Security.ArchieIdentity Identity
        {
            get
            {
                return GetProperty(IdentityProperty);
            }
#if !SILVERLIGHT
            set
            {
                LoadProperty(IdentityProperty, value);
            }
#endif
        }

        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "Review Id", 0));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
        }
        public static readonly PropertyInfo<string> ArchieReviewIdProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieReviewId", "ArchieReviewId"));
        public string ArchieReviewId
        {
            get
            {
                return GetProperty(ArchieReviewIdProperty);
            }
        }
        public static readonly PropertyInfo<string> ArchieReviewCDProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieReviewCD", "ArchieReviewCD", ""));
        public string ArchieReviewCD
        {
            get
            {
                return GetProperty(ArchieReviewCDProperty);
            }
        }
        public static readonly PropertyInfo<string> StageProperty = RegisterProperty<string>(new PropertyInfo<string>("Stage", "Stage"));
        public string Stage
        {
            get
            {
                return GetProperty(StageProperty);
            }
        }
        public static readonly PropertyInfo<string> StatusProperty = RegisterProperty<string>(new PropertyInfo<string>("Status", "Status"));
        public string Status
        {
            get
            {
                return GetProperty(StatusProperty);
            }
        }
        public static readonly PropertyInfo<bool> checkedOutInArchieProperty = RegisterProperty<bool>(new PropertyInfo<bool>("checkedOutInArchie", "checkedOutInArchie"));
        public bool checkedOutInArchie
        {
            get
            {
                return GetProperty(checkedOutInArchieProperty);
            }
        }
        public static readonly PropertyInfo<bool> isCheckedOutHereProperty = RegisterProperty<bool>(new PropertyInfo<bool>("isCheckedOutHere", "isCheckedOutHere"));
        public bool isCheckedOutHere
        {
            get
            {
                return GetProperty(isCheckedOutHereProperty);
            }
        }
        public static readonly PropertyInfo<bool> isLocalProperty = RegisterProperty<bool>(new PropertyInfo<bool>("isLocal", "isLocal"));
        public bool isLocal
        {
            get
            {
                return GetProperty(isLocalProperty);
            }
        }
        public static readonly PropertyInfo<string> ReviewNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewName", "Review Name", string.Empty));
        public string ReviewName
        {
            get
            {
                return GetProperty(ReviewNameProperty);
            }
        }
        public static readonly PropertyInfo<string> ContactReviewRolesProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactReviewRoles", "ContactReviewRoles", string.Empty));
        public string ContactReviewRoles
        {
            get
            {
                return GetProperty(ContactReviewRolesProperty);
            }
        }
        public static readonly PropertyInfo<string> ReviewOwnerProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewOwner", "ReviewOwner", string.Empty));
        public string ReviewOwner
        {
            get
            {
                return GetProperty(ReviewOwnerProperty);
            }
        }
        public static readonly PropertyInfo<DateTime> LastAccessProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("LastAccess", "LastAccess"));
        public DateTime LastAccess
        {
            get
            {
                return GetProperty(LastAccessProperty);
            }
        }
        public static readonly PropertyInfo<bool> UserIsInReviewProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UserIsInReview", "UserIsInReview"));
        public bool UserIsInReview
        {
            get
            {
                return GetProperty<bool>(UserIsInReviewProperty) && GetProperty<bool>(UserIsInReviewProperty);
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyReview), canRead);
        //    //AuthorizationRules.AllowRead(ReviewNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //}

#if !SILVERLIGHT

        public static ReadOnlyArchieReview GetReadOnlyReview(XElement element, ArchieIdentity identity)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            ReadOnlyArchieReview res = new ReadOnlyArchieReview();
            res.LoadProperty(ReviewNameProperty, element.Element("title").Value);
            res.Identity = identity;
            res.LoadProperty(ArchieReviewCDProperty, element.Attribute("cdNumber").Value);
            res.LoadProperty(checkedOutInArchieProperty, element.Attribute("checkedOut").Value == "true");
            res.LoadProperty(ArchieReviewIdProperty, element.Attribute("reviewId").Value);
            res.LoadProperty(StageProperty, element.Attribute("stage").Value);
            res.LoadProperty(StatusProperty, element.Attribute("status").Value);
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ArchieReviewFindFromArchieID", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@A_ID", res.ArchieReviewId));
                    command.Parameters.Add(new SqlParameter("@CID", ri.UserId));

                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            int rid = reader.GetInt32("Review_ID");
                            string nm = reader.GetString("REVIEW_NAME");
                            res.LoadProperty<int>(ReviewIdProperty, rid);
                            res.LoadProperty<string>(ReviewNameProperty, nm);
                            res.LoadProperty<string>(ContactReviewRolesProperty, reader.GetString("ROLES"));
                            //res.LoadProperty<string>(ReviewOwnerProperty, reader.GetString("OWNER"));
                            res.LoadProperty<DateTime>(LastAccessProperty, reader.GetDateTime("LAST_ACCESS"));

                            res.LoadProperty(isCheckedOutHereProperty, reader.GetBoolean("IS_CHECKEDOUT_HERE"));
                            res.LoadProperty(isLocalProperty, true);
                            res.LoadProperty(UserIsInReviewProperty, reader.GetInt32("CONTACT_IS_IN_REVIEW") == 1);
                        }
                        else
                        {
                            res.LoadProperty(isCheckedOutHereProperty, false);
                            res.LoadProperty(isLocalProperty, false);
                        }
                    }
                }
            }
            return res;
        }
        public static ReadOnlyArchieReview GetReadOnlyReview(ArchieIdentity identity)
        {
            ReadOnlyArchieReview res = new ReadOnlyArchieReview();
            res.LoadProperty(ReviewNameProperty, "Place holder: no real reviews are in this list");
            res.Identity = identity;
            res.LoadProperty(ArchieReviewCDProperty, "CD0");
            res.LoadProperty(checkedOutInArchieProperty, false);
            res.LoadProperty(ArchieReviewIdProperty, "-1");
            res.LoadProperty(StageProperty, "V");//vacant
            res.LoadProperty(StatusProperty, "I");//inactive
            res.LoadProperty(isCheckedOutHereProperty, false);
            res.LoadProperty(isLocalProperty, false);
            return res;
        }
        


#endif
    }
}
