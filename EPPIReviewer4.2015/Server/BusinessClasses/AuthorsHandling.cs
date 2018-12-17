using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using Csla.DataPortalClient;


/// <summary>
/// an effort to normalise the Author field of tb_ITEM
/// </summary>
namespace AuthorsHandling
{
    public static class NormaliseAuth
    {
        //private static List<AutH> AutList = new ArrayList();
        private static string inStr;
        private static int SkCount = 0;

        public static List<AutH> processField(string CurrLn, int OrigiN)
        {
            return processField(CurrLn, OrigiN, 1);
        }
        public static List<AutH> processField(string CurrLn, int OrigiN, int StartRank)
        {
            List<AutH> AutList = new List<AutH>();
            int Rank = StartRank;
            CurrLn = trimMe(CurrLn);
            CurrLn = CurrLn.Replace(@"//", ";");
            CurrLn = CurrLn.Replace(@"/", ";");
            if (!CurrLn.Contains(" ")) //if no spaces are in field, I guess there is one Author only
            {
                if (CurrLn.Contains(";"))
                {
                    char[] separator = findSep(ref CurrLn);
                    string[] moreAuth = CurrLn.Split(separator);
                    foreach (string sAuth in moreAuth)
                    {
                        AutList.Add(singleAuth(sAuth, Rank, OrigiN));
                        Rank++;
                    }
                }
                else
                {
                    AutList.Add(singleAuth(CurrLn, Rank, OrigiN));
                }
            }
            else if (CurrLn.IndexOf(" ") == CurrLn.LastIndexOf(" "))//one space only, as above
            {
                if (CurrLn.Contains(";"))
                {
                    char[] separator = findSep(ref CurrLn);
                    string[] moreAuth = CurrLn.Split(separator);
                    foreach (string sAuth in moreAuth)
                    {
                        AutList.Add(singleAuth(sAuth, Rank, OrigiN));
                        Rank++;
                    }
                }
                else
                {
                    AutList.Add(singleAuth(CurrLn, Rank, OrigiN));
                }
            }
            else if (CurrLn.ToLower().Contains(" and ") | CurrLn.Contains("& "))
            { // if uses "and" or &
                if (CurrLn.ToLower().IndexOf("et al") > (CurrLn.Length - 7))
                {
                    SkCount++;
                    //Console.WriteLine("Skipped (" + SkCount + ")");

                }
                else if (CurrLn.ToLower().Contains(" and others ") | CurrLn.IndexOfAny(new char[] { '.', ',', ';' }) == -1)
                {
                    SkCount++;
                    //Console.WriteLine("Skipped (" + SkCount + ")");
                }
                else if (splitme(CurrLn, new char[] { ' ' }).Length == 5)
                {// string contains "and" and 4 other words, it's likely to contain two authors...
                    string[] separator = new string[] { " and ", " AND ", " And ", "And ", " & ", "& " };
                    string[] Biauth = CurrLn.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string sAuth in Biauth)
                    {
                        AutList.Add(singleAuth(sAuth, Rank, OrigiN));
                        Rank++;
                    }
                }
                else //no special cases: find separator, substitute "and" with it and feed splitted string as usual
                {
                    inStr = CurrLn;
                    char[] separator = findSep(ref inStr);
                    //substitue "and" with the separator if the separator is not present only at the end of the string
                    int df = inStr.IndexOf(separator[0]);
                    if (inStr.IndexOf(separator[0]) < inStr.Length - 1) 
                    {
                        inStr = inStr.Replace("and", Convert.ToString(separator[0]));
                        inStr = inStr.Replace("And", Convert.ToString(separator[0]));
                        inStr = inStr.Replace("AND", Convert.ToString(separator[0]));
                        inStr = inStr.Replace("&", Convert.ToString(separator[0]));
                    }
                    string[] moreAuth = inStr.Split(separator);
                    foreach (string sAuth in moreAuth)
                    {
                        AutList.Add(singleAuth(sAuth, Rank, OrigiN));
                        Rank++;
                    }
                }
            }
            else
            {
                char[] separator = findSep(ref CurrLn);
                string[] moreAuth = CurrLn.Split(separator);
                foreach (string sAuth in moreAuth)
                {
                    AutList.Add(singleAuth(sAuth, Rank, OrigiN));
                    Rank++;
                }
            }
            int i = 0;
            if (AutList.Count > 256)
                AutList = AutList.GetRange(0, 256);
            while ( i < AutList.Count)
            {
                if (AutList[i].LastName == "")
                    AutList.RemoveAt(i);
                else i++;
            }
            if (AutList.Count == 0) AutList.Add(singleAuth(CurrLn, Rank, OrigiN));
            return AutList;
        }

