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
    public class Report : BusinessBase<Report>, IEquatable<Report>
    {
#if SILVERLIGHT
    public Report() { Columns = ReportColumnList.NewReportColumnList(); }
        
#else
        public Report() { }
#endif
        
        public override string ToString()
        {
            return Name;
        }

        public static readonly PropertyInfo<int> ReportIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReportId", "ReportId"));
        public int ReportId
        {
            get
            {
                return GetProperty(ReportIdProperty);
            }
        }

        public static readonly  PropertyInfo<string> NameProperty = RegisterProperty<string>(new PropertyInfo<string>("Name", "Name", string.Empty));
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

        public static readonly  PropertyInfo<string> DetailProperty = RegisterProperty<string>(new PropertyInfo<string>("Detail", "Detail", string.Empty));
        public string Detail
        {
            get
            {
                return GetProperty(DetailProperty);
            }
            set
            {
                SetProperty(DetailProperty, value);
                MarkDirty();
            }
        }

        public static readonly  PropertyInfo<ReportColumnList> ColumnsProperty = RegisterProperty<ReportColumnList>(new PropertyInfo<ReportColumnList>("Columns", "Columns"));
        public ReportColumnList Columns
        {
            get
            {
                return GetProperty(ColumnsProperty);
            }
            set
            {
                SetProperty(ColumnsProperty, value);
            }
        }

        public static readonly  PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName", string.Empty));
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
            }
            set
            {
                SetProperty(ContactNameProperty, value);
            }
        }

        public static readonly  PropertyInfo<string> ReportTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("ReportType", "ReportType", "Question"));
        public string ReportType
        {
            get
            {
                return GetProperty(ReportTypeProperty);
            }
            set
            {
                SetProperty(ReportTypeProperty, value);
            }
        }
        public bool isAnswer
        {
            get { return this.ReportType == "Answer"; }
        }
        public bool CheckOkToChangeToAnswer()
        {
            bool retVal = true;
            foreach (ReportColumn rc in this.Columns)
            {
                if (rc.Codes.Count > 1)
                {
                    retVal = false;
                }
                foreach (ReportColumnCode rcc in rc.Codes)
                {
                    if (rcc.AttributeId == 0)
                    {
                        retVal = false;
                    }
                }
                if (retVal == false)
                {
                    break;
                }
            }
            return retVal;
        }
        public bool Equals(Report other)
        {
            if (other == null)
                return false;

            if (this.ReportId == other.ReportId)
                return true;
            else
                return false;
        }
        
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(Report), admin);
        //    //AuthorizationRules.AllowDelete(typeof(Report), admin);
        //    //AuthorizationRules.AllowEdit(typeof(Report), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(Report), canRead);

        //    //AuthorizationRules.AllowRead(ReportIdProperty, canRead);
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
        protected void DataPortal_Fetch(SingleCriteria<int> crit)
        {
            Report willBeMe = new Report();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_Report", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@REPORT_ID", crit.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            willBeMe = Report.GetReport(reader);
                            LoadProperty<int>(ReportIdProperty, willBeMe.ReportId);
                            LoadProperty<string>(NameProperty, willBeMe.Name);
                            LoadProperty<string>(ContactNameProperty, willBeMe.ContactName);
                            LoadProperty<string>(ReportTypeProperty, willBeMe.ReportType);
                            LoadProperty<ReportColumnList>(ColumnsProperty, willBeMe.Columns);
                            MarkOld();
                        }
                        else
                        {
                            Exception e = new Exception("Report with id = " + crit.Value + " not found in review " + ri.ReviewId.ToString()  + ".");
                            throw e;
                        }
                    }
                }
                connection.Close();
            }
        }
        protected override void DataPortal_Insert()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REPORT_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@REPORT_TYPE", ReadProperty(ReportTypeProperty)));
                    SqlParameter par = new SqlParameter("@NEW_REPORT_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_REPORT_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ReportIdProperty, command.Parameters["@NEW_REPORT_ID"].Value);
                }
                WriteColumns(connection);
                connection.Close();
            }
        }

        protected void WriteColumns(SqlConnection connection)
        {
            DeleteColumns(connection);
            int order = 0;
            foreach (ReportColumn rc in this.Columns)
            {
                if (rc.IsDeleted) continue;
                int newRcId = WriteColumn(connection, rc, order);
                int order2 = 0;
                foreach (ReportColumnCode rcc in rc.Codes)
                {
                    WriteColumnCode(connection, rcc, newRcId, order2);
                    order2++;
                }
                order++;
            }
        }

        protected void DeleteColumns(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand("st_ReportColumnDelete", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@REPORT_ID", ReadProperty(ReportIdProperty)));
                command.ExecuteNonQuery();
            }
        }

        protected int WriteColumn(SqlConnection connection, ReportColumn rc, int order)
        {
            int returnValue = 0;
            using (SqlCommand command = new SqlCommand("st_ReportColumnInsert", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@REPORT_COLUMN_NAME", rc.Name));
                command.Parameters.Add(new SqlParameter("@REPORT_ID", ReadProperty(ReportIdProperty)));
                command.Parameters.Add(new SqlParameter("@COLUMN_ORDER", rc.ColumnOrder));
                command.Parameters.Add(new SqlParameter("@NEW_REPORT_COLUMN_ID", 0));
                command.Parameters["@NEW_REPORT_COLUMN_ID"].Direction = System.Data.ParameterDirection.Output;
                command.ExecuteNonQuery();
                returnValue = Convert.ToInt32(command.Parameters["@NEW_REPORT_COLUMN_ID"].Value);
            }
            return returnValue;
        }

        protected void WriteColumnCode(SqlConnection connection, ReportColumnCode rcc, int columnId, int order)
        {
            using (SqlCommand command = new SqlCommand("st_ReportColumnCodeInsert", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@REPORT_ID", ReadProperty(ReportIdProperty)));
                command.Parameters.Add(new SqlParameter("@REPORT_COLUMN_ID", columnId));
                command.Parameters.Add(new SqlParameter("@CODE_ORDER", order));
                command.Parameters.Add(new SqlParameter("@SET_ID", rcc.SetId));
                command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", rcc.AttributeId));
                command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", rcc.ParentAttributeId));
                command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_TEXT", rcc.ParentAttributeText));
                command.Parameters.Add(new SqlParameter("@USER_DEF_TEXT", rcc.UserDefText));
                command.Parameters.Add(new SqlParameter("@DISPLAY_CODE", rcc.DisplayCode));
                command.Parameters.Add(new SqlParameter("@DISPLAY_ADDITIONAL_TEXT", rcc.DisplayAdditionalText));
                command.Parameters.Add(new SqlParameter("@DISPLAY_CODED_TEXT", rcc.DisplayCodedText));
                command.ExecuteNonQuery();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REPORT_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@REPORT_TYPE", ReadProperty(ReportTypeProperty)));
                    command.Parameters.Add(new SqlParameter("@REPORT_ID", ReadProperty(ReportIdProperty)));
                    command.ExecuteNonQuery();
                }
                WriteColumns(connection);
                connection.Close();
            }
            MarkClean();
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REPORT_ID", ReadProperty(ReportIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static Report GetReport(SafeDataReader reader)
        {
            Report returnValue = new Report();
            returnValue.LoadProperty<int>(ReportIdProperty, reader.GetInt32("REPORT_ID"));
            returnValue.LoadProperty<string>(NameProperty, reader.GetString("NAME"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<string>(ReportTypeProperty, reader.GetString("REPORT_TYPE"));
            //returnValue.LoadProperty<string>(DetailProperty, reader.GetString("REPORT_DETAIL"));
            returnValue.Columns = ReportColumnList.NewReportColumnList();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportColumnList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REPORT_ID", returnValue.ReadProperty(ReportIdProperty)));
                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        returnValue.Columns.RaiseListChangedEvents = false;
                        while (reader2.Read())
                        {
                            returnValue.Columns.Add(ReportColumn.GetReportColumn(reader2, ri.ReviewId));
                        }
                        returnValue.Columns.RaiseListChangedEvents = true;
                    }
                }
                connection.Close();
            }
            returnValue.MarkOld();
            //returnValue.MarkAsChild();
            return returnValue;
        }
       
#endif

    }
}
