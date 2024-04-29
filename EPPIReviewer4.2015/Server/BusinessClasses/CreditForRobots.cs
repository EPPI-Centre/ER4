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
    public class CreditForRobots : ReadOnlyBase<CreditForRobots>
    {
        public CreditForRobots() { }

        public static readonly PropertyInfo<int> CreditPurchaseIdProperty = RegisterProperty<int>(new PropertyInfo<int>("CreditPurchaseId", "CreditPurchaseId"));
        public int CreditPurchaseId
        {
            get
            {
                return GetProperty(CreditPurchaseIdProperty);
            }
        }
        

        public static readonly PropertyInfo<int> CreditPurchaserIdProperty = RegisterProperty<int>(new PropertyInfo<int>("CreditPurchaserId", "CreditPurchaserId"));
        public int CreditPurchaserId
        {
            get
            {
                return GetProperty(CreditPurchaserIdProperty);
            }
        }

        public static readonly PropertyInfo<int> AmountPurchasedProperty = RegisterProperty<int>(new PropertyInfo<int>("AmountPurchased", "AmountPurchased"));
        public int AmountPurchased
        {
            get
            {
                return GetProperty(AmountPurchasedProperty);
            }
        }

        public static readonly PropertyInfo<double> AmountRemainingProperty = RegisterProperty<double>(new PropertyInfo<double>("AmountRemaining", "AmountRemaining"));
        public double AmountRemaining
        {
            get
            {
                return GetProperty(AmountRemainingProperty);
            }
        }

#if !SILVERLIGHT


        private void Child_Fetch(int IdToFetch)
        {//tv_credit_purchase_id int, tv_credit_purchaser_id int, tv_credit_purchased int,        tv_credit_remaining int

            using (SqlConnection connection = new SqlConnection(DataConnection.AdmConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_CreditPurchaseGet", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CREDIT_PURCHASE_ID", IdToFetch));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<int>(CreditPurchaseIdProperty, reader.GetInt32("tv_credit_purchase_id"));
                            LoadProperty<int>(CreditPurchaserIdProperty, reader.GetInt32("tv_credit_purchaser_id"));
                            LoadProperty<int>(AmountPurchasedProperty, reader.GetInt32("tv_credit_purchased"));
                            LoadProperty<double>(AmountRemainingProperty, reader.GetDouble("tv_credit_remaining"));
                        }
                    }
                }
            }
        }

#endif

    }
}
