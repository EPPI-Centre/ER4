using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
using System.ComponentModel;
//using Csla.Validation;

#if !SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagCurrentInfoList : DynamicBindingListBase<MagCurrentInfo>
    {
        public static void GetMagCurrentInfoList(EventHandler<DataPortalResult<MagCurrentInfoList>> handler)
        {
            DataPortal<MagCurrentInfoList> dp = new DataPortal<MagCurrentInfoList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch();
        }

#if SILVERLIGHT
        public MagCurrentInfoList() { }
#else
        public MagCurrentInfoList() { }
#endif

#if SILVERLIGHT
       
#else

        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagCurrentInfoList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(MagCurrentInfo.GetMagCurrentInfo(reader));
                        }
                    }
                }

                connection.Close();
            }
            RaiseListChangedEvents = true;
        }




#endif



    }


}
