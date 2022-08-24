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

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb(Item newERWebItem)
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

                newERWebItem.Title = _webSite.title;
                newERWebItem.TypeId = 7;
                newERWebItem.TypeName = "Web Site";
                newERWebItem.Abstract = _webSite.abstractNote;
                newERWebItem.DateCreated = smartDateAdded;
                newERWebItem.DateEdited = smartDateModified;
                newERWebItem.Edition = _webSite.edition;
                newERWebItem.Institution = _webSite.place;
                newERWebItem.Pages = _webSite.numPages;
                newERWebItem.Publisher = _webSite.publisher;
                newERWebItem.ShortTitle = _webSite.shortTitle;
                newERWebItem.URL = _webSite.url;
                newERWebItem.Country = _webSite.language;
                newERWebItem.Year = smartDate.Date.Year.ToString();
                newERWebItem.Month = smartDate.Date.Month.ToString();

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