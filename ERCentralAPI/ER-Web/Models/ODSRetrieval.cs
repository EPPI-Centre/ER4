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
using static BusinessLibrary.BusinessClasses.ImportJsonCommand;
using static BusinessLibrary.BusinessClasses.MagMakesHelpers;






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
        public static readonly PropertyInfo<ReviewSetsList> ReviewSetsProperty = RegisterProperty<ReviewSetsList>(new PropertyInfo<ReviewSetsList>("ReviewSets", "ReviewSets"));
        public ReviewSetsList ReviewSets
        {
            get { return ReadProperty(ReviewSetsProperty); }
            set { LoadProperty(ReviewSetsProperty, value); }
        }

        public List<Enhancement> LinkedDataEnhancements { get; set; } = new List<Enhancement>();
        public List<Enhancement> LinkedDataOutcomeEnhancements { get; set; } = new List<Enhancement>();


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
                IncomingItemsList newItems = ItemsList.Save();

                // check if the records have any linked data enhancements
                if (LinkedDataEnhancements.Count > 0)
                {
                    // First, we need to check if we have the right codesets
                    if (CheckCodeSets)
                    {
                        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                        var savedAttributes = new HashSet<(string ReferenceId, string AttributeUri, string AdditionalText)>();
                        var jsonLdPrefixesByContext = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
                        string vocabularyUri = LinkedDataEnhancements[0].content.vocabulary_uri; // this isn't great, but we don't have the notion of an individual data extraction, so the linked data doesn't tell us where the outcomes should go
                        foreach (Enhancement e in LinkedDataEnhancements)
                        {
                            if (e.content == null ||
                                e.content.data.ValueKind != System.Text.Json.JsonValueKind.Object)
                            {
                                continue;
                            }

                            Int64 itemId = Item.ItemIdFromOldItemId(e.reference_id);
                            if (itemId <= 0)
                            {
                                continue;
                            }

                            string contextKey = GetJsonLdContextKey(e.content.data);
                            if (!jsonLdPrefixesByContext.TryGetValue(contextKey, out var jsonLdPrefixes))
                            {
                                jsonLdPrefixes = GetJsonLdPrefixes(e.content.data);
                                jsonLdPrefixesByContext.Add(contextKey, jsonLdPrefixes);
                            }

                            if (TryGetPropertyIgnoreCase(e.content.data, "hasInvestigation", out var investigationElement) &&
                                TryGetPropertyIgnoreCase(investigationElement, "hasAppliedConcept", out var appliedConceptsElement) &&
                                appliedConceptsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                foreach (var conceptElement in appliedConceptsElement.EnumerateArray())
                                {
                                    if (conceptElement.ValueKind == System.Text.Json.JsonValueKind.String &&
                                        conceptElement.GetString() is string legacyId)
                                    {
                                        string? attributeUri = ExpandJsonLdIdentifier(legacyId, jsonLdPrefixes);
                                        if (!string.IsNullOrWhiteSpace(attributeUri) &&
                                            savedAttributes.Add((e.reference_id, attributeUri, string.Empty)))
                                        {
                                            SaveAttributeCode(e.content.vocabulary_uri, attributeUri, ri.ReviewId, ri.UserId, itemId, string.Empty);
                                        }
                                    }
                                }
                            }

                            // New linked-data payloads store vocabulary codes in nested coding annotations.
                            foreach (var annotation in EnumerateCodingAnnotations(e.content.data))
                            {
                                string? attributeUri = ExpandJsonLdIdentifier(annotation.Identifier, jsonLdPrefixes);
                                if (!string.IsNullOrWhiteSpace(attributeUri) &&
                                    savedAttributes.Add((e.reference_id, attributeUri, annotation.AdditionalText)))
                                {
                                    SaveAttributeCode(e.content.vocabulary_uri, attributeUri, ri.ReviewId, ri.UserId, itemId, annotation.AdditionalText);
                                }
                            }
                        }
                        foreach (Enhancement e in LinkedDataOutcomeEnhancements)
                        {
                            if (e.content != null &&
                                e.content.data.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                Int64 itemId = 0;
                                itemId = Item.ItemIdFromOldItemId(e.reference_id);
                                if (itemId > 0)
                                {
                                    if (TryGetPropertyIgnoreCase(e.content.data, "Outcomes", out var outcomesElement) &&
                                        outcomesElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                                    {
                                        var outcomes = System.Text.Json.JsonSerializer.Deserialize<ImportJsonCommand.Outcome[]>(
                                            outcomesElement,
                                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                            ?? Array.Empty<ImportJsonCommand.Outcome>();

                                        foreach (var outcome in outcomes)
                                        {
                                            SaveOutcome(outcome, ri.ReviewId,
                                                ri.UserId,
                                                itemId,
                                                vocabularyUri);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // log here that we failed to find the codesets for some reason?
                        return;
                    }
                    
                }
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

            //parts.Add("page=1");

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
                tItem.TypeId = biblioEnhancement.content.publication_venue != null ? ItemTypeIdFromOpenAlexPublicationType(biblioEnhancement.content.publication_venue.venue_type) : 1;
                tItem.Publisher = biblioEnhancement.content.publisher != null ? biblioEnhancement.content.publisher : "";
                tItem.Parent_title = biblioEnhancement.content.publication_venue != null ? biblioEnhancement.content.publication_venue.display_name : "";
                tItem.Pages = biblioEnhancement.content.pagination != null ? biblioEnhancement.content.pagination.first_page + "-" + biblioEnhancement.content.pagination.last_page : "";
                tItem.Volume = biblioEnhancement.content.pagination != null ? biblioEnhancement.content.pagination.volume + "-" + biblioEnhancement.content.pagination.volume : "";
                tItem.Issue = biblioEnhancement.content.pagination != null ? biblioEnhancement.content.pagination.issue + "-" + biblioEnhancement.content.pagination.issue: "";
                tItem.AuthorsLi = getAuthors(biblioEnhancement);
                tItem.Url = getURL(biblioEnhancement);
                tItem.DOI = getDoi(reference);
                tItem.Short_title = Item.SetShortTitle(getFirstAuthor(biblioEnhancement), tItem.Year);
                tItem.SearchText = Item.ToShortSearchText(biblioEnhancement.content.title);
                this.ItemsList.IncomingItems.Add(tItem);
                AddEnhancements(reference);
            }
        }
        private void AddEnhancements(Reference reference)
        {
            if (reference.enhancements != null)
            {
                foreach (Enhancement enhancement in reference.enhancements)
                {
                    if (enhancement.content != null && enhancement.content.enhancement_type == "linked_data" &&
                        enhancement.content.data.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        TryGetPropertyIgnoreCase(enhancement.content.data, "hasInvestigation", out _) &&
                        enhancement.content.vocabulary_uri != null)
                    {
                        LinkedDataEnhancements.Add(enhancement);
                    }
                    else
                        if (enhancement.content != null && enhancement.content.enhancement_type == "raw" &&
                            enhancement.content.data.ValueKind == System.Text.Json.JsonValueKind.Object &&
                            TryGetPropertyIgnoreCase(enhancement.content.data, "outcomes", out _))
                        {
                            LinkedDataOutcomeEnhancements.Add(enhancement);
                        }
                }
            }
        }
        private bool CheckCodeSets
        {
            get
            {
                // check if we have the right codesets for the linked data enhancements
                // if not, we need to create them first

                foreach (Enhancement e in LinkedDataEnhancements)
                {
                    // get the right codeset. We have to this per Enhancement as there might be multiple code sets to deal with
                    bool foundIt = false;
                    if (ReviewSets != null)
                    {
                        foreach (ReviewSet rs in ReviewSets)
                        {

                            if (rs.ExtURL == e.content.vocabulary_uri)
                            {
                                foundIt = true;
                            }
                        }
                    }
                    if (foundIt == false)
                    {
                        // we need to create /load up a new codeset for this linked data enhancement
                        ReviewSet newRS = GetOrCreateReviewSet(e);
                        if (newRS != null)
                        {
                            if (ReviewSets == null)
                            {
                                ReviewSets = new ReviewSetsList();
                            }
                            ReviewSets.Add(newRS);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        // GPT-6-Luna wrote the following functions *****************************************************
        private static bool TryGetPropertyIgnoreCase(
            System.Text.Json.JsonElement element,
            string name,
            out System.Text.Json.JsonElement value)
        {
            if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }

        private static string GetJsonLdContextKey(System.Text.Json.JsonElement data)
        {
            if (!TryGetPropertyIgnoreCase(data, "@context", out var context))
            {
                return string.Empty;
            }

            return context.ValueKind == System.Text.Json.JsonValueKind.String
                ? context.GetString() ?? string.Empty
                : context.GetRawText();
        }

        private Dictionary<string, string> GetJsonLdPrefixes(System.Text.Json.JsonElement data)
        {
            var prefixes = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!TryGetPropertyIgnoreCase(data, "@context", out var context))
            {
                return prefixes;
            }

            if (context.ValueKind == System.Text.Json.JsonValueKind.String &&
                context.GetString() is string contextUri &&
                !string.IsNullOrWhiteSpace(contextUri))
            {
                string contextJson = FetchVocabulary(contextUri).GetAwaiter().GetResult();
                using var contextDocument = System.Text.Json.JsonDocument.Parse(contextJson);
                if (TryGetPropertyIgnoreCase(contextDocument.RootElement, "@context", out var remoteContext))
                {
                    AddJsonLdPrefixes(remoteContext, prefixes);
                }
                else
                {
                    AddJsonLdPrefixes(contextDocument.RootElement, prefixes);
                }
            }
            else
            {
                AddJsonLdPrefixes(context, prefixes);
            }

            return prefixes;
        }

        private static void AddJsonLdPrefixes(
            System.Text.Json.JsonElement context,
            Dictionary<string, string> prefixes)
        {
            if (context.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var contextPart in context.EnumerateArray())
                {
                    AddJsonLdPrefixes(contextPart, prefixes);
                }
                return;
            }

            if (context.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return;
            }

            if (TryGetPropertyIgnoreCase(context, "@context", out var nestedContext))
            {
                AddJsonLdPrefixes(nestedContext, prefixes);
            }

            foreach (var property in context.EnumerateObject())
            {
                if (property.Name.StartsWith("@", StringComparison.Ordinal))
                {
                    continue;
                }

                string? iri = null;
                if (property.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    iri = property.Value.GetString();
                }
                else if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Object &&
                         TryGetPropertyIgnoreCase(property.Value, "@id", out var idElement) &&
                         idElement.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    iri = idElement.GetString();
                }

                if (!string.IsNullOrWhiteSpace(iri))
                {
                    prefixes[property.Name] = iri;
                }
            }
        }

        private static string? ExpandJsonLdIdentifier(
            string identifier,
            IReadOnlyDictionary<string, string> prefixes)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            int colonIndex = identifier.IndexOf(':');
            if (colonIndex > 0 && prefixes.TryGetValue(identifier[..colonIndex], out var prefixIri))
            {
                return prefixIri + identifier[(colonIndex + 1)..];
            }

            if (System.Uri.TryCreate(identifier, UriKind.Absolute, out var absoluteUri) &&
                (absoluteUri.Scheme == Uri.UriSchemeHttp ||
                 absoluteUri.Scheme == Uri.UriSchemeHttps ||
                 absoluteUri.Scheme == "urn"))
            {
                return absoluteUri.AbsoluteUri;
            }

            return null;
        }

        private static IEnumerable<(string Identifier, string AdditionalText)> EnumerateCodingAnnotations(
            System.Text.Json.JsonElement element)
        {
            if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (TryGetPropertyIgnoreCase(element, "codedValue", out var codedValue) &&
                    codedValue.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    TryGetPropertyIgnoreCase(codedValue, "@id", out var idElement) &&
                    idElement.ValueKind == System.Text.Json.JsonValueKind.String &&
                    idElement.GetString() is string identifier)
                {
                    string additionalText =
                        TryGetPropertyIgnoreCase(element, "supportingText", out var supportingText) &&
                        supportingText.ValueKind == System.Text.Json.JsonValueKind.String
                            ? supportingText.GetString() ?? string.Empty
                            : string.Empty;

                    yield return (identifier, additionalText);
                }

                foreach (var property in element.EnumerateObject())
                {
                    foreach (var annotation in EnumerateCodingAnnotations(property.Value))
                    {
                        yield return annotation;
                    }
                }
            }
            else if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    foreach (var annotation in EnumerateCodingAnnotations(item))
                    {
                        yield return annotation;
                    }
                }
            }
        }
        // end luna created stuff
        private ReviewSet GetOrCreateReviewSet(Enhancement e)
        {
            ReviewSet rs = ReviewSet.GetReviewSetFromExtURLAndReviewId(e.content.vocabulary_uri);
            if (rs == null)
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                rs = ReviewSet.NewReviewSet();
                rs.SetName = "Vocabulary from evidence repository";
                rs.SetDescription = "This vocabulary is sourced from the evidence repository";
                rs.UserCanEditURLs = false;
                rs.CodingIsFinal = true;
                rs.SetTypeId = 3;
                rs.ReviewId = ri.ReviewId;
                rs.SetOrder = -1; // i.e. the stored procedure will set the order to the next available number
                rs.ExtURL = e.content.vocabulary_uri;
                rs.ExtType = "Vocabulary from evidence repository";
                rs = rs.Save();
                AddAttributesToNewCodeSet(e.content.vocabulary_uri, rs);
            }
            return rs;
        }

        private bool AddAttributesToNewCodeSet(string uri, ReviewSet rs)
        {
            string vocab = FetchVocabulary(uri).GetAwaiter().GetResult();
            var roots = SkosTreeBuilder.Build(vocab);
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            SaveAttributes(roots, null, rs, ri);
            ReviewSet.ReviewSetFromDBCommonPart(rs);
            return true;
        }
        private void SaveAttributes(IEnumerable<VocabularyNode> nodes, AttributeSet? parent, ReviewSet rs, ReviewerIdentity ri)
        {
            int order = 1;
            
            foreach (var node in nodes)
            {
                AttributeSet a = new AttributeSet
                {
                    SetId = rs.SetId,
                    ParentAttributeId = parent != null ? parent.AttributeId : 0,
                    AttributeTypeId = node.IsConceptScheme ? 1 : 2,
                    AttributeSetDescription = node.Definition != null ? node.Definition : "",
                    AttributeOrder = order,
                    AttributeName = node.Label,
                    AttributeDescription = node.Definition != null ? node.Definition : "",
                    ExtURL = node.Uri,
                    ExtType = node.IsConceptScheme ? "Concept Scheme" : "Concept",
                    IsExclusive = false, // I have no idea what an exclusive attribute is!
                    ContactId = ri.UserId,
                    OriginalAttributeID = 0,
                    OldestKnownId = 0
                };
                
                // Save item and obtain its new object.
                a = a.Save();
                if (parent == null)
                {
                    if (rs.Attributes == null)
                    {
                        rs.Attributes = new AttributeSetList();
                    }
                    rs.Attributes.Add(a);
                }
                else
                {
                    if (parent.Attributes == null)
                    {
                        parent.Attributes = new AttributeSetList();
                    }
                    parent.Attributes.Add(a);
                }
                order++;

                // Every child is a direct child of this node.
                SaveAttributes(node.Children, a, rs, ri);
            }
        }
        // based on SaveOutcome from importjsoncommand.cs, but we don't have the notion of an individual data extraction, so the linked data doesn't tell us where the outcomes should go
        private void SaveOutcome(ImportJsonCommand.Outcome o, int ReviewId, int ContactId, Int64 ItemId, string VocabularyUri)
        {
            //commented out: we try something a bit better below
            //// If there is > 1 codeset in the json, we don't know where to 'tie' the outcomes, so don't try to import them
            //if (ReviewSets.Count != 1)
            //{
            //    return;
            //}

            ReviewSet rs = this.ReviewSets.GetReviewSetFromExtURL(VocabularyUri);
            if (rs == null) return;
            Int64 NewOutcomeId = 0;
            Int64 ItemSetId = 0;
            if (rs != null)
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command3 = new SqlCommand("st_ItemSetGetCompleted", connection))
                    {
                        command3.CommandType = System.Data.CommandType.StoredProcedure;
                        command3.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command3.Parameters.Add(new SqlParameter("@ITEM_ID", ItemId));
                        command3.Parameters.Add(new SqlParameter("@SET_ID", rs.SetId));
                        using (Csla.Data.SafeDataReader reader3 = new Csla.Data.SafeDataReader(command3.ExecuteReader()))
                        {
                            if (reader3.Read())
                                ItemSetId = Convert.ToInt64(reader3["ITEM_SET_ID"].ToString());
                        }
                    }

                    if (ItemSetId > 0)
                    {
                        AttributeSet InterventionId = rs.GetAttributeSetFromOriginalAttributeId(o.ItemAttributeIdIntervention);
                        AttributeSet ControlId = rs.GetAttributeSetFromOriginalAttributeId(o.ItemAttributeIdControl);
                        AttributeSet OutcomeId = rs.GetAttributeSetFromOriginalAttributeId(o.ItemAttributeIdOutcome);
                        int grp1 = 0;
                        int grp2 = 0;
                        //if (grp1 != 0 && this.ArmsDictionary.TryGetValue((ItemId, grp1), out int mapped1)) grp1 = mapped1;
                        //if (grp2 != 0 && this.ArmsDictionary.TryGetValue((ItemId, grp2), out int mapped2)) grp2 = mapped2;
                        using (SqlCommand command = new SqlCommand("st_OutcomeItemInsert", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", ItemSetId));
                            command.Parameters.Add(new SqlParameter("@OUTCOME_TYPE_ID", o.OutcomeTypeId));
                            command.Parameters.Add(new SqlParameter("@OUTCOME_TITLE", o.Title));
                            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_INTERVENTION", InterventionId == null ? 0 : InterventionId.AttributeId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_CONTROL", ControlId == null ? 0 : ControlId.AttributeId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_OUTCOME", OutcomeId == null ? 0 : OutcomeId.AttributeId));
                            command.Parameters.Add(new SqlParameter("@OUTCOME_DESCRIPTION", o.OutcomeDescription));
                            command.Parameters.Add(new SqlParameter("@DATA1", o.Data1));
                            command.Parameters.Add(new SqlParameter("@DATA2", o.Data2));
                            command.Parameters.Add(new SqlParameter("@DATA3", o.Data3));
                            command.Parameters.Add(new SqlParameter("@DATA4", o.Data4));
                            command.Parameters.Add(new SqlParameter("@DATA5", o.Data5));
                            command.Parameters.Add(new SqlParameter("@DATA6", o.Data6));
                            command.Parameters.Add(new SqlParameter("@DATA7", o.Data7));
                            command.Parameters.Add(new SqlParameter("@DATA8", o.Data8));
                            command.Parameters.Add(new SqlParameter("@DATA9", o.Data9));
                            command.Parameters.Add(new SqlParameter("@DATA10", o.Data10));
                            command.Parameters.Add(new SqlParameter("@DATA11", o.Data11));
                            command.Parameters.Add(new SqlParameter("@DATA12", o.Data12));
                            command.Parameters.Add(new SqlParameter("@DATA13", o.Data13));
                            command.Parameters.Add(new SqlParameter("@DATA14", o.Data14));
                            command.Parameters.Add(new SqlParameter("@ITEM_TIMEPOINT_ID", o.ItemTimepointId));
                            command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID_GRP1", grp1));
                            command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID_GRP2", grp2));
                            command.Parameters.Add(new SqlParameter("@NEW_OUTCOME_ID", 0));
                            command.Parameters["@NEW_OUTCOME_ID"].Direction = System.Data.ParameterDirection.Output;
                            command.ExecuteNonQuery();
                            if (command.Parameters["@NEW_OUTCOME_ID"].Value != DBNull.Value)
                            {
                                NewOutcomeId = (Int64)command.Parameters["@NEW_OUTCOME_ID"].Value;
                            }
                        }

                        // Currently this is in the "too difficult and not worth the effort" box, as these data structures will all change soon
                        //if (NewOutcomeId > 0)
                        //{
                        //    string attributes = "";
                        //    if (o.OutcomeCodes != null && o.OutcomeCodes.OutcomeItemAttributesList != null)
                        //    {
                        //        foreach (Outcomeitemattributeslist a in o.OutcomeCodes.OutcomeItemAttributesList)
                        //        {
                        //            if (attributes == "")
                        //            {
                        //                AttributeSet tmp = rs.GetAttributeSetFromOriginalAttributeId(a.AttributeId);
                        //                if (tmp != null) attributes = tmp.AttributeId.ToString();
                        //            }
                        //            else
                        //            {
                        //                AttributeSet tmp = rs.GetAttributeSetFromOriginalAttributeId(a.AttributeId);
                        //                if (tmp != null) attributes += "," + tmp.AttributeId.ToString();
                        //            }
                        //        }
                        //        using (SqlCommand command = new SqlCommand("st_OutcomeItemAttributesSave", connection))
                        //        {
                        //            command.CommandType = System.Data.CommandType.StoredProcedure;
                        //            command.Parameters.Add(new SqlParameter("@OUTCOME_ID", NewOutcomeId));
                        //            command.Parameters.Add(new SqlParameter("@ATTRIBUTES", attributes));
                        //            command.ExecuteNonQuery();
                        //        }
                        //    }
                        //}
                    }
                    connection.Close();
                }
            }
        }
        private Int64 SaveAttributeCode(string setURL, string extURL, int ReviewId, int ContactId, Int64 ItemId, string additionalText = "")
        {
            Int64 ItemAttributeId = 0;
            AttributeSet aset;
            ReviewSet rs = this.ReviewSets.GetReviewSetFromExtURL(setURL);
            if (rs != null)
            {
                // this *should* return the right attributeset
                aset = rs.GetAttributeSetFromExtURL(extURL);
            }
            else
            {
                // but if it doesn't, we have a backup
                aset = this.ReviewSets.GetAttributeSetFromExtURL(extURL);
            }
            if (aset != null)
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ItemAttributeInsert", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                        command.Parameters.Add(new SqlParameter("@ADDITIONAL_TEXT", additionalText ?? string.Empty));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", aset.AttributeId));
                        command.Parameters.Add(new SqlParameter("@SET_ID", aset.SetId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ID", ItemId));
                        command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                        command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@NEW_ITEM_ATTRIBUTE_ID", 0));
                        command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(new SqlParameter("@NEW_ITEM_SET_ID", 0));
                        command.Parameters["@NEW_ITEM_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        if (command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value != DBNull.Value)
                        {
                            ItemAttributeId = (Int64)command.Parameters["@NEW_ITEM_ATTRIBUTE_ID"].Value;
                        }
                        Int64 _itemSetId = (Int64)command.Parameters["@NEW_ITEM_SET_ID"].Value;
                    }
                    connection.Close();
                }
            }
            return ItemAttributeId;
        }
        private async Task<string> FetchVocabulary(string uri)
        {
            using var http = new HttpClient();
            var response = await http.GetAsync(uri);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // but we could handle explicitly
                throw new InvalidOperationException($"Evidence Repository get vocab call failed: {(int)response.StatusCode} {body}");
            }
            return body;
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
        private string getFirstAuthor(Enhancement biblioEnhancement)
        {
            string? displayName = biblioEnhancement?.content?.authorship?
                .FirstOrDefault()?
                .display_name;

            return string.IsNullOrWhiteSpace(displayName)
                ? string.Empty
                : displayName.Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)[^1];
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
        public static int ItemTypeIdFromOpenAlexPublicationType(string incomingType) // might want to move this somewhere else in the codebase as it works for any OpenAlex or CrossRef article type
        {
            return incomingType switch
            {
                "book" => 2,
                "book-chapter" => 3,
                
                "dissertation" => 4,

                "conference-abstract" or
                "conference-paper" => 5,

                "data-paper" => 7,
                "dataset" => 9,

                "book-review" or
                "article" or
                "editorial" or
                "erratum" or
                "letter" or
                "preprint" or
                "retraction" or
                "review" => 14,

                _ => 1
            };
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
    public string? supersedes { get; set; }
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
    public System.Text.Json.JsonElement data { get; set; }
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
