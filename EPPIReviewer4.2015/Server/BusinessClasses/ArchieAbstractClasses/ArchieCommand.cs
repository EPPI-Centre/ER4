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
    public abstract class ArchieCommand<T> : CommandBase<ArchieCommand<T>>
    {
        public static readonly PropertyInfo<Security.ArchieIdentity> IdentityProperty = RegisterProperty<Security.ArchieIdentity>(new PropertyInfo<Security.ArchieIdentity>("Identity", "Identity"));
        public Security.ArchieIdentity Identity
        {
            get
            {
                return ReadProperty(IdentityProperty);
            }
#if !SILVERLIGHT
            set
            {
                LoadProperty(IdentityProperty, value);
            }
#endif
        }
        public static readonly PropertyInfo<string> ArchieCodeProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieCode", "ArchieCode"));
        public string ArchieCode
        {
            get { return ReadProperty(ArchieCodeProperty); }
            set { LoadProperty(ArchieCodeProperty, value); }
        }
        public static readonly PropertyInfo<string> ArchieStateProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieState", "ArchieState"));
        public string ArchieState
        {
            get { return ReadProperty(ArchieStateProperty); }
            set { LoadProperty(ArchieStateProperty, value); }
        }
        internal  static void registerProperties()
        {
            //if (IdentityProperty == null) IdentityProperty = RegisterProperty<Security.ArchieIdentity>(new PropertyInfo<Security.ArchieIdentity>("Identity", "Identity"));
            //if (ArchieCodeProperty == null) ArchieCodeProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieCode", "ArchieCode"));
            //if (ArchieStateProperty == null) ArchieStateProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieState", "ArchieState"));
        }
    }
}
