using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public abstract class ReferenceCreator
    {
        public abstract IMapZoteroReference GetReference(Collection item);

        public abstract IMapERWebReference GetReference(ERWebItem item);

    }
}
