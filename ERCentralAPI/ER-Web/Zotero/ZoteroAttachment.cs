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
    public class ZoteroAttachment : IMapZoteroReference
    {
        private CollectionType _attachment;

        public ZoteroAttachment(CollectionType collection)
        {
            _attachment = collection;
        }

        ERWebItem IMapZoteroReference.MapReferenceFromZoteroToErWeb()
        {
            try
            {

                ItemDocument newERWebDocument = new ItemDocument();

                newERWebDocument.Title = _attachment.title;
                newERWebDocument.Extension = "";
                newERWebDocument.Text = "";

                var erWebItem = new ERWebItem();
                erWebItem.ItemDocument = newERWebDocument;
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