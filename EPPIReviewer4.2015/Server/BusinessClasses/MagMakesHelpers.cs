using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#if (CSLA_NETCORE && !SILVERLIGHT)
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
#endif

namespace BusinessLibrary.BusinessClasses
{
    public class MagMakesHelpers
    {
        // from a paper query
        public class PaperMakesResponse
        {
            public string expr { get; set; }
            public List<PaperMakes> entities { get; set; }
        }

        public class PaperMakes
        {
            public List<PaperMakesAuthor> AA { get; set; }
            public string BT { get; set; }
            // not bothering with conference information - less important outside computer science
            public Int32 CC { get; set; }
            public DateTime D { get; set; }
            public string DN { get; set; }
            public string DOI { get; set; }
            public Int32 ECC { get; set; }
            public List<PaperMakesFieldOfStudy> F { get; set; }
            public string FP { get; set; }
            public string I { get; set; }
            //public PaperMakesInvertedAbstract IA { get; set; }
            public string IA { get; set; }
            public Int64 Id { get; set; }
            public List<PaperMakesJournal> J { get; set; }
            //public PaperMakesJournal J { get; set; }
            public string LP { get; set; }
            public string PB { get; set; }
            public string Pt { get; set; }
            public List<Int64> RId { get; set; }
            //public List<PaperMakesSource> S { get; set; }
            public string S { get; set; }
            public string Ti { get; set; }
            public string V { get; set; }
            public string VFN { get; set; }
            public string VSN { get; set; }
            public Int32 Y { get; set; }

            // These are NOT part of the Microsoft data model, but are used in matching
            public double titleLeven { get; set; }
            public double volumeMatch { get; set; }
            public double pageMatch { get; set; }
            public double yearMatch { get; set; }
            public double journalJaro { get; set; }
            public double allAuthorsLeven { get; set; }
            public double matchingScore { get; set; }
        }

        public class PaperMakesAuthor
        {
            public Int64 AfId { get; set; }
            public string AfN { get; set; }
            public Int64 AuId { get; set; }
            public string AuN { get; set; }
            public string DAuN { get; set; }
            public string DAfN { get; set; }
            public Int32 S { get; set; }
        }

        public class PaperMakesFieldOfStudy
        {
            public string DFN { get; set; }
            public Int64 FId { get; set; }
            public string FN { get; set; }
        }

        public class PaperMakesJournal
        {
            public Int64 JId { get; set; }
            public string JN { get; set; }
            public string DJN { get; set; }
        }

        public class PaperMakesSource
        {
            public string Ty { get; set; }
            public string U { get; set; }
        }

        // from a field of study query
        public class FieldOfStudyMakes
        {
            public Int32 CC { get; set; }
            public string DFN { get; set; }
            public Int32 ECC { get; set; }
            public Int32 FL { get; set; }
            public string FN { get; set; }
            public List<FieldOfStudyRelationshipMakes> FC { get; set; }
            public List<FieldOfStudyRelationshipMakes> FP { get; set; }
            public Int64 Id { get; set; }
            public Int32 PC { get; set; }
        }

        public class FieldOfStudyRelationshipMakes
        {
            public Int64 FId { get; set; }
            public string FN { get; set; }
        }

        public class MakesResponseFoS
        {
            public string expr { get; set; }
            public List<FieldOfStudyMakes> entities { get; set; }
        }

        public class PaperMakesInvertedAbstract
        {
            public Int32 IndexLength { get; set; }
            public Dictionary<string, int[]> InvertedIndex { get; set; }
        }

        // from the interpret query type

        public class MakesInterpretResponse
        {
            public string query { get; set; }
            public List<MakesInterpretation> interpretations { get; set; }
        }

        public class MakesInterpretation
        {
            public string logprob { get; set; }
            public string parse { get; set; }
            public List<MakesInterpretationRule> rules { get; set; }
        }

        /// Looks like this has been removed. Early August 2020
        public class MakesInterpretationRule
        {
            public string name { get; set; }
            public MakesInterpretationOutput output { get; set; }
        }


        public class MakesInterpretationOutput
        {
            public string type { get; set; }
            public string value { get; set; }
            public List<PaperMakes> entities { get; set; }
        }

        // Calc histogram query
        public class MakesCalcHistogramResponse
        {
            public string expr { get; set; }
            public int num_entities { get; set; }
            public List<histograms> histograms { get; set; }
        }

