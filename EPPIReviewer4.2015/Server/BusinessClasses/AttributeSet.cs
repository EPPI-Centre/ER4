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
using System.ComponentModel;
using Csla.Rules.CommonRules;
using Csla.Rules;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using Csla.Data;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class AttributeSet : BusinessBase<AttributeSet>
    {

#if SILVERLIGHT
    public AttributeSet()
    { 
        Attributes = AttributeSetList.NewAttributeSetList();  // want the attributeSetList created without coming back to the server
    }
        public System.Windows.Media.Brush ForeGround
        {
            get
            {
                switch (AttributeTypeId)
                {
                     case 11:
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)235, (byte)90, (byte)40));
                    //case 5:
                    //    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)0, (byte)188, (byte)0));
                    default:
                        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                }
            }
        }
#else
        private AttributeSet()
        {
            //Attributes = AttributeSetList.NewAttributeSetList();  // want the attributeSetList created without coming back to the server
        }
#endif
        public override string ToString()
        {
            return AttributeName;
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
        public int AttributeTreeLevel //starts from 1!
        {
            get
            {
                int res = 1;
                if (this.ParentAttributeId == 0 || this.HostAttribute == null) return res;
                else
                {
                    res++;
                    res = this.HostAttribute.AttributeTreeLevelRec(res);
                }
                return res;
            }
        }
        public bool CanHaveChildren
        {
            get
            {
                if (MaxDepth != 0 && this.AttributeTreeLevel >= MaxDepth) return false;
                return true;
            }
        }
        public int ChildrenDepth//returns how many levels the contained codes tree includes, so the full depth would be ChildrenDepth + 1
        {
            get
            {
                if (Attributes == null || Attributes.Count == 0) return 0;
                int  max = 1, tmp = 0;
                foreach (AttributeSet c in Attributes)
                {
                    tmp = c.ChildrenDepthRec(1);
                    max = max < tmp ? tmp : max;
                }
                return max;
            }
        }
        public int ChildrenDepthRec(int res)
        {
            if (Attributes == null || Attributes.Count == 0) return res;
            int max = 1, tmp = 0;
            foreach (AttributeSet c in Attributes)
            {
                tmp = c.ChildrenDepthRec(res + 1);
                max = max < tmp ? tmp : max;
            }
            return max;
        }
        public int AttributeTreeLevelRec(int current)
        {
            
            if (this.ParentAttributeId == 0 || this.HostAttribute == null) return current;
            else
            {
                current++;
                current = this.HostAttribute.AttributeTreeLevelRec(current);
            }
            return current;
            
        }
        // for controlling the appearance of the tree control
        private static PropertyInfo<string> CurrentHotKeyTextProperty = RegisterProperty<string>(new PropertyInfo<string>("CurrentHotKeyText", "CurrentHotKeyText", ""));
        public string CurrentHotKeyText
        {
            get
            {
                return GetProperty(CurrentHotKeyTextProperty);
            }
            set
            {
                SetProperty(CurrentHotKeyTextProperty, value);
            }
        }

        // for controlling the appearance of the tree control
        private static PropertyInfo<bool> DisplayIsParentProperty = RegisterProperty<bool>(new PropertyInfo<bool>("DisplayIsParent", "DisplayIsParent", false));
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

        public AttributeSet GetAttributeSetFromAttributeId(Int64 attributeId)
        {
            AttributeSet returnValue = null;
            foreach (AttributeSet rs in Attributes)
            {
                if (rs.AttributeId == attributeId)
                {
                    return rs;
                }
                returnValue = rs.GetAttributeSetFromAttributeId(attributeId);
                if (returnValue != null)
                {
                    return returnValue;
                }
            }
            return returnValue;
        }

        public void ClearItemData()
        {
            this.ItemData = null;
            this.IsSelected = false;
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

        internal static AttributeSet NewAttributeSet()
        {
            AttributeSet returnValue = new AttributeSet();
            //returnValue.ValidationRules.CheckRules();
            return returnValue;
        }

        internal static AttributeSet NewAttributeSet(AttributeSet hostAttribute)
        {
            AttributeSet returnValue = new AttributeSet();
            BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            returnValue.SetId = hostAttribute.SetId;
            returnValue.HostAttribute = hostAttribute;
            returnValue.AttributeName = "New code";
            returnValue.AttributeOrder = hostAttribute.Attributes.Count + 1;
            returnValue.ContactId = ri.UserId;
            returnValue.AttributeTypeId = 1;
            //returnValue.ValidationRules.CheckRules();
            return returnValue;
        }

        public void SetIsNew()
        {
            this.MarkNew();
        }

        public void SetIsOld()
        {
            this.MarkOld();
        }

        public void SetIsClean()
        {
            this.MarkClean();
        }

        private static PropertyInfo<Int64> AttributeSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeSetId", "Set Id"));
        public Int64 AttributeSetId
        {
            get
            {
                return GetProperty(AttributeSetIdProperty);
            }
        }

        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "Set Id", 0));
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

        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
            set
            {
                SetProperty(AttributeIdProperty, value);
            }
        }

        private static PropertyInfo<Int64> ParentAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ParentAttributeId", "ParentAttributeId"));
        public Int64 ParentAttributeId
        {
            get
            {
                return GetProperty(ParentAttributeIdProperty);
            }
            set
            {
                SetProperty(ParentAttributeIdProperty, value);
            }
        }

        private static PropertyInfo<int> AttributeTypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("AttributeTypeId", "AttributeTypeId", 0));
        public int AttributeTypeId
        {
            get
            {
                return GetProperty(AttributeTypeIdProperty);
            }
            set
            {
                SetProperty(AttributeTypeIdProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeSetDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("SetDescription", "Set Description", string.Empty));
        public string AttributeSetDescription
        {
            get
            {
                return GetProperty(AttributeSetDescriptionProperty);
            }
            set
            {
                SetProperty(AttributeSetDescriptionProperty, value);
            }
        }

        private static PropertyInfo<int> AttributeOrderProperty = RegisterProperty<int>(new PropertyInfo<int>("AttributeOrder", "Attribute Order", 0));
        public int AttributeOrder
        {
            get
            {
                return GetProperty(AttributeOrderProperty);
            }
            set
            {
                SetProperty(AttributeOrderProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeType", "Attribute Type", string.Empty));
        public string AttributeType
        {
            get
            {
                return GetProperty(AttributeTypeProperty);
            }
            set
            {
                SetProperty(AttributeTypeProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "Attribute Name", string.Empty));
        public string AttributeName
        {
            get
            {
                return GetProperty(AttributeNameProperty);
            }
            set
            {
                SetProperty(AttributeNameProperty, value);
            }
        }

        private static PropertyInfo<string> AttributeDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeDescription", "Attribute Description", string.Empty));
        public string AttributeDescription
        {
            get
            {
                return GetProperty(AttributeDescriptionProperty);
            }
            set
            {
                SetProperty(AttributeDescriptionProperty, value);
            }
        }

        private static PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "Contact Id", 0));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
            set
            {
                SetProperty(ContactIdProperty, value);
            }
        }
        private static PropertyInfo<int> MaxDepthProperty = RegisterProperty<int>(new PropertyInfo<int>("MaxDepth", "MaxDepth", 0));
        public int MaxDepth
        {
            get
            {
                return GetProperty(MaxDepthProperty);
            }
            set
            {
                SetProperty(MaxDepthProperty, value);
            }
        }
        /* values for the item that is currently being data extracted / screened.
         * Not a PropertyInfo as we don't want the AttributeSet object to handle changes. */

        [NotUndoable]
        public ItemAttributeData ItemData;

        private static PropertyInfo<bool> IsSelectedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsSelected", "IsSelected", false));
        public bool IsSelected
        {
            get
            {
                return GetProperty(IsSelectedProperty);
            }
            set
            {
                SetProperty(IsSelectedProperty, value);
            }
        }

        private static PropertyInfo<bool> IsLockedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsLocked", "IsLocked", false));
        public bool IsLocked
        {
            get
            {
                return GetProperty(IsLockedProperty)|| 
                    !Csla.Rules.BusinessRules.HasPermission( AuthorizationActions.EditObject, this); 
                //| !Csla.Security.AuthorizationRules.CanEditObject(this.GetType()); 
            }
            set
            {
                SetProperty(IsLockedProperty, value);
                foreach (AttributeSet attributeSet in this.Attributes)
                {
                    attributeSet.IsLocked = value;
                }
            }
        }

        /* ***************** end item info ******************/

        [NotUndoable]
        private static PropertyInfo<AttributeSetList> AttributeSetProperty = RegisterProperty<AttributeSetList>(new PropertyInfo<AttributeSetList>("Attributes", "Attributes"));
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

        public void AddNewChildAttribute()
        {
            BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            AttributeSet newAttribute = new AttributeSet();
            newAttribute.AttributeId = 0;
            newAttribute.AttributeName = "Edit me";
            newAttribute.AttributeTypeId = 1;
            newAttribute.ContactId = ri.UserId;
            newAttribute.ParentAttributeId = this.AttributeId;
            newAttribute.AttributeOrder = this.Attributes.Count;
            newAttribute.SetId = this.SetId;
            newAttribute.Saved += (o, e2) =>
            {
                if (e2.NewObject != null)
                {
                    this.Attributes.Add(e2.NewObject as AttributeSet);
                    (e2.NewObject as AttributeSet).HostAttribute = this;
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
            attribute.MarkNew(); // so that it doesn't get saved here.
            attribute.Attributes.Clear();
            this.Attributes.Remove(attribute);

            attribute.Delete();
            attribute.BeginSave(true);
        }

        public void AddAttribute(AttributeSet attribute, int position) // adds to the attributes list (not necessarily a new one - from drag/drop)
        {
            attribute.HostAttribute = this;
            attribute.ParentAttributeId = this.AttributeId;
            attribute.MarkClean();
            //this.Attributes.Add(attribute);
            for (int i = position; i < this.Attributes.Count; i++)
            {
                this.Attributes[i].SetIsNew();
                this.Attributes[i].AttributeOrder = this.Attributes[i].AttributeOrder + 1;
                this.Attributes[i].SetIsOld();
            }
            this.Attributes.Insert(position, attribute);
        }

        public void RemoveAttribute(AttributeSet attribute, AttributeSetList list)
        {
            attribute.MarkNew(); // marking the attribute as 'new' means that its removal doesn't get sent to the dataportal
            attribute.HostAttribute = null;
            list.Remove(attribute);
            attribute.MarkOld();
        }

        public void RemoveRootAttribute(AttributeSet attribute, AttributeSetList attributeList)
        {
            attribute.MarkNew(); // marking the attribute as 'new' means that its removal doesn't get sent to the dataportal
            attribute.HostAttribute = null;
            attributeList.Remove(attribute);
            attribute.MarkOld();
        }

        [NotUndoable]
        private static PropertyInfo<AttributeSet> HostAttributeProperty = RegisterProperty<AttributeSet>(new PropertyInfo<AttributeSet>("HostAttribute", "Host Attribute"));
        public AttributeSet HostAttribute
        {
            get
            {
                return GetProperty(HostAttributeProperty);
            }
            set
            {
                SetProperty(HostAttributeProperty, value);
                if (value != null)
                    SetProperty(MaxDepthProperty, value.MaxDepth);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    string[] denyEditSave = new string[] { "ReadOnlyUser" };
        //    AuthorizationRules.DenyEdit(typeof(AttributeSet), denyEditSave);
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(AttributeSet), admin);
        //    //AuthorizationRules.AllowDelete(typeof(AttributeSet), admin);
        //    //AuthorizationRules.AllowEdit(typeof(AttributeSet), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(AttributeSet), canRead);
            
        //    //AuthorizationRules.AllowRead(AttributeSetIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ParentAttributeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeTypeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeSetDescriptionProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeOrderProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeTypeProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeDescriptionProperty, canRead);
        //    //AuthorizationRules.AllowRead(ContactIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeSetDescriptionProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeSetProperty, canRead);
        //    //AuthorizationRules.AllowRead(HostAttributeProperty, canRead);

        //    //AuthorizationRules.AllowWrite(AttributeSetIdProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(SetIdProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeIdProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(ParentAttributeIdProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeTypeIdProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeSetDescriptionProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeOrderProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeTypeProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeNameProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeDescriptionProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(ContactIdProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeSetDescriptionProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeSetProperty, canWrite);
        //    //AuthorizationRules.AllowWrite(AttributeSetProperty, canRead);
        //    //AuthorizationRules.AllowWrite(HostAttributeProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            BusinessRules.AddRule(new IsNotInRole(AuthorizationActions.EditObject, "ReadOnlyUser"));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT
        public Int64? OriginalAttributeID;//used when copying codesets
        protected override void DataPortal_Insert()
        {
            AddNew();
        }
        
        private void AddNew()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSetInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", ReadProperty(ParentAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", ReadProperty(AttributeTypeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_DESC", ReadProperty(AttributeSetDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", ReadProperty(AttributeOrderProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_NAME", ReadProperty(AttributeNameProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_DESC", ReadProperty(AttributeDescriptionProperty)));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));
                    if (OriginalAttributeID != null && OriginalAttributeID != 0)
                    {
                        command.Parameters.Add(new SqlParameter("@ORIGINAL_ATTRIBUTE_ID", OriginalAttributeID));
                    }
                    command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_SET_ID", 0));
                    command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@NEW_ATTRIBUTE_ID", 0));
                    command.Parameters["@NEW_ATTRIBUTE_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(AttributeSetIdProperty, command.Parameters["@NEW_ATTRIBUTE_SET_ID"].Value);
                    LoadProperty(AttributeIdProperty, command.Parameters["@NEW_ATTRIBUTE_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            if (this.AttributeId == 0)
            {
                AddNew();
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("st_AttributeSetUpdate", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID", ReadProperty(AttributeSetIdProperty)));
                        command.Parameters.Add(new SqlParameter("@SET_ID", ReadProperty(SetIdProperty)));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                        command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", ReadProperty(ParentAttributeIdProperty)));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_TYPE_ID", ReadProperty(AttributeTypeIdProperty)));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_DESC", ReadProperty(AttributeSetDescriptionProperty)));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", ReadProperty(AttributeOrderProperty)));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_NAME", ReadProperty(AttributeNameProperty)));
                        command.Parameters.Add(new SqlParameter("@ATTRIBUTE_DESC", ReadProperty(AttributeDescriptionProperty)));
                        command.Parameters.Add(new SqlParameter("@CONTACT_ID", ReadProperty(ContactIdProperty)));
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
        }

        protected override void DataPortal_DeleteSelf()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSetDelete", connection))
                {
                    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_SET_ID", ReadProperty(AttributeSetIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", ReadProperty(AttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", ReadProperty(ParentAttributeIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ORDER", ReadProperty(AttributeOrderProperty)));
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    //command.ExecuteNonQuery(); <-- carried out in a command object
                }
                connection.Close();
            }
        }

        internal static AttributeSet GetAttributeSet(SafeDataReader reader, int maxD)
        {
            AttributeSet returnValue = new AttributeSet();
            returnValue.LoadProperty<int>(MaxDepthProperty, maxD);//setting this here because it needs to be correct when adding a ref to returnValue inside it's children
            returnValue.Attributes = AttributeSetList.NewAttributeSetList();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SET_ID", reader.GetInt32("SET_ID")));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", reader.GetInt64("ATTRIBUTE_ID")));
                    returnValue.Attributes.RaiseListChangedEvents = false;
                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader2.Read())
                        {
                            AttributeSet newAttributeSet = AttributeSet.GetAttributeSet(reader2, maxD);
                            newAttributeSet.HostAttribute = returnValue;
                            returnValue.Attributes.Add(newAttributeSet);
                        }
                        reader2.Close();
                    }
                    returnValue.Attributes.RaiseListChangedEvents = true;
                }
                connection.Close();
            }
            returnValue.LoadProperty<Int64>(AttributeSetIdProperty, reader.GetInt64("ATTRIBUTE_SET_ID"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<Int64>(ParentAttributeIdProperty, reader.GetInt64("PARENT_ATTRIBUTE_ID"));
            returnValue.LoadProperty<int>(AttributeTypeIdProperty, reader.GetInt32("ATTRIBUTE_TYPE_ID"));
            returnValue.LoadProperty<string>(AttributeSetDescriptionProperty, reader.GetString("ATTRIBUTE_SET_DESC"));
            returnValue.LoadProperty<int>(AttributeOrderProperty, reader.GetInt32("ATTRIBUTE_ORDER"));
            returnValue.LoadProperty<string>(AttributeTypeProperty, reader.GetString("ATTRIBUTE_TYPE"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.LoadProperty<string>(AttributeDescriptionProperty, reader.GetString("ATTRIBUTE_DESC"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));
            
            returnValue.MarkOld();
            return returnValue;
        }

#endif
    }

    [Serializable]
    public class ItemAttributeData
    {
        public Int64 ItemAttributeId { get; set; }
        public Int64 ItemId { get; set; }
        public Int64 ItemSetId { get; set; }
        public Int64 AttributeId { get; set; }
        public string AdditionalText { get; set; }
        public int SetId { get; set; }
        public bool IsLocked { get; set; }
        public bool IsCompleted { get; set; }

        public Int64 AttributeSetId { get; set; }
        public ItemAttributeTextList ItemAttributeTextList { get; set; }

        /*
        private bool _IsSelected = true;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        */
    }
}
