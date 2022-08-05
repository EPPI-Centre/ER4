using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Zotero
{
    public class ERWebPeriodical : ERWebCreators, IMapERWebReference
    {
        private Item _item;

        public ERWebPeriodical(Item item)
        {
            _item = item;
        }

        // Need to use erweb mapping table to get fields on rhs into the correct lhs zotero fields 
        CollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {
            try
            {
                BookWhole newZoteroItem = new BookWhole();
                newZoteroItem.title = _item.Title;
                newZoteroItem.itemType = "book"; 
                newZoteroItem.creators = ObtainCreators(_item.Authors);
                tagObject tag = new tagObject
                {
                    tag = "awesome:",
                    type = "1"
                };
                relation rel = new relation // TODO hardcoded
                {
                    owlSameAs = "http://zotero.org/groups/1/items/JKLM6543",
                    dcRelation = "http://zotero.org/groups/1/items/PQRS6789",
                    dcReplaces = "http://zotero.org/users/1/items/BCDE5432"
                };
                newZoteroItem.tags = new List<tagObject>() { tag };
                newZoteroItem.collections = new List<string>() { };
                newZoteroItem.abstractNote = _item.Abstract;
                newZoteroItem.accessDate = "";
                newZoteroItem.archive = "";
                newZoteroItem.archiveLocation = "";
                newZoteroItem.callNumber = "";
                newZoteroItem.date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                newZoteroItem.dateAdded = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                newZoteroItem.dateModified = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                newZoteroItem.edition = _item.Edition;
                newZoteroItem.extra = "";
                newZoteroItem.ISBN = _item.DOI;
                newZoteroItem.language = _item.Country;
                newZoteroItem.libraryCatalog = "";
                newZoteroItem.numberOfVolumes = "";
                newZoteroItem.numPages = _item.Pages;
                newZoteroItem.place = _item.City;
                newZoteroItem.publisher = _item.Publisher;
                newZoteroItem.relations = rel;
                newZoteroItem.rights = "";
                newZoteroItem.series = "";
                newZoteroItem.seriesNumber = "";
                newZoteroItem.shortTitle = _item.ShortTitle;
                newZoteroItem.url = _item.URL;
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
