using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using Csla.Rules.CommonRules;
using Csla.Rules;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Collections;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class WebDbMap : BusinessBase<WebDbMap>
    {
        public WebDbMap() { }


        public override string ToString()
        {
            return WebDBMapName;
        }

        public static readonly PropertyInfo<int> WebDBMapIdProperty = RegisterProperty<int>(new PropertyInfo<int>("WebDBMapId", "WebDBMapId", 0));
        public int WebDBMapId
        {
            get
            {
                return GetProperty(WebDBMapIdProperty);
            }
            set
            {
                SetProperty(WebDBMapIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> WebDBIdProperty = RegisterProperty<int>(new PropertyInfo<int>("WebDBId", "WebDBId", 0));
        public int WebDBId
        {
            get
            {
                return GetProperty(WebDBIdProperty);
            }
            set
            {
                SetProperty(WebDBIdProperty, value);
            }
        }


        public static readonly PropertyInfo<string> WebDBMapNameProperty = RegisterProperty<string>(new PropertyInfo<string>("WebDBMapName", "WebDBMapName", ""));
        public string WebDBMapName
        {
            get
            {
                return GetProperty(WebDBMapNameProperty);
            }
            set
            {
                SetProperty(WebDBMapNameProperty,
                    value.Length > 1000 ? value.Substring(0, 1000) : value);
            }
        }

        public static readonly PropertyInfo<int> ColumnsPublicSetIDProperty = RegisterProperty<int>(new PropertyInfo<int>("ColumnsPublicSetID", "ColumnsPublicSetID", 0));
        public int ColumnsPublicSetID
        {
            get
            {
                return GetProperty(ColumnsPublicSetIDProperty);
            }
            set
            {
                SetProperty(ColumnsPublicSetIDProperty, value);
            }
        }
        public static readonly PropertyInfo<int> ColumnsSetIDProperty = RegisterProperty<int>(new PropertyInfo<int>("ColumnsSetID", "ColumnsSetID", 0));
        public int ColumnsSetID
        {
            get
            {
                return GetProperty(ColumnsSetIDProperty);
            }
            set
            {
                SetProperty(ColumnsSetIDProperty, value);
            }
        }
        public static readonly PropertyInfo<string> ColumnsPublicSetNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ColumnsPublicSetName", "ColumnsPublicSetName", ""));
        public string ColumnsPublicSetName
        {
            get
            {
                return GetProperty(ColumnsPublicSetNameProperty);
            }
        }

        public static readonly PropertyInfo<int> ColumnsPublicAttributeIDProperty = RegisterProperty<int>(new PropertyInfo<int>("ColumnsPublicAttributeID", "ColumnsPublicAttributeID", 0));
        public int ColumnsPublicAttributeID
        {
            get
            {
                return GetProperty(ColumnsPublicAttributeIDProperty);
            }
            set
            {
                SetProperty(ColumnsPublicAttributeIDProperty, value);
            }
        }
        public static readonly PropertyInfo<Int64> ColumnsAttributeIDProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ColumnsAttributeID", "ColumnsAttributeID"));
        public Int64 ColumnsAttributeID
        {
            get
            {
                return GetProperty(ColumnsAttributeIDProperty);
            }
            set
            {
                SetProperty(ColumnsAttributeIDProperty, value);
            }
        }
        public static readonly PropertyInfo<string> ColumnsPublicAttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ColumnsPublicAttributeName", "ColumnsPublicAttributeName", ""));
        public string ColumnsPublicAttributeName
        {
            get
            {
                return GetProperty(ColumnsPublicAttributeNameProperty);
            }
        }

        public static readonly PropertyInfo<int> RowsPublicSetIDProperty = RegisterProperty<int>(new PropertyInfo<int>("RowsPublicSetID", "RowsPublicSetID", 0));
        public int RowsPublicSetID
        {
            get
            {
                return GetProperty(RowsPublicSetIDProperty);
            }
            set
            {
                SetProperty(RowsPublicSetIDProperty, value);
            }
        }
        public static readonly PropertyInfo<int> RowsSetIDProperty = RegisterProperty<int>(new PropertyInfo<int>("RowsSetID", "RowsSetID", 0));
        public int RowsSetID
        {
            get
            {
                return GetProperty(RowsSetIDProperty);
            }
            set
            {
                SetProperty(RowsSetIDProperty, value);
            }
        }
        public static readonly PropertyInfo<string> RowsPublicSetNameProperty = RegisterProperty<string>(new PropertyInfo<string>("RowsPublicSetName", "RowsPublicSetName", ""));
        public string RowsPublicSetName
        {
            get
            {
                return GetProperty(RowsPublicSetNameProperty);
            }
        }

        public static readonly PropertyInfo<int> RowsPublicAttributeIDProperty = RegisterProperty<int>(new PropertyInfo<int>("RowsPublicAttributeID", "RowsPublicAttributeID", 0));
        public int RowsPublicAttributeID
        {
            get
            {
                return GetProperty(RowsPublicAttributeIDProperty);
            }
            set
            {
                SetProperty(RowsPublicAttributeIDProperty, value);
            }
        }
        public static readonly PropertyInfo<Int64> RowsAttributeIDProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("RowsAttributeID", "RowsAttributeID"));
        public Int64 RowsAttributeID
        {
            get
            {
                return GetProperty(RowsAttributeIDProperty);
            }
            set
            {
                SetProperty(RowsAttributeIDProperty, value);
            }
        }
        public static readonly PropertyInfo<string> RowsPublicAttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("RowsPublicAttributeName", "RowsPublicAttributeName", ""));
        public string RowsPublicAttributeName
        {
            get
            {
                return GetProperty(RowsPublicAttributeNameProperty);
            }
        }

        public static readonly PropertyInfo<int> SegmentsPublicSetIDProperty = RegisterProperty<int>(new PropertyInfo<int>("SegmentsPublicSetID", "SegmentsPublicSetID", 0));
        public int SegmentsPublicSetID
        {
            get
            {
                return GetProperty(SegmentsPublicSetIDProperty);
            }
            set
            {
                SetProperty(SegmentsPublicSetIDProperty, value);
            }
        }
        public static readonly PropertyInfo<int> SegmentsSetIDProperty = RegisterProperty<int>(new PropertyInfo<int>("SegmentsSetID", "SegmentsSetID", 0));
        public int SegmentsSetID
        {
            get
            {
                return GetProperty(SegmentsSetIDProperty);
            }
            set
            {
                SetProperty(SegmentsSetIDProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SegmentsPublicSetNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SegmentsPublicSetName", "SegmentsPublicSetName", ""));
        public string SegmentsPublicSetName
        {
            get
            {
                return GetProperty(SegmentsPublicSetNameProperty);
            }
        }

        public static readonly PropertyInfo<int> SegmentsPublicAttributeIDProperty = RegisterProperty<int>(new PropertyInfo<int>("SegmentsPublicAttributeID", "SegmentsPublicAttributeID", 0));
        public int SegmentsPublicAttributeID
        {
            get
            {
                return GetProperty(SegmentsPublicAttributeIDProperty);
            }
            set
            {
                SetProperty(SegmentsPublicAttributeIDProperty, value);
            }
        }
        public static readonly PropertyInfo<Int64> SegmentsAttributeIDProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("SegmentsAttributeID", "SegmentsAttributeID"));
        public Int64 SegmentsAttributeID
        {
            get
            {
                return GetProperty(SegmentsAttributeIDProperty);
            }
            set
            {
                SetProperty(SegmentsAttributeIDProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SegmentsPublicAttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SegmentsPublicAttributeName", "SegmentsPublicAttributeName", ""));
        public string SegmentsPublicAttributeName
        {
            get
            {
                return GetProperty(SegmentsPublicAttributeNameProperty);
            }
        }


        public static readonly PropertyInfo<string> WebDBMapDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("WebDBMapDescription", "WebDBMapDescription", ""));
        public string WebDBMapDescription
        {
            get
            {
                return GetProperty(WebDBMapDescriptionProperty);
            }
            set
            {
                SetProperty(WebDBMapDescriptionProperty, value);
            }
        }


        protected override void AddBusinessRules()
        {
            BusinessRules.AddRule(new IsNotInRole(AuthorizationActions.EditObject, "ReadOnlyUser"));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT
        public void MarkAsOldAndDirty()
        {
            this.MarkOld();
            this.MarkDirty(true);
        }

        protected void DataPortal_Fetch(WebDBMapCriteria crit)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbMap", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", crit.WebDbId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_MAP_ID", crit.WebDbMapId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            MarkOld();
                            LoadProperty(WebDBIdProperty, reader.GetInt32("WEBDB_ID"));
                            LoadProperty(WebDBMapIdProperty, reader.GetInt32("WEBDB_MAP_ID"));
                            LoadProperty(ColumnsPublicSetIDProperty, reader.GetInt32("COLUMNS_PUBLIC_SET_ID"));
                            LoadProperty(ColumnsSetIDProperty, reader.GetInt32("COLUMNS_SET_ID"));
                            LoadProperty(ColumnsPublicSetNameProperty, reader.GetString("COLUMNS_SET_NAME"));

                            LoadProperty(ColumnsPublicAttributeIDProperty, reader.GetInt32("COLUMNS_PUBLIC_ATTRIBUTE_ID"));
                            LoadProperty(ColumnsAttributeIDProperty, reader.GetInt64("COLUMNS_ATTRIBUTE_ID"));
                            LoadProperty(ColumnsPublicAttributeNameProperty, reader.GetString("COLUMNS_ATTRIBUTE_NAME"));

                            LoadProperty(RowsPublicSetIDProperty, reader.GetInt32("ROWS_PUBLIC_SET_ID"));
                            LoadProperty(RowsSetIDProperty, reader.GetInt32("ROWS_SET_ID"));
                            LoadProperty(RowsPublicSetNameProperty, reader.GetString("ROWS_SET_NAME"));

                            LoadProperty(RowsPublicAttributeIDProperty, reader.GetInt32("ROWS_PUBLIC_ATTRIBUTE_ID"));
                            LoadProperty(RowsAttributeIDProperty, reader.GetInt64("ROWS_ATTRIBUTE_ID"));
                            LoadProperty(RowsPublicAttributeNameProperty, reader.GetString("ROWS_ATTRIBUTE_NAME"));

                            LoadProperty(SegmentsPublicSetIDProperty, reader.GetInt32("SEGMENTS_PUBLIC_SET_ID"));
                            LoadProperty(SegmentsSetIDProperty, reader.GetInt32("SEGMENTS_SET_ID"));
                            LoadProperty(SegmentsPublicSetNameProperty, reader.GetString("SEGMENTS_SET_NAME"));

                            LoadProperty(SegmentsPublicAttributeIDProperty, reader.GetInt32("SEGMENTS_PUBLIC_ATTRIBUTE_ID"));
                            LoadProperty(SegmentsAttributeIDProperty, reader.GetInt64("SEGMENTS_ATTRIBUTE_ID"));
                            LoadProperty(SegmentsPublicAttributeNameProperty, reader.GetString("SEGMENTS_ATTRIBUTE_NAME"));

                            LoadProperty(WebDBMapNameProperty, reader.GetString("MAP_NAME"));
                            LoadProperty(WebDBMapDescriptionProperty, reader.GetString("MAP_DESCRIPTION"));
                        }
                    }
                }
                connection.Close();
            }
        }
        protected override void DataPortal_Insert()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbMapAdd", connection))
                {

                    
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId ));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", WebDBId)); 
                    command.Parameters.Add(new SqlParameter("@ColumnsSetID", ColumnsSetID));
                    command.Parameters.Add(new SqlParameter("@ColumnsAttributeID", ColumnsAttributeID));
                    command.Parameters.Add(new SqlParameter("@RowsSetID", RowsSetID));
                    command.Parameters.Add(new SqlParameter("@RowsAttributeID", RowsAttributeID));
                    command.Parameters.Add(new SqlParameter("@SegmentsSetID", SegmentsSetID));
                    command.Parameters.Add(new SqlParameter("@SegmentsAttributeID", SegmentsAttributeID));
                    command.Parameters.Add(new SqlParameter("@MapName", WebDBMapName));
                    command.Parameters.Add(new SqlParameter("@MapDescription", WebDBMapDescription));
                    SqlParameter par = new SqlParameter("@WEBDB_MAP_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    par.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(par);
                    command.ExecuteNonQuery();
                    int? res = par.Value as int?;
                    if (res > 0)
                    {
                        LoadProperty(WebDBMapIdProperty, par.Value);
                    }
                    else 
                    {
                        throw new Exception("Can't create MAP: an unexpected error occurred.");
                    }
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbMapEdit", connection))
                {
                    //@REVIEW_ID INT,
                    //@WEBDB_ID int,
                    //@ColumnsPublicSetID int,
                    //@ColumnsPublicAttributeID int,
                    //@RowsPublicSetID int,
                    //@RowsPublicAttributeID int,
                    //@SegmentsPublicSetID int,
                    //@SegmentsPublicAttributeID int,
                    //@MapName nvarchar(1000),
                    //@MapDescription nvarchar(max),
                    //@WEBDB_MAP_ID int output
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", WebDBId));
                    command.Parameters.Add(new SqlParameter("@ColumnsSetID", ColumnsSetID));
                    command.Parameters.Add(new SqlParameter("@ColumnsAttributeID", ColumnsAttributeID));
                    command.Parameters.Add(new SqlParameter("@RowsSetID", RowsSetID));
                    command.Parameters.Add(new SqlParameter("@RowsAttributeID", RowsAttributeID));
                    command.Parameters.Add(new SqlParameter("@SegmentsSetID", SegmentsSetID));
                    command.Parameters.Add(new SqlParameter("@SegmentsAttributeID", SegmentsAttributeID));
                    command.Parameters.Add(new SqlParameter("@MapName", WebDBMapName));
                    command.Parameters.Add(new SqlParameter("@MapDescription", WebDBMapDescription));
                    SqlParameter par = new SqlParameter("@WEBDB_MAP_ID", System.Data.SqlDbType.Int);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbMapDelete", connection))
                {
                    //@REVIEW_ID INT,
                    //@WEBDB_ID int,
                    //@WEBDB_MAP_ID int
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", WebDBId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_MAP_ID", WebDBMapId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            
        }

        internal static WebDbMap GetWebDbMap(SafeDataReader reader)
        {
            WebDbMap returnValue = new WebDbMap();
            returnValue.MarkOld();

            returnValue.LoadProperty(WebDBIdProperty, reader.GetInt32("WEBDB_ID"));
            returnValue.LoadProperty(WebDBMapIdProperty, reader.GetInt32("WEBDB_MAP_ID"));
            returnValue.LoadProperty(ColumnsPublicSetIDProperty, reader.GetInt32("COLUMNS_PUBLIC_SET_ID"));
            returnValue.LoadProperty(ColumnsSetIDProperty, reader.GetInt32("COLUMNS_SET_ID"));
            returnValue.LoadProperty(ColumnsPublicSetNameProperty, reader.GetString("COLUMNS_SET_NAME"));

            returnValue.LoadProperty(ColumnsPublicAttributeIDProperty, reader.GetInt32("COLUMNS_PUBLIC_ATTRIBUTE_ID"));
            returnValue.LoadProperty(ColumnsAttributeIDProperty, reader.GetInt64("COLUMNS_ATTRIBUTE_ID"));
            returnValue.LoadProperty(ColumnsPublicAttributeNameProperty, reader.GetString("COLUMNS_ATTRIBUTE_NAME"));

            returnValue.LoadProperty(RowsPublicSetIDProperty, reader.GetInt32("ROWS_PUBLIC_SET_ID"));
            returnValue.LoadProperty(RowsSetIDProperty, reader.GetInt32("ROWS_SET_ID"));
            returnValue.LoadProperty(RowsPublicSetNameProperty, reader.GetString("ROWS_SET_NAME"));

            returnValue.LoadProperty(RowsPublicAttributeIDProperty, reader.GetInt32("ROWS_PUBLIC_ATTRIBUTE_ID"));
            returnValue.LoadProperty(RowsAttributeIDProperty, reader.GetInt64("ROWS_ATTRIBUTE_ID"));
            returnValue.LoadProperty(RowsPublicAttributeNameProperty, reader.GetString("ROWS_ATTRIBUTE_NAME"));

            returnValue.LoadProperty(SegmentsPublicSetIDProperty, reader.GetInt32("SEGMENTS_PUBLIC_SET_ID"));
            returnValue.LoadProperty(SegmentsSetIDProperty, reader.GetInt32("SEGMENTS_SET_ID"));
            returnValue.LoadProperty(SegmentsPublicSetNameProperty, reader.GetString("SEGMENTS_SET_NAME"));

            returnValue.LoadProperty(SegmentsPublicAttributeIDProperty, reader.GetInt32("SEGMENTS_PUBLIC_ATTRIBUTE_ID"));
            returnValue.LoadProperty(SegmentsAttributeIDProperty, reader.GetInt64("SEGMENTS_ATTRIBUTE_ID"));
            returnValue.LoadProperty(SegmentsPublicAttributeNameProperty, reader.GetString("SEGMENTS_ATTRIBUTE_NAME"));

            returnValue.LoadProperty(WebDBMapNameProperty, reader.GetString("MAP_NAME"));
            returnValue.LoadProperty(WebDBMapDescriptionProperty, reader.GetString("MAP_DESCRIPTION"));

            return returnValue;
        }

#endif
    }
    [Serializable]
    public class WebDBMapCriteria : CriteriaBase<WebDBMapCriteria>
    {
        public WebDBMapCriteria(int webDbId, int webDbMapId)
        {
            LoadProperty(WebDbIdProperty, webDbId);
            LoadProperty(WebDbMapIdProperty, webDbMapId);
        }
        private static PropertyInfo<int> WebDbIdProperty = RegisterProperty<int>(typeof(WebDBMapCriteria), new PropertyInfo<int>("WebDbId", "WebDbId"));
        public int WebDbId
        {
            get { return ReadProperty(WebDbIdProperty); }
        }
        private static PropertyInfo<int> WebDbMapIdProperty = RegisterProperty<int>(typeof(WebDBMapCriteria), new PropertyInfo<int>("WebDbMapId", "WebDbMapId"));
        public int WebDbMapId
        {
            get { return ReadProperty(WebDbMapIdProperty); }
        }
    }
}
