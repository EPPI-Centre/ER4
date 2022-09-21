using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Csla;
using EPPIDataServices.Helpers;
using ERxWebClient2.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using ERxWebClient2.Controllers;
using static BusinessLibrary.BusinessClasses.ZoteroERWebReviewItem;
using ERxWebClient2.Zotero;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;

namespace SyncTests
{
    public class SyncTests
    {
        private ZoteroItemReviewIDs _zoteroItemReviewIds;
        private IConfigurationRoot _configuration;
        private ZoteroService _zoteroService;
        private string baseUrl;
        private ConcreteReferenceCreator _concreteReferenceCreator;
        private ZoteroController _zoteroController;

        [SetUp]
        public void Setup()
        {

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _configuration = builder.Build();
            var SqlHelper = new SQLHelper(_configuration, null);

            _concreteReferenceCreator = ConcreteReferenceCreator.Instance;
            _zoteroService = ZoteroService.Instance;
            baseUrl = _configuration["ZoteroSettings:baseUrl"];

            DataConnection.DataConnectionConfigure(SqlHelper);
            SetAuthenticationToBeChangedWithoutRealParamValues();

            string itemIds = "2680356, 2680355";

            var dp = new DataPortal<ZoteroItemReviewIDs>();
            var criteria = new SingleCriteria<string>(itemIds);
            _zoteroItemReviewIds = dp.Fetch(criteria);

            // TODO
            // 1 - Eventually need to mock database enter data for tests and then

            // 2 - then need to clear this table in mock database on tearDown
            var dictionary = new ZoteroConcurrentDictionary();
            _zoteroController = new ZoteroController(_configuration, null, dictionary);
           

        }

        private void SetAuthenticationToBeChangedWithoutRealParamValues()
        {
            string username = "qtnvpod";
            string password = "CrapBirkbeck1";
            int reviewId = 7;
            string LoginMode = "";
            string roles = "";
            ReviewerIdentity ri = ReviewerIdentity.GetIdentity(username, password, reviewId, LoginMode, null);
            if (ri.IsAuthenticated)
            {
                ri.Token = BuildToken(ri);
            }

            ReviewerPrincipal principal = new ReviewerPrincipal(ri);
            Csla.ApplicationContext.User = principal;

            var test = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
        }

