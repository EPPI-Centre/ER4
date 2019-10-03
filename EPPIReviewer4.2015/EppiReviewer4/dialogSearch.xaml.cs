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
using Csla.Silverlight;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class dialogSearch : UserControl
    {
        public event EventHandler CloseWindowRequest;
        public event EventHandler<ItemListRefreshEventArgs> RefreshItemList;

        public dialogSearch()
        {
            InitializeComponent();
            codesSelectControlSearchSelect.SelectionMode("Multiple");
        }

        private string _currentTab;

        public void SetupDialog(string currentTab)
        {
            _currentTab = currentTab;
            if (RadioButtonCodedByAnyone != null)
            {
                RadioButtonCodedByAnyone.IsChecked = true;
            }
            if (ComboSearchTypeSelect != null)
            {
                switch (ComboSearchTypeSelect.SelectedIndex)
                {
                    case 0:
                        dialogSearchGrid.RowDefinitions[1].MaxHeight = 40;
                        dialogSearchGrid.RowDefinitions[2].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[3].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[4].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[5].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[7].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
                        break;

                    case 1:
                        dialogSearchGrid.RowDefinitions[1].MaxHeight = 40;
                        dialogSearchGrid.RowDefinitions[2].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[3].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[4].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[5].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[7].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
                        break;

                    case 2:
                    case 3:
                        dialogSearchGrid.RowDefinitions[1].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[2].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[3].MaxHeight = 95;
                        tblockIDs.Visibility = System.Windows.Visibility.Visible;
                        dialogSearchGrid.RowDefinitions[4].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[5].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[7].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
                        break;

                    case 4:
                        dialogSearchGrid.RowDefinitions[1].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[2].MaxHeight = 40;
                        dialogSearchGrid.RowDefinitions[3].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[4].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[5].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[7].MaxHeight = 35;
                        dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
                        break;

                    case 5:
                        dialogSearchGrid.RowDefinitions[1].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[2].MaxHeight = 40;
                        dialogSearchGrid.RowDefinitions[3].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[4].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[5].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[7].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
                        break;

                    case 6:
                        dialogSearchGrid.RowDefinitions[1].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[2].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[3].MaxHeight = 35;
                        tblockIDs.Visibility = System.Windows.Visibility.Collapsed;
                        dialogSearchGrid.RowDefinitions[4].MaxHeight = 35;
                        dialogSearchGrid.RowDefinitions[5].MaxHeight = 40;
                        dialogSearchGrid.RowDefinitions[7].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
                        break;

                    case 7:
                        dialogSearchGrid.RowDefinitions[1].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[2].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[3].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[4].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[5].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[7].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
                        break;
                    case 8:
                        dialogSearchGrid.RowDefinitions[1].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[2].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[3].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[4].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[5].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[7].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
                        break;
                    case 9:
                        dialogSearchGrid.RowDefinitions[1].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[2].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[3].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[4].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[5].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[7].MaxHeight = 0;
                        dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
                        break;
                }
                
            }
        }

        private void ComboSearchTypeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetupDialog(_currentTab);
        }

        private void cmddialogSearchCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.CloseWindowRequest != null)
                this.CloseWindowRequest.Invoke(this, EventArgs.Empty);
        }

        private void dialogsearchTextBoxTextSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            dialogsearchTextBoxTextSearch.SelectAll();
        }

        private void cmddialogSearchSearch_Click(object sender, RoutedEventArgs e)
        {
            switch (ComboSearchTypeSelect.SelectedIndex)
            {
                case 0:
                    SearchCodes(true);
                    break;

                case 1:
                    SearchCodes(false);
                    break;

                case 2:
                    SearchIDs();
                    break;

                case 3:
                    SearchImportedIDs();
                    break;

                case 4:
                    SearchCodeSet(true);
                    break;

                case 5:
                    SearchCodeSet(false);
                    break;

                case 6:
                    SearchFreeText();
                    break;

                case 7:
                    SearchForUploadedFiles(true);
                    break;

                case 8:
                    SearchForUploadedFiles(false);
                    break;
                case 9:
                    SearchNullAbstract();
                    break;
            }
        }

        private void SearchImportedIDs()
        {
            SearchImportedIDsCommand cmd = new SearchImportedIDsCommand(dialogsearchTextBoxTextSearch.Text, dialogSearchRadioButtonIncluded.IsChecked.Value);
            if (cmd.Title == "No valid ID was found")
                Telerik.Windows.Controls.RadWindow.Alert(cmd.Title + Environment.NewLine + "Please one or more (comma separated) item IDs");
            else
            {
                DataPortal<SearchImportedIDsCommand> dp = new DataPortal<SearchImportedIDsCommand>();
                dp.ExecuteCompleted += (o, e2) =>
                {
                    this.IsEnabled = true;
                    dialogSearchBusyAnimation.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        CslaDataProvider provider = App.Current.Resources["SearchesData"] as CslaDataProvider;
                        provider.FactoryParameters.Clear();
                        provider.FactoryMethod = "GetSearchList";
                        provider.Refresh();
                        LoadSearch(e2.Object.SearchId, dialogsearchTextBoxTextSearch.Text);
                    }
                };
                dialogSearchBusyAnimation.IsRunning = true;
                this.IsEnabled = false;
                dp.BeginExecute(cmd);
            }
        }

        private void SearchIDs()
        {
            SearchIDsCommand cmd = new SearchIDsCommand(dialogsearchTextBoxTextSearch.Text,dialogSearchRadioButtonIncluded.IsChecked.Value);
            if (cmd.Title == "No valid ID was found")
                Telerik.Windows.Controls.RadWindow.Alert(cmd.Title + Environment.NewLine + "Please one or more (comma separated) item IDs");
            else
            {
                DataPortal<SearchIDsCommand> dp = new DataPortal<SearchIDsCommand>();
                dp.ExecuteCompleted += (o, e2) =>
                {
                    this.IsEnabled = true;
                    dialogSearchBusyAnimation.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        CslaDataProvider provider = App.Current.Resources["SearchesData"] as CslaDataProvider;
                        provider.FactoryParameters.Clear();
                        provider.FactoryMethod = "GetSearchList";
                        provider.Refresh();
                        LoadSearch(e2.Object.SearchId, dialogsearchTextBoxTextSearch.Text);
                    }
                };
                dialogSearchBusyAnimation.IsRunning = true;
                this.IsEnabled = false;
                dp.BeginExecute(cmd);
            }
        }

        private void SearchFreeText()
        {
            if (dialogsearchTextBoxTextSearch.Text == "")
            {
                MessageBox.Show("Please enter some text to search on");
            }
            else
            {
                DataPortal<SearchFreeTextCommand> dp = new DataPortal<SearchFreeTextCommand>();
                SearchFreeTextCommand command = new SearchFreeTextCommand(
                    dialogsearchTextBoxTextSearch.Text,
                    dialogSearchRadioButtonIncluded.IsChecked.Value,
                    (dialogSearchComboTextSearchType.SelectedItem as ComboBoxItem).Tag.ToString());
                dp.ExecuteCompleted += (o, e2) =>
                {
                    this.IsEnabled = true;
                    dialogSearchBusyAnimation.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        CslaDataProvider provider = App.Current.Resources["SearchesData"] as CslaDataProvider;
                        provider.FactoryParameters.Clear();
                        provider.FactoryMethod = "GetSearchList";
                        provider.Refresh();
                        LoadSearch(e2.Object.SearchId, dialogsearchTextBoxTextSearch.Text);
                    }
                };
                dialogSearchBusyAnimation.IsRunning = true;
                this.IsEnabled = false;
                dp.BeginExecute(command);
            }
        }

        private void SearchForUploadedFiles(bool present)
        {

            string title;
            if (present)
            {
                title = "With at least one document uploaded.";
            }
            else
            {
                title = "Without any documents uploaded.";
            }
            DataPortal<SearchForUploadedFilesCommand> dp = new DataPortal<SearchForUploadedFilesCommand>();
            SearchForUploadedFilesCommand command = new SearchForUploadedFilesCommand(
                title,
                dialogSearchRadioButtonIncluded.IsChecked.Value,
                present ? true : false);
            dp.ExecuteCompleted += (o, e2) =>
            {
                this.IsEnabled = true;
                dialogSearchBusyAnimation.IsRunning = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    CslaDataProvider provider = App.Current.Resources["SearchesData"] as CslaDataProvider;
                    provider.FactoryParameters.Clear();
                    provider.FactoryMethod = "GetSearchList";
                    provider.Refresh();
                    LoadSearch(e2.Object.SearchId, dialogsearchTextBoxTextSearch.Text);
                }
            };
            dialogSearchBusyAnimation.IsRunning = true;
            this.IsEnabled = false;
            dp.BeginExecute(command);
        }

        private void SearchCodes(bool withCode)
        {
            string attributeIDs = "";
            string attributeNames = "";
            if ((codesSelectControlSearchSelect.SelectedAttributeSet() == null) && codesSelectControlSearchSelect.SelectedAttributes().Count == 0)
            {
                MessageBox.Show("Please select one or more codes");
            }
            else
            {
                if (codesSelectControlSearchSelect.SelectedAttributes().Count == 0)
                {
                    attributeIDs = codesSelectControlSearchSelect.SelectedAttributeSet().AttributeSetId.ToString();
                    attributeNames = codesSelectControlSearchSelect.SelectedAttributeSet().AttributeName.ToString();
                }
                foreach (AttributeSet attribute in codesSelectControlSearchSelect.SelectedAttributes())
                {
                    if (attributeIDs == "")
                    {
                        attributeIDs = attribute.AttributeSetId.ToString();
                        attributeNames = attribute.AttributeName;
                    }
                    else
                    {
                        attributeIDs += "," + attribute.AttributeSetId.ToString();
                        attributeNames += ", OR " + attribute.AttributeName;
                    }
                }

                string searchTitle = withCode == true ? "Coded with: " + attributeNames : "Not coded with: " + attributeNames;
                DataPortal<SearchCodesCommand> dp = new DataPortal<SearchCodesCommand>();
                SearchCodesCommand command = new SearchCodesCommand(
                    searchTitle,
                    attributeIDs,
                    dialogSearchRadioButtonIncluded.IsChecked.Value,
                    withCode);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    this.IsEnabled = true;
                    dialogSearchBusyAnimation.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        CslaDataProvider provider = App.Current.Resources["SearchesData"] as CslaDataProvider;
                        provider.FactoryParameters.Clear();
                        provider.FactoryMethod = "GetSearchList";
                        provider.Refresh();
                        LoadSearch(e2.Object.SearchId, searchTitle);
                    }
                };
                this.IsEnabled = false;
                dialogSearchBusyAnimation.IsRunning = true;
                dp.BeginExecute(command);
            }
        }

        private void SearchCodeSet(bool hasCodes)
        {
            ReviewSet rs = dialogSearchComboSelectCodeSet.SelectedItem as ReviewSet;
            if (rs == null)
            {
                MessageBox.Show("Please select a review set first");
                return;
            }
            if (RadioButtonCodedByThisPerson.IsChecked == true &&
                (ComboReviewContactsSearch.SelectedItem as ReviewContactNVL.NameValuePair) == null)
            {
                MessageBox.Show("Please select a reviewer (if you intend to filter by reviewer)");
                return;
            }
            {
                DataPortal<SearchCodeSetCheckCommand> dp = new DataPortal<SearchCodeSetCheckCommand>();
                SearchCodeSetCheckCommand command = new SearchCodeSetCheckCommand(
                    rs.SetId,
                    dialogSearchRadioButtonIncluded.IsChecked.Value,
                    hasCodes,
                    rs.SetName,
                    RadioButtonCodedByThisPerson.IsChecked == true ?
                        (ComboReviewContactsSearch.SelectedItem as ReviewContactNVL.NameValuePair).Key : 0,
                    RadioButtonCodedByThisPerson.IsChecked == true ?
                        (ComboReviewContactsSearch.SelectedItem as ReviewContactNVL.NameValuePair).Value : "");
                dp.ExecuteCompleted += (o, e2) =>
                {
                    dialogSearchBusyAnimation.IsRunning = false;
                    this.IsEnabled = true;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        CslaDataProvider provider = App.Current.Resources["SearchesData"] as CslaDataProvider;
                        provider.FactoryParameters.Clear();
                        provider.FactoryMethod = "GetSearchList";
                        provider.Refresh();
                        LoadSearch(e2.Object.SearchId, hasCodes == true ? "Coded with: " + rs.SetName : "Not coded with: " + rs.SetName);
                    }
                };
                this.IsEnabled = false;
                dialogSearchBusyAnimation.IsRunning = true;
                dp.BeginExecute(command);
            }
        }

        private void SearchNullAbstract()
        {
            DataPortal<SearchNullAbstractCommand> dp = new DataPortal<SearchNullAbstractCommand>();
            SearchNullAbstractCommand command = new SearchNullAbstractCommand(dialogSearchRadioButtonIncluded.IsChecked.Value);
            dp.ExecuteCompleted += (o, e2) =>
            {
                dialogSearchBusyAnimation.IsRunning = false;
                this.IsEnabled = true;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    CslaDataProvider provider = App.Current.Resources["SearchesData"] as CslaDataProvider;
                    provider.FactoryParameters.Clear();
                    provider.FactoryMethod = "GetSearchList";
                    provider.Refresh();
                    LoadSearch(e2.Object.SearchId, "Citations without an abstract");
                }
            };
            this.IsEnabled = false;
            dialogSearchBusyAnimation.IsRunning = true;
            dp.BeginExecute(command);

        }

        private void LoadSearch(int SearchId, string title)
        {
            if (RefreshItemList != null && _currentTab == "DocumentsTab")
            {
                SelectionCriteria selcon = new SelectionCriteria();
                selcon.SearchId = SearchId;
                selcon.Description = title;
                selcon.ListType = "GetItemSearchList";
                ItemListRefreshEventArgs ilrea = new ItemListRefreshEventArgs(selcon);
                RefreshItemList.Invoke(this, ilrea);
                if (this.CloseWindowRequest != null)
                    this.CloseWindowRequest.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (this.CloseWindowRequest != null)
                    this.CloseWindowRequest.Invoke(this, EventArgs.Empty);
            }
        }

        private void RadioButtonCodedByAnyone_Click(object sender, RoutedEventArgs e)
        {
            dialogSearchGrid.RowDefinitions[8].MaxHeight = 0;
        }

        private void RadioButtonCodedByThisPerson_Click(object sender, RoutedEventArgs e)
        {
            dialogSearchGrid.RowDefinitions[8].MaxHeight = 35;
        }
    }
}
