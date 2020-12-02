using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Csla;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.Search;
using System.Text.RegularExpressions;
using System.Configuration;

namespace BusinessLibrary.BusinessClasses
{
    /*
    public class MagPaperItemsMatch
    {
        public Int64 id { get; set; }
        public int year { get; set; }
        public string journal { get; set; }
        public string [] authors { get; set; }
        public string volume { get; set; }
        public string issue { get; set; }
        public string title { get; set; }
        public string first_page { get; set; }

        public double titleLeven { get; set; }
        public double volumeMatch { get; set; }
        public double pageMatch { get; set; }
        public double yearMatch { get; set; }
        public double journalJaro { get; set; }
        public double allAuthorsLeven { get; set; }
        public double matchingScore { get; set; }
    }
    */
    public static class MagPaperItemMatch
    {
        public const double AutoMatchThreshold = 0.8;
        public const double AutoMatchMinScore = 0.4;
        private static SearchIndexClient CreateSearchIndexClient()
        {
#if (!CSLA_NETCORE)
            SearchIndexClient indexClient = new SearchIndexClient("eppimag", "mag-index", new SearchCredentials(ConfigurationManager.AppSettings["AzureSearchMAGApi-key"]));
            
#else
            var configuration = ERxWebClient2.Startup.Configuration.GetSection("AzureMagSettings");
            string MAGApikey = configuration["AzureSearchMAGApi-key"];
            SearchIndexClient indexClient = new SearchIndexClient("eppimag", "mag-index", new SearchCredentials(MAGApikey));

#endif
            return indexClient;
        }