        public class histograms
        {
            public string attribute { get; set; }
            public string distinct_values { get; set; }
            public int total_count { get; set; }
            public List<histogram> histogram { get; set; }
        }
        public class histogram
        {
            public string value { get; set; }
            public double logprob { get; set; }
            public int count { get; set; }
        }

        // ******************************************** OpenAlex objects ***************************************************************

        // Single WORK returned by e.g. https://api.openalex.org/W1767272795
        public class OaPaper
        {
            public string id { get; set; }
            public string doi { get; set; }
            public string title { get; set; }
            public string display_name { get; set; }
            public int publication_year { get; set; }
            public string publication_date { get; set; }
            public Ids ids { get; set; }
            public Host_Venue host_venue { get; set; }
            public string type { get; set; }
            public Open_Access open_access { get; set; }
            public Authorship[] authorships { get; set; }
            public int cited_by_count { get; set; }
            public Biblio biblio { get; set; }
            public bool is_retracted { get; set; }
            public bool is_paratext { get; set; }
            public Concept[] concepts { get; set; }
            public Mesh[] mesh { get; set; }
            public Alternate_Host_Venues[] alternate_host_venues { get; set; }
            public string[] referenced_works { get; set; }
            public string[] related_works { get; set; }
            //public Abstract_Inverted_Index abstract_inverted_index { get; set; }
            public Dictionary<string, int[]> abstract_inverted_index { get; set; }
            public string cited_by_api_url { get; set; }
            public Counts_By_Year[] counts_by_year { get; set; }
            public string updated_date { get; set; }
            public string created_date { get; set; }

            // These are NOT part of the OpenAlex data model, but are used in matching
            public double titleLeven { get; set; }
            public double volumeMatch { get; set; }
            public double pageMatch { get; set; }
            public double yearMatch { get; set; }
            public double journalJaro { get; set; }
            public double allAuthorsLeven { get; set; }
            public double matchingScore { get; set; }
        }

        // The Ids class is used in Works and Concepts
        public class Ids
        {
            public string openalex { get; set; }
            public string doi { get; set; }
            public string mag { get; set; }
            public string pmid { get; set; }

            // Just for concepts
            public string wikidata { get; set; }
            public string wikipedia { get; set; }
            public string[] umls_cui { get; set; }
        }

        public class Host_Venue
        {
            public string id { get; set; }
            public string issn_l { get; set; }
            public string[] issn { get; set; }
            public string display_name { get; set; }
            public string publisher { get; set; }
            public string type { get; set; }
            public string url { get; set; }
            public bool is_oa { get; set; }
            public string version { get; set; }
            public string license { get; set; }
        }

        public class Open_Access
        {
            public bool is_oa { get; set; }
            public string oa_status { get; set; }
            public string oa_url { get; set; }
        }

        public class Biblio
        {
            public string volume { get; set; }
            public string issue { get; set; }
            public string first_page { get; set; }
            public string last_page { get; set; }
        }

        public class Abstract_Inverted_Index
        {
            public Dictionary<string, Object> InvertedIndex { get; set; } = new Dictionary<string, Object>();
        }

        public class Authorship
        {
            public string author_position { get; set; }
            public Author author { get; set; }
            public Institution[] institutions { get; set; }
            public string raw_affiliation_string { get; set; }
        }

        public class Author
        {
            public string id { get; set; }
            public string display_name { get; set; }
            public string orcid { get; set; }
        }

        public class Institution
        {
            public string id { get; set; }
            public string display_name { get; set; }
            public object ror { get; set; }
            public string country_code { get; set; }
            public object type { get; set; }
        }

        public class Concept
        {
            public string id { get; set; }
            public string wikidata { get; set; }
            public string display_name { get; set; }
            public int level { get; set; }
            public string score { get; set; }
        }

        public class Mesh
        {
            public string descriptor_ui { get; set; }
            public string descriptor_name { get; set; }
            public string qualifier_ui { get; set; }
            public string qualifier_name { get; set; }
            public bool is_major_topic { get; set; }
        }

        public class Alternate_Host_Venues
        {
            public object id { get; set; }
            public string display_name { get; set; }
            public string type { get; set; }
            public string url { get; set; }
            public bool is_oa { get; set; }
            public string version { get; set; }
            public string license { get; set; }
        }

        public class Counts_By_Year
        {
            public int year { get; set; }
            public int cited_by_count { get; set; }

