﻿using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroBookChapter : ZoteroReferenceCreator, IMapZoteroReference
    {
        private Collection _bookItem;

        public ZoteroBookChapter(Collection collection)
        {
            _bookItem = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {                
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _bookItem);
                erWebItem.Item.TypeId = 3;
                erWebItem.Item.TypeName = "Book, Chapter";
                erWebItem.Item.StandardNumber = _bookItem.data.ISBN;
				erWebItem.Item.IsIncluded = true;
                erWebItem.Item.ParentTitle = _bookItem.data.bookTitle;
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