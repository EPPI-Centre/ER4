using BusinessLibrary.BusinessClasses;
using Csla;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroManuscript : IMapZoteroReference
    {
        private CollectionType _journalArticle;
        public ZoteroManuscript(CollectionType journal)
        {
            _journalArticle = journal;
        }

        public ERWebItem MapReferenceFromZoteroToErWeb(Item newERWebItem)
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

                newERWebItem.Title = _journalArticle.title;
                newERWebItem.TypeId = 14;
                newERWebItem.TypeName = "Journal; Article";
                newERWebItem.ShortTitle = _journalArticle.shortTitle;
                newERWebItem.ParentTitle = _journalArticle.parentTitle;
                newERWebItem.DateCreated = smartDateAdded;
                newERWebItem.CreatedBy = _journalArticle.createdBy;
                newERWebItem.DateEdited = smartDateModified;
                newERWebItem.EditedBy = _journalArticle.editedBy;
                newERWebItem.Year = smartDate.Date.Year.ToString();
                newERWebItem.Month = smartDate.Date.Month.ToString();
                newERWebItem.StandardNumber = _journalArticle.ISSN;
                newERWebItem.City = _journalArticle.archiveLocation;
                newERWebItem.Country = _journalArticle.place;//TODO COuntry and city fix
                newERWebItem.Publisher = _journalArticle.publicationTitle;
                newERWebItem.Institution = _journalArticle.institution;
                newERWebItem.Volume = _journalArticle.volume;
                newERWebItem.Pages = _journalArticle.pages;
                newERWebItem.Edition = _journalArticle.edition;
                newERWebItem.Issue = _journalArticle.issue;
                newERWebItem.IsLocal = false; //TODO what is this
                newERWebItem.Availability = ""; //TODO what is this;
                newERWebItem.URL = _journalArticle.url;
                newERWebItem.MasterItemId = 0;//TODO what is this;
                newERWebItem.Abstract = _journalArticle.abstractNote;
                newERWebItem.Comments = _journalArticle.comments;
                newERWebItem.DOI = _journalArticle.DOI;
                newERWebItem.Keywords = "";
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

//        {itemType: "manuscript", title: "", creators: [{creatorType: "author", firstName: "", lastName: ""}],…}
//abstractNote: ""
//accessDate: ""
//archive: ""
//archiveLocation: ""
//callNumber: ""
//collections:[]
//creators:[{ creatorType: "author", firstName: "", lastName: ""}]
//date: ""
//extra: ""
//itemType: "manuscript"
//language: ""
//libraryCatalog: ""
//manuscriptType: ""
//numPages: ""
//place: ""
//relations: { }
//rights: ""
//shortTitle: ""
//tags:[]
//title: ""
//url: ""
    }
}
