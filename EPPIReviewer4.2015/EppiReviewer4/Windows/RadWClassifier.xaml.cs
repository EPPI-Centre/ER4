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
using Csla;
using Csla.Xaml;

using Telerik.Windows.Controls;

namespace EppiReviewer4
{
    /// <summary>
    /// The process of creating a new window works as follow:
    /// 1) add new (template) in the Windows folder
    /// 2) find the original declaration in XAML and move it into the new XAML file, save the x:Name attribute for later. See xaml template for details. Also copy or move Reference entries as described in XAML.
    /// 3) in the old code-behind file, create a private instance of the new window type:
    ///     EXAMPLE: (this is a class member, don't place it in a method or porperty; change name of class and object appropriately)
    ///     private RadWClassifier RadWExample = new RadWClassifier(); 
    ///     the name of the private object should match the original name of the window (XAML declaration: x:Name="window...")
    /// 4) find all the event handlers declared in xaml,
    ///     FOREACH event handler
    ///         4a) in the new code-behind file, create an event, be sure to use the right EventArgs (see example below)
    ///         4b) in the new code-behind file, create a handler (see example below)
    ///         4c) in the old code-behind file, constructor method, hook up the old handler to the event created in 4a), place it after the InitializeComponent() call
    ///             EXAMPLE:
    ///             Public OldPage()
    ///             {
    ///                 InitializeComponent();
    ///                 [...]
    ///                 RadWExample.cmdButton_Clicked += new EventHandler<RoutedEventArgs>(Original_Handling_Method_In_OldPage);
    ///                 [...]
    ///             }
    ///     LOOP
    /// 5) Build app: this will probably fail, generating a list of errors where code in OldPage references elements in what used to be in local XAML.
    ///     Fix all errors, this is usually done by adding a prefix to elements that are now in the new xaml file.
    ///     EXAMPLE:
    ///     SomeList.SelectedIndex >-becomes-> RadWExample.SomeList.SelectedIndex
    ///     BEWARE: this is where things may go wrong. If the same window (or elements of it) is(/are) used in more than one way, changed dynamically, recylced, or who knows what,
    ///     then the new version of the code may insert bugs. You should understand the code you're changing!!
    /// 6) If you have copied or moved Resources into the new XAML file (point 2):
    ///     FOREACH Moved Resource
    ///         6a) If the Resouce was moved or copied into the new XAML,
    ///             go to the old file code-behind and search for the moved Resources name, fix their reference:
    ///             EXAMPLE:
    ///             this.Resources["ResName"] >-becomes-> RadWExample.Resources["ResName"] 
    ///             WARNING!!:If you have copied (not Moved!!) the resource across (because it's needed in both places, but you don't want it in App.xaml), 
    ///                    !! then you should fix some references BUT not all (!!!)
    ///                    !! some code will need access to the old reference, some to the one in the new page, so pay attention!
    ///                    !! it would certainly be easier to move them onto App.xaml
    ///         6b) If you have moved Resources into App.XAML, search for the name of the moved resouce in the old file code-behind, fix the references:
    ///             EXAMPLE:
    ///         this.Resources["ResName"] >-becomes-> App.Current.Resources["ResName"]
    /// 7) test it all! And I mean test each different usage scenario.
    /// 8) END
    /// </summary>
    public partial class RadWClassifier : RadWindow //change this to give it a unque name, must inherit from Radwindow.
    {
        public RadWClassifier()
        {
            InitializeComponent();
        }

