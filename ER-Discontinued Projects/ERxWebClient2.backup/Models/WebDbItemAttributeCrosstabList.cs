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

#if!SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class WebDbItemAttributeCrosstabList : ReadOnlyBase<WebDbItemAttributeCrosstabList>
    {

        public WebDbItemAttributeCrosstabList()
        {
            LoadProperty(RowsProperty, new MobileList<WebDbItemAttributeCrosstabRow>());
            LoadProperty(ColumnAttIDsProperty, new MobileList<long>());
            LoadProperty(ColumnAttNamesProperty, new MobileList<string>());
#if WEBDB
            LoadProperty(SegmentsAttIDsProperty, new MobileList<long>());
            LoadProperty(SegmentsAttNamesProperty, new MobileList<string>());
#endif
        }

        public static readonly PropertyInfo<MobileList<WebDbItemAttributeCrosstabRow>> RowsProperty = RegisterProperty<MobileList<WebDbItemAttributeCrosstabRow>>(new PropertyInfo<MobileList<WebDbItemAttributeCrosstabRow>>("Rows", "Rows", new MobileList<WebDbItemAttributeCrosstabRow>()));
        public MobileList<WebDbItemAttributeCrosstabRow> Rows
        {
            get
            {
                return GetProperty(RowsProperty);
            }
        }
        public static readonly PropertyInfo<MobileList<long>> ColumnAttIDsProperty = RegisterProperty<MobileList<long>>(new PropertyInfo<MobileList<long>>("ColumnAttIDs", "ColumnAttIDs", new MobileList<long>()));
        public MobileList<long> ColumnAttIDs
        {
            get
            {
                return GetProperty(ColumnAttIDsProperty);
            }
        }
        public static readonly PropertyInfo<MobileList<string>> ColumnAttNamesProperty = RegisterProperty<MobileList<string>>(new PropertyInfo<MobileList<string>>("ColumnAttNames", "ColumnAttNames", new MobileList<string>()));
        public MobileList<string> ColumnAttNames
        {
            get
            {
                return GetProperty(ColumnAttNamesProperty);
            }
        }
        public static readonly PropertyInfo<Int64> FilterAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("FilterAttributeId", "FilterAttributeId"));
        public Int64 FilterAttributeId
        {
            get
            {
                return GetProperty(FilterAttributeIdProperty);
            }
        }
        public static readonly PropertyInfo<int> SetIdXProperty = RegisterProperty<int>(new PropertyInfo<int>("SetIdX", "SetIdX"));
        public int SetIdX
        {
            get
            {
                return GetProperty(SetIdXProperty);
            }
        }

        public static readonly PropertyInfo<int> SetIdYProperty = RegisterProperty<int>(new PropertyInfo<int>("SetIdY", "SetIdY"));
        public int SetIdY
        {
            get
            {
                return GetProperty(SetIdYProperty);
            }
        }
        


        public static readonly PropertyInfo<string> SetIdXNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetIdXName", "SetIdXName"));
        public string SetIdXName
        {
            get
            {
                return GetProperty(SetIdXNameProperty);
            }
        }

        public static readonly PropertyInfo<string> SetIdYNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetIdYName", "SetIdYName"));
        public string SetIdYName
        {
            get
            {
                return GetProperty(SetIdYNameProperty);
            }
        }

        

       

        public static readonly PropertyInfo<int> AttibuteIdXProperty = RegisterProperty<int>(new PropertyInfo<int>("AttibuteIdX", "AttibuteIdX"));
        public int AttibuteIdX
        {
            get
            {
                return GetProperty(AttibuteIdXProperty);
            }
        }

        public static readonly PropertyInfo<int> AttibuteIdYProperty = RegisterProperty<int>(new PropertyInfo<int>("AttibuteIdY", "AttibuteIdY"));
        public int AttibuteIdY
        {
            get
            {
                return GetProperty(AttibuteIdYProperty);
            }
        }

        public static readonly PropertyInfo<string> AttibuteIdXNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttibuteIdXName", "AttibuteIdXName"));
        public string AttibuteIdXName
        {
            get
            {
                return GetProperty(AttibuteIdXNameProperty);
            }
        }

        public static readonly PropertyInfo<string> AttibuteIdYNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttibuteIdYName", "AttibuteIdYName"));
        public string AttibuteIdYName
        {
            get
            {
                return GetProperty(AttibuteIdYNameProperty);
            }
        }

        public static readonly PropertyInfo<string> GraphicProperty = RegisterProperty<string>(new PropertyInfo<string>("Graphic", "Graphic"));
        public string Graphic
        {
            get
            {
                return GetProperty(GraphicProperty);
            }
        }

        public static readonly PropertyInfo<string> IncludedProperty = RegisterProperty<string>(new PropertyInfo<string>("Included", "Included", ""));
        public string Included
        {
            get
            {
                return GetProperty(IncludedProperty);
            }
        }
        public static readonly PropertyInfo<int> WebDbIdProperty = RegisterProperty<int>(new PropertyInfo<int>("WebDbId", "WebDbId"));
        public int WebDbId
        {
            get
            {
                return GetProperty(WebDbIdProperty);
            }
        }

