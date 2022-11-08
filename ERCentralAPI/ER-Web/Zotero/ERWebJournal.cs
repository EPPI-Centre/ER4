using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;

namespace ERxWebClient2.Zotero
{
    public class ERWebJournal : ERWebCreators, IMapERWebReference
    {
        public ERWebJournal(IItem item)
        {
            _item = item;
        }

        ZoteroCollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {           
            try
            {

                var journal = new JournalArticle(_item, _item.ParentTitle, _item.Issue, _item.Pages, "", "", "", _item.DOI, _item.StandardNumber);
                journal.volume = _item.Volume;
                return journal;
            }
            catch (Exception ex)
            {
                var detailOfError = ex;
                throw;
            }


        }
    }
}


//"itemType": "journalArticle",
//    "title": "",
//    "creators": [
//        {
//            "creatorType": "author",
//            "firstName": "",
//            "lastName": ""
//        }
//    ],
//    "abstractNote": "",
//    "publicationTitle": "",
//    "volume": "",
//    "issue": "",
//    "pages": "",
//    "date": "",
//    "series": "",
//    "seriesTitle": "",
//    "seriesText": "",
//    "journalAbbreviation": "",
//    "language": "",
//    "DOI": "",
//    "ISSN": "",
//    "shortTitle": "",
//    "url": "",
//    "accessDate": "",
//    "archive": "",
//    "archiveLocation": "",
//    "libraryCatalog": "",
//    "callNumber": "",
//    "rights": "",
//    "extra": "",
//    "tags": [],
//    "collections": [],
//    "relations": { }