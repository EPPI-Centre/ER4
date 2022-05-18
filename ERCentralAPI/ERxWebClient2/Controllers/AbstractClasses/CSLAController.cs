using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using System.Data.SqlClient;
using BusinessLibrary.BusinessClasses;


namespace ERxWebClient2.Controllers
{
    public abstract class CSLAController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected readonly ILogger _logger;
        protected CSLAController(ILogger logger)
        {
            _logger = logger;
        }
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
#if WEBDB
                SetViewBag();
#endif
                return true;//we might want to do more checks!
            }
            catch (Exception e)
            {//to be logged!
                _logger.LogError(e, "SetCSLAUser failure");
                Csla.ApplicationContext.User = null;
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
                _logger.LogError(e, "SetCSLAUser4Writing failure");
                return false;
			}
		}
#if WEBDB
        protected int WebDbId
        {
            get
            {
                if (User == null || (Csla.ApplicationContext.User.Identity as ReviewerIdentity) == null) return -1;
                else
                {
                    List<Claim> claims = User.Claims.ToList();
                    Claim DBidC = claims.Find(f => f.Type == "WebDbID");
                    try
                    {
                        return int.Parse(DBidC.Value);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Error parsing the WebDBId value from the logon cookie", e);
                        return -1;
                    }
                }
            }
        }

        protected int ReviewID
        {
            get
            {
                try
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    if (User == null || ri == null) return -1;
                    else
                    {
                        return ri.ReviewId;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("Error getting ReviewerIdentity or ReviewId therein.", e);
                    return -1;
                }
            }
        }

        private void SetViewBag()
        {
            ViewBag.WebDbId = WebDbId;
            string WebDbTitle = "Unknown";
            if (User != null && (Csla.ApplicationContext.User.Identity as ReviewerIdentity) != null) 
            {
                List<Claim> claims = User.Claims.ToList();
                Claim DBn = claims.Find(f => f.Type == ClaimTypes.Name);
                if (DBn != null) WebDbTitle = DBn.Value;
                ViewBag.WebDbName = WebDbTitle;
            }

        }
        //this method is here, so to allow accessing it from EPPI-Vis, from both the regular controllers and the FAIR one.
        internal WebDatabasesMVC.ViewModels.FullItemDetails GetItemDetails(WebDatabasesMVC.Controllers.ItemSelCritMVC crit)
        {
            Item itm = DataPortal.Fetch<Item>(new SingleCriteria<Item, Int64>(crit.itemID));
            ItemArmList arms = DataPortal.Fetch<ItemArmList>(new SingleCriteria<Item, Int64>(crit.itemID));
            itm.Arms = arms;
            ItemTimepointList timepoints = DataPortal.Fetch<ItemTimepointList>(new SingleCriteria<Item, Int64>(crit.itemID));
            ItemDocumentList docs = DataPortal.Fetch<ItemDocumentList>(new SingleCriteria<ItemDocumentList, Int64>(crit.itemID));
            ReadOnlySource ros = DataPortal.Fetch<ReadOnlySource>(new SingleCriteria<ReadOnlySource, long>(crit.itemID));
            ItemDuplicatesReadOnlyList dups = DataPortal.Fetch<ItemDuplicatesReadOnlyList>(new SingleCriteria<ItemDuplicatesReadOnlyList, long>(crit.itemID));
            WebDatabasesMVC.ViewModels.FullItemDetails res = new WebDatabasesMVC.ViewModels.FullItemDetails
            {
                Item = itm,
                Documents = docs,
                Timepoints = timepoints,
                Duplicates = dups,
                Source = ros,
                ListCrit = crit as WebDatabasesMVC.Controllers.SelCritMVC,
                ItemIds = crit.itemIds
            };
            return res;
        }
        //as above, placing this method here, so to allow accessing it from EPPI-Vis, from both the regular controllers and the FAIR one. 
        internal ItemListWithCriteria GetItemList(SelectionCriteria crit)
        {
            if (crit.WebDbId == 0)
            {
                crit.WebDbId = WebDbId;
                crit.PageSize = 100;
            }
            else if (WebDbId != crit.WebDbId)
            {
                throw new Exception("WebDbId in ItemList Criteria is not the expected value - possible tampering attempt!");
            }

            if (crit.ListType == "StandardItemList")
            {
                crit.ListType = "WebDbAllItems";
                crit.OnlyIncluded = true;
                crit.Description = "All Items.";
            }
            else if (!crit.ListType.StartsWith("WebDb"))
            {
                throw new Exception("Not supported ListType (" + crit.ListType + ") possible tampering attempt!");
            }
            ItemList4Json res = new ItemList4Json(DataPortal.Fetch<ItemList>(crit));
            return new ItemListWithCriteria { items = res, criteria = new WebDatabasesMVC.Controllers.SelCritMVC(crit) };
        }


        protected void logActivity(string type, string details)
        {
            CSLAController.logActivityStatic(type, details, WebDbId, ReviewID);
        }
        public static void logActivityStatic(string type, string details, int WebDbId, int ReviewID)
        {
            //making this public and static, so we can call it from LoginController, which does not inherit from CSLAController
            string SP1 = "st_WebDBWriteToLog";
            List<SqlParameter> pars1 = new List<SqlParameter>();
            pars1.Add(new SqlParameter("@WebDBid", WebDbId));
            pars1.Add(new SqlParameter("@Reviewid", ReviewID));
            pars1.Add(new SqlParameter("@Type", type));
            pars1.Add(new SqlParameter("@Details", details));

            int result = WebDatabasesMVC.Program.SqlHelper.ExecuteNonQuerySP(WebDatabasesMVC.Program.SqlHelper.ER4AdminDB, SP1, pars1.ToArray());
            if (result == -2)
            {
                Console.WriteLine("Unable to write to WebDB log");
            }
        }
#endif
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