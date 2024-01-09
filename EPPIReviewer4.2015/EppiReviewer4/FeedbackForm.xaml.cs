using BusinessLibrary.BusinessClasses;
using BusinessLibrary.Security;
using Csla.Xaml;
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

namespace EppiReviewer4
{
    public partial class FeedbackForm : UserControl
    {
        public FeedbackForm()
        {
            InitializeComponent();
            LayoutRootFBF.Visibility = Visibility.Collapsed;
            FetchUserFeedback();
        }
        private void FetchUserFeedback()
        {
            CslaDataProvider provider = this.Resources["FeedbackAndClientErrorListData"] as CslaDataProvider;
            provider.FactoryMethod = "GetFeedbackAndClientErrorList";
            provider.DataChanged += FeedbackProvider_DataChanged;
            provider.Refresh();
        }

        private void FeedbackProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider dp = (Csla.Xaml.CslaDataProvider)sender;
            if (dp != null)
            {
                dp.DataChanged -= FeedbackProvider_DataChanged;
                if (dp.Error != null)
                {
                    Telerik.Windows.Controls.RadWindow.Alert(dp.Error.Message);
                }
                else
                {
                    LayoutRootFBF.Visibility = Visibility.Visible;
                    FeedbackAndClientErrorList data = dp.Data as FeedbackAndClientErrorList;
                    if (data != null)
                    {
                        List<FeedbackAndClientError> filtered = data.AsQueryable().Where(f => f.Context.Trim() == "ER4").ToList();
                        if (filtered.Count == 0)
                        {
                            ShowFullContent();
                        }
                        else
                        {
                            HideFullContent();
                        }
                    }
                    else
                    {
                        ShowFullContent();
                    }
                }
            }
        }

        private void Button_ClickShowFull(object sender, RoutedEventArgs e)
        {
            ShowFullContent();
        }
        private void Button_ClickHideFull(object sender, RoutedEventArgs e)
        {
            HideFullContent();
        }
        private void Button_ClickSubmit(object sender, RoutedEventArgs e)
        {
            //HideFullContent();
            if (tboxFeedback.Text.Trim().Length > 0)
            {
                ReviewerIdentity ri = Csla.ApplicationContext.User.Identity as ReviewerIdentity;
                FeedbackAndClientError fb = FeedbackAndClientError.CreateFeedbackAndClientError(ri.UserId, "ER4", false, tboxFeedback.Text);
                fb.Saved += (o, e2) =>
                {
                    if (e2.Error != null)
                    {
                        Telerik.Windows.Controls.RadWindow.Alert(e2.Error.Message);
                    }
                    this.IsEnabled = true;
                    HideFullContent();
                };
                this.IsEnabled = false;
                fb.BeginSave();
            }
        }

        private void ShowFullContent()
        {
            FbRow0.Height = new GridLength(0);
            FbRow1.Height = new GridLength(50,  GridUnitType.Auto);
            //rtb.Visibility = Visibility.Visible;
            FbRow2.Height = new GridLength(60);
            FbRow3.Height = new GridLength(30);
        }

        private void HideFullContent()
        {
            FbRow0.Height = new GridLength(30, GridUnitType.Auto);
            FbRow1.Height = new GridLength(0);
            //rtb.Visibility = Visibility.Collapsed;
            FbRow2.Height = new GridLength(0);
            FbRow3.Height = new GridLength(0);
        }
    }
}
