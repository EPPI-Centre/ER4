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

#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyArchieReviewList : ArchieReadOnlyListBase<ReadOnlyArchieReviewList, ReadOnlyArchieReview>
    {
        public ReadOnlyArchieReviewList() { }
#if SILVERLIGHT   
        //in the !SILVERLIGHT area, we have an Identity property with backing private member, but we can't use Loadproperty(...) for the root of a readonlylist.
        //hence, we get the Identity from the child object, if any
        public Security.ArchieIdentity Identity
        {
            get
            {
                Security.ArchieIdentity res = new Security.ArchieIdentity();
                if (this.Count == 0)
                {
                    return res;
                }
                return (this[0] as ReadOnlyArchieReview).Identity;
            }
        }
        public bool IsAuthenticated
        {//this property is used to trigger ArchieAuthentication if needed.
            get 
            {
                if (this.Count == 0)
                {
                    return false;
                }
                return Identity.IsAuthenticated; 
            }
        }
#endif
        
        public static void GetReviewList(ReadOnlyArchieReviewListCriteria criteria, EventHandler<DataPortalResult<ReadOnlyArchieReviewList>> handler)
        {
            DataPortal<ReadOnlyArchieReviewList> dp = new DataPortal<ReadOnlyArchieReviewList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(criteria);
        }
        public static void GetReviewList(int contactId, EventHandler<DataPortalResult<ReadOnlyArchieReviewList>> handler)
        {
            DataPortal<ReadOnlyArchieReviewList> dp = new DataPortal<ReadOnlyArchieReviewList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ReadOnlyArchieReviewList, int>(contactId));
        }
        public static void GetReviewList(EventHandler<DataPortalResult<ReadOnlyArchieReviewList>> handler)
        {
            DataPortal<ReadOnlyArchieReviewList> dp = new DataPortal<ReadOnlyArchieReviewList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }
        public ReadOnlyArchieReview GetReview(int Rid)
        {
            foreach (ReadOnlyArchieReview ror in this)
            {
                if (ror.ReviewId == Rid || ror.ReviewId == -Rid)
                {
                    return ror;
                }
            }
            return null;
        }
#if !SILVERLIGHT
        

        private void DataPortal_Fetch()
        {//default option (no special query): get the reviews with "myRole=Author" and "published=false"
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int currentRid = ri.ReviewId;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            _archieIdentity = ArchieIdentity.GetArchieIdentity(ri, true);
            //this is probably the only case: we have an ri, but no review/ticket, so we skip that check 
            if (_archieIdentity.IsAuthenticated)
            {
                List<KeyValuePair<string, string>> pars =  new List<KeyValuePair<string, string>>();
                pars.Add(new KeyValuePair<string, string>("q", "status:ACTIVE"));
                pars.Add(new KeyValuePair<string, string>("q", "cochraneReview:true"));
                pars.Add(new KeyValuePair<string, string>("myReviews", "true"));
                Newtonsoft.Json.Linq.JArray reviews = (Newtonsoft.Json.Linq.JArray)_archieIdentity.GetJson("reviews", pars);
                
                if (reviews == null || reviews.Count == 0) 
                {
                    RaiseListChangedEvents = true;
                    IsReadOnly = true;
                    return;
                }
                foreach(JToken Jreview in reviews)
                {
                    Add(ReadOnlyArchieReview.GetReadOnlyReview(Jreview, _archieIdentity));
                }

                RaiseListChangedEvents = true;
                IsReadOnly = true;
                //one more thing: if the user is authenticated, but no reviews are coming from archie, we need to place an empty review 
                //this will allow the list to report that user does not need to autheticate in Archie.
                if (this.Count == 0)
                {
                    Add(ReadOnlyArchieReview.GetReadOnlyReview(_archieIdentity));
                }
            }

        }
        private void DataPortal_Fetch(ReadOnlyArchieReviewListCriteria criteria)
        {//this is used when both the Archie tokens for this user are expired, user re-authenticates on client, sends code and status back
            //we are using default query to get the archie reviews, might need to extend the criteria to allow for more options
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            _archieIdentity = ArchieIdentity.GetArchieIdentity(criteria.ArchieCode, criteria.ArchieState);
            if (_archieIdentity.IsAuthenticated)
            {
                Dictionary<string, string> pars = new Dictionary<string, string>();
                pars.Add("myRole", "Author");
                pars.Add("published", "false");
                XDocument reviews = _archieIdentity.GetXMLQuery("rest/reviews", pars);
                foreach (XElement el in reviews.Elements().Elements("review"))
                {
                    Add(ReadOnlyArchieReview.GetReadOnlyReview(el, _archieIdentity));
                }
                //one more thing: if the user is authenticated, but no reviews are coming from archie, we need to place an empty review 
                //this will allow the list to report that user does not need to autheticate in Archie.
                if (this.Count == 0)
                {
                    Add(ReadOnlyArchieReview.GetReadOnlyReview(_archieIdentity));
                }
            }
        }

#endif
    }

    [Serializable]
    public class ReadOnlyArchieReviewListCriteria : Csla.CriteriaBase<ReadOnlyArchieReviewListCriteria>
    {
        public static readonly PropertyInfo<string> ArchieCodeProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieCode", "ArchieCode"));
        public string ArchieCode
        {
            get { return ReadProperty(ArchieCodeProperty); }
        }
        public static readonly PropertyInfo<string> ArchieStateProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieState", "ArchieState"));
        public string ArchieState
        {
            get { return ReadProperty(ArchieStateProperty); }
        }
        public ReadOnlyArchieReviewListCriteria(string Code, string state)
        //: base(type)
        {
            LoadProperty(ArchieCodeProperty, Code);
            LoadProperty(ArchieStateProperty, state);
        }

        public ReadOnlyArchieReviewListCriteria() { }
    }
}
