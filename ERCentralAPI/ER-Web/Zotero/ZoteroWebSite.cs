using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ZoteroWebSite : ZoteroCreator, IMapZoteroReference
    {
        private CollectionType _webSite;

        public ZoteroWebSite(CollectionType collection)
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