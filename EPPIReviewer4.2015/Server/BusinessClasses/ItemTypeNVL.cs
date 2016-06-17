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
    [MobileFactory("BusinessLibrary.BusinessClasses.ItemTypeNVLFactory, BusinessLibrary", "FetchItemTypeNVL")]
    public class ItemTypeNVL : NameValueListBase<int, string>
    {
#if SILVERLIGHT
    public ItemTypeNVL() { }

    public static void GetItemTypeNVL(EventHandler<DataPortalResult<ItemTypeNVL>> handler)
    {
      DataPortal<ItemTypeNVL> dp = new DataPortal<ItemTypeNVL>();
      dp.FetchCompleted += handler;
      dp.BeginFetch();
    }
#endif

        internal void SetReadOnlyFlag(bool flag)
        {
            IsReadOnly = flag;
        }

        public string GetItemTypeName(int ItemTypeId)
        {
            string returnValue = string.Empty;
            foreach (var oneItem in this)
            {
                if (oneItem.Key == ItemTypeId)
                {
                    returnValue = oneItem.Value;
                    break;
                }
            }
            return returnValue;
        }
    }
}
