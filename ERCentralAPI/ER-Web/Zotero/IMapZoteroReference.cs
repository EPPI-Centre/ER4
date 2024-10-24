﻿using AuthorsHandling;
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
        public IItem? Item { get; set; }

        public IItemIncomingData? ItemIncomingData  { get; set;} 
       
    }
}
