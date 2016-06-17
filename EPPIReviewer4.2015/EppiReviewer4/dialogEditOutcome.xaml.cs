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
using BusinessLibrary.BusinessClasses;
using Csla;
using Csla.DataPortalClient;
using Csla.Silverlight;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls;
using Csla.Xaml;


namespace EppiReviewer4
{
    public partial class dialogEditOutcome : UserControl
    {
        public event EventHandler CloseWindowRequest;

        public dialogEditOutcome()
        {
            InitializeComponent();
        }

        public OutcomeItemList CurrentOutcomeItemList;

        public void RefreshProviders()
        {
            //o.SetCalculatedValues();
            
            Outcome o = this.DataContext as Outcome;

            CslaDataProvider provider = this.Resources["ReviewSetOutcomeListData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(o.ItemSetId);
            provider.FactoryParameters.Add(0);
            provider.FactoryMethod = "GetReadOnlyReviewSetOutcomeList";
            provider.Refresh();

            CslaDataProvider provider2 = this.Resources["ReviewSetInterventionListData"] as CslaDataProvider;
            provider2.FactoryParameters.Clear();
            provider2.FactoryParameters.Add(o.ItemSetId);
            provider2.FactoryParameters.Add(0);
            provider2.FactoryMethod = "GetReadOnlyReviewSetInterventionList";
            provider2.Refresh();

            CslaDataProvider provider3 = this.Resources["ReviewSetControlListData"] as CslaDataProvider;
            provider3.FactoryParameters.Clear();
            provider3.FactoryParameters.Add(o.ItemSetId);
            provider3.FactoryParameters.Add(0);
            provider3.FactoryMethod = "GetReadOnlyReviewSetControlList";
            provider3.Refresh();

            SetClusteringVisibility();
        }

        public void BuildOutcomeClassificationList(AttributeSet attributeSet)
        {
            List<AttributeSet> OutcomeClassificationList = new List<AttributeSet>();
            //TreeViewClassifications.Items.Clear();
            ReviewSetsList rsl = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;
            if (rsl != null)
            {
                ReviewSet rs = rsl.GetReviewSet(attributeSet.SetId);
                if (rs != null)
                {
                    AddCodes(OutcomeClassificationList, rs.Attributes);
                    GridViewOutcomeCodes.ItemsSource = OutcomeClassificationList;
                    foreach (AttributeSet attributeset in (GridViewOutcomeCodes.ItemsSource as List<AttributeSet>))
                    {
                        foreach (OutcomeItemAttribute oia in (DataContext as Outcome).OutcomeCodes)
                        {
                            if (oia.AttributeId == attributeset.AttributeId)
                            {
                                GridViewOutcomeCodes.SelectedItems.Add(attributeset);
                            }
                        }
                    }
                }
            }
        }

        private List<AttributeSet> AddCodes(List<AttributeSet> theList, AttributeSetList attributes)
        {
            RadTreeViewItem currentParent = null;
            foreach (AttributeSet attribute in attributes)
            {
                AddCodes(theList, attribute.Attributes);
                if (attribute.AttributeTypeId == 9)
                {
                    if (currentParent == null && attribute.HostAttribute != null)
                    {
                        currentParent = new RadTreeViewItem();
                        currentParent.Header = attribute.HostAttribute.AttributeName;
                        //TreeViewClassifications.Items.Add(currentParent);
                        currentParent.OptionType = OptionListType.None;
                    }
                    RadTreeViewItem newItem = new RadTreeViewItem();
                    newItem.Header = attribute.AttributeName;
                    newItem.Tag = attribute.AttributeId;
                    newItem.DoubleClick += new EventHandler<Telerik.Windows.RadRoutedEventArgs>(newItem_DoubleClick);
                    if (currentParent != null)
                    {
                        currentParent.Items.Add(newItem);
                    }
                    else
                    {
                        //TreeViewClassifications.Items.Add(newItem);
                    }
                    theList.Add(attribute);
                }
            }
            return theList;
        }

        void newItem_DoubleClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            MessageBox.Show("clicked");
            RadTreeViewItem rtvi = sender as RadTreeViewItem;
            int i = Convert.ToInt32(rtvi.Tag);
        }

