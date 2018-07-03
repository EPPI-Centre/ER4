using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Klasifiki.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PubmedImport;

namespace Klasifiki.Controllers
{
    [Authorize("Authenticated")]
    public class FindCitationsController : Controller
    {
        private static string FetchAddress = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi";
        private static string SearchAddress = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi";
        // GET: FindByPubMedIDs
        public ActionResult Index()
        {
            return View();
        }

        // GET: FindByPubMedIDs/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: FindByPubMedIDs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FindByPubMedIDs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Fetch([FromForm] string SearchString, string SearchMethod, string ListOfIDs)
        {
            if (ListOfIDs != null && ListOfIDs.Length >0 && ListOfIDs.Contains("¬"))
            {
                ReferenceListResult results = new ReferenceListResult(SearchString, SearchMethod);
                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                {
                    results.Results = GetReferenceRecordsByRefIDs(conn, ListOfIDs);
                }
                return View("Fetch", results);
            }
            else if (SearchMethod == "PubMedSearch")
            {
                if (SearchString.Trim().Length > 0)
                {
                    ReferenceListResult results = new ReferenceListResult(SearchString, SearchMethod);
                    results.Results = GetReferenceRecordByPubMedSearch(SearchString);
                    return View(results);
                }
                else return Redirect("~/Home");
            }
            else if (SearchMethod == "PubMedIDs")
            {
                try
                {
                    //first, let's make sure what we have is comma delimited and change delimiter to '¬'
                    SearchString = SearchString.Trim();
                    ReferenceListResult results = new ReferenceListResult(SearchString, SearchMethod);
                    if (results.SearchString.Length > 10 && !results.SearchString.Contains(','))
                    {
                        return Redirect("~/Home");
                    }
                    //if (IdsText.Length > 10 && IdsText.Contains(' ') && IdsText.Contains(','))
                    //{
                    //       SearchString = SearchString.Replace(',', '¬');
                    //}
                    //second, let's check our string makes some sense...
                    SearchString = SearchString.Replace(',', '¬');
                    string[] splitted = SearchString.Split('¬');
                    double estMin = SearchString.Length / 9.5;
                    double estMax = SearchString.Length / 4;
                    if (splitted.Length < estMin || splitted.Length > estMax)
                    {//something is wrong, don't try...
                        return Redirect("~/Home");
                    }
                    using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                    {
                        results.Results = GetReferenceRecordsByPMIDs(conn, SearchString);
                    }
                    return View(results);
                }
                catch (Exception e)
                {
                    Program.Logger.LogException(e, "Error fetching list of type:" + SearchMethod + ".");
                    return Redirect("~/Home"); //View();
                }
            }
            else
            {
                return Redirect("~/Home");
            }
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Fetch([FromForm] string ListOfIDs, string SearchString, string SearchMethod)
        //{
        //    ReferenceListResult results = new ReferenceListResult(SearchString, SearchMethod);
        //    using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
        //    {
        //        results.Results = GetReferenceRecordsByRefIDs(conn, ListOfIDs);
        //        results.Results = results.Results.OrderBy(x => x.Title).ToList();
        //    }
        //    return View("Fetch", results);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SortByTitle([FromForm] string ListOfIDs, string SearchString, string SearchMethod)
        {
            ReferenceListResult results = new ReferenceListResult(SearchString, SearchMethod);
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
            {
                results.Results = GetReferenceRecordsByRefIDs(conn, ListOfIDs);
                results.Results = results.Results.OrderBy(x => x.Title).ToList();
            }
            return View("Fetch", results);
        }
        public ActionResult SortByYear([FromForm] string ListOfIDs, string SearchString, string SearchMethod)
        {
            ReferenceListResult results = new ReferenceListResult(SearchString, SearchMethod);
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
            {
                results.Results = GetReferenceRecordsByRefIDs(conn, ListOfIDs);
                results.Results = results.Results.OrderByDescending(x => x.PublicationYear).ToList();
            }
            return View("Fetch", results);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SortByScore([FromForm] string ListOfIDs, string SearchString, string SearchMethod)
        {
            ReferenceListResult results = new ReferenceListResult(SearchString, SearchMethod);
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
            {
                results.Results = GetReferenceRecordsByRefIDs(conn, ListOfIDs);
                results.Results = results.Results.OrderByDescending(x => x.Arrowsmith_RCT_Score).ToList();
            }
            return View("Fetch", results);
        }
        private static List<ReferenceRecord> GetReferenceRecordsByRefIDs(SqlConnection conn, string refIDs)
        {
            List<ReferenceRecord> res = new List<ReferenceRecord>();
            SqlParameter RefIDs = new SqlParameter("@RefIDs", refIDs);
            try
            {
                using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(conn, "st_findCitationsByReferenceIDs", RefIDs))
                {
                    res = ReferenceRecord.GetReferenceRecordList(reader);
                }
            }
            catch (Exception e)
            {
                Program.Logger.LogSQLException(e, "Error fetching existing ref and/or creating local object.");
            }
            return res;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ViewGraph([FromForm] string ListOfIDs, string SearchString, string SearchMethod)
        {
            ReferenceListResult results = new ReferenceListResult(SearchString, SearchMethod);
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
            {
                results.Results = GetReferenceRecordsByRefIDs(conn, ListOfIDs);
            }
            return View("FetchGraph", results);
        }




        private static List<ReferenceRecord> GetReferenceRecordsByPMIDs(SqlConnection conn, string pubmedIDs)
        {
            List<ReferenceRecord> res = new List<ReferenceRecord>();
            SqlParameter extName = new SqlParameter("@ExternalIDName", "pubmed");
            SqlParameter pmid = new SqlParameter("@ExternalIDs", pubmedIDs);
            try
            {
                using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(conn, "st_findCitationsByExternalIDs", extName, pmid))
                {
                    res = ReferenceRecord.GetReferenceRecordList(reader);
                }
            }
            catch (Exception e)
            {
                Program.Logger.LogSQLException(e, "Error fetching existing ref and/or creating local object.");
            }
            return res;
        }
        private static List<ReferenceRecord> GetReferenceRecordByPubMedSearch(string searchString)
        {
            List<ReferenceRecord> res = new List<ReferenceRecord>();
            string searchRawResult = DoPubMedSearchAsync(searchString, 0, 10000);
            XElement xResponse = XElement.Parse(searchRawResult);
            res = GetCitationsFromResponse(xResponse);
            return res;
        }
        private static string DoPubMedSearchAsync(string query, int start, int end)
        {
            System.Collections.Specialized.NameValueCollection nvcoll = new System.Collections.Specialized.NameValueCollection();
            //string FetchAddress = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi";
            using (WebClient webc = new WebClient())
            {
                //tool=EppiReviewer4&email=eppisupport@ioe.ac.uk&db=pubmed&term=pain sleep disorders&usehistory=y
                nvcoll.Clear();
                nvcoll.Add("tool", "EppiReviewer5");
                nvcoll.Add("email", "eppisupport@ucl.ac.uk");
                nvcoll.Add("db", "pubmed");
                nvcoll.Add("usehistory", "n");
                nvcoll.Add("term", query);
                nvcoll.Add("retstart", start.ToString());
                nvcoll.Add("retmax", end.ToString());
                string response = "";


                try
                {
                    //if (IsTesting)
                    //{
                    //    response = _wsClient.PubMedSearch2(query);
                    //}
                    //else
                    //{
                    //    byte[] responseArray =  webc.UploadValues(new System.Uri(SearchAddress), "POST", nvcoll);
                    //    response = Encoding.ASCII.GetString(responseArray);
                    //}
                    byte[] responseArray = webc.UploadValues(new System.Uri(SearchAddress), "POST", nvcoll);
                    response = Encoding.ASCII.GetString(responseArray);
                }
                catch (WebException we)
                {//if request is unsuccessful, we get an error inside the WebException
                    WebResponse wr = we.Response;
                    if (wr == null)
                    {
                        throw new Exception("WebResponse is null: " + we.Message + Environment.NewLine
                            + (we.InnerException != null && we.InnerException.Message != null ? we.InnerException.Message : ""));

                    }
                    using (var reader = new StreamReader(wr.GetResponseStream()))
                    {
                        if (reader == null)
                        {
                            throw new Exception("WebResponse reader is null: " + we.Message + Environment.NewLine
                            + (we.InnerException != null && we.InnerException.Message != null ? we.InnerException.Message : ""));
                        }
                        response = reader.ReadToEnd();

                        webc.Dispose();
                    }
                    return "";
                }
                return response;
            }
        }
        private static List<ReferenceRecord> GetCitationsFromResponse(XElement xResponse)
        {
            List<ReferenceRecord> res = new List<ReferenceRecord>();
            
            string ids = "";
            List<XElement> IdList = xResponse.Element("IdList").Elements("Id").ToList();
            if (IdList != null && IdList.Count > 0)
            {
                foreach (var item in IdList)
                {
                    ids += item.Value + "¬";
                }
                ids = ids.Trim('¬');
                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                {
                    res = GetReferenceRecordsByPMIDs(conn, ids);
                }
            }
            return res;
        }
        
        

        // POST: FindByPubMedIDs/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: FindByPubMedIDs/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: FindByPubMedIDs/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}