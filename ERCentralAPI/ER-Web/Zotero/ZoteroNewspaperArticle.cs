using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroNewspaperArticle : ZoteroReferenceCreator, IMapZoteroReference
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
                erWebItem.Item.TypeId = 10;
                erWebItem.Item.TypeName = "Article In A Periodical";
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