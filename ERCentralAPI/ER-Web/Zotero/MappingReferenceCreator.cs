using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using ERxWebClient2.Controllers;

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
			if (item == null) throw new Exception("item reference null exception");
 
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
                    return new ZoteroBook(item);
                case "bookChapter":
                    return new ZoteroBookChapter(item);
                case "journalArticle": 
                    return new ZoteroJournal(item);
                case "conferencePaper":
                    return new ZoteroConferenceProceeding(item);
                case "blogPost":
                    return new ZoteroWebSite(item);
                case "attachment":
                    return new ZoteroAttachment(item);
                default:
                    //throw new NotSupportedException();
                    // for development return something
                    // TODO production throw the above exception
                    return new ZoteroBook(item);
            }
        }

		// NB: This is a special case as we want ItemIncomingData to be an Item
		// but the fields contain different properties and methods hence cannot be wrapped.
		public override ItemIncomingData GetIncomingDataReference(Collection collectionItem, ERWebItem eRWebItem)
		{
			var incomingData = new ZoteroIncomingDataReference();
            return incomingData.MapReferenceFromZoteroToErWeb(collectionItem.data, new ItemIncomingData(), eRWebItem);

		}
	} 
}
