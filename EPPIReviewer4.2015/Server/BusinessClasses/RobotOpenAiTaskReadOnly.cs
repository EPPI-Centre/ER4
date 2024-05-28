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
using System.ComponentModel;
using BusinessLibrary.Security;

#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotOpenAiTaskReadOnly : ReadOnlyBase<RobotOpenAiTaskReadOnly>
    {
        public RobotOpenAiTaskReadOnly() { }



        public static readonly PropertyInfo<int> RobotApiCallIdProperty = RegisterProperty<int>(new PropertyInfo<int>("RobotApiCallId", "RobotApiCallId"));
        public int RobotApiCallId
        {
            get
            {
                return GetProperty(RobotApiCallIdProperty);
            }
        }

        public static readonly PropertyInfo<int> CreditPurchaseIdProperty = RegisterProperty<int>(new PropertyInfo<int>("CreditPurchaseId", "CreditPurchaseId"));
        public int CreditPurchaseId
        {
            get
            {
                return GetProperty(CreditPurchaseIdProperty);
            }
        }

        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId"));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
        }

        public static readonly PropertyInfo<int> RobotIdProperty = RegisterProperty<int>(new PropertyInfo<int>("RobotId", "RobotId"));
        public int RobotId
        {
            get
            {
                return GetProperty(RobotIdProperty);
            }
        }
        public static readonly PropertyInfo<int> JobOwnerIdProperty = RegisterProperty<int>(new PropertyInfo<int>("JobOwnerId", "JobOwnerId"));
        public int JobOwnerId
        {
            get
            {
                return GetProperty(JobOwnerIdProperty);
            }
        }
        public static readonly PropertyInfo<int> ReviewSetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewSetId", "ReviewSetId"));
        public int ReviewSetId
        {
            get
            {
                return GetProperty(ReviewSetIdProperty);
            }
        }

        public static readonly PropertyInfo<string> RawCriteriaProperty = RegisterProperty<string>(new PropertyInfo<string>("RawCriteria", "RawCriteria"));
        public string RawCriteria
        {
            get
            {
                return GetProperty(RawCriteriaProperty);
            }
        }
        public static readonly PropertyInfo<MobileList<long>> ItemIDsListProperty = RegisterProperty<MobileList<long>>(new PropertyInfo<MobileList<long>>("ItemIDsList", "ItemIDsList"));
        public MobileList<long> ItemIDsList
        {
            get
            {
                return GetProperty(ItemIDsListProperty);
            }
        }

        public static readonly PropertyInfo<string> StatusProperty = RegisterProperty<string>(new PropertyInfo<string>("Status", "Status"));
        public string Status
        {
            get
            {
                return GetProperty(StatusProperty);
            }
        }

        public static readonly PropertyInfo<long> CurrentItemIdProperty = RegisterProperty<long>(new PropertyInfo<long>("CurrentItemId", "CurrentItemId"));
        public long CurrentItemId
        {
            get
            {
                return GetProperty(CurrentItemIdProperty);
            }
        }

        public static readonly PropertyInfo<DateTime> CreatedProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("Created", "Created"));
        public DateTime Created
        {
            get
            {
                return GetProperty(CreatedProperty);
            }
        }

        public static readonly PropertyInfo<DateTime> UpdatedProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("Updated", "Updated"));
        public DateTime Updated
        {
            get
            {
                return GetProperty(UpdatedProperty);
            }
        }

        public static readonly PropertyInfo<bool> SuccessProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Success", "Success"));
        public bool Success
        {
            get
            {
                return GetProperty(SuccessProperty);
            }
        }

        public static readonly PropertyInfo<int> InputTokensProperty = RegisterProperty<int>(new PropertyInfo<int>("InputTokens", "InputTokens"));
        public int InputTokens
        {
            get
            {
                return GetProperty(InputTokensProperty);
            }
        }

        public static readonly PropertyInfo<int> OutputTokensProperty = RegisterProperty<int>(new PropertyInfo<int>("OutputTokens", "OutputTokens"));
        public int OutputTokens
        {
            get
            {
                return GetProperty(OutputTokensProperty);
            }
        }


        public static readonly PropertyInfo<double> CostProperty = RegisterProperty<double>(new PropertyInfo<double>("Cost", "Cost"));
        public double Cost
        {
            get
            {
                return GetProperty(CostProperty);
            }
        }


        public static readonly PropertyInfo<bool> OnlyCodeInTheRobotNameProperty = RegisterProperty<bool>(new PropertyInfo<bool>("OnlyCodeInTheRobotName", "OnlyCodeInTheRobotName"));
        public bool OnlyCodeInTheRobotName
        {
            get
            {
                return GetProperty(OnlyCodeInTheRobotNameProperty);
            }
        }

        public static readonly PropertyInfo<bool> LockTheCodingProperty = RegisterProperty<bool>(new PropertyInfo<bool>("LockTheCoding", "LockTheCoding"));
        public bool LockTheCoding
        {
            get
            {
                return GetProperty(LockTheCodingProperty);
            }
        }

        public static readonly PropertyInfo<int> RobotContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("RobotContactId", "RobotContactId"));
        public int RobotContactId
        {
            get
            {
                return GetProperty(RobotContactIdProperty);
            }
        }
        



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyItemAttribute), canRead);
        //    //AuthorizationRules.AllowRead(ItemAttributeIdProperty, canRead);
        //}

