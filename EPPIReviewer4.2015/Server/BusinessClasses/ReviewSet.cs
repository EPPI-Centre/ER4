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
    [JsonObject(MemberSerialization.OptIn)]
    public class ReviewSet : BusinessBase<ReviewSet>
    {
        public static void GetReviewSet(int SetId, EventHandler<DataPortalResult<ReviewSet>> handler)
        {
            DataPortal<ReviewSet> dp = new DataPortal<ReviewSet>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new SingleCriteria<ReviewSet, int>(SetId));
        }
        public ReviewSet() { }
#if SILVERLIGHT        
        public System.Windows.Media.Brush ForeGround
        {
            get
            {
                switch (SetTypeId)
                {
                    case 6:
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)40, (byte)90, (byte)235));
                    case 5:
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)0, (byte)188, (byte)0));
                    default:
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                        
                }
            }
        }
        public bool CanBeScreeningSet
        {
            get
            {
                return SetTypeId == 5;
            }
        }
        private bool _IsInArmContext = false;
        public bool IsInArmContext
        {
            get { return _IsInArmContext; }
            set
            {
                _IsInArmContext = value;
                if (this.SetTypeId == 5)
                {//we want to preven coding inside arms for Screening sets
                    foreach (AttributeSet attributeSet in this.Attributes)
                    {
                        attributeSet.IsInArmContext = value;
                    }
                }
            }
        }
