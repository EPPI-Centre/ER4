using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;

using Csla.Data;
//using Csla.Validation;

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [Csla.Server.MobileFactory("BusinessLibrary.BusinessClasses.AttributeTypesFactory, BusinessLibrary", "FetchAttributeTypes")]
    public class AttributeTypes : NameValueListBase<int, string>
    {
#if SILVERLIGHT
    public AttributeTypes() { }

    public static void GetAttributeTypes(EventHandler<DataPortalResult<AttributeTypes>> handler)
    {
      DataPortal<AttributeTypes> dp = new DataPortal<AttributeTypes>();
      dp.FetchCompleted += handler;
      dp.BeginFetch();
    }
#else 
        
        private void Child_Fetch(SafeDataReader reader)
        {
            this.Add(new AttributeTypes.NameValuePair(reader.GetInt32("ATTRIBUTE_TYPE_ID"), reader.GetString("ATTRIBUTE_TYPE")));
        }
#endif

        internal void SetReadOnlyFlag(bool flag)
        {
            IsReadOnly = flag;
        }

        public string GetAttributeTypeName(int AttributeType)
        {
            string returnValue = string.Empty;
            foreach (var oneItem in this)
            {
                if (oneItem.Key == AttributeType)
                {
                    returnValue = oneItem.Value;
                    break;
                }
            }
            return returnValue;
        }
    }
}
