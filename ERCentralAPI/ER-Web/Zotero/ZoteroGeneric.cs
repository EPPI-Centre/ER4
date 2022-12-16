using BusinessLibrary.BusinessClasses;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ZoteroGeneric : ZoteroReferenceCreator,  IMapZoteroReference
    {
        private Collection _genericItem;

        public ZoteroGeneric(Collection collection)
        {
            _genericItem = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _genericItem);
                erWebItem.Item.TypeId = 12;
                erWebItem.Item.TypeName = "Generic";
				erWebItem.Item.IsIncluded = true;
                if (!string.IsNullOrWhiteSpace(_genericItem.data.publisher)) erWebItem.Item.ParentTitle = _genericItem.data.publisher.Trim();
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