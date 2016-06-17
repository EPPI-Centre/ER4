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
using Csla.Silverlight;
using BusinessLibrary.BusinessClasses;
using System.Windows.Data;
using System.ComponentModel;
using Csla.Xaml;
using System.Text;
using System.Text.RegularExpressions;

namespace EppiReviewer4
{
    public partial class dialogItemDetails : UserControl, INotifyPropertyChanged
    {//INotifyPropertyChanged is needed to notify UI that haswriterights may have changed (rebind isEn)
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private bool CodingOnlyMode = false;
        public bool IsPriorityScreening = false;
        public void PrepareCodingOnly()
        {
            CodingOnlyMode = true;
            NotifyPropertyChanged("HasWriteRights");//coding only changes the value of haswriterights
        }
        //first bunch of lines to make the read-only UI work
        private BusinessLibrary.Security.ReviewerIdentity ri;
        public bool HasWriteRights
        {
            get
            {
                if (ri == null) return false;
                else
                {
                    return ri.HasWriteRights() && !CodingOnlyMode;//false if user does not have write rights or if control is set for coding only mode.
                }
            }
        }
        //end of read-only ui hack
        public event RoutedEventHandler UnMarkAsDuplicate;
        public event RoutedEventHandler ShowTermsClicked;
        public dialogItemDetails()
        {
            InitializeComponent();
            //two lines to make the read-only UI work
            //add <Button x:Name="isEn" IsEnabled="{Binding HasWriteRights, Mode=OneWay}" ></Button>
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            //end of read-only ui hack
        }

        private void ItemTypesData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemTypesData"]);
            if (provider.Error != null)
                MessageBox.Show(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
        }

        private void CslaDataProvider_DataChanged_ItemDuplicateListData(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemDuplicateListData"]);
            if (provider.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            else if (provider.Data != null)
            {
                string res = "";
                foreach (ItemDuplicatesReadOnly IDRO in (provider.Data as ItemDuplicatesReadOnlyList))
                {
                    res += IDRO.ItemId + ", ";
                }
                res = res.Trim(new char[] { ' ', ',' });
                DuplIDsList.Text = res;
                if (res != "") DuplIDsList.Visibility = System.Windows.Visibility.Visible;
                else DuplIDsList.Visibility = System.Windows.Visibility.Collapsed;

            }
        }

        private void CslaDataProvider_DataChanged_ItemSourceData(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemSourceData"]);
            if (provider.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            else
                TextBlockSourceName.DataContext = provider.Data;
            
        }

        private void ComboPubType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboPubType.SelectedItem != null)
            {
                int TypeId = ((ItemTypeNVL.NameValuePair)(ComboPubType.SelectedItem)).Key;
                TextBoxParentTitle.Visibility = Visibility.Collapsed;
                TextBlockParentTitle.Text = "";
                TextBlockEdition.Text = "";
                TextBoxBookAuthors.Visibility = Visibility.Collapsed;
                TextBlockBookAuthors.Visibility = Visibility.Collapsed;
                switch (TypeId)
                {
                    case 1: // report
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 2: // Book, Whole
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 3: // Book, Chapter
                        TextBoxParentTitle.Visibility = Visibility.Visible;
                        TextBlockParentTitle.Text = "Book title";
                        TextBlockEdition.Text = "Edition";
                        TextBlockBookAuthors.Visibility = Visibility.Visible;
                        TextBoxBookAuthors.Visibility = Visibility.Visible;
                        break;
                    case 4: // Dissertation
                        TextBoxParentTitle.Visibility = Visibility.Visible;
                        TextBlockParentTitle.Text = "Pub title";
                        TextBlockEdition.Text = "Thesis type";
                        break;
                    case 5: // Conference Proceedings
                        TextBoxParentTitle.Visibility = Visibility.Visible;
                        TextBlockParentTitle.Text = "Conference";
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 6: // Document From Internet Site
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 7: // Web Site
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 8: // DVD, Video, Media
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 9: // Research project
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 10: // Article In A Periodical
                        TextBoxParentTitle.Visibility = Visibility.Visible;
                        TextBlockParentTitle.Text = "Periodical";
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 11: // Interview
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 12: // Generic
                        TextBlockEdition.Text = "Edition";
                        break;
                    case 14: // Journal, Article - as 0 doesn't work
                        TextBoxParentTitle.Visibility = Visibility.Visible;
                        TextBlockParentTitle.Text = "Journal title";
                        TextBlockEdition.Text = "Edition";
                        break;
                    default:
                        break;
                }
            }
        }

