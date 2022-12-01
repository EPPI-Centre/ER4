using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static BusinessLibrary.BusinessClasses.ZoteroERWebItemDocument;
using static BusinessLibrary.BusinessClasses.ZoteroERWebReviewItem;
//using ErWebState = BusinessLibrary.BusinessClasses.ZoteroERWebItemDocument.ErWebState;
using BusinessLibrary.BusinessClasses;
using ERxWebClient2.Zotero;
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;

namespace ERxWebClient2.Controllers
{

	public class ZoteroApiKey
	{
		public string key { get; set; }
		public long userID { get; set; }
		public string username { get; set; }
		public string displayName { get; set; }

		public Access access { get; set; }
	}

	public class Access
	{
		public User user { get; set; }

		public Groups groups { get; set; }
	}

	public class User
	{
		public bool library { get; set; }
		public bool files { get; set; }
	}

	public class Groups
	{
		public All all { get; set; }
	}

	public class All
	{
		public bool library { get; set; }
		public bool write { get; set; }

	}

	public class ErWebZoteroItemDocument
	{
		public long itemId { get; set; }
		public string parentItemFileKey { get; set; }
		public long itemDocumentId { get; set; }
	}

	public class JsonErrorModel
	{
		public int ErrorCode { get; set; }

		public string ErrorMessage { get; set; }
	}


	public class GroupSelf
	{
		public string href { get; set; }
		public string type { get; set; }
	}

	public class GroupAlternate
	{
		public string href { get; set; }
		public string type { get; set; }
	}

	public class GroupLinks
	{
		public GroupSelf self { get; set; }
		public GroupAlternate alternate { get; set; }
	}

	public class GroupMeta
	{
		public string created { get; set; }
		public string lastModified { get; set; }
		public int numItems { get; set; }
	}

	public class GroupData
	{
		public int id { get; set; }
		public int version { get; set; }
		public string name { get; set; }
		public int owner { get; set; }
		public string type { get; set; }
		public string description { get; set; }
		public string url { get; set; }
		public string libraryEditing { get; set; }
		public string libraryReading { get; set; }
		public string fileEditing { get; set; }
	}

	public class Group
	{
		public int id { get; set; }
		public int version { get; set; }
		public GroupLinks links { get; set; }
		public GroupMeta meta { get; set; }
		public GroupData data { get; set; }
		public bool groupBeingSynced { get; set; }
	}

    public abstract class ZoteroCollectionData
	{

		private string MapFromERWebTypeToZoteroType(string erWebType)
		{
			string zoteroType = "";

			switch (erWebType)
			{
                case "Book, Whole":
                    zoteroType = "book";
                    break;
				case "Report":
					zoteroType = "report";
					break;
				case "Book, Chapter":
					zoteroType = "bookSection";
					break;
				case "Dissertation":
					zoteroType = "thesis";
					break;
				case "Conference Proceedings":
					zoteroType = "conferencePaper";
					break;
				case "Document From Internet Site":
					zoteroType = "webpage";
					break;
				case "Web Site":
					zoteroType = "webpage";
					break;
				case "DVD, Video, Media":
					zoteroType = "film"; //videoRecording //tvBroadcast
					break;
				case "Research project":
					zoteroType = "thesis";
					break;
				case "Article In A Periodical":
					zoteroType = "newspaperArticle";
					break;
				case "Interview":
					zoteroType = "interview";
					break;
				case "Generic":
					zoteroType = "book";
					break;
				case "Journal, Article":
					zoteroType = "journalArticle";
					break;
				default:
					break;
			}
			return zoteroType;
		}
		private List<CreatorsItem> ObtainCreatorsAsAuthors(string authors)
		{
			if (authors.Length == 0)
			{
				return new List<CreatorsItem>();
			}
			//var authorsArray = authors.Split(';');
			var authorsArray = AuthorsHandling.NormaliseAuth.processField(authors, 0);

            var creatorsArray = new List<CreatorsItem>();
			foreach (var author in authorsArray)
			{
				var creatorsItem = new CreatorsItem
				{
					creatorType = "author",
					lastName = author.LastName,
					firstName = author.FirstName + (author.MiddleName.Trim().Length == 0 ? "" : " " + author.MiddleName.Trim())
                };
				creatorsArray.Add(creatorsItem);


				//var firstAndLastNames = author.TrimStart().TrimEnd().Split(' ');
				//if (firstAndLastNames.Count() > 1)
				//{
				//	var creatorsItem = new CreatorsItem
				//	{
				//		creatorType = "author",
				//          lastName = firstAndLastNames[0].ToString(),
				//          firstName = firstAndLastNames[1].ToString()
				//	};
				//	creatorsArray.Add(creatorsItem);
				//}
			}
            return creatorsArray;
		}

