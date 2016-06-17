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
using System.ComponentModel;

namespace EppiReviewer4
{
    public partial class ReviewerTerms : UserControl, INotifyPropertyChanged
    {
        public ReviewerTerms()
        {
            InitializeComponent();
            //two lines to make the read-only UI work
            //add <Button x:Name="isEn" IsEnabled="{Binding HasWriteRights, Mode=OneWay}" ></Button>
            //to the xaml resources section!!
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            //end of read-only ui hack
        }
        //first bunch of lines to make the read-only UI work
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private bool CodingOnlyMode = false;
        public void PrepareCodingOnly()
        {
            CodingOnlyMode = true;
            NotifyPropertyChanged("HasWriteRights");//coding only changes the value of haswriterights
        }
        private BusinessLibrary.Security.ReviewerIdentity ri;
        public bool HasWriteRights
        {
            get
            {
                if (ri == null) return false;
                else
                {
                    return ri.HasWriteRights() && !CodingOnlyMode;//false if user does not have write rights or if control is set for coding only mode.
                }
            }
        }
        //end of read-only ui hack

        public event RoutedEventHandler TermsChanged;
        private void cmdDeleteTrainingReviewerTerm_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this term?", "Confirm delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                TrainingReviewerTerm trt;
                trt = (sender as Button).DataContext as TrainingReviewerTerm;
                trt.BeginEdit();
                trt.Delete();
                trt.ApplyEdit();
                trt = null;
                if (TermsChanged != null)
                {
                    TermsChanged.Invoke(sender, e);
                }
            }
        }


        private void GridViewTrainingReviewerTerms_AddingNewDataItem(object sender, Telerik.Windows.Controls.GridView.GridViewAddingNewEventArgs e)
        {
            if (TermsChanged != null)
            {
                TermsChanged.Invoke(sender, new RoutedEventArgs());
            }
        }

        private void GridViewTrainingReviewerTerms_CellEditEnded(object sender, Telerik.Windows.Controls.GridViewCellEditEndedEventArgs e)
        {
            if (TermsChanged != null)
            {
                TermsChanged.Invoke(sender, e);
            }
        }

        private void GridViewTrainingReviewerTerms_RowEditEnded(object sender, Telerik.Windows.Controls.GridViewRowEditEndedEventArgs e)
        {
            if (TermsChanged != null)
            {
                TermsChanged.Invoke(sender, e);
            }
        }
    }
}
