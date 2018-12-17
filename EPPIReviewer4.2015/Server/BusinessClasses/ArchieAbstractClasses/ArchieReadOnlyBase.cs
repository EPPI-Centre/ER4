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
using Csla.Rules.CommonRules;
using Csla.Rules;
using BusinessLibrary.Security;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public  class ArchieReadOnlyBase<T> : ReadOnlyBase<ArchieReadOnlyBase<T>>
    {

#if SILVERLIGHT
    public ArchieReadOnlyBase()
    { 
        
    }
#else
        public ArchieReadOnlyBase()//needs to be public for some compiler reason
        {

        }
#endif

        //something in the generic inheritance doesn't work, as a result, the ArchieIdentity object needs to be placed in the final class, for one reason or the other
        //also, I couldn't make the Readonlylist work without passing through this now somewhat reduntant virtual class, not really sure why.

//        private static PropertyInfo<ArchieIdentity> IdentityProperty = RegisterProperty<ArchieIdentity>(new PropertyInfo<ArchieIdentity>("Identity", "Identity"));
//        public ArchieIdentity Identity
//        {
//            get
//            {
//                return GetProperty(IdentityProperty);
//            }
//#if !SILVERLIGHT
//            set
//            {
//                LoadProperty(IdentityProperty, value);
//            }
//#endif        
//        }

        

        
#if !SILVERLIGHT


#endif
    }
}