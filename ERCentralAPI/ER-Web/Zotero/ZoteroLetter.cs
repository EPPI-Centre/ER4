using BusinessLibrary.BusinessClasses;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;


namespace ERxWebClient2.Zotero
{
    public class ZoteroLetter : ZoteroReferenceCreator, IMapZoteroReference
    {
        private Collection _letterArticle;
        public ZoteroLetter(Collection letter)
        {
            _letterArticle = letter;
        }

        public ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {

                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _letterArticle);
                erWebItem.Item.TypeId = 12;
                erWebItem.Item.TypeName = "Generic";
				erWebItem.Item.IsIncluded = true;
				return erWebItem;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }

        }

//        {itemType: "letter", title: "", creators: [{creatorType: "author", firstName: "", lastName: ""}],…}
//abstractNote: ""
//accessDate: ""
//archive: ""
//archiveLocation: ""
//callNumber: ""
//collections:[]
//creators:[{ creatorType: "author", firstName: "", lastName: ""}]
//date: ""
//extra: ""
//itemType: "letter"
//language: ""
//letterType: ""
//libraryCatalog: ""
//relations: { }
//rights: ""
//shortTitle: ""
//tags:[]
//title: ""
//url: ""
    }
}
