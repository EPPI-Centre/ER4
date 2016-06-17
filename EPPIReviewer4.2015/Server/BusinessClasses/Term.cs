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
    public class Term : BusinessBase<Term>
    {
#if SILVERLIGHT
    public Term() { }

        
#else
        private Term() { }
#endif

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            
            Term tOb = obj as Term;
            if (tOb == null) return false;
            else if (this.TermValue == tOb.TermValue && this.Name == tOb.Name) return true;
            else return false;
        }
        /*
        private static PropertyInfo<int> TermIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TermId", "TermId"));
        public int TermId
        {
            get
            {
                return GetProperty(TermIdProperty);
            }
        }

         */

        private static PropertyInfo<double> TermValueProperty = RegisterProperty<double>(new PropertyInfo<double>("TermValue", "TermValue"));
        public double TermValue
        {
            get
            {
                return GetProperty(TermValueProperty);
            }
            set
            {
                SetProperty(TermValueProperty, value);
            }
        }

        private static PropertyInfo<string> NameProperty = RegisterProperty<string>(new PropertyInfo<string>("Name", "Name", string.Empty));
        public string Name
        {
            get
            {
                return GetProperty(NameProperty);
            }
            set
            {
                SetProperty(NameProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Term), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Term), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Term), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Term), canRead);

        //    //AuthorizationRules.AllowRead(NameProperty, canRead);

        //    //AuthorizationRules.AllowWrite(NameProperty, canWrite);
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
                using (SqlCommand command = new SqlCommand("st_TermInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@Term_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@Term_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_Term_ID", 0));
                    command.Parameters["@NEW_Term_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(TermIdProperty, command.Parameters["@NEW_Term_ID"].Value);
                }
                connection.Close();
            }
             */
        }

        protected override void DataPortal_Update()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TermUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Term_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@Term_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@Term_ID", ReadProperty(TermIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TermDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Term_ID", ReadProperty(TermIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        internal static Term GetTerm(string Term, double Value)
        {
            Term returnValue = new Term();
            returnValue.LoadProperty<double>(TermValueProperty, Math.Max(Value, 0.1));
            returnValue.LoadProperty<string>(NameProperty, Term);
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
