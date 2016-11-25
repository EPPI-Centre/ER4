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
using Csla.DataPortalClient;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class SearchVisualise : BusinessBase<SearchVisualise>
    {
#if SILVERLIGHT
    public SearchVisualise() { }

        
#else
        private SearchVisualise() { }
#endif

       
        private static PropertyInfo<int> CountProperty = RegisterProperty<int>(new PropertyInfo<int>("Count", "Count"));
        public int Count
        {
            get
            {
                return GetProperty(CountProperty);
            }
        }
       
        private static PropertyInfo<string> RangeProperty = RegisterProperty<string>(new PropertyInfo<string>("Range", "Range"));
        public string Range
        {
            get
            {
                return GetProperty(RangeProperty);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(SearchVisualise), admin);
        //    //AuthorizationRules.AllowDelete(typeof(SearchVisualise), admin);
        //    //AuthorizationRules.AllowEdit(typeof(SearchVisualise), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(SearchVisualise), canRead);

        //    //AuthorizationRules.AllowRead(SearchVisualiseIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);

        //    ////AuthorizationRules.AllowWrite(NameProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            /*
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            LoadProperty(ReviewIdProperty, ri.ReviewId);
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SearchVisualiseInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SearchVisualise_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@SearchVisualise_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_SearchVisualise_ID", 0));
                    command.Parameters["@NEW_SearchVisualise_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(SearchVisualiseIdProperty, command.Parameters["@NEW_SearchVisualise_ID"].Value);
                }
                connection.Close();
            }
             */
        }

        protected override void DataPortal_Update()
        {
            
        }

        protected override void DataPortal_DeleteSelf()
        {
            
        }

        internal static SearchVisualise GetSearchVisualise(SafeDataReader reader)
        {
            SearchVisualise returnValue = new SearchVisualise();
            returnValue.LoadProperty<int>(CountProperty, reader.GetInt32("NUM"));
            returnValue.LoadProperty<string>(RangeProperty, reader.GetString("TITLE"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
