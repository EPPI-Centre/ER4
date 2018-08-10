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
    public class ItemListController : CSLAController
    {

        [HttpGet("[action]")]
        public ItemList4Json IncludedItems()//should receive a reviewID!
        {
            SetCSLAUser();
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            DataPortal<ItemList> dp = new DataPortal<ItemList>();            
            SelectionCriteria crit = new SelectionCriteria();
            //crit = new SelectionCriteria();
            crit.ListType = "StandardItemList";
            crit.OnlyIncluded = true;
            crit.ShowDeleted = false;
            crit.AttributeSetIdList = "";
            crit.PageSize = 5;
            crit.PageNumber = 0;
            ItemList result = dp.Fetch(crit);
            return new ItemList4Json(result);
        } 
    }
    public class ItemList4Json
    {
        private ItemList _list;
        public int pagesize
        {
            get { return _list.PageSize; }
        }
        public int pagecount
        {
            get { return _list.PageCount; }
        }
        public int pageindex
        {
            get { return _list.PageIndex; }
        }

        public List<Item> Items
        {
            get { return _list.ToList(); }
        }
        public ItemList4Json(ItemList list)
        { _list = list; }
    }
}
