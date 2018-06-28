using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PubmedImport;

namespace Klasifiki.Controllers
{
    public class FindByPubMedIDsController : Controller
    {
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
        public ActionResult Fetch([FromForm] string IdsText)
        {
            try
            {
                //first, let's make sure what we have is comma delimited and change delimiter to '¬'
                IdsText = IdsText.Trim();
                //if (IdsText.Length > 10 && IdsText.Contains(' ') && IdsText.Contains(','))
                //{
                    IdsText = IdsText.Replace(',', '¬');
                //}
                //second, let's check our string makes some sense...
                string[] splitted = IdsText.Split('¬');
                double estMin = IdsText.Length / 9.5;
                double estMax = IdsText.Length / 4;
                if (splitted.Length < estMin || splitted.Length > estMax)
                {//something is wrong, don't try...
                    return View();
                }
                List<CitationRecord> results = new List<CitationRecord>();
                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.DataServiceDB))
                {
                    results = GetCitationRecordByPMID(conn, IdsText);
                }
                return View(results);
            }
            catch
            {
                return View();
            }
        }
        private static List<CitationRecord> GetCitationRecordByPMID(SqlConnection conn, string pubmedIDs)
        {
            List<CitationRecord> res = new List<CitationRecord>();
            SqlParameter extName = new SqlParameter("@ExternalIDName", "pubmed");
            SqlParameter pmid = new SqlParameter("@ExternalIDs", pubmedIDs);
            try
            {
                using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(conn, "st_findCitationsByExternalIDs", extName, pmid))
                {
                    while (reader.Read()) res.Add(CitationRecord.GetCitationRecord(reader));
                }
            }
            catch (Exception e)
            {
                Program.Logger.LogException(e, "Error fetching existing ref and/or creating local object.");
                
            }
            return res;
        }
        // GET: FindByPubMedIDs/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: FindByPubMedIDs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FindByPubMedIDs/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: FindByPubMedIDs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}