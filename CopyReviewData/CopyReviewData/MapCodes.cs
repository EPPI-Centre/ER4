using EPPIDataServices.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Data;

namespace CopyReviewData
{
    internal partial class Program
    {
        public static string queryGetReviewSetsFromReview(int ReviewId)
        {
            return "SELECT * from tb_REVIEW_SET rs INNER JOIN tb_SET s on s.SET_ID = rs.SET_ID and rs.REVIEW_ID = " + ReviewId.ToString();
        }
        public static string queryGetReviewSetsFromSetId(int SetId)
        {
            return "SELECT * from tb_REVIEW_SET rs INNER JOIN tb_SET s on s.SET_ID = rs.SET_ID and rs.SET_ID = " + SetId.ToString();
        }
        public static string queryGetAttributesFromSetId(int SetId)
        {
            return "SELECT *, dbo.fn_IsAttributeInTree(a.ATTRIBUTE_ID) as IsInTree from tb_ATTRIBUTE_SET tas INNER JOIN tb_ATTRIBUTE a " 
                + "on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and tas.SET_ID =" + SetId.ToString();
        }
        public static string queryGetAttributeFromAttributeId(long AttributeId)
        {
            return "SELECT *, dbo.fn_IsAttributeInTree(a.ATTRIBUTE_ID) as IsInTree from tb_ATTRIBUTE_SET tas INNER JOIN tb_ATTRIBUTE a "
                + "on a.ATTRIBUTE_ID = tas.ATTRIBUTE_ID and a.ATTRIBUTE_ID =" + AttributeId.ToString();
        }
        public static void DoMapCodesBetween2Reviews()
        {
            List<MiniSet> setsInOffspringRev = new List<MiniSet>();
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, queryGetReviewSetsFromReview(DestinationRevId)))
                {
                    while (reader.Read())// && ItemIDs.Count < 5000)
                    {
                        MiniSet ms = new MiniSet(reader);
                        if (ms.OriginalSetId > 0) setsInOffspringRev.Add(ms);
                    }
                }
            }
            LogLine("Found " + setsInOffspringRev.Count.ToString() + " sets with OriginalSetId values in review " + DestinationRevId  +".");
            LogLine("Looking for ancestors of these sets...");
            List<(MiniSet, MiniSet)> MatchedSets = new List<(MiniSet, MiniSet)>();
            foreach (MiniSet ms in setsInOffspringRev)
            {
                MiniSet? found = FindTheAncestorOfThisSet(ms);
                if (found != null) MatchedSets.Add((ms,found));
            }
            LogLine("Found " + MatchedSets.Count.ToString() + " ancestor sets in Review " + SourceRevId.ToString() + ".");
            List<(MiniAttribute, MiniAttribute)> MatchedAttributes = new List<(MiniAttribute, MiniAttribute)>();
            LogLine("Looking for Attribute ancestors...");
            foreach((MiniSet, MiniSet) match in MatchedSets)
            {
                List<MiniAttribute> offspringAtts = new List<MiniAttribute>();
                using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
                {
                    using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, queryGetAttributesFromSetId(match.Item1.SetId)))
                    {
                        while (reader.Read())// && ItemIDs.Count < 5000)
                        {
                            Console.Write(".");
                            MiniAttribute ma = new MiniAttribute(reader);
                            if (ma.OriginalAttributeId > 0) offspringAtts.Add(ma);
                        }
                    }
                    foreach (MiniAttribute ma in offspringAtts)
                    {
                        Console.Write("+"); 
                        MiniAttribute? maMatch = FindTheAncestorOfThisAttribute(ma, match.Item2.SetId, conn);
                        if (maMatch != null) MatchedAttributes.Add((ma, maMatch));
                    }
                    Console.WriteLine("");
                }
            }
            LogLine("Found " + MatchedAttributes.Count.ToString() + " matching attributes.");
            LogLine("Printing results.");
            LogLine("");
            LogLine("Offspring\tAncestor\tName");
            foreach ((MiniAttribute, MiniAttribute) match in MatchedAttributes)
            {
                LogLine(match.Item1.AttributeId.ToString() + "\t"
                    + match.Item2.AttributeId.ToString() + "\t"
                    + match.Item1.Name.Replace('\t', ' '));
            }
        }
        public static MiniSet? FindTheAncestorOfThisSet(MiniSet offspringSet)
        {
            if (offspringSet.OriginalSetId < 1) 
            { 
                return null; 
            }
            using (SqlConnection conn = new SqlConnection(Program.SqlHelper.ER4DB))
            {
                MiniSet? ms = null;
                using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, queryGetReviewSetsFromSetId(offspringSet.OriginalSetId)))
                {
                    while (reader.Read())// && ItemIDs.Count < 5000)
                    {
                        ms = new MiniSet(reader);
                        if (ms.ReviewId == SourceRevId) return ms;
                    }
                }
                if (ms != null)
                {
                    return FindTheAncestorOfThisSet(ms);
                }
            }
            return null;
        }

        public static MiniAttribute? FindTheAncestorOfThisAttribute(MiniAttribute offspringAtt, long AncestorSetId, SqlConnection conn)
        {
            if (offspringAtt.OriginalAttributeId < 1) return null;
            MiniAttribute? ma = null;
            using (SqlDataReader reader = SqlHelper.ExecuteQueryNonSP(conn, queryGetAttributeFromAttributeId(offspringAtt.OriginalAttributeId)))
            {
                while (reader.Read())// && ItemIDs.Count < 5000)
                {
                    ma = new MiniAttribute(reader);
                    if (ma.SetId == AncestorSetId) return ma;
                }
            }
            if (ma != null)
            {
                return FindTheAncestorOfThisAttribute(ma, AncestorSetId, conn);
            }
            return null;
        }
    }
    public class MiniSet
    {
        public MiniSet(SqlDataReader reader)
        {
            Name = reader.GetString("SET_NAME");
            Description = DBNull.Value.Equals(reader["SET_DESCRIPTION"]) ? "" : reader.GetString("SET_DESCRIPTION");
            SetId = reader.GetInt32("SET_ID");
            ReviewId = reader.GetInt32("REVIEW_ID");
            OriginalSetId = DBNull.Value.Equals(reader["ORIGINAL_SET_ID"]) ? -1 : reader.GetInt32("ORIGINAL_SET_ID");
        }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int SetId { get; set; } = 0;
        public int OriginalSetId { get; set; } = 0;
        public int ReviewId { get; set; } = 0;
    }

    public class MiniAttribute
    {
        public MiniAttribute(SqlDataReader reader) 
        {
            Name = reader.GetString("ATTRIBUTE_NAME");
            Description = DBNull.Value.Equals(reader["ATTRIBUTE_SET_DESC"]) ? "" : reader.GetString("ATTRIBUTE_SET_DESC");
            AttributeId = reader.GetInt64("ATTRIBUTE_ID");
            SetId = reader.GetInt32("SET_ID"); 
            //ReviewId = reader.GetInt32("REVIEW_ID");
            OriginalAttributeId = DBNull.Value.Equals(reader["ORIGINAL_ATTRIBUTE_ID"]) ? -1 : reader.GetInt64("ORIGINAL_ATTRIBUTE_ID");
            IsInTree = reader.GetBoolean("IsInTree");
        }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Int64 AttributeId { get; set; } = 0;
        public Int64 OriginalAttributeId { get; set; } = 0;
        public int SetId { get; set; } = 0;
        //public int ReviewId { get; set; } = 0;
        public bool IsInTree { get; set; } = false;
    }
}