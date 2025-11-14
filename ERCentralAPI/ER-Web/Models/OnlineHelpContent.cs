using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Csla;
using Csla.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class OnlineHelpContent : Csla.BusinessBase<OnlineHelpContent>
    {
        public OnlineHelpContent() { }

        public static OnlineHelpContent UpdateHelpContent(string Context, string SectionName, string HelpContent, string ParentContext)
        {
            OnlineHelpContent res = new OnlineHelpContent(Context, SectionName, HelpContent, ParentContext);
            return res;
        }
        public static OnlineHelpContent DeleteHelpContent(string Context, string SectionName, string HelpContent, string ParentContext)
        {
            OnlineHelpContent res = new OnlineHelpContent(Context, SectionName, HelpContent, ParentContext);
            return res;
        }
        private OnlineHelpContent(string context, string sectionName, string helpContent, string parentContext)
        {
            SetProperty(ContextProperty, context);
            SetProperty(SectionNameProperty, sectionName);
            SetProperty(HelpHTMLProperty, helpContent);
            SetProperty(ParentContextProperty, parentContext);
        }

        public static readonly PropertyInfo<int> OnlineHelpContentIdProperty = RegisterProperty<int>(new PropertyInfo<int>("OnlineHelpContentId", "OnlineHelpContentId"));
        public int OnlineHelpContentId
        {
            get
            {
                return GetProperty(OnlineHelpContentIdProperty);
            }
        }

        public static readonly PropertyInfo<string> ContextProperty = RegisterProperty<string>(new PropertyInfo<string>("Context", "Context", string.Empty));
        public string Context
        {
            get
            {
                return GetProperty(ContextProperty);
            }
            set
            {
                SetProperty(ContextProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ParentContextProperty = RegisterProperty<string>(new PropertyInfo<string>("ParentContext", "ParentContext", string.Empty));
        public string ParentContext
        {
            get
            {
                return GetProperty(ParentContextProperty);
            }
            set
            {
                SetProperty(ParentContextProperty, value);
            }
        }

        public static readonly PropertyInfo<string> SectionNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SectionName", "SectionName", string.Empty));
        public string SectionName
        {
            get
            {
                return GetProperty(SectionNameProperty);
            }
            set
            {
                SetProperty(SectionNameProperty, value);
            }
        }

        public static readonly PropertyInfo<string> HelpHTMLProperty = RegisterProperty<string>(new PropertyInfo<string>("HelpHTML", "HelpHTML", string.Empty));
        public string HelpHTML
        {
            get
            {
                return GetProperty(HelpHTMLProperty);
            }
            set
            {
                SetProperty(HelpHTMLProperty, value);
            }
        }


        //protected override void AddAuthorizationRules()
        //{
        //    
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected void DataPortal_Fetch(OnlineHelpCriteria crit)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            LoadProperty<string>(ContextProperty, crit.Context);//we do this in all cases...
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("sp_OnlineHelpGet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTEXT", crit.Context));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<int>(OnlineHelpContentIdProperty, reader.GetInt32("ONLINE_HELP_ID"));
                            LoadProperty<string>(HelpHTMLProperty, reader.GetString("HELP_HTML"));
                            LoadProperty<string>(ContextProperty, reader.GetString("CONTEXT"));
                            LoadProperty<string>(SectionNameProperty, reader.GetString("SECTION_NAME"));
                            LoadProperty<string>(ParentContextProperty, reader.GetString("PARENT_CONTEXT"));
                        }
                    }
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            UpdateOrInsert();
        }


        private void UpdateOrInsert()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OnlineHelpCreateOrEdit", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTEXT", ReadProperty(ContextProperty)));
                    command.Parameters.Add(new SqlParameter("@SECTION_NAME", ReadProperty(SectionNameProperty)));
                    command.Parameters.Add(new SqlParameter("@HELP_HTML", ReadProperty(HelpHTMLProperty)));
                    command.Parameters.Add(new SqlParameter("@PARENT_CONTEXT", ReadProperty(ParentContextProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }


        protected override void DataPortal_Insert()
        {
            UpdateOrInsert();
        }
        

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OnlineHelpDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CONTEXT", ReadProperty(ContextProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

#endif

    }



    [Serializable()]
    public class OnlineHelpCriteria : CriteriaBase<CredentialsCriteria>
    {
        public string Context = "";
        public OnlineHelpCriteria(string context) {
            Context = context;
        }
    }
}

