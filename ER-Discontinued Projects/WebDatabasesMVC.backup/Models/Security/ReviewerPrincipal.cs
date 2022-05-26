using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using System.Security.Principal;
using System.Security.Claims;

namespace BusinessLibrary.Security
{
   
    [Serializable]
    public class ReviewerPrincipal : CslaPrincipal
    {
        public ReviewerPrincipal(IIdentity identity)
            : base(identity)
        { }

        public ReviewerPrincipal() : base() { }



        public static void Login(ClaimsPrincipal user)
        {
            ReviewerIdentity.GetIdentity(user);
        }


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

        public static void Logout()
        {
            Csla.Security.UnauthenticatedIdentity identity = ReviewerIdentity.UnauthenticatedIdentity();
            ReviewerPrincipal principal = new ReviewerPrincipal(identity);
            Csla.ApplicationContext.User = principal;
        }

        public override bool IsInRole(string role)
        {
            return ((ICheckRoles)base.Identity).IsInRole(role);
        }
    }
}
