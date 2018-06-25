using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using EPPIDataServices.Helpers;
//do you see me?
namespace PubmedImport
{
	//http://xmlns.com/foaf/spec/


	public class CitationRecord 
	{
        public Int64 CitationId { get; set; }

		//http://purl.org/dc/terms/title
		public string Title { get; set; }

		//http://www.sparontologies.net/ontologies/fabio/source.html#d4e1765
		//http://purl.org/spar/fabio/Abstract
		public string Abstract { get; set; }

		//http://www.sparontologies.net/ontologies/fabio/source.html#d4e1631
		//http://purl.org/spar/fabio/hasShortTitle
		public string ShortTitle { get; set; }

		//what is this ?
		public string CitationText { get; set; }

		//http://www.sparontologies.net/ontologies/fabio/source.html#d4e749
		//http://prismstandard.org/namespaces/basic/2.0/issn
		public string Issn { get; set; }

		//http://www.sparontologies.net/ontologies/fabio/source.html#d4e903
		//http://prismstandard.org/namespaces/basic/2.0/volume
		public string Volume { get; set; }

		//http://xmlns.com/foaf/spec/
		public List<Author> Authors { get; set; }
		public string AuthorsString
		{
			get
			{
				if (Authors == null) return "";
				return string.Join("; ", Authors.Where(c => c.AuthorshipLevel == 0).Select(x => x.Name).ToArray());
			}
		}

		//http://prismstandard.org/namespaces/basic/2.0/doi
		//public string DOI { get; set; }

			//http://purl.org/spar/fabio/hasPublicationYear
		public string PublicationYear { get; set; }

		//http://prismstandard.org/namespaces/basic/2.0/startingPage
		public string StartPage { get; set; }

		//http://prismstandard.org/namespaces/basic/2.0/endingPage
		public string EndPage { get; set; }

		//http://prismstandard.org/namespaces/basic/2.0/issueIdentifier
		public string Issue { get; set; }

		//http://purl.org/dc/terms/publisher
		public string Publisher { get; set; }

		//http://purl.org/spar/fabio/Journal & more
		public string ParentTitle { get; set; }

		public string Month { get; set; }

		public string City { get; set; }

		public string Country { get; set; }

		public string SearchIssn { get; set; }

		public string Place
		{
			get
			{
				string res = "";
				if (City.IsEmpty() && !Country.IsEmpty()) res = Country;
				else if (!City.IsEmpty() && Country.IsEmpty()) res = City;
				else if (!City.IsEmpty() && !Country.IsEmpty()) res = City + ", " + Country;
				return res;
			}
		}

		public string Pages { get; set; }

		//http://purl.org/spar/fabio/Expression
		public string Type { get; set; }
        public int TypeID { get; set; }
		public string SearchText { get; set; }

		public string SearchStartPage { get; set; }

		//http://prismstandard.org/namespaces/basic/2.0/edition
		public string Edition { get; set; }

		//http://purl.org/spar/fabio/hasURL
		public List<string> Urls { get; set; } = new List<string>();

		//http://prismstandard.org/namespaces/basic/2.0/keyword
		//prismstandard links do not resolve
		public List<KeywordObject> Keywords { get; set; }

		//ISSN,Volume,Issue,Start page
		public string SearchIVIS { get; set; }

		public List<string> TextDuplicateCandidates { get; set; }

		public bool ManuallyAdded { get; set; } = false;

		public List<ExternalID> ExternalIDs { get; set; }
        private string ExternalIdsString
        {
            get
            {
                string res = "";
                if (ExternalIDs == null || ExternalIDs.Count == 0) return res;
                foreach (ExternalID ExId in ExternalIDs)
                {
                    res = res + ExId.Name + "¬" + ExId.Value + "; ";
                }
                res = res.TrimEnd();
                return res.TrimEnd(';');
            }
        }

		public DateTime PubMedDate { get; set; }
        public int PubmedPmidVersion { get; set; }

