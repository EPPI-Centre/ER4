using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using Csla.Server;
//using Csla.Validation;

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [MobileFactory("BusinessLibrary.BusinessClasses.ReviewContactNVLFactory, BusinessLibrary", "FetchReviewContactNVL")]
    public class ReviewContactNVL : NameValueListBase<int, string>
    {
#if SILVERLIGHT
    public ReviewContactNVL() { }

    public static void GetReviewContactNVL(EventHandler<DataPortalResult<ReviewContactNVL>> handler)
    {
      DataPortal<ReviewContactNVL> dp = new DataPortal<ReviewContactNVL>();
      dp.FetchCompleted += handler;
      dp.BeginFetch();
    }
#endif

        internal void SetReadOnlyFlag(bool flag)
        {
            IsReadOnly = flag;
        }

        public string GetContactName(int ContactId)
        {
            string returnValue = string.Empty;
            foreach (var oneItem in this)
            {
                if (oneItem.Key == ContactId)
                {
                    returnValue = oneItem.Value;
                    break;
                }
            }
            return returnValue;
        }
    }
}
