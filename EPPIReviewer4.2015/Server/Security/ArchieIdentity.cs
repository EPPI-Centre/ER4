﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using Csla.DataPortalClient;



#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using System.Net;
using System.IO;
using System.Xml.Linq;
using BusinessLibrary.BusinessClasses;
using Newtonsoft.Json;

#endif




namespace BusinessLibrary.Security
{

    [Serializable]

    public class ArchieIdentity : BusinessBase<ArchieIdentity>
    {
        public ArchieIdentity()
        { }

        public static readonly PropertyInfo<string> ArchieIDProperty = RegisterProperty<string>(new PropertyInfo<string>("ArchieID", "ArchieID"));
        public string ArchieID
        {
            get
            {
                return GetProperty(ArchieIDProperty);
            }
            private set
            {
                SetProperty(ArchieIDProperty, value);
            }
        }
        //private static PropertyInfo<string> NameProperty = RegisterProperty<string>(new PropertyInfo<string>("Name", "Name"));
        //public string Name
        //{
        //    get
        //    {
        //        return GetProperty(NameProperty);
        //    }
        //    set
        //    {
        //        SetProperty(NameProperty, value);
        //    }
        //}
        public bool IsAuthenticated
        {
            get
            {
#if !SILVERLIGHT
                if (Token != null && Token != ""
                    && RefreshToken != null && RefreshToken != "")
                {//Token and RefreshT never reach Silverlight side!
                    return true;
                }
#else
                if (ArchieID != null && ArchieID != "")
                {
                    return true;
                }
#endif
                return false;
            }

        }
        public static readonly PropertyInfo<string> ErrorProperty = RegisterProperty<string>(new PropertyInfo<string>("Error", "Error"));
        public string Error
        {
            get
            {
                return GetProperty(ErrorProperty);
            }
            private set
            {
                SetProperty(ErrorProperty, value);
            }
        }
        public static readonly PropertyInfo<string> ErrorReasonProperty = RegisterProperty<string>(new PropertyInfo<string>("ErrorReason", "ErrorReason"));
        public string ErrorReason
        {
            get
            {
                return GetProperty(ErrorReasonProperty);
            }
            private set
            {
                SetProperty(ErrorReasonProperty, value);
            }
        }

#if !SILVERLIGHT

