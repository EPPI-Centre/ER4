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
using System.Collections.ObjectModel;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    

    [Serializable]
    public class MetaAnalysisModerator : BusinessBase<MetaAnalysisModerator>
    {
    public MetaAnalysisModerator()
    {
        this.References = new MetaAnalysisModeratorReferenceList();
    }

  

    public override string ToString()
        {
            return Name;
        }
        //public void DoMarkAsOld()
        //{
        //    this.MarkOld();
        //    foreach (MetaAnalysisModeratorReference reff in References)
        //    {
        //        reff.DoMarkAsOld();
        //    }
        //}
        
        public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(new PropertyInfo<string>("Name", "Name", string.Empty));
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

        public static readonly PropertyInfo<string> FieldNameProperty = RegisterProperty<string>(new PropertyInfo<string>("FieldName", "FieldName", string.Empty));
        public string FieldName
        {
            get
            {
                return GetProperty(FieldNameProperty);
            }
            set
            {
                SetProperty(FieldNameProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> AttributeIDProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeID", "AttributeID", 0.0));
        public Int64 AttributeID
        {
            get
            {
                return GetProperty(AttributeIDProperty);
            }
            set
            {
                SetProperty(AttributeIDProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ReferenceProperty = RegisterProperty<string>(new PropertyInfo<string>("Reference", "Reference", string.Empty));
        public string Reference
        {
            get
            {
                return GetProperty(ReferenceProperty);
            }
            set
            {
                SetProperty(ReferenceProperty, value);
            }
        }


        public static readonly PropertyInfo<MetaAnalysisModeratorReferenceList> ReferencesProperty = RegisterProperty<MetaAnalysisModeratorReferenceList>(new PropertyInfo<MetaAnalysisModeratorReferenceList>("References", "References"));
        public MetaAnalysisModeratorReferenceList References
        {
            get
            {
                return GetProperty(ReferencesProperty);
            }
            set
            {
                SetProperty(ReferencesProperty, value);
            }
        }

        public bool hasRefValue(string val)
        {
            foreach (MetaAnalysisModeratorReference rv in References)
            {
                if (rv.Name == val) return true;
            }
            return false;
        }

        public void addReferenceValue(string name, int id)
        {
            if (name != "")
            {
                MetaAnalysisModeratorReference rv = new MetaAnalysisModeratorReference();
                rv.Name = name;
                rv.AttributeID = id;
                References.Add(rv);
            }
        }

        public static readonly PropertyInfo<bool> IsSelectedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsSelected", "IsSelected", false));
        public bool IsSelected
        {
            get
            {
                return GetProperty(IsSelectedProperty);
            }
            set
            {
                SetProperty(IsSelectedProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> IsFactorProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsFactor", "IsFactor", false));
        public bool IsFactor
        {
            get
            {
                return GetProperty(IsFactorProperty);
            }
            set
            {
                SetProperty(IsFactorProperty, value);
            }
        }

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
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisModeratorInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModerator_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModerator_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_MetaAnalysisModerator_ID", 0));
                    command.Parameters["@NEW_MetaAnalysisModerator_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MetaAnalysisModeratorIdProperty, command.Parameters["@NEW_MetaAnalysisModerator_ID"].Value);
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
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisModeratorUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModerator_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModerator_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModerator_ID", ReadProperty(MetaAnalysisModeratorIdProperty)));
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
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisModeratorDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModerator_ID", ReadProperty(MetaAnalysisModeratorIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        internal static MetaAnalysisModerator GetMetaAnalysisModerator()
        {
            MetaAnalysisModerator returnValue = new MetaAnalysisModerator();
            returnValue.References = MetaAnalysisModeratorReferenceList.GetMetaAnalysisModeratorReferenceList();
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }

    
}
