using Csla;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
namespace BusinessLibrary.Security.Extensions
{
//    public static class SecurityExtensions
//    {
//        public static ReviewerIdentityWebClient GetReviewerIdentity(this Csla.IBusinessBase me)
//        {
//#if (!SILVERLIGHT && !CSLA_NETCORE)
//            return Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//#elif (CSLA_NETCORE)
//            var User =   me.;
//            var fff = User.Claims.First(c => c.Type == "userId").Value;
//            int cID;
//            if (!int.TryParse(fff, out cID))
//            {
//                return null;
//            }
//            ReviewerIdentityWebClient ri = ReviewerIdentityWebClient.GetIdentity(cID, RevId);
//#endif
//        }
//    }
}