            // for concepts
            public int works_count { get; set; }
        }


        // ************************* Multiple WORKS returned by a filter search **************************
        // the OaPaper is called a 'Result'
        // search e.g. https://api.openalex.org/works?filter=openalex_id:https://openalex.org/W2159419601|https://openalex.org/W2741809807

        public class OaPaperFilterResult
        {
            public Meta meta { get; set; }
            public Result[] results { get; set; }
            public object[] group_by { get; set; }
        }

        public class Meta
        {
            public int count { get; set; }
            public int db_response_time_ms { get; set; }
            public int page { get; set; }
            public int per_page { get; set; }
            public string next_cursor { get; set; }
        }

        public class Result : OaPaper
        {
            // Nothing in here, as it's an OaPaper with a different name
        }

        // ******************************************** Single CONCEPT **************************************
        // from e.g. https://api.openalex.org/C2778407487


        public class OaFullConcept // ('full' concept, as the Works object has a 'dehydrated' concept within it)
        {
            public string id { get; set; }
            public string wikidata { get; set; }
            public string display_name { get; set; }
            public int level { get; set; }
            public string description { get; set; }
            public int works_count { get; set; }
            public int cited_by_count { get; set; }
            public Ids ids { get; set; }
            public string image_url { get; set; }
            public string image_thumbnail_url { get; set; }
            public International international { get; set; }
            public Ancestor[] ancestors { get; set; }
            public Related_Concepts[] related_concepts { get; set; }
            public Counts_By_Year[] counts_by_year { get; set; }
            public string works_api_url { get; set; }
            public string updated_date { get; set; }
            public string created_date { get; set; }
        }

        public class International
        {
            public Display_Name display_name { get; set; }
            public Description description { get; set; }
        }

        public class Display_Name
        {
            public string ar { get; set; }
            public string be { get; set; }
            public string bn { get; set; }
            public string ca { get; set; }
            public string da { get; set; }
            public string de { get; set; }
            public string en { get; set; }
            public string eo { get; set; }
            public string es { get; set; }
            public string et { get; set; }
            public string fa { get; set; }
            public string fr { get; set; }
            public string hr { get; set; }
            public string hu { get; set; }
            public string id { get; set; }
            public string it { get; set; }
            public string ja { get; set; }
            public string kkcyrl { get; set; }
            public string ko { get; set; }
            public string ky { get; set; }
            public string nb { get; set; }
            public string nn { get; set; }
            public string pl { get; set; }
            public string ru { get; set; }
            public string sv { get; set; }
            public string tr { get; set; }
            public string uk { get; set; }
            public string ur { get; set; }
            public string vi { get; set; }
            public string yue { get; set; }
            public string zh { get; set; }
            public string zhcn { get; set; }
            public string zhhans { get; set; }
            public string zhhant { get; set; }
            public string cs { get; set; }
            public string fi { get; set; }
            public string he { get; set; }
            public string pt { get; set; }
            public string ptbr { get; set; }
            public string zhhk { get; set; }
            public string eu { get; set; }
            public string nl { get; set; }
            public string el { get; set; }
            public string gl { get; set; }
            public string io { get; set; }
            public string _is { get; set; }
            public string lv { get; set; }
            public string sk { get; set; }
            public string af { get; set; }
            public string ast { get; set; }
            public string az { get; set; }
            public string ba { get; set; }
            public string betarask { get; set; }
            public string bg { get; set; }
            public string br { get; set; }
            public string bs { get; set; }
            public string cy { get; set; }
            public string dech { get; set; }
            public string enca { get; set; }
            public string engb { get; set; }
            public string fo { get; set; }
            public string ga { get; set; }
            public string hi { get; set; }
            public string hsb { get; set; }
            public string ht { get; set; }
            public string hy { get; set; }
            public string ia { get; set; }
            public string ka { get; set; }
            public string kab { get; set; }
            public string kn { get; set; }
            public string kulatn { get; set; }
            public string kw { get; set; }
            public string lb { get; set; }
            public string lt { get; set; }
            public string mi { get; set; }
            public string mk { get; set; }
            public string mr { get; set; }
            public string ms { get; set; }
            public string msarab { get; set; }
            public string mt { get; set; }
            public string oc { get; set; }
            public string ro { get; set; }
            public string scn { get; set; }
            public string sco { get; set; }
            public string se { get; set; }
            public string sl { get; set; }
            public string smn { get; set; }
            public string sms { get; set; }
            public string sq { get; set; }
            public string sr { get; set; }
            public string srec { get; set; }
            public string srel { get; set; }
            public string ta { get; set; }
            public string te { get; set; }
            public string tg { get; set; }
            public string tgcyrl { get; set; }
            public string th { get; set; }
            public string tl { get; set; }
            public string tt { get; set; }
            public string vec { get; set; }
            public string xmf { get; set; }
            public string yo { get; set; }
            public string zhtw { get; set; }
            public string zhsg { get; set; }
        }

