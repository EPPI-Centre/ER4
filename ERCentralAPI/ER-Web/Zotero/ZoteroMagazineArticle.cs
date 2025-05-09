﻿using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroMagazineArticle : ZoteroReferenceCreator, IMapZoteroReference
    {
        private Collection _magazineItem;

        public ZoteroMagazineArticle(Collection collection)
        {
            _magazineItem = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {

                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _magazineItem);
                erWebItem.Item.TypeId = 10;
                erWebItem.Item.TypeName = "Article In A Periodical";
				erWebItem.Item.IsIncluded = true;
                erWebItem.Item.StandardNumber = _magazineItem.data.ISSN;
				return erWebItem;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }

//        {itemType: "magazineArticle", title: "",…}
//    ISSN: ""
//abstractNote: ""
//accessDate: ""
//archive: ""
//archiveLocation: ""
//callNumber: ""
//collections: []
//    creators: [{creatorType: "author", firstName: "", lastName: ""}]
//date: ""
//extra: ""
//issue: ""
//itemType: "magazineArticle"
//language: ""
//libraryCatalog: ""
//pages: ""
//publicationTitle: ""
//relations: { }
//rights: ""
//shortTitle: ""
//tags:[]
//title: ""
//url: ""
//volume: ""
    }
}