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
        ReferenceListResult(string searchString, string searchMethod)
        {
            SearchMethod = searchMethod;
            SearchString = searchString;
        }
        public void BuildResultsList(SqlDataReader reader)
        {

        }
    }
}
