using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using Csla;
using ERxWebClient2.Controllers;
using ERxWebClient2.Zotero;
using Microsoft.IdentityModel.Tokens;
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
            if (collectionType.creators != null)
            {
                foreach (var creator in collectionType.creators)
                {
                    if (creator.creatorType == "editor" || creator.creatorType == "seriesEditor" || creator.creatorType == "translator"
                    || creator.creatorType == "bookAuthor" || creator.creatorType == "counsel" || creator.creatorType == "reviewedAuthor"
                     || creator.creatorType == "scriptwriter" || creator.creatorType == "producer" || creator.creatorType == "attorneyAgent")
                    {
                        if (creator.lastName != null && creator.lastName != "")
                        {
                            newERWebItem.ParentAuthors += creator.lastName + (creator.firstName == null ? "" : " " + creator.firstName) + ";";
                        }
                        else if (creator.name != null && creator.name != "")
                        {
                            newERWebItem.ParentAuthors += creator.name + ";";
                        }
                        else
                        {
                            newERWebItem.ParentAuthors += "[Unknown];";
                        }
                    }
                    else
                    {
                        if (creator.lastName != null && creator.lastName != "")
                        {
                            consolidatedAuthors += creator.lastName + (creator.firstName == null ? "" : " " + creator.firstName) + ";";
                        }
                        else if (creator.name != null && creator.name != "")
                        {
                            consolidatedAuthors += creator.name + ";";
                        }
                        else
                        {
                            consolidatedAuthors += "[Unknown];";
                        }
                    }
                }
            }
            consolidatedAuthors = consolidatedAuthors.Trim();
            consolidatedAuthors = consolidatedAuthors.TrimEnd(';');
            newERWebItem.ParentAuthors = newERWebItem.ParentAuthors.Trim();
            newERWebItem.ParentAuthors = newERWebItem.ParentAuthors.TrimEnd(';');
            newERWebItem.Title = collectionType.title;
            newERWebItem.Authors = consolidatedAuthors;
            newERWebItem.CreatedBy = collection.meta.createdByUser.username;
            newERWebItem.Abstract = collectionType.abstractNote;
            newERWebItem.DateCreated = smartDateAdded;
            newERWebItem.DateEdited = SmartDateModified;
            newERWebItem.Edition = collectionType.edition;
            newERWebItem.Institution = collectionType.institution;
            if (collectionType.numPages != null && collectionType.numPages != "") newERWebItem.Pages = collectionType.numPages;
            newERWebItem.Publisher = collectionType.publisher;
            newERWebItem.ShortTitle = collectionType.shortTitle;
            newERWebItem.Volume = collectionType.volume;
            newERWebItem.IsLocal = false;
            if (collectionType.pages != null && collectionType.pages != "") newERWebItem.Pages = collectionType.pages;
            newERWebItem.Issue = collectionType.issue;
            newERWebItem.City = collectionType.place;
            newERWebItem.ParentTitle = collectionType.parentTitle;
            newERWebItem.DOI = collectionType.DOI;
            string[] tmpParsedDate = ImportRefs.getDate(collectionType.date);
            if (tmpParsedDate[0].IsNullOrEmpty()) newERWebItem.Year = "";
            else newERWebItem.Year = tmpParsedDate[0];
            if (tmpParsedDate[1].IsNullOrEmpty()) newERWebItem.Month = "";
            else newERWebItem.Month = tmpParsedDate[1];
            newERWebItem.URL = collectionType.url;
            SetItemIdAndComments(newERWebItem, collectionType);

            var erWebItem = new ERWebItem
            {
                Item = newERWebItem
            };
            return erWebItem;
        }
        public static readonly string[] separators = { "\r\n", "\n", "\r", Environment.NewLine };
        public static readonly string searchFor = "EPPI-Reviewer ID: ";
        private static void SetItemIdAndComments(Item newERWebItem, CollectionType collectionType)
        {//despite the name, we DO NOT need to set the itemId...
            if (collectionType.extra == null) collectionType.extra = "";
            if (newERWebItem.Comments == null) newERWebItem.Comments = "";
            var arrayOfIdAndComments = collectionType.extra.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in arrayOfIdAndComments)
            {
                if (!line.StartsWith(searchFor))
                {
                    newERWebItem.Comments += line + Environment.NewLine;
                }
            }
        }
    }
}
