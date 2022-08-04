using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Csla;
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
                        }
                    }
                }
                connection.Close();
            }
        }
        //protected override void DataPortal_Insert()
        //{

        //}
        //protected override void DataPortal_Update()
        //{

        //}

        //protected override void DataPortal_DeleteSelf()
        //{

        //}

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

