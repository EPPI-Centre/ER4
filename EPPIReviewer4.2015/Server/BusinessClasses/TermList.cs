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
using System.Globalization;

#if!SILVERLIGHT
using System.Data.SqlClient;
using BusinessLibrary.Data;
using BusinessLibrary.Security;
using System.Text.RegularExpressions;
using System.Xml;
using BusinessLibrary.s.uk.ac.nactem.www;
using System.Data.SqlTypes;

using Microsoft.SqlServer.Server;
using Microsoft.SqlServer;
//using Microsoft.SqlServer.Dts.Runtime;

using System.Web;
using System.Net;
using System.IO;

#endif

namespace BusinessLibrary.BusinessClasses
{
    [Serializable]
    public class TermList : DynamicBindingListBase<Term>
    {

        public static void GetTermList(string text, string getwhat, EventHandler<DataPortalResult<TermList>> handler)
        {
            DataPortal<TermList> dp = new DataPortal<TermList>();
            dp.FetchCompleted += handler;
            dp.BeginFetch(new GetTermListCriteria(typeof(TermList), getwhat, text));
        }

        public string ListInSearchFormat()
        {
            string returnResult = "";
            IEnumerable<Term> query = from term in this
                                        orderby term.TermValue descending
                                        select term;
            bool firstTime = true;
            double topScore = 0;
            foreach (Term t in query)
            {
                string toAdd = "";
                if (firstTime == true)
                {
                    topScore = t.TermValue;
                    firstTime = false;
                }
                if (returnResult == "")
                {
                    toAdd = "\"" + t.Name + "\" weight(" + (t.TermValue / topScore).ToString("G3", CultureInfo.InvariantCulture) + ")";
                }
                else
                {
                    toAdd = ", \"" + t.Name + "\" weight(" + (t.TermValue / topScore).ToString("G3", CultureInfo.InvariantCulture) + ")";
                }
                if (toAdd.Length + returnResult.Length < 3988 && returnResult.IndexOf("\"" + t.Name + "\" weight(") == -1)
                {
                    returnResult += toAdd;
                }
                else
                {
                    break;
                }
            }
            return returnResult;
        }


#if SILVERLIGHT
        public TermList() { }
        public TermList Clone()
        {
            TermList res = new TermList();
            foreach (Term T in this)
            {
                Term newT = new Term();
                newT.Name = T.Name;
                newT.TermValue = T.TermValue;
                res.Add(newT);
            }
            return res;
        }
#else
        private TermList() { }
#endif
        

#if !SILVERLIGHT

        protected void DataPortal_Fetch(GetTermListCriteria criteria)
        {
            GetTerms(criteria, this);
        }

