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
using Telerik.Windows.Controls;
using System.IO;

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
        SaveFileDialog dialog = new SaveFileDialog();


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
            if (chkbxExportJSON.IsChecked == true)
            {//need to put this one here because file dialog needs to be opened directly as a result of a user click, can't have async stuff between clicking and opening the dialog
                string extension = "json";
                dialog = new SaveFileDialog();
                dialog.DefaultExt = extension;
                dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, "json");
                dialog.FilterIndex = 1;
                if (dialog.ShowDialog() != true)
                {//do nothing, user cancelled or didn't pick a filename
                    return;
                }
            }
            cmdRunItemReportWriter.IsEnabled = false;
            GetNext();
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
            {//haven't finished!
                if (chkbxExportJSON.IsChecked == true && currentIndex > 0 && currentIndex < SelectedItems.Count)
                {//doing JSON and in the middle of the list of items, injecting separators in array of items
                    report += "," + Environment.NewLine;
                }
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
                        // From here down - finish the report off by closing relevant tags etc
            {   
                BusyLoadingItemReportWriter.IsRunning = false;
                cmdRunItemReportWriter.IsEnabled = true;
                if (LaunchReportViewer != null && (chkbxExportJSON.IsChecked == false))
                {
                    LaunchReportViewerEventArgs lrvea = new LaunchReportViewerEventArgs("<html><body>" + report + "</body></html>");
                    LaunchReportViewer.Invoke(this, lrvea);
                }
                else if (chkbxExportJSON.IsChecked == true)
                {
                    //finish-off the manual parts of the JSON syntax (close array and file)
                    report += Environment.NewLine + "]" + Environment.NewLine + "}";
                    using (Stream stream = dialog.OpenFile())
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(report);
                        }
                    }
                }
            }
        }

        private void AddNewItem(ItemSetList dataList)
        {
            Item currentItem = SelectedItems[currentIndex] as Item;
            if (chkbxExportJSON.IsChecked == false)
            {
                if (report != "")
                {
                    report += "<br /><br />";
                }
                

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
            }
            else
            {//JSON!
                if (report == "")
                {
                    //we avoid importing JSON libraries in the SL project itself, so we do a little hand-editing to start with
                    report += "{" + Environment.NewLine + "\"CodeSets\": [" + Environment.NewLine;
                    for (int i = 0; i < GridSelectCodeSets.SelectedItems.Count; i++)
                    {
                        report += (GridSelectCodeSets.SelectedItems[i] as ReviewSet).ToJSON();
                        if (i < GridSelectCodeSets.SelectedItems.Count - 1)
                        {//this is not the last codeset, add separator
                            report += ", " + Environment.NewLine;
                        }
                    }
                    //close the array of codesets
                    report += Environment.NewLine + "], " + Environment.NewLine;//we assume we have items to add!
                    report += "\"References\": [" + Environment.NewLine;
                }
            }
            if (chkbxExportJSON.IsChecked == false)
            {
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
                                    report += writeCodingReportAttributesWithArms(itemSet, attributeSet, "");
                                }
                                report += "</ul></p>";
                                report += "<p>" + itemSet.OutcomeItemList.OutcomesTable() + "</p>";
                            }

                        }
                    }
                }
            }
            else
            {//we are producing JSON!
                List<ItemSet> relevantCodes = new List<ItemSet>();
                for (int i = 0; i < GridSelectCodeSets.SelectedItems.Count; i++)
                {
                    foreach (ItemSet itemSet in dataList)
                    {
                        if (itemSet.SetId == (GridSelectCodeSets.SelectedItems[i] as ReviewSet).SetId && itemSet.IsCompleted == true)
                        {
                            relevantCodes.Add(itemSet);
                        }
                    }
                }
                report += currentItem.ToJSON(relevantCodes);//we add separators in GetNext
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
                    string AttributeName = attributeSet.AttributeName;
                    if (roia.ArmId != 0)
                    {
                        AttributeName += " [" + roia.ArmTitle + "]";
                    }

                    report += "<li>" + AttributeName + "<br /><i>" + roia.AdditionalText.Replace("\n", "<br />") + "</i>";
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

        public static string writeCodingReportAttributesWithArms(ItemSet itemSet, AttributeSet attributeSet, string report)
        {
            List<ReadOnlyItemAttribute> roias = itemSet.GetItemAttributes(attributeSet.AttributeId);
            if (roias != null && roias.Count > 0)
            {
                foreach (ReadOnlyItemAttribute roia in roias)
                {
                    string AttributeName = attributeSet.AttributeName;
                    if (roia.ArmId != 0)
                    {
                        AttributeName += " [" + roia.ArmTitle + "]";
                    }

                    report += "<li>" + AttributeName + "<br /><i>" + roia.AdditionalText.Replace("\n", "<br />") + "</i>";
                    if (roia.ItemAttributeFullTextList != null && roia.ItemAttributeFullTextList.Count > 0)
                    {
                        List<ItemAttributeFullTextDetails> ll = roia.ItemAttributeFullTextList.ToList();
                        ll.Sort();
                        report += dialogCoding.addFullTextToComparisonReport(ll);
                    }
                    report += "</li>";
                }
                if (CodingReportCheckChildSelected(itemSet, attributeSet) == true) // ie an attribute below this is selected, even though this one isn't
                {
                    report += "<ul>";
                    foreach (AttributeSet child in attributeSet.Attributes)
                    {
                        report = writeCodingReportAttributesWithArms(itemSet, child, report);
                    }
                    report += "</ul>";
                }
            }
            else
            {
                if (CodingReportCheckChildSelected(itemSet, attributeSet) == true) // ie an attribute below this is selected, even though this one isn't
                {
                    report += "<li style='color:DarkGray;'>" + attributeSet.AttributeName + "</li>";
                    report += "<ul>";
                    foreach (AttributeSet child in attributeSet.Attributes)
                    {
                        report = writeCodingReportAttributesWithArms(itemSet, child, report);
                    }
                    report += "</ul>";
                }
            }
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
