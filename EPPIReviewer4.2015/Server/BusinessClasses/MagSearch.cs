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
using BusinessLibrary.BusinessClasses;
using System.Text.RegularExpressions;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using System.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagSearch : BusinessBase<MagSearch>
    {
#if SILVERLIGHT
    public MagSearch() { }

        
#else
        public MagSearch() { }
#endif
        public void SetToRerun(MagSearch oldSearch)
        {
            this.SearchText = oldSearch.SearchText;
            this.MagSearchText = oldSearch.MagSearchText;
        }

        /*
        public void SetCombinedSearches(List<MagSearch> searches, string AndOrOr)
        {
            string combinedMagSearch = "";
            string combinedSearchText = AndOrOr + "(";
            foreach (MagSearch ms in searches)
            {
                if (combinedMagSearch == "")
                {
                    combinedMagSearch = ms.MagSearchText;
                    combinedSearchText += "#" + ms.SearchNo.ToString();
                }
                else
                {
                    combinedMagSearch += "," + ms.MagSearchText;
                    combinedSearchText += ", #" + ms.SearchNo.ToString();
                }
            }
            this.SearchText = combinedSearchText + ")";
            this.MagSearchText = AndOrOr + "(" + combinedMagSearch + ")";
        }
        */

        public string GetSearchTextTitle(string searchText)
        {
            string cleaned = CleanText(searchText);
            cleaned = removeStopwords(" " + cleaned + " ").Trim().ToLower();
            string [] words = cleaned.Split(' ');
            if (words.Length == 0)
                return "";
            if (words.Length == 1)
            {
                cleaned = "W='" + words[0] + "'";
            }
            else
            {
                cleaned = "AND(W='" + string.Join(",", words).Replace(",", "',W='") + "')";
            }
            return cleaned;
        }

        public string GetSearchTextAbstract(string searchText)
        {
            string cleaned = CleanText(searchText);
            cleaned = removeStopwords(" " + cleaned + " ").Trim().ToLower();
            string[] words = cleaned.Split(' ');
            if (words.Length == 0)
                return "";
            if (words.Length == 1)
            {
                cleaned = "AW='" + words[0] + "'";
            }
            else
            {
                cleaned = "AND(AW='" + string.Join(",", words).Replace(",", "',AW='") + "')";
            }
            
            return cleaned;
        }

        public string GetSearchTextMagIds(string searchText)
        {
            string cleaned = CleanText(searchText);
            cleaned = removeStopwords(" " + cleaned + " ").Trim().ToLower();
            string[] words = cleaned.Split(' ');
            if (words.Length == 0)
                return "";

            foreach (string s in words)
            {
                Int64 test = 0;
                if (!Int64.TryParse(s, out test))
                {
                    return "Error: not a valid list of IDs";
                }
            }

            if (words.Length == 1)
            {
                cleaned = "W" + words[0];
            }
            else
            {
                cleaned = "W" + string.Join("|", words).Replace("|", "|W");
            }

            return cleaned;
        }

        public string GetSearchTextAuthors(string searchText)
        {
            string cleaned = CleanText(searchText);
            cleaned = removeStopwords(" " + cleaned + " ").Trim().ToLower();
            string[] words = cleaned.Split(' ');
            if (words.Length == 0)
                return "";
            if (words.Length == 1)
            {
                cleaned = "COMPOSITE(AA.AuN = '" + words[0] + "'...)";
            }
            else
            {
                cleaned = "AND(COMPOSITE(AA.AuN='" + string.Join(",", words).Replace(",", "'...),COMPOSITE(AA.AuN='") + "'...))";
            }
            return cleaned;
        }

        public string GetSearchTextJournals(string searchText)
        {
            string cleaned = CleanText(searchText);
            cleaned = removeStopwords(" " + cleaned + " ").Trim().ToLower();
            cleaned = "COMPOSITE(J.JN = '" + cleaned + "'...)";
            return cleaned;
        }

        public string GetSearchTextFieldOfStudy(string FId)
        {
            return "Composite(F.FId=" + FId + ")";
        }
        public string GetSearchTextPubDateExactly(string date)
        {
            return "D='" + date + "'";
        }
        public string GetSearchTextPubDateFrom(string date)
        {
            return "D>'" + date + "'";
        }

        public string GetSearchTextPubDateBefore(string date)
        {
            return "D<'" + date + "'";
        }

        public string GetSearchTextPubDateBetween(string date1, string date2)
        {
            return "D=['" + date1 + "','" + date2 + "']";
        }
        public string GetSearchTextYearExactly(string year)
        {
            return "Y=" + year;
        }
        public string GetSearchTextYearBefore(string year)
        {
            return "Y<" + year;
        }

        public string GetSearchTextYearAfter(string year)
        {
            return "Y>" + year;
        }

        public string GetSearchTextYearBetween(string year1, string year2)
        {
            return "Y=[" + year1 + "," + year2 + "]";
        }
        public string GetSearchTextPublicationType(string pubType)
        {
            return "Pt='" + pubType + "'";
        }
        public string GetPublicationType(int pubType)
        {
            string pType = "";
            switch (pubType)
            {
                case 1:
                    pType = "Journal article";
                    break;
                case 2:
                    pType = "Patent";
                    break;
                case 3:
                    pType = "Conference paper";
                    break;
                case 4:
                    pType = "Book chapter";
                    break;
                case 5:
                    pType = "Book";
                    break;
                case 6:
                    pType = "Book reference entry";
                    break;
                case 7:
                    pType = "Dataset";
                    break;
                case 8:
                    pType = "Repository";
                    break;
                default:
                    pType = "Unknown";
                    break;
            }
            return pType;
        }

        // *********** These are from MagMakesHelpers - which is a server-side class. We should really move to a generic text helpers class?
        public static string removeStopwords(string input)
        {
            //string[] stopWords = { " and ", " for ", " are ", " from ", " have ", " results ", " based ", " between ", " can ", " has ", " analysis ", " been ", " not ", " method ", " also ", " new ", " its ", " all ", " but ", " during ", " after ", " into ", " other ", " our ", " non ", " present ", " most ", " only ", " however ", " associated ", " compared ", " des ", " related ", " proposed ", " about ", " each ", " obtained ", " increased ", " had ", " among ", " due ", " how ", " out ", " les ", " los ", " abstract ", " del ", " many ", " der ", " including ", " could ", " report ", " cases ", " possible ", " further ", " given ", " result ", " las ", " being ", " like ", " any ", " made ", " because ", " discussed ", " known ", " recent ", " findings ", " reported ", " considered ", " described ", " although ", " available ", " particular ", " provides ", " improved ", " here ", " need ", " improve ", " analyzed ", " either ", " produced ", " demonstrated ", " evaluated ", " provided ", " did ", " does ", " required ", " before ", " along ", " presents ", " having ", " much ", " near ", " demonstrate ", " iii ", " often ", " making ", " the ", " that ", " with ", " this ", " were ", " was ", " which ", " study ", " using ", " these ", " their ", " used ", " than ", " use ", " such ", " when ", " well ", " some ", " through ", " there ", " under ", " they ", " within ", " will ", " while ", " those ", " various ", " where ", " then ", " very ", " who ", " und ", " should ", " thus ", " suggest ", " them ", " therefore ", " since ", " une ", " what ", " whether ", " una ", " von ", " would ", " of ", " in ", " a ", " to ", " is ", " on ", " by ", " as ", " de ", " an ", " be ", " we ", " or ", " s ", " it ", " la ", " e ", " en ", " i ", " no ", " et ", " el ", " do ", " up ", " se ", " un ", " ii " };
            string[] stopWords = { " a ", " about ", " abstract ", " after ", " all ", " along ", " also ", " although ", " among ", " an ", " analysis ", " analyzed ", " and ", " any ", " are ", " as ", " associated ", " available ", " based ", " be ", " because ", " been ", " before ", " being ", " between ", " but ", " by ", " can ", " cases ", " compared ", " considered ", " could ", " de ", " del ", " demonstrate ", " demonstrated ", " der ", " des ", " described ", " did ", " discussed ", " do ", " does ", " due ", " during ", " e ", " each ", " either ", " el ", " en ", " et ", " evaluated ", " findings ", " for ", " from ", " further ", " given ", " had ", " has ", " have ", " having ", " here ", " how ", " however ", " i ", " ii ", " iii ", " improve ", " improved ", " in ", " including ", " increased ", " into ", " is ", " it ", " its ", " known ", " la ", " las ", " les ", " like ", " los ", " made ", " making ", " many ", " method ", " most ", " much ", " near ", " need ", " new ", " no ", " non ", " not ", " obtained ", " of ", " often ", " on ", " only ", " or ", " other ", " our ", " out ", " particular ", " possible ", " present ", " presents ", " produced ", " proposed ", " provided ", " provides ", " recent ", " related ", " report ", " reported ", " required ", " result ", " results ", " s ", " se ", " should ", " since ", " some ", " study ", " such ", " suggest ", " than ", " that ", " the ", " their ", " them ", " then ", " there ", " therefore ", " these ", " they ", " this ", " those ", " through ", " thus ", " to ", " un ", " una ", " und ", " under ", " une ", " up ", " use ", " used ", " using ", " various ", " very ", " von ", " was ", " we ", " well ", " were ", " what ", " when ", " where ", " whether ", " which ", " while ", " who ", " will ", " with ", " within ", " would " };
            foreach (string word in stopWords)
            {
                input = input.Replace(word, " ");
            }
            return input;
        }

        public static string CleanText(string text)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 ]");
            Dictionary<string, string> charMap = EuropeanCharacterMap();
            foreach (KeyValuePair<string, string> replacement in charMap)
            {
                text = text.Replace(replacement.Key, replacement.Value);
            }

            text = rgx.Replace(text, " ").ToLower().Trim();
            while (text.IndexOf("  ") != -1)
            {
                text = text.Replace("  ", " ");
            }
            return text;
        }

        private static Dictionary<string, string> EuropeanCharacterMap()
        {
            return new Dictionary<string, string>()
                                            {
                                                { "¡", "i" },
                                                { "¿", "?" },
                                                { "Ä", "A" },
                                                { "Å", "A" },
                                                { "ä", "a" },
                                                { "ª", "a" },
                                                { "À", "A" },
                                                { "Á", "A" },
                                                { "Ã", "A" },
                                                { "à", "a" },
                                                { "á", "a" },
                                                { "ã", "a" },
                                                { "å", "a" },
                                                { "Æ", "AE" },
                                                { "æ", "ae" },
                                                { "Ç", "C" },
                                                { "Č", "C" },
                                                { "Ć", "C" },
                                                { "ç", "c" },
                                                { "č", "c" },
                                                { "ć", "c" },
                                                { "È", "E" },
                                                { "É", "E" },
                                                { "Ê", "E" },
                                                { "Ë", "E" },
                                                { "Ε", "E" },
                                                { "è", "e" },
                                                { "é", "e" },
                                                { "ê", "e" },
                                                { "ë", "e" },
                                                { "ę", "e" },
                                                { "ε", "e" },
                                                { "ğ", "g" },
                                                { "Ì", "I" },
                                                { "Í", "I" },
                                                { "Î", "I" },
                                                { "Ï", "I" },
                                                { "İ", "I" },
                                                { "ì", "i" },
                                                { "í", "i" },
                                                { "î", "i" },
                                                { "ï", "i" },
                                                { "ı", "i" },
                                                { "ℓ", "l" },
                                                { "ł", "l" },
                                                { "Ñ", "N" },
                                                { "ń", "n" },
                                                { "ñ", "n" },
                                                { "ô", "o" },
                                                { "º", "o" },
                                                { "Ò", "O" },
                                                { "Ó", "O" },
                                                { "Ô", "O" },
                                                { "Õ", "O" },
                                                { "Ö", "O" },
                                                { "Ø", "O" },
                                                { "ò", "o" },
                                                { "ó", "o" },
                                                { "õ", "o" },
                                                { "ö", "o" },
                                                { "ø", "o" },
                                                { "Š", "S" },
                                                { "ş", "s" },
                                                { "š", "s" },
                                                { "ß", "s" },
                                                { "Û", "U" },
                                                { "Ù", "U" },
                                                { "Ú", "U" },
                                                { "Ü", "U" },
                                                { "ù", "u" },
                                                { "ú", "u" },
                                                { "û", "u" },
                                                { "ü", "u" },
                                                { "ÿ", "y" },
                                                { "ż", "z" },
                                                { "Ⅰ", "I" },
                                                { "Ⅱ", "II" },
                                                { "Ⅲ", "III" },
                                                { "Ⅳ", "IV" },
                                                { "Ⅴ", "V" },
                                                { "Ⅵ", "VI" },
                                                { "Ⅶ", "VII" },
                                                { "Ⅷ", "VIII" },
                                                { "Ⅸ", "IX" },
                                                { "Ⅹ", "X" },
                                                { "Ⅺ", "XI" },
                                                { "Ⅻ", "XII" },
                                                { "(r)", "" },
                                                { "(R)", "" },
                                                { "(c)", "" },
                                                { "(C)", "" },
                                                { "™", "" } // JT added this one (not in MAG team's list)
                                            };
        }

        public static readonly PropertyInfo<int> MagSearchIdProperty = RegisterProperty<int>(new PropertyInfo<int>("MagSearchId", "MagSearchId"));
        public int MagSearchId
        {
            get
            {
                return GetProperty(MagSearchIdProperty);
            }
        }

        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId", 0));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
            set
            {
                SetProperty(ReviewIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "ContactId", 0));
        public int ContactId
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

        public static readonly PropertyInfo<string> SearchTextProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchText", "SearchText", ""));
        public string SearchText
        {
            get
            {
                return GetProperty(SearchTextProperty);
            }
            set
            {
                SetProperty(SearchTextProperty, value);
            }
        }

        public static readonly PropertyInfo<int> SearchNoProperty = RegisterProperty<int>(new PropertyInfo<int>("SearchNo", "SearchNo", 0));
        public int SearchNo
        {
            get
            {
                return GetProperty(SearchNoProperty);
            }
            set
            {
                SetProperty(SearchNoProperty, value);
            }
        }

        public static readonly PropertyInfo<int> HitsNoProperty = RegisterProperty<int>(new PropertyInfo<int>("HitsNo", "HitsNo", 0));
        public int HitsNo
        {
            get
            {
                return GetProperty(HitsNoProperty);
            }
            set
            {
                SetProperty(HitsNoProperty, value);
            }
        }

        public static readonly PropertyInfo<DateTime> SearchDateProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("SearchDate", "SearchDate", 0));
        public DateTime SearchDate
        {
            get
            {
                return GetProperty(SearchDateProperty);
            }
            set
            {
                SetProperty(SearchDateProperty, value);
            }
        }

        public static readonly PropertyInfo<string> MagFolderProperty = RegisterProperty<string>(new PropertyInfo<string>("MagFolder", "MagFolder", ""));
        public string MagFolder
        {
            get
            {
                return GetProperty(MagFolderProperty);
            }
            set
            {
                SetProperty(MagFolderProperty, value);
            }
        }

        public static readonly PropertyInfo<string> MagSearchTextProperty = RegisterProperty<string>(new PropertyInfo<string>("MagSearchText", "MagSearchText", ""));
        public string MagSearchText
        {
            get
            {
                return GetProperty(MagSearchTextProperty);
            }
            set
            {
                SetProperty(MagSearchTextProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ContactName", "ContactName", ""));
        public string ContactName
        {
            get
            {
                return GetProperty(ContactNameProperty);
            }
            set
            {
                SetProperty(ContactNameProperty, value);
            }
        }

        public static readonly PropertyInfo<string> Date1Property = RegisterProperty<string>(new PropertyInfo<string>("Date1", "Date1", ""));
        public string Date1
        {
            get
            {
                return GetProperty(Date1Property);
            }
            set
            {
                SetProperty(Date1Property, value);
            }
        }

        public static readonly PropertyInfo<string> Date2Property = RegisterProperty<string>(new PropertyInfo<string>("Date2", "Date2", ""));
        public string Date2
        {
            get
            {
                return GetProperty(Date2Property);
            }
            set
            {
                SetProperty(Date2Property, value);
            }
        }

        public static readonly PropertyInfo<string> DateFilterProperty = RegisterProperty<string>(new PropertyInfo<string>("DateFilter", "DateFilter", ""));
        public string DateFilter
        {
            get
            {
                return GetProperty(DateFilterProperty);
            }
            set
            {
                SetProperty(DateFilterProperty, value);
            }
        }

        public static readonly PropertyInfo<string> PublicationTypeFilterProperty = RegisterProperty<string>(new PropertyInfo<string>("PublicationTypeFilter", "PublicationTypeFilter", ""));
        public string PublicationTypeFilter
        {
            get
            {
                return GetProperty(PublicationTypeFilterProperty);
            }
            set
            {
                SetProperty(PublicationTypeFilterProperty, value);
            }
        }
        public static readonly PropertyInfo<string> PublicationTypeTextFilterProperty = RegisterProperty<string>(new PropertyInfo<string>("PublicationTypeTextFilter", "PublicationTypeTextFilter", ""));
        public string PublicationTypeTextFilter
        {
            get
            {
                return GetProperty(PublicationTypeTextFilterProperty);
            }
            set
            {
                SetProperty(PublicationTypeTextFilterProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> SearchIdsStoredProperty = RegisterProperty<bool>(new PropertyInfo<bool>("SearchIdsStored", "SearchIdsStored", false));
        public bool SearchIdsStored
        {
            get
            {
                return GetProperty(SearchIdsStoredProperty);
            }
            set
            {
                SetProperty(SearchIdsStoredProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SearchIdsProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchIds", "SearchIds", ""));
        public string SearchIds
        {
            get
            {
                return GetProperty(SearchIdsProperty);
            }
            set
            {
                SetProperty(SearchIdsProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> IsOASearchProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsOASearch", "IsOASearch", true));
        public bool IsOASearch
        {
            get
            {
                return (this.MagFolder == null || this.MagFolder == "") ? true : false ;
            }
            
        }



        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(MagSearch), admin);
        //    //AuthorizationRules.AllowDelete(typeof(MagSearch), admin);
        //    //AuthorizationRules.AllowEdit(typeof(MagSearch), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(MagSearch), canRead);

        //    //AuthorizationRules.AllowRead(MagSearchIdProperty, canRead);
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
            if (!SearchText.StartsWith("¬"))
            {
                SearchText = "¬" + SearchText; // JT added 27/04/2024 to fix bug on re-running searches. The logic below assumes this is present (as it is for new searches), but it's then removed before saving.
            }

            if (SearchText.IndexOf("¬COMBINE SEARCHES") > -1)
            {
                CombineSearches();
                return;
            }

            bool TitleAndAbstract = false;
            bool DoSearch = false;

            if (MagSearchText != "")
            {
                MagMakesHelpers.OaPaperFilterResult resp = null;
                if (SearchText.StartsWith("¬Title:"))
                {
                    if (MagSearchText.IndexOf("display_name.search:") == -1) // i.e. for a rerun the search is already fully described
                    {
                        MagSearchText = "display_name.search:" + MagSearchText;
                    }
                }
                if (SearchText.StartsWith("¬Title and abstract:"))
                {
                    //TitleAndAbstract = true;
                    MagSearchText = "default.search:" + MagSearchText;
                    //MagSearchText = MagSearchText;
                }
                if (SearchText.StartsWith("¬Topic:"))
                {
                    if (MagSearchText.IndexOf("concepts.id:") == -1) // i.e. for a rerun the search is already fully described
                    {
                        MagSearchText = "concepts.id:" + "https://openalex.org/C" + MagSearchText;
                    }
                }
                if (SearchText.StartsWith("¬OpenAlex ID(s):"))
                {
                    MagSearchText = "openalex_id:" + MagSearchText.Replace("W", "https://openalex.org/W");
                }
                if (SearchText.StartsWith("¬Custom filter:"))
                {
                    // Custom filter - just execute the searchtext
                }
                if (SearchText.StartsWith("¬Custom search:"))
                {
                    // Custom search - just execute the searchtext
                    DoSearch = true;
                }
                SearchText = SearchText.Replace("¬", "");

                if (TitleAndAbstract)
                {
                    resp = MagMakesHelpers.EvaluateOaPaperFilter(MagSearchText, "1", "1", true);
                }
                else
                {
                    if (this.DateFilter != "")
                    {
                        switch (DateFilter)
                        {
                            case "Published on":
                                MagSearchText += ",publication_date:" + Date1;
                                break;
                            case "Published before":
                                MagSearchText += ",to_publication_date:" + Date1;
                                break;
                            case "Published after":
                                MagSearchText += ",from_publication_date:" + Date1;
                                break;
                            case "Published between":
                                MagSearchText += ",from_publication_date:" + Date1 + ",to_publication_date:" + Date2;
                                break;
                            case "Publication year":
                                MagSearchText += ",publication_year:" + Date1;
                                break;
                            case "Created after":
                                MagSearchText += ",from_created_date:" + Date1;
                                break;
                        }
                    }
                    resp = MagMakesHelpers.EvaluateOaPaperFilter(MagSearchText, "1", "1", DoSearch);
                }

                if (resp != null && resp.meta != null)
                {
                    HitsNo = resp.meta.count;
                }
                else
                {
                    HitsNo = 0;
                    SearchText = "INVALID SEARCH";
                }

                
            }
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide("LIVE");
            this.SearchDate = DateTime.Now.Date;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSearchInsert", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TEXT", SearchText));
                    command.Parameters.Add(new SqlParameter("@SEARCH_NO", 0)); // set in the SP
                    command.Parameters.Add(new SqlParameter("@HITS_NO", HitsNo)); 
                    command.Parameters.Add(new SqlParameter("@SEARCH_DATE", SearchDate));
                    command.Parameters.Add(new SqlParameter("@MAG_FOLDER", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@MAG_SEARCH_TEXT", MagSearchText));
                    command.Parameters.Add(new SqlParameter("@MAG_SEARCH_ID", 0));
                    command.Parameters["@MAG_SEARCH_ID"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagSearchIdProperty, Convert.ToInt32(command.Parameters["@MAG_SEARCH_ID"].Value));
                }
                connection.Close();
            }
        }

        private void CombineSearches()
        {
            // grabbing these variables now in case of ticket expiery or anything else odd if the task takes a while
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int ContactId = ri.UserId;
            int ReviewId = ri.ReviewId;

            HitsNo = 0;
            bool CombineWithOR = true;
            string[] SearchIDs = null;
            if (MagSearchText.IndexOf("AND") > -1)
            {
                CombineWithOR = false;
                SearchIDs = MagSearchText.Replace("AND", "¬").Split('¬');
            }
            else
            {
                SearchIDs = MagSearchText.Replace("OR", "¬").Split('¬');
            }

            // Get the IDs for the first search
            List<string> OverallResults = GetIdsFromSearchId(SearchIDs[0], ReviewId);

            // Loop through the rest of the searches and AND or OR them to the original
            for (int i = 1; i < SearchIDs.Length; i++)
            {
                List<string> CurrentSearchResults = GetIdsFromSearchId(SearchIDs[i], ReviewId);
                if (CombineWithOR == true)
                {
                    OverallResults = OverallResults.Union(CurrentSearchResults).ToArray().ToList();
                }
                else
                {
                    OverallResults = OverallResults.Intersect(CurrentSearchResults).ToArray().ToList();
                }
            }

            this.SearchDate = DateTime.Now.Date;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSearchInsert", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command.Parameters.Add(new SqlParameter("@SEARCH_TEXT", SearchText.Replace("¬COMBINE SEARCHES", "").Replace("AND", " AND ").Replace("OR", " OR ")));
                    command.Parameters.Add(new SqlParameter("@SEARCH_NO", 0)); // set in the SP
                    command.Parameters.Add(new SqlParameter("@HITS_NO", OverallResults.Count));
                    command.Parameters.Add(new SqlParameter("@SEARCH_DATE", SearchDate));
                    command.Parameters.Add(new SqlParameter("@MAG_FOLDER", ""));
                    command.Parameters.Add(new SqlParameter("@MAG_SEARCH_TEXT", MagSearchText));
                    command.Parameters.Add(new SqlParameter("@MAG_SEARCH_ID", 0));
                    command.Parameters.Add(new SqlParameter("@SEARCH_IDS_STORED", true));
                    command.Parameters.Add(new SqlParameter("@SEARCH_IDS", string.Join(",", OverallResults)));
                    command.Parameters["@MAG_SEARCH_ID"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(MagSearchIdProperty, Convert.ToInt32(command.Parameters["@MAG_SEARCH_ID"].Value));
                }
                connection.Close();
            }

        }

        private List<string> GetIdsFromSearchId(string Id, int ReviewId)
        {
            MagSearch theSearch = GetMagSearchById(Id, ReviewId);
            if (theSearch.SearchIdsStored == true)
            {
                return GetStoredSearchIds(theSearch);
            }
            else
            {
                return GetIdsFromOpenAlex(theSearch);
            }
        }

        private List<string> GetStoredSearchIds(MagSearch theSearch)
        {
            List<string> results = new List<string>();
            string [] allIds = theSearch.SearchIds.Split(',');
            foreach (string s in allIds)
            {
                results.Add(s);
            }
            return results;
        }

        private List<string> GetIdsFromOpenAlex(MagSearch theSearch)
        {
            List<string> results = new List<string>();
            bool doSearch = false;
            if ((theSearch.MagSearchText.IndexOf("display_name.search:") == -1) && (theSearch.MagSearchText.IndexOf("concepts.id:") == -1) && (theSearch.MagSearchText.IndexOf("openalex_id:") == -1))
                {
                doSearch = true; // i.e. title/abstract search where we 'search' rather than 'filter'
            }
            List<MagMakesHelpers.OaPaperFilterResult> res = MagMakesHelpers.downloadOaPaperFilterUsingCursor(theSearch.MagSearchText, doSearch);
            foreach (MagMakesHelpers.OaPaperFilterResult r in res)
            {
                foreach (MagMakesHelpers.OaPaper p in r.results)
                {
                    results.Add(p.id.Replace("https://openalex.org/W", ""));
                }
            }
            return results;
        }

        protected override void DataPortal_Update()
        {
            // no need to update this object
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSearchDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_SEARCH_ID", ReadProperty(MagSearchIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        // this version used when we want a list of searches
        internal static MagSearch GetMagSearch(SafeDataReader reader)
        {
            MagSearch returnValue = new MagSearch();
            returnValue.LoadProperty<int>(MagSearchIdProperty, reader.GetInt32("MAG_SEARCH_ID"));
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<string>(SearchTextProperty, reader.GetString("SEARCH_TEXT"));
            returnValue.LoadProperty<int>(SearchNoProperty, reader.GetInt32("SEARCH_NO"));
            returnValue.LoadProperty<int>(HitsNoProperty, reader.GetInt32("HITS_NO"));
            returnValue.LoadProperty<DateTime>(SearchDateProperty, reader.GetDateTime("SEARCH_DATE"));
            returnValue.LoadProperty<string>(MagFolderProperty, reader.GetString("MAG_FOLDER"));
            returnValue.LoadProperty<string>(MagSearchTextProperty, reader.GetString("MAG_SEARCH_TEXT"));
            returnValue.LoadProperty<bool>(SearchIdsStoredProperty, reader.GetBoolean("SEARCH_IDS_STORED"));
            returnValue.MarkOld();
            return returnValue;
        }

        // this version for a specific search when we load optional search results too
        internal static MagSearch GetMagSearchWithIds(SafeDataReader reader)
        {
            MagSearch returnValue = new MagSearch();
            returnValue.LoadProperty<int>(MagSearchIdProperty, reader.GetInt32("MAG_SEARCH_ID"));
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            returnValue.LoadProperty<string>(ContactNameProperty, reader.GetString("CONTACT_NAME"));
            returnValue.LoadProperty<string>(SearchTextProperty, reader.GetString("SEARCH_TEXT"));
            returnValue.LoadProperty<int>(SearchNoProperty, reader.GetInt32("SEARCH_NO"));
            returnValue.LoadProperty<int>(HitsNoProperty, reader.GetInt32("HITS_NO"));
            returnValue.LoadProperty<DateTime>(SearchDateProperty, reader.GetDateTime("SEARCH_DATE"));
            returnValue.LoadProperty<string>(MagFolderProperty, reader.GetString("MAG_FOLDER"));
            returnValue.LoadProperty<string>(MagSearchTextProperty, reader.GetString("MAG_SEARCH_TEXT"));
            returnValue.LoadProperty<bool>(SearchIdsStoredProperty, reader.GetBoolean("SEARCH_IDS_STORED"));
            returnValue.LoadProperty<string>(SearchIdsProperty, reader.GetString("SEARCH_IDS"));
            returnValue.MarkOld();
            return returnValue;
        }

        internal static MagSearch GetMagSearchById(string Id, int ReviewId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSearch", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@MAG_SEARCH_ID", Convert.ToInt32(Id)));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            return GetMagSearchWithIds(reader);
                        }
                    }
                }
                connection.Close();
            }
            return null;
        }

#endif

    }
}