        public class Description
        {
            public string ar { get; set; }
            public string de { get; set; }
            public string en { get; set; }
            public string fr { get; set; }
            public string it { get; set; }
            public string nb { get; set; }
            public string nn { get; set; }
            public string pl { get; set; }
            public string ru { get; set; }
            public string et { get; set; }
            public string es { get; set; }
            public string ca { get; set; }
            public string da { get; set; }
            public string fa { get; set; }
            public string nl { get; set; }
            public string az { get; set; }
            public string betarask { get; set; }
            public string bg { get; set; }
            public string br { get; set; }
            public string cs { get; set; }
            public string cy { get; set; }
            public string fi { get; set; }
            public string ga { get; set; }
            public string gl { get; set; }
            public string he { get; set; }
            public string hsb { get; set; }
            public string hu { get; set; }
            public string id { get; set; }
            public string mr { get; set; }
            public string ms { get; set; }
            public string mt { get; set; }
            public string ptbr { get; set; }
            public string ro { get; set; }
            public string scn { get; set; }
            public string sl { get; set; }
            public string ta { get; set; }
            public string te { get; set; }
            public string tl { get; set; }
            public string tr { get; set; }
            public string uk { get; set; }
            public string zh { get; set; }
            public string sr { get; set; }
        }

        public class Ancestor
        {
            public string id { get; set; }
            public string wikidata { get; set; }
            public string display_name { get; set; }
            public int level { get; set; }
        }

        public class Related_Concepts
        {
            public string id { get; set; }
            public object wikidata { get; set; }
            public string display_name { get; set; }
            public int level { get; set; }
            public float score { get; set; }
        }



        // ******************************************** LIST OF CONCEPTS: from filter or search *************************
        // e.g. https://api.openalex.org/concepts?filter=ancestors.id:https://openalex.org/C2522767166&page=1&per_page=50
        // e.g. https://api.openalex.org/concepts?search=equity&page=1&per_page=10

        public class OaConceptFilterResult
        {
            public Meta meta { get; set; }
            public OaFullConcept[] results { get; set; }
            public object[] group_by { get; set; }
        }


        // ******************************************** end OpenAlex objects *********************************************


        public static string getAuthors(List<PaperMakesAuthor> authors)
        {
            string tmp = "";
            if (authors != null)
            {
                foreach (PaperMakesAuthor author in authors)
                {
                    if (tmp == "")
                    {
                        tmp = author.AuN;
                    }
                    else
                    {
                        tmp += ", " + author.AuN;
                    }
                }
            }
            return tmp;
        }

        public static string getAuthors(Authorship[] authors)
        {
            string tmp = "";
            if (authors != null)
            {
                foreach (Authorship author in authors)
                {
                    if (tmp == "")
                    {
                        tmp = author.author.display_name;
                    }
                    else
                    {
                        tmp += ", " + author.author.display_name;
                    }
                }
            }
            return tmp;
        }

        public static string getErStyleAuthors(List<PaperMakesAuthor> authors)
        {
            string ret = "";
            if (authors != null)
            {
                for (int x = 0; x < authors.Count; x++)
                {
                    AuthorsHandling.AutH author = AuthorsHandling.NormaliseAuth.singleAuth(authors[x].DAuN, x + 1, 0, true);
                    if (ret == "")
                    {
                        ret = author.FullName;
                    }
                    else
                    {
                        ret += "; " + author.FullName;
                    }
                }
            }
            return ret;
        }

