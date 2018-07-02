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
        public string ListOfIDs
        {
            get
            {
                if (Results == null || Results.Count == 0) return "";
                return string.Join('¬', Results.Select(x => x.CitationId));
            }
                
        }//used to quickly re-fetch results when needed.
        public ReferenceListResult(string searchString, string searchMethod)
        {
            SearchMethod = searchMethod;
            SearchString = searchString;
        }
    }
}
