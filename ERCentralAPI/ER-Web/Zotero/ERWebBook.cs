using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Zotero
{
    public class ERWebBook : ERWebCreators, IMapERWebReference
    {      

        public ERWebBook(Item item)
        {
            _item = item;
        }

        // Need to use erweb mapping table to get fields on rhs into the correct lhs zotero fields 
        CollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {
            try
            {
                var newZoteroItem = new BookWhole(_item, "", _item.Edition, _item.City, _item.Publisher, 
                    _item.Pages,_item.DOI);
      
                return newZoteroItem;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }
    }
}
