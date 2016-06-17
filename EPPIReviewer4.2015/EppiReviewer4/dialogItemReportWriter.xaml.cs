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
using Csla;
using Csla.DataPortalClient;
using BusinessLibrary.BusinessClasses;
using Csla.Silverlight;
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class dialogItemReportWriter : UserControl
    {
        public dialogItemReportWriter()
        {
            InitializeComponent();
        }

        public System.Collections.ObjectModel.ObservableCollection<object> SelectedItems;
        private int currentIndex;
        private int itemCount;
        private string report;
        public event EventHandler<LaunchReportViewerEventArgs> LaunchReportViewer;

        private void cmdRunItemReportWriter_Click(object sender, RoutedEventArgs e)
        {
            if (GridSelectCodeSets.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one code set");
                return;
            }

            currentIndex = -1;
            report = "";
            itemCount = SelectedItems.Count;
            GetNext();
            cmdRunItemReportWriter.IsEnabled = false;
        }

        public void SetupItemReportWriter(System.Collections.ObjectModel.ObservableCollection<object> items)
        {
            SelectedItems = items;
            TextBlockCurrentStatus.Text = "";
        }


        private void GetNext()
        {
            currentIndex++;
            if (currentIndex < SelectedItems.Count)
            {
                DataPortal<ItemSetList> dp = new DataPortal<ItemSetList>();
                dp.FetchCompleted += (o, e2) =>
                {
                    if (e2.Object != null)
                    {
                        AddNewItem(e2.Object);
                    }
                };
                BusyLoadingItemReportWriter.IsRunning = true;
                TextBlockCurrentStatus.Text = "Retrieving " + Convert.ToString(currentIndex + 1) + " / " + itemCount.ToString();
                dp.BeginFetch(new SingleCriteria<ItemSetList, Int64>(-(SelectedItems[currentIndex] as Item).ItemId));
            }
            else
            {
                if (LaunchReportViewer != null)
                {
                    BusyLoadingItemReportWriter.IsRunning = false;
                    cmdRunItemReportWriter.IsEnabled = true;
                    LaunchReportViewerEventArgs lrvea = new LaunchReportViewerEventArgs("<html><body>" + report + "</body></html>");
                    LaunchReportViewer.Invoke(this, lrvea);
                }
            }
        }

        private void AddNewItem(ItemSetList dataList)
        {
            if (report != "")
            {
                report += "<br /><br />";
            }
            Item currentItem = SelectedItems[currentIndex] as Item;

            report += "<h1>ID " + currentItem.ItemId.ToString() + ": " + currentItem.ShortTitle + "</h1><br />" +
                currentItem.GetCitation() + "<br /><br />";
            if (currentItem.OldItemId != "")
            {
                report += "<b>Your ID:</b> " + currentItem.OldItemId + "<br /><br />";
            }
            if (currentItem.Abstract != "")
            {
                report += "<b>Abstract:</b> " + currentItem.Abstract + "<br /><br />";
            }

            for (int i = 0; i < GridSelectCodeSets.SelectedItems.Count; i++)
            {
                foreach (ItemSet itemSet in dataList)
                {
                    if (itemSet.SetId == (GridSelectCodeSets.SelectedItems[i] as ReviewSet).SetId && itemSet.IsCompleted == true)
                    {
                        report += "<br /><h2>Reviewer: " + itemSet.ContactName + "</h2>" + "<h3>Date: " + DateTime.Now.ToShortDateString() + "</h3>";
                        ReviewSetsList rsl = (App.Current.Resources["CodeSetsData"] as CslaDataProvider).Data as ReviewSetsList;

                        ReviewSet reviewSet = rsl.GetReviewSet(itemSet.SetId);
                        if (reviewSet != null)
                        {
                            report += "<p><h1>" + reviewSet.SetName + "</h1></p><p><ul>";
                            foreach (AttributeSet attributeSet in reviewSet.Attributes)
                            {
                                report += writeCodingReportAttributes(itemSet, attributeSet, "");
                            }
                            report += "</ul></p>";
                            report += "<p>" + itemSet.OutcomeItemList.OutcomesTable() + "</p>";
                        }
                    }
                }
            }
            GetNext();
        }


        public static string writeCodingReportAttributes(ItemSet itemSet, AttributeSet attributeSet, string report)
        {
            //if (attributeSet.AttributeTypeId > 1)
            //{
                ReadOnlyItemAttribute roia = itemSet.GetItemAttribute(attributeSet.AttributeId);
                if (roia != null)
                {
                    report += "<li>" + attributeSet.AttributeName + "<br /><i>" + roia.AdditionalText.Replace("\n", "<br />") + "</i>";
                    if (roia.ItemAttributeFullTextList != null && roia.ItemAttributeFullTextList.Count > 0)
                    {
                        List<ItemAttributeFullTextDetails> ll = roia.ItemAttributeFullTextList.ToList();
                        ll.Sort();
                        report += dialogCoding.addFullTextToComparisonReport(ll);
                    }
                    report += "</li>";
                    report += "<ul>";
                    foreach (AttributeSet child in attributeSet.Attributes)
                    {
                        report = writeCodingReportAttributes(itemSet, child, report);
                    }
                    report += "</ul>";
                }
                else
                {
                    if (CodingReportCheckChildSelected(itemSet, attributeSet) == true) // ie an attribute below this is selected, even though this one isn't
                    {
                        report += "<li style='color:DarkGray;'>" + attributeSet.AttributeName + "</li>";
                        report += "<ul>";
                        foreach (AttributeSet child in attributeSet.Attributes)
                        {
                            report = writeCodingReportAttributes(itemSet, child, report);
                        }
                        report += "</ul>";
                    }
                }
            //}
            //else
            //{
            //    report += "<li>" + attributeSet.AttributeName + "</li>";
            //    report += "<ul>";
            //    foreach (AttributeSet child in attributeSet.Attributes)
            //    {
            //        report = writeCodingReportAttributes(itemSet, child, report);
            //    }
            //    report += "</ul>";
            //}
            return report;
        }

        private static bool CodingReportCheckChildSelected(ItemSet itemSet, AttributeSet attributeSet)
        {
            if (itemSet != null)
            {
                if (itemSet.GetItemAttribute(attributeSet.AttributeId) != null)
                {
                    return true;
                }
                foreach (AttributeSet child in attributeSet.Attributes)
                {
                    if (CodingReportCheckChildSelected(itemSet, child) == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


    }
}
