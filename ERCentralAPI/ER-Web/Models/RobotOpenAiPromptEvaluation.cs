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
using System.IO;
using static Csla.Security.MembershipIdentity;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using Azure.Storage.Blobs;
using BusinessLibrary.BusinessClasses;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class RobotOpenAiPromptEvaluation : BusinessBase<RobotOpenAiPromptEvaluation>
    {
#if SILVERLIGHT
    public RobotOpenAiPromptEvaluation() { }

        
#else
        public RobotOpenAiPromptEvaluation() { }
#endif


        public static readonly PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title"));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
            set
            {
                SetProperty(TitleProperty, value);
            }
        }
        public static readonly PropertyInfo<string> ReviewSetHtmlProperty = RegisterProperty<string>(new PropertyInfo<string>("ReviewSetHtml", "ReviewSetHtml"));
        public string ReviewSetHtml
        {
            get
            {
                return GetProperty(ReviewSetHtmlProperty);
            }
            set
            {
                SetProperty(ReviewSetHtmlProperty, value);
            }
        }
        public static readonly PropertyInfo<string> RobotNameProperty = RegisterProperty<string>(new PropertyInfo<string>("RobotName", "RobotName"));
        public string RobotName
        {
            get
            {
                return GetProperty(RobotNameProperty);
            }
            set
            {
                SetProperty(RobotNameProperty, value);
            }
        }
        public static readonly PropertyInfo<SmartDate> WhenRunProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("WhenRun", "WhenRun"));
        public SmartDate WhenRun
        {
            get
            {
                return GetProperty(WhenRunProperty);
            }
            set
            {
                SetProperty(WhenRunProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> UsePdfsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UsePdfs", "UsePdfs"));
        public bool UsePdfs
        {
            get
            {
                return GetProperty(UsePdfsProperty);
            }
            set
            {
                SetProperty(UsePdfsProperty, value);
            }
        }
        public static readonly PropertyInfo<int> NRecordsProperty = RegisterProperty<int>(new PropertyInfo<int>("NRecords", "NRecords"));
        public int NRecords
        {
            get
            {
                return GetProperty(NRecordsProperty);
            }
            set
            {
                SetProperty(NRecordsProperty, value);
            }
        }
        public static readonly PropertyInfo<int> NIterationsProperty = RegisterProperty<int>(new PropertyInfo<int>("NIterations", "NIterations"));
        public int NIterations
        {
            get
            {
                return GetProperty(NIterationsProperty);
            }
            set
            {
                SetProperty(NIterationsProperty, value);
            }
        }
        public static readonly PropertyInfo<int> NCodesProperty = RegisterProperty<int>(new PropertyInfo<int>("NCodes", "NCodes"));
        public int NCodes
        {
            get
            {
                return GetProperty(NCodesProperty);
            }
            set
            {
                SetProperty(NCodesProperty, value);
            }
        }
        public static readonly PropertyInfo<int> TPProperty = RegisterProperty<int>(new PropertyInfo<int>("TP", "TP"));
        public int TP
        {
            get
            {
                return GetProperty(TPProperty);
            }
            set
            {
                SetProperty(TPProperty, value);
            }
        }
        public static readonly PropertyInfo<int> TNProperty = RegisterProperty<int>(new PropertyInfo<int>("TN", "TN"));
        public int TN
        {
            get
            {
                return GetProperty(TNProperty);
            }
            set
            {
                SetProperty(TNProperty, value);
            }
        }
        public static readonly PropertyInfo<int> FPProperty = RegisterProperty<int>(new PropertyInfo<int>("FP", "FP"));
        public int FP
        {
            get
            {
                return GetProperty(FPProperty);
            }
            set
            {
                SetProperty(FPProperty, value);
            }
        }
        public static readonly PropertyInfo<int> FNProperty = RegisterProperty<int>(new PropertyInfo<int>("FN", "FN"));
        public int FN
        {
            get
            {
                return GetProperty(FNProperty);
            }
            set
            {
                SetProperty(FNProperty, value);
            }
        }
        public static readonly PropertyInfo<int> OpenAiPromptEvaluationIdProperty = RegisterProperty<int>(new PropertyInfo<int>("OpenAiPromptEvaluationId", "OpenAiPromptEvaluationId"));
        public int OpenAiPromptEvaluationId
        {
            get
            {
                return GetProperty(OpenAiPromptEvaluationIdProperty);
            }
            set
            {
                SetProperty(OpenAiPromptEvaluationIdProperty, value);
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



#if !SILVERLIGHT

        protected void DataPortal_Fetch(SingleCriteria<RobotOpenAiPromptEvaluation, string> criteria) // used to return a specific Paper
        {

        }
        protected override void DataPortal_Insert()
        {

        }

        protected override void DataPortal_Update()
        {
            // never updated via ui
        }

        protected override void DataPortal_DeleteSelf()
        {

        }

        protected void DataPortal_Delete(SingleCriteria<RobotOpenAiPromptEvaluation, string> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[st_RobotOpenAIPromptEvaluationDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@OPENAI_PROMPT_EVALUATION_ID", this.OpenAiPromptEvaluationId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static RobotOpenAiPromptEvaluation GetRobotOpenAiPromptEvaluation(SafeDataReader reader)
        {
            RobotOpenAiPromptEvaluation returnValue = new RobotOpenAiPromptEvaluation();
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(ReviewSetHtmlProperty, reader.GetString("REVIEW_SET_HTML"));
            returnValue.LoadProperty<string>(RobotNameProperty, reader.GetString("ROBOT_NAME"));
            returnValue.LoadProperty<Int32>(NRecordsProperty, reader.GetInt32("N_RECORDS"));
            returnValue.LoadProperty<Int32>(NIterationsProperty, reader.GetInt32("N_ITERATIONS"));
            returnValue.LoadProperty<Int32>(OpenAiPromptEvaluationIdProperty, reader.GetInt32("OPENAI_PROMPT_EVALUATION_ID"));
            returnValue.LoadProperty<Int32>(NCodesProperty, reader.GetInt32("N_CODES"));
            returnValue.LoadProperty<Int32>(TPProperty, reader.GetInt32("TP"));
            returnValue.LoadProperty<Int32>(TNProperty, reader.GetInt32("TN"));
            returnValue.LoadProperty<Int32>(FPProperty, reader.GetInt32("FP"));
            returnValue.LoadProperty<Int32>(FNProperty, reader.GetInt32("FN"));
            returnValue.LoadProperty<SmartDate>(WhenRunProperty, reader.GetSmartDate("WHEN_RUN"));
            returnValue.LoadProperty<bool>(UsePdfsProperty, reader.GetBoolean("USE_PDFS"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
