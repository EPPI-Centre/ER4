using BusinessLibrary.BusinessClasses;
using Csla;
using ERxWebClient2.Controllers;
using ERxWebClient2.Zotero;

namespace ER_Web.Zotero
{
    public abstract class ZoteroCreator
    {
        public ERWebItem CreateErWebItemFromCollection(Item newERWebItem, CollectionType collectionType) { 
            
            var smartDate = new SmartDate();
            var parseDateResult = SmartDate.TryParse(collectionType.date, ref smartDate);
            if (!parseDateResult) throw new System.Exception("Date parsing exception");
            var smartDateAdded = new SmartDate();
            var parseDateAddedResult = SmartDate.TryParse(collectionType.dateAdded, ref smartDateAdded);
            if (!parseDateAddedResult) throw new System.Exception("Date parsing exception");
            var smartDateModified = new SmartDate();
            var parseDateModifiedResult = SmartDate.TryParse(collectionType.dateModified, ref smartDateModified);
            if (!parseDateModifiedResult) throw new System.Exception("Date parsing exception");

            newERWebItem.Title = collectionType.title;       
            string consolidatedAuthors = "";
            foreach (var creator in collectionType.creators)
            {
                consolidatedAuthors += creator.firstName + " " + creator.lastName;
            }
            newERWebItem.Authors = consolidatedAuthors;
            newERWebItem.Abstract = collectionType.abstractNote;
            newERWebItem.DateCreated = smartDateAdded;
            newERWebItem.DateEdited = smartDateModified;
            newERWebItem.Edition = collectionType.edition;
            newERWebItem.Institution = collectionType.place;
            newERWebItem.Pages = collectionType.numPages;
            newERWebItem.Publisher = collectionType.publisher;
            newERWebItem.ShortTitle = collectionType.shortTitle;
            newERWebItem.Volume = collectionType.volume;
            newERWebItem.IsLocal = false;
            newERWebItem.Pages = collectionType.pages;
            newERWebItem.Issue = collectionType.issue;
            newERWebItem.City = collectionType.archiveLocation;
            newERWebItem.ParentTitle = collectionType.parentTitle;
            newERWebItem.ParentAuthors = "";
            newERWebItem.DOI = collectionType.DOI;

            var erWebItem = new ERWebItem();
            erWebItem.Item = newERWebItem;
            return erWebItem;
        }
    }
}
