using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPPIDataServices.Helpers;
using ERxWebClient2.Services;
using BusinessLibrary.BusinessClasses;
using Csla;
using BusinessLibrary.Security;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
using Csla.Data;
using ERxWebClient2.Zotero;
using System.Net;
using System.Data;
using BusinessLibrary.BusinessClasses.ImportItems;
using Csla.Core;
using AuthorsHandling;
using System.Security.Cryptography;
using System.Collections.Generic;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System.Diagnostics;
using System.Linq.Expressions;
using ER_Web.Zotero;
using Azure;

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

        private MappingReferenceCreator _mapZoteroCollectionToErWebReference; 
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
            _mapZoteroCollectionToErWebReference = MappingReferenceCreator.Instance;
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
                return StatusCode(500, e.Message);
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
            string searchFor = ZoteroCreator.searchFor;// "EPPI-Reviewer ID: ";
            string[] separators = ZoteroCreator.separators;// { "\r\n", "\n", "\r", Environment.NewLine };
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
                            List<tagObject> currentTags = tInput.data.tags.ToList().FindAll(f => !f.tag.StartsWith(ZoteroCreator.searchFor));
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
                    ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();

                    var GETGroupsUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items?sort=title");
                    var httpClientProvider = SetZoteroHttpClientProvider(zrc.ApiKey);
                    List<JObject> Zitems = await _zoteroService.GetPagedCollections<JObject>(GETGroupsUri.ToString(), httpClientProvider);
                    DataTable TagsAndIds = new DataTable();
                    DataTable TagsAndIdsOfItemsWithDocs = new DataTable();
                    TagsAndIds.Columns.Add(new DataColumn("ERId", Int64.MaxValue.GetType()));
                    TagsAndIds.Columns.Add(new DataColumn("ZOTEROKEY", Type.GetType("System.String")));
                    TagsAndIdsOfItemsWithDocs.Columns.Add(new DataColumn("ERId", Int64.MaxValue.GetType()));
                    TagsAndIdsOfItemsWithDocs.Columns.Add(new DataColumn("ZOTEROKEY", Type.GetType("System.String")));
                    DataRow tRow;
                    //foreach (Item itm in this.Items)
                    //{
                    //    DataRow dr = InputTable.NewRow();
                    //    dr["ItemId"] = itm.ItemId;
                    //    InputTable.Rows.Add(dr);
                    //}

                    string searchFor = ZoteroCreator.searchFor;// "EPPI-Reviewer ID: ";
                    string[] separators = ZoteroCreator.separators;// { "\r\n", "\n", "\r", Environment.NewLine };
                    foreach (JObject Jzitem in Zitems)
                    {
                        var collectionItem = JsonConvert.DeserializeObject<Collection>(Jzitem.ToString());
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
                             //18 chars
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
                    //now we have the list of all known ER-Ids and ZoteroKey pairs, we'll pass it to SQL st_ZoteroRebuildItemLinks to re-insert whatever records are missing
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
                    
                    return Ok(true);
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

//public async Task UpdateSyncStatusOfItemAsync(Dictionary<long, ErWebState> syncStateResults, 
//    Dictionary<long, DocumentSyncState> docSyncStateResults,
//    ZoteroERWebReviewItem item)
//{

//    await GetItemSyncState(syncStateResults, docSyncStateResults, item);
//}

//private async Task GetItemSyncState(Dictionary<long, ErWebState> syncStateResults,
//    Dictionary<long, DocumentSyncState> docSyncStateResults,
//    ZoteroERWebReviewItem localSyncedItem)
//{
//    if (localSyncedItem.Zotero_item_review_ID > 0)
//    {
//        var zoteroItem = await GetZoteroConvertedItemAsync(localSyncedItem.ItemKey);
//        var zoteroItemDateLastModified = Convert.ToDateTime(zoteroItem.data.dateModified);

//        var lastModified = localSyncedItem.LAST_MODIFIED.ToUniversalTime();
//        var result = syncStateResults.TryGetValue(localSyncedItem.ItemID, out ErWebState state);
//        if (lastModified.CompareTo(zoteroItemDateLastModified.ToUniversalTime()) == 0)
//        {
//            if (result)
//            {
//                syncStateResults[localSyncedItem.ItemID] = ErWebState.upToDate;
//            }
//            else
//            {
//                syncStateResults.TryAdd(localSyncedItem.ItemID, ErWebState.upToDate);

//            }
//        }
//        else if (lastModified.CompareTo(zoteroItemDateLastModified.ToUniversalTime()) == 1)
//        {

//            if (result)
//            {
//                syncStateResults[localSyncedItem.ItemID] = ErWebState.ahead;
//            }
//            else
//            {
//                syncStateResults.TryAdd(localSyncedItem.ItemID, ErWebState.ahead);

//            }
//        }
//        else
//        {
//            if (result)
//            {
//                syncStateResults[localSyncedItem.ItemID] = ErWebState.behind;
//            }
//            else
//            {
//                syncStateResults.TryAdd(localSyncedItem.ItemID, ErWebState.behind);
//            }
//        }


//        if (localSyncedItem.PdfList.Count() > 0)
//        {
//            await GetPdfSyncStateAsync(localSyncedItem, zoteroItem, docSyncStateResults);
//        }
//    }
//    else
//    {
//        syncStateResults.TryAdd(localSyncedItem.ItemID, ErWebState.doesNotExist);
//    }
//}

//private async Task GetPdfSyncStateAsync(ZoteroERWebReviewItem item,
//    Collection zoteroItem, Dictionary<long, DocumentSyncState> docSyncStateResults)
//{

//    var zoteroPdf = await GetZoteroAttachmentNamesAsync(zoteroItem);
//    foreach (var document in item.PdfList)
//    {
//        var erWebDoc = document.DOCUMENT_TITLE;
//        if (erWebDoc.Equals(zoteroPdf))
//        {
//            docSyncStateResults.TryAdd(document.Item_Document_Id, DocumentSyncState.upToDate);
//        }
//        else
//        {
//            docSyncStateResults.TryAdd(document.Item_Document_Id, DocumentSyncState.existsOnlyOnER);
//        }
//    }            
//}

///// <summary>
///// TO BE REPLACED by the real method
///// written by SG to try out the extensions to IncomingItemsList
///// </summary>
///// <returns></returns>
////TODO it looks like I just need to replace InsertNewZoteroItemsIntoErWeb with this!!!!!!!!!!!!!!!!!!!!!!!!!!!
////!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//[HttpPost("[action]")]
//public async Task<IActionResult> PullZoteroErWebReviewItemList2([FromBody] ZoteroERWebReviewItem[] zoteroERWebReviewItems)
//{
//    try
//    {
//        if (!SetCSLAUser4Writing()) return Unauthorized();
//        List<ZoteroERWebReviewItem> toPush = zoteroERWebReviewItems.Where(f => f.ItemID < 1).ToList();
//        MobileList<ItemIncomingData> incomingItems = new MobileList<ItemIncomingData>();
//        foreach (ZoteroERWebReviewItem zoteroERWebReviewItem in toPush)
//        {
//            var collectionItem = await GetZoteroConvertedItemAsync(zoteroERWebReviewItem.ItemKey);
//            ItemIncomingData itemIncomingData = new ItemIncomingData
//            {
//                Abstract = collectionItem.data.abstractNote ?? "",
//                Year = collectionItem.data.date ?? "0",
//                Title = collectionItem.data.title,
//                Short_title = collectionItem.data.shortTitle ?? "",
//                Type_id = MapFromZoteroTypeToERWebTypeID(collectionItem.data.itemType),
//                AuthorsLi = new AuthorsHandling.AutorsList(),
//                pAuthorsLi = new MobileList<AuthorsHandling.AutH>(),
//                ZoteroKey = zoteroERWebReviewItem.ItemKey,//new member of ItemIncomingData
//                DateEdited = DateTime.Parse(collectionItem.data.dateModified)//NB we set the date as found in the Zotero record!!
//            };
//            //this bit (and the one above) should sit in a better place, possibly an itemIncomingData constructor that receives a "Collection" and "ZoteroERWebReviewItem" as input parameters
//            int AuthRank = 0;
//            foreach (var Zau in collectionItem.data.creators)
//            {
//                if (Zau.creatorType == "author")
//                {
//                    AutH a = new AutH();
//                    a.FirstName = Zau.firstName;
//                    a.MiddleName = "";
//                    a.LastName = Zau.lastName;
//                    a.Role = 0;//only looking for "actual authors" not parent authors which can be Book editors and the like.
//                    a.Rank = AuthRank;
//                    AuthRank++;
//                    itemIncomingData.AuthorsLi.Add(a);
//                }
//            }
//            //itemIncomingData.AuthorsLi.AddRange(NormaliseAuth.processField(ReadProperty(AuthorsProperty), 0));
//            incomingItems.Add(itemIncomingData);
//        }
//        IncomingItemsList forSaving = new IncomingItemsList
//        {
//            FilterID = 0,
//            SourceName = "Zotero " + DateTime.Now.ToString("dd-MMM-yyyy"),
//            SourceDB = "Zotero",
//            DateOfImport = DateTime.Now,
//            DateOfSearch = DateTime.Now,
//            Included = true,
//            Notes = "",
//            SearchDescr = "Items pulled from Zotero",
//            SearchStr = "N/A",
//            IncomingItems = incomingItems
//        };
//        forSaving.buildShortTitles();
//        forSaving = forSaving.Save();//at this point, forSaving.IncomingItems has the NewItemId field populated with the correct (just created) values.
//                                     //thus the same list can be used to upload PDFs as we have a list of items with ZoteroKey, ItemId and list of (Zotero) PDFs therein.
//        return Ok();

//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "Pull In ERWeb has an error");
//        return StatusCode(500, e.Message);
//    }

//}

//private async Task<Collection> GetZoteroConvertedItemAsync(string itemKey)
//{

//    (ZoteroReviewConnection zrc, string groupIDBeingSynced) = CheckPermissionsWithZoteroKey();
//    var GETItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + itemKey);
//    SetZoteroHttpService(GETItemUri, zrc.ApiKey);
//    var zoteroItem = await _zoteroService.GetCollectionItem(GETItemUri.ToString());
//    return zoteroItem;
//}


//private async Task<string> GetZoteroAttachmentNamesAsync(Collection zoteroItem)
//{
//    List<string> docNames = new List<string>();
//    var parentKey = zoteroItem.key;
//    var index = zoteroItem.links.attachment.href.LastIndexOf('/');
//    var lengthOfAttachmentStr = zoteroItem.links.attachment.href.Length - index;
//    var fileKey = zoteroItem.links.attachment.href.Substring(index + 1, lengthOfAttachmentStr - 1);

//    return await GetZoteroAttachmentFileNameAsync(fileKey);
//}

//private async Task<string> GetZoteroAttachmentFileNameAsync(string fileKey)
//{
//    var zoteroAttachmentLastModified = await GetZoteroAttachmentStateAsync(fileKey);

//    ZoteroReviewConnection zrc = ApiKey();
//    var GetFileUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items/{fileKey}");
//    SetZoteroHttpService(GetFileUri, zrc.ApiKey);

//    var zoteroCollection = await _zoteroService.GetCollectionItem(GetFileUri.ToString());
//    if (zoteroCollection != null)
//    {
//        return zoteroCollection.data.title;

//    }
//    return "";
//}


//private async Task<ZoteroERWebReviewItem.ErWebState> GetZoteroAttachmentStateAsync(string itemKey)
//{

//    ZoteroReviewConnection zrc = ApiKey();
//    var GetFileUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items/{itemKey}/file");
//    SetZoteroHttpService(GetFileUri, zrc.ApiKey);

//    //act
//    var response = await _zoteroService.GetDocumentHeader(GetFileUri.ToString());
//    var lastModifiedDate = response.Content.Headers.GetValues("Last-Modified").FirstOrDefault();
//    if (!string.IsNullOrEmpty(lastModifiedDate))
//    {
//        return ZoteroERWebReviewItem.ErWebState.upToDate;
//    }
//    else
//    {
//        return ZoteroERWebReviewItem.ErWebState.canPush;
//    }
//}

//[HttpGet("[action]")]
//public async Task<IActionResult> FetchGroupToReviewLinks()
//{
//    try
//    {
//        if (!SetCSLAUser()) return Unauthorized();
//        //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//        //if (ri == null) throw new ArgumentNullException("Not sure why this is null");

//        //var getKeyResult = ApiKey();
//        //DataPortal<ZoteroReviewCollectionList> dp = new DataPortal<ZoteroReviewCollectionList>();
//        //SingleCriteria<ZoteroReviewCollectionList, long> criteria =
//        //        new SingleCriteria<ZoteroReviewCollectionList, long>(Convert.ToInt64(ri.ReviewId));
//        //var reviewCollectionList = await dp.FetchAsync(criteria);

//        return Ok("not implemented");
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "Fetch GroupToReview has an error");
//        return StatusCode(500, e.Message);
//    }
//}

