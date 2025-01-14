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
using System.ComponentModel;
using Newtonsoft.Json;

#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ReadOnlySetType : ReadOnlyBase<ReadOnlySetType>
    {
        public ReadOnlySetType() { }
        public override string ToString()
        {
            return SetTypeName;
        }
        public static readonly PropertyInfo<int> SetTypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetTypeId", "SetTypeId", 0));

        [JsonProperty]
        public int SetTypeId
        {
            get
            {
                return GetProperty(SetTypeIdProperty);
            }
        }
        public static readonly PropertyInfo<string> SetTypeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetTypeName", "SetTypeName", string.Empty));
        [JsonProperty]
        public string SetTypeName
        {
            get
            {
                return GetProperty(SetTypeNameProperty);
            }
        }
        public static readonly PropertyInfo<string> SetTypeDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("SetTypeDescription", "SetTypeDescription", string.Empty));
        [JsonProperty]
        public string SetTypeDescription
        {
            get
            {
                return GetProperty(SetTypeDescriptionProperty);
            }
        }
        public static readonly PropertyInfo<bool> AllowComparisonProperty = RegisterProperty<bool>(new PropertyInfo<bool>("AllowComparison", "AllowComparison"));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public bool AllowComparison
        {
            get
            {
                return GetProperty(AllowComparisonProperty);
            }
        }
        public static readonly PropertyInfo<bool> AllowRandomAllocationProperty = RegisterProperty<bool>(new PropertyInfo<bool>("AllowRandomAllocation", "AllowRandomAllocation"));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public bool AllowRandomAllocation
        {
            get
            {
                return GetProperty(AllowRandomAllocationProperty);
            }
        }
        
        public static readonly PropertyInfo<int> MaxDepthProperty = RegisterProperty<int>(new PropertyInfo<int>("MaxDepth", "MaxDepth", 0));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public int MaxDepth
        {
            get
            {
                return GetProperty(MaxDepthProperty);
            }
        }
        public static readonly PropertyInfo<AttributeTypes> AllowedCodeTypesProperty = RegisterProperty<AttributeTypes>(new PropertyInfo<AttributeTypes>("AllowedCodeTypes", "AllowedCodeTypes"));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public AttributeTypes AllowedCodeTypes
        {
            get
            {
                return GetProperty(AllowedCodeTypesProperty);
            }
        }
        public static readonly PropertyInfo<MobileList<int>> AllowedSetTypesID4PasteProperty = RegisterProperty<MobileList<int>>(new PropertyInfo<MobileList<int>>("AllowedSetTypesID4Paste", "AllowedSetTypesID4Paste"));
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
        public MobileList<int> AllowedSetTypesID4Paste
        {
            get
            {
                return GetProperty(AllowedSetTypesID4PasteProperty);
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyReview), canRead);
        //    //AuthorizationRules.AllowRead(ReviewNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //}
#if !SILVERLIGHT
        public static ReadOnlySetType GetReadOnlySetType(SafeDataReader reader)
        {
            return DataPortal.FetchChild<ReadOnlySetType>(reader);
        }
        private void Child_Fetch(SafeDataReader reader)
        {
            LoadProperty<int>(SetTypeIdProperty, reader.GetInt32("SET_TYPE_ID"));
            LoadProperty<string>(SetTypeNameProperty, reader.GetString("SET_TYPE"));
            LoadProperty<string>(SetTypeDescriptionProperty, reader.GetString("SET_DESCRIPTION"));
            LoadProperty<bool>(AllowComparisonProperty, reader.GetBoolean("ALLOW_COMPARISON"));
            LoadProperty<bool>(AllowRandomAllocationProperty, reader.GetBoolean("ACCEPTS_RANDOM_ALLOCATIONS"));
            LoadProperty<int>(MaxDepthProperty, reader.GetInt32("MAX_DEPTH"));
        }
        public void AddAllowedCodeType(int CodeTypeID, string CodeTypeName)
        {
            
            if (AllowedCodeTypes == null)
            {
                LoadProperty(AllowedCodeTypesProperty, new AttributeTypes());
            }
            AllowedCodeTypes.RaiseListChangedEvents = false;
            AllowedCodeTypes.SetReadOnlyFlag(false);
            AllowedCodeTypes.Add(new AttributeTypes.NameValuePair(CodeTypeID, CodeTypeName));
            AllowedCodeTypes.RaiseListChangedEvents = true;
            AllowedCodeTypes.SetReadOnlyFlag(true);
        }
        public void AddAllowedSetTypesID4Paste(int SetTypeID)
        {
            if (AllowedSetTypesID4Paste == null)
            {
                LoadProperty(AllowedSetTypesID4PasteProperty, new MobileList<int>());
            }
            AllowedSetTypesID4Paste.Add(SetTypeID);
        }

#endif
    }
}