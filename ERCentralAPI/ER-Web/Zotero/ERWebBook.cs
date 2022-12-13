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

        public ERWebBook(IItem item)
        {
            _item = item;
        }

        // Need to use erweb mapping table to get fields on rhs into the correct lhs zotero fields 
        ZoteroCollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {
            try
            {
                var newZoteroItem = new BookWhole(_item, "", _item.Edition, _item.Country, _item.Publisher, 
                    _item.Pages,_item.StandardNumber);
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

//"itemType": "book",
//    "title": "",
//    "creators": [
//        {
//            "creatorType": "author",
//            "firstName": "",
//            "lastName": ""
//        }
//    ],
//    "abstractNote": "",
//    "series": "",
//    "seriesNumber": "",
//    "volume": "",
//    "numberOfVolumes": "",
//    "edition": "",
//    "place": "",
//    "publisher": "",
//    "date": "",
//    "numPages": "",
//    "language": "",
//    "ISBN": "",
//    "shortTitle": "",
//    "url": "",
//    "accessDate": "",
//    "archive": "",
//    "archiveLocation": "",
//    "libraryCatalog": "",
//    "callNumber": "",
//    "rights": "",
//    "extra": "",
//    "tags": [],
//    "collections": [],
//    "relations": { }
