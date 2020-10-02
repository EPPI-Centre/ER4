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
    public class WebDbReviewSet : BusinessBase<WebDbReviewSet>
    {
        
        public WebDbReviewSet() { }


        public override string ToString()
        {
            return SetName;
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new SkipEmptyContractResolver() });
        }
        

        public AttributeSet GetAttributeSet(Int64 AttributeSetId)
        {
            AttributeSet returnValue = null;
            foreach (AttributeSet rs in Attributes)
            {
                if (rs.AttributeSetId == AttributeSetId)
                {
                    return rs;
                }
                returnValue = rs.GetAttributeSet(AttributeSetId);
                if (returnValue != null)
                {
                    return returnValue;
                }
            }
            return returnValue;
        }

        public AttributeSet GetAttributeSetFromAttributeId(Int64 AttributeId)
        {
            AttributeSet returnValue = null;
            foreach (AttributeSet rs in Attributes)
            {
                if (rs.AttributeId == AttributeId)
                {
                    return rs;
                }
                returnValue = rs.GetAttributeSetFromAttributeId(AttributeId);
                if (returnValue != null)
                {
                    return returnValue;
                }
            }
            return returnValue;
        }

        

        public int AttributesCount()
        {
            int retVal = 0;
            foreach (AttributeSet attribute in this.Attributes)
            {
                retVal += AttributesCount();
            }
            retVal += Attributes.Count;
            return retVal;
        }
        
        

        // for controlling the appearance of the tree control
        public static readonly PropertyInfo<bool> DisplayIsParentProperty = RegisterProperty<bool>(new PropertyInfo<bool>("DisplayIsParent", "DisplayIsParent", false));
        public bool DisplayIsParent
        {
            get
            {
                return GetProperty(DisplayIsParentProperty);
            }
            set
            {
                SetProperty(DisplayIsParentProperty, value);
            }
        }

        public static readonly PropertyInfo<int> ReviewSetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewSetId", "Set Id", 0));
        public int ReviewSetId
        {
            get
            {
                return GetProperty(ReviewSetIdProperty);
            }
        }

        public static readonly PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "Set Id", 0));
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
            set
            {
                SetProperty(SetIdProperty, value);
            }
        }
        public static readonly PropertyInfo<int> WebDBSetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("WebDBSetId", "WebDBSetId", 0));
        public int WebDBSetId
        {
            get
            {
                return GetProperty(WebDBSetIdProperty);
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
                LoadProperty(WebDBIdProperty, value);
            }
        }
        public static readonly PropertyInfo<int> OriginalSetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("OriginalSetId", "OriginalSetId", 0));
        public int OriginalSetId
        {
            get
            {
                return GetProperty(OriginalSetIdProperty);
            }
        }
        public static readonly PropertyInfo<int> ReviewIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ReviewId", "ReviewId", 0));
        public int ReviewId
        {
            get
            {
                return GetProperty(ReviewIdProperty);
            }
            set
            {
                SetProperty(ReviewIdProperty, value);
            }
        }

        public static readonly PropertyInfo<int> SetTypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TypeId", "TypeId", 0));
        public int SetTypeId
        {
            get
            {
                return GetProperty(SetTypeIdProperty);
            }
            set
            {
                SetProperty(SetTypeIdProperty, value);
            }
        }

        public static readonly PropertyInfo<ReadOnlySetType> SetTypeProperty = RegisterProperty<ReadOnlySetType>(new PropertyInfo<ReadOnlySetType>("SetType", "set type"));
        public ReadOnlySetType SetType
        {
            get
            {
                return GetProperty(SetTypeProperty);
            }
            set
            {
                SetProperty(SetTypeProperty, value);
            }
        }

        public static readonly PropertyInfo<string> SetNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SetName", "Set Name", string.Empty));
        public string SetName
        {
            get
            {
                return GetProperty(SetNameProperty);
            }
            set
            {
                SetProperty(SetNameProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SetDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("SetDescription", "SetDescription", string.Empty));
        public string SetDescription
        {
            get
            {
                return GetProperty(SetDescriptionProperty);
            }
            set
            {
                SetProperty(SetDescriptionProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> AllowCodingEditsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("AllowCodingEdits", "Allow coding edits", false));

        public bool AllowCodingEdits
        {
            get
            {

                return GetProperty(AllowCodingEditsProperty)&& 
                    Csla.Rules.BusinessRules.HasPermission( AuthorizationActions.EditObject, this);
                //&& Csla.Security.AuthorizationRules.CanEditObject(this.GetType());
            }
            set
            {
                SetProperty(AllowCodingEditsProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> CodingIsFinalProperty = RegisterProperty<bool>(new PropertyInfo<bool>("CodingIsFinal", "Coding Is Final", true));
        //consider changing this by applying the following implementation inside ER4!
        //custom runtime JSON serialisation:
        //https://blog.rsuter.com/advanced-newtonsoft-json-dynamically-rename-or-ignore-properties-without-changing-the-serialized-class/

        public bool CodingIsFinal
        {
            get
            {
                return GetProperty(CodingIsFinalProperty);
            }
            set
            {
                SetProperty(CodingIsFinalProperty, value);
            }
        }

        public static readonly PropertyInfo<int> SetOrderProperty = RegisterProperty<int>(new PropertyInfo<int>("SetOrder", "SetOrder", 0));

        public int SetOrder
        {
            get
            {
                return GetProperty(SetOrderProperty);
            }
            set
            {
                SetProperty(SetOrderProperty, value);
            }
        }

        [NotUndoable]
        public static readonly PropertyInfo<AttributeSetList> AttributeSetProperty = RegisterProperty<AttributeSetList>(new PropertyInfo<AttributeSetList>("Attributes", "Attributes"));
        public AttributeSetList Attributes
        {
            get
            {
                return GetProperty(AttributeSetProperty);
            }
            set
            {
                SetProperty(AttributeSetProperty, value);
            }
        }

        

        public static readonly PropertyInfo<bool> UserCanEditURLsProperty = RegisterProperty<bool>(new PropertyInfo<bool>("UserCanEditURLs", "UserCanEditURLs", false));
        public bool UserCanEditURLs
        {
            get
            {
                return GetProperty(UserCanEditURLsProperty);
            }
            set
            {
                SetProperty(UserCanEditURLsProperty, value);
            }
        }

        public AttributeSet GetSetByExt_URL(string val)
        {
            AttributeSet retVal = null;
            foreach (AttributeSet atset in Attributes)
            {
                if (atset.ExtURL == val)
                {
                    return atset;
                }
                else
                {
                    if (atset.Attributes.Count > 0)
                    {
                        retVal = atset.GetSetByExt_URL(val);
                        if (retVal != null)
                        {
                            return retVal;
                        }
                    }
                }
            }
            return retVal;
        }

        public AttributeSet GetSetByName(string val) // gets the first occurence of a given AttributeName
        {
            AttributeSet retVal = null;
            foreach (AttributeSet atset in Attributes)
            {
                if (atset.AttributeName.Trim().ToLower() == val.Trim().ToLower())
                {
                    return atset;
                }
                else
                {
                    if (atset.Attributes.Count > 0)
                    {
                        retVal = atset.GetSetByName(val.Trim().ToLower());
                        if (retVal != null)
                        {
                            return retVal;
                        }
                    }
                }
            }
            return retVal;
        }
        

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser", "RegularUser" };
        //    string[] denyEditSave = new string[] { "ReadOnlyUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ReviewSet), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ReviewSet), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ReviewSet), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ReviewSet), canRead);
        //    AuthorizationRules.DenyEdit(typeof(ReviewSet), denyEditSave);

        //    //AuthorizationRules.AllowRead(ReviewSetIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AllowCodingEditsProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetTypeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetTypeProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeSetProperty, canRead);
        //    //AuthorizationRules.AllowRead(CodingIsFinalProperty, canRead);

        //    //AuthorizationRules.AllowWrite(AllowCodingEditsProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(SetNameProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(CodingIsFinalProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            BusinessRules.AddRule(new IsNotInRole(AuthorizationActions.EditObject, "ReadOnlyUser"));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected void DataPortal_Fetch(WebDBReviewSetCrit criteria)//contains the WebDbId
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbGetCodesets", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", criteria.WebDbId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_PUBLIC_SET_ID", criteria.WebDBSetId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            WebDbReviewSet.GetWebDbReviewSet(reader, this);
                            WebDbReviewSet.ReviewSetFromDBCommonPart(this);
                        }
                    }
                }
                connection.Close();
                //we get the list of set types and plug them into each set in the review
                ReadOnlySetTypeList RoSTL = DataPortal.Fetch<ReadOnlySetTypeList>();
                
                foreach (ReadOnlySetType rost in RoSTL)
                {
                    if (this.SetTypeId == rost.SetTypeId)
                    {
                        this.SetType = rost;
                        break;
                    }
                }
                
            }
        }

        protected override void DataPortal_Insert()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbCodesetAdd", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", WebDBId)); // All set to review specific keywords atm
                    command.Parameters.Add(new SqlParameter("@Set_ID", SetId));

                    SqlParameter par = new SqlParameter("@WEBDB_PUBLIC_SET_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par); 
                    command.Parameters["@WEBDB_PUBLIC_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    
                    command.ExecuteNonQuery();
                    LoadProperty(WebDBSetIdProperty, command.Parameters["@WEBDB_PUBLIC_SET_ID"].Value);
                    DataPortal_Fetch(new WebDBReviewSetCrit(WebDBId, WebDBSetId));
                }
                connection.Close();
            }
            WebDbReviewSet.ReviewSetFromDBCommonPart(this);
            MarkOld();
        }

        protected override void DataPortal_Update()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_WebDbCodeSetEdit", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", WebDBId));
                    command.Parameters.Add(new SqlParameter("@Set_ID", SetId));
                    command.Parameters.Add(new SqlParameter("@Public_Name", SetName));
                    command.Parameters.Add(new SqlParameter("@Public_Descr", SetDescription));
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
                using (SqlCommand command = new SqlCommand("st_WebDbCodeSetDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@WEBDB_ID", WebDBId));
                    command.Parameters.Add(new SqlParameter("@Set_ID", SetId));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

        }

        internal static WebDbReviewSet GetWebDbReviewSet(SafeDataReader reader, WebDbReviewSet selfCreated = null)
        {//the second parameter allows to use this static method from the DataPortalFetch() method in this same class
            WebDbReviewSet returnValue;
            if (selfCreated == null) returnValue = new WebDbReviewSet();
            else returnValue = selfCreated;
            returnValue.MarkOld();
            returnValue.Attributes = AttributeSetList.NewAttributeSetList();
            returnValue.LoadProperty<int>(ReviewSetIdProperty, reader.GetInt32("REVIEW_SET_ID"));
            returnValue.LoadProperty<int>(OriginalSetIdProperty, reader.GetInt32("ORIGINAL_SET_ID"));
            returnValue.LoadProperty<int>(ReviewIdProperty, reader.GetInt32("REVIEW_ID"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<bool>(AllowCodingEditsProperty, reader.GetBoolean("ALLOW_CODING_EDITS"));
            returnValue.LoadProperty<bool>(CodingIsFinalProperty, reader.GetBoolean("CODING_IS_FINAL"));
            returnValue.LoadProperty<int>(SetTypeIdProperty, reader.GetInt32("SET_TYPE_ID"));
            returnValue.LoadProperty<int>(SetOrderProperty, reader.GetInt32("SET_ORDER"));
            returnValue.LoadProperty<string>(SetNameProperty, reader.GetString("SET_NAME"));
            returnValue.LoadProperty<string>(SetDescriptionProperty, reader.GetString("SET_DESCRIPTION"));
            returnValue.LoadProperty<bool>(UserCanEditURLsProperty, reader.GetBoolean("USER_CAN_EDIT_URLS"));

            //returnValue.LoadProperty<string>(SetTypeProperty, reader.GetString("SET_TYPE"));
            returnValue.TempMaxDepth = reader.GetInt32("MAX_DEPTH");
            
            returnValue.LoadProperty<int>(WebDBIdProperty, reader.GetInt32("WEBDB_ID"));
            returnValue.LoadProperty<int>(WebDBSetIdProperty, reader.GetInt32("WEBDB_PUBLIC_SET_ID"));

            return returnValue;
        }

        

        

        internal static void ReviewSetFromDBCommonPart(WebDbReviewSet reviewSet)
        {

            List<AttributeSet> flatList = new List<AttributeSet>();
            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
            {

                connection2.Open();
                using (SqlCommand command2 = new SqlCommand("st_WebDbGetAllAttributesInSet", connection2))
                {

                    command2.CommandType = System.Data.CommandType.StoredProcedure;
                    command2.Parameters.Add(new SqlParameter("@SET_ID", reviewSet.SetId));
                    command2.Parameters.Add(new SqlParameter("@WEBDB_ID", reviewSet.WebDBId));
                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command2.ExecuteReader()))
                    {
                        while (reader2.Read())
                        {

                            AttributeSet newAttributeSet = AttributeSet.GetAttributeSetForFlatList(reader2, reviewSet.TempMaxDepth);
                            flatList.Add(newAttributeSet);
                        }
                        reader2.Close();
                    }
                }
                connection2.Close();
            }
            reviewSet.RecursiveBuildTree(null, flatList);

        }

        private void RecursiveBuildTree(AttributeSet parent, List<AttributeSet> flatList)
        {
            AttributeSetList workingAttsList = new AttributeSetList();
            //AttributeSet parent = null;
            long parentId = 0;
            workingAttsList.RaiseListChangedEvents = false;
            if (parent == null)
            {
                this.Attributes = workingAttsList;
            }
            else
            {//need to find the list of children we'll be populating
                //parent = this.GetAttributeSetFromAttributeId(parentId);
                parentId = parent.AttributeId;
                parent.Attributes = workingAttsList;
            }
            
                IEnumerable<AttributeSet> CodesAtThisLevel = flatList.FindAll(found => found.ParentAttributeId == parentId).OrderBy(Att => Att.AttributeOrder);
                flatList.RemoveAll(found => found.ParentAttributeId == parentId);
                foreach(AttributeSet aSet in CodesAtThisLevel)
                {
                    if (parent != null)
                    {
                        aSet.HostAttribute = parent;
                    }
                    workingAttsList.Add(aSet);
                    //flatList.Remove(aSet);
                    this.RecursiveBuildTree(aSet, flatList);
                }

            //if (parentId == 0 && flatList.Count > 0) throw new Exception("did not parse all codes in tree...");
            workingAttsList.RaiseListChangedEvents = true;
        }

        public int TempMaxDepth = 0;
#endif
    }
    [Serializable]
    public class WebDBReviewSetCrit : Csla.CriteriaBase<WebDBReviewSetCrit>
    {
        public WebDBReviewSetCrit() { }
        public WebDBReviewSetCrit(int webDbId, int webDBSetId) 
        {
            LoadProperty(WebDbIdProperty, webDbId);
            LoadProperty(WebDBSetIdProperty, webDBSetId);
        }
        private static PropertyInfo<int> WebDbIdProperty = RegisterProperty<int>(typeof(WebDBReviewSetCrit), new PropertyInfo<int>("WebDbId", "WebDbId"));
        public int WebDbId
        {
            get { return ReadProperty(WebDbIdProperty); }
        }

        private static PropertyInfo<int> WebDBSetIdProperty = RegisterProperty<int>(typeof(WebDBReviewSetCrit), new PropertyInfo<int>("WebDBSetId", "WebDBSetId"));
        public int WebDBSetId
        {
            get { return ReadProperty(WebDBSetIdProperty); }
        }

    }
}