#if !SILVERLIGHT



        protected void DataPortal_Fetch(RobotOpenAiTaskCriteria criteria)
        {
            //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (criteria.NextCreditTask)
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_RobotApiJobFetchNextCreditTaksByRobotName", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ROBOT_NAME", "OpenAI GPT4"));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                Child_Fetch(reader);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            
        }

        private void Child_Fetch(SafeDataReader reader)
        { //look into CreditForRobotList to see how to change this so that it uses to parameters, where the 2nd is used to make sure we don't fill in values that the current user shouldn't see
            LoadProperty<int>(RobotApiCallIdProperty, reader.GetInt32("ROBOT_API_CALL_ID"));  
            LoadProperty<int>(CreditPurchaseIdProperty, reader.GetInt32("CREDIT_PURCHASE_ID"));
            LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            LoadProperty<int>(RobotIdProperty, reader.GetInt32("ROBOT_ID"));
            LoadProperty<int>(ReviewSetIdProperty, reader.GetInt32("REVIEW_SET_ID"));
            LoadProperty<string>(RawCriteriaProperty, reader.GetString("CRITERIA"));
            LoadProperty<string>(StatusProperty, reader.GetString("STATUS"));
            LoadProperty<long>(CurrentItemIdProperty, reader.GetInt64("CURRENT_ITEM_ID"));
            LoadProperty<DateTime>(CreatedProperty, reader.GetSmartDate("DATE_CREATED"));
            LoadProperty<DateTime>(UpdatedProperty, reader.GetSmartDate("DATE_UPDATED"));
            LoadProperty<bool>(SuccessProperty, reader.GetBoolean("SUCCESS"));
            LoadProperty<int>(InputTokensProperty, reader.GetInt32("INPUT_TOKENS_COUNT"));
            LoadProperty<int>(OutputTokensProperty, reader.GetInt32("OUTPUT_TOKENS_COUNT"));
            LoadProperty<double>(CostProperty, reader.GetDouble("COST"));
            LoadProperty<bool>(OnlyCodeInTheRobotNameProperty, reader.GetBoolean("FORCE_CODING_IN_ROBOT_NAME"));
            LoadProperty<bool>(LockTheCodingProperty, reader.GetBoolean("LOCK_CODING"));
            LoadProperty<int>(RobotContactIdProperty, reader.GetInt32("ROBOT_CONTACT_ID"));
            LoadProperty<int>(JobOwnerIdProperty, reader.GetInt32("CONTACT_ID"));

            LoadProperty<MobileList<long>>(ItemIDsListProperty, new MobileList<long>());
            if (RawCriteria.StartsWith("ItemIds: "))
            {
                string toSplit = RawCriteria.Substring(8);
                string[] IdsString = toSplit.Split(',');
                foreach (string s in IdsString)
                {
                    long ItemId;
                    if (long.TryParse(s, out ItemId)) ItemIDsList.Add(ItemId);
                }
            }
        }


#endif
    }
    [Serializable]
    public class RobotOpenAiTaskCriteria : CriteriaBase<RobotOpenAiTaskCriteria>
    {
        public bool NextCreditTask { get; private set; } = true;
        public int JobId { get; private set; } = 0;
        public RobotOpenAiTaskCriteria() { }
        public static RobotOpenAiTaskCriteria NewNextCreditTaskCriteria()
        {
            RobotOpenAiTaskCriteria res = new RobotOpenAiTaskCriteria();
            res.NextCreditTask = true;
            res.JobId = 0;
            return res;
        }
        public static RobotOpenAiTaskCriteria NewByJobIdCriteria(int JobId)
        {
            RobotOpenAiTaskCriteria res = new RobotOpenAiTaskCriteria();
            res.NextCreditTask = false;
            res.JobId = JobId;
            return res;
        }
    }
}
