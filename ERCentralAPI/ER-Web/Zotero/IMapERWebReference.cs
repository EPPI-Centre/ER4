using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Zotero
{
    public interface IMapERWebReference
    {
        CollectionData MapReferenceFromErWebToZotero();
    }
}
