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
    public class ItemLink : BusinessBase<ItemLink>
    {
#if SILVERLIGHT
    public ItemLink() { }

        
#else
        private ItemLink() { }
#endif

        public override string ToString()
        {
            return ShortTitle;
        }

        private static PropertyInfo<int> ItemLinkIdProperty = RegisterProperty<int>(new PropertyInfo<int>("ItemLinkId", "ItemLinkId"));
        public int ItemLinkId
        {
            get
            {
                return GetProperty(ItemLinkIdProperty);
            }
        }

        private static PropertyInfo<Int64> ItemIdPrimaryProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemIdPrimary", "ItemIdPrimary"));
        public Int64 ItemIdPrimary
        {
            get
            {
                return GetProperty(ItemIdPrimaryProperty);
            }
            set
            {
                SetProperty(ItemIdPrimaryProperty, value);
            }
        }

        private static PropertyInfo<Int64> ItemIdSecondaryProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemIdSecondary", "ItemIdSecondary"));
        public Int64 ItemIdSecondary
        {
            get
            {
                return GetProperty(ItemIdSecondaryProperty);
            }
            set
            {
                SetProperty(ItemIdSecondaryProperty, value);
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

        private static PropertyInfo<string> ShortTitleProperty = RegisterProperty<string>(new PropertyInfo<string>("ShortTitle", "ShortTitle", string.Empty));
        public string ShortTitle
        {
            get
            {
                return GetProperty(ShortTitleProperty);
            }
            set
            {
                SetProperty(ShortTitleProperty, value);
            }
        }

        private static PropertyInfo<string> DescriptionProperty = RegisterProperty<string>(new PropertyInfo<string>("Description", "Description", string.Empty));
        public string Description
        {
            get
            {
                return GetProperty(DescriptionProperty);
            }
            set
            {
                SetProperty(DescriptionProperty, value);
            }
        }

        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(ItemLink), admin);
        //    //AuthorizationRules.AllowDelete(typeof(ItemLink), admin);
        //    //AuthorizationRules.AllowEdit(typeof(ItemLink), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(ItemLink), canRead);

        //    //AuthorizationRules.AllowRead(ItemLinkIdProperty, canRead);
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
                using (SqlCommand command = new SqlCommand("st_ItemLinkInsert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_PRIMARY", ReadProperty(ItemIdPrimaryProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_ID_SECONDARY", ReadProperty(ItemIdSecondaryProperty)));
                    command.Parameters.Add(new SqlParameter("@LINK_DESCRIPTION", ReadProperty(DescriptionProperty)));
                    SqlParameter par = new SqlParameter("@NEW_ITEM_LINK_ID", System.Data.SqlDbType.Int);
                    par.Value = 0;
                    command.Parameters.Add(par);
                    command.Parameters["@NEW_ITEM_LINK_ID"].Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    LoadProperty(ItemLinkIdProperty, command.Parameters["@NEW_ITEM_LINK_ID"].Value);
                }
                connection.Close();
            }
        }

        protected override void DataPortal_Update()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemLinkUpdate", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_LINK_ID", ReadProperty(ItemLinkIdProperty)));
                    command.Parameters.Add(new SqlParameter("@ITEM_LINK_SECONDARY", ReadProperty(ItemIdSecondaryProperty)));
                    command.Parameters.Add(new SqlParameter("@LINK_DESCRIPTION", ReadProperty(DescriptionProperty)));
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
                using (SqlCommand command = new SqlCommand("st_ItemLinkDelete", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ITEM_LINK_ID", ReadProperty(ItemLinkIdProperty)));
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        internal static ItemLink GetItemLink(SafeDataReader reader)
        {
            ItemLink returnValue = new ItemLink();
            returnValue.LoadProperty<int>(ItemLinkIdProperty, reader.GetInt32("ITEM_LINK_ID"));
            returnValue.LoadProperty<Int64>(ItemIdPrimaryProperty, reader.GetInt64("ITEM_ID_PRIMARY"));
            returnValue.LoadProperty<Int64>(ItemIdSecondaryProperty, reader.GetInt64("ITEM_ID_SECONDARY"));
            returnValue.LoadProperty<string>(TitleProperty, reader.GetString("TITLE"));
            returnValue.LoadProperty<string>(ShortTitleProperty, reader.GetString("SHORT_TITLE"));
            returnValue.LoadProperty<string>(DescriptionProperty, reader.GetString("LINK_DESCRIPTION"));
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
