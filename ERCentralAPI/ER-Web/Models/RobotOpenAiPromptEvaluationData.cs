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
    public class RobotOpenAiPromptEvaluationData : BusinessBase<RobotOpenAiPromptEvaluationData>
    {
#if SILVERLIGHT
    public RobotOpenAiPromptEvaluationData() { }

        
#else
        public RobotOpenAiPromptEvaluationData() { }
#endif


        public static readonly PropertyInfo<int> OpenAiPromptEvaluationDataIdProperty = RegisterProperty<int>(new PropertyInfo<int>("OpenAiPromptEvaluationDataId", "OpenAiPromptEvaluationDataId"));
        public int OpenAiPromptEvaluationDataId
        {
            get
            {
                return GetProperty(OpenAiPromptEvaluationDataIdProperty);
            }
            set
            {
                SetProperty(OpenAiPromptEvaluationDataIdProperty, value);
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
        public static readonly PropertyInfo<int> IterationProperty = RegisterProperty<int>(new PropertyInfo<int>("Iteration", "Iteration"));
        public int Iteration
        {
            get
            {
                return GetProperty(IterationProperty);
            }
            set
            {
                SetProperty(IterationProperty, value);
            }
        }
        public static readonly PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
            set
            {
                SetProperty(ItemIdProperty, value);
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
        public static readonly PropertyInfo<string> AdditionalTextProperty = RegisterProperty<string>(new PropertyInfo<string>("AdditionalText", "AdditionalText"));
        public string AdditionalText
        {
            get
            {
                return GetProperty(AdditionalTextProperty);
            }
            set
            {
                SetProperty(AdditionalTextProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> GoldStandardProperty = RegisterProperty<bool>(new PropertyInfo<bool>("GoldStandard", "GoldStandard"));
        public bool GoldStandard
        {
            get
            {
                return GetProperty(GoldStandardProperty);
            }
            set
            {
                SetProperty(GoldStandardProperty, value);
            }
        }


#if !SILVERLIGHT

        protected void DataPortal_Fetch(SingleCriteria<PriorityScreeningSimulation, string> criteria) // used to return a specific record
        {

        }
        protected override void DataPortal_Insert()
        {
            
        }

        protected override void DataPortal_Update()
        {

        }

        protected override void DataPortal_DeleteSelf()
        {

        }

        protected void DataPortal_Delete(SingleCriteria<RobotOpenAiPromptEvaluationData, string> criteria)
        {
            
        }

        internal static RobotOpenAiPromptEvaluationData GetRobotOpenAiPromptEvaluationData(SafeDataReader reader)
        {
            RobotOpenAiPromptEvaluationData returnValue = new RobotOpenAiPromptEvaluationData();
            returnValue.LoadProperty<Int32>(OpenAiPromptEvaluationIdProperty, reader.GetInt32("OPENAI_PROMPT_EVALUATION_ID"));
            returnValue.LoadProperty<Int32>(IterationProperty, reader.GetInt32("ITERATION"));
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<string>(AdditionalTextProperty, reader.GetString("ADDITIONAL_TEXT"));
            returnValue.LoadProperty<bool>(GoldStandardProperty, reader.GetBoolean("GOLD_STANDARD"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
