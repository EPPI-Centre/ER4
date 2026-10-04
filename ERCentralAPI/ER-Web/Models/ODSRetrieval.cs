using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using System.ComponentModel;
using Csla.DataPortalClient;
using System.Threading;
using BusinessLibrary.BusinessClasses.ImportItems;
using static System.Net.WebRequestMethods;
using System.Buffers.Text;
using System.Web;
using AuthorsHandling;
using NuGet.Configuration;
using Azure;
using NuGet.Common;
using static Csla.Security.MembershipIdentity;
using System.Buffers;




#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
//using BusinessLibrary.eFetchPubmed;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ODSRetrieval : BusinessBase<ODSRetrieval>
    {
        public ODSRetrieval() { }

        public static void GetODSRetrieval(string UrlStr, EventHandler<DataPortalResult<ODSRetrieval>> handler)
        {
            DataPortal<ODSRetrieval> dp = new DataPortal<ODSRetrieval>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ODSRetrieval, string>(UrlStr));
        }
        public static readonly PropertyInfo<string> UrlStrProperty = RegisterProperty<string>(new PropertyInfo<string>("UrlStr", "UrlStr"));
        public string UrlStr
        {
            get
            {
                return GetProperty(UrlStrProperty);
            }
            set
            {
                SetProperty(UrlStrProperty, value);
            }
        }

        public static readonly PropertyInfo<int> QueMaxProperty = RegisterProperty<int>(new PropertyInfo<int>("QueMax", "QueMax"));
        public int QueMax
        {
            get
            {
                return GetProperty(QueMaxProperty);
            }
            set
            {
                SetProperty(QueMaxProperty, value);
            }
        }
        public static readonly PropertyInfo<int> showStartProperty = RegisterProperty<int>(new PropertyInfo<int>("showStart", "showStart"));
        public int showStart
        {
            get
            {
                return GetProperty(showStartProperty);
            }
            set
            {
                SetProperty(showStartProperty, value);
            }
        }
        public static readonly PropertyInfo<int> showEndProperty = RegisterProperty<int>(new PropertyInfo<int>("showEnd", "showEnd"));
        public int showEnd
        {
            get
            {
                return GetProperty(showEndProperty);
            }
            set
            {
                SetProperty(showEndProperty, value);
            }
        }
        public static readonly PropertyInfo<int> saveStartProperty = RegisterProperty<int>(new PropertyInfo<int>("saveStart", "saveStart"));
        public int saveStart
        {
            get
            {
                return GetProperty(saveStartProperty);
            }
            set
            {
                SetProperty(saveStartProperty, value);
            }
        }
        public static readonly PropertyInfo<int> saveEndProperty = RegisterProperty<int>(new PropertyInfo<int>("saveEnd", "saveEnd"));
        public int saveEnd
        {
            get
            {
                return GetProperty(saveEndProperty);
            }
            set
            {
                SetProperty(saveEndProperty, value);
            }
        }
        public static readonly PropertyInfo<int> pageStartProperty = RegisterProperty<int>(new PropertyInfo<int>("pageStart", "pageStart", 1));
        public int pageStart
        {
            get
            {
                return GetProperty(pageStartProperty);
            }
            set
            {
                SetProperty(pageStartProperty, value);
            }
        }
        public static readonly PropertyInfo<int> pageEndProperty = RegisterProperty<int>(new PropertyInfo<int>("pageEnd", "pageEnd", 1));
        public int pageEnd
        {
            get
            {
                return GetProperty(pageEndProperty);
            }
            set
            {
                SetProperty(pageEndProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SummaryProperty = RegisterProperty<string>(new PropertyInfo<string>("Summary", "Summary"));
        public string Summary
        {
            get
            {
                return GetProperty(SummaryProperty);
            }
            set
            {
                SetProperty(SummaryProperty, value);
            }
        }
        public static readonly PropertyInfo<MobileList<string>> SavedIndexesProperty = RegisterProperty<MobileList<string>>(new PropertyInfo<MobileList<string>>("SavedIndexes", "SavedIndexes"));
        public MobileList<string> SavedIndexes
        {
            get
            {
                return GetProperty(SavedIndexesProperty);
            }
            set
            {
                SetProperty(SavedIndexesProperty, value);
            }
        }
        public static readonly PropertyInfo<IncomingItemsList> ItemsListProperty = RegisterProperty<IncomingItemsList>(new PropertyInfo<IncomingItemsList>("ItemsList", "ItemsList"));
        public IncomingItemsList ItemsList
        {
            get { return GetProperty(ItemsListProperty); }
            set { SetProperty(ItemsListProperty, value); }
        }
        
#if !SILVERLIGHT
        protected void DataPortal_Fetch(SingleCriteria<ODSRetrieval, string> criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            if (!ri.IsAuthenticated) return;

            if (ItemsList == null)
            {
                ItemsList = new IncomingItemsList();
            }
            UrlStr = criteria.Value;
            showStart = 1;
            showEnd = showStart + 19;
            

            FetchOrSaveResults();
        }

        private void FetchOrSaveResults()
        {   
            bool toSave = false;
            if (saveEnd != 0 && saveStart <= saveEnd && showEnd == 0 && showStart == 0)
            {
                toSave = true;
            }
            const int pageSize = 20;
            pageStart = (Math.Max(Math.Max(showStart, saveStart), 1) - 1) / pageSize + 1;
            pageEnd = (Math.Max(Math.Max(showEnd, saveEnd), 1) - 1) / pageSize + 1;


            if (ItemsList.IncomingItems == null)
            {
                //ItemsList = IncomingItemsList.NewIncomingItemsList();
                ItemsList.IncomingItems = new MobileList<ItemIncomingData>();
            }
            else
            {
                ItemsList.IncomingItems.Clear();
            }
            ItemsList.SearchStr = UrlStr;
            ItemsList.SourceDB = "Evidence Repository";
            ItemsList.DateOfSearch = DateTime.Now;
            if (!toSave)
            {
                ItemsList.SourceName = "Evidence Repository retrieval on " + DateTime.Now.ToShortDateString();
            }
            FillIncomingItems();
            if (toSave && ItemsList != null && ItemsList.IncomingItems.Count > 0)
            {
                ItemsList.Saved += new EventHandler<SavedEventArgs>(ItemsList_Saved);
                IncomingItemsList throwaway = ItemsList.Save();
            }
            else
            {
                NormalSummary();
            }
        }

        private void FillIncomingItems()
        {
            string apiURL = BuildApiUrl(UrlStr);
            string token = getODSToken().GetAwaiter().GetResult();

            for (int currentPage = pageStart; currentPage <= pageEnd; currentPage++) {

                // Token doesn't last long, so need to refresh it
                if (currentPage % 100 == 0)
                {
                    token = getODSToken().GetAwaiter().GetResult();
                }
                string body = FetchODSBodyAsync(apiURL, token, currentPage).GetAwaiter().GetResult();
                if (body != "")
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new StringOrNumberConverter());
                    options.Converters.Add(new LenientBoolConverter());
                    var result = System.Text.Json.JsonSerializer.Deserialize<ODSResponse>(body, options);

                    QueMax = result.total.count;

                    if (QueMax == 0)
                    {
                        Summary = "That query in the Evidence Repository for \""
                                + (UrlStr.Length > 200 ? "[...long query...]" : UrlStr)
                                + "\" returned no Items.\r\nPlease check the URL.";
                    }
                    else
                    {
                        FetchResults(result);
                    }
                }
            }
        }
        private async Task<string> FetchODSBodyAsync(string apiURL, string token, int currentPage)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await http.GetAsync(apiURL + "&page=" + currentPage.ToString());
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                //System.InvalidOperationException: 'Evidence Repository API call failed: 401 {"detail":"Token is expired."}' - refreshing every 100 pages hopefully fixes this
                // but we could handle explicitly
                throw new InvalidOperationException($"Evidence Repository API call failed: {(int)response.StatusCode} {body}");
            }
            return body;
        }

        private async Task<string> getODSToken()
        {
            string TokenEndpoint = AzureSettings.ODSTokenEndpoint;
            string ClientId = AzureSettings.ODSClientId;
            string ClientSecret = AzureSettings.ODSClientSecret;
            string Scope = "destiny-repository-client-production";
            string BaseUrl = "";

            using var http = new HttpClient();

            var response = await http.PostAsync(TokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                //["scope"] = Scope,
            }));
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Token request failed: {(int)response.StatusCode} {body}");

            return System.Text.Json.JsonDocument.Parse(body).RootElement.GetProperty("access_token").GetString()!;
        }

        private static readonly Dictionary<string, string> AnnotationOverrides = new(StringComparer.OrdinalIgnoreCase)
        {
            ["esea"] = "jacobs-education",
            ["destiny"] = "destiny-high-recall"
        };
        private static string BuildApiUrl(string pageUrl)
        {
            var uri = new Uri(pageUrl);
            var query = HttpUtility.ParseQueryString(uri.Query);

            string database = uri.AbsolutePath.Trim('/').Split('/')[0];
            string annotation = "domain-inclusion/" +
                (AnnotationOverrides.TryGetValue(database, out var mapped) ? mapped : database);

            string? q = query["q"];
            bool hasQ = !string.IsNullOrWhiteSpace(q);

            var parts = new List<string>
            {
                "q=" + (hasQ ? Uri.EscapeDataString(q!) : "*")
            };

            // Pass through any other page parameters (e.g. end_year), keeping their order
            foreach (string? key in query.AllKeys)
            {
                if (key is null || key is "q" or "concept" or "page") continue;
                foreach (string value in query.GetValues(key) ?? [])
                    parts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
            }

            parts.Add("annotation=" + Uri.EscapeDataString(annotation));

            foreach (string concept in query.GetValues("concept") ?? [])
                parts.Add("concept=" + Uri.EscapeDataString(concept));

            parts.Add("page=1");

            if (!hasQ)
                parts.Add("sort=-publication_year");

            if (pageUrl.IndexOf("staging.evidence-repository.org") > 1)
            {
                return "https://api.staging.evidence-repository.org/v1/references/search/?" + string.Join("&", parts);
            }
            return "https://api.evidence-repository.org/v1/references/search/?" + string.Join("&", parts);
        }

        private System.Collections.Specialized.NameValueCollection nvcoll = new System.Collections.Specialized.NameValueCollection();

        private void FetchResults(ODSResponse? result)
        {
            bool toSave = false;

            foreach(Reference reference in result.references)
            {
                Enhancement biblioEnhancement = getEnhancement("bibliographic", reference);
                if (biblioEnhancement is null || biblioEnhancement.content is null)
                {
                    continue; // if there's no bibliographic information, we can't create a bibliographic record!
                }
                Enhancement abstractEnhancement = getEnhancement("abstract", reference);
                ItemIncomingData tItem = ItemIncomingData.NewItem();
                tItem.pAuthorsLi = new MobileList<AutH>();

                tItem.OldItemId = reference.id;
                tItem.Title = biblioEnhancement.content.title;
                tItem.Abstract = abstractEnhancement != null && abstractEnhancement.content != null ? abstractEnhancement.content._abstract : "";
                tItem.Year = biblioEnhancement.content.publication_year != null ? biblioEnhancement.content.publication_year.ToString() : "";
                tItem.Publisher = biblioEnhancement.content.publisher != null ? biblioEnhancement.content.publisher : "";
                tItem.Parent_title = biblioEnhancement.content.publication_venue != null ? biblioEnhancement.content.publication_venue.display_name : "";
                tItem.Pages = biblioEnhancement.content.pagination != null ? biblioEnhancement.content.pagination.first_page + "-" + biblioEnhancement.content.pagination.last_page : "";
                tItem.Volume = biblioEnhancement.content.pagination != null ? biblioEnhancement.content.pagination.volume + "-" + biblioEnhancement.content.pagination.volume : "";
                tItem.Volume = biblioEnhancement.content.pagination != null ? biblioEnhancement.content.pagination.issue + "-" + biblioEnhancement.content.pagination.issue: "";
                tItem.AuthorsLi = getAuthors(biblioEnhancement);
                tItem.Url = getURL(biblioEnhancement);
                tItem.DOI = getDoi(reference);
                this.ItemsList.IncomingItems.Add(tItem);
            }
        }

        public class Identifier
        {
            public object identifier { get; set; }
            public string identifier_type { get; set; }
            public string other_identifier_name { get; set; }
        }
        private AutorsList getAuthors(Enhancement biblioEnhancement)
        {
            AutorsList authors = new AutorsList();
            if (biblioEnhancement.content.authorship != null)
            {
                for (int x = 0; x < biblioEnhancement.content.authorship.Length; x++)
                {
                    AutH author = NormaliseAuth.singleAuth(biblioEnhancement.content.authorship[x].display_name, x + 1, 0);
                    if (author != null)
                    {
                        authors.Add(author);
                    }
                }
            }
            return authors;
        }
                

        private string getURL(Enhancement biblioEnhancement)
        {
            string ret = "";
            if (biblioEnhancement.content.locations != null)
            {
                foreach (Location loc in biblioEnhancement.content.locations)
                {
                    if (loc.version == "publishedVersion")
                    {
                        ret = loc.landing_page_url;
                    }
                }
            }
            return ret;
        }
        private string getDoi(Reference reference)
        {
            string ret = "";
            if (reference.identifiers != null)
            {
                foreach (ODSIdentifier i in reference.identifiers)
                {
                    if (i.identifier_type == "doi")
                    {
                        ret = i.identifier;
                    }
                }
            }
            return ret;
        }
        private void NormalSummary()
        {
            Summary = "Search in the Evidence Repository for \"" + ItemsList.SearchStr + "\" returned " + QueMax + " Items.\r\n";
            Summary += "Displaying Items from N°" + (showStart) + " to N°" + (showEnd) + ".\r\n";
        }

        private Enhancement? getEnhancement(string enhancementName, Reference reference)
        {
            string res = "";
            foreach (Enhancement enhancement in reference.enhancements)
            {
                if (enhancement.content.enhancement_type == enhancementName)
                {
                    return enhancement;
                }
            }
            return null;
        }
        
        protected void OldDataPortal_Fetch(SingleCriteria<ODSRetrieval, string> criteria)
        {

        }
        protected void DP_F_GetNew(SingleCriteria<ODSRetrieval, string> criteria)
        {


        }
        protected override void DataPortal_Insert()
        {
            FetchOrSaveResults();
        }

        void ItemsList_Saved(object sender, SavedEventArgs e)
        {
            ItemsList = IncomingItemsList.NewIncomingItemsList();
            if ((e.NewObject as IncomingItemsList).SearchStr == "")
            {
                Summary = "Nothing was saved: items requested could not be fetched from PubMed";
            }
            else
            {
                Summary = "Source Saved";
            }
        }
        protected override void DataPortal_Update()
        {
            //for (int i = currStart; i < currEnd - currStart; i++)
            //{
            //    SavedIndexes.Add(i);
            //}
            //ItemsList.Saved += new EventHandler<SavedEventArgs>(ItemsList_Saved);
            //ItemsList.Save();
            FetchOrSaveResults();
        }