        private void GetTerms(GetTermListCriteria criteria, TermList theList)
        {
            RaiseListChangedEvents = false;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            switch (criteria.getWhat)
            {
                case "TFIDF":
                    // INTEGRATION SERVICES VERSION
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        //using (SqlCommand command = new SqlCommand("st_Generate_st_TempTermExtractionItemList", connection))
                        //{
                        //    command.CommandType = System.Data.CommandType.StoredProcedure;
                        //    command.Parameters.Add(new SqlParameter("@ITEMS", criteria.Text));
                        //    command.ExecuteNonQuery();
                        //    /*
                        //    Package pkg = new Package();
                        //    Microsoft.SqlServer.Dts.Runtime.Application app = new Microsoft.SqlServer.Dts.Runtime.Application();
                        //    Boolean packageExists = app.ExistsOnSqlServer(@"\DictionaryCreation", @"ssrulap41\2008", null, null);
                        //    pkg = app.LoadFromSqlServer(@"\DictionaryCreation", @"ssrulap41\2008", null, null, null);
                        //    DTSExecResult result = pkg.Execute();
                        //    */
                        //    using (SqlCommand command2 = new SqlCommand("st_ItemTermDictionary", connection))
                        //    {
                        //        command2.CommandType = System.Data.CommandType.StoredProcedure;
                        //        using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command2.ExecuteReader()))
                        //        {
                        //            while (reader.Read())
                        //            {
                        //                Add(Term.GetTerm(reader["TERM"].ToString(), Convert.ToDouble(reader["SCORE"].ToString())));
                        //            }
                        //            reader.Close();
                        //        }
                        //    }
                        //}
                        using (SqlCommand command = new SqlCommand("st_Extract_Terms", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@IDs", criteria.Text));
                            using (Csla.Data.SafeDataReader reader = new Csla.Data.SafeDataReader(command.ExecuteReader()))
                            {
                                while (reader.Read())
                                {
                                    Add(Term.GetTerm(reader["TERM"].ToString(), Convert.ToDouble(reader["SCORE"].ToString())));
                                }
                                reader.Close();
                            }
                        }
                        connection.Close();
                    }
                    break;

                case "Termine":
                    termine t = new termine();
                    string s = stripText(criteria.Text);
                    s = s.Replace(". ", "." + Environment.NewLine);
                    int size = s.Length;
                    //s = s.Substring(0, 16000);
                    //int temp = 0, temp2 = 0;
                    //while (temp2 < size - 1000)
                    //{
                    //    temp = s.IndexOf(Environment.NewLine, temp2 + 1);
                    //    if (temp == -1) break;
                    //    else if (temp - temp2 > 999)
                    //        s = s.Insert(s.IndexOf(" ", temp2 + 400), Environment.NewLine);
                    //    temp2 = temp;
                    //}
                    //if (size > 3000000)
                    //    return;   
                    //s = s.Substring(144500, 500);
                    //s = s.Replace('â', 'a');

                    //the general idea: try the first method (keyed termine), then the second (not keyed) after each step look at the result, 
                    //if it looks like it didn't work (an exception was raised or an error returned) keep track of the error and finally raise a meaningful exception.

                    //list of known errors messages: these are retruned as "results" not actual errors!
                    //The key for the web service has expired
                    //'This key expired on ' + expiry_date_str
        
                    //Number of daily requests exceeded
                    //'Daily invocation limit exceeded (%d). Try again tomorrow.' % daily_invocation_limit
                    // this is more than 100000 (bytes) but we don't know the number

                    //Daily data volume exceeded
                    //'Daily volume limit exceeded (%d). Try again tomorrow.' % daily_data_volume_limit
                    //this is probably 1000
                    XmlDocument doc = new XmlDocument();
                    XmlElement root ;
                    String error = "";
                    string tmp = "";
                    bool isErr = false, second = false;
                    StringBuilder returned = new StringBuilder();
                    try
                    {
                        returned.Append(t.analyze(s, "03e629aa24badd0ec520496d4640bec0df390451dbd73c87d0736ba6", "plain.genia", "xml", "", ""));
                    }
                    catch (Exception e)
                    {
                        error = "ERROR: TerMine (keyed) failed with the following message:" + Environment.NewLine + e.Message;
                        error += Environment.NewLine + "Please contact EPPISupport@ioe.ac.uk and report the error message above";
                        isErr = true;
                    }
                    tmp = returned.ToString();
                    if (tmp.Length < 70 && !isErr)//expected max size or error msg, and no exception
                    {
                        isErr = true;
                        Regex r1 = new Regex(@"Daily invocation limit exceeded \([0-9]{1,8}\). Try again tomorrow.");
                        Regex r2 = new Regex(@"Daily volume limit exceeded \([0-9]{1,8}\). Try again tomorrow.");
                        Regex r3 = new Regex(@"This key expired on ");
                        Match m = r1.Match(tmp);
                        Match m2 = r2.Match(tmp);
                        Match m3 = r3.Match(tmp);
                        if (m.Success)
                        {
                            error = "ERROR: TerMine was used too many times today," + Environment.NewLine + "please try again tomorrow (after midnight, according to London time zone)";
                        } 
                        else if (m2.Success)
                        {
                            error = "ERROR: TerMine has processed too much data today," + Environment.NewLine + "please try again tomorrow (after midnight, according to London time zone)";
                        }
                        else if (m3.Success)
                        {
                            error = "ERROR: the EPPI-Reviewer subscription to TerMine has expired," + Environment.NewLine + "please let EPPISupport@ioe.ac.uk know about this problem";
                        }
                        else
                        {
                            
                            //see if the result has the expected format, if it does, reset the error
                            int ind = tmp.IndexOf("<rankedTermCandidates ");
                            if (ind != -1 && ind < 30)//the expected xml root element is where one would think
                            {
                                isErr = false;
                            }
                            else
                            {
                                error = "ERROR: TerMine has returned unexpected results";
                            }
                        }
                    }
                    if (isErr && error != "" && error.IndexOf("ERROR: TerMine (keyed) failed with the following message:") >-1) //calling the keyed service raised an exception, we'll try the other one
                    {
                        returned.Clear();
                        second = true;
                        uk.ac.nactem.www.termine tt = new uk.ac.nactem.www.termine();
                        try
                        {
                            returned.Append(tt.analyze(s, "plain.genia", "xml", "", ""));
                            error = "";
                            isErr = false;
                        }
                        catch (Exception e2)
                        {
                            error += Environment.NewLine + "Also the secondary (not keyed) service returned an error: " + Environment.NewLine + e2.Message;
                            error += Environment.NewLine + "Please contact EPPISupport@ioe.ac.uk and report the error messages above";
                            isErr = true;
                        }
                    }
                    
                    
                    //returned = "<?xml version='1.0'?><baselevel><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">cochrane collaboration</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">childbirth group</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">group pcg</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">childbirth group pcg</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">international consumer</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">consumer panel</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">international consumer panel</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">editorial process.</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">country australia</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">australia brazil</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">country australia brazil</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">brazil canada</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">australia brazil canada</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">country australia brazil canada</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">canada china</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">brazil canada china</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">australia brazil canada china</termCandidate><termCandidate cValueScore=\"1.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">country australia brazil canada china</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">china mexico</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">canada china mexico</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">brazil canada china mexico</termCandidate><termCandidate cValueScore=\"1.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">australia brazil canada china mexico</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">country australia brazil canada china mexico</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">mexico new</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">china mexico new</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">canada china mexico new</termCandidate><termCandidate cValueScore=\"1.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">brazil canada china mexico new</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">australia brazil canada china mexico new</termCandidate><termCandidate cValueScore=\"1.807355\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">country australia brazil canada china mexico new</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">new zealand</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">mexico new zealand</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">china mexico new zealand</termCandidate><termCandidate cValueScore=\"1.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">canada china mexico new zealand</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">brazil canada china mexico new zealand</termCandidate><termCandidate cValueScore=\"1.807355\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">australia brazil canada china mexico new zealand</termCandidate><termCandidate cValueScore=\"2.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">country australia brazil canada china mexico new zealand</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">zealand norway</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">new zealand norway</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">mexico new zealand norway</termCandidate><termCandidate cValueScore=\"1.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">china mexico new zealand norway</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">canada china mexico new zealand norway</termCandidate><termCandidate cValueScore=\"1.807355\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">brazil canada china mexico new zealand norway</termCandidate><termCandidate cValueScore=\"2.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">australia brazil canada china mexico new zealand norway</termCandidate><termCandidate cValueScore=\"2.169925\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">country australia brazil canada china mexico new zealand norway</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">norway south</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">zealand norway south</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">new zealand norway south</termCandidate><termCandidate cValueScore=\"1.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">mexico new zealand norway south</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">china mexico new zealand norway south</termCandidate><termCandidate cValueScore=\"1.807355\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">canada china mexico new zealand norway south</termCandidate><termCandidate cValueScore=\"2.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">brazil canada china mexico new zealand norway south</termCandidate><termCandidate cValueScore=\"2.169925\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">australia brazil canada china mexico new zealand norway south</termCandidate><termCandidate cValueScore=\"3.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">country australia brazil canada china mexico new zealand norway south</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">south africa</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">norway south africa</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">zealand norway south africa</termCandidate><termCandidate cValueScore=\"1.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">new zealand norway south africa</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">mexico new zealand norway south africa</termCandidate><termCandidate cValueScore=\"1.807355\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">china mexico new zealand norway south africa</termCandidate><termCandidate cValueScore=\"2.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">canada china mexico new zealand norway south africa</termCandidate><termCandidate cValueScore=\"2.169925\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">brazil canada china mexico new zealand norway south africa</termCandidate><termCandidate cValueScore=\"3.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">australia brazil canada china mexico new zealand norway south africa</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">africa uk</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">south africa uk</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">norway south africa uk</termCandidate><termCandidate cValueScore=\"1.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">zealand norway south africa uk</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">new zealand norway south africa uk</termCandidate><termCandidate cValueScore=\"1.807355\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">mexico new zealand norway south africa uk</termCandidate><termCandidate cValueScore=\"2.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">china mexico new zealand norway south africa uk</termCandidate><termCandidate cValueScore=\"2.169925\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">canada china mexico new zealand norway south africa uk</termCandidate><termCandidate cValueScore=\"3.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">brazil canada china mexico new zealand norway south africa uk</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">pcg protocol</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">editorial process</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">impact. objective</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">editor reviewer</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">consumer contribution</termCandidate><termCandidate cValueScore=\"2.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">cochrane review</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">cochrane pcg.</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">pcg. method</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">cochrane pcg. method</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">routine documentation</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">editorial base</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">mapping interview</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">short questionnaire.</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">diverse views.</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">semi-structured telephone</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">telephone interview</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">semi-structured telephone interview</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">reviewer consumer</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">editor reviewer consumer</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">review group</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">group co-ordinator.</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">review group co-ordinator.</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">main issue</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">issue impression</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">main issue impression</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">themes. datum</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">process documents.</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">documents. results</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">process documents. results</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">results key</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">documents. results key</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">process documents. results key</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">key point</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">results key point</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">documents. results key point</termCandidate><termCandidate cValueScore=\"2.321928\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">process documents. results key point</termCandidate><termCandidate cValueScore=\"3.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">consumer input</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">review process</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">final review</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">review feedback</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">final review feedback</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">feedback mechanism</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">review feedback mechanism</termCandidate><termCandidate cValueScore=\"2.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">final review feedback mechanism</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">key area</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">process conclusion</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">review process conclusion</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">key issue</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">consumer involvement</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">further research</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">cochrane reviews.</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">reviews. acknowledgement</termCandidate><termCandidate cValueScore=\"1.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">cochrane reviews. acknowledgement</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">steering group</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">group discretionary</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">steering group discretionary</termCandidate><termCandidate cValueScore=\"-0.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">discretionary fund</termCandidate><termCandidate cValueScore=\"0.584962\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">group discretionary fund</termCandidate><termCandidate cValueScore=\"2.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">steering group discretionary fund</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">financial support</termCandidate><termCandidate cValueScore=\"1.000000\" xmlns=\"http://www.nactem.ac.uk/xsd/termine\">carol grant-pearce</termCandidate></baselevel>";
                    if (!isErr)
                    {
                            doc.InnerXml = returned.ToString();
                        root = doc.DocumentElement;
                        foreach (XmlNode nodex in root)
                        {
                            if (nodex.Attributes["cValueScore"].Value.Trim().ToLower() == "c-value") continue;
                            if (Convert.ToDouble(nodex.Attributes["cValueScore"].Value) > 0 && nodex.InnerText.Length < 50 && nodex.InnerText.ToLower().IndexOf("psychinfo") == -1)
                                if (nodex.InnerText.Contains("	"))
                                {
                                    Add(Term.GetTerm(nodex.InnerText.Split(new char[] { '	' }, StringSplitOptions.None)[1], Convert.ToDouble(nodex.Attributes["cValueScore"].Value)));
                                }
                                else
                                {
                                    Add(Term.GetTerm(nodex.InnerText, Convert.ToDouble(nodex.Attributes["cValueScore"].Value)));
                                }
                        }
                        // log the attempt
                        using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                        {
                            connection.Open();

                            using (SqlCommand command = new SqlCommand("st_Termine_Log_Insert", connection))
                            {
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.Add(new SqlParameter("@C_ID", ri.UserId));
                                command.Parameters.Add(new SqlParameter("@R_ID", ri.ReviewId));
                                command.Parameters.Add(new SqlParameter("@BYTES", s.Length * 2));
                                command.Parameters.Add(new SqlParameter("@SUCCESS", !isErr));
                                command.Parameters.Add(new SqlParameter("@N", this.Count));
                                if (error != "") command.Parameters.Add(new SqlParameter("@ERR", error));
                                command.ExecuteNonQuery();
                            }
                            connection.Close();
                        }
                    }
                     
                    else
                    {
                        throw new Exception(error);
                    }
                    if (this.Count == 0)
                    {
                        throw new Exception("FAILURE: the Termine service could not identify any term." + Environment.NewLine + "Please make sure the items used contain enough text.");
                    }
                    break;

                case "Zemanta":
                    // zemanta application key: n4hm7vw7kz95tsaaja5j8uyk
                    // yahoo application key: aFeZh3rV34HkagdgR8z9Gc585C8on7JWZFvqUbpLry6xs0sipn9.myr1ijglCYvRcAoaa5wM

                    string xml = getZemanta("zemanta.suggest", stripText(criteria.Text), "xml");
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(xml);
                    XmlNodeList xmlnode = xmldoc.GetElementsByTagName("keyword");
                    for (int i = 0; i < xmlnode.Count; i++)
                    {

                        Add(Term.GetTerm(xmlnode[i]["name"].InnerText, Convert.ToDouble(xmlnode[i]["confidence"].InnerText)));
                    }
                    break;

                case "Yahoo":
                    string xml2 = getYahoo(stripText(criteria.Text));
                    XmlDocument xmldoc2 = new XmlDocument();
                    xmldoc2.LoadXml(xml2);
                    XmlNodeList xmlnode2 = xmldoc2.GetElementsByTagName("Result");
                    for (int i = 0; i < xmlnode2.Count; i++)
                    {
                        Add(Term.GetTerm(xmlnode2[i].InnerText, (xmlnode2.Count - i) / 100));
                    }
                    break;

                default:
                    break;
            }
            RaiseListChangedEvents = true;
        }

