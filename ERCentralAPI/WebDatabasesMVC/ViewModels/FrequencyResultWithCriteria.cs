using BusinessLibrary.BusinessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDatabasesMVC.Controllers;

namespace BusinessLibrary.BusinessClasses
{
    public class FrequencyResultWithCriteria
    {
        public WebDbItemAttributeChildFrequencyList results { get; set; }
        public WebDbFrequencyCrosstabAndMapSelectionCriteria criteria { get; set; }
    }
}
