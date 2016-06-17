using System;
using System.Collections.Generic;
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
using BusinessLibrary.BusinessClasses;


using Telerik.Windows.Controls;
using System.ComponentModel;
using Csla;
using Csla.Xaml;


namespace EppiReviewer4
{
    
    public partial class RadWReviewWizard : RadWindow //change this to give it a unque name, must inherit from Radwindow.
    {
        public RadWReviewWizard()
        {
            InitializeComponent();
            this.Activated += new EventHandler(RadWReviewWizard_Activated);
            CslaDataProvider provider = this.Resources["ReviewTemplates"] as CslaDataProvider;
            if (provider != null && provider.Data == null && !provider.IsBusy)
            {
                provider.FactoryMethod = "GetReviewTemplates";
                provider.Refresh();

            }
        }

        void RadWReviewWizard_Activated(object sender, EventArgs e)
        {
           ShowStep(1);
        }
        void ShowStep(int Step)
        {
            if (Step > 2 || Step < 1) Step = 1;
            if (Step == 1)
            {
                if (listTemplates.SelectedItem != null)
                {
                    cmdProceed.IsEnabled = true;
                }
                else
                {
                    cmdProceed.IsEnabled = false;
                }
                WizardStep1Grid.Visibility = System.Windows.Visibility.Visible;
                WizardStep2GridManual.Visibility = System.Windows.Visibility.Collapsed;
                CodeSetPreview.Reset(true);
            }
            else if (Step == 2)
            {
                WizardStep1Grid.Visibility = System.Windows.Visibility.Collapsed;
                WizardStep2GridManual.Visibility = System.Windows.Visibility.Visible;
            }
        }
        private void ReviewTemplatesProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReviewTemplates"]);
            if (provider == null || provider.Error != null)
            {
                RadWindow.Alert(provider.Error.Message);
                return;
            }
            if (provider.Data != null && listTemplates.HasItems)
            {
                listTemplates.SelectedIndex = 0;
            }
        }
        private void listTemplates_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (listTemplates.SelectedItem != null)
            {
                cmdProceed.IsEnabled = true;
                ReadOnlyTemplateReview roTr = listTemplates.SelectedItem as ReadOnlyTemplateReview;
                if (roTr != null)
                {
                    if (
                        (roTr.TemplateName == "Manually pick from Public codesets..." && roTr.TemplateId == 1000)
                        ||
                        (roTr.TemplateName == "Manually pick from your own codesets..." && roTr.TemplateId == 2000)
                       )
                    {
                        SelectedDetail.Text = roTr.TemplateDescription;
                    }
                    else
                    {
                        SelectedDetail.Text = roTr.TemplateDescription + Environment.NewLine + "This template contains " + roTr.ReviewSetIds.Count.ToString() + " sets.";
                    }
                }
            }
        }

        private void btProceed_Click(object sender, RoutedEventArgs e)
        {
            ReadOnlyTemplateReview roTr = listTemplates.SelectedItem as ReadOnlyTemplateReview;
            if (roTr == null) return;
            if (
                (roTr.TemplateName == "Manually pick from Public codesets..." && roTr.TemplateId == 1000)
                ||
                (roTr.TemplateName == "Manually pick from your own codesets..." && roTr.TemplateId == 2000)
               )
            {
                OpenListOfSets(roTr);
            }
            else
            {
                CslaDataProvider provider = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
                if (provider == null) 
                {
                    return;
                }
                ReviewSetsList reviewSets = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;
                int currentPlace = 0;
                if (reviewSets.Count > 0)
                {
                    currentPlace = reviewSets[reviewSets.Count - 1].SetOrder + 1;
                }
                if (roTr.ReviewSetIds == null || roTr.ReviewSetIds.Count < 1)
                {
                    return;
                }
                this.IsEnabled = false;
                BusyCopying.IsRunning = true;
                sendSetCopyCommand(roTr, 0, currentPlace);

            }

        }

        private void sendSetCopyCommand(ReadOnlyTemplateReview roTr, int currentIndex, int currentOrder)
        {
            ReviewSetCopyCommand rsCC = new ReviewSetCopyCommand(roTr.ReviewSetIds[currentIndex], currentOrder);
            DataPortal<ReviewSetCopyCommand> dp = new DataPortal<ReviewSetCopyCommand>();
            dp.ExecuteCompleted += (o, e2) =>
            {
                if (e2.Error != null)
                {
                    this.IsEnabled = true;
                    BusyCopying.IsRunning = false;
                    RadWindow.Alert(e2.Error.Message);
                    return;
                }
                else
                {
                    if (currentIndex < roTr.ReviewSetIds.Count - 1)
                    {
                        currentIndex++; currentOrder++;
                        sendSetCopyCommand(roTr, currentIndex, currentOrder);
                    }
                    else
                    {//we're done
                        this.IsEnabled = true;
                        BusyCopying.IsRunning = false;
                        CslaDataProvider provider = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
                        provider.Refresh();
                        this.Close();
                    }
                }

            };
            dp.BeginExecute(rsCC);
        }

        private void OpenListOfSets(ReadOnlyTemplateReview roTr)
        {
            bool GetPrivateSets = false;
            if (roTr.TemplateName == "Manually pick from Public codesets..." && roTr.TemplateId == 1000)
            {
                GetPrivateSets = false;
                NameOfList.Text = "Available CodeSets:";
            }
            else if (roTr.TemplateName == "Manually pick from your own codesets..." && roTr.TemplateId == 2000)
            {
                GetPrivateSets = true;
                NameOfList.Text = "CodeSets from Your Reviews:";
            }
            else
            {
                return;
            }
            ShowStep(2);
            CslaDataProvider provider = this.Resources["CodeSetsData4Copy"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(GetPrivateSets);
            CodeSetPreview.SetPublicOrPersonalReviews(GetPrivateSets);
            this.IsEnabled = false;
            provider.Refresh();
        }

        void GetReviewSetsList4Copy_DataChanged(object sender, EventArgs e)
        {
            this.IsEnabled = true;
            CslaDataProvider provider = this.Resources["CodeSetsData4Copy"] as CslaDataProvider;
            if (provider == null) return;
            if (provider.Data != null)
            {
                ReviewSetsList rSets = provider.Data as ReviewSetsList;
                if (rSets != null && rSets.Count > 0)
                {
                    listSets4Copy.SelectedIndex = 0;
                }
            }
        }

        private void listSets4Copy_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            CodeSetPreview.Reset(false);
            if (listSets4Copy.SelectedItem != null)
            {
                ReviewSet RS = listSets4Copy.SelectedItem as ReviewSet;
                CodeSetPreview.DataContext = RS;
                cmdCopySet.IsEnabled = true;
            }
            else
            {
                cmdCopySet.IsEnabled = false;
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cmdBack_Click(object sender, RoutedEventArgs e)
        {
            ShowStep(1);
        }

        private void cmdCopySet_Click(object sender, RoutedEventArgs e)
        {
            ReviewSet rSet = listSets4Copy.SelectedItem as ReviewSet;
            if (rSet == null) return;
            CslaDataProvider provider = App.Current.Resources["CodeSetsData"] as CslaDataProvider;
            if (provider == null)
            {
                return;
            }
            this.IsEnabled = true;
            BusyCopying.IsRunning = false;
            ReviewSetsList reviewSets = provider.Data as ReviewSetsList;
            int currentPlace = 0;
            if (reviewSets.Count > 0)
            {
                currentPlace = reviewSets[reviewSets.Count - 1].SetOrder + 1;
            }
            ReviewSetCopyCommand rsCC = new ReviewSetCopyCommand(rSet.ReviewSetId, currentPlace);
            DataPortal<ReviewSetCopyCommand> dp = new DataPortal<ReviewSetCopyCommand>();
            dp.ExecuteCompleted += (o, e2) =>
            {
                this.IsEnabled = true;
                BusyCopying.IsRunning = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                    return;
                }
                else
                {
                    //we're done
                    provider.Refresh();
                }

            };
            dp.BeginExecute(rsCC);
        }

        
        
    }
}