        public void ShowBoldTerms()
        {
            TextBoxTitle.Visibility = Visibility.Collapsed;
            TextBoxAbstract.Visibility = Visibility.Collapsed;
            TextBlockBoldTermsAbstract.Visibility = Visibility.Visible;
            TextBlockBoldTermsTitle.Visibility = Visibility.Visible;
            BorderBoldTermsAbstract.Visibility = Visibility.Visible;
            BorderBoldTermsTitle.Visibility = Visibility.Visible;
            termsToolBar.Visibility = Visibility.Visible;
            cmdShowHideReviewerTerms.Visibility = Visibility.Visible;
        }

        public void ShowEditableBoxes()
        {
            TextBlockBoldTermsTitle.Visibility = Visibility.Collapsed;
            TextBlockBoldTermsAbstract.Visibility = Visibility.Collapsed;
            BorderBoldTermsAbstract.Visibility = Visibility.Collapsed;
            BorderBoldTermsTitle.Visibility = Visibility.Collapsed;
            TextBoxAbstract.Visibility = Visibility.Visible;
            TextBoxTitle.Visibility = Visibility.Visible;
            termsToolBar.Visibility = Visibility.Collapsed;
            cmdShowHideReviewerTerms.Visibility = Visibility.Collapsed;
        }

        //private void TextBlockURL_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    if (itemURL.Text != "")
        //    {
        //        System.Windows.Browser.HtmlPage.Window.Invoke("LoadURLNewWindow", itemURL.Text);
        //    }
        //}

        //private void TextBlockURL_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    TextBlockURL.Cursor = Cursors.Hand;
        //}

        //private void TextBlockURL_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    TextBlockURL.Cursor = Cursors.Arrow;
        //}