		public ZoteroCollectionData(IItem data)
        {
			tagObject tag = new tagObject
            {
                tag = "EPPI-Reviewer ID: " + data.ItemId.ToString(),
                type = "1"
            }; 
			//relation rel = new relation();
			//// TODO hardcoded
			//{
			//	owlSameAs = "http://zotero.org/groups/1/items/JKLM6543",
			//	dcRelation = "http://zotero.org/groups/1/items/PQRS6789",
			//	dcReplaces = "http://zotero.org/users/1/items/BCDE5432"
			//};

            this.itemType = MapFromERWebTypeToZoteroType(data.TypeName);
            this.title = data.Title;
            this.creators = ObtainCreatorsAsAuthors(data.Authors).ToArray();
			this.abstractNote = data.Abstract;
            this.series = "";
            this.seriesNumber = "";            
            this.language = data.Country;
            this.shortTitle = data.ShortTitle;
            this.url = data.URL;
            this.accessDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			this.archive = "";
            //this.archiveLocation = "";
            this.libraryCatalog = "";
            this.callNumber = "";
            this.rights = "";
			var arrayOfIdAndComments = new string[2];
			arrayOfIdAndComments[0] = "EPPI-Reviewer ID: " + data.ItemId.ToString();
			if (data.Comments.ToString().Trim().Length > 0)
				arrayOfIdAndComments[1] = "EPPI-Reviewer Comments: " + data.Comments.ToString().Trim();

			this.extra = string.Join(Environment.NewLine, arrayOfIdAndComments);
            this.tags = new tagObject[1] { tag };
            this.collections = new object[0];
            //this.relations = rel;
            this.dateAdded = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			this.dateModified = ((DateTime) data.DateEdited).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
			this.date = data.Year;
		}

		public ZoteroCollectionData()
        {

        }

		public string key { get; set; }
		public long? version { get; set; } = null;
		public string itemType { get; set; }
		public string title { get; set; }
		public CreatorsItem[] creators { get; set; }
		public string abstractNote { get; set; }
		public string series { get; set; }
		public string seriesNumber { get; set; }
		public string volume { get; set; }
		public string date { get; set; }
		public string language { get; set; }
		public string shortTitle { get; set; }
		public string url { get; set; }
		public string accessDate { get; set; }
		public string archive { get; set; }
		//public string archiveLocation { get; set; } = null;
		public string libraryCatalog { get; set; }
		public string callNumber { get; set; }
		public string rights { get; set; }
		public string extra { get; set; }
		//public Object tags { get; set; } = null;
		public Object[] collections { get; set; }
		public Object relations { get; set; } = new object();
		public string dateAdded { get; set; }
		public string dateModified { get; set; }
		public string DOI { get; set; }

		public tagObject[] tags { get; set; }

    }

	public class WebSite : ZoteroCollectionData
	{
        public WebSite(IItem data, string websiteTitle, string websiteType) : base(data)
        {
            this.websiteTitle = websiteTitle;
            this.websiteType = websiteType;
        }

        public string websiteTitle { get; set; }
		public string websiteType { get; set; }

	}
	public class SyncStateDictionaries
	{
		public Dictionary<long, ZoteroERWebReviewItem.ErWebState> itemSyncStateResults { get; set; }
		public Dictionary<long, ZoteroERWebReviewItem.ErWebState> docSyncStateResults { get; set; }

	}