        private void cmdSaveOutcome_Click(object sender, RoutedEventArgs e)
        {
            Outcome thisOutcome = DataContext as Outcome;
            if (ComboBoxOutcome.SelectedItem != null)
                thisOutcome.ItemAttributeIdOutcome = (ComboBoxOutcome.SelectedItem as ReadOnlyReviewSetOutcome).AttributeId;
            if (ComboBoxIntervention.SelectedItem != null)
                thisOutcome.ItemAttributeIdIntervention = (ComboBoxIntervention.SelectedItem as ReadOnlyReviewSetIntervention).AttributeId;
            if (ComboBoxControl.SelectedItem != null)
                thisOutcome.ItemAttributeIdControl = (ComboBoxControl.SelectedItem as ReadOnlyReviewSetControl).AttributeId;

            if (thisOutcome.IsNew == true)
            {
                CurrentOutcomeItemList.Add(thisOutcome);
            }
            if (CurrentOutcomeItemList.HasSavedHandler == false)
            {
                CurrentOutcomeItemList.HasSavedHandler = true;
                CurrentOutcomeItemList.Saved += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        string attributes = "";
                        foreach (AttributeSet attributeSet in GridViewOutcomeCodes.SelectedItems)
                        {
                            if (attributes == "")
                            {
                                attributes = attributeSet.AttributeId.ToString();
                            }
                            else
                            {
                                attributes += "," + attributeSet.AttributeId.ToString();
                            }
                        }
                        DataPortal<OutcomeItemAttributesCommand> dp = new DataPortal<OutcomeItemAttributesCommand>();
                        OutcomeItemAttributesCommand command = new OutcomeItemAttributesCommand(
                            (e2.NewObject as Outcome).OutcomeId,
                            attributes);

                        dp.ExecuteCompleted += (o2, e22) =>
                        {
                            if (this.CloseWindowRequest != null)
                                this.CloseWindowRequest.Invoke(this, EventArgs.Empty);
                        };
                        dp.BeginExecute(command);
                        
                    }
                    this.IsEnabled = true;
                };
            }
            this.IsEnabled = false;
            thisOutcome.ApplyEdit();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Outcome thisOutcome = DataContext as Outcome;
            switch (thisOutcome.NRows)
            {
                case 0:
                    LayoutRoot.RowDefinitions[3].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 35;
                    break;

                case 1:
                    LayoutRoot.RowDefinitions[3].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[4].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 0;

                    break;

                case 2:
                    LayoutRoot.RowDefinitions[3].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 0;
                    break;

                case 3:
                    LayoutRoot.RowDefinitions[3].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 0;
                    break;

                case 4:
                    LayoutRoot.RowDefinitions[3].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 0;
                    break;

                case 5:
                    LayoutRoot.RowDefinitions[3].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 0;
                    break;

                case 6:
                    LayoutRoot.RowDefinitions[3].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 35;
                    break;
            }
            SetClusteringVisibility();
        }

        private void CslaDataProviderReadOnlyReviewSetOutcomeList_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReviewSetOutcomeListData"]);
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            Outcome o = this.DataContext as Outcome;
            foreach (ReadOnlyReviewSetOutcome RSO in provider.Data as ReadOnlyReviewSetOutcomeList)
            {
                if (RSO.AttributeId == o.ItemAttributeIdOutcome)
                {
                    ComboBoxOutcome.SelectedItem = RSO;
                }
            }
        }

        private void CslaDataProviderReadOnlyReviewSetInterventionList_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReviewSetInterventionListData"]);
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            Outcome o = this.DataContext as Outcome;
            foreach (ReadOnlyReviewSetIntervention RSI in provider.Data as ReadOnlyReviewSetInterventionList)
            {
                if (RSI.AttributeId == o.ItemAttributeIdIntervention)
                {
                    ComboBoxIntervention.SelectedItem = RSI;
                }
            }
        }

        private void CslaDataProviderReadOnlyReviewSetControlList_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ReviewSetControlListData"]);
            if (provider.Error != null)
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            Outcome o = this.DataContext as Outcome;
            foreach (ReadOnlyReviewSetControl RSC in provider.Data as ReadOnlyReviewSetControlList)
            {
                if (RSC.AttributeId == o.ItemAttributeIdControl)
                {
                    ComboBoxControl.SelectedItem = RSC;
                }
            }
        }

        private void TextBoxData1_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
                tb.SelectAll();
        }

        private void cbUnitOfAnalysis_Click(object sender, RoutedEventArgs e)
        {
            if (cbUnitOfAnalysis.IsChecked == true)
            {
                LayoutRoot.RowDefinitions[10].MaxHeight = 35;
            }
            else
            {
                LayoutRoot.RowDefinitions[10].MaxHeight = 0;
                Outcome o = this.DataContext as Outcome;
                if (o != null)
                {
                    o.Data9 = 0;
                    o.Data10 = 0;
                }
            }
        }

        private void SetClusteringVisibility()
        {
            Outcome o = this.DataContext as Outcome;
            if (o.Data9 == 0)
            {
                LayoutRoot.RowDefinitions[10].MaxHeight = 0;
                cbUnitOfAnalysis.IsChecked = false;
            }
            else
            {
                LayoutRoot.RowDefinitions[10].MaxHeight = 35;
                cbUnitOfAnalysis.IsChecked = true;
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Outcome o = this.DataContext as Outcome;
            o.CancelEdit();
            if (this.CloseWindowRequest != null)
                this.CloseWindowRequest.Invoke(this, EventArgs.Empty);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var parentGrid = checkBox.ParentOfType<GridViewDataControl>();

            if (checkBox.IsChecked.Value)
                parentGrid.SelectAll();
            else
                parentGrid.UnselectAll();
        }
    }
}
