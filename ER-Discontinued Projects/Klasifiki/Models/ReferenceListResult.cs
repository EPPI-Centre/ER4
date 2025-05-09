﻿using PubmedImport;
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
        public bool HideNAinGraph { get; set; } = true;
        private string[] _GraphLabels = new string[] { "90-100%", "80-89%", "70-79%", "60-69%", "50-59%", "40-49%", "30-39%", "20-29%", "10-19%", "0-9%", "N/A" };
        public string[] GraphLabels
        {
            get
            {
                return _GraphLabels;
            }
        }
        public int[] RCTGraphValues
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
        public Dictionary<string, string> RCTDataPoints
        {
            get
            {
                int[] vals = RCTGraphValues;
                Dictionary<string, string> res = new Dictionary<string, string>();
                res.Add(_GraphLabels[9], vals[9].ToString());
                res.Add(_GraphLabels[8], vals[8].ToString());
                res.Add(_GraphLabels[7], vals[7].ToString());
                res.Add(_GraphLabels[6], vals[6].ToString());
                res.Add(_GraphLabels[5], vals[5].ToString());
                res.Add(_GraphLabels[4], vals[4].ToString());
                res.Add(_GraphLabels[3], vals[3].ToString());
                res.Add(_GraphLabels[2], vals[2].ToString());
                res.Add(_GraphLabels[1], vals[1].ToString());
                res.Add(_GraphLabels[0], vals[0].ToString());
                if (!HideNAinGraph) res.Add(_GraphLabels[10], vals[10].ToString() );
                return res;
            }
        }

        public int[] HumanGraphValues
        {
            get
            {
                int v90 = 0, v80 = 0, v70 = 0, v60 = 0, v50 = 0, v40 = 0, v30 = 0, v20 = 0, v10 = 0, v0 = 0, vNA = 0;
                foreach (ReferenceRecord refr in Results)
                {
                    if (refr.Arrowsmith_Human_Score >= 0.9) v90++;
                    else if (refr.Arrowsmith_Human_Score >= 0.8) v80++;
                    else if (refr.Arrowsmith_Human_Score >= 0.7) v70++;
                    else if (refr.Arrowsmith_Human_Score >= 0.6) v60++;
                    else if (refr.Arrowsmith_Human_Score >= 0.5) v50++;
                    else if (refr.Arrowsmith_Human_Score >= 0.4) v40++;
                    else if (refr.Arrowsmith_Human_Score >= 0.3) v30++;
                    else if (refr.Arrowsmith_Human_Score >= 0.2) v20++;
                    else if (refr.Arrowsmith_Human_Score >= 0.1) v10++;
                    else if (refr.Arrowsmith_Human_Score >= 0.0) v0++;
                    else vNA++;
                }
                return new int[] { v90, v80, v70, v60, v50, v40, v30, v20, v10, v0, vNA };
            }
        }
        public Dictionary<string, string> HumanDataPoints
        {
            get
            {
                int[] vals = HumanGraphValues;
                Dictionary<string, string> res = new Dictionary<string, string>();
                res.Add(_GraphLabels[9], vals[9].ToString());
                res.Add(_GraphLabels[8], vals[8].ToString());
                res.Add(_GraphLabels[7], vals[7].ToString());
                res.Add(_GraphLabels[6], vals[6].ToString());
                res.Add(_GraphLabels[5], vals[5].ToString());
                res.Add(_GraphLabels[4], vals[4].ToString());
                res.Add(_GraphLabels[3], vals[3].ToString());
                res.Add(_GraphLabels[2], vals[2].ToString());
                res.Add(_GraphLabels[1], vals[1].ToString());
                res.Add(_GraphLabels[0], vals[0].ToString());
                if (!HideNAinGraph) res.Add(_GraphLabels[10], vals[10].ToString() );
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