/////<summary>
///// Updates Zotero items with changes to local ERWeb items that have changed
///// <paramref name="items"/>
///// <returns>IActionResult</returns>
///// </summary>
//[HttpPut("[action]")]
//public async Task<IActionResult> UpdateERWebItemsInZoteroAsync([FromBody] List<iItemReviewID> items)
//{
//    if (!SetCSLAUser4Writing()) return Unauthorized();
//    (ZoteroReviewConnection zrc, string groupIDBeingSynced) = CheckPermissionsWithZoteroKey();
//    var localItems = new List<Item>();
//    var zoteroItems = new List<CollectionData>();

//    // 1 - get the items as listed in the parameter argument from the local db which would already changed
//    foreach (var item in items)
//    {
//        DataPortal<Item> dpItemFetch = new DataPortal<Item>();
//        SingleCriteria<Item, Int64> criteriaItem =
//            new SingleCriteria<Item, Int64>(item.itemID);

//        var itemResult = await dpItemFetch.FetchAsync(criteriaItem);
//        localItems.Add(itemResult);
//    }
//    var _httpClient = new HttpClient();
//    _httpClient.DefaultRequestHeaders.Add("Zotero-API-Version", "3");
//    _httpClient.DefaultRequestHeaders.Add("Zotero-API-Key", zrc.ApiKey);

//    HttpClientProvider httpClientProvider = new HttpClientProvider(_httpClient);
//    _zoteroService.SetZoteroServiceHttpProvider(httpClientProvider);

//    // 3 - Finally push using a post or a put to Zotero to update the items that should be already present
//    var PUTItemsUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/");
//    SetZoteroHttpService(PUTItemsUri, zrc.ApiKey);

//    // 2 - Convert these items to their Zotero counter parts using the factory pattern class already created
//    foreach (var item in localItems)
//    {
//        DataPortal<ZoteroItemReviewIDs> dp = new DataPortal<ZoteroItemReviewIDs>();
//        SingleCriteria<string> criteria =
//      new SingleCriteria<string>(item.ItemId.ToString());

//        var itemReviewIds = dp.Fetch(criteria);

//        foreach (var itemReviewId in itemReviewIds)
//        {
//            var zoteroItemContent = await ItemsItemId(itemReviewId.ITEM_REVIEW_ID.ToString());
//            var payload = JsonConvert.SerializeObject(zoteroItemContent);
//            var response = await _zoteroService.UpdateItem(payload, PUTItemsUri.ToString());
//            var actualContent = await response.Content.ReadAsStringAsync();
//            if (actualContent.Contains("success"))
//            {
//                var test = "Blah";
//            }
//            else
//            {
//                throw new Exception("PUTTING to Zotero has failed");
//            }
//        }

