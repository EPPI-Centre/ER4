using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroNewspaperArticle : ZoteroCreator, IMapZoteroReference
    {
        private CollectionType _newspaper;

        public ZoteroNewspaperArticle(CollectionType collection)
        {
            _newspaper = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {

                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _newspaper);
                erWebItem.Item.TypeId = 12;
                erWebItem.Item.TypeName = "Generic";
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