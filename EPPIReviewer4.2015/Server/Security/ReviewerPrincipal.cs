using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using System.Security.Principal;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.Security
{
   
    [Serializable]
    public class ReviewerPrincipal : CslaPrincipal
    {
        public ReviewerPrincipal(IIdentity identity)
            : base(identity)
        { }

        public ReviewerPrincipal() : base() { }

#if SILVERLIGHT

    public static void Login(string username, string password, int reviewId, string LoginMode, EventHandler<EventArgs> completed)
    {
      ReviewerIdentity.GetIdentity(username, password, reviewId, LoginMode, (o, e) =>
      {
        if (e.Object == null)
        {
          SetPrincipal(ReviewerIdentity.UnauthenticatedIdentity());
        }
        else
        {
          SetPrincipal(e.Object);
        }
        completed(null, EventArgs.Empty);
      });
    }
    public static void Login(string ArchieCode, string Status, string LoginMode, int reviewId, EventHandler<EventArgs> completed)
    {//changed parameters order so to have a different signature this is used for a normal logon via Archie
        ReviewerIdentity.GetIdentity(ArchieCode, Status, LoginMode, reviewId, (o, e) =>
        {
            if (e.Object == null)
            {
                SetPrincipal(ReviewerIdentity.UnauthenticatedIdentity());
            }
            else
            {
                SetPrincipal(e.Object);
            }
            completed(null, EventArgs.Empty);
        });
    }
    public static void Login(string Username, string Password, string ArchieCode, string Status, string LoginMode, int reviewId, EventHandler<EventArgs> completed)
    {//special case, user is linking an existing ER4 acccount to an Archie one
        ReviewerIdentity.GetIdentity(Username, Password, ArchieCode, Status, LoginMode, reviewId, (o, e) =>
        {
            if (e.Object == null)
            {
                SetPrincipal(ReviewerIdentity.UnauthenticatedIdentity());
            }
            else
            {
                SetPrincipal(e.Object);
            }
            completed(null, EventArgs.Empty);
        });
    }

#elif (!CSLA_NETCORE)
        public static void Login(string username, string password, int reviewId, string roles, string LoginMode)
        {
            ReviewerIdentity.GetIdentity(username, password, reviewId, roles, LoginMode);
        }
#elif (CSLA_NETCORE)
        public static void Login(string username, string password, int reviewId, string roles, string LoginMode)
        {
            ReviewerIdentityWebClient.GetIdentity(username, password, reviewId, roles, LoginMode);
        }
#endif

        private static void SetPrincipal(Csla.Security.CslaIdentity identity)
        {
            ReviewerPrincipal principal = new ReviewerPrincipal(identity);
            Csla.ApplicationContext.User = principal;
        }
        private static void SetPrincipal(Csla.Security.UnauthenticatedIdentity identity)
        {
            ReviewerPrincipal principal = new ReviewerPrincipal(identity);
            Csla.ApplicationContext.User = principal;
        }
#if (!CSLA_NETCORE)
        public static void Logout()
        {
            Csla.Security.UnauthenticatedIdentity identity = ReviewerIdentity.UnauthenticatedIdentity();
            ReviewerPrincipal principal = new ReviewerPrincipal(identity);
            Csla.ApplicationContext.User = principal;
        }
#else 
        public static void Logout()
        {
            Csla.Security.UnauthenticatedIdentity identity = ReviewerIdentityWebClient.UnauthenticatedIdentity();
            ReviewerPrincipal principal = new ReviewerPrincipal(identity);
            Csla.ApplicationContext.User = principal;
        }
#endif
        public override bool IsInRole(string role)
        {
            return ((ICheckRoles)base.Identity).IsInRole(role);
        }
    }
}
