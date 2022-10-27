using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Controllers;
using System;

namespace ERxWebClient2.Zotero
{
    public sealed class MappingReferenceCreator : ReferenceCreator
    {
        static MappingReferenceCreator _instance = null;

        static readonly object padLock = new object();

        public static MappingReferenceCreator Instance 
        {                
            get
            {
				lock (padLock)
				{
                    if (_instance == null)
                    {
                        _instance = new MappingReferenceCreator();
                    }
                    return _instance;
                }               
            }
        }

        private MappingReferenceCreator()
		{

		}

        public override IMapERWebReference GetReference(ERWebItem eRWebItem)
        {
            if (eRWebItem.Item != null)
            {
                var item = eRWebItem.Item;
                switch (item.TypeName)
                {
                    case "Book, Whole":
                        return new ERWebBook(item);
                    case "Book, Chapter":
                        return new ERWebBookChapter(item);
                    case "Journal, Article":
                        return new ERWebJournal(item);
                    //case "Conference Proceedings":
                    //    return new ERWebConferenceProceeding(item);
                    //case "Web Site":
                    //    return new ERWebWebSite(item);
                    default:
                        //    //throw new NotSupportedException();
                        //    // for development return something
                        //    // TODO production throw the above exception
                      return new ERWebBook(item);
                }
            }
            else
            {
                var itemDocument = eRWebItem.ItemDocument;
                //return new ERWebDocument(itemDocument, null);
                return new ERWebBook(null);
            }
        }

        public override IMapZoteroReference GetReference(Collection item)
        {
            switch (item.data.itemType)
            {
                case "book": 
                    return new ZoteroBook(item.data);
                case "bookChapter":
                    return new ZoteroBookChapter(item.data);
                case "journalArticle": 
                    return new ZoteroJournal(item.data);
                case "conferencePaper":
                    return new ZoteroConferenceProceeding(item.data);
                case "blogPost":
                    return new ZoteroWebSite(item.data);
                case "attachment":
                    return new ZoteroAttachment(item.data);
                default:
                    //throw new NotSupportedException();
                    // for development return something
                    // TODO production throw the above exception
                    return new ZoteroBook(item.data);
            }
        }
    }

}
