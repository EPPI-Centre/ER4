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
    public class ComparisonAttributeList : DynamicBindingListBase<ComparisonAttribute>
    {

        public static void GetComparisonAttributeList(int ComparisonId, Int64 ParentAttributeId, int setId, EventHandler<DataPortalResult<ComparisonAttributeList>> handler)
        {
            DataPortal<ComparisonAttributeList> dp = new DataPortal<ComparisonAttributeList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new ComparisonAttributeSelectionCriteria(typeof(ComparisonAttributeList), ComparisonId, ParentAttributeId, setId));
        }


#if SILVERLIGHT
        public ComparisonAttributeList() { }
#else
        public  ComparisonAttributeList() { }
#endif


#if SILVERLIGHT
       
#else
        protected void DataPortal_Fetch(ComparisonAttributeSelectionCriteria criteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("st_ComparisonAttributesList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@COMPARISON_ID", criteria.ComparisonId));
                    command.Parameters.Add(new SqlParameter("@PARENT_ATTRIBUTE_ID", criteria.ParentAttributeId));
                    command.Parameters.Add(new SqlParameter("@SET_ID", criteria.SetId));
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(ComparisonAttribute.GetComparisonAttribute(reader));
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
    public class ComparisonAttributeSelectionCriteria : Csla.CriteriaBase<ComparisonAttributeSelectionCriteria>
    {
        private static PropertyInfo<int> ComparisonIdProperty = RegisterProperty<int>(typeof(ComparisonAttributeSelectionCriteria), new PropertyInfo<int>("ComparisonId", "ComparisonId"));
        public int ComparisonId
        {
            get { return ReadProperty(ComparisonIdProperty); }
        }

        private static PropertyInfo<Int64> ParentAttributeIdProperty = RegisterProperty<Int64>(typeof(ComparisonAttributeSelectionCriteria), new PropertyInfo<Int64>("ParentAttributeId", "ParentAttributeId"));
        public Int64 ParentAttributeId
        {
            get { return ReadProperty(ParentAttributeIdProperty); }
        }

        private static PropertyInfo<Int64> SetIdProperty = RegisterProperty<Int64>(typeof(ComparisonAttributeSelectionCriteria), new PropertyInfo<Int64>("SetId", "SetId"));
        public Int64 SetId
        {
            get { return ReadProperty(SetIdProperty); }
        }

        public ComparisonAttributeSelectionCriteria(Type type, int comparisonId, Int64 parentAttributeId, int setId) //: base(type)
        {
            LoadProperty(ComparisonIdProperty, comparisonId);
            LoadProperty(ParentAttributeIdProperty, parentAttributeId);
            LoadProperty(SetIdProperty, setId);
        }

        public ComparisonAttributeSelectionCriteria() { }

    }

}
