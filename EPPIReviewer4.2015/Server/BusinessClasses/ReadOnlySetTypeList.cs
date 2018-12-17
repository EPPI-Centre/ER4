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
using Csla.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class ReadOnlySetTypeList : ReadOnlyListBase<ReadOnlySetTypeList, ReadOnlySetType>
    {
        public ReadOnlySetTypeList() { }
        public static void GetSetTypeList(EventHandler<DataPortalResult<ReadOnlySetTypeList>> handler)
        {
            DataPortal<ReadOnlySetTypeList> dp = new DataPortal<ReadOnlySetTypeList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }
        public ReadOnlySetType GetSetType(int TypeID)
        {
            foreach (ReadOnlySetType rost in this)
            {
                if (rost.SetTypeId == TypeID) return rost;
            }
            return null;
        }
#if !SILVERLIGHT

        private void DataPortal_Fetch()
        {
            RaiseListChangedEvents = false;
            IsReadOnly = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_SetTypeList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ReadOnlySetType.GetReadOnlySetType(reader));
                        }
                        reader.NextResult();
                        while (reader.Read())
                        {
                            foreach (ReadOnlySetType rost in this)
                            {
                                if (rost.SetTypeId == reader.GetInt32("SET_TYPE_ID"))
                                {
                                    rost.AddAllowedCodeType(reader.GetInt32("ATTRIBUTE_TYPE_ID"), reader.GetString("ATTRIBUTE_TYPE"));
                                    break;
                                }
                            }
                        }
                        reader.NextResult();
                        while (reader.Read())
                        {
                            foreach (ReadOnlySetType rost in this)
                            {
                                if (rost.SetTypeId == reader.GetInt32("DEST_SET_TYPE_ID"))
                                {
                                    rost.AddAllowedSetTypesID4Paste(reader.GetInt32("SRC_SET_TYPE_ID"));
                                    break;
                                }
                            }
                        }
                    }
                }
                connection.Close();
            }
            IsReadOnly = true;
            RaiseListChangedEvents = true;
        }

#endif
    }
}
