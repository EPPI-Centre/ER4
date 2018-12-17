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
    public class Diagram : BusinessBase<Diagram>
    {
#if SILVERLIGHT
    public Diagram() { }

        
#else
        private Diagram() { }
#endif

        public override string ToString()
        {
            return Name;
        }

        private static PropertyInfo<int> DiagramIdProperty = RegisterProperty<int>(new PropertyInfo<int>("DiagramId", "DiagramId"));
        public int DiagramId
        {
            get
            {
                return GetProperty(DiagramIdProperty);
            }
        }

        private static PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId"));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
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

        private static PropertyInfo<string> DetailProperty = RegisterProperty<string>(new PropertyInfo<string>("Detail", "Detail", string.Empty));
        public string Detail
        {
            get
            {
                return GetProperty(DetailProperty);
            }
            set
            {
                SetProperty(DetailProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Diagram), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Diagram), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Diagram), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Diagram), canRead);

        //    //AuthorizationRules.AllowRead(DiagramIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(NameProperty, canRead);
        //    //AuthorizationRules.AllowRead(DetailProperty, canRead);

        //    //AuthorizationRules.AllowWrite(NameProperty, canWrite);
        //    //AuthorizationRules.AllowRead(DetailProperty, canRead);
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
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            LoadProperty(ReviewIdProperty, ri.ReviewId);
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_DiagramInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@DIAGRAM_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@DIAGRAM_DETAIL", ReadProperty(DetailProperty)));
                    SqlParameter par = new SqlParameter("@NEW_DIAGRAM_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_DIAGRAM_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(DiagramIdProperty, command.Parameters["@NEW_DIAGRAM_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_DiagramUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@DIAGRAM_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@DIAGRAM_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@DIAGRAM_ID", ReadProperty(DiagramIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_DiagramDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@DIAGRAM_ID", ReadProperty(DiagramIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static Diagram GetDiagram(SafeDataReader reader)
        {
            Diagram returnValue = new Diagram();
            returnValue.LoadProperty<int>(DiagramIdProperty, reader.GetInt32("DIAGRAM_ID"));
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<string>(NameProperty, reader.GetString("DIAGRAM_NAME"));
            returnValue.LoadProperty<string>(DetailProperty, reader.GetString("DIAGRAM_DETAIL"));

            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
