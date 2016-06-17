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
    public class TrainingNextItem : BusinessBase<TrainingNextItem>
    {
#if SILVERLIGHT
    public TrainingNextItem() { }

        
#else
        private TrainingNextItem() { }
#endif

        private static PropertyInfo<Int64> ItemIdProperty = RegisterProperty<Int64>(new PropertyInfo<Int64>("ItemId", "ItemId"));
        public Int64 ItemId
        {
            get
            {
                return GetProperty(ItemIdProperty);
            }
        }

        private static PropertyInfo<Item> ItemProperty = RegisterProperty<Item>(new PropertyInfo<Item>("Item", "Item"));
        public Item Item
        {
            get
            {
                return GetProperty(ItemProperty);
            }
            set
            {
                SetProperty(ItemProperty, value);
            }
        }

        private static PropertyInfo<int> TrainingItemIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TrainingItemId", "TrainingItemId"));
        public int TrainingItemId
        {
            get
            {
                return GetProperty(TrainingItemIdProperty);
            }
        }

        private static PropertyInfo<int> TrainingIdProperty = RegisterProperty<int>(new PropertyInfo<int>("TrainingId", "TrainingId"));
        public int TrainingId
        {
            get
            {
                return GetProperty(TrainingIdProperty);
            }
        }

        private static PropertyInfo<int> RankProperty = RegisterProperty<int>(new PropertyInfo<int>("Rank", "Rank"));
        public int Rank
        {
            get
            {
                return GetProperty(RankProperty);
            }
        }

        
        
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser" };
        //    //AuthorizationRules.AllowCreate(typeof(TrainingNextItem), admin);
        //    //AuthorizationRules.AllowDelete(typeof(TrainingNextItem), admin);
        //    //AuthorizationRules.AllowEdit(typeof(TrainingNextItem), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(TrainingNextItem), canRead);

        //    //AuthorizationRules.AllowRead(TrainingNextItemIdProperty, canRead);
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

        }

        protected override void DataPortal_Update()
        {

        }

        protected override void DataPortal_DeleteSelf()
        {

        }

        protected void DataPortal_Fetch(SingleCriteria<TrainingNextItem, int> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_TrainingNextItem", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@CONTACT_ID", ri.UserId));
                    command.Parameters.Add(new SqlParameter("@TRAINING_CODE_SET_ID", criteria.Value));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<int>(TrainingItemIdProperty, reader.GetInt32("TRAINING_ITEM_ID"));
                            LoadProperty<Int64>(ItemIdProperty, reader.GetInt64("ITEM_ID"));
                            LoadProperty<int>(RankProperty, reader.GetInt32("RANK"));
                            LoadProperty<int>(TrainingIdProperty, reader.GetInt32("TRAINING_ID"));
                        }
                    }
                }
                using (SqlCommand command2 = new SqlCommand("st_Item", connection))
                {
                    command2.CommandType = System.Data.CommandType.StoredProcedure;
                    command2.Parameters.Add(new SqlParameter("@ITEM_ID", this.ItemId));
                    command2.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader2 = new Csla.Data.SafeDataReader(command2.ExecuteReader()))
                    {
                        if (reader2.Read())
                        {
                            this.Item = Item.GetItem(reader2);
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static TrainingNextItem GetTrainingNextItem(SafeDataReader reader)
        {
            TrainingNextItem returnValue = new TrainingNextItem();
            //returnValue.LoadProperty<int>(TrainingNextItemIdProperty, reader.GetInt32("TrainingNextItem_ID"));
            
            returnValue.MarkOld();
            return returnValue;
        }

#endif

    }
}
