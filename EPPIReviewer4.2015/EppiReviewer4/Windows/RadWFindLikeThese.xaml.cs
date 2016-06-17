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
using System.ComponentModel;
using System.Collections.ObjectModel;
using BusinessLibrary.BusinessClasses;

namespace EppiReviewer4
{
    /// <summary>
    /// The process of creating a new window works as follow:
    /// 1) add new (template) in the Windows folder
    /// 2) find the original declaration in XAML and move it into the new XAML file, save the x:Name attribute for later. See xaml template for details. Also copy or move Reference entries as described in XAML.
    /// 3) in the old code-behind file, create a private instance of the new window type:
    ///     EXAMPLE: (this is a class member, don't place it in a method or porperty; change name of class and object appropriately)
    ///     private RadWFindLikeThese RadWExample = new RadWFindLikeThese(); 
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
    public partial class RadWFindLikeThese : RadWindow //change this to give it a unque name, must inherit from Radwindow.
    {
        #region EVENTS
        //put one event for each code-behind handler declared in XAML
        //EXAMPLE: 
        //public event EventHandler<RoutedEventArgs> cmdButton_Clicked;
        //
        public event EventHandler<RoutedEventArgs> RadioTermine_Clicked;
        public event EventHandler<RoutedEventArgs> cmdGetTerms_Clicked;
        public event EventHandler<RoutedEventArgs> cmdTermSearch_Clicked;
        public event EventHandler<RoutedEventArgs> cmdExportTermList_Clicked;
        public event EventHandler<RoutedEventArgs> cmdDeleteTerm_Clicked;
        public event EventHandler<SelectionChangeEventArgs> TermsGrid_SelectionChange;
        public event EventHandler<System.Windows.Controls.SelectionChangedEventArgs> TermSearchComboSearchScope_SelectionChange;
        #endregion
        public RadWFindLikeThese()
        {
            InitializeComponent();
        }

        
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
        private void RadioTermine_Click(object sender, RoutedEventArgs e)
        {
            if (RadioTermine_Clicked != null) RadioTermine_Clicked.Invoke(sender, e);
        }
        private void cmdGetTerms_Click(object sender, RoutedEventArgs e)
        {
            if (cmdGetTerms_Clicked != null) cmdGetTerms_Clicked.Invoke(sender, e);
        }
        private void cmdTermSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cmdTermSearch_Clicked != null) cmdTermSearch_Clicked.Invoke(sender, e);
        }
        private void cmdExportTermList_Click(object sender, RoutedEventArgs e)
        {
            if (cmdExportTermList_Clicked != null) cmdExportTermList_Clicked.Invoke(sender, e);
        }
        //private void cmdDeleteTerm_Click(object sender, RoutedEventArgs e)
        //{
        //    if (cmdDeleteTerm_Clicked != null) cmdDeleteTerm_Clicked.Invoke(sender, e);
        //}
        public void TermsGrid_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            if (TermsGrid_SelectionChange != null) TermsGrid_SelectionChange.Invoke(sender, e);
        }
        private void TermSearchComboSearchScope_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TermSearchComboSearchScope_SelectionChange != null) TermSearchComboSearchScope_SelectionChange.Invoke(sender, e);
        }
        #endregion
        private BackgroundWorker bw;
        
        
        private void cmdDeleteTerm_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            if (bw == null)
            {
                bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            }
            if (TermsGrid.SelectedItems != null && TermsGrid.SelectedItems.Count > 0)
            {
                if (bw.IsBusy != true)
                {
                    DeletionProgress.Visibility = System.Windows.Visibility.Visible;
                    DeletionProgressTxt.Text = "0";
                    TermList source = TermsGrid.ItemsSource as TermList;
                    
                    ObservableCollection<object> selection = TermsGrid.SelectedItems as ObservableCollection<object>;
                    object[] arr;
                    arr = new object[selection.Count];
                    selection.CopyTo(arr, 0);
                    TermsGrid.SelectionChanged -= TermsGrid_SelectionChanged;
                    TermsGrid.ItemsSource = null;
                    //(TermsGrid.ItemsSource as TermList).Clear();
                    bw.RunWorkerAsync(new KeyValuePair<object[], TermList>(arr, source));
                }
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            KeyValuePair<object[], TermList> kvp = (KeyValuePair<object[], TermList>)e.Argument;
            object[] arr = kvp.Key;
            TermList source = kvp.Value;
            TermList result = source.Clone();
            if (arr == null || source == null) return;
            int i = 0;
            while (i < arr.Length)
            {
                worker.ReportProgress((i * 100 / arr.Length));
                try
                {
                    result.Remove(arr[i] as Term);
                }
                catch (Exception eee)
                {
                    e.Cancel = true;
                }
                //selection.RemoveAt(i - 1);
                i++;
            }
            e.Result = result;
            //for (int i = 1; (i <= 10); i++)
            //{
            //    if ((worker.CancellationPending == true))
            //    {
            //        e.Cancel = true;
            //        break;
            //    }
            //    else
            //    {
            //        // Perform a time consuming operation and report progress.
            //        System.Threading.Thread.Sleep(500);
            //        worker.ReportProgress((i * 10));
            //    }
            //}
        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TermList source = e.Result as TermList;
            DeletionProgress.Visibility = System.Windows.Visibility.Collapsed;
            TermsGrid.SelectionChanged += new EventHandler<SelectionChangeEventArgs>(TermsGrid_SelectionChanged);
            //TermsGrid.Visibility = System.Windows.Visibility.Visible;
            TermsGrid.ItemsSource = source;
            //(windowFindLikeThese.TermsGrid.ItemsSource as TermList).Remove(windowFindLikeThese.TermsGrid.SelectedItem as Term);
            if (source.Count < 1)
            {
                cmdTermSearch.IsEnabled = false;
            }
            this.IsEnabled = true;
            
            //if ((e.Cancelled == true))
            //{
            //    this.tbProgress.Text = "Canceled!";
            //}

            //else if (!(e.Error == null))
            //{
            //    this.tbProgress.Text = ("Error: " + e.Error.Message);
            //}

            //else
            //{
            //    this.tbProgress.Text = "Done!";
            //}
        }
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DeletionProgressTxt.Text = (e.ProgressPercentage.ToString() + "%");
        }

        
    }
}