        public static List<OaPaperFilterResult> downloadOaPaperFilterUsingCursor(string expression, bool doSearch)
        {
            List<OaPaperFilterResult> results = new List<OaPaperFilterResult>();
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            string filterOrSearch = "filter";
            if (doSearch == true)
                filterOrSearch = "search";
            string cursor = "*";
            
            bool done = false;
            while (done == false)
            {
                string query = "works?" + filterOrSearch + "=" + expression + "&page=1&per_page=10&cursor=" + cursor;
                string responseText = doOaRequest(query);
                OaPaperFilterResult respJson = JsonConvert.DeserializeObject<OaPaperFilterResult>(responseText, jsonsettings);
                if (respJson != null && respJson.results != null)
                {
                    results.Add(respJson);
                }
                if (respJson.meta != null && respJson.meta.next_cursor != null && respJson.meta.next_cursor != "")
                {
                    cursor = respJson.meta.next_cursor;
                }
                else
                {
                    done = true;
                }
            }
            return results;
        }

        public static OaPaperFilterResult EvaluateOaPaperFilter(string expression, string PageSize, string PageNo, bool doSearch)
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            string filterOrSearch = "filter";
            if (doSearch == true)
                filterOrSearch = "search";
            string query = "works?" + filterOrSearch + "=" + expression + "&page=" + PageNo + "&per_page=" + PageSize;
            string responseText = doOaRequest(query);
            OaPaperFilterResult respJson = JsonConvert.DeserializeObject<OaPaperFilterResult>(responseText, jsonsettings);
            return respJson;
        }

