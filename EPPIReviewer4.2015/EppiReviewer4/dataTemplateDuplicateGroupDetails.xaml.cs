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
using Csla.Silverlight;
using BusinessLibrary.BusinessClasses;

namespace EppiReviewer4
{
    public partial class dataTemplateDuplicateGroupDetails : UserControl
    {
        public dataTemplateDuplicateGroupDetails()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(scoreTxt_Loaded);
        }

        void dataTemplateDuplicateGroupDetails_LayoutUpdated(object sender, EventArgs e)
        {
            scoreTxt_Loaded(scoreTxt, new RoutedEventArgs());
        }
        public event RoutedEventHandler changeMaster;
        public event RoutedEventHandler MarkAsDuplicate;
        public event RoutedEventHandler UnMarkAsDuplicate;
        public event RoutedEventHandler ScoreTxtLoaded;
        private void SaveRecord(ItemDuplicate record)
        {
            //record.Saved += (o, e2) =>
            //{
            //    BusyLoading.IsRunning = false;
            //    if (e2.Error != null)
            //        MessageBox.Show(e2.Error.Message);
            //};
            //record.BeginSave(true);
            //BusyLoading.IsRunning = true;
        }

        private void cmdMakeDuplicate_Click(object sender, RoutedEventArgs e)
        {
            this.MarkAsDuplicate.Invoke(sender, e);
            //ItemDuplicate record = (sender as Button).DataContext as ItemDuplicate;
            //if (record != null)
            //{
            //    record.IsChecked1 = true;
            //    record.IsChecked2 = true;
            //    record.IsDuplicate1 = true;
            //    record.IsDuplicate2 = false;
            //    SaveRecord(record);
            //}
        }

        private void cmdMakeNotDuplicate_Click(object sender, RoutedEventArgs e)
        {
            this.UnMarkAsDuplicate.Invoke(sender, e);
            //ItemDuplicate record = (sender as Button).DataContext as ItemDuplicate;
            //if (record != null)
            //{
            //    record.IsChecked1 = true;
            //    record.IsChecked2 = true;
            //    record.IsDuplicate1 = false;
            //    record.IsDuplicate2 = false;
            //    SaveRecord(record);
            //}
        }

        private void cmdMarkAsMaster_Click(object sender, RoutedEventArgs e)
        {
            this.changeMaster.Invoke(sender, e);
        }

        private void scoreTxt_Loaded(object sender, RoutedEventArgs e)
        {
            if (ScoreTxtLoaded != null) this.ScoreTxtLoaded.Invoke(scoreTxt, e);
        }
    }
}
