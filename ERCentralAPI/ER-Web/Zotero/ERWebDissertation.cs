using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ERWebDissertation :ERWebCreators, IMapERWebReference
    {
        public ERWebDissertation(IItem item)
        {
            _item = item;
        }

        // Need to use erweb mapping table to get fields on rhs into the correct lhs zotero fields 
        ZoteroCollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {
            try
            {
                var newZoteroItem = new Dissertation(_item, "", _item.Edition, _item.City, _item.Pages
                    , _item.DOI);
                newZoteroItem.volume = _item.Volume;

                return newZoteroItem;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }
    }
}
