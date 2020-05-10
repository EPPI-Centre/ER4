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
using System.Net;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

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
        public MagFieldOfStudyList() { }
#endif

#if SILVERLIGHT
       
#else
        

        protected void DataPortal_Fetch(MagFieldOfStudyListSelectionCriteria selectionCriteria)
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            string searchString = "";

            switch (selectionCriteria.ListType)
            {
                case "PaperFieldOfStudyList":
                    searchString = selectionCriteria.PaperIdList == "" ? "" : 
                        searchString = "OR(Id=" + selectionCriteria.PaperIdList.Replace(",", ", Id=") +
                            @")&attributes=Id,F.DFN,F.FId,F.FN";
                    break;
                case "FieldOfStudyParentsList":
                    searchString = "Id=" + selectionCriteria.FieldOfStudyId.ToString() +
                        @"&attributes=Id,CC,DFN,ECC,FL,FN,FC.FId,FC.FN,FP.FId,FP.FN";
                    break;
                case "FieldOfStudyChildrenList":
                    searchString = "Id=" + selectionCriteria.FieldOfStudyId.ToString() +
                        @"&attributes=Id,CC,DFN,ECC,FL,FN,FC.FId,FC.FN,FP.FId,FP.FN";
                    break;
                case "FieldOfStudySearchList":
                    searchString = selectionCriteria.SearchText;
                    break;
            }

            if (searchString != "")
            {
                if (selectionCriteria.ListType == "FieldOfStudySearchList")
                {
                    MagMakesHelpers.MakesInterpretResponse respJson = MagMakesHelpers.InterpretQuery(searchString);
                    var fosDict = new Dictionary<string, int>();
                    if (respJson != null && respJson.interpretations != null && respJson.interpretations.Count > 0)
                    {
                        foreach (MagMakesHelpers.MakesInterpretation i in respJson.interpretations)
                        {
                            foreach (MagMakesHelpers.MakesInterpretationRule r in i.rules)
                            {
                                foreach (MagMakesHelpers.PaperMakes pm in r.output.entities)
                                {
                                    if (pm.F != null)
                                    {
                                        foreach (MagMakesHelpers.PaperMakesFieldOfStudy pmfos in pm.F)
                                        {
                                            if (MagPaperItemMatch.HaBoLevenshtein(pmfos.DFN, selectionCriteria.SearchText) > 20)
                                            {
                                                string key = pmfos.FId.ToString() + "¬" + pmfos.DFN;
                                                if (!fosDict.ContainsKey(key))
                                                {
                                                    fosDict.Add(key, 1);
                                                }
                                                else
                                                {
                                                    fosDict[key] = fosDict[key] + 1;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    foreach (KeyValuePair<string, int> eachFos in fosDict.OrderByDescending(val => val.Value))
                    {
                        MagMakesHelpers.PaperMakesFieldOfStudy newPmfos = new MagMakesHelpers.PaperMakesFieldOfStudy();
                        newPmfos.FId = Convert.ToInt64(eachFos.Key.Split('¬')[0]);
                        newPmfos.DFN = eachFos.Key.Split('¬')[1];
                        Add(MagFieldOfStudy.GetMagFieldOfStudyFromPaperMakesFieldOfStudy(newPmfos));
                    }
                    RaiseListChangedEvents = true;
                    return;
                }
                if (selectionCriteria.ListType == "PaperFieldOfStudyList") // these are paper entities not fields of study
                {
                    var fosDict = new Dictionary<string, int>();
                    //var respJson = JsonConvert.DeserializeObject<MagMakesHelpers.PaperMakesResponse>(responseText, jsonsettings);
                    MagMakesHelpers.PaperMakesResponse pmr = MagMakesHelpers.EvaluateExpressionNoPaging(searchString);
                    if (pmr.entities != null && pmr.entities.Count > 0)
                    {
                        foreach (MagMakesHelpers.PaperMakes fosm in pmr.entities)
                        {
                            if (fosm.F != null)
                            {
                                foreach (MagMakesHelpers.PaperMakesFieldOfStudy pmfos in fosm.F)
                                {
                                    string key = pmfos.FId.ToString() + "¬" + pmfos.DFN;
                                    if (!fosDict.ContainsKey(key))
                                    {
                                        fosDict.Add(key, 1);
                                    }
                                    else
                                    {
                                        fosDict[key] = fosDict[key] + 1;
                                    }
                                }
                            }
                        }
                        foreach (KeyValuePair<string, int> eachFos in fosDict.OrderByDescending(val => val.Value))
                        {
                            MagMakesHelpers.PaperMakesFieldOfStudy newPmfos = new MagMakesHelpers.PaperMakesFieldOfStudy();
                            newPmfos.FId = Convert.ToInt64(eachFos.Key.Split('¬')[0]);
                            newPmfos.DFN = eachFos.Key.Split('¬')[1];
                            Add(MagFieldOfStudy.GetMagFieldOfStudyFromPaperMakesFieldOfStudy(newPmfos));
                        }
                    }
                }
                else
                {
                    //var respJson = JsonConvert.DeserializeObject<MakesResponse>(responseText, jsonsettings);
                    MagMakesHelpers.MakesResponseFoS mrFoS = MagMakesHelpers.EvaluateFieldOfStudyExpression(searchString);
                    if (mrFoS.entities != null && mrFoS.entities.Count > 0)
                    {
                        foreach (MagMakesHelpers.FieldOfStudyMakes fosm in mrFoS.entities)
                        {
                            if (selectionCriteria.ListType == "FieldOfStudyParentsList")
                            {
                                if (fosm.FP != null && fosm.FP.Count > 0)
                                {
                                    foreach (MagMakesHelpers.FieldOfStudyRelationshipMakes fosrm in fosm.FP)
                                        Add(MagFieldOfStudy.GetMagFieldOfStudyRelationship(fosrm));
                                }
                            }
                            else
                            {
                                if (fosm.FC != null && fosm.FC.Count > 0)
                                {
                                    foreach (MagMakesHelpers.FieldOfStudyRelationshipMakes fosrm in fosm.FC)
                                        Add(MagFieldOfStudy.GetMagFieldOfStudyRelationship(fosrm));
                                }
                            }

                        }
                    }
                }
            }

            /*
            using (SqlConnection connection = new SqlConnection(DataConnection.AcademicControllerConnectionString))
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
            */
            RaiseListChangedEvents = true;
        }

        //private SqlCommand SpecifyListCommand(SqlConnection connection, MagFieldOfStudyListSelectionCriteria criteria, ReviewerIdentity ri)
        //{
        //    SqlCommand command = null;
        //    switch (criteria.ListType)
        //    {
        //        case "PaperFieldOfStudyList":
        //            command = new SqlCommand("st_AggregateFoSPaperList", connection);
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.Parameters.Add(new SqlParameter("@PaperIdList", criteria.PaperIdList));
        //            break;
        //        case "FieldOfStudyParentsList":
        //            command = new SqlCommand("st_FieldsOfStudyParentsList", connection);
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.Parameters.Add(new SqlParameter("@FieldOfStudyId", criteria.FieldOfStudyId));
        //            break;
        //        case "FieldOfStudyChildrenList":
        //            command = new SqlCommand("st_FieldsOfStudyChildrenList", connection);
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.Parameters.Add(new SqlParameter("@FieldOfStudyId", criteria.FieldOfStudyId));
        //            break;
        //            /* NOT CURRENTLY IMPLEMENTED
        //        case "FieldOfStudyRelatedFoSList":
        //            command = new SqlCommand("st_FieldsOfStudyRelatedFoSList", connection);
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.Parameters.Add(new SqlParameter("@FieldOfStudyId", criteria.FieldOfStudyId));
        //            break;
        //            */
        //            /*
        //        case "FieldOfStudySearchList":
        //            FullTextSearch fts = new FullTextSearch(criteria.SearchText);
        //            command = new SqlCommand("st_FieldsOfStudySearch", connection);
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.Parameters.Add(new SqlParameter("@SearchText", fts.NormalForm));
        //            break;
        //            */
        //    }
        //    return command;
        //}




#endif

    }
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
