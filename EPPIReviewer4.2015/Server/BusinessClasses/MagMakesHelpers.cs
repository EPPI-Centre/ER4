using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Newtonsoft.Json;
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
            public PaperMakesInvertedAbstract IA { get; set; }
            public Int64 Id { get; set; }
            public PaperMakesJournal J { get; set; }
            public string LP { get; set; }
            public string PB { get; set; }
            public string Pt { get; set; }
            public List<Int64> RId { get; set; }
            public List<PaperMakesSource> S { get; set; }
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


        public static PaperMakesResponse EvaluateSinglePaperId(string PaperId, string MakesDeploymentStatus = "LIVE")
        {
            string query = @"/evaluate?expr=Id=" + PaperId;
            return doMakesRequest(query, "", MakesDeploymentStatus);
        }


        public static PaperMakesResponse EvaluateExpressionNoPaging(string expression, string MakesDeploymentStatus = "LIVE")
        {
            string query = @"/evaluate?expr=" + expression;
            return doMakesRequest(query, "", MakesDeploymentStatus);
        }

        public static PaperMakesResponse EvaluateExpressionNoPagingWithCount(string expression, string count, string MakesDeploymentStatus = "LIVE")
        {
            string query = query = @"/evaluate?expr=" + expression;
            string appendPageInfo = @"&count=" + count;
            return doMakesRequest(query, appendPageInfo, MakesDeploymentStatus);
        }

        public static PaperMakesResponse EvaluateExpressionWithPaging(string searchString, string PageSize, string offSet, string MakesDeploymentStatus = "LIVE")
        {
            string query = query = @"/evaluate?expr=" + searchString;
            string appendPageInfo = @"&count=" + PageSize + "&offset=" + offSet;
            return doMakesRequest(query, appendPageInfo, MakesDeploymentStatus);
        }

        public static MakesResponseFoS EvaluateFieldOfStudyExpression(string expression, string MakesDeploymentStatus = "LIVE")
        {
            string query = @"/evaluate?expr=" + expression;
            return doMakesRequestFoS(query, "", MakesDeploymentStatus);
        }

        public static MakesInterpretResponse InterpretQuery(string expression, string MakesDeploymentStatus = "LIVE")
        {
            string query = @"/interpret?query=" + System.Web.HttpUtility.UrlEncode(CleanText(expression));
            return doMakesInterpretRequest(query, "", MakesDeploymentStatus);
        }

        public static MakesCalcHistogramResponse CalcHistoramCount(string expression, string MakesDeploymentStatus = "LIVE")
        {
            string query = @"/calchistogram?expr=" + expression;
            return doMakesCalcHistogramRequest(query, MakesDeploymentStatus);
        }

        public static FieldOfStudyMakes EvaluateSingleFieldOfStudyId(string FosId, string MakesDeploymentStatus = "LIVE")
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            string responseText = "";
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide(MakesDeploymentStatus);
            WebRequest request = WebRequest.Create(MagInfo.MakesEndPoint + @"/evaluate?expr=Id=" +
                FosId.ToString() + @"&attributes=Id,CC,DFN,ECC,FL,FN,FC.FId,FC.FN,FP.FId,FP.FN");
            WebResponse response = request.GetResponse();
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader sreader = new StreamReader(dataStream);
                responseText = sreader.ReadToEnd();
            }
            response.Close();
            var respJson = JsonConvert.DeserializeObject<MagMakesHelpers.MakesResponseFoS>(responseText, jsonsettings);
            if (respJson != null && respJson.entities != null && respJson.entities.Count > 0)
            {
                return respJson.entities[0];
            }
            else
                return null;
        }

        public static List<PaperMakes> GetCandidateMatches(string text, string MakesDeploymentStatus = "LIVE", bool TryAgain = false)
        {
            List<PaperMakes> PaperList = new List<PaperMakes>();
            
            string searchText = CleanText(text);
            // Hard to tell whether it's better or worse removing stopwords
            searchText = (removeStopwords(" " + searchText + " ")).Trim();
            string[] words = searchText.Split(' ');
            if (searchText != "")
            {
                searchText = "AND(W='" + string.Join(",", words).Replace(",", "',W='") + "')"; // words.Take(6)).Replace(",", "',W='") + "')";
                var jsonsettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide(MakesDeploymentStatus);

                searchText = System.Web.HttpUtility.UrlEncode(searchText);//uses "+" for spaces, letting his happen when creating the request would put 20% for spaces => makes the querystring longer!

                string queryString =  @"/evaluate?expr=" +
                    searchText + "&entityCount=5&attributes=" + System.Web.HttpUtility.UrlEncode("Id,DN,AA.AuN,J.JN,V,I,FP,Y,DOI,VFN,AA.DAuN") +
                    "&complete=0&count=100&offset=0&timeout=2000&model=latest";
                string FullRequestStr = MagInfo.MakesEndPoint + queryString;
                if (FullRequestStr.Length >= 2048 || queryString.Length >= 1024)
                {//this would fail entire URL is too long or the query string is.
                    int attempts = 0;
                    int maxattempts = queryString.Count(found => found == '+');
                    while ((FullRequestStr.Length >= 2048 || queryString.Length >=1024) && attempts < maxattempts)
                    {
                        attempts++;
                        int truncateAt = searchText.LastIndexOf("+");
                        if (truncateAt != -1)
                        {
                            searchText = searchText.Substring(0, truncateAt);
                            queryString = @"/evaluate?expr=" +
                                searchText + "&entityCount=5&attributes=" + System.Web.HttpUtility.UrlEncode("Id,DN,AA.AuN,J.JN,V,I,FP,Y,DOI,VFN,AA.DAuN") +
                                "&complete=0&count=100&offset=0&timeout=2000&model=latest";
                            FullRequestStr = MagInfo.MakesEndPoint + queryString;
                        }
                    }
                }
                //WebRequest request = WebRequest.Create(FullRequestStr);
                try
                {
                    /*
                    WebResponse response = request.GetResponse();
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader sreader = new StreamReader(dataStream);
                        responseText = sreader.ReadToEnd();
                    }
                    response.Close();
                    */
                    HttpClient client = new HttpClient();
                    var response = client.GetAsync(FullRequestStr).Result;

                    var resp = response.Content.ReadAsStringAsync().Result;
                    var respJson = JsonConvert.DeserializeObject<MagMakesHelpers.PaperMakesResponse>(resp, jsonsettings);
                    if (respJson != null && respJson.entities != null && respJson.entities.Count > 0)
                    {
                        foreach (PaperMakes pm in respJson.entities)
                        {

                            var found = PaperList.Find(e => e.Id == pm.Id);
                            if (found == null)
                            {
                                PaperList.Add(pm);
                            }
                        }
                    }


                    /* when using an interpret request...
                    var respJson = JsonConvert.DeserializeObject<MagMakesHelpers.MakesInterpretResponse>(resp, jsonsettings);
                    if (respJson != null && respJson.interpretations != null && respJson.interpretations.Count > 0)
                    {
                        foreach (MakesInterpretation i in respJson.interpretations)
                        {
                            //foreach (MakesInterpretationRule r in i.rules)
                            //{
                                foreach (PaperMakes pm in i.topEntities)
                                {
                                    var found = PaperList.Find(e => e.Id == pm.Id);
                                    if (found == null)
                                    {
                                        PaperList.Add(pm);
                                    }
                                }
                            //}
                        }
                    }
                    */
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
            if (TryAgain && PaperList.Count == 0)
            {
                PaperList = MagMakesHelpers.GetCandidateMatchesTake2(searchText, MakesDeploymentStatus);
            }
            return PaperList;
        }

        private static List<PaperMakes> GetCandidateMatchesTake2(string text, string MakesDeploymentStatus)
        {//will try searching again, but truncating the search string when we find a problem word (if possible)
            List<PaperMakes> PaperList = new List<PaperMakes>();
            string searchText = RestoreGreekLetters(text);

            if (searchText != "")
            {
                var jsonsettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                string responseText = "";
                MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide(MakesDeploymentStatus);
                string queryString = @"/interpret?query=" +
                    searchText + "&entityCount=5&attributes=" + System.Web.HttpUtility.UrlEncode("Id,DN,AA.AuN,J.JN,V,I,FP,Y,DOI,VFN,AA.DAuN") +
                    "&complete=0&count=100&offset=0&timeout=2000&model=latest";

                WebRequest request = WebRequest.Create(MagInfo.MakesEndPoint + queryString);
                WebResponse response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader sreader = new StreamReader(dataStream);
                    responseText = sreader.ReadToEnd();
                }
                response.Close();

                var respJson = JsonConvert.DeserializeObject<MagMakesHelpers.MakesInterpretResponse>(responseText, jsonsettings);
                if (respJson != null && respJson.interpretations != null && respJson.interpretations.Count > 0)
                {
                    foreach (MakesInterpretation i in respJson.interpretations)
                    {
                        foreach (MakesInterpretationRule r in i.rules)
                        {
                            foreach (PaperMakes pm in r.output.entities)
                            {
                                var found = PaperList.Find(e => e.Id == pm.Id);
                                if (found == null)
                                {
                                    PaperList.Add(pm);
                                }
                            }
                        }
                    }
                }
            }
            
            return PaperList;
        }

        public static List<PaperMakes> GetCandidateMatchesOnDOI(string DOI, string MakesDeploymentStatus = "LIVE")
        {//will try searching again, but truncating the search string when we find a problem word (if possible)
            List<PaperMakes> PaperList = new List<PaperMakes>();
            //string searchText = RestoreGreekLetters(text);

            if (DOI != null && DOI != "")
            {
                var jsonsettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                string responseText = "";
                MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide(MakesDeploymentStatus);
                string queryString = @"/evaluate?expr=DOI='" +
                    System.Web.HttpUtility.UrlEncode(DOI.ToUpper().Replace("HTTPS://DX.DOI.ORG/", "").Replace("HTTPS://DOI.ORG/", "").Replace("HTTP://DX.DOI.ORG/", "").Replace("HTTP://DOI.ORG/", "").Replace("[DOI]", "").TrimEnd('.').Trim())
                    + "'&entityCount=5&attributes=" +
                    System.Web.HttpUtility.UrlEncode("Id,DN,AA.AuN,J.JN,V,I,FP,Y,DOI,VFN,AA.DAuN") +
                    "&complete=0&count=10&offset=0&timeout=2000&model=latest";
                WebRequest request = WebRequest.Create(MagInfo.MakesEndPoint + queryString);
                WebResponse response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader sreader = new StreamReader(dataStream);
                    responseText = sreader.ReadToEnd();
                }
                response.Close();

                var respJson = JsonConvert.DeserializeObject<MagMakesHelpers.PaperMakesResponse>(responseText, jsonsettings);
                if (respJson != null && respJson.entities != null && respJson.entities.Count > 0)
                {
                    foreach (PaperMakes i in respJson.entities)
                    {
                        var found = PaperList.Find(e => e.Id == i.Id);
                        if (found == null)
                        {
                            PaperList.Add(i);
                        }
                    }
                }
            }
            return PaperList;
        }

        private static PaperMakesResponse doMakesRequest(string query, string appendPageInfo, string MakesDeploymentStatus)
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            string responseText = "";
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide(MakesDeploymentStatus);
            WebRequest request = WebRequest.Create(MagInfo.MakesEndPoint + query +
                "&attributes=AA.AfId,AA.AuN,AA.DAfN,AA.DAuN,AA.AuId,CC,Id,DN,DOI,Pt,Ti,Y,D,PB,I,J.JN,J.JId,V,FP,LP,RId,ECC,IA,S,VFN" +
                appendPageInfo);
            WebResponse response = request.GetResponse();
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader sreader = new StreamReader(dataStream);
                responseText = sreader.ReadToEnd();
            }
            response.Close();

            PaperMakesResponse respJson = JsonConvert.DeserializeObject<PaperMakesResponse>(responseText, jsonsettings);
            return respJson;
        }
        private static MakesResponseFoS doMakesRequestFoS(string query, string appendPageInfo, string MakesDeploymentStatus)
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            string responseText = "";
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide(MakesDeploymentStatus);
            WebRequest request = WebRequest.Create(MagInfo.MakesEndPoint + query +
                "&attributes=AA.AfId,AA.AuN,AA.DAfN,AA.DAuN,AA.AuId,CC,Id,DN,DOI,Pt,Ti,Y,D,PB,I,J.JN,J.JId,V,FP,LP,RId,ECC,IA,S,VFN" +
                appendPageInfo);
            WebResponse response = request.GetResponse();
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader sreader = new StreamReader(dataStream);
                responseText = sreader.ReadToEnd();
            }
            response.Close();

            MakesResponseFoS respJson = JsonConvert.DeserializeObject<MakesResponseFoS>(responseText, jsonsettings);
            return respJson;
        }

        private static MakesInterpretResponse doMakesInterpretRequest(string query, string appendPageInfo, string MakesDeploymentStatus)
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            string responseText = "";
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide(MakesDeploymentStatus);
            WebRequest request = WebRequest.Create(MagInfo.MakesEndPoint + query +
                "&entityCount=5&attributes=" + System.Web.HttpUtility.UrlEncode("AA.AfId,AA.AfN,AA.DAfN,AA.AuId,AA.AuN,AA.DAuN,AA.S,C.CId,C.CN,CC,D,DN,DOI,ECC,F.FId,F.DFN,F.FN,I,IA,Id,J.JId,J.JN,LP,PB,PCS.CId,PCS.CN,Pt,RId,S,Ti,Ty,V,VFN,VSN,W,Y") +
                "&complete=1&count=10&normalize=1&model=latest");
            WebResponse response = request.GetResponse();
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader sreader = new StreamReader(dataStream);
                responseText = sreader.ReadToEnd();
            }
            response.Close();
            
            MakesInterpretResponse respJson = JsonConvert.DeserializeObject<MakesInterpretResponse>(responseText, jsonsettings);
            return respJson;
        }

        private static MakesCalcHistogramResponse doMakesCalcHistogramRequest(string query, string MakesDeploymentStatus)
        {
            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            string responseText = "";
            MagCurrentInfo MagInfo = MagCurrentInfo.GetMagCurrentInfoServerSide(MakesDeploymentStatus);
            WebRequest request = WebRequest.Create(MagInfo.MakesEndPoint + query +
                "&attributes=" + System.Web.HttpUtility.UrlEncode("F.FN,Id"));
            WebResponse response = request.GetResponse();
            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader sreader = new StreamReader(dataStream);
                responseText = sreader.ReadToEnd();
            }
            response.Close();

            MakesCalcHistogramResponse respJson = JsonConvert.DeserializeObject<MakesCalcHistogramResponse>(responseText, jsonsettings);
            return respJson;
        }


        public static PaperMakes GetPaperMakesFromMakes(Int64 PaperId, string MakesDeploymentStatus = "LIVE")
        {
            PaperMakesResponse pmr = EvaluateSinglePaperId(PaperId.ToString(), MakesDeploymentStatus);

            if (pmr.entities != null && pmr.entities.Count > 0)
            {
                return pmr.entities[0];
            }
            else
            {
                return null;
            }
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

        // Gets the EPPI-Reviewer equivalent publication type from a MAKES paper record
        public static int GetErEquivalentPubType(string Pt)
        {
            switch (Pt)
            {
                case "0":
                    return 12; //unknown
                    break;
                case "1":
                    return 14; // journal article
                    break;
                case "2":
                    return 12; // patent
                    break;
                case "3":
                    return 1; // journal article, as they put the conference in the journal name field
                    break;
                case "4":
                    return 3; // book chapter
                    break;
                case "5":
                    return 2; // book
                    break;
                case "6": // Book reference entry (whatever that is - mapping to generic)
                    return 12;
                    break;
                case "7": // dataset
                    return 12;
                    break;
                case "8": // repository
                    return 12;
                    break;
            }
            return 12; // just in case
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
