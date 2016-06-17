using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Configuration;
using System.Collections;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;


public partial class PresenterEditIntro : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Utils.GetSessionString("Contact_ID") != null)
        {
            //if (Utils.GetSessionString("IsAdm") == "True")
            //{
                if (!IsPostBack)
                {
                    //lblWebDB_ID.Text = Request.QueryString.Get("WebDb_ID").ToString();
                    lblWebDB_ID.Text = Utils.GetSessionString("WebDatabaseID");
                    /*
                    if (Request.QueryString.Get("Area").ToString() == "intro")
                    {
                        lblAreaName.Text = "Edit introduction text";
                    }
                    else if (Request.QueryString.Get("Area").ToString() == "header")
                    {
                        lblAreaName.Text = "Edit header text";
                    }
                    else
                    {
                        lblAreaName.Text = "Edit report text";
                    }
                    */

                    if (Utils.GetSessionString("DescriptionAdminEdit").ToString() == "True")
                    {
                        pnlAdminEdit.Visible = true;
                        pnlUserEdit.Visible = false;
                    }
                    char bullet = (char)0x2022;
                    bool isAdmDB = true;
                    IDataReader idr, idr2;
                    idr = Utils.GetReader(isAdmDB, "st_WDDescriptionGet",
                        lblWebDB_ID.Text);
                    if (idr.Read())
                    {
                        if (Request.QueryString.Get("Area").ToString() == "intro")
                        {
                            //Editor1.ContentHTML = idr["DESCRIPTION"].ToString();
                            //RadEditor.Content = idr["DESCRIPTION"].ToString();
                            
                            idr2 = Utils.GetReader(isAdmDB, "st_WDGetIntroductionData",
                                lblWebDB_ID.Text);                           
                            if (idr2.Read())
                            {                               
                                tbHeading1.Text = removeHtmlLinks(idr2["HEADER_1"].ToString(), "h1");
                                tbHeading2.Text = removeHtmlLinks(idr2["HEADER_2"].ToString(), "h2");
                                tbHeading3.Text = removeHtmlLinks(idr2["HEADER_3"].ToString(), "h3");
                                tbHeading4.Text = removeHtmlLinks(idr2["HEADER_4"].ToString(), "h4");
                                tbHeading5.Text = removeHtmlLinks(idr2["HEADER_5"].ToString(), "h5");
                                tbHeading6.Text = removeHtmlLinks(idr2["HEADER_6"].ToString(), "h6");
                                tbHeading7.Text = removeHtmlLinks(idr2["HEADER_7"].ToString(), "h7");
                                tbHeading8.Text = removeHtmlLinks(idr2["HEADER_8"].ToString(), "h8");
                                tbHeading9.Text = removeHtmlLinks(idr2["HEADER_9"].ToString(), "h9");
                                tbHeading10.Text = removeHtmlLinks(idr2["HEADER_10"].ToString(), "h10");
                                tbHeading11.Text = removeHtmlLinks(idr2["HEADER_11"].ToString(), "h11");
                                tbHeading12.Text = removeHtmlLinks(idr2["HEADER_12"].ToString(), "h12");
                                tbHeading13.Text = removeHtmlLinks(idr2["HEADER_13"].ToString(), "h13");
                                tbHeading14.Text = removeHtmlLinks(idr2["HEADER_14"].ToString(), "h14");
                                tbHeading15.Text = removeHtmlLinks(idr2["HEADER_15"].ToString(), "h15");
                                tbHeading16.Text = removeHtmlLinks(idr2["HEADER_16"].ToString(), "h16");
                                tbHeading17.Text = removeHtmlLinks(idr2["HEADER_17"].ToString(), "h17");
                                tbHeading18.Text = removeHtmlLinks(idr2["HEADER_18"].ToString(), "h18");
                                tbHeading19.Text = removeHtmlLinks(idr2["HEADER_19"].ToString(), "h19");
                                tbHeading20.Text = removeHtmlLinks(idr2["HEADER_20"].ToString(), "h20");

                                tbParagraph1.Text = removeHtmlLinks(idr2["PARAGRAPH_1"].ToString(), "p1");
                                tbParagraph2.Text = removeHtmlLinks(idr2["PARAGRAPH_2"].ToString(), "p2");
                                tbParagraph3.Text = removeHtmlLinks(idr2["PARAGRAPH_3"].ToString(), "p3");
                                tbParagraph4.Text = removeHtmlLinks(idr2["PARAGRAPH_4"].ToString(), "p4");
                                tbParagraph5.Text = removeHtmlLinks(idr2["PARAGRAPH_5"].ToString(), "p5");
                                tbParagraph6.Text = removeHtmlLinks(idr2["PARAGRAPH_6"].ToString(), "p6");
                                tbParagraph7.Text = removeHtmlLinks(idr2["PARAGRAPH_7"].ToString(), "p7");
                                tbParagraph8.Text = removeHtmlLinks(idr2["PARAGRAPH_8"].ToString(), "p8");
                                tbParagraph9.Text = removeHtmlLinks(idr2["PARAGRAPH_9"].ToString(), "p9");
                                tbParagraph10.Text = removeHtmlLinks(idr2["PARAGRAPH_10"].ToString(), "p10");
                                tbParagraph11.Text = removeHtmlLinks(idr2["PARAGRAPH_11"].ToString(), "p11");
                                tbParagraph12.Text = removeHtmlLinks(idr2["PARAGRAPH_12"].ToString(), "p12");
                                tbParagraph13.Text = removeHtmlLinks(idr2["PARAGRAPH_13"].ToString(), "p13");
                                tbParagraph14.Text = removeHtmlLinks(idr2["PARAGRAPH_14"].ToString(), "p14");
                                tbParagraph15.Text = removeHtmlLinks(idr2["PARAGRAPH_15"].ToString(), "p15");
                                tbParagraph16.Text = removeHtmlLinks(idr2["PARAGRAPH_16"].ToString(), "p16");
                                tbParagraph17.Text = removeHtmlLinks(idr2["PARAGRAPH_17"].ToString(), "p17");
                                tbParagraph18.Text = removeHtmlLinks(idr2["PARAGRAPH_18"].ToString(), "p18");
                                tbParagraph19.Text = removeHtmlLinks(idr2["PARAGRAPH_19"].ToString(), "p19");
                                tbParagraph20.Text = removeHtmlLinks(idr2["PARAGRAPH_20"].ToString(), "p20");


                                 string htmlIntro = idr2["HEADER_1"].ToString() + idr2["PARAGRAPH_1"].ToString() +
                                    idr2["HEADER_2"].ToString() + idr2["PARAGRAPH_2"].ToString() +
                                    idr2["HEADER_3"].ToString() + idr2["PARAGRAPH_3"].ToString() +
                                    idr2["HEADER_4"].ToString() + idr2["PARAGRAPH_4"].ToString() +
                                    idr2["HEADER_5"].ToString() + idr2["PARAGRAPH_5"].ToString() +
                                    idr2["HEADER_6"].ToString() + idr2["PARAGRAPH_6"].ToString() +
                                    idr2["HEADER_7"].ToString() + idr2["PARAGRAPH_7"].ToString() +
                                    idr2["HEADER_8"].ToString() + idr2["PARAGRAPH_8"].ToString() +
                                    idr2["HEADER_9"].ToString() + idr2["PARAGRAPH_9"].ToString() +
                                    idr2["HEADER_10"].ToString() + idr2["PARAGRAPH_10"].ToString() +
                                    idr2["HEADER_11"].ToString() + idr2["PARAGRAPH_11"].ToString() +
                                    idr2["HEADER_12"].ToString() + idr2["PARAGRAPH_12"].ToString() +
                                    idr2["HEADER_13"].ToString() + idr2["PARAGRAPH_13"].ToString() +
                                    idr2["HEADER_14"].ToString() + idr2["PARAGRAPH_14"].ToString() +
                                    idr2["HEADER_15"].ToString() + idr2["PARAGRAPH_15"].ToString() +
                                    idr2["HEADER_16"].ToString() + idr2["PARAGRAPH_16"].ToString() +
                                    idr2["HEADER_17"].ToString() + idr2["PARAGRAPH_17"].ToString() +
                                    idr2["HEADER_18"].ToString() + idr2["PARAGRAPH_18"].ToString() +
                                    idr2["HEADER_19"].ToString() + idr2["PARAGRAPH_19"].ToString() +
                                    idr2["HEADER_20"].ToString() + idr2["PARAGRAPH_20"].ToString();
                                
                                htmlIntro = htmlIntro.Replace("<p></p>", "");
                                RadEditor.Content = htmlIntro;


                                /*
                                string linkText = "";
                                string webAddress = "";
                                int startWA = 0;
                                int endWA = 0;
                                int startLT = 0;
                                int endLT = 0;
                                int count = 0;
                                
                                tbHeading1.Text = idr2["HEADER_1"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading1.Text.Contains("</a>"))
                                {
                                    // there could be multiple links so deal with each one
                                    count = Regex.Matches(tbHeading1.Text, "</a>").Count;
                                    for (int i = 0; i < count; i++)
                                    {
                                        startWA = tbHeading1.Text.IndexOf("<a href=") + 9;
                                        endWA = tbHeading1.Text.IndexOf('"', startWA);
                                        startLT = tbHeading1.Text.IndexOf('>');
                                        endLT = tbHeading1.Text.IndexOf("</a>");
                                        webAddress = tbHeading1.Text.Substring(startWA, endWA - startWA);
                                        linkText = tbHeading1.Text.Substring(startLT + 1, endLT - startLT - 1);
                                        tbHeading1.Text = tbHeading1.Text.Remove(endLT, 4);                                                                         
                                        if (webAddress != linkText)
                                        {
                                            tbHeading1.Text = tbHeading1.Text.Remove(startLT + 1, linkText.Length);
                                            tbHeading1.Text = tbHeading1.Text.Insert(startLT + 1, webAddress);
                                            tbHeading1.Text = tbHeading1.Text.Insert(startLT + 1, "[" + linkText + "]");
                                        }
                                        tbHeading1.Text = tbHeading1.Text.Remove(startWA - 9, startLT - startWA + 10);
                                    }                                    
                                }
                                
                                tbHeading2.Text = idr2["HEADER_2"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading2.Text.Contains("</a>"))
                                {
                                    count = Regex.Matches(tbHeading2.Text, "</a>").Count;
                                    for (int i = 0; i < count; i++)
                                    {
                                        startWA = tbHeading2.Text.IndexOf("<a href=") + 9;
                                        endWA = tbHeading2.Text.IndexOf('"', startWA);
                                        startLT = tbHeading2.Text.IndexOf('>');
                                        endLT = tbHeading2.Text.IndexOf("</a>");
                                        webAddress = tbHeading2.Text.Substring(startWA, endWA - startWA);
                                        linkText = tbHeading2.Text.Substring(startLT + 1, endLT - startLT - 1);
                                        tbHeading2.Text = tbHeading2.Text.Remove(endLT, 4);
                                        if (webAddress != linkText)
                                        {
                                            tbHeading2.Text = tbHeading2.Text.Remove(startLT + 1, linkText.Length);
                                            tbHeading2.Text = tbHeading2.Text.Insert(startLT + 1, webAddress);
                                            tbHeading2.Text = tbHeading2.Text.Insert(startLT + 1, "[" + linkText + "]");
                                        }
                                        tbHeading2.Text = tbHeading2.Text.Remove(startWA - 9, startLT - startWA + 10);
                                    }
                                }
                                tbHeading3.Text = idr2["HEADER_3"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading3.Text.Contains("</a>"))
                                {
                                    tbHeading3.Text = tbHeading3.Text.Replace("</a>", "");
                                    tbHeading3.Text = tbHeading3.Text.Remove(tbHeading3.Text.IndexOf('<'),
                                        tbHeading3.Text.IndexOf('>') - tbHeading3.Text.IndexOf('<') + 1);
                                }
                                tbHeading4.Text = idr2["HEADER_4"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading4.Text.Contains("</a>"))
                                {
                                    tbHeading4.Text = tbHeading4.Text.Replace("</a>", "");
                                    tbHeading4.Text = tbHeading4.Text.Remove(tbHeading4.Text.IndexOf('<'),
                                        tbHeading4.Text.IndexOf('>') - tbHeading4.Text.IndexOf('<') + 1);
                                }
                                tbHeading5.Text = idr2["HEADER_5"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading5.Text.Contains("</a>"))
                                {
                                    tbHeading5.Text = tbHeading5.Text.Replace("</a>", "");
                                    tbHeading5.Text = tbHeading5.Text.Remove(tbHeading5.Text.IndexOf('<'),
                                        tbHeading5.Text.IndexOf('>') - tbHeading5.Text.IndexOf('<') + 1);
                                }
                                tbHeading6.Text = idr2["HEADER_6"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading6.Text.Contains("</a>"))
                                {
                                    tbHeading6.Text = tbHeading6.Text.Replace("</a>", "");
                                    tbHeading6.Text = tbHeading6.Text.Remove(tbHeading6.Text.IndexOf('<'),
                                        tbHeading6.Text.IndexOf('>') - tbHeading6.Text.IndexOf('<') + 1);
                                }
                                tbHeading7.Text = idr2["HEADER_7"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading7.Text.Contains("</a>"))
                                {
                                    tbHeading7.Text = tbHeading7.Text.Replace("</a>", "");
                                    tbHeading7.Text = tbHeading7.Text.Remove(tbHeading7.Text.IndexOf('<'),
                                        tbHeading7.Text.IndexOf('>') - tbHeading7.Text.IndexOf('<') + 1);
                                }
                                tbHeading8.Text = idr2["HEADER_8"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading8.Text.Contains("</a>"))
                                {
                                    tbHeading8.Text = tbHeading8.Text.Replace("</a>", "");
                                    tbHeading8.Text = tbHeading8.Text.Remove(tbHeading8.Text.IndexOf('<'),
                                        tbHeading8.Text.IndexOf('>') - tbHeading8.Text.IndexOf('<') + 1);
                                }
                                tbHeading9.Text = idr2["HEADER_9"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading9.Text.Contains("</a>"))
                                {
                                    tbHeading9.Text = tbHeading9.Text.Replace("</a>", "");
                                    tbHeading9.Text = tbHeading9.Text.Remove(tbHeading9.Text.IndexOf('<'),
                                        tbHeading9.Text.IndexOf('>') - tbHeading9.Text.IndexOf('<') + 1);
                                }
                                tbHeading10.Text = idr2["HEADER_10"].ToString().Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
                                if (tbHeading10.Text.Contains("</a>"))
                                {
                                    tbHeading10.Text = tbHeading10.Text.Replace("</a>", "");
                                    tbHeading10.Text = tbHeading10.Text.Remove(tbHeading10.Text.IndexOf('<'),
                                        tbHeading10.Text.IndexOf('>') - tbHeading10.Text.IndexOf('<') + 1);
                                }
                                

                                tbParagraph1.Text = idr2["PARAGRAPH_1"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph1.Text.Contains("<ul>"))
                                {
                                    tbParagraph1.Text = tbParagraph1.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph1.Text = "   " + bullet + " " + tbParagraph1.Text;
                                    cbIndent1.Checked = true;
                                }
                                if (tbParagraph1.Text.Contains("</a>"))
                                {
                                    count = Regex.Matches(tbParagraph1.Text, "</a>").Count;
                                    for (int i = 0; i < count; i++)
                                    {
                                        startWA = tbParagraph1.Text.IndexOf("<a href=") + 9;
                                        endWA = tbParagraph1.Text.IndexOf('"', startWA);
                                        startLT = tbParagraph1.Text.IndexOf('>');
                                        endLT = tbParagraph1.Text.IndexOf("</a>");
                                        webAddress = tbParagraph1.Text.Substring(startWA, endWA - startWA);
                                        linkText = tbParagraph1.Text.Substring(startLT + 1, endLT - startLT - 1);
                                        tbParagraph1.Text = tbParagraph1.Text.Remove(endLT, 4);
                                        if (webAddress != linkText)
                                        {
                                            tbParagraph1.Text = tbParagraph1.Text.Remove(startLT + 1, linkText.Length);
                                            tbParagraph1.Text = tbParagraph1.Text.Insert(startLT + 1, webAddress);
                                            tbParagraph1.Text = tbParagraph1.Text.Insert(startLT + 1, "[" + linkText + "]");
                                        }
                                        tbParagraph1.Text = tbParagraph1.Text.Remove(startWA - 9, startLT - startWA + 10);
                                    }
                                }
                                
                                tbParagraph2.Text = idr2["PARAGRAPH_2"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph2.Text.Contains("<ul>"))
                                {
                                    tbParagraph2.Text = tbParagraph2.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph2.Text = "   " + bullet + " " + tbParagraph2.Text;
                                    cbIndent2.Checked = true;
                                }
                                if (tbParagraph2.Text.Contains("</a>"))
                                {
                                    count = Regex.Matches(tbParagraph2.Text, "</a>").Count;
                                    for (int i = 0; i < count; i++)
                                    {
                                        startWA = tbParagraph2.Text.IndexOf("<a href=") + 9;
                                        endWA = tbParagraph2.Text.IndexOf('"', startWA);
                                        startLT = tbParagraph2.Text.IndexOf('>');
                                        endLT = tbParagraph2.Text.IndexOf("</a>");
                                        webAddress = tbParagraph2.Text.Substring(startWA, endWA - startWA);
                                        linkText = tbParagraph2.Text.Substring(startLT + 1, endLT - startLT - 1);
                                        tbParagraph2.Text = tbParagraph2.Text.Remove(endLT, 4);
                                        if (webAddress != linkText)
                                        {
                                            tbParagraph2.Text = tbParagraph2.Text.Remove(startLT + 1, linkText.Length);
                                            tbParagraph2.Text = tbParagraph2.Text.Insert(startLT + 1, webAddress);
                                            tbParagraph2.Text = tbParagraph2.Text.Insert(startLT + 1, "[" + linkText + "]");
                                        }
                                        tbParagraph2.Text = tbParagraph2.Text.Remove(startWA - 9, startLT - startWA + 10);
                                    }
                                }

                                tbParagraph3.Text = idr2["PARAGRAPH_3"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph3.Text.Contains("<ul>"))
                                {
                                    tbParagraph3.Text = tbParagraph3.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph3.Text = "   " + bullet + " " + tbParagraph3.Text;
                                    cbIndent3.Checked = true;
                                }
                                if (tbParagraph3.Text.Contains("</a>"))
                                {
                                    tbParagraph3.Text = tbParagraph3.Text.Replace("</a>", "");
                                    tbParagraph3.Text = tbParagraph3.Text.Remove(tbParagraph3.Text.IndexOf('<'),
                                        tbParagraph3.Text.IndexOf('>') - tbParagraph3.Text.IndexOf('<') + 1);
                                }

                                tbParagraph4.Text = idr2["PARAGRAPH_4"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph4.Text.Contains("<ul>"))
                                {
                                    tbParagraph4.Text = tbParagraph4.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph4.Text = "   " + bullet + " " + tbParagraph4.Text;
                                    cbIndent4.Checked = true;
                                }
                                if (tbParagraph4.Text.Contains("</a>"))
                                {
                                    tbParagraph4.Text = tbParagraph4.Text.Replace("</a>", "");
                                    tbParagraph4.Text = tbParagraph4.Text.Remove(tbParagraph4.Text.IndexOf('<'),
                                        tbParagraph4.Text.IndexOf('>') - tbParagraph4.Text.IndexOf('<') + 1);
                                }

                                tbParagraph5.Text = idr2["PARAGRAPH_5"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph5.Text.Contains("<ul>"))
                                {
                                    tbParagraph5.Text = tbParagraph5.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph5.Text = "   " + bullet + " " + tbParagraph5.Text;
                                    cbIndent5.Checked = true;
                                }
                                if (tbParagraph5.Text.Contains("</a>"))
                                {
                                    tbParagraph5.Text = tbParagraph5.Text.Replace("</a>", "");
                                    tbParagraph5.Text = tbParagraph5.Text.Remove(tbParagraph5.Text.IndexOf('<'),
                                        tbParagraph5.Text.IndexOf('>') - tbParagraph5.Text.IndexOf('<') + 1);
                                }

                                tbParagraph6.Text = idr2["PARAGRAPH_6"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph6.Text.Contains("<ul>"))
                                {
                                    tbParagraph6.Text = tbParagraph6.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph6.Text = "   " + bullet + " " + tbParagraph6.Text;
                                    cbIndent6.Checked = true;
                                }
                                if (tbParagraph6.Text.Contains("</a>"))
                                {
                                    tbParagraph6.Text = tbParagraph6.Text.Replace("</a>", "");
                                    tbParagraph6.Text = tbParagraph6.Text.Remove(tbParagraph6.Text.IndexOf('<'),
                                        tbParagraph6.Text.IndexOf('>') - tbParagraph6.Text.IndexOf('<') + 1);
                                }

                                tbParagraph7.Text = idr2["PARAGRAPH_7"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph7.Text.Contains("<ul>"))
                                {
                                    tbParagraph7.Text = tbParagraph7.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph7.Text = "   " + bullet + " " + tbParagraph7.Text;
                                    cbIndent7.Checked = true;
                                }
                                if (tbParagraph7.Text.Contains("</a>"))
                                {
                                    tbParagraph7.Text = tbParagraph7.Text.Replace("</a>", "");
                                    tbParagraph7.Text = tbParagraph7.Text.Remove(tbParagraph7.Text.IndexOf('<'),
                                        tbParagraph7.Text.IndexOf('>') - tbParagraph7.Text.IndexOf('<') + 1);
                                }

                                tbParagraph8.Text = idr2["PARAGRAPH_8"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph8.Text.Contains("<ul>"))
                                {
                                    tbParagraph8.Text = tbParagraph8.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph8.Text = "   " + bullet + " " + tbParagraph8.Text;
                                    cbIndent8.Checked = true;
                                }
                                if (tbParagraph8.Text.Contains("</a>"))
                                {
                                    tbParagraph8.Text = tbParagraph8.Text.Replace("</a>", "");
                                    tbParagraph8.Text = tbParagraph8.Text.Remove(tbParagraph8.Text.IndexOf('<'),
                                        tbParagraph8.Text.IndexOf('>') - tbParagraph8.Text.IndexOf('<') + 1);
                                }

                                tbParagraph9.Text = idr2["PARAGRAPH_9"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph9.Text.Contains("<ul>"))
                                {
                                    tbParagraph9.Text = tbParagraph9.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph9.Text = "   " + bullet + " " + tbParagraph9.Text;
                                    cbIndent9.Checked = true;
                                }
                                if (tbParagraph9.Text.Contains("</a>"))
                                {
                                    tbParagraph9.Text = tbParagraph9.Text.Replace("</a>", "");
                                    tbParagraph9.Text = tbParagraph9.Text.Remove(tbParagraph9.Text.IndexOf('<'),
                                        tbParagraph9.Text.IndexOf('>') - tbParagraph9.Text.IndexOf('<') + 1);
                                }

                                tbParagraph10.Text = idr2["PARAGRAPH_10"].ToString().Replace("<p>", "").Replace("</p>", "");
                                if (tbParagraph10.Text.Contains("<ul>"))
                                {
                                    tbParagraph10.Text = tbParagraph10.Text.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
                                    tbParagraph10.Text = "   " + bullet + " " + tbParagraph10.Text;
                                    cbIndent10.Checked = true;
                                }
                                if (tbParagraph10.Text.Contains("</a>"))
                                {
                                    tbParagraph10.Text = tbParagraph10.Text.Replace("</a>", "");
                                    tbParagraph10.Text = tbParagraph10.Text.Remove(tbParagraph10.Text.IndexOf('<'),
                                        tbParagraph10.Text.IndexOf('>') - tbParagraph10.Text.IndexOf('<') + 1);
                                }
                                */
                            }
                            idr2.Close();
                        }
                        else if (Request.QueryString.Get("Area").ToString() == "header")
                        {
                            RadEditor.Content = idr["HEADER"].ToString();
                        }
                        else
                        {
                            RadEditor.Content = idr["REPORT_INTRO"].ToString();
                        }

                        lblWebDbName.Text = idr["WEBDB_NAME"].ToString();
                        lblWebDbName1.Text = idr["WEBDB_NAME"].ToString();
                    }
                    idr.Close();
                    

                    //cmdSave.Attributes.Add("onclick", "if (confirm('Are you sure you are updating the correct database? Do you wish to continue?') == false) return false;");
                }
                imgHeaderImage1.ImageUrl = "~/ShowImage1.ashx?id=" + lblWebDB_ID.Text;
                imgHeaderImage2.ImageUrl = "~/ShowImage2.ashx?id=" + lblWebDB_ID.Text;

            //}
            //else
            //{
            //    Server.Transfer("Error.aspx");
            //}
        }
        else
        {
            Server.Transfer("Error.aspx");
        }
    }
    protected void cmdSave_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDSaveIntroduction", lblWebDB_ID.Text, ""/*lblAreaName.Text*/, RadEditor.Content);
    }
    protected void lbClose_Click(object sender, EventArgs e)
    {
        string scriptString = "";
        Type cstype = this.GetType();
        ClientScriptManager cs = Page.ClientScript;
        scriptString = "<script language=JavaScript>";
        scriptString += "window.close();</script>";

        if (!cs.IsStartupScriptRegistered("Startup"))
            cs.RegisterStartupScript(cstype, "Startup", scriptString);
    }

    protected void cmdSave_Click1(object sender, EventArgs e)
    {
        // for security purposes remove any < or > that the user may have added
        string heading1 = tbHeading1.Text.Replace("<", "").Replace(">", "");
        string heading2 = tbHeading2.Text.Replace("<", "").Replace(">", "");
        string heading3 = tbHeading3.Text.Replace("<", "").Replace(">", "");
        string heading4 = tbHeading4.Text.Replace("<", "").Replace(">", "");
        string heading5 = tbHeading5.Text.Replace("<", "").Replace(">", "");
        string heading6 = tbHeading6.Text.Replace("<", "").Replace(">", "");
        string heading7 = tbHeading7.Text.Replace("<", "").Replace(">", "");
        string heading8 = tbHeading8.Text.Replace("<", "").Replace(">", "");
        string heading9 = tbHeading9.Text.Replace("<", "").Replace(">", "");
        string heading10 = tbHeading10.Text.Replace("<", "").Replace(">", "");
        string heading11 = tbHeading11.Text.Replace("<", "").Replace(">", "");
        string heading12 = tbHeading12.Text.Replace("<", "").Replace(">", "");
        string heading13 = tbHeading13.Text.Replace("<", "").Replace(">", "");
        string heading14 = tbHeading14.Text.Replace("<", "").Replace(">", "");
        string heading15 = tbHeading15.Text.Replace("<", "").Replace(">", "");
        string heading16 = tbHeading16.Text.Replace("<", "").Replace(">", "");
        string heading17 = tbHeading17.Text.Replace("<", "").Replace(">", "");
        string heading18 = tbHeading18.Text.Replace("<", "").Replace(">", "");
        string heading19 = tbHeading19.Text.Replace("<", "").Replace(">", "");
        string heading20 = tbHeading20.Text.Replace("<", "").Replace(">", "");

        string paragraph1 = tbParagraph1.Text.Replace("<", "").Replace(">", "");
        string paragraph2 = tbParagraph2.Text.Replace("<", "").Replace(">", "");
        string paragraph3 = tbParagraph3.Text.Replace("<", "").Replace(">", "");
        string paragraph4 = tbParagraph4.Text.Replace("<", "").Replace(">", "");
        string paragraph5 = tbParagraph5.Text.Replace("<", "").Replace(">", "");
        string paragraph6 = tbParagraph6.Text.Replace("<", "").Replace(">", "");
        string paragraph7 = tbParagraph7.Text.Replace("<", "").Replace(">", "");
        string paragraph8 = tbParagraph8.Text.Replace("<", "").Replace(">", "");
        string paragraph9 = tbParagraph9.Text.Replace("<", "").Replace(">", "");
        string paragraph10 = tbParagraph10.Text.Replace("<", "").Replace(">", "");
        string paragraph11 = tbParagraph11.Text.Replace("<", "").Replace(">", "");
        string paragraph12 = tbParagraph12.Text.Replace("<", "").Replace(">", "");
        string paragraph13 = tbParagraph13.Text.Replace("<", "").Replace(">", "");
        string paragraph14 = tbParagraph14.Text.Replace("<", "").Replace(">", "");
        string paragraph15 = tbParagraph15.Text.Replace("<", "").Replace(">", "");
        string paragraph16 = tbParagraph16.Text.Replace("<", "").Replace(">", "");
        string paragraph17 = tbParagraph17.Text.Replace("<", "").Replace(">", "");
        string paragraph18 = tbParagraph18.Text.Replace("<", "").Replace(">", "");
        string paragraph19 = tbParagraph19.Text.Replace("<", "").Replace(">", "");
        string paragraph20 = tbParagraph20.Text.Replace("<", "").Replace(">", "");

        // make any urls linkable
        Regex r = new Regex("(https?://[^ )?]+)");
        heading1 = r.Replace(heading1, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading2 = r.Replace(heading2, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading3 = r.Replace(heading3, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading4 = r.Replace(heading4, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading5 = r.Replace(heading5, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading6 = r.Replace(heading6, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading7 = r.Replace(heading7, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading8 = r.Replace(heading8, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading9 = r.Replace(heading9, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading10 = r.Replace(heading10, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading11 = r.Replace(heading11, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading12 = r.Replace(heading12, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading13 = r.Replace(heading13, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading14 = r.Replace(heading14, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading15 = r.Replace(heading15, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading16 = r.Replace(heading16, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading17 = r.Replace(heading17, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading18 = r.Replace(heading18, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading19 = r.Replace(heading19, "<a href=\"$1\" target=\"_blank\">$1</a>");
        heading20 = r.Replace(heading20, "<a href=\"$1\" target=\"_blank\">$1</a>");

        paragraph1 = r.Replace(paragraph1, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph2 = r.Replace(paragraph2, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph3 = r.Replace(paragraph3, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph4 = r.Replace(paragraph4, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph5 = r.Replace(paragraph5, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph6 = r.Replace(paragraph6, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph7 = r.Replace(paragraph7, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph8 = r.Replace(paragraph8, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph9 = r.Replace(paragraph9, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph10 = r.Replace(paragraph10, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph11 = r.Replace(paragraph11, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph12 = r.Replace(paragraph12, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph13 = r.Replace(paragraph13, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph14 = r.Replace(paragraph14, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph15 = r.Replace(paragraph15, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph16 = r.Replace(paragraph16, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph17 = r.Replace(paragraph17, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph18 = r.Replace(paragraph18, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph19 = r.Replace(paragraph19, "<a href=\"$1\" target=\"_blank\">$1</a>");
        paragraph20 = r.Replace(paragraph20, "<a href=\"$1\" target=\"_blank\">$1</a>");


        heading1 = addHtmlLinks(heading1);
        heading2 = addHtmlLinks(heading2);
        heading3 = addHtmlLinks(heading3);
        heading4 = addHtmlLinks(heading4);
        heading5 = addHtmlLinks(heading5);
        heading6 = addHtmlLinks(heading6);
        heading7 = addHtmlLinks(heading7);
        heading8 = addHtmlLinks(heading8);
        heading9 = addHtmlLinks(heading9);
        heading10 = addHtmlLinks(heading10);
        heading11 = addHtmlLinks(heading11);
        heading12 = addHtmlLinks(heading12);
        heading13 = addHtmlLinks(heading13);
        heading14 = addHtmlLinks(heading14);
        heading15 = addHtmlLinks(heading15);
        heading16 = addHtmlLinks(heading16);
        heading17 = addHtmlLinks(heading17);
        heading18 = addHtmlLinks(heading18);
        heading19 = addHtmlLinks(heading19);
        heading20 = addHtmlLinks(heading20);

        paragraph1 = addHtmlLinks(paragraph1);
        paragraph2 = addHtmlLinks(paragraph2);
        paragraph3 = addHtmlLinks(paragraph3);
        paragraph4 = addHtmlLinks(paragraph4);
        paragraph5 = addHtmlLinks(paragraph5);
        paragraph6 = addHtmlLinks(paragraph6);
        paragraph7 = addHtmlLinks(paragraph7);
        paragraph8 = addHtmlLinks(paragraph8);
        paragraph9 = addHtmlLinks(paragraph9);
        paragraph10 = addHtmlLinks(paragraph10);
        paragraph11 = addHtmlLinks(paragraph11);
        paragraph12 = addHtmlLinks(paragraph12);
        paragraph13 = addHtmlLinks(paragraph13);
        paragraph14 = addHtmlLinks(paragraph14);
        paragraph15 = addHtmlLinks(paragraph15);
        paragraph16 = addHtmlLinks(paragraph16);
        paragraph17 = addHtmlLinks(paragraph17);
        paragraph18 = addHtmlLinks(paragraph18);
        paragraph19 = addHtmlLinks(paragraph19);
        paragraph20 = addHtmlLinks(paragraph20);

        
        /*
        string linkText = "";
        string webAddress = "";
        int startWA = 0;
        int endWA = 0;
        int startLT = 0;
        int endLT = 0;
        int count = 0;
        if (heading1.Contains("]<a"))
        {
            count = Regex.Matches(heading1, @"]<a").Count;
            for (int i = 0; i < count; i++)
            {              
                endLT = heading1.IndexOf("]<a") - 1;
                startLT = heading1.LastIndexOf('[', endLT) + 1; 
                startWA = heading1.IndexOf('>', endLT) + 1;
                endWA = heading1.IndexOf("</a", startWA) - 1;
                webAddress = heading1.Substring(startWA, endWA - startWA + 1);
                linkText = heading1.Substring(startLT, endLT - startLT + 1);
                if (webAddress != linkText)
                {
                    heading1 = heading1.Remove(startWA, endWA - startWA + 1);
                    heading1 = heading1.Insert(startWA, linkText);
                    heading1 = heading1.Remove(startLT - 1, endLT - startLT + 3);;
                }
            }
        }
        if (heading2.Contains("]<a"))
        {
            count = Regex.Matches(heading2, @"]<a").Count;
            for (int i = 0; i < count; i++)
            {
                endLT = heading2.IndexOf("]<a") - 1;
                startLT = heading2.LastIndexOf('[', endLT) + 1;
                startWA = heading2.IndexOf('>', endLT) + 1;
                endWA = heading2.IndexOf("</a", startWA) - 1;
                webAddress = heading2.Substring(startWA, endWA - startWA + 1);
                linkText = heading2.Substring(startLT, endLT - startLT + 1);
                if (webAddress != linkText)
                {
                    heading2 = heading2.Remove(startWA, endWA - startWA + 1);
                    heading2 = heading2.Insert(startWA, linkText);
                    heading2 = heading2.Remove(startLT - 1, endLT - startLT + 3); ;
                }
            }
        }

        if (paragraph1.Contains("]<a"))
        {
            count = Regex.Matches(paragraph1, @"]<a").Count;
            for (int i = 0; i < count; i++)
            {
                endLT = paragraph1.IndexOf("]<a") - 1;
                startLT = paragraph1.LastIndexOf('[', endLT) + 1;
                startWA = paragraph1.IndexOf('>', endLT) + 1;
                endWA = paragraph1.IndexOf("</a", startWA) - 1;
                webAddress = paragraph1.Substring(startWA, endWA - startWA + 1);
                linkText = paragraph1.Substring(startLT, endLT - startLT + 1);
                if (webAddress != linkText)
                {
                    paragraph1 = paragraph1.Remove(startWA, endWA - startWA + 1);
                    paragraph1 = paragraph1.Insert(startWA, linkText);
                    paragraph1 = paragraph1.Remove(startLT - 1, endLT - startLT + 3); ;
                }
            }
        }
        if (paragraph2.Contains("]<a"))
        {
            count = Regex.Matches(paragraph2, @"]<a").Count;
            for (int i = 0; i < count; i++)
            {
                endLT = paragraph2.IndexOf("]<a") - 1;
                startLT = paragraph2.LastIndexOf('[', endLT) + 1;
                startWA = paragraph2.IndexOf('>', endLT) + 1;
                endWA = paragraph2.IndexOf("</a", startWA) - 1;
                webAddress = paragraph2.Substring(startWA, endWA - startWA + 1);
                linkText = paragraph2.Substring(startLT, endLT - startLT + 1);
                if (webAddress != linkText)
                {
                    paragraph2 = paragraph2.Remove(startWA, endWA - startWA + 1);
                    paragraph2 = paragraph2.Insert(startWA, linkText);
                    paragraph2 = paragraph2.Remove(startLT - 1, endLT - startLT + 3); ;
                }
            }
        }
        */
        

        // create the bullit points
        char bullet = (char)0x2022;
        if (!cbBullet1.Checked && cbIndent1.Checked)
            paragraph1 = "<blockquote>" + paragraph1 + "</blockquote>";
        else if (cbBullet1.Checked && cbIndent1.Checked)
            paragraph1 = "<ul><li>" + paragraph1.Remove(paragraph1.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet2.Checked && cbIndent2.Checked)
            paragraph2 = "<blockquote>" + paragraph2 + "</blockquote>";
        else if (cbBullet2.Checked && cbIndent2.Checked)
            paragraph2 = "<ul><li>" + paragraph2.Remove(paragraph2.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet3.Checked && cbIndent3.Checked)
            paragraph3 = "<blockquote>" + paragraph3 + "</blockquote>";
        else if (cbBullet3.Checked && cbIndent3.Checked)
            paragraph3 = "<ul><li>" + paragraph3.Remove(paragraph3.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet4.Checked && cbIndent4.Checked)
            paragraph4 = "<blockquote>" + paragraph4 + "</blockquote>";
        else if (cbBullet4.Checked && cbIndent4.Checked)
            paragraph4 = "<ul><li>" + paragraph4.Remove(paragraph4.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet5.Checked && cbIndent5.Checked)
            paragraph5 = "<blockquote>" + paragraph5 + "</blockquote>";
        else if (cbBullet5.Checked && cbIndent5.Checked)
            paragraph5 = "<ul><li>" + paragraph5.Remove(paragraph5.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet6.Checked && cbIndent6.Checked)
            paragraph6 = "<blockquote>" + paragraph6 + "</blockquote>";
        else if (cbBullet6.Checked && cbIndent6.Checked)
            paragraph6 = "<ul><li>" + paragraph6.Remove(paragraph6.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet7.Checked && cbIndent7.Checked)
            paragraph7 = "<blockquote>" + paragraph7 + "</blockquote>";
        else if (cbBullet7.Checked && cbIndent7.Checked)
            paragraph7 = "<ul><li>" + paragraph7.Remove(paragraph7.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet8.Checked && cbIndent8.Checked)
            paragraph8 = "<blockquote>" + paragraph8 + "</blockquote>";
        else if (cbBullet8.Checked && cbIndent8.Checked)
            paragraph8 = "<ul><li>" + paragraph8.Remove(paragraph8.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet9.Checked && cbIndent9.Checked)
            paragraph9 = "<blockquote>" + paragraph9 + "</blockquote>";
        else if (cbBullet9.Checked && cbIndent9.Checked)
            paragraph9 = "<ul><li>" + paragraph9.Remove(paragraph9.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet10.Checked && cbIndent10.Checked)
            paragraph10 = "<blockquote>" + paragraph10 + "</blockquote>";
        else if (cbBullet10.Checked && cbIndent10.Checked)
            paragraph10 = "<ul><li>" + paragraph10.Remove(paragraph10.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet11.Checked && cbIndent11.Checked)
            paragraph11 = "<blockquote>" + paragraph11 + "</blockquote>";
        else if (cbBullet11.Checked && cbIndent11.Checked)
            paragraph11 = "<ul><li>" + paragraph11.Remove(paragraph11.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet12.Checked && cbIndent12.Checked)
            paragraph12 = "<blockquote>" + paragraph12 + "</blockquote>";
        else if (cbBullet12.Checked && cbIndent12.Checked)
            paragraph12 = "<ul><li>" + paragraph12.Remove(paragraph12.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet13.Checked && cbIndent13.Checked)
            paragraph13 = "<blockquote>" + paragraph13 + "</blockquote>";
        else if (cbBullet13.Checked && cbIndent13.Checked)
            paragraph13 = "<ul><li>" + paragraph13.Remove(paragraph13.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet14.Checked && cbIndent14.Checked)
            paragraph14 = "<blockquote>" + paragraph14 + "</blockquote>";
        else if (cbBullet14.Checked && cbIndent14.Checked)
            paragraph14 = "<ul><li>" + paragraph14.Remove(paragraph14.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet15.Checked && cbIndent15.Checked)
            paragraph15 = "<blockquote>" + paragraph15 + "</blockquote>";
        else if (cbBullet15.Checked && cbIndent15.Checked)
            paragraph15 = "<ul><li>" + paragraph15.Remove(paragraph15.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet16.Checked && cbIndent16.Checked)
            paragraph16 = "<blockquote>" + paragraph16 + "</blockquote>";
        else if (cbBullet16.Checked && cbIndent16.Checked)
            paragraph16 = "<ul><li>" + paragraph16.Remove(paragraph16.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet17.Checked && cbIndent17.Checked)
            paragraph17 = "<blockquote>" + paragraph17 + "</blockquote>";
        else if (cbBullet17.Checked && cbIndent17.Checked)
            paragraph17 = "<ul><li>" + paragraph17.Remove(paragraph17.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet18.Checked && cbIndent18.Checked)
            paragraph18 = "<blockquote>" + paragraph18 + "</blockquote>";
        else if (cbBullet18.Checked && cbIndent18.Checked)
            paragraph18 = "<ul><li>" + paragraph18.Remove(paragraph18.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet19.Checked && cbIndent19.Checked)
            paragraph19 = "<blockquote>" + paragraph19 + "</blockquote>";
        else if (cbBullet19.Checked && cbIndent19.Checked)
            paragraph19 = "<ul><li>" + paragraph19.Remove(paragraph19.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }
        if (!cbBullet20.Checked && cbIndent20.Checked)
            paragraph20 = "<blockquote>" + paragraph20 + "</blockquote>";
        else if (cbBullet20.Checked && cbIndent20.Checked)
            paragraph20 = "<ul><li>" + paragraph20.Remove(paragraph20.IndexOf(bullet), 1).Trim() + "</li></ul>";
        else { }


        


        /*
        char bullet = (char)0x2022;
        if (paragraph1.Contains(bullet))
            paragraph1 = "<ul><li>" + paragraph1.Remove(paragraph1.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph2.Contains(bullet))
            paragraph2 = "<ul><li>" + paragraph2.Remove(paragraph2.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph3.Contains(bullet))
            paragraph3 = "<ul><li>" + paragraph3.Remove(paragraph3.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph4.Contains(bullet))
            paragraph4 = "<ul><li>" + paragraph4.Remove(paragraph4.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph5.Contains(bullet))
            paragraph5 = "<ul><li>" + paragraph5.Remove(paragraph5.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph6.Contains(bullet))
            paragraph6 = "<ul><li>" + paragraph6.Remove(paragraph6.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph7.Contains(bullet))
            paragraph7 = "<ul><li>" + paragraph7.Remove(paragraph7.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph8.Contains(bullet))
            paragraph8 = "<ul><li>" + paragraph8.Remove(paragraph8.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph9.Contains(bullet))
            paragraph9 = "<ul><li>" + paragraph9.Remove(paragraph9.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph10.Contains(bullet))
            paragraph10 = "<ul><li>" + paragraph10.Remove(paragraph10.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph11.Contains(bullet))
            paragraph11 = "<ul><li>" + paragraph11.Remove(paragraph11.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph12.Contains(bullet))
            paragraph12 = "<ul><li>" + paragraph12.Remove(paragraph12.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph13.Contains(bullet))
            paragraph13 = "<ul><li>" + paragraph13.Remove(paragraph13.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph14.Contains(bullet))
            paragraph14 = "<ul><li>" + paragraph14.Remove(paragraph14.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph15.Contains(bullet))
            paragraph15 = "<ul><li>" + paragraph15.Remove(paragraph15.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph16.Contains(bullet))
            paragraph16 = "<ul><li>" + paragraph16.Remove(paragraph16.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph17.Contains(bullet))
            paragraph17 = "<ul><li>" + paragraph17.Remove(paragraph17.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph18.Contains(bullet))
            paragraph18 = "<ul><li>" + paragraph18.Remove(paragraph18.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph19.Contains(bullet))
            paragraph19 = "<ul><li>" + paragraph19.Remove(paragraph19.IndexOf(bullet), 1).Trim() + "</li></ul>";
        if (paragraph20.Contains(bullet))
            paragraph20 = "<ul><li>" + paragraph20.Remove(paragraph20.IndexOf(bullet), 1).Trim() + "</li></ul>";
        */

        // if a heading is left blank leave out the html
        if (heading1 != "") heading1 = "<p><b>" + heading1 + "</b></p>";
        if (heading2 != "") heading2 = "<p><b>" + heading2 + "</b></p>";
        if (heading3 != "") heading3 = "<p><b>" + heading3 + "</b></p>";
        if (heading4 != "") heading4 = "<p><b>" + heading4 + "</b></p>";
        if (heading5 != "") heading5 = "<p><b>" + heading5 + "</b></p>";
        if (heading6 != "") heading6 = "<p><b>" + heading6 + "</b></p>";
        if (heading7 != "") heading7 = "<p><b>" + heading7 + "</b></p>";
        if (heading8 != "") heading8 = "<p><b>" + heading8 + "</b></p>";
        if (heading9 != "") heading9 = "<p><b>" + heading9 + "</b></p>";
        if (heading10 != "") heading10 = "<p><b>" + heading10 + "</b></p>";
        if (heading11 != "") heading11 = "<p><b>" + heading11 + "</b></p>";
        if (heading12 != "") heading12 = "<p><b>" + heading12 + "</b></p>";
        if (heading13 != "") heading13 = "<p><b>" + heading13 + "</b></p>";
        if (heading14 != "") heading14 = "<p><b>" + heading14 + "</b></p>";
        if (heading15 != "") heading15 = "<p><b>" + heading15 + "</b></p>";
        if (heading16 != "") heading16 = "<p><b>" + heading16 + "</b></p>";
        if (heading17 != "") heading17 = "<p><b>" + heading17 + "</b></p>";
        if (heading18 != "") heading18 = "<p><b>" + heading18 + "</b></p>";
        if (heading19 != "") heading19 = "<p><b>" + heading19 + "</b></p>";
        if (heading20 != "") heading20 = "<p><b>" + heading20 + "</b></p>";

        // save what's left
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDSaveIntroductionData", lblWebDB_ID.Text,
            heading1,
            heading2,
            heading3,
            heading4,
            heading5,
            heading6,
            heading7,
            heading8,
            heading9,
            heading10,
            heading11,
            heading12,
            heading13,
            heading14,
            heading15,
            heading16,
            heading17,
            heading18,
            heading19,
            heading20,
            "<p>" + paragraph1 + "</p>",
            "<p>" + paragraph2 + "</p>",
            "<p>" + paragraph3 + "</p>",
            "<p>" + paragraph4 + "</p>",
            "<p>" + paragraph5 + "</p>",
            "<p>" + paragraph6 + "</p>",
            "<p>" + paragraph7 + "</p>",
            "<p>" + paragraph8 + "</p>",
            "<p>" + paragraph9 + "</p>",
            "<p>" + paragraph10 + "</p>",
            "<p>" + paragraph11 + "</p>",
            "<p>" + paragraph12 + "</p>",
            "<p>" + paragraph13 + "</p>",
            "<p>" + paragraph14 + "</p>",
            "<p>" + paragraph15 + "</p>",
            "<p>" + paragraph16 + "</p>",
            "<p>" + paragraph17 + "</p>",
            "<p>" + paragraph18 + "</p>",
            "<p>" + paragraph19 + "</p>",
            "<p>" + paragraph20 + "</p>");
         
    }
    protected void cmdSaveAdmin_Click(object sender, EventArgs e)
    {
        bool isAdmDB = true;
        Utils.ExecuteSP(isAdmDB, Server, "st_WDSaveIntroduction", lblWebDB_ID.Text, "Edit introduction text", RadEditor.Content /*Editor1.ContentHTML**/);
    }
    protected void cbUserEditIntro_CheckedChanged(object sender, EventArgs e)
    {
        if (cbUserEditIntro.Text == "View user edit intro")
        {
            pnlUserEdit.Visible = true;
            cbUserEditIntro.Text = "Hide user edit intro";
            cbUserEditIntro.Checked = false;
        }
        else
        {
            pnlUserEdit.Visible = false;
            cbUserEditIntro.Text = "View user edit intro";
            cbUserEditIntro.Checked = false;
        }
    }
    protected void cbIndent1_CheckedChanged(object sender, EventArgs e)
    {
        /*
        char bullet = (char)0x2022;
        if (cbIndent1.Checked == true)
        {
            tbParagraph1.Text = "   " + bullet + " " + tbParagraph1.Text;
        }
        else
        {
            tbParagraph1.Text = tbParagraph1.Text.Remove(0, 5);
        }
        */
        char bullet = (char)0x2022;
        if (cbIndent1.Checked == true)
        {
            tbParagraph1.CssClass = "IndentedTextBox";
            cbBullet1.Enabled = true;
        }
        else
        {
            tbParagraph1.CssClass = "ParagraphBox";
            cbBullet1.Checked = false;
            cbBullet1.Enabled = false;
            if (tbParagraph1.Text.Contains(bullet))
                tbParagraph1.Text = tbParagraph1.Text.Remove(tbParagraph1.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet1_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet1.Checked == true)
        {
            tbParagraph1.Text = bullet + " " + tbParagraph1.Text;
        }
        else
        {
            tbParagraph1.Text = tbParagraph1.Text.Remove(tbParagraph1.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent2_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent2.Checked == true)
        {
            tbParagraph2.CssClass = "IndentedTextBox";
            cbBullet2.Enabled = true;
        }
        else
        {
            tbParagraph2.CssClass = "ParagraphBox";
            cbBullet2.Checked = false;
            cbBullet2.Enabled = false;
            if (tbParagraph2.Text.Contains(bullet))
                tbParagraph2.Text = tbParagraph2.Text.Remove(tbParagraph2.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet2_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet2.Checked == true)
        {
            tbParagraph2.Text = bullet + " " + tbParagraph2.Text;
        }
        else
        {
            tbParagraph2.Text = tbParagraph2.Text.Remove(tbParagraph2.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent3_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent3.Checked == true)
        {
            tbParagraph3.CssClass = "IndentedTextBox";
            cbBullet3.Enabled = true;
        }
        else
        {
            tbParagraph3.CssClass = "ParagraphBox";
            cbBullet3.Checked = false;
            cbBullet3.Enabled = false;
            if (tbParagraph3.Text.Contains(bullet))
                tbParagraph3.Text = tbParagraph3.Text.Remove(tbParagraph3.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet3_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet3.Checked == true)
        {
            tbParagraph3.Text = bullet + " " + tbParagraph3.Text;
        }
        else
        {
            tbParagraph3.Text = tbParagraph3.Text.Remove(tbParagraph3.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent4_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent4.Checked == true)
        {
            tbParagraph4.CssClass = "IndentedTextBox";
            cbBullet4.Enabled = true;
        }
        else
        {
            tbParagraph4.CssClass = "ParagraphBox";
            cbBullet4.Checked = false;
            cbBullet4.Enabled = false;
            if (tbParagraph4.Text.Contains(bullet))
                tbParagraph4.Text = tbParagraph4.Text.Remove(tbParagraph4.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet4_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet4.Checked == true)
        {
            tbParagraph4.Text = bullet + " " + tbParagraph4.Text;
        }
        else
        {
            tbParagraph4.Text = tbParagraph4.Text.Remove(tbParagraph4.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent5_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent5.Checked == true)
        {
            tbParagraph5.CssClass = "IndentedTextBox";
            cbBullet5.Enabled = true;
        }
        else
        {
            tbParagraph5.CssClass = "ParagraphBox";
            cbBullet5.Checked = false;
            cbBullet5.Enabled = false;
            if (tbParagraph5.Text.Contains(bullet))
                tbParagraph5.Text = tbParagraph5.Text.Remove(tbParagraph5.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet5_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet5.Checked == true)
        {
            tbParagraph5.Text = bullet + " " + tbParagraph5.Text;
        }
        else
        {
            tbParagraph5.Text = tbParagraph5.Text.Remove(tbParagraph5.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent6_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent6.Checked == true)
        {
            tbParagraph6.CssClass = "IndentedTextBox";
            cbBullet6.Enabled = true;
        }
        else
        {
            tbParagraph6.CssClass = "ParagraphBox";
            cbBullet6.Checked = false;
            cbBullet6.Enabled = false;
            if (tbParagraph6.Text.Contains(bullet))
                tbParagraph6.Text = tbParagraph6.Text.Remove(tbParagraph6.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet6_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet6.Checked == true)
        {
            tbParagraph6.Text = bullet + " " + tbParagraph6.Text;
        }
        else
        {
            tbParagraph6.Text = tbParagraph6.Text.Remove(tbParagraph6.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent7_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent7.Checked == true)
        {
            tbParagraph7.CssClass = "IndentedTextBox";
            cbBullet7.Enabled = true;
        }
        else
        {
            tbParagraph7.CssClass = "ParagraphBox";
            cbBullet7.Checked = false;
            cbBullet7.Enabled = false;
            if (tbParagraph7.Text.Contains(bullet))
                tbParagraph7.Text = tbParagraph7.Text.Remove(tbParagraph7.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet7_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet7.Checked == true)
        {
            tbParagraph7.Text = bullet + " " + tbParagraph7.Text;
        }
        else
        {
            tbParagraph7.Text = tbParagraph7.Text.Remove(tbParagraph7.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent8_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent8.Checked == true)
        {
            tbParagraph8.CssClass = "IndentedTextBox";
            cbBullet8.Enabled = true;
        }
        else
        {
            tbParagraph8.CssClass = "ParagraphBox";
            cbBullet8.Checked = false;
            cbBullet8.Enabled = false;
            if (tbParagraph8.Text.Contains(bullet))
                tbParagraph8.Text = tbParagraph8.Text.Remove(tbParagraph8.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet8_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet8.Checked == true)
        {
            tbParagraph8.Text = bullet + " " + tbParagraph8.Text;
        }
        else
        {
            tbParagraph8.Text = tbParagraph8.Text.Remove(tbParagraph8.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent9_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent9.Checked == true)
        {
            tbParagraph9.CssClass = "IndentedTextBox";
            cbBullet9.Enabled = true;
        }
        else
        {
            tbParagraph9.CssClass = "ParagraphBox";
            cbBullet9.Checked = false;
            cbBullet9.Enabled = false;
            if (tbParagraph9.Text.Contains(bullet))
                tbParagraph9.Text = tbParagraph9.Text.Remove(tbParagraph9.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet9_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet9.Checked == true)
        {
            tbParagraph9.Text = bullet + " " + tbParagraph9.Text;
        }
        else
        {
            tbParagraph9.Text = tbParagraph9.Text.Remove(tbParagraph9.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent10_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent10.Checked == true)
        {
            tbParagraph10.CssClass = "IndentedTextBox";
            cbBullet10.Enabled = true;
        }
        else
        {
            tbParagraph10.CssClass = "ParagraphBox";
            cbBullet10.Checked = false;
            cbBullet10.Enabled = false;
            if (tbParagraph10.Text.Contains(bullet))
                tbParagraph10.Text = tbParagraph10.Text.Remove(tbParagraph10.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet10_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet10.Checked == true)
        {
            tbParagraph10.Text = bullet + " " + tbParagraph10.Text;
        }
        else
        {
            tbParagraph10.Text = tbParagraph10.Text.Remove(tbParagraph10.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent11_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent11.Checked == true)
        {
            tbParagraph11.CssClass = "IndentedTextBox";
            cbBullet11.Enabled = true;
        }
        else
        {
            tbParagraph11.CssClass = "ParagraphBox";
            cbBullet11.Checked = false;
            cbBullet11.Enabled = false;
            if (tbParagraph11.Text.Contains(bullet))
                tbParagraph11.Text = tbParagraph11.Text.Remove(tbParagraph11.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet11_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet11.Checked == true)
        {
            tbParagraph11.Text = bullet + " " + tbParagraph11.Text;
        }
        else
        {
            tbParagraph11.Text = tbParagraph11.Text.Remove(tbParagraph11.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent12_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent12.Checked == true)
        {
            tbParagraph12.CssClass = "IndentedTextBox";
            cbBullet12.Enabled = true;
        }
        else
        {
            tbParagraph12.CssClass = "ParagraphBox";
            cbBullet12.Checked = false;
            cbBullet12.Enabled = false;
            if (tbParagraph12.Text.Contains(bullet))
                tbParagraph12.Text = tbParagraph12.Text.Remove(tbParagraph12.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet12_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet12.Checked == true)
        {
            tbParagraph12.Text = bullet + " " + tbParagraph12.Text;
        }
        else
        {
            tbParagraph12.Text = tbParagraph12.Text.Remove(tbParagraph12.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent13_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent13.Checked == true)
        {
            tbParagraph13.CssClass = "IndentedTextBox";
            cbBullet13.Enabled = true;
        }
        else
        {
            tbParagraph13.CssClass = "ParagraphBox";
            cbBullet13.Checked = false;
            cbBullet13.Enabled = false;
            if (tbParagraph13.Text.Contains(bullet))
                tbParagraph13.Text = tbParagraph13.Text.Remove(tbParagraph13.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet13_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet13.Checked == true)
        {
            tbParagraph13.Text = bullet + " " + tbParagraph13.Text;
        }
        else
        {
            tbParagraph13.Text = tbParagraph13.Text.Remove(tbParagraph13.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent14_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent14.Checked == true)
        {
            tbParagraph14.CssClass = "IndentedTextBox";
            cbBullet14.Enabled = true;
        }
        else
        {
            tbParagraph14.CssClass = "ParagraphBox";
            cbBullet14.Checked = false;
            cbBullet14.Enabled = false;
            if (tbParagraph14.Text.Contains(bullet))
                tbParagraph14.Text = tbParagraph14.Text.Remove(tbParagraph14.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet14_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet14.Checked == true)
        {
            tbParagraph14.Text = bullet + " " + tbParagraph14.Text;
        }
        else
        {
            tbParagraph14.Text = tbParagraph14.Text.Remove(tbParagraph14.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent15_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent15.Checked == true)
        {
            tbParagraph15.CssClass = "IndentedTextBox";
            cbBullet15.Enabled = true;
        }
        else
        {
            tbParagraph15.CssClass = "ParagraphBox";
            cbBullet15.Checked = false;
            cbBullet15.Enabled = false;
            if (tbParagraph15.Text.Contains(bullet))
                tbParagraph15.Text = tbParagraph15.Text.Remove(tbParagraph15.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet15_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet15.Checked == true)
        {
            tbParagraph15.Text = bullet + " " + tbParagraph15.Text;
        }
        else
        {
            tbParagraph15.Text = tbParagraph15.Text.Remove(tbParagraph15.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent16_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent16.Checked == true)
        {
            tbParagraph16.CssClass = "IndentedTextBox";
            cbBullet16.Enabled = true;
        }
        else
        {
            tbParagraph16.CssClass = "ParagraphBox";
            cbBullet16.Checked = false;
            cbBullet16.Enabled = false;
            if (tbParagraph16.Text.Contains(bullet))
                tbParagraph16.Text = tbParagraph16.Text.Remove(tbParagraph16.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet16_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet16.Checked == true)
        {
            tbParagraph16.Text = bullet + " " + tbParagraph16.Text;
        }
        else
        {
            tbParagraph16.Text = tbParagraph16.Text.Remove(tbParagraph16.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent17_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent17.Checked == true)
        {
            tbParagraph17.CssClass = "IndentedTextBox";
            cbBullet17.Enabled = true;
        }
        else
        {
            tbParagraph17.CssClass = "ParagraphBox";
            cbBullet17.Checked = false;
            cbBullet17.Enabled = false;
            if (tbParagraph17.Text.Contains(bullet))
                tbParagraph17.Text = tbParagraph17.Text.Remove(tbParagraph17.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet17_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet17.Checked == true)
        {
            tbParagraph17.Text = bullet + " " + tbParagraph17.Text;
        }
        else
        {
            tbParagraph17.Text = tbParagraph17.Text.Remove(tbParagraph17.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent18_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent18.Checked == true)
        {
            tbParagraph18.CssClass = "IndentedTextBox";
            cbBullet18.Enabled = true;
        }
        else
        {
            tbParagraph18.CssClass = "ParagraphBox";
            cbBullet18.Checked = false;
            cbBullet18.Enabled = false;
            if (tbParagraph18.Text.Contains(bullet))
                tbParagraph18.Text = tbParagraph18.Text.Remove(tbParagraph18.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet18_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet18.Checked == true)
        {
            tbParagraph18.Text = bullet + " " + tbParagraph18.Text;
        }
        else
        {
            tbParagraph18.Text = tbParagraph18.Text.Remove(tbParagraph18.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent19_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent19.Checked == true)
        {
            tbParagraph19.CssClass = "IndentedTextBox";
            cbBullet19.Enabled = true;
        }
        else
        {
            tbParagraph19.CssClass = "ParagraphBox";
            cbBullet19.Checked = false;
            cbBullet19.Enabled = false;
            if (tbParagraph19.Text.Contains(bullet))
                tbParagraph19.Text = tbParagraph19.Text.Remove(tbParagraph19.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet19_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet19.Checked == true)
        {
            tbParagraph19.Text = bullet + " " + tbParagraph19.Text;
        }
        else
        {
            tbParagraph19.Text = tbParagraph19.Text.Remove(tbParagraph19.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbIndent20_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbIndent20.Checked == true)
        {
            tbParagraph20.CssClass = "IndentedTextBox";
            cbBullet20.Enabled = true;
        }
        else
        {
            tbParagraph20.CssClass = "ParagraphBox";
            cbBullet20.Checked = false;
            cbBullet20.Enabled = false;
            if (tbParagraph20.Text.Contains(bullet))
                tbParagraph20.Text = tbParagraph20.Text.Remove(tbParagraph20.Text.IndexOf(bullet), 2);
        }
    }
    protected void cbBullet20_CheckedChanged(object sender, EventArgs e)
    {
        char bullet = (char)0x2022;
        if (cbBullet20.Checked == true)
        {
            tbParagraph20.Text = bullet + " " + tbParagraph20.Text;
        }
        else
        {
            tbParagraph20.Text = tbParagraph20.Text.Remove(tbParagraph20.Text.IndexOf(bullet), 2);
        }
    }
    private string removeHtmlLinks(string introText, string target)
    {
        // find the webAddress between '<a href="' and '"'
        // find the linktext (between '>' and '</a>
        // remove the html between '<a' and '>'
        // if webAddress != linktext
        //      then place [linkText] in front of webAddress
        
        string linkText = "";
        string webAddress = "";
        int startWA = 0;
        int endWA = 0;
        int startLT = 0;
        int endLT = 0;
        int count = 0;
        char bullet = (char)0x2022;

        introText = introText.Replace("<p><b>", "").Replace("</b>", "").Replace("</p>", "");
        introText = introText.Replace("<p>", "").Replace("</p>", "");
        if (introText.Contains("<ul>"))
        {
            introText = introText.Replace("<ul>", "").Replace("</ul>", "").Replace("<li>", "").Replace("</li>", "");
            introText = "   " + bullet + " " + introText;
            if (target == "p1") {
                cbIndent1.Checked = true; cbBullet1.Checked = true;
            }
            else if (target == "p2")
                cbIndent2.Checked = true;
            else if (target == "p3")
                cbIndent3.Checked = true;
            else if (target == "p4")
                cbIndent4.Checked = true;
            else if (target == "p5")
                cbIndent5.Checked = true;
            else if (target == "p6")
                cbIndent6.Checked = true;
            else if (target == "p7")
                cbIndent7.Checked = true;
            else if (target == "p8")
                cbIndent8.Checked = true;
            else if (target == "p9")
                cbIndent9.Checked = true;
            else if (target == "p10")
                cbIndent10.Checked = true;
            else if (target == "p11")
                cbIndent11.Checked = true;
            else if (target == "p12")
                cbIndent12.Checked = true;
            else if (target == "p13")
                cbIndent13.Checked = true;
            else if (target == "p14")
                cbIndent14.Checked = true;
            else if (target == "p15")
                cbIndent15.Checked = true;
            else if (target == "p16")
                cbIndent16.Checked = true;
            else if (target == "p17")
                cbIndent17.Checked = true;
            else if (target == "p18")
                cbIndent18.Checked = true;
            else if (target == "p19")
                cbIndent19.Checked = true;
            else cbIndent20.Checked = true;
        }
        if (introText.Contains("<blockquote>"))
        {
            introText = introText.Replace("<blockquote>", "").Replace("</blockquote>", "");
            if (target == "p1") {
                cbIndent1.Checked = true; tbParagraph1.CssClass = "IndentedTextBox";
            }
            else if (target == "p2")
            {
                cbIndent2.Checked = true; tbParagraph2.CssClass = "IndentedTextBox";
            }
            else if (target == "p3")
            {
                cbIndent3.Checked = true; tbParagraph3.CssClass = "IndentedTextBox";
            }
            else if (target == "p4")
            {
                cbIndent4.Checked = true; tbParagraph4.CssClass = "IndentedTextBox";
            }
            else if (target == "p5")
            {
                cbIndent5.Checked = true; tbParagraph5.CssClass = "IndentedTextBox";
            }
            else if (target == "p6")
            {
                cbIndent6.Checked = true; tbParagraph6.CssClass = "IndentedTextBox";
            }
            else if (target == "p7")
            {
                cbIndent7.Checked = true; tbParagraph7.CssClass = "IndentedTextBox";
            }
            else if (target == "p8")
            {
                cbIndent8.Checked = true; tbParagraph8.CssClass = "IndentedTextBox";
            }
            else if (target == "p9")
            {
                cbIndent9.Checked = true; tbParagraph9.CssClass = "IndentedTextBox";
            }
            else if (target == "p10")
                cbIndent10.Checked = true;
            else if (target == "p11")
                cbIndent11.Checked = true;
            else if (target == "p12")
                cbIndent12.Checked = true;
            else if (target == "p13")
                cbIndent13.Checked = true;
            else if (target == "p14")
                cbIndent14.Checked = true;
            else if (target == "p15")
                cbIndent15.Checked = true;
            else if (target == "p16")
                cbIndent16.Checked = true;
            else if (target == "p17")
                cbIndent17.Checked = true;
            else if (target == "p18")
                cbIndent18.Checked = true;
            else if (target == "p19")
                cbIndent19.Checked = true;
            else cbIndent20.Checked = true;
        }
        if (introText.Contains("</a>"))
        {
            // there could be multiple links so deal with each one
            count = Regex.Matches(introText, "</a>").Count;
            for (int i = 0; i < count; i++)
            {
                startWA = introText.IndexOf("<a href=") + 9;
                endWA = introText.IndexOf('"', startWA);
                startLT = introText.IndexOf('>');
                endLT = introText.IndexOf("</a>");
                webAddress = introText.Substring(startWA, endWA - startWA);
                linkText = introText.Substring(startLT + 1, endLT - startLT - 1);
                introText = introText.Remove(endLT, 4);
                if (webAddress != linkText)
                {
                    introText = introText.Remove(startLT + 1, linkText.Length);
                    introText = introText.Insert(startLT + 1, webAddress);
                    introText = introText.Insert(startLT + 1, "[" + linkText + "]");
                }
                introText = introText.Remove(startWA - 9, startLT - startWA + 10);
            }
        }
        return introText;
    }
    private string addHtmlLinks(string introText)
    {
        // find the webAddress between '<a href="' and '"'
        // find the linktext (between '>' and '</a>
        // remove the html between '<a' and '>'
        // if webAddress != linktext
        //      then place [linkText] in front of webAddress

        string linkText = "";
        string webAddress = "";
        int startWA = 0;
        int endWA = 0;
        int startLT = 0;
        int endLT = 0;
        int count = 0;
        //char bullet = (char)0x2022;

        if (introText.Contains("]<a"))
        {
            count = Regex.Matches(introText, @"]<a").Count;
            for (int i = 0; i < count; i++)
            {
                endLT = introText.IndexOf("]<a") - 1;
                startLT = introText.LastIndexOf('[', endLT) + 1;
                startWA = introText.IndexOf('>', endLT) + 1;
                endWA = introText.IndexOf("</a", startWA) - 1;
                webAddress = introText.Substring(startWA, endWA - startWA + 1);
                linkText = introText.Substring(startLT, endLT - startLT + 1);
                if (webAddress != linkText)
                {
                    introText = introText.Remove(startWA, endWA - startWA + 1);
                    introText = introText.Insert(startWA, linkText);
                    introText = introText.Remove(startLT - 1, endLT - startLT + 3); ;
                }
            }
        }
        return introText;
    }
}