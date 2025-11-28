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
    /// <summary>
    /// LongLastingFireAndForgetCommand (or other long-lasting task workers) can implement this interface to signal that they can be resumed.
    /// LongLastingTaskResumer object runs at app startup and looks for tasks to resume (as List<RawTaskToResume>, populated from tb_REVIEW_JOB)
    /// If the class used to run a given task implements LongLastingTaskResumer, it then calls its ResumeJob(...) method and moves onto the next task in the list
    /// Therefore, iResumableLongLastingTask object need to:
    ///     - Implement logic to shutdown gracefully, and in doing so, save to tb_REVIEW_JOB all the data it needs to resume correctly.
    ///         [The interface itself doesn't specify this, hence this Summary!]
    ///     - In ResumeJob(...) implement all the logic needed to actually resume. 
    ///         LongLastingTaskResumer will "fire and forget" the ResumeJob(...) method, so knows nothing of what happens therein.
    /// </summary>
    public interface iResumableLongLastingTask
    {
#if !SILVERLIGHT
        public void ResumeJob(ER_Web.Services.RawTaskToResume rtrs);
#endif
    }
}
