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
using Telerik.Windows.Controls;
using Csla.Xaml;
using Telerik.Windows.DragDrop;
using Telerik.Windows.Controls.TreeView;

namespace EppiReviewer4
{
    public partial class homeReports : UserControl
    {
        public BusinessLibrary.Security.ReviewerIdentity ri;
        public Report activeReport;
        public bool ReportIsEdited = false;
        public bool InitialisingControl = true;
        public event EventHandler ReportTypeChanged;
        public event EventHandler OpenReportWindowCommand;
        private RadWLoadReport windowLoadReport = new RadWLoadReport();
        private int AsynchSaveCheck = 0;
        public homeReports()
        {
            InitializeComponent();
            ri = Csla.ApplicationContext.User.Identity as BusinessLibrary.Security.ReviewerIdentity;
            isEn.DataContext = this;
            MakeNewReport();
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["ReportListData"]);
            if (provider != null)
            {
                provider.DataChanged += CslaDataProvider_DataChanged;
                provider.Refresh();
            }
            windowLoadReport.RestrictedAreaMargin = new Thickness(20);
            windowLoadReport.IsRestricted = true;
            //windowLoadReport.ResizeMode = ResizeMode.CanResize;
            
            windowLoadReport.cmdDelete_Clicked +=new EventHandler<RoutedEventArgs>(cmdDelete_Click);
            windowLoadReport.cmdGo_Clicked +=new EventHandler<RoutedEventArgs>(cmdGo_Click);
            TileViewActiveReport.SizeChanged += new SizeChangedEventHandler(val_SizeChanged);
            
        }

        

        public bool HasWriteRights
        {
            get
            {
                if (ri == null) return false;
                else return ri.HasWriteRights();
            }
        }

