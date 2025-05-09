﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;

#if !SILVERLIGHT
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class WebDbItemAttributeChildFrequencyList : ReadOnlyListBase<WebDbItemAttributeChildFrequencyList, WebDbItemAttributeChildFrequency>
    {
#if SILVERLIGHT
    public ReadOnlyItemAttributeChildFrequencyList() { }
#else
        public WebDbItemAttributeChildFrequencyList() { }
#endif

        

#if !SILVERLIGHT

        private void DataPortal_Fetch(WebDbFrequencyCrosstabAndMapSelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            Dictionary<long, string> codes = new Dictionary<long, string>();
            List<MiniItem> items = new List<MiniItem>();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbFrequencyCrosstabAndMap", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandTimeout =100;
                    command.Parameters.Add(new SqlParameter("@attributeIdXAxis", criteria.attributeIdXAxis));
                    command.Parameters.Add(new SqlParameter("@setIdXAxis", criteria.setIdXAxis));
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
                            codes.Add(reader.GetInt64("ATTRIBUTE_ID"), reader.GetString("ATTRIBUTE_NAME"));
                        }
                        reader.NextResult();
                        while (reader.Read())
                        {
                            MiniItem mit = new MiniItem(reader.GetInt64("ItemId"));
                            string[] tmp = reader.GetString("X_atts").Split(',', StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in tmp)
                            {
                                mit.Attributes.Add(long.Parse(s));
                            }
                            items.Add(mit);
                        }
                    }
                }
                connection.Close();
                foreach(KeyValuePair<long, string> kvp in codes)
                {
                    int count = items.FindAll(found => found.Attributes.Contains(kvp.Key)).Count;
                    Add(WebDbItemAttributeChildFrequency.GetReadOnlyItemAttributeChildFrequency(
                        kvp.Value, kvp.Key, count, criteria.setIdXAxis, criteria.onlyThisAttribute, criteria.included
                        ));
                }
                int others = items.FindAll(found => found.Attributes.Count == 0).Count;
                Add(WebDbItemAttributeChildFrequency.GetReadOnlyItemAttributeChildFrequency(
                        "None of the above", -999999, others, criteria.setIdXAxis, criteria.onlyThisAttribute, criteria.included
                        ));
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }
        
