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
    public abstract class LongLastingFireAndForgetCommand<T> : CommandBase<LongLastingFireAndForgetCommand<T>>
    {
#if !SILVERLIGHT
        protected Boolean AppIsShuttingDown
        {
            get
            {
#if CSLA_NETCORE
                try { return Program.AppIsShuttingDown; }
                catch { return false; }
#else
                return false;
#endif
            }
        }
        protected System.Threading.CancellationToken CancelToken
        { 
            get 
            {
#if CSLA_NETCORE
                if (Program.TokenSource != null)
                {
                    return Program.TokenSource.Token;
                }
                else return default(CancellationToken);
#else
                return  default(System.Threading.CancellationToken);
#endif
            }
        }
        protected virtual void ErrorLogSink(string Message)
        {
#if CSLA_NETCORE
            try
            {
                if (Program.Logger != null) Program.Logger.Error(Message);
            }
            catch { }
#endif
        }
#endif
    }
}
