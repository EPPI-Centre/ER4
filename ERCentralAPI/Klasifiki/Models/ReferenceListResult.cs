using PubmedImport;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Klasifiki.Models
{
    public class ReferenceListResult
    {
        public List<ReferenceRecord> Results { get; set; } = new List<ReferenceRecord>();
        public string SearchMethod { get;  }
        public string SearchString { get;  }
        public string ListOfIDs//used to quickly re-fetch results when needed
        {
            get
            {
                if (Results == null || Results.Count == 0) return "";
                return string.Join('¬', Results.Select(x => x.CitationId));
            }
                
        }
        private string[] _GraphLabels = new string[] { "100-90%", "89-80%", "79-70%", "69-60%", "59-50%", "49-40%", "39-30%", "29-20%", "19-80%", "9-0%", "N/A" };
        public string[] GraphLabels
        {
            get
            {
                return _GraphLabels;
            }
        }
        public int[] GraphValues
        {
            get
            {
                int v90 = 0, v80 = 0, v70 = 0, v60 = 0, v50 = 0, v40 = 0, v30 = 0, v20 = 0, v10 = 0, v0 = 0, vNA = 0;
                foreach(ReferenceRecord refr in Results)
                {
                    if (refr.Arrowsmith_RCT_Score >= 0.9) v90++;
                    else if (refr.Arrowsmith_RCT_Score >= 0.8) v80++;
                    else if (refr.Arrowsmith_RCT_Score >= 0.7) v70++;
                    else if (refr.Arrowsmith_RCT_Score >= 0.6) v60++;
                    else if (refr.Arrowsmith_RCT_Score >= 0.5) v50++;
                    else if (refr.Arrowsmith_RCT_Score >= 0.4) v40++;
                    else if (refr.Arrowsmith_RCT_Score >= 0.3) v30++;
                    else if (refr.Arrowsmith_RCT_Score >= 0.2) v20++;
                    else if (refr.Arrowsmith_RCT_Score >= 0.1) v10++;
                    else if (refr.Arrowsmith_RCT_Score >= 0.0) v0++;
                    else vNA++;
                }
                return new int[] { v90, v80, v70, v60, v50, v40, v30, v20, v10, v0, vNA };
            }
        }
        public Dictionary<string, string> DataPoints
        {
            get
            {
                int[] vals = GraphValues;
                Dictionary<string, string> res = new Dictionary<string, string>();
                res.Add(_GraphLabels[0], vals[0].ToString());
                res.Add(_GraphLabels[1], vals[1].ToString() );
                res.Add(_GraphLabels[2], vals[2].ToString() );
                res.Add(_GraphLabels[3], vals[3].ToString() );
                res.Add(_GraphLabels[4], vals[4].ToString() );
                res.Add(_GraphLabels[5], vals[5].ToString());
                res.Add(_GraphLabels[6], vals[6].ToString());
                res.Add(_GraphLabels[7], vals[7].ToString() );
                res.Add(_GraphLabels[8], vals[8].ToString() );
                res.Add(_GraphLabels[9], vals[9].ToString() );
                res.Add(_GraphLabels[10], vals[10].ToString() );
                return res;
            }
        }
        public ReferenceListResult(string searchString, string searchMethod)
        {
            SearchMethod = searchMethod;
            SearchString = searchString;
        }
    }
}
