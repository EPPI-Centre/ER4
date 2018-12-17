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
    public class MetaAnalysisModeratorReference : BusinessBase<MetaAnalysisModeratorReference>
    {
#if SILVERLIGHT
    public MetaAnalysisModeratorReference()
    {
        
    }

        
#else
        public MetaAnalysisModeratorReference() { }
#endif

       

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

        private static PropertyInfo<Int64> AttributeIDProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeID", "AttributeID", 0.0));
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
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisModeratorReferenceInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModeratorReference_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModeratorReference_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_MetaAnalysisModeratorReference_ID", 0));
                    command.Parameters["@NEW_MetaAnalysisModeratorReference_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MetaAnalysisModeratorReferenceIdProperty, command.Parameters["@NEW_MetaAnalysisModeratorReference_ID"].Value);
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
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisModeratorReferenceUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModeratorReference_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModeratorReference_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModeratorReference_ID", ReadProperty(MetaAnalysisModeratorReferenceIdProperty)));
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
                using (SqlCommand command = new SqlCommand("st_MetaAnalysisModeratorReferenceDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MetaAnalysisModeratorReference_ID", ReadProperty(MetaAnalysisModeratorReferenceIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
             */
        }

        internal static MetaAnalysisModeratorReference GetMetaAnalysisModeratorReference()
        {
            MetaAnalysisModeratorReference returnValue = new MetaAnalysisModeratorReference();
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }


}
