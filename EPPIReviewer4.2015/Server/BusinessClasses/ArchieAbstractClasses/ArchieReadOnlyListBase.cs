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

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Csla.Data;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public abstract class ArchieReadOnlyListBase<T, C> : ReadOnlyListBase<ArchieReadOnlyListBase<T,C>, ArchieReadOnlyBase<C>>
    {

#if SILVERLIGHT
    public ArchieReadOnlyListBase()
    { 
        
    }
#else
        public ArchieReadOnlyListBase()//needs to be public for some compiler reason
        {

        }
#endif


        //we can't add this as a moving part, we add a !SILVERLIGHT member instead
        //private static PropertyInfo<ArchieIdentity> IdentityProperty = r RegisterProperty<ArchieIdentity>(new PropertyInfo<ArchieIdentity>("Identity", "Identity"));
        //public ArchieIdentity Identity
        //{
        //    get
        //    {
        //        return GetProperty(IdentityProperty);
        //    }
        //    set
        //    {
        //        SetProperty(IdentityProperty, value);
        //    }
        //}

        

        

#if !SILVERLIGHT
        //lists don't contain members, only a list of children, hence, to make this type of object on server-side, we add this member
        internal ArchieIdentity _archieIdentity;
        public ArchieIdentity archieIdentity
        {
            get { return _archieIdentity; }
        }

#endif
    }
}