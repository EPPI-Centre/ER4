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
using Csla.DataPortalClient;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ItemArm : BusinessBase<ItemArm>
    {
#if SILVERLIGHT
    public ItemArm() { }

        
#else
        private ItemArm() { }
#endif

        public override string ToString()
        {
            return Title;
        }

        private static PropertyInfo<Int64> ItemArmIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemArmId", "ItemArmId"));
        public Int64 ItemArmId
        {
            get
            {
                return GetProperty(ItemArmIdProperty);
            }
        }

        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
            set
            {
                SetProperty(ItemIdProperty, value);
            }
        }

        private static PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        public string Title
        {
            get
            {
                return GetProperty(TitleProperty);
            }
            set
            {
                SetProperty(TitleProperty, value);
            }
        }

        private static PropertyInfo<int> OrderingProperty = RegisterProperty<int>(new PropertyInfo<int>("Ordering", "Ordering", 0));
        public int Ordering
        {
            get
            {
                return GetProperty(OrderingProperty);
            }
            set
            {
                SetProperty(OrderingProperty, value);
            }
        }


        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ItemArm), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ItemArm), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ItemArm), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ItemArm), canRead);

        //    //AuthorizationRules.AllowRead(ItemArmIdProperty, canRead);
        //    //AuthorizationRules.AllowRead(ReviewIdProperty, canRead);

        //    ////AuthorizationRules.AllowWrite(NameProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }

#if !SILVERLIGHT

        protected override void DataPortal_Insert()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemArmCreate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID", ReadProperty(ItemIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ARM_NAME", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@ORDERING", ReadProperty(OrderingProperty)));
                    command.Parameters.Add(new SqlParameter("@NEW_ITEM_ARM_ID", 0));
                    command.Parameters["@NEW_ITEM_ARM_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ItemArmIdProperty, command.Parameters["@NEW_ITEM_ARM_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemArmUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", ReadProperty(ItemArmIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ARM_NAME", ReadProperty(TitleProperty)));
                    command.Parameters.Add(new SqlParameter("@ORDERING", ReadProperty(OrderingProperty)));
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
                using (SqlCommand command = new SqlCommand("st_ItemArmDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ARM_ID", ReadProperty(ItemArmIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static ItemArm GetItemArm(SafeDataReader reader)
        {
            ItemArm returnValue = new ItemArm();
            returnValue.LoadProperty<Int64>(ItemArmIdProperty, reader.GetInt64("ITEM_ARM_ID"));
            returnValue.LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("ARM_NAME"));
            returnValue.LoadProperty<int>(OrderingProperty, reader.GetInt32("ORDERING"));

            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
