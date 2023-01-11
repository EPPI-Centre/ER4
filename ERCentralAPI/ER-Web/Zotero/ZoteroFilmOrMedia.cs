using BusinessLibrary.BusinessClasses;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ZoteroFilmOrMedia : ZoteroReferenceCreator, IMapZoteroReference
    {
        private Collection _media;

        public ZoteroFilmOrMedia(Collection collection)
        {
            _media = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {

            try
            {
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _media);
                erWebItem.Item.TypeId = 8;
                erWebItem.Item.TypeName = "DVD, Video, Media";
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
