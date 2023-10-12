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

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    //public class ReviewStatisticsReviewerList : DynamicBindingListBase<ReviewStatisticsReviewer>
    public class ReviewStatisticsReviewerList2 : BusinessListBase<ReviewStatisticsReviewerList2, ReviewStatisticsReviewer2>
    {


        public ReviewStatisticsReviewerList2() { }


#if SILVERLIGHT
       
#else
        
        

#endif



    }
}
