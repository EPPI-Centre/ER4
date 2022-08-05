using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Zotero
{
    public class ERWebDocument : IMapERWebReference
    {
        private ItemDocument _itemDocument;
        private CollectionData _zoteroParentItem;

        public ERWebDocument(ItemDocument itemDocument, CollectionData zoteroParentItem)
        {
            _itemDocument = itemDocument;
            _zoteroParentItem = zoteroParentItem;
        }

        //"itemType": "attachment",
        //"parentItem": "ABCD2345",
        //"linkMode": "imported_url",
        //"title": "My Document",
        //"accessDate": "2012-03-14T17:45:54Z",
        //"url": "http://example.com/doc.pdf",
        //"note": "",
        //"tags": [],
        //"relations": {},
        //"contentType": "application/pdf",
        //"charset": "",
        //"filename": "doc.pdf",
        //"md5": null,
        //"mtime": null
        CollectionData IMapERWebReference.MapReferenceFromErWebToZotero()
        {
            try
            {

                string filename = _itemDocument.Title;
                int ind = filename.LastIndexOf(".");
                string ext = filename.Substring(ind);
                string contentType = "";
                switch (ext)
                {
                    case "pdf":
                        contentType = "application/pdf";
                        break;
                    default:
                        break;
                }
                //Stream stream = incoming.files[0].OpenReadStream();
                //byte[] Binary = new byte[stream.Length];
                //stream.Read(Binary, 0, (int)stream.Length);
                //if (ext.ToLower() == ".txt")
                //{
                //    string SimpleText = System.Text.Encoding.UTF8.GetString(Binary);
                //    ItemDocumentSaveCommand cmd = new ItemDocumentSaveCommand(incoming.itemID,
                //        filename,
                //        ext,
                //        SimpleText
                //        );
                //    cmd.doItNow();
                //}
                //else
                //{
                //    ItemDocumentSaveBinCommand cmd = new ItemDocumentSaveBinCommand(incoming.itemID,
                //        filename,
                //        ext,
                //        Binary
                //        );
                //    cmd.doItNow();
                //}



                Attachment newZoteroItemDocument = new Attachment();
                newZoteroItemDocument.itemType = "attachment";
                newZoteroItemDocument.parentItem = _zoteroParentItem.key;
                // this needs to be the zotero document
                // associated with it...
                newZoteroItemDocument.linkMode = "imported_url";
                newZoteroItemDocument.title = _itemDocument.Title;
                newZoteroItemDocument.accessDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                newZoteroItemDocument.url = "";
                newZoteroItemDocument.note = "";
                tagObject tag = new tagObject
                {
                    tag = "awesome:",
                    type = "1"
                };
                relation rel = new relation // TODO hardcoded
                {
                    owlSameAs = "http://zotero.org/groups/1/items/JKLM6543",
                    dcRelation = "http://zotero.org/groups/1/items/PQRS6789",
                    dcReplaces = "http://zotero.org/users/1/items/BCDE5432"
                };
                newZoteroItemDocument.tags = new List<tagObject>() { tag };
                newZoteroItemDocument.relations = rel;
                newZoteroItemDocument.contentType = contentType; 
                newZoteroItemDocument.charset = "";
                newZoteroItemDocument.filename = _itemDocument.Title;
                newZoteroItemDocument.md5 = ""; 
                newZoteroItemDocument.mtime = ""; 

                return newZoteroItemDocument;

            }
            catch (System.Exception ex)
            {
                var detailOfError = ex;
                throw;
            }
}
    }
}
