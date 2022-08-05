using BusinessLibrary.BusinessClasses;
using Csla;
using ERxWebClient2.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Zotero
{
    public class ZoteroWebSite : IMapZoteroReference
    {
        private CollectionType _webSite;

        public ZoteroWebSite(CollectionType collection)
        {
            _webSite = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb()
        {

            try
            {

                var smartDate = new SmartDate();
                var parseDateResult = SmartDate.TryParse(_webSite.date, ref smartDate);
                if (!parseDateResult) throw new System.Exception("Date parsing exception");
                var smartDateAdded = new SmartDate();
                var parseDateAddedResult = SmartDate.TryParse(_webSite.dateAdded, ref smartDateAdded);
                if (!parseDateAddedResult) throw new System.Exception("Date parsing exception");
                var smartDateModified = new SmartDate();
                var parseDateModifiedResult = SmartDate.TryParse(_webSite.accessDate, ref smartDateModified);
                if (!parseDateModifiedResult) throw new System.Exception("Date parsing exception");

                Item newERWebItem = new Item
                {
                    Title = _webSite.title,
                    TypeId = 7,
                    TypeName = "Web Site",
                    Abstract = _webSite.abstractNote,
                    DateCreated = smartDateAdded,
                    DateEdited = smartDateModified,
                    Edition = _webSite.edition,
                    Institution = _webSite.place,
                    Pages = _webSite.numPages,
                    Publisher = _webSite.publisher,
                    ShortTitle = _webSite.shortTitle,
                    URL = _webSite.url,
                    Country = _webSite.language,
                    Year = smartDate.Date.Year.ToString(),
                    Month = smartDate.Date.Month.ToString(),

                };
                string consolidatedAuthors = "";
                foreach (var creator in _webSite.creators)
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
    }
}