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

#if(!SILVERLIGHT && !CSLA_NETCORE)
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#elif(CSLA_NETCORE)
using Csla.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyReview : ReadOnlyBase<ReadOnlyReview>
    {

#if SILVERLIGHT
    public ReadOnlyReview() { }
#else
        public ReadOnlyReview() { }
#endif
        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "Review Id", 0));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
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
        public static readonly PropertyInfo<int> NAutoUpdatesProperty = RegisterProperty<int>(new PropertyInfo<int>("NAutoUpdates", "Review Id", 0));
        public int NAutoUpdates
        {
            get
            {
                return GetProperty(NAutoUpdatesProperty);
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

        public static ReadOnlyReview GetReadOnlyReview(SafeDataReader reader)
        {
            return DataPortal.FetchChild<ReadOnlyReview>(reader);
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            int rid = reader.GetInt32("Review_ID");
            string nm = reader.GetString("REVIEW_NAME");
            LoadProperty<int>(ReviewIdProperty, rid);
            LoadProperty<string>(ReviewNameProperty, nm);
            LoadProperty<string>(ContactReviewRolesProperty, reader.GetString("ROLES"));
            LoadProperty<string>(ReviewOwnerProperty, reader.GetString("OWNER"));
            LoadProperty<DateTime>(LastAccessProperty, reader.GetDateTime("LAST_ACCESS"));
            LoadProperty<int>(NAutoUpdatesProperty, reader.GetInt32("NAutoUpdates"));
        }


#endif
    }

}