#endif
    }
    
    [Serializable]
    public class WebDbFrequencyCrosstabAndMapSelectionCriteria : Csla.CriteriaBase<WebDbFrequencyCrosstabAndMapSelectionCriteria>
    {
        private static PropertyInfo<int> webDbIdProperty = RegisterProperty<int>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<int>("WebDbId", "WebDbId"));
        public int webDbId
        {
            get { return ReadProperty(webDbIdProperty); }
        }

        private static PropertyInfo<Int64> attributeIdXAxisProperty = RegisterProperty<Int64>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<Int64>("attributeIdXAxis", "attributeIdXAxis"));
        public Int64 attributeIdXAxis
        {
            get { return ReadProperty(attributeIdXAxisProperty); }
        }

        private static PropertyInfo<int> setIdXAxisProperty = RegisterProperty<int>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<int>("setIdXAxis", "setIdXAxis"));
        public int setIdXAxis
        {
            get { return ReadProperty(setIdXAxisProperty); }
        }
        private static PropertyInfo<string> NameXAxisProperty = RegisterProperty<string>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<string>("NameXAxis", "NameXAxis"));
        public string nameXAxis
        {
            get { return ReadProperty(NameXAxisProperty); }
        }
        private static PropertyInfo<string> GraphicProperty = RegisterProperty<string>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<string>("Graphic", "Graphic"));
        public string graphic
        {
            get { return ReadProperty(GraphicProperty); }
        }
        private static PropertyInfo<Int64> attributeIdYAxisProperty = RegisterProperty<Int64>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<Int64>("attributeIdYAxis", "attributeIdYAxis"));
        public Int64 attributeIdYAxis
        {
            get { return ReadProperty(attributeIdYAxisProperty); }
        }
        private static PropertyInfo<int> setIdYAxisProperty = RegisterProperty<int>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<int>("setIdYAxis", "setIdYAxis"));
        public int setIdYAxis
        {
            get { return ReadProperty(setIdYAxisProperty); }
        }
        private static PropertyInfo<string> NameYAxisProperty = RegisterProperty<string>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<string>("NameYAxis", "NameYAxis"));
        public string nameYAxis
        {
            get { return ReadProperty(NameYAxisProperty); }
        }
        private static PropertyInfo<string> includedProperty = RegisterProperty<string>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<string>("Included", "Included", ""));
        public string included
        {
            get { return ReadProperty(includedProperty); }
        }

        private static PropertyInfo<Int64> segmentsParentProperty = RegisterProperty<Int64>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<Int64>("segmentsParent", "segmentsParent"));
        public Int64 segmentsParent
        {
            get { return ReadProperty(segmentsParentProperty); }
        }
        private static PropertyInfo<int> setIdSegmentsProperty = RegisterProperty<int>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<int>("setIdSegments", "setIdSegments"));
        public int setIdSegments
        {
            get { return ReadProperty(setIdSegmentsProperty); }
        }
        private static PropertyInfo<Int64> onlyThisAttributeProperty = RegisterProperty<Int64>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<Int64>("onlyThisAttribute", "onlyThisAttribute"));
        public Int64 onlyThisAttribute
        {
            get { return ReadProperty(onlyThisAttributeProperty); }
        }

        private static PropertyInfo<string> WebDBMapNameProperty = RegisterProperty<string>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<string>("WebDBMapName", "WebDBMapName"));
        public string WebDBMapName
        {
            get { return ReadProperty(WebDBMapNameProperty); }
        }
        private static PropertyInfo<string> WebDBMapDescriptionProperty = RegisterProperty<string>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<string>("WebDBMapDescription", "WebDBMapDescription"));
        public string WebDBMapDescription
        {
            get { return ReadProperty(WebDBMapDescriptionProperty); }
        }
        private static PropertyInfo<int> MapIdProperty = RegisterProperty<int>(typeof(WebDbFrequencyCrosstabAndMapSelectionCriteria), new PropertyInfo<int>("MapId", "MapId"));
        public int MapId
        {
            get { return ReadProperty(MapIdProperty); }
        }


        public WebDbFrequencyCrosstabAndMapSelectionCriteria(int WebDbId, Int64 AttributeIdXAxis, int SetIdXAxis, string NameXaxis
                                                            , string Included = "", string Graphic =""
                                                            , Int64 OnlyThisAttribute = 0
                                                            , Int64 AttributeIdYAxis = 0, int SetIdYAxis = 0, string NameYaxis = ""
                                                            , Int64 SegmentsParent = 0, int SegmentsSetId = 0
                                                            , string WebDBMapName = "", string WebDBMapDescription = ""
                                                            , int MapId = 0)
        {
            LoadProperty(webDbIdProperty, WebDbId);
            LoadProperty(attributeIdXAxisProperty, AttributeIdXAxis);
            LoadProperty(setIdXAxisProperty, SetIdXAxis);
            LoadProperty(NameXAxisProperty, NameXaxis);
            LoadProperty(GraphicProperty, Graphic);
            LoadProperty(attributeIdYAxisProperty, AttributeIdYAxis);
            LoadProperty(setIdYAxisProperty, SetIdYAxis);
            LoadProperty(NameYAxisProperty, NameYaxis);
            LoadProperty(includedProperty, Included);
            LoadProperty(onlyThisAttributeProperty, OnlyThisAttribute);
            LoadProperty(segmentsParentProperty, SegmentsParent);
            LoadProperty(setIdSegmentsProperty, SegmentsSetId);
            LoadProperty(WebDBMapNameProperty, WebDBMapName);
            LoadProperty(WebDBMapDescriptionProperty, WebDBMapDescription);
            LoadProperty(MapIdProperty, MapId);
        }
        public WebDbFrequencyCrosstabAndMapSelectionCriteria() { }
    }

}