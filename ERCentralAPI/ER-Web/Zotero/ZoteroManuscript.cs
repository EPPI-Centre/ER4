using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ZoteroManuscript : ZoteroCreator, IMapZoteroReference
    {
        private CollectionType _manuscriptArticle;
        public ZoteroManuscript(CollectionType manuscript)
        {
            _manuscriptArticle = manuscript;
        }

        public ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _manuscriptArticle);
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
