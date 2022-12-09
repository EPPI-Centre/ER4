using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Zotero
{
    public class ERWebBookChapter : ERWebCreators, IMapERWebReference
    {
        public ERWebBookChapter(IItem item)
        {
            _item = item;
        }

        // Need to use erweb mapping table to get fields on rhs into the correct lhs zotero fields 
        ZoteroCollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {

            try
            {

                var newZoteroItem = new BookChapter(_item, _item.ParentTitle, "", _item.Edition, _item.City,
                    _item.Publisher, _item.Pages, _item.StandardNumber);
                //newZoteroItem.itemType = "bookSection";      
                newZoteroItem.volume = _item.Volume;
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
