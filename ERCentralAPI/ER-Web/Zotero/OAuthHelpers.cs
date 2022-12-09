using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ERxWebClient2.Zotero
{
    public sealed class OAuthHelper
    {
        private readonly static object padlock = new object();
        private static OAuthHelper instance = null;

		private OAuthHelper()
		{

		}

        public static OAuthHelper Instance
        {
            get {

				lock (padlock)
				{
					if (instance == null)
					{
                        instance = new OAuthHelper();
					}
                    return instance;
				}
            }
        }

        public enum SignatureTypes
        {
            HMACSHA1,
            PLAINTEXT,
            RSASHA1
        }

        public class QueryParameter
        {
            private string name = null;
            private string value = null;

            public QueryParameter(string name, string value)
            {
                this.name = name;
                this.value = value;
            }

            public string Name
            {
                get { return name; }
            }

            public string Value
            {
                get { return value; }
            }
        }

        public class QueryParameterComparer : IComparer<QueryParameter>
        {


            public int Compare(QueryParameter x, QueryParameter y)
            {
                if (x.Name == y.Name)
                {
                    return string.Compare(x.Value, y.Value);
                }
                else
                {
                    return string.Compare(x.Name, y.Name);
                }
            }

        }

        public const string OAuthVersion = "1.0";
        static string OAuthParameterPrefix = "oauth_";
        public static bool includeVersion = true;
        public const string OAuthConsumerKeyKey = "oauth_consumer_key";
        public const string OAuthCallbackKey = "oauth_callback";
        public const string OAuthVersionKey = "oauth_version";
        public const string OAuthSignatureMethodKey = "oauth_signature_method";
        public const string OAuthSignatureKey = "oauth_signature";
        public const string OAuthTimestampKey = "oauth_timestamp";
        public const string OAuthNonceKey = "oauth_nonce";
        public const string OAuthTokenKey = "oauth_token";
        public const string OAuthTokenSecretKey = "oauth_token_secret";
        public const string OAuthVerifier = "oauth_verifier";

        public const string HMACSHA1SignatureType = "HMAC-SHA1";
        public const string PlainTextSignatureType = "PLAINTEXT";
        public const string RSASHA1SignatureType = "RSA-SHA1";

        public static Random random = new Random();

        public static string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        private static string calculateHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        private static List<QueryParameter> getQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            List<QueryParameter> result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }

            return result;
        }
        public static string EncodeURL(string value)
        {
            StringBuilder result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }

        public static string normalizeParameters(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            QueryParameter p = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                if (p.Name.StartsWith("oauth"))
                {
                    sb.AppendFormat("{0}={1}", p.Name, EncodeURL(p.Value));
                }
                else
                {
                    sb.AppendFormat("{0}={1}", p.Name, p.Value);
                }

                if (i < parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }

        public static string signatureBase(Uri url, string consumerKey, string token, string tokenSecret, string httpMethod, IDictionary<string, string> postParams, string timeStamp, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters, string oAuthVersion, string verifier)
        {
            if (token == null)
            {
                token = string.Empty;
            }

            if (tokenSecret == null)
            {
                tokenSecret = string.Empty;
            }

            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentNullException("httpMethod");
            }

            if (string.IsNullOrEmpty(signatureType))
            {
                throw new ArgumentNullException("signatureType");
            }

            normalizedUrl = null;
            normalizedRequestParameters = null;

            List<QueryParameter> parameters = getQueryParameters(url.Query);
            if (!String.IsNullOrEmpty(oAuthVersion))
            {
                parameters.Add(new QueryParameter(OAuthVersionKey, oAuthVersion));
            }
            if (postParams != null & httpMethod.ToUpper() == "POST")
            {
                foreach (var key in postParams.Keys)
                {
                    parameters.Add(new QueryParameter(key, postParams[key]));
                }
            }
            parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
            parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
            parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType));
            parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));
            parameters.Add(new QueryParameter(OAuthVerifier, verifier));

            if (!string.IsNullOrEmpty(token))
            {
                parameters.Add(new QueryParameter(OAuthTokenKey, token));
            }

            parameters.Sort(new QueryParameterComparer());

            normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
            {
                normalizedUrl += ":" + url.Port;
            }
            normalizedUrl += url.AbsolutePath;
            normalizedRequestParameters = normalizeParameters(parameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
            signatureBase.AppendFormat("{0}&", EncodeURL(normalizedUrl));
            signatureBase.AppendFormat("{0}", EncodeURL(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        public static string createSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return calculateHash(hash, signatureBase);
        }

        public static string createSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, string verifier, out string normalizedUrl, out string normalizedRequestParameters, Dictionary<string, string> postParameters)
        {
            return createSignature(url, consumerKey, consumerSecret, token, tokenSecret, httpMethod, timeStamp, nonce, SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters, postParameters, OAuthVersion, verifier);
        }

        public static string createSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters, IDictionary<string, string> postParameters, string oAuthVersion, string verifier)
        {
            normalizedUrl = null;
            normalizedRequestParameters = null;

            switch (signatureType)
            {
                case SignatureTypes.PLAINTEXT:
                    return HttpUtility.UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
                case SignatureTypes.HMACSHA1:
                    var signatureBase = OAuthHelper.signatureBase(url, consumerKey, token, tokenSecret, httpMethod, postParameters, timeStamp, nonce, HMACSHA1SignatureType, out normalizedUrl, out normalizedRequestParameters, oAuthVersion, verifier);

                    HMACSHA1 hmacsha1 = new HMACSHA1();
                    hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", EncodeURL(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : EncodeURL(tokenSecret)));

                    return createSignatureUsingHash(signatureBase, hmacsha1);
                case SignatureTypes.RSASHA1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
        }

        public static string timestamp(double additionalTimeForUserToClick)
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			if (additionalTimeForUserToClick > 0)
			{
                ts.Add(TimeSpan.FromSeconds(additionalTimeForUserToClick));
            }            
            string timeStamp = ts.TotalSeconds.ToString();
            timeStamp = timeStamp.Substring(0, timeStamp.IndexOf("."));
            return timeStamp;
        }

        public static string nonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return random.Next(123400, 9999999).ToString();
        }

    }
}