        public CitationRecord()
		{
			Title = "";
			Abstract = "";
			ShortTitle = "";
			Issn = "";
			Volume = "";
			//DOI = "";
			PublicationYear = "";
			StartPage = "";
			EndPage = "";
			Publisher = "";
			ParentTitle = "";
			Month = "";
			City = "";
			Country = "";
			Pages = "";
			Type = "";
			Edition = "";
			Keywords = new List<KeywordObject>();
			Authors = new List<Author>();
			ExternalIDs = new List<ExternalID>();
			CitationText = "";
			SearchText = "";
			SearchIVIS = "";
			TextDuplicateCandidates = new List<string>();
			SearchIssn = "";

		}


		public void AddTextDuplicateCandidates(List<string> list)
		{
			if (list == null || list.Count == 0)
				return;

			TextDuplicateCandidates.AddRange(list);
			TextDuplicateCandidates = TextDuplicateCandidates.Distinct().ToList();
		}

		


		private void ParseISSN(string s)
		{
			Issn = s.Replace("issn", "").Replace(":", "");

			SearchIssn = s.ToSimpleText().Replace("issn", "").Replace(":", "");
		}

		public void ParseStartEndPage(string start, string end)
		{
			//this '–' manifests as a joker, need to fix encoding, or pre-clean troublesome chars
			if (start.Contains('–')) { }

			if (start.Contains('-'))
			{
				var split = start.Split('-');

				StartPage = split[0];
				if (split.Length > 1)
					EndPage = split[1];

				return;
			}

			StartPage = start.ToSimpleText().TrimStart('s');
			EndPage = end.ToSimpleText();
		}

		public void SetSearchText()
		{
			var tempTitle = Title;

			if (tempTitle.IndexOf('(') > 15)
				tempTitle = Regex.Replace(tempTitle, @" ?\(.*?\)", string.Empty);

			if (tempTitle.IndexOf('[') > 15)
				tempTitle = Regex.Replace(tempTitle, @" ?\[.*?\]", string.Empty);

			SearchText = $"{tempTitle}".ToShortSearchText();


			if (SearchIssn.NotEmpty() && Volume.NotEmpty() && Issue.NotEmpty() && StartPage.NotEmpty())
				SearchIVIS = $"{SearchIssn}-{Volume}-{Issue}-{StartPage}";
		}

		

        



		//private List<Author> GetAuthors(List<SearchItemAuthor> authors)
		//{
		//	return authors.Select(author =>
		//		new Author() { FamilyName = author.FamilyName, Name = author.Name, GivenName = author.GivenName, AuthorshipLevel = 1 })
		//		.ToList();
		//}

		//private List<KeywordObject> GetSubjectHeadings(List<SearchItemSubjectHeading> headings)
		//{
		//	var keys = new Dictionary<string, bool>();

		//	IEnumerable<KeywordObject> enumerate()
		//	{
		//		foreach (var k in keys)
		//			yield return new KeywordObject(k.Key, k.Value);
		//	}

		//	foreach (var h in headings)
		//		if (h.IsMajor || !keys.ContainsKey(h.Title))
		//			keys[h.Title] = h.IsMajor;
		//	return enumerate().ToList();
		//}

		private Author BindPerson(string text, int autorshipLevel)
        {
            text = text.Replace("  ", " ");

            var ret = new Author {Name = text, AuthorshipLevel = autorshipLevel };

            if (text.Contains(" "))
            {
                var spliName = text.Split(new[] { ' ' }, 2);
                ret.FamilyName = spliName[0];
                ret.GivenName = spliName[1];
            }
            else
                ret.FamilyName = text;

            return ret;
        }

        public string GetAuthorString()
        {
            return string.Join("; ", Authors.Where(c => c.AuthorshipLevel == 0).Select(x => x.Name).ToArray());
		}
		public string GetParentAuthorString()
		{
			return string.Join("; ",  Authors.Where(c => c.AuthorshipLevel == 1).Select(x => x.Name).ToArray());
		}
		public string GetKeywordsString()
		{
			string res = "";
			foreach (KeywordObject Keyw in Keywords)
			{
				res += ((Keyw.Major) ? "*" : "") + Keyw.Name + Environment.NewLine;
			}
			return res;
		}
		public string GetDisplayPages()
        {
            if (!string.IsNullOrEmpty(Pages))
                return Pages;

            if (!string.IsNullOrEmpty(StartPage))
                return string.IsNullOrEmpty(EndPage) ? StartPage : $"{StartPage}-{EndPage}";

            return "";
        }

       

