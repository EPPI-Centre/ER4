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
    public class MagBrowseHistory : BusinessBase<MagBrowseHistory>
    {
#if SILVERLIGHT
    public MagBrowseHistory() { }

        
#else
        private MagBrowseHistory() { }
#endif

        private static PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
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

        private static PropertyInfo<string> BrowseTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("BrowseType", "BrowseType", string.Empty));
        public string BrowseType
        {
            get
            {
                return GetProperty(BrowseTypeProperty);
            }
            set
            {
                SetProperty(BrowseTypeProperty, value);
            }
        }

        private static PropertyInfo<Int64> PaperIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("PaperId", "PaperId"));
        public Int64 PaperId
        {
            get
            {
                return GetProperty(PaperIdProperty);
            }
            set
            {
                SetProperty(PaperIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> LinkedITEM_IDProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("LinkedITEM_ID", "LinkedITEM_ID"));
        public Int64 LinkedITEM_ID
        {
            get
            {
                return GetProperty(LinkedITEM_IDProperty);
            }
            set
            {
                SetProperty(LinkedITEM_IDProperty, value);
            }
        }

        private static PropertyInfo<string> PaperFullRecordProperty = RegisterProperty<string>(new PropertyInfo<string>("PaperFullRecord", "PaperFullRecord", string.Empty));
        public string PaperFullRecord
        {
            get
            {
                return GetProperty(PaperFullRecordProperty);
            }
            set
            {
                SetProperty(PaperFullRecordProperty, value);
            }
        }

        private static PropertyInfo<string> PaperAbstractProperty = RegisterProperty<string>(new PropertyInfo<string>("PaperAbstract", "PaperAbstract", string.Empty));
        public string PaperAbstract
        {
            get
            {
                return GetProperty(PaperAbstractProperty);
            }
            set
            {
                SetProperty(PaperAbstractProperty, value);
            }
        }

        private static PropertyInfo<string> URLsProperty = RegisterProperty<string>(new PropertyInfo<string>("URLs", "URLs", string.Empty));
        public string URLs
        {
            get
            {
                return GetProperty(URLsProperty);
            }
            set
            {
                SetProperty(URLsProperty, value);
            }
        }

        private static PropertyInfo<Int64> FieldOfStudyIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("FieldOfStudyId", "FieldOfStudyId"));
        public Int64 FieldOfStudyId
        {
            get
            {
                return GetProperty(FieldOfStudyIdProperty);
            }
            set
            {
                SetProperty(FieldOfStudyIdProperty, value);
            }
        }

        private static PropertyInfo<string> FieldOfStudyProperty = RegisterProperty<string>(new PropertyInfo<string>("FieldOfStudy", "FieldOfStudy", string.Empty));
        public string FieldOfStudy
        {
            get
            {
                return GetProperty(FieldOfStudyProperty);
            }
            set
            {
                SetProperty(FieldOfStudyProperty, value);
            }
        }

        private static PropertyInfo<Int64> ContactIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ContactId", "ContactId"));
        public Int64 ContactId
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

        private static PropertyInfo<SmartDate> DateBrowsedProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("DateBrowsed", "DateBrowsed"));
        public SmartDate DateBrowsed
        {
            get
            {
                return GetProperty(DateBrowsedProperty);
            }
            set
            {
                SetProperty(DateBrowsedProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeIdsProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeIds", "AttributeIds", string.Empty));
        public string AttributeIds
        {
            get
            {
                return GetProperty(AttributeIdsProperty);
            }
            set
            {
                SetProperty(AttributeIdsProperty, value);
            }
        }

        private static PropertyInfo<int> MagRelatedRunIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagRelatedRunId", "MagRelatedRunId", 0));
        public int MagRelatedRunId
        {
            get
            {
                return GetProperty(MagRelatedRunIdProperty);
            }
            set
            {
                SetProperty(MagRelatedRunIdProperty, value);
            }
        }



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagBrowseHistory), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagBrowseHistory), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagBrowseHistory), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagBrowseHistory), canRead);

        //    //AuthorizationRules.AllowRead(MagBrowseHistoryIdProperty, canRead);
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
            /*  Not implemented yet. I don't know whether this needs saving to the database or not?
             *  Using standard business objects in case we do need to
             
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagBrowseHistoryInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_PRIMARY", ReadProperty(ItemIdPrimaryProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_SECONDARY", ReadProperty(ItemIdSecondaryProperty)));
                    command.Parameters.Add(new SqlParameter("@LINK_DESCRIPTION", ReadProperty(DescriptionProperty)));
                    SqlParameter par = new SqlParameter("@NEW_ITEM_LINK_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_ITEM_LINK_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagBrowseHistoryIdProperty, command.Parameters["@NEW_ITEM_LINK_ID"].Value);
                }
                connection.Close();
            }
            */
        }

        protected override void DataPortal_Update()
        {
            /*  Not implemented yet
             *  
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagBrowseHistoryUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_LINK_ID", ReadProperty(MagBrowseHistoryIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_LINK_SECONDARY", ReadProperty(ItemIdSecondaryProperty)));
                    command.Parameters.Add(new SqlParameter("@LINK_DESCRIPTION", ReadProperty(DescriptionProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*  Not implemented yet
             *  
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagBrowseHistoryDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_LINK_ID", ReadProperty(MagBrowseHistoryIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        internal static MagBrowseHistory GetMagBrowseHistory(SafeDataReader reader)
        {
            MagBrowseHistory returnValue = new MagBrowseHistory();
            /*  Not implemented yet
             *  
            returnValue.LoadProperty<int>(MagBrowseHistoryIdProperty, reader.GetInt32("ITEM_LINK_ID"));
            returnValue.LoadProperty<Int64>(ItemIdPrimaryProperty, reader.GetInt64("ITEM_ID_PRIMARY"));
            returnValue.LoadProperty<Int64>(ItemIdSecondaryProperty, reader.GetInt64("ITEM_ID_SECONDARY"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.LoadProperty<string>(DescriptionProperty, reader.GetString("LINK_DESCRIPTION"));
            returnValue.MarkOld();
            
            */
            return returnValue;
        }

#endif

    }
}