        private string BuildToken(ReviewerIdentity ri)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:EPPIApiClientSecret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            IIdentity id = ri as IIdentity;
            ClaimsIdentity riCI = new ClaimsIdentity(id);
            IEnumerable<Claim> claims = riCI.Claims;
            riCI.AddClaim(new Claim("reviewId", ri.ReviewId.ToString()));
            riCI.AddClaim(new Claim("userId", ri.UserId.ToString()));
            riCI.AddClaim(new Claim("name", ri.Name));
            riCI.AddClaim(new Claim("reviewTicket", ri.Ticket));
            riCI.AddClaim(new Claim("isSiteAdmin", ri.IsSiteAdmin.ToString()));
            riCI.AddClaim(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            foreach (var userRole in ri.Roles)
            {
                riCI.AddClaim(new Claim(ClaimTypes.Role, userRole));
            }
            var token = new JwtSecurityToken(_configuration["AppSettings:EPPIApiUrl"],
              _configuration["AppSettings:EPPIApiClientName"],
              riCI.Claims,
              notBefore: DateTime.Now.AddSeconds(-60),
              expires: DateTime.Now.AddHours(6),
              //expires: DateTime.Now.AddSeconds(15),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [TearDown]
        public void TearDown()
        {
            _zoteroItemReviewIds.Clear();
        }

        [Test] //TODO should check mock database eventually
        public void CheckSyncStatusOfItemsInBoth()
        {
            var dpItemsInBoth = new DataPortal<ZoteroERWebReviewItemList>();
            var result = dpItemsInBoth.Fetch();

            var itemsInZotero = result.Select(x => x.ITEM_REVIEW_ID).Intersect(_zoteroItemReviewIds.Select(x => x.ITEM_REVIEW_ID));

            Assert.That(itemsInZotero.Any());

        }


        [Test]
        public async Task CheckSyncStatusOfListOfItemIdsAsync()
        {
            var resultantSyncStateDictionary = await UpdateSyncStateOfLocalItemsRelativeToZoteroAsync(_zoteroItemReviewIds);

            var countOfUpToDate = resultantSyncStateDictionary.Count(x => x.Value == State.upToDate);

            Assert.That(countOfUpToDate, Is.EqualTo(0));
        }


        [TestCase("2625028, 2625029")]
        public async Task UpdateListOfLocalItemsBehindZoteroAsync(string listOfLocalItemReviewIdsBehindZotero)
        {
            var resultantSyncStateDictionary = await PushBehindLocalItemsToZoteroAsync(listOfLocalItemReviewIdsBehindZotero);

            var countOfUpToDate = resultantSyncStateDictionary.Count(x => x.Value == State.upToDate);

            Assert.That(countOfUpToDate, Is.EqualTo(1));
        }

        [Test]
        public async Task UsingControllerInitialTest()
        {
            var actionResult = await _zoteroController.ItemIdLocal("2625028");

            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);

            var actualResult = okResult.Value as ZoteroItemIDPerItemReview;
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(actualResult.ITEM_DOCUMENT_ID, 0);
            Assert.AreEqual(actualResult.ITEM_ID, 2680356);
            Assert.AreEqual(actualResult.ITEM_REVIEW_ID, 0);
        }

        private async Task<IDictionary<long, State>> PushBehindLocalItemsToZoteroAsync(string listOfLocalItemReviewIdsBehindZotero)
        {
            Dictionary<long, State> syncStateResults = new Dictionary<long, State>();
            var listOfItemReviewIdsToPush = listOfLocalItemReviewIdsBehindZotero.Split(',');
            foreach (var itemReviewId in listOfItemReviewIdsToPush)
            {
                var dp = new DataPortal<ZoteroERWebReviewItem>();
                var criteria = new SingleCriteria<ZoteroERWebReviewItem, string>(itemReviewId);
                var localSyncedItem = await dp.FetchAsync(criteria);

                var dp2 = new DataPortal<Item>();
                SingleCriteria<Item, Int64> criteriaItem =
                   new SingleCriteria<Item, Int64>(localSyncedItem.ItemID);

                var itemResult = await dp2.FetchAsync(criteriaItem);
                if(itemResult.ItemId > 0 && localSyncedItem.ItemKey.Length > 0 && itemReviewId.Length > 0)
                {
                    var result = await PushItemToZotero(localSyncedItem.ItemKey, itemResult, itemReviewId);
                    if (result == true)
                    {
                        syncStateResults.TryAdd(Convert.ToInt64(itemReviewId), State.upToDate);
                    }
                }                
            }
            return syncStateResults;
        }

        private async Task<bool> PushItemToZotero(string itemKey, Item updatedItem, string itemReviewId)
        {
            var zoteroItemContent = await GetZoteroConvertedItemAsync(itemKey);
            ZoteroReviewConnection zrc = ApiKey();
            string groupIDBeingSynced = zrc.LibraryId;
            var _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Zotero-API-Version", "3");
            _httpClient.DefaultRequestHeaders.Add("Zotero-API-Key", zrc.ApiKey);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Unmodified-Since-Version", zoteroItemContent.version.ToString());

            HttpClientProvider httpClientProvider = new HttpClientProvider(_httpClient);
            _zoteroService.SetZoteroServiceHttpProvider(httpClientProvider);
            var PUTItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + itemKey);
            _httpClient.BaseAddress = new Uri(PUTItemUri.ToString());

            HttpResponseMessage response = new HttpResponseMessage();
            var erWebItem = new ERWebItem
            {
                Item = updatedItem
            };
            var zoteroReference = _concreteReferenceCreator.GetReference(erWebItem);
            var zoteroItem = zoteroReference.MapReferenceFromErWebToZotero();

            var payload = JsonConvert.SerializeObject(zoteroItem);          

            response = await _zoteroService.UpdateZoteroItem<ZoteroReviewItem>(payload, PUTItemUri.ToString());
            var actualCode = response.StatusCode;
            if (actualCode == System.Net.HttpStatusCode.NoContent)
            {
                zoteroItemContent = await GetZoteroConvertedItemAsync(itemKey);

                var dp1 = new DataPortal<ZoteroReviewItem>();
                SingleCriteria<ZoteroReviewItem, string> criteria = new SingleCriteria<ZoteroReviewItem, string>(itemKey);
                var zoteroReviewItemFetch = dp1.Fetch(criteria);

                zoteroReviewItemFetch.ItemKey = itemKey;
                zoteroReviewItemFetch.Version = zoteroItemContent.version;
                zoteroReviewItemFetch.SyncState = (int)State.upToDate;

                zoteroReviewItemFetch = dp1.Execute(zoteroReviewItemFetch);
                return true;
            }
            return false;
        }

