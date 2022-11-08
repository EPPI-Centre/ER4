using BusinessLibrary.BusinessClasses;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;


namespace ERxWebClient2.Zotero
{
    public class ZoteroJournal : ZoteroCreator, IMapZoteroReference
    {
        private Collection _journalArticle;
        public ZoteroJournal(Collection journal)
        {
            _journalArticle = journal;
        }

        public ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {
                
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _journalArticle);
                erWebItem.Item.TypeId = 14;
                erWebItem.Item.TypeName = "Journal, Article";
                erWebItem.Item.StandardNumber = _journalArticle.data.ISSN;
				erWebItem.Item.IsIncluded = true;
                erWebItem.Item.ParentTitle = _journalArticle.data.publicationTitle;
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
