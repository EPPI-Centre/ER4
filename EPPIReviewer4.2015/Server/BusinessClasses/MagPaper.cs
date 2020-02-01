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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;

#if !SILVERLIGHT
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
        public static void GetMagPaper(Int64 PaperId, EventHandler<DataPortalResult<MagPaper>> handler)
        {
            DataPortal<MagPaper> dp = new DataPortal<MagPaper>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<MagPaper, Int64>(PaperId));
        }

        private static PropertyInfo<string> ExternalMagLinkProperty = RegisterProperty<string>(new PropertyInfo<string>("ExternalMagLink", "ExternalMagLink", string.Empty));
        public string ExternalMagLink()
        {
            return "https://academic.microsoft.com/paper/" + PaperId.ToString();
        }

        private static PropertyInfo<string> FullRecordProperty = RegisterProperty<string>(new PropertyInfo<string>("FullRecord", "FullRecord", string.Empty));
        public string FullRecord
        {
            get
            {
                return Authors + " (" + Year.ToString() + ") " + OriginalTitle + ". " + Journal + ". " + Volume.ToString() + " (" + Issue + ") " + FirstPage + "-" + LastPage;
            }
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

        private static PropertyInfo<Int64> JournalIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("JournalId", "JournalId"));
        public Int64 JournalId
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

        private static PropertyInfo<Int64> ConferenceSeriesIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ConferenceSeriesId", "ConferenceSeriesId"));
        public Int64 ConferenceSeriesId
        {
            get
            {
                return GetProperty(ConferenceSeriesIdProperty);
            }
        }

        private static PropertyInfo<Int64> ConferenceInstanceIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ConferenceInstanceId", "ConferenceInstanceId"));
        public Int64 ConferenceInstanceId
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
                return (GetProperty(VolumeProperty) != null) ? GetProperty(VolumeProperty) : "";
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

        private static PropertyInfo<Int64> ReferenceCountProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ReferenceCount", "ReferenceCount"));
        public Int64 ReferenceCount
        {
            get
            {
                return GetProperty(ReferenceCountProperty);
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

        private static PropertyInfo<int> EstimatedCitationCountProperty = RegisterProperty<int>(new PropertyInfo<int>("EstimatedCitationCount", "EstimatedCitationCount"));
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

        private static PropertyInfo<string> URLsProperty = RegisterProperty<string>(new PropertyInfo<string>("URLs", "URLs", string.Empty));
        public string URLs
        {
            get
            {
                return GetProperty(URLsProperty);
            }
        }

        private static PropertyInfo<Int64> LinkedITEM_IDProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("LinkedITEM_ID", "LinkedITEM_ID"));
        public Int64 LinkedITEM_ID
        {
            get
            {
                return GetProperty(LinkedITEM_IDProperty);
            }
        }

        private static PropertyInfo<bool> IsSelectedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsSelected", "IsSelected", false));
        public bool IsSelected
        {
            get
            {
                return GetProperty(IsSelectedProperty);
            }
            set
            {
                SetProperty(IsSelectedProperty, value);
                SetProperty(CanBeSelectedProperty,
                    IsSelected && LinkedITEM_ID == 0 ? "Unselect" :
                    !IsSelected && LinkedITEM_ID == 0 ? "Select" :
                    "In review");
            }
        }

        private static PropertyInfo<string> CanBeSelectedProperty = RegisterProperty<string>(new PropertyInfo<string>("CanBeSelected", "CanBeSelected", string.Empty));
        public string CanBeSelected
        {
            get
            {
                return GetProperty(CanBeSelectedProperty);
            }
            set
            {
                SetProperty(CanBeSelectedProperty, value);
            }
        }

        private static PropertyInfo<string> AbstractProperty = RegisterProperty<string>(new PropertyInfo<string>("Abstract", "Abstract", string.Empty));
        public string Abstract
        {
            get
            {
                return GetProperty(AbstractProperty);
            }
        }

        private static PropertyInfo<double> AutoMatchScoreProperty = RegisterProperty<double>(new PropertyInfo<double>("AutoMatchScore", "AutoMatchScore"));
        public double AutoMatchScore
        {
            get
            {
                return GetProperty(AutoMatchScoreProperty);
            }
        }

        private static PropertyInfo<bool> ManualTrueMatchProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ManualTrueMatch", "ManualTrueMatch", false));
        public bool ManualTrueMatch
        {
            get
            {
                return GetProperty(ManualTrueMatchProperty);
            }
            set
            {
                SetProperty(ManualTrueMatchProperty, value);
            }
        }

        private static PropertyInfo<bool> ManualFalseMatchProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ManualFalseMatch", "ManualFalseMatch", false));
        public bool ManualFalseMatch
        {
            get
            {
                return GetProperty(ManualFalseMatchProperty);
            }
            set
            {
                SetProperty(ManualFalseMatchProperty, value);
            }
        }

        private static PropertyInfo<string> FindOnWebProperty = RegisterProperty<string>(new PropertyInfo<string>("FindOnWeb", "FindOnWeb", string.Empty));
        public string FindOnWeb
        {
            get
            {
                return "http://academic.microsoft.com/paper/" + this.PaperId.ToString();
            }
        }

        


        /*

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
        
        public void GetRelatedPaperList(string listType)
        {
            DataPortal<MagPaperList> dp = new DataPortal<MagPaperList>();
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
            MagPaperListSelectionCriteria sc = new BusinessClasses.MagPaperListSelectionCriteria();
            sc.MagPaperId = this.PaperId;
            sc.ListType = listType;
            dp.BeginFetch(sc);
        }
        */



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
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemMatchedPaperUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(LinkedITEM_IDProperty)));
                    command.Parameters.Add(new SqlParameter("@PaperId", ReadProperty(PaperIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ManualTrueMatch", ReadProperty(ManualTrueMatchProperty)));
                    command.Parameters.Add(new SqlParameter("@ManualFalseMatch", ReadProperty(ManualFalseMatchProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
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

        protected void DataPortal_Fetch(SingleCriteria<MagPaper, Int64> criteria) // used to return a specific Paper
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_Paper", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<Int64>(PaperIdProperty, reader.GetInt64("PaperID"));
                            LoadProperty<string>(DOIProperty, reader.GetString("DOI"));
                            LoadProperty<string>(DocTypeProperty, reader.GetString("DocType"));
                            LoadProperty<string>(PaperTitleProperty, reader.GetString("PaperTitle"));
                            LoadProperty<string>(OriginalTitleProperty, reader.GetString("OriginalTitle"));
                            LoadProperty<string>(BookTitleProperty, reader.GetString("BookTitle"));
                            LoadProperty<Int32>(YearProperty, reader.GetInt32("Year"));
                            LoadProperty<SmartDate>(DateProperty, reader.GetSmartDate("Date"));
                            LoadProperty<string>(PublisherProperty, reader.GetString("Publisher"));
                            LoadProperty<Int64>(JournalIdProperty, reader.GetInt64("JournalId"));
                            LoadProperty<string>(JournalProperty, reader.GetString("DisplayName"));
                            LoadProperty<Int64>(ConferenceSeriesIdProperty, reader.GetInt64("ConferenceSeriesId"));
                            LoadProperty<Int64>(ConferenceInstanceIdProperty, reader.GetInt64("ConferenceInstanceId"));
                            LoadProperty<string>(VolumeProperty, reader.GetString("Volume"));
                            LoadProperty<string>(FirstPageProperty, reader.GetString("FirstPage"));
                            LoadProperty<string>(LastPageProperty, reader.GetString("LastPage"));
                            LoadProperty<Int64>(ReferenceCountProperty, reader.GetInt64("ReferenceCount"));
                            LoadProperty<Int64>(CitationCountProperty, reader.GetInt64("CitationCount"));
                            LoadProperty<int>(EstimatedCitationCountProperty, reader.GetInt32("EstimatedCitationCount"));
                            LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CreatedDate"));
                            LoadProperty<string>(AuthorsProperty, reader.GetString("Authors"));
                            LoadProperty<Int64>(LinkedITEM_IDProperty, reader.GetInt64("ITEM_ID"));
                            LoadProperty<string>(AbstractProperty, ReconstructInvertedAbstract(reader.GetString("IndexedAbstract")));
                            LoadProperty<string>(URLsProperty, reader.GetString("URLs"));
                            LoadProperty<bool>(ManualTrueMatchProperty, reader.GetBoolean("ManualTrueMatch"));
                            LoadProperty<bool>(ManualFalseMatchProperty, reader.GetBoolean("ManualFalseMatch"));
                            LoadProperty<double>(AutoMatchScoreProperty, reader.GetDouble("AutoMatchScore"));
                        }
                    }
                }
                connection.Close();
            }
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
            returnValue.LoadProperty<Int64>(JournalIdProperty, reader.GetInt64("JournalId"));
            returnValue.LoadProperty<string>(JournalProperty, reader.GetString("DisplayName"));
            returnValue.LoadProperty<Int64>(ConferenceSeriesIdProperty, reader.GetInt64("ConferenceSeriesId"));
            returnValue.LoadProperty<Int64>(ConferenceInstanceIdProperty, reader.GetInt64("ConferenceInstanceId"));
            returnValue.LoadProperty<string>(VolumeProperty, reader.GetString("Volume"));
            returnValue.LoadProperty<string>(FirstPageProperty, reader.GetString("FirstPage"));
            returnValue.LoadProperty<string>(LastPageProperty, reader.GetString("LastPage"));
            returnValue.LoadProperty<Int64>(ReferenceCountProperty, reader.GetInt64("ReferenceCount"));
            returnValue.LoadProperty<Int64>(CitationCountProperty, reader.GetInt64("CitationCount"));
            returnValue.LoadProperty<int>(EstimatedCitationCountProperty, reader.GetInt32("EstimatedCitationCount"));
            returnValue.LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CreatedDate"));
            returnValue.LoadProperty<string>(AuthorsProperty, reader.GetString("Authors"));
            returnValue.LoadProperty<Int64>(LinkedITEM_IDProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<string>(AbstractProperty, ReconstructInvertedAbstract(reader.GetString("IndexedAbstract")));
            returnValue.LoadProperty<string>(URLsProperty, reader.GetString("URLs"));
            returnValue.LoadProperty<bool>(ManualTrueMatchProperty, reader.GetBoolean("ManualTrueMatch"));
            returnValue.LoadProperty<bool>(ManualFalseMatchProperty, reader.GetBoolean("ManualFalseMatch"));
            returnValue.LoadProperty<double>(AutoMatchScoreProperty, reader.GetDouble("AutoMatchScore"));

            returnValue.MarkOld();
            return returnValue;
        }

        internal static MagPaper GetMagPaper(PaperMakes pm)
        {
            MagPaper returnValue = new MagPaper();
            returnValue.LoadProperty<Int64>(PaperIdProperty, pm.Id);
            returnValue.LoadProperty<string>(DOIProperty, pm.DOI);
            returnValue.LoadProperty<string>(DocTypeProperty, pm.Pt);
            returnValue.LoadProperty<string>(PaperTitleProperty, pm.Ti);
            returnValue.LoadProperty<string>(OriginalTitleProperty, pm.Ti);
            //returnValue.LoadProperty<string>(BookTitleProperty, reader.GetString("BookTitle"));
            returnValue.LoadProperty<Int32>(YearProperty, pm.Y);
            returnValue.LoadProperty<SmartDate>(DateProperty, pm.D);
            returnValue.LoadProperty<string>(PublisherProperty, pm.PB);
            returnValue.LoadProperty<Int64>(JournalIdProperty, pm.JJId);
            returnValue.LoadProperty<string>(JournalProperty, pm.JJN);
            //returnValue.LoadProperty<Int64>(ConferenceSeriesIdProperty, );
            //returnValue.LoadProperty<Int64>(ConferenceInstanceIdProperty, reader.GetInt64("ConferenceInstanceId"));
            returnValue.LoadProperty<string>(VolumeProperty, pm.V);
            returnValue.LoadProperty<string>(FirstPageProperty, pm.FP);
            returnValue.LoadProperty<string>(LastPageProperty, pm.LP);
            //returnValue.LoadProperty<Int64>(ReferenceCountProperty, pm.RId.Count);
            //returnValue.LoadProperty<Int64>(CitationCountProperty, reader.GetInt64("CitationCount"));
            //returnValue.LoadProperty<int>(EstimatedCitationCountProperty, reader.GetInt32("EstimatedCitationCount"));
            //returnValue.LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CreatedDate"));
            returnValue.LoadProperty<string>(AuthorsProperty, pm.AADAuN);
            //returnValue.LoadProperty<Int64>(LinkedITEM_IDProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<string>(AbstractProperty, ReconstructInvertedAbstract(pm.IA));
            //returnValue.LoadProperty<string>(URLsProperty, pm.S);
            //returnValue.LoadProperty<bool>(ManualTrueMatchProperty, reader.GetBoolean("ManualTrueMatch"));
            //returnValue.LoadProperty<bool>(ManualFalseMatchProperty, reader.GetBoolean("ManualFalseMatch"));
            //returnValue.LoadProperty<double>(AutoMatchScoreProperty, reader.GetDouble("AutoMatchScore"));

            returnValue.MarkOld();
            return returnValue;
        }

        internal static MagPaper GetMagPaper(PaperAzureSearch pm, SafeDataReader reader)
        {
            TextInfo myTI = new CultureInfo("en-GB", false).TextInfo;
            MagPaper returnValue = new MagPaper();
            returnValue.LoadProperty<Int64>(PaperIdProperty, pm.id);
            returnValue.LoadProperty<string>(DOIProperty, pm.doi);
            //returnValue.LoadProperty<string>(DocTypeProperty, pm.Pt);
            returnValue.LoadProperty<string>(PaperTitleProperty, myTI.ToTitleCase(pm.title == null ? "": pm.title));
            returnValue.LoadProperty<string>(OriginalTitleProperty, myTI.ToTitleCase(pm.title == null ? "": pm.title));
            //returnValue.LoadProperty<string>(BookTitleProperty, reader.GetString("BookTitle"));
            returnValue.LoadProperty<Int32>(YearProperty, pm.year);
            //returnValue.LoadProperty<SmartDate>(DateProperty, pm.D);
            //returnValue.LoadProperty<string>(PublisherProperty, pm.PB);
            //returnValue.LoadProperty<Int64>(JournalIdProperty, pm.JJId);
            returnValue.LoadProperty<string>(JournalProperty, myTI.ToTitleCase(pm.journal == null ? "": pm.journal));
            //returnValue.LoadProperty<Int64>(ConferenceSeriesIdProperty, );
            //returnValue.LoadProperty<Int64>(ConferenceInstanceIdProperty, reader.GetInt64("ConferenceInstanceId"));
            returnValue.LoadProperty<string>(VolumeProperty, pm.volume);
            returnValue.LoadProperty<string>(IssueProperty, pm.issue);
            returnValue.LoadProperty<string>(FirstPageProperty, pm.first_page);
            returnValue.LoadProperty<string>(LastPageProperty, pm.last_page);
            //returnValue.LoadProperty<Int64>(ReferenceCountProperty, pm.RId.Count);
            //returnValue.LoadProperty<Int64>(CitationCountProperty, reader.GetInt64("CitationCount"));
            //returnValue.LoadProperty<int>(EstimatedCitationCountProperty, reader.GetInt32("EstimatedCitationCount"));
            //returnValue.LoadProperty<SmartDate>(CreatedDateProperty, reader.GetSmartDate("CreatedDate"));
            string tmp = "";
            foreach (string s in pm.authors)
            {
                if (tmp == "")
                {
                    tmp = s;
                }
                else
                {
                    tmp += ", " + s;
                }
            }
            returnValue.LoadProperty<string>(AuthorsProperty, myTI.ToTitleCase(tmp));
            returnValue.LoadProperty<Int64>(LinkedITEM_IDProperty, reader.GetInt64("ITEM_ID"));
            //returnValue.LoadProperty<string>(AbstractProperty, ReconstructInvertedAbstract(pm.IA));
            //returnValue.LoadProperty<string>(URLsProperty, pm.S);
            returnValue.LoadProperty<bool>(ManualTrueMatchProperty, reader.GetBoolean("ManualTrueMatch"));
            returnValue.LoadProperty<bool>(ManualFalseMatchProperty, reader.GetBoolean("ManualFalseMatch"));
            returnValue.LoadProperty<double>(AutoMatchScoreProperty, reader.GetDouble("AutoMatchScore"));
            returnValue.MarkOld();
            return returnValue;
        }

        public static string ReconstructInvertedAbstract(string str)
        {
            try
            {
                var j = (JObject)JsonConvert.DeserializeObject(str);
                int indexLength = j["IndexLength"].ToObject<int>();
                Dictionary<string, int[]> invertedIndex = j["InvertedIndex"].ToObject<Dictionary<string, int[]>>();
                string[] abstractStr = new string[indexLength];
                foreach (var pair in invertedIndex)
                {
                    string word = pair.Key;
                    foreach (var index in pair.Value)
                    {
                        abstractStr[index] = word;
                    }
                }
                return String.Join(" ", abstractStr);
            }
            catch
            {
                return "";
            }
        }

#endif
    }
}
