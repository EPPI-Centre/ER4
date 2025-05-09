﻿using System;
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
    public class Search : BusinessBase<Search>
    {
#if SILVERLIGHT
    public Search() { }

        
#else
        public Search() { }
#endif

        public override string ToString()
        {
            return Title;
        }

        public static readonly PropertyInfo<int> SearchIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SearchId", "SearchId"));
        public int SearchId
        {
            get
            {
                return GetProperty(SearchIdProperty);
            }
        }

		public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId"));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
            set
            {
                SetProperty(ReviewIdProperty, value);
            }
        }

		public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
        }

		public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName"));
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
            }
        }

		public static readonly PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
            set
            {
                SetProperty(TitleProperty, value);
            }
        }

		public static readonly PropertyInfo<int> SearchNoProperty = RegisterProperty<int>(new PropertyInfo<int>("SearchNo", "SearchNo"));
        public int SearchNo
        {
            get
            {
                return GetProperty(SearchNoProperty);
            }
            set
            {
                SetProperty(SearchNoProperty, value);
            }
        }

		public static readonly PropertyInfo<string> AnswersProperty = RegisterProperty<string>(new PropertyInfo<string>("Answers", "Answers", string.Empty));
        public string Answers
        {
            get
            {
                return GetProperty(AnswersProperty);
            }
            set
            {
                SetProperty(AnswersProperty, value);
            }
        }

		public static readonly PropertyInfo<int> HitsNoProperty = RegisterProperty<int>(new PropertyInfo<int>("HitsNo", "HitsNo"));
        public int HitsNo
        {
            get
            {
                return GetProperty(HitsNoProperty);
            }
        }

		public static readonly PropertyInfo<bool> IsClassifierResultProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsClassifierResult", "IsClassifierResult", false));
        public bool IsClassifierResult
        {
            get
            {
                return GetProperty(IsClassifierResultProperty);
            }
        }

		public static readonly PropertyInfo<SmartDate> SearchDateProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("SearchDate", "SearchDate"));
        public SmartDate SearchDate
        {
            get
            {
                return GetProperty(SearchDateProperty);
            }
        }

        public static readonly PropertyInfo<Boolean> ResultProperty = RegisterProperty<Boolean>(new PropertyInfo<Boolean>("Result", "Result"));
        public Boolean Result
        {
            get
            {
                return GetProperty(ResultProperty);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Search), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Search), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Search), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Search), canRead);

        //    //AuthorizationRules.AllowRead(SearchIdProperty, canRead);
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
                using (SqlCommand command = new SqlCommand("st_SearchInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@Search_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@Search_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_Search_ID", 0));
                    command.Parameters["@NEW_Search_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(SearchIdProperty, command.Parameters["@NEW_Search_ID"].Value);
                }
                connection.Close();
            }
             */
        }


        protected void DataPortal_Fetch(SingleCriteria<Search, int> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SearchGet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SEARCH_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<int>(SearchIdProperty, reader.GetInt32("Search_ID"));
                            LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
                            LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
                            LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
                            LoadProperty<string>(TitleProperty, reader.GetString("SEARCH_TITLE"));
                            LoadProperty<int>(SearchNoProperty, reader.GetInt32("SEARCH_NO"));
                            LoadProperty<string>(AnswersProperty, reader.GetString("ANSWERS"));
                            LoadProperty<int>(HitsNoProperty, reader.GetInt32("HITS_NO"));
                            LoadProperty<bool>(IsClassifierResultProperty, reader.GetBoolean("IS_CLASSIFIER_RESULT"));
                            LoadProperty<SmartDate>(SearchDateProperty, reader.GetSmartDate("SEARCH_DATE"));
                        }
                    }
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				connection.Open();
                using (SqlCommand command = new SqlCommand("st_SearchUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SEARCH_ID", ReadProperty(SearchIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TITLE", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				connection.Open();
                using (SqlCommand command = new SqlCommand("st_SearchDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Search_ID", ReadProperty(SearchIdProperty)));
					command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
					command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static Search GetSearch(SafeDataReader reader)
        {
            Search returnValue = new Search();
            returnValue.LoadProperty<int>(SearchIdProperty, reader.GetInt32("Search_ID"));
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("SEARCH_TITLE"));
            returnValue.LoadProperty<int>(SearchNoProperty, reader.GetInt32("SEARCH_NO"));
            returnValue.LoadProperty<string>(AnswersProperty, reader.GetString("ANSWERS"));
            returnValue.LoadProperty<int>(HitsNoProperty, reader.GetInt32("HITS_NO"));
            returnValue.LoadProperty<bool>(IsClassifierResultProperty, reader.GetBoolean("IS_CLASSIFIER_RESULT"));
            returnValue.LoadProperty<SmartDate>(SearchDateProperty, reader.GetSmartDate("SEARCH_DATE"));

            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
