using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

public partial class StoredFunctions
{
    // To compile: C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:library /out:UserFunctions.dll StoredFunctions.cs
    // Then add as an assembly in SQL Server.
    // Then execute CreateLevenshteinAndSoShortText.sql

    // ********************** BEGIN LEVENSHTEIN *****************************************

    [Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true, IsPrecise = false)]
    public static SqlDouble HaBoLevenshtein(SqlString stringOne, SqlString stringTwo)
    {
        #region Handle for Null value

        if (stringOne.IsNull)
            stringOne = new SqlString("");

        if (stringTwo.IsNull)
            stringTwo = new SqlString("");

        #endregion

        #region Convert to Uppercase

        string strOneUppercase = stringOne.Value.ToUpper();
        string strTwoUppercase = stringTwo.Value.ToUpper();

        #endregion

        #region Quick Check and quick match score

        int strOneLength = strOneUppercase.Length;
        int strTwoLength = strTwoUppercase.Length;

        int[,] dimension = new int[strOneLength + 1, strTwoLength + 1];
        int matchCost = 0;

        if (strOneLength + strTwoLength == 0)
        {
            return 100;
        }
        else if (strOneLength == 0)
        {
            return 0;
        }
        else if (strTwoLength == 0)
        {
            return 0;
        }

        #endregion

        #region Levenshtein Formula

        for (int i = 0; i <= strOneLength; i++)
            dimension[i, 0] = i;

        for (int j = 0; j <= strTwoLength; j++)
            dimension[0, j] = j;

        for (int i = 1; i <= strOneLength; i++)
        {
            for (int j = 1; j <= strTwoLength; j++)
            {
                if (strOneUppercase[i - 1] == strTwoUppercase[j - 1])
                    matchCost = 0;
                else
                    matchCost = 1;

                dimension[i, j] = System.Math.Min(System.Math.Min(dimension[i - 1, j] + 1, dimension[i, j - 1] + 1), dimension[i - 1, j - 1] + matchCost);
            }
        }

        #endregion

        // Calculate Percentage of match
        double percentage = System.Math.Round((1.0 - ((double)dimension[strOneLength, strTwoLength] / (double)System.Math.Max(strOneLength, strTwoLength))) * 100.0, 2);

        return percentage;
    }

    // ************************** END LEVENSHTEIN ****************************************




    // *********************** BEGIN TOSHORTSEARCHTEXT **********************

    private static readonly Lazy<Regex> alphaNumericRegex = new Lazy<Regex>(() => new Regex("[^a-zA-Z0-9]"));

    public static SqlString ToShortSearchText(SqlString s)
    {
        string ss = RemoveLanguageAndThesisText(s.Value.ToString());
        SqlString r = Truncate(ToSimpleText(RemoveDiacritics(ss))
                .Replace("a", "")
                .Replace("e", "")
                .Replace("i", "")
                .Replace("o", "")
                .Replace("u", "")
                .Replace("ize", "")
                .Replace("ise", ""), 500);
        return r;
    }

    public static string RemoveDiacritics(string stIn)
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

    public static string ToSimpleText(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        return alphaNumericRegex.Value.Replace(s, "").ToLower();
    }

    // This added by JT. Removes [German] and [Master's thesis] etc from the end of the string
    // so long as the text removed isn't more than 25% of the length of the title
    public static string RemoveLanguageAndThesisText(string s)
    {
        s = s.TrimEnd(' ');
        if (s.EndsWith("]"))
        {
            int i = s.LastIndexOf('[');
            if ((s.Length - i) * 4 < s.Length)
            {
                s = s.Substring(0, i).TrimEnd(' ');
            }
        }
        return s;
    }

    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    // **************************** END TOSHORTSEARCHTEXT ******************************
};