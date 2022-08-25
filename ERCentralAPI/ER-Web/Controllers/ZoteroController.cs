using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPPIDataServices.Helpers;
using ERxWebClient2.Services;
using System.Net.Http;
using System.Threading.Tasks;
using BusinessLibrary.BusinessClasses;
using Csla;
using BusinessLibrary.Security;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using Csla.Data;
using ERxWebClient2.Zotero;
using System.IO;
using System.Net;
using System.Data;
using Microsoft.Extensions.Configuration;
using BusinessLibrary.BusinessClasses.ImportItems;
using Csla.Core;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ZoteroController : CSLAController
    {
        // For thread safety each of these needs to be a Singleton      
        private ZoteroService _zoteroService;  //THREAD SAFE Singleton
        // below are inherently threadsafe
        private string baseUrl;
        private static string AdmConnStr;
        private string clientKey;
        private string clientSecret;
        private string callbackUrl; //TODO remember to change in Zotero when moving to production for app callback url
        private string zotero_request_token_endpoint;
        private string zotero_authorize_endpoint;
        private string zotero_access_token_endpoint;
        private static string access_oauth_Token_Secret = "";
        // all above strings are inherently thread safe
        private ConcreteReferenceCreator _concreteReferenceCreator; //THREAD SAFE Singleton
        private ZoteroConcurrentDictionary _zoteroConcurrentDictionary;   //THREAD SAFE type
        private ILogger<ZoteroController> _logger;   //THREAD SAFE Singleton
        private IConfiguration _configuration; // THIS IS THREAD SAFE AS services.AddSingleton(Configuration);
        private OAuthParameters _oAuth;   //THREAD SAFE Singleton

        private void SetZoteroHttpService(UriBuilder uri, string zoteroApiKey, bool ifNoneMatchHeader = false)
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

        public ZoteroController(IConfiguration appConfiguration, ILogger<ZoteroController> logger, ZoteroConcurrentDictionary zoteroConcurrentDictionary) : base(logger)
        {
            _configuration = appConfiguration;
            _logger = logger;
            _zoteroService = ZoteroService.Instance;
            
            AdmConnStr = appConfiguration.GetSection("AppSettings")["ER4DB"];
            if (_zoteroConcurrentDictionary == null)
            {
                _zoteroConcurrentDictionary = zoteroConcurrentDictionary;
            }
            var configuration = appConfiguration.GetSection("ZoteroSettings");
            clientKey = configuration["clientKey"];
            clientSecret = configuration["clientSecret"];
            baseUrl = configuration["baseUrl"];
            callbackUrl = configuration["callbackUrl"];
            zotero_request_token_endpoint = configuration["zotero_request_token_endpoint"];
            zotero_authorize_endpoint = configuration["zotero_authorize_endpoint"];
            zotero_access_token_endpoint = configuration["zotero_access_token_endpoint"];
            _concreteReferenceCreator = ConcreteReferenceCreator.Instance;
            _oAuth = OAuthParameters.Instance;
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> OauthProcess([FromQuery] string erWebuserId, [FromQuery] long reviewID)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                _oAuth.ClientKey = clientKey;
                _oAuth.ClientSecret = clientSecret;
                var oauthURL = _oAuth.GetAuthorizationUrl(zotero_request_token_endpoint);

                _zoteroConcurrentDictionary.Session.TryAdd("oauthTimeStamp-" + reviewID, _oAuth.timeStamp);
                _zoteroConcurrentDictionary.Session.TryAdd("oauthNonce-" + reviewID, _oAuth.nonce);

                var requestZoteroUri = new UriBuilder(oauthURL);
                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(requestZoteroUri.ToString());

                HttpClientProvider httpClientProvider = new HttpClientProvider(_httpClient);
                _zoteroService.SetZoteroServiceHttpProvider(httpClientProvider);

                var response = await _zoteroService.GetUserPermissions(requestZoteroUri.ToString());

                var indexOfAnd = response.IndexOf('&');

                var responseJson = "";

                if (indexOfAnd > -1)
                {
                    responseJson = response.Substring(0, indexOfAnd);
                }

                var equalsIndexToken = responseJson.IndexOf('=');
                var token = responseJson.Substring(equalsIndexToken + 1);

                var remainingStringResponse = response.Substring(indexOfAnd + 1);
                var indexOfSecretAnd = remainingStringResponse.IndexOf('&');
                var secretString = remainingStringResponse.Substring(0, indexOfSecretAnd);
                var equalsIndex = secretString.IndexOf('=');
                var zotero_token_secret = secretString.Substring(equalsIndex + 1);
                var redirectUrl = zotero_authorize_endpoint + "?" + responseJson;

                _zoteroConcurrentDictionary.Session.TryRemove("zotero_temp_token-" + reviewID, out string zotero_temp_tokenOut);
                _zoteroConcurrentDictionary.Session.TryRemove("erWebuserId-" + token, out string erWebuserIdOut);
                _zoteroConcurrentDictionary.Session.TryRemove("zotero_token_secret-" + token, out string zotero_token_secretOut);
                _zoteroConcurrentDictionary.Session.TryRemove("reviewID", out string reviewIDOut);

                _zoteroConcurrentDictionary.Session.TryAdd("zotero_temp_token-" + reviewID, token);
                _zoteroConcurrentDictionary.Session.TryAdd("erWebuserId-" + token, erWebuserId);
                _zoteroConcurrentDictionary.Session.TryAdd("zotero_token_secret-" + token, zotero_token_secret);
                _zoteroConcurrentDictionary.Session.TryAdd("reviewID", reviewID.ToString());

                return Json(token);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Starting the Oauth Process has an error");
                return StatusCode(500, e.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> OauthVerifyGet([FromQuery] string oauth_token, [FromQuery] string oauth_verifier)
        {
            try
            {
                var eRWebuserIdResult = _zoteroConcurrentDictionary.Session.TryGetValue("erWebuserId-" + oauth_token, out string erWebuserId);
                var reviewIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("reviewID", out string ReviewID);
                var tempTokenResult = _zoteroConcurrentDictionary.Session.TryGetValue("zotero_temp_token-" + ReviewID, out string zotero_temp_token);

                if (!tempTokenResult) return StatusCode(400, "temp token is null");
                if (!eRWebuserIdResult) return StatusCode(400, "eRWebuserId is null");
                if (!reviewIDResult) return StatusCode(400, "reviewID is null");
                var reviewID = Convert.ToInt64(ReviewID);
                if (!zotero_temp_token.Equals(oauth_token)) return Unauthorized();

                string verifier = oauth_verifier;
                string tokenValue = oauth_token;
                string url = zotero_access_token_endpoint;
                var getSecretTokenResult = _zoteroConcurrentDictionary.Session.TryGetValue("zotero_token_secret-" + oauth_token, out string zotero_token_secret);
                if (!getSecretTokenResult) return StatusCode(400, "zotero_token_secret is null");

                //TODO need to build something in to retry this for a longer time period if it fails at
                // first and just say waiting on Zotero
                var signedURL = GetSignedUrl(ReviewID, url, tokenValue, zotero_token_secret, verifier);

                var accessZoteroUri = new UriBuilder(signedURL);
                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(accessZoteroUri.ToString());

                var httpClientProviderF = new HttpClientProvider(_httpClient);
                _zoteroService.SetZoteroServiceHttpProvider(httpClientProviderF);
                var responseThree = await _zoteroService.GetKey(accessZoteroUri.ToString());

                var access_oauth_TokenIndex = responseThree.IndexOf('=');
                var indexOfAccessAnd = responseThree.IndexOf('&');
                var access_oauth_Token = responseThree.Substring(access_oauth_TokenIndex + 1, indexOfAccessAnd - access_oauth_TokenIndex - 1);
                var remainingStringresponseThree = responseThree.Substring(indexOfAccessAnd + 1);
                var indexOfSecondEquals = remainingStringresponseThree.IndexOf('=');
                var indexOfSecondAnd = remainingStringresponseThree.IndexOf('&');
                access_oauth_Token_Secret = remainingStringresponseThree.Substring(indexOfSecondEquals + 1, indexOfSecondAnd - indexOfSecondEquals - 1);
                var remainingStringresponseFour = remainingStringresponseThree.Substring(indexOfSecondAnd + 1);
                var indexOfThirdEquals = remainingStringresponseFour.IndexOf('=');
                var indexOfThirdAnd = remainingStringresponseFour.IndexOf('&');
                var access_userId = remainingStringresponseFour.Substring(indexOfThirdEquals + 1, indexOfThirdAnd - indexOfThirdEquals - 1);

                var remainingStringresponseFive = remainingStringresponseFour.Substring(indexOfThirdAnd + 1);
                var indexOfFourthEquals = remainingStringresponseFive.IndexOf('=');
                var access_user_name = remainingStringresponseFive.Substring(indexOfFourthEquals + 1);
                var apiKeyURL = $"{baseUrl}/users/" + access_userId + "/collections?limit=1&key=" + access_oauth_Token_Secret + "";
                var responseCollections = await _zoteroService.GetKey(apiKeyURL);
                JArray collections = JsonConvert.DeserializeObject<JArray>(responseCollections);

                DataPortal<ZoteroReviewCollection> dp = new DataPortal<ZoteroReviewCollection>();
                UserDetails userDetails = new UserDetails();
                userDetails.reviewId = Convert.ToInt64(ReviewID);
                userDetails.userId = Convert.ToInt16(erWebuserId);

                SingleCriteria<ZoteroReviewCollection, long> criteria =
                        new SingleCriteria<ZoteroReviewCollection, long>(userDetails.reviewId);

                var resultCollection = await dp.FetchAsync(criteria);

                if (!string.IsNullOrEmpty(access_oauth_Token_Secret))
                {
                    _zoteroConcurrentDictionary.Session.TryAdd("apiKey-" + ReviewID, access_oauth_Token_Secret);
                    _zoteroConcurrentDictionary.Session.TryUpdate("apiKey-" + ReviewID, access_oauth_Token_Secret, "");
                }
                var firstGroup = await this.GetGroups(access_userId, reviewID.ToString());

                if (firstGroup.Count == 0)
                {
                    return Redirect(callbackUrl + "?error=nogroups");
                }
                if (resultCollection.ApiKey.Length == 0)
                {
                    if (collections.Count > 0)
                    {
                        foreach (var collection in collections)
                        {
                            var reviewCollection = new ZoteroReviewCollection
                            {
                                ApiKey = access_oauth_Token_Secret,
                                CollectionKey = collection["key"].ToString(),
                                LibraryID = firstGroup.FirstOrDefault().id.ToString(),
                                USER_ID = Convert.ToInt16(erWebuserId),
                                REVIEW_ID = reviewID,
                                ParentCollection = collection["data"]["parentCollection"].ToString(),
                                CollectionName = collection["data"]["name"].ToString(),
                                Version = Convert.ToInt64(collection["data"]["version"].ToString()),
                                DateCreated = DateTime.Now
                            };
                            reviewCollection = dp.Execute(reviewCollection);
                            _zoteroConcurrentDictionary.Session.TryAdd("libraryId-" + reviewID + collection["library"]["id"].ToString(), collection["library"]["id"].ToString());
                        }

                    }
                    else
                    {
                        var reviewCollection = new ZoteroReviewCollection
                        {
                            ApiKey = access_oauth_Token_Secret,
                            CollectionKey = "",
                            LibraryID = "",
                            USER_ID = Convert.ToInt16(erWebuserId),
                            REVIEW_ID = reviewID,
                            ParentCollection = "",
                            CollectionName = "",
                            Version = 0,
                            DateCreated = DateTime.Now
                        };
                        reviewCollection = dp.Execute(reviewCollection);
                    }
                }

                return Redirect(callbackUrl);
            }
            catch (Exception e)
            {
                // On error ensure groupIdBeingSynced is being removed
                var removeGroupIdSynced = _zoteroConcurrentDictionary.Session.TryRemove("groupIDBeingSynced-" + oauth_token, out string groupIdSync);

                if (e.Message == "Response status code does not indicate success: 401 (Unauthorized).")
                {
                    _logger.LogException(e, "Zotero Oauth Verify Process has the classic Unauthorized error");
                    return Redirect(callbackUrl + "?error=unauthorised");
                }
                _logger.LogException(e, "Zotero Oauth Verify Process has an error");
                return StatusCode(500, e.Message);
            }
            finally
            {
                RemoveSessionVariablesNoLongerRequired(oauth_token);
            }
        }

        private void RemoveSessionVariablesNoLongerRequired(string oauth_token)
        {
            var removeuserId = _zoteroConcurrentDictionary.Session.TryRemove("erWebuserId-" + oauth_token, out string erWebuserIdOut);
            var removeReviewID = _zoteroConcurrentDictionary.Session.TryRemove("reviewID", out string reviewIDOut);
            var removeZotero_temp_token = _zoteroConcurrentDictionary.Session.TryRemove("zotero_temp_token-" + reviewIDOut, out string zotero_temp_tokenOut);
            var removeZotero_token_secret = _zoteroConcurrentDictionary.Session.TryRemove("zotero_token_secret-" + oauth_token, out string zotero_token_secretOut);
            var removeZoteroApiKey = _zoteroConcurrentDictionary.Session.TryRemove("apiKey-" + reviewIDOut, out string zotero_api_key);
            var removeoauthTimeStamp = _zoteroConcurrentDictionary.Session.TryRemove("oauthTimeStamp-" + reviewIDOut, out string oauthTimeStamp);
            var removeoauthNonce = _zoteroConcurrentDictionary.Session.TryRemove("oauthNonce-" + reviewIDOut, out string oauthNonce);


            //if (!removeuserId || !removeReviewID || !removeZotero_temp_token || !removeZotero_token_secret || !removeZoteroApiKey 
            //    || !removeoauthTimeStamp || !removeoauthNonce) 
            //    throw new Exception("Error removing session variables");
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> FetchGroupToReviewLinks()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");

                var getKeyResult = await ApiKey();
                DataPortal<ZoteroReviewCollectionList> dp = new DataPortal<ZoteroReviewCollectionList>();
                SingleCriteria<ZoteroReviewCollectionList, long> criteria =
                        new SingleCriteria<ZoteroReviewCollectionList, long>(Convert.ToInt64(ri.ReviewId));
                var reviewCollectionList = await dp.FetchAsync(criteria);

                return Ok(reviewCollectionList);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetch GroupToReview has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateGroupToReview([FromBody] string groupId, string deleteLink)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");

                var apiKey = await ApiKey();
                var zoteroApiKey = apiKey?.Value?.ToString();
                if (string.IsNullOrEmpty(zoteroApiKey))
                {
                    return Unauthorized();
                }
                DataPortal<ZoteroReviewCollectionList> dp = new DataPortal<ZoteroReviewCollectionList>();

                SingleCriteria<ZoteroReviewCollectionList, long> criteria =
                        new SingleCriteria<ZoteroReviewCollectionList, long>(ri.ReviewId);
                var reviewCollection = await dp.FetchAsync(criteria);

                var reviewCollectionItem = reviewCollection.FirstOrDefault(x => x.LibraryID == groupId && x.REVIEW_ID.ToString() == ri.ReviewId.ToString());
                var deleteReviewLink = Convert.ToBoolean(deleteLink);

                if (reviewCollectionItem == null && !deleteReviewLink)
                {
                    DataPortal<ZoteroReviewCollection> dpNew = new DataPortal<ZoteroReviewCollection>();
                    var newReviewCollectionItem = new ZoteroReviewCollection
                    {
                        ApiKey = zoteroApiKey,
                        LibraryID = groupId,
                        USER_ID = ri.UserId,
                        REVIEW_ID = ri.ReviewId,
                        DateCreated = DateTime.Now,
                        GroupBeingSynced = Convert.ToInt32(groupId)
                    };

                    newReviewCollectionItem = dpNew.Execute(newReviewCollectionItem);

                    return Ok(newReviewCollectionItem);
                }


                if (deleteReviewLink && reviewCollectionItem != null)
                {
                    reviewCollectionItem.Delete(); // thinking this should keep the row just change the linked review to -1??
                    reviewCollectionItem = reviewCollectionItem.Save();
                }
                else
                {
                    if (deleteReviewLink && reviewCollectionItem == null)
                    {
                        return Ok();
                    }
                    reviewCollectionItem.ApiKey = zoteroApiKey;
                    reviewCollectionItem.LibraryID = groupId;
                    reviewCollectionItem.USER_ID = ri.UserId;
                    reviewCollectionItem.REVIEW_ID = ri.ReviewId;
                    reviewCollectionItem.DateCreated = DateTime.Now;
                    reviewCollectionItem.GroupBeingSynced = Convert.ToInt32(groupId);

                    DataPortal<ZoteroReviewCollection> dpUpdate = new DataPortal<ZoteroReviewCollection>();
                    reviewCollectionItem = reviewCollectionItem.Save();

                    return Ok(reviewCollectionItem);
                }

                return Ok(reviewCollection);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "UpdateGroupToReview has an error");
                return StatusCode(500, e.Message);
            }
        }



        [HttpPost("[action]")]
        public async Task<IActionResult> GroupId([FromBody] string groupId)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");

                var apiKey = await ApiKey();
                var zoteroApiKey = apiKey?.Value?.ToString();
                var result = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string currentGroupID);
                if (result)
                {
                    _zoteroConcurrentDictionary.Session.TryUpdate("groupIDBeingSynced-" + zoteroApiKey, groupId, currentGroupID);
                }
                else
                {
                    _zoteroConcurrentDictionary.Session.TryAdd("groupIDBeingSynced-" + zoteroApiKey, groupId);
                }
                var dp = new DataPortal<ZoteroReviewCollectionList>();
                var criteria =
                       new SingleCriteria<ZoteroReviewCollectionList, long>(ri.ReviewId);
                var reviewCollection = await dp.FetchAsync(criteria);
                var reviewCollectionItem = reviewCollection.FirstOrDefault();
                if (reviewCollectionItem != null)
                {
                    reviewCollectionItem.GroupBeingSynced = Convert.ToInt32(groupId);
                    reviewCollectionItem = reviewCollectionItem.Save();
                }
                else
                {
                    throw new ArgumentNullException("There is no apiKey and group for this review!");
                }

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Posting a groupId has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Collection([FromBody] string payload)
        {
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                var apiKey = await ApiKey();
                var zoteroApiKey = apiKey?.Value?.ToString();
                var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string groupIDBeingSynced);
                if (!groupIDResult) return StatusCode(500, "Concurrent Zotero session error");

                UriBuilder GetCollectionsUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/collections");
                SetZoteroHttpService(GetCollectionsUri, zoteroApiKey);
                payload = "[{\"name\" : \"My Collection Test 2\"}]"; // TODO hardcoded remove when ready

                var response = await _zoteroService.CollectionPost(payload, GetCollectionsUri.ToString());

                return Ok();

            }
            catch (Exception e)
            {
                _logger.LogException(e, "Collection post has an error");
                var message = "";
                if (e.Message.Contains("403"))
                {
                    message += "No Zotero API Token; either it has been revoked or never created";
                }
                else
                {
                    message += e.Message;
                }
                return StatusCode(500, message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GroupMember([FromQuery] string groupId)
        {
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");

                var apiKey = await ApiKey();
                var zoteroApiKey = apiKey?.Value?.ToString();
                var GetKeyUri = new UriBuilder($"{baseUrl}/groups/{groupId}/settings/members");
                SetZoteroHttpService(GetKeyUri, zoteroApiKey);
                var responseString = await _zoteroService.GetUserPermissions(GetKeyUri.ToString());

                return Ok(responseString);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching Zotero user permissions has an error");
                var message = "";
                if (e.Message.Contains("403"))
                {
                    message += "No Zotero API Token; either it has been revoked or never created";
                }
                else
                {
                    message += e.Message;
                }
                return StatusCode(500, message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> UserPermissions()
        {
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");

                var apiKey = await ApiKey();
                var zoteroApiKey = apiKey?.Value?.ToString();
                var GetKeyUri = new UriBuilder($"{baseUrl}/keys/current");
                SetZoteroHttpService(GetKeyUri, zoteroApiKey);
                var responseString = await _zoteroService.GetUserPermissions(GetKeyUri.ToString());

                return Ok(responseString);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching Zotero user permissions has an error");
                var message = "";
                if (e.Message.Contains("403"))
                {
                    message += "No Zotero API Token; either it has been revoked or never created";
                }
                else
                {
                    message += e.Message;
                }
                return StatusCode(500, message);
            }
        }

        [HttpGet("[action]")]
        public async Task<List<Group>> GroupMetaData([FromQuery] string zoteroUserId)
        {
            try
			{
                if (string.IsNullOrEmpty(zoteroUserId) || zoteroUserId == "undefined")
                {
                    throw new ArgumentNullException("zoteroUserId needs to be populated");
                }
				if (!SetCSLAUser4Writing()) return new List<Group>();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
				if (ri == null) throw new ArgumentNullException("Not sure why this is null");

				return await GetGroups(zoteroUserId, ri.ReviewId.ToString());
			}
			catch (Exception e)
            {
                _logger.LogException(e, "Fetching GroupMetaDataAsync has an error");
                return new List<Group>();
            }
        }

		private async Task<List<Group>> GetGroups(string zoteroUserId, string reviewId)
		{
			var getKeyResult = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + reviewId, out string zoteroApiKey);
			var GETGroupsUri = new UriBuilder($"{baseUrl}/users/{zoteroUserId}/groups");
			SetZoteroHttpService(GETGroupsUri, zoteroApiKey);
			var groups = await _zoteroService.GetCollections<Group>(GETGroupsUri.ToString());
			if (groups.Count > 0)
			{
				var groupIDBeingSynced = groups.FirstOrDefault().id.ToString();
				_zoteroConcurrentDictionary.Session.TryAdd("groupIDBeingSynced-" + zoteroApiKey, groupIDBeingSynced);
			}
			else
			{
				// TODO remove everything from dictionary and Database and tell user no groups have been set so revoking

			}

			// need to fetch the group being synced from the db and then find that group in the list that is returned!!
			var dp = new DataPortal<ZoteroReviewCollectionList>();
			var criteria =
				   new SingleCriteria<ZoteroReviewCollectionList, long>(Convert.ToInt64(reviewId));
			var reviewCollection = await dp.FetchAsync(criteria);
			var reviewCollectionItem = reviewCollection.FirstOrDefault();
			if (reviewCollectionItem != null)
			{
				var groupIDBeingSynced = reviewCollectionItem.GroupBeingSynced;
				var group = groups.FirstOrDefault(x => x.id == groupIDBeingSynced);
				if (group != null)
				{
					group.groupBeingSynced = groupIDBeingSynced;
				}
			}

			return groups;
		}

		[HttpGet("[action]")]
        public async Task<ApiKey[]> FetchApiKeys()
        {
            try
            {
                if (!SetCSLAUser()) return new ApiKey[] { };
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                
                var getKeyResult = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + ri.ReviewId.ToString(), out string zoteroApiKey);
                var GETApiKeysUri = new UriBuilder($"{baseUrl}/keys/{zoteroApiKey}");
                SetZoteroHttpService(GETApiKeysUri, zoteroApiKey);
                var key = await _zoteroService.GetApiKey(GETApiKeysUri.ToString());
                return new ApiKey[] { key };
            }
            catch (Exception e)
            {
                _logger.LogException(e, "FetchApiKeys has an error");
                return new ApiKey[] { };
            }
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteZoteroApiKey()
        {
            try
            {
                if (!SetCSLAUser()) return Ok(false);
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                var getKeyResult = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + ri.ReviewId.ToString(), out string zoteroApiKey);
                var DELETEApiKeysUri = new UriBuilder($"{baseUrl}/keys/{zoteroApiKey}");
                SetZoteroHttpService(DELETEApiKeysUri, zoteroApiKey);
                var result = await _zoteroService.DeleteApiKey(DELETEApiKeysUri.ToString());

                // if it is deleted from Zotero then it needs to be deleted locally also!!
                if (result)
                {
                    DataPortal<ZoteroReviewCollectionList> dp = new DataPortal<ZoteroReviewCollectionList>();

                    SingleCriteria<ZoteroReviewCollectionList, long> criteria =
                            new SingleCriteria<ZoteroReviewCollectionList, long>(ri.ReviewId);
                    var reviewCollection = await dp.FetchAsync(criteria);


                    var reviewCollectionItems = reviewCollection.Where(x => x.ApiKey == zoteroApiKey);
                    for (int i = 0; i < reviewCollectionItems.Count(); i++)
                    {
                        var item = reviewCollectionItems.ElementAt(i);
                        item.Delete();
                        item = item.Save();
                    }
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Delete Zotero Api Key has an error");
                return Ok(false);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Items()
        {
            try
            {
                if (SetCSLAUser4Writing())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                    var apiKey = await ApiKey();
                    var zoteroApiKey = apiKey?.Value?.ToString();
                    var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey,
                        out string groupIDBeingSynced);
                    if (!groupIDResult) return StatusCode(500, "Concurrent Zotero session error");
                    var GETGroupsUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items?sort=title");
                    SetZoteroHttpService(GETGroupsUri, zoteroApiKey);
                    var items = await _zoteroService.GetPagedCollections<object>(GETGroupsUri.ToString());
                    return Ok(items);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "FetchZoteroObjects list has an error");
                var message = "";
                if (e.Message.Contains("403"))
                {
                    message += "No Zotero API Token; either it has been revoked or never created";
                }
                else
                {
                    message += e.Message;
                }
                return StatusCode(500, message);
            }
        }

        [HttpGet("[action]")]
        public async Task<Collection> ItemsItemId([FromQuery] string itemKey)
        {
            Collection item = new Collection();
            try
            {

                if (SetCSLAUser())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                    var apiKey = await ApiKey();
                    var zoteroApiKey = apiKey?.Value?.ToString();
                    var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string groupIDBeingSynced);
                    if (!groupIDResult) return item;
                    var GETItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + itemKey);
                    SetZoteroHttpService(GETItemUri, zoteroApiKey);
                    item = await _zoteroService.GetCollectionItem(GETItemUri.ToString());
                    return item;

                }
                else return new Collection();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "FetchZoteroObjects list has an error");
                return item;
            }
        }

        [HttpGet("[action]")]
        public async Task<JsonResult> ApiKey([FromQuery] int groupId = -1, [FromQuery] bool deleteApiKey = false)
        {
            try
            {
                if (!SetCSLAUser())
                {
                    var error = new JsonErrorModel
                    {
                        ErrorCode = 403,
                        ErrorMessage = "Forbidden"
                    };
                    return Json(error);
                }
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                UserDetails userDetails = new UserDetails
                {
                    reviewId = ri.ReviewId,
                    userId = ri.UserId
                };

                SingleCriteria<ZoteroReviewCollectionList, long> criteria =
                        new SingleCriteria<ZoteroReviewCollectionList, long>(userDetails.reviewId);

                DataPortal<ZoteroReviewCollectionList> dp = new DataPortal<ZoteroReviewCollectionList>();

                var reviewCollection = await dp.FetchAsync(criteria);

                ZoteroReviewCollection reviewCollectionItem;
                if (groupId == -1)
                {
                    reviewCollectionItem = reviewCollection.FirstOrDefault(x => x.REVIEW_ID == ri.ReviewId);
                }
                else
                {
                    reviewCollectionItem = reviewCollection.FirstOrDefault(x => x.REVIEW_ID == ri.ReviewId && x.LibraryID == groupId.ToString());
                }

                if (deleteApiKey && reviewCollection.Count > 0 && reviewCollectionItem != null)
                {
                    reviewCollectionItem.Delete();
                    reviewCollectionItem = reviewCollectionItem.Save();
                    return Json("DeletedApiKey");
                }

                var result = await dp.FetchAsync(criteria);
                if (result == null || result.Count() == 0)
                {
                    //               var Result = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + reviewID, out string apiKeyOutFirst);
                    //if (Result)
                    //{
                    //                   return Json(apiKeyOutFirst);
                    //}
                    //else
                    //{
                    return Json("");
                    //}
                }
                if (string.IsNullOrEmpty(result.FirstOrDefault().ApiKey))
                {
                    var error = new JsonErrorModel
                    {
                        ErrorCode = 403,
                        ErrorMessage = "Forbidden"
                    };
                    return Json(error);
                }
                if (ri.ReviewId != result.FirstOrDefault().REVIEW_ID)
                {
                    var error = new JsonErrorModel
                    {
                        ErrorCode = 403,
                        ErrorMessage = "Forbidden"
                    };
                    return Json(error);
                }
                var apiOutResult = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + ri.ReviewId, out string apiKeyOut);
                if (!string.IsNullOrEmpty(result.FirstOrDefault().ApiKey) && !apiOutResult)
                {
                    _zoteroConcurrentDictionary.Session.TryAdd("apiKey-" + ri.ReviewId, result.FirstOrDefault().ApiKey);
                }
                _zoteroConcurrentDictionary.Session.TryAdd("reviewID", ri.ReviewId.ToString());

                // TODO create an object here to bring back both strings that are required
                //result.ApiKey, result.LibraryID
                return Json(result.FirstOrDefault().ApiKey);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Get Zotero ApiKey has an error");
                var error = new JsonErrorModel
                {
                    ErrorCode = 500,
                    ErrorMessage = e.Message
                };

                return Json(error);
            }
        }


        [HttpGet("[action]")]
        public async Task<JsonResult> CheckApiKey([FromQuery] int groupId = -1, [FromQuery] bool deleteApiKey = false)
        {
            try
            {
                if (!SetCSLAUser())
                {
                    var error = new JsonErrorModel
                    {
                        ErrorCode = 403,
                        ErrorMessage = "Forbidden"
                    };
                    return Json(error);
                }
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                UserDetails userDetails = new UserDetails
                {
                    reviewId = ri.ReviewId,
                    userId = ri.UserId
                };

                SingleCriteria<ZoteroReviewCollectionList, long> criteria =
                        new SingleCriteria<ZoteroReviewCollectionList, long>(userDetails.reviewId);

                DataPortal<ZoteroReviewCollectionList> dp = new DataPortal<ZoteroReviewCollectionList>();

                var reviewCollection = await dp.FetchAsync(criteria);

                ZoteroReviewCollection reviewCollectionItem;
                if (groupId == -1)
                {
                    reviewCollectionItem = reviewCollection.FirstOrDefault(x => x.REVIEW_ID == ri.ReviewId);
                }
                else
                {
                    reviewCollectionItem = reviewCollection.FirstOrDefault(x => x.REVIEW_ID == ri.ReviewId && x.LibraryID == groupId.ToString());
                }

                if (deleteApiKey && reviewCollection.Count > 0 && reviewCollectionItem != null)
                {
                    reviewCollectionItem.Delete();
                    reviewCollectionItem = reviewCollectionItem.Save();
                    return Json("DeletedApiKey");
                }

                var result = await dp.FetchAsync(criteria);
                if (result == null || result.Count() == 0)
                {                    
                    return Json("");
                }
                if (string.IsNullOrEmpty(result.FirstOrDefault().ApiKey))
                {
                    var error = new JsonErrorModel
                    {
                        ErrorCode = 403,
                        ErrorMessage = "Forbidden"
                    };
                    return Json(error);
                }
                if (ri.ReviewId != result.FirstOrDefault().REVIEW_ID)
                {
                    var error = new JsonErrorModel
                    {
                        ErrorCode = 403,
                        ErrorMessage = "Forbidden"
                    };
                    return Json(error);
                }
                var apiOutResult = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + ri.ReviewId, out string apiKeyOut);
                if (!string.IsNullOrEmpty(result.FirstOrDefault().ApiKey) && !apiOutResult)
                {
                    _zoteroConcurrentDictionary.Session.TryAdd("apiKey-" + ri.ReviewId, result.FirstOrDefault().ApiKey);
                }
                _zoteroConcurrentDictionary.Session.TryAdd("reviewID", ri.ReviewId.ToString());

                return Json(true);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Get Zotero ApiKey has an error");
                var error = new JsonErrorModel
                {
                    ErrorCode = 500,
                    ErrorMessage = e.Message
                };

                return Json(error);
            }
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> ItemKeyVersionLocal([FromQuery] string ItemKey, string ItemType)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                if (ItemType == "attachment") return Ok();

                // TODO For now just ignore getting the version of the attachment
                DataPortal<ZoteroReviewItem> dp = new DataPortal<ZoteroReviewItem>();
                SingleCriteria<ZoteroReviewItem, string> criteria =
                    new SingleCriteria<ZoteroReviewItem, string>(ItemKey);

                var result = await dp.FetchAsync(criteria);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Get a Version Of the Item In ErWeb has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ItemsItemKey([FromQuery] string itemKey)
        {
            try
            {
                if (SetCSLAUser())
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                    var apiKey = await ApiKey();
                    var zoteroApiKey = apiKey?.Value?.ToString();
                    var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string groupIDBeingSynced);
                    if (!groupIDResult) return StatusCode(500, "Concurrent Zotero session error");
                    var GETItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + itemKey + "");
                    SetZoteroHttpService(GETItemUri, zoteroApiKey);
                    JObject item = await _zoteroService.GetItem(GETItemUri.ToString());
                    return Ok(item);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ItemsItemKeyGet has an error");
                var message = "";
                if (e.Message.Contains("403"))
                {
                    message += "No Zotero API Token; either it has been revoked or never created";
                }
                else
                {
                    message += e.Message;
                }
                return StatusCode(500, message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ItemIdLocal([FromQuery] string itemReviewID)
        {
            try
            {
                if (string.IsNullOrEmpty(itemReviewID)) return Ok();

                if (SetCSLAUser())
                {
                    DataPortal<ZoteroItemIDPerItemReview> dp = new DataPortal<ZoteroItemIDPerItemReview>();
                    SingleCriteria<long> criteria =
                            new SingleCriteria<long>(Convert.ToInt64(itemReviewID));

                    var resultItemID = await dp.FetchAsync(criteria);
                    return Ok(resultItemID);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ItemIdLocalGet error");
                return StatusCode(500, e.Message);
            }

        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ItemReviewIds([FromQuery] string itemIds)
        {
            try
            {
                if (string.IsNullOrEmpty(itemIds)) return Ok();

                if (SetCSLAUser4Writing())
                {

                    //    // TODO...!
                    //    // Need to check if they are in Zotero local Table or not
                    //    // if they are do nothing as the meta check will pick them up
                    //    // If not we will need to push these items, so build
                    //    // zoterItem objects list and get ready to post to Zotero
                    //    // the post is tricky...


                    // TODO instead collect the top ten for demo purposes
                    DataPortal<ZoteroItemReviewIDs> dp = new DataPortal<ZoteroItemReviewIDs>();
                    SingleCriteria<string> criteria =
                            new SingleCriteria<string>(itemIds);

                    var result = await dp.FetchAsync(criteria);
                    var topTenItemReviewIDs = result.Take(10).Select(x => x.ITEM_REVIEW_ID).ToList();
                    return Ok(topTenItemReviewIDs);
                    //var testReviewIDS = result.Where( x=> x.ITEM_DOCUMENT_ID != 0).Take(10).ToList();
                    //return Ok(testReviewIDS);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ItemReviewIdsGet has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ItemsERWebAndZotero()
        {
            try
            {
                if (SetCSLAUser4Writing())
                {

                    DataPortal<ZoteroERWebReviewItemList> dp = new DataPortal<ZoteroERWebReviewItemList>();
                    var result = await dp.FetchAsync();
                    return Ok(result);

                }
                else return Forbid();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ItemsERWebAndZoteroGet has an error");
                return StatusCode(500, e.Message);
            }
        }

        // 3 TODO remove direct database code logic leave to find out how to do a rollback with CSLA here
        [HttpPost("[action]")]
        public async Task<IActionResult> ItemsItemsIdLocal([FromBody] Collection collection)
        {
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();

                if (collection == null || collection.data == null) return Ok();

                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

                var receivedZoteroItem = collection.data;
                bool updateLocalVersion = false;
                ZoteroReviewItem zoteroItem = new ZoteroReviewItem();
                DataPortal<ZoteroReviewItem> dpFetch = new DataPortal<ZoteroReviewItem>();

                if (!string.IsNullOrEmpty(receivedZoteroItem.key))
                {

                    SingleCriteria<ZoteroReviewItem, string> criteriaZoteroItem =
                   new SingleCriteria<ZoteroReviewItem, string>(receivedZoteroItem.key);

                    zoteroItem = dpFetch.Fetch(criteriaZoteroItem);
                    if (zoteroItem.ITEM_REVIEW_ID > 0)
                    {
                        updateLocalVersion = true;
                    }
                }
                else
                {
                    return Ok();
                }

                long itemId = 0;
                using (SqlConnection connection = new SqlConnection(AdmConnStr))
                {
                    connection.Open();

                    try
                    {
                        using (var ctx = TransactionManager<SqlConnection, SqlTransaction>.GetManager(AdmConnStr, false))
                        {

                            DataPortal<ZoteroItemIDByItemReview> dp = new DataPortal<ZoteroItemIDByItemReview>();
                            ZoteroItemIDByItemReviewCriteria criteria =
                                    new ZoteroItemIDByItemReviewCriteria(zoteroItem.ITEM_REVIEW_ID, ri.ReviewId);

                            var resultItemID = await dp.FetchAsync(criteria);

                            DataPortal<Item> dpFetchItem = new DataPortal<Item>();
                            SingleCriteria<Item, Int64> criteriaItem =
                                new SingleCriteria<Item, long>(itemId);

                            var itemFetch = dpFetchItem.Fetch(criteriaItem);

                            await UpdateMiddleAndLeftTableItem(collection);

                            // TODO check update has happened as expected
                            itemFetch = dpFetchItem.Execute(itemFetch);

                            zoteroItem.ItemKey = receivedZoteroItem.key;
                            zoteroItem.ITEM_REVIEW_ID = zoteroItem.ITEM_REVIEW_ID;
                            zoteroItem.LAST_MODIFIED = DateTime.Now;
                            zoteroItem.LibraryID = collection.library.id.ToString();
                            zoteroItem.Version = receivedZoteroItem.version.ToString();

                            zoteroItem = dpFetch.Update(zoteroItem);
                            ctx.Commit();
                        }

                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e, "Error with ItemsItemsIdLocalPost");
                        return StatusCode(500, e.Message);
                    }
                }
                return Ok(zoteroItem);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ItemsItemsIdLocalPost has an error");
                return StatusCode(500, e.Message);
            }
        }

        private int MapFromZoteroTypeToERWebTypeID(string zoteroItemType)
        {
            int erWebTypeId = -1;
            
//1   Report
//2   Book, Whole
//3   Book, Chapter
//4   Dissertation
//5   Conference Proceedings
//6   Document From Internet Site
//7   Web Site
//8   DVD, Video, Media
//9   Research project
//10  Article In A Periodical
//11  Interview
//12  Generic
//14  Journal, Article
            switch (zoteroItemType)
            {
                case "journalArticle":
                    erWebTypeId = 14;
                    break;
                case "attachment":
                    erWebTypeId = 12;
                    break;
                default:
                    // code block
                    break;
            }
            return erWebTypeId;

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ItemsLocal([FromBody] Collection[] collectionItems)
        {
            try
            {

                if (!SetCSLAUser4Writing()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                int countItemsToBeInserted = 0;

                for (int j = 0; j < collectionItems.Length; j++)
                {
                    var collectionItem = collectionItems[j];
                    DataPortal<ZoteroReviewItem> dpRI = new DataPortal<ZoteroReviewItem>();
                    SingleCriteria<ZoteroReviewItem, string> criteria =
                        new SingleCriteria<ZoteroReviewItem, string>(collectionItem.data.key);

                    var zoteroReviewItem = dpRI.Fetch(criteria);

                    // Means we already have a version of it
                    if (zoteroReviewItem.Zotero_item_review_ID != 0)
                    {
                        var localModifiedDate = zoteroReviewItem.LAST_MODIFIED.ToUniversalTime();
                        var zoteroModifiedDate = DateTime.Parse(collectionItem.data.dateModified).ToUniversalTime();
                        // check if it is the same version
                        if (localModifiedDate == zoteroModifiedDate)
                        {
                            return Ok();
                        }
                        else
                        {
                            // TODO Need to update middle table       
                            await UpdateMiddleAndLeftTableItem(collectionItem);
                            continue;
                        }
                    }
                    else
                    {
                        countItemsToBeInserted++;
                    }
                }

                if(countItemsToBeInserted == 0)
                {
                    return Ok(true);
                }

                var forSaving = new IncomingItemsList();
                var incomingItems = new MobileList<ItemIncomingData>();
                for (int j = 0; j < collectionItems.Length; j++)
                {
                    var collectionItem = collectionItems[j];
                    ItemIncomingData itemIncomingData = new ItemIncomingData
                    {
                        Abstract = collectionItem.data.abstractNote ?? "",
                        Year = collectionItem.data.date ?? "0",
                        Title = collectionItem.data.title,
                        Short_title = collectionItem.data.shortTitle ?? "",
                        Type_id = MapFromZoteroTypeToERWebTypeID(collectionItem.data.itemType),
                        AuthorsLi = new AuthorsHandling.AutorsList(),
                        pAuthorsLi = new MobileList<AuthorsHandling.AutH>(),
                    };
                    incomingItems.Add(itemIncomingData);
                }

                forSaving = new IncomingItemsList
                {
                    FilterID = 0,
                    SourceName = "Zotero_" + DateTime.Now,
                    SourceDB = "Zotero",
                    DateOfImport = DateTime.Now,
                    DateOfSearch = DateTime.Now,
                    Included = true,
                    Notes = "TestZoteroSource",
                    SearchDescr = "TestZoteroSource",
                    SearchStr = "TestZoteroSource",
                    IncomingItems = incomingItems
                };
                forSaving.buildShortTitles();
                forSaving = forSaving.Save();


                var itemReviewIdsInserted = new List<long>();

                var dpZoteroItemIdsPerSource = new DataPortal<ZoteroItemSourceList>();
                var criteriaSource = new SingleCriteria<ZoteroItemSourceList, Int32>(forSaving.SourceID);
                var itemsInserted = dpZoteroItemIdsPerSource.Fetch(criteriaSource);

                for (int i = 0; i < collectionItems.Length; i++)
                {
                    var collection = collectionItems[i];
                    if (collection == null || collection.data == null) return Ok(false);
                    DataPortal<ZoteroReviewItem> dpRI = new DataPortal<ZoteroReviewItem>();
                    SingleCriteria<ZoteroReviewItem, string> criteria =
                        new SingleCriteria<ZoteroReviewItem, string>(collection.data.key);

                    var zoteroReviewItem = dpRI.Fetch(criteria);
                    if (zoteroReviewItem.Zotero_item_review_ID != 0)
                    {
                        var localModifiedDate = zoteroReviewItem.LAST_MODIFIED.ToUniversalTime();
                        var zoteroModifiedDate = DateTime.Parse(collection.data.dateModified).ToUniversalTime();
                        // check if it is the same version
                        if (localModifiedDate == zoteroModifiedDate)
                        {
                            return Ok(true);
                        }
                    }

                    var apiKey = await ApiKey();
                    var zoteroApiKey = apiKey?.Value?.ToString();
                    var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string groupIDBeingSynced);
                    if (!groupIDResult) throw new Exception("Concurrent Zotero session error");

                    var key = collection.key;
                    var version = collection.version;
                    // This pulls a straight attachment
                    if (collection.data.itemType == "attachment" || collection.links.attachment != null)
                    {
                        string parentKey = "";

                        if (collection.data.itemType == "attachment")
                        {
                            parentKey = collection.data.parentItem;
                        }
                        else
                        {
                            parentKey = collection.key;
                            var index = collection.links.attachment.href.LastIndexOf('/');
                            var lengthOfAttachmentStr = collection.links.attachment.href.Length - index;
                            key = collection.links.attachment.href.Substring(index + 1, lengthOfAttachmentStr - 1);
                        }

                        DataPortal<ZoteroReviewItem> dpCheckParent = new DataPortal<ZoteroReviewItem>();
                        SingleCriteria<ZoteroReviewItem, string> criteriaCheckParent =
                            new SingleCriteria<ZoteroReviewItem, string>(parentKey);

                        if (string.IsNullOrEmpty(parentKey))
                            throw new NotImplementedException("Only attachments with a parent can be imported currently");

                        var parentItem = dpCheckParent.Fetch(criteriaCheckParent);

                        if (parentItem.ITEM_REVIEW_ID == 0)
                        {
                            //need to insert the parent here then continue with the child attachment
                            ZoteroReviewItem zoteroItemParent = new ZoteroReviewItem();
                            IMapZoteroReference referenceParent = _concreteReferenceCreator.GetReference(collection);
                            var erWebItemParent = referenceParent.MapReferenceFromZoteroToErWeb(new Item());
                            erWebItemParent.Item.IsIncluded = true;
                            parentItem.ITEM_REVIEW_ID = itemsInserted[i].ITEM_REVIEW_ID;
                            zoteroItemParent = new ZoteroReviewItem
                            {
                                ItemKey = parentKey,
                                ITEM_REVIEW_ID = parentItem.ITEM_REVIEW_ID,
                                LAST_MODIFIED = DateTime.Now,
                                LibraryID = collection.library.id.ToString(),
                                Version = version.ToString(),
                                TypeName = erWebItemParent.Item.TypeName
                            };

                            DataPortal<ZoteroReviewItem> dpParent = new DataPortal<ZoteroReviewItem>();
                            zoteroItemParent = dpParent.Execute(zoteroItemParent);

                            //return Ok("Parent Item not yet inserted into ERWeb");
                        }

                        DataPortal<ZoteroItemReview> dpItemReview = new DataPortal<ZoteroItemReview>();
                        SingleCriteria<ZoteroItemReview, long> criteriaItemReview =
                            new SingleCriteria<ZoteroItemReview, long>(parentItem.ITEM_REVIEW_ID);
                        var parentItemID = dpItemReview.Fetch(criteriaItemReview);

                        string fileName = "";
                        if(collection.links.attachment != null)
                        {
                            //get filename before downloading the file
                            var GetFileNameUri = new UriBuilder(collection.links.attachment.href);
                            SetZoteroHttpService(GetFileNameUri, zoteroApiKey);
                            var fileNameResponse = await _zoteroService.GetCollectionItem(GetFileNameUri.ToString());
                            fileName = fileNameResponse.data.title;
                        }
                        else
                        {
                            fileName = collection.data.title;//fileNameResponse.data.title;
                        }                                               

                        var GetFileUri = new UriBuilder($"{baseUrl}/groups/{ groupIDBeingSynced}/items/{key}/file");
                        SetZoteroHttpService(GetFileUri, zoteroApiKey);

                        //act
                        var response = await _zoteroService.GetDocumentHeader(GetFileUri.ToString());
                        var lastModifiedDate = response.Content.Headers.GetValues("Last-Modified").FirstOrDefault();
                        var fileStream = await response.Content.ReadAsStreamAsync();
                        
                        int ind = fileName.LastIndexOf(".");
                        string ext = fileName.Substring(ind);
                        Stream stream = fileStream;
                        byte[] Binary = new byte[stream.Length];
                        stream.Read(Binary, 0, (int)stream.Length);
                        if (ext.ToLower() == ".txt")
                        {
                            string SimpleText = System.Text.Encoding.UTF8.GetString(Binary);
                            ItemDocumentSaveCommand cmd = new ItemDocumentSaveCommand(parentItemID.ITEM_ID,
                                fileName,
                                ext,
                                SimpleText
                                );
                            cmd.doItNow();
                        }
                        else
                        {
                            ItemDocumentSaveBinCommand cmd = new ItemDocumentSaveBinCommand(parentItemID.ITEM_ID,
                                fileName,
                                ext,
                                Binary
                                );
                            cmd.doItNow();
                        }
                        continue;
                    }
                                      


                    ZoteroReviewItem zoteroItem = new ZoteroReviewItem();
                    IMapZoteroReference reference = _concreteReferenceCreator.GetReference(collection);

                    var erWebItem = reference.MapReferenceFromZoteroToErWeb(new Item());
                    erWebItem.Item.IsIncluded = true;

                    if (key.Length > 0)
                    {
                        zoteroItem = new ZoteroReviewItem
                        {
                            ItemKey = key,
                            ITEM_REVIEW_ID = itemsInserted[i].ITEM_REVIEW_ID,
                            LAST_MODIFIED = DateTime.Now,
                            LibraryID = collection.library.id.ToString(),
                            Version = version.ToString(),
                            TypeName = erWebItem.Item.TypeName
                        };

                        DataPortal<ZoteroReviewItem> dp = new DataPortal<ZoteroReviewItem>();
                        zoteroItem = dp.Execute(zoteroItem);

                        //This pulls a parent item's attachment
                        if (collection.links.attachment != null)
                        {
                            // need to call the attachment data from Zotero
                            var IndexLastSlash = collection.links.attachment.href.LastIndexOf('/');
                            var keyLength = collection.links.attachment.href.Length - IndexLastSlash;
                            var attachmentKey = collection.links.attachment.href.Substring(IndexLastSlash + 1, keyLength - 1);
                            var result = await this.ItemsItemKey(attachmentKey);

                            var resultCollection = result as OkObjectResult;
                            var attachmentCollection = JsonConvert.DeserializeObject<Collection>(resultCollection.Value.ToString());

                            var parentKey = attachmentCollection.data.parentItem;
                            DataPortal<ZoteroReviewItem> dpCheckParent = new DataPortal<ZoteroReviewItem>();
                            SingleCriteria<ZoteroReviewItem, string> criteriaCheckParent =
                                new SingleCriteria<ZoteroReviewItem, string>(parentKey);

                            if (string.IsNullOrEmpty(parentKey)) throw new NotImplementedException("Only attachments with a parent can be imported currently");

                            var parentItem = dpCheckParent.Fetch(criteriaCheckParent);

                            if (parentItem.ITEM_REVIEW_ID == 0)
                            {
                                //TODO Later do smarter logic 
                                return Ok("Parent Item not yet inserted into ERWeb");
                            }

                            DataPortal<ZoteroItemReview> dpItemReview = new DataPortal<ZoteroItemReview>();
                            SingleCriteria<ZoteroItemReview, long> criteriaItemReview =
                                new SingleCriteria<ZoteroItemReview, long>(parentItem.ITEM_REVIEW_ID);
                            var parentItemID = dpItemReview.Fetch(criteriaItemReview);

                            var GetFileUri = new UriBuilder($"{baseUrl}/groups/{ groupIDBeingSynced}/items/{attachmentCollection.data.key}/file");
                            SetZoteroHttpService(GetFileUri, zoteroApiKey);

                            //act
                            var response = await _zoteroService.GetDocumentHeader(GetFileUri.ToString());
                            var lastModifiedDate = response.Content.Headers.GetValues("Last-Modified").FirstOrDefault();
                            var fileStream = await response.Content.ReadAsStreamAsync();
                            var fileName = attachmentCollection.data.title;
                            int ind = fileName.LastIndexOf(".");
                            string ext = fileName.Substring(ind);
                            Stream stream = fileStream;
                            byte[] Binary = new byte[stream.Length];
                            stream.Read(Binary, 0, (int)stream.Length);
                            if (ext.ToLower() == ".txt")
                            {
                                string SimpleText = System.Text.Encoding.UTF8.GetString(Binary);
                                ItemDocumentSaveCommand cmd = new ItemDocumentSaveCommand(parentItemID.ITEM_ID,
                                    fileName,
                                    ext,
                                    SimpleText
                                    );
                                cmd.doItNow();
                            }
                            else
                            {
                                ItemDocumentSaveBinCommand cmd = new ItemDocumentSaveBinCommand(parentItemID.ITEM_ID,
                                    fileName,
                                    ext,
                                    Binary
                                    );
                                cmd.doItNow();
                            }
                            var zoteroItemPostAttachment = new ZoteroReviewItem
                            {
                                ItemKey = attachmentCollection.key,
                                ITEM_REVIEW_ID = parentItem.ITEM_REVIEW_ID,
                                LAST_MODIFIED = DateTime.Now,
                                LibraryID = attachmentCollection.library.id.ToString(),
                                Version = version.ToString(),
                                TypeName = "attachment" //TODO magic string for now
                            };

                            // TODO not sure what to do with the attachment insert it into the
                            // zoteroDoc table or simply into the ZoteroItemReview table for now...?
                            DataPortal<ZoteroReviewItem> dpf = new DataPortal<ZoteroReviewItem>();
                            zoteroItemPostAttachment = dpf.Execute(zoteroItemPostAttachment);
                            return Ok(true);
                        }
                    }
                    else
                    {
                        return Ok(true);
                    }

                }

                return Ok(true);

            }
            catch (Exception e)
            {
                _logger.LogException(e, "ItemsLocalPost has an error");
                return Ok(false);
            }
        }

        private async Task UpdateMiddleAndLeftTableItem(Collection collection)
        {
            DataPortal<ZoteroReviewItem> dpGetMiddleTableItem = new DataPortal<ZoteroReviewItem>();
            SingleCriteria<ZoteroReviewItem, string> criteriaUpdate =
              new SingleCriteria<ZoteroReviewItem, string>(collection.data.key);
            var result = await dpGetMiddleTableItem.FetchAsync(criteriaUpdate);

            DataPortal<ZoteroItemReview> dpGetItemId = new DataPortal<ZoteroItemReview>();
            SingleCriteria<ZoteroItemReview, Int64> criteriaItemId =
                new SingleCriteria<ZoteroItemReview, long>(result.ITEM_REVIEW_ID);
            var itemId = dpGetItemId.Fetch(criteriaItemId);

            DataPortal<Item> dpFetchItem = new DataPortal<Item>();
            SingleCriteria<Item, Int64> criteriaItem =
                new SingleCriteria<Item, long>(itemId.ITEM_ID);
            var itemFetch = dpFetchItem.Fetch(criteriaItem);

            IMapZoteroReference referenceUpdate = _concreteReferenceCreator.GetReference(collection);
            var erWebItemUpdate = referenceUpdate.MapReferenceFromZoteroToErWeb(itemFetch);

            result.LAST_MODIFIED = DateTime.Now;
            result.LibraryID = collection.library.id.ToString();
            result.Version = collection.version.ToString();
            result.TypeName = erWebItemUpdate.Item.TypeName;
            result = result.Save();
            erWebItemUpdate.Item = erWebItemUpdate.Item.Save();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ErWebDocumentExists([FromQuery] long itemReviewId)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<ZoteroItemReview> dpItemReview = new DataPortal<ZoteroItemReview>();
                SingleCriteria<ZoteroItemReview, long> criteriaItemReview =
                    new SingleCriteria<ZoteroItemReview, long>(itemReviewId);
                var parentItemID = dpItemReview.Fetch(criteriaItemReview);

                if (parentItemID.ITEM_ID > -1)
                {
                    DataPortal<ItemDocument> dpDoc = new DataPortal<ItemDocument>();
                    SingleCriteria<ItemDocument, long> criteriaDoc =
                        new SingleCriteria<ItemDocument, long>(parentItemID.ITEM_ID);
                    var doc = dpDoc.Fetch(criteriaDoc);

                    if (doc.ItemDocumentId > 0)
                    {
                        return Ok(true);
                    }
                    else
                    {
                        return Ok(false);
                    }
                }
                else
                {
                    // TODO error
                }

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ErWebDocumentExists has an error");
                return StatusCode(500, e.Message);
            }
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> ItemReviewIdsLocal()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");


                DataPortal<ZoteroReviewItemsNotInZotero> dp = new DataPortal<ZoteroReviewItemsNotInZotero>();
                SingleCriteria<Item, Int64> criteria =
                    new SingleCriteria<Item, long>(ri.ReviewId);
                var zoteroReviewItemsNotInZotero = dp.Fetch(criteria);

                List<iItemReviewID> itemReviewIDs = new List<iItemReviewID>();
                foreach (var item in zoteroReviewItemsNotInZotero)
                {
                    var itemReviewID = new iItemReviewID
                    {
                        itemID = item.ITEM_ID,
                        itemReviewID = item.ITEM_REVIEW_ID,
                        itemDocumentID = item.ITEM_DOCUMENT_ID
                    };
                    itemReviewIDs.Add(itemReviewID);

                }
                // TODO only ever do the first ten items in dev and staging only in production optimise for lots
                return Ok(itemReviewIDs);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "ItemReviewIdsLocalGet has an error");
                return StatusCode(500, e.Message);
            }
        }



        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteMiddleMan([FromQuery] string itemKey)
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                DataPortal<ZoteroReviewItem> dp = new DataPortal<ZoteroReviewItem>();
                SingleCriteria<ZoteroReviewItem, string> criteriaDelete =
                   new SingleCriteria<ZoteroReviewItem, string>(itemKey);
                var zoteroReviewItemsNotInZotero = dp.Fetch(criteriaDelete);

                zoteroReviewItemsNotInZotero.Delete();
                zoteroReviewItemsNotInZotero = zoteroReviewItemsNotInZotero.Save();

                return Ok(true);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "DeleteMiddleMan has an error");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GroupsGroupIdItems([FromBody] List<iItemReviewID> items)
        {
            List<Item> localItems = new List<Item>();
            List<CollectionData> zoteroItems = new List<CollectionData>();
            List<ErWebZoteroItemDocument> erWebZoteroItemDocs = new List<ErWebZoteroItemDocument>();

            var failedItemsMsg = "These items failed when posting to Zotero: ";
            var numberOfFailedItems = 0;

            if (items.Count == 0) throw new Exception("Items to push to Zotero is empty!");

            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();

                var apiKey = await ApiKey();
                var zoteroApiKey = apiKey?.Value?.ToString();
                var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string groupIDBeingSynced);
                if (!groupIDResult) return StatusCode(500, "Concurrent Zotero session error");

                // 0 -dataportal fetch for documents
                var itemIDsWithDocuments = items.Where(x => x.itemDocumentID > 0);

                // 1 - dataportal fetch for items
                var itemIDsWithoutDocuments = items.Where(x => x.itemDocumentID == 0);
                DataPortal<Item> dp = new DataPortal<Item>();

                foreach (var item in itemIDsWithoutDocuments)
                {
                    SingleCriteria<Item, Int64> criteria =
                        new SingleCriteria<Item, Int64>(item.itemID);

                    var itemResult = await dp.FetchAsync(criteria);
                    localItems.Add(itemResult);
                }

                var POSTItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/");
                SetZoteroHttpService(POSTItemUri, zoteroApiKey);

                // 2 - map this item to an object that can be a valid payload (difficult)
                var count = 0;
                HttpResponseMessage response = new HttpResponseMessage();
                foreach (var item in localItems)
                {
                    var erWebItem = new ERWebItem
                    {
                        Item = item
                    };
                    var zoteroReference = _concreteReferenceCreator.GetReference(erWebItem);
                    var zoteroItem = zoteroReference.MapReferenceFromErWebToZotero();
                    zoteroItems.Add(zoteroItem);
                }

                if (zoteroItems.Count() > 0)
                {
                    var payload = JsonConvert.SerializeObject(zoteroItems);

                    response = await _zoteroService.CreateItem(payload, POSTItemUri.ToString());
                    var actualContent = await response.Content.ReadAsStringAsync();
                    if (actualContent.Contains("success"))
                    {
                        JObject keyValues = JsonConvert.DeserializeObject<JObject>(actualContent);

                        numberOfFailedItems = keyValues["failed"].Count();
                        foreach (var item in keyValues["failed"].Children())
                        {
                            failedItemsMsg += item.FirstOrDefault()["message"];
                        }

                        var version = keyValues["successful"]["0"]["version"].ToString();
                        var libraryId = keyValues["successful"]["0"]["library"]["id"].ToString();

                        foreach (var item in keyValues["success"].Children())
                        {
                            List<object> values = new List<object>();
                            var key = "";
                            foreach (var keyJson in item.Children())
                            {
                                key = keyJson.ToString() ?? "";
                            }

                            zoteroItems[count].key = key;
                            var itemReviewID = items.FirstOrDefault(x => x.itemID == localItems[count].ItemId).itemReviewID;
                            var zoteroItemToInsert = new ZoteroReviewItem
                            {
                                ItemKey = key,
                                ITEM_REVIEW_ID = itemReviewID,
                                LAST_MODIFIED = DateTime.Now,
                                LibraryID = libraryId,
                                Version = version,
                                TypeName = localItems[count].TypeName
                            };

                            DataPortal<ZoteroReviewItem> dp2 = new DataPortal<ZoteroReviewItem>();
                            zoteroItemToInsert = dp2.Execute(zoteroItemToInsert);

                            count++;
                        }

                    }
                }

                //TODO index is incorrect somehow getting documents attacjed to wrong parents
                if (itemIDsWithDocuments.Count() > 0)
                {
                    // TODO advanced:  if the key is not present for the associated item to an itemDocument
                    // then find this item locally and push to Zotero 
                    // next get the key and continue as expected
                    // for some reason have to do this again it loses the context!!!
                    // TODO:  
                    if (!SetCSLAUser4Writing()) return Unauthorized();
                    zoteroItems.Clear();
                    int countItems = 0;
                    var itemIDsDistinctWithDocuments = itemIDsWithDocuments.Select(x => x.itemID).Distinct().ToList();
                    foreach (var itemId in itemIDsDistinctWithDocuments)
                    {
                        var itemDoc = itemIDsWithDocuments.FirstOrDefault(y => y.itemID == itemId);
                        DataPortal<Item> dpItem = new DataPortal<Item>();
                        SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, long>(itemDoc.itemID);
                        var item = dpItem.Fetch(criteria);
                        var erWebItem = new ERWebItem
                        {
                            Item = item
                        };
                        var zoteroReference = _concreteReferenceCreator.GetReference(erWebItem);
                        var zoteroItem = zoteroReference.MapReferenceFromErWebToZotero();
                        zoteroItems.Add(zoteroItem);
                        countItems++;
                    }

                    //PUSH items that have documents
                    var payload = JsonConvert.SerializeObject(zoteroItems);
                    count = 0;
                    response = await _zoteroService.CreateItem(payload, POSTItemUri.ToString());
                    var actualContent = await response.Content.ReadAsStringAsync();
                    if (actualContent.Contains("success"))
                    {
                        //TODO since this parent item is now in Zotero we need to insert
                        // into the ERWebZotero table as a record  
                        JObject keyValues = JsonConvert.DeserializeObject<JObject>(actualContent);

                        numberOfFailedItems = keyValues["failed"].Count();
                        foreach (var item in keyValues["failed"].Children())
                        {
                            failedItemsMsg += item.FirstOrDefault()["message"];
                        }

                        var version = keyValues["successful"]["0"]["version"].ToString();
                        var libraryId = keyValues["successful"]["0"]["library"]["id"].ToString();

                        foreach (var item in keyValues["success"].Children())
                        {
                            List<object> values = new List<object>();
                            var key = "";
                            foreach (var keyJson in item.Children())
                            {
                                key = keyJson.ToString() ?? "";
                            }
                            var itemWithDoc = itemIDsWithDocuments.FirstOrDefault(x => x.itemID == itemIDsDistinctWithDocuments[count]);
                            // okay what I am doing here is just uploading the first document associated with the item in question
                            // TODO advanced add all documents associated with an item...
                            // for now have to repeat process... of pushnig items...
                            if (itemWithDoc != null)
                            {
                                var itemDoc = new ErWebZoteroItemDocument
                                {
                                    itemId = itemWithDoc.itemID,
                                    parentItemFileKey = key,
                                    itemDocumentId = itemWithDoc.itemDocumentID
                                };
                                erWebZoteroItemDocs.Add(itemDoc);
                            }
                            zoteroItems[count].key = key;
                            var itemReviewID = items.FirstOrDefault(x => x.itemID == itemIDsWithDocuments.ToList()[count].itemID).itemReviewID;
                            var zoteroItemToInsert = new ZoteroReviewItem
                            {
                                ItemKey = key,
                                ITEM_REVIEW_ID = itemReviewID,
                                LAST_MODIFIED = DateTime.Now,
                                LibraryID = libraryId,
                                Version = version,
                                TypeName = "Journal, Article" //TODO magic string 
                            };
                            // for some reason have to do this again it loses the context!!!
                            // TODO:  
                            if (!SetCSLAUser4Writing()) return Unauthorized();
                            DataPortal<ZoteroReviewItem> dp2 = new DataPortal<ZoteroReviewItem>();
                            zoteroItemToInsert = dp2.Execute(zoteroItemToInsert);
                            count++;
                        }
                    }
                    if (erWebZoteroItemDocs.Count() > 0)
                    {
                        await UploadERWebDocumentsToZoteroAsync(erWebZoteroItemDocs, zoteroItems);
                    }
                }
                if (numberOfFailedItems == 0)
                {
                    return Ok("true");
                }
                else
                {
                    return Ok(failedItemsMsg);
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "UpdateZoteroObjectInERWebAsync has an error");
                var message = "";
                if (e.Message.Contains("403"))
                {
                    message += "No Zotero API Token; either it has been revoked or never created";
                }
                else
                {
                    message += e.Message;
                }
                return StatusCode(500, message);
            }
        }

        private async Task UploadERWebDocumentsToZoteroAsync(List<ErWebZoteroItemDocument> erWebZoteroItemDocs, List<CollectionData> zoteroItems)
        {
            var counter = 0;
            foreach (var itemDoc in erWebZoteroItemDocs)
            {
                SQLHelper sQLHelper = new SQLHelper(_configuration, _logger);
                SqlParameter DOC_ID = new SqlParameter("@DOC_ID", SqlDbType.Int);
                SqlParameter REV_ID = new SqlParameter("@REV_ID", SqlDbType.Int);

                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = DOC_ID;
                parameters[1] = REV_ID;

                try
                {

                    DOC_ID.Value = itemDoc.itemDocumentId;
                    _zoteroConcurrentDictionary.Session.TryGetValue("reviewID", out string reviewId);
                    REV_ID.Value = reviewId;

                    using (SqlConnection conn = new SqlConnection(sQLHelper.ER4DB))
                    {
                        conn.Open();

                        using (SqlDataReader dr = sQLHelper.ExecuteQuerySP(conn, "st_ItemDocumentBin", DOC_ID, REV_ID))
                        {

                            dr.Read();
                            // TODO CHANGE THIS AT THE END
                            if (!dr.HasRows) throw new Exception("");

                            string type = (string)dr["DOCUMENT_EXTENSION"];
                            string name = (string)dr["DOCUMENT_TITLE"];

                            name = System.Web.HttpUtility.UrlEncode(name.Replace(type, "") + type);
                            if (name.IndexOf(type) == -1) name = name + type;
                            byte[] stBytes;
                            if (type.ToLower() != ".txt")
                            {
                                stBytes = (byte[])dr["DOCUMENT_BINARY"];

                            }
                            else
                            {
                                stBytes = System.Text.Encoding.UTF8.GetBytes(dr["DOCUMENT_TEXT"].ToString());
                            }

                            var parentItemKey = itemDoc.parentItemFileKey;
                            var fileBytes = stBytes;

                            //2) Now upload to Zotero
                            await UploadFileBytesToZoteroAsync(fileBytes, parentItemKey, name, reviewId, itemDoc.itemDocumentId);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                }
                counter++;
            }
        }

        //    [HttpGet("[action]")]
        //    public async Task<IActionResult> Usersubscription(string userId, long reviewId)
        //    {
        //        try
        //        {
        //            if (SetCSLAUser4Writing())
        //            {
        //                var apiKey = await ApiKeyGet(Convert.ToInt64(reviewId), Convert.ToInt32(userId));
        //                var zoteroApiKey = apiKey.Value.ToString();
        //                UriBuilder GetUserSubscriptionUri = new UriBuilder($"{baseUrl}/storage/usersubscription");
        //                SetZoteroHttpService(GetUserSubscriptionUri, zoteroApiKey);
        //                var response = await _zoteroService.GetDocument(GetUserSubscriptionUri.ToString());

        //                return Ok();
        //            }
        //else
        //{
        //                return Forbid();
        //}
        //        }
        //        catch(Exception e)
        //        {
        //            _logger.LogException(e, "UpdateZoteroObjectInERWebAsync has an error");
        //            var message = "";
        //            if (e.Message.Contains("403"))
        //            {
        //                message += "No Zotero API Token; either it has been revoked or never created";
        //            }
        //            else
        //            {
        //                message += e.Message;
        //            }
        //            return StatusCode(500, message);
        //        }
        //    }

        private async Task UploadFileBytesToZoteroAsync(byte[] fileBytes, string fileKey, string name, string reviewId, long itemDocumentID)
        {
            try
            {

                string filename = name;

                Stream stream = new MemoryStream(fileBytes);
                var md5Content = ZoteroAPIHelpers.GetMD5HashFromStream(stream);
                var dt = DateTime.Now;

                string payload = "[ " +
                               "{" +
                                    " \"itemType\": \"attachment\", " +
                                     "\"parentItem\": \"" + fileKey + "\", " +
                                   "\"linkMode\": \"imported_file\", " +
                                   "\"title\": \"" + filename + "\"," +
                                   "\"accessDate\": \"2012-03-14T17:55:54Z\", " +
                                       "\"note\": \"\", " +
                                    " \"tags\": [], " +
                                     "\"collections\": [], " +
                                     " \"relations\": { }," +
                                      " \"contentType\": \"application/pdf\"," +
                                       "    \"charset\": \"\"," +
                                         "   \"filename\": \"" + filename + "\"," +
                                         "  \"md5\": null," +
                                          " \"mtime\": null" +
                                         "}" +
                                       "]";

                var getKeyResult = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + reviewId, out string zoteroApiKey);
                var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string groupIDBeingSynced);
                if (!groupIDResult) throw new Exception("Concurrent Zotero session error");
                var POSTItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/?v=3");
                SetZoteroHttpService(POSTItemUri, zoteroApiKey);

                var responseTwo = await _zoteroService.POSTJDocument(payload, POSTItemUri.ToString());

                var successful = responseTwo["successful"];
                var zero = successful["0"];
                var key = zero["key"];

                long filesize = fileBytes.Length;
                var hash = md5Content;

                var PDFAuthUri = new UriBuilder($"{baseUrl}/groups/{ groupIDBeingSynced}/items/{key}/file");
                SetZoteroHttpService(PDFAuthUri, zoteroApiKey, true);

                dt = DateTime.Now;
                long milliseconds = dt.Millisecond;

                var payload2 = new List<KeyValuePair<string, string>>();
                payload2.Add(new KeyValuePair<string, string>("md5", hash));
                payload2.Add(new KeyValuePair<string, string>("filename", filename));
                payload2.Add(new KeyValuePair<string, string>("filesize", filesize.ToString()));
                payload2.Add(new KeyValuePair<string, string>("mtime", milliseconds.ToString()));

                var responseJObject = await _zoteroService.POSTFormMultiPart(payload2, PDFAuthUri.ToString());

                if (responseJObject["exists"] != null)
                {
                    return;
                }

                var url = responseJObject["url"].ToString();
                var prefix = responseJObject["prefix"].ToString();
                var suffix = responseJObject["suffix"].ToString();
                var contentType = responseJObject["contentType"].ToString();
                var uploadKey = responseJObject["uploadKey"];

                var prefixBytes = Encoding.UTF8.GetBytes(prefix);
                var suffixBytes = Encoding.UTF8.GetBytes(suffix);
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                wr.ContentType = contentType;
                wr.Method = "POST";
                wr.KeepAlive = true;
                Stream rs = wr.GetRequestStream();
                rs.Write(prefixBytes, 0, prefixBytes.Length);
                Stream stream21 = new MemoryStream(fileBytes);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = stream21.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
                stream21.Close();

                rs.Write(suffixBytes, 0, suffixBytes.Length);
                rs.Close();
                rs = null;

                WebResponse wresp = null;
                try
                {
                    //Get the response
                    wresp = wr.GetResponse();
                    Stream stream2 = wresp.GetResponseStream();
                    StreamReader reader2 = new StreamReader(stream2);
                    string responseData = reader2.ReadToEnd();

                    var checkStatusCode = (HttpWebResponse)wresp;
                    var statusCodeResult = checkStatusCode.StatusCode;
                    if (statusCodeResult != HttpStatusCode.Created)
                    {
                        throw new Exception("child attachment was not created");
                    }
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                    throw new Exception("child attachment was not created: " + s);
                }
                finally
                {
                    if (wresp != null)
                    {
                        wresp.Close();
                        wresp = null;
                    }
                    wr = null;
                }

                var fileURI = new UriBuilder($"{baseUrl}/groups/{ groupIDBeingSynced}/items/{key}/file");
                SetZoteroHttpService(fileURI, zoteroApiKey, true);

                var uploadKeyString = uploadKey.ToString();
                var payloadUpload = $"upload={uploadKeyString}";
                var responseRegisterUpload = await _zoteroService.POSTDocument(payloadUpload, $"{baseUrl}/groups/{ groupIDBeingSynced}/items/{key}/file");
                if (!string.IsNullOrEmpty(responseRegisterUpload))
                {
                    throw new Exception("Registering upload in Zotero Error");
                }
                //TODO NEXT IMPORTANT*****************************************************

                // If this is successful then there should be a row in the local DB to log that there is an attachment in Zotero
                // associated with a parent item
                //int startIndex = filename.IndexOf('.') + 1;
                //int extLength = filename.Length - startIndex;
                //var zoteroItemDocumentToInsert = new ZoteroReviewItemDocument
                //{
                //    FileKey = uploadKey,
                //    ITEM_DOCUMENT_ID = itemDocumentID,
                //    parentItem = fileKey,
                //    Version = "blah",
                //    LAST_MODIFIED = DateTime.Now,
                //    SimpleText = "blah",
                //    FileName = filename,
                //    Extension = filename.Substring(startIndex, extLength);
                //};            

                //DataPortal<ZoteroReviewItemDocument> dp2 = new DataPortal<ZoteroReviewItemDocument>();
                //zoteroItemDocumentToInsert = dp2.Execute(zoteroItemDocumentToInsert);

            }
            catch (Exception ex)
            {
                throw new Exception("Uploading file to Zotero Error: " + ex.Message);
            }
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> GroupsGroupdIdItems([FromBody] List<iItemReviewIDZoteroKey> items)
        {
            List<Item> localItems = new List<Item>();
            List<BookWhole> zoteroItems = new List<BookWhole>();

            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                var apiKey = await ApiKey();
                var zoteroApiKey = apiKey?.Value?.ToString();
                var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string groupIDBeingSynced);
                if (!groupIDResult) return StatusCode(500, "Concurrent Zotero session error");

                var itemIDs = items.Select(x => x.itemID);
                var itemkey = items.Select(x => x.itemKey);
                var zoteroItemContent = await ItemsItemId(itemkey.FirstOrDefault());

                if (!SetCSLAUser4Writing()) return Unauthorized();


                foreach (var itemID in itemIDs)
                {
                    DataPortal<Item> dpItemFetch = new DataPortal<Item>();
                    SingleCriteria<Item, Int64> criteriaItem =
                        new SingleCriteria<Item, Int64>(itemID);

                    var itemResult = await dpItemFetch.FetchAsync(criteriaItem);
                    localItems.Add(itemResult);
                }
                var _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Add("Zotero-API-Version", "3");
                _httpClient.DefaultRequestHeaders.Add("Zotero-API-Key", zoteroApiKey);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Unmodified-Since-Version", zoteroItemContent.version.ToString());

                HttpClientProvider httpClientProvider = new HttpClientProvider(_httpClient);
                _zoteroService.SetZoteroServiceHttpProvider(httpClientProvider);

                // 2 - map this item to an object that can be a valid payload (difficult)
                var count = 0;
                HttpResponseMessage response = new HttpResponseMessage();
                foreach (var item in localItems)
                {
                    var payload = JsonConvert.SerializeObject(zoteroItemContent);

                    var PUTItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + itemkey.FirstOrDefault());
                    _httpClient.BaseAddress = new Uri(PUTItemUri.ToString());

                    response = await _zoteroService.UpdateZoteroItem<ZoteroReviewItem>(payload, PUTItemUri.ToString());
                    var actualCode = response.StatusCode;
                    if (actualCode == System.Net.HttpStatusCode.NoContent)
                    {
                        zoteroItemContent = await ItemsItemId(itemkey.FirstOrDefault());

                        DataPortal<ZoteroItemReview> dp = new DataPortal<ZoteroItemReview>();
                        SingleCriteria<ZoteroReviewItem, long> criteria =
                      new SingleCriteria<ZoteroReviewItem, long>(item.ItemId);

                        var zoteroReviewItem = dp.Fetch(criteria);

                        DataPortal<ZoteroReviewItem> dp2 = new DataPortal<ZoteroReviewItem>();
                        SingleCriteria<ZoteroReviewItem, long> criteria2 =
                      new SingleCriteria<ZoteroReviewItem, long>(zoteroReviewItem.ITEM_REVIEW_ID);

                        var zoteroReviewItemFetch = dp2.Fetch(criteria);

                        zoteroReviewItemFetch.ItemKey = itemkey.FirstOrDefault();
                        zoteroReviewItemFetch.Version = zoteroItemContent.version.ToString();

                        // TODO this might need to be changed to an update somehow...
                        zoteroReviewItemFetch = dp2.Execute(zoteroReviewItemFetch);

                    }
                }
                return Ok(true);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Update Zotero Object In ERWeb has an error");
                return StatusCode(500, e.Message);
            }
        }

        //private Item UpdateItemWithZoteroItem(Collection zoteroItem)
        //{
        //    try
        //    {
        //        var concreteReferenceCreator = ConcreteReferenceCreator.Instance;
        //        var reference = concreteReferenceCreator.GetReference(zoteroItem);
        //        var erWebItem = reference.MapReferenceFromZoteroToErWeb();

        //        DataPortal<Item> dp = new DataPortal<Item>();
        //        var updatedErWebItem = dp.Update(erWebItem.Item);
        //        return updatedErWebItem;
        //    }
        //    catch (Exception)
        //    {
        //        throw new Exception("Error updating erWebItem with Zotero item");
        //    }
        //}

        public string GetSignedUrl(string ReviewID, string urlWithParameters, string userToken, string userSecret, string verifier)
        {
            _zoteroConcurrentDictionary.Session.TryGetValue("oauthTimeStamp-" + ReviewID, out string timestamp);
            _zoteroConcurrentDictionary.Session.TryGetValue("oauthNonce-" + ReviewID, out string nonce);
            var signature = OAuthHelper.createSignature(new Uri(urlWithParameters), clientKey,
                                                        clientSecret,
                                                        userToken, userSecret, "GET", timestamp, nonce,
                                                        verifier,
                                                        out string normalizedUrl,
                                                        out string normalizedRequestParameters,
                                                        new Dictionary<string, string>());

            var signedUrl = string.Format("{0}?{1}&oauth_signature={2}", normalizedUrl, normalizedRequestParameters,
                                          signature);

            return signedUrl;
        }
    }
}


