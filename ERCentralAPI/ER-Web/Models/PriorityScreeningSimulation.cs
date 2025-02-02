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
    public class PriorityScreeningSimulation : BusinessBase<PriorityScreeningSimulation>
    {
#if SILVERLIGHT
    public PriorityScreeningSimulation() { }

        
#else
        public PriorityScreeningSimulation() { }
#endif


        public static readonly PropertyInfo<string> SimulationNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SimulationName", "SimulationName"));
        public string SimulationName
        {
            get
            {
                return GetProperty(SimulationNameProperty);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(PriorityScreeningSimulation), admin);
        //    //AuthorizationRules.AllowDelete(typeof(PriorityScreeningSimulation), admin);
        //    //AuthorizationRules.AllowEdit(typeof(PriorityScreeningSimulation), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(PriorityScreeningSimulation), canRead);

        //    //AuthorizationRules.AllowRead(PriorityScreeningSimulationIdProperty, canRead);
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
                using (SqlCommand command = new SqlCommand("st_PriorityScreeningSimulationInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PriorityScreeningSimulation_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@PriorityScreeningSimulation_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_PriorityScreeningSimulation_ID", 0));
                    command.Parameters["@NEW_PriorityScreeningSimulation_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(PriorityScreeningSimulationIdProperty, command.Parameters["@NEW_PriorityScreeningSimulation_ID"].Value);
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

        internal static PriorityScreeningSimulation GetPriorityScreeningSimulation(string simulationName)
        {
            PriorityScreeningSimulation returnValue = new PriorityScreeningSimulation();
            returnValue.LoadProperty<string>(SimulationNameProperty, simulationName);
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
