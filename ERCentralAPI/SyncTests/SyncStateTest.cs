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
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;

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
        private SourcesController _sourcesController;

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
            ClaimsPrincipal user = WasBuildToken(SetAuthenticationToBeChangedWithoutRealParamValues() );

            // My set up is like so, with one item being found in Zotero
            // which contains a child attachment (which is different to a straight attachment item)
            // the attachment supersedes the state of the original item, meaning
            // after pulling, one may need to push as well after attachment is updated
            // but parent item may for instance be ahead.
            string itemIds = "2680363, 2680364";
            

            var dp = new DataPortal<ZoteroItemReviewIDs>();
            var criteria = new SingleCriteria<string>(itemIds);
            _zoteroItemReviewIds = dp.Fetch(criteria);

            var dictionary = new ZoteroConcurrentDictionary();
            var logger = new LoggerConfiguration().CreateBootstrapLogger();
            var zoteroLogger = new SerilogLoggerFactory(logger)
                    .CreateLogger<ZoteroController>(); 
            _zoteroController = new ZoteroController(_configuration, zoteroLogger, dictionary);
            SetControllerUserContext(_zoteroController, user);
            var sourcesLogger = new SerilogLoggerFactory(logger)
                   .CreateLogger<SourcesController>();
            _sourcesController = new SourcesController(sourcesLogger);
            SetControllerUserContext(_sourcesController, user);
            UploadOrCheckSource source = new UploadOrCheckSource();
            
            _sourcesController.UploadSource(source);

        }

        private ReviewerIdentity SetAuthenticationToBeChangedWithoutRealParamValues()
        {
            string username = "sg";
            string password = "ePageuke3";
            int reviewId = 7;
            string LoginMode = "";
            string roles = ""; 
            ReviewerIdentity ri = ReviewerIdentity.GetIdentity(username, password, reviewId, LoginMode, null);
            if (ri.IsAuthenticated)
            {
                //ri.Token = BuildToken(ri);
            }
            else throw new UnauthorizedAccessException();

            ReviewerPrincipal principal = new ReviewerPrincipal(ri);
            Csla.ApplicationContext.User = principal;

            var test = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            return test;
        }

        private ClaimsPrincipal WasBuildToken(ReviewerIdentity ri)
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
            return new ClaimsPrincipal(riCI);
            //var token = new JwtSecurityToken(_configuration["AppSettings:EPPIApiUrl"],
            //  _configuration["AppSettings:EPPIApiClientName"],
            //  riCI.Claims,
            //  notBefore: DateTime.Now.AddSeconds(-60),
            //  expires: DateTime.Now.AddHours(6),
            //  //expires: DateTime.Now.AddSeconds(15),
            //  signingCredentials: creds);

            //return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void SetControllerUserContext(Controller controller, ClaimsPrincipal user)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = user
                }
            };
        }


        [TearDown]
        public void TearDown()
        {
            _zoteroItemReviewIds.Clear();
        }

        [Test] //TODO should check mock database eventually
        public void CheckSyncStatusOfItemsInBothTest()
        {
            var dpItemsInBoth = new DataPortal<ZoteroERWebReviewItemList>();
            var result = dpItemsInBoth.Fetch();

            var itemsInZotero = result.Select(x => x.ITEM_REVIEW_ID).Intersect(_zoteroItemReviewIds.Select(x => x.ITEM_REVIEW_ID));

            Assert.That(itemsInZotero.Any());

        }


        [Test]
        public async Task CheckSyncStatusOfListOfItemIdsAsyncTest()
        {
            var resultantSyncStateDictionary = await UpdateSyncStateOfLocalItemsRelativeToZoteroAsync(_zoteroItemReviewIds);

            var countOfUpToDate = resultantSyncStateDictionary.Count(x => x.Value == State.upToDate);

            Assert.That(countOfUpToDate, Is.EqualTo(0));
        }


        [TestCase("2471361, 2471362")]
        public async Task UpdateListOfLocalItemsBehindZoteroAsyncTest(string listOfLocalItemReviewIdsBehindZotero)
        {
            var resultantSyncStateDictionary = await PushBehindLocalItemsToZoteroAsync(listOfLocalItemReviewIdsBehindZotero);

            var countOfUpToDate = resultantSyncStateDictionary.Count(x => x.Value == State.upToDate);

            Assert.That(countOfUpToDate, Is.EqualTo(2));
        }

        [TestCase("2526686")]
        public async Task UsingControllerInitialTest(string itemId)
        {
            var actionResult = await _zoteroController.ItemIdLocal(itemId);

            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);

            var actualResult = okResult.Value as ZoteroItemIDPerItemReview;
            Assert.IsNotNull(actualResult);
        }

        [Test]
        public async Task FetchSyncStateTest()
        {
            var actionResult = await _zoteroController.FetchSyncState(_zoteroItemReviewIds);
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);

            var actualResult = okResult.Value as Dictionary<long, State>;
            Assert.That(State.attachmentAhead, Is.EqualTo(actualResult?.FirstOrDefault().Value)); 
        }

        // 1 - Helper method for test will not be required when DB is setup for testing
        private async Task<IDictionary<long, State>> PushBehindLocalItemsToZoteroAsync(string listOfLocalItemReviewIdsBehindZotero)
        {
            Dictionary<long, State> syncStateResults = new Dictionary<long, State>();
            var listOfItemReviewIdsToPush = listOfLocalItemReviewIdsBehindZotero.Split(',');
            foreach (var itemReviewId in listOfItemReviewIdsToPush)
            {
                var dp = new DataPortal<ZoteroERWebReviewItem>();
                var criteria = new SingleCriteria<ZoteroERWebReviewItem, string>(itemReviewId);
                var localSyncedItem = await dp.FetchAsync(criteria);

                if (localSyncedItem.ItemID == 0)
                {
                    continue;
                }

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

        // 2 - Helper method for test will not be required when DB is setup for testing
        private async Task<bool> PushItemToZotero(string itemKey, Item updatedItem, string itemReviewId)
        {
            var zoteroItemContent = await _zoteroController.GetZoteroConvertedItemAsync(itemKey);
            ZoteroReviewConnection zrc = _zoteroController.ApiKey();
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
                zoteroItemContent = await _zoteroController.GetZoteroConvertedItemAsync(itemKey);

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

        // 3 - Helper method for test will not be required when DB is setup for testing
        private async Task<IDictionary<long, State>> UpdateSyncStateOfLocalItemsRelativeToZoteroAsync(ZoteroItemReviewIDs itemList)
        {
            var syncStateResults = new Dictionary<long, State>();
            foreach (var item in itemList)
            {
                await _zoteroController.UpdateSyncStatusOfItemAsync(syncStateResults, item);
            }
            return syncStateResults;
        }    
    }
}