#endif
    }

}

public class StringOrNumberConverter : System.Text.Json.Serialization.JsonConverter<string>
{
    public override string Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case System.Text.Json.JsonTokenType.String:
                return reader.GetString();
            case System.Text.Json.JsonTokenType.Number:
                // Preserves the original text (no rounding or exponent changes)
                return System.Text.Encoding.UTF8.GetString(reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan);
            case System.Text.Json.JsonTokenType.True:
                return "true";
            case System.Text.Json.JsonTokenType.False:
                return "false";
            case System.Text.Json.JsonTokenType.Null:
                return null;
            default:
                throw new System.Text.Json.JsonException($"Unexpected token {reader.TokenType} when reading a string or number.");
        }
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, string value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
public class LenientBoolConverter : System.Text.Json.Serialization.JsonConverter<bool>
{
    public override bool Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case System.Text.Json.JsonTokenType.True:
                return true;
            case System.Text.Json.JsonTokenType.False:
                return false;
            case System.Text.Json.JsonTokenType.Null:
                return false; // non-nullable bool: treat null as false
            case System.Text.Json.JsonTokenType.Number:
                return reader.TryGetInt64(out long l) ? l != 0 : reader.GetDouble() != 0;
            case System.Text.Json.JsonTokenType.String:
                string s = reader.GetString();
                if (bool.TryParse(s, out bool b)) return b;
                if (long.TryParse(s, out long n)) return n != 0;
                if (string.IsNullOrWhiteSpace(s)) return false;
                throw new System.Text.Json.JsonException($"Cannot convert \"{s}\" to a Boolean.");
            default:
                throw new System.Text.Json.JsonException($"Unexpected token {reader.TokenType} when reading a Boolean.");
        }
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, bool value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}



