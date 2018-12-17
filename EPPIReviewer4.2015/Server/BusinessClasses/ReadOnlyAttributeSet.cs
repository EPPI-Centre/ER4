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
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlyAttributeSet : ReadOnlyBase<ReadOnlyAttributeSet>
    {
#if SILVERLIGHT
    public ReadOnlyAttributeSet()
    {
        
    }
#else
        private ReadOnlyAttributeSet()
        {
            //LoadProperty(ReadOnlyAttributeSetProperty, ReadOnlyAttributeSet.NewReadOnlyAttributeSet());
        }
#endif
        public override string ToString()
        {
            return AttributeName;
        }

        internal static ReadOnlyAttributeSet NewReadOnlyAttributeSet()
        {
            ReadOnlyAttributeSet returnValue = new ReadOnlyAttributeSet();
            return returnValue;
        }

        private static PropertyInfo<Int64> ReadOnlyAttributeSetIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ReadOnlyAttributeSetId", "Set Id"));
        public Int64 ReadOnlyAttributeSetId
        {
            get
            {
                return GetProperty(ReadOnlyAttributeSetIdProperty);
            }
        }

        private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(new PropertyInfo<int>("SetId", "Set Id", 0));
        public int SetId
        {
            get
            {
                return GetProperty(SetIdProperty);
            }
        }

        private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("AttributeId", "AttributeId"));
        public Int64 AttributeId
        {
            get
            {
                return GetProperty(AttributeIdProperty);
            }
        }

        private static PropertyInfo<Int64> ParentAttributeIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ParentAttributeId", "ParentAttributeId"));
        public Int64 ParentAttributeId
        {
            get
            {
                return GetProperty(ParentAttributeIdProperty);
            }
        }

        private static PropertyInfo<int> AttributeTypeIdProperty = RegisterProperty<int>(new PropertyInfo<int>("AttributeTypeId", "AttributeTypeId", 0));
        public int AttributeTypeId
        {
            get
            {
                return GetProperty(AttributeTypeIdProperty);
            }
        }

        private static PropertyInfo<string> ReadOnlyAttributeSetDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("SetDescription", "Set Description", string.Empty));
        public string ReadOnlyAttributeSetDescription
        {
            get
            {
                return GetProperty(ReadOnlyAttributeSetDescriptionProperty);
            }
        }

        private static PropertyInfo<int> AttributeOrderProperty = RegisterProperty<int>(new PropertyInfo<int>("AttributeOrder", "Attribute Order", 0));
        public int AttributeOrder
        {
            get
            {
                return GetProperty(AttributeOrderProperty);
            }
        }

        private static PropertyInfo<string> AttributeTypeProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeType", "Attribute Type", string.Empty));
        public string AttributeType
        {
            get
            {
                return GetProperty(AttributeTypeProperty);
            }
        }

        private static PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeName", "Attribute Name", string.Empty));
        public string AttributeName
        {
            get
            {
                return GetProperty(AttributeNameProperty);
            }
        }

        private static PropertyInfo<string> AttributeDescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("AttributeDescription", "Attribute Description", string.Empty));
        public string AttributeDescription
        {
            get
            {
                return GetProperty(AttributeDescriptionProperty);
            }
        }

        private static PropertyInfo<int> ContactIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ContactId", "Contact Id", 0));
        public int ContactId
        {
            get
            {
                return GetProperty(ContactIdProperty);
            }
        }


        private static PropertyInfo<ReadOnlyAttributeSetList> ReadOnlyAttributeSetProperty = RegisterProperty<ReadOnlyAttributeSetList>(new PropertyInfo<ReadOnlyAttributeSetList>("Attributes", "Attributes"));
        public ReadOnlyAttributeSetList Attributes
        {
            get
            {
                return GetProperty(ReadOnlyAttributeSetProperty);
            }
        }

        public void CreateAttributeSetList()
        {
            LoadProperty(ReadOnlyAttributeSetProperty, ReadOnlyAttributeSetList.NewReadOnlyAttributeSetList());
        }

        /*
        private static PropertyInfo<MobileList<ReadOnlyAttributeSet>> ReadOnlyAttributeSetProperty = RegisterProperty<MobileList<ReadOnlyAttributeSet>>(new PropertyInfo<MobileList<ReadOnlyAttributeSet>>("Attributes", "Attributes"));
        public MobileList<ReadOnlyAttributeSet> Attributes
        {
            get
            {
                return GetProperty(ReadOnlyAttributeSetProperty);
            }
        }
        */
        private static PropertyInfo<ReadOnlyAttributeSet> ParentAttributeProperty = RegisterProperty<ReadOnlyAttributeSet>(new PropertyInfo<ReadOnlyAttributeSet>("ParentAttribute", "Parent Attribute"));
        public ReadOnlyAttributeSet ParentAttribute
        {
            get
            {
                return GetProperty(ParentAttributeProperty);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ReadOnlyAttributeSet), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ReadOnlyAttributeSet), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ReadOnlyAttributeSet), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ReadOnlyAttributeSet), canRead);

        //    //AuthorizationRules.AllowRead(ReadOnlyAttributeSetIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(SetIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ParentAttributeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeTypeIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReadOnlyAttributeSetDescriptionProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeOrderProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeTypeProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeNameProperty, canRead);
        //    //AuthorizationRules.AllowRead(AttributeDescriptionProperty, canRead);
        //    //AuthorizationRules.AllowRead(ContactIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReadOnlyAttributeSetDescriptionProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReadOnlyAttributeSetProperty, canRead);
        //    //AuthorizationRules.AllowRead(ParentAttributeProperty, canRead);
        //}

        

