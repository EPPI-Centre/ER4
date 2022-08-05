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

        public ERWebItem MapReferenceFromZoteroToErWeb()
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

                Item newERWebItem = new Item
                {
                    Title = _conferenceProceeding.title,
                    TypeId = 5,
                    TypeName = "Conference Proceedings",
                    ShortTitle = _conferenceProceeding.shortTitle,
                    ParentTitle = _conferenceProceeding.parentTitle,
                    DateCreated = smartDateAdded,
                    CreatedBy = _conferenceProceeding.createdBy,
                    DateEdited = smartDateModified,
                    EditedBy = _conferenceProceeding.editedBy,
                    Year = smartDate.Date.Year.ToString(), 
                    Month = smartDate.Date.Month.ToString(), 
                    StandardNumber = _conferenceProceeding.ISSN,
                    City = _conferenceProceeding.place,
                    Country = _conferenceProceeding.place,//TODO COuntry and city fix
                    Publisher = _conferenceProceeding.publisher,
                    Institution=  _conferenceProceeding.institution,
                    Volume = _conferenceProceeding.volume,
                    Pages = _conferenceProceeding.pages,
                    Edition = _conferenceProceeding.edition,
                    Issue = _conferenceProceeding.issue,
                    IsLocal = false, //TODO what is this
                    Availability = "", //TODO what is this,
                    URL = _conferenceProceeding.url,
                    MasterItemId = 0,//TODO what is this,
                    Abstract = _conferenceProceeding.abstractNote ,
                    Comments = _conferenceProceeding.comments,
                    DOI = _conferenceProceeding.DOI,
                    Keywords = "" //TODO what is this,         
                };
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
