using BusinessLibrary.BusinessClasses;
using Csla;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroReport : IMapZoteroReference
    {
        private CollectionType _journalArticle;
        public ZoteroReport(CollectionType journal)
        {
            _journalArticle = journal;
        }

        public ERWebItem MapReferenceFromZoteroToErWeb()
        {
            try
            {
                var smartDate = new SmartDate();
                var parseDateResult = SmartDate.TryParse(_journalArticle.date, ref smartDate);
                if (!parseDateResult) throw new System.Exception("Date parsing exception");
                var smartDateAdded = new SmartDate();
                var parseDateAddedResult = SmartDate.TryParse(_journalArticle.dateAdded, ref smartDateAdded);
                if (!parseDateAddedResult) throw new System.Exception("Date parsing exception");
                var smartDateModified = new SmartDate();
                var parseDateModifiedResult = SmartDate.TryParse(_journalArticle.dateModified, ref smartDateModified);
                if (!parseDateModifiedResult) throw new System.Exception("Date parsing exception");

                Item newERWebItem = new Item
                {
                    Title = _journalArticle.title,
                    TypeId = 14,
                    TypeName = "Journal, Article",
                    ShortTitle = _journalArticle.shortTitle,
                    ParentTitle = _journalArticle.parentTitle,
                    DateCreated = smartDateAdded,
                    CreatedBy = _journalArticle.createdBy,
                    DateEdited = smartDateModified,
                    EditedBy = _journalArticle.editedBy,
                    Year = smartDate.Date.Year.ToString(),  
                    Month = smartDate.Date.Month.ToString(), 
                    StandardNumber = _journalArticle.ISSN,
                    City = _journalArticle.archiveLocation,
                    Country = _journalArticle.place,//TODO COuntry and city fix
                    Publisher = _journalArticle.publicationTitle,
                    Institution = _journalArticle.institution,
                    Volume = _journalArticle.volume,
                    Pages = _journalArticle.pages,
                    Edition = _journalArticle.edition,
                    Issue = _journalArticle.issue,
                    IsLocal = false, //TODO what is this
                    Availability = "", //TODO what is this,
                    URL = _journalArticle.url,
                    MasterItemId = 0,//TODO what is this,
                    Abstract = _journalArticle.abstractNote,
                    Comments = _journalArticle.comments,
                    DOI = _journalArticle.DOI,
                    Keywords = "",          
                   
                };
                string consolidatedAuthors = "";
                foreach (var creator in _journalArticle.creators)
                {
                    consolidatedAuthors += creator.firstName + " " + creator.lastName ;
                }
                newERWebItem.Authors = consolidatedAuthors;
                var erWebItem = new ERWebItem();
                erWebItem.Item = newERWebItem;
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
