using BusinessLibrary.BusinessClasses;
using Csla;
using ER_Web.Zotero;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroReport : ZoteroCreator, IMapZoteroReference
    {
        private Collection _report;
        public ZoteroReport(Collection report)
        {
            _report = report;
        }

        public ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem)
        {
            try
            {
                var erWebItem = CreateErWebItemFromCollection(newERWebItem, _report);
                erWebItem.Item.TypeId = 1;
                erWebItem.Item.TypeName = "Report";
				erWebItem.Item.IsIncluded = true;
				return erWebItem;
            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }

        }

//        {itemType: "report", title: "", creators: [{creatorType: "author", firstName: "", lastName: ""}],…}
//abstractNote: ""
//accessDate: ""
//archive: ""
//archiveLocation: ""
//callNumber: ""
//collections:[]
//creators:[{ creatorType: "author", firstName: "", lastName: ""}]
//date: ""
//extra: ""
//institution: ""
//itemType: "report"
//language: ""
//libraryCatalog: ""
//pages: ""
//place: ""
//relations: { }
//reportNumber: ""
//reportType: ""
//rights: ""
//seriesTitle: ""
//shortTitle: ""
//tags:[]
//title: ""
//url: ""
    }
}
