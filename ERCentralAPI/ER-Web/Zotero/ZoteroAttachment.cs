using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ZoteroAttachment : IMapZoteroReference
    {
        private Collection _attachment;

        public ZoteroAttachment(Collection collection)
        {
            _attachment = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item item)
        {
            try
            {
                ItemDocument newERWebDocument = new ItemDocument();

                newERWebDocument.Title = _attachment.data.title;
                newERWebDocument.Extension = "";
                newERWebDocument.Text = "";

                var erWebItem = new ERWebItem();
				//erWebItem.ItemDocument = newERWebDocument;
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