using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLibrary.BusinessClasses
{
    public class ExportItemToRISString
    {
        private static List<string> calend = new List<string>{"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", 
                "Aug", "Sep", "Oct", "Nov", "Dec"};
       
        public static string ExportItemToRIS(Item it)
        {
            string res = "TY  - ";
            string tmp = "";
            switch (it.TypeId)
            {
                case 14:
                    res += "JOUR" + System.Environment.NewLine;
                    break;
                case 1:
                    res += "RPRT" + System.Environment.NewLine;
                    break;
                case 2:
                    res += "BOOK" + System.Environment.NewLine;
                    break;
                case 3:
                    res += "CHAP" + System.Environment.NewLine;
                    break;
                case 4:
                    res += "THES" + System.Environment.NewLine;
                    break;
                case 5:
                    res += "CONF" + System.Environment.NewLine;
                    break;
                case 6:
                    res += "ELEC" + System.Environment.NewLine;
                    break;
                case 7:
                    res += "ELEC" + System.Environment.NewLine;
                    break;
                case 8:
                    res += "ADVS" + System.Environment.NewLine;
                    break;
                case 10:
                    res += "MGZN" + System.Environment.NewLine;
                    break;
                default:
                    res += "GEN" + System.Environment.NewLine;
                    break;
            }
            res += "T1  - " + it.Title + System.Environment.NewLine;
            if(it.TypeId == 10 | it.TypeId == 14) 
                res += "JF  - " + it.ParentTitle + System.Environment.NewLine;
            else 
                res += "T2  - " + it.ParentTitle + System.Environment.NewLine;
            foreach (string au in it.Authors.Split(';'))
            {
                tmp = au.Trim();
                if (tmp != "") res += "A1  - " + tmp + System.Environment.NewLine;
            }
            foreach (string au in it.ParentAuthors.Split(';'))
            {
                tmp = au.Trim();
                if (tmp != "") res += "A2  - " + tmp + System.Environment.NewLine;
            }
            res += "ST  - " + it.ShortTitle + System.Environment.NewLine;
            res += "KW  - eppi-reviewer4" + System.Environment.NewLine
                + ((it.Keywords != null && it.Keywords.Length > 2) ? it.Keywords.Trim() + Environment.NewLine : "");
            int Month, Yr;
            string tmpDate = "";
            int.TryParse(it.Month, out Month) ;
            if (Month < 1 || Month > 12 )
            {
                Month = 1 + it.Month.Length > 2 ? calend.IndexOf(it.Month.Substring(0, 3)) + 1 : 0;
            }
            if (it.Year != "" & int.TryParse(it.Year, out Yr))
            {
                if (Yr > 0)
                {
                    if (Yr < 20) Yr += 1900;
                    else if (Yr < 100) Yr += 2000;
                    if ((Yr.ToString()).Length == 4)
                    {
                        res += "PY  - " + Yr.ToString() + Environment.NewLine;
                        if (Month != 0)
                        {

                            tmpDate += it.Year + "/" +
                                ((Month.ToString().Length == 1 ? "0" + Month.ToString() : Month.ToString()))
                                + "//";
                        }
                        else
                        {
                            tmpDate += it.Year + "///" + it.Month;//"Y1  - " 
                        }
                    }
                }
            }
            if (tmpDate.Length > 0)
            {
                res += "DA  - " + tmpDate + Environment.NewLine;
                res += "Y1  - " + tmpDate;
            
            
                //little trick: edition information is supposed to be the additional info at the end of the 
                //Y1 filed. For Thesis pubtype (4) we use the edition field to hold the thesys type,
                //the following finishes up the Y1 field keeping all this into account
            
                if (it.TypeId == 4 & it.Edition.Length > 0)
                    res += System.Environment.NewLine + "KW  - " + it.Edition + System.Environment.NewLine;
                else if (it.Edition.Length > 0)
                    res += " " + it.Edition + System.Environment.NewLine;
                else res += System.Environment.NewLine;
                
            }
            else if (it.TypeId == 4 & it.Edition.Length > 0)
            {
                res += System.Environment.NewLine + "KW  - " + it.Edition + System.Environment.NewLine;
            }//end of little trick

            res += "N2  - " + it.Abstract + System.Environment.NewLine;
            res += "AB  - " + it.Abstract + System.Environment.NewLine;
            if (it.DOI.Length > 0) res += "DO  - " + it.DOI + System.Environment.NewLine;
            res += "VL  - " + it.Volume + System.Environment.NewLine;
            res += "IS  - " + it.Issue + System.Environment.NewLine;
            char[] split = new char[] { '-' };
            Yr = it.Pages.IndexOfAny(split);
            if (Yr > 0)
            {
                string[] pgs = it.Pages.Split(split, StringSplitOptions.None);
                res += "SP  - " + pgs[0] + System.Environment.NewLine;
                res += "EP  - " + pgs[1] + System.Environment.NewLine;
            }
            else if (it.Pages.Length > 0) res += "SP  - " + it.Pages + System.Environment.NewLine;
            res += "CY  - " + it.City + (it.Country.Length > 0 ? " " + it.Country : "" )+ System.Environment.NewLine;
            if(it.URL.Length > 0)
                res += "UR  - " + it.URL + System.Environment.NewLine; 
            if(it.Availability.Length > 0)
                res += "AV  - " + it.Availability + System.Environment.NewLine; 
            if (it.Publisher.Length > 0)
                res += "PB  - " + it.Publisher + System.Environment.NewLine;
            if (it.StandardNumber.Length > 0)
                res += "SN  - " + it.StandardNumber + System.Environment.NewLine;
            res += "U1  - " + it.ItemId.ToString() + System.Environment.NewLine;
            if(it.OldItemId.Length > 0)
                res += "U2  - " + it.OldItemId + System.Environment.NewLine;
            
           
            res += "N1  - " + it.Comments + System.Environment.NewLine;
            
            res += "ER  - " + System.Environment.NewLine + System.Environment.NewLine;

            res = res.Replace("     ", " ");
            res = res.Replace("    ", " ");
            res = res.Replace("   ", " ");
            res = res.Replace("   ", " ");
            return res;
        }

    }
}