using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using RefParsingLib;

namespace EppiReviewer4
{
    public partial class ReferenceSearchPage : UserControl
    {
        RefParser RP;
        public ReferenceSearchPage()
        {
            InitializeComponent();
            helpTXT.Text = @"This page is designed to search the internet for references found in the bibliography of a given Item.
To use it:
1)	(copy &) paste references list into the big textbox on the left side & Click “GO”. The program will try to automatically separate the references and display the result on the top right corner.
2)	If you don’t like the separation result (this is possible, since the separation system can't be 100% precise), modify the input text following the TIPS (below)
3)	Click on a (isolated) reference from the list on the top right corner. This will update the list of links below.
4)	Click on the links: a new page will be created with the result of different search strings generated from the selected reference.

TIPS:
The quickest way to adjust results is to insert an empty line between troublesome references; there is no need to put empty lines between references that are separated correctly.
The system is also very reliable when the list has one and only one reference per line (every carriage return is interpreted as the start of a new reference).
The other two ways to help the separation are: 
(1) remove unwanted line-breaks (useful when a reference is ended prematurely in the result); 
(2) Play with (add or remove) final full-stops – the system detects lines that end with a full-stop and considers them as a likely end of a reference.
";
            
                //helpTXT.Style 
           
        }

        private void btGo_Click(object sender, RoutedEventArgs e)
        {
            isEn.IsEnabled = false;
            Busy.IsRunning = !isEn.IsEnabled;
            RP = new RefParser(inTxt.Text);
            RP.ReferenceSeparationComplete += new RefParser.RefSeparationCompleteEH(RP_ReferenceSeparationComplete);
            RP.StartRefSeparation();
            
            
        }
        private void RP_ReferenceSeparationComplete(object sender, RefParser.RefSeparationCompleteEA e)
        {
            isEn.IsEnabled = true;
            Busy.IsRunning = !isEn.IsEnabled;
            if (e.Message == "OK")
            {
                LBresult.ItemsSource = e.result;
            }
            else LBresult.ItemsSource = null;
        }
        private void LBresult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*foreach (string LBI in LBresult.SelectedItems)
            {
                RP.GetSearchString(LBI);
            }
            */
            string s = "", s1 = "", s2 = "", s3 = "";
            if (LBresult.SelectedItem != null)
            {
                s = RP.GetSearchString(LBresult.SelectedItem.ToString(), false);
                s1 = RP.GetSearchString(LBresult.SelectedItem.ToString(), true);
                s2 = RP.GetSearchString2(LBresult.SelectedItem.ToString(), false);
                s3 = RP.GetSearchString2(LBresult.SelectedItem.ToString(), true);
            }
            string roarPre = @"http://roar.eprints.org/content.html?cref=http%3A%2F%2Froar.eprints.org%2Fcref_cse.xml&cof=FORID%3A9&q=";
            string roarSuff = @"&sa=Search&siteurl=roar.eprints.org%252Fcontent.html#1079";
            GoogleHL.NavigateUri = new Uri("http://www.google.co.uk/search?hl=en&q=" 
                + s, UriKind.Absolute);
            GoogleHL1.NavigateUri = new Uri("http://www.google.co.uk/search?hl=en&q=" 
                + s1, UriKind.Absolute);
            GoogleHL2.NavigateUri = new Uri("http://www.google.co.uk/search?hl=en&q=" 
                + s2, UriKind.Absolute);
            GoogleHL3.NavigateUri = new Uri("http://www.google.co.uk/search?hl=en&q="
                + s3, UriKind.Absolute);
            BinGHL.NavigateUri = new Uri("http://www.bing.com/search?q=" 
                + s, UriKind.Absolute);
            BinGHL1.NavigateUri = new Uri("http://www.bing.com/search?q=" 
                + s1, UriKind.Absolute);
            BinGHL2.NavigateUri = new Uri("http://www.bing.com/search?q=" 
                + s2, UriKind.Absolute);
            BinGHL3.NavigateUri = new Uri("http://www.bing.com/search?q="
                + s3, UriKind.Absolute);
            PubMedHL.NavigateUri = new Uri("http://www.ncbi.nlm.nih.gov/pubmed?term="
                + s, UriKind.Absolute);
            PubMedHL1.NavigateUri = new Uri("http://www.ncbi.nlm.nih.gov/pubmed?term="
                + s1, UriKind.Absolute);
            PubMedHL2.NavigateUri = new Uri("http://www.ncbi.nlm.nih.gov/pubmed?term="
                + s2, UriKind.Absolute);
            PubMedHL3.NavigateUri = new Uri("http://www.ncbi.nlm.nih.gov/pubmed?term="
                + s3, UriKind.Absolute);
            
            RoarHL.NavigateUri = new Uri(roarPre + System.Windows.Browser.HttpUtility.HtmlEncode(s)
                + roarSuff, UriKind.Absolute);
            RoarHL1.NavigateUri = new Uri(roarPre + System.Windows.Browser.HttpUtility.HtmlEncode(s1)
                + roarSuff, UriKind.Absolute);
            RoarHL2.NavigateUri = new Uri(roarPre + System.Windows.Browser.HttpUtility.HtmlEncode(s2)
                + roarSuff, UriKind.Absolute);
            RoarHL3.NavigateUri = new Uri(roarPre + System.Windows.Browser.HttpUtility.HtmlEncode(s3)
                + roarSuff, UriKind.Absolute);
            
        }
        private void RefSearchHelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (RefSearchHelpButton.Tag.ToString() == "0")
            {
                ExpRow.Height = GridLength.Auto;
                RefSearchHelpButton.Tag = "1";
            } 
            else
            {
                ExpRow.Height = new GridLength(0);
                RefSearchHelpButton.Tag = "0";
            }
        }
    }
}
