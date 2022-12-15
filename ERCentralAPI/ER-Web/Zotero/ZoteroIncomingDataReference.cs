using AuthorsHandling;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using Csla;
using ERxWebClient2.Controllers;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

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
            int pAuthRank = 0;
            foreach (var Zau in creators)
            {
                int role = 0;
                if (Zau.creatorType == "editor" || Zau.creatorType == "seriesEditor" || Zau.creatorType == "translator" 
                    || Zau.creatorType == "bookAuthor" || Zau.creatorType == "counsel" || Zau.creatorType == "reviewedAuthor"
                     || Zau.creatorType == "scriptwriter" || Zau.creatorType == "producer" || Zau.creatorType == "attorneyAgent") 
                {
                    role = 1;
                    AutH a = new AutH();
                    if (Zau.name != null && Zau.name != "")
                    {
                        a = NormaliseAuth.singleAuth(Zau.name, pAuthRank, role);
                    }
                    else
                    {
                        a.FirstName = Zau.firstName ?? "";
                        a.MiddleName = "";
                        a.LastName = Zau.lastName ?? "";
                        a.Role = role;
                        a.Rank = pAuthRank;
                    }
                    pAuthRank++;
                    pAuthorsLi.Add(a);
                }
                else if (Zau.creatorType != "recipient")
                {
                    AutH a = new AutH();
                    if (Zau.name != null && Zau.name != "")
                    {
                        a = NormaliseAuth.singleAuth(Zau.name, AuthRank, role);
                    }
                    else
                    {
                        a.FirstName = Zau.firstName ?? "";
                        a.MiddleName = "";
                        a.LastName = Zau.lastName ?? "";
                        a.Role = role;
                        a.Rank = AuthRank;
                    }
                    AuthRank++;
                    authorsLi.Add(a);
                }
            }

            return (authorsLi, pAuthorsLi);

        }

        public ItemIncomingData MapReferenceFromZoteroToErWeb(CollectionType collection, ItemIncomingData newERWebItem, ERWebItem eRWebItem)
        {
            //try
            //{
            var item = eRWebItem.Item;
            if (item == null) throw new Exception("ErWeb Item cannot be null");
            newERWebItem.Title = item.Title;
            newERWebItem.TypeId = item.TypeId;
            var authors = AuthorsListForIncomingData(collection.creators ?? new CreatorsItem[0]);
            newERWebItem.AuthorsLi = authors.authorsLi ?? new AutorsList();
            newERWebItem.pAuthorsLi = authors.pAuthorsLi ?? new Csla.Core.MobileList<AutH>();
            newERWebItem.Abstract = item.Abstract ?? "";
            //we NEED to update DateEdited, otherwise, if we keep the same timestamp we received from Zotero,
            //upon updating the Zot record, the value we'll include for dateModified will be identical to the value currently in Zotero
            //despite what the docs say, it appears that when we do this, Zotero will update the dateModified to the current time, which breaks our "change detection"
            //Setting the timestamp here (and then in the Update call) ensures the new timestamp is different, which forces Zotero to respect it.
            newERWebItem.DateEdited = DateTime.Now;
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
            newERWebItem.Comments = item.Comments ?? "";
            newERWebItem.Country = item.Country ?? "";
            newERWebItem.Month = item.Month ?? "";
            newERWebItem.Keywords = item.Keywords ?? "";
            newERWebItem.Parent_title = item.ParentTitle ?? "";
            newERWebItem.Url = item.URL ?? "";
            newERWebItem.OldItemId = collection.key;
            newERWebItem.ZoteroKey = collection.key;
            string[] tmpParsedDate = ImportRefs.getDate(collection.date);
            if (tmpParsedDate[0].IsNullOrEmpty()) newERWebItem.Year = "";
            else newERWebItem.Year = tmpParsedDate[0];
            if (tmpParsedDate[1].IsNullOrEmpty()) newERWebItem.Month = "";
            else newERWebItem.Month = tmpParsedDate[1];

            SetFieldsBasedOnZoteroType(collection, newERWebItem);

            return newERWebItem;

            //}
            //catch (System.Exception ex)
            //{
            //    var detailOfError = ex;
            //    throw ex;
            //}
        }

        private static void SetFieldsBasedOnZoteroType(CollectionType collection, ItemIncomingData newERWebItem)
        {
            if (collection.itemType == "journalArticle")
            {
                newERWebItem.Standard_number = collection.ISSN ?? "";
            }
            else if (collection.itemType == "book")
            {
                newERWebItem.Standard_number = collection.ISBN ?? "";
            }
            else if (collection.itemType == "bookChapter")
            {
                newERWebItem.Standard_number = collection.ISBN ?? "";
            }
            else if (collection.itemType == "dissertation")
            {
                var standardNumber = string.IsNullOrWhiteSpace(collection.ISBN) ? collection.ISSN : collection.ISBN;
                newERWebItem.Standard_number = standardNumber ?? "";
            }
            else if (collection.itemType == "conferencePaper")
            {
                var standardNumber = string.IsNullOrWhiteSpace(collection.ISBN) ? collection.ISSN : collection.ISBN;
                newERWebItem.Standard_number = standardNumber ?? "";
            }
            else if (collection.itemType == "periodical")
            {
                var standardNumber = string.IsNullOrWhiteSpace(collection.ISBN) ? collection.ISSN : collection.ISBN;
                newERWebItem.Standard_number = standardNumber ?? "";
            }
        }
    }

    public interface IMapZoteroIncomingDataReference
    {
		ItemIncomingData MapReferenceFromZoteroToErWeb(CollectionType collection, ItemIncomingData newERWebItem, ERWebItem eRWebItem);

	}
}