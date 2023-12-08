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

using Telerik.Windows.Controls;

using BusinessLibrary.BusinessClasses;
using Csla;

namespace EppiReviewer4
{
    /// <summary>
    /// Interaction logic for RadWbyS.xaml
    /// </summary>
    public partial class RadWDocumentCluster : RadWindow
    {
        public event EventHandler<System.Windows.Controls.SelectionChangedEventArgs> ClusterWhat_SelectionChanged;
        public event EventHandler<RoutedEventArgs> cmdCluster_Clicked;
        public event EventHandler<RoutedEventArgs> cmdGetMicrosoftAcademicTopics_Clicked;
        public event EventHandler<RoutedEventArgs> cmdGetOpenAlexTopicsNLP_Clicked;

        public RadWDocumentCluster()
        {
            InitializeComponent();
        }
        
        private void ComboClusterWhat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ClusterWhat_SelectionChanged != null) ClusterWhat_SelectionChanged.Invoke(sender, e);
        }
        private void cmdCluster_Click(object sender, RoutedEventArgs e)
        {
            if (cmdCluster_Clicked != null) cmdCluster_Clicked.Invoke(sender, e);
        }

        private void cmdGetMicrosoftAcademicTopics_Click(object sender, RoutedEventArgs e)
        {
            if (cmdGetMicrosoftAcademicTopics_Clicked != null) cmdGetMicrosoftAcademicTopics_Clicked.Invoke(sender, e);
        }

        private void rbClusterExistingCodeSet_Checked(object sender, RoutedEventArgs e)
        {
            dialogClusterComboSelectCodeSet.IsEnabled = true;
        }

        private void rbClusterNewCodeSet_Checked(object sender, RoutedEventArgs e)
        {
            if (dialogClusterComboSelectCodeSet != null)
                    dialogClusterComboSelectCodeSet.IsEnabled = false;
        }

        private void dialogClusterMinItems_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e)
        {
            if (dialogClusterMinItems != null && dialogClusterThresholdWarning != null && dialogClusterMinItems.Value.Value < 5)
            {
                dialogClusterThresholdWarning.Visibility = Visibility.Visible;
            }
            else
            {
                if (dialogClusterThresholdWarning != null)
                    dialogClusterThresholdWarning.Visibility = Visibility.Collapsed;
            }
        }

        private void cmdGetOpenAlexTopicsNLP_Click(object sender, RoutedEventArgs e)
        {
            RadWindow.Confirm("Are you sure you want to assign OpenAlex topics automatically?", this.doGetOpenAlexTopicsNLP);
        }

        private void doGetOpenAlexTopicsNLP(object sender, WindowClosedEventArgs e)
        {
            var result = e.DialogResult;
            if (result == true)
            {
                if (cmdGetOpenAlexTopicsNLP_Clicked != null) cmdGetOpenAlexTopicsNLP_Clicked.Invoke(sender, new RoutedEventArgs());
            }
        }

        private void rbClusterEmbeddingsNewCodeSet_Checked(object sender, RoutedEventArgs e)
        {
            if (codesSelectControlClusterEmbeddingsSelect != null)
            {
                codesSelectControlClusterEmbeddingsSelect.IsEnabled = false;
            }
        }

        private void rbClusterEmbeddingsExistingCodeSet_Checked(object sender, RoutedEventArgs e)
        {
            if (codesSelectControlClusterEmbeddingsSelect != null)
            {
                codesSelectControlClusterEmbeddingsSelect.IsEnabled = true;
            }
        }

        private void rbClusterEmbeddingsUseAbstract_Checked(object sender, RoutedEventArgs e)
        {
            if (codesSelectControlClusterEmbeddingsThisCodeSelect != null)
            {
                codesSelectControlClusterEmbeddingsThisCodeSelect.IsEnabled = false;
            }
        }

        private void rbClusterEmbeddingsUseThisCode_Checked(object sender, RoutedEventArgs e)
        {
            if (codesSelectControlClusterEmbeddingsThisCodeSelect != null)
            {
                codesSelectControlClusterEmbeddingsThisCodeSelect.IsEnabled = true;
            }
        }

        private void cmdClusterEmbeddings_Click(object sender, RoutedEventArgs e)
        {
            RadWindow.Confirm("Are you sure you want to cluster using embeddings?", this.doClusterEmbeddings);
        }

        private void doClusterEmbeddings(object sender, WindowClosedEventArgs e)
        {
            RadWindow.Alert("at least herer");
            var result = e.DialogResult;
            if (result == true)
            {
                RadWindow.Alert("true");
                Int64 AttributeId = -1;
                if (codesSelectControlClusterSelect.selectedAttributes.Count > 0)
                {
                    AttributeId = codesSelectControlClusterSelect.SelectedAttributeSet().AttributeId;
                    RadWindow.Alert("in here");
                }
                RadWindow.Alert("outagain");
                DataPortal<ClassifierTopicModelCommand> dp = new DataPortal<ClassifierTopicModelCommand>();
                ClassifierTopicModelCommand command = new ClassifierTopicModelCommand(
                    "Topic models",
                    -1,
                    -1,
                    AttributeId,
                    -1,
                    -2);
                dp.ExecuteCompleted += (o, e2) =>
                {
                    //BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        RadWindow.Alert(e2.Error.Message);
                    }
                    else
                    {
                        ClassifierTopicModelCommand rh2 = e2.Object as ClassifierTopicModelCommand;
                        RadWindow.Alert(rh2.ReturnMessage);

                        //RadWindow.Alert("Your data have been uploaded for topic modelling successfully." + Environment.NewLine +
                         //   "As this can take some time," + Environment.NewLine +
                         //   "please refresh the list of search results periodially.");
                        // no point in refreshing the search, as the command returns before it's been updated
                        //RadWindow.Alert((e2.Object as ClassifierCommand).ReturnMessage);
                        //RadWindow.Alert("Check the latest search for your results");
                        //CslaDataProvider provider2 = App.Current.Resources["SearchesData"] as CslaDataProvider;
                        //provider2.FactoryParameters.Clear();
                        //provider2.FactoryMethod = "GetSearchList";
                        //provider2.Refresh();
                    }
                };
                RadWindow.Alert("executing");
                dp.BeginExecute(command);
            }
        }
    }
}