        public static OaPaper EvaluateSingleOaPaper(string PaperId)
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            string responseText = doOaRequest("works/https://openalex.org/W" + PaperId);
            return JsonConvert.DeserializeObject<OaPaper>(responseText, jsonsettings);
        }

        // the OpenAlex API syntax is pretty much identical for filters and searches, so can use one helper for both with the doSearch switch
        public static OaConceptFilterResult EvaluateOaConceptFilter(string expression, string PageSize, string PageNo, bool doSearch)
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            string filterOrSearch = "filter";
            if (doSearch == true)
                filterOrSearch = "search";
            string query = "concepts?" + filterOrSearch + "=" + expression + "&page=" + PageNo + "&per_page=" + PageSize;
            string responseText = doOaRequest(query);
            OaConceptFilterResult respJson = JsonConvert.DeserializeObject<OaConceptFilterResult>(responseText, jsonsettings);
            return respJson;
        }

        public static OaFullConcept EvaluateSingleConcept(string ConceptId)
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            string responseText = doOaRequest("concepts/https://openalex.org/C" + ConceptId);
            return JsonConvert.DeserializeObject<OaFullConcept>(responseText, jsonsettings);
        }


        

        public static string doOaRequest(string expression)
        {
#if (CSLA_NETCORE)
            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
#else
            var configuration = ConfigurationManager.AppSettings;
#endif
            string endpoint = configuration["OpenAlexEndpoint"];
            string responseText = "";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpWebRequest request = WebRequest.CreateHttp(configuration["OpenAlexEndpoint"] + expression);
            request.UserAgent = "mailto:" + configuration["OpenAlexEmailHeader"];

            try
            {
                WebResponse response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader sreader = new StreamReader(dataStream);
                    responseText = sreader.ReadToEnd();
                }
                response.Close();
            }
            catch (WebException e)
            {
                //string s = (e.Response as HttpWebResponse).StatusCode == HttpStatusCode.
                if (e.Message.Contains("429"))
                {
                    System.Threading.Thread.Sleep(500);
                    return doOaRequest(expression);
                }
            }
            return responseText;
        }


        public static List<OaPaper> GetCandidateMatches(string text, string MakesDeploymentStatus = "LIVE", bool TryAgain = false)
        {
            List<OaPaper> PaperList = new List<OaPaper>();

            string searchText = CleanText(text);
            // Hard to tell whether it's better or worse removing stopwords
            searchText = (removeStopwords(" " + searchText + " ")).Trim();
            string[] words = searchText.Split(' ');
            Array.Sort(words);
            searchText = string.Join(" ", words.Take(10));
            if (searchText != "")
            {
                //searchText = "AND(W='" + string.Join(",", words).Replace(",", "',W='") + "')"; // words.Take(6)).Replace(",", "',W='") + "')";
                var jsonsettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };


                string searchTextEncoded = System.Web.HttpUtility.UrlEncode(searchText);//uses "+" for spaces, letting his happen when creating the request would put 20% for spaces => makes the querystring longer!
                
                /*
                string FullRequestStr = MagInfo.MakesEndPoint + queryString;
                if (FullRequestStr.Length >= 2048 || queryString.Length >= 1024)
                {//this would fail entire URL is too long or the query string is.
                    int attempts = 0;
                    int maxattempts = searchText.Count(found => found == ',');
                    while ((FullRequestStr.Length >= 2048 || queryString.Length >= 1024) && attempts < maxattempts)
                    {
                        attempts++;
                        int truncateAt = searchText.LastIndexOf(",");
                        if (truncateAt != -1)
                        {
                            searchText = searchText.Substring(0, truncateAt);
                            searchTextEncoded = System.Web.HttpUtility.UrlEncode(searchText);
                            queryString = @"/evaluate?expr=" +
                                searchTextEncoded + "&entityCount=5&attributes=" + System.Web.HttpUtility.UrlEncode("Id,DN,AA.AuN,J.JN,V,I,FP,Y,DOI,AA.DAuN") +
                                "&complete=0&count=100&offset=0&timeout=2000&model=latest";
                            FullRequestStr = MagInfo.MakesEndPoint + queryString;
                        }
                    }
                }*/

                //WebRequest request = WebRequest.Create(FullRequestStr);
                try
                {
                    string responseText = doOaRequest(@"works?filter=display_name.search:" + searchTextEncoded);

                    var respJson = JsonConvert.DeserializeObject<MagMakesHelpers.OaPaperFilterResult>(responseText, jsonsettings);
                    if (respJson != null && respJson.results != null && respJson.results.Length > 0)
                    {
                        foreach (MagMakesHelpers.Result r in respJson.results)
                        {
                            var found = PaperList.Find(e => e.id == r.id);
                            if (found == null)
                            {
                                PaperList.Add(r);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
#if !CSLA_NETCORE
                    //not clear what to do on ER4, how do we log this?
                    Console.WriteLine(e.Message, searchText);
#elif WEBDB
                    WebDatabasesMVC.Startup.Logger.LogError(e, "Searching on MAKES failed for text: ", searchText);
#else
                    ERxWebClient2.Startup.Logger.LogError(e, "Searching on MAKES failed for text: ", searchText);
#endif
                    return PaperList;
                }
            }
            return PaperList;
        }

        
       
        public static List<OaPaper> GetCandidateMatchesOnDOI(string DOI)
        {
            List<OaPaper> PaperList = new List<OaPaper>();
            if (DOI != null && DOI != "")
            {
                var jsonsettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                if (DOI.IndexOf("doi.org") == -1 && DOI.IndexOf("DOI.ORG") == -1)
                {
                    DOI = "https://doi.org/" + DOI;
                }

                string responseText = doOaRequest(@"works/" + DOI);

                var respJson = JsonConvert.DeserializeObject<MagMakesHelpers.OaPaper>(responseText, jsonsettings);
                if (respJson != null)
                {
                    PaperList.Add(respJson);
                }
            }
            return PaperList;
        }


        public static OaPaper GetPaperMakesFromMakes(Int64 PaperId)
        {
            OaPaper pmr = EvaluateSingleOaPaper(PaperId.ToString());

            return pmr;
        }

        public static string ReconstructInvertedAbstract(PaperMakesInvertedAbstract ab)
        {
            if (ab == null) { return ""; }
            try
            {
                //var j = (JObject)JsonConvert.DeserializeObject(str);
                //int indexLength = j["IndexLength"].ToObject<int>();
                int indexLength = ab.IndexLength;
                //Dictionary<string, int[]> invertedIndex = j["InvertedIndex"].ToObject<Dictionary<string, int[]>>();
                Dictionary<string, int[]> invertedIndex = ab.InvertedIndex;
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

        public static string ReconstructInvertedAbstract(Dictionary<string, int[]> ab)
        {
            if (ab == null) { return ""; }
            try
            {
                //Dictionary<string, int[]> invertedIndex = j["InvertedIndex"].ToObject<Dictionary<string, int[]>>();
                string[] abstractStr = new string[10000];
                foreach (var pair in ab)
                {
                    string word = (string)pair.Key;
                    foreach (var index in (int[])pair.Value)
                    {
                        abstractStr[index] = word;
                    }
                }
                return String.Join(" ", abstractStr).Trim();
            }
            catch
            {
                return "";
            }
        }

        public static string ReconstructInvertedAbstract(string ab)
        {
            if (ab == null) { return ""; }
            try
            {
                var j = (JObject)JsonConvert.DeserializeObject(ab);
                int indexLength = j["IndexLength"].ToObject<int>();

                //int indexLength = ab.IndexLength;
                //Dictionary<string, int[]> invertedIndex = j["InvertedIndex"].ToObject<Dictionary<string, int[]>>();
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

        // Gets the EPPI-Reviewer equivalent publication type from a MAKES paper record
        public static int GetErEquivalentPubType(string Pt)
        {
            switch (Pt)
            {
                case "0":
                    return 12; //unknown
                case "1":
                    return 14; // journal article
                case "2":
                    return 12; // patent
                case "3":
                    return 1; // journal article, as they put the conference in the journal name field
                case "4":
                    return 3; // book chapter
                case "5":
                    return 2; // book
                case "6": // Book reference entry (whatever that is - mapping to generic)
                    return 12;
                case "7": // dataset
                    return 12;
                case "8": // repository
                    return 12;
                case "9": // thesis
                    return 4;
            }
            return 12; // just in case
        }

        public static string GetOaPubTypeIndexFromString(string s)
        {
            switch (s)
            {
                case "Journal":
                    return "1";
                case "Patent":
                    return "2";
                case "Conference":
                    return "3";
                case "BookChapter":
                    return "4";
                case "Book":
                    return "5";
                case "BookReferenceEntry":
                    return "6";
                case "Dataset":
                    return "7";
                case "Repository":
                    return "8";
                case "Thesis":
                    return "9";
                default:
                    return "0"; // "Unknown"
            }
        }

        public static string GetOaPubTypeStringFromIndex(string s)
        {
            switch (s)
            {
                case "0":
                    return "Unknown";
                case "1":
                    return "Journal";
                case "2":
                    return "Patent";
                case "3":
                    return "Conference";
                case "4":
                    return "BookChapter";
                case "5":
                    return "Book";
                case "6":
                    return "BookReferenceEntry";
                case "7":
                    return "Dataset";
                case "8":
                    return "Repository";
                case "9":
                    return "Thesis";
                default:
                    return "Unknown";
            }
        }

        public static int GetErEquivalentPubTypeFromOa(string Pt)
        {
            switch (Pt)
            {
                case "peer-review":
                case "other":
                case "reference-entry":
                case "component":
                case "proceedings-series":
                case "report-series":
                case "standard":
                case "posted-content":
                case "grant":
                case "book-series":
                case "standard-series":
                    return 12; //unknown
                case "journal-article":
                case "proceedings-article":
                    return 14; // journal article
                case "2":
                    return 12; // patent
                case "3":
                    return 1; // journal article, as they put the conference in the journal name field
                case "book-section":
                case "book-chapter":
                    return 3; // book chapter
                case "monograph":
                case "report":
                case "book-part":
                case "book":
                case "journal-volume":
                case "book-set":
                case "journal":
                case "proceedings":
                case "reference-book":
                case "journal-issue":
                case "edited-book":
                    return 2; // book
                case "book-track": // Book reference entry (whatever that is - mapping to generic)
                    return 12;
                case "dataset": // dataset
                    return 12;
                case "8": // repository
                    return 12;
                case "dissertation": // thesis
                    return 4;
            }
            return 12; // just in case
        }

        public static readonly Regex CleanTextWhiteList = new Regex("[^a-zA-Z0-9 ]");
        public static readonly Regex CleanTextBlackList = new Regex("[!-/:-@[-`{-¿"
                        + Char.ConvertFromUtf32(697) + "-" + Char.ConvertFromUtf32(866)//using the unicode codes because they look odd and might not work in VS
                        + Char.ConvertFromUtf32(1154) + "-" + Char.ConvertFromUtf32(1161)
                        + Char.ConvertFromUtf32(1369) + "-" + Char.ConvertFromUtf32(1375)
                        + Char.ConvertFromUtf32(1417) + "-" + Char.ConvertFromUtf32(1479)
                        + Char.ConvertFromUtf32(1519) + "-" + Char.ConvertFromUtf32(1567)
                        + "]"
                        );//these are ranges of the unicode chars that contain various symbols, pretty much stopping before the arabic range because ignorance...

        //private static int DebugCounter = 0; //this should be commented out in production!
        public static string CleanText(string text, bool UseLighterTouch = false)
        {
            if (text == null || text == "") return "";

            Dictionary<string, string> charMap = EuropeanCharacterMap();
            foreach (KeyValuePair<string, string> replacement in charMap)
            {
                text = text.Replace(replacement.Key, replacement.Value);
            }
            string orig = text;

            text = CleanTextWhiteList.Replace(text, " ").ToLower().Trim();
            while (text.IndexOf("  ") != -1)
            {
                text = text.Replace("  ", " ");
            }
            if (UseLighterTouch)
            {//this means: if the string got shortened too much, clean it by removing a blacklist, instead of the whitelist (rgx) we used above

                //int full = orig.Length;
                //int cut = text.Length;

                double full = orig.Length;
                double cut = text.Length;
                if (full == 0 || cut / full >= 0.1) return text; //cleaned text is 10% or more of the original text, we'll keep it.
                else
                {// we removed too much, so we'll use the less aggressive approach, getting rid of what's in the ranges below
                 //hopefully the BlackList removes a great deal of noise, even if perhaps not all noise.

                    //used for debugging, should be commented in production:
                    //DebugCounter++;

                    text = CleanTextBlackList.Replace(orig, " ").ToLower().Trim();
                    int tLen = text.Length;
                    while (text.IndexOf("  ") != -1)
                    {
                        text = text.Replace("  ", " ");
                        if (tLen == text.Length) break;
                        tLen = text.Length;
                    }
                }
            }
            return text;
        }
        public static string RemoveTextInParentheses(string s)
        {
            s = s.TrimEnd(' ');
            if (s.EndsWith("]"))
            {
                int i = s.LastIndexOf('[');
                if ((s.Length - i) * 4 < s.Length)
                {
                    s = s.Substring(0, i).TrimEnd(' ');
                }
            }
            return s;
        }
        private static string RestoreGreekLetters(string text)
        {

            Dictionary<string, string> charMap = GreekLettersMap();

            foreach (KeyValuePair<string, string> replacement in charMap)
            {
                Regex rx = new Regex("(?<![a-zA-Z])" + replacement.Key + "(?![a-zA-Z])");//the name of the letter, preceded and followed by something that is not a regular alphabet letter
                text = rx.Replace(text, replacement.Value);
            }

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

        private static Dictionary<string, string> GreekLettersMap()
        {
            return new Dictionary<string, string>()
            {
                { "alpha", "α" },
                { "beta", "β" },
                { "gamma", "γ" },
                { "delta", "δ" },
                { "epsilon", "ε" },
                { "zeta", "ζ" },
                { "eta", "η" },
                { "theta", "θ" },
                { "iota", "ι" },
                { "kappa", "κ" },
                { "lambda", "λ" },
                { "mu", "μ" },
                { "nu", "ν" },
                { "xi", "ξ" },
                { "omicron", "ο" },
                { "pi", "π" },
                { "rho", "ρ" },
                { "sigma", "σ" },
                { "tau", "τ" },
                { "upsilon", "υ" },
                { "phi", "φ" },
                { "chi", "χ" },
                { "psi", "ψ" },
                { "omega", "ω" }
            };
        }

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
        //the below is a failed attempt, we're doing something more sophysticated, in the end [see RestoreGreekLetters(string text)]
        //public static Regex ProblemWords = new Regex("[^a-zA-Z]alpha[^a-zA-Z]|[^a-zA-Z]beta[^a-zA-Z]|[^a-zA-Z]gamma[^a-zA-Z]|[^a-zA-Z]delta[^a-zA-Z]|[^a-zA-Z]epsilon[^a-zA-Z]|[^a-zA-Z]zeta[^a-zA-Z]|[^a-zA-Z]eta[^a-zA-Z]|[^a-zA-Z]theta[^a-zA-Z]|[^a-zA-Z]iota[^a-zA-Z]|[^a-zA-Z]kappa[^a-zA-Z]|[^a-zA-Z]lambda[^a-zA-Z]|[^a-zA-Z]mu[^a-zA-Z]|[^a-zA-Z]nu[^a-zA-Z]|[^a-zA-Z]xi[^a-zA-Z]|[^a-zA-Z]omicron[^a-zA-Z]|[^a-zA-Z]pi[^a-zA-Z]|[^a-zA-Z]rho[^a-zA-Z]|[^a-zA-Z]sigma[^a-zA-Z]|[^a-zA-Z]tau[^a-zA-Z]|[^a-zA-Z]upsilon[^a-zA-Z]|[^a-zA-Z]phi[^a-zA-Z]|[^a-zA-Z]chi[^a-zA-Z]|[^a-zA-Z]psi[^a-zA-Z]|[^a-zA-Z]omega[^a-zA-Z]");

    }
}
