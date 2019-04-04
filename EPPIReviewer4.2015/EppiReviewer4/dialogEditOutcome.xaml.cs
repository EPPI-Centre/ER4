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
        public Int64 CurrentItemId;

        public void RefreshProviders()
        {
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

            provider = ((CslaDataProvider)App.Current.Resources["ItemTimepointsData"]);
            ComboBoxTimepoint.SelectedItem = null;
            foreach (ItemTimepoint it in provider.Data as ItemTimepointList)
            {
                if (it.ItemTimepointId == o.ItemTimepointId)
                {
                    ComboBoxTimepoint.SelectedItem = it;
                }
            }

            provider = ((CslaDataProvider)App.Current.Resources["ItemArmsData"]);
            ComboBoxGrp1Arm.SelectedItem = null;
            ComboBoxGrp2Arm.SelectedItem = null;
            foreach (ItemArm ia in provider.Data as ItemArmList)
            {
                if (ia.ItemArmId == o.ItemArmIdGrp1)
                {
                    ComboBoxGrp1Arm.SelectedItem = ia;
                }
                if (ia.ItemArmId == o.ItemArmIdGrp2)
                {
                    ComboBoxGrp2Arm.SelectedItem = ia;
                }
            }

            SetClusteringVisibility();
        }

        public void BuildOutcomeClassificationList(AttributeSet attributeSet)
        {
            TreeViewClassifications.Items.Clear();
            ReviewSetsList rsl = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;
            if (rsl != null)
            {
                ReviewSet rs = rsl.GetReviewSet(attributeSet.SetId);
                if (rs != null)
                {
                    AddClassificationTreeCodes(rs.Attributes, null);
                }
            }
        }

        private void AddClassificationTreeCodes(AttributeSetList attributes, RadTreeViewItem currentParent)
        {
            foreach (AttributeSet attribute in attributes)
            {
                RadTreeViewItem newItem = new RadTreeViewItem();
                newItem.Header = attribute.AttributeName;
                newItem.Tag = attribute.AttributeId;
                newItem.DoubleClick += new EventHandler<Telerik.Windows.RadRoutedEventArgs>(newItem_DoubleClick);
                if (attribute.AttributeTypeId == 1)
                {
                    newItem.OptionType = OptionListType.None;
                }
                else
                {
                    newItem.OptionType = OptionListType.CheckList;
                }
                foreach (OutcomeItemAttribute oia in (DataContext as Outcome).OutcomeCodes)
                {
                    if (oia.AttributeId == attribute.AttributeId)
                    {
                        newItem.IsChecked = true;
                    }
                }
                if (attribute.AttributeTypeId > 1) // have to do this after the checking is set, or we trigger the event
                {
                    newItem.Checked += OutcomeClassificationItem_Checked;
                    newItem.Unchecked += OutcomeClassificationItem_Unchecked;
                }

                if (currentParent != null)
                {
                    currentParent.Items.Add(newItem);
                }
                else
                {
                    TreeViewClassifications.Items.Add(newItem);
                }
                AddClassificationTreeCodes(attribute.Attributes, newItem);
            }
        }

        private void OutcomeClassificationItem_Unchecked(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            RadTreeViewItem item = e.OriginalSource as RadTreeViewItem;
            foreach (OutcomeItemAttribute oia in (DataContext as Outcome).OutcomeCodes)
            {
                if (oia.AttributeId == Convert.ToInt64(item.Tag.ToString()))
                {
                    (DataContext as Outcome).OutcomeCodes.Remove(oia);
                    break;
                }
            }
        }

        private void OutcomeClassificationItem_Checked(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            RadTreeViewItem item = e.OriginalSource as RadTreeViewItem;

            // This is probably unnecessary, but better be safe, than risk ending up with multiple codes saved for some reason
            foreach (OutcomeItemAttribute oia1 in (DataContext as Outcome).OutcomeCodes)
            {
                if (oia1.AttributeId == Convert.ToInt64(item.Tag.ToString()))
                {
                    return;
                }
            }

            OutcomeItemAttribute oia = new OutcomeItemAttribute();
            oia.AttributeId = Convert.ToInt64(item.Tag.ToString());
            oia.AttributeName = item.Header.ToString();
            (DataContext as Outcome).OutcomeCodes.Add(oia);
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
            if (ComboBoxTimepoint.SelectedItem != null)
                thisOutcome.ItemTimepointId = (ComboBoxTimepoint.SelectedItem as ItemTimepoint).ItemTimepointId;
            if (ComboBoxGrp1Arm.SelectedItem != null)
                thisOutcome.ItemArmIdGrp1 = (ComboBoxGrp1Arm.SelectedItem as ItemArm).ItemArmId;
            if (ComboBoxGrp2Arm.SelectedItem != null)
                thisOutcome.ItemArmIdGrp2 = (ComboBoxGrp2Arm.SelectedItem as ItemArm).ItemArmId;

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
                        foreach (OutcomeItemAttribute oia in (DataContext as Outcome).OutcomeCodes)
                        {
                            if (attributes == "")
                            {
                                attributes = oia.AttributeId.ToString();
                            }
                            else
                            {
                                attributes += "," + oia.AttributeId.ToString();
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
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[9].MaxHeight = 35;
                    break;

                case 1:
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[9].MaxHeight = 0;

                    break;

                case 2:
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[9].MaxHeight = 0;
                    break;

                case 3:
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[9].MaxHeight = 0;
                    break;

                case 4:
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 0;
                    LayoutRoot.RowDefinitions[9].MaxHeight = 0;
                    break;

                case 5:
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[9].MaxHeight = 0;
                    break;

                case 6:
                    LayoutRoot.RowDefinitions[4].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[5].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[6].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[7].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[8].MaxHeight = 35;
                    LayoutRoot.RowDefinitions[9].MaxHeight = 35;
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
            {
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
                return;
            }
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
                LayoutRoot.RowDefinitions[11].MaxHeight = 35;
            }
            else
            {
                LayoutRoot.RowDefinitions[11].MaxHeight = 0;
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
                LayoutRoot.RowDefinitions[11].MaxHeight = 0;
                cbUnitOfAnalysis.IsChecked = false;
            }
            else
            {
                LayoutRoot.RowDefinitions[11].MaxHeight = 35;
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
