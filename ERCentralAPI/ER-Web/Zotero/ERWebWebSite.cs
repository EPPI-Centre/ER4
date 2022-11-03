//using BusinessLibrary.BusinessClasses;
//using ERxWebClient2.Controllers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//parentTitle: { txt: 'Parent Title', optional: true }
//                , parentAuthors: { txt: 'Parent Authors', optional: true }
//                , standardNumber: { txt: 'Standard Number', optional: true }


//namespace ERxWebClient2.Zotero
//{
//    public class ERWebWebSite : ERWebCreators, IMapERWebReference
//    {
//        private Item _item;

//        public ERWebWebSite(Item item)
//        {
//            _item = item;
//        }

//        // Need to use erweb mapping table to get fields on rhs into the correct lhs zotero fields 
//        CollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
//        {
//            try
//            {

//                WebSite newZoteroItem = new WebSite
//                {
//                    blogTitle = _item.Title,
//                    websiteType = _item.TypeName,
//                    itemType = "blogPost",
//                    creators = new List<CreatorsItem>(),
//                    abstractNote = _item.Abstract,
//                    accessDate = "",
//                    archive = "",
//                    archiveLocation = "",
//                    callNumber = "",
//                    date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
//                    dateAdded = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
//                    dateModified = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
//                    extra = "",
//                    language = _item.Country,
//                    libraryCatalog = "",
//                    rights = "",
//                    series = "",
//                    seriesNumber = "",
//                    shortTitle = _item.ShortTitle,
//                    url = _item.URL,
//                    volume = _item.Volume,
//                };          
//                newZoteroItem.creators = ObtainCreators(_item.Authors);
//                tagObject tag = new tagObject
//                {
//                    tag = "awesome:",
//                    type = "1"
//                };
//                relation rel = new relation // TODO hardcoded
//                {
//                    owlSameAs = "http://zotero.org/groups/1/items/JKLM6543",
//                    dcRelation = "http://zotero.org/groups/1/items/PQRS6789",
//                    dcReplaces = "http://zotero.org/users/1/items/BCDE5432"
//                };
//                newZoteroItem.tags = new List<tagObject>() { tag };
//                newZoteroItem.relations = rel;
//                return newZoteroItem;
//            }
//            catch (System.Exception ex)
//            {
//                var detailOfError = ex;
//                throw;
//            }
//        }
//    }
//}