public class ODSResponse
{
    public Total total { get; set; }
    public Page page { get; set; }
    public Reference[] references { get; set; }
}

public class Total
{
    public int count { get; set; }
    public bool is_lower_bound { get; set; }
}

public class Page
{
    public int count { get; set; }
    public int number { get; set; }
    public int max_result_window { get; set; }
}

public class Reference
{
    public string visibility { get; set; }
    public string id { get; set; }
    public ODSIdentifier[] identifiers { get; set; }
    public Enhancement[] enhancements { get; set; }
}

public class ODSIdentifier
{
    public string identifier { get; set; }
    public string identifier_type { get; set; }
    public string other_identifier_name { get; set; }
}

public class Enhancement
{
    public string id { get; set; }
    public string reference_id { get; set; }
    public string source { get; set; }
    public string visibility { get; set; }
    public string robot_version { get; set; }
    public string[] derived_from { get; set; }
    public Content content { get; set; }
    public DateTime created_at { get; set; }
}

public class Content
{
    public string enhancement_type { get; set; }
    public Annotation[] annotations { get; set; }
    public Authorship[] authorship { get; set; }
    public int? cited_by_count { get; set; }
    public string created_date { get; set; }
    public string updated_date { get; set; }
    public string publication_date { get; set; }
    public int? publication_year { get; set; }
    public string publisher { get; set; }
    public string title { get; set; }
    public Pagination pagination { get; set; }
    public Publication_Venue publication_venue { get; set; }
    public string process { get; set; }
    public string _abstract { get; set; }
    public Location[] locations { get; set; }
    public Associated_Reference_Ids[] associated_reference_ids { get; set; }
    public string association_type { get; set; }
    public string vocabulary_uri { get; set; }
    public Data data { get; set; }
}