//    }

//    return Ok(true);
//}

//PROBABLY not needed? Consider deleting this, the API call in the services, and what this method uses...
//[HttpPost("[action]")]
//public async Task<IActionResult> Collection([FromBody] string payload)
//{
//    try
//    {
//        if (!SetCSLAUser4Writing()) return Unauthorized();
//        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//        if (ri == null) throw new ArgumentNullException("Not sure why this is null");
//        ZoteroReviewConnection zrc = ApiKey();
//        var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string groupIDBeingSynced);
//        if (!groupIDResult) return StatusCode(500, "Concurrent Zotero session error");

//        UriBuilder GetCollectionsUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/collections");
//        SetZoteroHttpService(GetCollectionsUri, zrc.ApiKey);
//        payload = "[{\"name\" : \"My Collection Test 2\"}]"; // TODO hardcoded remove when ready

//        var response = await _zoteroService.CollectionPost(payload, GetCollectionsUri.ToString());

//        return Ok();

//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "Collection post has an error");
//        var message = "";
//        if (e.Message.Contains("403"))
//        {
//            message += "No Zotero API Token; either it has been revoked or never created";
//        }
//        else
//        {
//            message += e.Message;
//        }
//        return StatusCode(500, message);
//    }
//}

//[HttpGet("[action]")]
//public async Task<IActionResult> GroupMember([FromQuery] string groupId)
//{
//    try
//    {
//        if (!SetCSLAUser4Writing()) return Unauthorized();
//        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//        if (ri == null) throw new ArgumentNullException("Not sure why this is null");

//        ZoteroReviewConnection zrc = ApiKey();
//        var GetKeyUri = new UriBuilder($"{baseUrl}/groups/{groupId}/settings/members");
//        SetZoteroHttpService(GetKeyUri, zrc.ApiKey);
//        var responseString = await _zoteroService.GetUserPermissions(GetKeyUri.ToString());

//        return Ok(responseString);
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "Fetching Zotero user permissions has an error");
//        var message = "";
//        if (e.Message.Contains("403"))
//        {
//            message += "No Zotero API Token; either it has been revoked or never created";
//        }
//        else
//        {
//            message += e.Message;
//        }
//        return StatusCode(500, message);
//    }
//}

//[HttpGet("[action]")]
//public async Task<IActionResult> UserPermissions()
//{
//    try
//    {
//        if (!SetCSLAUser4Writing()) return Unauthorized();
//        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//        if (ri == null) throw new ArgumentNullException("Not sure why this is null");

//        ZoteroReviewConnection zrc = ApiKey();
//        var GetKeyUri = new UriBuilder($"{baseUrl}/keys/current");
//        SetZoteroHttpService(GetKeyUri, zrc.ApiKey);
//        var responseString = await _zoteroService.GetUserPermissions(GetKeyUri.ToString());

//        return Ok(responseString);
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "Fetching Zotero user permissions has an error");
//        var message = "";
//        if (e.Message.Contains("403"))
//        {
//            message += "No Zotero API Token; either it has been revoked or never created";
//        }
//        else
//        {
//            message += e.Message;
//        }
//        return StatusCode(500, message);
//    }
//}

//[HttpGet("[action]")]
//      public async Task<ApiKey[]> FetchApiKeys()
//      {
//          try
//          {
//              if (!SetCSLAUser()) return new ApiKey[] { };
//              ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//              if (ri == null) throw new ArgumentNullException("Not sure why this is null");

//              var getKeyResult = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + ri.ReviewId.ToString(), out string zoteroApiKey);
//              var GETApiKeysUri = new UriBuilder($"{baseUrl}/keys/{zoteroApiKey}");
//              SetZoteroHttpService(GETApiKeysUri, zoteroApiKey);
//              var key = await _zoteroService.GetApiKey(GETApiKeysUri.ToString());
//              return new ApiKey[] { key };
//          }
//          catch (Exception e)
//          {
//              _logger.LogException(e, "FetchApiKeys has an error");
//              return new ApiKey[] { };
//          }
//      }

//[HttpGet("[action]")]
//public async Task<JsonResult> OLDApiKey([FromQuery] int groupId = -1, [FromQuery] bool deleteApiKey = false)
//{
//    try
//    {
//        //if (!SetCSLAUser())
//        //{
//        //    var error = new JsonErrorModel
//        //    {
//        //        ErrorCode = 403,
//        //        ErrorMessage = "Forbidden"
//        //    };
//        //    return Json(error);
//        //}
//        //ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//        //if (ri == null) throw new ArgumentNullException("Not sure why this is null");
//        //UserDetails userDetails = new UserDetails
//        //{
//        //    reviewId = ri.ReviewId,
//        //    userId = ri.UserId
//        //};

//        //SingleCriteria<ZoteroReviewCollectionList, long> criteria =
//        //        new SingleCriteria<ZoteroReviewCollectionList, long>(userDetails.reviewId);

//        //DataPortal<ZoteroReviewCollectionList> dp = new DataPortal<ZoteroReviewCollectionList>();

//        //var reviewCollection = await dp.FetchAsync(criteria);

//        //ZoteroReviewCollection reviewCollectionItem;
//        //if (groupId == -1)
//        //{
//        //    reviewCollectionItem = reviewCollection.FirstOrDefault(x => x.REVIEW_ID == ri.ReviewId);
//        //}
//        //else
//        //{
//        //    reviewCollectionItem = reviewCollection.FirstOrDefault(x => x.REVIEW_ID == ri.ReviewId && x.LibraryID == groupId.ToString());
//        //}

//        //if (deleteApiKey && reviewCollection.Count > 0 && reviewCollectionItem != null)
//        //{
//        //    reviewCollectionItem.Delete();
//        //    reviewCollectionItem = reviewCollectionItem.Save();
//        //    return Json("DeletedApiKey");
//        //}

//        //var result = await dp.FetchAsync(criteria);
//        //if (result == null || result.Count() == 0)
//        //{
//        //    //               var Result = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + reviewID, out string apiKeyOutFirst);
//        //    //if (Result)
//        //    //{
//        //    //                   return Json(apiKeyOutFirst);
//        //    //}
//        //    //else
//        //    //{
//        //    return Json("");
//        //    //}
//        //}
//        //if (string.IsNullOrEmpty(result.FirstOrDefault().ApiKey))
//        //{
//        //    var error = new JsonErrorModel
//        //    {
//        //        ErrorCode = 403,
//        //        ErrorMessage = "Forbidden"
//        //    };
//        //    return Json(error);
//        //}
//        //if (ri.ReviewId != result.FirstOrDefault().REVIEW_ID)
//        //{
//        //    var error = new JsonErrorModel
//        //    {
//        //        ErrorCode = 403,
//        //        ErrorMessage = "Forbidden"
//        //    };
//        //    return Json(error);
//        //}
//        //var apiOutResult = _zoteroConcurrentDictionary.Session.TryGetValue("apiKey-" + ri.ReviewId, out string apiKeyOut);
//        //if (!string.IsNullOrEmpty(result.FirstOrDefault().ApiKey) && !apiOutResult)
//        //{
//        //    _zoteroConcurrentDictionary.Session.TryAdd("apiKey-" + ri.ReviewId, result.FirstOrDefault().ApiKey);
//        //}
//        //_zoteroConcurrentDictionary.Session.TryAdd("reviewID", ri.ReviewId.ToString());

//        //// TODO create an object here to bring back both strings that are required
//        ////result.ApiKey, result.LibraryID
//        //return Json(result.FirstOrDefault().ApiKey);
//        return Json("");
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "Get Zotero ApiKey has an error");
//        var error = new JsonErrorModel
//        {
//            ErrorCode = 500,
//            ErrorMessage = e.Message
//        };

