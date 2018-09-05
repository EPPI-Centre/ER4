using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ERxWebClient2.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    public class ItemDocumentListController : CSLAController
    {

        [HttpPost("[action]")]
        public ItemDocumentList GetDocuments([FromBody] SingleInt64Criteria ItemIDCrit)
        {

            SetCSLAUser();

            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            DataPortal<ItemDocumentList> dp = new DataPortal<ItemDocumentList>();
            SingleCriteria<ItemDocumentList, Int64> criteria = new SingleCriteria<ItemDocumentList, Int64>(ItemIDCrit.Value);

            ItemDocumentList result = dp.Fetch(criteria);
            
            return result;

        }
    }
}
