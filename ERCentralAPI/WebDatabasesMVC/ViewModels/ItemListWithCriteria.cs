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
        public ItemList items { get; set; }
        public SelCritMVC criteria { get; set; }
    }
}
