using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static BusinessLibrary.BusinessClasses.ZoteroERWebItemDocument;
using static BusinessLibrary.BusinessClasses.ZoteroERWebReviewItem;
using ErWebState = BusinessLibrary.BusinessClasses.ZoteroERWebItemDocument.ErWebState;

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

	public abstract class CollectionData
	{
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
		public string archiveLocation { get; set; }
		public string libraryCatalog { get; set; }
		public string callNumber { get; set; }
		public string rights { get; set; }
		public string extra { get; set; }
		public Object tags { get; set; }
		public Object[] collections { get; set; }
		public Object relations { get; set; }
		public string dateAdded { get; set; }
		public string dateModified { get; set; }
	}

	public class WebSite : CollectionData
	{
		public string blogTitle { get; set; }
		public string websiteType { get; set; }
		public List<CreatorsItem> creators { get; set; }
	}
	public class SyncStateDictionaries
	{
		public Dictionary<long, ErWebState> itemSyncStateResults { get; set; }
		public Dictionary<long, ErWebState> docSyncStateResults { get; set; }

	}


	public class CollectionType : CollectionData
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
		public string bookTitle { get; set; } = null;
		public string parentTitle { get; set; } = null;
		public string createdBy { get; set; } = null;
		public string editedBy { get; set; } = null;
		public string institution { get; set; } = null;
		public string comments { get; set; } = null;

		public string parentItem { get; set; } = null;

	}

	public class JournalArticle : CollectionData
	{
		public string publicationTitle { get; set; }
		public string issue { get; set; }
		public string pages { get; set; }
		public string seriesTitle { get; set; }
		public string seriesText { get; set; }
		public string journalAbbreviation { get; set; }
		public string DOI { get; set; }
		public string ISSN { get; set; }
	}

	public class ConferencePaper : CollectionData
	{
		public string proceedingsTitle { get; set; }
		public string conferenceName { get; set; }
		public string place { get; set; }


	}

	public class BookWhole : CollectionData
	{
		public List<CreatorsItem> creators { get; set; }
		public string numberOfVolumes { get; set; }
		public string edition { get; set; }
		public string place { get; set; }
		public string publisher { get; set; }
		public string numPages { get; set; }
		public string ISBN { get; set; }

		public List<tagObject> tags { get; set; }
		public List<string> collections { get; set; }
		public relation relations { get; set; }
	}

	public class BookChapter : CollectionData
	{
		public string bookTitle { get; set; }
		public List<CreatorsItem> creators { get; set; }
		public string numberOfVolumes { get; set; }
		public string edition { get; set; }
		public string place { get; set; }
		public string publisher { get; set; }
		public string numPages { get; set; }
		public string ISBN { get; set; }

		public List<tagObject> tags { get; set; }
		public List<string> collections { get; set; }
		public relation relations { get; set; }
	}

	public class Attachment : CollectionData
	{
		public string parentItem { get; set; }
		public string linkMode { get; set; }
		public string note { get; set; }
		public string contentType { get; set; }
		public string charset { get; set; }
		public string filename { get; set; }
		public string md5 { get; set; }
		public string mtime { get; set; }
	}

	public class CreatorsItem
	{
		public string creatorType { get; set; }
		public string firstName { get; set; }
		public string lastName { get; set; }
	}

	public class Collection
	{
		public string key { get; set; }
		public long version { get; set; }
		public Library library { get; set; }
		public Links links { get; set; }
		public JObject meta { get; set; }
		public CollectionType data { get; set; }
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

		public string type { get; set; }
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
}

