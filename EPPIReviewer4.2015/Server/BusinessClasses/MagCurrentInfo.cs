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
        public static void GetMagCurrentInfo(EventHandler<DataPortalResult<MagCurrentInfo>> handler)
        {
            DataPortal<MagCurrentInfo> dp = new DataPortal<MagCurrentInfo>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public MagCurrentInfo() { }

        
#else
        private MagCurrentInfo() { }
#endif
        private static PropertyInfo<int> MagCurrentInfoIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagCurrentInfoId", "MagCurrentInfoId", 0));
        public int MagCurrentInfoId
        {
            get
            {
                return GetProperty(MagCurrentInfoIdProperty);
            }
        }

        private static PropertyInfo<string> MagVersionProperty = RegisterProperty<string>(new PropertyInfo<string>("MagVersion", "MagVersion", ""));
        public string MagVersion
        {
            get
            {
                return GetProperty(MagVersionProperty);
            }
        }

        private static PropertyInfo<bool> MatchingAvailableProperty = RegisterProperty<bool>(new PropertyInfo<bool>("MatchingAvailable", "MatchingAvailable", true));
        public bool MatchingAvailable
        {
            get
            {
                return GetProperty(MatchingAvailableProperty);
            }
        }

        private static PropertyInfo<bool> MagOnlineProperty = RegisterProperty<bool>(new PropertyInfo<bool>("MagOnline", "MagOnline", true));
        public bool MagOnline
        {
            get
            {
                return GetProperty(MagOnlineProperty);
            }
        }

        private static PropertyInfo<DateTime> WhenLiveProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("WhenLive", "WhenLive"));
        public DateTime WhenLive
        {
            get
            {
                return GetProperty(WhenLiveProperty);
            }
        }

        private static PropertyInfo<string> MakesEndPointProperty = RegisterProperty<string>(new PropertyInfo<string>("MakesEndPoint", "MakesEndPoint", ""));
        public string MakesEndPoint
        {
            get
            {
                return GetProperty(MakesEndPointProperty);
            }
        }

        private static PropertyInfo<string> MakesDeploymentStatusProperty = RegisterProperty<string>(new PropertyInfo<string>("MakesDeploymentStatus", "MakesDeploymentStatus", ""));
        public string MakesDeploymentStatus
        {
            get
            {
                return GetProperty(MakesDeploymentStatusProperty);
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
                    command.Parameters.Add(new SqlParameter("@MAKES_DEPLOYMENT_STATUS", "LIVE")); // only need live info for client side
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<int>(MagCurrentInfoIdProperty, reader.GetInt32("MAG_CURRENT_INFO_ID"));
                            LoadProperty<string>(MagVersionProperty, reader.GetString("MAG_VERSION"));
                            LoadProperty<DateTime>(WhenLiveProperty, reader.GetDateTime("WHEN_LIVE"));
                            LoadProperty<bool>(MatchingAvailableProperty, reader.GetBoolean("MATCHING_AVAILABLE"));
                            LoadProperty<bool>(MagOnlineProperty, reader.GetBoolean("MAG_ONLINE"));
                            //LoadProperty<string>(MakesEndPointProperty, reader.GetString("MAKES_ENDPOINT")); // don't need to send this information back to the client (and probably shouldn't)
                            //LoadProperty<string>(MakesDeploymentStatusProperty, reader.GetString("MAKES_DEPLOYMENT_STATUS"));
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static MagCurrentInfo GetMagCurrentInfo(SafeDataReader reader) // not sure this is needed??
        {
            MagCurrentInfo returnValue = new MagCurrentInfo();
            returnValue.MarkOld();
            return returnValue;
        }

        public static MagCurrentInfo GetMagCurrentInfoServerSide(string MakesDeploymentStatus)
        {
            MagCurrentInfo returnValue = new MagCurrentInfo();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfo", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAKES_DEPLOYMENT_STATUS", MakesDeploymentStatus));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            returnValue.LoadProperty<int>(MagCurrentInfoIdProperty, reader.GetInt32("MAG_CURRENT_INFO_ID"));
                            returnValue.LoadProperty<string>(MagVersionProperty, reader.GetString("MAG_VERSION"));
                            returnValue.LoadProperty<DateTime>(WhenLiveProperty, reader.GetDateTime("WHEN_LIVE"));
                            returnValue.LoadProperty<bool>(MatchingAvailableProperty, reader.GetBoolean("MATCHING_AVAILABLE"));
                            returnValue.LoadProperty<bool>(MagOnlineProperty, reader.GetBoolean("MAG_ONLINE"));
                            returnValue.LoadProperty<string>(MakesEndPointProperty, reader.GetString("MAKES_ENDPOINT"));
                            returnValue.LoadProperty<string>(MakesDeploymentStatusProperty, reader.GetString("MAKES_DEPLOYMENT_STATUS"));
                        }
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
