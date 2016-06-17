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

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Xml.Linq;
using System.Web;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class TranslationCommand : CommandBase<TranslationCommand>
    {
#if SILVERLIGHT
    public TranslationCommand(){}
#else
        protected TranslationCommand() { }
#endif
        private string _translatedText;
        private string _textToTranslate;
        private string _langTranslateFrom;
        private string _langTranslateTo;

        public string TranslatedText
        {
            get
            {
                return _translatedText;
            }
        }

        public string LangTranslatedTo
        {
            get
            {
                return _langTranslateTo;
            }
        }

        public TranslationCommand(string textToTranslate, string langTranslateFrom, string langTranslateTo)
        {
            _textToTranslate = textToTranslate;
            _langTranslateFrom = langTranslateFrom;
            _langTranslateTo = langTranslateTo;
        }

        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_translatedText", _translatedText);
            info.AddValue("_textToTranslate", _textToTranslate);
            info.AddValue("_langTranslateFrom", _langTranslateFrom);
            info.AddValue("_langTranslateTo", _langTranslateTo);
        }

        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _translatedText = info.GetValue<string>("_translatedText");
            _textToTranslate = info.GetValue<string>("_textToTranslate");
            _langTranslateFrom = info.GetValue<string>("_langTranslateFrom");
            _langTranslateTo = info.GetValue<string>("_langTranslateTo");
        }


#if !SILVERLIGHT

        public class AdmAccessToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string scope { get; set; }
        }

        protected override void DataPortal_Execute()
        {
            string clientID = "EPPI-Reviewer4";
            //Thread.Sleep(3000);
            string clientSecret = "EKX3mNOfy55k86oGJWGcfX5v8wmTcnxOTE8J+AbeYsg=";

            String strTranslatorAccessURI = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            String strRequestDetails = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientID), HttpUtility.UrlEncode(clientSecret));

            System.Net.WebRequest webRequest = System.Net.WebRequest.Create(strTranslatorAccessURI);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(strRequestDetails);
            webRequest.ContentLength = bytes.Length;
            using (System.IO.Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            System.Net.WebResponse webResponse;
            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch (Exception e)
            {
                _translatedText = e.Message; // not ideal, as it saves the message to the item record, but at least someone can read it there and let us know what it says!
                return;
            }

            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(AdmAccessToken));
            //Get deserialized object from JSON stream 
            AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());

            string headerValue = "Bearer " + token.access_token;

            string txtToTranslate = _textToTranslate;
            string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + System.Web.HttpUtility.UrlEncode(txtToTranslate) + "&to=" + _langTranslateTo; //+ "&from=" + _langTranslateFrom
            System.Net.WebRequest translationWebRequest = System.Net.WebRequest.Create(uri);
            translationWebRequest.Headers.Add("Authorization", headerValue);
            System.Net.WebResponse response = null;
            try
            {
                response = translationWebRequest.GetResponse();
                System.IO.Stream stream = response.GetResponseStream();
                System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                System.IO.StreamReader translatedStream = new System.IO.StreamReader(stream, encode);
                System.Xml.XmlDocument xTranslation = new System.Xml.XmlDocument();
                xTranslation.LoadXml(translatedStream.ReadToEnd());
                _translatedText = xTranslation.InnerText;
            }
            catch (Exception e)
            {
                _translatedText = e.Message; // not ideal, as it saves the message to the item record, but at least someone can read it there and let us know what it says!
            }
        }

#endif
    }
}
