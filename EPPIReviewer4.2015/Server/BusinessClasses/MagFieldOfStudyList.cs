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
    public class MagFieldOfStudyList : DynamicBindingListBase<MagFieldOfStudy>
    {
        public static void GetMagFieldOfStudyList(MagFieldOfStudyListSelectionCriteria selectionCriteria, EventHandler<DataPortalResult<MagFieldOfStudyList>> handler)
        {
            DataPortal<MagFieldOfStudyList> dp = new DataPortal<MagFieldOfStudyList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(selectionCriteria);
        }


#if SILVERLIGHT
        public MagFieldOfStudyList() { }
#else
        private MagFieldOfStudyList() { }
#endif

#if SILVERLIGHT
       
#else

        protected void DataPortal_Fetch(MagFieldOfStudyListSelectionCriteria selectionCriteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = SpecifyListCommand(connection, selectionCriteria, ri))
                {
                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(MagFieldOfStudy.GetMagFieldOfStudy(reader));
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }

        private SqlCommand SpecifyListCommand(SqlConnection connection, MagFieldOfStudyListSelectionCriteria criteria, ReviewerIdentity ri)
        {
            SqlCommand command = null;
            switch (criteria.ListType)
            {
                case "PaperFieldOfStudyList":
                    command = new SqlCommand("st_AggregateFoSPaperList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PaperIdList", criteria.PaperIdList));
                    break;
                case "FieldOfStudyParentsList":
                    command = new SqlCommand("st_FieldsOfStudyParentsList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@FieldOfStudyId", criteria.FieldOfStudyId));
                    break;
                case "FieldOfStudyChildrenList":
                    command = new SqlCommand("st_FieldsOfStudyChildrenList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@FieldOfStudyId", criteria.FieldOfStudyId));
                    break;
                case "FieldOfStudyRelatedFoSList":
                    command = new SqlCommand("st_FieldsOfStudyRelatedFoSList", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@FieldOfStudyId", criteria.FieldOfStudyId));
                    break;
                case "FieldOfStudySearchList":
                    FullTextSearch fts = new FullTextSearch(criteria.SearchText);
                    command = new SqlCommand("st_FieldsOfStudySearch", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SearchText", fts.NormalForm));
                    break;
            }
            return command;
        }




#endif

                // used to define the parameters for the query.
        [Serializable]
        public class MagFieldOfStudyListSelectionCriteria : BusinessBase //Csla.CriteriaBase
        {
            public MagFieldOfStudyListSelectionCriteria() { }
            public static readonly PropertyInfo<Int64> FieldOfStudyIdProperty = RegisterProperty<Int64>(typeof(MagFieldOfStudyListSelectionCriteria), new PropertyInfo<Int64>("FieldOfStudyId", "FieldOfStudyId"));
            public Int64 FieldOfStudyId
            {
                get { return ReadProperty(FieldOfStudyIdProperty); }
                set
                {
                    SetProperty(FieldOfStudyIdProperty, value);
                }
            }

            public static readonly PropertyInfo<string> ListTypeProperty = RegisterProperty<string>(typeof(MagFieldOfStudyListSelectionCriteria), new PropertyInfo<string>("ListType", "ListType", ""));
            public string ListType
            {
                get { return ReadProperty(ListTypeProperty); }
                set
                {
                    SetProperty(ListTypeProperty, value);
                }
            }

            public static readonly PropertyInfo<string> PaperIdListProperty = RegisterProperty<string>(typeof(MagFieldOfStudyListSelectionCriteria), new PropertyInfo<string>("PaperIdList", "PaperIdList", ""));
            public string PaperIdList
            {
                get { return ReadProperty(PaperIdListProperty); }
                set
                {
                    SetProperty(PaperIdListProperty, value);
                }
            }

            public static readonly PropertyInfo<string> SearchTextProperty = RegisterProperty<string>(typeof(MagFieldOfStudyListSelectionCriteria), new PropertyInfo<string>("SearchText", "SearchText", ""));
            public string SearchText
            {
                get { return ReadProperty(SearchTextProperty); }
                set
                {
                    SetProperty(SearchTextProperty, value);
                }
            }
        }


        }

    
}