        public static string getZemanta(string whichMethod, string whatContent, string whatFormat)
        {
            Uri address = new Uri("http://api.zemanta.com/services/rest/0.0/");
            // we need to create the web request
            HttpWebRequest wreq = WebRequest.Create(address) as HttpWebRequest;
            
            // we want to post the data - so set that here.
            wreq.Method = "POST";
            wreq.ContentType = "application/x-www-form-urlencoded";
            // build string with data for REST
            StringBuilder d = new StringBuilder();
            d.Append("method=" + HttpUtility.UrlEncode(whichMethod));
            d.Append("&api_key=" + HttpUtility.UrlEncode("n4hm7vw7kz95tsaaja5j8uyk"));
            d.Append("&text=" + HttpUtility.UrlEncode(whatContent));
            d.Append("&format=" + HttpUtility.UrlEncode(whatFormat));
            // break it down to a byte array
            byte[] bd = UTF8Encoding.UTF8.GetBytes(d.ToString());
            // set length & write
            wreq.ContentLength = bd.Length;
            using (Stream ps = wreq.GetRequestStream()) { ps.Write(bd, 0, bd.Length); }
            // response?
            using (HttpWebResponse wres = wreq.GetResponse() as HttpWebResponse)
            {
                // capture the response
                StreamReader r = new StreamReader(wres.GetResponseStream());
                // return the results
                return r.ReadToEnd();
            }
        }

