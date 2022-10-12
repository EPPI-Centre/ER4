using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Zotero
{
    public class ERWebJournal : ERWebCreators, IMapERWebReference
    {
        private Item _item;

        public ERWebJournal(Item item)
        {
            _item = item;
        }

        CollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {
            JournalArticle journal = new JournalArticle();

            try
            {
                journal.title = _item.Title;
                journal.itemType = "journalArticle";
                journal.creators = ObtainCreators(_item.Authors).ToArray();      
                
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
                journal.tags = new List<tagObject>() { tag };
                journal.collections = new Object[0];
                journal.abstractNote = _item.Abstract;
                journal.accessDate = "";
                journal.archive = "";
                journal.archiveLocation = "";
                journal.callNumber = "";
                journal.archiveLocation = _item.Country;
                journal.DOI = _item.DOI;
                journal.date = _item.Year;
                journal.dateAdded = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                journal.dateModified = ((DateTime)_item.DateEdited).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                journal.series = _item.Edition;
                journal.extra = "";
                journal.language = _item.Country;
                journal.libraryCatalog = "";
                journal.pages = _item.Pages;
                journal.publicationTitle = _item.Publisher;
                journal.relations = rel;
                journal.rights = "";
                journal.series = "";
                journal.seriesNumber = "";
                journal.shortTitle = _item.ShortTitle;
                journal.url = _item.URL;
                journal.volume = _item.Volume;
                journal.issue = _item.Issue;

                return journal;
            }
            catch (Exception ex)
            {
                var detailOfError = ex;
                throw;
            }

        
        }
    }
}