	public class CollectionType : ZoteroCollectionData
	{
        
        public string numberOfVolumes { get; set; } = null;
		public string edition { get; set; } = null;
		public string place { get; set; } = null;
		public string publisher { get; set; } = null;
		public string numPages { get; set; } = null;
		public string ISBN { get; set; } = null;

		public string publicationTitle { get; set; } = null;
		public string issue { get; set; } = null;
		public string pages { get; set; } = null;
		public string seriesTitle { get; set; } = null;
		public string seriesText { get; set; } = null;
		public string journalAbbreviation { get; set; } = null;
		public string DOI { get; set; } = null;
		public string ISSN { get; set; } = null;
		public string parentTitle { get; set; } = null;
		public string createdBy { get; set; } = null;
		public string editedBy { get; set; } = null;
		public string institution { get; set; } = null;
		public string comments { get; set; } = null;

		public string parentItem { get; set; } = null;

		public string proceedingsTitle { get; set; } = null;

	}
	public class MiniCollectionType
	{
		public	MiniCollectionType(CollectionType fullSize) 
		{
			key = fullSize.key;
			if (fullSize.tags != null) tags = fullSize.tags;
			else tags = Array.Empty<tagObject>();
			extra = fullSize.extra;
			version = fullSize.version ?? -1;
			dateModified = fullSize.dateModified;
        }
        public string dateModified { get; set; }
        public string key { get; set; }
        public long version { get; set; }
        public tagObject[] tags { get; set; }
        public string extra { get; set; }
    }

    //used to update Attachments on the Zotero end
    public class MiniAttachmentCollectionData
    {
		protected MiniAttachmentCollectionData() { }
        public MiniAttachmentCollectionData(long ItemDocumentId, string Key)
		{
			tags = new tagObject[1] { new tagObject()
			{
				type = "1", tag = "EPPI-Reviewer ID: " + ItemDocumentId.ToString()
            }};
			key = Key;
        }
		public string key { get; set; } = "";
        public long version { get; set; } = 0;
        public tagObject[] tags { get; set; } = Array.Empty<tagObject>();
    }

	//used only to push a new attchment to Zotero
    public class MiniAttachmentCollectionDataForPushing
    {
        public MiniAttachmentCollectionDataForPushing() { }
        public tagObject[] tags { get; set; } = Array.Empty<tagObject>();
        public string accessDate { get; set; } = "";
        public string charset { get; set; } = "";
        public string contentType { get; set; } = "";
        public string filename { get; set; } = "";
        public readonly string itemType = "attachment";
        public readonly string linkMode = "imported_file";
        public string? md5 { get; set; } = null;
        public string? mtime { get; set; } = null;
        public string note { get; set; } = "";
        public string parentItem { get; set; } = "";
        public object? relations { get; set; } = new object();
        public string title { get; set; } = "";
    }

    //outer object we receive when we're asking for specific attachments...
    //this is ugly, as it's a copy of "Collection" class, with one member changed
    public class AttachmentCollection
    {
        public string key { get; set; }
        public long version { get; set; }
        public Library library { get; set; }
        public Links links { get; set; }
        public Meta meta { get; set; }
        public AttachmentCollectionData data { get; set; }
    }
    public class AttachmentCollectionData : MiniAttachmentCollectionData
	{
		//public AttachmentCollection(long ItemDocumentId, string Key) : base(ItemDocumentId, Key)
		//{ }
		public AttachmentCollectionData() : base() {}
        public string accessDate { get; set; } = "";
        public string charset { get; set; } = "";
        public string contentType { get; set; } = "";
        public string dateAdded { get; set; } = "";
        public string dateModified { get; set; } = "";
        public string filename { get; set; } = "";

        public readonly string itemType = "attachment"; 
        public readonly string linkMode = "imported_file";
		public string? md5 { get; set; } = null;
        public string? mtime { get; set; } = null;
		public string note { get; set; } = "";
        public string parentItem { get; set; } = "";
        public object? relations { get; set; } = new object();
		public string title { get; set; } = "";
    }

