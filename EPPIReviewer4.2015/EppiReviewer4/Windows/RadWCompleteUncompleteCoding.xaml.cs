using BusinessLibrary.BusinessClasses;
using Csla;
using Csla.Xaml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Telerik.Windows.Controls;

namespace EppiReviewer4
{
    
    public partial class RadWCompleteUncompleteCoding : RadWindow //change this to give it a unque name, must inherit from Radwindow.
    {
        public RadWCompleteUncompleteCoding()
        {
            InitializeComponent();
            codesSelectControlFilterCode.SelectCode_SelectionChanged += new EventHandler<Telerik.Windows.Controls.SelectionChangedEventArgs>(codesSelectControlReports_SelectCode_SelectionChanged);
            codesSelectControlFilterCode.SetMode(false, true, false);
        }
        private bool isBulkCompleting = true;
        private Paragraph StartPreviewPar()
        {

            Paragraph _StartPreviewPar = new Paragraph();
            Run cont = new Run(); cont.Text = "Please make your selections from the dropdown menus.";
            _StartPreviewPar.Inlines.Add(cont);
            return _StartPreviewPar;
        }
        public bool HasDoneSomething = false;
        private SolidColorBrush OKBg()
        {
            Color mycolor = new Color();
            mycolor.R = 152;
            mycolor.G = 251;
            mycolor.B = 152;
            mycolor.A = 160;
            return new SolidColorBrush(mycolor);
        }
        private SolidColorBrush NotOKBg()
        {
            Color mycolor = new Color();
            mycolor.R = 255;
            mycolor.G = 228;
            mycolor.B = 225;
            mycolor.A = 255;
            return new SolidColorBrush(mycolor);
        }
        public void SetMode(bool IsBulkCompleting)
        {
            HasDoneSomething = false;
            ButtonPreview.IsEnabled = false;
            ButtonDoIt.IsEnabled = false;
            ComboBoxReviewer.SelectedIndex = -1;
            codesSelectControlDestSet.SelectedIndex = -1;
            codesSelectControlFilterCode.ClearSelection();
            isBulkCompleting = IsBulkCompleting;
            TxtPreviewStatus.Blocks.Clear();
            TxtPreviewStatus.Blocks.Add(StartPreviewPar());
            TxtPreviewStatus.Background = null;
            if (isBulkCompleting)
            {
                Header = "Complete Coding in Bulk";//Complete the coding in this set:
                txtUseThis_set.Text = "Complete the coding in this set:";
                TxtCompleting.Visibility = Visibility.Visible;
                TxtUnCompleting.Visibility = Visibility.Collapsed;
                GridComplUncompl.RowDefinitions[1].Height = new GridLength(35);
                ButtonDoIt.Content = "Complete!";
            }
            else
            {
                Header = "Un-Complete Coding in Bulk";
                txtUseThis_set.Text = "Un-Complete the coding in this set:";
                TxtCompleting.Visibility = Visibility.Collapsed;
                TxtUnCompleting.Visibility = Visibility.Visible;
                GridComplUncompl.RowDefinitions[1].Height = new GridLength(0);
                ButtonDoIt.Content = "Un-complete!";
            }
        }
        
