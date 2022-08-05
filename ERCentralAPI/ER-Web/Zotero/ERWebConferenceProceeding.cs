using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Zotero
{
    public class ERWebConferenceProceeding :ERWebCreators,  IMapERWebReference
    {
        private Item _item;

        public ERWebConferenceProceeding(Item item)
        {
            _item = item;
        }

        CollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {
            try
            {
        
                ConferencePaper conferencePaper = new ConferencePaper();

                conferencePaper.title = _item.Title;
                conferencePaper.itemType = "conferencePaper";
                conferencePaper.title = _item.Title;
                conferencePaper.creators = ObtainCreators(_item.Authors).ToArray();
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
                conferencePaper.tags = new List<tagObject>() { tag };
                conferencePaper.collections = new Object[0]; 
                conferencePaper.abstractNote = _item.Abstract;
                conferencePaper.accessDate = "";
                conferencePaper.archive = "";
                conferencePaper.archiveLocation = "";
                conferencePaper.callNumber = "";
                conferencePaper.date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                conferencePaper.dateAdded = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                conferencePaper.dateModified = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                conferencePaper.extra = "";
                conferencePaper.language = _item.Country;
                conferencePaper.libraryCatalog = "";
                conferencePaper.relations = rel;
                conferencePaper.rights = "";
                conferencePaper.series = "";
                conferencePaper.seriesNumber = "";
                conferencePaper.shortTitle = _item.ShortTitle;
                conferencePaper.url = _item.URL;
                conferencePaper.volume = _item.Volume;

                return conferencePaper;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }
    }
}
