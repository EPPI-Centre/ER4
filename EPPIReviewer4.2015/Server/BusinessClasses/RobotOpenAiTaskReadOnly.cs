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
        public static readonly PropertyInfo<bool> UseFullTextDocumentProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UseFullTextDocument", "UseFullTextDocument"));
        public bool UseFullTextDocument
        {
            get
            {
                return GetProperty(UseFullTextDocumentProperty);
            }
        }

        public static readonly PropertyInfo<MobileList<RobotOpenAiTaskError>> ErrorsProperty = RegisterProperty<MobileList<RobotOpenAiTaskError>>(new PropertyInfo<MobileList<RobotOpenAiTaskError>>("Errors", "Errors"));
        public MobileList<RobotOpenAiTaskError> Errors
        {
            get
            {
                return GetProperty(ErrorsProperty);
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
            LoadProperty(ErrorsProperty, new MobileList<RobotOpenAiTaskError>());
            //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (criteria.NextCreditTask)
            {
                List<RobotOpenAiTaskReadOnly> JobsToConsider = new List<RobotOpenAiTaskReadOnly>();
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_RobotApiJobFetchNextCreditTasksByRobotName", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ROBOT_NAME", "OpenAI GPT4"));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            while (reader.Read())
                            {
                                RobotOpenAiTaskReadOnly job = new RobotOpenAiTaskReadOnly();
                                job.Child_Fetch(reader, false);
                                JobsToConsider.Add(job);
                            }
                        }
                        //JobsToConsider contains running jobs and queued jobs, task is to pick up the one job we want to run next
                        //Criteria: if there are jobs running, pick up one from different reviews, otherwise the older
                        //So, we'll filter our list and see what remains...
                        int ChosenJobId = 0;
                        List<RobotOpenAiTaskReadOnly> RunningJobs = JobsToConsider.FindAll(f => f.Status == "Running");
                        List<RobotOpenAiTaskReadOnly> QueuedJobs = JobsToConsider.FindAll(f => f.Status != "Running");
                        List<RobotOpenAiTaskReadOnly> QueuedJobsTmp = QueuedJobs;
                        if (RunningJobs.Count == 0 && QueuedJobs.Count > 0)
                        {//easy case, just pick the 1st!
                            ChosenJobId = QueuedJobs[0].RobotApiCallId;
                        }
                        else if (RunningJobs.Count > 0 && QueuedJobs.Count > 0)
                        {
                            foreach (RobotOpenAiTaskReadOnly RunningJob in RunningJobs)
                            {
                                QueuedJobsTmp = QueuedJobsTmp.FindAll(f => f.ReviewId != RunningJob.ReviewId);
                            }
                            if (QueuedJobsTmp.Count > 0)
                            {
                                ChosenJobId = QueuedJobsTmp[0].RobotApiCallId;
                            }
                        }
                        if (ChosenJobId == 0 && QueuedJobs.Count > 0)
                        {//we didn't find any job from a review for which a job isn't running already, so we'll just pick the oldest queued job...
                            ChosenJobId = QueuedJobs[0].RobotApiCallId;
                        }
                        if (ChosenJobId > 0)
                        {
                            //we run the same SP again, but this time, we know what RobotApiCallId to use
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                int LineJobId = 0;
                                while (reader.Read())
                                {
                                    LineJobId = reader.GetInt32("ROBOT_API_CALL_ID");
                                    if (ChosenJobId == LineJobId)
                                    {
                                        Child_Fetch(reader, false);
                                    }
                                    
                                }
                            }
                        }
                    }
                    
                    connection.Close();
                }
            }
            
        }

        private void Child_Fetch(SafeDataReader reader, bool isPrivate, int ReviewId = 0, int ContactId = 0 )
        {
            LoadProperty(ErrorsProperty, new MobileList<RobotOpenAiTaskError>());
            if (isPrivate)
            {
                if (reader.GetInt32("REVIEW_ID") == ReviewId || reader.GetInt32("CONTACT_ID") == ContactId) Child_FetchAllDetails(reader);
                else Child_FetchFilteredDetails(reader);
            }
            else Child_FetchAllDetails(reader);
        }
        private void Child_FetchAllDetails(SafeDataReader reader)
        { 
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
            LoadProperty<bool>(UseFullTextDocumentProperty, reader.GetBoolean("USE_PDFS")); 

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
        private void Child_FetchFilteredDetails(SafeDataReader reader)
        {//mask out all the details: this job does not belong to the current review or user, so we keep the private parts private
            //todo: a mark as failed SP!
            LoadProperty<int>(RobotApiCallIdProperty, reader.GetInt32("ROBOT_API_CALL_ID"));
            LoadProperty<int>(CreditPurchaseIdProperty, -1);
            LoadProperty<int>(ReviewIdProperty, -1);
            LoadProperty<int>(RobotIdProperty, reader.GetInt32("ROBOT_ID"));
            LoadProperty<int>(ReviewSetIdProperty, -1);
            LoadProperty<string>(RawCriteriaProperty, reader.GetString("CRITERIA"));
            LoadProperty<string>(StatusProperty, reader.GetString("STATUS"));
            LoadProperty<long>(CurrentItemIdProperty, reader.GetInt64("CURRENT_ITEM_ID"));
            LoadProperty<DateTime>(CreatedProperty, reader.GetSmartDate("DATE_CREATED"));
            LoadProperty<DateTime>(UpdatedProperty, reader.GetSmartDate("DATE_UPDATED"));
            LoadProperty<bool>(SuccessProperty, reader.GetBoolean("SUCCESS"));
            LoadProperty<int>(InputTokensProperty, -1);
            LoadProperty<int>(OutputTokensProperty, -1);
            LoadProperty<double>(CostProperty, -1);
            LoadProperty<bool>(OnlyCodeInTheRobotNameProperty, true);
            LoadProperty<bool>(LockTheCodingProperty, true);
            LoadProperty<int>(RobotContactIdProperty, reader.GetInt32("ROBOT_CONTACT_ID"));
            LoadProperty<int>(JobOwnerIdProperty, -1);

            LoadProperty<MobileList<long>>(ItemIDsListProperty, new MobileList<long>());
            if (RawCriteria.StartsWith("ItemIds: "))
            {
                string toSplit = RawCriteria.Substring(8);
                string[] IdsString = toSplit.Split(',');
                foreach (string s in IdsString)
                {
                    long ItemId;
                    if (long.TryParse(s, out ItemId))
                    {
                        if (CurrentItemId == ItemId) ItemIDsList.Add(-1);
                        else ItemIDsList.Add(0);
                    }
                }
                LoadProperty<string>(RawCriteriaProperty, ItemIDsList.Count.ToString() + " items");
                
            }
            if (CurrentItemId > 0) LoadProperty<long>(CurrentItemIdProperty, -1);
        }


#endif
    }
    
    [Serializable]
    public class RobotOpenAiTaskError : ReadOnlyBase<RobotOpenAiTaskError>
    {
        public static readonly PropertyInfo<long> AffectedItemIdProperty = RegisterProperty<long>(new PropertyInfo<long>("AffectedItemId", "AffectedItemId"));
        public long AffectedItemId
        {
            get
            {
                return GetProperty(AffectedItemIdProperty);
            }
        }

        public static readonly PropertyInfo<string> ErrorMessageProperty = RegisterProperty<string>(new PropertyInfo<string>("ErrorMessage", "ErrorMessage"));
        public string ErrorMessage
        {
            get
            {
                return GetProperty(ErrorMessageProperty);
            }
        }

        public static readonly PropertyInfo<string> StackTraceProperty = RegisterProperty<string>(new PropertyInfo<string>("StackTrace", "StackTrace"));
        public string StackTrace
        {
            get
            {
                return GetProperty(StackTraceProperty);
            }
        }
        public static RobotOpenAiTaskError Child_FetchError(SafeDataReader reader)
        {
            RobotOpenAiTaskError res = new RobotOpenAiTaskError();
            res.LoadProperty(AffectedItemIdProperty, reader.GetInt64("ITEM_ID"));
            res.LoadProperty(ErrorMessageProperty, reader.GetString("ERROR_MESSAGE"));
            res.LoadProperty(StackTraceProperty, reader.GetString("STACK_TRACE"));
            return res;
        }
    }
    
    [Serializable]
    public class RobotOpenAiTaskCriteria : CriteriaBase<RobotOpenAiTaskCriteria>
    {
        public bool NextCreditTask { get; private set; } = true;
        public bool PastJobs { get; private set; } = false;
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
        public static RobotOpenAiTaskCriteria NewPastJobsCriteria()
        {
            RobotOpenAiTaskCriteria res = new RobotOpenAiTaskCriteria();
            res.NextCreditTask = false;
            res.PastJobs = true;
            return res;
        }
    }
}