		public string ExternalIDStringByType(string ExternalIdtype)
		{
			ExternalID exID = ExternalIDByType(ExternalIdtype);
			if (exID == null) return "";
			else return exID.Value;
		}
		public ExternalID ExternalIDByType(string ExternalIdtype)
		{
			if (ExternalIDs == null) return null;
			foreach (ExternalID exID in ExternalIDs)
			{
				if (exID.Name.ToLower() == ExternalIdtype.ToLower())
				{ return exID; }
			}
			return null;
		}
		public void AutoSetShortTitle()
		{//in here logic to determine short tile
			if (Authors != null && Authors.Count > 0)
			{
				if (PublicationYear != null && PublicationYear != "")
				{
					ShortTitle = Authors[0].FamilyName + " (" + PublicationYear + ")";
					return;
				}
				ShortTitle = CutLongTitle() + " (" + Authors[0].FamilyName + ")";
				return;
			}
			if (PublicationYear != null && PublicationYear != "")
			{
				ShortTitle = CutLongTitle() + " (" + PublicationYear + ")";
				return;
			}
			ShortTitle = CutLongTitle();
		}
		private string CutLongTitle()
		{
			if (Title == null || Title == "") return "{Missing Title}";
			if (Title.Length < 20) return Title;
			int usefulSp = Title.IndexOf(' ', 19);
			if (Title.Length > 20 & usefulSp < 55 & usefulSp > 0)
				return Title.Substring(0, usefulSp) + "...";
			else
			{
				int mke = Title.Length > 49 ? 49 : Title.Length - 1;
				usefulSp = Title.LastIndexOf(' ', mke);
				if (usefulSp != -1)
					return Title.Substring(0, usefulSp) + "...";
				usefulSp = Title.LastIndexOfAny(new char[] { '-', '_', '\\', '/', ',', '.', ':', ';', '?', '\'', '!', '"', '£', '$', '%', '^', '&', '*', '`', '|', '¦', '+', '=' }
					, mke);
				if (usefulSp != -1)
					return Title.Substring(0, usefulSp);
				return Title.Substring(0, 20) + "...";
			}
		}
        
