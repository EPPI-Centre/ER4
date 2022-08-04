using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PubmedImport
{
    public static class StringExtensions
    {
        private static readonly Lazy<Regex> alphaNumericRegex = new Lazy<Regex>(() => new Regex("[^a-zA-Z0-9]"));

		private static readonly Lazy<Regex> codesetRegex = new Lazy<Regex>(() => new Regex(@"(codeset\/\d+)"));

		public static bool IsEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool NotEmpty(this string s)
        {
            return !s.IsEmpty();
        }

        public static bool HasText(this string s)
        {
            if (s == null)
                return false;

            return s.Replace(" ", "").NotEmpty();
        }

	    public static string ToNotNull(this string s)
	    {
		    if (s == null)
			    return "";

		    return s;
	    }

		public static string CleanFreetext(this string s)
		{
			if (string.IsNullOrWhiteSpace(s)) return "";
			s = s.Replace("\t", " ");
			while (s.Contains("  "))
				s = s.Replace("  ", " ");

			return s.Trim();
		}

		public static IEnumerable<string> ToWordList(this string s)
		{
			return Regex.Replace(s.ToLowerInvariant(), "[^a-z0-9 ]", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct() ;
		}

        public static bool IsDateTime(this string s)
        {
            DateTime date;
            return DateTime.TryParse(s, out date);
        }

        public static string HtmlEncode(this string s)
        {
            return WebUtility.HtmlEncode(s);
        }

        public static string UrlEncode(this string s)
        {
            return WebUtility.UrlEncode(s);
        }

        public static string UrlDecode(this string s)
        {
            return WebUtility.UrlDecode(s);
        }

        public static string MailToEncode(this string s)
        {
            return s.RemoveDiacritics().UrlEncode().Replace("+", " ");
        }

        

        public static string ToHTMLId(this string s)
        {
			return Regex.Replace(s.TrimStart('/'), "[/|?|=]", "-");
		}


        public static DateTime ToDateTime(this string s)
        {
            if (s.IsEmpty() || !s.IsDateTime())
                return DateTime.MinValue;

            return DateTime.Parse(s);
        }

	    public static bool IsFloat(this string s)
	    {
		    float ret;
		    return float.TryParse(s, out ret);
	    }

		public static bool IsInt(this string s)
        {
            int ret;
            return int.TryParse(s, out ret);
        }

        public static int ToInt(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;

            int ret;

            bool result = int.TryParse(s, out ret);

            if (result)
                return ret;

            return 0;
        }


	    public static float ToFloat(this string s)
	    {
		    if (string.IsNullOrEmpty(s))
			    return 0;

		    float ret;

		    bool result = float.TryParse(s, out ret);

		    if (result)
			    return ret;

		    return 0;
	    }

		public static bool IsBool(this string s)
		{
			bool ret;
			return bool.TryParse(s, out ret);
		}

		public static bool ToBool(this string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				return false;
			bool ret;
			if (bool.TryParse(s, out ret))
				return ret;
			return false;
		}

        public static string JsonPrettify(this string value)
        {
            using (var stringReader = new StringReader(value))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) {Formatting = Formatting.Indented};
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }


        public static string RemoveDiacritics(this string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        public static string Truncate(this string s, int length)
        {
            return s.Substring(0, Math.Min(length, s.Length)) + "...";
        }

        public static string ToSimpleText(this string s)
        {
			if (string.IsNullOrWhiteSpace(s)) return "";
            return alphaNumericRegex.Value.Replace(s, "").ToLower();
        }

	    public static string ToParentCodeSetId(this string s)
	    {
			var remove =  codesetRegex.Value.Replace(s, "").ToLower();

		    if (!string.IsNullOrEmpty(remove))
			    return s.Replace(remove, "");

		    return s;
	    }

        public static string ToStandardDOI(this string s)
        {
	        return s.ToLower().Trim().TrimEnd('/')
		        .Replace("https://doi.org/", "")
		        .Replace("http://doi.org/", "")
		        .Replace("http://dx.doi.org/", "")
		        .Replace("https://dx.doi.org/", "");
        }

        public static string ToStandardYear(this string s)
        {
            return s.ToSimpleText();
        }

        public static string ToShortSearchText(this string s)
        {
            return s.ToSimpleText()
                .Replace("a", "")
                .Replace("e", "")
                .Replace("i", "")
                .Replace("o", "")
                .Replace("u", "")
                .Replace("ize", "")
                .Replace("ise", "");
        }

        public static string ToHash(this string input)
        {
            var enc = Encoding.GetEncoding(0);

            byte[] buffer = enc.GetBytes(input);
            var sha1 = SHA1.Create();
            var hash = BitConverter.ToString(sha1.ComputeHash(buffer)).Replace("-", "");
            return hash;
        }

        private static Dictionary<string, string> _mlClean_RegexReplace = new Dictionary<string, string>()
        {
            { "<[^<>]+>", "" },
            { @"(http|https)://[^\s]*", "httpaddr" },
            { @"[^\s]+@[^\s]+", "emailaddr" },
            { "[$]+", "dollar" },
            { @"@[^\s]+", "username" }
        };

        private static List<string> _mlClean_SpaceReplace = new List<string>()
        {
            "'",
            "\"",
            ",",
            Environment.NewLine,
            "\n\r",
            "\n",
            "\r"
        };

        public static string MLClean(this string str)
        {
			if (string.IsNullOrWhiteSpace(str)) return "";
            _mlClean_RegexReplace.ToList().ForEach(x => str = Regex.Replace(str, x.Key, x.Value));
            _mlClean_SpaceReplace.ForEach(x => str = str.Replace(x, " "));

            return str;
        }

	    public static string RemoveQueryString(this string str)
	    {
		    if (string.IsNullOrEmpty(str))
			    return string.Empty;

		    return str.Contains("?") ? str.Substring(0, str.LastIndexOf("?", StringComparison.Ordinal)) : str;
	    }

		public static string AppendToQueryString(this string str, string key, string value)
		{
			if (string.IsNullOrEmpty(str))
				return string.Empty;

			return str.Contains("?") ? $"{str}&{key}={value}" : $"{str}?{key}={value}";
		}


		public static int ToIntId(this string s, string replaceText)
		{
			var ret = s.Replace(replaceText, "");
			return ret.ToInt();
		}

	    public static string ToLuceneEncoded(this string s)
	    {
		    return s.Replace("/", "\\/");
	    }

		public static string WithPlaceHolderIfEmpty(this string s, string placeholder = "-")
		{
			if (s.IsEmpty())
				return placeholder;
			return s;
		}

		public static string GetFileName(this string filepath)
		{
			return Path.GetFileName(filepath);
		}


	    public static string EmptyIfNull(this string s)
	    {
		    if (s == null)
			    return "";

		    return s;
	    }

	    public static string UppercaseFirst(this string s)
	    {
		    if (string.IsNullOrEmpty(s))
			    return string.Empty;

		    char[] a = s.ToCharArray();
		    a[0] = char.ToUpper(a[0]);
		    return new string(a);
	    }
	}

}
