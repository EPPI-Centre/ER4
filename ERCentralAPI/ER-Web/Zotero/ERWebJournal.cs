using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ERWebJournal : ERWebCreators, IMapERWebReference
    {
        public ERWebJournal(Item item)
        {
            _item = item;
        }

        CollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {           
            try
            {

                var journal = new JournalArticle(_item, _item.Publisher, _item.Issue, _item.Pages, "", "", "", _item.DOI, _item.DOI);
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
