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
    public class OutcomeList : DynamicBindingListBase<Outcome>
    {
        public static void GetOutcomeList(int setId, Int64 attributeIdIntervention, Int64 attributeIdControl,
                Int64 attributeIdOutcome, Int64 attributeId, int metaAnalysisId, string questions, string answers,
            EventHandler<DataPortalResult<OutcomeList>> handler)
        {
            DataPortal<OutcomeList> dp = new DataPortal<OutcomeList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new OutcomeListSelectionCriteria(typeof(OutcomeList), setId, attributeIdIntervention, attributeIdControl,
                attributeIdOutcome, attributeId, metaAnalysisId, questions, answers));
        }

        public bool HasSavedHandler = false;

        public void SetMetaAnalysisType(int MAType)
        {
            foreach (Outcome o in this)
            {
                o.SetESForThisOutcomeType(MAType);
                o.updateCanSelect(MAType);
                if (!o.CanSelect)
                {
                    o.IsSelected = false; 
                }
            }
            //return GetOutcomeClassificationFieldList();
        }

        public void SelectSelectable()
        {
            foreach (Outcome o in this)
            {
                o.IsSelected = true;
            }
        }

        public Dictionary<long, string> GetOutcomeClassificationFieldList()
        {
            Dictionary<long, string> fieldNames = new Dictionary<long, string>();
            foreach (Outcome o in this.Items)
            {
                foreach (OutcomeItemAttribute oia in o.OutcomeCodes)
                {
                    if (!fieldNames.ContainsKey(oia.AttributeId))
                    {
                        fieldNames.Add(oia.AttributeId, oia.AttributeName);
                    }
                }
            }
            foreach (Outcome o in this.Items)
            {
                foreach (OutcomeItemAttribute oia in o.OutcomeCodes)
                {
                    int c = 1;
                    foreach (KeyValuePair<long, string> currField in fieldNames)
                    {
                        
                        if (currField.Key == oia.AttributeId)
                        {
                            System.Reflection.PropertyInfo prop = o.GetType().GetProperty("occ" + c.ToString());
                            if (prop != null)
                            {
                                prop.SetValue(o, 1, null);
                            }
                        }
                        c++;
                    }
                }
            }
            return fieldNames;
        }

        public void UnSelectAll()
        {
            foreach (Outcome o in this)
            {
                o.IsSelected = false;
            }
        }

        public int CountSelected()
        {
            int c = 0;
            foreach (Outcome o in this)
            {
                if (o.IsSelected)
                    c++;
            }
            return c;
        }
        public Outcome GetOutcomebyID(int OutcomeID)
        {
            foreach (Outcome res in this)
            {
                if (res.OutcomeId == OutcomeID)
                {
                    return res;
                }
            }
            return null;
        }

#if SILVERLIGHT
        public OutcomeList() { }

    protected override void AddNewCore()
    {
        Add(Outcome.NewOutcome());
    }

#else
        public OutcomeList() { }
#endif


#if SILVERLIGHT
    
