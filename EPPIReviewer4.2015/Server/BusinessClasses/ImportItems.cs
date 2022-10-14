using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AuthorsHandling;
using System.Reflection;
using Csla;
using Csla.Security;
using Csla.Core;
using Csla.Serialization;
using Csla.Silverlight;
//using Csla.Validation;
using Csla.DataPortalClient;

#if!SILVERLIGHT
using System.Data;
using System.Data.SqlClient;
using BusinessLibrary.Data;
using Csla.Data;
using BusinessLibrary.Security;
#endif


namespace BusinessLibrary.BusinessClasses.ImportItems
{
    #region datacrunching
    public static class ImportRefs
    {
        public static MobileList<ItemIncomingData> Imp(string DataIn, FilterRules rules)
        {
            //char[] trimmer = {'\r\n', ' '};
            DataIn = DataIn.Trim();
            Match MatchO;
            Regex rExp;
            bool found = false;
            AuthorsHandling.AutorsList tempAut, tempPAuth;
            MobileList<ItemIncomingData> OutO = new MobileList<ItemIncomingData>();
            //rules.StartOfNewField = new Regex(@"\w\w[\w\s][\w\s]-");
            
            List<string> records = splitme(DataIn, rules.StartOfNewRec);
            List<string> rDates = new List<string>();
            string c_rec;
            int cAuth, cpAuth, count = records.Count, Cindex = 0;
            PropertyInfo[] propertyInfos;
            propertyInfos = typeof(FilterRules).GetProperties();//used to identify rules that need to be remapped for a given reference type

            while (Cindex < count)
            {//first of all, a round trip to create all the resulting item and assign them a type, this is necessary because rules can be remapped depending on the reference type
                found = false;
                c_rec = records[Cindex];
                List<string> fields = splitme(c_rec, rules.StartOfNewField);
                ItemIncomingData tItem = ItemIncomingData.NewItem();
                foreach (string fil in fields)
                {
                    rExp = rules.typeField;
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        
                        foreach (KeyValuePair<int, Regex> cKey in rules.typesMap)
                        {
                            MatchO = cKey.Value.Match(fil);
                            //if (MatchO.Success & MatchO.Index > 4 & MatchO.Index < 8)
                            if (MatchO.Success)
                            {
                                tItem.Type_id = cKey.Key;
                                found = true;
                                break;
                            }

                        }
                    }
                }

                if (found != true)
                    tItem.Type_id = rules.DefaultTypeCode;
                Cindex++;
                OutO.Add(tItem);
            }//end of first round trip
            rExp = rules.Title[0];
            Cindex = 0;
            FilterRules currentR = rules;
            double d;
            System.Globalization.CultureInfo MyCultureInfo = new System.Globalization.CultureInfo("en-GB");
            foreach (string rec in records)
            {
                currentR = new FilterRules(rules);
                foreach (TypeRules TyR in rules.typeRules)
                {
                    if (OutO[Cindex].Type_id == TyR.Type_ID)//check if current object is of a type that needs rules remapping
                    {
                        foreach (PropertyInfo pinfo in propertyInfos)
                        {
                            if (pinfo.Name == TyR.RuleName)//relies on the property name of filterule to remap! 
                            {
                                if (TyR.RuleRegexSt == null) TyR.RuleRegexSt = @"\\M\\w";
                                pinfo.SetValue(currentR, FilterRules.buildRegexList(@TyR.RuleRegexSt), null);
                            }
                        }
                    }
                }
                cAuth = 0;
                cpAuth = 0;
                found = false;
                tempAut = new AuthorsHandling.AutorsList();
                tempPAuth = new AuthorsHandling.AutorsList();
                List<string> fields = splitme(rec, currentR.StartOfNewField);
                ItemIncomingData tItem = ItemIncomingData.NewItem();
                foreach (string fil in fields)
                {

                    rExp = currentR.Title[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].Title = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.Title.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Title[1];
                            MatchO = rExp.Match(OutO[Cindex].Title);
                            if (MatchO.Success)
                                OutO[Cindex].Title = OutO[Cindex].Title.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Title = cleanup(OutO[Cindex].Title);
                        //continue;
                    }
                    rExp = currentR.pTitle[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0 & OutO[Cindex].Parent_title == "")
                    {
                        OutO[Cindex].Parent_title = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.pTitle.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.pTitle[1];
                            MatchO = rExp.Match(OutO[Cindex].Parent_title);
                            if (MatchO.Success)
                                OutO[Cindex].Parent_title = OutO[Cindex].Parent_title.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Parent_title = cleanup(OutO[Cindex].Parent_title);
                    }
                    rExp = currentR.shortTitle[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].Short_title = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.shortTitle.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.shortTitle[1];
                            MatchO = rExp.Match(OutO[Cindex].Short_title);
                            if (MatchO.Success)
                                OutO[Cindex].Short_title = OutO[Cindex].Short_title.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Short_title = cleanup(OutO[Cindex].Short_title);
                        OutO[Cindex].Short_title = cleanup(OutO[Cindex].Short_title);
                        //continue ;
                    }
                    rExp = currentR.Date[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        //tItem.Parent_title = fil.Substring(MatchO.Length).Trim();
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.Date.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Date[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        }
                        string[] dateStrings = getDate( tmp);
                        found = true;
                        if (OutO[Cindex].Year == "" || OutO[Cindex].Year == "0") OutO[Cindex].Year = dateStrings[0];
                        if (OutO[Cindex].Month == "" || OutO[Cindex].Month == "0") OutO[Cindex].Month = dateStrings[1];
                        #region dataFormatTests
                        /*string[] dateStrings = {"trtrfgv January 2008", //0
                        "05/2009",//1
                        "2009///",//2
                        @"2008/// 2-27-2009",//3
                        "2009/05",//4
                        "trtrfgv January 2008 2:57:32.8 PM", //5
                        "trtrfgv January 08",//6
                        "2009/02//ciiccoo", //7
                        "2/27/2009", //8
                        "2-27-2009",//9
                        "02-27-2009",//10
                        "22/2/2009",//11
                        "trtrfgv f 58 fvdsf ",//12
                        "22/02/2009 erer",//13
                        "02/27/2009 erer",//14
                        "1 May 2008 2:57:32.8 PM", //15
                        "sd1 May 2008 2:57:32.8 PM", //16
                        " opor 22/07/09",//17
                        "07/22/09",//18
                        "22/07/09",//19
                        "uuu22/07/09 tyung",//20
                        "22/07/09 22/09/09",//21
                        "Fri, 15 May 2009 20:10:57 GMT" };
                        rDates.Clear();
                        foreach (string dateString in dateStrings)
                        {
                            
                            string[] dt = getDate(dateString);
                            rDates.Add(string.Concat(dt));
                            //Console.WriteLine(dt.Year + " " + dt.Month);
                        }*/
                        #endregion
                        //continue;
                    }
                    rExp = currentR.Month[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        found = true;
                        if (OutO[Cindex].Month == "" || OutO[Cindex].Month == "0")
                            {
                            string tmp = fil.Substring(MatchO.Length).Trim();
                            if (currentR.Month.Count > 1)
                            {
                                //discard what's not needed
                                rExp = currentR.Month[1];
                                MatchO = rExp.Match(OutO[Cindex].Month);
                                if (MatchO.Success)
                                    tmp = tmp.Substring(0, MatchO.Index);
                            }
                            tmp = monthstr(tmp);
                            if (tmp.Length > 0)
                            {
                                OutO[Cindex].Month = tmp;
                            }
                            else
                            {
                                Double.TryParse(fil.Substring(MatchO.Length).Trim(), System.Globalization.NumberStyles.Integer, MyCultureInfo, out d);
                                if (d > 0)
                                {
                                    OutO[Cindex].Month = monthstr((int)d);
                                }
                            }
                        }
                        //continue;
                    }
                    rExp = currentR.Author[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.Author.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Author[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        }
                        tempAut.AddRange(NormaliseAuth.processField(tmp, 0, cAuth));
                        cAuth = tempAut.Count;
                        found = true;
                        //continue;
                    }
                    rExp = currentR.ParentAuthor[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.ParentAuthor.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.ParentAuthor[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        }
                        tempPAuth.AddRange(NormaliseAuth.processField(tmp, 1, cpAuth));
                        cpAuth = tempPAuth.Count;
                        found = true;
                        //continue;
                    }
                    rExp = currentR.StandardN[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].Standard_number = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.StandardN.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.StandardN[1];
                            MatchO = rExp.Match(OutO[Cindex].Standard_number);
                            if (MatchO.Success)
                                OutO[Cindex].Standard_number = OutO[Cindex].Standard_number.Length > 0 ? 
                                    OutO[Cindex].Standard_number + " | " + OutO[Cindex].Standard_number.Substring(0, MatchO.Index)
                                    : OutO[Cindex].Standard_number.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Standard_number = cleanup(OutO[Cindex].Standard_number);
                        //continue;
                    }
                    rExp = currentR.City[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].City = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.City.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.City[1];
                            MatchO = rExp.Match(OutO[Cindex].City);
                            if (MatchO.Success)
                                OutO[Cindex].City = OutO[Cindex].City.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].City = cleanup(OutO[Cindex].City);//continue;
                    }
                    rExp = currentR.Publisher[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].Publisher = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.Publisher.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Publisher[1];
                            MatchO = rExp.Match(OutO[Cindex].Publisher);
                            if (MatchO.Success)
                                OutO[Cindex].Publisher = OutO[Cindex].Publisher.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Publisher = cleanup(OutO[Cindex].Publisher);
                        //continue;
                    }
                    rExp = currentR.Institution[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].Institution = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.Institution.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Institution[1];
                            MatchO = rExp.Match(OutO[Cindex].Institution);
                            if (MatchO.Success)
                                OutO[Cindex].Institution = OutO[Cindex].Institution.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Institution = cleanup(OutO[Cindex].Institution);
                        //continue;
                    }
                    rExp = currentR.Volume[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].Volume = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.Volume.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Volume[1];
                            MatchO = rExp.Match(OutO[Cindex].Volume);
                            if (MatchO.Success)
                                OutO[Cindex].Volume = OutO[Cindex].Volume.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Volume = cleanup(OutO[Cindex].Volume);
                        //continue;
                    }
                    rExp = currentR.Edition[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].Edition = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.Edition.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Edition[1];
                            MatchO = rExp.Match(OutO[Cindex].Edition);
                            if (MatchO.Success)
                                OutO[Cindex].Edition = OutO[Cindex].Edition.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Edition = cleanup(OutO[Cindex].Edition);
                        //continue;
                    }
                    rExp = currentR.Issue[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].Issue = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.Issue.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Issue[1];
                            MatchO = rExp.Match(OutO[Cindex].Issue);
                            if (MatchO.Success)
                                OutO[Cindex].Issue = OutO[Cindex].Issue.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Issue = cleanup(OutO[Cindex].Issue);
                        //continue;
                    }
                    rExp = currentR.StartPage[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.StartPage.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.StartPage[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        }
                        if (OutO[Cindex].Pages != "" & OutO[Cindex].Pages.IndexOf('-') == -1)
                            OutO[Cindex].Pages = tmp + "-" + OutO[Cindex].Pages;
                        else if (OutO[Cindex].Pages == "")
                            OutO[Cindex].Pages = tmp;
                        found = true;
                        OutO[Cindex].Pages = cleanup(OutO[Cindex].Pages);
                        //continue;
                    }
                    rExp = currentR.EndPage[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.EndPage.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.EndPage[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        }
                        if (OutO[Cindex].Pages != "" & OutO[Cindex].Pages.IndexOf('-') == -1)
                            OutO[Cindex].Pages = OutO[Cindex].Pages + "-" + tmp;
                        else if (OutO[Cindex].Pages == "")
                            OutO[Cindex].Pages = tmp;
                        found = true;
                        OutO[Cindex].Pages = cleanup(OutO[Cindex].Pages);
                        //continue;
                    }
                    rExp = currentR.Pages[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.Pages.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Pages[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        }
                        if (OutO[Cindex].Pages == "")
                            OutO[Cindex].Pages = tmp;
                        found = true;
                        OutO[Cindex].Pages = cleanup(OutO[Cindex].Pages);
                        //continue;
                    }
                    rExp = currentR.Availability[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].Availability = fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.Availability.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Availability[1];
                            MatchO = rExp.Match(OutO[Cindex].Availability);
                            if (MatchO.Success)
                                OutO[Cindex].Availability = OutO[Cindex].Availability.Substring(0, MatchO.Index);
                        }
                        OutO[Cindex].Availability = cleanup(OutO[Cindex].Availability);
                        //continue;
                    }
                    rExp = currentR.Url[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.Url.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Url[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        }
                        if (OutO[Cindex].Url != "" & OutO[Cindex].Url != tmp)
                            OutO[Cindex].Url = OutO[Cindex].Url + " " + tmp;
                        else OutO[Cindex].Url = tmp;
                        found = true;
                        OutO[Cindex].Url = cleanup(OutO[Cindex].Url);
                        //continue;
                    }
                    rExp = currentR.Abstract[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.Abstract.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Abstract[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        } 
                        if (OutO[Cindex].Abstract != "")
                            OutO[Cindex].Abstract = OutO[Cindex].Abstract + " " + tmp;
                        else
                            OutO[Cindex].Abstract = tmp;
                        found = true;
                        OutO[Cindex].Abstract = cleanup(OutO[Cindex].Abstract);
                        //continue;
                    }
                    rExp = currentR.OldItemID[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        OutO[Cindex].OldItemId =  fil.Substring(MatchO.Length).Trim();
                        found = true;
                        if (currentR.OldItemID.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.OldItemID[1];
                            MatchO = rExp.Match(OutO[Cindex].OldItemId);
                            if (MatchO.Success)
                                OutO[Cindex].OldItemId = OutO[Cindex].OldItemId.Substring(0, MatchO.Index);
                        }
                        //continue;
                    }
                    rExp = currentR.Notes[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.Notes.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Notes[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        } 
                        if (OutO[Cindex].Comments != "")
                            OutO[Cindex].Comments = OutO[Cindex].Comments + " | " + tmp;
                        else OutO[Cindex].Comments = tmp;
                        found = true;
                        OutO[Cindex].Comments = cleanup(OutO[Cindex].Comments);
                        //continue;
                    }
                    rExp = currentR.DOI[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.DOI.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.DOI[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        }
                        if (OutO[Cindex].DOI != "")//there is something here already, what shall we do?
                        {
                            if (OutO[Cindex].DOI.Trim() == tmp.Trim()) { }//they are the same, do nothing
                            else if (OutO[Cindex].DOI.Trim().Length > tmp.Trim().Length
                                        && OutO[Cindex].DOI.Trim().Contains(tmp.Trim())
                                    ) { } //the current contains the new, do nothing again

                            else if (tmp.Trim().Length > OutO[Cindex].DOI.Trim().Length
                                        && tmp.Trim().Contains(OutO[Cindex].DOI.Trim())
                                    ) //the new contains the current, put the new in
                            {
                                OutO[Cindex].DOI = tmp;
                            }
                            else //don't know which one is good: keep both:
                            {
                                OutO[Cindex].DOI = OutO[Cindex].DOI + " | " + tmp;
                            }
                        }
                        else
                        {//there was nothing in, add this one, simples!
                            OutO[Cindex].DOI = tmp;
                        }
                        found = true;
                        OutO[Cindex].DOI = cleanup(OutO[Cindex].DOI);
                        //continue;
                    }
                    rExp = currentR.Keywords[0];
                    MatchO = rExp.Match(fil);
                    if (MatchO.Success & MatchO.Index == 0)
                    {
                        string tmp = fil.Substring(MatchO.Length).Trim();
                        if (currentR.Keywords.Count > 1)
                        {
                            //discard what's not needed
                            rExp = currentR.Keywords[1];
                            MatchO = rExp.Match(tmp);
                            if (MatchO.Success)
                                tmp = tmp.Substring(0, MatchO.Index);
                        }
                        if (OutO[Cindex].Keywords != "")
                            OutO[Cindex].Keywords = OutO[Cindex].Keywords + Environment.NewLine + tmp;
                        else OutO[Cindex].Keywords = tmp;
                        found = true;
                    }
                }
                if (found == true)
                {
                    if (tempAut.Count == 0 & tempPAuth.Count > 0)
                    {
                        OutO[Cindex].AuthorsLi = tempPAuth;
                        OutO[Cindex].pAuthorsLi = tempAut;
                    }
                    else
                    {
                        OutO[Cindex].AuthorsLi = tempAut;
                        OutO[Cindex].pAuthorsLi = tempPAuth;
                    }
                    if (OutO[Cindex].Title.Trim() == "" & OutO[Cindex].Parent_title != "")
                    {
                        OutO[Cindex].Title = OutO[Cindex].Parent_title;
                        OutO[Cindex].Parent_title = "";
                    }
                    Cindex++;
                }
                else
                {
                    OutO.RemoveAt(Cindex);
                }
                
            }            
            return OutO;
        }
        private static string cleanup(string inStr)
        {
            Regex rx = new Regex(@"(\s\s+)|(\r\n)|(\n)");
            return rx.Replace(inStr, " ");
        }
        public static string[] getDate(string inDate)
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
            System.Globalization.DateTimeStyles styles = System.Globalization.DateTimeStyles.None;
            string[] res = new string[2];
            inDate = trimMe(inDate);
            int month, year;
            DateTime dateV = new DateTime(1000, 1, 1);
            Regex RE = new Regex("[^0-9][0-9][0-9](?=[^0-9])|^[0-9][0-9](?=[^0-9])|[^0-9][0-9][0-9]$");

            if (DateTime.TryParse(inDate, out dateV))
            {

                res[0] = Convert.ToString(dateV.Year);
                res[1] = monthstr(dateV.Month);
            }
            else if (DateTime.TryParse(inDate, culture, styles, out dateV))
            {

                res[0] = Convert.ToString(dateV.Year);
                res[1] = monthstr(dateV.Month);
            }

            if ((res[0] != null && res[0].Length > 0) || (res[1] != null && res[1].Length > 0))
            {//the TryParse found something, but we now need to check if it made something up. It will add details to fill in the blanks, using the current date.
                DateTime today = DateTime.Now;
                string chk = "";
                if (res[0].Length > 0)//there is something in the Year slot
                {
                    if (dateV.Year == today.Year)//it's possible that tryparse made this up
                    {
                        chk = today.Year.ToString();
                        if (!inDate.Contains(chk))//the full year does not appear in the original date, not good!
                        {
                            chk = chk.Substring(2);
                            if (!inDate.Contains(chk))//the two digits version of Year does not appear in the original date, certainly not good!
                            {
                                res[0] = "";
                                return res;
                            }
                        }
                    }
                }
            }
            else
            {
                RE = new Regex("[^0-9][0-9][0-9](?=[^0-9])|^[0-9][0-9](?=[^0-9])|[^0-9][0-9][0-9]$");
                MatchCollection ddMatches = RE.Matches(inDate);
                int dDigits = ddMatches.Count;
                RE = new Regex("[^0-9][0-9](?=[^0-9])|^[0-9](?=[^0-9])|[^0-9][0-9]$");
                MatchCollection sdMatches = RE.Matches(inDate);
                int sDigit = sdMatches.Count;

                RE = new Regex("[^0-9][0-9][0-9][0-9][0-9](?=[^0-9])|^[0-9][0-9][0-9][0-9](?=[^0-9])|[^0-9][0-9][0-9][0-9][0-9]$|^[0-9][0-9][0-9][0-9]$");
                MatchCollection qdMatches = RE.Matches(inDate);
                int qDigit = qdMatches.Count;
                RE = new Regex(".*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec).*");
                MatchCollection smMatches = RE.Matches(inDate);
                int sMonth = smMatches.Count;
                if (qDigit == 1 & sDigit == 0 & dDigits == 1 & sMonth == 0)
                {

                    res[0] = qdMatches[0].Value.Substring(qdMatches[0].Value.Length - 4, 4);
                    res[1] = monthstr(Convert.ToInt32(ddMatches[0].Value.Substring(ddMatches[0].Value.Length - 2, 2)));

                }
                else if (qDigit == 1 & sDigit == 1 & dDigits == 0 & sMonth == 0)
                {

                    res[0] = qdMatches[0].Value.Substring(qdMatches[0].Value.Length - 4, 4);
                    res[1] = monthstr(Convert.ToInt32(sdMatches[0].Value.Substring(sdMatches[0].Value.Length - 1, 1)));
                }
                else if (qDigit == 1 & sMonth == 1)
                {

                    res[0] = qdMatches[0].Value.Substring(qdMatches[0].Value.Length - 4, 4);
                    res[1] = monthstr(smMatches[0].Value);
                }
                else if (qDigit == 1 & sDigit == 0 & dDigits == 0 & sMonth == 0)
                {

                    res[0] = qdMatches[0].Value.Substring(qdMatches[0].Value.Length - 4, 4);
                    res[1] = "";
                }
                else if (qDigit == 0 & sDigit == 0 & dDigits == 1 & sMonth == 1)
                {
                    year = Convert.ToInt32(ddMatches[0].Value.Substring(ddMatches[0].Value.Length - 2, 2));
                    if (year < 20)
                        year = 2000 + year;
                    else if (year >= 20)
                        year = 1900 + year;
                    res[0] = Convert.ToString(year);
                    res[1] = monthstr(smMatches[0].Value);
                }
                else if (qDigit == 0 & sDigit == 0 & dDigits == 3 & sMonth == 0)
                {
                    year = Convert.ToInt32(ddMatches[2].Value.Substring(ddMatches[2].Value.Length - 2, 2));
                    if (year < 20)
                        year = 2000 + year;
                    else if (year >= 20)
                        year = 1900 + year;
                    res[0] = Convert.ToString(year);
                    res[1] = monthstr(Convert.ToInt32(ddMatches[1].Value.Substring(ddMatches[1].Value.Length - 2, 2)));
                }
                else if (qDigit == 1 & sDigit == 0 & dDigits == 2 & sMonth == 0)
                {
                    res[0] = qdMatches[0].Value.Substring(qdMatches[0].Value.Length - 4, 4);
                    month = Convert.ToInt32(ddMatches[1].Value.Substring(ddMatches[1].Value.Length - 2, 2));
                    if (month > 12)
                        month = Convert.ToInt32(ddMatches[0].Value.Substring(ddMatches[0].Value.Length - 2, 2));
                    res[1] = monthstr(month);
                }
                else if (qDigit == 0 & sDigit == 0 & dDigits == 1 & sMonth == 0)
                {
                    year = Convert.ToInt32(ddMatches[0].Value.Substring(ddMatches[0].Value.Length - 2, 2));
                    if (year < 20)
                        year = 2000 + year;
                    else if (year >= 20)
                        year = 1900 + year;
                    res[0] = Convert.ToString(year);
                    res[1] = "";
                }
                else res = new string[2] { "0", "0" };
            }


            /*Console.WriteLine("  String '{0}' gave {1} single digit matches.", dateString,
                                          ddMatches.Count);
                    

                    foreach (Match sMatch in ddMatches)
                    {
                        Console.Write(sMatch.Value + " " + sMatch.Index + " - ");
                    }
                    Console.WriteLine("  ------   ");
                     
            Console.WriteLine("Attempting to parse strings using {0} culture.",
                              System.Globalization.CultureInfo.CurrentCulture.Name);
            foreach (string dateString in dateStrings)
            {
                if (DateTime.TryParse(dateString, out dateValue))
                    Console.WriteLine("  Converted '{0}' to {1} ({2}).", dateString,
                                      dateValue, dateValue.Kind);
                else
                    Console.WriteLine("  Unable to parse '{0}'.", dateString);
            }*/
            return res;
        }
        private static string monthstr(string MonIn)
        {
            string result = "";
            MonIn = MonIn.Trim();
            string[] calend = new string[] {"January", "February", "March", "April", "May", "June", "July", 
                "August", "September", "October", "November", "December", "Winter", "Spring", "Summer", "Autunm", "Fall"};
            foreach (string fmonth in calend)
            {
                if (MonIn.ToLower().Contains(fmonth.Substring(0, 3).ToLower()))
                {
                    return fmonth;
                }
            }
            return result;
        }
        private static string monthstr(int MonIn)
        {
            string[] calend = new string[] {"January", "February", "March", "April", "May", "June", "July", 
                "August", "September", "October", "November", "December"};
            if (0 < MonIn & 13 > MonIn) return calend[MonIn - 1];
            else return "";
        }
        private static List<string> splitme(string DataIn, Regex rExp)
        {
            int lastmatch = 0;
            int len = DataIn.Length;
            List<string> splitted = new List<string>();
            Match MatchO = rExp.Match(DataIn);
            while (MatchO.Success)
            {
                if (lastmatch != MatchO.Index)
                    splitted.Add(DataIn.Substring(lastmatch, MatchO.Index - lastmatch));
                lastmatch = MatchO.Index;
                MatchO = MatchO.NextMatch();
            }
            splitted.Add(DataIn.Substring(lastmatch));
            return splitted;
        }
        static string trimMe(string inSt)
        {
            String delim = " ,.*|@/\\+=-_!£$%^&*()`\"";
            return inSt.Trim(delim.ToCharArray());
        }
        public static string StripIllegalChars(string text)
        {//from http://codeclarity.blogspot.com/2009/05/c-strip-invalid-xml-10-characters.html
            //text = text.Replace("ﬁ", "fi"); 
            const string illegalXmlChars = @"[\u0000-\u0008]|[\u000B-\u000C]|[\u000E-\u0019]|[\u007F-\u009F]|[\uD800-\uDBFF]|[\uDC00-\uDFFF]";
            //const string illegalXmlChars = @"[\u0000-\u0008]|[\u000B-\u000C]|[\u000E-\u0019]|[\u007F-\u009F]|[\uD800-\uDBFF]";
            
            //ﬃ ﬁ ﬀ ﬂ ﬄ
            System.Text.RegularExpressions.Regex regex = new Regex("ﬃ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "ffi");
            }
            regex = new Regex("ﬁ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "fi");
            }
            regex = new Regex("ﬀ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "ff");
            }
            regex = new Regex("ﬂ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "fl");
            }
            regex = new Regex("ﬄ");
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, "fll");
            }
            regex = new System.Text.RegularExpressions.Regex(illegalXmlChars, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, " ");
            }
           
            int aiai = text.Length;
            //regex = new Regex(((char)13).ToString());
            //if (regex.IsMatch(text))
            //{
            //    text = regex.Replace(text, "");
            //}
            int returns = text.Split('\r').Length;
            int newlines = text.Split('\n').Length;
            if (returns > 1 && newlines == 1)
            {
                text = text.Replace("\r", System.Environment.NewLine);
            }
            else if (returns == 1 && newlines > 1)
            {
                text = text.Replace("\n", System.Environment.NewLine);
            }
            return text.Trim();
        }
    }
    #endregion
    #region DataStructures
    [Serializable]
    public class ItemIncomingData : ReadOnlyBase<ItemIncomingData>
    {
        #region set_and_get
        public static readonly PropertyInfo<int> Type_idProperty = RegisterProperty<int>(new PropertyInfo<int>("Type_id", "Type_id", 14));
        public int Type_id
        {
            get { return GetProperty(Type_idProperty); }
            set
            {
                if (value > 14 | value < 0) LoadProperty(Type_idProperty, 12);
                else LoadProperty(Type_idProperty, value);
            }
        }
        public static readonly PropertyInfo<string> TitleProperty = RegisterProperty<string>(new PropertyInfo<string>("Title", "Title", string.Empty));
        public string Title
        {
            get { return GetProperty(TitleProperty); }
            set
            {
                if (value.Length > 4000) LoadProperty(TitleProperty, value.Substring(0, 4000));
                else LoadProperty(TitleProperty, value);
            }
        }
        public static readonly PropertyInfo<string> Parent_titleProperty = RegisterProperty<string>(new PropertyInfo<string>("Parent_title", "Parent_title", string.Empty));
        public string Parent_title
        {
            get { return GetProperty(Parent_titleProperty); }
            set
            {
                if (value.Length > 4000) LoadProperty(Parent_titleProperty, value.Substring(0, 4000));
                else LoadProperty(Parent_titleProperty, value);
            }
        }
        public static readonly PropertyInfo<string> Short_titleProperty = RegisterProperty<string>(new PropertyInfo<string>("Short_title", "Short_title", string.Empty));
        public string Short_title
        {
            get { return GetProperty(Short_titleProperty); }
            set
            {
                if (value.Length > 70)
                {

                    LoadProperty(Short_titleProperty, value.Substring(0, value.Substring(0, 66).LastIndexOf(' ') + 1) + "...");
                }
                else LoadProperty(Short_titleProperty, value);
            }
        }
        public static readonly PropertyInfo<AuthorsHandling.AutorsList> AuthorsLiProperty = RegisterProperty<AuthorsHandling.AutorsList>(new PropertyInfo<AuthorsHandling.AutorsList>("AuthorsLi", "AuthorsLi"));
        public AuthorsHandling.AutorsList AuthorsLi
        {
            get { return GetProperty(AuthorsLiProperty); }
            set { LoadProperty(AuthorsLiProperty, value); }
        }
        public static readonly PropertyInfo<MobileList<AuthorsHandling.AutH>> pAuthorsLiProperty = RegisterProperty<MobileList<AuthorsHandling.AutH>>(new PropertyInfo<MobileList<AuthorsHandling.AutH>>("pAuthorsLi", "pAuthorsLi"));
        public MobileList<AuthorsHandling.AutH> pAuthorsLi
        {
            get { return GetProperty(pAuthorsLiProperty); }
            set { LoadProperty(pAuthorsLiProperty, value); }
        }
        public static readonly PropertyInfo<string> YearProperty = RegisterProperty<string>(new PropertyInfo<string>("Year", "Year", string.Empty));
        public string Year
        {
            get { return GetProperty(YearProperty); }
            set
            {
                if (
                        value.Trim().Length == 0 ||
                        value.Trim().Length > 4 ||
                        Convert.ToInt16(value) > System.DateTime.Now.Year ||
                        Convert.ToInt16(value) <= 0
                    )
                    LoadProperty(YearProperty, "");
                else LoadProperty(YearProperty, value);
            }
        }
        public static readonly PropertyInfo<string> MonthProperty = RegisterProperty<string>(new PropertyInfo<string>("Month", "Month", string.Empty));
        public string Month
        {
            get { return GetProperty(MonthProperty); }
            set
            {
                if (value.Length > 10)
                {
                    LoadProperty(MonthProperty, value.Substring(0, 10));
                }
                else if (value == "0")
                {
                    LoadProperty(MonthProperty, "");
                }
                else LoadProperty(MonthProperty, value);
            }
        }
        public static readonly PropertyInfo<string> Standard_numberProperty = RegisterProperty<string>(new PropertyInfo<string>("Standard_number", "Standard_number", string.Empty));
        public string Standard_number
        {
            get { return GetProperty(Standard_numberProperty); }
            set
            {
                if (value.Length > 255) LoadProperty(Standard_numberProperty, value.Substring(0, 255));
                else LoadProperty(Standard_numberProperty, value);
            }
        }
        public static readonly PropertyInfo<string> CityProperty = RegisterProperty<string>(new PropertyInfo<string>("City", "City", string.Empty));
        public string City
        {
            get { return GetProperty(CityProperty); }
            set
            {
                if (value.Length > 100) LoadProperty(CityProperty, value.Substring(0, 100));
                else LoadProperty(CityProperty, value);
            }
        }
        public static readonly PropertyInfo<string> CountryProperty = RegisterProperty<string>(new PropertyInfo<string>("Country", "Country", string.Empty));
        public string Country
        {
            get { return GetProperty(CountryProperty); }
            set
            {
                if (value.Length > 100) LoadProperty(CountryProperty, value.Substring(0, 100));
                else LoadProperty(CountryProperty, value);
            }
        }
        public static readonly PropertyInfo<string> PublisherProperty = RegisterProperty<string>(new PropertyInfo<string>("Publisher", "Publisher", string.Empty));
        public string Publisher
        {
            get { return GetProperty(PublisherProperty); }
            set
            {
                if (value.Length > 1000) LoadProperty(PublisherProperty, value.Substring(0, 1000));
                else LoadProperty(PublisherProperty, value);
            }
        }
        public static readonly PropertyInfo<string> InstitutionProperty = RegisterProperty<string>(new PropertyInfo<string>("Institution", "Institution", string.Empty));
        public string Institution
        {
            get { return GetProperty(InstitutionProperty); }
            set
            {
                if (value.Length > 1000) LoadProperty(InstitutionProperty, value.Substring(0, 1000));
                else LoadProperty(InstitutionProperty, value);
            }
        }
        public static readonly PropertyInfo<string> VolumeProperty = RegisterProperty<string>(new PropertyInfo<string>("Volume", "Volume", string.Empty));
        public string Volume
        {
            get { return GetProperty(VolumeProperty); }
            set
            {
                if (value.Length > 56) LoadProperty(VolumeProperty, value.Substring(0, 56));
                else LoadProperty(VolumeProperty, value);
            }
        }
        public static readonly PropertyInfo<string> PagesProperty = RegisterProperty<string>(new PropertyInfo<string>("Pages", "Pages", string.Empty));
        public string Pages
        {
            get { return GetProperty(PagesProperty); }
            set
            {
                if (value.Length > 50) LoadProperty(PagesProperty, value.Substring(0, 50));
                else LoadProperty(PagesProperty, value);
            }
        }
        public static readonly PropertyInfo<string> EditionProperty = RegisterProperty<string>(new PropertyInfo<string>("Edition", "Edition", string.Empty));
        public string Edition
        {
            get { return GetProperty(EditionProperty); }
            set
            {
                if (value.Length > 200) LoadProperty(EditionProperty, value.Substring(0, 200));
                else LoadProperty(EditionProperty, value);
            }
        }
        public static readonly PropertyInfo<string> IssueProperty = RegisterProperty<string>(new PropertyInfo<string>("Issue", "Issue", string.Empty));
        public string Issue
        {
            get { return GetProperty(IssueProperty); }
            set
            {
                if (value.Length > 100) LoadProperty(IssueProperty, value.Substring(0, 100));
                else LoadProperty(IssueProperty, value);
            }
        }
        public static readonly PropertyInfo<string> AvailabilityProperty = RegisterProperty<string>(new PropertyInfo<string>("Availability", "Availability", string.Empty));
        public string Availability
        {
            get { return GetProperty(AvailabilityProperty); }
            set
            {
                if (value.Length > 255) LoadProperty(AvailabilityProperty, value.Substring(0, 255));
                else LoadProperty(AvailabilityProperty, value);
            }
        }
        public static readonly PropertyInfo<string> UrlProperty = RegisterProperty<string>(new PropertyInfo<string>("Url", "Url", string.Empty));
        public string Url
        {
            get { return GetProperty(UrlProperty); }
            set
            {
                if (value.Length > 500) LoadProperty(UrlProperty, value.Substring(0, 500));
                else LoadProperty(UrlProperty, value);
            }
        }
        public static readonly PropertyInfo<string> AbstractProperty = RegisterProperty<string>(new PropertyInfo<string>("Abstract", "Abstract", string.Empty));
        public string Abstract
        {
            get
            {
                return GetProperty(AbstractProperty);
            }
            set
            {
                LoadProperty(AbstractProperty, value);
            }
        }
        public static readonly PropertyInfo<string> CommentsProperty = RegisterProperty<string>(new PropertyInfo<string>("Comments", "Comments", string.Empty));
        public string Comments
        {
            get { return GetProperty(CommentsProperty); }
            set { LoadProperty(CommentsProperty, value); }
        }
        public static readonly PropertyInfo<string> OldItemIdProperty = RegisterProperty<string>(new PropertyInfo<string>("OldItemId", "OldItemId", string.Empty));
        public string OldItemId
        {
            get { return GetProperty(OldItemIdProperty); }
            set
            {
                if (value.Length > 50) LoadProperty(OldItemIdProperty, value.Substring(0, 50));
                else LoadProperty(OldItemIdProperty, value);
            }
        }
        public static readonly PropertyInfo<string> DOIProperty = RegisterProperty<string>(new PropertyInfo<string>("DOI", "DOI", string.Empty));
        public string DOI
        {
            get { return GetProperty(DOIProperty); }
            set
            {
                if (value.Length > 500) LoadProperty(DOIProperty, value.Substring(0, 500));
                else LoadProperty(DOIProperty, value);
            }
        }
        public static readonly PropertyInfo<string> KeywordsProperty = RegisterProperty<string>(new PropertyInfo<string>("Keywords", "Keywords", string.Empty));
        public string Keywords
        {
            get { return GetProperty(KeywordsProperty); }
            set
            {
                LoadProperty(KeywordsProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SearchTextProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchText", "SearchText", string.Empty));
        public string SearchText
        {
            get { return GetProperty(SearchTextProperty); }
            set
            {
                if (value.Length > 500) LoadProperty(SearchTextProperty, value.Substring(0, 500));
                else LoadProperty(SearchTextProperty, value);
            }
        }

        //keeping MAG-only fields on server-side, while comms with MAG happen here...
#if !SILVERLIGHT
        public static readonly PropertyInfo<Double> MAGMatchScoreProperty = RegisterProperty<Double>(new PropertyInfo<Double>("MAGMatchScore", "MAGMatchScore", 1.0));
        public Double MAGMatchScore
        {
            get { return GetProperty(MAGMatchScoreProperty); }
            set
            {
                LoadProperty(MAGMatchScoreProperty, value);
            }
        }

        public static readonly PropertyInfo<bool> MAGManualTrueMatchProperty = RegisterProperty<bool>(new PropertyInfo<bool>("MAGManualTrueMatch", "MAGManualTrueMatch", false));
        public bool MAGManualTrueMatch
        {
            get { return GetProperty(MAGManualTrueMatchProperty); }
            set
            {
                LoadProperty(MAGManualTrueMatchProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> MAGManualFalseMatchProperty = RegisterProperty<bool>(new PropertyInfo<bool>("MAGManualFalseMatch", "MAGManualFalseMatch", false));
        public bool MAGManualFalseMatch
        {
            get { return GetProperty(MAGManualFalseMatchProperty); }
            set
            {
                LoadProperty(MAGManualFalseMatchProperty, value);
            }
        }
#endif
#endregion

        #region constructors
        internal static ItemIncomingData NewItem()
        {
            ItemIncomingData returnValue = new ItemIncomingData();
            //returnValue.ValidationRules.CheckRules();
            return returnValue;
        }
        public ItemIncomingData(){}
        public void buildShortTitle()
        {
            if (Short_title != null && Short_title != "") return ;
            if (AuthorsLi != null && AuthorsLi.Count > 0 )
            {
                if (Year!= null && Year != "")
                {
                    Short_title = AuthorsLi[0].LastName + " (" + Year + ")";
                    return;
                }
                Short_title = cutLongTitle() + " (" + AuthorsLi[0].LastName + ")";
                return;
            }
            if (Year != null && Year != "")
            {
                Short_title = cutLongTitle() + " (" + Year + ")";
                return;
            }
            Short_title = cutLongTitle();
        }
        private string cutLongTitle()
        {
            //CASE
            //    WHEN (len(title) > 20 and 55 > CHARINDEX(' ', title, 19) and CHARINDEX(' ', title, 19) > 0) THEN
            //        SUBSTRING(title, 0, CHARINDEX(' ', title, 19) ) + '...' 
            //    WHEN (len(title) > 20 and 55 <= CHARINDEX(' ', title, 19)) THEN
            //        SUBSTRING(title, 0, CHARINDEX(' ', title, 1)) +'...'
            //    else title  
            //end
            if (Title == null || Title == "") return "{Missing Title}";
            if (Title.Length <20) return Title;
            int usefulSp = Title.IndexOf(' ', 19);
            if (Title.Length > 20 & usefulSp < 55 & usefulSp > 0)
                return Title.Substring(0, usefulSp) + "...";
            else
            {
                int mke = Title.Length > 49 ? 50 : Title.Length - 1;
                usefulSp = Title.LastIndexOf(' ', mke);
                if (usefulSp != -1)
                    return Title.Substring(0, usefulSp) + "...";
                usefulSp = Title.LastIndexOfAny(new char[] { '-', '_', '\\', '/', ',', '.', ':', ';', '?', '\'', '!', '"', '£', '$', '%', '^', '&', '*', '`', '|', '¦', '+', '=' }
                    ,mke);
                if (usefulSp != -1)
                    return Title.Substring(0, usefulSp);
                return Title.Substring(0, 20)+ "...";
            }
            
        }
        #endregion
    }
    [Serializable]
    public class IncomingItemsList : BusinessBase<IncomingItemsList>
    {
        public IncomingItemsList(){}
#if !SILVERLIGHT
        private bool savedAll = false;
#endif
        internal static IncomingItemsList NewIncomingItemsList()
        {
            IncomingItemsList returnValue = new IncomingItemsList();
            returnValue.DateOfImport = DateTime.Now;
            returnValue.DateOfSearch = DateTime.Now;
            returnValue.Included = true;
            return returnValue;
        }

        
        
        public static readonly PropertyInfo<MobileList<ItemIncomingData>> IncomingItemsProperty = RegisterProperty<MobileList<ItemIncomingData>>(new PropertyInfo<MobileList<ItemIncomingData>>("IncomingItems", "IncomingItems"));
        public MobileList<ItemIncomingData> IncomingItems
        {
            get
            {
                return GetProperty(IncomingItemsProperty);
            }
            set
            {
                SetProperty(IncomingItemsProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SourceNameProperty = RegisterProperty<string>(new PropertyInfo<string>("SourceName", "SourceName"));
        public string SourceName
        {
            get
            {
                return GetProperty(SourceNameProperty);
            }
            set
            {
                if (value.Length > 255) SetProperty(SourceNameProperty, value.Substring(0, 255));
                else SetProperty(SourceNameProperty, value);
            }
        }
        public static readonly PropertyInfo<int> FilterIDProperty = RegisterProperty<int>(new PropertyInfo<int>("FilterID", "FilterID"));
        public int FilterID
        {
            get
            {
                return GetProperty(FilterIDProperty);
            }
            set
            {
                SetProperty(FilterIDProperty, value);
            }
        }
        public static readonly PropertyInfo<int> SourceIDProperty = RegisterProperty<int>(new PropertyInfo<int>("SourceID", "SourceID"));
        public int SourceID
        {
            get
            {
                return GetProperty(SourceIDProperty);
            }
            set
            {
                SetProperty(SourceIDProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SourceDBProperty = RegisterProperty<string>(new PropertyInfo<string>("SourceDB", "SourceDB"));
        public string SourceDB
        {
            get
            {
                return GetProperty(SourceDBProperty);
            }
            set
            {
                if (value.Length > 200) SetProperty(SourceDBProperty, value.Substring(0, 200));
                else SetProperty(SourceDBProperty, value);
                
            }
        }
        public static readonly PropertyInfo<string> SearchDescrProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchDescr", "SearchDescr"));
        public string SearchDescr
        {
            get
            {
                return GetProperty(SearchDescrProperty);
            }
            set
            {
                if (value.Length > 4000) SetProperty(SearchDescrProperty, value.Substring(0, 4000));
                else SetProperty(SearchDescrProperty, value);
            }
        }
        public static readonly PropertyInfo<string> SearchStrProperty = RegisterProperty<string>(new PropertyInfo<string>("SearchStr", "SearchStr"));
        public string SearchStr
        {
            get
            {
                return GetProperty(SearchStrProperty);
            }
            set
            {
                SetProperty(SearchStrProperty, value);
            }
        }
        public static readonly PropertyInfo<string> NotesProperty = RegisterProperty<string>(new PropertyInfo<string>("NotesStr", "NotesStr"));
        public string Notes
        {
            get
            {
                return GetProperty(NotesProperty);
            }
            set
            {
                if (value.Length > 4000) SetProperty(NotesProperty, value.Substring(0, 4000));
                else SetProperty(NotesProperty, value);
            }
        }
        public static readonly PropertyInfo<DateTime> DateOfSearchProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("DateOfSearch", "DateOfSearch"));
        public DateTime DateOfSearch
        {
            get
            {
                return GetProperty(DateOfSearchProperty);
            }
            set
            {
                SetProperty(DateOfSearchProperty, value);
            }
        }
        public static readonly PropertyInfo<DateTime> DateOfImportProperty = RegisterProperty<DateTime>(new PropertyInfo<DateTime>("DateOfImport", "DateOfImport"));
        public DateTime DateOfImport
        {
            get
            {
                return GetProperty(DateOfImportProperty);
            }
            set
            {
                SetProperty(DateOfImportProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> IncludedProperty = RegisterProperty<bool>(new PropertyInfo<bool>("Included", "Included"));
        public bool Included
        {
            get
            {
                return GetProperty(IncludedProperty);
            }
            set
            {
                SetProperty(IncludedProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> IsLastProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsLast", "IsLast", true));
        public bool IsLast
        {
            get
            {
                return GetProperty(IsLastProperty);
            }
            set
            {
                SetProperty(IsLastProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> IsFirstProperty = RegisterProperty<bool>(new PropertyInfo<bool>("IsFirst", "IsFirst", true));
        public bool IsFirst
        {
            get
            {
                return GetProperty(IsFirstProperty);
            }
            set
            {
                SetProperty(IsFirstProperty, value);
            }
        }
        public static readonly PropertyInfo<bool> HasMAGScoresProperty = RegisterProperty<bool>(new PropertyInfo<bool>("HasMAGScores", "HasMAGScores", false));
        public bool HasMAGScores
        {
            get
            {
                return GetProperty(HasMAGScoresProperty);
            }
            set
            {
                SetProperty(HasMAGScoresProperty, value);
            }
        }

        public static readonly PropertyInfo<string> ZoteroKeyProperty = RegisterProperty<string>(new PropertyInfo<string>("ZoteroKey", "ZoteroKey", ""));
        public string ZoteroKey
        {
            get
            {
                return GetProperty(ZoteroKeyProperty);
            }
            set
            {
                SetProperty(ZoteroKeyProperty, value);
            }
        }
        //protected override void AddAuthorizationRules()
        //{
        //    //string[] canWrite = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] canRead = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //string[] admin = new string[] { "AdminUser", "RegularUser", "ReadOnlyUser" };
        //    //AuthorizationRules.AllowCreate(typeof(IncomingItemsList), admin);
        //    //AuthorizationRules.AllowDelete(typeof(IncomingItemsList), admin);
        //    //AuthorizationRules.AllowEdit(typeof(IncomingItemsList), canWrite);
        //    //AuthorizationRules.AllowGet(typeof(IncomingItemsList), canRead);
        //    //AuthorizationRules.AllowRead(IncomingItemsProperty, canRead);
        //    //AuthorizationRules.AllowWrite(IncomingItemsProperty, canWrite);
        //}

        protected override void AddBusinessRules()
        {
            //ValidationRules.AddRule(Csla.Validation.CommonRules.MaxValue<decimal>, new Csla.Validation.CommonRules.MaxValueRuleArgs<decimal>(ReviewCodeSetFte1Property, 1));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringRequired, new Csla.Validation.RuleArgs(ReviewCodeSetNameProperty));
            //ValidationRules.AddRule(Csla.Validation.CommonRules.StringMaxLength, new Csla.Validation.CommonRules.MaxLengthRuleArgs(ReviewCodeSetNameProperty, 20));
        }
        #region serverside
#if!SILVERLIGHT
        protected override void DataPortal_Insert()
        {
            savedAll = false;
            int Source_S = 0;
            ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
            int review_ID = ri.ReviewId, AuthorsN = 0;
            if (review_ID == 0) return;
            //declare variables
            Int64 Items_S = 0;
            Int64 Author_S = 0;
            Int64 Item_Source_S = 0;
            Int64 Item_Review_S = 0;
            //BusinessLibrary.BusinessClasses.TestDataSet TDS = new BusinessLibrary.BusinessClasses.TestDataSet();
            Data.ImportItemsDataset TDS = new Data.ImportItemsDataset();
            foreach (ItemIncomingData item in IncomingItems)
            {
                AuthorsN = AuthorsN + item.AuthorsLi.Count + item.pAuthorsLi.Count;
            }
            DataRow ItemR;
            DataRow AuthorR;
            DataRow ItemSourceR;
            DataRow SourceR;
            DataRow ItemReviewR;
            
            if (IsFirst && IsLast) //old method save all in one go
            {
                SeedTables(TDS, AuthorsN, Items_S, Author_S, Item_Source_S, Item_Review_S, ref Source_S);
                //start putting data is: declare rows
                //BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEMRow ItemR;
                //BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEM_AUTHORRow AuthorR;
                //BusinessLibrary.BusinessClasses.TestDataSet.TB_ITEM_SOURCERow ItemSourceR;
                //BusinessLibrary.BusinessClasses.TestDataSet.TB_SOURCERow SourceR;
                //BusinessLibrary.BusinessClasses.TestDataSet.TB_ITEM_REVIEWRow ItemReviewR;

                
                //put data in the source table
                

                //SourceR = (BusinessLibrary.BusinessClasses.TestDataSet.TB_SOURCERow)TDS.TB_SOURCE.NewRow();
                SourceR = TDS.TB_SOURCE.NewRow();
                FillSourceTB(TDS, SourceR, review_ID);
                
                //put data in the item tables, tb_item, tb_item_review, tb_item_source and TB_ITEM_AUTHOR
                foreach (ItemIncomingData item in IncomingItems)
                {
                    //ItemR = (BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEMRow)TDS.tb_ITEM.NewRow();
                    ItemR = TDS.TB_ITEM.NewRow();
                    FillItemTB(TDS, ItemR, item, review_ID, ri);
                    foreach (AutH Auth in item.AuthorsLi)
                    {
                        //AuthorR = (BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEM_AUTHORRow)TDS.tb_ITEM_AUTHOR.NewRow();
                        AuthorR = TDS.TB_ITEM_AUTHOR.NewRow();
                        fillAuthorsTB(TDS, AuthorR, Auth, ItemR["ITEM_ID"]);
                    }
                    foreach (AutH Auth in item.pAuthorsLi)
                    {
                        AuthorR = TDS.TB_ITEM_AUTHOR.NewRow();
                        fillAuthorsTB(TDS, AuthorR, Auth, ItemR["ITEM_ID"]);
                    }
                    //ItemReviewR = (BusinessLibrary.BusinessClasses.TestDataSet.TB_ITEM_REVIEWRow)TDS.TB_ITEM_REVIEW.NewRow();
                    ItemReviewR = TDS.TB_ITEM_REVIEW.NewRow();
                    //ItemReviewR.ITEM_ID = ItemR.ITEM_ID;
                    //ItemReviewR.REVIEW_ID = review_ID;
                    //ItemReviewR.IS_DELETED = false;
                    //ItemReviewR.IS_INCLUDED = Included;
                    ItemReviewR["ITEM_ID"] = ItemR["ITEM_ID"];
                    ItemReviewR["REVIEW_ID"] = review_ID;
                    ItemReviewR["IS_DELETED"] = false;
                    ItemReviewR["IS_INCLUDED"] = Included;
                    TDS.TB_ITEM_REVIEW.Rows.Add(ItemReviewR);

                    //ItemSourceR = (BusinessLibrary.BusinessClasses.TestDataSet.TB_ITEM_SOURCERow)TDS.TB_ITEM_SOURCE.NewRow();
                    ItemSourceR = TDS.TB_ITEM_SOURCE.NewRow();
                    //ItemSourceR.ITEM_ID = ItemR.ITEM_ID;
                    //ItemSourceR.SOURCE_ID = SourceR.SOURCE_ID;
                    ItemSourceR["ITEM_ID"] = ItemR["ITEM_ID"];
                    ItemSourceR["SOURCE_ID"] = SourceR["SOURCE_ID"];
                    TDS.TB_ITEM_SOURCE.Rows.Add(ItemSourceR);
                    int cippa = TDS.TB_ITEM_SOURCE.Rows.Count;
                }
                BulkUpload(TDS, Source_S);
            }
            else if (IsFirst && !IsLast)//this is the first batch of a series
            {
                //prepare tb_source, tb_item, tb_item_source and tb_item_authors
                SeedTables(TDS, AuthorsN, Items_S, Author_S, Item_Source_S, Item_Review_S, ref Source_S);

                //start putting data is: declare rows
                //BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEMRow ItemR;
                //BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEM_AUTHORRow AuthorR;
                //BusinessLibrary.BusinessClasses.TestDataSet.TB_ITEM_SOURCERow ItemSourceR;
                //BusinessLibrary.BusinessClasses.TestDataSet.TB_SOURCERow SourceR;

                //put data in the source table
                SourceR = TDS.TB_SOURCE.NewRow();
                FillSourceTB(TDS, SourceR, review_ID);
               
                //put data in the item tables, tb_item, tb_item_source and TB_ITEM_AUTHOR
                foreach (ItemIncomingData item in IncomingItems)
                {
                    //ItemR = (BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEMRow)TDS.tb_ITEM.NewRow();
                    ItemR = TDS.TB_ITEM.NewRow();
                    FillItemTB(TDS, ItemR, item, review_ID, ri);
                    foreach (AutH Auth in item.AuthorsLi)
                    {
                        //AuthorR = (BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEM_AUTHORRow)TDS.tb_ITEM_AUTHOR.NewRow();
                        AuthorR = TDS.TB_ITEM_AUTHOR.NewRow();
                        fillAuthorsTB(TDS, AuthorR, Auth, ItemR["ITEM_ID"]);
                    }
                    foreach (AutH Auth in item.pAuthorsLi)
                    {
                        //AuthorR = (BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEM_AUTHORRow)TDS.tb_ITEM_AUTHOR.NewRow();
                        AuthorR = TDS.TB_ITEM_AUTHOR.NewRow();
                        fillAuthorsTB(TDS, AuthorR, Auth, ItemR["ITEM_ID"]);
                    }

                    //ItemSourceR = (BusinessLibrary.BusinessClasses.TestDataSet.TB_ITEM_SOURCERow)TDS.TB_ITEM_SOURCE.NewRow();
                    ItemSourceR = TDS.TB_ITEM_SOURCE.NewRow();
                    //ItemSourceR.ITEM_ID = ItemR.ITEM_ID;
                    //ItemSourceR.SOURCE_ID = SourceR.SOURCE_ID;
                    ItemSourceR["ITEM_ID"] = ItemR["ITEM_ID"];
                    ItemSourceR["SOURCE_ID"] = SourceR["SOURCE_ID"];
                    TDS.TB_ITEM_SOURCE.Rows.Add(ItemSourceR);
                    int cippa = TDS.TB_ITEM_SOURCE.Rows.Count;

                }
                BulkUpload(TDS, Source_S);
                
            }
            else //this is one of a batch but not the first 
            {
                //prepare tb_item, tb_item_source and tb_item_authors

                //add lines to local tables,

                //bulk add into tb_item, tb_item_source and tb_item_authors
                SeedTables(TDS, AuthorsN, Items_S, Author_S, Item_Source_S, Item_Review_S, ref Source_S);
                //start putting data is: declare rows
                //BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEMRow ItemR;
                //BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEM_AUTHORRow AuthorR;
                //BusinessLibrary.BusinessClasses.TestDataSet.TB_ITEM_SOURCERow ItemSourceR;
                
                //put data in the item tables, tb_item, tb_item_source and TB_ITEM_AUTHOR
                foreach (ItemIncomingData item in IncomingItems)
                {
                    //ItemR = (BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEMRow)TDS.tb_ITEM.NewRow();
                    ItemR = TDS.TB_ITEM.NewRow();
                    FillItemTB(TDS, ItemR, item, review_ID, ri);
                    foreach (AutH Auth in item.AuthorsLi)
                    {
                        //AuthorR = (BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEM_AUTHORRow)TDS.tb_ITEM_AUTHOR.NewRow();
                        AuthorR = TDS.TB_ITEM_AUTHOR.NewRow();
                        fillAuthorsTB(TDS, AuthorR, Auth, ItemR["ITEM_ID"]);
                    }
                    foreach (AutH Auth in item.pAuthorsLi)
                    {
                        //AuthorR = (BusinessLibrary.BusinessClasses.TestDataSet.tb_ITEM_AUTHORRow)TDS.tb_ITEM_AUTHOR.NewRow();
                        AuthorR = TDS.TB_ITEM_AUTHOR.NewRow();
                        fillAuthorsTB(TDS, AuthorR, Auth, ItemR["ITEM_ID"]);
                    }
                    //ItemSourceR = (BusinessLibrary.BusinessClasses.TestDataSet.TB_ITEM_SOURCERow)TDS.TB_ITEM_SOURCE.NewRow();
                    //ItemSourceR.ITEM_ID = ItemR.ITEM_ID;
                    //ItemSourceR.SOURCE_ID = SourceID;
                    //TDS.TB_ITEM_SOURCE.Rows.Add(ItemSourceR);
                    ItemSourceR = TDS.TB_ITEM_SOURCE.NewRow();
                    ItemSourceR["ITEM_ID"] = ItemR["ITEM_ID"];
                    ItemSourceR["SOURCE_ID"] = SourceID;
                    TDS.TB_ITEM_SOURCE.Rows.Add(ItemSourceR);
                    int cippa = TDS.TB_ITEM_SOURCE.Rows.Count;
                }
                BulkUpload(TDS, Source_S);
                if (IsLast)//this is the last in the batch, create lines in tb_item_review
                {
                    using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand cmd = new SqlCommand("st_SourceAddToReview", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@Source_ID", SourceID));
                            cmd.Parameters.Add(new SqlParameter("@Review_ID", ri.ReviewId));
                            cmd.Parameters.Add(new SqlParameter("@Included", Included == true ? 1 : 0));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private void SeedTables(Data.ImportItemsDataset TDS, int AuthorsN
            , long Items_S, long Author_S, long Item_Source_S, long Item_Review_S, ref int Source_S)
        {
            //there are three cases:
            //1. if (IsFirst && IsLast): all 5 tables are filled in one go.
            //2. if (IsFirst && !IsLast): first batch in a series. Only TB_ITEM_REVIEW is ignored.
            //3. ELSE: a batch in a series, but not the first one: source record already exist, Ignore TB_SOURCE and TB_ITEM_REVIEW
            //in this last case, a SP will create the records in tb_ITEM_REVIEW after uploading the data (based on SOURCE).
            using (SqlConnection connection = new SqlConnection(DataConnection.ConnectionString))
            {
                string SPname = "";
                if (IsFirst && IsLast) SPname = "st_ItemImportPrepare";
                else SPname = "st_ItemImportPrepareBatch";
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(SPname, connection))
                {
                    //prepare all tables
                    cmd.Parameters.Add("@Source_Seed", SqlDbType.Int);
                    cmd.Parameters["@Source_Seed"].Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Items_Number", IncomingItems.Count));
                    cmd.Parameters.Add(new SqlParameter("@Authors_Number", AuthorsN));
                    if (SPname == "st_ItemImportPrepareBatch") cmd.Parameters.Add(new SqlParameter("@Source_Id", SourceID));//if 0 is a new source, will get a source seed such cases
                    cmd.Parameters.Add("@Item_Seed", SqlDbType.BigInt);
                    cmd.Parameters["@Item_Seed"].Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Author_Seed", SqlDbType.BigInt);
                    cmd.Parameters["@Author_Seed"].Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Item_Source_Seed", SqlDbType.BigInt);
                    cmd.Parameters["@Item_Source_Seed"].Direction = ParameterDirection.Output;
                    if (SPname == "st_ItemImportPrepare")
                    {//TB_ITEM_Review gets filled in this way only if import is done all in one go.
                        cmd.Parameters.Add("@Item_Review_Seed", SqlDbType.BigInt);
                        cmd.Parameters["@Item_Review_Seed"].Direction = ParameterDirection.Output;
                    }
                    cmd.ExecuteNonQuery();

                    //get seeds values
                    Items_S = (Int64)cmd.Parameters["@Item_Seed"].Value;
                    Author_S = (Int64)cmd.Parameters["@Author_Seed"].Value;
                    Item_Source_S = (Int64)cmd.Parameters["@Item_Source_Seed"].Value;
                    if (SPname == "st_ItemImportPrepare")
                    {//TB_ITEM_Review gets filled in this way only if import is done all in one go.
                        Item_Review_S = (Int64)cmd.Parameters["@Item_Review_Seed"].Value;
                    }
                    if (SourceID == 0 & IsNew == true) Source_S = (int)cmd.Parameters["@Source_Seed"].Value;//check this!
                }
            }
            //set local tables and seeds
            //TDS.tb_ITEM.ITEM_IDColumn.AutoIncrement = true;
            //TDS.tb_ITEM.ITEM_IDColumn.AutoIncrementSeed = Items_S + 1;
            //TDS.tb_ITEM.ITEM_IDColumn.AutoIncrementStep = 1;
            //TB_ITEM, TB_ITEM_SOURCE & TB_ITEM_AUTHOR are filled in all 3 cases.
            TDS.TB_ITEM.Columns["ITEM_ID"].AutoIncrement = true;
            TDS.TB_ITEM.Columns["ITEM_ID"].AutoIncrementSeed = Items_S + 1;
            TDS.TB_ITEM.Columns["ITEM_ID"].AutoIncrementStep = 1;
            //TDS.TB_ITEM_SOURCE.ITEM_SOURCE_IDColumn.AutoIncrement = true;
            //TDS.TB_ITEM_SOURCE.ITEM_SOURCE_IDColumn.AutoIncrementSeed = Item_Source_S + 1;
            //TDS.TB_ITEM_SOURCE.ITEM_SOURCE_IDColumn.AutoIncrementStep = 1;
            TDS.TB_ITEM_SOURCE.Columns["ITEM_SOURCE_ID"].AutoIncrement = true;
            TDS.TB_ITEM_SOURCE.Columns["ITEM_SOURCE_ID"].AutoIncrementSeed = Item_Source_S + 1;
            TDS.TB_ITEM_SOURCE.Columns["ITEM_SOURCE_ID"].AutoIncrementStep = 1;
            //TDS.tb_ITEM_AUTHOR.ITEM_AUTHOR_IDColumn.AutoIncrement = true;
            //TDS.tb_ITEM_AUTHOR.ITEM_AUTHOR_IDColumn.AutoIncrementSeed = Author_S + 1;
            //TDS.tb_ITEM_AUTHOR.ITEM_AUTHOR_IDColumn.AutoIncrementStep = 1;
            TDS.TB_ITEM_AUTHOR.Columns["ITEM_AUTHOR_ID"].AutoIncrement = true;
            TDS.TB_ITEM_AUTHOR.Columns["ITEM_AUTHOR_ID"].AutoIncrementSeed = Author_S + 1;
            TDS.TB_ITEM_AUTHOR.Columns["ITEM_AUTHOR_ID"].AutoIncrementStep = 1;
            if (SourceID == 0 & IsNew == true)//either all in one go, OR first batch in a series.
            {
                //TDS.TB_SOURCE.SOURCE_IDColumn.AutoIncrement = true;
                //TDS.TB_SOURCE.SOURCE_IDColumn.AutoIncrementSeed = Source_S + 1;
                //TDS.TB_SOURCE.SOURCE_IDColumn.AutoIncrementStep = 1;
                TDS.TB_SOURCE.Columns["SOURCE_ID"].AutoIncrement = true;
                TDS.TB_SOURCE.Columns["SOURCE_ID"].AutoIncrementSeed = Source_S + 1;
                TDS.TB_SOURCE.Columns["SOURCE_ID"].AutoIncrementStep = 1;
                
            }
            if (IsFirst && IsLast)
            {//ALL in one go, do also TB_ITEM_REVIEW
                //TDS.TB_ITEM_REVIEW.ITEM_REVIEW_IDColumn.AutoIncrement = true;
                //TDS.TB_ITEM_REVIEW.ITEM_REVIEW_IDColumn.AutoIncrementSeed = Item_Review_S + 1;
                //TDS.TB_ITEM_REVIEW.ITEM_REVIEW_IDColumn.AutoIncrementStep = 1;
                TDS.TB_ITEM_REVIEW.Columns["ITEM_REVIEW_ID"].AutoIncrement = true;
                TDS.TB_ITEM_REVIEW.Columns["ITEM_REVIEW_ID"].AutoIncrementSeed = Item_Review_S + 1;
                TDS.TB_ITEM_REVIEW.Columns["ITEM_REVIEW_ID"].AutoIncrementStep = 1;
            }
        }

        private void FillSourceTB(Data.ImportItemsDataset TDS, DataRow SourceR, int review_ID)
        {
            //SourceR = TDS.TB_SOURCE.NewRow();
            //SourceR.REVIEW_ID = review_ID;
            //SourceR.SOURCE_NAME = SourceName;
            //SourceR.DATE_OF_IMPORT = DateOfImport;
            //SourceR.DATE_OF_SEARCH = DateOfSearch;
            //SourceR.SOURCE_DATABASE = SourceDB;
            //SourceR.SEARCH_DESCRIPTION = SearchDescr;
            //SourceR.SEARCH_STRING = SearchStr;
            //SourceR.NOTES = Notes;
            //SourceR.IMPORT_FILTER_ID = FilterID;
            SourceR["REVIEW_ID"] = review_ID;
            SourceR["SOURCE_NAME"] = SourceName;
            SourceR["DATE_OF_IMPORT"] = DateOfImport;
            SourceR["DATE_OF_SEARCH"] = DateOfSearch;
            SourceR["SOURCE_DATABASE"] = SourceDB;
            SourceR["SEARCH_DESCRIPTION"] = SearchDescr;
            SourceR["SEARCH_STRING"] = SearchStr;
            SourceR["NOTES"] = Notes;
            SourceR["IMPORT_FILTER_ID"] = FilterID;

            TDS.TB_SOURCE.Rows.Add(SourceR);
        }
        private void fillAuthorsTB(Data.ImportItemsDataset TDS, DataRow AuthorR, AutH Auth, object ItemID)
        {
            //AuthorR.ITEM_ID = ItemR.ITEM_ID;
            //AuthorR.FIRST = Auth.FirstName;
            //AuthorR.SECOND = Auth.MiddleName;
            //AuthorR.LAST = Auth.LastName;
            //AuthorR.RANK = (byte)Auth.Rank;
            //AuthorR.ROLE = (byte)Auth.Role;
            AuthorR["ITEM_ID"] = ItemID;
            AuthorR["FIRST"] = Auth.FirstName;
            AuthorR["SECOND"] = Auth.MiddleName;
            AuthorR["LAST"] = Auth.LastName;
            AuthorR["RANK"] = (short)Auth.Rank;
            AuthorR["ROLE"] = (byte)Auth.Role;
            //TDS.tb_ITEM_AUTHOR.Rows.Add(AuthorR);
            TDS.TB_ITEM_AUTHOR.Rows.Add(AuthorR);
        }
        private void FillItemTB(Data.ImportItemsDataset TDS, DataRow ItemR, ItemIncomingData item, int r_id, ReviewerIdentity ri)
        {
            //ItemR.ABSTRACT = item.Abstract;
            //ItemR.AVAILABILITY = item.Availability;
            //ItemR.CITY = item.City;
            //ItemR.COMMENTS = item.Comments;
            //ItemR.COUNTRY = item.Country;
            //ItemR.CREATED_BY = ri.Name;
            //ItemR.DATE_CREATED = System.DateTime.Now;
            //ItemR.DATE_EDITED = System.DateTime.Now;
            //ItemR.EDITED_BY = ri.Name;
            //ItemR.EDITION = item.Edition;
            //ItemR.INSTITUTION = item.Institution;
            //ItemR.IS_LOCAL = false;
            //ItemR.ISSUE = item.Issue;
            //ItemR.MONTH = item.Month;
            //ItemR.OLD_ITEM_ID = item.OldItemId;
            //ItemR.PAGES = item.Pages;
            //ItemR.PARENT_TITLE = item.Parent_title;
            //ItemR.PUBLISHER = item.Publisher;
            //ItemR.SHORT_TITLE = item.Short_title;
            //ItemR.STANDARD_NUMBER = item.Standard_number;
            //ItemR.TITLE = item.Title;
            //ItemR.TYPE_ID = (byte)item.Type_id;
            //ItemR.URL = item.Url;
            //ItemR.VOLUME = item.Volume;
            //ItemR.YEAR = item.Year;
            //ItemR.DOI = item.DOI;
            //ItemR.KEYWORDS = item.Keywords;
            //TDS.tb_ITEM.Rows.Add(ItemR);
            ItemR["ABSTRACT"] = item.Abstract;
            ItemR["AVAILABILITY"] = item.Availability;
            ItemR["CITY"] = item.City;
            ItemR["COMMENTS"] = item.Comments;
            ItemR["COUNTRY"] = item.Country;
            ItemR["CREATED_BY"] = ri.Name;
            ItemR["DATE_CREATED"] = System.DateTime.Now;
            ItemR["DATE_EDITED"] = System.DateTime.Now;
            ItemR["EDITED_BY"] = ri.Name;
            ItemR["EDITION"] = item.Edition;
            ItemR["INSTITUTION"] = item.Institution;
            ItemR["IS_LOCAL"] = false;
            ItemR["ISSUE"] = item.Issue;
            ItemR["MONTH"] = item.Month;
            ItemR["OLD_ITEM_ID"] = item.OldItemId;
            ItemR["PAGES"] = item.Pages;
            ItemR["PARENT_TITLE"] = item.Parent_title;
            ItemR["PUBLISHER"] = item.Publisher;
            ItemR["SHORT_TITLE"] = item.Short_title;
            ItemR["STANDARD_NUMBER"] = item.Standard_number;
            ItemR["TITLE"] = item.Title;
            ItemR["TYPE_ID"] = (byte)item.Type_id;
            ItemR["URL"] = item.Url;
            ItemR["VOLUME"] = item.Volume;
            ItemR["YEAR"] = item.Year;
            ItemR["DOI"] = item.DOI;
            ItemR["KEYWORDS"] = item.Keywords;
            ItemR["SearchText"] = item.SearchText;
            TDS.TB_ITEM.Rows.Add(ItemR);
            if (this.HasMAGScores)
            {
                Int64 MagId;
                if (Int64.TryParse(item.OldItemId, out MagId))
                {
                    DataRow ItemMag = TDS.TB_ITEM_MAG_MATCH.NewRow();
                    ItemMag["ITEM_ID"] = ItemR["ITEM_ID"];
                    ItemMag["REVIEW_ID"] = r_id; //   
                    ItemMag["PaperId"] = MagId;
                    ItemMag["AutoMatchScore"] = item.MAGMatchScore;
                    ItemMag["ManualTrueMatch"] = item.MAGManualTrueMatch;
                    ItemMag["ManualFalseMatch"] = item.MAGManualFalseMatch;
                    TDS.TB_ITEM_MAG_MATCH.Rows.Add(ItemMag);
                }
            }
        }
        private void BulkUpload(Data.ImportItemsDataset TDS, int Source_S)
        {
            //three cases also here:
            //1. do all tables
            //2. Avoid TB_ITEM_REVIEW (first batch in a series)
            //3. Avoid TB_ITEM_REVIEW and TB_SOURCE (a batch in a series, but not the first one)
            DateTime startTime = DateTime.Now;
            using (SqlBulkCopy sbc = new SqlBulkCopy(DataConnection.ConnectionString, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.CheckConstraints))
            {
                if (SourceID == 0 & IsNew == true)
                {//cases 1 and 2
                    sbc.DestinationTableName = "TB_SOURCE";
                    // Number of records to be processed in one go
                    sbc.BatchSize = 1;
                    // Map the Source Column from DataTabel to the Destination Columns in SQL Server 2005 Person Table
                    sbc.ColumnMappings.Add("SOURCE_ID", "SOURCE_ID");
                    sbc.ColumnMappings.Add("SOURCE_NAME", "SOURCE_NAME");
                    sbc.ColumnMappings.Add("REVIEW_ID", "REVIEW_ID");
                    sbc.ColumnMappings.Add("DATE_OF_SEARCH", "DATE_OF_SEARCH");
                    sbc.ColumnMappings.Add("DATE_OF_IMPORT", "DATE_OF_IMPORT");
                    sbc.ColumnMappings.Add("SOURCE_DATABASE", "SOURCE_DATABASE");
                    sbc.ColumnMappings.Add("SEARCH_DESCRIPTION", "SEARCH_DESCRIPTION");
                    sbc.ColumnMappings.Add("SEARCH_STRING", "SEARCH_STRING");
                    sbc.ColumnMappings.Add("NOTES", "NOTES");
                    sbc.ColumnMappings.Add("IMPORT_FILTER_ID", "IMPORT_FILTER_ID");
                    // Number of records after which client has to be notified about its status
                    sbc.NotifyAfter = TDS.TB_SOURCE.Rows.Count;
                    // Event that gets fired when NotifyAfter number of records are processed.
                    //sbc.SqlRowsCopied += new SqlRowsCopiedEventHandler(sbc_SqlRowsCopied);
                    // Finally write to server
                    sbc.WriteToServer(TDS.TB_SOURCE);
                }

                //As above, but on TB_ITEM
                sbc.DestinationTableName = "TB_ITEM";
                sbc.ColumnMappings.Clear();
                sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
                sbc.ColumnMappings.Add("TYPE_ID", "TYPE_ID");
                sbc.ColumnMappings.Add("TITLE", "TITLE");
                sbc.ColumnMappings.Add("PARENT_TITLE", "PARENT_TITLE");
                sbc.ColumnMappings.Add("SHORT_TITLE", "SHORT_TITLE");
                sbc.ColumnMappings.Add("DATE_CREATED", "DATE_CREATED");
                sbc.ColumnMappings.Add("CREATED_BY", "CREATED_BY");
                sbc.ColumnMappings.Add("DATE_EDITED", "DATE_EDITED");
                sbc.ColumnMappings.Add("EDITED_BY", "EDITED_BY");
                sbc.ColumnMappings.Add("YEAR", "YEAR");
                sbc.ColumnMappings.Add("MONTH", "MONTH");
                sbc.ColumnMappings.Add("STANDARD_NUMBER", "STANDARD_NUMBER");
                sbc.ColumnMappings.Add("CITY", "CITY");
                sbc.ColumnMappings.Add("COUNTRY", "COUNTRY");
                sbc.ColumnMappings.Add("PUBLISHER", "PUBLISHER");
                sbc.ColumnMappings.Add("INSTITUTION", "INSTITUTION");
                sbc.ColumnMappings.Add("VOLUME", "VOLUME");
                sbc.ColumnMappings.Add("PAGES", "PAGES");
                sbc.ColumnMappings.Add("ISSUE", "ISSUE");
                sbc.ColumnMappings.Add("IS_LOCAL", "IS_LOCAL");
                sbc.ColumnMappings.Add("AVAILABILITY", "AVAILABILITY");
                sbc.ColumnMappings.Add("URL", "URL");
                sbc.ColumnMappings.Add("OLD_ITEM_ID", "OLD_ITEM_ID");
                sbc.ColumnMappings.Add("ABSTRACT", "ABSTRACT");
                sbc.ColumnMappings.Add("COMMENTS", "COMMENTS");
                sbc.ColumnMappings.Add("DOI", "DOI");
                sbc.ColumnMappings.Add("KEYWORDS", "KEYWORDS");

                sbc.BatchSize = 1000;
                //sbc.NotifyAfter = TDS.tb_ITEM.Rows.Count;
                //sbc.WriteToServer(TDS.tb_ITEM);
                sbc.NotifyAfter = TDS.TB_ITEM.Rows.Count;
                sbc.WriteToServer(TDS.TB_ITEM);

                //Write tb_ITEM_AUTHOR
                sbc.DestinationTableName = "tb_ITEM_AUTHOR";
                sbc.ColumnMappings.Clear();
                sbc.ColumnMappings.Add("ITEM_AUTHOR_ID", "ITEM_AUTHOR_ID");
                sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
                sbc.ColumnMappings.Add("LAST", "LAST");
                sbc.ColumnMappings.Add("FIRST", "FIRST");
                sbc.ColumnMappings.Add("SECOND", "SECOND");
                sbc.ColumnMappings.Add("ROLE", "ROLE");
                sbc.ColumnMappings.Add("RANK", "RANK");

                //sbc.NotifyAfter = TDS.tb_ITEM_AUTHOR.Rows.Count;
                //sbc.WriteToServer(TDS.tb_ITEM_AUTHOR);
                sbc.NotifyAfter = TDS.TB_ITEM_AUTHOR.Rows.Count;
                sbc.WriteToServer(TDS.TB_ITEM_AUTHOR);

                if (IsFirst && IsLast)
                {//CASE 1 only
                    //Write TB_ITEM_REVIEW
                    sbc.DestinationTableName = "TB_ITEM_REVIEW";
                    sbc.ColumnMappings.Clear();
                    sbc.ColumnMappings.Add("ITEM_REVIEW_ID", "ITEM_REVIEW_ID");
                    sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
                    sbc.ColumnMappings.Add("REVIEW_ID", "REVIEW_ID");
                    sbc.ColumnMappings.Add("IS_INCLUDED", "IS_INCLUDED");
                    sbc.ColumnMappings.Add("MASTER_ITEM_ID", "MASTER_ITEM_ID");
                    sbc.ColumnMappings.Add("IS_DELETED", "IS_DELETED");

                    sbc.NotifyAfter = TDS.TB_ITEM_REVIEW.Rows.Count;
                    sbc.WriteToServer(TDS.TB_ITEM_REVIEW);
                }
                if (HasMAGScores)
                {
                    sbc.DestinationTableName = "tb_ITEM_MAG_MATCH";
                    sbc.ColumnMappings.Clear();
                    sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
                    sbc.ColumnMappings.Add("REVIEW_ID", "REVIEW_ID");
                    sbc.ColumnMappings.Add("PaperId", "PaperId");
                    sbc.ColumnMappings.Add("AutoMatchScore", "AutoMatchScore");
                    sbc.ColumnMappings.Add("ManualTrueMatch", "ManualTrueMatch");
                    sbc.ColumnMappings.Add("ManualFalseMatch", "ManualFalseMatch");

                    sbc.NotifyAfter = TDS.TB_ITEM_MAG_MATCH.Rows.Count;
                    sbc.WriteToServer(TDS.TB_ITEM_MAG_MATCH);
                }
                //write TB_ITEM_SOURCE
                sbc.DestinationTableName = "TB_ITEM_SOURCE";
                sbc.ColumnMappings.Clear();
                sbc.ColumnMappings.Add("ITEM_SOURCE_ID", "ITEM_SOURCE_ID");
                sbc.ColumnMappings.Add("ITEM_ID", "ITEM_ID");
                sbc.ColumnMappings.Add("SOURCE_ID", "SOURCE_ID");
                sbc.NotifyAfter = TDS.TB_ITEM_SOURCE.Rows.Count;
                sbc.SqlRowsCopied += new SqlRowsCopiedEventHandler(sbc_SqlRowsCopied);
                sbc.WriteToServer(TDS.TB_ITEM_SOURCE);
                sbc.Close();
                if (savedAll == true)// this is only true if we've saved to TB_ITEM_SOURCE
                {
                    TimeSpan time = DateTime.Now - startTime;
                    this.SourceName = "Server Saved it in (ms): " + time.TotalMilliseconds;
                    this.IncomingItems.Clear();
                    if (SourceID == 0 & IsNew == true)
                    {//CASES 1 & 2, we want to keep the ID of the new source.
                        this.SourceID = Source_S + 1;
                    }
                }
            }
        }

        protected override void DataPortal_Update()
        {
            DataPortal_Insert();
           
        }
        void sbc_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            if (((SqlBulkCopy)sender).DestinationTableName == "TB_ITEM_SOURCE")
            {
                savedAll = true;
            }
            

        }

        public bool AddUnique(ItemIncomingData i)
        {
            if (!this.IncomingItems.Exists(element => element.OldItemId == i.OldItemId))
            {
                this.IncomingItems.Add(i);
                return true;
            }
            return false;
        }

#endif
        public void buildShortTitles()
        {
            foreach (ItemIncomingData iid in IncomingItems)
            {
                iid.buildShortTitle();
            }
        }
        #endregion
    }
    public class FilterRules
    {
        #region members
        public List<TypeRules> typeRules { get; set; }
        public TypesMap typesMap { get; set; }
        //originally the regex set values where private, so to force to use a string to set it
        //in SL reflection will not work with private set, so we're forced to make it public...
        public Regex StartOfNewRec { get; set; }
        public Regex typeField { get; set; }
        public Regex StartOfNewField { get; set; }
        public List<Regex> Title { get; set; }
        public List<Regex> pTitle { get; set; }
        public List<Regex> shortTitle { get; set; }
        public List<Regex> Date { get; set; }
        public List<Regex> Month { get; set; }
        public List<Regex> Author { get; set; }
        public List<Regex> ParentAuthor { get; set; }
        public List<Regex> StandardN { get; set; }
        public List<Regex> City { get; set; }
        public List<Regex> Publisher { get; set; }
        public List<Regex> Institution { get; set; }
        public List<Regex> Volume { get; set; }
        public List<Regex> Issue { get; set; }
        public List<Regex> Edition { get; set; }
        public List<Regex> StartPage { get; set; }
        public List<Regex> EndPage { get; set; }
        public List<Regex> Pages { get; set; }
        public List<Regex> Availability { get; set; }
        public List<Regex> Url { get; set; }
        public List<Regex> Abstract { get; set; }
        public List<Regex> OldItemID { get; set; }
        public List<Regex> Notes { get; set; }
        public List<Regex> DOI { get; set; }
        public List<Regex> Keywords { get; set; }
        public int DefaultTypeCode { get; set; }
        #endregion

        #region setRegexStrings
        public void AddTypeDef(int code, string @val)
        {
            typesMap.Add(code, new Regex(val));
        }
        public void startOfNewRec_Set(string @val)
        {
            StartOfNewRec = new Regex(val, RegexOptions.Multiline);
        }
        public void typeField_Set(string @val)
        {
            typeField = new Regex(val, RegexOptions.Multiline);
        }
        public void startOfNewField_Set(string @val)
        {
            StartOfNewField = new Regex(val, RegexOptions.Multiline);
        }

        public void title_Set(string @val)
        {
            Title = buildRegexList(val);
        }
        public void pTitle_Set(string @val)
        {
            pTitle = buildRegexList(val);
        }
        public void shortTitle_Set(string @val)
        {
            shortTitle = buildRegexList(val);
        }
        public void date_Set(string @val)
        {
            Date = buildRegexList(val);
        }
        public void month_Set(string @val)
        {
            Month = buildRegexList(val);
        }
        public void author_Set(string @val)
        {
            Author = buildRegexList(val);
        }
        public void pAuthor_Set(string @val)
        {
            ParentAuthor = buildRegexList(val);
        }
        public void StandardN_Set(string @val)
        {
            StandardN = buildRegexList(val);
        }
        public void City_Set(string @val)
        {
            City = buildRegexList(val);
        }
        public void Publisher_Set(string @val)
        {
            Publisher = buildRegexList(val);
        }
        public void Institution_Set(string @val)
        {
            Institution = buildRegexList(val);
        }
        public void Volume_Set(string @val)
        {
            Volume = buildRegexList(val);
        }
        public void Issue_Set(string @val)
        {
            Issue = buildRegexList(val);
        }
        public void Edition_Set(string @val)
        {
            Edition = buildRegexList(val);
        }
        public void StartPage_Set(string @val)
        {
            StartPage = buildRegexList(val);
        }
        public void EndPage_Set(string @val)
        {
            EndPage = buildRegexList(val);
        }
        public void Pages_Set(string @val)
        {
            Pages = buildRegexList(val);
        }
        public void Availability_Set(string @val)
        {
            Availability = buildRegexList(val);
        }
        public void Url_Set(string @val)
        {
            Url = buildRegexList(val);
        }
        public void Abstract_Set(string @val)
        {
            Abstract = buildRegexList(val);
        }
        public void OldItemID_Set(string @val)
        {
            OldItemID = buildRegexList(val);
        }
        public void Notes_Set(string @val)
        {
            Notes = buildRegexList(val);
        }
        public void DOI_Set(string @val)
        {
            DOI = buildRegexList(val);
        }
        public void Keywords_Set(string @val)
        {
            Keywords = buildRegexList(val);
        }
        #endregion

        #region constructor
        public FilterRules()
        {
            typesMap = new TypesMap();
            typeRules = new List<TypeRules>();
            StartOfNewRec = new Regex(@"\\M\\w");
            typeField = new Regex(@"\\M\\w");
            StartOfNewField = new Regex(@"\\M\\w");
            Title = buildRegexList(@"\\M\\w");
            pTitle = buildRegexList(@"\\M\\w");
            shortTitle = buildRegexList(@"\\M\\w");
            Date = buildRegexList(@"\\M\\w");
            Month = buildRegexList(@"\\M\\w");
            Author = buildRegexList(@"\\M\\w");
            ParentAuthor = buildRegexList(@"\\M\\w");
            StandardN = buildRegexList(@"\\M\\w");
            City = buildRegexList(@"\\M\\w");
            Publisher = buildRegexList(@"\\M\\w");
            Institution = buildRegexList(@"\\M\\w");
            Volume = buildRegexList(@"\\M\\w");
            Issue = buildRegexList(@"\\M\\w");
            Edition = buildRegexList(@"\\M\\w");
            StartPage = buildRegexList(@"\\M\\w");
            EndPage = buildRegexList(@"\\M\\w");
            Pages = buildRegexList(@"\\M\\w");
            Availability = buildRegexList(@"\\M\\w");
            Url = buildRegexList(@"\\M\\w");
            Abstract = buildRegexList(@"\\M\\w");
            OldItemID = buildRegexList(@"\\M\\w");
            Notes = buildRegexList(@"\\M\\w");
            DOI = buildRegexList(@"\\M\\w");
            Keywords = buildRegexList(@"\\M\\w");
            DefaultTypeCode = 12;
        }
        public static List<Regex> buildRegexList(string input)
        {
            List<Regex> result = new List<Regex>();
            string[] StArr = input.Split(new char[] { '¬' });
            foreach (string curr in StArr) result.Add(new Regex(curr, RegexOptions.Multiline|RegexOptions.Singleline));
            return result;
        }
        public FilterRules(FilterRules src) //this does SHALLOW cloning!!!! consider deepening it
        {
            this.typesMap = src.typesMap;
            this.typeRules = src.typeRules;
            this.StartOfNewRec = src.StartOfNewRec;
            this.typeField = src.typeField;
            this.StartOfNewField = src.StartOfNewField;
            this.Title = src.Title;
            this.pTitle = src.pTitle;
            this.shortTitle = src.shortTitle;
            this.Date = src.Date;
            this.Month = src.Month;
            this.Author = src.Author;
            this.ParentAuthor = src.ParentAuthor;
            this.StandardN = src.StandardN;
            this.City = src.City;
            this.Publisher = src.Publisher;
            this.Institution = src.Institution;
            this.Volume = src.Volume;
            this.Issue = src.Issue;
            this.Edition = src.Edition;
            this.StartPage = src.StartPage;
            this.EndPage = src.EndPage;
            this.Pages = src.Pages;
            this.Availability = src.Availability;
            this.Url = src.Url;
            this.Abstract = src.Abstract;
            this.OldItemID = src.OldItemID;
            this.Notes = src.Notes;
            this.DOI = src.DOI;
            this.Keywords = src.Keywords;
            this.DefaultTypeCode = src.DefaultTypeCode;
        }
        #endregion
    }
    public class TypesMap : Dictionary<int, Regex>
    {
        public void Add(int code, string value)
        {
            base.Add(code, new Regex(value));
        }
    }
    public class TypeRules
    {
        private string _RuleName, _RuleRegexSt;
        private int _Type_ID;

        public string RuleName
        {
            get { return _RuleName; }
            set { _RuleName = value; }
        }
        public string RuleRegexSt
        {
            get { return _RuleRegexSt; }
            set { _RuleRegexSt = value; }
        }
        public int Type_ID
        {
            get { return _Type_ID; }
            set { _Type_ID = value; }
        }
        public TypeRules(int TypeID, string RuleName, string RuleRegex)
        {
            _RuleName = RuleName;
            _RuleRegexSt = RuleRegex;
            _Type_ID = TypeID;
        }
    }
    
    #endregion

}
