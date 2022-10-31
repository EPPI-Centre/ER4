using AuthorsHandling;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using Csla;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{

	// Special case for itemIncomingData
	public class ZoteroIncomingDataReference  : IMapZoteroIncomingDataReference
    {

        private (AutorsList authorsLi, Csla.Core.MobileList<AutH> pAuthorsLi) AuthorsListForIncomingData(CreatorsItem[] creators)
        {
   
            var authorsLi = new AutorsList();
            var pAuthorsLi = new Csla.Core.MobileList<AutH>();
            int AuthRank = 0;
            foreach (var Zau in creators)
            {
                if (Zau.creatorType == "author")
                {
                    AutH a = new AutH();
                    a.FirstName = Zau.firstName;
                    a.MiddleName = "";
                    a.LastName = Zau.lastName;
                    a.Role = 0;//only looking for "actual authors" not parent authors which can be Book editors and the like.
                    a.Rank = AuthRank;
                    AuthRank++;
                    authorsLi.Add(a);
                    //itemIncomingData.pAuthorsLi.Add(a);
                }
            }

            return (authorsLi, pAuthorsLi);

        }

        public ItemIncomingData MapReferenceFromZoteroToErWeb(CollectionType collection, ItemIncomingData newERWebItem)
        {
            try
            {
                var smartDate = new SmartDate();
                var parseDateResult = SmartDate.TryParse(collection.date, ref smartDate);
                if (!parseDateResult) throw new System.Exception("Date parsing exception");
                var smartDateAdded = new SmartDate();
                var parseDateAddedResult = SmartDate.TryParse(collection.dateAdded, ref smartDateAdded);
                if (!parseDateAddedResult) throw new System.Exception("Date parsing exception");
                var smartDateModified = new SmartDate();
                var parseDateModifiedResult = SmartDate.TryParse(collection.dateModified, ref smartDateModified);
                if (!parseDateModifiedResult) throw new System.Exception("Date parsing exception");

                newERWebItem.Title = collection.title ?? "";
                newERWebItem.TypeId = 2;
                var authors = AuthorsListForIncomingData(collection.creators ?? new CreatorsItem[0]);
                newERWebItem.AuthorsLi = authors.authorsLi ?? new AutorsList();
                newERWebItem.pAuthorsLi = authors.pAuthorsLi ?? new Csla.Core.MobileList<AutH>();
                newERWebItem.Abstract = collection.abstractNote ?? "";
                newERWebItem.DateEdited = smartDateModified;
                newERWebItem.Edition = collection.edition ?? "";
                newERWebItem.Institution = collection.place ?? "";
                newERWebItem.Pages = collection.numPages ?? "";
                newERWebItem.Publisher = collection.publisher ?? "";
                newERWebItem.Short_title = collection.shortTitle ?? "";
                newERWebItem.Volume = collection.volume ?? "";
                newERWebItem.Pages = collection.pages ?? "";
                newERWebItem.Issue = collection.issue ?? "";
                newERWebItem.City = collection.archiveLocation ?? "";
                newERWebItem.DOI = collection.ISBN ?? "";
                newERWebItem.ZoteroKey = collection.key;
                return newERWebItem;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }
    }

    public interface IMapZoteroIncomingDataReference
    {
		ItemIncomingData MapReferenceFromZoteroToErWeb(CollectionType collection, ItemIncomingData newERWebItem);

	}
}