        private void ShowHideControlsForScreeningClarity()
        {
            if (IsPriorityScreening)
            {
                RowAuthors.Height = new GridLength(0);
                RowMonth.Height = new GridLength(0);
                cmdShowUnnecessaryRowsForScreening.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                RowAuthors.Height = new GridLength(30);
                RowMonth.Height = new GridLength(30);
                cmdShowUnnecessaryRowsForScreening.Visibility = System.Windows.Visibility.Collapsed;
                cmdHideUnnecessaryRowsForScreening.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void BindNew(Item item)
        {
            GridHost.Background = new SolidColorBrush(Colors.White);
            CheckBoxIsIncluded.IsEnabled = true;
            GetItemDuplicatesList(item);
            GetItemSource(item);
            FixURL(item.URL);
            ShowHideControlsForScreeningClarity();
        }

        public void BindTree(Item item)
        {
            GetItemDuplicatesList(item);
            GetItemSource(item);
            FixURL(item.URL);
            ShowHideControlsForScreeningClarity();
            if (item.IsItemDeleted == true)
            {
                GridHost.Background = new SolidColorBrush(Colors.LightGray);
                CheckBoxIsIncluded.IsEnabled = false;
            }
            else
            {
                GridHost.Background = new SolidColorBrush(Colors.White);
                CheckBoxIsIncluded.IsEnabled = HasWriteRights;
            }
        }
        private void itemURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;
            FixURL(tb.Text);
        }
        private void FixURL(string url)
        {
            if (url.Length < 5)
            {
                hLinkUrL.NavigateUri = null;
            }
            else
            {
                if (url.Substring(0, 7).ToLower() != "http://" && url.Substring(0, 8).ToLower() != "https://" && url.Substring(0, 6).ToLower() != "ftp://") url = "http://" + url;
                try
                {
                    hLinkUrL.NavigateUri = new Uri(url, UriKind.Absolute);
                }
                catch 
                {
                    hLinkUrL.NavigateUri = null;
                }
            }
            
        }
        private void GetItemDuplicatesList(Item item)
        {
            CslaDataProvider provider = this.Resources["ItemDuplicateListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(item.ItemId);
            provider.FactoryMethod = "getItemDuplicatesReadOnlyList";
            provider.Refresh();
        }
        private void GetItemSource(Item item)
        {
            CslaDataProvider provider = this.Resources["ItemSourceData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(item.ItemId);
            provider.FactoryMethod = "GetItemReadOnlySource";
            provider.Refresh();
        }

        private void bt_UnDuplicate_docDetails_Click(object sender, RoutedEventArgs e)
        {
            this.UnMarkAsDuplicate.Invoke(sender, e);
        }

        public static string GetparasList(RichTextBlock element)
        {
            if (element != null)
                return element.GetValue(ArticleContentProperty) as string;
            return string.Empty;
        }

        public static void SetparasList(RichTextBlock element, string value)
        {
            if (element != null)
                element.SetValue(ArticleContentProperty, value);
        }


        // based on: http://www.jevgeni.net/2011/04/19/binding-text-containing-tags-to-textblock-inlines-using-attached-property-in-silverlight-for-windows-phone-7/
        /*public static readonly DependencyProperty ArticleContentProperty =
            DependencyProperty.RegisterAttached(
                "InlineList",
                typeof(List<Inline>),
                typeof(dialogItemDetails),
                new PropertyMetadata(null, OnInlineListPropertyChanged));

        private static void OnInlineListPropertyChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var tb = obj as TextBlock;
            if (tb != null)
            {
                // clear previous inlines
                tb.Inlines.Clear();

                // add new inlines
                var inlines = e.NewValue as List<Inline>;
                if (inlines != null)
                {
                    inlines.ForEach(inl => tb.Inlines.Add((inl)));
                }
            }
        }
         */

        public static readonly DependencyProperty ArticleContentProperty =
            DependencyProperty.RegisterAttached(
                "parasList",
                typeof(List<Paragraph>),
                typeof(dialogItemDetails),
                new PropertyMetadata(null, OnparasListPropertyChanged));

        private static void OnparasListPropertyChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var tb = obj as RichTextBlock;
            if (tb != null)
            {
                // clear previous inlines
                tb.Blocks.Clear();

                // add new inlines
                var paras = e.NewValue as List<Paragraph>;
                if (paras != null)
                {
                    paras.ForEach(
                        inl => tb.Blocks.Add((inl))
                        );
                }
            }
        }
        public void RefreshHighlights()
        {
            var o = TextBlockBoldTermsTitle.Blocks.ToList();
            if (o != null)
            {
                TextBlockBoldTermsTitle.Blocks.Clear();
                TextBlockBlockConvertor tc = new TextBlockBlockConvertor();



                List<Paragraph> paras = tc.Convert(TextBoxTitle.Text, typeof(Paragraph), null, System.Globalization.CultureInfo.CurrentCulture) as List<Paragraph>;
                if (paras != null)
                {
                    paras.ForEach(inl => TextBlockBoldTermsTitle.Blocks.Add((inl)));
                }
                TextBlockBoldTermsAbstract.Blocks.Clear();
                paras = tc.Convert(TextBoxAbstract.Text, typeof(Paragraph), null, System.Globalization.CultureInfo.CurrentCulture) as List<Paragraph>;
                if (paras != null)
                {
                    paras.ForEach(inl => TextBlockBoldTermsAbstract.Blocks.Add((inl)));
                }
                //List<Paragraph> paras = be.DataItem as List<Paragraph>;
                //TextBlockBoldTermsTitle.Blocks.Clear();
                //if (paras != null)
                //{
                //    paras.ForEach(inl => TextBlockBoldTermsTitle.Blocks.Add((inl)));
                //}
            }
        }
        private void cmdAddPositiveTerm_Click(object sender, RoutedEventArgs e)
        {
            string s = TextBlockBoldTermsAbstract.SelectedText.Trim().ToLower();
            if (s == null || s.Length == 0) return;
            if (s.Length > 50) return;
            string[] terms = s.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string term in terms)
            {
                TrainingReviewerTerm cTrt = FindTerm(term);
                if (cTrt == null)
                {
                    TrainingReviewerTerm trt = new TrainingReviewerTerm();
                    trt.ReviewerTerm = term;
                    trt.Included = (sender as Button).Name == "cmdAddPositiveTerm";
                    CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["TrainingReviewerTermData"]);
                    if (provider != null)
                    {
                        (provider.Data as TrainingReviewerTermList).Add(trt);
                        trt.BeginEdit();
                        trt.ApplyEdit();

                    }
                }
                else
                {//term is already there, see if we need to flip the Included flag
                    if (
                        (cTrt.Included && (sender as Button).Name =="cmdAddNegativeTerm" )//adding as positive, but it's already there as negative
                        ||
                        (!cTrt.Included && (sender as Button).Name == "cmdAddPositiveTerm")//adding as negative, but it's already there as positive
                       )
                    {
                        cTrt.Included = !cTrt.Included;
                        cTrt.BeginSave(true);
                    }
                    
                }
            }
            RefreshHighlights();
        }