        private async Task<IDictionary<long, State>> UpdateSyncStateOfLocalItemsRelativeToZoteroAsync(ZoteroItemReviewIDs itemList)
        {
            var syncStateResults = new Dictionary<long, State>();
            foreach (var item in itemList)
            {
                await UpdateSyncStatusOfItemAsync(syncStateResults, item);
            }
            return syncStateResults;
        }

        //private void UpdateSyncStatusOfDocumentItem(Dictionary<long, SyncState> syncStateResults, Item erWebItem, 
        //    ItemDocument erWebItemDocument)
        //{
        //        //Compare item with Zotero
        //        var zoteroAttachment = GetZoteroAttachment(erWebItem);
        //        // does not assume attachments below
        //        if (erWebItemDocument.DateEdited == zoteroAttachment.DateEdited)
        //        {
        //            syncStateResults.Add(erWebItem.ItemId, SyncState.upToDate);

        //        }
        //        else if (erWebItemDocument.DateEdited > zoteroAttachment.DateEdited)
        //        {
        //            syncStateResults.Add(erWebItem.ItemId, SyncState.ahead);
        //        }
        //        else
        //        {
        //            syncStateResults.Add(erWebItem.ItemId, SyncState.behind);
        //        }
        //}

        private async Task UpdateSyncStatusOfItemAsync(Dictionary<long, State> syncStateResults, ZoteroItemIDPerItemReview item)
        {
            // 1 - Convert from itemReviewId to itemKey
            var dp = new DataPortal<ZoteroERWebReviewItem>();
            var criteria = new SingleCriteria<ZoteroERWebReviewItem, string>(item.ITEM_REVIEW_ID.ToString());
            var localSyncedItem = dp.Fetch(criteria);
                     
            if (localSyncedItem.Zotero_item_review_ID > 0)
            {
                var zoteroItem = await GetZoteroConvertedItemAsync(localSyncedItem.ItemKey);
                var zoteroItemDateLastModified = Convert.ToDateTime(zoteroItem.data.dateModified);
                // TODO does not assume attachments below
                if (localSyncedItem.LAST_MODIFIED == zoteroItemDateLastModified)
                {
                    syncStateResults.TryAdd(item.ITEM_ID, State.upToDate);
                }
                else if (localSyncedItem.LAST_MODIFIED > zoteroItemDateLastModified)
                {
                    syncStateResults.TryAdd(item.ITEM_ID, State.ahead);
                }
                else
                {
                    syncStateResults.TryAdd(item.ITEM_ID, State.behind);
                }

                
                //if (item.ITEM_DOCUMENT_ID > 0)
                //{
                //    var itemDocument = GetErWebDocument(item);
                //    UpdateSyncStatusOfDocumentItem(syncStateResults, erWebItem, itemDocument);
                //}
            }
            else
            {
                syncStateResults.TryAdd(item.ITEM_ID, State.doesNotExist);
            }
        }

        private ItemDocument GetErWebDocument(long item)
        {
            return new ItemDocument();
        }

        //private object GetZoteroAttachment(Item erWebItem)
        //{

        //}

        public void SetZoteroHttpService(UriBuilder uri, string zoteroApiKey, bool ifNoneMatchHeader = false)
        {
            var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(uri.ToString())
            };
            _httpClient.DefaultRequestHeaders.Add("Zotero-API-Version", "3");
            _httpClient.DefaultRequestHeaders.Add("Zotero-API-Key", zoteroApiKey);
            if (ifNoneMatchHeader)
            {
                _httpClient.DefaultRequestHeaders.Add("If-None-Match", "*");
            }
            var httpClientProvider = new HttpClientProvider(_httpClient);
            _zoteroService.SetZoteroServiceHttpProvider(httpClientProvider);
        }

        private async Task<Collection> GetZoteroConvertedItemAsync(string itemKey){

            ZoteroReviewConnection zrc = ApiKey();
            string groupIDBeingSynced = zrc.LibraryId;
            var GETItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + itemKey);
            SetZoteroHttpService(GETItemUri, zrc.ApiKey);
            var zoteroItem = await _zoteroService.GetCollectionItem(GETItemUri.ToString());
            return zoteroItem;
        }

        //private Item GetSyncedErWebItem(long itemReviewId){
        //    var criteria = new SingleCriteria<ZoteroItemReview, long>(itemReviewId);
        //    var dp = new DataPortal<ZoteroItemReview>();
        //    var item = dp.Fetch(criteria);
        //    if (item != null) return item;
        //    return new Item();
        //}

        public ZoteroReviewConnection ApiKey()
        {
            ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();
            return zrc;
        }
    }
}
