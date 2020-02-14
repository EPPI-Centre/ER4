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
    public class MagCurrentInfo : BusinessBase<MagCurrentInfo>
    {
#if SILVERLIGHT
    public MagCurrentInfo() { }

        
#else
        private MagCurrentInfo() { }
#endif

        

        private static PropertyInfo<string> CurrentAvailabilityProperty = RegisterProperty<string>(new PropertyInfo<string>("CurrentAvailability", "CurrentAvailability", string.Empty));
        public string CurrentAvailability
        {
            get
            {
                return GetProperty(CurrentAvailabilityProperty);
            }
        }
        
        private static PropertyInfo<SmartDate> LastUpdatedProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("LastUpdated", "LastUpdated"));
        public SmartDate LastUpdated
        {
            get
            {
                return GetProperty(LastUpdatedProperty);
            }
        }

        /*
        public static readonly PropertyInfo<MagCurrentInfoList> CitationsProperty = RegisterProperty<MagCurrentInfoList>(new PropertyInfo<MagCurrentInfoList>("Citations", "Citations"));
        public MagCurrentInfoList Citations
        {
            get
            {
                return GetProperty(CitationsProperty);
            }
            set
            {
                SetProperty(CitationsProperty, value);
            }
        }

        public static readonly PropertyInfo<MagCurrentInfoList> CitedByProperty = RegisterProperty<MagCurrentInfoList>(new PropertyInfo<MagCurrentInfoList>("CitedBy", "CitedBy"));
        public MagCurrentInfoList CitedBy
        {
            get
            {
                return GetProperty(CitedByProperty);
            }
            set
            {
                SetProperty(CitedByProperty, value);
            }
        }

        public static readonly PropertyInfo<MagCurrentInfoList> RecommendedProperty = RegisterProperty<MagCurrentInfoList>(new PropertyInfo<MagCurrentInfoList>("Recommended", "Recommended"));
        public MagCurrentInfoList Recommended
        {
            get
            {
                return GetProperty(RecommendedProperty);
            }
            set
            {
                SetProperty(RecommendedProperty, value);
            }
        }

        public static readonly PropertyInfo<MagCurrentInfoList> RecommendedByProperty = RegisterProperty<MagCurrentInfoList>(new PropertyInfo<MagCurrentInfoList>("RecommendedBy", "RecommendedBy"));
        public MagCurrentInfoList RecommendedBy
        {
            get
            {
                return GetProperty(RecommendedByProperty);
            }
            set
            {
                SetProperty(RecommendedByProperty, value);
            }
        }
        
        public void GetRelatedFieldOfStudyList(string listType)
        {
            DataPortal<MagCurrentInfoList> dp = new DataPortal<MagCurrentInfoList>();
            dp.FetchCompleted += (o, e2) =>
            {
                if (e2.Object != null)
                {
                    if (e2.Error == null)
                    {
                        this.Citations = e2.Object;
                        //this.MarkClean(); // don't want the object marked as 'dirty' just because it's loaded a new list
                    }
                }
                if (e2.Error != null)
                {
#if SILVERLIGHT
                    System.Windows.MessageBox.Show(e2.Error.Message);
#endif
                }
            };
            MagCurrentInfoListSelectionCriteria sc = new BusinessClasses.MagCurrentInfoListSelectionCriteria();
            sc.MagCurrentInfoId = this.FieldOfStudyId;
            sc.ListType = listType;
            dp.BeginFetch(sc);
        }
        */



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagCurrentInfo), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagCurrentInfo), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagCurrentInfo), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagCurrentInfo), canRead);

        //    //AuthorizationRules.AllowRead(MagCurrentInfoIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
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

        protected override void DataPortal_Insert()
        {
            /*
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfoInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@MagCurrentInfo_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MagCurrentInfo_DETAIL", ReadProperty(DetailProperty)));
                    SqlParameter par = new SqlParameter("@NEW_MagCurrentInfo_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_MagCurrentInfo_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MFieldOfStudyIdProperty, command.Parameters["@NEW_MagCurrentInfo_ID"].Value);
                }
                connection.Close();
            }
            */
        }

        protected override void DataPortal_Update()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfoUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagCurrentInfo_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MagCurrentInfo_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@MagCurrentInfo_ID", ReadProperty(MagCurrentInfoIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        protected override void DataPortal_DeleteSelf()
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfoDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagCurrentInfo_ID", ReadProperty(MagCurrentInfoIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        protected void DataPortal_Fetch()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfo", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<string>(CurrentAvailabilityProperty, reader.GetString("current_availability"));
                            LoadProperty<SmartDate>(LastUpdatedProperty, reader.GetSmartDate("current_version"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static MagCurrentInfo GetMagCurrentInfo(SafeDataReader reader)
        {
            MagCurrentInfo returnValue = new MagCurrentInfo();
            

            returnValue.MarkOld();
            return returnValue;
        }

#endif
    }
}