    public class JournalArticle : ZoteroCollectionData, IJournalArticle
	{
		public JournalArticle(IItem data, string publicationTitle, string issue, string pages, string seriesTitle, string seriesText, string journalAbbreviation, string dOI, string iSSN): base(data)
        {
            this.publicationTitle = publicationTitle;
            this.issue = issue;
            this.pages = pages;
            this.seriesTitle = seriesTitle;
            this.seriesText = seriesText;
            this.journalAbbreviation = journalAbbreviation;
			this.DOI = dOI;
			this.ISSN = iSSN;
        }

        public string publicationTitle { get; set; }
		public string issue { get; set; }
		public string pages { get; set; }
		public string seriesTitle { get; set; }
		public string seriesText { get; set; }
		public string journalAbbreviation { get; set; }
		public string DOI { get; set; }
		public string ISSN { get; set; }
	}

	internal interface IJournalArticle
	{
		string publicationTitle { get; set; }
		string issue { get; set; }
		string pages { get; set; }
		string seriesTitle { get; set; }
		string seriesText { get; set; }
		string journalAbbreviation { get; set; }
		string DOI { get; set; }
		string ISSN { get; set; }
	}

	public class ConferencePaper : ZoteroCollectionData
	{
		public ConferencePaper(IItem data, string proceedingstitle, string conferencename, string pLace, string iSBN, string publisher) : base(data)
        {
			this.proceedingsTitle = proceedingstitle;
			this.conferenceName = conferencename;
			this.place = pLace;
			this.ISBN = iSBN;
			this.Publisher = publisher;
		}
		public string proceedingsTitle { get; set; }
		public string conferenceName { get; set; }
		public string place { get; set; }
		public string ISBN { get; set; }

		public string Publisher { get; set; }

	}


	public class ReportZotero : ZoteroCollectionData
	{
		public ReportZotero(IItem data, string proceedingstitle, string conferencename, string pLace, string iSSN) : base(data)
		{
			this.ISSN = iSSN;
			this.proceedingsTitle = proceedingstitle;
			this.conferenceName = conferencename;
			this.place = pLace;

		}
		public string proceedingsTitle { get; set; }
		public string conferenceName { get; set; }
		public string place { get; set; }
		public string ISSN { get; set; }

	}

	public class Periodical : ZoteroCollectionData
	{
		public Periodical(IItem data, string proceedingstitle, string conferencename, string pLace, string iSSN) : base(data)
		{
			this.ISSN = iSSN;
			this.proceedingsTitle = proceedingstitle;
			this.conferenceName = conferencename;
			this.place = pLace;

		}
		public string proceedingsTitle { get; set; }
		public string conferenceName { get; set; }
		public string place { get; set; }
		public string ISSN { get; set; }

	}

	public class Generic : ZoteroCollectionData
	{
		public Generic(IItem data) : base(data)
		{			

		}

	}

	public class Dvd : ZoteroCollectionData
	{
		public Dvd(IItem data) : base(data)
		{

		}

	}

	public class BookWhole : ZoteroCollectionData, IBookWhole
	{
		// parentTitle: { txt: 'Parent Title', optional: true }
	    //              , parentAuthors: { txt: 'Parent Authors', optional: true }
	    //              , standardNumber: { txt: 'ISBN', optional: false }

		public BookWhole(IItem data, string numberOfVolumes, string edition, string place, string publisher,
			string numPages, string iSBN): base(data)
		{
            this.numberOfVolumes = numberOfVolumes;
            this.edition = edition;
            this.place = place;
            this.publisher = publisher;
            this.numPages = numPages;
            this.ISBN = iSBN;
            if (!string.IsNullOrWhiteSpace(data.DOI))
            {
				this.extra = "DOI: " + data.DOI;
			}
		}
		public string numberOfVolumes { get; set; }
		public string edition { get; set; }
		public string place { get; set; }
		public string publisher { get; set; }
		public string numPages { get; set; }
		public string ISBN { get; set; }
      
    }

