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
using Csla.DataPortalClient;
using System.Threading;


#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReviewStatisticsCountsCommand : CommandBase<ReviewStatisticsCountsCommand>
    {
        public ReviewStatisticsCountsCommand() { }
        
        private int _itemsIncluded;
        private int _itemsExcluded;
        private int _itemsDeleted;
        private int _DuplicateItems;

        public int ItemsIncluded
        {
            get { return _itemsIncluded; }
        }

        public int ItemsExcluded
        {
            get { return _itemsExcluded; }
        }

        public int ItemsDeleted
        {
            get { return _itemsDeleted; }
        }
        public int DuplicateItems
        {
            get { return _DuplicateItems; }
        }
        protected override void OnGetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            base.OnGetState(info, mode);
            info.AddValue("_itemsIncluded", _itemsIncluded);
            info.AddValue("_itemsExcluded", _itemsExcluded);
            info.AddValue("_itemsDeleted", _itemsDeleted);
            info.AddValue("_DuplicateItems", _DuplicateItems);
        }
        protected override void OnSetState(Csla.Serialization.Mobile.SerializationInfo info, Csla.Core.StateMode mode)
        {
            _itemsIncluded = info.GetValue<int>("_itemsIncluded");
            _itemsExcluded = info.GetValue<int>("_itemsExcluded");
            _itemsDeleted = info.GetValue<int>("_itemsDeleted");
            _DuplicateItems = info.GetValue<int>("_DuplicateItems");
        }


#if !SILVERLIGHT

        protected override void DataPortal_Execute()
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ReviewStatisticsCounts", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            _itemsIncluded = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _itemsExcluded = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _itemsDeleted = Convert.ToInt32(reader[0].ToString());
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            _DuplicateItems = Convert.ToInt32(reader[0].ToString());
                        }
                    }
                }
                connection.Close();
            }
        }

#endif
    }
}