#endif

        public override string ToString()
        {
            return SetName;
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new SkipEmptyContractResolver() });
        }
        internal static ReviewSet NewReviewSet()
        {
            ReviewSet returnValue = new ReviewSet();
            //returnValue.ValidationRules.CheckRules();
            return returnValue;
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

        public void ClearItemData()
        {
            ItemSetId = 0;
            ItemSetContactId = 0;
            ItemSetIsCompleted = false;
            ItemSetIsLocked = false;
            ItemSetItemId = 0;
            ItemSetSetId = 0;
            foreach (AttributeSet attributeSet in this.Attributes)
            {
                if (attributeSet.AttributeTypeId != 1) // 1 is not selectable
                {
                    attributeSet.ItemData = null;
                    attributeSet.IsSelected = false;
                }
                attributeSet.ClearItemData();
            }
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
        public int ItemAttributesCount()
        {
            int retVal = 0;
            foreach (AttributeSet attribute in this.Attributes)
            {
                if (attribute.ItemData != null) retVal++;
                retVal += attribute.ItemAttributesCount();
            }
            return retVal;
        }
        public void SetIsClean()
        {
            this.MarkClean();
        }

        public void SetIsNew()
        {
            this.MarkNew();
        }

        public void SetIsOld()
        {
            this.MarkOld();
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
        [JsonProperty]
        public int ReviewSetId
        {
            get
            {
                return GetProperty(ReviewSetIdProperty);
            }
        }

        public static readonly PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "Set Id", 0));
        [JsonProperty]
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
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
        [JsonProperty]
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
        [JsonProperty]
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
        [JsonProperty]
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
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
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
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
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
#if (CSLA_NETCORE)
        [JsonProperty]
#endif
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
        [JsonProperty(Order = 200)]
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

        public static readonly PropertyInfo<Int64> ItemSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemSetId", "ItemSetId"));
        public Int64 ItemSetId
        {
            get
            {
                return GetProperty(ItemSetIdProperty);
            }
            set
            {
                SetProperty(ItemSetIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int64> ItemSetItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemSetItemId", "ItemSetItemId"));
        public Int64 ItemSetItemId
        {
            get
            {
                return GetProperty(ItemSetItemIdProperty);
            }
            set
            {
                SetProperty(ItemSetItemIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int32> ItemSetSetIdProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("ItemSetSetId", "ItemSetSetId"));
        public Int32 ItemSetSetId
        {
            get
            {
                return GetProperty(ItemSetSetIdProperty);
            }
            set
            {
                SetProperty(ItemSetSetIdProperty, value);
            }
        }

        public static readonly PropertyInfo<Int32> ItemSetContactIdProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("ItemSetContactId", "ItemSetContactId"));
        public Int32 ItemSetContactId
        {
            get
            {
                return GetProperty(ItemSetContactIdProperty);
            }
            set
            {
                SetProperty(ItemSetContactIdProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ItemSetContactNameProperty = RegisterProperty<string>(new PropertyInfo<string>("ItemSetContactName", "ItemSetContactName"));
        public string ItemSetContactName
        {
            get
            {
                return GetProperty(ItemSetContactNameProperty);
            }
            set
            {
                SetProperty(ItemSetContactNameProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> ItemSetIsCompletedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ItemSetIsCompleted", "ItemSetIsCompleted"));
        public bool ItemSetIsCompleted
        {
            get
            {
                return GetProperty(ItemSetIsCompletedProperty);
            }
            set
            {
                SetProperty(ItemSetIsCompletedProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> ItemSetIsLockedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("ItemSetIsLocked", "ItemSetIsLocked"));
        public bool ItemSetIsLocked
        {
            get
            {
#if SILVERLIGHT
                if (this.SetTypeId == 5 && this.IsInArmContext) return true;
#endif
                return GetProperty(ItemSetIsLockedProperty);
            }
            set
            {
                SetProperty(ItemSetIsLockedProperty, value);
                foreach (AttributeSet attributeSet in this.Attributes)
                {
                    attributeSet.IsLocked = value;
                }
            }
        }
        //new properties to account for set_types



        //end of set_type properties
        
        public void AddNewChildAttribute()
        {
            AttributeSet newAttribute = AttributeSet.NewAttributeSet();
            newAttribute.AttributeId = 0;
            newAttribute.AttributeName = "Edit me";
            newAttribute.AttributeTypeId = 1;
            BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            newAttribute.ContactId = ri.UserId;
            newAttribute.ParentAttributeId = 0;
            newAttribute.AttributeOrder = this.Attributes.Count + 1;
            newAttribute.SetId = this.SetId;
            newAttribute.Saved += (o, e2) =>
            {
                if (e2.NewObject != null)
                {
                    this.Attributes.Add(e2.NewObject as AttributeSet);
                    (e2.NewObject as AttributeSet).HostAttribute = null;
                }
            };
            newAttribute.BeginSave();
        }

        public void DeleteChildAttribute(AttributeSet attribute)
        {
            for (int i = this.Attributes.IndexOf(attribute) + 1; i < this.Attributes.Count; i++)
            {
                this.Attributes[i].SetIsNew();
                this.Attributes[i].AttributeOrder = this.Attributes[i].AttributeOrder - 1;
                this.Attributes[i].SetIsOld();
            }
            attribute.SetIsNew(); // so that it doesn't get saved here.
            attribute.Attributes.Clear();
            this.Attributes.Remove(attribute);

            attribute.Delete();
            attribute.BeginSave(true);
        }

        public void RemoveCodeSet()
        {
            //this.MarkNew(); // marking the code set as 'new' means that its removal doesn't get sent to the dataportal
            this.Delete();
            this.BeginSave();
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

        

        protected override void DataPortal_Insert()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_TYPE_ID", SetTypeId)); // All set to review specific keywords atm
                    command.Parameters.Add(new SqlParameter("@ALLOW_CODING_EDITS", ReadProperty(AllowCodingEditsProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_NAME", ReadProperty(SetNameProperty)));
                    command.Parameters.Add(new SqlParameter("@CODING_IS_FINAL", ReadProperty(CodingIsFinalProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ORDER", ReadProperty(SetOrderProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_DESCRIPTION", ReadProperty(SetDescriptionProperty)));
                    if (ReadProperty(OriginalSetIdProperty) != null && ReadProperty(OriginalSetIdProperty) != 0)
                    {
                        command.Parameters.Add(new SqlParameter("@ORIGINAL_SET_ID", ReadProperty(OriginalSetIdProperty)));
                    }

                    SqlParameter par = new SqlParameter("@NEW_REVIEW_SET_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par); 
                    command.Parameters["@NEW_REVIEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;

                    SqlParameter par2 = new SqlParameter("@NEW_SET_ID", System.Data.SqlDbType.Int);
                    par2.Value = 0;
                    command.Parameters.Add(par2); 
                    command.Parameters["@NEW_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ReviewSetIdProperty, command.Parameters["@NEW_REVIEW_SET_ID"].Value);
                    LoadProperty(SetIdProperty, command.Parameters["@NEW_SET_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", ReadProperty(ReviewSetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_NAME", ReadProperty(SetNameProperty)));
                    command.Parameters.Add(new SqlParameter("@ALLOW_CODING_EDITS", ReadProperty(AllowCodingEditsProperty)));
                    command.Parameters.Add(new SqlParameter("@CODING_IS_FINAL", ReadProperty(CodingIsFinalProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_DESCRIPTION", ReadProperty(SetDescriptionProperty)));

                    command.Parameters.Add(new SqlParameter("@ITEM_SET_ID", ReadProperty(ItemSetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@IS_COMPLETED", ReadProperty(ItemSetIsCompletedProperty)));
                    command.Parameters.Add(new SqlParameter("@IS_LOCKED", ReadProperty(ItemSetIsLockedProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSetDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", ReadProperty(ReviewSetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ReadProperty(ReviewIdProperty)));
                    command.Parameters.Add(new SqlParameter("@SET_ORDER", ReadProperty(SetOrderProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            /*
            if (this.Attributes.Count == 0)
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_ReviewSetDelete", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", ReadProperty(ReviewSetIdProperty)));
                        command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
             */
        }

        internal static ReviewSet GetReviewSet(SafeDataReader reader)
        {
            ReviewSet returnValue = new ReviewSet();
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
            
            //returnValue.LoadProperty<string>(SetTypeProperty, reader.GetString("SET_TYPE"));
            returnValue.TempMaxDepth = reader.GetInt32("MAX_DEPTH");
            returnValue.LoadProperty<bool>(ItemSetIsCompletedProperty, false);
            
           
            return returnValue;
        }

        internal static ReviewSet GetReviewSet(int RevID, int setTypeID, bool allowCodingEdits, string name, bool codingIsFinal, int setOrder, string setDescription, int originalSetID)
        {//used on server side to copy codesets (destination)
            ReviewSet returnValue = new ReviewSet();
            returnValue.Attributes = AttributeSetList.NewAttributeSetList();
            returnValue.LoadProperty<int>(OriginalSetIdProperty, originalSetID);
            returnValue.LoadProperty<int>(ReviewIdProperty, RevID);
            returnValue.LoadProperty<bool>(AllowCodingEditsProperty, allowCodingEdits);
            returnValue.LoadProperty<bool>(CodingIsFinalProperty, codingIsFinal);
            returnValue.LoadProperty<int>(SetTypeIdProperty, setTypeID);
            returnValue.LoadProperty<int>(SetOrderProperty, setOrder);
            returnValue.LoadProperty<string>(SetNameProperty, name);
            returnValue.LoadProperty<string>(SetDescriptionProperty, setDescription);
            returnValue.TempMaxDepth = 0;
            returnValue.LoadProperty<bool>(ItemSetIsCompletedProperty, false);
            return returnValue;
        }

        internal static ReviewSet GetReviewSet(int criteria)
        {//used on server side to copy codesets (source)
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            ReviewSet returnValue = new ReviewSet();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {//build the root
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewSet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    //problem!!! When taking a reviewSet as template, I currently don't know the source ReviewID!!!!!!!
                    //command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId)); // more secure using server stored object
                    command.Parameters.Add(new SqlParameter("@REVIEW_SET_ID", criteria));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
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
                            returnValue.TempMaxDepth = reader.GetInt32("MAX_DEPTH");
                            returnValue.LoadProperty<bool>(ItemSetIsCompletedProperty, false);
                            returnValue.MarkOld();
                        }
                    }
                }
            }
            ReviewSet.ReviewSetFromDBCommonPart(returnValue);
            return returnValue;
        }

        internal static void ReviewSetFromDBCommonPart(ReviewSet reviewSet)
        {

#if OLD_BUILDTREE
                            //compiler directive above is for testing purposes, so that we can re-activate old code by setting env. var. to OLD_BUILDTREE
                            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
                            {
                                connection2.Open();
                                using (SqlCommand command2 = new SqlCommand("st_AttributeSet", connection2))
                                {
                                    
                                    command2.CommandType = System.Data.CommandType.StoredProcedure;
                                    command2.Parameters.Add(new SqlParameter("@SET_ID", reviewSet.SetId));
                                    command2.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", 0));
                                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command2.ExecuteReader()))
                                    {
                                        while (reader2.Read())
                                        {
                                             
                                            AttributeSet newAttributeSet = AttributeSet.GetAttributeSet(reader2, reviewSet.TempMaxDepth);
                                            reviewSet.Attributes.Add(newAttributeSet);
                                        }
                                        reader2.Close();
                                    }
                                }
                                connection2.Close();
                            }
#else
            List<AttributeSet> flatList = new List<AttributeSet>();
            using (SqlConnection connection2 = new SqlConnection(DataConnection.ConnectionString))
            {

                connection2.Open();
                using (SqlCommand command2 = new SqlCommand("st_AllAttributesInSet", connection2))
                {

                    command2.CommandType = System.Data.CommandType.StoredProcedure;
                    command2.Parameters.Add(new SqlParameter("@SET_ID", reviewSet.SetId));
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
#endif
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
    public class SkipEmptyContractResolver : DefaultContractResolver
    {//this wierd object is used to avoid producing JSON elements for empty collections (i.e. the attributes field of an attribute/code that doesn't have codes)
        //see https://blog.hompus.nl/2015/12/04/json-on-a-diet-how-to-shrink-your-dtos-part-2-skip-empty-collections/ (adapted from!)
        public SkipEmptyContractResolver() : base() { }

        protected override JsonProperty CreateProperty(MemberInfo member,
                MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            bool isDefaultValueIgnored =
                ((property.DefaultValueHandling ?? DefaultValueHandling.Ignore)
                    & DefaultValueHandling.Ignore) != 0;
            if (isDefaultValueIgnored
                    && !typeof(string).IsAssignableFrom(property.PropertyType)
                    && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                Predicate<object> newShouldSerialize = obj => {
                    var collection = property.ValueProvider.GetValue(obj) as ICollection;
                    return collection == null || collection.Count != 0;
                };
                Predicate<object> oldShouldSerialize = property.ShouldSerialize;
                property.ShouldSerialize = oldShouldSerialize != null
                    ? o => oldShouldSerialize(o) && newShouldSerialize(o)
                    : newShouldSerialize;
            }
            return property;
        }
    }
}