#if !SILVERLIGHT

        internal static ReadOnlyAttributeSet GetReadOnlyAttributeSet(SafeDataReader reader)
        {
            ReadOnlyAttributeSet returnValue = new ReadOnlyAttributeSet();
            returnValue.CreateAttributeSetList();
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_AttributeSet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SET_ID", reader.GetInt32("SET_ID")));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", reader.GetInt64("ATTRIBUTE_ID")));
                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader2.Read())
                        {
                            ReadOnlyAttributeSet newReadOnlyAttributeSet = ReadOnlyAttributeSet.GetReadOnlyAttributeSet(reader2);
                            newReadOnlyAttributeSet.LoadProperty<ReadOnlyAttributeSet>(ParentAttributeProperty, returnValue);
                            // THIS ONE newReadOnlyAttributeSet.ParentAttribute = returnValue;
                            returnValue.Attributes.Add(newReadOnlyAttributeSet);
                        }
                        reader2.Close();
                    }
                }
                connection.Close();
            }
            returnValue.LoadProperty<Int64>(ReadOnlyAttributeSetIdProperty, reader.GetInt64("ATTRIBUTE_SET_ID"));
            returnValue.LoadProperty<int>(SetIdProperty, reader.GetInt32("SET_ID"));
            returnValue.LoadProperty<Int64>(AttributeIdProperty, reader.GetInt64("ATTRIBUTE_ID"));
            returnValue.LoadProperty<Int64>(ParentAttributeIdProperty, reader.GetInt64("PARENT_ATTRIBUTE_ID"));
            returnValue.LoadProperty<int>(AttributeTypeIdProperty, reader.GetInt32("ATTRIBUTE_TYPE_ID"));
            returnValue.LoadProperty<string>(ReadOnlyAttributeSetDescriptionProperty, reader.GetString("ATTRIBUTE_SET_DESC"));
            returnValue.LoadProperty<int>(AttributeOrderProperty, reader.GetInt32("ATTRIBUTE_ORDER"));
            returnValue.LoadProperty<string>(AttributeTypeProperty, reader.GetString("ATTRIBUTE_TYPE"));
            returnValue.LoadProperty<string>(AttributeNameProperty, reader.GetString("ATTRIBUTE_NAME"));
            returnValue.LoadProperty<string>(AttributeDescriptionProperty, reader.GetString("ATTRIBUTE_SET_DESC"));
            returnValue.LoadProperty<int>(ContactIdProperty, reader.GetInt32("CONTACT_ID"));

            return returnValue;
        }

#endif
    }
}