	internal interface IBookWhole
	{
		string numberOfVolumes { get; set; }
		string edition { get; set; }
		string place { get; set; }
		string publisher { get; set; }
		string numPages { get; set; }
		string ISBN { get; set; }
	}

	public class Dissertation : ZoteroCollectionData
	{
		//parentTitle: { txt: 'Publ. Title', optional: false }
		  //              , parentAuthors: { txt: 'Parent Authors', optional: true }
		  //              , standardNumber: { txt: 'ISSN/ISBN', optional: false }
		public Dissertation(IItem data, string numberOfVolumes, string edition, string place, string publisher,
			string numPages, string iSBN) : base(data)
		{
			this.numberOfVolumes = numberOfVolumes;
			this.edition = edition;
			this.place = place;
			this.publisher = publisher;
			this.numPages = numPages;
			this.ISBN = iSBN;
		}
		public string numberOfVolumes { get; set; }
		public string edition { get; set; }
		public string place { get; set; }
		public string publisher { get; set; }
		public string numPages { get; set; }
		public string ISBN { get; set; }


	}

	public class BookChapter : ZoteroCollectionData , iBookChapter
	{
	
		  //              , parentAuthors: { txt: 'Editors', optional: false }
		public BookChapter(IItem data, string bookTitle, string numberOfVolumes, 
			string edition, string place, string publisher, string numPages, string iSBN ): base(data)
        {
            this.bookTitle = bookTitle;
            this.numberOfVolumes = numberOfVolumes;
            this.edition = edition;
            this.place = place;
            this.publisher = publisher;
			this.ISBN = iSBN;
		}

        public string bookTitle { get; set; }
		public string numberOfVolumes { get; set; }
		public string edition { get; set; }
		public string place { get; set; }
		public string publisher { get; set; }
		public string ISBN { get; set; }

		//the below is WRONG, used to deliberately cause an error!
		//public string numPages = "aaa";


    }

	internal interface iBookChapter
	{
		string bookTitle { get; set; }
		string numberOfVolumes { get; set; }
		string edition { get; set; }
		string place { get; set; }
		string publisher { get; set; }
		string ISBN { get; set; }
	}

	public class Attachment : ZoteroCollectionData
	{
        public Attachment(Item data, string parentItem, string linkMode, string note, string contentType, string charset, string filename, string md5, string mtime): base(data)
        {
            this.parentItem = parentItem;
            this.linkMode = linkMode;
            this.note = note;
            this.contentType = contentType;
            this.charset = charset;
            this.filename = filename;
            this.md5 = md5;
            this.mtime = mtime;
        }

		public string parentItem { get; set; }
		public string linkMode { get; set; }
		public string note { get; set; }
		public string contentType { get; set; }
		public string charset { get; set; }
		public string filename { get; set; }
		public string md5 { get; set; }
		public string mtime { get; set; }
	}


    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CreatorsItem
	{
		public string creatorType { get; set; }
		public string firstName { get; set; }
		public string lastName { get; set; }
		public string name { get; set; }
	}

	public class Collection
	{
		public string key { get; set; }
		public long version { get; set; }
		public Library library { get; set; }
		public Links links { get; set; }
		public Meta meta { get; set; }
		public CollectionType data { get; set; }
	}

	public class CreatedByUser
	{
		public int id { get; set; }
		public string username { get; set; }
		public string name { get; set; }
		public Links links { get; set; }
	}

	public class Meta
	{
		public CreatedByUser createdByUser { get; set; }
		public int numChildren { get; set; }
	}


	public class Links
	{
		public Alternate alternate { get; set; }
		public Self self { get; set; }
		public Up up { get; set; }
		public AttachmentLink attachment { get; set; }

	}

	public class Library
	{
		public string type { get; set; }
		public int id { get; set; }
		public string name { get; set; }
		public Links links { get; set; }
	}

	public class Alternate
	{
		public string href { get; set; }
		public string type { get; set; }
	}

	public class AttachmentLink
	{
		public string href { get; set; }
		public string type { get; set; }
	}

