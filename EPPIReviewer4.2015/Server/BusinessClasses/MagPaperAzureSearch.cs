using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary.BusinessClasses
{

    public class PaperAzureSearch
    {
        public Int64 id { get; set; }
        //public int rank { get; set; }
        public Int32 year { get; set; }
        public string journal { get; set; }
        public List<string> authors { get; set; }
        public string volume { get; set; }
        public string issue { get; set; }
        public string first_page { get; set; }
        public string last_page { get; set; }
        public string title { get; set; }
        public string doi { get; set; }

        public Int64 LinkedItemId { get; set; }
        public bool ManualTrueMatch { get; set; }
        public bool ManualFalseMatch { get; set; }
        public double AutoMatchScore { get; set; }
    }
    class MagPaperAzureSearch
    {
        
        public static PaperAzureSearch GetPaperAzureSearch(string PaperId)
        {
            PaperAzureSearch pas = new BusinessClasses.PaperAzureSearch();

            string responseText = "";
            WebRequest request = WebRequest.Create(@"https://eppimag.search.windows.net/indexes/mag-index/docs/" + PaperId + @"?api-version=2017-11-11");
            request.ContentType = "application/json";
            request.Headers.Add("api-key", "***REMOVED***");
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader sreader = new StreamReader(dataStream);
                    responseText = sreader.ReadToEnd();
                }
                response.Close();
                pas = JsonConvert.DeserializeObject<PaperAzureSearch>(responseText);
            }
            catch (Exception ex)
            {
                pas.id = 0;
                pas.title = "(PaperId: " + PaperId + " not found in current MAG index / index unavailable";
                pas.authors = new List<string>();
                pas.authors.Add("");
                pas.doi = "";
                pas.first_page = "";
                pas.issue = "";
                pas.journal = "";
                pas.last_page = "";
                pas.volume = "";
                pas.year = 0;
            }

            return pas;

        }


    }
}
