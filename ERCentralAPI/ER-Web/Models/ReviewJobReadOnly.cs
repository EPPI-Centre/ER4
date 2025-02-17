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
    public class ReviewJobReadOnly : ReadOnlyBase<ReviewJobReadOnly>
    {
        public ReviewJobReadOnly() { }

        public static readonly PropertyInfo<int> ReviewJobIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewJobId", "ReviewJobId"));
        public int ReviewJobId
        {
            get
            {
                return GetProperty(ReviewJobIdProperty);
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

        public static readonly PropertyInfo<string> JobTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("JobType", "JobType"));
        public string JobType
        {
            get
            {
                return GetProperty(JobTypeProperty);
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

        public static readonly PropertyInfo<int> SuccessProperty = RegisterProperty<int>(new PropertyInfo<int>("Success", "Success"));
        public int Success
        {
            get
            {
                return GetProperty(SuccessProperty);
            }
        }


        public static readonly PropertyInfo<string> JobMessageProperty = RegisterProperty<string>(new PropertyInfo<string>("JobMessage", "JobMessage"));
        public string JobMessage
        {
            get
            {
                return GetProperty(JobMessageProperty);
            }
        }


        public static readonly PropertyInfo<int> LengthInMinutesProperty = RegisterProperty<int>(new PropertyInfo<int>("LengthInMinutes", "LengthInMinutes"));
        public int LengthInMinutes
        {
            get
            {
                return GetProperty(LengthInMinutesProperty);
            }
        }
        public static readonly PropertyInfo<int> LengthInSecondsProperty = RegisterProperty<int>(new PropertyInfo<int>("LengthInSeconds", "LengthInSeconds"));
        public int LengthInSeconds
        {
            get
            {
                return GetProperty(LengthInSecondsProperty);
            }
        }

        public static readonly PropertyInfo<string> OwnerNameProperty = RegisterProperty<string>(new PropertyInfo<string>("OwnerName", "OwnerName"));
        public string OwnerName
        {
            get
            {
                return GetProperty(OwnerNameProperty);
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
            
            
        }

        private void Child_Fetch(SafeDataReader reader)
        {
            Child_FetchDetails(reader);
        }
        private void Child_FetchDetails(SafeDataReader reader)
        {
            LoadProperty<int>(ReviewJobIdProperty, reader.GetInt32("REVIEW_JOB_ID"));  
            LoadProperty<int>(JobOwnerIdProperty, reader.GetInt32("CONTACT_ID"));
            LoadProperty<DateTime>(CreatedProperty, reader.GetDateTime("START_TIME"));
            LoadProperty<DateTime>(UpdatedProperty, reader.GetDateTime("END_TIME"));
            LoadProperty<string>(StatusProperty, reader.GetString("CURRENT_STATE"));
            LoadProperty<int>(SuccessProperty, reader.GetInt32("SUCCESS"));
            LoadProperty<string>(JobMessageProperty, reader.GetString("JOB_MESSAGE"));
            LoadProperty<string>(JobTypeProperty, reader.GetString("JOB_TYPE"));
            LoadProperty<int>(LengthInMinutesProperty, reader.GetInt32("Len in Minutes"));
            LoadProperty<int>(LengthInSecondsProperty, reader.GetInt32("Len in Seconds"));
            LoadProperty<string>(OwnerNameProperty, reader.GetString("CONTACT_NAME"));
        }

#endif
    }
}
