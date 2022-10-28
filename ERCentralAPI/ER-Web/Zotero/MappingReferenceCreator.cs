using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using Csla;
using ERxWebClient2.Controllers;
using Microsoft.CodeAnalysis;
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

        public override IMapERWebReference GetReference(IItem item)
        {
			if (item != null) throw new Exception("item reference null exception");
 
				switch (item.TypeId)
				{
					case 2:
						return new ERWebBook(item);
					case 3:
						return new ERWebBookChapter(item);
					case 14:
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

		// This is a special case as we want ItemIncomingData to be an Item
		// but the fields contain different Names.
		public override ItemIncomingData GetIncomingDataReference(Collection collectionItem)
		{
			var incomingData = new ZoteroIncomingDataReference();
            return incomingData.MapReferenceFromZoteroToErWeb(collectionItem.data, new ItemIncomingData());

		}
	} 
}