//        return Json(error);
//    }
//}

//[HttpGet("[action]")]
//public async Task<Collection> ItemsItemId([FromQuery] string itemKey)
//{
//    Collection item = new Collection();
//    try
//    {

//        if (SetCSLAUser())
//        {
//            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//            if (ri == null) throw new ArgumentNullException("Not sure why this is null");
//            (ZoteroReviewConnection zrc, string groupIDBeingSynced) = CheckPermissionsWithZoteroKey();
//            var GETItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + itemKey);
//            SetZoteroHttpService(GETItemUri, zrc.ApiKey);
//            item = await _zoteroService.GetCollectionItem(GETItemUri.ToString());
//            return item;

//        }
//        else return new Collection();
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "FetchZoteroObjects list has an error");
//        return item;
//    }
//}


//[HttpGet("[action]")]
//public async Task<IActionResult> ItemKeyVersionLocal([FromQuery] string ItemKey, string ItemType)
//{
//    try
//    {
//        if (!SetCSLAUser()) return Unauthorized();
//        if (ItemType == "attachment") return Ok();

//        // TODO For now just ignore getting the version of the attachment
//        DataPortal<ZoteroReviewItem> dp = new DataPortal<ZoteroReviewItem>();
//        SingleCriteria<ZoteroReviewItem, string> criteria =
//            new SingleCriteria<ZoteroReviewItem, string>(ItemKey);

//        var result = await dp.FetchAsync(criteria);
//        return Ok(result);
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "Get a Version Of the Item In ErWeb has an error");
//        return StatusCode(500, e.Message);
//    }
//}

//// TODO going through this to understand how to refactor it to new pattern
//// where we are syncing via the client
//// it includes the two below functions and will have to change drastically as it is comparing dates etc.
//[HttpPost("[action]")]
//public async Task<IActionResult> ItemsLocal([FromBody] Collection[] collectionItems)
//{
//    try
//    {

//        if (!SetCSLAUser4Writing()) return Unauthorized();
//        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//        if (ri == null) throw new ArgumentNullException("Not sure why this is null");
//        int countItemsToBeInserted = 0;

//        for (int j = 0; j < collectionItems.Length; j++)
//        {
//            var collectionItem = collectionItems[j];
//            DataPortal<ZoteroReviewItem> dpRI = new DataPortal<ZoteroReviewItem>();
//            SingleCriteria<ZoteroReviewItem, string> criteria =
//                new SingleCriteria<ZoteroReviewItem, string>(collectionItem.data.key);

//            var zoteroReviewItem = dpRI.Fetch(criteria);

//            // Means we already have a version of it
//            if (zoteroReviewItem.Zotero_item_review_ID != 0)
//            {
//                var localModifiedDate = zoteroReviewItem.LAST_MODIFIED.ToUniversalTime();
//                var zoteroModifiedDate = DateTime.Parse(collectionItem.data.dateModified).ToUniversalTime();
//                // check if it is the same version
//                if (localModifiedDate == zoteroModifiedDate)
//                {
//                    //return Ok();
//                }
//                else
//                {
//                    //NB THIS METHOD WILL NOW FAIL!
//                    //await UpdateMiddleAndLeftTableItem(collectionItem);
//                    continue;
//                }
//            }
//            else
//            {
//                countItemsToBeInserted++;
//            }
//        }

//        if (countItemsToBeInserted == 0)
//        {
//            return Ok(true);
//        }

//        var forSaving = new IncomingItemsList();
//        var incomingItems = new MobileList<ItemIncomingData>();
//        for (int j = 0; j < collectionItems.Length; j++)
//        {
//            var collectionItem = collectionItems[j];
//            ItemIncomingData itemIncomingData = new ItemIncomingData
//            {
//                Abstract = collectionItem.data.abstractNote ?? "",
//                Year = collectionItem.data.date ?? "0",
//                Title = collectionItem.data.title,
//                Short_title = collectionItem.data.shortTitle ?? "",
//                Type_id = MapFromZoteroTypeToERWebTypeID(collectionItem.data.itemType),
//                AuthorsLi = new AuthorsHandling.AutorsList(),
//                pAuthorsLi = new MobileList<AuthorsHandling.AutH>(),
//            };
//            incomingItems.Add(itemIncomingData);
//        }

//        forSaving = new IncomingItemsList
//        {
//            FilterID = 0,
//            SourceName = "Zotero_" + DateTime.Now,
//            SourceDB = "Zotero",
//            DateOfImport = DateTime.Now,
//            DateOfSearch = DateTime.Now,
//            Included = true,
//            Notes = "TestZoteroSource",
//            SearchDescr = "TestZoteroSource",
//            SearchStr = "TestZoteroSource",
//            IncomingItems = incomingItems
//        };
//        forSaving.buildShortTitles();
//        forSaving = forSaving.Save();


//        var itemReviewIdsInserted = new List<long>();

//        var dpZoteroItemIdsPerSource = new DataPortal<ZoteroItemSourceList>();
//        var criteriaSource = new SingleCriteria<ZoteroItemSourceList, Int32>(forSaving.SourceID);
//        var itemsInserted = dpZoteroItemIdsPerSource.Fetch(criteriaSource);

//        for (int i = 0; i < collectionItems.Length; i++)
//        {
//            var collection = collectionItems[i];
//            if (collection == null || collection.data == null) return Ok(false);
//            DataPortal<ZoteroReviewItem> dpRI = new DataPortal<ZoteroReviewItem>();
//            SingleCriteria<ZoteroReviewItem, string> criteria =
//                new SingleCriteria<ZoteroReviewItem, string>(collection.data.key);

//            var zoteroReviewItem = dpRI.Fetch(criteria);
//            if (zoteroReviewItem.Zotero_item_review_ID != 0)
//            {
//                var localModifiedDate = zoteroReviewItem.LAST_MODIFIED.ToUniversalTime();
//                var zoteroModifiedDate = DateTime.Parse(collection.data.dateModified).ToUniversalTime();
//                // check if it is the same version
//                if (localModifiedDate == zoteroModifiedDate)
//                {
//                    return Ok(true);
//                }
//            }

//            ZoteroReviewConnection zrc = DataPortal.Fetch<ZoteroReviewConnection>();
//            //var groupIDResult = _zoteroConcurrentDictionary.Session.TryGetValue("groupIDBeingSynced-" + zoteroApiKey, out string groupIDBeingSynced);
//            //if (!groupIDResult) throw new Exception("Concurrent Zotero session error");

//            var key = collection.key;
//            var version = collection.version;
//            // This pulls a straight attachment
//            if (collection.data.itemType == "attachment" || collection.links.attachment != null)
//            {
//                string parentKey = "";

//                if (collection.data.itemType == "attachment")
//                {
//                    parentKey = collection.data.parentItem;
//                }
//                else
//                {
//                    parentKey = collection.key;
//                    var index = collection.links.attachment.href.LastIndexOf('/');
//                    var lengthOfAttachmentStr = collection.links.attachment.href.Length - index;
//                    key = collection.links.attachment.href.Substring(index + 1, lengthOfAttachmentStr - 1);
//                }

//                DataPortal<ZoteroReviewItem> dpCheckParent = new DataPortal<ZoteroReviewItem>();
//                SingleCriteria<ZoteroReviewItem, string> criteriaCheckParent =
//                    new SingleCriteria<ZoteroReviewItem, string>(parentKey);

//                if (string.IsNullOrEmpty(parentKey))
//                    throw new NotImplementedException("Only attachments with a parent can be imported currently");

//                var parentItem = dpCheckParent.Fetch(criteriaCheckParent);