#if WEBDB
        //data to create a 3D map, on WEBDB only, for now(19/08/21)...
        public static readonly PropertyInfo<int> SetIdSegmentsProperty = RegisterProperty<int>(new PropertyInfo<int>("SetIdSegments", "SetIdSegments", 0));
        public int SetIdSegments
        {
            get
            {
                return GetProperty(SetIdSegmentsProperty);
            }
        }
        public static readonly PropertyInfo<string> SetIdSegmentsNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetIdSegmentsName", "SetIdSegmentsName"));
        public string SetIdSegmentsName
        {
            get
            {
                return GetProperty(SetIdSegmentsNameProperty);
            }
        }
        public static readonly PropertyInfo<MobileList<long>> SegmentsAttIDsProperty = RegisterProperty<MobileList<long>>(new PropertyInfo<MobileList<long>>("SegmentsAttIDs", "SegmentsAttIDs", new MobileList<long>()));
        public MobileList<long> SegmentsAttIDs
        {
            get
            {
                return GetProperty(SegmentsAttIDsProperty);
            }
        }
        public static readonly PropertyInfo<MobileList<string>> SegmentsAttNamesProperty = RegisterProperty<MobileList<string>>(new PropertyInfo<MobileList<string>>("SegmentsAttNames", "SegmentsAttNames", new MobileList<string>()));
        public MobileList<string> SegmentsAttNames
        {
            get
            {
                return GetProperty(SegmentsAttNamesProperty);
            }
        }
        public static readonly PropertyInfo<string> AttibuteIdSegmentsNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttibuteIdSegmentsName", "AttibuteIdSegmentsName"));
        public string AttibuteIdSegmentsName
        {
            get
            {
                return GetProperty(AttibuteIdSegmentsNameProperty);
            }
        }
        public static readonly PropertyInfo<long> AttibuteIdSegmentsProperty = RegisterProperty<long>(new PropertyInfo<long>("AttibuteIdSegments", "AttibuteIdSegments"));
        public long AttibuteIdSegments
        {
            get
            {
                return GetProperty(AttibuteIdSegmentsProperty);
            }
        }
#endif

#if !SILVERLIGHT

        private void DataPortal_Fetch(WebDbFrequencyCrosstabAndMapSelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            Dictionary<long, string> codesY = new Dictionary<long, string>();
            List<MiniItem> items = new List<MiniItem>();


            string SetIdXAxisName = "";
            string SetIdYAxisName = "";
            Int64 AttibuteIdXAxis = 0;
            Int64 AttibuteIdYAxis = 0;
            string AttibuteIdXAxisName = "";
            string AttibuteIdYAxisName = "";
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
#if WEBDB
                using (SqlCommand command = new SqlCommand("st_WebDbFrequencyCrosstabAndMap", connection))
#else
                using (SqlCommand command = new SqlCommand("st_ErFrequencyCrosstabAndMap", connection))
#endif
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@attributeIdXAxis", criteria.attributeIdXAxis));
                    command.Parameters.Add(new SqlParameter("@setIdXAxis", criteria.setIdXAxis));
                    command.Parameters.Add(new SqlParameter("@attributeIdYAxis", criteria.attributeIdYAxis));
                    command.Parameters.Add(new SqlParameter("@setIdYAxis", criteria.setIdYAxis));
                    if (criteria.included != "")
                    {
                        command.Parameters.Add(new SqlParameter("@included", criteria.included.ToLower() == "true" ? true : false));
                    }
                    command.Parameters.Add(new SqlParameter("@onlyThisAttribute", criteria.onlyThisAttribute));
                    command.Parameters.Add(new SqlParameter("@RevId", ri.ReviewId));
#if WEBDB
                    command.Parameters.Add(new SqlParameter("@WebDbId", criteria.webDbId));
                    if (criteria.setIdSegments > 0 )
                    {
                        LoadProperty(SetIdSegmentsProperty, criteria.setIdSegments);
                        LoadProperty(AttibuteIdSegmentsProperty, criteria.segmentsParent);
                        command.Parameters.Add(new SqlParameter("@setIdSegments", criteria.setIdSegments));
                        command.Parameters.Add(new SqlParameter("@segmentsParent", criteria.segmentsParent));
                    }
#endif
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            ColumnAttIDs.Add(reader.GetInt64("ATTRIBUTE_ID"));
                            ColumnAttNames.Add(reader.GetString("ATTRIBUTE_NAME"));
                        }
                        reader.NextResult(); 
                        while (reader.Read())
                        {
                            codesY.Add(reader.GetInt64("ATTRIBUTE_ID"), reader.GetString("ATTRIBUTE_NAME"));
                        }
                        reader.NextResult();
