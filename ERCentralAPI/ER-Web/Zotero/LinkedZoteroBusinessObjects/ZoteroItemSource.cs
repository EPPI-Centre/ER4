using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using Csla.DataPortalClient;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

//using Csla.Configuration;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Linq;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ZoteroItemSource : BusinessBase<ZoteroItemSource>
    {
        public static void GetZoteroItemSources(EventHandler<DataPortalResult<ZoteroItemSource>> handler)
        {
            DataPortal<ZoteroItemSource> dp = new DataPortal<ZoteroItemSource>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
    public ZoteroItemSource() { }

        
#else
        public ZoteroItemSource() { }
#endif       

        public static readonly PropertyInfo<long> ITEM_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("ITEM_ID", "ITEM_ID", 0m));
        public long ITEM_ID
        {
            get
            {
                return GetProperty(ITEM_IDProperty);
            }
            set
            {
                SetProperty(ITEM_IDProperty, value);
            }
        }

        public static readonly PropertyInfo<Int32> SOURCE_IDProperty = RegisterProperty<Int32>(new PropertyInfo<Int32>("SOURCE_ID", "SOURCE_ID", 0m));
        public Int32 SOURCE_ID
        {
            get
            {
                return GetProperty(SOURCE_IDProperty);
            }
            set
            {
                SetProperty(SOURCE_IDProperty, value);
            }
        }

        public static readonly PropertyInfo<long> ITEM_REVIEW_IDProperty = RegisterProperty<long>(new PropertyInfo<long>("ITEM_REVIEW_ID", "ITEM_REVIEW_ID", 0m));
        public long ITEM_REVIEW_ID
        {
            get
            {
                return GetProperty(ITEM_REVIEW_IDProperty);
            }
            set
            {
                SetProperty(ITEM_REVIEW_IDProperty, value);
            }
        }

#if !SILVERLIGHT


        protected void DataPortal_Fetch(SingleCriteria<ZoteroItemSource, Int32> criteria)
        {
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ItemIDsFromSource", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SOURCE_ID", criteria.Value)); 
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        if (reader.Read())
                        {
                            LoadProperty<long>(ITEM_IDProperty, reader.GetInt64("ITEM_ID"));
                            MarkOld();
                        }
                    }
                }
                connection.Close();
            }
        }

        internal static ZoteroItemSource GetZoteroItemReviewIdsPerSource(SafeDataReader reader)
        {
            ZoteroItemSource returnValue = new ZoteroItemSource();
            returnValue.LoadProperty<long>(ITEM_REVIEW_IDProperty, reader.GetInt64("ITEM_REVIEW_ID"));
            returnValue.MarkOld();
            return returnValue;
        }
#endif
    }
}