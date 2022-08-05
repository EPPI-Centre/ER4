using ERxWebClient2.Zotero;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ERxWebClient2
{
    public class OAuthParameters
    {
        private static OAuthParameters instance = null;
        private static readonly object padlock = new object();
        private string _clientKey;
        private string _clientSecret;

		private OAuthParameters()
		{

		}

        public static OAuthParameters Instance
        {
            get {

				if (instance == null)
				{
					lock (padlock)
					{
                        instance = new OAuthParameters();
                    }
                }
                return instance;
            }
        }

        public string RedirectUrl { get; set; }
        public string ClientKey { 
            get {
                lock (padlock)
                {
                    return _clientKey;
                }
            }
            set {
				lock (padlock)
				{
                    _clientKey = value;
                }
            } 
        }
        public string ClientSecret
        {
            get
            {
                lock (padlock)
                {
                    return _clientSecret;
                }
            }
            set
            {
                lock (padlock)
                {
                    _clientSecret = value;
                }
            }
        }
        public string nonce { get; set; }
        public string timeStamp { get; set; }
        public string signature { get; set; }

        protected string NormalizeParameters(SortedDictionary<string, string> parameters)
        {
            StringBuilder sb = new StringBuilder();

            var i = 0;
            foreach (var parameter in parameters)
            {
                if (i > 0)
                    sb.Append("&");

                sb.AppendFormat("{0}={1}", parameter.Key, parameter.Value);

                i++;
            }

            return sb.ToString();
        }

        private string GenerateBase(string nonce, string timeStamp, Uri url)
        {
            var parameters = new SortedDictionary<string, string>
            {
                {"oauth_consumer_key", ClientKey},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", timeStamp},
                {"oauth_nonce", nonce},
                {"oauth_version", "1.0"}
            };

            var sb = new StringBuilder();
            sb.Append("GET");
            sb.Append("&" + Uri.EscapeDataString(url.AbsoluteUri));
            sb.Append("&" + Uri.EscapeDataString(NormalizeParameters(parameters)));
            return sb.ToString();
        }

        public string GenerateSignature(string nonce, string timeStamp, Uri url)
        {
            var signatureBase = GenerateBase(nonce, timeStamp, url);
            var signatureKey = string.Format("{0}&{1}", ClientSecret, "");
            var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signatureKey));
            return Convert.ToBase64String(hmac.ComputeHash(new ASCIIEncoding().GetBytes(signatureBase)));
        }

        public string GenerateSignatureTwo(string nonce, string verifier, string token, string timeStamp, string tokenSecret, Uri url)
        {
            string requestURL = url.ToString();
            UriBuilder tokenRequestBuilder = new UriBuilder(requestURL);
            var query = HttpUtility.ParseQueryString(tokenRequestBuilder.Query);
            query["oauth_consumer_key"] = ClientKey;
            query["oauth_nonce"] = Guid.NewGuid().ToString("N");
            query["oauth_signature_method"] = "HMAC-SHA1";
            query["oauth_timestamp"] = (Math.Truncate((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds)).ToString();
            query["oauth_token"] = token;
            query["oauth_verifier"] = Uri.EscapeDataString(verifier);
            query["oauth_version"] = Uri.EscapeDataString("1.0");
            string signature = string.Format("{0}&{1}&{2}", "GET", Uri.EscapeDataString(requestURL), Uri.EscapeDataString(query.ToString()));
            string oauth_Signature = "";
            var signatureKey = string.Format("{0}&{1}", ClientSecret, tokenSecret);
            using (HMACSHA1 hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signatureKey)))
            {
                byte[] hashPayLoad = hmac.ComputeHash(Encoding.ASCII.GetBytes(signature));
                oauth_Signature = Convert.ToBase64String(hashPayLoad);
            }
            return oauth_Signature;
        }


        public string GetAuthorizationUrl(string Url)
        {
			lock (padlock)
			{
                var sb = new StringBuilder();

                nonce = Guid.NewGuid().ToString();
                timeStamp = OAuthHelper.timestamp(0);

                signature = GenerateSignature(nonce, timeStamp, new Uri(Url));

                sb.Append(GenerateQueryStringOperator(sb.ToString()) + "oauth_consumer_key=" + Uri.EscapeDataString(ClientKey));
                sb.Append("&oauth_nonce=" + Uri.EscapeDataString(nonce));
                sb.Append("&oauth_timestamp=" + Uri.EscapeDataString(timeStamp));
                sb.Append("&oauth_signature_method=" + Uri.EscapeDataString("HMAC-SHA1"));
                sb.Append("&oauth_version=" + Uri.EscapeDataString("1.0"));
                sb.Append("&oauth_signature=" + Uri.EscapeDataString(signature));

                return Url + sb.ToString();
            }           
        }

        private string GenerateQueryStringOperator(string currentUrl)
        {
            if (currentUrl.Contains("?"))
                return "&";
            else
                return "?";
        }
    }
}