        static string[] splitme(string instr, char[] separator)
        {
            return instr.Split(separator);
        }
        static char[] findSep(ref string inStr)
        {

            if (inStr.Contains("; "))
            {
                inStr = inStr.Replace("; ", ";");
                return new char[] { ';' };
            }
            else if (inStr.Contains(";") )
            {
                return new char[] { ';' };
            }
            else if (inStr.Contains(".,"))
            {
                inStr = inStr.Replace(".,", ";");
                return new char[] { ';' };
            }
            else if (inStr.Contains(Environment.NewLine))
            {
                inStr = inStr.Replace(Environment.NewLine, ";");
                return new char[] { ';' };
            }
            else if (inStr.Contains(","))
            {
                if (inStr.IndexOf(' ') == -1 || inStr.IndexOf(' ') > inStr.IndexOf(','))
                {
                    return new char[] { '¬' };
                }
                else
                {
                    inStr = inStr.Replace(",", ";");
                    return new char[] { ';' };
                }
            }
            return new char[] { '!' };
        }

        static string trimMe(string inSt)
        {
            String delim = " ,.*|@/\\+=-_!£$%^&*()`\"";
            return inSt.Trim(delim.ToCharArray());
        }
        
        static AutH singleAuth(string AuthSt, int Rank, int OrigiN)
        {
            AutH Au;
            string Second, First, Last, temP;
            Last = null;
            First = null;
            Second = null;
            string[] CompP = new string[] {"da ","de la ", "de ", "del ","des ","di ","dr ","el ","la ","le ",
                "mc ","st ","van de ","van der ","van den ","van ","von "};


            char[] delimiters = new char[] { ',', '.', ' ' };
            AuthSt = trimMe(AuthSt);
            if (AuthSt.IndexOfAny(delimiters) != -1)
            {   //
                temP = AuthSt.Substring(0, 1).ToLower() + AuthSt.Substring(1);
                if (temP.IndexOf(' ') != -1)
                    temP = temP.Substring(0, temP.IndexOf(' ') + 1) + temP.Substring(temP.IndexOf(' ') + 1, 1).ToLower() + temP.Substring(temP.IndexOf(' ') + 2);
                foreach (string ComPs in CompP)
                {
                    if (temP.IndexOf(ComPs) == 0)
                    {
                        temP = AuthSt.Substring(0, ComPs.Length);
                        temP = temP.Replace(' ', '¬');
                        AuthSt = temP + AuthSt.Substring(temP.Length);
                        if (AuthSt.IndexOfAny(delimiters) == -1)
                        {
                            Au = AutH.NewAutH(AuthSt.Replace('¬', ' '), Rank, OrigiN);
                            return Au;
                        }
                    }
                }

                Last = AuthSt.Substring(0, AuthSt.IndexOfAny(delimiters));
                Last = trimMe(Last.Replace('¬', ' '));
                AuthSt = AuthSt.Substring(AuthSt.IndexOfAny(delimiters) + 1);
                AuthSt = trimMe(AuthSt);

                if (AuthSt.IndexOfAny(delimiters) != -1)
                {
                    First = AuthSt.Substring(0, AuthSt.IndexOfAny(delimiters));
                    First = trimMe(First);
                    Second = trimMe(AuthSt.Substring(AuthSt.IndexOfAny(delimiters) + 1));
                    Au = AutH.NewAutH(Last, First, Second, Rank, OrigiN);
                }
                else
                {
                    First = trimMe(AuthSt);
                    Au = AutH.NewAutH(Last, First, Rank, OrigiN);
                }
            }
            else
            {
                Au = AutH.NewAutH(AuthSt, Rank, OrigiN);
            }
            return Au;
        }
    }

    [Serializable]
    public class AutH : BusinessBase<AutH> //this type stores the 5 values that define one author
    {
        
        #region properties
        public static readonly PropertyInfo<string> FirstNameProperty = RegisterProperty<string>(new PropertyInfo<string>("FirstName", "FirstName", 0));
        public string FirstName
        {
            get { return GetProperty(FirstNameProperty); }
            set
            {
                if (value.Length > 50) SetProperty(FirstNameProperty, value.Substring(0, 50));
                else SetProperty(FirstNameProperty, value);
            }
        }
        public static readonly PropertyInfo<string> LastNameProperty = RegisterProperty<string>(new PropertyInfo<string>("LastName", "LastName", 0));
        public string LastName
        {
            get { return GetProperty(LastNameProperty); }
            set
            {
                if (value.Length > 50) SetProperty(LastNameProperty, value.Substring(0, 50));
                else SetProperty(LastNameProperty, value);
            }
        }
        public static readonly PropertyInfo<string> MiddleNameProperty = RegisterProperty<string>(new PropertyInfo<string>("MiddleName", "MiddleName", 0));
        public string MiddleName
        {
            get { return GetProperty(MiddleNameProperty); }
            set
            {
                if (value.Length > 50) SetProperty(MiddleNameProperty, value.Substring(0, 50));
                else SetProperty(MiddleNameProperty, value);
            }
        }
        public static readonly PropertyInfo<int> RankProperty = RegisterProperty<int>(new PropertyInfo<int>("Rank", "Rank", 0));
        public int Rank
        {
            get { return GetProperty(RankProperty); }
            set { SetProperty(RankProperty, value); }
        }
        public static readonly PropertyInfo<int> RoleProperty = RegisterProperty<int>(new PropertyInfo<int>("Role", "Role", 0));
        public int Role
        {
            get { return GetProperty(RoleProperty); }
            set { SetProperty(RoleProperty, value); }
        }
        #endregion
        #region constructors
        
        internal static AutH NewAutH(string LastName, string FirstName, string MiddleName, int Rank, int Role)
        {
            AutH res = new AutH();
            if (LastName.Length > 50)
                res.SetProperty(LastNameProperty, LastName.Substring(0, 50));
            else res.SetProperty(LastNameProperty, LastName);
            if (FirstName.Length > 50)
                res.SetProperty(FirstNameProperty, FirstName.Substring(0, 50));
            else res.SetProperty(FirstNameProperty, FirstName);
            if (MiddleName.Length > 50)
                res.SetProperty(MiddleNameProperty, MiddleName.Substring(0, 50));
            else res.SetProperty(MiddleNameProperty, MiddleName);
            res.SetProperty(RankProperty, Rank);
            res.SetProperty(RoleProperty, Role);
            return res;
        }
        internal static AutH NewAutH(string LastName, string FirstName, int Rank, int Role)
        {
            AutH res = new AutH(); 
            if (LastName.Length > 50)
                res.SetProperty(LastNameProperty, LastName.Substring(0, 50));
            else res.SetProperty(LastNameProperty, LastName);
            if (FirstName.Length > 50)
                res.SetProperty(FirstNameProperty, FirstName.Substring(0, 50));
            else res.SetProperty(FirstNameProperty, FirstName);
            res.SetProperty(MiddleNameProperty, "");
            res.SetProperty(RankProperty, Rank);
            res.SetProperty(RoleProperty, Role);
            return res;
        }
        internal static AutH NewAutH(string LastName, int Rank, int Role)
        {
            AutH res = new AutH();
            if (LastName.Length > 50)
                res.SetProperty(LastNameProperty, LastName.Substring(0, 50));
            else res.SetProperty(LastNameProperty, LastName);
            res.SetProperty(FirstNameProperty, "");
            res.SetProperty(MiddleNameProperty, "");
            res.SetProperty(RankProperty, Rank);
            res.SetProperty(RoleProperty, Role);
            return res;
        }
        internal static AutH NewAutH()
        {
            AutH res = new AutH();
            res.SetProperty(LastNameProperty, "");
            res.SetProperty(FirstNameProperty, "");
            res.SetProperty(MiddleNameProperty, "");
            res.SetProperty(RankProperty, 0);
            res.SetProperty(RoleProperty, 0);
            return res;
        }

        public AutH() { }

        #endregion
    }
    [Serializable]
    public class AutorsList : MobileList<AutH>
    {
        public override string ToString()
        {
            string res = "";
            foreach (AutH au in this)
            {
                res += (au.LastName.Length > 0 ? " " + au.LastName : "")
                    + (au.FirstName.Length > 0 ? " " + au.FirstName : "")
                    + (au.MiddleName.Length > 0 ? " " + au.MiddleName : "")
                    + "; ";
            }
            return res.Trim();
        }
    }
}
