using BusinessLibrary.BusinessClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Zotero
{
    public interface IMapZoteroReference
    {
        ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem);
    }

    public class ERWebItem
    {
        public Item Item { get; set; }
        public ItemDocument ItemDocument { get; set; }
        
    }
}
