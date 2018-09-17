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
    public class WorkAllocation : BusinessBase<WorkAllocation>
    {
#if SILVERLIGHT
    public WorkAllocation() { }

        
#else
        public WorkAllocation() { }
#endif

        public override string ToString()
        {
            return ContactName;
        }

        public static readonly PropertyInfo<int> WorkAllocationIdProperty = RegisterProperty<int>(new PropertyInfo<int>("WorkAllocationId", "WorkAllocationId"));
        public int WorkAllocationId
        {
            get
            {
                return GetProperty(WorkAllocationIdProperty);
            }
        }

        public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName", string.Empty));
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

        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
            set
            {
                SetProperty(ContactIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> SetNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetName", "SetName", string.Empty));
        public string SetName
        {
            get
            {
                return GetProperty(SetNameProperty);
            }
            set
            {
                SetProperty(SetNameProperty, value);
            }
        }

        public static readonly PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "SetId"));
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
            set
            {
                SetProperty(SetIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "AttributeName", string.Empty));
        public string AttributeName
        {
            get
            {
                return GetProperty(AttributeNameProperty);
            }
            set
            {
                SetProperty(AttributeNameProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
            set
            {
                SetProperty(AttributeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> TotalAllocationProperty = RegisterProperty<int>(new PropertyInfo<int>("TotalAllocation", "TotalAllocation"));
        public int TotalAllocation
        {
            get
            {
                return GetProperty(TotalAllocationProperty);
            }
            set
            {
                SetProperty(TotalAllocationProperty, value);
            }
        }

        public static readonly PropertyInfo<int> TotalStartedProperty = RegisterProperty<int>(new PropertyInfo<int>("TotalStarted", "TotalStarted"));
        public int TotalStarted
        {
            get
            {
                return GetProperty(TotalStartedProperty);
            }
            set
            {
                SetProperty(TotalStartedProperty, value);
            }
        }

        public static readonly PropertyInfo<int> TotalRemainingProperty = RegisterProperty<int>(new PropertyInfo<int>("TotalRemaining", "TotalRemaining"));
        public int TotalRemaining
        {
            get
            {
                return GetProperty(TotalRemainingProperty);
            }
            set
            {
                SetProperty(TotalRemainingProperty, value);
            }
        }
        
        
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(WorkAllocation), admin);
        //    //AuthorizationRules.AllowDelete(typeof(WorkAllocation), admin);
        //    //AuthorizationRules.AllowEdit(typeof(WorkAllocation), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(WorkAllocation), canRead);

        //    //AuthorizationRules.AllowRead(WorkAllocationIdProperty, canRead);
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
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                using (SqlCommand command = new SqlCommand("st_ReviewWorkAllocationInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));

                    SqlParameter par = new SqlParameter("@NEW_WORK_ALLOCATION_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par); 
                    command.Parameters["@NEW_WORK_ALLOCATION_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(WorkAllocationIdProperty, command.Parameters["@NEW_WORK_ALLOCATION_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            // isn't updated
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewWorkAllocationDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@WORK_ALLOCATION_ID", ReadProperty(WorkAllocationIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected void DataPortal_Fetch(SingleCriteria<WorkAllocation, Int64> criteria) // used to return a specific item
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WorkAllocation", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            //LoadProperty<Int64>(WorkAllocationIdProperty, reader.GetInt64("ITEM_DOCUMENT_ID"));
                            //LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static WorkAllocation GetWorkAllocation(SafeDataReader reader)
        {
            WorkAllocation returnValue = new WorkAllocation();
            returnValue.LoadProperty<int>(WorkAllocationIdProperty, reader.GetInt32("WORK_ALLOCATION_ID"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<string>(SetNameProperty, reader.GetString("SET_NAME"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.LoadProperty<int>(TotalAllocationProperty, reader.GetInt32("TOTAL_ALLOCATION"));
            returnValue.LoadProperty<int>(TotalStartedProperty, reader.GetInt32("TOTAL_STARTED"));
            returnValue.LoadProperty<int>(TotalRemainingProperty, returnValue.TotalAllocation - returnValue.TotalStarted);
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
