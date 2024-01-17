
using BusinessLibrary.Security;
using Csla;
using Csla.Serialization;
using Csla.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if !SILVERLIGHT
using BusinessLibrary.Data;
using System.Data.SqlClient;
#endif


namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class FeedbackAndClientError : BusinessBase<FeedbackAndClientError>
    {
        public FeedbackAndClientError() { }
        public static FeedbackAndClientError CreateFeedbackAndClientError(int ContactId, string Context, bool IsError, string Message)
        {
            FeedbackAndClientError res = new FeedbackAndClientError(ContactId, Context, IsError, Message);
            return res;
        }
        private FeedbackAndClientError(int contactId, string context, bool isError, string message)
        {
            if (message.Length > 4000) message = message.Substring(0, 4000);
            LoadProperty(ContactIdProperty, contactId);
            LoadProperty(ContextProperty, context);
            LoadProperty(IsErrorProperty, isError);
            LoadProperty(MessageProperty, message);
        }
        public static readonly PropertyInfo<int> MessageIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MessageId", "MessageId"));
        public int MessageId
        {
            get
            {
                return GetProperty(MessageIdProperty);
            }
        }
        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId"));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
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
        public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName"));
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
            }
        }
        public static readonly PropertyInfo<string> ContactEmailProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactEmail", "ContactEmail"));
        public string ContactEmail
        {
            get
            {
                return GetProperty(ContactEmailProperty);
            }
        }
        public static readonly PropertyInfo<string> ContextProperty = RegisterProperty<string>(new PropertyInfo<string>("Context", "Context"));
        public string Context
        {
            get
            {
                return GetProperty(ContextProperty);
            }
        }

        public static readonly PropertyInfo<bool> IsErrorProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsError", "IsError"));
        public bool IsError
        {
            get
            {
                return GetProperty(IsErrorProperty);
            }
        }

        public static readonly PropertyInfo<string> MessageProperty = RegisterProperty<string>(new PropertyInfo<string>("Message", "Message"));
        public string Message
        {
            get
            {
                return GetProperty(MessageProperty);
            }
        }

        public static readonly PropertyInfo<SmartDate> DateCreatedProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateCreated", "DateCreated"));
        public SmartDate DateCreated
        {
            get
            {
                return GetProperty(DateCreatedProperty);
            }
        }
#if !SILVERLIGHT
        protected override void DataPortal_Insert()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_OnlineFeedbackCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ONLINE_FEEDBACK_ID", ReadProperty(MessageIdProperty)));
                    command.Parameters.Add(new SqlParameter("@CONTEXT", ReadProperty(ContextProperty)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));
                    command.Parameters.Add(new SqlParameter("@IS_ERROR", ReadProperty(IsErrorProperty)));
                    command.Parameters.Add(new SqlParameter("@MESSAGE", ReadProperty(MessageProperty)));
                    command.Parameters["@ONLINE_FEEDBACK_ID"].Direction = System.Data.ParameterDirection.Output;
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));

                    command.ExecuteNonQuery();
                    LoadProperty(MessageIdProperty, command.Parameters["@ONLINE_FEEDBACK_ID"].Value);
                    LoadProperty(DateCreatedProperty, DateTime.Now);
                }
                connection.Close();
            }
        }
        internal static FeedbackAndClientError GetFeedbackAndClientError(SafeDataReader reader)
        {
            //CONTACT_ID CONTACT_NAME ,[CONTEXT]
            //,[REVIEW_ID]
            //,[IS_ERROR]
            //,[MESSAGE]
            //,[DATE]
            FeedbackAndClientError returnValue = new FeedbackAndClientError();
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<string>(ContextProperty, reader.GetString("CONTEXT"));
            returnValue.LoadProperty<string>(MessageProperty, reader.GetString("MESSAGE"));
            returnValue.LoadProperty<string>(ContactEmailProperty, reader.GetString("EMAIL"));//ContactEmail
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<bool>(IsErrorProperty, reader.GetBoolean("IS_ERROR"));
            returnValue.LoadProperty<SmartDate>(DateCreatedProperty, reader.GetSmartDate("DATE"));
            return returnValue;
        }
#endif
    }
}
