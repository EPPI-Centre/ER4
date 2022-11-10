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

        public ERWebItem CreateErWebItemFromCollection(Item newERWebItem, Collection collection)
        {

            var collectionType = collection.data;
            var smartDate = new SmartDate();

            var SmartDateModified = new SmartDate();
            DateTime dateModified;
            var smartDateModifiedBool = DateTime.TryParseExact(collectionType.dateModified, V, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateModified);
            var smartDateMod = dateModified.ToUniversalTime().ToString(V);

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
            newERWebItem.Year = collectionType.date;
            newERWebItem.URL = collectionType.url;
            newERWebItem.ParentAuthors = "";
            SetItemIdAndComments(newERWebItem, collectionType);

            var erWebItem = new ERWebItem
            {
                Item = newERWebItem
            };
            return erWebItem;
        }

        private static void SetItemIdAndComments(Item newERWebItem, CollectionType collectionType)
        {
            var arrayOfIdAndComments = collectionType.extra.Split('\n', StringSplitOptions.None);
            if (arrayOfIdAndComments.Length == 2)
            {
                var itemIdStr = arrayOfIdAndComments[0];
                var index = itemIdStr.LastIndexOf(':');
                var commentsStr = arrayOfIdAndComments[1];
                var indexTwo = commentsStr.IndexOf(':');
                var itemId = arrayOfIdAndComments[0].Substring(index + 1, itemIdStr.Length - index - 1);
                var comments = arrayOfIdAndComments[0].Substring(indexTwo + 1, commentsStr.Length - indexTwo - 1);
                newERWebItem.ItemId = Convert.ToInt64(itemId);
                newERWebItem.Comments = comments;
            }
        }
    }
}