        private void ComboBoxReviewer_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PreviewMode();
        }
        void codesSelectControlReports_SelectCode_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            PreviewMode();
        }
        private void PreviewMode()
        {//you want to check if everything is specified, if it is, enable preview button. Always disable the do it button.
            //set messages accordingly
            bool isOK = true; HasDoneSomething = false;
            string compORuncomp = isBulkCompleting ? "completed" : "un-completed";
            string msg = "Please click \"Preview\" to continue.";
            if (isBulkCompleting && ComboBoxReviewer.SelectedIndex == -1 )
            {
                isOK = false;
                msg = "Please select whose codings should be " +compORuncomp + ".";
            }
            else if (codesSelectControlDestSet.SelectedIndex == -1)
            {
                isOK = false;
                msg = "Please select the codeset to be " + compORuncomp + ".";
            }
            else if (codesSelectControlFilterCode.SelectedAttributeSet() == null)
            {
                isOK = false;
                msg = "Please select the code used to specify what items are to be " + compORuncomp + ".";
            }
            if (isOK)
            {//check if the filter Attribute is in the set that will be affected
                AttributeSet aSet = codesSelectControlFilterCode.SelectedAttributeSet();
                ReviewSet rSet = codesSelectControlDestSet.SelectedItem as ReviewSet;
                if (aSet.SetId == rSet.SetId)
                {
                    isOK = false;
                    msg = "This can't be done: the selected code belongs to the Codeset you wish to act on." + Environment.NewLine + "Please select a different Code/Codeset combination.";
                }
            }
            ButtonPreview.IsEnabled = isOK;
            ButtonDoIt.IsEnabled = false;
            TxtPreviewStatus.Blocks.Clear();
            Paragraph MsgP = new Paragraph();
            Run cont = new Run(); cont.Text = msg;
            MsgP.Inlines.Add(cont);
            TxtPreviewStatus.Blocks.Add(MsgP);
            TxtPreviewStatus.Background = isOK ? OKBg() : NotOKBg();
        }

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            
            DataPortal<BulkCompleteUncompleteCommand> dp = new DataPortal<BulkCompleteUncompleteCommand>();
            BulkCompleteUncompleteCommand command = new BulkCompleteUncompleteCommand();
            AttributeSet aSet = codesSelectControlFilterCode.SelectedAttributeSet();
            if (aSet == null) return;
            ReviewSet rSet = codesSelectControlDestSet.SelectedItem as ReviewSet;
            if (rSet == null) return;
            ReviewContactNVL.NameValuePair rContact = new NameValueListBase<int, string>.NameValuePair();
            if (isBulkCompleting)
            {
                rContact = ComboBoxReviewer.SelectedItem as ReviewContactNVL.NameValuePair;
                if (rContact == null) return;
                command = new BulkCompleteUncompleteCommand(aSet.AttributeId, isBulkCompleting, rSet.SetId, rContact.Key, true);
            }
            else
            {
                command = new BulkCompleteUncompleteCommand(aSet.AttributeId, isBulkCompleting, rSet.SetId, true);
            }
            dp.ExecuteCompleted += (o, e2) =>
            {
                BusyLoading.IsRunning = false;
                Enabler.IsEnabled = true;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    TxtPreviewStatus.Blocks.Clear();
                    Paragraph MsgP = new Paragraph();
                    Run cont = new Run();
                    cont.Text = "Your selected code (" + aSet.AttributeName + ") is associated with ";
                    MsgP.Inlines.Add(cont);
                    cont = new Run();
                    cont.Text = e2.Object.PotentiallyAffectedItems.ToString() + " Items.";
                    cont.FontWeight = FontWeights.Bold;
                    MsgP.Inlines.Add(cont);
                    TxtPreviewStatus.Blocks.Add(MsgP);
                    if (e2.Object.PotentiallyAffectedItems > 0)
                    {
                        MsgP = new Paragraph();
                        cont = new Run();
                        cont.Text = "Of these, "
                            + (e2.Object.IsCompleting ? "un-completed" : "completed")  
                            + " codings in the chosen Codeset (\"" + rSet.SetName + "\") will be "
                            + (e2.Object.IsCompleting ? "completed, if they belong to " + rContact.Value : "un-completed")
                            + "." + Environment.NewLine;
                        
                        cont.Text += "As a result, the coding of ";
                        MsgP.Inlines.Add(cont);
                        cont = new Run();
                        cont.Text = e2.Object.AffectedItems.ToString() + " Items ";
                        cont.FontWeight = FontWeights.Bold;
                        MsgP.Inlines.Add(cont);
                        cont = new Run();
                        cont.Text = "will be " + (e2.Object.IsCompleting ? "completed" : "un-completed") + ".";
                        cont.FontWeight = FontWeights.Bold;
                        MsgP.Inlines.Add(cont);
                        cont = new Run();
                        if (e2.Object.AffectedItems > 0)
                        {
                            cont.Text = Environment.NewLine + "If this looks ok, you may now press the " 
                                + (e2.Object.IsCompleting ? "\"Complete!\"" : "\"Un-Complete!\"")+ " button.";
                            MsgP.Inlines.Add(cont);
                            cont = new Run();
                            cont.Text = Environment.NewLine + "Warning: this action does not have a direct \"Undo\" function so please use with care!";
                            cont.FontWeight = FontWeights.Bold;
                            MsgP.Inlines.Add(cont);
                            TxtPreviewStatus.Blocks.Add(MsgP);
                            ButtonDoIt.IsEnabled = true;
                            TxtPreviewStatus.Background = OKBg();
                        }
                        else
                        {
                            cont = new Run();
                            cont.Text = Environment.NewLine +  "Nothing to be " + (e2.Object.IsCompleting ? "completed" : "un-completed") + "!";
                            cont.FontWeight = FontWeights.Bold;
                            MsgP.Inlines.Add(cont);
                            TxtPreviewStatus.Blocks.Add(MsgP);
                            TxtPreviewStatus.Background = NotOKBg();
                        }
                    }
                    else
                    {
                        MsgP = new Paragraph();
                        cont = new Run();
                        cont.Text = "Nothing to be " + (e2.Object.IsCompleting ? "completed" : "un-completed") + "!";
                        cont.FontWeight = FontWeights.Bold;
                        MsgP.Inlines.Add(cont);
                        TxtPreviewStatus.Blocks.Add(MsgP);
                        TxtPreviewStatus.Background = NotOKBg();
                    }
                }
                
            };
            BusyLoading.IsRunning = true;
            ButtonDoIt.IsEnabled = false;
            Enabler.IsEnabled = false;
            dp.BeginExecute(command);
        }

        private void ButtonDoIt_Click(object sender, RoutedEventArgs e)
        {
            DataPortal<BulkCompleteUncompleteCommand> dp = new DataPortal<BulkCompleteUncompleteCommand>();
            BulkCompleteUncompleteCommand command = new BulkCompleteUncompleteCommand();
            AttributeSet aSet = codesSelectControlFilterCode.SelectedAttributeSet();
            if (aSet == null) return;
            ReviewSet rSet = codesSelectControlDestSet.SelectedItem as ReviewSet;
            if (rSet == null) return;
            ReviewContactNVL.NameValuePair rContact = new NameValueListBase<int, string>.NameValuePair();
            if (isBulkCompleting)
            {
                rContact = ComboBoxReviewer.SelectedItem as ReviewContactNVL.NameValuePair;
                if (rContact == null) return;
                command = new BulkCompleteUncompleteCommand(aSet.AttributeId, isBulkCompleting, rSet.SetId, rContact.Key, false);
            }
            else
            {
                command = new BulkCompleteUncompleteCommand(aSet.AttributeId, isBulkCompleting, rSet.SetId, false);
            }
            dp.ExecuteCompleted += (o, e2) =>
            {
                BusyLoading.IsRunning = false;
                Enabler.IsEnabled = true;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {//function reports N of items affected, we might want to show this, but should we?
                    HasDoneSomething = true;
                    this.Close();
                }

            };
            BusyLoading.IsRunning = true;
            ButtonDoIt.IsEnabled = false;
            Enabler.IsEnabled = false;
            dp.BeginExecute(command);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
