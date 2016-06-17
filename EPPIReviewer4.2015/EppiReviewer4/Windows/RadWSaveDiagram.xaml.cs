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

namespace EppiReviewer4
{
    /// <summary>
    /// The process of creating a new window works as follow:
    /// 1) add new (template) in the Windows folder
    /// 2) find the original declaration in XAML and move it into the new XAML file, save the x:Name attribute for later. See xaml template for details. Also copy or move Reference entries as described in XAML.
    /// 3) in the old code-behind file, create a private instance of the new window type:
    ///     EXAMPLE: (this is a class member, don't place it in a method or porperty; change name of class and object appropriately)
    ///     private RadWSaveDiagram RadWExample = new RadWSaveDiagram(); 
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
    public partial class RadWSaveDiagram : RadWindow //change this to give it a unque name, must inherit from Radwindow.
    {
        #region EVENTS
        //put one event for each code-behind handler declared in XAML
        //EXAMPLE: 
        //public event EventHandler<RoutedEventArgs> cmdButton_Clicked;
        public event EventHandler<RoutedEventArgs> cmdSaveDiagram_Clicked;
        #endregion
        public RadWSaveDiagram()
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
        private void cmdSaveDiagram_Click(object sender, RoutedEventArgs e)
        {
            if (cmdSaveDiagram_Clicked != null) cmdSaveDiagram_Clicked.Invoke(sender, e);
        }
        #endregion
    }
}
