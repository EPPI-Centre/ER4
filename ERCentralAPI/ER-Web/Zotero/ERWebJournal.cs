using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ERWebJournal : ERWebCreators, IMapERWebReference
    {
        public ERWebJournal(IItem item)
        {
            _item = item;
        }

        ZoteroCollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {           
            try
            {

                var journal = new JournalArticle(_item, _item.ParentTitle, _item.Issue, _item.Pages, "", "", "", _item.DOI, _item.StandardNumber);
                return journal;
            }
            catch (Exception ex)
            {
                var detailOfError = ex;
                throw;
            }


        }
    }
}
