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
    public class MagSimulationResultList : ReadOnlyListBase<MagSimulationResultList, MagSimulationResult>
    {
        public static void GetMagSimulationResultList(int MagSimulationID, string OrderBy, EventHandler<DataPortalResult<MagSimulationResultList>> handler)
        {
            DataPortal<MagSimulationResultList> dp = new DataPortal<MagSimulationResultList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new MagSimulationResultListSelectionCriteria(typeof(MagSimulationResultList), MagSimulationID, OrderBy));
        }

#if SILVERLIGHT
        public MagSimulationResultList() { }
#else
        private MagSimulationResultList() { }
#endif

#if SILVERLIGHT
       
#else

        protected void DataPortal_Fetch(MagSimulationResultListSelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagSimulationResults", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MagSimulationId", criteria.MagSimulationId));
                    command.Parameters.Add(new SqlParameter("@OrderBy", criteria.OrderBy));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        int index = 1;
                        int CumulativeIncludeCount = 0;
                        while (reader.Read())
                        {
                            // doing this here, as it's much faster than using IEnumerable + order by in the browser, and will work for Angular UI too
                            MagSimulationResult msr = MagSimulationResult.GetMagSimulationResult(reader, index, CumulativeIncludeCount);
                            CumulativeIncludeCount = msr.CumulativeIncludeCount;
                            index++;
                            Add(msr);
                        }
                    }
                }
                connection.Close();
                if (this.Items.Count > 50000)
                {
                    bool deleteThisOne = false;
                    for (int i = this.Items.Count - 1; i >= 0; i--)
                    {
                        if (deleteThisOne == true)
                        {
                            this.Items.RemoveAt(i);
                            deleteThisOne = false;
                        }
                        else
                        {
                            deleteThisOne = true;
                        }
                    }
                }
            }
            RaiseListChangedEvents = true;
        }




#endif



    }


    [Serializable]
    public class MagSimulationResultListSelectionCriteria : Csla.CriteriaBase<MagSimulationResultListSelectionCriteria>
    {
        private static PropertyInfo<int> MagSimulationIdProperty = RegisterProperty<int>(typeof(MagSimulationResultListSelectionCriteria), new PropertyInfo<int>("MagSimulationId", "MagSimulationId"));
        public int MagSimulationId
        {
            get { return ReadProperty(MagSimulationIdProperty); }
        }

        private static PropertyInfo<string> OrderByProperty = RegisterProperty<string>(typeof(MagSimulationResultListSelectionCriteria), new PropertyInfo<string>("OrderBy", "OrderBy"));
        public string OrderBy
        {
            get { return ReadProperty(OrderByProperty); }
        }

        public MagSimulationResultListSelectionCriteria(Type type, int magSimulationId, string orderBy)
        //: base(type)
        {
            LoadProperty(MagSimulationIdProperty, magSimulationId);
            LoadProperty(OrderByProperty, orderBy);
        }

        public MagSimulationResultListSelectionCriteria() { }
    }

}
