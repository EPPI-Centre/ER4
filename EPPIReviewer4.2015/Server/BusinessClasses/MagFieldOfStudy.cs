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
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Configuration;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagFieldOfStudy : BusinessBase<MagFieldOfStudy>
    {
#if SILVERLIGHT
    public MagFieldOfStudy() { }

        
#else
        private MagFieldOfStudy() { }
#endif

        private static PropertyInfo<Int64> FieldOfStudyIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("FieldOfStudyId", "FieldOfStudyId"));
        public Int64 FieldOfStudyId
        {
            get
            {
                return GetProperty(FieldOfStudyIdProperty);
            }
        }

        private static PropertyInfo<int> RankProperty = RegisterProperty<int>(new PropertyInfo<int>("Rank", "Rank", 0));
        public int Rank
        {
            get
            {
                return GetProperty(RankProperty);
            }
        }

        private static PropertyInfo<string> NormalizedNameProperty = RegisterProperty<string>(new PropertyInfo<string>("NormalizedName", "NormalizedName", string.Empty));
        public string NormalizedName
        {
            get
            {
                return GetProperty(NormalizedNameProperty);
            }
        }

        private static PropertyInfo<string> DisplayNameProperty = RegisterProperty<string>(new PropertyInfo<string>("DisplayName", "DisplayName", string.Empty));
        public string DisplayName
        {
            get
            {
                return GetProperty(DisplayNameProperty);
            }
        }

        private static PropertyInfo<string> MainTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("MainType", "MainType", string.Empty));
        public string MainType
        {
            get
            {
                return GetProperty(MainTypeProperty);
            }
        }

        private static PropertyInfo<int> LevelProperty = RegisterProperty<int>(new PropertyInfo<int>("Level", "Level", 0));
        public int Level
        {
            get
            {
                return GetProperty(LevelProperty);
            }
        }

        private static PropertyInfo<Int64> PaperCountProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("PaperCount", "PaperCount"));
        public Int64 PaperCount
        {
            get
            {
                return GetProperty(PaperCountProperty);
            }
        }

        private static PropertyInfo<Int64> CitationCountProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("CitationCount", "CitationCount"));
        public Int64 CitationCount
        {
            get
            {
                return GetProperty(CitationCountProperty);
            }
        }

        private static PropertyInfo<SmartDate> CreatedDateProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("CreatedDate", "CreatedDate"));
        public SmartDate CreatedDate
        {
            get
            {
                return GetProperty(CreatedDateProperty);
            }
        }
        /*
        private static PropertyInfo<decimal> ScoreProperty = RegisterProperty<decimal>(new PropertyInfo<decimal>("Score", "Score"));
        public decimal Score
        {
            get
            {
                return GetProperty(ScoreProperty);
            }
        }
        */
        private static PropertyInfo<int> num_timesProperty = RegisterProperty<int>(new PropertyInfo<int>("num_times", "num_times"));
        public int num_times
        {
            get
            {
                return GetProperty(num_timesProperty);
            }
        }

        public string ExternalMagLink
        {
            get
            {
                return "https://academic.microsoft.com/topic/" + FieldOfStudyId.ToString();
            }
        }

        /*
        public static readonly PropertyInfo<MagFieldOfStudyList> CitationsProperty = RegisterProperty<MagFieldOfStudyList>(new PropertyInfo<MagFieldOfStudyList>("Citations", "Citations"));
        public MagFieldOfStudyList Citations
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

        public static readonly PropertyInfo<MagFieldOfStudyList> CitedByProperty = RegisterProperty<MagFieldOfStudyList>(new PropertyInfo<MagFieldOfStudyList>("CitedBy", "CitedBy"));
        public MagFieldOfStudyList CitedBy
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

        public static readonly PropertyInfo<MagFieldOfStudyList> RecommendedProperty = RegisterProperty<MagFieldOfStudyList>(new PropertyInfo<MagFieldOfStudyList>("Recommended", "Recommended"));
        public MagFieldOfStudyList Recommended
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

        public static readonly PropertyInfo<MagFieldOfStudyList> RecommendedByProperty = RegisterProperty<MagFieldOfStudyList>(new PropertyInfo<MagFieldOfStudyList>("RecommendedBy", "RecommendedBy"));
        public MagFieldOfStudyList RecommendedBy
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
            DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();
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
            MagFieldOfStudyListSelectionCriteria sc = new BusinessClasses.MagFieldOfStudyListSelectionCriteria();
            sc.MagFieldOfStudyId = this.FieldOfStudyId;
            sc.ListType = listType;
            dp.BeginFetch(sc);
        }
        */



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagFieldOfStudy), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagFieldOfStudy), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagFieldOfStudy), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagFieldOfStudy), canRead);

        //    //AuthorizationRules.AllowRead(MagFieldOfStudyIdProperty, canRead);
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
                using (SqlCommand command = new SqlCommand("st_MagFieldOfStudyInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@MagFieldOfStudy_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MagFieldOfStudy_DETAIL", ReadProperty(DetailProperty)));
                    SqlParameter par = new SqlParameter("@NEW_MagFieldOfStudy_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_MagFieldOfStudy_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MFieldOfStudyIdProperty, command.Parameters["@NEW_MagFieldOfStudy_ID"].Value);
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
                using (SqlCommand command = new SqlCommand("st_MagFieldOfStudyUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagFieldOfStudy_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MagFieldOfStudy_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@MagFieldOfStudy_ID", ReadProperty(MagFieldOfStudyIdProperty)));
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
                using (SqlCommand command = new SqlCommand("st_MagFieldOfStudyDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagFieldOfStudy_ID", ReadProperty(MagFieldOfStudyIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        protected void DataPortal_Fetch(SingleCriteria<MagFieldOfStudy, Int64> criteria) // used to return a specific FieldOfStudy
        {
            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_FieldOfStudy", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@FieldOfStudyId", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<Int64>(FieldOfStudyIdProperty, reader.GetInt64("FieldOfStudyId"));
                            LoadProperty<string>(DisplayNameProperty, reader.GetString("DisplayName"));
                            LoadProperty<Int32>(num_timesProperty, reader.GetInt32("n_papers"));
                            LoadProperty<Int64>(PaperCountProperty, reader.GetInt64("PaperCount"));
                            //LoadProperty<decimal>(ScoreProperty, Convert.ToDecimal(reader.GetFloat("sum_similarity")));
                        }
                    }
                }
                connection.Close();
            }
            */
        }

        internal static MagFieldOfStudy GetMagFieldOfStudy(FieldOfStudyMakes fos)
        {
            MagFieldOfStudy returnValue = new MagFieldOfStudy();
            /*
            returnValue.LoadProperty<Int64>(FieldOfStudyIdProperty, reader.GetInt64("FieldOfStudyId"));
            returnValue.LoadProperty<string>(DisplayNameProperty, reader.GetString("DisplayName"));
            returnValue.LoadProperty<Int32>(num_timesProperty, reader.GetInt32("n_papers"));
            returnValue.LoadProperty<Int64>(PaperCountProperty, reader.GetInt64("PaperCount"));
            //returnValue.LoadProperty<decimal>(SimilarityScoreProperty, Convert.ToDecimal(reader.GetFloat("sum_similarity"))); // Getting the float data type into c# is a pain
            */
            returnValue.LoadProperty<Int64>(FieldOfStudyIdProperty, fos.Id);
            returnValue.LoadProperty<string>(DisplayNameProperty, fos.DFN);
            returnValue.LoadProperty<Int32>(num_timesProperty, fos.CC);
            //returnValue.LoadProperty<Int64>(PaperCountProperty, fos.PC);

            returnValue.MarkOld();
            return returnValue;
        }

        internal static MagFieldOfStudy GetMagFieldOfStudyRelationship(FieldOfStudyRelationshipMakes fosrm)
        {
            MagFieldOfStudy returnValue = new MagFieldOfStudy();
            returnValue.LoadProperty<Int64>(FieldOfStudyIdProperty, fosrm.FId);
            returnValue.LoadProperty<string>(DisplayNameProperty, fosrm.FN);

            returnValue.MarkOld();
            return returnValue;
        }

        internal static MagFieldOfStudy GetMagFieldOfStudyFromPaperMakesFieldOfStudy(PaperMakesFieldOfStudy pmfos)
        {
            MagFieldOfStudy returnValue = new MagFieldOfStudy();
            /*
            returnValue.LoadProperty<Int64>(FieldOfStudyIdProperty, reader.GetInt64("FieldOfStudyId"));
            returnValue.LoadProperty<string>(DisplayNameProperty, reader.GetString("DisplayName"));
            returnValue.LoadProperty<Int32>(num_timesProperty, reader.GetInt32("n_papers"));
            returnValue.LoadProperty<Int64>(PaperCountProperty, reader.GetInt64("PaperCount"));
            //returnValue.LoadProperty<decimal>(SimilarityScoreProperty, Convert.ToDecimal(reader.GetFloat("sum_similarity"))); // Getting the float data type into c# is a pain
            */
            returnValue.LoadProperty<Int64>(FieldOfStudyIdProperty, pmfos.FId);
            returnValue.LoadProperty<string>(DisplayNameProperty, pmfos.DFN);
            //returnValue.LoadProperty<Int32>(num_timesProperty, pmfos.c);
            //returnValue.LoadProperty<Int64>(PaperCountProperty, fos.PC);

            returnValue.MarkOld();
            return returnValue;
        }

        public static FieldOfStudyMakes GetPaperMakesFieldOfStudy(Int64 FosId)
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            string responseText = "";
            WebRequest request = WebRequest.Create(ConfigurationManager.AppSettings["AzureMAKESBaseURL"] + @"?expr=Id=" +
                FosId.ToString() + @"&attributes=Id,CC,DFN,ECC,FL,FN,FC.FId,FC.FN,FP.FId,FP.FN");
            WebResponse response = request.GetResponse();
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader sreader = new StreamReader(dataStream);
                responseText = sreader.ReadToEnd();
            }
            response.Close();
            var respJson = JsonConvert.DeserializeObject<MakesResponse>(responseText, jsonsettings);
            if (respJson != null && respJson.entities != null && respJson.entities.Count > 0)
            {
                return respJson.entities[0];
            }
            else
                return null;
        }

#endif
    }
}
