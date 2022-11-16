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

        public ItemIncomingData MapReferenceFromZoteroToErWeb(CollectionType collection, ItemIncomingData newERWebItem, ERWebItem eRWebItem)
        {
            try
            {
                var item = eRWebItem.Item;
                if (item == null) throw new Exception("ErWeb Item cannot be null");
                newERWebItem.Title = item.Title;
                newERWebItem.TypeId = item.TypeId;
                var authors = AuthorsListForIncomingData(collection.creators ?? new CreatorsItem[0]);
                newERWebItem.AuthorsLi = authors.authorsLi ?? new AutorsList();
                newERWebItem.pAuthorsLi = authors.pAuthorsLi ?? new Csla.Core.MobileList<AutH>();
                newERWebItem.Abstract = item.Abstract ?? "";
                newERWebItem.DateEdited = item.DateEdited;
                newERWebItem.Edition = item.Edition ?? "";
                newERWebItem.Institution = item.Institution ?? "";
                newERWebItem.Pages = item.Pages ?? "";
                newERWebItem.Publisher = item.Publisher ?? "";
                newERWebItem.Short_title = item.ShortTitle ?? "";
                newERWebItem.Volume = item.Volume ?? "";
                newERWebItem.Pages = item.Pages ?? "";
                newERWebItem.Issue = item.Issue ?? "";
                newERWebItem.City = item.City ?? "";
                newERWebItem.DOI = item.DOI ?? "";
                newERWebItem.Comments = item.Comments;
                newERWebItem.Country = item.Country;
                newERWebItem.Month = item.Month;
                newERWebItem.Keywords = item.Keywords;
                newERWebItem.Parent_title = item.ParentTitle;
                newERWebItem.Url = item.URL;
                newERWebItem.ZoteroKey = collection.key;

                SetFieldsBasedOnZoteroType(collection, newERWebItem);

                return newERWebItem;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }

        private static void SetFieldsBasedOnZoteroType(CollectionType collection, ItemIncomingData newERWebItem)
        {
            if (collection.itemType == "journalArticle")
            {
                newERWebItem.Standard_number = collection.ISSN;
            }
            else if (collection.itemType == "book")
            {
                newERWebItem.Standard_number = collection.ISBN;
            }
            else if (collection.itemType == "bookChapter")
            {
                newERWebItem.Standard_number = collection.ISBN;
            }
            else if (collection.itemType == "Dissertation")
            {
                var standardNumber = string.IsNullOrWhiteSpace(collection.ISBN) ? collection.ISSN : collection.ISBN;
                newERWebItem.Standard_number = standardNumber;
            }
            else if (collection.itemType == "conferencePaper")
            {
                var standardNumber = string.IsNullOrWhiteSpace(collection.ISBN) ? collection.ISSN : collection.ISBN;
                newERWebItem.Standard_number = standardNumber;
            }
            else if (collection.itemType == "periodical")
            {
                var standardNumber = string.IsNullOrWhiteSpace(collection.ISBN) ? collection.ISSN : collection.ISBN;
                newERWebItem.Standard_number = standardNumber;
            }
        }
    }

    public interface IMapZoteroIncomingDataReference
    {
		ItemIncomingData MapReferenceFromZoteroToErWeb(CollectionType collection, ItemIncomingData newERWebItem, ERWebItem eRWebItem);

	}
}