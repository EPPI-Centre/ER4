using BusinessLibrary.BusinessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDatabasesMVC.Controllers;

namespace BusinessLibrary.BusinessClasses
{
    public class ItemListWithCriteria
    {
        public ItemList4Json items { get; set; }
        public SelCritMVC criteria { get; set; }
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
        public int totalItemCount
        {
            get { return _list.TotalItemCount; }
        }
        public ItemList Items
        {
            get { return _list; }
        }
        public ItemList4Json(ItemList list)
        { _list = list; }
    }
}
