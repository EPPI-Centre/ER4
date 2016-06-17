using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.Globalization;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Text.RegularExpressions;
using System.Xml;
using BusinessLibrary.s.uk.ac.nactem.www;
using System.Data.SqlTypes;

using Microsoft.SqlServer.Server;
using Microsoft.SqlServer;
//using Microsoft.SqlServer.Dts.Runtime;

using System.Web;
using System.Net;
using System.IO;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MetaAnalysisModeratorReferenceList : DynamicBindingListBase<MetaAnalysisModeratorReference>
    {



#if SILVERLIGHT
        public MetaAnalysisModeratorReferenceList() { }
        
#else
        private MetaAnalysisModeratorReferenceList() { }
#endif


#if !SILVERLIGHT

        public static MetaAnalysisModeratorReferenceList GetMetaAnalysisModeratorReferenceList()
        {
            MetaAnalysisModeratorReferenceList theList = new MetaAnalysisModeratorReferenceList();
            return theList;
        }

#endif



    }


}
