using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2.Models
{
    public class SetAttribute
    {
        
        public List<SetAttribute> children { get; set; }
        public long id { get; set; }
        public Nullable<int> own_set_id { get; set; }
        public Nullable<long> attribute_id { get; set; }
        public Nullable<long> parent_attribute_id { get; set; }
        public Nullable<int> attribute_type_id { get; set; }
        public string attribute_set_desc { get; set; }
        public Nullable<int> attribute_order { get; set; }
        public string attribute_type { get; set; }
        public bool ShowCheckBox { get
            {
                if (attribute_type_id == 1) return false;
                else return true;
            }
        }
        public string name { get; set; }
        public Nullable<int> contact_id { get; set; }
        public string attribute_desc { get; set; }
        //        interface SetAttribute
        //        {
        //            attribute_id: number;
        //    attribute_name: string;
        //    attribute_order: number;
        //    attribute_type: string;
        //    attribute_set_desc: string;
        //    attribute_desc: string;
        //    parent_attribute_id: number;
        //    attribute_type_id: number;
        //    attributes: SetAttribute[];
        //}
        //public string text
        //{
        //    get
        //    {
        //        return ATTRIBUTE_NAME;
        //    }
        //    set
        //    {
        //        ATTRIBUTE_NAME = value;
        //    }
        //}
        //public List<SetAttribute> Items
        //{
        //    get { return Attributes; }
        //    set { Attributes = value; }
        //}
        public static List<SetAttribute> GetAttributes(int? codesetID, Int64? parentID)
        {
            List<SetAttribute> result = new List<SetAttribute>();
            SqlParameter setId = new SqlParameter("@SET_ID", (int)codesetID);
            SqlParameter parentId = new SqlParameter("@PARENT_ATTRIBUTE_ID", parentID);
            try
            {
                using (SqlDataReader reader = Program.SqlHelper.ExecuteQuerySP(Program.SqlHelper.ER4DB, "st_AttributeSet", setId, parentId))
                {
                    while (reader.Read())
                    {
                        SetAttribute SetA = SetAttribute.GetSingleSetAttribute(reader);
                        SetA.children = SetAttribute.GetAttributes(SetA.own_set_id, SetA.attribute_id);
                        result.Add(SetA);
                    }
                }
            }
            catch (Exception e)
            {
                
                //Program.Logger.LogException(e, "Error fetching existing ref and/or creating local object.");
            }
            return result;
        }
        public static SetAttribute GetSingleSetAttribute(SqlDataReader reader)
        {
            SetAttribute SetA = new SetAttribute();
            //set.REVIEW_SET_ID = (int)reader["REVIEW_SET_ID"];//(Int64)reader["REFERENCE_ID"];
            //set.REVIEW_ID = (int?)reader["REVIEW_ID"];
            //set.SET_ID = (int?)reader["SET_ID"];
            //set.ALLOW_CODING_EDITS = (bool?)reader["ALLOW_CODING_EDITS"];
            //set.SET_TYPE_ID = (int)reader["SET_TYPE_ID"];
            //set.SET_NAME = (string)reader["SET_NAME"];

            //SetA.attribute_set_id = (long)reader["ATTRIBUTE_SET_ID"];
            SetA.own_set_id = (int?)reader["SET_ID"];
            SetA.attribute_id = (long?)reader["ATTRIBUTE_ID"];
            SetA.parent_attribute_id = (long?)reader["PARENT_ATTRIBUTE_ID"];
            SetA.attribute_type_id= (int?)reader["ATTRIBUTE_TYPE_ID"];
            if (reader["ATTRIBUTE_SET_DESC"].GetType() != System.DBNull.Value.GetType())
                SetA.attribute_set_desc = (string)reader["ATTRIBUTE_SET_DESC"];
            SetA.id = (long)reader["ATTRIBUTE_SET_ID"];
            if (reader["ATTRIBUTE_ORDER"].GetType() != System.DBNull.Value.GetType())
                SetA.attribute_order = (int?)reader["ATTRIBUTE_ORDER"];
            SetA.name = (string)reader["ATTRIBUTE_NAME"];
            SetA.contact_id = (int?)reader["CONTACT_ID"];
            if (reader["ATTRIBUTE_DESC"].GetType() != System.DBNull.Value.GetType())
                SetA.attribute_desc = (string)reader["ATTRIBUTE_DESC"];
            SetA.children = new List<SetAttribute>();
            return SetA;
        }
    }
}