#if WEBDB
                        if (SetIdSegments > 0)
                        {
                            //get the segment names/ids
                            while (reader.Read())
                            {
                                SegmentsAttIDs.Add(reader.GetInt64("ATTRIBUTE_ID"));
                                SegmentsAttNames.Add(reader.GetString("ATTRIBUTE_NAME"));
                            }
                            reader.NextResult();
                            while (reader.Read())
                            {
                                Make3DminiItem(items, reader);
                            }
                        }
                        else
                        {
                            while (reader.Read())
                            {
                                Make2DminiItem(items, reader);
                            }
                        }
#else
                        while (reader.Read())
                        {
                            Make2DminiItem(items, reader);
                        }
#endif
                        reader.NextResult();

                        //string test = "";
                        while (reader.Read())
                        {
                            //test = reader.GetInt64("SETIDX_ID").ToString();
                            SetIdXAxisName = reader.GetString("SETIDX_NAME"); 
                            //test = reader.GetInt64("SETIDY_ID").ToString();
                            SetIdYAxisName = reader.GetString("SETIDY_NAME");
                            AttibuteIdXAxis = reader.GetInt64("ATTIBUTEIDX_ID");
                            AttibuteIdXAxisName = reader.GetString("ATTIBUTEIDX_NAME");
                            AttibuteIdYAxis = reader.GetInt64("ATTIBUTEIDY_ID");
                            AttibuteIdYAxisName = reader.GetString("ATTIBUTEIDY_NAME");
#if WEBDB
                            if (SetIdSegments > 0)
                            {
                                LoadProperty(AttibuteIdSegmentsNameProperty, reader.GetString("SEGMENTS_PARENT_NAME"));//we did the ID above...
                            }
#endif
                        }

                    }
                }
                connection.Close();
                LoadProperty(WebDbIdProperty, criteria.webDbId);
                LoadProperty(SetIdXProperty, criteria.setIdXAxis);
                LoadProperty(SetIdYProperty, criteria.setIdYAxis);
                LoadProperty(FilterAttributeIdProperty, criteria.onlyThisAttribute);
                LoadProperty(GraphicProperty, criteria.graphic);
                LoadProperty(IncludedProperty, criteria.included);
                LoadProperty(AttibuteIdXProperty, Convert.ToInt32(AttibuteIdXAxis));
                LoadProperty(AttibuteIdYProperty, Convert.ToInt32(AttibuteIdYAxis));
                LoadProperty(AttibuteIdXNameProperty, AttibuteIdXAxisName);
                LoadProperty(AttibuteIdYNameProperty, AttibuteIdYAxisName);
                LoadProperty(SetIdXNameProperty, SetIdXAxisName);
                LoadProperty(SetIdYNameProperty, SetIdYAxisName);
