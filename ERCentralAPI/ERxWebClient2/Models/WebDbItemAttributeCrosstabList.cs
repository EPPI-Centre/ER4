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

#if !SILVERLIGHT

        private void DataPortal_Fetch(WebDbFrequencyCrosstabAndMapSelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;

            Dictionary<long, string> codesY = new Dictionary<long, string>();
            List<MiniItem> items = new List<MiniItem>();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbFrequencyCrosstabAndMap", connection))
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
                    command.Parameters.Add(new SqlParameter("@WebDbId", criteria.webDbId));
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
                        while (reader.Read())
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
                        
                    }
                }
                connection.Close();
                LoadProperty(WebDbIdProperty, criteria.webDbId);
                LoadProperty(SetIdXProperty, criteria.setIdXAxis);
                LoadProperty(SetIdYProperty, criteria.setIdYAxis);
                LoadProperty(FilterAttributeIdProperty, criteria.onlyThisAttribute);
                LoadProperty(IncludedProperty, criteria.included);
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
            }

        }

#endif
    }


}
