using AuthorsHandling;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public interface IMapZoteroReference
    {
        ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem);
    }

    public class ERWebItem
    {
        public IItem Item { get; set; }

        //public ItemDocument ItemDocument { get; set;  // NOT NEEDED

        public IItemIncomingData ItemIncomingData  { get; set;} // ALSO AN ITEM
       
    }
}