#else
        public static OutcomeList GetOutcomeList(int setId, Int64 attributeIdIntervention, Int64 attributeIdControl,
                Int64 attributeIdOutcome, Int64 attributeId, int metaAnalysisId, string questions, string answers)
        {
            OutcomeListSelectionCriteria criteria = new OutcomeListSelectionCriteria(typeof(OutcomeList), setId, attributeIdIntervention, attributeIdControl,
                attributeIdOutcome, attributeId, metaAnalysisId, questions, answers);
            OutcomeList returnValue = new OutcomeList();
            returnValue.DataPortal_Fetch(criteria);
            return returnValue;
        }

        //public static OutcomeList GetOutcomeList(int setId, Int64 attributeIdIntervention, Int64 attributeIdControl,
        //        Int64 attributeIdOutcome, Int64 attributeId, int metaAnalysisId, string questions, string answers)
        //{
        //    OutcomeList returnValue = new OutcomeList();
        //    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
        //    returnValue.RaiseListChangedEvents = false;

        //    // Unfortunately, this is a dynamic SQL query. Sergio, any ideas as to how to avoid this?

        //    string Variables = getVariables(metaAnalysisId, ri.ReviewId);
        //    string SQLAnswers = getAnswers(answers);
        //    string SQLQuestions = getQuestions(questions);

        //    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
        //    {
        //        connection.Open();
        //        using (SqlCommand command = new SqlCommand("st_OutcomeList", connection))
        //        {
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.Parameters.Add(new SqlParameter("@VARIABLES", Variables));
        //            command.Parameters.Add(new SqlParameter("@ANSWERS", " " + SQLAnswers));
        //            command.Parameters.Add(new SqlParameter("@QUESTIONS", " " + SQLQuestions));
        //            /*
        //            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
        //            command.Parameters.Add(new SqlParameter("@SET_ID", setId));
        //            command.Parameters.Add(new SqlParameter("@META_ANALYSIS_ID", metaAnalysisId));
        //            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_INTERVENTION", attributeIdIntervention)); // unnecessary filters???
        //            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_CONTROL", attributeIdControl));
        //            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_OUTCOME", attributeIdOutcome));
        //            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", attributeId));
        //            */

        //            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
        //            {
        //                while (reader.Read())
        //                {
        //                    returnValue.Add(Outcome.GetOutcome(reader));
        //                }
        //            }
        //        }
        //        connection.Close();
        //    }
        //    returnValue.RaiseListChangedEvents = true;
        //    return returnValue;
        //}
        private static string getVariables(int metaAnalysisId, int reviewId)
        {
            return "DECLARE @META_ANALYSIS_ID INT = " + metaAnalysisId.ToString() +
                " DECLARE @REVIEW_ID INT = " + reviewId.ToString() + " ";
        }

        private static string getAnswers(string answers)
        {
            string[] splitAnswers = answers.Split(',');
            string SQLAnswers = "";
            if (splitAnswers.Length > 0)
            {
                for (int i = 0; i < splitAnswers.Length; i++)
                {
                    if (splitAnswers[i] != "")
                    {
                        SQLAnswers += ", (SELECT TOP 1 CASE WHEN NOT ATTRIBUTE_ID IS NULL THEN 1 ELSE 0 END FROM TB_ITEM_ATTRIBUTE INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND tio.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID AND TB_ITEM_SET.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID WHERE TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID = " + splitAnswers[i] + ") AS AA" + (i + 1).ToString() + " ";
                    }
                }
            }
            return SQLAnswers;
        }

        private static string getQuestions(string questions)
        {
            string[] splitQuestions = questions.Split(',');
            string SQLQuestions = "";
            if (splitQuestions.Length > 0)
            {
                for (int i = 0; i < splitQuestions.Length; i++)
                {
                    if (splitQuestions[i] != "")
                    {
                        SQLQuestions += ", (SELECT TOP 1 CASE WHEN NOT ATTRIBUTE_NAME IS NULL THEN ATTRIBUTE_NAME ELSE 'missing' END FROM TB_ITEM_ATTRIBUTE INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID AND TB_ITEM_SET.IS_COMPLETED = 'TRUE' AND tio.ITEM_SET_ID = TB_ITEM_SET.ITEM_SET_ID AND TB_ITEM_SET.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.SET_ID = TB_ITEM_SET.SET_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID INNER JOIN TB_ATTRIBUTE ON TB_ATTRIBUTE.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID WHERE TB_ATTRIBUTE_SET.PARENT_ATTRIBUTE_ID = " + splitQuestions[i] + ") AS AQ" + (i + 1).ToString() + " ";
                    }
                }
            }
            return SQLQuestions;
        }

        protected void DataPortal_Fetch(OutcomeListSelectionCriteria criteria)
        {
            //this = GetOutcomeList(criteria.SetId, criteria.AttributeIdIntervention, criteria.AttributeIdControl, criteria.AttributeIdOutcome
            //    , criteria.AttributeId, criteria.MetaAnalysisId, criteria.Questions, criteria.Answers);
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;

            // Unfortunately, this is a dynamic SQL query. Sergio, any ideas as to how to avoid this?

            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                connection.Open();
                //using (SqlCommand command = new SqlCommand("st_OutcomeListTMP_TEST", connection))
                using (SqlCommand command = new SqlCommand("st_OutcomeList", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
                    command.Parameters.Add(new SqlParameter("@META_ANALYSIS_ID", criteria.MetaAnalysisId));
                    command.Parameters.Add(new SqlParameter("@ANSWERS", criteria.Answers));
                    command.Parameters.Add(new SqlParameter("@QUESTIONS", criteria.Questions));


                    using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            Add(Outcome.GetOutcome(reader));
                        }
                        reader.NextResult();//fill in the questions data
                        //match (static!) fields with list of questions
                        
                        char[] separator = new char[] { ',' };
                        string[] QuestionsArr = criteria.Questions.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        Dictionary<long, string> fieldMapper = new Dictionary<long, string>();
                        int i = 1;
                        foreach (string q in QuestionsArr)
                        {
                            fieldMapper.Add(long.Parse(q), "aq" + i.ToString());
                            i++;
                            if (i > 20) break;
                        }
                        i = 0;
                        Outcome currO =  new Outcome();
                        while (reader.Read())//fill in the questions data
                        {
                            if (i == 0)//first iteration!
                            {
                                currO = this.GetOutcomebyID(reader.GetInt32("OUTCOME_ID"));
                            }
                            else if (currO.OutcomeId != reader.GetInt32("OUTCOME_ID"))
                            {
                                //currO.markOld();
                                currO = this.GetOutcomebyID(reader.GetInt32("OUTCOME_ID"));
                            }
                            string fieldName;
                            fieldMapper.TryGetValue(reader.GetInt64("PARENT_ATTRIBUTE_ID"), out fieldName);
                            switch (fieldName)
                            {
                                case "aq1":
                                    if (currO.aq1 != null && currO.aq1.Length > 0) continue;
                                    currO.aq1 = reader.GetString("Codename");
                                    break;
                                case "aq2":
                                    if (currO.aq2 != null && currO.aq2.Length > 0) continue;
                                    currO.aq2 = reader.GetString("Codename");
                                    break;
                                case "aq3":
                                    if (currO.aq3 != null && currO.aq3.Length > 0) continue;
                                    currO.aq3 = reader.GetString("Codename");
                                    break;
                                case "aq4":
                                    if (currO.aq4 != null && currO.aq4.Length > 0) continue;
                                    currO.aq4 = reader.GetString("Codename");
                                    break;
                                case "aq5":
                                    if (currO.aq5 != null && currO.aq5.Length > 0) continue;
                                    currO.aq5 = reader.GetString("Codename");
                                    break;
                                case "aq6":
                                    if (currO.aq6 != null && currO.aq6.Length > 0) continue;
                                    currO.aq6 = reader.GetString("Codename");
                                    break;
                                case "aq7":
                                    if (currO.aq7 != null && currO.aq7.Length > 0) continue;
                                    currO.aq7 = reader.GetString("Codename");
                                    break;
                                case "aq8":
                                    if (currO.aq8 != null && currO.aq8.Length > 0) continue;
                                    currO.aq8 = reader.GetString("Codename");
                                    break;
                                case "aq9":
                                    if (currO.aq9 != null && currO.aq9.Length > 0) continue;
                                    currO.aq9 = reader.GetString("Codename");
                                    break;
                                case "aq10":
                                    if (currO.aq10 != null && currO.aq10.Length > 0) continue;
                                    currO.aq10 = reader.GetString("Codename");
                                    break;
                                case "aq11":
                                    if (currO.aq11 != null && currO.aq11.Length > 0) continue;
                                    currO.aq11 = reader.GetString("Codename");
                                    break;
                                case "aq12":
                                    if (currO.aq12 != null && currO.aq12.Length > 0) continue;
                                    currO.aq12 = reader.GetString("Codename");
                                    break;
                                case "aq13":
                                    if (currO.aq13 != null && currO.aq13.Length > 0) continue;
                                    currO.aq13 = reader.GetString("Codename");
                                    break;
                                case "aq14":
                                    if (currO.aq14 != null && currO.aq14.Length > 0) continue;
                                    currO.aq14 = reader.GetString("Codename");
                                    break;
                                case "aq15":
                                    if (currO.aq15 != null && currO.aq15.Length > 0) continue;
                                    currO.aq15 = reader.GetString("Codename");
                                    break;
                                case "aq16":
                                    if (currO.aq16 != null && currO.aq16.Length > 0) continue;
                                    currO.aq16 = reader.GetString("Codename");
                                    break;
                                case "aq17":
                                    if (currO.aq17 != null && currO.aq17.Length > 0) continue;
                                    currO.aq17 = reader.GetString("Codename");
                                    break;
                                case "aq18":
                                    if (currO.aq18 != null && currO.aq18.Length > 0) continue;
                                    currO.aq18 = reader.GetString("Codename");
                                    break;
                                case "aq19":
                                    if (currO.aq19 != null && currO.aq19.Length > 0) continue;
                                    currO.aq19 = reader.GetString("Codename");
                                    break;
                                case "aq20":
                                    if (currO.aq20 != null && currO.aq20.Length > 0) continue;
                                    currO.aq20 = reader.GetString("Codename");
                                    break;
                                default:
                                    break;
                            }
                            i++;
                        }
                        reader.NextResult();
                        fieldMapper.Clear();
                        string[] AnswersArr = criteria.Answers.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        i = 1;
                        foreach (string q in AnswersArr)
                        {
                            fieldMapper.Add(long.Parse(q), "aa" + i.ToString());
                            i++;
                            if (i > 20) break;
                        }
                        i = 0;
                        currO = new Outcome();
                        while (reader.Read() )//fill in the answers data
                        {
                            if (i == 0)//first iteration!
                            {
                                currO = this.GetOutcomebyID(reader.GetInt32("OUTCOME_ID")); 
                            }
                            else if (currO.OutcomeId != reader.GetInt32("OUTCOME_ID"))
                            {
                                currO = this.GetOutcomebyID(reader.GetInt32("OUTCOME_ID"));
                            }
                            string fieldName;
                            long currAttID = reader.GetInt64("ATTRIBUTE_ID");
                            fieldMapper.TryGetValue(currAttID, out fieldName);
                            switch (fieldName)
                            {
                                case "aa1":
                                    currO.aa1 = 1;
                                    break;
                                case "aa2":
                                    currO.aa2 = 1;
                                    break;
                                case "aa3":
                                    currO.aa3 = 1;
                                    break;
                                case "aa4":
                                    currO.aa4 = 1;
                                    break;
                                case "aa5":
                                    currO.aa5 = 1;
                                    break;
                                case "aa6":
                                    currO.aa6 = 1;
                                    break;
                                case "aa7":
                                    currO.aa7 = 1;
                                    break;
                                case "aa8":
                                    currO.aa8 = 1;
                                    break;
                                case "aa9":
                                    currO.aa9 = 1;
                                    break;
                                case "aa10":
                                    currO.aa10 = 1;
                                    break;
                                case "aa11":
                                    currO.aa11 = 1;
                                    break;
                                case "aa12":
                                    currO.aa12 = 1;
                                    break;
                                case "aa13":
                                    currO.aa13 = 1;
                                    break;
                                case "aa14":
                                    currO.aa14 = 1;
                                    break;
                                case "aa15":
                                    currO.aa15 = 1;
                                    break;
                                case "aa16":
                                    currO.aa16 = 1;
                                    break;
                                case "aa17":
                                    currO.aa17 = 1;
                                    break;
                                case "aa18":
                                    currO.aa18 = 1;
                                    break;
                                case "aa19":
                                    currO.aa19 = 1;
                                    break;
                                case "aa20":
                                    currO.aa20 = 1;
                                    break;
                                default:
                                    break;
                            }
                            i++;
                        }
                    }
                }
                connection.Close();
            }
            RaiseListChangedEvents = true;
        }
        //protected void DataPortal_Fetch(OutcomeListSelectionCriteria criteria)
        //{
        //    ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
        //    RaiseListChangedEvents = false;
        //    string Variables = getVariables(criteria.MetaAnalysisId, ri.ReviewId);
        //    string SQLAnswers = getAnswers(criteria.Answers);
        //    string SQLQuestions = getQuestions(criteria.Questions);
        //    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
        //    {
        //        connection.Open();
        //        using (SqlCommand command = new SqlCommand("st_OutcomeList", connection))
        //        {
        //            command.CommandType = System.Data.CommandType.StoredProcedure;
        //            command.Parameters.Add(new SqlParameter("@VARIABLES", Variables));
        //            command.Parameters.Add(new SqlParameter("@ANSWERS", " " + SQLAnswers));
        //            command.Parameters.Add(new SqlParameter("@QUESTIONS", " " + SQLQuestions));
        //            /*
        //            command.Parameters.Add(new SqlParameter("@REVIEW_ID", ri.ReviewId));
        //            command.Parameters.Add(new SqlParameter("@SET_ID", criteria.SetId));
        //            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_INTERVENTION", criteria.AttributeIdIntervention));
        //            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_CONTROL", criteria.AttributeIdControl));
        //            command.Parameters.Add(new SqlParameter("@ITEM_ATTRIBUTE_ID_OUTCOME", criteria.AttributeIdOutcome));
        //            command.Parameters.Add(new SqlParameter("@ATTRIBUTE_ID", criteria.AttributeId));
        //            command.Parameters.Add(new SqlParameter("@META_ANALYSIS_ID", criteria.MetaAnalysisId));
        //            */
        //            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
        //            {
        //                while (reader.Read())
        //                {
        //                    Add(Outcome.GetOutcome(reader));
        //                }
        //            }
        //        }
        //        connection.Close();
        //    }
        //    RaiseListChangedEvents = true;
        //}

        protected void DataPortal_Fetch()
        {
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            RaiseListChangedEvents = false;
            Random rand = new Random();
            /*
            for (int i = 1; i < 4; i++)
            {
                Outcome outcome = new Outcome();
                outcome.OutcomeTypeId = 1;
                outcome.Title = "Title " + i.ToString();
                outcome.Data1 = rand.Next(1, 1 * i) + 50;
                outcome.Data2 = rand.Next(1, 1 * i) + 50;
                outcome.Data3 = 13.2 + rand.Next(2, 16);
                outcome.Data4 = 11.2 + rand.Next(2, 15);
                outcome.Data5 = 3.5;
                outcome.Data6 = 3.2;
                Add(outcome);
            }
             */
            Outcome outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 1";
            outcome.ShortTitle = "Study 1";
            outcome.Data1 = 23;
            outcome.Data2 = 23;
            outcome.Data3 = 3.1;
            outcome.Data4 = 1.8;
            outcome.Data5 = 1;
            outcome.Data6 = 1;
            outcome.IsSelected = true;
            Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 2";
            outcome.ShortTitle = "Study 2";
            outcome.Data1 = 33;
            outcome.Data2 = 43;
            outcome.Data3 = 33;
            outcome.Data4 = 32;
            outcome.Data5 = 2.5;
            outcome.Data6 = 2.5;
            outcome.IsSelected = true;
            Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 3";
            outcome.ShortTitle = "Study 3";
            outcome.Data1 = 78;
            outcome.Data2 = 78;
            outcome.Data3 = 24;
            outcome.Data4 = 22.3;
            outcome.Data5 = 2.8;
            outcome.Data6 = 2.8;
            outcome.IsSelected = true;
            Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 4";
            outcome.ShortTitle = "Study 4";
            outcome.Data1 = 230;
            outcome.Data2 = 229;
            outcome.Data3 = 33.9;
            outcome.Data4 = 33.25;
            outcome.Data5 = 1.6;
            outcome.Data6 = 1.7;
            outcome.IsSelected = true;
            Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 5";
            outcome.ShortTitle = "Study 5";
            outcome.Data1 = 20;
            outcome.Data2 = 20;
            outcome.Data3 = 2.1;
            outcome.Data4 = 0.8;
            outcome.Data5 = 1.5;
            outcome.Data6 = 1.4;
            outcome.IsSelected = true;
            Add(outcome);
            outcome = new Outcome();
            outcome.OutcomeTypeId = 1;
            outcome.Title = "Study 6";
            outcome.ShortTitle = "Study 6";
            outcome.Data1 = 56;
            outcome.Data2 = 57;
            outcome.Data3 = 18.3;
            outcome.Data4 = 18.18;
            outcome.Data5 = 1.2;
            outcome.Data6 = 1.3;
            outcome.IsSelected = true;
            Add(outcome);
            RaiseListChangedEvents = true;
        }

        

        
