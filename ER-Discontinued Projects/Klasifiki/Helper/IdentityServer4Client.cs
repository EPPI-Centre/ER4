using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EPPIDataServices.Helpers
{
    public class IdentityServer4Client
    {
        private static TokenClient _tokenClient;
        private static DiscoveryResponse _disco;
        private static bool _isSpoofForTesting = false;
        public static IdentityServer4Client GetSpoofClientForTesting()
        {
            return new IdentityServer4Client();
        }

        private IdentityServer4Client()
        {
            _isSpoofForTesting = true;
        }

        public IdentityServer4Client(Microsoft.Extensions.Configuration.IConfiguration conf)
        {
            string EPPIApiClientSecret = conf["AppSettings:EPPIApiClientSecret"];
            string EPPIApiUrl = conf["AppSettings:EPPIApiUrl"];
            string EPPIApiClientName = conf["AppSettings:EPPIApiClientName"];
            //_tokenClient = new TokenClient(
            //	disco.TokenEndpoint,
            //	"aaa",
            //	"dataEventRecordsSecret2");
            _disco = DiscoveryClient.GetAsync(EPPIApiUrl).Result;
            if (_disco.IsError) throw new Exception(_disco.Error);
            _tokenClient = new TokenClient(
                _disco.TokenEndpoint,
                EPPIApiClientName,
                EPPIApiClientSecret);

        }

        public static async Task<(bool, TokenResponse)> LoginAsync(string user, string password, ClaimsIdentity userIdentity)
        {
            ////branch out if we're running tests
            //if (_isSpoofForTesting)
            //    return await SpoofLoginForTestingAsync(user, password, userIdentity);

            TokenResponse Tokens = await RequestAccessTokensAsync(user, password);
            //check what we've got! (return false if login failed)
            if (Tokens.Error != null || Tokens.AccessToken == null) return (false, Tokens);
            //next: find user data
            var userInfoClient = new UserInfoClient(_disco.UserInfoEndpoint);
            var response = await userInfoClient.GetAsync(Tokens.AccessToken);
            userIdentity.AddClaims(response.Claims);
           
            //now save them in RAVEN?
            return (true, Tokens);
        }
        private static async Task<bool> SpoofLoginForTestingAsync(string user, string password, ClaimsIdentity userIdentity)
        {
            if (user.ToLower() != "alice" || password != "123") return false;
            if (user.ToLower() == "alice" && password == "123")
            {//valid user, put required claims in
                List<Claim> claims = new List<Claim>();
                Claim displayname = new Claim(ClaimTypes.Name, "Alice Fake");
                claims.Add(displayname);
                Claim globalid = new Claim(ClaimTypes.PrimarySid, "-1");
                claims.Add(globalid);
                Claim email = new Claim(ClaimTypes.Email, "alice@eppireviewer.org.uk");
                claims.Add(email);
                userIdentity.AddClaims(claims);
                return true;
            }
            else return false;
        }


        public static async Task RunRefreshAsync(TokenResponse response, int milliseconds)
        {
            var refresh_token = response.RefreshToken;

            //while (true)
            //{
            response = await RefreshTokenAsync(refresh_token);

            // Get the resource data using the new tokens...
            //await ResourceDataClient.GetDataAndDisplayInConsoleAsync();
            //await ResourceDataClient.AskWithoutAuthorisation();
            //await ResourceDataClient.AskWithAuthorisation(response.AccessToken);


            if (response.RefreshToken != refresh_token)
            {
                //ShowResponse(response);
                refresh_token = response.RefreshToken;
            }

            Task.Delay(milliseconds).Wait();
            //}
        }
        private static async Task<TokenResponse> RequestAccessTokensAsync(string user, string password)
        {
            
            return await _tokenClient.RequestResourceOwnerPasswordAsync(
                user,
                password,
                "email openid offline_access");
        }
        private static async Task<TokenResponse> RequestUserInfoAsync(string user, string password)
        {
            return await _tokenClient.RequestResourceOwnerPasswordAsync(
                user,
                password,
                "email openid offline_access");
        }

        private static async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            //Log.Logger.Verbose("Using refresh token: {RefreshToken}", refreshToken);

            return await _tokenClient.RequestRefreshTokenAsync(refreshToken);
        }

    }
}