        public void MakeNewReport()
        {
            if (HasWriteRights)
            {
                activeReport = new Report();
                activeReport.Name = "Report title";
                activeReport.Columns = new ReportColumnList();
                activeReport.ContactName = ri.Name;
                BindActiveReport();
            }
            else
            {
                activeReport = new Report();
                activeReport.Columns = new ReportColumnList();
                BindActiveReport();
            }
            if (InitialisingControl != true)
                ReportIsEdited = true;
        }

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["ReportListData"]);
            if (provider.Error != null)
            {
                RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            }
            else
            {
                if ((activeReport != null) && (activeReport.ReportId == 0)) // this ensures we get the most recently created report as the live one for editing
                {
                    int maxId = 0;
                    foreach (Report rep in provider.Data as ReportList)
                    {
                        if (rep.ReportId > maxId)
                        {
                            activeReport = rep;
                            maxId = activeReport.ReportId;
                        }
                    }
                    BindActiveReport();
                    InitialisingControl = false;
                    ReportIsEdited = false;
                }
            }
        }

        private void BindActiveReport()
        {
            CslaDataProvider provider = App.Current.Resources["ReportListData"] as CslaDataProvider;
            if (provider != null)
            {
                ReportList reportList = provider.Data as ReportList;
                if (reportList != null && !reportList.Contains(activeReport))
                {
                    reportList.Add(activeReport);
                }
            }
            
            TileViewActiveReport.ItemsSource = activeReport.Columns;
            TextBlockReportTitle.DataContext = activeReport;
            
            switch (activeReport.ReportType)
            {
                case "Question": ComboReportType.SelectedIndex = 0;
                    break;
                case "Answer": ComboReportType.SelectedIndex = 1;
                    break;
            }
            val_SizeChanged(TileViewActiveReport, null);
        }

        private void cmdGo_Click(object sender, RoutedEventArgs e)
        {
            activeReport = (sender as Button).DataContext as Report;
            BindActiveReport();
            windowLoadReport.Close();
        }

        private void TileViewActiveReport_TileStateChanged(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            RadTileViewItem item = e.Source as RadTileViewItem;
            
            if (item != null)
            {
                ReportColumn data = item.DataContext as ReportColumn;
                int pos = data.ColumnOrder;
                RadFluidContentControl fluidControl = item.ChildrenOfType<RadFluidContentControl>().First();
                if (fluidControl != null)
                {
                    //TileViewActiveReport.TilesStateChanged -= TileViewActiveReport_TilesStateChanged;
                    switch (item.TileState)
                    {
                        case TileViewItemState.Maximized:
                            fluidControl.State = FluidContentControlState.Large;
                            data.BufferPosition = pos;
                            //TileViewActiveReport.TilesStateChanged +=new EventHandler<Telerik.Windows.RadRoutedEventArgs>(TileViewActiveReport_TilesStateChanged);
                            break;
                        case TileViewItemState.Minimized:
                            fluidControl.State = FluidContentControlState.Small;
                            data.BufferPosition = pos;
                            //TileViewActiveReport.TilesStateChanged += new EventHandler<Telerik.Windows.RadRoutedEventArgs>(TileViewActiveReport_TilesStateChanged);
                            break;
                        case TileViewItemState.Restored:
                            TileViewActiveReport.TilesStateChanged -= TileViewActiveReport_TilesStateChanged;
                            fluidControl.State = FluidContentControlState.Normal;
                            TileViewActiveReport.TilesStateChanged += new EventHandler<Telerik.Windows.RadRoutedEventArgs>(TileViewActiveReport_TilesStateChanged);
                            break;
                    }
                }
            }
        }
        private void TileViewActiveReport_TilesStateChanged(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            TileViewActiveReport.TilesStateChanged -= TileViewActiveReport_TilesStateChanged;
            ReportColumnList rep = TileViewActiveReport.ItemsSource as ReportColumnList;
            if (rep == null) return;
            foreach (ReportColumn data in rep)
            {
                data.ColumnOrder = data.BufferPosition;
                data.BufferPosition = data.ColumnOrder;
            }
            AsynchSaveCheck = 10001;
        }
        public void BindTreeViewSelectedItem(Object AttributeOrReviewSet)
        {
            /*
            if (AttributeOrReviewSet is AttributeSet)
            {
                TextBlockAddCodeToReport.DataContext = AttributeOrReviewSet;
                TextBlockAddCodeToReport.Text = (AttributeOrReviewSet as AttributeSet).AttributeName;
            }
            else
            {
                if (AttributeOrReviewSet is ReviewSet)
                {
                    TextBlockAddCodeToReport.DataContext = AttributeOrReviewSet;
                    TextBlockAddCodeToReport.Text = (AttributeOrReviewSet as ReviewSet).SetName;
                }
                else
                {
                    TextBlockAddCodeToReport.DataContext = new AttributeSet();
                    TextBlockAddCodeToReport.Text = "";
                }
            }
            */
        }

        public void cmdAddCodeToReport_Click(object sender, RoutedEventArgs e)
        {/*
            AttributeSet attributeSet = TextBlockAddCodeToReport.DataContext as AttributeSet;
            if ((attributeSet != null) && (attributeSet.Attributes != null))
            {
                if (activeReport.ReportType != "Multiple" || (activeReport.ReportType == "Multiple" && attributeSet.Attributes.Count > 0))
                {
                    ReportColumn rc = new ReportColumn();
                    rc.Codes = new ReportColumnCodeList();
                    ReportColumnCode rcc = new ReportColumnCode();
                    rcc.AttributeId = attributeSet.AttributeId;
                    rcc.CodeOrder = 0;
                    rcc.DisplayAdditionalText = true;
                    rcc.DisplayCode = true;
                    rcc.DisplayCodedText = true;
                    rcc.ParentAttributeId = attributeSet.AttributeId;
                    rcc.ParentAttributeText = attributeSet.AttributeName;
                    rcc.ReportColumnId = 0;
                    rcc.SetId = attributeSet.SetId;
                    rcc.UserDefText = attributeSet.AttributeName;
                    rc.Name = rcc.UserDefText;
                    rc.Codes.Add(rcc);
                    activeReport.Columns.Add(rc);
                    ReportIsEdited = true;
                }
                else
                {
                    MessageBox.Show("Please select a code with codes below it in the tree");
                }
            }
            else
            {
                if (activeReport.ReportType == "Multiple")
                {
                    ReviewSet rs = TextBlockAddCodeToReport.DataContext as ReviewSet;
                    if (rs != null && rs.Attributes != null)
                    {
                        if (rs.Attributes.Count > 0)
                        {
                            ReportColumn rc = new ReportColumn();
                            rc.Codes = new ReportColumnCodeList();
                            ReportColumnCode rcc = new ReportColumnCode();
                            rcc.AttributeId = 0;
                            rcc.CodeOrder = 0;
                            rcc.DisplayAdditionalText = true;
                            rcc.DisplayCode = true;
                            rcc.DisplayCodedText = true;
                            rcc.ParentAttributeId = 0;
                            rcc.ParentAttributeText = rs.SetName;
                            rcc.ReportColumnId = 0;
                            rcc.SetId = rs.SetId;
                            rcc.UserDefText = rs.SetName;
                            rc.Name = rcc.UserDefText;
                            rc.Codes.Add(rcc);
                            activeReport.Columns.Add(rc);
                            ReportIsEdited = true;
                        }
                        else
                        {
                            MessageBox.Show("Please select a code set with codes below it in the tree");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Code sets are only allowable in 'Multiple' reports.");
                }
            }
          */
        }

        private void cmdDeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            ReportColumn rc = (sender as Button).DataContext as ReportColumn;
            if (rc != null)
            {
                rc.RemoveSelf(activeReport.Columns);
                ReportIsEdited = true;
            }
            val_SizeChanged(TileViewActiveReport, null);
        }

        private void RadTreeView_PreviewDragEnded(object sender, Telerik.Windows.DragDrop.DragDropCompletedEventArgs e)
        {
            e.Handled = true;
            TreeViewDragDropOptions options = DragDropPayloadManager.GetDataFromObject(e.Data, TreeViewDragDropOptions.Key) as TreeViewDragDropOptions;
            if (options == null) return;
            foreach (object o in options.DraggedItems)
            {
                ReportColumnCode rcc = o as ReportColumnCode;
                if (rcc != null)
                {
                    rcc.SetCodeAsNew(); // need to pretend it's 'new', so that the lists don't panic and think they have to go back to the dataportal when removing a node
                    e.Handled = false;
                }
                else
                {
                    AttributeSet attributeSet = o as AttributeSet;
                    if ((attributeSet != null) &&
                        (activeReport.ReportType != "Question" || (activeReport.ReportType == "Question" && attributeSet.Attributes != null && attributeSet.Attributes.Count > 0)))
                    {
                        ReportColumnCode rccNew = new ReportColumnCode();
                        rccNew.AttributeId = attributeSet.AttributeId;
                        rccNew.CodeOrder = 0;
                        rccNew.DisplayAdditionalText = true;
                        rccNew.DisplayCode = true;
                        rccNew.DisplayCodedText = true;
                        rccNew.ParentAttributeId = attributeSet.AttributeId;
                        rccNew.ParentAttributeText = attributeSet.AttributeName;
                        rccNew.ReportColumnId = 0;
                        rccNew.SetId = attributeSet.SetId;
                        rccNew.UserDefText = attributeSet.AttributeName;

                        if (options.DropTargetItem != null)
                        {
                            int index = 0;
                            switch (options.DropPosition)
                            {
                                case DropPosition.After:
                                    index = options.DropTargetItem.Index + 1;
                                    break;
                                case DropPosition.Before:
                                    index = options.DropTargetItem.Index;
                                    break;
                                case DropPosition.Inside:
                                    MessageBox.Show("Inside?");
                                    break;
                                default: break;
                            }
                            if (activeReport.ReportType == "Question")
                            {
                                ((sender as Telerik.Windows.Controls.RadTreeView).ItemsSource as ReportColumnCodeList).Insert(index, rccNew);
                                ReportIsEdited = true;
                            }
                            else
                            {
                                MessageBox.Show("Only 'question' reports can have more than one code per column.");
                            }
                        }
                        else // empty treeview so allowable for both types of report
                        {
                            ((sender as Telerik.Windows.Controls.RadTreeView).ItemsSource as ReportColumnCodeList).Add(rccNew);
                            ReportIsEdited = true;
                        }
                    }
                    else
                    {
                        if (activeReport.ReportType == "Question")
                        {
                            ReviewSet rs = o as ReviewSet;
                            if ((rs != null) && (rs.Attributes != null) && (rs.Attributes.Count > 0))
                            {
                                ReportColumnCode rccNew = new ReportColumnCode();
                                rccNew.AttributeId = 0;
                                rccNew.CodeOrder = 0;
                                rccNew.DisplayAdditionalText = true;
                                rccNew.DisplayCode = true;
                                rccNew.DisplayCodedText = true;
                                rccNew.ParentAttributeId = 0;
                                rccNew.ParentAttributeText = rs.SetName;
                                rccNew.ReportColumnId = 0;
                                rccNew.SetId = rs.SetId;
                                rccNew.UserDefText = rs.SetName;

                                if (options.DropTargetItem != null)
                                {
                                    int index = 0;
                                    switch (options.DropPosition)
                                    {
                                        case DropPosition.After:
                                            index = options.DropTargetItem.Index + 1;
                                            break;
                                        case DropPosition.Before:
                                            index = options.DropTargetItem.Index;
                                            break;
                                        case DropPosition.Inside:
                                            MessageBox.Show("Inside?");
                                            break;
                                        default: break;
                                    }
                                    ((sender as Telerik.Windows.Controls.RadTreeView).ItemsSource as ReportColumnCodeList).Insert(index, rccNew);
                                    ReportIsEdited = true;
                                }
                                else // empty treeview
                                {
                                    ((sender as Telerik.Windows.Controls.RadTreeView).ItemsSource as ReportColumnCodeList).Add(rccNew);
                                    ReportIsEdited = true;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Code sets can only be used in 'Question' reports.");
                        }
                    }
                }
            }
        }

        private void RadTreeView_PreviewDragEnded(object sender, RadTreeViewDragEndedEventArgs e)
        {
            e.Handled = true;

            foreach (object o in e.DraggedItems)
            {
                ReportColumnCode rcc = o as ReportColumnCode;
                if (rcc != null)
                {
                    rcc.SetCodeAsNew(); // need to pretend it's 'new', so that the lists don't panic and think they have to go back to the dataportal when removing a node
                    e.Handled = false;
                }
                else
                {
                    AttributeSet attributeSet = o as AttributeSet;
                    if ((attributeSet != null) &&
                        (activeReport.ReportType != "Question" || (activeReport.ReportType == "Question" && attributeSet.Attributes != null && attributeSet.Attributes.Count > 0)))
                    {
                        ReportColumnCode rccNew = new ReportColumnCode();
                        rccNew.AttributeId = attributeSet.AttributeId;
                        rccNew.CodeOrder = 0;
                        rccNew.DisplayAdditionalText = true;
                        rccNew.DisplayCode = true;
                        rccNew.DisplayCodedText = true;
                        rccNew.ParentAttributeId = attributeSet.AttributeId;
                        rccNew.ParentAttributeText = attributeSet.AttributeName;
                        rccNew.ReportColumnId = 0;
                        rccNew.SetId = attributeSet.SetId;
                        rccNew.UserDefText = attributeSet.AttributeName;

                        if (e.TargetDropItem != null)
                        {
                            int index = 0;
                            switch (e.DropPosition)
                            {
                                case DropPosition.After:
                                    index = e.TargetDropItem.Index + 1;
                                    break;
                                case DropPosition.Before:
                                    index = e.TargetDropItem.Index;
                                    break;
                                case DropPosition.Inside:
                                    MessageBox.Show("Inside?");
                                    break;
                                default: break;
                            }
                            if (activeReport.ReportType == "Question")
                            {
                                ((e.Source as Telerik.Windows.Controls.RadTreeView).ItemsSource as ReportColumnCodeList).Insert(index, rccNew);
                                ReportIsEdited = true;
                            }
                            else
                            {
                                MessageBox.Show("Only 'question' reports can have more than one code per column.");
                            }
                        }
                        else // empty treeview so allowable for both types of report
                        {
                            ((e.Source as Telerik.Windows.Controls.RadTreeView).ItemsSource as ReportColumnCodeList).Add(rccNew);
                            ReportIsEdited = true;
                        }
                    }
                    else
                    {
                        if (activeReport.ReportType == "Question")
                        {
                            ReviewSet rs = o as ReviewSet;
                            if ((rs != null) && (rs.Attributes != null) && (rs.Attributes.Count > 0))
                            {
                                ReportColumnCode rccNew = new ReportColumnCode();
                                rccNew.AttributeId = 0;
                                rccNew.CodeOrder = 0;
                                rccNew.DisplayAdditionalText = true;
                                rccNew.DisplayCode = true;
                                rccNew.DisplayCodedText = true;
                                rccNew.ParentAttributeId = 0;
                                rccNew.ParentAttributeText = rs.SetName;
                                rccNew.ReportColumnId = 0;
                                rccNew.SetId = rs.SetId;
                                rccNew.UserDefText = rs.SetName;

                                if (e.TargetDropItem != null)
                                {
                                    int index = 0;
                                    switch (e.DropPosition)
                                    {
                                        case DropPosition.After:
                                            index = e.TargetDropItem.Index + 1;
                                            break;
                                        case DropPosition.Before:
                                            index = e.TargetDropItem.Index;
                                            break;
                                        case DropPosition.Inside:
                                            MessageBox.Show("Inside?");
                                            break;
                                        default: break;
                                    }
                                    ((e.Source as Telerik.Windows.Controls.RadTreeView).ItemsSource as ReportColumnCodeList).Insert(index, rccNew);
                                    ReportIsEdited = true;
                                }
                                else // empty treeview
                                {
                                    ((e.Source as Telerik.Windows.Controls.RadTreeView).ItemsSource as ReportColumnCodeList).Add(rccNew);
                                    ReportIsEdited = true;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Code sets can only be used in 'Question' reports.");
                        }
                    }
                }
            }
        }

        private void contextMenuDelete_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            RadTreeViewItem item = ((e.OriginalSource as RadMenuItem).Parent as RadContextMenu).GetClickedElement<RadTreeViewItem>();
            if (item != null)
            {
                ReportColumnCode rcc = item.DataContext as ReportColumnCode;
                if (rcc != null)
                {
                    ReportColumn rccl = item.ParentTreeView.DataContext as ReportColumn;
                    if (rccl != null)
                    {
                        rcc.SetCodeAsNew();
                        rccl.Codes.Remove(rcc);
                        ReportIsEdited = true;
                    }
                }
            }
        }

        private void cmdAddNewColumn_Click(object sender, RoutedEventArgs e)
        {
            ReportColumn rc = new ReportColumn();
            rc.Codes = new ReportColumnCodeList();
            rc.Name = "New column";
            activeReport.Columns.Add(rc);
            ReportIsEdited = true;
            val_SizeChanged(TileViewActiveReport, null);
        }
        void val_SizeChanged(object sender, EventArgs e)
        {
            if (sender == null) return;
            Telerik.Windows.Controls.RadTileView val = sender as Telerik.Windows.Controls.RadTileView;
            if (val == null) return;
            double wid = val.ActualWidth;
            double expected = wid / val.Items.Count;

            if (expected < 160) val.ColumnWidth = new GridLength(160.0);
            else if (expected > 360 || val.Items.Count == 0) val.ColumnWidth = new GridLength(360.0);
            else val.ColumnWidth = new GridLength(expected);
        }
        private void cmdNewReport_Click(object sender, RoutedEventArgs e)
        {
            MakeNewReport();
        }

        public void SaveReport()
        {
            if (ReportIsEdited == true && InitialisingControl == false &&
                MessageBox.Show("Report has changed. Save report?", "Confirm report save", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                cmdSaveReport_Click(null, null);
            }
            else
            {
                ReportIsEdited = false;
            }
        }

        private void cmdSaveReport_Click(object sender, RoutedEventArgs e)
        {
            Report report = TextBlockReportTitle.DataContext as Report;
            foreach (RadTileViewItem tile in TileViewActiveReport.ChildrenOfType<RadTileViewItem>())
            {
                if (tile.TileState == TileViewItemState.Maximized)
                {//means we have messed up the orders of the tiles and have to use the buffer values to save to DB
                    //ReportColumnList rep = TileViewActiveReport.ItemsSource as ReportColumnList;
                    //if (rep == null) return;
                    //foreach (ReportColumn data in rep)
                    //{
                    //    data.ColumnOrder = data.BufferPosition;
                    //    data.BufferPosition = data.ColumnOrder;
                    //}
                    //break;
                    AsynchSaveCheck = 0;
                    tile.TileState = TileViewItemState.Restored;
                    while (AsynchSaveCheck < 100)
                    {//this is a precaution, apparently the setting the Restored state makes the two events that reinstate the correct order of tiles happen 
                        //before the rest of this procedure is executed, but I'm not sure this will always be the case (they are asynch events after all)
                        //hence, I've added this loop that will wait for a max of 600ms while letting com and message pumping to perform (join method)
                        //the AsynchSaveCheck value is set to 1001 by the last event handler (TilesStateChanged), 
                        //so it will jump up above 100 once the adjustments are done, usually happens before reaching the loop!
                        AsynchSaveCheck++;
                        System.Threading.Thread.CurrentThread.Join(6);
                    }
                }
            }
            if (report != null)
            {
                CslaDataProvider provider = App.Current.Resources["ReportListData"] as CslaDataProvider;
                ReportList reportList = provider.Data as ReportList;
                if (!reportList.Contains(report)) reportList.Add(report);
                report.Detail = DateTime.Now.ToString(); // makes sure the report IsDirty for saving
                report.Saved += (o, e2) =>
                {
                    BusyReportSave.IsRunning = false;
                    windowLoadReport.GridViewReports.IsEnabled = true;
                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                    else
                    {
                        //report = e2.NewObject as Report;
                        //reportList.Remove(report);
                        activeReport = e2.NewObject as Report;
                        //reportList.Add(activeReport);
                        int ind = reportList.IndexOf(report);
                        reportList.RaiseListChangedEvents = false;
                        if (ind > 0)
                        {
                            reportList[ind] = activeReport;

                        }
                        else
                        {
                            ind = reportList.IndexOf(activeReport);
                            if (ind > 0)
                            {
                                reportList[ind] = activeReport;
                            }
                            else reportList.Add(activeReport);
                        }
                        
                        reportList.RaiseListChangedEvents = true;
                        BindActiveReport();
                        ReportIsEdited = false;
                    }
                    //provider.Refresh();
                    
                };
                //report.ApplyEdit();
                
                //not needed if the binding of position actually works...
                //for (int i = 0; i < report.Columns.Count; i++)
                //{
                //    RadTileViewItem tileViewItem = this.TileViewActiveReport.ItemContainerGenerator.ContainerFromIndex(i) as RadTileViewItem;
                //    if (tileViewItem != null)
                //    {
                //        ReportColumn rc = tileViewItem.DataContext as ReportColumn;
                //        if (rc != null)
                //        {
                //            rc.ColumnOrder = tileViewItem.Position;
                //        }
                //    }
                //}

                
                BusyReportSave.IsRunning = true;
                windowLoadReport.GridViewReports.IsEnabled = false;
                
                //reportList.SaveItem(report);
                //provider.Saved += (o, e2) =>
                //{
                //    object oo = o;
                //    object ee2 = e2;
                //    string s = o.ToString();
                //};
                //provider.Save();
                //provider.Refresh();
                reportList.SaveItem(report);
                //report.BeginSave();
            }
        }

        private void cmdDelete_Click(object sender, RoutedEventArgs e)
        {
            Report report =(sender as Button).DataContext as Report;
            if ((report != null) && (MessageBox.Show("Are you sure you want to delete this report?", "Delete report?", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
            {
                
                
                CslaDataProvider provider = App.Current.Resources["ReportListData"] as CslaDataProvider;
                ReportList reportList = provider.Data as ReportList;
                if (report.ReportId == 0)
                {//report was never saved, just remove it from the list
                    reportList.Remove(report);
                    if (report == TextBlockReportTitle.DataContext as Report)
                    {//we just deleted the report that was being edited.
                        if (reportList == null || reportList.Count == 0)
                        {
                            MakeNewReport();
                        }
                        else
                        {
                            activeReport = reportList[reportList.Count - 1];
                            BindActiveReport();
                        }
                    }
                    return;
                }
                report.Delete();
                report.Saved += (o, e2) =>
                {
                    BusyReportSave.IsRunning = false;
                    windowLoadReport.GridViewReports.IsEnabled = true;
                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                    else
                    {
                        if (report == TextBlockReportTitle.DataContext as Report)
                        {//we just deleted the report that was being edited.
                            if (reportList == null || reportList.Count == 0)
                            {
                                MakeNewReport();//also binds it!
                            }
                            else
                            {
                                activeReport = reportList[reportList.Count - 1];
                                BindActiveReport();
                            }
                        }
                    }
                };
                BusyReportSave.IsRunning = true;
                
                windowLoadReport.GridViewReports.IsEnabled = false;
                reportList.SaveItem(report);
            }
        }

        private void cmdOpenReport_Click(object sender, RoutedEventArgs e)
        {
            //windowLoadReport.MaxHeight = this.LayoutRoot.ActualHeight - 20;
            windowLoadReport.GridViewReports.Height = this.LayoutRoot.ActualHeight - 58;
            windowLoadReport.ShowDialog();
        }

        private void ComboReportType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboReportType.SelectedIndex > 0)
            {
                if (!activeReport.CheckOkToChangeToAnswer())
                {
                    MessageBox.Show("Sorry: 'answer' reports cannot contain more than one code per column and cannot contain code sets.");
                    ComboReportType.SelectedIndex = 0;
                    return;
                }
            }
            string repType = "";
            switch (ComboReportType.SelectedIndex)
            {
                case 0: repType = "Question";
                    break;
                case 1: repType = "Answer";
                    break;
            }
            activeReport.ReportType = repType;
            ReportIsEdited = true;
            if (ReportTypeChanged != null)
                ReportTypeChanged.Invoke(this, EventArgs.Empty);

        }

        private void cmdOpenReportsWindow_Click(object sender, RoutedEventArgs e)
        {
            SaveReport();
            if (OpenReportWindowCommand != null)
                OpenReportWindowCommand.Invoke(this, EventArgs.Empty);
        }

        private void RadTreeView_Loaded(object sender, RoutedEventArgs e)
        {
            //DependencyObject o2 = sender as DependencyObject;
            //DragDropManager.AddDragDropCompletedHandler(o2, RadTreeView_PreviewDragEnded, true);
            Telerik.Windows.Controls.RadTreeView o2 = sender as Telerik.Windows.Controls.RadTreeView;
            o2.PreviewDragEnded -= RadTreeView_PreviewDragEnded;
            o2.PreviewDragEnded += new RadTreeViewDragEndedEventHandler(RadTreeView_PreviewDragEnded);
            TreeViewSettings.SetDragDropExecutionMode(o2, TreeViewDragDropExecutionMode.Legacy);
        }

        public void UnHookMe()
        {
            CslaDataProvider provider = ((CslaDataProvider)App.Current.Resources["ReportListData"]);
            if (provider != null)
            {
                provider.DataChanged -= CslaDataProvider_DataChanged;
                provider.DataChanged -= CslaDataProvider_DataChanged;
            }
        }

        
    }
}