//                if (parentItem.ITEM_REVIEW_ID == 0)
//                {
//                    //need to insert the parent here then continue with the child attachment
//                    ZoteroReviewItem zoteroItemParent = new ZoteroReviewItem();
//                    IMapZoteroReference referenceParent = _concreteReferenceCreator.GetReference(collection);
//                    var erWebItemParent = referenceParent.MapReferenceFromZoteroToErWeb(new Item());
//                    erWebItemParent.Item.IsIncluded = true;
//                    parentItem.ITEM_REVIEW_ID = itemsInserted[i].ITEM_REVIEW_ID;
//                    zoteroItemParent = new ZoteroReviewItem
//                    {
//                        ItemKey = parentKey,
//                        ITEM_REVIEW_ID = parentItem.ITEM_REVIEW_ID,
//                        LAST_MODIFIED = DateTime.Now,
//                        LibraryID = collection.library.id.ToString(),
//                        Version = version,
//                        TypeName = erWebItemParent.Item.TypeName
//                    };

//                    DataPortal<ZoteroReviewItem> dpParent = new DataPortal<ZoteroReviewItem>();
//                    zoteroItemParent = dpParent.Execute(zoteroItemParent);

//                    //return Ok("Parent Item not yet inserted into ERWeb");
//                }

//                DataPortal<ZoteroItemReview> dpItemReview = new DataPortal<ZoteroItemReview>();
//                SingleCriteria<ZoteroItemReview, long> criteriaItemReview =
//                    new SingleCriteria<ZoteroItemReview, long>(parentItem.ITEM_REVIEW_ID);
//                var parentItemID = dpItemReview.Fetch(criteriaItemReview);

//                string fileName = "";
//                if (collection.links.attachment != null)
//                {
//                    //get filename before downloading the file
//                    var GetFileNameUri = new UriBuilder(collection.links.attachment.href);
//                    SetZoteroHttpService(GetFileNameUri, zrc.ApiKey);
//                    var fileNameResponse = await _zoteroService.GetCollectionItem(GetFileNameUri.ToString());
//                    fileName = fileNameResponse.data.title;
//                }
//                else
//                {
//                    fileName = collection.data.title;//fileNameResponse.data.title;
//                }

//                var GetFileUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items/{key}/file");
//                SetZoteroHttpService(GetFileUri, zrc.ApiKey);

//                //act
//                var response = await _zoteroService.GetDocumentHeader(GetFileUri.ToString());
//                var lastModifiedDate = response.Content.Headers.GetValues("Last-Modified").FirstOrDefault();
//                var fileStream = await response.Content.ReadAsStreamAsync();

//                int ind = fileName.LastIndexOf(".");
//                string ext = fileName.Substring(ind);
//                Stream stream = fileStream;
//                byte[] Binary = new byte[stream.Length];
//                stream.Read(Binary, 0, (int)stream.Length);
//                if (ext.ToLower() == ".txt")
//                {
//                    string SimpleText = System.Text.Encoding.UTF8.GetString(Binary);
//                    ItemDocumentSaveCommand cmd = new ItemDocumentSaveCommand(parentItemID.ITEM_ID,
//                        fileName,
//                        ext,
//                        SimpleText
//                        );
//                    cmd.doItNow();
//                }
//                else
//                {
//                    ItemDocumentSaveBinCommand cmd = new ItemDocumentSaveBinCommand(parentItemID.ITEM_ID,
//                        fileName,
//                        ext,
//                        Binary
//                        );
//                    cmd.doItNow();
//                }
//                continue;
//            }



//            ZoteroReviewItem zoteroItem = new ZoteroReviewItem();
//            IMapZoteroReference reference = _concreteReferenceCreator.GetReference(collection);

//            var erWebItem = reference.MapReferenceFromZoteroToErWeb(new Item());
//            erWebItem.Item.IsIncluded = true;

//            if (key.Length > 0)
//            {
//                zoteroItem = new ZoteroReviewItem
//                {
//                    ItemKey = key,
//                    ITEM_REVIEW_ID = itemsInserted[i].ITEM_REVIEW_ID,
//                    LAST_MODIFIED = DateTime.Now,
//                    LibraryID = collection.library.id.ToString(),
//                    Version = version,
//                    TypeName = erWebItem.Item.TypeName
//                };

//                DataPortal<ZoteroReviewItem> dp = new DataPortal<ZoteroReviewItem>();
//                zoteroItem = dp.Execute(zoteroItem);

//                //This pulls a parent item's attachment
//                if (collection.links.attachment != null)
//                {
//                    // need to call the attachment data from Zotero
//                    var IndexLastSlash = collection.links.attachment.href.LastIndexOf('/');
//                    var keyLength = collection.links.attachment.href.Length - IndexLastSlash;
//                    var attachmentKey = collection.links.attachment.href.Substring(IndexLastSlash + 1, keyLength - 1);
//                    var result = await this.ItemsItemKey(attachmentKey);

//                    var resultCollection = result as OkObjectResult;
//                    var attachmentCollection = JsonConvert.DeserializeObject<Collection>(resultCollection.Value.ToString());

//                    var parentKey = attachmentCollection.data.parentItem;
//                    DataPortal<ZoteroReviewItem> dpCheckParent = new DataPortal<ZoteroReviewItem>();
//                    SingleCriteria<ZoteroReviewItem, string> criteriaCheckParent =
//                        new SingleCriteria<ZoteroReviewItem, string>(parentKey);

//                    if (string.IsNullOrEmpty(parentKey)) throw new NotImplementedException("Only attachments with a parent can be imported currently");

//                    var parentItem = dpCheckParent.Fetch(criteriaCheckParent);

//                    if (parentItem.ITEM_REVIEW_ID == 0)
//                    {
//                        //TODO Later do smarter logic 
//                        return Ok("Parent Item not yet inserted into ERWeb");
//                    }

//                    DataPortal<ZoteroItemReview> dpItemReview = new DataPortal<ZoteroItemReview>();
//                    SingleCriteria<ZoteroItemReview, long> criteriaItemReview =
//                        new SingleCriteria<ZoteroItemReview, long>(parentItem.ITEM_REVIEW_ID);
//                    var parentItemID = dpItemReview.Fetch(criteriaItemReview);

//                    var GetFileUri = new UriBuilder($"{baseUrl}/groups/{zrc.LibraryId}/items/{attachmentCollection.data.key}/file");
//                    SetZoteroHttpService(GetFileUri, zrc.ApiKey);

//                    //act
//                    var response = await _zoteroService.GetDocumentHeader(GetFileUri.ToString());
//                    var lastModifiedDate = response.Content.Headers.GetValues("Last-Modified").FirstOrDefault();
//                    var fileStream = await response.Content.ReadAsStreamAsync();
//                    var fileName = attachmentCollection.data.title;
//                    int ind = fileName.LastIndexOf(".");
//                    string ext = fileName.Substring(ind);
//                    Stream stream = fileStream;
//                    byte[] Binary = new byte[stream.Length];
//                    stream.Read(Binary, 0, (int)stream.Length);
//                    if (ext.ToLower() == ".txt")
//                    {
//                        string SimpleText = System.Text.Encoding.UTF8.GetString(Binary);
//                        ItemDocumentSaveCommand cmd = new ItemDocumentSaveCommand(parentItemID.ITEM_ID,
//                            fileName,
//                            ext,
//                            SimpleText
//                            );
//                        cmd.doItNow();
//                    }
//                    else
//                    {
//                        ItemDocumentSaveBinCommand cmd = new ItemDocumentSaveBinCommand(parentItemID.ITEM_ID,
//                            fileName,
//                            ext,
//                            Binary
//                            );
//                        cmd.doItNow();
//                    }
//                    var zoteroItemPostAttachment = new ZoteroReviewItem
//                    {
//                        ItemKey = attachmentCollection.key,
//                        ITEM_REVIEW_ID = parentItem.ITEM_REVIEW_ID,
//                        LAST_MODIFIED = DateTime.Now,
//                        LibraryID = attachmentCollection.library.id.ToString(),
//                        Version = version,
//                        TypeName = "attachment" //TODO magic string for now
//                    };

