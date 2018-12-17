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
    public class ReportColumn : BusinessBase<ReportColumn>, IComparable
    {

        public void RemoveSelf(ReportColumnList rcl)
        {
            this.MarkNew();
            rcl.Remove(this);
            this.MarkOld();
        }


#if SILVERLIGHT
    public ReportColumn() { }
    public int BufferPosition = 0;
        
#else
        private ReportColumn() { }
#endif

        public override string ToString()
        {
            return Name;
        }

        private static PropertyInfo<int> ReportColumnIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReportColumnId", "ReportColumnId"));
        public int ReportColumnId
        {
            get
            {
                return GetProperty(ReportColumnIdProperty);
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

        private static PropertyInfo<int> ColumnOrderProperty = RegisterProperty<int>(new PropertyInfo<int>("ColumnOrder", "ColumnOrder", 0));
        public int ColumnOrder
        {
            get
            {
                return GetProperty(ColumnOrderProperty);
            }
            set
            {
                SetProperty(ColumnOrderProperty, value);
            }
        }

        private static PropertyInfo<ReportColumnCodeList> CodesProperty = RegisterProperty<ReportColumnCodeList>(new PropertyInfo<ReportColumnCodeList>("Codes", "Codes"));
        public ReportColumnCodeList Codes
        {
            get
            {
                return GetProperty(CodesProperty);
            }
            set
            {
                SetProperty(CodesProperty, value);
            }
        }
        public int CompareTo(object y)
        {//implements IComparable, used to sort items!
            if (y == null) return 1;
            ReportColumn yy = y as ReportColumn;
            if (yy == null) return 1;
            if (yy.ColumnOrder == this.ColumnOrder) return 0;
            else if (yy.ColumnOrder > this.ColumnOrder) return -1;
            else return 1;
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ReportColumn), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ReportColumn), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ReportColumn), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ReportColumn), canRead);

        //    //AuthorizationRules.AllowRead(ReportColumnIdProperty, canRead);
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
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportColumnInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REPORT_COLUMN_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@COLUMN_ORDER", ReadProperty(ColumnOrderProperty)));
                    SqlParameter par = new SqlParameter("@NEW_REPORT_COLUMN_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par); 
                    command.Parameters["@NEW_REPORT_COLUMN_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ReportColumnIdProperty, command.Parameters["@NEW_REPORT_COLUMN_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportColumnUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REPORT_COLUMN_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@COLUMN_ORDER", ReadProperty(ColumnOrderProperty)));
                    command.Parameters.Add(new SqlParameter("@REPORT_COLUMN_ID", ReadProperty(ReportColumnIdProperty)));
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
                using (SqlCommand command = new SqlCommand("st_ReportColumnDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REPORT_COLUMN_ID", ReadProperty(ReportColumnIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static ReportColumn GetReportColumn(SafeDataReader reader)
        {
            ReportColumn returnValue = new ReportColumn();
            returnValue.LoadProperty<int>(ReportColumnIdProperty, reader.GetInt32("REPORT_COLUMN_ID"));
            returnValue.LoadProperty<string>(NameProperty, reader.GetString("REPORT_COLUMN_NAME"));
            returnValue.LoadProperty<int>(ColumnOrderProperty, reader.GetInt32("COLUMN_ORDER"));

            returnValue.Codes = ReportColumnCodeList.NewReportColumnCodeList();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReportColumnCodeList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REPORT_COLUMN_ID", returnValue.ReadProperty(ReportColumnIdProperty)));
                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        returnValue.Codes.RaiseListChangedEvents = false;
                        while (reader2.Read())
                        {
                            returnValue.Codes.Add(ReportColumnCode.GetReportColumnCode(reader2));
                        }
                        returnValue.Codes.RaiseListChangedEvents = true;
                    }
                }
                connection.Close();
            }

            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
