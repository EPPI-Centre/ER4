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
using Telerik.Windows.Controls;
using BusinessLibrary.BusinessClasses;
using Telerik.Windows;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class CodesTreeViewOnlyControl : UserControl
    {
        ReadOnlyReviewList roReviewsList;
        ReadOnlyArchieReviewList roArchieReviewsList;
        string context = "PublicReviews";
        private ReviewSetsList OneOrMoreReviewSet;
        public CodesTreeViewOnlyControl()
        {
            InitializeComponent();
            radTreeView.IsExpandOnSingleClickEnabled = true;
            radTreeView.IsLineEnabled = true;
            radTreeView.IsEditable = false;
            radTreeView.SelectionMode = Telerik.Windows.Controls.SelectionMode.Single;
        }
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ReviewOfOrigin.Visibility = System.Windows.Visibility.Collapsed;
            CodesetsInReview.Visibility = System.Windows.Visibility.Collapsed;
            if (this.DataContext is ReviewSetsList)
            {//not much to do...
                OneOrMoreReviewSet = this.DataContext as ReviewSetsList;
                radTreeView.ItemsSource = OneOrMoreReviewSet;
                CodesetsInReview.Visibility = System.Windows.Visibility.Visible;
                
            }
            else if (this.DataContext is ReviewSet)
            {//this is showing one single codeset to show it as a preview
                CodesetsInReview.Visibility = System.Windows.Visibility.Collapsed;
                
                ReviewSet RS = this.DataContext as ReviewSet;
                OneOrMoreReviewSet = new ReviewSetsList();
                OneOrMoreReviewSet.Add(RS);
                radTreeView.ItemsSource = OneOrMoreReviewSet;
                if (radTreeView.HasItems)
                {
                    radTreeView.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
                    radTreeView.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);
                }
                if (context == "PersonalReviews")
                {//we want to show the name of the review where this codeset is sitting
                    ReviewOfOrigin.Visibility = System.Windows.Visibility.Visible;
                    if (roReviewsList == null)
                    {//load reviewsList to get access to the names
                        CslaDataProvider provider = App.Current.Resources["ReviewsData"] as CslaDataProvider;
                        if (provider != null)
                        {
                            roReviewsList = provider.Data as ReadOnlyReviewList;
                        }
                    }
                    if (roArchieReviewsList == null && (Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity).IsCochraneUser)
                    {//load ArchieReviewsList to get access to the names
                        CslaDataProvider provider = App.Current.Resources["ArchieReviewsData"] as CslaDataProvider;
                        if (provider != null)
                        {
                            roArchieReviewsList = provider.Data as ReadOnlyArchieReviewList;
                        }
                    }
                    foreach (ReadOnlyReview ror in roReviewsList)
                    {
                        if (RS.ReviewId == ror.ReviewId)
                        {
                            reviewName.Text = "\"" + ror.ReviewName + "\"";
                            return;
                        }
                    }
                    if (roArchieReviewsList != null)
                    {
                        foreach (ReadOnlyArchieReview roAr in roArchieReviewsList)
                        {
                            if (RS.ReviewId == roAr.ReviewId)
                            {
                                reviewName.Text = "\"" + roAr.ReviewName + "\"";
                                return;
                            }
                        }
                    }

                }
            }
        }
        public void SetPublicOrPersonalReviews(bool isShowingPersonalReviews)
        {
            if (isShowingPersonalReviews)
            {
                context = "PersonalReviews";
            }
            else
            {
                context = "PublicReviews";
            }
        }
        public void Reset(bool ResetContext)
        {
            ReviewOfOrigin.Visibility = System.Windows.Visibility.Collapsed;
            CodesetsInReview.Visibility = System.Windows.Visibility.Collapsed;
            radTreeView.ItemsSource = null;
            roReviewsList = null;
            roArchieReviewsList = null;
            if (ResetContext) context = "PublicReviews";
            OneOrMoreReviewSet = null;
        }
        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            TextBoxGuidance.Text = "";
            if (radTreeView.HasItems)
            {
                RadTreeViewItem rootItem = radTreeView.ItemContainerGenerator.ContainerFromIndex(0) as RadTreeViewItem;
                // the item container maybe null if it is still not generated from the runtime  
                if (rootItem != null)
                {
                    radTreeView.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
                    rootItem.IsExpanded = true;
                }
            }
        }
        private void radTreeView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if ((radTreeView.SelectedItem != null))
            {
                if (radTreeView.SelectedItem is AttributeSet)
                {
                    TextBoxGuidance.Text = (radTreeView.SelectedItem as AttributeSet).AttributeSetDescription;
                }
                else if (radTreeView.SelectedItem is ReviewSet)
                {
                    TextBoxGuidance.Text = (radTreeView.SelectedItem as ReviewSet).SetDescription;
                }
            }
            else
            {
                TextBoxGuidance.Text = "";
            }
        }
    }
}
