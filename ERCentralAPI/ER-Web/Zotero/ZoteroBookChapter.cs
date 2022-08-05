using BusinessLibrary.BusinessClasses;
using Csla;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroBookChapter : IMapZoteroReference
    {
        private CollectionType _bookItem;

        public ZoteroBookChapter(CollectionType collection)
        {
            _bookItem = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb()
        {
            try
            {

                var smartDate = new SmartDate();
                var parseDateResult = SmartDate.TryParse(_bookItem.date, ref smartDate);
                if (!parseDateResult) throw new System.Exception("Date parsing exception");
                var smartDateAdded = new SmartDate();
                var parseDateAddedResult = SmartDate.TryParse(_bookItem.dateAdded, ref smartDateAdded);
                if (!parseDateAddedResult) throw new System.Exception("Date parsing exception");
                var smartDateModified = new SmartDate();
                var parseDateModifiedResult = SmartDate.TryParse(_bookItem.dateModified, ref smartDateModified);
                if (!parseDateModifiedResult) throw new System.Exception("Date parsing exception");

                Item newERWebItem = new Item
                {
                    Title = _bookItem.bookTitle,
                    TypeId = 3,
                    TypeName = "Book, Chapter"
                };
                newERWebItem.ParentTitle = _bookItem.title;
                newERWebItem.ShortTitle = _bookItem.shortTitle;
                newERWebItem.URL = _bookItem.url;
                string consolidatedAuthors = "";           
                foreach (var creator in _bookItem.creators)
                {
                    consolidatedAuthors += creator.firstName + " " + creator.lastName;
                }
                newERWebItem.Authors = consolidatedAuthors;
                newERWebItem.Abstract = _bookItem.abstractNote;
                newERWebItem.DateCreated = smartDateAdded;
                newERWebItem.DateEdited = smartDateModified;
                newERWebItem.Edition = _bookItem.edition;
                newERWebItem.Institution = _bookItem.place;
                newERWebItem.Pages = _bookItem.numPages;
                newERWebItem.Publisher = _bookItem.publisher;
                newERWebItem.ShortTitle = _bookItem.shortTitle;
                newERWebItem.Volume = _bookItem.volume;
                newERWebItem.IsLocal = false;
                newERWebItem.Pages = _bookItem.pages;
                newERWebItem.Issue = _bookItem.issue;
                newERWebItem.City = _bookItem.archiveLocation;
                newERWebItem.DOI = _bookItem.ISBN;

                var erWebItem = new ERWebItem();
                erWebItem.Item = newERWebItem;
                return erWebItem;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }
    }
}