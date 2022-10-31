using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ERWebConferenceProceeding :ERWebCreators,  IMapERWebReference
    {
        public ERWebConferenceProceeding(Item item)
        {
            _item = item;
        }

        ZoteroCollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {
            try
            {

                ZoteroCollectionData conferencePaper = new ConferencePaper(_item, _item.Title, "", "");
              

                return conferencePaper;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }
    }
}
