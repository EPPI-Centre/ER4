using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace ERxWebClient2.Controllers
{
    public abstract class CSLAController : Controller
    {
        /// <summary>
        /// This method is used to populate Csla.ApplicationContext.User, which is necessary because it is different from 
        /// the MVC user that we get when a JWT is added to the request headers.
        /// This method has to be called within a controller whenever it relies on a BO that uses the ReviewerIdentity object.
        /// Therein, if the ReviewerIdentity object is used to get the ReviewId, it will check the validity of the session,
        /// based on "Ticket" associated with currently logged on user and the corresponding review ((ReviewerAdmin)TB_LOGON_TICKET).
        /// </summary>
        /// <returns>bool</returns>
        protected bool SetCSLAUser()
        {
            try
            {
                ReviewerIdentity ri = ReviewerIdentity.GetIdentity(User);
				int ReviewID = ri.ReviewId;//this ensures current ticket is valid (goes to DB to check!)
                ReviewerPrincipal principal = new ReviewerPrincipal(ri);
                Csla.ApplicationContext.User = principal;
                return true;//we might want to do more checks!
            }
            catch (Exception e)
            {//to be logged!
                return false;
            }
        }
		protected bool SetCSLAUser4Writing()
		{
			try
			{
				ReviewerIdentity ri = ReviewerIdentity.GetIdentity(User);
				int ReviewID = ri.ReviewId;//this ensures current ticket is valid (goes to DB to check!)
				if (ri.HasWriteRights())
				{//all is well, ticket is valid and user has write access
					ReviewerPrincipal principal = new ReviewerPrincipal(ri);
					Csla.ApplicationContext.User = principal;
					return true;
				}
				else throw new Exception("User does not have write rights");
			}
			catch (Exception e)
			{//to be logged!
				return false;
			}
		}
	}
    public class SingleStringCriteria
    {
        public string Value { get; set; }
    }
    public class SingleIntCriteria
    {
        public int Value { get; set; }
    }
    public class SingleInt64Criteria
    {
        public Int64 Value { get; set; }
    }
    public class SingleBoolCriteria
    {
        public bool Value { get; set; }
    }
}