﻿using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroBook : ZoteroReferenceCreator,  IMapZoteroReference
    {
        private Collection _bookItem;

        public ZoteroBook(Collection collection)
        {
            _bookItem = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _bookItem);
                erWebItem.Item.TypeId = 2;
                erWebItem.Item.TypeName = "Book, Whole";
                erWebItem.Item.StandardNumber = _bookItem.data.ISBN;
				erWebItem.Item.IsIncluded = true;
                erWebItem.Item.Institution = _bookItem.data.university;
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