using BusinessLibrary.BusinessClasses;
using Csla;
using ERxWebClient2.Controllers;
using ERxWebClient2.Zotero;
using System.Globalization;

namespace ER_Web.Zotero
{
    public abstract class ZoteroCreator
    {
        private const string V = "yyyy-MM-ddTHH:mm:ssZ";

        public ERWebItem CreateErWebItemFromCollection(Item newERWebItem, Collection collection) {

            var collectionType = collection.data;
			var smartDate = new SmartDate();
            DateTime dateYear;
			DateTime.TryParseExact(collectionType.date, "yyyy", CultureInfo.InvariantCulture,  DateTimeStyles.None, out dateYear);
            var smartDateYear = dateYear.ToString(V);

			var SmartDateModified = new SmartDate();
			DateTime dateModified;
			var smartDateModifiedBool = DateTime.TryParseExact(collectionType.dateModified, V, CultureInfo.InvariantCulture,DateTimeStyles.None, out dateModified);
            var smartDateMod = dateModified.ToUniversalTime().ToString(V);

			var parseDateResult = SmartDate.TryParse(smartDateYear, ref smartDate);
            if (!parseDateResult) throw new System.Exception("Date parsing exception");
            var smartDateAdded = new SmartDate();
            var parseDateAddedResult = SmartDate.TryParse(collectionType.dateAdded, ref smartDateAdded);
            if (!parseDateAddedResult) throw new System.Exception("Date parsing exception");
            
            var parseDateModifiedResult = SmartDate.TryParse(smartDateMod, ref SmartDateModified);
            if (!parseDateModifiedResult) throw new System.Exception("Date parsing exception");

			string consolidatedAuthors = "";
			foreach (var creator in collectionType.creators)
			{
				consolidatedAuthors += creator.lastName + " " + creator.firstName + ";";
			}
			consolidatedAuthors = consolidatedAuthors.TrimStart();
			consolidatedAuthors = consolidatedAuthors.TrimEnd();
			newERWebItem.Title = collectionType.title; 
            newERWebItem.Authors = consolidatedAuthors;
            newERWebItem.CreatedBy = collection.meta.createdByUser.username;
            newERWebItem.Abstract = collectionType.abstractNote;
            newERWebItem.DateCreated = smartDateAdded;
            newERWebItem.DateEdited = SmartDateModified; 
            newERWebItem.Edition = collectionType.edition;
            newERWebItem.Institution = collectionType.institution;
            newERWebItem.Pages = collectionType.numPages;
            newERWebItem.Publisher = collectionType.publisher;
            newERWebItem.ShortTitle = collectionType.shortTitle;
            newERWebItem.Volume = collectionType.volume;
            newERWebItem.IsLocal = false;
            newERWebItem.Pages = collectionType.pages;
            newERWebItem.Issue = collectionType.issue;
            newERWebItem.City = collectionType.place;
            newERWebItem.ParentTitle = collectionType.parentTitle;
            newERWebItem.DOI = collectionType.DOI;
            newERWebItem.Year = smartDateYear;
			newERWebItem.ParentAuthors = "";

            var erWebItem = new ERWebItem
            {
                Item = newERWebItem
            };
            return erWebItem;
        }
    }
}