        private TrainingReviewerTerm FindTerm(string text)
        {
            text = text.ToLower();
             CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["TrainingReviewerTermData"]);
             if (provider != null)
             {
                 foreach (TrainingReviewerTerm t in provider.Data as TrainingReviewerTermList)
                 {
                     if (t.ReviewerTerm.ToLower() == text)
                         return t;
                 }
             }
             return null;
        }

        private void cmdRemoveTerm_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this term?", "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                string s = TextBlockBoldTermsAbstract.SelectedText.Trim();
                string[] terms = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool found = false;
                foreach (string term in terms)
                {
                    TrainingReviewerTerm trt = FindTerm(term);
                    if (trt != null)
                    {
                        found = true;
                        trt.BeginEdit();
                        trt.Delete();
                        trt.ApplyEdit();
                        trt = null;
                    }
                }
                if (found) RefreshHighlights();
            }
        }

        private void cmdShowHideReviewerTerms_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowTermsClick(object sender, RoutedEventArgs e)
        {
            if (ShowTermsClicked != null) ShowTermsClicked.Invoke(sender, e);
        }

        private void cmdShowUnnecessaryRowsForScreening_Click(object sender, RoutedEventArgs e)
        {
            cmdShowUnnecessaryRowsForScreening.Visibility = System.Windows.Visibility.Collapsed;
            cmdHideUnnecessaryRowsForScreening.Visibility = System.Windows.Visibility.Visible;
            RowMonth.Height = new GridLength(30);
            RowAuthors.Height = new GridLength(30);
        }

        private void cmdHideUnnecessaryRowsForScreening_Click(object sender, RoutedEventArgs e)
        {
            cmdHideUnnecessaryRowsForScreening.Visibility = System.Windows.Visibility.Collapsed;
            cmdShowUnnecessaryRowsForScreening.Visibility = System.Windows.Visibility.Visible;
            RowMonth.Height = new GridLength(0);
            RowAuthors.Height = new GridLength(0);
        }

       
        
       

       
      
        
    } // end control code


    public class ItemTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Csla.Xaml.CslaDataProvider provider = parameter as Csla.Xaml.CslaDataProvider;

            if (value != null && provider != null && provider.Data != null)
            {
                ItemTypeNVL ItemTypeList = provider.Data as ItemTypeNVL;
                return ItemTypeList.GetItemByKey((int)value);
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Csla.Xaml.CslaDataProvider provider = parameter as Csla.Xaml.CslaDataProvider;

            if (value != null && provider != null && provider.Data != null)
            {
                var returnValue = ((ItemTypeNVL.NameValuePair)value);
                if (returnValue != null)
                    return returnValue.Key;
                else
                    return 0;
            }
            else
                return 0;
        }
    }


    public class TextBlockBlockConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            //SolidColorBrush red = new SolidColorBrush(Colors.Red);
            //SolidColorBrush green = new SolidColorBrush(Colors.Green);
            //var paras = new List<Paragraph>();
            //var paragraph = new Paragraph();
            //if (value != null)
            //{
            //    CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["TrainingReviewerTermData"]);
            //    if (provider != null)
            //    {
            //        TrainingReviewerTermList terms = provider.Data as TrainingReviewerTermList;
            //        if (terms != null)
            //        {
            //            string splitThis = value.ToString();
            //            foreach (TrainingReviewerTerm t in terms)
            //            {
            //                //if (!t.IsDeleted) splitThis = splitThis.Replace(t.ReviewerTerm, "splitrighthere" + t.ReviewerTerm + "splitrighthere");
            //                if (!t.IsDeleted)
            //                {
            //                    splitThis = ReplaceString(splitThis, t.ReviewerTerm);
            //                }
            //            }

            //            // parse text
            //            var textLines =
            //                splitThis.Split(
            //                    new string[] { "splitrighthere" }
            //                    , StringSplitOptions.RemoveEmptyEntries);

            //            // add formatting
            //            foreach (string line in textLines)
            //            {
            //                Run newRun = new Run() { Text = line };
            //                foreach (TrainingReviewerTerm t in terms)
            //                {
            //                    if (t.ReviewerTerm.ToLower() == line.ToLower())
            //                    {
            //                        if (t.Included == true)
            //                        {
            //                            newRun.FontWeight = FontWeights.ExtraBold;
            //                            newRun.Foreground = green;
            //                        }
            //                        else
            //                        {
            //                            newRun.FontStyle = FontStyles.Italic;
            //                            newRun.FontWeight = FontWeights.Bold;
            //                            newRun.Foreground = red;
            //                        }
            //                    }
            //                }
            //                paragraph.Inlines.Add(newRun);
            //            }
            //        }
            //    }
            //}
            //paras.Add(paragraph);
            //return paras;
            
            SolidColorBrush red = new SolidColorBrush(Colors.Red);
            SolidColorBrush green = new SolidColorBrush(Colors.Green);
            string input = value.ToString();
            string termSt;
            string[] specials = new string[] {"\\", ".", "$", "^", "{", "[", "(", "|", ")", "*", "+", "?"};
            Run newRun;
            var paras = new List<Paragraph>();
            var paragraph = new Paragraph();
            bool done = false;
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["TrainingReviewerTermData"]);
            if (provider != null)
            {
                TrainingReviewerTermList terms = provider.Data as TrainingReviewerTermList;
                if (terms != null && terms.Count > 0)
                {
                    done = true;
                    string matcher = "(";//building the regex, change it to "\b(" if you want to match only at the beginning of a word: "and" would match "android" but not "standard"
                    List<string> inclL = new List<string>();
                    List<string> exclL = new List<string>();
                    foreach (TrainingReviewerTerm term in terms)
                    {

                        if (!term.IsDeleted)
                        {
                            termSt = term.ReviewerTerm;
                            //matcher += term.ReviewerTerm +"|";
                            foreach (string special in specials)
                            {
                                termSt = termSt.Replace(special, @"\" + special);
                            }
                            matcher += termSt + "|";    
                            if (term.Included)
                            {
                                inclL.Add(term.ReviewerTerm);
                            }
                            else
                            {
                                exclL.Add(term.ReviewerTerm);
                            }
                        }
                    }
                    matcher = matcher.Substring(0, matcher.Length - 1) + ")";

                    //string[] lines = value.ToString().Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    
                    //Regex re = new Regex(@"\b.+?\b");
                    Regex re = new Regex(matcher, RegexOptions.IgnoreCase);
                    MatchCollection mc = re.Matches(input);
                    int lastStart = 0;
                    foreach (Match match in mc)
                    {
                        if (inclL.Contains(match.Value.ToLower()))
                        {
                            newRun = new Run() { Text = input.Substring(lastStart, match.Index - lastStart) };
                            paragraph.Inlines.Add(newRun);
                            newRun = new Run() { Text = match.Value };
                            newRun.FontWeight = FontWeights.ExtraBold;
                            newRun.Foreground = green;
                            paragraph.Inlines.Add(newRun);
                            lastStart = match.Index + match.Length;
                        }
                        else if (exclL.Contains(match.Value.ToLower()))
                        {
                            newRun = new Run() { Text = input.Substring(lastStart, match.Index - lastStart) };
                            paragraph.Inlines.Add(newRun);
                            newRun = new Run() { Text = match.Value };
                            newRun.FontStyle = FontStyles.Italic;
                            newRun.FontWeight = FontWeights.Bold;
                            newRun.Foreground = red;
                            paragraph.Inlines.Add(newRun);
                            lastStart = match.Index + match.Length;
                        }
                    }
                    newRun = new Run() { Text = input.Substring(lastStart) };
                    paragraph.Inlines.Add(newRun);
                }
            }
            if (!done)
            {
                newRun = new Run() { Text = input };
                paragraph.Inlines.Add(newRun);
            }
            paras.Add(paragraph);
            return paras;
            
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
        private static string ReplaceString(string str, string oldValue)
        {
            StringBuilder sb = new StringBuilder();
            
            int previousIndex = 0;
            int index = str.IndexOf(oldValue, StringComparison.CurrentCultureIgnoreCase);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append("splitrighthere");
                sb.Append(str.Substring(index, oldValue.Length));
                sb.Append("splitrighthere");
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, StringComparison.CurrentCultureIgnoreCase);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }
    }
}