        public static void MatchItemToMag(Int64 ItemId, int ReviewId)
        {
            Item i = null;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_Item", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", ItemId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                            i = Item.GetItem(reader);
                    }
                }
                connection.Close();
            }

            if (i != null)
            {
                // similar code is used in MagCheckPaperIdChangesCommand
                List<MagMakesHelpers.PaperMakes> candidatePapersOnDOI = MagMakesHelpers.GetCandidateMatchesOnDOI(i.DOI, "LIVE");
                if (candidatePapersOnDOI != null && candidatePapersOnDOI.Count > 0)
                {
                    foreach (MagMakesHelpers.PaperMakes cpm in candidatePapersOnDOI)
                    {
                        MagPaperItemMatch.doComparison(i, cpm);
                    }
                }
                if (candidatePapersOnDOI.Count == 0 || (candidatePapersOnDOI.Max(t => t.matchingScore) < AutoMatchThreshold))
                {
                    List<MagMakesHelpers.PaperMakes> candidatePapersOnTitle = MagMakesHelpers.GetCandidateMatches(i.Title, "LIVE", true);
                    foreach (MagMakesHelpers.PaperMakes pm in candidatePapersOnTitle)
                    {
                        doComparison(i, pm);
                    }
                    for (int inn = 0; inn < candidatePapersOnTitle.Count; inn++)
                    {
                        if (candidatePapersOnTitle[inn].matchingScore < AutoMatchMinScore)
                        {
                            candidatePapersOnTitle.RemoveAt(inn);
                            inn--;
                        }
                    }
                    foreach (MagMakesHelpers.PaperMakes pm in candidatePapersOnTitle)
                    {
                        var found = candidatePapersOnDOI.Find(e => e.Id == pm.Id);
                        if (found == null && pm.matchingScore >= AutoMatchMinScore)
                        {
                            candidatePapersOnDOI.Add(pm);
                        }
                    }
                    // add in matching on journals / authors if we don't have an exact match on title
                    if (candidatePapersOnTitle.Count == 0 || (candidatePapersOnTitle.Count > 0 && candidatePapersOnTitle.Max(t => t.matchingScore) < AutoMatchThreshold))
                    {
                        List<MagMakesHelpers.PaperMakes> candidatePapersOnAuthorJournal = MagMakesHelpers.GetCandidateMatches(i.Authors + " " + i.ParentTitle);
                        foreach (MagMakesHelpers.PaperMakes pm in candidatePapersOnAuthorJournal)
                        {
                            doComparison(i, pm);
                        }
                        foreach (MagMakesHelpers.PaperMakes pm in candidatePapersOnAuthorJournal)
                        {
                            var found = candidatePapersOnDOI.Find(e => e.Id == pm.Id);
                            if (found == null && pm.matchingScore >= AutoMatchMinScore)
                            {
                                candidatePapersOnDOI.Add(pm);
                            }
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    foreach (MagMakesHelpers.PaperMakes pm in candidatePapersOnDOI) {
                        if (pm.matchingScore > 0.40)
                        {
                            using (SqlCommand command = new SqlCommand("st_MagMatchedPapersInsert", connection))
                            {
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReviewId));
                                command.Parameters.Add(new SqlParameter("@ITEM_ID", i.ItemId));
                                command.Parameters.Add(new SqlParameter("@PaperId", pm.Id));
                                command.Parameters.Add(new SqlParameter("@ManualTrueMatch", 0));
                                command.Parameters.Add(new SqlParameter("@ManualFalseMatch", 0));
                                command.Parameters.Add(new SqlParameter("@AutoMatchScore", pm.matchingScore));
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    connection.Close();
                }
            }
        }

        // Also see the algorithm in ItemDuplicateReadOnlyGroupList.cs. They should (probably) be identical
        public static void doComparison(Item i, MagMakesHelpers.PaperMakes pm)
        {
            ItemDuplicateReadOnlyGroupList.Comparator comparator = new ItemDuplicateReadOnlyGroupList.Comparator();
            pm.matchingScore = comparator.CompareItems(new ItemDuplicateReadOnlyGroupList.ItemComparison(i),
                new ItemDuplicateReadOnlyGroupList.ItemComparison(pm));
            //pm.titleLeven = HaBoLevenshtein(pm.DN, i.Title);
            //pm.volumeMatch = pm.V == i.Volume ? 1 : 0;
            //pm.pageMatch = pm.FP == i.FirstPage() ? 1 : 0;
            //pm.yearMatch = pm.Y.ToString() == i.Year ? 1 : 0;
            //pm.journalJaro = pm.J != null ? Jaro(pm.J.JN, i.ParentTitle) : 0;
            //pm.allAuthorsLeven = Jaro(MagMakesHelpers.getAuthors(pm.AA).Replace(",", " "), i.Authors.Replace(";", " "));
            //pm.matchingScore = ((pm.titleLeven / 100 * 2.71) +
            //    (pm.volumeMatch * 0.02) +
            //    (pm.pageMatch * 0.18) +
            //    (pm.yearMatch * 0.82) +
            //    (pm.journalJaro * 0.55) +
            //    (pm.allAuthorsLeven / 100 * 1.25)) / 5.53;
        }

        public static void doMakesPapersComparison(MagMakesHelpers.PaperMakes i, MagMakesHelpers.PaperMakes pm)
        {
            ItemDuplicateReadOnlyGroupList.Comparator comparator = new ItemDuplicateReadOnlyGroupList.Comparator();
            pm.matchingScore = comparator.CompareItems(new ItemDuplicateReadOnlyGroupList.ItemComparison(i),
                new ItemDuplicateReadOnlyGroupList.ItemComparison(pm));
            //pm.titleLeven = HaBoLevenshtein(pm.DN, i.DN);
            //pm.volumeMatch = pm.V == i.V ? 1 : 0;
            //pm.pageMatch = pm.FP == i.FP ? 1 : 0;
            //pm.yearMatch = pm.Y == i.Y ? 1 : 0;
            //pm.journalJaro = pm.J != null && i.J != null ? Jaro(pm.J.JN, i.J.JN) : 0;
            //pm.allAuthorsLeven = Jaro(MagMakesHelpers.getAuthors(pm.AA).Replace(",", " "), MagMakesHelpers.getAuthors(i.AA).Replace(",", " "));
            //pm.matchingScore = ((pm.titleLeven / 100 * 2.71) +
            //    (pm.volumeMatch * 0.02) +
            //    (pm.pageMatch * 0.18) +
            //    (pm.yearMatch * 0.82) +
            //    (pm.journalJaro * 0.55) +
            //    (pm.allAuthorsLeven / 100 * 1.25)) / 5.53;
        }

        

        // *********************** copied from: https://github.com/admin2210/EditDistance *******************************
            private struct JaroMetrics
            {
                public int Matches;
                public int Transpositions;
            }

            private static JaroMetrics Matches(string s1, string s2)
            {
                string text;
                string text2;
                if (s1.Length > s2.Length)
                {
                    text = s1;
                    text2 = s2;
                }
                else
                {
                    text = s2;
                    text2 = s1;
                }
                int num = Math.Max(text.Length / 2 - 1, 0);
                int[] array = new int[text2.Length];
                int i;
                for (i = 0; i < array.Length; i++)
                {
                    array[i] = -1;
                }
                bool[] array2 = new bool[text.Length];
                int num2 = 0;
                for (int j = 0; j < text2.Length; j++)
                {
                    char c = text2[j];
                    int k = Math.Max(j - num, 0);
                    int num3 = Math.Min(j + num + 1, text.Length);
                    while (k < num3)
                    {
                        if (!array2[k] && c == text[k])
                        {
                            array[j] = k;
                            array2[k] = true;
                            num2++;
                            break;
                        }
                        k++;
                    }
                }
                char[] array3 = new char[num2];
                char[] ms2 = new char[num2];
                i = 0;
                int num4 = 0;
                while (i < text2.Length)
                {
                    if (array[i] != -1)
                    {
                        array3[num4] = text2[i];
                        num4++;
                    }
                    i++;
                }
                i = 0;
                num4 = 0;
                while (i < text.Length)
                {
                    if (array2[i])
                    {
                        ms2[num4] = text[i];
                        num4++;
                    }
                    i++;
                }
                int num5 = array3.Where((char t, int mi) => t != ms2[mi]).Count<char>();
                JaroMetrics result;
                result.Matches = num2;
                result.Transpositions = num5 / 2;
                return result;
            }

        public static double Jaro(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;

            JaroMetrics jaroMetrics = Matches(s1, s2);
            float num = (float)jaroMetrics.Matches;
            int transpositions = jaroMetrics.Transpositions;
            float result;
            if (num == 0f)
            {
                result = 0f;
            }
            else
            {
                float num2 = (num / (float)s1.Length + num / (float)s2.Length + (num - (float)transpositions) / num) / 3f;
                result = num2;
            }
            return result;
        }

        public static double HaBoLevenshtein(string stringOne, string stringTwo)
        {
            #region Handle for Null value

            if (stringOne == null)
                stringOne = "";

            if (stringTwo == null)
                stringTwo = "";

            #endregion

            #region Convert to Uppercase

            string strOneUppercase = stringOne.ToUpper();
            string strTwoUppercase = stringTwo.ToUpper();

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


    }
}
