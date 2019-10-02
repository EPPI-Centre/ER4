using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using BusinessLibrary.BusinessClasses;

namespace EppiReviewer4.Helpers
{
    public class SourceButtonConverter : IValueConverter
    {
        public SourceButtonConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is bool)) return string.Empty;
            System.Windows.Controls.Image img = new Image();
            img.Height = 16; img.Width = 16;
            img.HorizontalAlignment = HorizontalAlignment.Center;
            img.VerticalAlignment = VerticalAlignment.Center;
            if ((bool)value)
            {
                //return "Undelete";
                img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("Icons/edit-undo.png", UriKind.RelativeOrAbsolute));
                img.Tag = "Undelete";
            }
            else
            {
                img.Tag = "Delete";
                img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("Icons/trash.png", UriKind.RelativeOrAbsolute));
            }
            //else return "Delete";
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class SourceButtonTipConverter : IValueConverter
    {
        public SourceButtonTipConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is bool)) return string.Empty;
            if ((bool)value)
            {
                return "Undelete";
            }
            else return "Delete";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class ViewDocConverter : IValueConverter
    {
        public ViewDocConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string) || (string)value == "") return string.Empty;
            string low = ((string)value).ToLower();
            if (low == ".pdf")
            {
                return "View";
            }
            else if (low == ".doc" | low == ".docx")
            {
                return "Open";
            }
            else if (low == "txt")
            {
                return string.Empty;
            }
            else return "Download";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class EnableDownloadButtonConverter : IValueConverter
    {
        public EnableDownloadButtonConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string) || (string)value == "") return false;
            string low = ((string)value).ToLower();
            if (low == ".txt") return false;
            else return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class EnableViewPDFButtonConverter : IValueConverter
    {
        public EnableViewPDFButtonConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string) || (string)value == "") return false;
            string low = ((string)value).ToLower();
            if (low == ".pdf") return true;
            else return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class URIgetbinButtonConverter : IValueConverter
    {
        public URIgetbinButtonConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            //if (value == null || !(value is long) ) return "";
            //string passwd = (App.Current.Resources["UserLogged"] as LoginSuccessfulEventArgs).Password;
            //string revID = BusinessLibrary.BusinessClasses.Cryptography.Encrypt(ri.ReviewId.ToString(), passwd);
            //revID = System.Windows.Browser.HttpUtility.UrlEncode(revID);
            //string DocID = BusinessLibrary.BusinessClasses.Cryptography.Encrypt(value.ToString(), passwd);
            //DocID = System.Windows.Browser.HttpUtility.UrlEncode(DocID);
            //string boh = Application.Current.Host.Source.AbsolutePath;
            //Uri tmpuri = new Uri(App.Current.Host.Source.ToString().Substring(0, App.Current.Host.Source.ToString().IndexOf("ClientBin")) +
            //                        "getbin.aspx?U=" + System.Windows.Browser.HttpUtility.UrlEncode(ri.Ticket)
            //                       + "&ID=" + revID
            //                       + "&DID=" + DocID, UriKind.Absolute); 
            if (value == null || !(value is long)) return "";
            BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            string DocID = System.Windows.Browser.HttpUtility.UrlEncode(value.ToString());
            Uri tmpuri = new Uri(App.Current.Host.Source.ToString().Substring(0, App.Current.Host.Source.ToString().IndexOf("ClientBin")) +
                                    "getbin.aspx?U=" + System.Windows.Browser.HttpUtility.UrlEncode(ri.Ticket)
                                   + "&ID=" + ri.UserId.ToString()
                                   + "&DID=" + DocID, UriKind.Absolute); 
            return tmpuri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class ShowMasterItemConverter : IValueConverter
    {
        public ShowMasterItemConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is bool) || !(bool)value ) return Visibility.Collapsed;
            else return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class ManualGroupCommentConverter : IValueConverter
    {
        public ManualGroupCommentConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is int)) return "ERROR: no data, please contact the support team!";
            int i = (int)value;
            if (i == 0) return "This Item does not belong to any group, you can use it as a master.";
            if (i == 1) return "This Item belongs to some other group(s): consider using the related group(s) instead of manually creating a new group.";
            if (i == 2) return "This Item is the Master of some other group: it can't be inserted into the new group. Consider using its own group instead of manually creating a new group.";
            else return "ERROR: data is inconsistent, please contact the support team!";            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class SourceNameConverter : IValueConverter
    {
        public SourceNameConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string) || (string)value == "") return string.Empty;
            string low = ((string)value).ToLower();
            if ((string)value == "NN_SOURCELESS_NN")
            {
                return "Manually Created Items";
            }

            else return (string)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class SourceNameStyleConverter : IValueConverter
    {
        public SourceNameStyleConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string) || (string)value == "") return FontStyles.Normal;
            string low = ((string)value).ToLower();
            if ((string)value == "NN_SOURCELESS_NN")
            {
                return FontStyles.Italic;
            }

            else return FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class LoginToReviewIsEn : IValueConverter
    {
        public LoginToReviewIsEn()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string) || (string)value == "") return false;
            if (value.ToString().Contains("Coding only"))
            {
                return false;
            }

            else return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class LoginToArchieReviewIsEn : IValueConverter
    {
        public LoginToArchieReviewIsEn()
        {
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string)) return false;
            if (value.ToString().Contains("Coding only"))
            {
                return false;
            }

            else return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else if (value is bool?)
            {
                bool? nullable = (bool?)value;
                flag = nullable.HasValue ? nullable.Value : false;
            }
            return (flag ?Visibility.Visible : Visibility.Collapsed );
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((value is Visibility) && (((Visibility)value) == Visibility.Visible));
        }
    }
    public sealed class BooleanToVisibilityReversedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else if (value is bool?)
            {
                bool? nullable = (bool?)value;
                flag = nullable.HasValue ? nullable.Value : false;
            }
            Visibility ww = flag ? Visibility.Collapsed : Visibility.Visible;
            return (flag ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((value is Visibility) && (((Visibility)value) == Visibility.Visible));
        }
    }
    public class ZoomLevelConverter : IValueConverter
    {
        /// <summary>
        /// Converts a double value to a percentage string
        /// </summary>
        /// <param name="value">the value in double format</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int zoomPercent = (int)(Math.Round((double)value, 2) * 100);
            return (zoomPercent.ToString());
        }

        /// <summary>
        /// Converts a percentage string back to a double value
        /// </summary>
        /// <param name="value">the value in percentage as string</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (((string)value) == string.Empty)
            {
                return 1;
            }
            else
            {
                string zoomString = ((string)value);
                char[] charsToTrim = { ' ', '%' };
                string trimmedString = zoomString.TrimEnd(charsToTrim);
                double result;

                if (double.TryParse((string)trimmedString, out result) && result != 0)
                    return result * 0.01;
                else
                    return 1;
            }
        }
    }
    public sealed class ThicknessToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is Thickness)) return 0;
            else
            {
                Thickness t = (Thickness)value;
                return t.Top;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class ReverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = System.Convert.ToBoolean(value);
            return !val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            var val = System.Convert.ToBoolean(value);
            return !val;
        }
    }
    public class EditVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var number = System.Convert.ToDouble(value);
            return number != 1 && number !=9 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OutcomesButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var number = System.Convert.ToDouble(value);
            return (number > 3) && (number < 7) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EditTrueFalseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = System.Convert.ToBoolean(value);
            return val == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReverseEditTrueFalseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = System.Convert.ToBoolean(value);
            return val == false ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EditIntVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = System.Convert.ToInt32(value);
            return val == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsCodeTypeAvailable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value as string;
            if (val == null ||val.Length < 1 || val.Contains("- N/A")) return false;
            else return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /*
     * JT - added these for the MAG integration. Didn't need in the end, but leaving code in case I do...
     * 
    public class SelectedNotSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            if (val == "False") return "Select";
            else return "Unselect";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectedNotSelectedInReviewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    */

    public class ReconcilingCodesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ReconcilingItem RecI = value as ReconcilingItem;
            System.Collections.Generic.List<ReconcilingCode> val =  new System.Collections.Generic.List<ReconcilingCode>();
            if (RecI == null) return null;
            if (parameter.ToString() == "CodesReviewer1")
            {
                val = RecI.CodesReviewer1;
            }
            else if (parameter.ToString() == "CodesReviewer2")
            {
                val = RecI.CodesReviewer2;
            }
            else if (parameter.ToString() == "CodesReviewer3")
            {
                val = RecI.CodesReviewer3;
            }
            else return null;
            Border res = new Border();
            res.HorizontalAlignment = HorizontalAlignment.Stretch;
            //res.Width =  
            res.VerticalAlignment = VerticalAlignment.Stretch;
            
            //res.Orientation = Orientation.Vertical;
            res.Margin = new Thickness(4);
            //res.Background = new SolidColorBrush(Colors.Cyan);
            SolidColorBrush br = new SolidColorBrush(Colors.DarkGray);
            SolidColorBrush Bluebr = new SolidColorBrush(Color.FromArgb(255, 42, 89, 155)); //#FF0C4f4F91 


            Thickness tk = new Thickness(2);
            Thickness tk2 = new Thickness(1);
            
            StackPanel ArmsContainer = new StackPanel();
            ArmsContainer.Orientation = Orientation.Vertical;
            ArmsContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
            TextBlock studyTblock = new TextBlock();
            studyTblock.Text = "Whole Study";
            StackPanel studyStack = new StackPanel();
            studyStack.Orientation = Orientation.Vertical;
            studyStack.Children.Add(studyTblock);
            System.Collections.Generic.Dictionary<Int64, StackPanel> Rows = new System.Collections.Generic.Dictionary<long, StackPanel>();
            Rows.Add(0, studyStack);
            ArmsContainer.Children.Add(studyStack);
            foreach (ItemArm iArm in RecI.ItemArms)
            {
                StackPanel armStack = new StackPanel();
                armStack.Orientation = Orientation.Vertical;
                Border aBd = new Border();
                aBd.HorizontalAlignment = HorizontalAlignment.Center;
                aBd.BorderBrush = Bluebr;
                aBd.Background = new SolidColorBrush(Colors.Black);
                Line l1 = new Line();
                l1.SetValue(Grid.RowProperty, 1);
                l1.X1 = 0; l1.X2 = 1; l1.Y1 = 0; l1.Y2 = 0; l1.Stroke = new SolidColorBrush(Colors.Red); l1.StrokeThickness = 1;
                l1.Stretch = Stretch.Uniform;
                aBd.Child = l1;
                armStack.Children.Add(aBd);
                TextBlock armTblock = new TextBlock();
                armTblock.Text = iArm.Title;
                armStack.Children.Add(armTblock);
                Rows.Add(iArm.ItemArmId, armStack);
                ArmsContainer.Children.Add(armStack);
            }
            foreach (ReconcilingCode rc in val)
            {
                if (rc != null)
                {
                    Grid singleCode = new Grid();//contains the full info for a single code
                    singleCode.HorizontalAlignment = HorizontalAlignment.Left;
                    singleCode.MouseLeftButtonDown += new MouseButtonEventHandler(tx_MouseLeftButtonDown);
                    ColumnDefinition col1 = new ColumnDefinition();
                    col1.Width = new GridLength(0);
                    ColumnDefinition col2 = new ColumnDefinition();
                    singleCode.ColumnDefinitions.Add(col1);
                    singleCode.ColumnDefinitions.Add(col2);
                    RowDefinition row1 = new RowDefinition();
                    RowDefinition row2 = new RowDefinition();
                    row2.Height = new GridLength(0);
                    singleCode.RowDefinitions.Add(row1);
                    singleCode.RowDefinitions.Add(row2);
                    Border bd = new Border();//contains the actual code
                    bd.BorderBrush = br;
                    bd.HorizontalAlignment = HorizontalAlignment.Left;
                    bd.Background = new SolidColorBrush(Colors.White);
                    bd.Margin = tk;
                    bd.BorderThickness = tk;
                    bd.SetValue(Grid.ColumnProperty, 1);
                    TextBlock tx = new TextBlock();//the code!
                    tx.Text = rc.Name;
                    tx.Height = 17;
                    tx.Margin = tk;
                    tx.DataContext = rc;
                    bd.Child = tx;
                    singleCode.Children.Add(bd);
                    StackPanel graphicpath = new StackPanel();//contains the path of the full code, goes in singleCode [0,0]
                    graphicpath.Orientation = Orientation.Horizontal;
                    graphicpath.Margin = tk;
                    string[] sep = { "<¬sep¬>" };
                    Image img = new Image();
                    img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("../Icons/NextDocument.png", UriKind.Relative));
                    img.Height = 10;
                    img.Width = 10;
                    img.Opacity = 100;
                    img.Stretch = Stretch.Fill;
                    graphicpath.Children.Add(img);
                    graphicpath.SetValue(Grid.ColumnProperty, 0);
                    string[] path = rc.Fullpath.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string node in path)
                    {
                        Border bd2 = new Border();
                        bd2.BorderBrush = new SolidColorBrush(Colors.White);
                        bd2.HorizontalAlignment = HorizontalAlignment.Left;
                        //bd2.Background = new SolidColorBrush(Color.FromArgb(255, 185, 240, 185));
                        bd2.Margin = tk;
                        bd2.BorderThickness = tk;

                        TextBlock tx2 = new TextBlock();
                        tx2.Text = node;
                        tx2.Margin = tk;
                        bd2.Child = tx2;
                        graphicpath.Children.Add(bd2);
                        Image img2 = new Image();
                        img2.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("../Icons/NextDocument.png", UriKind.Relative));
                        img2.Height = 10;
                        img2.Width = 10;
                        img2.Opacity = 100;
                        img2.Stretch = Stretch.Fill;
                        graphicpath.Children.Add(img2);
                    }
                    singleCode.Children.Add(graphicpath);
                    graphicpath.Visibility = Visibility.Collapsed;
                    StackPanel spInfo = new StackPanel();
                    spInfo.Visibility = Visibility.Collapsed;
                    spInfo.SetValue(Grid.ColumnProperty, 0);
                    spInfo.SetValue(Grid.RowProperty, 1);
                    spInfo.SetValue(Grid.ColumnSpanProperty, 2);
                    spInfo.Orientation = Orientation.Vertical;
                    spInfo.Tag = rc.InfoBox;

                    Border bdInfobox = new Border();
                    bdInfobox.BorderBrush = new SolidColorBrush(Colors.Black);
                    bdInfobox.BorderThickness = new Thickness(1);

                    RichTextBox MyRTB = new RichTextBox();
                    MyRTB.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                    // Create a Run of plain text and some bold text.
                    Bold myBold = new Bold();
                    myBold.Inlines.Add("[Info] ");
                    Run myRun2 = new Run();
                    myRun2.Text = rc.InfoBox;

                    // Create a paragraph and add the Run and Bold to it.
                    Paragraph myParagraph = new Paragraph();
                    myParagraph.Inlines.Add(myBold);
                    myParagraph.Inlines.Add(myRun2);

                    // Add the paragraph to the RichTextBox.
                    MyRTB.Blocks.Add(myParagraph);

                    TextBlock infobox = new TextBlock();
                    infobox.TextWrapping = TextWrapping.Wrap;
                    infobox.Text = rc.InfoBox.Length > 0 ? "[Info] " + rc.InfoBox : "";
                    bdInfobox.Child = infobox;
                    spInfo.Children.Add(MyRTB);
                    singleCode.Children.Add(spInfo);
                    Rows[rc.ArmID].Children.Add(singleCode);
                    //res.Children.Add(singleCode);
                }
            }
            res.Child = ArmsContainer;
            //res.Children.Add(ArmsContainer);
            return res;

            //ColumnDefinition col1= new ColumnDefinition();
            //col1.Width = new GridLength(99, GridUnitType.Star);
            //ArmsContainer.ColumnDefinitions.Add(col1);
            //ArmsContainer.RowDefinitions.Add(new RowDefinition());
            //ColumnDefinition col2 = new ColumnDefinition();
            //col2.Width = new GridLength(1, GridUnitType.Star);
            //ArmsContainer.ColumnDefinitions.Add(col2);
            ////ArmsContainer.Orientation = Orientation.Vertical;

            //Border studyBorder = new Border();
            //studyBorder.BorderBrush = Bluebr;
            ////studyBorder.Background = new SolidColorBrush(Colors.LightGray);
            //studyBorder.Margin = tk2;
            //studyBorder.BorderThickness = tk2;
            //studyBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            //StackPanel studyStack = new StackPanel();
            //studyStack.Orientation = Orientation.Vertical;
            //studyStack.HorizontalAlignment = HorizontalAlignment.Stretch;
            //studyBorder.Child = studyStack;
            //studyBorder.SetValue(Grid.RowProperty, 0);
            //studyBorder.SetValue(Grid.ColumnProperty, 0);
            //studyStack.Children.Add(studyTblock);
            //ArmsContainer.RowDefinitions.Add(new RowDefinition());
            //<Line Grid.Row="1" X1="0" Y1="0" X2="1"  Y2="0" Stroke = "Red" StrokeThickness = "10" Stretch = "Uniform" ></ Line >
            //    Line l1 = new Line();
            //l1.SetValue(Grid.RowProperty, 1);
            //l1.X1 = 0; l1.X2 = 1; l1.Y1 = 0; l1.Y2 = 0; l1.Stroke = new SolidColorBrush(Colors.Red); l1.StrokeThickness = 1;
            //l1.Stretch = Stretch.Uniform;

            //System.Collections.Generic.Dictionary<Int64, StackPanel> Rows = new System.Collections.Generic.Dictionary<long, StackPanel>();
            //ArmsContainer.Children.Add(studyBorder);
            //ArmsContainer.Background = new SolidColorBrush(Colors.LightGray);
            //Rows.Add(0, studyStack);
            //int armCounter = 1;
            //foreach (ItemArm iArm in RecI.ItemArms)
            //{
            //    TextBlock armTblock = new TextBlock();
            //    armTblock.Text = iArm.Title;
                
            //    Border armBorder = new Border();
            //    //armBorder.Background = new SolidColorBrush(Colors.Yellow);
            //    armBorder.Margin = tk2;
            //    armBorder.BorderThickness = tk2;
            //    armBorder.BorderBrush = Bluebr;
            //    StackPanel armStack = new StackPanel();
            //    armStack.Orientation = Orientation.Vertical;
            //    armBorder.Child = armStack;
            //    armStack.Children.Add(armTblock);
            //    Rows.Add(iArm.ItemArmId, armStack);
            //    RowDefinition row = new RowDefinition();
                
            //    ArmsContainer.RowDefinitions.Add(new RowDefinition());
            //    armBorder.SetValue(Grid.RowProperty, armCounter);
            //    ArmsContainer.Children.Add(armBorder);

            //    armCounter++;
            //}
            




            ////System.Collections.Generic.List<ReconcilingCode> val = value as System.Collections.Generic.List<ReconcilingCode>;
            ////val.Sort(delegate (ReconcilingCode x, ReconcilingCode y)
            ////{
            ////    return x.ArmID.CompareTo(y.ArmID);
            ////});
            //if (val == null || val.Count == 0) return "";
            //else
            //{
                

                
            //}
        }
        void tx_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid tx = sender as Grid;
            if (tx == null || tx.Children[1] == null) return;
            object o = tx.Children[1];
            object o2 = tx.Children[2];
            if (tx.Children[1].Visibility == Visibility.Collapsed)
            {
                tx.Children[1].Visibility = Visibility.Visible;
                tx.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
                if ((tx.Children[2] as StackPanel) != null && (tx.Children[2] as StackPanel).Tag.ToString() != "")
                {
                    tx.Children[2].Visibility = Visibility.Visible;
                    tx.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
                }
            }
            else
            {
                tx.Children[1].Visibility = Visibility.Collapsed;
                tx.ColumnDefinitions[0].Width = new GridLength(0);
                tx.Children[2].Visibility = Visibility.Collapsed;
                tx.RowDefinitions[1].Height = new GridLength(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ButtonContentCompleteAs : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ReconcilingItemList val = value as ReconcilingItemList;
            if (val == null || val.Comparison == null) return "N/A";
            else
            {
                if (parameter as string == "r1")
                {
                    return "Complete as: " + val.Comparison.ContactName1;
                }
                else if (parameter as string == "r2")
                {
                    return "Complete as: " + val.Comparison.ContactName2;
                }
                if (parameter as string == "r3")
                {
                    return "Complete as: " + val.Comparison.ContactName3;
                }
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ButtonTagCompleteAs : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            ReconcilingItemList val = value as ReconcilingItemList;
            if (val == null || val.Comparison == null) return -1;
            else
            {
                if (parameter as string == "r1")
                {
                    return val.Comparison.ContactId1;
                }
                else if (parameter as string == "r2")
                {
                    return val.Comparison.ContactId2;
                }
                else if (parameter as string == "r3")
                {
                    return val.Comparison.ContactId3;
                }
            }
            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ButtonThirdReviewerVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ReconcilingItemList val = value as ReconcilingItemList;
            if (val == null || val.Comparison == null) return Visibility.Collapsed;
            else
            {
                return val.ShowReviewer3 ? Visibility.Visible : Visibility.Collapsed;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ReconcileRowIsCompletedBackGround : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? val = value as bool?;
            if (val == null) return new SolidColorBrush(Colors.Black);
            else
            {
                if (val == true)
                {
                    if (parameter as string == "n")
                    {
                        return new SolidColorBrush(Color.FromArgb(255, 185, 240, 185));
                    }
                    if (parameter as string == "s")
                    {
                        return new SolidColorBrush(Color.FromArgb(255, 175, 220, 175));
                    }
                }
                else
                {
                    if (parameter as string == "n")
                    {
                        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    }
                    if (parameter as string == "s")
                    {
                        return new SolidColorBrush(Color.FromArgb(255, 225, 225, 225));
                    }
                } 
                return new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ReconcileShowAsCompletor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int? val = value as int?;
            if (val == null) return Visibility.Collapsed;
            else
            {
                if (val <= 0)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    ContentControl par = parameter as ContentControl;
                    if (par == null || par.Tag == null) return Visibility.Collapsed;
                    int? columnID = par.Tag as int?;
                    if (columnID == null || columnID <= 0) return Visibility.Collapsed;
                    else if (val == columnID)
                    {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ButtonReconcCompleteIsEnabled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Collections.Generic.List<ReconcilingCode> val = value as System.Collections.Generic.List<ReconcilingCode>;
            if (val == null || val.Count < 1) return false;
            else
            {
                Button  bt =  parameter as Button;
                if (bt == null) return false;
                return bt.IsEnabled;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CodeSetVisibility4RandomAllocation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ListBox lb = parameter as ListBox;
            if (value == null || lb == null || lb.Items == null || lb.Items.Count == 0) return true;
            ReviewSet rs = value as ReviewSet;
            AttributeSet aSet = value as AttributeSet;
            if (rs == null && aSet == null) return true;
            ReadOnlySetTypeList stl = lb.ItemsSource as ReadOnlySetTypeList;
            if (stl == null) return true;
            if (rs != null)
            {
                ReadOnlySetType st = stl.GetSetType(rs.SetTypeId);
                if (!st.AllowRandomAllocation) return false;
            }
            else if (aSet != null)
            {
                return aSet.CanHaveChildren;
            }
            return true;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public static class WindowHelper
    {
        public static void MaxOnly_WindowStateChanged(object sender, EventArgs e)
        {
            Telerik.Windows.Controls.RadWindow rw = sender as Telerik.Windows.Controls.RadWindow;
            if (rw == null || !rw.IsActiveWindow) return;
            if (rw.WindowState != WindowState.Maximized)
            {
                rw.WindowStateChanged -= MaxOnly_WindowStateChanged;
                rw.ResizeMode = Telerik.Windows.Controls.ResizeMode.CanResize;
                System.Windows.Interop.Content root = App.Current.Host.Content;
                rw.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                rw.WindowStateChanged += new EventHandler(rw_LayoutUpdated);
            }
        }
        public static void rw_LayoutUpdated(object sender, EventArgs e)
        {
            Telerik.Windows.Controls.RadWindow rw = sender as Telerik.Windows.Controls.RadWindow;
            if (rw == null || !rw.IsActiveWindow) return;
            if (rw.WindowState == WindowState.Maximized)
            {
                rw.WindowStateChanged -= rw_LayoutUpdated;
                rw.ResizeMode = Telerik.Windows.Controls.ResizeMode.NoResize;
                rw.WindowStateChanged += MaxOnly_WindowStateChanged;
            }
        }
    }

    /*
    public class Enabler : BusinessBase<Enabler>
    {
        public bool HasWriteRights
        { 
            get { return HasWriteRightsM();}
        }
        public Enabler()
        {
        }
        
        public static Enabler newEnabler()
        {
            return new Enabler();
        }
        public static bool HasWriteRightsM()
        {
            BusinessLibrary.Security.ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            return (ri.IsInRole("AdminUser") | ri.IsInRole("RegularUser"));
        }
        private void DataPortal_Create()
        {
        }
    }*/
}