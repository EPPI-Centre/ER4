using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla;
using Csla.Data;
using ERxWebClient2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ItemSetListController : CSLAController
    {
        //[HttpPost("[action]")]
        //public IActionResult Fetch([FromBody] SelCritMVC crit )
        //{
        //    SetCSLAUser();
        //    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

        //    DataPortal<ItemList> dp = new DataPortal<ItemList>();
        //    SelectionCriteria CSLAcrit = new SelectionCriteria();
        //    CSLAcrit.OnlyIncluded = crit.onlyIncluded;
        //    CSLAcrit.ShowDeleted = crit.showDeleted;
        //    CSLAcrit.SourceId = crit.sourceId;
        //    CSLAcrit.SearchId = crit.searchId;
        //    CSLAcrit.XAxisSetId = crit.xAxisSetId;
        //    CSLAcrit.XAxisAttributeId = crit.xAxisAttributeId;
        //    CSLAcrit.YAxisSetId = crit.yAxisSetId;
        //    CSLAcrit.YAxisAttributeId = crit.yAxisAttributeId;
        //    CSLAcrit.FilterSetId = crit.filterSetId;
        //    CSLAcrit.FilterAttributeId = crit.filterAttributeId;
        //    CSLAcrit.AttributeSetIdList = crit.attributeSetIdList;
        //    CSLAcrit.ListType = crit.listType;
        //    CSLAcrit.PageNumber = crit.pageNumber;
        //    CSLAcrit.PageSize = crit.pageSize;
        //    CSLAcrit.WorkAllocationId = crit.workAllocationId;
        //    CSLAcrit.ComparisonId = crit.comparisonId;
        //    CSLAcrit.Description = crit.description;
        //    CSLAcrit.ContactId = crit.contactId;
        //    CSLAcrit.SetId = crit.setId;
        //    CSLAcrit.ShowInfoColumn = crit.showInfoColumn;
        //    CSLAcrit.ShowScoreColumn = crit.showScoreColumn;
        //    ItemList result = dp.Fetch(CSLAcrit);
        //    return new ItemList4Json(result);
        //}
        
        
    }
}