	public class Self
	{
		public string href { get; set; }
		public string type { get; set; }
	}

	public class Up
	{
		public string href { get; set; }
		public string type { get; set; }
	}

	public class iItemReviewID
	{
		public long itemID { get; set; }
		public long itemReviewID { get; set; }
		public long itemDocumentID { get; set; }
	}

	public class iItemReviewIDZoteroKey
	{
		public long itemID { get; set; }
		public long itemReviewID { get; set; }

		public string itemKey { get; set; }

		public string version { get; set; }
	}

	public class tagObject
	{
		public string tag { get; set; }

		public string type { get; set; } = "0";
	}

	public class relation
	{
		[JsonProperty(PropertyName = "owl:sameAs")]
		public string owlSameAs { get; set; }

		[JsonProperty(PropertyName = "dc:relation")]
		public string dcRelation { get; set; }

		[JsonProperty(PropertyName = "dc:replaces")]
		public string dcReplaces { get; set; }

	}

	public class UserDetails
	{
		public int userId { get; set; }

		public long reviewId { get; set; }
	}

	//TODO probably have to remove these
	// why does Zotero not let us access this
	// via the API
	public class PlanQuotas
	{
		public int[] Quotas { get; set; }
	}

	public class Subscription
	{
		public UserSubscription userSubscription { get; set; }
		public StorageGroups storageGroups { get; set; }
		public bool lastPurchaseQuota { get; set; }
		public PlanQuotas planQuotas { get; set; }
	}

	public class StorageGroups
	{
		public Object[] Groups { get; set; }
	}

	public class Usage
	{
		public string total { get; set; }
		public string library { get; set; }
		public Group groups { get; set; }
	}

	public class UserSubscription
	{
		public string userID { get; set; }
		public int storageLevel { get; set; }
		public string quota { get; set; }
		public int expirationDate { get; set; }
		public bool recur { get; set; }
		public string paymentType { get; set; }
		public string orderID { get; set; }
		public Usage usage { get; set; }
		public bool discounted { get; set; }
		public bool discountEligible { get; set; }
		public List<object> institutions { get; set; }
		public bool hasExistingSubscription { get; set; }
	}
    public class ZoteroBatchError
    {
        public ZoteroBatchError(string OpName)
        {
            operationName = OpName;
        }
        public ZoteroBatchError(string OpName, int BatchSize)
        {
			batchSize = BatchSize;
            operationName = OpName;
        }
        public string operationName = ""; //the name of the API call/method
        public int batchSize = 0;
        public int failCount = 0;
        public List<SingleError> failedIdsAndMessage = new List<SingleError>();
		public void Add(SingleError error)
		{
			failedIdsAndMessage.Add(error);
			failCount++;
		}
    }
	public class SingleError
	{
		public SingleError() { }
        public SingleError(Exception e)
        {
            Exception = e;
            ErrorMsg = e.Message;
        }
        public SingleError(Exception e, string Id)
        {
            Exception = e;
            ErrorMsg = e.Message;
			UniqueIdentifier = Id;
        }
        public SingleError(Exception e, string Id, string message)
        {
            Exception = e;
            ErrorMsg = message;
            UniqueIdentifier = Id;
        }
        public SingleError(string Id, string message)
        {
            ErrorMsg = message;
            UniqueIdentifier = Id;
        }
        public SingleError(string message)
        {
            ErrorMsg = message;
        }
        public string ErrorMsg = "";
		public string UniqueIdentifier = "";
		[JsonIgnore]
		public Exception? Exception = null;

		public override string ToString()
		{
			string res = (UniqueIdentifier != "" ? "ID: " + UniqueIdentifier + " " : "") 
				+ (ErrorMsg != "" ? "Error Msg: " + ErrorMsg + " " : "") 
				+ (Exception != null ? "Exception Message: " + Exception.Message : "");
			res = res.Trim();
			return res;
        }
	}

	public class PutErrorResult
    {
		public int code { get; set; }
		public string message { get; set; }
    }
}