public class Pagination
{
    public string volume { get; set; }
    public string issue { get; set; }
    public string first_page { get; set; }
    public string last_page { get; set; }
}

public class Publication_Venue
{
    public string display_name { get; set; }
    public string venue_type { get; set; }
    public string[] issn { get; set; }
    public string issn_l { get; set; }
    public string host_organization_name { get; set; }
}

public class Data
{
    public string type { get; set; }
    public string context { get; set; }
    public Hasinvestigation hasInvestigation { get; set; }
}

public class Hasinvestigation
{
    public string type { get; set; }
    public string[] hasAppliedConcept { get; set; }
}

public class Annotation
{
    public string scheme { get; set; }
    public string label { get; set; }
    public string annotation_type { get; set; }
    public bool value { get; set; }
    public float? score { get; set; }
    public Data1 data { get; set; }
}

public class Data1
{
    public string id { get; set; }
    public Field field { get; set; }
    public float score { get; set; }
    public Domain domain { get; set; }
    public Subfield subfield { get; set; }
    public string display_name { get; set; }
    public Vote[] votes { get; set; }
    public float exclusion_score { get; set; }
    public float inclusion_score { get; set; }
    public float score_threshold { get; set; }
}

public class Field
{
    public string id { get; set; }
    public string display_name { get; set; }
}

public class Domain
{
    public string id { get; set; }
    public string display_name { get; set; }
}

public class Subfield
{
    public string id { get; set; }
    public string display_name { get; set; }
}

public class Vote
{
    public bool value { get; set; }
    public string reasoning { get; set; }
}

public class Authorship
{
    public string display_name { get; set; }
    public string orcid { get; set; }
    public string position { get; set; }
}

public class Location
{
    public bool is_oa { get; set; }
    public string version { get; set; }
    public string landing_page_url { get; set; }
    public string pdf_url { get; set; }
    public string license { get; set; }
    public Extra extra { get; set; }
}

public class Extra
{
    public string id { get; set; }
    public string[] issn { get; set; }
    public string type { get; set; }
    public bool is_oa { get; set; }
    public string issn_l { get; set; }
    public bool is_core { get; set; }
    public bool is_in_doaj { get; set; }
    public string display_name { get; set; }
    public string host_organization { get; set; }
    public string host_organization_name { get; set; }
    public string[] host_organization_lineage { get; set; }
    public string[] host_organization_lineage_names { get; set; }
}

public class Associated_Reference_Ids
{
    public string identifier { get; set; }
    public string identifier_type { get; set; }
}
