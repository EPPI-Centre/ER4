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


            if (selectionCriteria.ListType == "FieldOfStudySearchList")
            {
                string pageNumber = selectionCriteria.PageNumber.ToString();
                if (pageNumber == "0") {
                    pageNumber = "1";
                }
                MagMakesHelpers.OaConceptFilterResult concepts = MagMakesHelpers.EvaluateOaConceptFilter(selectionCriteria.SearchText, "50", pageNumber, true);

                var fosDict = new Dictionary<string, int>();
                if (concepts != null && concepts.results != null && concepts.results.Length > 0)
                {
                    foreach (MagMakesHelpers.OaFullConcept c in concepts.results)
                    {
                        string key = c.id.Replace("https://openalex.org/C", "") + "¬" + c.display_name;
                        if (!fosDict.ContainsKey(key))
                        {
                            fosDict.Add(key, 1);
                        }
                        else
                        {
                            fosDict[key] = fosDict[key] + 1;
                        }
                    }

                    int totalResults = concepts.meta.count;
                }
                foreach (KeyValuePair<string, int> eachFos in fosDict.OrderByDescending(val => val.Value))
                {
                    MagMakesHelpers.OaFullConcept newPmfos = new MagMakesHelpers.OaFullConcept();
                    newPmfos.id = "https://openalex.org/C" + eachFos.Key.Split('¬')[0];
                    newPmfos.display_name = eachFos.Key.Split('¬')[1];
                    Add(MagFieldOfStudy.GetMagFieldOfStudyFromPaperMakesFieldOfStudy(newPmfos, concepts.meta.count));
                }
                RaiseListChangedEvents = true;
                return;
            }
            if (selectionCriteria.ListType == "PaperFieldOfStudyList") // these are paper entities not fields of study
            {
                if (selectionCriteria.PaperIdList != "")
                {
                    var fosDict = new Dictionary<string, int>();
                    string searchString = "openalex_id:https://openalex.org/W" + selectionCriteria.PaperIdList.Replace(",", "|https://openalex.org/W");
                    MagMakesHelpers.OaPaperFilterResult pmr = MagMakesHelpers.EvaluateOaPaperFilter(searchString, "50", "1", false);
                    if (pmr.results != null && pmr.results.Length > 0)
                    {
                        foreach (MagMakesHelpers.OaPaper fosm in pmr.results)
                        {
                            if (fosm.concepts != null)
                            {
                                foreach (MagMakesHelpers.Concept pmfos in fosm.concepts)
                                {
                                    string key = pmfos.id.ToString().Replace("https://openalex.org/C", "") + "¬" + pmfos.display_name;
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
                            MagMakesHelpers.OaFullConcept newPmfos = new MagMakesHelpers.OaFullConcept();
                            newPmfos.id = "https://openalex.org/C" + eachFos.Key.Split('¬')[0];
                            newPmfos.display_name = eachFos.Key.Split('¬')[1];
                            Add(MagFieldOfStudy.GetMagFieldOfStudyFromPaperMakesFieldOfStudy(newPmfos, 0));
                        }
                    }
                }
            }
            else
            {
                MagMakesHelpers.OaFullConcept mrFoS = MagMakesHelpers.EvaluateSingleConcept(selectionCriteria.FieldOfStudyId.ToString());
                if (mrFoS != null)
                {
                    if (selectionCriteria.ListType == "FieldOfStudyParentsList")
                    {
                        if (mrFoS.ancestors != null && mrFoS.ancestors.Length > 0)
                        {
                            foreach (MagMakesHelpers.Ancestor fosrm in mrFoS.ancestors)
                                Add(MagFieldOfStudy.GetAncestor(fosrm));
                        }
                    }
                    else
                    {
                        string query = "ancestors.id:https://openalex.org/C" + selectionCriteria.FieldOfStudyId.ToString();
                        MagMakesHelpers.OaConceptFilterResult children = MagMakesHelpers.EvaluateOaConceptFilter(query, "50", "1", false);
                        if (children != null && children.results.Length > 0)
                        {
                            foreach (MagMakesHelpers.OaFullConcept concept in children.results)
                                Add(MagFieldOfStudy.GetConcept(concept));
                        }
                    }
                }
            }

            RaiseListChangedEvents = true;
        }

        
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
        

            public static readonly PropertyInfo<Int64> PageNumberProperty = RegisterProperty<Int64>(typeof(MagFieldOfStudyListSelectionCriteria), new PropertyInfo<Int64>("PageNumber", "PageNumber"));
            public Int64 PageNumber
            {
                get { return ReadProperty(PageNumberProperty); }
                set
                {
                    SetProperty(PageNumberProperty, value);
                }
            }
        }

}