#endif
        [Serializable]
        public class OutcomeListSelectionCriteria : Csla.CriteriaBase<OutcomeListSelectionCriteria>
        {
            private static PropertyInfo<int> SetIdProperty = RegisterProperty<int>(typeof(OutcomeListSelectionCriteria), new PropertyInfo<int>("SetId", "SetId"));
            public int SetId
            {
                get { return ReadProperty(SetIdProperty); }
            }

            private static PropertyInfo<Int64> AttributeIdInterventionProperty = RegisterProperty<Int64>(typeof(OutcomeListSelectionCriteria), new PropertyInfo<Int64>("AttributeIdIntervention", "AttributeIdIntervention"));
            public Int64 AttributeIdIntervention
            {
                get { return ReadProperty(AttributeIdInterventionProperty); }
            }

            private static PropertyInfo<Int64> AttributeIdControlProperty = RegisterProperty<Int64>(typeof(OutcomeListSelectionCriteria), new PropertyInfo<Int64>("AttributeIdControl", "AttributeIdControl"));
            public Int64 AttributeIdControl
            {
                get { return ReadProperty(AttributeIdControlProperty); }
            }

            private static PropertyInfo<Int64> AttributeIdOutcomeProperty = RegisterProperty<Int64>(typeof(OutcomeListSelectionCriteria), new PropertyInfo<Int64>("AttributeIdOutcome", "AttributeIdOutcome"));
            public Int64 AttributeIdOutcome
            {
                get { return ReadProperty(AttributeIdOutcomeProperty); }
            }

            private static PropertyInfo<Int64> AttributeIdProperty = RegisterProperty<Int64>(typeof(OutcomeListSelectionCriteria), new PropertyInfo<Int64>("AttributeId", "AttributeId"));
            public Int64 AttributeId
            {
                get { return ReadProperty(AttributeIdProperty); }
            }

            private static PropertyInfo<int> MetaAnalysisIdProperty = RegisterProperty<int>(typeof(OutcomeListSelectionCriteria), new PropertyInfo<int>("MetaAnalysisId", "MetaAnalysisId"));
            public int MetaAnalysisId
            {
                get { return ReadProperty(MetaAnalysisIdProperty); }
            }

            private static PropertyInfo<string> QuestionsProperty = RegisterProperty<string>(typeof(OutcomeListSelectionCriteria), new PropertyInfo<string>("Questions", "Questions"));
            public string Questions
            {
                get { return ReadProperty(QuestionsProperty); }
            }

            private static PropertyInfo<string> AnswersProperty = RegisterProperty<string>(typeof(OutcomeListSelectionCriteria), new PropertyInfo<string>("Answers", "Answers"));
            public string Answers
            {
                get { return ReadProperty(AnswersProperty); }
            }

            public OutcomeListSelectionCriteria(Type type, int setId, Int64 attributeIdIntervention, Int64 attributeIdControl,
                Int64 attributeIdOutcome, Int64 attributeId, int metaAnalysisId, string questions, string answers)//: base(type)
            {
                LoadProperty(SetIdProperty, setId);
                LoadProperty(AttributeIdInterventionProperty, attributeIdIntervention);
                LoadProperty(AttributeIdControlProperty, attributeIdControl);
                LoadProperty(AttributeIdOutcomeProperty, attributeIdOutcome);
                LoadProperty(AttributeIdProperty, attributeId);
                LoadProperty(MetaAnalysisIdProperty, metaAnalysisId);
                LoadProperty(QuestionsProperty, questions);
                LoadProperty(AnswersProperty, answers);
            }

            public OutcomeListSelectionCriteria() { }
        }

    }
}