//                    // TODO not sure what to do with the attachment insert it into the
//                    // zoteroDoc table or simply into the ZoteroItemReview table for now...?
//                    DataPortal<ZoteroReviewItem> dpf = new DataPortal<ZoteroReviewItem>();
//                    zoteroItemPostAttachment = dpf.Execute(zoteroItemPostAttachment);
//                    return Ok(true);
//                }
//            }
//            else
//            {
//                return Ok(true);
//            }

//        }

//        return Ok(true);

//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "ItemsLocalPost has an error");
//        return Ok(false);
//    }
//}

//[HttpGet("[action]")]
//public async Task<IActionResult> ItemIdLocal([FromQuery] string itemReviewID)
//{
//    try
//    {
//        if (string.IsNullOrEmpty(itemReviewID)) return Ok();

//        if (SetCSLAUser())
//        {
//            DataPortal<ZoteroItemIDPerItemReview> dp = new DataPortal<ZoteroItemIDPerItemReview>();
//            SingleCriteria<long> criteria =
//                    new SingleCriteria<long>(Convert.ToInt64(itemReviewID));

//            var resultItemID = await dp.FetchAsync(criteria);
//            return Ok(resultItemID);

//        }
//        else return Forbid();
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "ItemIdLocalGet error");
//        return StatusCode(500, e.Message);
//    }

//}

//[HttpGet("[action]")]
//public async Task<IActionResult> ItemReviewIds([FromQuery] string itemIds)
//{
//    try
//    {
//        if (string.IsNullOrEmpty(itemIds)) return Ok();

//        if (SetCSLAUser4Writing())
//        {

//            //    // TODO...!
//            //    // Need to check if they are in Zotero local Table or not
//            //    // if they are do nothing as the meta check will pick them up
//            //    // If not we will need to push these items, so build
//            //    // zoterItem objects list and get ready to post to Zotero
//            //    // the post is tricky...


//            // TODO instead collect the top ten for demo purposes
//            DataPortal<ZoteroItemReviewIDs> dp = new DataPortal<ZoteroItemReviewIDs>();
//            SingleCriteria<string> criteria =
//                    new SingleCriteria<string>(itemIds);

//            var result = await dp.FetchAsync(criteria);
//            var topTenItemReviewIDs = result.Take(10).Select(x => x.ITEM_REVIEW_ID).ToList();
//            return Ok(topTenItemReviewIDs);
//            //var testReviewIDS = result.Where( x=> x.ITEM_DOCUMENT_ID != 0).Take(10).ToList();
//            //return Ok(testReviewIDS);

//        }
//        else return Forbid();
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "ItemReviewIdsGet has an error");
//        return StatusCode(500, e.Message);
//    }
//}

//[HttpGet("[action]")]
//public async Task<IActionResult> ItemsERWebAndZotero()
//{
//    try
//    {
//        if (SetCSLAUser4Writing())
//        {

//            DataPortal<ZoteroERWebReviewItemList> dp = new DataPortal<ZoteroERWebReviewItemList>();
//            var result = await dp.FetchAsync();
//            return Ok(result);

//        }
//        else return Forbid();
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "ItemsERWebAndZoteroGet has an error");
//        return StatusCode(500, e.Message);
//    }
//}

// COMMENTING AS THIS METHOD WILL NOW FAIL; WILL PROBABLY BE REMOVED AS SOME OF THE LOGIC IS IN THE NEW METHODS
// AND SOME OF IT ON THE CLIENT
//// 3 TODO remove direct database code logic leave to find out how to do a rollback with CSLA here
//[HttpPost("[action]")]
//public async Task<IActionResult> ItemsItemsIdLocal([FromBody] Collection collection)
//{
//    try
//    {
//        if (!SetCSLAUser4Writing()) return Unauthorized();

//        if (collection == null || collection.data == null) return Ok();

//        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

//        var receivedZoteroItem = collection.data;
//        bool updateLocalVersion = false;
//        ZoteroReviewItem zoteroItem = new ZoteroReviewItem();
//        DataPortal<ZoteroReviewItem> dpFetch = new DataPortal<ZoteroReviewItem>();

//        if (!string.IsNullOrEmpty(receivedZoteroItem.key))
//        {

//            SingleCriteria<ZoteroReviewItem, string> criteriaZoteroItem =
//           new SingleCriteria<ZoteroReviewItem, string>(receivedZoteroItem.key);

//            zoteroItem = dpFetch.Fetch(criteriaZoteroItem);
//            if (zoteroItem.ITEM_REVIEW_ID > 0)
//            {
//                updateLocalVersion = true;
//            }
//        }
//        else
//        {
//            return Ok();
//        }

//        long itemId = 0;
//        using (SqlConnection connection = new SqlConnection(AdmConnStr))
//        {
//            connection.Open();

//            try
//            {
//                using (var ctx = TransactionManager<SqlConnection, SqlTransaction>.GetManager(AdmConnStr, false))
//                {

//                    DataPortal<ZoteroItemIDByItemReview> dp = new DataPortal<ZoteroItemIDByItemReview>();
//                    ZoteroItemIDByItemReviewCriteria criteria =
//                            new ZoteroItemIDByItemReviewCriteria(zoteroItem.ITEM_REVIEW_ID, ri.ReviewId);

//                    var resultItemID = await dp.FetchAsync(criteria);

//                    DataPortal<Item> dpFetchItem = new DataPortal<Item>();
//                    SingleCriteria<Item, Int64> criteriaItem =
//                        new SingleCriteria<Item, long>(itemId);

//                    var itemFetch = dpFetchItem.Fetch(criteriaItem);

//                    //NB THIS METHOD WILL NOW FAIL!
//                    //await UpdateMiddleAndLeftTableItem(collection);

//                    // TODO check update has happened as expected
//                    itemFetch = dpFetchItem.Execute(itemFetch);

//                    zoteroItem.ItemKey = receivedZoteroItem.key;
//                    zoteroItem.ITEM_REVIEW_ID = zoteroItem.ITEM_REVIEW_ID;
//                    zoteroItem.LAST_MODIFIED = DateTime.Now;
//                    zoteroItem.LibraryID = collection.library.id.ToString();
//                    zoteroItem.Version = receivedZoteroItem.version ?? 0;

//                    zoteroItem = dpFetch.Update(zoteroItem);
//                    ctx.Commit();
//                }

//            }
//            catch (Exception e)
//            {
//                _logger.LogException(e, "Error with ItemsItemsIdLocalPost");
//                return StatusCode(500, e.Message);
//            }
//        }
//        return Ok(zoteroItem);
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "ItemsItemsIdLocalPost has an error");
//        return StatusCode(500, e.Message);
//    }
//}

//// TODO THIS SHOULD BE INSIDE THE MAPPING CLASS
//private int MapFromZoteroTypeToERWebTypeID(string zoteroItemType)
//{
//    int erWebTypeId = -1;

//    //1   Report
//    //2   Book, Whole
//    //3   Book, Chapter
//    //4   Dissertation
//    //5   Conference Proceedings
//    //6   Document From Internet Site
//    //7   Web Site
//    //8   DVD, Video, Media
//    //9   Research project
//    //10  Article In A Periodical
//    //11  Interview
//    //12  Generic
//    //14  Journal, Article
//    switch (zoteroItemType)
//    {
//        case "journalArticle":
//            erWebTypeId = 14;
//            break;
//        case "attachment":
//            erWebTypeId = 12;
//            break;
//        default:
//            // code block
//            break;
//    }
//    return erWebTypeId;

//}

//[HttpGet("[action]")]
//public async Task<IActionResult> ErWebDocumentExists([FromQuery] long itemReviewId)
//{
//    try
//    {
//        if (!SetCSLAUser()) return Unauthorized();