        private static class ser
        {
            public static object DeserializeObjectToDictionary(string json)
            {
                try
                {
                    var res = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    return res;
                }
                catch
                {
                    return null;
                }
            }
            public static object DeserializeObject(string json)
            {
                try
                {
                    var res = JsonConvert.DeserializeObject(json);
                    return res;
                }
                catch
                {
                    return null;
                }
            }
        }
        //string authInfo = Convert.ToBase64String(Encoding.Default.GetBytes("eppi" + ":" + "k45m19g80")).Trim();
        string authInfo = Convert.ToBase64String(Encoding.Default.GetBytes("eppi" + ":" + CochraneOAuthSS)).Trim();
        internal static ArchieIdentity GetArchieIdentity(string code, string status)
        {
            ArchieIdentity res = new ArchieIdentity(code, status, false);
            return res;
        }
        internal static ArchieIdentity GetArchieIdentityFromLocalDB(string code, string status)
        {
            ArchieIdentity res = new ArchieIdentity(code, status, true);
            return res;
        }
        //internal static ArchieIdentity GetArchieIdentity(ReviewerIdentity Ri)
        //{
        //    ArchieIdentity res = new ArchieIdentity(Ri, false);
        //    return res;
        //}
        internal static ArchieIdentity GetArchieIdentity(ReviewerIdentity Ri, bool NoReviewCheck)
        {
            ArchieIdentity res = new ArchieIdentity(Ri, NoReviewCheck);
            return res;
        }
        internal static ArchieIdentity GetUnidentifiedArchieIdentity(string code, string status, int ContactID)
        {
            ArchieIdentity res = new ArchieIdentity(code, status, ContactID);
            return res;
        }
        private ArchieIdentity(string code, string status, bool FromLocal)
        {//constructor that requires to validate AccessCode & status
         //this may be done in two ways: by validating them in Archie (first logon, not accessing a review) and retrieve new Tokens
         //or by validating them against the ER4 DB, this is used when accessing a review (archie will not validate the code+status anymore)
         //also used when the tokens failed and the user Re-authenticated in Archie via the popup (fromLocal == false)
#if (CSLA_NETCORE)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#else
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
#endif
            _code = code;
            _status = status;
            //string dest = AccountBaseAddress + "oauth2/token";
            string dest = oAuthBaseAddress + "realms/cochrane/protocol/openid-connect/token";
            string json = "";
            Dictionary<string, object> dict;
            System.Collections.Specialized.NameValueCollection nvcoll = new System.Collections.Specialized.NameValueCollection();

            if (!FromLocal)//we need to authenticate on Archie
            {
                string redirect = Redirect;
                //see http://code.pearson.com/pearson-learningstudio/apis/authentication/authentication-sample-code/sample-code-oauth-2-c_x
                //and http://stackoverflow.com/questions/2764577/forcing-basic-authentication-in-webrequest


                WebClient webc = new WebClient();
                webc.Headers[HttpRequestHeader.Authorization] = "Basic " + authInfo;
                nvcoll.Add("code", _code);//apparently Uri.EscapeDataString(...) breaks this, so no escaping is done.
                nvcoll.Add("redirect_uri", redirect);
                nvcoll.Add("grant_type", "authorization_code");

                Token = "";
                RefreshToken = "";
                Error = "";
                ErrorReason = "";
                int ValidFor;

                try
                {
                    byte[] responseArray = webc.UploadValues(dest, "POST", nvcoll);
                    json = Encoding.ASCII.GetString(responseArray);
                }
                catch (WebException we)
                {//if request is unsuccessful, we get an error inside the WebException
                    WebResponse wr = we.Response;
                    if (wr == null)
                    {
                        throw new Exception("WebResponse is null: " + we.Message + Environment.NewLine
                            + (we.InnerException != null && we.InnerException.Message != null ? we.InnerException.Message : ""));

                    }
                    using (var reader = new StreamReader(wr.GetResponseStream()))
                    {
                        if (reader == null)
                        {
                            throw new Exception("WebResponse reader is null: " + we.Message + Environment.NewLine
                            + (we.InnerException != null && we.InnerException.Message != null ? we.InnerException.Message : ""));
                        }
                        json = reader.ReadToEnd();

                        webc.Dispose();
                    }
                }
                dict = (Dictionary<string, object>)ser.DeserializeObjectToDictionary(json);
                if (dict == null)
                {
                    MarkAuthenticationFailed("Access denied, could not obtain Access tokens.", "The request failed with an unexpected error.");
                    return;
                }
                if (dict.ContainsKey("access_token"))
                {
                    Token = dict["access_token"].ToString();
                }
                if (dict.ContainsKey("refresh_token"))
                {
                    RefreshToken = dict["refresh_token"].ToString();
                }
                if (Token != "" && dict.ContainsKey("expires_in"))
                {
                    int.TryParse(dict["expires_in"].ToString(), out ValidFor);
                    if (ValidFor > 0)
                    {
                        ValidUntil = DateTime.Now.AddSeconds(ValidFor - 1);
                    }
                }
                if (dict.ContainsKey("error"))
                {
                    Error = "Code invalid, no Token retrieved: " + dict["error"].ToString();
                }
                if (dict.ContainsKey("error_description"))
                {
                    ErrorReason = dict["error_description"].ToString();
                }
                if (Error == "") VerifyUserRoles();
                if (IsAuthenticated) SaveTokens();
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ArchieIdentityFromCodeAndStatus", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ARCHIE_CODE", _code));
                        command.Parameters.Add(new SqlParameter("@ARCHIE_STATE", _status));
                        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                        {
                            if (reader.Read())
                            {
                                RefreshToken = reader.GetString("ARCHIE_REFRESH_TOKEN");
                                ValidUntil = reader.GetDateTime("ARCHIE_TOKEN_VALID_UNTIL");
                                if (ValidUntil > DateTime.Now)
                                {//token is still valid, we can use it
                                    Token = reader.GetString("ARCHIE_ACCESS_TOKEN");
                                }
                                Error = "";
                                ErrorReason = "";
                                ArchieID = reader.GetString("ARCHIE_ID");
                            }
                            else
                            {//no tokens, user needs to re-authenticate via UI
                                RefreshToken = "";
                                Token = "";
                                Error = "No Archie Tokens";
                                ErrorReason = "User has no Tokens saved";
                                ArchieID = "";
                            }
                        }
                    }
                    if (Token != null && Token.Length >= 30 && RefreshToken != null && RefreshToken.Length >= 30
                            && ArchieID != null && ArchieID != "")
                    {//all is well: check token with Archie (just in case) and go back

                        VerifyUserRoles();
                        if (IsAuthenticated) SaveTokens();

                    }
                }
            }
        }


        private void VerifyUserRoles()
        {
            {
                //all data from first round trip to Cochrane (oAuth) is filled in, 
                //now, if user is authenticated, we need to check if user has some supervised role, otherwise it is a self-generated Cochrane account (open to anyone)
                //in which case access should be denied. 
                // IsAuthenticated property will return false if we wipe out the token and refresh token values...
                
                string dest = AzureSettings.CochraneAccountBaseAddress + "api/me";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dest);
                HttpWebResponse response = null;
                request.Method = "GET";
                request.MediaType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + Token);
                string result = "";//used internally to decide what to do
                string json = ""; 
                Dictionary<string, object> dict;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        json = reader.ReadToEnd();
                    }
                }
                catch (WebException we)
                {//if request is unsuccessful, we get an error inside the WebException
                    if (we.Message.StartsWith("The remote server returned an error: (401)"))
                    {
                        MarkAuthenticationFailed("Access denied, could not verify user roles", "The request to verify roles was not authorised.");                        
                        return;
                    }
                    else if (we.Response != null)
                    {
                        string error = "Access denied, could not verify user roles.";
                        string errDescr = "";
                        WebResponse wr = we.Response;
                        using (var reader = new StreamReader(wr.GetResponseStream()))
                        {
                            if (reader != null)
                            {
                                json = reader.ReadToEnd();
                                if (json != "")
                                {
                                    dict = (Dictionary<string, object>)ser.DeserializeObjectToDictionary(json);
                                    if (dict != null && dict.ContainsKey("detail"))
                                    {
                                        errDescr = dict["detail"].ToString();
                                        if (dict.ContainsKey("status"))
                                        {
                                            errDescr += " (" + dict["status"].ToString() + ")";
                                        }
                                        MarkAuthenticationFailed(error, errDescr);
                                        return;
                                    }
                                    else if (dict != null && dict.ContainsKey("error"))
                                    {
                                        errDescr = dict["error"].ToString();
                                        if (dict.ContainsKey("status"))
                                        {
                                            errDescr += " (" + dict["status"].ToString() + ")";
                                        }
                                        MarkAuthenticationFailed(error, errDescr);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    MarkAuthenticationFailed("Access denied, could not verify user roles.", "The request to verify roles failed unexpectedly.");
                    return;
                }
                //if we've reached this point, we should have a CochraneAccount serialised in the response contents
                CochraneAccount ca = new CochraneAccount();
                ca = JsonConvert.DeserializeObject<CochraneAccount>(json);
                
                if (ca == null || ca.cochraneId == "") 
                {
                    MarkAuthenticationFailed("Access denied, could not verify user roles.", "Did not receive a valid Cochrane User for verification.");
                    return;
                }
                //check for user details, does it have the isCochraneAuthor value set to true?
                if (ca.isCochraneAuthor == true)
                {
                    ArchieID = ca.cochraneId;
                    return;
                }
                else
                {
                    //user was authenticated in Cochrane, tokens retrieved, all that works.
                    //but: user does not have the isCochraneAuthor flag set to true. There are 2 possible reasons for this:
                    //1. it's a new user, who just can't get the Cochrane link/licensing as they aren't a cochrane author
                    //OR
                    //2. it's an existing user, who has now lost their CochraneAuthor status (as of Nov 2024, this can happen)
                    //So we check for this possibility and return two different errors depending on what we find.
                    //In this case, we also unlink the ER account to the now disempowered Cochrane account.
                    bool isSiteAdmin; string contactName;
                    int CID = 0;
                    try
                    {
                        ArchieID = ca.cochraneId;
                        CID = getContactID(out isSiteAdmin, out contactName);
                    }
                    finally
                    {//making extra sure we wipe the ArchieID, as we shouldn't use it!
                        ArchieID = "";
                    }
                    if (CID == 0)
                    {//easy case: new user and not a cochrane author
                        MarkAuthenticationFailed("Access denied.", "Not a Cochrane Author.");
                    }
                    else
                    {//more to do, we need to unlink this ER account from their not-licensed-anymore Cochrane account.                        
                        UnlinkERaccountFromCochraneAccount(CID, ca.cochraneId);
                        MarkAuthenticationFailed("Access denied.", "This account is no longer a Cochrane Author.");
                    }
                }

                //old code use for Archie, which is dead/dying as of Oct 2024
                ////System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                //dict = (Dictionary<string, object>)ser.DeserializeObjectToDictionary(json);
                //if (dict.ContainsKey("error"))
                //{
                //    Error = "Token Failed Validation: " + dict["error"].ToString();
                //    if (dict.ContainsKey("error_description"))
                //    {
                //        ErrorReason = dict["error_description"].ToString();
                //    }
                //}
                //else
                //{
                //    string result = "";
                //    if (dict.ContainsKey("groupRoles"))
                //    {
                //        Newtonsoft.Json.Linq.JArray meR = dict["groupRoles"] as Newtonsoft.Json.Linq.JArray;
                //        foreach (Newtonsoft.Json.Linq.JToken role in meR)
                //        {
                //            if (role["name"].ToString().ToLower() != "possible contributor"
                //                && role["name"].ToString().ToLower() != "mailing list"
                //                && role["name"].ToString().ToLower() != "other")
                //            {
                //                if (dict.ContainsKey("user"))
                //                {

                //                    var userD = dict["user"] as Newtonsoft.Json.Linq.JToken;
                //                    if (userD != null && userD["userId"] != null)
                //                    {
                //                        ArchieID = userD["userId"].ToString();
                //                        result = "OK";
                //                        break;
                //                    }
                //                    else result = "no userId for this person!";
                //                }
                //                else
                //                {
                //                    result = "no user Info returned!";
                //                    break;
                //                }
                //            }
                //        }
                //    }
                //    if (result != "OK")
                //    {
                //        Token = "";
                //        RefreshToken = "";
                //        ValidUntil = DateTime.Now.AddMonths(-1);
                //        if (result == "")
                //        {
                //            Error = "Access denied, not an Author";
                //            ErrorReason = "No suitable groupRoles found";
                //        }
                //        else
                //        {
                //            Error = "Access denied, can't verify";
                //            ErrorReason = "The call to rest/people/me returned data, but " + result;
                //        }

                //    }

                //}
            }
        }
        private void MarkAuthenticationFailed(string error, string errorReason)
        {
            Token = "";
            RefreshToken = "";
            Error = error;
            ErrorReason = errorReason;
        }
        private ArchieIdentity(string code, string status, int ContactID)
        {//this is used to create the identity using values in TB_UNASSIGNED_ARCHIE_KEYS
            //it is called after checking ER4 username and PW so we'll trust the user without checking stuff
            _code = code;
            _status = status;

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ArchieIdentityFromUnassignedCodeAndStatus", connection))
                {//SP will save the unassigned keys before resturning the tb_CONTACT record, if and only if details are correct
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CID", ContactID));
                    command.Parameters.Add(new SqlParameter("@ARCHIE_CODE", _code));
                    command.Parameters.Add(new SqlParameter("@ARCHIE_STATE", _status));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            RefreshToken = reader.GetString("ARCHIE_REFRESH_TOKEN");
                            ValidUntil = reader.GetDateTime("ARCHIE_TOKEN_VALID_UNTIL");
                            if (ValidUntil > DateTime.Now)
                            {//token is still valid, we can use it
                                Token = reader.GetString("ARCHIE_ACCESS_TOKEN");
                            }
                            Error = "";
                            ErrorReason = "";
                            ArchieID = reader.GetString("ARCHIE_ID");
                        }
                        else
                        {//no tokens, user needs to re-authenticate via UI
                            RefreshToken = "";
                            Token = "";
                            Error = "No Archie Tokens";
                            ErrorReason = "User has no Tokens saved";
                            ArchieID = "";
                        }
                    }
                }
            }
        }

        private ArchieIdentity(ReviewerIdentity Ri, bool NoReviewCheck)
        {//constructor used when retrieving the ArchieIdentity of an already logged-on user
            if (!NoReviewCheck)
            {
                int Rid = Ri.ReviewId;//this is added to force the LogonTicket validation, if current user doesn't have a valid ticket things will fail at this stage.
                if (Rid == 0)//user is not logged on, doesn't have a ticket and should not be trusted!
                {
                    ArchieID = "";
                    Error = "Invalid User";
                    ErrorReason = "Invalid User";
                    return;
                }
            }
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ArchieIdentityFromReviewer", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CID", Ri.UserId));

                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            RefreshToken = reader.GetString("ARCHIE_REFRESH_TOKEN");
                            ValidUntil = reader.GetDateTime("ARCHIE_TOKEN_VALID_UNTIL");
                            if (ValidUntil > DateTime.Now)
                            {//token is still valid, we can use it
                                Token = reader.GetString("ARCHIE_ACCESS_TOKEN");
                            }
                            Error = "";
                            ErrorReason = "";
                            ArchieID = reader.GetString("ARCHIE_ID");
                        }
                        else
                        {//no tokens, user needs to re-authenticate via UI
                            RefreshToken = "";
                            Token = "";
                            Error = "No Archie Tokens";
                            ErrorReason = "User has no Tokens saved";
                            ArchieID = "";
                        }
                    }
                }//at this point we can have three situations: Token is valid, all done
                //token is expired, try refreshing
                //no tokens need to re-authenticate
                if (Token != null && Token.Length >= 64 && RefreshToken != null && RefreshToken.Length >= 30)
                {//all good!
                    return;
                }
                else if (RefreshToken != null && RefreshToken.Length >= 30 && Error == "")
                {//need to refresh the token
                    //call the refresh API
                    //save new Token to DB
                    RefreshExpiredToken();
                }
                //no need for third case, correct errors are already set.

            }
        }


        public bool RefreshExpiredToken()
        {
            if (_RefreshToken == null || _RefreshToken.Length < 64)
            {//no tokens, user needs to re-authenticate via UI
                RefreshToken = "";
                Token = "";
                Error = "No Archie Tokens";
                ErrorReason = "User has no Tokens saved";
                return false;
            }
            string redirect = Redirect;

            //call the refresh API
            //if success, return true and save new Token to DB
            //string dest = AccountBaseAddress + "oauth2/token";
            string dest = oAuthBaseAddress + "realms/cochrane/protocol/openid-connect/token";

            System.Collections.Specialized.NameValueCollection nvcoll = new System.Collections.Specialized.NameValueCollection();
            WebClient webc = new WebClient();
            webc.Headers[HttpRequestHeader.Authorization] = "Basic " + authInfo;
            nvcoll.Add("redirect_uri", redirect);
            nvcoll.Add("refresh_token", _RefreshToken);
            nvcoll.Add("grant_type", "refresh_token");
            string json = "";

            int ValidFor;
            ValidUntil = new DateTime();
            Dictionary<string, object> dict;
            //System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();//(in System.Web.Extensions.dll)
            try
            {
                byte[] responseArray = webc.UploadValues(dest, "POST", nvcoll);
                json = Encoding.ASCII.GetString(responseArray);
                Token = "";
                RefreshToken = "";
                Error = "";
                ErrorReason = "";
            }
            catch (WebException we)
            {//if request is unsuccessful, we get an error inside the WebException
                Token = "";
                RefreshToken = "";
                Error = "";
                ErrorReason = "";
                WebResponse wr = we.Response;
                if (wr == null)
                {
                    json = "{\"error\": \"no_response\", \"error_description\": \"Could not contact webserver.\" }";
                    webc.Dispose();
                }
                else
                {
                    using (var reader = new StreamReader(wr.GetResponseStream()))
                    {
                        json = reader.ReadToEnd();
                        webc.Dispose();
                    }
                }
            }
            dict = (Dictionary<string, object>)ser.DeserializeObjectToDictionary(json);
            if (dict == null)
            {
                MarkAuthenticationFailed("Access denied, failed to refresh access token.", "The request failed with an unexpected error.");
            }
            if (dict.ContainsKey("access_token"))
            {
                Token = dict["access_token"].ToString();
            }
            if (dict.ContainsKey("refresh_token"))
            {
                RefreshToken = dict["refresh_token"].ToString();
            }
            if (Token != "" && dict.ContainsKey("expires_in"))
            {
                int.TryParse(dict["expires_in"].ToString(), out ValidFor);
                if (ValidFor > 0)
                {
                    ValidUntil = DateTime.Now.AddSeconds(ValidFor - 1);
                }
            }
            if (dict.ContainsKey("error"))
            {
                Error = "Code invalid, no Token retrieved: " + dict["error"].ToString();
            }
            if (dict.ContainsKey("error_description"))
            {
                ErrorReason = dict["error_description"].ToString();
            }
            if (Error == "")
            {//save to DB
                VerifyUserRoles();
                if (IsAuthenticated)
                {
                    SaveTokens();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            //if fail:
            {//no tokens, user needs to re-authenticate via UI
                ArchieID = "";
                RefreshToken = "";
                Token = "";
                Error = "No Archie Tokens - " + Error;
                ErrorReason = "User has no Tokens saved - " + ErrorReason;
                return false;
            }
        }
        private bool SaveTokens()
        {
            //if (Token != null && Token.Length == 64 && RefreshToken != null && RefreshToken.Length == 64
            //    && ArchieID != null && ArchieID != "")
            if (Token != null && Token.Length >= 30 && RefreshToken != null && RefreshToken.Length >= 30
                && ArchieID != null && ArchieID != "")
            {//we can do something
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ArchieSaveTokens", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ARCHIE_ID", ArchieID));
                        command.Parameters.Add(new SqlParameter("@TOKEN", Token));
                        command.Parameters.Add(new SqlParameter("@VALID_UNTIL", ValidUntil));
                        command.Parameters.Add(new SqlParameter("@REFRESH_T", RefreshToken));
                        command.Parameters.Add(new SqlParameter("@ARCHIE_CODE", _code));
                        command.Parameters.Add(new SqlParameter("@ARCHIE_STATE", _status));

                        try
                        {
                            command.ExecuteNonQuery();
                            return true;
                        }
                        catch
                        {//should we kill the current values??
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }
        public int getContactID(out bool IsSiteAdmin, out string ContactName)
        {//this is used while logging on a review, we need to get the basic user details in order to log on a review and return a full RI object
            IsSiteAdmin = false;
            ContactName = "{UnidentifiedArchieUser}";
            if (!IsAuthenticated && (RefreshToken == null || RefreshToken == ""))
            {//we could just have the refresh token!!!!!!!!!!!
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                return 0;
            }
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ArchieFindER4UserFromArchieID", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ARCHIE_ID", ArchieID));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            ContactName = reader.GetString("contact_name");
                            IsSiteAdmin = reader.GetBoolean("IS_SITE_ADMIN");
                            return reader.GetInt32("CONTACT_ID");
                        }
                        else
                        {//no tokens, user needs to re-authenticate via UI
                            RefreshToken = "";
                            Token = "";
                            Error = "No Archie Tokens";
                            ErrorReason = "User has no Tokens saved";
                            ArchieID = "";
                            return 0;
                        }
                    }
                }
            }
        }
        private void UnlinkERaccountFromCochraneAccount(int ContactId, string InvalidCochraneId)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ArchieUnlinkAccount", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ARCHIE_ID", InvalidCochraneId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ContactId));
                    command.ExecuteNonQuery();
                }
            }
        }
        private string _Token, _RefreshToken, _code, _status;
        private DateTime _ValidUntil;
        [JsonIgnore]
        public string Token
        {
            get
            {
                return _Token;
            }
            private set
            {
                _Token = value;
            }
        }
        [JsonIgnore]
        public string RefreshToken
        {
            get
            {
                return _RefreshToken;
            }
            private set
            {
                _RefreshToken = value;
            }
        }
        [JsonIgnore]
        public DateTime ValidUntil
        {
            get { return _ValidUntil; }
            private set { _ValidUntil = value; }
        }

        private string BaseAddress
        {
            get
            {
                string res = AzureSettings.CochraneAPIBaseAddress;
                if (res == null || res == "")
                {
                    throw new Exception("No Cochrane API base address!");
                }
                return res;
            }
        }


        private static string oAuthBaseAddress
        {
            get
            {
                string res = AzureSettings.CochraneoAuthBaseAddress;
                if (res == null || res == "")
                {
                    throw new Exception("No Cochrane oAuth base address!");
                }
                return res;
            }
        }
        private static string CochraneOAuthSS
        {
            get
            {
                string res = AzureSettings.CochraneOAuthSS;
                if (res == null || res == "")
                {
                    throw new Exception("No Cochrane client secret!");
                }
                return res;
            }
        }
        private string Redirect
        {
            get
            {
                string res = AzureSettings.CochraneOAuthRedirectUri;
                if (res == null || res == "")
                {
                    throw new Exception("No redirect URL value!");
                }
                return res;
            }
        }
        /// <summary>
        /// DEPRECATED - Archie used to provide answers in XML, but Archie is dead (dying as of Oct 2024), and the new API provides answers in JSON
        /// </summary>
        /// <param name="PartialEndpoint"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public XDocument GetXMLQuery(string PartialEndpoint, Dictionary<string, string> parameters)
        {
            if (!IsAuthenticated) return null;
            string dest = BaseAddress + PartialEndpoint;
            System.Collections.Specialized.NameValueCollection nvcoll = new System.Collections.Specialized.NameValueCollection();

            if (parameters != null && parameters.Count > 0)
            {
                dest += "?";
            }
            bool First = true;
            foreach (KeyValuePair<string, string> KVP in parameters)
            {
                if (!First)
                {
                    dest += "&";

                }
                else
                {
                    First = false;
                }
                dest += KVP.Key + "=" + KVP.Value;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dest);


            // Send Request & Get Response
            string json = "";
            HttpWebResponse response = null;
            request.Method = "GET";
            request.MediaType = "application/xml";
            request.Headers.Add("Authorization", "Bearer " + Token);
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }
                //json = ((WebResponse)request.GetResponse()).ToString();
            }
            catch (WebException we)
            {
                WebResponse wr = we.Response;
                using (var reader = new StreamReader(wr.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }
            }

            return XDocument.Parse(json);
        }
        public object GetJson(string PartialEndpoint, List<KeyValuePair<string, string>> parameters)
        {
            if (!IsAuthenticated) return null;
            string dest = BaseAddress + PartialEndpoint;
            System.Collections.Specialized.NameValueCollection nvcoll = new System.Collections.Specialized.NameValueCollection();

            if (parameters != null && parameters.Count > 0)
            {
                dest += "?";
            }
            bool First = true;
            foreach (KeyValuePair<string, string> KVP in parameters)
            {
                if (!First)
                {
                    dest += "&";

                }
                else
                {
                    First = false;
                }
                dest += KVP.Key + "=" + KVP.Value;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dest);

            // Send Request & Get Response
            string json = "";
            HttpWebResponse response = null;
            request.Method = "GET";
            request.MediaType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + Token);

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }
            }
            catch (WebException we)
            {
                WebResponse wr = we.Response;
                using (var reader = new StreamReader(wr.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }
            }


            Dictionary<string, object> dict = (Dictionary<string, object>)ser.DeserializeObjectToDictionary(json);
            if (dict != null && dict["Reviews"] != null)
            {
                object toParse = dict["Reviews"];
                if (toParse != null)
                {
                    var res = ser.DeserializeObject(toParse.ToString());
                    return res;
                }
            }
            return dict;
            //return XDocument.Parse(json);
        }
        public XDocument CheckOutReview(string ArchieReviewID)
        {
            if (!IsAuthenticated) return null;
            string dest = BaseAddress + "rest/reviews/" + ArchieReviewID + "/latest";
            System.Collections.Specialized.NameValueCollection nvcoll = new System.Collections.Specialized.NameValueCollection();
            //System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dest);
            string json = "";
            HttpWebResponse response = null;
            request.Method = "PUT";
            request.MediaType = "application/xml";
            request.Headers.Add("Authorization", "Bearer " + Token);
            XDocument res;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }
                //json = ((WebResponse)request.GetResponse()).ToString();
            }
            catch (WebException we)
            {
                //the web response is actually HTML
                //WebResponse wr = we.Response;
                //using (var reader = new StreamReader(wr.GetResponseStream()))
                //{
                //    json = reader.ReadToEnd();
                //}

                res = new XDocument();
                res.Add(new XElement("Error", we.Message));
                return res;
            }
            res = XDocument.Parse(json);
            return res;
        }
        public string UndoCheckOutReview(string ArchieReviewID)
        {
            if (!IsAuthenticated) return "Error: not authenticated in Archie";
            string dest = BaseAddress + "rest/reviews/" + ArchieReviewID + "/latest/undo";
            System.Collections.Specialized.NameValueCollection nvcoll = new System.Collections.Specialized.NameValueCollection();
            //System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dest);
            string json = "";
            HttpWebResponse response = null;
            request.Method = "PUT";
            request.MediaType = "application/xml";
            request.Headers.Add("Authorization", "Bearer " + Token);
            string res;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }

            }
            catch (WebException we)
            {
                //the web response is actually HTML
                WebResponse wr = we.Response;
                using (var reader = new StreamReader(wr.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }

                res = "Error: " + we.Message;
                return res;
            }
            res = "Done";
            return res;
        }

        private class CochraneAccount
        {
            public string cochraneId { get; set; } = "";
            public string prefix { get; set; } = "";
            public string firstName { get; set; } = "";
            public string middleInitial { get; set; } = "";
            public string lastName { get; set; } = "";
            public string suffix { get; set; } = "";
            public string country { get; set; } = "";
            public string membershipStatus { get; set; } = "";
            public bool isCochraneAuthor { get; set; } = false;
            public bool isCochraneCore { get; set; } = false;
        }
#endif

    }
}
