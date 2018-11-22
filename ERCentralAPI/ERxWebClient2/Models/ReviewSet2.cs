using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Models
{
    public class ReviewSet2
    {
        public ReviewSet2()
        {
            children = new List<SetAttribute>();
        }
        public List<SetAttribute> children { get; set; }
        public int review_set_id { get; set; }
        public Nullable<int> review_id { get; set; }
        public Nullable<int> id { get; set; }
        public Nullable<bool> allow_coding_edits { get; set; }
        public int set_type_id { get; set; }
        public string name { get; set; }
        public string set_type { get; set; }
        public Nullable<bool> coding_is_final { get; set; }
        public Nullable<int> set_order { get; set; }
        public int max_depth { get; set; }
        public string set_description { get; set; }
        public Nullable<int> original_set_id { get; set; }
       


        public static List<ReviewSet2> GetReviewSets(SqlConnection conn, SqlDataReader reader)
        {
            List<ReviewSet2> res = new List<ReviewSet2>();
            while (reader.Read())
            {
                ReviewSet2 set = new ReviewSet2();
                set.review_set_id= (int)reader["REVIEW_SET_ID"];
                set.review_id = (int?)reader["REVIEW_ID"];
                set.id = (int?)reader["SET_ID"];
                set.allow_coding_edits = (bool?)reader["ALLOW_CODING_EDITS"];
                set.set_type_id = (int)reader["SET_TYPE_ID"];
                set.name = (string)reader["SET_NAME"];
                set.set_type = (string)reader["SET_TYPE"];
                set.coding_is_final = (bool?)reader["CODING_IS_FINAL"];
                set.set_order = (int?)reader["SET_ORDER"];
                set.max_depth = (int)reader["MAX_DEPTH"];
                if (reader["SET_DESCRIPTION"].GetType() != System.DBNull.Value.GetType())
                    set.set_description = (string)reader["SET_DESCRIPTION"];
                if (reader["ORIGINAL_SET_ID"].GetType() != System.DBNull.Value.GetType())
                    set.original_set_id = (int?)reader["ORIGINAL_SET_ID"];
                set.children = SetAttribute.GetAttributes(set.id, 0);
                res.Add(set);
            }
            return res;
        }
    }
}