        public void SaveSelf(SqlConnection conn, SQLHelper SqlHelper)
        {
            if (CitationId == 0) InsertSelf(conn, SqlHelper);
            else UpdateSelf(conn, SqlHelper);
        }
        public void InsertSelf(SqlConnection conn, SQLHelper SqlHelper)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            SqlParameter IdParam = new SqlParameter("@REFERENCE_ID", (Int64)(-1));
            IdParam.Direction = System.Data.ParameterDirection.Output;
            Parameters.Add(IdParam);
            Parameters.Add(new SqlParameter("@TITLE", Title));
            Parameters.Add(new SqlParameter("@TYPE_ID", TypeID));
            Parameters.Add(new SqlParameter("@PARENT_TITLE", ParentTitle));
            Parameters.Add(new SqlParameter("@SHORT_TITLE", ShortTitle));
            //command.Parameters.Add(new SqlParameter("@DATE_CREATED", ReadProperty(DateCreatedProperty).DBValue));
            //command.Parameters.Add(new SqlParameter("@CREATED_BY", ReadProperty(CreatedByProperty)));
            //command.Parameters.Add(new SqlParameter("@DATE_EDITED", ReadProperty(DateEditedProperty).DBValue));
            //command.Parameters.Add(new SqlParameter("@EDITED_BY", ReadProperty(EditedByProperty)));
            Parameters.Add(new SqlParameter("@YEAR", PublicationYear));
            Parameters.Add(new SqlParameter("@MONTH", Month));
            Parameters.Add(new SqlParameter("@STANDARD_NUMBER", Issn));
            Parameters.Add(new SqlParameter("@CITY", City));
            Parameters.Add(new SqlParameter("@COUNTRY", Country));
            Parameters.Add(new SqlParameter("@PUBLISHER", Publisher));
            //command.Parameters.Add(new SqlParameter("@INSTITUTION", ReadProperty(InstitutionProperty)));
            Parameters.Add(new SqlParameter("@VOLUME", Volume));
            Parameters.Add(new SqlParameter("@PAGES", Pages));
            Parameters.Add(new SqlParameter("@EDITION", Edition));
            Parameters.Add(new SqlParameter("@ISSUE", Issue));
            Parameters.Add(new SqlParameter("@EXTERNAL_IDS", ExternalIdsString));
            //command.Parameters.Add(new SqlParameter("@IS_LOCAL", ReadProperty(IsLocalProperty)));
            //command.Parameters.Add(new SqlParameter("@AVAILABILITY", ReadProperty(AvailabilityProperty)));
            Parameters.Add(new SqlParameter("@URLS", string.Join("¬", Urls.ToArray())));
            //command.Parameters.Add(new SqlParameter("@COMMENTS", ReadProperty(CommentsProperty)));
            Parameters.Add(new SqlParameter("@ABSTRACT", Abstract));
            //command.Parameters.Add(new SqlParameter("@DOI", ReadProperty(DOIProperty)));
            Parameters.Add(new SqlParameter("@KEYWORDS", GetKeywordsString()));
            SqlHelper.ExecuteNonQuerySP(conn, "st_ReferenceInsert", Parameters.ToArray());
            CitationId = (Int64)IdParam.Value;
            SaveAuthors(conn, SqlHelper);
        }
        public void UpdateSelf(SqlConnection conn, SQLHelper SqlHelper)
        {
            List<SqlParameter> Parameters = new List<SqlParameter>();
            SqlParameter IdParam = new SqlParameter("@REFERENCE_ID", CitationId);
            //IdParam.Direction = System.Data.ParameterDirection.Output;
            Parameters.Add(IdParam);
            Parameters.Add(new SqlParameter("@TITLE", Title));
            Parameters.Add(new SqlParameter("@TYPE_ID", TypeID));
            Parameters.Add(new SqlParameter("@PARENT_TITLE", ParentTitle));
            Parameters.Add(new SqlParameter("@SHORT_TITLE", ShortTitle));
            //command.Parameters.Add(new SqlParameter("@DATE_CREATED", ReadProperty(DateCreatedProperty).DBValue));
            //command.Parameters.Add(new SqlParameter("@CREATED_BY", ReadProperty(CreatedByProperty)));
            //command.Parameters.Add(new SqlParameter("@DATE_EDITED", ReadProperty(DateEditedProperty).DBValue));
            //command.Parameters.Add(new SqlParameter("@EDITED_BY", ReadProperty(EditedByProperty)));
            Parameters.Add(new SqlParameter("@YEAR", PublicationYear));
            Parameters.Add(new SqlParameter("@MONTH", Month));
            Parameters.Add(new SqlParameter("@STANDARD_NUMBER", Issn));
            Parameters.Add(new SqlParameter("@CITY", City));
            Parameters.Add(new SqlParameter("@COUNTRY", Country));
            Parameters.Add(new SqlParameter("@PUBLISHER", Publisher));
            //command.Parameters.Add(new SqlParameter("@INSTITUTION", ReadProperty(InstitutionProperty)));
            Parameters.Add(new SqlParameter("@VOLUME", Volume));
            Parameters.Add(new SqlParameter("@PAGES", Pages));
            Parameters.Add(new SqlParameter("@EDITION", Edition));
            Parameters.Add(new SqlParameter("@ISSUE", Issue));
            Parameters.Add(new SqlParameter("@EXTERNAL_IDS", ExternalIdsString));
            //command.Parameters.Add(new SqlParameter("@IS_LOCAL", ReadProperty(IsLocalProperty)));
            //command.Parameters.Add(new SqlParameter("@AVAILABILITY", ReadProperty(AvailabilityProperty)));
            Parameters.Add(new SqlParameter("@URLS", string.Join("¬", Urls.ToArray())));
            //command.Parameters.Add(new SqlParameter("@COMMENTS", ReadProperty(CommentsProperty)));
            Parameters.Add(new SqlParameter("@ABSTRACT", Abstract));
            //command.Parameters.Add(new SqlParameter("@DOI", ReadProperty(DOIProperty)));
            //Parameters.Add(new SqlParameter("@KEYWORDS", string.Join("¬", Keywords.ToArray())));

            SqlHelper.ExecuteNonQuerySP(conn, "st_ReferenceUpdate", Parameters.ToArray());
            SaveAuthors(conn, SqlHelper);
        }
        public void DeleteSelf(SqlConnection conn, SQLHelper SqlHelper)
        {

        }
        protected void SaveAuthors(SqlConnection connection, SQLHelper SqlHelper)
        {
            List<Author> AuthorLi = Authors.Where(c => c.AuthorshipLevel == 0).ToList();
            List<Author> ParentAuthors = Authors.Where(c => c.AuthorshipLevel == 1).ToList();
            SqlHelper.ExecuteNonQuerySP(connection, "st_ReferenceAuthorDelete", new SqlParameter("@REFERENCE_ID", CitationId));
            int rank = 0;
            foreach (Author a in AuthorLi)
            {
                SqlHelper.ExecuteNonQuerySP(connection, "st_ReferenceAuthorUpdate"
                                            , new SqlParameter("@REFERENCE_ID", CitationId)
                                            , new SqlParameter("@RANK", rank)
                                            , new SqlParameter("@ROLE", 0)
                                            , new SqlParameter("@LAST", a.FamilyName)
                                            , new SqlParameter("@FIRST", a.GivenName)
                                            );
                rank++;
            }
            rank = 0;
            foreach (Author a in ParentAuthors)
            {
                SqlHelper.ExecuteNonQuerySP(connection, "st_ReferenceAuthorUpdate"
                                            , new SqlParameter("@REFERENCE_ID", CitationId)
                                            , new SqlParameter("@RANK", rank)
                                            , new SqlParameter("@ROLE", 1)
                                            , new SqlParameter("@LAST", a.FamilyName)
                                            , new SqlParameter("@FIRST", a.GivenName)
                                            );
                rank++;
            }
        }
        public static CitationRecord GetCitationRecord(SqlDataReader reader)
        {
            CitationRecord res = new CitationRecord();
            res.CitationId = (Int64)reader["REFERENCE_ID"];
            res.Type = reader["TYPE_NAME"].ToString();
            res.TypeID = (int)reader["TYPE_ID"];
            string tmp = reader["AUTHORS"].ToString();
            if (tmp != null && tmp.Length > 0)
            {
                string[] tmp1 = tmp.Split(";", StringSplitOptions.RemoveEmptyEntries);
                foreach (string author in tmp1)
                {
                    res.Authors.Add(res.BindPerson(author.Trim(), 0));
                }
            }
            tmp = reader["PARENT_AUTHORS"].ToString();
            if (tmp != null && tmp.Length > 0)
            {
                string[] tmp1 = tmp.Split(";", StringSplitOptions.RemoveEmptyEntries);
                foreach (string author in tmp1)
                {
                    res.Authors.Add(res.BindPerson(author.Trim(), 1));
                }
            }
            res.Title = reader["TITLE"].ToString();
            res.ParentTitle = reader["PARENT_TITLE"].ToString();
            res.ShortTitle = reader["SHORT_TITLE"].ToString();
            //returnValue.LoadProperty<SmartDate>(DateCreatedProperty, reader.GetSmartDate("DATE_CREATED"));
            //returnValue.LoadProperty<string>(CreatedByProperty, reader.GetString("CREATED_BY"));
            //returnValue.LoadProperty<SmartDate>(DateEditedProperty, reader.GetSmartDate("DATE_EDITED"));
            //returnValue.LoadProperty<string>(EditedByProperty, reader.GetString("EDITED_BY"));
            res.PublicationYear = reader["YEAR"].ToString();
            res.Month = reader["MONTH"].ToString();
            tmp = reader["EXTERNAL_IDS"].ToString();
            if (tmp != null && tmp.Length > 0)
            {
                string[] tmp1 = tmp.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (string extIDst in tmp1)
                {
                    string[] tmp2 = extIDst.Split('¬', StringSplitOptions.RemoveEmptyEntries);
                    if  (tmp2.Length == 2)
                    {
                        res.ExternalIDs.Add(new ExternalID(tmp2[0].Trim(), tmp2[1].Trim()));
                    }
                }
            }
            res.Issn = reader["STANDARD_NUMBER"].ToString();
            res.City = reader["CITY"].ToString();
            res.Country = reader["COUNTRY"].ToString();
            res.Publisher = reader["PUBLISHER"].ToString();
            //returnValue.LoadProperty<string>(InstitutionProperty, reader.GetString("INSTITUTION"));
            res.Volume = reader["VOLUME"].ToString();
            res.Pages = reader["PAGES"].ToString();
            res.Edition = reader["EDITION"].ToString();
            res.Issue = reader["ISSUE"].ToString();
            //returnValue.LoadProperty<bool>(IsLocalProperty, reader.GetBoolean("IS_LOCAL"));
            //returnValue.LoadProperty<string>(AvailabilityProperty, reader.GetString("AVAILABILITY"));
            tmp = reader["URLS"].ToString();
            if (tmp != null && tmp.Length > 0)
            {
                string[] tmp1 = tmp.Split('¬', StringSplitOptions.RemoveEmptyEntries);
                foreach (string url in tmp1)
                {
                    res.Urls.Add(url);
                }
            }
            //returnValue.LoadProperty<string>(CommentsProperty, reader.GetString("COMMENTS"));
            //returnValue.LoadProperty<string>(TypeNameProperty, reader.GetString("TYPE_NAME"));
            res.Abstract = reader["ABSTRACT"].ToString();
            tmp = reader["KEYWORDS"].ToString();
            if (tmp != null && tmp.Length > 0)
            {
                string[] tmp1 = tmp.Split('¬', StringSplitOptions.RemoveEmptyEntries);
                foreach (string kw in tmp1)
                {
                    res.Keywords.Add(new KeywordObject(kw));
                }
            }
            
            tmp = reader["MESHTERMS"].ToString();
            if (tmp != null && tmp.Length > 0)
            {
                string[] tmp1 = tmp.Split('¬', StringSplitOptions.RemoveEmptyEntries);
                foreach (string kw in tmp1)
                {
                    res.Keywords.Add(new KeywordObject(kw));
                }
            }
            return res;
        }
        public static List<DataTable> ToDataTables(List<CitationRecord> citations, Int64 Ref_seed, Int64 ExtID_seed, Int64 Auth_seed)
        {
            List<DataTable> Result = new List<DataTable>();
            if (citations == null || citations.Count == 0) return Result;
            DataTable TB_REFERENCE = new DataTable("TB_REFERENCE");
            DataTable TB_EXTERNALID = new DataTable("TB_EXTERNALID");
            DataTable TB_REFERENCE_AUTHOR = new DataTable("TB_REFERENCE_AUTHOR");
            Type int64type = System.Type.GetType("System.Int64");
            Type inttype = System.Type.GetType("System.Int16");
            Type stringtype = System.Type.GetType("System.String");
            Type datetimetype = System.Type.GetType("System.DateTime");

            DataColumn tCol = new DataColumn("REFERENCE_ID", int64type);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("TYPE_ID", typeof(int));
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("TITLE", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("PARENT_TITLE", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("SHORT_TITLE", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("DATE_CREATED", datetimetype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("CREATED_BY", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("DATE_EDITED", datetimetype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("EDITED_BY", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("PUBMED_REVISED", datetimetype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("PUBMED_PMID_VERSION", inttype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("YEAR", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("MONTH", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("STANDARD_NUMBER", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("CITY", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("COUNTRY", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("PUBLISHER", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("INSTITUTION", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("VOLUME", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("PAGES", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("EDITION", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("ISSUE", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("URLS", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("ABSTRACT", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("COMMENTS", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("MESHTERMS", stringtype);
            TB_REFERENCE.Columns.Add(tCol);
            tCol = new DataColumn("KEYWORDS", stringtype);
            TB_REFERENCE.Columns.Add(tCol);

            tCol = new DataColumn("EXTERNALID_ID", int64type);
            TB_EXTERNALID.Columns.Add(tCol);
            tCol = new DataColumn("REFERENCE_ID", int64type);
            TB_EXTERNALID.Columns.Add(tCol);
            tCol = new DataColumn("TYPE", stringtype);
            TB_EXTERNALID.Columns.Add(tCol);
            tCol = new DataColumn("VALUE", stringtype);
            TB_EXTERNALID.Columns.Add(tCol);

            tCol = new DataColumn("REFERENCE_AUTHOR_ID", int64type);
            TB_REFERENCE_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("REFERENCE_ID", int64type);
            TB_REFERENCE_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("LAST", stringtype);
            TB_REFERENCE_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("FIRST", stringtype);
            TB_REFERENCE_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("ROLE", inttype);
            TB_REFERENCE_AUTHOR.Columns.Add(tCol);
            tCol = new DataColumn("RANK", inttype);
            TB_REFERENCE_AUTHOR.Columns.Add(tCol);

            foreach (CitationRecord cit in citations)
            {
                Ref_seed++;
                DataRow row = TB_REFERENCE.NewRow();
                row["REFERENCE_ID"] = Ref_seed;
                row["TYPE_ID"] = cit.TypeID;
                row["TITLE"] = cit.Title;
                row["PARENT_TITLE"] = cit.ParentTitle;
                row["SHORT_TITLE"] = cit.ShortTitle;
                row["DATE_CREATED"] = DateTime.Now;
                row["CREATED_BY"] = "Pubmed Import Utility";
                row["DATE_EDITED"] = DateTime.Now;
                row["EDITED_BY"] = "Pubmed Import Utility";
                row["PUBMED_REVISED"] = cit.PubMedDate;
                row["PUBMED_PMID_VERSION"] = cit.PubmedPmidVersion;
                row["YEAR"] = cit.PublicationYear.Length > 4 ? cit.PublicationYear.Substring(0, 4) : cit.PublicationYear ;
                row["MONTH"] = cit.Month.Length > 10 ? cit.Month.Substring(0, 10) : cit.Month;
                row["STANDARD_NUMBER"] = cit.Issn;
                row["CITY"] = cit.City;
                row["COUNTRY"] = cit.Country;
                row["PUBLISHER"] = cit.Publisher;
                row["INSTITUTION"] = "";
                row["VOLUME"] = cit.Volume;
                row["PAGES"] = cit.Pages;
                row["EDITION"] = cit.Edition;
                row["ISSUE"] = cit.Issue;
                row["URLS"] = string.Join("¬", cit.Urls.ToArray());
                row["ABSTRACT"] = cit.Abstract;
                row["COMMENTS"] = "";
                row["MESHTERMS"] = "";
                row["KEYWORDS"] = cit.GetKeywordsString();
                TB_REFERENCE.Rows.Add(row);
                foreach (Author au in cit.Authors)
                {
                    Auth_seed++;
                    DataRow aRow = TB_REFERENCE_AUTHOR.NewRow();
                    aRow["REFERENCE_AUTHOR_ID"] = Auth_seed;
                    aRow["REFERENCE_ID"] = Ref_seed;
                    aRow["LAST"] = au.FamilyName;
                    aRow["FIRST"] = au.GivenName;
                    aRow["ROLE"] = au.AuthorshipLevel;
                    aRow["RANK"] = au.Rank;
                    TB_REFERENCE_AUTHOR.Rows.Add(aRow);
                }
                foreach (ExternalID exId in cit.ExternalIDs)
                {
                    ExtID_seed++;
                    DataRow exRow = TB_EXTERNALID.NewRow();
                    exRow["EXTERNALID_ID"] = ExtID_seed;
                    exRow["REFERENCE_ID"] = Ref_seed;
                    exRow["TYPE"] = exId.Name;
                    exRow["VALUE"] = exId.Value;
                    TB_EXTERNALID.Rows.Add(exRow);
                }
            }
            Result.Add(TB_REFERENCE);
            Result.Add(TB_EXTERNALID);
            Result.Add(TB_REFERENCE_AUTHOR);
            return Result;
        }
    }
}

public class ExternalID
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public ExternalID()	{ }
		public ExternalID(string name, string value)
		{
			Name = name;
			Value = value;
		}
		public override bool Equals(object obj)
		{//we use this to quickly find if a citation contains the same ID on app-server side (there is also an Index for this, to do it on Raven side)
		 //adapted from: http://codebetter.com/davidhayden/2005/02/22/object-identity-vs-object-equality-overriding-system-object-equalsobject-obj/
			if (obj == null) return false;
			if (Object.ReferenceEquals(this, obj)) return true;
			if (this.GetType() != obj.GetType()) return false;

			ExternalID incoming = (ExternalID)obj;
			if (
					incoming.Name == this.Name
					&&
					incoming.Value == this.Value
				) return true;//the two objects are the same IFF the name/value pairs are the same
			return false;
		}
	}

