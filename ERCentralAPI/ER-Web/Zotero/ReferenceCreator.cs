using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public abstract class ReferenceCreator    
    {
        public abstract IMapZoteroReference GetReference(Collection item);

        public abstract IMapERWebReference GetReference(IItem item);
         
        public abstract ItemIncomingData GetIncomingDataReference(Collection item,ERWebItem eRWebItem);

	}
}
