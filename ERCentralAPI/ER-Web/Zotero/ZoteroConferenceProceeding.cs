using BusinessLibrary.BusinessClasses;
using Csla;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;

namespace ERxWebClient2.Zotero
{
    public class ZoteroConferenceProceeding : IMapZoteroReference
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
                var smartDate = new SmartDate();
                var parseDateResult = SmartDate.TryParse(_conferenceProceeding.date, ref smartDate);
                if (!parseDateResult) throw new System.Exception("Date parsing exception");
                var smartDateAdded = new SmartDate();
                var parseDateAddedResult = SmartDate.TryParse(_conferenceProceeding.dateAdded, ref smartDateAdded);
                if (!parseDateAddedResult) throw new System.Exception("Date parsing exception");
                var smartDateModified = new SmartDate();
                var parseDateModifiedResult = SmartDate.TryParse(_conferenceProceeding.dateModified, ref smartDateModified);
                if (!parseDateModifiedResult) throw new System.Exception("Date parsing exception");


                newERWebItem.Title = _conferenceProceeding.title;
                newERWebItem.TypeId = 5;
                newERWebItem.TypeName = "Conference Proceedings";
                newERWebItem.ShortTitle = _conferenceProceeding.shortTitle;
                newERWebItem.ParentTitle = _conferenceProceeding.parentTitle;
                newERWebItem.DateCreated = smartDateAdded;
                newERWebItem.CreatedBy = _conferenceProceeding.createdBy;
                newERWebItem.DateEdited = smartDateModified;
                newERWebItem.EditedBy = _conferenceProceeding.editedBy;
                newERWebItem.Year = smartDate.Date.Year.ToString(); 
                newERWebItem.Month = smartDate.Date.Month.ToString();
                newERWebItem.StandardNumber = _conferenceProceeding.ISSN;
                newERWebItem.City = _conferenceProceeding.place;
                newERWebItem.Country = _conferenceProceeding.place;//TODO COuntry and city fix
                newERWebItem.Publisher = _conferenceProceeding.publisher;
                newERWebItem.Institution = _conferenceProceeding.institution;
                newERWebItem.Volume = _conferenceProceeding.volume;
                newERWebItem.Pages = _conferenceProceeding.pages;
                newERWebItem.Edition = _conferenceProceeding.edition;
                newERWebItem.Issue = _conferenceProceeding.issue;
                newERWebItem.IsLocal = false; //TODO what is this
                newERWebItem.Availability = ""; //TODO what is this,
                newERWebItem.URL = _conferenceProceeding.url;
                newERWebItem.MasterItemId = 0;//TODO what is this,
                newERWebItem.Abstract = _conferenceProceeding.abstractNote;
                newERWebItem.Comments = _conferenceProceeding.comments;
                newERWebItem.DOI = _conferenceProceeding.DOI;
                newERWebItem.Keywords = "";
                string consolidatedAuthors = "";
                foreach (var creator in _conferenceProceeding.creators)
                {
                    consolidatedAuthors += creator.firstName + " " + creator.lastName;
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
