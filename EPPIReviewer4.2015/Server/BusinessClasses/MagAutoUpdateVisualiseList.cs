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
using BusinessLibrary.Security;
#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class MagAutoUpdateVisualiseList : DynamicBindingListBase<MagAutoUpdateVisualise>
    //public class MagAutoUpdateVisualiseList : BusinessListBase<MagAutoUpdateVisualiseList, MagAutoUpdateVisualise>
    {
        public static void GetMagAutoUpdateVisualiseList(MagAutoUpdateVisualiseSelectionCriteria selectionCriteria, EventHandler<DataPortalResult<MagAutoUpdateVisualiseList>> handler)
        {
            DataPortal<MagAutoUpdateVisualiseList> dp = new DataPortal<MagAutoUpdateVisualiseList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(selectionCriteria);
        }


#if SILVERLIGHT
        public MagAutoUpdateVisualiseList() { }
#else
        public MagAutoUpdateVisualiseList() { }
#endif


#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch(MagAutoUpdateVisualiseSelectionCriteria criteria)
        {
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_MagAutoUpdateVisualise", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MAG_AUTO_UPDATE_RUN_ID", criteria.MagAutoUpdateRunId));
                    command.Parameters.Add(new SqlParameter("@FIELD", criteria.Field));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(MagAutoUpdateVisualise.GetMagAutoUpdateVisualise(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

#endif
    }


        // used to define the parameters for the query.
        [Serializable]
        public class MagAutoUpdateVisualiseSelectionCriteria : BusinessBase //Csla.CriteriaBase
        {
            public MagAutoUpdateVisualiseSelectionCriteria() { }
            public static readonly PropertyInfo<int> MagAutoUpdateRunIdProperty = RegisterProperty<int>(typeof(MagAutoUpdateVisualiseSelectionCriteria), new PropertyInfo<int>("MagAutoUpdateRunId", "MagAutoUpdateRunId"));
            public int MagAutoUpdateRunId
            {
                get { return ReadProperty(MagAutoUpdateRunIdProperty); }
                set
                {
                    SetProperty(MagAutoUpdateRunIdProperty, value);
                }
            }

            public static readonly PropertyInfo<string> FieldProperty = RegisterProperty<string>(typeof(MagAutoUpdateVisualiseSelectionCriteria), new PropertyInfo<string>("Field", "Field"));
            public string Field
            {
                get { return ReadProperty(FieldProperty); }
                set
                {
                    SetProperty(FieldProperty, value);
                }
            }

            
        }


    
}