#if WEBDB
                if (SetIdSegments > 0)
                {//3Dmap!
                    foreach (KeyValuePair<long, string> kvp in codesY)
                    {//cycle on rows
                        WebDbItemAttributeCrosstabRow row = WebDbItemAttributeCrosstabRow.GetReadOnlyItemAttributeCrosstabRow(kvp.Key, kvp.Value);
                        for (int i = 0; i < ColumnAttIDs.Count; i++)
                        {//cycle on columns, within the row
                            for (int ii = 0; ii < SegmentsAttIDs.Count; ii++)
                            {//cycle also through all segments... So that each cell will contain N values, where N = number of segments.
                                int count = items.FindAll(found => found.Attributes2.Contains(kvp.Key) 
                                                            && found.Attributes.Contains(ColumnAttIDs[i])
                                                            && found.Attributes3.Contains(SegmentsAttIDs[ii])
                                                            ).Count;
                                row.Counts.Add(count);
                            }
                        }
                        for (int ii = 0; ii < SegmentsAttIDs.Count; ii++)
                        {
                            int others = items.FindAll(found => found.Attributes.Count == 0 
                                                        && found.Attributes2.Contains(kvp.Key)
                                                        && found.Attributes3.Contains(SegmentsAttIDs[ii])
                                                        ).Count;
                            row.Counts.Add(others);
                        }
                            
                        Rows.Add(row);
                    }
                    WebDbItemAttributeCrosstabRow lastRow = WebDbItemAttributeCrosstabRow.GetReadOnlyItemAttributeCrosstabRow(-999999, "None of the above");
                    for (int i = 0; i < ColumnAttIDs.Count; i++)
                    {//last cycles for the "none of the above" row
                        for (int ii = 0; ii < SegmentsAttIDs.Count; ii++)
                        {
                            int others = items.FindAll(found => found.Attributes2.Count == 0 
                                                        && found.Attributes.Contains(ColumnAttIDs[i])
                                                        && found.Attributes3.Contains(SegmentsAttIDs[ii])).Count;
                            lastRow.Counts.Add(others);
                        }                        
                    }
                    for (int ii = 0; ii < SegmentsAttIDs.Count; ii++)
                    {//last cell, never has counts...
                        lastRow.Counts.Add(0);
                    }
                    Rows.Add(lastRow);
                    //add the ID and Name for the last "none of these" column
                    ColumnAttIDs.Add(-999999);
                    ColumnAttNames.Add("None of these");
                }
                else
                {
#endif
                    //2D only
                    foreach (KeyValuePair<long, string> kvp in codesY)
                    {//cycle on rows
                        WebDbItemAttributeCrosstabRow row = WebDbItemAttributeCrosstabRow.GetReadOnlyItemAttributeCrosstabRow(kvp.Key, kvp.Value);
                        for (int i = 0; i < ColumnAttIDs.Count; i++)
                        {//cycle on columns, within the row
                            int count = items.FindAll(found => found.Attributes2.Contains(kvp.Key) && found.Attributes.Contains(ColumnAttIDs[i])).Count;
                            row.Counts.Add(count);

                        }
                        int others = items.FindAll(found => found.Attributes.Count == 0 && found.Attributes2.Contains(kvp.Key)).Count;
                        row.Counts.Add(others);
                        Rows.Add(row);
                    }
                    WebDbItemAttributeCrosstabRow lastRow = WebDbItemAttributeCrosstabRow.GetReadOnlyItemAttributeCrosstabRow(-999999, "None of the above");
                    for (int i = 0; i < ColumnAttIDs.Count; i++)
                    {//last cycle for the "none of the above" row
                        int others = items.FindAll(found => found.Attributes2.Count == 0 && found.Attributes.Contains(ColumnAttIDs[i])).Count;
                        lastRow.Counts.Add(others);
                    }
                    lastRow.Counts.Add(0);
                    Rows.Add(lastRow);
                    //add the ID and Name for the last "none of these" column
                    ColumnAttIDs.Add(-999999);
                    ColumnAttNames.Add("None of these");
#if WEBDB
                }
#endif
            }

        }
        private void Make2DminiItem(List<MiniItem> items, Csla.Data.SafeDataReader reader)
        {
            MiniItem mit = new MiniItem(reader.GetInt64("ItemId"));
            string[] tmp = reader.GetString("X_atts").Split(',', StringSplitOptions.RemoveEmptyEntries);
            string[] tmp2 = reader.GetString("Y_atts").Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in tmp)
            {
                mit.Attributes.Add(long.Parse(s));
            }
            foreach (string s in tmp2)
            {
                mit.Attributes2.Add(long.Parse(s));
            }
            items.Add(mit);
        }
        private void Make3DminiItem(List<MiniItem> items, Csla.Data.SafeDataReader reader)
        {
            MiniItem mit = new MiniItem(reader.GetInt64("ItemId"));
            string[] tmp = reader.GetString("X_atts").Split(',', StringSplitOptions.RemoveEmptyEntries);
            string[] tmp2 = reader.GetString("Y_atts").Split(',', StringSplitOptions.RemoveEmptyEntries);
            string[] tmp3 = reader.GetString("segments").Split(',', StringSplitOptions.RemoveEmptyEntries); 
            foreach (string s in tmp)
            {
                mit.Attributes.Add(long.Parse(s));
            }
            foreach (string s in tmp2)
            {
                mit.Attributes2.Add(long.Parse(s));
            }
            foreach (string s in tmp3)
            {
                mit.Attributes3.Add(long.Parse(s));
            }
            items.Add(mit);
        }

#endif
                    }    
}
