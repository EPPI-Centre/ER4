using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroThesis : ZoteroReferenceCreator, IMapZoteroReference
    {
        private Collection _dissertation;
        public ZoteroThesis(Collection dissertation)
        {
            _dissertation= dissertation;
        }

        public ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _dissertation);
                erWebItem.Item.TypeId = 4;
                erWebItem.Item.TypeName = "Dissertation";
				erWebItem.Item.IsIncluded = true;
                erWebItem.Item.Institution = _dissertation.data.university;
                return erWebItem;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }

        }

//        {itemType: "thesis", title: "", creators: [{creatorType: "author", firstName: "", lastName: ""}],…}
//abstractNote: ""
//accessDate: ""
//archive: ""
//archiveLocation: ""
//callNumber: ""
//collections:[]
//creators:[{ creatorType: "author", firstName: "", lastName: ""}]
//date: ""
//extra: ""
//itemType: "thesis"
//language: ""
//libraryCatalog: ""
//numPages: ""
//place: ""
//relations: { }
//rights: ""
//shortTitle: ""
//tags:[]
//thesisType: ""
//title: ""
//university: ""
//url: ""
    }
}
