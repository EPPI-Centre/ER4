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
    public class MagPaper : BusinessBase<MagPaper>
    {
#if SILVERLIGHT
    public MagPaper() { }

        
#else
        private MagPaper() { }
#endif

        public override string ToString()
        {
            return Authors + " (" + Year.ToString() + ") " + PaperTitle + ". " + Journal + ". " + Volume.ToString() + " (" + Issue + ") " + FirstPage + "-" + LastPage;
        }

        private static PropertyInfo<Int64> PaperIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("PaperId", "PaperId"));
        public Int64 PaperId
        {
            get
            {
                return GetProperty(PaperIdProperty);
            }
        }

        private static PropertyInfo<string> DOIProperty = RegisterProperty<string>(new PropertyInfo<string>("DOI", "DOI", string.Empty));
        public string DOI
        {
            get
            {
                return GetProperty(DOIProperty);
            }
        }

        private static PropertyInfo<string> DocTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("DocType", "DocType", string.Empty));
        public string DocType
        {
            get
            {
                return GetProperty(DocTypeProperty);
            }
        }

        private static PropertyInfo<string> PaperTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("PaperTitle", "PaperTitle", string.Empty));
        public string PaperTitle
        {
            get
            {
                return GetProperty(PaperTitleProperty);
            }
        }

        private static PropertyInfo<string> OriginalTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("OriginalTitle", "OriginalTitle", string.Empty));
        public string OriginalTitle
        {
            get
            {
                return GetProperty(OriginalTitleProperty);
            }
        }

        private static PropertyInfo<string> BookTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("BookTitle", "BookTitle", string.Empty));
        public string BookTitle
        {
            get
            {
                return GetProperty(BookTitleProperty);
            }
        }

        private static PropertyInfo<int> YearProperty = RegisterProperty<int>(new PropertyInfo<int>("Year", "Year", 0));
        public int Year
        {
            get
            {
                return GetProperty(YearProperty);
            }
        }

        private static PropertyInfo<SmartDate> DateProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("Date", "Date"));
        public SmartDate SmartDate
        {
            get
            {
                return GetProperty(DateProperty);
            }
        }

        private static PropertyInfo<string> PublisherProperty = RegisterProperty<string>(new PropertyInfo<string>("Publisher", "Publisher", string.Empty));
        public string Publisher
        {
            get
            {
                return GetProperty(PublisherProperty);
            }
        }

        private static PropertyInfo<int> JournalIdProperty = RegisterProperty<int>(new PropertyInfo<int>("JournalId", "JournalId", 0));
        public int JournalId
        {
            get
            {
                return GetProperty(JournalIdProperty);
            }
        }

        private static PropertyInfo<string> JournalProperty = RegisterProperty<string>(new PropertyInfo<string>("Journal", "Journal", string.Empty));
        public string Journal
        {
            get
            {
                return GetProperty(JournalProperty);
            }
        }

        private static PropertyInfo<int> ConferenceSeriesIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ConferenceSeriesId", "ConferenceSeriesId", 0));
        public int ConferenceSeriesId
        {
            get
            {
                return GetProperty(ConferenceSeriesIdProperty);
            }
        }

        private static PropertyInfo<int> ConferenceInstanceIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ConferenceInstanceId", "ConferenceInstanceId", 0));
        public int ConferenceInstanceId
        {
            get
            {
                return GetProperty(ConferenceInstanceIdProperty);
            }
        }

        private static PropertyInfo<string> VolumeProperty = RegisterProperty<string>(new PropertyInfo<string>("Volume", "Volume", string.Empty));
        public string Volume
        {
            get
            {
                return GetProperty(VolumeProperty);
            }
        }

        private static PropertyInfo<string> IssueProperty = RegisterProperty<string>(new PropertyInfo<string>("Issue", "Issue", string.Empty));
        public string Issue
        {
            get
            {
                return GetProperty(IssueProperty);
            }
        }

        private static PropertyInfo<string> FirstPageProperty = RegisterProperty<string>(new PropertyInfo<string>("FirstPage", "FirstPage", string.Empty));
        public string FirstPage
        {
            get
            {
                return GetProperty(FirstPageProperty);
            }
        }

        private static PropertyInfo<string> LastPageProperty = RegisterProperty<string>(new PropertyInfo<string>("LastPage", "LastPage", string.Empty));
        public string LastPage
        {
            get
            {
                return GetProperty(LastPageProperty);
            }
        }

        private static PropertyInfo<int> ReferenceCountProperty = RegisterProperty<int>(new PropertyInfo<int>("ReferenceCount", "ReferenceCount", 0));
        public int ReferenceCount
        {
            get
            {
                return GetProperty(ReferenceCountProperty);
            }
        }

        private static PropertyInfo<int> CitationCountProperty = RegisterProperty<int>(new PropertyInfo<int>("CitationCount", "CitationCount", 0));
        public int CitationCount
        {
            get
            {
                return GetProperty(CitationCountProperty);
            }
        }

        private static PropertyInfo<int> EstimatedCitationCountProperty = RegisterProperty<int>(new PropertyInfo<int>("EstimatedCitationCount", "EstimatedCitationCount", 0));
        public int EstimatedCitationCount
        {
            get
            {
                return GetProperty(EstimatedCitationCountProperty);
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

        private static PropertyInfo<string> AuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("Authors", "Authors", string.Empty));
        public string Authors
        {
            get
            {
                return GetProperty(AuthorsProperty);
            }
        }

        public static readonly PropertyInfo<MagPaperList> CitationsProperty = RegisterProperty<MagPaperList>(new PropertyInfo<MagPaperList>("Citations", "Citations"));
        public MagPaperList Citations
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

        public static readonly PropertyInfo<MagPaperList> CitedByProperty = RegisterProperty<MagPaperList>(new PropertyInfo<MagPaperList>("CitedBy", "CitedBy"));
        public MagPaperList CitedBy
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

        public static readonly PropertyInfo<MagPaperList> RecommendedProperty = RegisterProperty<MagPaperList>(new PropertyInfo<MagPaperList>("Recommended", "Recommended"));
        public MagPaperList Recommended
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

        public static readonly PropertyInfo<MagPaperList> RecommendedByProperty = RegisterProperty<MagPaperList>(new PropertyInfo<MagPaperList>("RecommendedBy", "RecommendedBy"));
        public MagPaperList RecommendedBy
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





        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagPaper), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagPaper), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagPaper), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagPaper), canRead);

        //    //AuthorizationRules.AllowRead(MagPaperIdProperty, canRead);
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
                using (SqlCommand command = new SqlCommand("st_MagPaperInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@MagPaper_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MagPaper_DETAIL", ReadProperty(DetailProperty)));
                    SqlParameter par = new SqlParameter("@NEW_MagPaper_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_MagPaper_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MPaperIdProperty, command.Parameters["@NEW_MagPaper_ID"].Value);
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
                using (SqlCommand command = new SqlCommand("st_MagPaperUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagPaper_NAME", ReadProperty(NameProperty)));
                    command.Parameters.Add(new SqlParameter("@MagPaper_DETAIL", ReadProperty(DetailProperty)));
                    command.Parameters.Add(new SqlParameter("@MagPaper_ID", ReadProperty(MagPaperIdProperty)));
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
                using (SqlCommand command = new SqlCommand("st_MagPaperDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagPaper_ID", ReadProperty(MagPaperIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            */
        }

        internal static MagPaper GetMagPaper(SafeDataReader reader)
        {
            MagPaper returnValue = new MagPaper();
            returnValue.LoadProperty<Int64>(PaperIdProperty, reader.GetInt64("PaperId"));
            returnValue.LoadProperty<string>(DOIProperty, reader.GetString("DOI"));
            returnValue.LoadProperty<string>(DocTypeProperty, reader.GetString("DocType"));
            returnValue.LoadProperty<string>(PaperTitleProperty, reader.GetString("PaperTitle"));
            returnValue.LoadProperty<string>(OriginalTitleProperty, reader.GetString("OriginalTitle"));
            returnValue.LoadProperty<string>(BookTitleProperty, reader.GetString("BookTitle"));
            returnValue.LoadProperty<Int32>(YearProperty, reader.GetInt32("Year"));
            returnValue.LoadProperty<SmartDate>(DateProperty, reader.GetSmartDate("Date"));
            returnValue.LoadProperty<string>(PublisherProperty, reader.GetString("Publisher"));
            returnValue.LoadProperty<Int32>(JournalIdProperty, reader.GetInt32("JournalId"));
            returnValue.LoadProperty<string>(JournalProperty, reader.GetString("Journal"));
            returnValue.LoadProperty<Int32>(ConferenceSeriesIdProperty, reader.GetInt32("ConferenceSeriesId"));
            returnValue.LoadProperty<Int32>(ConferenceInstanceIdProperty, reader.GetInt32("ConferenceInstanceId"));
            returnValue.LoadProperty<string>(VolumeProperty, reader.GetString("Volume"));
            returnValue.LoadProperty<string>(FirstPageProperty, reader.GetString("FirstPage"));
            returnValue.LoadProperty<string>(LastPageProperty, reader.GetString("LastPage"));
            returnValue.LoadProperty<Int32>(ReferenceCountProperty, reader.GetInt32("ReferenceCount"));
            returnValue.LoadProperty<Int32>(CitationCountProperty, reader.GetInt32("CitationCount"));
            returnValue.LoadProperty<Int32>(EstimatedCitationCountProperty, reader.GetInt32("EstimatedCitationCount"));
            returnValue.LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CreatedDate"));
            returnValue.LoadProperty<string>(AuthorsProperty, reader.GetString("Authors"));

            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
