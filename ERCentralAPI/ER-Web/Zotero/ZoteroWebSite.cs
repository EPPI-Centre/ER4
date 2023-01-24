using BusinessLibrary.BusinessClasses;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ZoteroWebSite : ZoteroReferenceCreator, IMapZoteroReference
    {
        private Collection _webSite;

        public ZoteroWebSite(Collection collection)
        {
            _webSite = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {

            try
            {

                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _webSite);
                erWebItem.Item.TypeId = 7;
                erWebItem.Item.TypeName = "Web Site";
				erWebItem.Item.IsIncluded = true;
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