        public static string getYahoo(string whatContent)
        {
            string returnValue;
            Uri address = new Uri("http://api.search.yahoo.com/ContentAnalysisService/V1/termExtraction");
            // Create the web request
            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
            // Set type to POST
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            string appId = "aFeZh3rV34HkagdgR8z9Gc585C8on7JWZFvqUbpLry6xs0sipn9.myr1ijglCYvRcAoaa5wM";
            
            StringBuilder data = new StringBuilder();
            data.Append("appid=" + HttpUtility.UrlEncode(appId));
            data.Append("&context=" + HttpUtility.UrlEncode(whatContent));
            //data.Append("&query=" + HttpUtility.UrlEncode(query));  

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());
            request.ContentLength = byteData.Length;
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream
                StreamReader reader = new StreamReader(response.GetResponseStream());
                // Console application output
                //Console.WriteLine(reader.ReadToEnd());
                returnValue = reader.ReadToEnd();
            }
            return returnValue;
        }

        private string stripText(string strIn)
        {
            return Regex.Replace(strIn, @"[^\n\rA-Za-z0-9\.@-]", " ", RegexOptions.Multiline);
        }

        public static TermList GetTermList(string text, string getwhat)
        {
            TermList theList = new TermList();
            theList.GetTerms(new GetTermListCriteria(typeof(TermList), getwhat, text), theList);
            return theList;
        }

        public static TermList AddTermsToList(string text, string getwhat, TermList theList)
        {
            theList.GetTerms(new GetTermListCriteria(typeof(TermList), getwhat, text), theList);
            return theList;
        }

#endif



    }

    // used to define parameters getting the terms: contains the parameters and whether we're using SQL Server (TF*IDF) or TerMine (NaCTeM) or Zemanta
    [Serializable]
    public class GetTermListCriteria : Csla.CriteriaBase<GetTermListCriteria>
    {
        private static PropertyInfo<string> getWhatProperty = RegisterProperty<string>(typeof(GetTermListCriteria), new PropertyInfo<string>("getWhat", "getWhat", string.Empty));
        public string getWhat
        {
            get { return ReadProperty(getWhatProperty); }
        }

        private static PropertyInfo<string> TextProperty = RegisterProperty<string>(typeof(GetTermListCriteria), new PropertyInfo<string>("Text", "Text"));
        public string Text
        {
            get { return ReadProperty(TextProperty); }
        }

        public GetTermListCriteria(Type type, string getwhat, string text)
            //: base(type)
        {
            LoadProperty(getWhatProperty, getwhat);
            LoadProperty(TextProperty, text);
        }

        public GetTermListCriteria() { }

    }
}