//        DataPortal<ZoteroItemReview> dpItemReview = new DataPortal<ZoteroItemReview>();
//        SingleCriteria<ZoteroItemReview, long> criteriaItemReview =
//            new SingleCriteria<ZoteroItemReview, long>(itemReviewId);
//        var parentItemID = dpItemReview.Fetch(criteriaItemReview);

//        if (parentItemID.ITEM_ID > -1)
//        {
//            DataPortal<ItemDocument> dpDoc = new DataPortal<ItemDocument>();
//            SingleCriteria<ItemDocument, long> criteriaDoc =
//                new SingleCriteria<ItemDocument, long>(parentItemID.ITEM_ID);
//            var doc = dpDoc.Fetch(criteriaDoc);

//            if (doc.ItemDocumentId > 0)
//            {
//                return Ok(true);
//            }
//            else
//            {
//                return Ok(false);
//            }
//        }
//        else
//        {
//            // TODO error
//        }

//        return Ok();
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "ErWebDocumentExists has an error");
//        return StatusCode(500, e.Message);
//    }
//}

//[HttpPost("[action]")]
//public async Task<IActionResult> GroupsGroupIdItems([FromBody] string[] itemIds)
//{

//    var items = await ItemReviewIdsLocal(itemIds);

//    List<Item> localItems = new List<Item>();
//    List<CollectionData> zoteroItems = new List<CollectionData>();
//    List<ErWebZoteroItemDocument> erWebZoteroItemDocs = new List<ErWebZoteroItemDocument>();

//    var failedItemsMsg = "These items failed when posting to Zotero: ";
//    var numberOfFailedItems = 0;

//    if (items.Count == 0) return Ok("false");

//    try
//    {
//        if (!SetCSLAUser4Writing()) return Unauthorized();
//        (ZoteroReviewConnection zrc, string groupIDBeingSynced) = CheckPermissionsWithZoteroKey();
//        // 0 -dataportal fetch for documents
//        var itemIDsWithDocuments = items.Where(x => x.itemDocumentID > 0);

//        // 1 - dataportal fetch for items
//        var itemIDsWithoutDocuments = items.Where(x => x.itemDocumentID == 0);
//        DataPortal<Item> dp = new DataPortal<Item>();

//        foreach (var item in itemIDsWithoutDocuments)
//        {
//            SingleCriteria<Item, Int64> criteria =
//                new SingleCriteria<Item, Int64>(item.itemID);

//            var itemResult = await dp.FetchAsync(criteria);
//            localItems.Add(itemResult);
//        }

//        var POSTItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/");
//        SetZoteroHttpService(POSTItemUri, zrc.ApiKey);

//        // 2 - map this item to an object that can be a valid payload (difficult)
//        var count = 0;
//        HttpResponseMessage response = new HttpResponseMessage();
//        foreach (var item in localItems)
//        {
//            var erWebItem = new ERWebItem
//            {
//                Item = item
//            };
//            var zoteroReference = _concreteReferenceCreator.GetReference(erWebItem);
//            var zoteroItem = zoteroReference.MapReferenceFromErWebToZotero();
//            zoteroItems.Add(zoteroItem);
//        }

//        if (zoteroItems.Count() > 0)
//        {
//            var payload = JsonConvert.SerializeObject(zoteroItems);

//            response = await _zoteroService.CreateItem(payload, POSTItemUri.ToString());
//            var actualContent = await response.Content.ReadAsStringAsync();
//            if (actualContent.Contains("success"))
//            {
//                JObject keyValues = JsonConvert.DeserializeObject<JObject>(actualContent);

//                numberOfFailedItems = keyValues["failed"].Count();
//                foreach (var item in keyValues["failed"].Children())
//                {
//                    failedItemsMsg += item.FirstOrDefault()["message"];
//                }

//                var version = Convert.ToInt64(keyValues["successful"]["0"]["version"].ToString());
//                var libraryId = keyValues["successful"]["0"]["library"]["id"].ToString();

//                foreach (var item in keyValues["success"].Children())
//                {
//                    List<object> values = new List<object>();
//                    var key = "";
//                    foreach (var keyJson in item.Children())
//                    {
//                        key = keyJson.ToString() ?? "";
//                    }

//                    zoteroItems[count].key = key;
//                    var itemReviewID = items.FirstOrDefault(x => x.itemID == localItems[count].ItemId).itemReviewID;
//                    var zoteroItemToInsert = new ZoteroReviewItem
//                    {
//                        ItemKey = key,
//                        ITEM_REVIEW_ID = itemReviewID,
//                        LAST_MODIFIED = DateTime.Now,
//                        LibraryID = libraryId,
//                        Version = version,
//                        TypeName = localItems[count].TypeName
//                    };

//                    DataPortal<ZoteroReviewItem> dp2 = new DataPortal<ZoteroReviewItem>();
//                    zoteroItemToInsert = dp2.Execute(zoteroItemToInsert);

//                    count++;
//                }

//            }
//        }

//        //TODO index is incorrect somehow getting documents attacjed to wrong parents
//        if (itemIDsWithDocuments.Count() > 0)
//        {
//            // TODO advanced:  if the key is not present for the associated item to an itemDocument
//            // then find this item locally and push to Zotero 
//            // next get the key and continue as expected
//            // for some reason have to do this again it loses the context!!!
//            // TODO:  
//            if (!SetCSLAUser4Writing()) return Unauthorized();
//            zoteroItems.Clear();
//            int countItems = 0;
//            var itemIDsDistinctWithDocuments = itemIDsWithDocuments.Select(x => x.itemID).Distinct().ToList();
//            foreach (var itemId in itemIDsDistinctWithDocuments)
//            {
//                var itemDoc = itemIDsWithDocuments.FirstOrDefault(y => y.itemID == itemId);
//                DataPortal<Item> dpItem = new DataPortal<Item>();
//                SingleCriteria<Item, Int64> criteria = new SingleCriteria<Item, long>(itemDoc.itemID);
//                var item = dpItem.Fetch(criteria);
//                var erWebItem = new ERWebItem
//                {
//                    Item = item
//                };
//                var zoteroReference = _concreteReferenceCreator.GetReference(erWebItem);
//                var zoteroItem = zoteroReference.MapReferenceFromErWebToZotero();
//                zoteroItems.Add(zoteroItem);
//                countItems++;
//            }

//            //PUSH items that have documents
//            var payload = JsonConvert.SerializeObject(zoteroItems);
//            count = 0;
//            response = await _zoteroService.CreateItem(payload, POSTItemUri.ToString());
//            var actualContent = await response.Content.ReadAsStringAsync();
//            if (actualContent.Contains("success"))
//            {
//                //TODO since this parent item is now in Zotero we need to insert
//                // into the ERWebZotero table as a record  
//                JObject keyValues = JsonConvert.DeserializeObject<JObject>(actualContent);

//                numberOfFailedItems = keyValues["failed"].Count();
//                foreach (var item in keyValues["failed"].Children())
//                {
//                    failedItemsMsg += item.FirstOrDefault()["message"];
//                }

//                var version = Convert.ToInt64(keyValues["successful"]["0"]["version"].ToString());
//                var libraryId = keyValues["successful"]["0"]["library"]["id"].ToString();

