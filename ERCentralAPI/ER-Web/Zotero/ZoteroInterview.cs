using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroInterview : ZoteroCreator, IMapZoteroReference
    {
        private CollectionType _interview;
        public ZoteroInterview(CollectionType interview)
        {
            _interview = interview;
        }

        public ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _interview);
                erWebItem.Item.TypeId = 11;
                erWebItem.Item.TypeName = "Interview";
                return erWebItem;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }

        }

//        {itemType: "interview", title: "",…}
//    abstractNote: ""
//accessDate: ""
//archive: ""
//archiveLocation: ""
//callNumber: ""
//collections: []
//    creators: [{creatorType: "interviewee", firstName: "", lastName: ""}]
//date: ""
//extra: ""
//interviewMedium: ""
//itemType: "interview"
//language: ""
//libraryCatalog: ""
//relations: { }
//rights: ""
//shortTitle: ""
//tags:[]
//title: ""
//url: ""
    }
}
