using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroNewspaperArticle : ZoteroCreator, IMapZoteroReference
    {
        private Collection _newspaper;

        public ZoteroNewspaperArticle(Collection collection)
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