        private void cmdBuildModel_Click(object sender, RoutedEventArgs e)
        {
            if (codesSelectControlTrainOn.selectedAttributes != null && codesSelectControlTrainNotOn.selectedAttributes != null)
            {
                if (MessageBox.Show("Are you sure you want to build this model?", "Are you sure?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (codesSelectControlTrainOn.SelectedAttributeSet().AttributeId == codesSelectControlTrainNotOn.SelectedAttributeSet().AttributeId)
                    {
                        RadWindow.Alert("Ahem. You do need to select different codes...");
                        return;
                    }
                    CslaDataProvider provider = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
                    ReviewInfo RevInfo = provider.Data as ReviewInfo;

                    DataPortal<ClassifierCommand> dp = new DataPortal<ClassifierCommand>();
                    ClassifierCommand command = new ClassifierCommand(
                        tbModelTitle.Text,
                        codesSelectControlTrainOn.SelectedAttributeSet().AttributeId,
                        codesSelectControlTrainNotOn.SelectedAttributeSet().AttributeId, 0, -1, -1);
                    dp.ExecuteCompleted += (o, e2) =>
                    {
                        //BusyLoading.IsRunning = false;
                        textUploadingDataBuild.Visibility = Visibility.Collapsed;
                        cmdBuildModel.IsEnabled = true;
                        //cmdLearnAndApplyModel.IsEnabled = true;
                        cmdApplyModel.IsEnabled = true;
                        if (e2.Error != null)
                        {
                            RadWindow.Alert(e2.Error.Message);
                        }
                        else
                        {
                            if ((e2.Object as ClassifierCommand).ReturnMessage == "Already running")
                            {
                                RadWindow.Alert("You have a classification task in progress." +
                                    Environment.NewLine + " Please wait until it has completed before starting another.");
                            }
                            else
                                if ((e2.Object as ClassifierCommand).ReturnMessage == "Insufficient data")
                            {
                                RadWindow.Alert("Sorry, insufficient data for training." + Environment.NewLine +
                                    "Please ensure you have at least 5 items for the classifier" + Environment.NewLine +
                                    "to 'learn' from. (For good performance, many more.)");
                            }
                            else
                            if ((e2.Object as ClassifierCommand).ReturnMessage == "BuildFailed")
                            {
                                RadWindow.Alert("Sorry, building the model failed." + Environment.NewLine +
                                    "This is probably because your data set is too small." +
                                    "If possible, try again with more data.");
                            }
                            else
                            {
                                RadWindow.Alert("Your data have been successfully uploaded to the server." + Environment.NewLine +
                                "Building models can take a long time, so you can continue to work" + Environment.NewLine +
                                "on other things, refreshing the list of models from time to time");
                            }
                            /*
                            RadWindow.Alert((e2.Object as ClassifierCommand).ReturnMessage);
                            CslaDataProvider provider2 = this.Resources["ClassifierModelListData"] as CslaDataProvider;
                            if (provider2 != null)
                                provider2.Refresh();
                            */
                        }
                    };
                    //BusyLoading.IsRunning = true;
                    textUploadingDataBuild.Visibility = Visibility.Visible;
                    cmdBuildModel.IsEnabled = false;
                    cmdApplyModel.IsEnabled = false;
                    //cmdLearnAndApplyModel.IsEnabled = false;
                    command.RevInfo = RevInfo;
                    dp.BeginExecute(command);
                }
            }
        }

        private void ClassifierModelsData_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ClassifierModelListData"]);
            if (provider.Error != null)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            }
        }

        private void cmdApplyModel_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider1 = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            ReviewInfo RevInfo = provider1.Data as ReviewInfo;
            string modelTitle = "RCT";
            Int32 ModelId = -1; // the RCT model as default
            if (rbApplyDAREModel.IsChecked == true)
            {
                modelTitle = "Systematic review";
                ModelId = -2;
            }
            Int64 AttributeId = -1; // the attributeID we might be limiting the application of model to. -1 == apply to whole review
            int SourceId = -2;//source_id == -1 means "sourceless items"

            if (rbApplySelectedModel.IsChecked == true)
            {
                CslaDataProvider provider = ((CslaDataProvider)this.Resources["ClassifierModelListData"]);
                if (provider != null && GridViewClassifierModels.SelectedItem != null)
                {
                    modelTitle = (GridViewClassifierModels.SelectedItem as ClassifierModel).ModelTitle;
                    ModelId = (GridViewClassifierModels.SelectedItem as ClassifierModel).ModelId;
                }
                else
                {
                    RadWindow.Alert("Please select a model first.");
                    return;
                }
                ClassifierModel selectedModel = GridViewClassifierModels.SelectedItem as ClassifierModel;
                if (selectedModel.Precision * selectedModel.Recall == 0)
                {
                    RadWindow.Alert("Sorry, this model cannot be applied." + Environment.NewLine +
                        "It has either not finished building yet," + Environment.NewLine +
                        "or cannot distinguish between your selected codes.");
                    return;
                }
                if (selectedModel.Precision < 0)
                {
                    RadWindow.Alert("Sorry, this model cannot be applied as it failed" + Environment.NewLine +
                        "to build properly. (It is here for information only and" + Environment.NewLine +
                        "can be deleted.)");
                    return;
                }
            }

            if (rbApplyToSelected.IsChecked == true)
            {
                if (codesSelectControlClassifyTo.SelectedAttributeSet() != null)
                {
                    AttributeId = codesSelectControlClassifyTo.SelectedAttributeSet().AttributeId;
                }
                else
                {
                    RadWindow.Alert("Please select code to limit model to first.");
                    return;
                }
            }