//                foreach (var item in keyValues["success"].Children())
//                {
//                    List<object> values = new List<object>();
//                    var key = "";
//                    foreach (var keyJson in item.Children())
//                    {
//                        key = keyJson.ToString() ?? "";
//                    }
//                    var itemWithDoc = itemIDsWithDocuments.FirstOrDefault(x => x.itemID == itemIDsDistinctWithDocuments[count]);
//                    // okay what I am doing here is just uploading the first document associated with the item in question
//                    // TODO advanced add all documents associated with an item...
//                    // for now have to repeat process... of pushnig items...
//                    if (itemWithDoc != null)
//                    {
//                        var itemDoc = new ErWebZoteroItemDocument
//                        {
//                            itemId = itemWithDoc.itemID,
//                            parentItemFileKey = key,
//                            itemDocumentId = itemWithDoc.itemDocumentID
//                        };
//                        erWebZoteroItemDocs.Add(itemDoc);
//                    }
//                    zoteroItems[count].key = key;
//                    var itemReviewID = items.FirstOrDefault(x => x.itemID == itemIDsWithDocuments.ToList()[count].itemID).itemReviewID;
//                    var zoteroItemToInsert = new ZoteroReviewItem
//                    {
//                        ItemKey = key,
//                        ITEM_REVIEW_ID = itemReviewID,
//                        LAST_MODIFIED = DateTime.Now,
//                        LibraryID = libraryId,
//                        Version = version,
//                        TypeName = "Journal, Article" //TODO magic string 
//                    };
//                    // for some reason have to do this again it loses the context!!!
//                    // TODO:  
//                    if (!SetCSLAUser4Writing()) return Unauthorized();
//                    DataPortal<ZoteroReviewItem> dp2 = new DataPortal<ZoteroReviewItem>();
//                    zoteroItemToInsert = dp2.Execute(zoteroItemToInsert);
//                    count++;
//                }
//            }
//            if (erWebZoteroItemDocs.Count() > 0)
//            {
//                await UploadERWebDocumentsToZoteroAsync(erWebZoteroItemDocs, zrc.REVIEW_ID);
//            }
//        }
//        if (numberOfFailedItems == 0)
//        {
//            return Ok("true");
//        }
//        else
//        {
//            return Ok(failedItemsMsg);
//        }
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "UpdateZoteroObjectInERWebAsync has an error");
//        var message = "";
//        if (e.Message.Contains("403"))
//        {
//            message += "No Zotero API Token; either it has been revoked or never created";
//        }
//        else
//        {
//            message += e.Message;
//        }
//        return StatusCode(500, message);
//    }
//}


//[HttpGet("[action]")]
//public async Task<List<iItemReviewID>> ItemReviewIdsLocal(string[] itemIds)
//{
//    try
//    {
//        if (!SetCSLAUser()) return new List<iItemReviewID>();
//        ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
//        if (ri == null) throw new ArgumentNullException("Not sure why this is null");


//        DataPortal<ZoteroReviewItemsNotInZotero> dp = new DataPortal<ZoteroReviewItemsNotInZotero>();
//        SingleCriteria<Item, Int64> criteria =
//            new SingleCriteria<Item, long>(ri.ReviewId);
//        var zoteroReviewItemsNotInZotero = dp.Fetch(criteria);

//        List<iItemReviewID> itemReviewIDs = new List<iItemReviewID>();
//        foreach (var item in zoteroReviewItemsNotInZotero)
//        {
//            var itemReviewID = new iItemReviewID
//            {
//                itemID = item.ITEM_ID,
//                itemReviewID = item.ITEM_REVIEW_ID,
//                itemDocumentID = item.ITEM_DOCUMENT_ID
//            };
//            if (itemIds.Contains(item.ITEM_ID.ToString()))
//            {
//                itemReviewIDs.Add(itemReviewID);
//            }
//        }
//        // TODO only ever do the first ten items in dev and staging only in production optimise for lots
//        return itemReviewIDs;
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "ItemReviewIdsLocalGet has an error");
//        return new List<iItemReviewID>();
//    }
//}

//[HttpPut("[action]")]
//public async Task<IActionResult> GroupsGroupdIdItems([FromBody] List<iItemReviewIDZoteroKey> items)
//{
//    List<Item> localItems = new List<Item>();
//    List<BookWhole> zoteroItems = new List<BookWhole>();

//    try
//    {
//        if (!SetCSLAUser4Writing()) return Unauthorized();
//        ZoteroReviewConnection zrc = ApiKey();
//        string groupIDBeingSynced = zrc.LibraryId;

//        var itemIDs = items.Select(x => x.itemID);
//        var itemkey = items.Select(x => x.itemKey);
//        var zoteroItemContent = await ItemsItemId(itemkey.FirstOrDefault());

//        if (!SetCSLAUser4Writing()) return Unauthorized();


//        foreach (var itemID in itemIDs)
//        {
//            DataPortal<Item> dpItemFetch = new DataPortal<Item>();
//            SingleCriteria<Item, Int64> criteriaItem =
//                new SingleCriteria<Item, Int64>(itemID);

//            var itemResult = await dpItemFetch.FetchAsync(criteriaItem);
//            localItems.Add(itemResult);
//        }
//        var _httpClient = new HttpClient();
//        _httpClient.DefaultRequestHeaders.Add("Zotero-API-Version", "3");
//        _httpClient.DefaultRequestHeaders.Add("Zotero-API-Key", zrc.ApiKey);
//        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Unmodified-Since-Version", zoteroItemContent.version.ToString());

//        HttpClientProvider httpClientProvider = new HttpClientProvider(_httpClient);
//        _zoteroService.SetZoteroServiceHttpProvider(httpClientProvider);

//        // 2 - map this item to an object that can be a valid payload (difficult)
//        var count = 0;
//        HttpResponseMessage response = new HttpResponseMessage();
//        foreach (var item in localItems)
//        {
//            var payload = JsonConvert.SerializeObject(zoteroItemContent);

//            var PUTItemUri = new UriBuilder($"{baseUrl}/groups/{groupIDBeingSynced}/items/" + itemkey.FirstOrDefault());
//            _httpClient.BaseAddress = new Uri(PUTItemUri.ToString());

//            response = await _zoteroService.UpdateZoteroItem<ZoteroReviewItem>(payload, PUTItemUri.ToString());
//            var actualCode = response.StatusCode;
//            if (actualCode == System.Net.HttpStatusCode.NoContent)
//            {
//                zoteroItemContent = await ItemsItemId(itemkey.FirstOrDefault());

//                DataPortal<ZoteroItemReview> dp = new DataPortal<ZoteroItemReview>();
//                SingleCriteria<ZoteroItemReview, long> criteria =
//              new SingleCriteria<ZoteroItemReview, long>(item.ItemId);

//                var zoteroReviewItem = dp.Fetch(criteria);

//                DataPortal<ZoteroReviewItem> dp2 = new DataPortal<ZoteroReviewItem>();
//                SingleCriteria<ZoteroReviewItem, string> criteria2 =
//              new SingleCriteria<ZoteroReviewItem, string>(zoteroReviewItem.ITEM_REVIEW_ID.ToString());

//                var zoteroReviewItemFetch = dp2.Fetch(criteria);

//                zoteroReviewItemFetch.ItemKey = itemkey.FirstOrDefault();
//                zoteroReviewItemFetch.Version = zoteroItemContent.version;

//                // TODO this might need to be changed to an update somehow...
//                zoteroReviewItemFetch = dp2.Execute(zoteroReviewItemFetch);

//            }
//        }
//        return Ok(true);
//    }
//    catch (Exception e)
//    {
//        _logger.LogException(e, "Update Zotero Object In ERWeb has an error");
//        return StatusCode(500, e.Message);
//    }
//}

//private Item UpdateItemWithZoteroItem(Collection zoteroItem)
//{
//    try
//    {
//        var concreteReferenceCreator = ConcreteReferenceCreator.Instance;
//        var reference = concreteReferenceCreator.GetReference(zoteroItem);
//        //CHANGE from SG 26/08/2022 which is needed to compile, but code below clearly can't work :-(
//        var erWebItem = reference.MapReferenceFromZoteroToErWeb(new Item());

//        DataPortal<Item> dp = new DataPortal<Item>();
//        var updatedErWebItem = dp.Update(erWebItem.Item);
//        return updatedErWebItem;
//    }
//    catch (Exception)
//    {
//        throw new Exception("Error updating erWebItem with Zotero item");
//    }
//}