using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroConferenceProceeding : ZoteroCreator, IMapZoteroReference
    {
        private CollectionType _conferenceProceeding;
        public ZoteroConferenceProceeding(CollectionType conferenceProceeding)
        {
            _conferenceProceeding = conferenceProceeding;
        }

        public ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _conferenceProceeding);
                erWebItem.Item.TypeId = 5;
                erWebItem.Item.TypeName = "Conference Proceedings";
                erWebItem.Item.StandardNumber = _conferenceProceeding.ISBN;
                return erWebItem;
               
                

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
        }

//        {itemType: "conferencePaper", title: "",…}
//    DOI: ""
//ISBN: ""
//abstractNote: ""
//accessDate: ""
//archive: ""
//archiveLocation: ""
//callNumber: ""
//collections: []
//    conferenceName: ""
//creators: [{creatorType: "author", firstName: "", lastName: ""}]
//date: ""
//extra: ""
//itemType: "conferencePaper"
//language: ""
//libraryCatalog: ""
//pages: ""
//place: ""
//proceedingsTitle: ""
//publisher: ""
//relations: { }
//rights: ""
//series: ""
//shortTitle: ""
//tags:[]
//title: ""
//url: ""
//volume: ""
    }
}
