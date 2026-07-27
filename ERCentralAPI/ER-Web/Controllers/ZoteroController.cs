using AuthorsHandling;
using Azure;
using BusinessLibrary.BusinessClasses;
using BusinessLibrary.BusinessClasses.ImportItems;
using BusinessLibrary.Security;
using Csla;
using Csla.Core;
using Csla.Data;
using EPPIDataServices.Helpers;
using ER_Web.Services;
using ER_Web.Zotero;
using ERxWebClient2.Zotero;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ERxWebClient2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ZoteroController : CSLAController
    {
        private ZoteroService _zoteroService; 
        private string baseUrl;
        private string clientKey;
        private string clientSecret;
        private string callbackUrl; 
        private string zotero_request_token_endpoint;
        private string zotero_access_token_endpoint;

        private ErWebAndZoteroReferenceCreator _mapZoteroCollectionToErWebReference; 
        private ZoteroConcurrentDictionary _zoteroConcurrentDictionary;  
        private IConfiguration _configuration; 
        private OAuthParameters _oAuth;
        private IHttpClientFactory _httpClientFactory;

        #region constructor_and_setup

        public ZoteroController(IHttpClientFactory httpClientFactory, IConfiguration appConfiguration, ILogger<Controller> logger, ZoteroConcurrentDictionary zoteroConcurrentDictionary) : base(logger)
        {
            _httpClientFactory = httpClientFactory;

            _configuration = appConfiguration;
            
            _zoteroService = ZoteroService.Instance;

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
            zotero_access_token_endpoint = configuration["zotero_access_token_endpoint"];
            _mapZoteroCollectionToErWebReference = ErWebAndZoteroReferenceCreator.Instance;
            _oAuth = OAuthParameters.Instance;
        }
        public IHttpClientProvider SetZoteroHttpClientProvider(string zoteroApiKey,
                bool ifNoneMatchHeader = false, bool IfUnmodifiedSinceVersion = false, string version = null)
        {
    
            var _httpClient = _httpClientFactory.CreateClient("zoteroApi");
            _httpClient.BaseAddress = new Uri(baseUrl);    
            _httpClient.DefaultRequestHeaders.Add("Zotero-API-Version", "3");
            _httpClient.DefaultRequestHeaders.Add("Zotero-API-Key", zoteroApiKey);
            if (ifNoneMatchHeader)
            {
                _httpClient.DefaultRequestHeaders.Add("If-None-Match", "*");
            }
            if (IfUnmodifiedSinceVersion)
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Unmodified-Since-Version", version);
            }
            return new HttpClientProvider(_httpClient,_configuration);
        }

        #endregion

        #region oAuth_and_related
        [EnableRateLimiting("HighCostEndpoints")]
        [HttpGet("[action]")]
        public async Task<IActionResult> StartOauthProcess()
        {
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                int reviewID = ri.ReviewId;
                _oAuth.ClientKey = clientKey;
                _oAuth.ClientSecret = clientSecret;
                var oauthURL = _oAuth.GetAuthorizationUrl(zotero_request_token_endpoint);

                //dictionary will contain one key: TempOAuthData-[TemporaryToken].
                //TemporaryToken is the unique identifier of the requests chain we are starting here.
                //we send to the client the "temporary token", to make sure the chain can be "closed".
                //the value for this key contains ALL the data we need to temporary store, so to close the loop and get the final Authorisation Token.
                //this value is semicolon separated, as follows:
                //timeStamp; nonce; reviewId; (ER)userId; tokenSecret
                //timestamp and nonce are needed to sign the request when we'll ask for the permanent token.
                //review and user IDs are needed by us, to save the permanent token appropriately.
                //tokenSecret is needed also for signing the request, added last because we receive it from Zotero

                string dictionaryVal = _oAuth.timeStamp + ";" + _oAuth.nonce + ";" + reviewID +";" + ri.UserId + ";";
                          
                var requestZoteroUri = new UriBuilder(oauthURL);
                var _httpClient = _httpClientFactory.CreateClient();
                _httpClient.BaseAddress = new Uri(requestZoteroUri.ToString());

                var httpClientProvider = new HttpClientProvider(_httpClient, _configuration);
               
                var response = await _zoteroService.GetUserPermissions(requestZoteroUri.ToString(), httpClientProvider);

                var indexOfAnd = response.IndexOf('&');

                var responseJson = "";

                if (indexOfAnd > -1)
                {
                    responseJson = response.Substring(0, indexOfAnd);
                }

                var equalsIndexToken = responseJson.IndexOf("oauth_token=");
                var TemporaryToken = responseJson.Substring(equalsIndexToken + 12);

                var remainingStringResponse = response.Substring(indexOfAnd + 1);
                var indexOfSecretAnd = remainingStringResponse.IndexOf('&');
                var secretString = remainingStringResponse.Substring(0, indexOfSecretAnd);
                var equalsIndex = secretString.IndexOf('=');
                var oauth_token_secret = secretString.Substring(equalsIndex + 1);

                _zoteroConcurrentDictionary.Session.TryRemove("TempOAuthData-" + TemporaryToken, out string? throwAway);
                
                _zoteroConcurrentDictionary.Session.TryAdd("TempOAuthData-" + TemporaryToken, dictionaryVal + oauth_token_secret);

                return Json(TemporaryToken);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Starting the Oauth Process has an error");
                return StatusCode(500, e.Message);
            }
        }

        [EnableRateLimiting("HighCostEndpoints")]
        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult?> OauthVerifyGet([FromQuery] string oauth_token, [FromQuery] string oauth_verifier)
        {
            try
            {
                string? tempDicVal = "";
                var CouldFindDictKey = _zoteroConcurrentDictionary.Session.TryGetValue("TempOAuthData-" + oauth_token, out tempDicVal);
                //we then remove the dictionary entry, as we won't need it anymore
                _zoteroConcurrentDictionary.Session.TryRemove("TempOAuthData-" + oauth_token, out string? throwAway);
                if (!CouldFindDictKey || tempDicVal == null || tempDicVal.Length < 1)
                {
                    _logger.LogError("Zotero OauthVerifyGet failed. No Dictionary values found for token: '" + oauth_token +"'.");
                    return Redirect(callbackUrl + "?error=nodictvals");
                }
                string[] vals = tempDicVal.Split(';');
                if (vals.Length != 5) Redirect(callbackUrl + "?error=noDictVals");
                //vals are: timeStamp; nonce; reviewId; (ER)userId; tokenSecret
                string timeStamp = vals[0];
                string nonce = vals[1];
                int reviewId;
                if (!int.TryParse(vals[2], out reviewId))
                {
                    _logger.LogError("Zotero OauthVerifyGet failed. Coud not parse review ID for dictionary value: (" 
                        + oauth_token + ") '" + tempDicVal + "'.");
                    return Redirect(callbackUrl + "?error=nodictvals");
                }
                int contactId;
                if (!int.TryParse(vals[3], out contactId))
                {
                    _logger.LogError("Zotero OauthVerifyGet failed. Coud not parse contact ID for dictionary value: ("
                        + oauth_token + ") '" + tempDicVal + "'.");
                    return Redirect(callbackUrl + "?error=nodictvals");
                }
                string zotero_token_secret = vals[4];

                string url = zotero_access_token_endpoint;
                var signedURL = GetSignedUrl(timeStamp, nonce, reviewId.ToString(), url, oauth_token, zotero_token_secret, oauth_verifier);
                var accessZoteroUri = new UriBuilder(signedURL);
                var _httpClient = _httpClientFactory.CreateClient();
                _httpClient.BaseAddress = new Uri(accessZoteroUri.ToString());

                var httpClientProviderF = new HttpClientProvider(_httpClient, _configuration);
                var responseThree = await _zoteroService.DoGetReq(accessZoteroUri.ToString(), httpClientProviderF);
                var access_oauth_TokenIndex = responseThree.IndexOf('=');
                var indexOfAccessAnd = responseThree.IndexOf('&');
                var access_oauth_Token = responseThree.Substring(access_oauth_TokenIndex + 1, indexOfAccessAnd - access_oauth_TokenIndex - 1);
                
                var remainingStringresponseThree = responseThree.Substring(indexOfAccessAnd + 1);
                var indexOfSecondEquals = remainingStringresponseThree.IndexOf('=');
                var indexOfSecondAnd = remainingStringresponseThree.IndexOf('&');
                
                var remainingStringresponseFour = remainingStringresponseThree.Substring(indexOfSecondAnd + 1);
                var indexOfThirdEquals = remainingStringresponseFour.IndexOf('=');
                var indexOfThirdAnd = remainingStringresponseFour.IndexOf('&');
                var access_userId = remainingStringresponseFour.Substring(indexOfThirdEquals + 1, indexOfThirdAnd - indexOfThirdEquals - 1);
                //Check how many GroupIds the user has write access to, and react in 1 of 3 ways:
                //1. no groups -> specific error
                //2. 1 group only. Perfect, save the data, with Zotero GROUP_ID;
                //3. Many groups, meh. User needs to use the UI to pick the group (no instructions needed by the client), save data without GROUP_ID.
                List<int> GroupIds = await GetGroupsPermissions(access_userId, reviewId.ToString(), access_oauth_Token);

                ZoteroReviewConnection zRc = new ZoteroReviewConnection();
                zRc.ErUserId = contactId;
                zRc.REVIEW_ID = reviewId;
                zRc.ApiKey = access_oauth_Token;

                zRc.ZoteroUserId = int.Parse(access_userId);//we don't "tryParse" as it's not clear what to do if this fails: we don't want to save the API Key if we don't know who it belongs to.
                if (GroupIds.Count == 0)
                {
                    //tell the client things are bad: can't setup any sync, as we don't have access to any groups
                    return Redirect(callbackUrl + "?error=nogroups");
                }
                else if (GroupIds.Count == 1)
                {//best option: we can associate this group with the review and key combo, user will be sent direct to the Sync screen on the client, as all is well, now.
                    //Otehrise we'll create our record in TB_ZOTERO_REVIEW_CONNECTION, but without the Zotero Group ID, user will have to tell us which Group to use
                    zRc.LibraryId = GroupIds[0].ToString();
                }
                zRc = zRc.Save();
                return Redirect(callbackUrl);
            }
            catch (Exception e) {
                if (e.Message == "Response status code does not indicate success: 401 (Unauthorized).")
                {
                    _logger.LogException(e, "Zotero Oauth Verify Process has the classic Unauthorized error");
                    return Redirect(callbackUrl + "?error=unauthorised");
                }
                else if (e.Message.StartsWith("DataPortal.Update failed (Cannot insert duplicate key row in object 'dbo.TB_ZOTERO_REVIEW_CONNECTION' with unique index"))
                {
                    _logger.LogException(e, "Zotero Oauth Verify Process error: attempted to link to a library that is already in use.");
                    return Redirect(callbackUrl + "?error=library_clash");
                }
                _logger.LogException(e, "Zotero Oauth Verify Process has an error");
                return StatusCode(500, e.Message);
            }
        }
        /// <summary>
        /// Returns the list of Group Library IDs the user has write access to
        /// </summary>
        /// <param name="zoteroUserId"></param>
        /// <param name="reviewId"></param>
        /// <param name="zoteroApiKey"></param>
        /// <returns></returns>
        private async Task<List<int>> GetGroupsPermissions(string zoteroUserId, string reviewId, string zoteroApiKey)
        {
            List<int> res = new List<int>();
            var GETGroupsUri = new UriBuilder($"{baseUrl}/keys/current");
            var httpClientProvider = SetZoteroHttpClientProvider(zoteroApiKey);
            var response = await _zoteroService.DoGetReq(GETGroupsUri.ToString(), httpClientProvider);
            
            JObject joResponse = JObject.Parse(response);
            JObject ojObject = (JObject)joResponse["access"];
            if (ojObject != null) {
                JObject? jGroups = (JObject?)ojObject["groups"];
                if (jGroups != null)
                {
                    IList<JToken> list = jGroups;
                    if (jGroups["all"] != null && jGroups["all"]["library"] != null && jGroups["all"]["library"].Value<bool>() == true && jGroups["all"]["write"].Value<bool>() == true)
                    {
                        //user gave acess to ALL groups, so we can proceed, as minimum requirements are met
                        //we need to return all groups, though!
                        List<Group> groupsList = await GetGroups(zoteroUserId, reviewId, zoteroApiKey);
                        foreach (Group g in groupsList)
                        {
                            res.Add(g.id);
                        }

                    }
                    else
                    {//we need to look for groups with "write" permissions
                        for (int i = 0; i < list.Count; i++)
                        {
                            JToken jtGroup = list[i];
                            if (jtGroup.First["library"] != null && jtGroup.First["library"].Value<bool>() == true)
                            {
                                if (jtGroup.First["write"] != null && jtGroup.First["write"].Value<bool>() == true)
                                {//OK, whatever this is, it's a Library and has Write permissions, but could be "all" (All groups), or a specific group library
                                    string StVal = ((Newtonsoft.Json.Linq.JProperty)jtGroup).Name;
                                    if (StVal != "all") //we want the specific groups, not the "all" case! (checking just in case)
                                    {
                                        int g_id;
                                        if (int.TryParse(StVal, out g_id))
                                        {//yeah, could get our Int GroupID
                                            res.Add(g_id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            


            return res;
        }

        public string GetSignedUrl(string timestamp, string nonce, string ReviewID, string urlWithParameters, string userToken, string userSecret, string verifier)
        {
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

        /// <summary>
        /// sets the supplied groupId to the one being used, unless removeLink is true, in which case sets the groupId to "no group" (empty string)
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="removeLink"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateGroupToReview([FromBody] int groupId, bool removeLink)
        {
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");

                ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();
                if (string.IsNullOrEmpty(zrc.ApiKey))
                {
                    return Unauthorized();
                }
                if (removeLink == true)
                {//check that the supplied id is the one we're trying to remove, do so if that's the case
                    if (zrc.LibraryId == groupId.ToString())
                    {
                        zrc.LibraryId = "";
                        zrc = zrc.Save();
                        return Ok(zrc);
                    }
                    else
                    {
                        return StatusCode(400, "Data supplied appears incorrect");//400 is "Bad Request"
                    }
                }
                else
                {
                    List<Group> grps = await GetGroups(zrc.ZoteroUserId.ToString(), ri.ReviewId.ToString(), zrc.ApiKey);
                    Group? g = grps.FirstOrDefault(f => f.id == groupId);
                    if (g == null) return StatusCode(400, "Data supplied appears incorrect");//400 is "Bad Request"
                    //if we got here, it's because the group exists, so we assume user has access and set it without further checks.
                    zrc.LibraryId = groupId.ToString();
                    zrc = zrc.Save();
                    return Ok(zrc);
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "UpdateGroupToReview has an error");
                if (e.Message.Contains("dbo.TB_ZOTERO_REVIEW_CONNECTION' with unique index 'UIX_TB_ZOTERO_REVIEW_CONNECTION_LibraryId"))
                {
                    return StatusCode(500, "<b>The selected group library is already linked to a review.</b><br><br>" + e.Message);
                }
                else {
                    return StatusCode(500, e.Message);
                }
            }
        }
        /// <summary>
        /// Returns ZoteroReviewConnection for the current review (if any)
        /// </summary>
        /// <returns></returns>

        [HttpGet("[action]")]
        public async Task<IActionResult> CheckApiKey()
        {
            string Phase = "prep";
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");
                ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();
                if (zrc == null) return Json("No API Key");
                else if (zrc.Status != "OK") return Ok(zrc);//nothing more to check...
                else
                {
                    //OK we have an API Key and a Group Library ID, but will they work?
                    Phase = "GetGroupsPermissions";
                    List<int> Gids = await GetGroupsPermissions(zrc.ZoteroUserId.ToString(), ri.ReviewId.ToString(), zrc.ApiKey);
                    int gid;
                    int.TryParse(zrc.LibraryId, out gid);
                    if (Gids.Contains(gid)) return Ok(zrc);
                    else if (Gids.Count > 0) return Json("No write access to Group Library");
                    else return Json("No write access");
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Get Zotero ApiKey has an error at phase " + Phase);
                //this is ugly, but has to happen here, because GetGroupsPermissions is called from 2 places, but how to react differs
                if (Phase == "GetGroupsPermissions" && e.Message == "Response status code does not indicate success: 403 (Forbidden).")
                {//in this special case, we assume the API Key doesn't work (revoked by user on Zotero page, perhaps!)
                    return Json("Invalid API Key");
                }
                else return StatusCode(500, e.Message);//something not predictable happened!
            }
        }

        public ZoteroReviewConnection ApiKey()
        {
            ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();
            return zrc;
        }

        [HttpGet("[action]")]
        public IActionResult GetApiKey()
        {
            try
            {
                if (!SetCSLAUser()) return Unauthorized();
                ZoteroReviewConnection zrc = ApiKey();
                if (zrc.ZoteroConnectionId > 0) return Ok(zrc);
                else return StatusCode(404, "no API Key");
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Error in GetApiKey");
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GroupMetaData()
        {
            try
            {

                if (!SetCSLAUser()) return Unauthorized();
                ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                if (ri == null) throw new ArgumentNullException("Not sure why this is null");

                List<Group> TempGroups = await GetGroups(zrc.ZoteroUserId.ToString(), ri.ReviewId.ToString(), zrc.ApiKey, true, zrc.LibraryId);
                List<int> ids = await GetGroupsPermissions(zrc.ZoteroUserId.ToString(), ri.ReviewId.ToString(), zrc.ApiKey);//list of group IDs for which we have write rights
                List<Group> result = new List<Group>();
                foreach (Group tGr in TempGroups)
                {
                    if (ids.Contains(tGr.id)) result.Add(tGr);
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Fetching GroupMetaDataAsync has an error");
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// if passing alsoCheckIfWeAlreadyHaveAGroupToSinc = True
        /// then you need to supply the current group library id that is being used (if any).
        /// As a result, the groups returned will have the groupBeingSynced flag set to "true" for the one group that is being synced.
        /// </summary>
        /// <param name="zoteroUserId"></param>
        /// <param name="reviewId"></param>
        /// <param name="zoteroApiKey"></param>
        /// <param name="alsoCheckIfWeAlreadyHaveAGroupToSinc"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
		private async Task<List<Group>> GetGroups(string zoteroUserId, string reviewId, string zoteroApiKey, bool alsoCheckIfWeAlreadyHaveAGroupToSinc = false, string groupId = "")
        {
            //var getKeyResult = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + reviewId, out string zoteroApiKey);
            var GETGroupsUri = new UriBuilder($"{baseUrl}/users/{zoteroUserId}/groups");
            var httpClientProvider  = SetZoteroHttpClientProvider(zoteroApiKey);
            List<Group> groups = await _zoteroService.GetCollections<Group>(GETGroupsUri.ToString(), httpClientProvider);

            if (alsoCheckIfWeAlreadyHaveAGroupToSinc)
            {
                int grIdint;
                if (int.TryParse(groupId, out grIdint) && grIdint > 0)
                {
                    foreach (Group g in groups)
                    {
                        if (g.id == grIdint)
                        {
                            g.groupBeingSynced = true;
                            break;
                        }
                    }
                }
            }

            return groups;
        }

        private (ZoteroReviewConnection, string) CheckPermissionsWithZoteroKey()
        {
            if (Csla.ApplicationContext.User.Identity is not ReviewerIdentity ri) throw new ArgumentNullException("ReviewerIdentity is null!");
            ZoteroReviewConnection zrc = ApiKey();
            string groupIDBeingSynced = zrc.LibraryId;
            return (zrc, groupIDBeingSynced);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> DeleteZoteroApiKey()
        {
            try
            {
                //user can delete the API key, even in read-only, IF they own the Key
                if (!SetCSLAUser()) return Unauthorized();

                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();
                if (zrc == null || string.IsNullOrEmpty(zrc.ApiKey)) return StatusCode(400, "Nothing to delete");
                if (ri == null || zrc.ErUserId != ri.UserId) return Unauthorized();
                var DELETEApiKeysUri = new UriBuilder($"{baseUrl}/keys/{zrc.ApiKey}");
                var httpClientProvider  = SetZoteroHttpClientProvider(zrc.ApiKey);
                bool result = true;
                try
                {
                    result = await _zoteroService.DeleteApiKey(DELETEApiKeysUri.ToString(), httpClientProvider);
                }
                catch (Exception e)
                {//catching here, as it could happen that user wants to "delete" the key BECAUSE they deleted it from Zotero directly
                    //in such a case, the call above would fail, as there is nothing to delete and we tried to delete it with a Key that doesn't authorise anything
                    //as the key itself doesn't exist already!
                    //so we need to finish the job and delete the record in ER as well...
                    _logger.LogException(e, "Delete Zotero Api Key has an error on deleting the Key from the Zotero side");
                }
                // if it is deleted from Zotero then it needs to be deleted locally also!!
                if (result)
                {
                    zrc.Delete();
                    zrc = zrc.Save();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "Delete Zotero Api Key has an error");
                return StatusCode(500, e.Message);
            }
        }

        #endregion

        #region SyncStates_Pull_and_push

        [EnableRateLimiting("MaxCostEndpoints")]
        [HttpPost("[action]")]
        public async Task<IActionResult> PushZoteroErWebReviewItemList([FromBody] ZoteroERWebReviewItem[] zoteroERWebReviewItems)
        {
            //error handling in this method is unusual, because the method consists in many parts, and we try to complete 
            //as much as possible operations, *even when* errors happen.
            ZoteroBatchError errors = new ZoteroBatchError("PushZoteroErWebReviewItemList", zoteroERWebReviewItems.Length);
            try
            {
                if (!SetCSLAUser()) return Unauthorized();

                (ZoteroReviewConnection zrc, string groupIDBeingSynced) = CheckPermissionsWithZoteroKey();

				var zoteroERWebReviewItemsToBePushed = zoteroERWebReviewItems.
                    Where(x => x.SyncState == ZoteroERWebReviewItem.ErWebState.canPush && x.ItemKey.Length == 0).ToList();
                var zoteroItemsToBeUpdated = zoteroERWebReviewItems.
                    Where(x => x.SyncState == ZoteroERWebReviewItem.ErWebState.canPush && x.ItemKey.Length > 0).ToList();

                if(zoteroERWebReviewItemsToBePushed.Count() > 0){
                    var postResult = await PushItemsForThisGroupToZotero(zoteroERWebReviewItemsToBePushed, zrc, groupIDBeingSynced, errors);
                    //not sure if we need to do anything with postResult: errors that happened PushItemsForThisGroupToZotero have been saved in "errors"...
                    //if (!postResult) throw new Exception("Pushing to Zotero failed miserably");
                }

                if (zoteroItemsToBeUpdated.Count() > 0){
                    var updateResult = await UpdatingItemsInZotero(zoteroItemsToBeUpdated, zrc, groupIDBeingSynced, errors);
                    //not sure if we need to do anything with updateResult: errors that happened UpdatingItemsInZotero have been saved in "errors"...
                    //if (!updateResult) throw new Exception("Updating items to Zotero failed miserably");
                }

                foreach (var parentItemWithChildrenList in zoteroERWebReviewItems.Where(x => x.PdfList.Count > 0 && x.ItemKey != "")
                    .Select(x => new { x.PdfList, x.ItemID, x.ItemKey } ))
                {
                    var itemId = parentItemWithChildrenList.ItemID;
                    var parentZoteroKey = parentItemWithChildrenList.ItemKey;
                    var erWebZoteroItemDocs = new List<ErWebZoteroItemDocument>();
                    
                    foreach (ZoteroERWebItemDocument pdf in parentItemWithChildrenList.PdfList)
                    {
                        if (pdf.SyncState == ZoteroERWebReviewItem.ErWebState.canPush)
                        {
                            var itemDoc = new ErWebZoteroItemDocument
                            {
                                itemId = itemId,
                                parentItemFileKey = parentZoteroKey,
                                itemDocumentId = pdf.itemDocumentId
                            };
                            erWebZoteroItemDocs.Add(itemDoc);
                        }
                    }

                    await UploadERWebDocumentsToZoteroAsync(erWebZoteroItemDocs, zrc.REVIEW_ID, errors);
                }
                if (errors.failCount > 0) LogBatchErrors(errors);
                else return Ok();
            }
            catch (Exception e)
            {
                errors.Add(new SingleError(e, "Error in main API method: ZoteroErWebReviewItemList"));
                LogBatchErrors(errors);
            }
            
            string message = "<br>Pushing ended, with " + errors.failCount.ToString() + " error(s), listed below.<ul>";
            foreach (SingleError error in errors.failedIdsAndMessage)
            {
                message += "<li>" + error.ToString() + "</li>";
            }
            message += "</ul> Please try again.<br>If the problem persists, please contact EPPISupport.";
            return StatusCode(500, message);
        }

        private async Task<bool> UpdatingItemsInZotero(IEnumerable<ZoteroERWebReviewItem> zoteroERWebReviewItems,
            ZoteroReviewConnection zrc, string groupIDBeingSynced, ZoteroBatchError errors)
        {

            var result = false;
            var count = 0;
            var failedItemsMsg = "";
            foreach (var item in zoteroERWebReviewItems)
            {
                var PUTItemsUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + item.ItemKey);
                var httpClientProvider  = SetZoteroHttpClientProvider(zrc.ApiKey, false, true, item.Version.ToString());

                var criteria = new SingleCriteria<Item, Int64>(item.ItemID);
                var localItem = DataPortal.Fetch<Item>(criteria);

                if (localItem == null || localItem.ItemId < 1)
                {
                    errors.Add(new SingleError(item.ItemID.ToString(), "Could not find this item."));
                }
                else
                {
                    var zoteroReference = _mapZoteroCollectionToErWebReference.GetReference(localItem);
                    var zoteroItem = zoteroReference.MapReferenceFromErWebToZotero();
                    // TODO move this into the MapReferenceFromErWebToZotero() method, extract to super class
                    zoteroItem.version = item.Version;
                    try
                    {
                        var payload = JsonConvert.SerializeObject(zoteroItem);
                        var response = await _zoteroService.UpdateItem(payload, PUTItemsUri.ToString(), httpClientProvider);
                        var actualContent = await response.Content.ReadAsStringAsync();
                        if (actualContent == "")
                        {
                            result = true;
                        }
                        else
                        {
                            errors.Add(new SingleError(localItem.ItemId.ToString(), "Updating this Item on Zotero failed."));
                        }
                    } 
                    catch (Exception e)
                    {
                        errors.Add(new SingleError(e, localItem.ItemId.ToString(), "Updating this Item on Zotero failed."));
                    }
                }
            }
            return result;
        }

        private async Task<bool> PushItemsForThisGroupToZotero(List<ZoteroERWebReviewItem> zoteroERWebReviewItems, 
            ZoteroReviewConnection zrc, string groupIDBeingSynced, ZoteroBatchError errors)
        {
            var localItems = new List<Item>();
            var zoteroItems = new List<ZoteroCollectionData>();
            
            foreach (var zoteroERWebReviewItem in zoteroERWebReviewItems)
            {
                var erWebLocalItem = GetErWebItem(zoteroERWebReviewItem.ItemID, errors);
                if (erWebLocalItem != null)
                {
                    localItems.Add(erWebLocalItem);
                    var zoteroReference = _mapZoteroCollectionToErWebReference.GetReference(erWebLocalItem);
                    var zoteroItem = zoteroReference.MapReferenceFromErWebToZotero();
                    zoteroItems.Add(zoteroItem);
                }
            }

            var POSTItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/");
            var httpClientProvider = SetZoteroHttpClientProvider(zrc.ApiKey);


            var result = false; 
            if (zoteroItems.Count() > 0)
            {
                var payload = JsonConvert.SerializeObject(zoteroItems);
                var response = await _zoteroService.CreateItem(payload, POSTItemUri.ToString(), httpClientProvider);
                var actualContent = await response.Content.ReadAsStringAsync();

                try
                {
                    bool errorFree = InsertTheseRecentlyPushedItemsLocally(zoteroERWebReviewItems, actualContent, errors);
                    if (errorFree == true && errors.failedIdsAndMessage.Count == 0)
                    {
                        result = true;
                    }
                }
                catch (Exception e)
                {
                    errors.Add(new SingleError(e));
                }
            }
            else { result = true; }
            return result;
        }

        private bool InsertTheseRecentlyPushedItemsLocally(List<ZoteroERWebReviewItem> zoteroERWebReviewItems, string actualContent, ZoteroBatchError errors)
        {
            bool ErrorFree = true;
            Dictionary<int, string> successIndexesAndKeys = new Dictionary<int, string>();
            Dictionary<int,PutErrorResult> failIndexesAndErrors = new Dictionary<int, PutErrorResult>();
            ErrorFree = ParseBatchReply(actualContent, successIndexesAndKeys, failIndexesAndErrors, errors);
            
            //at this point, we should have a list of successes, and a list of errors;
            //we want to create records for successes in TB_ZOTERO_ITEM_REVIEW
            foreach (KeyValuePair<int, string> kvp in successIndexesAndKeys)
            {
                ZoteroERWebReviewItem item = zoteroERWebReviewItems[kvp.Key];
                item.ItemKey = kvp.Value;
                var zoteroItemToInsert = new ZoteroERWebReviewItem
                {
                    ItemKey = item.ItemKey,
                    //ItemID = item.ItemID,
                    iteM_REVIEW_ID = item.iteM_REVIEW_ID,
                    //LAST_MODIFIED = DateTime.Now,
                    //LibraryID = libraryId,
                    //Version = version,
                    //TypeName = item.TypeName
                };
                zoteroItemToInsert = zoteroItemToInsert.Save();
            }
            foreach (KeyValuePair<int, PutErrorResult> kvp in failIndexesAndErrors)
            {
                SingleError err = new SingleError(zoteroERWebReviewItems[kvp.Key].ItemID.ToString(), "Code: "
                    + kvp.Value.code.ToString() + "; Message: " + kvp.Value.message + ".");
                errors.Add(err);
            }
            
            return ErrorFree;
        }
        private bool ParseBatchReply(string replyContent, Dictionary<int, string> successIndexesAndKeys, Dictionary<int, PutErrorResult> failIndexesAndErrors, ZoteroBatchError errors)
        {
            bool ErrorFree = true;
            JObject? joReplyWhole = JsonConvert.DeserializeObject<JObject>(replyContent);
            if (joReplyWhole != null)
            {
                List<JProperty>? jtListSuccessNodes = joReplyWhole["success"]?.Children().OfType<JProperty>().ToList();
                if (jtListSuccessNodes != null)
                {//at least some successes!!
                    foreach (JProperty jpSuccess in jtListSuccessNodes)
                    {
                        int index;
                        if (int.TryParse(jpSuccess.Name, out index))
                        {
                            successIndexesAndKeys.Add(index, jpSuccess.Value.ToString());
                        }
                    }
                }
                List<JProperty>? jtListFailedNodes = joReplyWhole["failed"]?.Children().OfType<JProperty>().ToList();
                if (jtListFailedNodes != null)
                {//at least some failures :-(
                    foreach (JProperty jpFail in jtListFailedNodes)
                    {
                        ErrorFree = false;
                        int index;
                        if (int.TryParse(jpFail.Name, out index))
                        {
                            PutErrorResult? val = JsonConvert.DeserializeObject<PutErrorResult>(jpFail.Value.ToString());
                            if (val != null) failIndexesAndErrors.Add(index, val);
                        }
                    }
                }
            }
            else
            {//we could not parse the reply!!
                ErrorFree = false;
                SingleError err = new SingleError("Unexpected failure when creating/updating items in the Zotero Library: the Zotero API reply could not be parsed.");
                errors.Add(err);
            }
            return ErrorFree;
        }

        private void LogBatchErrors(ZoteroBatchError errors)
        {
            foreach (var err in errors.failedIdsAndMessage)
            {
                if (err.Exception != null)
                {
                    _logger.LogError(err.Exception, "Error pushing items to Zotero, Message: " + err.ErrorMsg + " ID: " + err.UniqueIdentifier);
                }
                else
                {
                    _logger.LogError("Error pushing items to Zotero, Message: " + err.ErrorMsg + " ID: " + err.UniqueIdentifier);
                }
            }
        }


        [EnableRateLimiting("MaxCostEndpoints")]
        [HttpPost("[action]")]
        public async Task<IActionResult> FetchZoteroERWebReviewItemList([FromBody] string attributeId)
        {
			try
			{
				if (!SetCSLAUser()) return Unauthorized();
				ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                var dpZoteroErWebItemList = new DataPortal<ZoteroERWebReviewItemList>();
                var crit = new SingleCriteria<ZoteroERWebReviewItemList, string>(attributeId);

                var result = await dpZoteroErWebItemList.FetchAsync(crit);

                return Ok(result);
		    }
            catch (Exception e)
            {
                _logger.LogException(e, "FetchZoteroERWebReviewItemList has an error");
                return StatusCode(500, e.Message);
            }
        }
        
        private Item? GetErWebItem(long itemId, ZoteroBatchError errors)
        {
            try
            {
                var dp = new DataPortal<Item>();
                var criteria = new SingleCriteria<Item, long>(itemId);
                var item = dp.Fetch(criteria);
                return item;
            } 
            catch (Exception e)
            {
                errors.Add(new SingleError(e, itemId.ToString(), "Could not retreive item from ER DB"));
                return null;
            }
        }

        [EnableRateLimiting("MaxCostEndpoints")]
        [HttpGet("[action]")]
        public async Task<IActionResult> ZoteroItems()
        {
            try
            {
                if (SetCSLAUser())
                {
                    ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();

                    //var APIwatch = new System.Diagnostics.Stopwatch();

                    //https://forums.zotero.org/discussion/76292/search-multiple-item-types-with-api
                    //we ask for things in 2 steps,
                    //1st: everything EXCLUDING attachments and notes, URL encoded url means: "itemType=-attachment || note" as in "NOT(attachment OR note)"
                    //we DO NOT want to know ANYTHING about notes!!

                    //APIwatch.Start();

                    var GETGroupsUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items?sort=title&itemType=-note%20%7C%7C%20attachment");
                    var httpClientProvider = SetZoteroHttpClientProvider(zrc.ApiKey);
                    var items = await _zoteroService.GetPagedCollections<object>(GETGroupsUri.ToString(), httpClientProvider);

                    //2nd: ask for just the attachments and nothing else
                    GETGroupsUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items?itemType=attachment");
                    var attachments = await _zoteroService.GetPagedCollections<object>(GETGroupsUri.ToString(), httpClientProvider);
                    
                    //APIwatch.Stop();
                    //var APItime = APIwatch.ElapsedMilliseconds / 1000;
                    //System.Diagnostics.Debug.WriteLine(". . .");
                    //System.Diagnostics.Debug.WriteLine("APItime: " + APItime.ToString());
                    //System.Diagnostics.Debug.WriteLine(". . .");

                    ZoteroERWebReviewItemList pairedItems = DataPortal.Fetch<ZoteroERWebReviewItemList>(new SingleCriteria<ZoteroERWebReviewItemList, string>((-1).ToString()));
                    ZoteroItemsResult res = new ZoteroItemsResult();
                    res.zoteroItems = items;
                    res.zoteroItems.AddRange(attachments);
                    res.pairedItems = pairedItems;
                    return Ok(res);

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

        private async Task<JObject?> GetZoteroItem(string itemKey, ZoteroReviewConnection zrc, string groupIDBeingSynced, ZoteroBatchError errors)
        {
            try
            {
                var GETItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + itemKey + "");
                var httpClientProvider = SetZoteroHttpClientProvider(zrc.ApiKey);
                JObject item = await _zoteroService.GetItem(GETItemUri.ToString(), httpClientProvider);
                return item;
            }
            catch (Exception e)
            {
                errors.Add(new SingleError(e, itemKey, "GetZoteroItem has an error"));
                //_logger.LogException(e, "GetZoteroItem has an error");
                //var message = "";
                //if (e.Message.Contains("403"))
                //{
                //    message += "No Zotero API Token; either it has been revoked or never created";
                //}
                //else
                //{
                //    message += e.Message;
                //}
                return null;
            }
        }

        [EnableRateLimiting("MaxCostEndpoints")]
        [HttpPost("[action]")]
        public async Task<IActionResult> PullZoteroErWebReviewItemList([FromBody] 
            ZoteroERWebReviewItem[] zoteroERWebReviewItems)
		{
            ZoteroBatchError errors = new ZoteroBatchError("PullZoteroErWebReviewItemList", zoteroERWebReviewItems.Length);
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                (ZoteroReviewConnection zrc, string groupIDBeingSynced) = CheckPermissionsWithZoteroKey();

                var zoteroKeysItemsToBeUpdated = zoteroERWebReviewItems.Where(x => x.SyncState ==
                ZoteroERWebReviewItem.ErWebState.canPull && x.ItemID > 0).ToList();

                var zoteroItemsToBeInserted = zoteroERWebReviewItems.Where(x => x.SyncState ==
                ZoteroERWebReviewItem.ErWebState.canPull && x.ItemID == 0).ToArray();
               
                foreach (var ItemToUpdate in zoteroKeysItemsToBeUpdated)
                {
                    var resultCollection = await GetZoteroItem(ItemToUpdate.ItemKey, zrc, groupIDBeingSynced, errors);
                    if (resultCollection != null)
                    {
                        var collectionItem = JsonConvert.DeserializeObject<Collection>(resultCollection.ToString());
                        if (collectionItem != null)
                        {
                            var res = UpdateErWebItem(collectionItem, ItemToUpdate.ItemID, errors);
                        }
                        else
                        {
                            errors.Add(new SingleError(ItemToUpdate.ItemID.ToString() + "|" + ItemToUpdate.ItemKey
                                , "Failed to parse/cast the ZoteroItem received via the API call."));
                        }
                    }
                    else
                    {
                        errors.Add(new SingleError(ItemToUpdate.ItemID.ToString() + "|" + ItemToUpdate.ItemKey
                                , "The API call to fetch the Zotero data returned no data."));
                    }
                }
                IncomingItemsList forSaving = new IncomingItemsList();
                if (zoteroItemsToBeInserted.Any())
                {
                    forSaving = await InsertNewZoteroItemsIntoErWeb(zoteroItemsToBeInserted, zrc, groupIDBeingSynced, errors);
                }
                List<MiniAttachmentCollectionData> AttachmentsToUpdate = new List<MiniAttachmentCollectionData>();
                foreach (ZoteroERWebReviewItem zoteroERWebReviewItem in zoteroERWebReviewItems)
                {
                    if (zoteroERWebReviewItem.PdfList != null && zoteroERWebReviewItem.PdfList.Count > 0)
                    {
                        if (zoteroERWebReviewItem.ItemID < 1)//we'll look in forSaving.IncomingItems for the newly created ItemID
                        {
                            ItemIncomingData? t = forSaving.IncomingItems.FirstOrDefault(x => x.ZoteroKey == zoteroERWebReviewItem.ItemKey);
                            if (t != null) zoteroERWebReviewItem.ItemID = t.NewItemId;
                            else
                            {
                                errors.Add(new SingleError(zoteroERWebReviewItem.ItemKey.ToString(), "Could not find the new ItemId for this Zotero reference."));
                            }
                        }
                        if (zoteroERWebReviewItem.ItemID > 0)//to be very sure: we do NOT try to add PDFs when we don't have the ItemID
                        {
                            foreach (var pdf in zoteroERWebReviewItem.PdfList)
                            {
                                if (pdf.SyncState == ZoteroERWebReviewItem.ErWebState.canPull)
                                {
                                    await InsertZoteroChildDocumentInErWeb(zrc, pdf, zoteroERWebReviewItem, errors, AttachmentsToUpdate);
                                }
                            }
                        }
                    }
                }
                if (AttachmentsToUpdate.Count > 0)
                {//we need to update the Attachments in Zotero, so to record their ItemDocumentId as a Tag.
                    await AddERIdToZoteroAttachments(zrc, AttachmentsToUpdate, errors);
                }
                if (errors.failCount > 0) LogBatchErrors(errors);
                else return Ok();
            }
            catch (Exception e)
            {
                errors.Add(new SingleError(e, "Error in main API method: PullZoteroErWebReviewItemList"));
                LogBatchErrors(errors);
            }
            string message = "<br>Pulling ended, with " + errors.failCount.ToString() + " error(s), listed below.<ul>";
            foreach (SingleError error in errors.failedIdsAndMessage)
            {
                message += "<li>" + error.ToString() + "</li>";
            }
            message += "</ul> Please try again.<br>If the problem persists, please contact EPPISupport.";
            return StatusCode(500, message);
        }

        private async Task<IncomingItemsList> InsertNewZoteroItemsIntoErWeb(ZoteroERWebReviewItem[] zoteroERWebReviewItems
            , ZoteroReviewConnection zrc, string groupIDBeingSynced, ZoteroBatchError errors)
        {
            var forSaving = new IncomingItemsList();
            var incomingItems = new MobileList<ItemIncomingData>();
            forSaving.IncomingItems = incomingItems;//makes sure forSaving.IncomingItems is never null, even if we didn't get anything from Zotero

            List<Collection> ZoteroRefs = new List<Collection>();//used to send back the ItemIds to the Zot side

            foreach (ZoteroERWebReviewItem zri in zoteroERWebReviewItems)
            {
                string zoteroKey = zri.ItemKey;
                var jObj = await this.GetZoteroItem(zoteroKey, zrc, groupIDBeingSynced, errors);

                if (jObj != null)
                {
                    var collectionItem = JsonConvert.DeserializeObject<Collection>(jObj.ToString());
                    if (collectionItem != null)
                    {
                        ZoteroRefs.Add(collectionItem);
                        try
                        {
                            IMapZoteroReference reference = _mapZoteroCollectionToErWebReference.GetReference(collectionItem);
                            var erWebItem = reference.MapReferenceFromZoteroToErWeb(new Item());

                            var erWebItemIncomingData = _mapZoteroCollectionToErWebReference.GetIncomingDataReference(collectionItem, erWebItem);
                            incomingItems.Add(erWebItemIncomingData);
                        } 
                        catch (Exception e)
                        {
                            errors.Add(new SingleError(e, zri.ItemKey, "Failed to create the ER object to save for this reference."));
                        }
                    }
                    else
                    {
                        errors.Add(new SingleError(zri.ItemKey, "Failed to parse/cast the ZoteroItem received via the API call."));
                    }
                }
                //else below is not needed, probably: we return NULL IFF the API call failed, which already produces an exception.
                //else
                //{
                //    errors.Add(new SingleError(zri.ItemKey, "The API call to fetch the Zotero data returned no data."));
                //}

            }
            if (incomingItems.Count > 0)
            {
                try
                {
                    forSaving = new IncomingItemsList
                    {
                        FilterID = 0,
                        SourceName = "Zotero " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm") + " (" + incomingItems.Count 
                        + (incomingItems.Count == 1 ? " item)": " items)"),
                        SourceDB = "Zotero",
                        DateOfImport = DateTime.Now,
                        DateOfSearch = DateTime.Now,
                        IsIncluded = true,
                        Notes = "",
                        SearchDescr = "Items pulled from Zotero",
                        SearchStr = "N/A",
                        IncomingItems = incomingItems
                    };
                    forSaving.buildShortTitles();
                    forSaving = forSaving.Save();
                }
                catch (Exception e)
                {
                    errors.Add(new SingleError(e, "Saving new items to ER DB failed."));
                }
            }
            //FINAL step, send back the newly created item IDs, so that refs in Zotero will "know" what Item they correspond to
            var PUTItemsUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/");
            var httpClientProvider = SetZoteroHttpClientProvider(zrc.ApiKey);
            List<MiniCollectionType> batch = new List<MiniCollectionType>();
            int batchSize = 50; 
            string searchFor = ZoteroReferenceCreator.searchForERid;// "EPPI-Reviewer ID: ";
            string[] separators = ZoteroReferenceCreator.separators;// { "\r\n", "\n", "\r", Environment.NewLine };
            foreach (ItemIncomingData iid in forSaving.IncomingItems)
            {
                try
                {
                    Collection? zRef = ZoteroRefs.FirstOrDefault(f => f.key == iid.ZoteroKey);
                    if (zRef == null) continue;
                    MiniCollectionType updating = new MiniCollectionType(zRef.data);

                    //for tidyness, we remove any already present ID tag, replace with current one.
                    List<tagObject> tags = updating.tags.ToList().FindAll(f=> !f.tag.StartsWith(searchFor));
                    tags.Add(new tagObject() { type = "1", tag = searchFor + iid.NewItemId.ToString() });
                    updating.tags = tags.ToArray();

                    //ditto, make sure we only have one "EPPI-Reviewer ID: ..." line in the extra field.
                    List<string> extras = updating.extra.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList().FindAll(f => !f.StartsWith(searchFor));
                    updating.extra = searchFor + iid.NewItemId.ToString() + Environment.NewLine 
                                         + string.Join(Environment.NewLine, extras);

                    //DateEdited is set in the ItemIncomingData instance, to "now", at creation time
                    //sending a new timestamp back to Zot forces Zot to respect the explicit timestamp (would update it to "now" again, if we sent the exact value that is already in Zotero).
                    updating.dateModified = ((DateTime)iid.DateEdited).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

                    if (batch.Count < batchSize)
                    {
                        batch.Add(updating);
                    }
                    else
                    {//current batch is full, send it to Zot, empty the batch and add our present element
                        var res = await _zoteroService.UpdatePartialItems(JsonConvert.SerializeObject(batch.ToArray()), PUTItemsUri.ToString(), httpClientProvider);
                        var actualContent = await res.Content.ReadAsStringAsync();

                        Dictionary<int, string> successIndexesAndKeys = new Dictionary<int, string>();
                        Dictionary<int, PutErrorResult> failIndexesAndErrors = new Dictionary<int, PutErrorResult>();
                        bool success = ParseBatchReply(actualContent, successIndexesAndKeys, failIndexesAndErrors, errors);
                        foreach (KeyValuePair<int, PutErrorResult> kvp in failIndexesAndErrors)
                        {
                            SingleError err = new SingleError(batch[kvp.Key].key, "Failed to send back the Item ID for this reference, with err. code: "
                                + kvp.Value.code.ToString() + "; Message: " + kvp.Value.message + ".");
                            errors.Add(err);
                        }
                        batch.Clear();
                        batch.Add(updating);
                    }
                } 
                catch(Exception e)
                {
                    errors.Add(new SingleError(e, "Error sending a batch of new ER-Ids back to Zotero. This means that up to " + batch.Count + " item(s) on the Zotero end will not \"know\" their EPPI-Reviewer ID."));
                }
            }
            if (batch.Count > 0)
            {
                try
                {
                    var res = await _zoteroService.UpdatePartialItems(JsonConvert.SerializeObject(batch.ToArray()), PUTItemsUri.ToString(), httpClientProvider);
                    var actualContent = await res.Content.ReadAsStringAsync();
                    Dictionary<int, string> successIndexesAndKeys = new Dictionary<int, string>();
                    Dictionary<int, PutErrorResult> failIndexesAndErrors = new Dictionary<int, PutErrorResult>();
                    bool success = ParseBatchReply(actualContent, successIndexesAndKeys, failIndexesAndErrors, errors);
                    foreach (KeyValuePair<int, PutErrorResult> kvp in failIndexesAndErrors)
                    {
                        SingleError err = new SingleError(batch[kvp.Key].key, "Failed to send back the Item ID for this reference, with err. code: "
                            + kvp.Value.code.ToString() + "; Message: " + kvp.Value.message + ".");
                        errors.Add(err);
                    }
                }
                catch (Exception e)
                {
                    errors.Add(new SingleError(e, "Error sending a batch of new ER-Ids back to Zotero. This means that up to " + batch.Count + " item(s) on the Zotero end will not \"know\" their EPPI-Reviewer ID."));
                }
            }
            return forSaving;
        }

        private async Task InsertZoteroChildDocumentInErWeb(ZoteroReviewConnection zrc, ZoteroERWebItemDocument pdf,
			ZoteroERWebReviewItem zoteroERWebReviewItem, ZoteroBatchError errors, List<MiniAttachmentCollectionData> AttachmentsToUpdate)
		{
            string fileName = pdf.documenT_TITLE;
            string key = pdf.DocZoteroKey;
            var GetFileUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items/{key}/file");
            var httpClientProvider = SetZoteroHttpClientProvider(zrc.ApiKey);
            try
            {
                var response = await _zoteroService.GetDocumentHeader(GetFileUri.ToString(), httpClientProvider);
                //var lastModifiedDate = response.Content.Headers.GetValues("Last-Modified").FirstOrDefault();
                string ContentType = "";
                string ext = "";
                if (response.Content.Headers.ContentType != null && response.Content.Headers.ContentType.MediaType != null)
                {
                    ContentType = response.Content.Headers.ContentType.MediaType;
                    switch (ContentType)
                    {
                        case @"application/pdf":
                            ext = ".pdf";
                            break;
                        case @"application/msword":
                            ext = ".doc";
                            break;
                        case @"application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                            ext = ".docx";
                            break;
                        case @"application/vnd.ms-powerpoint":
                            ext = ".ppt";
                            break;
                        case @"application/vnd.openxmlformats-officedocument.presentationml.presentation":
                            ext = ".pptx";
                            break;
                        case @"application/vnd.openxmlformats-officedocument.presentationml.slideshow":
                            ext = ".ppsx";
                            break;
                        case @"application/vnd.ms-excel":
                            ext = ".xls";
                            break;
                        case @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                            ext = ".xlsx";
                            break;
                        case @"text/html":
                            ext = ".html";
                            break;
                        case @"application/vnd.oasis.opendocument.text":
                            ext = ".odt";
                            break;
                        case @"application/vnd.oasis.opendocument.spreadsheet":
                            ext = ".ods";
                            break;
                        case @"application/vnd.oasis.opendocument.presentation":
                            ext = ".odp";
                            break;
                        case @"application/postscript":
                            ext = ".ps";
                            break;
                        case @"text/plain":
                            ext = ".txt";
                            break;
                        default:
                            ext = "NotAllowed";
                            break;
                    }
                }
                if (ext == "NotAllowed")
                {
                    errors.Add(new SingleError(pdf.DocZoteroKey + " in " + zoteroERWebReviewItem.ItemKey, "This document was not saved: this file type is not supported."));
                    return;
                }
                else if (ext == "")
                {
                    errors.Add(new SingleError(pdf.DocZoteroKey + " in " + zoteroERWebReviewItem.ItemKey, "This document was not saved: this file type is unknown."));
                    return;
                }

                if (fileName == "Full Text")
                {//this is the filename we get from Zotero, when a doc is found by Zotero automatically, and we don't like it
                    if (zoteroERWebReviewItem.ShortTitle != "")
                    {
                        fileName = zoteroERWebReviewItem.ShortTitle + ext;
                    }
                    else
                    {
                        fileName = zoteroERWebReviewItem.ItemID.ToString() + ext;
                    }
                }
                var fileStream = await response.Content.ReadAsStreamAsync();


                Stream stream = fileStream;
                byte[] Binary = new byte[stream.Length];
                stream.Read(Binary, 0, (int)stream.Length);
                if (ext.ToLower() == ".txt")
                {
                    string SimpleText = System.Text.Encoding.UTF8.GetString(Binary);
                    ItemDocumentSaveCommand cmd = new ItemDocumentSaveCommand(zoteroERWebReviewItem.ItemID,
                        fileName,
                        ext,
                        SimpleText,
                        pdf.DocZoteroKey
                        );
                    cmd = cmd.doItNow();
                    pdf.itemDocumentId = cmd.ItemDocumentId;
                }
                else if (ext != "NotAllowed")
                {
                    ItemDocumentSaveBinCommand cmd = new ItemDocumentSaveBinCommand(zoteroERWebReviewItem.ItemID,
                        fileName,
                        ext,
                        Binary,
                        pdf.DocZoteroKey
                        );
                    cmd = cmd.doItNow();
                    pdf.itemDocumentId = cmd.ItemDocumentId;
                }
                AttachmentsToUpdate.Add(new MiniAttachmentCollectionData(pdf.itemDocumentId, pdf.DocZoteroKey));
            }
            catch (Exception e)
            {
                errors.Add(new SingleError(e, pdf.DocZoteroKey + " in " + zoteroERWebReviewItem.ItemKey, "Error in InsertZoteroChildDocumentInErWeb"));
            }
        }
        private async Task AddERIdToZoteroAttachments(ZoteroReviewConnection zrc, List<MiniAttachmentCollectionData> AttachmentsToUpdate, ZoteroBatchError errors)
        {
            string QuerySt = "";
            var httpClientProvider = SetZoteroHttpClientProvider(zrc.ApiKey);
            var PUTItemsUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items/");
            List<AttachmentCollection> list = new List<AttachmentCollection>();
            List<MiniAttachmentCollectionData> TempList = new List<MiniAttachmentCollectionData>();
            for(int i = 0; i < AttachmentsToUpdate.Count; i++)
            {
                MiniAttachmentCollectionData mac = AttachmentsToUpdate[i];
                TempList.Add(mac);
                if (TempList.Count == 1) QuerySt = mac.key;//first element in the list, no comma!
                else
                {//an element in the list, not the first
                    QuerySt += "," + mac.key;
                }
                if (TempList.Count == 50 || i == AttachmentsToUpdate.Count -1)
                {//OK, this is the last element in the current batch, considering we can ask for, and then update, up to 50 entities in one go
                    //we'll do the work: (1) get details for current batch, (2) ask to update them, (3) parse results...
                    var GETItemUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items?itemKey=" + QuerySt);
                    List<AttachmentCollection> ListFromZot = new List<AttachmentCollection>();
                    try
                    {
                        //get our 50 attachments from Zotero, gives us their version N...
                        ListFromZot = await _zoteroService.GetCollections<AttachmentCollection>(GETItemUri.ToString(), httpClientProvider);
                    }
                    catch(Exception e)
                    {
                        errors.Add(new SingleError(e, "Error in Fetching details of pulled Attachments, affecting up to : " + TempList.Count + " record(s)."));
                    }
                    foreach (MiniAttachmentCollectionData tac in TempList)
                    {
                        AttachmentCollection? tInput = ListFromZot.FirstOrDefault(f => tac.key == f.key);
                        if (tInput != null)
                        {
                            tac.version = tInput.version;
                            List<tagObject> currentTags = tInput.data.tags.ToList().FindAll(f => !f.tag.StartsWith(ZoteroReferenceCreator.searchForERid));
                            currentTags.Add(tac.tags[0]);
                            tac.tags = currentTags.ToArray();
                        }
                    }
                    var updating = TempList.FindAll(f => f.version != 0);//making sure we only try to update Attachs that are well formed...
                    if (updating.Count > 0)
                    {
                        //update our attachments and then figure out how well it worked.
                        try
                        {
                            var res = await _zoteroService.UpdatePartialItems(JsonConvert.SerializeObject(updating.ToArray()), PUTItemsUri.ToString(), httpClientProvider);
                            var actualContent = await res.Content.ReadAsStringAsync();

                            Dictionary<int, string> successIndexesAndKeys = new Dictionary<int, string>();
                            Dictionary<int, PutErrorResult> failIndexesAndErrors = new Dictionary<int, PutErrorResult>();
                            bool success = ParseBatchReply(actualContent, successIndexesAndKeys, failIndexesAndErrors, errors);
                            foreach (KeyValuePair<int, PutErrorResult> kvp in failIndexesAndErrors)
                            {
                                SingleError err = new SingleError(updating[kvp.Key].key, "Failed to send back the ItemDoc ID for this Attachment, with err. code: "
                                    + kvp.Value.code.ToString() + "; Message: " + kvp.Value.message + ".");
                                errors.Add(err);
                            }
                        }
                        catch (Exception e)
                        {
                            errors.Add(new SingleError(e, "Error in updating pulled Attachments (to add their ER IDs), affecting up to : " + updating.Count + " record(s)."));
                        }
                    }
                    TempList.Clear();
                    QuerySt = "";

                }
            }
            
        }

        private bool UpdateErWebItem(Collection collection, long itemId, ZoteroBatchError errors)
        {
            try
            {
                Item itemFetch = DataPortal.Fetch<Item>(new SingleCriteria<Item, long>(itemId));
                if (itemFetch.ItemId != itemId)
                {
                    errors.Add(new SingleError(itemId.ToString(), "Failed to retreive the ER item to update."));
                    return false;
                }
                else
                {
                    IMapZoteroReference referenceUpdate = _mapZoteroCollectionToErWebReference.GetReference(collection);
                    var erWebItemUpdate = referenceUpdate.MapReferenceFromZoteroToErWeb(itemFetch);

                    if (erWebItemUpdate == null || erWebItemUpdate.Item == null)
                    {
                        errors.Add(new SingleError(itemId.ToString(), "Failed to convert Zotero data into ER Item data."));
                        return false;
                    }

                    erWebItemUpdate.Item = erWebItemUpdate.Item.Save();
                    return true;
                }
            } 
            catch (Exception e)
            {
                errors.Add(new SingleError(e, itemId.ToString() + "|" + collection.key, "Error in UpdateErWebItem"));
                return false;
            }
        }

        [EnableRateLimiting("HighCostEndpoints")]
        [HttpPost("[action]")]
        public  IActionResult DeleteLinkedDocsAndItems([FromBody] ZoteroLinksToDelete incomingkeys )
        {
            try
            {
                if (!SetCSLAUser4Writing()) return Unauthorized();
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                //we go directly to the DB, bypassing CSLA!!

                //but we need to make sure we're not passing a list of keys that's longer than 8000 chars...
                List<string> UseableItemKeys = new List<string>();
                List<string> UseableDocKeys = new List<string>();
                string? itemKeys = incomingkeys.itemKeys, docKeys = incomingkeys.docKeys;
                if (itemKeys != null && itemKeys !="")
                {
                    if (itemKeys.Length > 8000)
                    {
                        //ugh, this is painful... Alright, we'll do it in batches of a bit more than 7000 chars...
                        string[] tmp = itemKeys.Split(',' ,StringSplitOptions.RemoveEmptyEntries);
                        string tmpList = "";
                        foreach(string oneKey in tmp)
                        {
                            if (tmpList.Length > 7000)
                            {
                                tmpList += oneKey;
                                UseableItemKeys.Add(tmpList);
                                tmpList = "";
                            }
                            else
                            {
                                tmpList += oneKey + ",";
                            }
                        }
                    }
                    else 
                    {
                        UseableItemKeys.Add(itemKeys);
                    }
                }
                if (docKeys != null && docKeys !="")
                {
                    if (docKeys.Length > 8000)
                    {
                        //ugh, this is painful... Alright, we'll do it in batches of a bit more than 7000 chars...
                        string[] tmp = docKeys.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        string tmpList = "";
                        foreach (string oneKey in tmp)
                        {
                            if (tmpList.Length > 7000)
                            {
                                tmpList += oneKey;
                                UseableDocKeys.Add(tmpList);
                                tmpList = "";
                            }
                            else
                            {
                                tmpList += oneKey + ",";
                            }
                        }
                    }
                    else
                    {
                        UseableDocKeys.Add(docKeys);
                    }
                }
                SQLHelper sQLHelper = new SQLHelper(_configuration, _logger);
                using (SqlConnection connection = new SqlConnection(BusinessLibrary.Data.DataConnection.ConnectionString))
                {
                    SqlParameter[] parameters = new SqlParameter[2];
                    parameters[0] = new SqlParameter("@DocumentKeys", "");
                    parameters[1] = new SqlParameter("@ReviewId", ri.ReviewId);
                    foreach (string keys in UseableDocKeys)
                    {
                        parameters[0].Value = keys;
                        sQLHelper.ExecuteNonQuerySP(connection, "st_ZoteroItemDocumentDeleteInBulk", parameters);
                    }
                    SqlParameter[] parameters2 = new SqlParameter[2];
                    parameters2[0] = new SqlParameter("@ItemKeys", ""); 
                    parameters2[1] = new SqlParameter("@ReviewId", ri.ReviewId);
                    foreach (string keys in UseableItemKeys)
                    {
                        parameters2[0].Value = keys;
                        sQLHelper.ExecuteNonQuerySP(connection, "st_ZoteroItemReviewDeleteInBulk", parameters2);
                    }
                }

                return Ok(true);
            }
            catch (Exception e)
            {
                _logger.LogException(e, "DeleteMiddleMan has an error");
                return StatusCode(500, e.Message);
            }
        }

        private async Task UploadERWebDocumentsToZoteroAsync(List<ErWebZoteroItemDocument> erWebZoteroItemDocs, 
             int RevId, ZoteroBatchError errors)
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
                    REV_ID.Value = RevId;

                    using (SqlConnection conn = new SqlConnection(sQLHelper.ER4DB))
                    {
                        conn.Open();

                        using (SqlDataReader dr = sQLHelper.ExecuteQuerySP(conn, "st_ItemDocumentBin", DOC_ID, REV_ID))
                        {

                            dr.Read();
                            // TODO CHANGE THIS AT THE END
                            if (!dr.HasRows) throw new Exception("No rows from SP this does not make sense");

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

                            var uploadKeyString =await UploadFileBytesToZoteroAsync(fileBytes, itemDoc.itemDocumentId, parentItemKey, name, type, errors);
                            if (uploadKeyString != "Failure!")
                            {
                                InsertUploadedDocLocally(itemDoc.itemDocumentId, name, uploadKeyString, errors);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new SingleError(ex, itemDoc.itemDocumentId.ToString(), "An error happened when uploading this document to Zotero."));
                    this._logger.LogError("uploading docs to Zotero failed", ex);
                }
                counter++;
            }
        }
              
        private async Task<string> UploadFileBytesToZoteroAsync(byte[] fileBytes, long itemDocumentId, string fileKey, string filename, string type, ZoteroBatchError errors)
        {
            try
            {
                string key = "Failure!";//if we do not get the key val from the uploaded document (something didn't work) we report failure
                Stream stream = new MemoryStream(fileBytes);
                var md5Content = GetMD5HashFromStream(stream);
                //var dt = DateTime.Now;

                string contentType = "";
                switch (type.ToLower())
                {
                    case ".pdf":
                        contentType = @"application/pdf";
                        break;
                    case ".doc":
                        contentType = @"application/msword";
                        break;
                    case ".docx":
                        contentType = @"application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        break;
                    case ".ppt":
                        contentType = @"application/vnd.ms-powerpoint";
                        break;
                    case ".pps":
                        contentType = @"application/vnd.ms-powerpoint";
                        break;
                    case ".pptx":
                        contentType = @"application/vnd.openxmlformats-officedocument.presentationml.presentation";
                        break;
                    case ".ppsx":
                        contentType = @"application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                        break;
                    case ".xls":
                        contentType = @"application/vnd.ms-excel";
                        break;
                    case ".xlsx":
                        contentType = @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    case ".htm":
                        contentType = @"text/html";
                        break;
                    case ".html":
                        contentType = @"text/html";
                        break;
                    case ".odt":
                        contentType = @"application/vnd.oasis.opendocument.text";
                        break;
                    case ".ods":
                        contentType = @"application/vnd.oasis.opendocument.spreadsheet";
                        break;
                    case ".odp":
                        contentType = @"application/vnd.oasis.opendocument.presentation";
                        break;
                    case ".ps":
                        contentType = @"application/postscript";
                        break;
                    case ".eps":
                        contentType = @"application/postscript";
                        break;
                    case ".csv":
                        contentType = @"application/vnd.ms-excel";
                        break;
                    case "txt":
                    case ".txt":
                        contentType = @"text/plain";
                        break;
                    default:
                        errors.Add(new SingleError(itemDocumentId.ToString(), "Unsupported file type, for extension: " + type + "."));
                        return key;
                }
                tagObject tag = new tagObject
                {
                    tag = "EPPI-Reviewer ID: " + itemDocumentId.ToString(),
                    type = "1"
                };
                MiniAttachmentCollectionDataForPushing ZotRecord = new MiniAttachmentCollectionDataForPushing()
                {
                    parentItem = fileKey,
                    title = filename,
                    filename = filename,
                    tags = new tagObject[1] { tag },
                    contentType = contentType
                };
                MiniAttachmentCollectionDataForPushing[] tArray = new MiniAttachmentCollectionDataForPushing[1] { ZotRecord };
                string payload = JsonConvert.SerializeObject(tArray);
                //string payload = "[ " +
                //               "{" +
                //                    " \"itemType\": \"attachment\", " +
                //                     "\"parentItem\": \"" + fileKey + "\", " +
                //                   "\"linkMode\": \"imported_file\", " +
                //                   "\"title\": \"" + filename + "\"," +
                //                   "\"accessDate\": \"\", " +
                //                       "\"note\": \"\", " +
                //                    " \"tags\": [" + JsonConvert.SerializeObject(tag) +"], " +
                //                     "\"collections\": [], " +
                //                     " \"relations\": { }," +
                //                      " \"contentType\": \"" + contentType +"\"," +
                //                       "    \"charset\": \"\"," +
                //                         "   \"filename\": \"" + filename + "\"," +
                //                         "  \"md5\": null," +
                //                          " \"mtime\": null" +
                //                         "}" +
                //                       "]";

				(ZoteroReviewConnection zrc, string groupIDBeingSynced) = CheckPermissionsWithZoteroKey();
				var POSTItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/?v=3");
                var httpClientProvider = SetZoteroHttpClientProvider(zrc.ApiKey);

                //phase 1 create a new record on Zotero Group Library, to obtain a key...
                //This also tells Zot that this new record is a child of the appropriate reference.
                var responseTwo = await _zoteroService.POSTJDocument(payload, POSTItemUri.ToString(), httpClientProvider);
                //var successful = responseTwo["successful"];
                //var zero = successful["0"];
                //string key = zero["key"].ToString();
                Dictionary<int, string> successIndexesAndKeys = new Dictionary<int, string>();
                Dictionary<int, PutErrorResult> failIndexesAndErrors = new Dictionary<int, PutErrorResult>();
                bool success = ParseBatchReply(responseTwo, successIndexesAndKeys, failIndexesAndErrors, errors);
                if (!success)
                {//Phase1 of uploading failed :-(
                    foreach (KeyValuePair<int, PutErrorResult> kvp in failIndexesAndErrors)
                    {
                        SingleError err = new SingleError(itemDocumentId.ToString(), "Code: "
                            + kvp.Value.code.ToString() + "; Message: " + kvp.Value.message + ".");
                        errors.Add(err);
                    }
                    return key;
                } 
                else if (successIndexesAndKeys.Count > 0)
                {//Phase1 worked, we do have a ZoteroKey to use, let's take it
                    key = successIndexesAndKeys[0]; //Key\index for successful elements in the batch has to be zero, because we provided only one thing to upload
                }
                else
                {//weird, shouldn't happen - we received the success signal, but didn't get the success values
                    //adding this clause for safety ONLY
                    return key;
                }

                //now we have a key for the Doc to upload, we can proceed

                //Phase2: send file metadata
                long filesize = fileBytes.Length;
                var hash = md5Content;

                var PDFAuthUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/{key}/file");
                var httpClientPdf = SetZoteroHttpClientProvider(zrc.ApiKey, true);

                DateTime dt = DateTime.Now;
                long milliseconds = dt.Millisecond;

                var payload2 = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("md5", hash),
                    new KeyValuePair<string, string>("filename", filename),
                    new KeyValuePair<string, string>("filesize", filesize.ToString()),
                    new KeyValuePair<string, string>("mtime", milliseconds.ToString())
                };
                
                var responseJObject = await _zoteroService.POSTFormMultiPart(payload2, PDFAuthUri.ToString(), httpClientPdf);

                if (responseJObject["exists"] != null)
                {
                    return key; //same binary already exists in Zotero, we can stop here :-)
                }

                //Phase3: actually upload the binary content
                var url = responseJObject["url"].ToString();
                var prefix = responseJObject["prefix"].ToString();
                var suffix = responseJObject["suffix"].ToString();
                contentType = responseJObject["contentType"].ToString();
                //upoloadKey is used in Phase4
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
                        errors.Add(new SingleError(itemDocumentId.ToString(), 
                            "Failure uplodading this document to Zotero: did not receive the required 'created' response."));
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new SingleError(ex, itemDocumentId.ToString(),
                            "Failure uplodading this document to Zotero: did not receive the required 'created' response."));
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

                //Phase4: "Register the upload", which I believe "links" the binary data to the actual zotero record
                var fileURI = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/{key}/file");
                var httpClientF = SetZoteroHttpClientProvider(zrc.ApiKey, true);

                var uploadKeyString = uploadKey.ToString();
                var payloadUpload = $"upload={uploadKeyString}";
                var responseRegisterUpload = await _zoteroService.POSTDocument(payloadUpload, 
                    $"{baseUrl}/groups/{groupIDBeingSynced}/items/{key}/file", httpClientF);
                if (!string.IsNullOrEmpty(responseRegisterUpload))
                {
                    errors.Add(new SingleError(itemDocumentId.ToString(),
                            "Failure registering upload in Zotero: did not receive the expected response."));
                }

                return key;

            }
            catch (Exception ex)
            {
                errors.Add(new SingleError(ex, itemDocumentId.ToString(),
                            "Failure uploading file to Zotero Error."));
            }
            return "Failure!";
        }

        private static string GetMD5HashFromStream(Stream stream)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(stream);
            stream.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        private void InsertUploadedDocLocally( long itemDocumentId, string filename, 
            string uploadKeyString, ZoteroBatchError errors)
        {
            try
            {
                var zoteroItemDocumentToInsert = new ZoteroERWebItemDocument
                {
                    DocZoteroKey = uploadKeyString,
                    itemDocumentId = itemDocumentId,
                    //ParentItem = fileKey,
                    //Version = 0, // TODO check with Sergio
                    //LAST_MODIFIED = DateTime.Now,
                    //SimpleText = "blah",  // TODO check with Sergio
                    documenT_TITLE = filename
                    //Extension = extension
                };

                var dp2 = new DataPortal<ZoteroERWebItemDocument>();
                zoteroItemDocumentToInsert = dp2.Execute(zoteroItemDocumentToInsert);
            }
            catch (Exception e)
            {
                errors.Add(new SingleError(e, uploadKeyString + "|" + itemDocumentId, "Failed to record the connection between this 'pushed to Zotero' doc and its ER origin."));
            }
              
            return;
        }

        [EnableRateLimiting("MaxCostEndpoints")]
        [HttpGet("[action]")]
        public async Task<IActionResult> RebuildItemConnections()
        {
            try
            {
                if (!SetCSLAUser4Writing())
                {
                    return Forbid();
                }
                else
                {
                    ZoteroBatchError errors = new ZoteroBatchError("RebuildItemConnections", 0);
                    List<JObject> Zitems = new List<JObject>();

                    try
                    {
                        ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();

                        var GETGroupsUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items?sort=title");
                        var httpClientProvider = SetZoteroHttpClientProvider(zrc.ApiKey);
                        Zitems = await _zoteroService.GetPagedCollections<JObject>(GETGroupsUri.ToString(), httpClientProvider);
                    }
                    catch (Exception e)
                    {
                        //errors.Add(new SingleError(e, "Failed to fetch the data needed to 'rebuild' items links."));
                        _logger.LogException(e, "Error in RebuildItemConnections while fetching data.");
                        return StatusCode(500, e.Message);
                    }
                    DataTable TagsAndIds = new DataTable();
                    DataTable TagsAndIdsOfItemsWithDocs = new DataTable();
                    TagsAndIds.Columns.Add(new DataColumn("ERId", Int64.MaxValue.GetType()));
                    TagsAndIds.Columns.Add(new DataColumn("ZOTEROKEY", string.Empty.GetType()));
                    TagsAndIdsOfItemsWithDocs.Columns.Add(new DataColumn("ERId", Int64.MaxValue.GetType()));
                    TagsAndIdsOfItemsWithDocs.Columns.Add(new DataColumn("ZOTEROKEY", string.Empty.GetType()));
                    DataRow tRow;
                    //foreach (Item itm in this.Items)
                    //{
                    //    DataRow dr = InputTable.NewRow();
                    //    dr["ItemId"] = itm.ItemId;
                    //    InputTable.Rows.Add(dr);
                    //}

                    string searchFor = ZoteroReferenceCreator.searchForERid;// "EPPI-Reviewer ID: ";
                    string[] separators = ZoteroReferenceCreator.separators;// { "\r\n", "\n", "\r", Environment.NewLine };
                    Collection? collectionItem = null;
                    foreach (JObject Jzitem in Zitems)
                    {
                        try
                        {
                            collectionItem = JsonConvert.DeserializeObject<Collection>(Jzitem.ToString());
                            if (collectionItem != null && collectionItem.data != null)
                            {
                                tagObject? IdTag = null;
                                if (collectionItem.data.tags.Any())
                                {
                                    IdTag = collectionItem.data.tags.FirstOrDefault((f) => { return f.tag != null && f.tag.StartsWith(searchFor); });
                                }
                                long ERId; string ERIdSt = "";
                                if (IdTag != null)
                                {//we have an ItemId in the IdTag
                                    ERIdSt = IdTag.tag.Replace(searchFor, "");
                                }
                                else if (collectionItem.data.extra != null && collectionItem.data.extra.Contains("EPPI-Reviewer ID: "))
                                {//still OK, we have it in the extra field...
                                    string[] lines = collectionItem.data.extra.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string line in lines)
                                    {
                                        if (line.StartsWith(searchFor))
                                        {
                                            ERIdSt = line.Replace(searchFor, "");
                                            break;
                                        }
                                    }
                                }

                                if (ERIdSt != "" && long.TryParse(ERIdSt, out ERId))
                                {//SUCCESS, we have an ERId to use :-)
                                    if (collectionItem.data.itemType == "attachment")
                                    {
                                        tRow = TagsAndIdsOfItemsWithDocs.NewRow();
                                        tRow["ERId"] = ERId;
                                        tRow["ZOTEROKEY"] = collectionItem.key;
                                        TagsAndIdsOfItemsWithDocs.Rows.Add(tRow);
                                        Debug.WriteLine("Doc: " + collectionItem.key + "|" + ERId.ToString());
                                    }
                                    else
                                    {
                                        tRow = TagsAndIds.NewRow();
                                        tRow["ERId"] = ERId;
                                        tRow["ZOTEROKEY"] = collectionItem.key;
                                        TagsAndIds.Rows.Add(tRow);
                                        //if (collectionItem.links != null
                                        //    && collectionItem.links.attachment != null
                                        //    && collectionItem.links.attachment.href != null
                                        //    && collectionItem.links.attachment.href != ""
                                        //    )
                                        //{
                                        //    //this ref has docs, so we'll need to do _more_ work...
                                        //    TagsAndIdsOfItemsWithDocs.Add;
                                        //    Debug.WriteLine(collectionItem.key + " has docs");
                                        //}
                                        Debug.WriteLine("Item: " + collectionItem.key + "|" + ERId.ToString());
                                    }
                                }

                            }
                        }
                        catch(Exception e)
                        {//might not fail for the next entity? we collect all errors
                            if (collectionItem != null && collectionItem.key != null)
                                errors.Add(new SingleError(e, collectionItem.key, "Failed to parse the data needed to 'rebuild', for this record."));
                            else
                                errors.Add(new SingleError(e, "Failed to parse the data needed to 'rebuild' items links."));
                        }
                    }
                    //now we have the list of all known ER-Ids and ZoteroKey pairs, we'll pass it to SQL st_ZoteroRebuildItemLinks to re-insert whatever records are missing
                    try
                    {
                        SQLHelper sQLHelper = new SQLHelper(_configuration, _logger);

                        SqlParameter[] parameters = new SqlParameter[3];
                        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                        parameters[0] = new SqlParameter("@revID", ri.ReviewId);

                        parameters[1] = new SqlParameter("@itemsAndKeys", TagsAndIds);
                        parameters[1].SqlDbType = SqlDbType.Structured;
                        parameters[1].TypeName = "dbo.ITEMS_ZOT_INPUT_TB";

                        parameters[2] = new SqlParameter("@docsAndKeys", TagsAndIdsOfItemsWithDocs);
                        parameters[2].SqlDbType = SqlDbType.Structured;
                        parameters[2].TypeName = "dbo.ITEMS_ZOT_INPUT_TB";

                        int res = sQLHelper.ExecuteNonQuerySP(sQLHelper.ER4DB, "st_ZoteroRebuildItemLinks", parameters);
                        if (res < 0)
                        {
                            //something went wrong, we'll report failure, SQL error has been logged already
                            return StatusCode(500, "Rebuilding links failed when saving data to the Database");
                        }
                    }
                    catch(Exception e)
                    {
                        errors.Add(new SingleError(e, "Failed to 'rebuild' items links at the final 'update' stage."));
                    }
                    if (errors.failCount == 0)
                    return Ok(true);
                    else
                    {
                        LogBatchErrors(errors);
                        string message = "<br>Rebuilding (links) ended, with " + errors.failCount.ToString() + " error(s), listed below.<ul>";
                        foreach (SingleError error in errors.failedIdsAndMessage)
                        {
                            message += "<li>" + error.ToString() + "</li>";
                        }
                        message += "</ul> Please try again.<br>If the problem persists, please contact EPPISupport.";
                        return StatusCode(500, message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in RebuildItemConnections");
                return StatusCode(500, e.Message);
            }
        }

        #endregion

        /// <summary>
        /// Used to ship raw unfiltered data to Cleint, 
        /// contains all the data needed to "know" what can be done (pull, push, nothing?) with refs present on the Zotero End
        /// </summary>
        private class ZoteroItemsResult
        {
            public List<object>? zoteroItems { get; set; }//what Zotero API told us, "as is"
            public ZoteroERWebReviewItemList? pairedItems { get; set; }//Items for which we "know" their "ZoteroKey" - client will do the pairing 
        }
        public class ZoteroLinksToDelete
        {
            public string? itemKeys { get; set; }//what Zotero API told us, "as is"
            public string? docKeys { get; set; }//Items for which we "know" their "ZoteroKey" - client will do the pairing 
        }
    }
}