            if (rbApplyToSource.IsChecked == true)
            {
                if (comboSources.SelectedIndex > -1)
                {
                    SourceId = (comboSources.SelectedItem as ReadOnlySource).Source_ID;
                }
                else
                {
                    RadWindow.Alert("Please select a source first.");
                    return;
                }
            }

            if (MessageBox.Show("Are you sure you want to apply the selected model?", "Are you sure?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                DataPortal<ClassifierCommand> dp = new DataPortal<ClassifierCommand>();
                ClassifierCommand command = new ClassifierCommand(
                    modelTitle,
                    -1,
                    -1,
                    AttributeId,
                    ModelId,
                    SourceId);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    //BusyLoading.IsRunning = false;
                    textUploadingDataApply.Visibility = Visibility.Collapsed;
                    cmdBuildModel.IsEnabled = true;
                    //cmdLearnAndApplyModel.IsEnabled = true;
                    cmdApplyModel.IsEnabled = true;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RadWindow.Alert("Your data have been uploaded for classification successfully." + Environment.NewLine +
                            "As the classification can take some time," + Environment.NewLine +
                            "please refresh the list of search results periodially.");
                        // no point in refreshing the search, as the command returns before it's been updated
                        //RadWindow.Alert((e2.Object as ClassifierCommand).ReturnMessage);
                        //RadWindow.Alert("Check the latest search for your results");
                        //CslaDataProvider provider2 = App.Current.Resources["SearchesData"] as CslaDataProvider;
                        //provider2.FactoryParameters.Clear();
                        //provider2.FactoryMethod = "GetSearchList";
                        //provider2.Refresh();
                    }
                };
                //BusyLoading.IsRunning = true;
                textUploadingDataApply.Visibility = Visibility.Visible;
                cmdBuildModel.IsEnabled = false;
                cmdApplyModel.IsEnabled = false;
                //cmdLearnAndApplyModel.IsEnabled = false;
                command.RevInfo = RevInfo;
                dp.BeginExecute(command);
            }
        }

        // Decided not to support build and apply for the time being.
        //private void cmdLearnAndApplyModel_Click(object sender, RoutedEventArgs e)
        //{
        //    /*if (codesSelectControlTrainOn.SelectedAttributeSet() != null && codesSelectControlTrainNotOn.SelectedAttributeSet() != null &&
        //        codesSelectControlClassifyTo.SelectedAttributeSet() != null)
        //    { */
        //        if (MessageBox.Show("Are you sure you want to build and apply this model?", "Are you sure?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        //        {
        //        CslaDataProvider provider1 = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
        //        ReviewInfo RevInfo = provider1.Data as ReviewInfo;
        //        DataPortal<ClassifierCommand> dp = new DataPortal<ClassifierCommand>();
        //            ClassifierCommand command = new ClassifierCommand(
        //                tbModelTitle.Text,
        //                codesSelectControlTrainOn.SelectedAttributeSet().AttributeId,
        //                codesSelectControlTrainNotOn.SelectedAttributeSet().AttributeId,
        //                codesSelectControlClassifyTo.SelectedAttributeSet().AttributeId, - 2);
        //            dp.ExecuteCompleted += (o, e2) =>
        //            {
        //                //BusyLoading.IsRunning = false;
        //                cmdBuildModel.IsEnabled = true;
        //                cmdApplyModel.IsEnabled = true;
        //                cmdLearnAndApplyModel.IsEnabled = true;
        //                if (e2.Error != null)
        //                {
        //                    RadWindow.Alert(e2.Error.Message);
        //                }
        //                else
        //                {
        //                    if ((e2.Object as ClassifierCommand).ReturnMessage == "Sorry, another classifcation task is already in progress on this review")
        //                    {
        //                        RadWindow.Alert("Sorry, another classifcation task is already in progress on this review");
        //                    }
        //                    else
        //                    {
        //                        RadWindow.Alert("Your data have been uploaded successfully." + Environment.NewLine +
        //                            "Building and then applying machine learning models can take hours," + Environment.NewLine +
        //                            "so you can continue to work as usual, and your results will appear on the search page." + Environment.NewLine +
        //                            "Please refresh the list of searches from time to time to check whether classification is complete.");
        //                    }
        //                    /*
        //                    RadWindow.Alert((e2.Object as ClassifierCommand).ReturnMessage);
        //                    RadWindow.Alert("Check the latest search for your results");
        //                    CslaDataProvider provider2 = App.Current.Resources["SearchesData"] as CslaDataProvider;
        //                    provider2.FactoryParameters.Clear();
        //                    provider2.FactoryMethod = "GetSearchList";
        //                    provider2.Refresh();
        //                    */
        //                }
        //            };
        //            //BusyLoading.IsRunning = true;
        //            cmdBuildModel.IsEnabled = false;
        //            cmdLearnAndApplyModel.IsEnabled = false;
        //            cmdApplyModel.IsEnabled = false;
        //            command.RevInfo = RevInfo;
        //            dp.BeginExecute(command);
        //        }
        //   // }
        //}
        #region HANDLERS
        //put each XAML-declared handler in here, make it fire the corresponding event
        //EXAMPLE:
        //private void cmdButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (cmdButton_Clicked != null) cmdCluster_Clicked.Invoke(sender, e);
        //      //the if statement makes sure the event fires only if someone is listening
        //      //this is necessary as the event will be hooked up in the parent page (the page that will Show this window)
        //      //and there is no guarantee that it will be hooked up before the XAML fires the triggering event
        //}
        #endregion

        private void cmdRefreshModelList_Click(object sender, RoutedEventArgs e)
        {
            CslaDataProvider provider2 = this.Resources["ClassifierModelListData"] as CslaDataProvider;
            if (provider2 != null)
                provider2.Refresh();
        }

        private void rbApplyToAll_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).Tag.ToString() == "ApplyToAll")
            {
                codesSelectControlClassifyTo.IsEnabled = false;
                comboSources.IsEnabled = false;
            }
            else
            if ((sender as RadioButton).Tag.ToString() == "ApplyToSelected")
            {
                codesSelectControlClassifyTo.IsEnabled = true;
                comboSources.IsEnabled = false;
            }
            else
            {
                codesSelectControlClassifyTo.IsEnabled = false;
                comboSources.IsEnabled = true;
            }
        }

        private void cmdDeleteModel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the selected model?", "Are you sure?", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                return;
            }
            CslaDataProvider provider1 = App.Current.Resources["ReviewInfoData"] as CslaDataProvider;
            ReviewInfo RevInfo = provider1.Data as ReviewInfo;
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ClassifierModelListData"]);
            if (provider != null && GridViewClassifierModels.SelectedItem != null)
            {
                DataPortal<ClassifierCommand> dp = new DataPortal<ClassifierCommand>();
                ClassifierCommand command = new ClassifierCommand(
                    "DeleteThisModel~~",
                    -1,
                    -1,
                    -1,
                    (GridViewClassifierModels.SelectedItem as ClassifierModel).ModelId,
                    -1);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    cmdBuildModel.IsEnabled = true;
                    cmdApplyModel.IsEnabled = true;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        RadWindow.Alert("Model deleted");
                    }
                    provider.Refresh();
                };
                cmdBuildModel.IsEnabled = false;
                cmdApplyModel.IsEnabled = false;
                command.RevInfo = RevInfo;
                dp.BeginExecute(command);
            }
            else
            {
                RadWindow.Alert("Please select a model to delete first");
            }

        }

        private void GridViewClassifierModels_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ClassifierModelListData"]);
            if (provider != null && GridViewClassifierModels.SelectedItem != null)
            {
                cmdDeleteModel.IsEnabled = true;
            }
            else
            {
                cmdDeleteModel.IsEnabled = false;
            }

        }

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["SourcesData"]);
            if (provider.Error != null)
            {
                System.Windows.Browser.HtmlPage.Window.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            }
        }

        private void RadWindow_Activated(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["SourcesData"]);
            if (provider != null)
            {
                provider.Refresh();
            }
            CslaDataProvider provider2 = ((CslaDataProvider)this.Resources["ClassifierModelListData"]);
            if (provider2 != null)
            {
                provider2.Refresh();
            }
            App theApp = (Application.Current as App);
            theApp.ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;

            rbApplyDAREModel.Visibility = theApp.ri.IsSiteAdmin ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            if (theApp.ri.UserId == 1451 || theApp.ri.UserId == 1576 || theApp.ri.UserId == 4688 || theApp.ri.UserId == 1095) // Alison, Ian and Dylan, Claire
            {
                rbApplyDAREModel.Visibility = Visibility.Visible;
                rbApplyDAREModel.Visibility = Visibility.Visible;
            }

        }
    }
}
