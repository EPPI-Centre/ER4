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
using System.Net;
using System.Configuration;
using System.IO;
//using Csla.Configuration;

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
        public MagPaper() { }
#endif

        
        

        public static void GetMagPaper(Int64 PaperId, EventHandler<DataPortalResult<MagPaper>> handler)
        {
            DataPortal<MagPaper> dp = new DataPortal<MagPaper>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<MagPaper, Int64>(PaperId));
        }

        public static readonly PropertyInfo<string> ExternalMagLinkProperty = RegisterProperty<string>(new PropertyInfo<string>("ExternalMagLink", "ExternalMagLink", string.Empty));
        public string ExternalMagLink()
        {
            return "https://academic.microsoft.com/paper/" + PaperId.ToString();
        }

        public static readonly PropertyInfo<string> FullRecordProperty = RegisterProperty<string>(new PropertyInfo<string>("FullRecord", "FullRecord", string.Empty));
        public string FullRecord
        {
            get
            {
                return Authors + " (" + Year.ToString() + ") " + OriginalTitle + ". " + Journal + ". " +
                    Volume.ToString() + (Issue == "" || Issue == null ? "" :  " (" + Issue + ") ") + FirstPage + "-" + LastPage +
                    (DOI == "" ? "" : ". DOI: " + DOI);
            }
        }
        public string ShortRecord
        {
            get
            {
                string shortednedAuthors = Authors;
                if (Authors.Length > 25)
                {
                    shortednedAuthors = Authors.Substring(0, 25);
                }
                return shortednedAuthors  + "... (" + Year.ToString() + ") " + OriginalTitle + ". " + Journal + ". " +
                    Volume.ToString() + (Issue == "" || Issue == null ? "" : " (" + Issue + ") ") + FirstPage + "-" + LastPage;
            }
        }
        public static readonly PropertyInfo<Int64> PaperIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("PaperId", "PaperId"));
        public Int64 PaperId
        {
            get
            {
                return GetProperty(PaperIdProperty);
            }
        }

        public static readonly PropertyInfo<string> DOIProperty = RegisterProperty<string>(new PropertyInfo<string>("DOI", "DOI", string.Empty));
        public string DOI
        {
            get
            {
                return GetProperty(DOIProperty);
            }
        }

        public static readonly PropertyInfo<string> DocTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("DocType", "DocType", string.Empty));
        public string DocType
        {
            get
            {
                return GetProperty(DocTypeProperty);
            }
        }

        public static readonly PropertyInfo<string> PaperTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("PaperTitle", "PaperTitle", string.Empty));
        public string PaperTitle
        {
            get
            {
                return GetProperty(PaperTitleProperty);
            }
        }

        public static readonly PropertyInfo<string> OriginalTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("OriginalTitle", "OriginalTitle", string.Empty));
        public string OriginalTitle
        {
            get
            {
                return GetProperty(OriginalTitleProperty);
            }
        }

        public static readonly PropertyInfo<string> BookTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("BookTitle", "BookTitle", string.Empty));
        public string BookTitle
        {
            get
            {
                return GetProperty(BookTitleProperty);
            }
        }

        public static readonly PropertyInfo<int> YearProperty = RegisterProperty<int>(new PropertyInfo<int>("Year", "Year", 0));
        public int Year
        {
            get
            {
                return GetProperty(YearProperty);
            }
        }

        public static readonly PropertyInfo<SmartDate> DateProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("Date", "Date"));
        public SmartDate SmartDate
        {
            get
            {
                return GetProperty(DateProperty);
            }
        }

        public static readonly PropertyInfo<string> PublisherProperty = RegisterProperty<string>(new PropertyInfo<string>("Publisher", "Publisher", string.Empty));
        public string Publisher
        {
            get
            {
                return GetProperty(PublisherProperty);
            }
        }

        public static readonly PropertyInfo<Int64> JournalIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("JournalId", "JournalId"));
        public Int64 JournalId
        {
            get
            {
                return GetProperty(JournalIdProperty);
            }
        }

        public static readonly PropertyInfo<string> JournalProperty = RegisterProperty<string>(new PropertyInfo<string>("Journal", "Journal", string.Empty));
        public string Journal
        {
            get
            {
                return GetProperty(JournalProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ConferenceSeriesIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ConferenceSeriesId", "ConferenceSeriesId"));
        public Int64 ConferenceSeriesId
        {
            get
            {
                return GetProperty(ConferenceSeriesIdProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ConferenceInstanceIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ConferenceInstanceId", "ConferenceInstanceId"));
        public Int64 ConferenceInstanceId
        {
            get
            {
                return GetProperty(ConferenceInstanceIdProperty);
            }
        }

        public static readonly PropertyInfo<string> VolumeProperty = RegisterProperty<string>(new PropertyInfo<string>("Volume", "Volume", string.Empty));
        public string Volume
        {
            get
            {
                return (GetProperty(VolumeProperty) != null) ? GetProperty(VolumeProperty) : "";
            }
        }

        public static readonly PropertyInfo<string> IssueProperty = RegisterProperty<string>(new PropertyInfo<string>("Issue", "Issue", string.Empty));
        public string Issue
        {
            get
            {
                return GetProperty(IssueProperty);
            }
        }

        public static readonly PropertyInfo<string> FirstPageProperty = RegisterProperty<string>(new PropertyInfo<string>("FirstPage", "FirstPage", string.Empty));
        public string FirstPage
        {
            get
            {
                return GetProperty(FirstPageProperty);
            }
        }

        public static readonly PropertyInfo<string> LastPageProperty = RegisterProperty<string>(new PropertyInfo<string>("LastPage", "LastPage", string.Empty));
        public string LastPage
        {
            get
            {
                return GetProperty(LastPageProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ReferenceCountProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ReferenceCount", "ReferenceCount"));
        public Int64 ReferenceCount
        {
            get
            {
                return GetProperty(ReferenceCountProperty);
            }
        }

        public static readonly PropertyInfo<Int64> ReferencesProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("References", "References"));
        public Int64 References
        {
            get
            {
                return GetProperty(ReferencesProperty);
            }
        }

        public static readonly PropertyInfo<Int64> CitationCountProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("CitationCount", "CitationCount"));
        public Int64 CitationCount
        {
            get
            {
                return GetProperty(CitationCountProperty);
            }
        }

        public static readonly PropertyInfo<int> EstimatedCitationCountProperty = RegisterProperty<int>(new PropertyInfo<int>("EstimatedCitationCount", "EstimatedCitationCount"));
        public int EstimatedCitationCount
        {
            get
            {
                return GetProperty(EstimatedCitationCountProperty);
            }
        }

        public static readonly PropertyInfo<SmartDate> CreatedDateProperty = RegisterProperty<SmartDate>(new PropertyInfo<SmartDate>("CreatedDate", "CreatedDate"));
        public SmartDate CreatedDate
        {
            get
            {
                return GetProperty(CreatedDateProperty);
            }
        }

        public static readonly PropertyInfo<string> AuthorsProperty = RegisterProperty<string>(new PropertyInfo<string>("Authors", "Authors", string.Empty));
        public string Authors
        {
            get
            {
                return GetProperty(AuthorsProperty);
            }
        }

        public static readonly PropertyInfo<string> URLsProperty = RegisterProperty<string>(new PropertyInfo<string>("URLs", "URLs", string.Empty));
        public string URLs
        {
            get
            {
                return GetProperty(URLsProperty);
            }
        }

        public static readonly PropertyInfo<string> PdfLinksProperty = RegisterProperty<string>(new PropertyInfo<string>("PdfLinks", "PdfLinks", string.Empty));
        public string PdfLinks
        {
            get
            {
                return GetProperty(PdfLinksProperty);
            }
        }

        public static readonly PropertyInfo<Int64> LinkedITEM_IDProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("LinkedITEM_ID", "LinkedITEM_ID"));
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

        public static readonly PropertyInfo<bool> IsSelectedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsSelected", "IsSelected", false));
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

        public static readonly PropertyInfo<string> CanBeSelectedProperty = RegisterProperty<string>(new PropertyInfo<string>("CanBeSelected", "CanBeSelected", string.Empty));
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

        public static readonly PropertyInfo<string> AbstractProperty = RegisterProperty<string>(new PropertyInfo<string>("Abstract", "Abstract", string.Empty));
        public string Abstract
        {
            get
            {
                return GetProperty(AbstractProperty);
            }
        }

        public static readonly PropertyInfo<double> AutoMatchScoreProperty = RegisterProperty<double>(new PropertyInfo<double>("AutoMatchScore", "AutoMatchScore"));
        public double AutoMatchScore
        {
            get
            {
                return GetProperty(AutoMatchScoreProperty);
            }
        }

        public static readonly PropertyInfo<bool> ManualTrueMatchProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ManualTrueMatch", "ManualTrueMatch", false));
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

        public static readonly PropertyInfo<bool> ManualFalseMatchProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ManualFalseMatch", "ManualFalseMatch", false));
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

        public static readonly PropertyInfo<string> FindOnWebProperty = RegisterProperty<string>(new PropertyInfo<string>("FindOnWeb", "FindOnWeb", string.Empty));
        public string FindOnWeb
        {
            get
            {
                return "http://academic.microsoft.com/paper/" + this.PaperId.ToString();
            }
        }

        public static readonly PropertyInfo<double> SimilarityScoreProperty = RegisterProperty<double>(new PropertyInfo<double>("SimilarityScore", "SimilarityScore"));
        public double SimilarityScore
        {
            get
            {
                return GetProperty(SimilarityScoreProperty);
            }
            set
            {
                SetProperty(SimilarityScoreProperty, value);
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
            MagMatchItemToPaperManualCommand.SaveManualMatchDecision(ReadProperty(LinkedITEM_IDProperty), ReadProperty(PaperIdProperty),
                ReadProperty(ManualTrueMatchProperty), ReadProperty(ManualFalseMatchProperty));
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
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagPaper", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperId", criteria.Value));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        MagMakesHelpers.PaperMakes pm = MagMakesHelpers.GetPaperMakesFromMakes(criteria.Value);
                        if (pm != null)
                        {
                            if (reader.Read())
                            {
                                fillValues(this, pm, reader);
                            }
                            else
                            {
                                fillValues(this, pm, null);
                            }
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static MagPaper GetMagPaper(SafeDataReader reader)
        {
            MagPaper returnValue = new MagPaper();
            /*
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
            //returnValue.LoadProperty<string>(AbstractProperty, ReconstructInvertedAbstract(reader.GetString("IndexedAbstract")));
            returnValue.LoadProperty<string>(URLsProperty, reader.GetString("URLs"));
            returnValue.LoadProperty<bool>(ManualTrueMatchProperty, reader.GetBoolean("ManualTrueMatch"));
            returnValue.LoadProperty<bool>(ManualFalseMatchProperty, reader.GetBoolean("ManualFalseMatch"));
            returnValue.LoadProperty<double>(AutoMatchScoreProperty, reader.GetDouble("AutoMatchScore"));

            returnValue.MarkOld();
            */
            return returnValue;
        }

        
        

        public static MagPaper GetMagPaperFromMakes(Int64 PaperId, SafeDataReader reader)
        {
            MagPaper returnValue = new MagPaper();
            MagMakesHelpers.PaperMakes pm = MagMakesHelpers.GetPaperMakesFromMakes(PaperId);
            if (pm != null)
            {
                fillValues(returnValue, pm, reader);
            }
            else
            {
                returnValue.LoadProperty<string>(OriginalTitleProperty, "ID not found in this version of MAG");
            }
            returnValue.MarkOld();
            return returnValue;
        }

        // Can send a reader as null in some situations, but most fields contained in PaperMakes object
        internal static MagPaper GetMagPaperFromPaperMakes(MagMakesHelpers.PaperMakes pm, SafeDataReader reader)
        {
            MagPaper returnValue = new MagPaper();
            if (pm != null)
            {
                fillValues(returnValue, pm, reader);
            }
            else
            {
                returnValue.LoadProperty<string>(OriginalTitleProperty, "ID not found in this version of MAG");
            }
            returnValue.MarkOld();
            return returnValue;
        }

        public static void fillValues(MagPaper returnValue, MagMakesHelpers.PaperMakes pm, SafeDataReader reader)
        {
            TextInfo myTI = new CultureInfo("en-GB", false).TextInfo;
            returnValue.LoadProperty<Int64>(PaperIdProperty, pm.Id);
            returnValue.LoadProperty<string>(DOIProperty, pm.DOI);
            returnValue.LoadProperty<string>(DocTypeProperty, pm.Pt);
            returnValue.LoadProperty<string>(PaperTitleProperty, pm.Ti);
            returnValue.LoadProperty<string>(OriginalTitleProperty, pm.DN);
            //returnValue.LoadProperty<string>(BookTitleProperty, reader.GetString("BookTitle"));
            returnValue.LoadProperty<Int32>(YearProperty, pm.Y);
            returnValue.LoadProperty<SmartDate>(DateProperty, pm.D);
            returnValue.LoadProperty<string>(PublisherProperty, pm.PB);
            if (pm.J != null)
            {
                returnValue.LoadProperty<Int64>(JournalIdProperty, pm.J.JId);
                returnValue.LoadProperty<string>(JournalProperty, pm.J.JN != null ?  myTI.ToTitleCase(pm.J.JN) : "");
            }
            //returnValue.LoadProperty<Int64>(ConferenceSeriesIdProperty, );
            //returnValue.LoadProperty<Int64>(ConferenceInstanceIdProperty, reader.GetInt64("ConferenceInstanceId"));
            returnValue.LoadProperty<string>(VolumeProperty, pm.V);
            returnValue.LoadProperty<string>(IssueProperty, pm.I);
            returnValue.LoadProperty<string>(FirstPageProperty, pm.FP);
            returnValue.LoadProperty<string>(LastPageProperty, pm.LP);
            if (pm.RId != null)
            {
                returnValue.LoadProperty<Int64>(ReferenceCountProperty, pm.RId.Count);
                string r = "";
                foreach (Int64 RId in pm.RId)
                {
                    if (r == "")
                        r = RId.ToString();
                    else
                        r += "," + RId.ToString();
                }
            }
            else
                returnValue.LoadProperty<Int64>(ReferenceCountProperty, 0);
            returnValue.LoadProperty<Int64>(CitationCountProperty, pm.CC);
            returnValue.LoadProperty<int>(EstimatedCitationCountProperty, pm.ECC);
            if (pm.AA != null)
            {
                string a = "";
                foreach (MagMakesHelpers.PaperMakesAuthor pma in pm.AA)
                {
                    if (a == "")
                    {
                        a = pma.DAuN;
                    }
                    else
                    {
                        a += ", " + pma.DAuN;
                    }
                }
                returnValue.LoadProperty<string>(AuthorsProperty, a);
            }
            returnValue.LoadProperty<string>(AbstractProperty, MagMakesHelpers.ReconstructInvertedAbstract(pm.IA));
            if (pm.S != null)
            {
                string u = "";
                string p = "";
                foreach (MagMakesHelpers.PaperMakesSource pms in pm.S)
                {
                    if (pms.Ty == "3")
                    {
                        if (p == "")
                        {
                            p = pms.U;
                        }
                        else
                        {
                            p += ";" + pms.U;
                        }
                    }
                    else
                    {
                        if (u == "")
                        {
                            u = pms.U;
                        }
                        else
                        {
                            u += ";" + pms.U;
                        }
                    }
                }
                returnValue.LoadProperty<string>(URLsProperty, u);
                returnValue.LoadProperty<string>(PdfLinksProperty, u);
            }
            if (reader != null)
            {
                try
                {
                    returnValue.LoadProperty<Int64>(LinkedITEM_IDProperty, reader.GetInt64("ITEM_ID"));
                    returnValue.LoadProperty<bool>(ManualTrueMatchProperty, reader.GetBoolean("ManualTrueMatch"));
                    returnValue.LoadProperty<bool>(ManualFalseMatchProperty, reader.GetBoolean("ManualFalseMatch"));
                    returnValue.LoadProperty<double>(AutoMatchScoreProperty, reader.GetDouble("AutoMatchScore"));
                }
                catch
                {
                    // if the reader didn't read but we have a valid paper, it's not in the review
                }
            }
        }

        

#endif
    }
}
