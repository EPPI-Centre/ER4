using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
//using ErWebState = BusinessLibrary.BusinessClasses.ZoteroERWebItemDocument.ErWebState;
using BusinessLibrary.BusinessClasses;

using ER_Web.Zotero;
using System.Text.RegularExpressions;

namespace ERxWebClient2.Controllers
{

	public class ErWebZoteroItemDocument
	{
		public long itemId { get; set; }
		public string parentItemFileKey { get; set; }
		public long itemDocumentId { get; set; }
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
				case "Journal, Article":
					zoteroType = "journalArticle";
					break;
                case "Book, Whole":
                    zoteroType = "book";
                    break;
				case "Report":
					zoteroType = "report";
					break;
				case "Book, Chapter":
					zoteroType = "bookSection";
					break;
				case "Generic":
					zoteroType = "document";
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
					zoteroType = "report";
					break;
				case "Article In A Periodical":
					zoteroType = "newspaperArticle";
					break;
				case "Interview":
					zoteroType = "interview";
					break;
				default:
                    zoteroType = "document";
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
                string tmp = author.FullNameClean;
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    var NewCreator = new CreatorsItem
                    {
                        creatorType = "author",
                        name = tmp
                    };
                    creatorsArray.Add(NewCreator);
                }
            }
            return creatorsArray;
		}
        /// <summary>
        /// Called by specific reference-type classes, as/when required
        /// </summary>
        /// <param name="parentAuthors">String of authors to process</param>
        /// <param name="creatorType">The value to use for Zotero's creatorType</param>
        protected void BuildParentAuthors(string parentAuthors, string creatorType)
        {
            List<CreatorsItem> tempList = new List<CreatorsItem>();
            var authorsArray = AuthorsHandling.NormaliseAuth.processField(parentAuthors, 0);
            foreach (var author in authorsArray)
            {
				string tmp = author.FullNameClean;
                if (!string.IsNullOrWhiteSpace(tmp))
				{
					var NewCreator = new CreatorsItem
					{
						creatorType = creatorType,
						name = tmp
                    };
					tempList.Add(NewCreator);
				}
            }
            //concatenate existing this.creators with tempList 
            this.creators = this.creators.Concat(tempList).ToArray();//adds templist to this.creators array
        }
        
		protected void AddLineToExtraField(string prefix, string value)
		{
			if (this.extra == null || this.extra.Trim() == "") this.extra = prefix + value.Trim();
			else this.extra += Environment.NewLine + prefix + value.Trim();
        }
		
		/// <summary>
        /// Used for Push, common method for all reference types
        /// </summary>
        /// <param name="data"></param>
        public ZoteroCollectionData(IItem data)
        {
			
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
            //this.language = data.Country;
            this.shortTitle = data.ShortTitle;
            this.url = data.URL;
            this.accessDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			this.archive = "";
            //this.archiveLocation = "";
            this.libraryCatalog = "";
            this.callNumber = "";
            this.rights = "";
			var eppiIdCountryCommentsKeywords = new string[3];
			eppiIdCountryCommentsKeywords[0] = "EPPI-Reviewer ID: " + data.ItemId.ToString();
			if (data.Comments.ToString().Trim().Length > 0)
				eppiIdCountryCommentsKeywords[1] = ZoteroReferenceCreator.searchForComments + data.Comments.ToString().Trim();
			if (data.Country.ToString().Trim().Length > 0)
				eppiIdCountryCommentsKeywords[2] = ZoteroReferenceCreator.searchForCountry + data.Country.ToString().Trim();
			//if (data.Keywords.ToString().Trim().Length > 0)
			//	eppiIdCountryCommentsKeywords[3] = "EPPI-Reviewer Keywords: " + data.Keywords.ToString().Trim();
			//if (!string.IsNullOrWhiteSpace(data.DOI))
			//{
			//	eppiIdCountryCommentsKeywords[4] = "EPPI-Reviewer DOI: " + data.DOI.ToString().Trim();
			//}
			this.extra = string.Join(Environment.NewLine, eppiIdCountryCommentsKeywords);
			BuildTags(data);
            this.collections = new object[0];
            //this.relations = rel;
            this.dateAdded = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			this.dateModified = ((DateTime) data.DateEdited).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
			this.date = data.Year.Trim();
			if (this.date != "" && data.Month.Trim() != "") this.date += ", " + data.Month.Trim();
		}
        private void BuildTags(IItem data)
		{
            tagObject tag = new tagObject
            {
                tag = "EPPI-Reviewer ID: " + data.ItemId.ToString(),
                type = "1"
            };
            string[] keywordLines = new string[0];
			if (!string.IsNullOrWhiteSpace(data.Keywords))
			{
				string[] keywordSeparator = FindKeywordSeparator(data.Keywords);
				keywordLines = data.Keywords.Trim().Split(keywordSeparator, StringSplitOptions.RemoveEmptyEntries);
			}
			List<tagObject> list = new List<tagObject>();
            list.Add(tag);
            foreach (string keyword in keywordLines)
			{//splitted with our best guess on how to separate them...
				if (keyword.Length > 254)
				{//won't add a keyword longer than 254 chars (max is actually 256!), would fail on the Zot side!
                    tag = new tagObject
                    {
                        tag = keyword.Substring(0, 254).Trim(),
                        type = "0"
                    };
				}
				else
				{
					tag = new tagObject
					{
						tag = keyword.Trim(),
						type = "0"
					};
				}
                list.Add(tag);
            }
            this.tags = list.ToArray();
        }
        private string[] FindKeywordSeparator(string Keywords)
		{
			List<string> res = new List<string>();
			Dictionary<string, int> scores = new Dictionary<string, int>();
			int len = Keywords.Length;
			if (len <= 6) return res.ToArray();
            string[] separators4Keywords = { "\r\n", "\r", "\n", ";", "\t", ",", "\\.", ":" }; //Does not include space!!
            //we don't want keywords to be more that 30 chars long, on average
            int aimFor = (int)Math.Ceiling((double)len / 30.0);
            //we don't want keywords to be less than 7 chars long, on average (mean word-length in Eng, including aritcles, prepositions, etc. is around 5)
            int tooMany = (int)Math.Ceiling((double)len / 7.0);

			//first simple attempt, use all separators, see if it gives us a nice result
			string[] throwAway = Keywords.Split(separators4Keywords, StringSplitOptions.RemoveEmptyEntries);
			if (throwAway.Length >= aimFor && throwAway.Length <= tooMany)
			{
				separators4Keywords[6] = ".";
                return separators4Keywords;
			}
			
			//too bad, the quick and easy didn't work...
            int totalMatches = 0;
            foreach (string s in separators4Keywords)
			{
				
				int count = Regex.Matches(Keywords, s).Count;
				if (count > 0)
				{//good, this might be an ideal separator
					if (count >= aimFor && count <= tooMany )
					{//we found the ideal separator
                        if (s == "\\.") res.Add(".");
                        else res.Add(s);
                        return res.ToArray();
                    }
					else if (count <= tooMany)//we won't add a separator that matches too often!!
					{
                        if (s == "\\.") scores.Add(".", count);
                        else scores.Add(s, count);
                    }
				}
            }
			//if we reached this point, no SINGLE separator appeared to be good enough :-(
			//so we'll return a number of separators, based on the ones we've collected so far.
			//we want to return the minimum number of separators, so we sort them in desc order
            Dictionary<string, int> sortedDict = (from entry in scores orderby entry.Value descending select entry).ToDictionary(k=> k.Key, v=> v.Value);
			
			foreach (KeyValuePair<string, int> kvp in sortedDict)
			{
				totalMatches += kvp.Value;
				res.Add(kvp.Key);
				if (totalMatches > aimFor)
				{//alright, we have what we came for...
					return res.ToArray();
				}
			}
			//if we reached this point, no combination of separators appeared to be good enough :-(
			//so, does a simple "space" work well?
			string[] space = { " " };
            throwAway = Keywords.Split(space, StringSplitOptions.RemoveEmptyEntries);
            if (throwAway.Length >= aimFor && throwAway.Length <= tooMany) return space;
			//meh, not even "just space" worked, we'll add "space" to our results, return and hope for the best...
			res.Add(" ");
            return res.ToArray();
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

		public tagObject[] tags { get; set; }

    }

	public class WebSite : ZoteroCollectionData
	{
        public WebSite(IItem data, string websiteTitle, string websiteType) : base(data)
        {
            this.websiteTitle = websiteTitle;
            this.websiteType = websiteType;
            if (data.DOI != null && data.DOI.Trim() != "") AddLineToExtraField(ZoteroReferenceCreator.searchForDOI, data.DOI);
        }

        public string websiteTitle { get; set; }
		public string websiteType { get; set; }

	}


	public class CollectionType : ZoteroCollectionData
	{
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
		//public string parentTitle { get; set; } = null;
		public string createdBy { get; set; } = null;
		public string editedBy { get; set; } = null;
		public string institution { get; set; } = null;
		public string comments { get; set; } = null;

		public string parentItem { get; set; } = null;

		public string proceedingsTitle { get; set; } = null;

		public string university { get; set; } = null;

		public string bookTitle { get; set; } = null;

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
			BuildParentAuthors(data.ParentAuthors, "editor");
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
			this.pages = data.Pages;
			if (data.DOI != null && data.DOI.Trim() != "") this.DOI = data.DOI.Trim();
			BuildParentAuthors(data.ParentAuthors, "editor");
		}
		public string proceedingsTitle { get; set; }
		public string conferenceName { get; set; }
		public string place { get; set; }
		public string ISBN { get; set; }
		public string DOI { get; set; } = "";
		public string pages { get; set; } = "";

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
			this.pages = data.Pages;
			this.institution = data.Institution;
			BuildParentAuthors(data.ParentAuthors, "seriesEditor");
            if (data.DOI != null && data.DOI.Trim() != "") AddLineToExtraField(ZoteroReferenceCreator.searchForDOI, data.DOI);

        }
		public string proceedingsTitle { get; set; }
		public string conferenceName { get; set; }
		public string place { get; set; }
		public string ISSN { get; set; }
        public string pages { get; set; }
        public string institution { get; set; }

    }

	public class Periodical : ZoteroCollectionData
	{
		public Periodical(IItem data, string proceedingstitle, string conferencename, string pLace, string iSSN) : base(data)
		{
			this.ISSN = iSSN;
			this.proceedingsTitle = proceedingstitle;
			this.conferenceName = conferencename;
			this.place = pLace;
            if (data.DOI != null && data.DOI.Trim() != "") AddLineToExtraField(ZoteroReferenceCreator.searchForDOI, data.DOI);

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
            if (data.DOI != null && data.DOI.Trim() != "") AddLineToExtraField(ZoteroReferenceCreator.searchForDOI, data.DOI);
			if (data.Publisher != null && data.Publisher.Trim() != "")
			{
                AddLineToExtraField("EPPI-Reviewer Publisher: ", data.Publisher);
            }
			if (data.ParentTitle != null && data.ParentTitle.Trim() != "") publisher = data.ParentTitle.Trim();
			else publisher = "";
        }
        public string publisher { get; set; }
    }

	public class Dvd : ZoteroCollectionData
	{
		public Dvd(IItem data) : base(data)
		{

            if (data.DOI != null && data.DOI.Trim() != "") AddLineToExtraField(ZoteroReferenceCreator.searchForDOI, data.DOI);
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
            if (data.DOI != null && data.DOI.Trim() != "") AddLineToExtraField(ZoteroReferenceCreator.searchForDOI, data.DOI);
            BuildParentAuthors(data.ParentAuthors, "Editor");
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
		public Dissertation(IItem data, string edition, string place, 
			string numPages) : base(data)
		{
			//this.numberOfVolumes = numberOfVolumes;
			//this.edition = edition;
			this.place = place;
			if (!string.IsNullOrWhiteSpace(data.Institution))
			{
				this.university = data.Institution;
			}
			else if (!string.IsNullOrWhiteSpace(data.ParentTitle)) this.university = data.ParentTitle;
			else this.university = string.Empty;
            this.numPages = numPages;
            if (data.DOI != null && data.DOI.Trim() != "") AddLineToExtraField(ZoteroReferenceCreator.searchForDOI, data.DOI);
            BuildParentAuthors(data.ParentAuthors, "contributor");
		}
		public string numberOfVolumes { get; set; }
		//public string edition { get; set; }
		public string place { get; set; }
		public string university { get; set; }
		public string numPages { get; set; }

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
			this.pages = numPages;
            if (data.DOI != null && data.DOI.Trim() != "") AddLineToExtraField(ZoteroReferenceCreator.searchForDOI, data.DOI);
            BuildParentAuthors(data.ParentAuthors, "editor");
		}

        public string bookTitle { get; set; }
		public string numberOfVolumes { get; set; }
		public string edition { get; set; }
		public string place { get; set; }
		public string publisher { get; set; }
		public string ISBN { get; set; }

		public string pages { get; set; }


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



	public class tagObject
	{
		public string tag { get; set; }

		public string type { get; set; } = "0";
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

