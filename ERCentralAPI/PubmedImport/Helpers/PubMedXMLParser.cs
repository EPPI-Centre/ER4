using PubmedImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPPIDataServices.Helpers
{
    public static class PubMedXMLParser
    {
		public static CitationRecord ParseCitation(XElement xCit)
		{
			//Type = RisCode.GetType(doc).DisplayName();
			//Title = doc.GetData(RisCode.Title);
			//ParentTitle = doc.GetData(RisCode.ParentTitle);
			//DOI = doc.GetData(RisCode.DOI).ToStandardDOI();
			//PublicationYear = doc.GetData(RisCode.PublicationYear).ToSimpleText();
			//Volume = doc.GetData(RisCode.Volume).ToSimpleText();
			//Issue = doc.GetData(RisCode.Issue).ToSimpleText();
			//Abstract = doc.GetData(RisCode.Abstract);
			//ShortTitle = doc.GetData(RisCode.ShortTitle);
			//Edition = doc.GetData(RisCode.Edition);
			//Url = doc.GetData(RisCode.Url);
			//Publisher = doc.GetData(RisCode.Publisher);
			//Country = doc.GetData(RisCode.Country);
			//Keywords = doc.GetAllKeywords(RisCode.Keywords);
			//Authors = GetAuthors(doc);

			//ParseStartEndPage(doc.GetData(RisCode.StartPage), doc.GetData(RisCode.EndPage));
			//ParseISSN(doc.GetData(RisCode.Issn));

			//TextDuplicateCandidates = new List<string>();

			//SetSearchText();

			CitationRecord res = new CitationRecord();
            //hardcoded == not good!
            res.Type = "Journal Article";
            res.TypeID = 14;

            List<XElement> IDs = xCit.Element("PubmedData").Element("ArticleIdList").Descendants().ToList();//PMID, DOI, etc...
			XElement MedlineCitation = xCit.Element("MedlineCitation");
			XElement DateRevised = MedlineCitation.Element("DateRevised");
			XElement xArticle = MedlineCitation.Element("Article");
			XElement PubmedData = MedlineCitation.Element("PubmedData");
			XElement xMedlineJournalInfo = MedlineCitation.Element("MedlineJournalInfo");
			List<XElement> xTypes = xArticle.Element("PublicationTypeList").Descendants().ToList();//this could be useful to identify trials!
			XElement XKeywords = MedlineCitation.Element("KeywordList");
			if (IDs != null && IDs.Count > 0)
			{
				foreach (XElement ID in IDs)
				{
					string IdType = ID.Attribute("IdType").Value;
					if (IdType == "pubmed")
					{
						res.Urls.Add("https://www.ncbi.nlm.nih.gov/pubmed/" + ID.Value);
						//Console.WriteLine("Importing record, PMID = " + ID.Value + ".");
					}
					//if (IdType == "doi")
					//{
					//	res.DOI = ID.Value;
					//}
					//else if (IdType == "pubmed") { }
					//else if (IdType == "pii") { }//Publisher Item Identifier, see some background here: https://www.uksg.org/serials/doi
					//else if (IdType == "pmc") { }
					res.ExternalIDs.Add(new ExternalID(IdType, ID.Value));
				}
			}
			if (xTypes != null && xTypes.Count == 1)
			{
				foreach (XElement type in xTypes)
				{//a special case, we might want to remove this citation...
					if (type.Value == "Retraction of Publication")
					{
						//Console.WriteLine("Current record is a retraction, skipping...");
						res.Type = "Retraction";//this requires to decide what to do in these cases!
						//citation already contains PMID, so the original one (if any) can be found, problem is what to do with it! (deleting it is not on)
						return res;
					}
				}
			}
			if (XKeywords != null) res.Keywords = GetKeywords(XKeywords);
			XElement xJournal = xArticle.Element("Journal");
			XElement xIssue = xJournal.Element("JournalIssue");
			XElement xPagination = xArticle.Element("Pagination");
			if (xPagination != null)
			{
				string pages = SafeGetValue(xPagination, "MedlinePgn");
				if (pages.NotEmpty()) res.ParseStartEndPage(pages, "");
			}
			res.Title = SafeGetValue(xArticle, "ArticleTitle");
			res.Abstract = SafeGetValue(xArticle, "Abstract");
			res.Issn = SafeGetValue(xJournal, "ISSN");
			res.ParentTitle = SafeGetValue(xJournal, "Title");
			res.Volume = SafeGetValue(xIssue, "Volume");
			res.Issue = SafeGetValue(xIssue, "Issue");
			//pubmed revision date...
			XElement versionedPMID = MedlineCitation.Element("PMID");
            //.Attribute("MajorTopicYN")
            res.PubMedDate = new DateTime(SafeGetValue(DateRevised, "Year").ToInt()
                                          , SafeGetValue(DateRevised, "Month").ToInt()
                                          , SafeGetValue(DateRevised, "Day").ToInt());
			res.PubmedPmidVersion = versionedPMID.Attribute("Version").Value.ToInt();

			if (xMedlineJournalInfo != null)
				res.Country = SafeGetValue(xMedlineJournalInfo, "Country");
			
			XElement xAuthorList = xArticle.Element("AuthorList");
			if (xAuthorList != null)
			{
				foreach (XElement xAuthor in xAuthorList.Elements("Author"))
				{
					Author au = new Author() { AuthorshipLevel = 0, Rank = res.Authors.Count };
					au.FamilyName = SafeGetValue(xAuthor, "LastName");
					au.GivenName = SafeGetValue(xAuthor, "ForeName");
					au.Name = au.FamilyName + (au.GivenName.IsEmpty() ? "" : ", " + au.GivenName);
					if (au.Name.NotEmpty()) res.Authors.Add(au);
				}
			}
			XElement PubDate = xIssue.Element("PubDate");
			XElement xMedlineDate = PubDate.Element("MedlineDate");
			if (xMedlineDate == null)
			{
				res.PublicationYear = SafeGetValue(PubDate, "Year");
				res.Month = SafeGetValue(PubDate, "Month");
			}
			else
			{
				Match match = Regex.Match(xMedlineDate.Value, @"(?<!\d)\d{4}(?!\d)");
				if (match != null && match.Success) res.PublicationYear = match.Value;
				else
				{
					match = Regex.Match(xMedlineDate.Value, @"(?<!\d)\d{2}(?!\d)");
					if (match != null && match.Success) res.PublicationYear = match.Value;
				}
				if (!res.PublicationYear.IsEmpty())
				{//we'll put the rest of xMedlineDate.Value in the month field, usually a range...
					res.Month = xMedlineDate.Value.Replace(res.PublicationYear, "").Trim();
				}
			}
			res.AutoSetShortTitle();
			res.SetSearchText();
			return res;
		}
		private static string SafeGetValue(XElement parent, string ChildElementName)
		{
			if (parent == null || ChildElementName.IsEmpty()) return "";
			XElement child = parent.Element(ChildElementName);
			if (child == null) return "";
			else return child.Value;
		}
		public static List<KeywordObject> GetKeywords(XElement XKeywords)
		{
			List<KeywordObject> kWs = new List<KeywordObject>();
			List<XElement> xKWlist = XKeywords.Elements("Keyword").ToList();
			if (xKWlist != null && xKWlist.Count > 0)
			{
				foreach (XElement xKw in xKWlist)
				{
					kWs.Add(new KeywordObject(xKw.Value, xKw.Attribute("MajorTopicYN").Value == "Y"));
				}
			}
			return kWs;
		}

	}
}
