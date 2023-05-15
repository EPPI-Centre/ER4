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
using Telerik.Windows.Controls.GridView;
using Csla.Xaml;
using Telerik.Windows.Persistence;
using System.IO;
using System.Text.RegularExpressions;

// for persistence of gridview
using System.ComponentModel;
using Telerik.Windows.Data;
using Telerik.Windows.Persistence.Services;
using System.Diagnostics;
using Csla;

namespace EppiReviewer4
{
    public partial class dialogMetaAnalysisSetup : RadWindow
    {
        //public event EventHandler ReloadMetaAnalyses;
        private bool SettingUp;
        dialogReportViewer reports = new dialogReportViewer();
        private RadWindow windowReportsDocuments = new RadWindow();
        private RadWAddColumnToMA windowAddColumn = new RadWAddColumnToMA();
        

        public dialogMetaAnalysisSetup()
        {
            InitializeComponent();
            this.AddHandler(GridViewHeaderCell.MouseRightButtonDownEvent, new MouseButtonEventHandler(MouseDownOnHeaderCell), true);
            ServiceProvider.RegisterPersistenceProvider<ICustomPropertyProvider>(typeof(RadGridView), new GridViewCustomPropertyProvider());
            //prepare windowReportsDocuments
            windowReportsDocuments.Header = "Report viewer";
            windowReportsDocuments.WindowStateChanged += new EventHandler(Helpers.WindowHelper.MaxOnly_WindowStateChanged);
            windowReportsDocuments.Style = Application.Current.Resources["CustomRadWindowStyle"] as Style;
            windowReportsDocuments.WindowState = WindowState.Maximized;
            windowReportsDocuments.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            windowReportsDocuments.RestrictedAreaMargin = new Thickness(20); 
            windowReportsDocuments.CanClose = true;
            windowReportsDocuments.Width = 500;
            Grid grd = new Grid();
            grd.Children.Add(reports);
            windowReportsDocuments.Content = grd;
            //end of windowReportsDocuments
            
            windowAddColumn.cmdOk_Clicked += new EventHandler<RoutedEventArgs>(windowAddColumn_cmdOk_Clicked);
        }

        public void ShowWindow(MetaAnalysis ma)
        {
            SettingUp = true;
            this.DataContext = ma;
            ComboBoxMetaOutcomeType.SelectedIndex = -1;

            if (!ma.IsNew)
            {
                ComboBoxMetaOutcomeType.SelectedIndex = ma.MetaAnalysisTypeId;
                ComboBoxNetMetaOutcomeType.SelectedIndex = ma.NMAStatisticalModel;
                ComboBoxNMAOutcomeType.SelectedIndex = ma.MetaAnalysisTypeId;
            }
            else
            {
                ComboBoxMetaOutcomeType.SelectedIndex = 0;
                ComboBoxNetMetaOutcomeType.SelectedIndex = 0;
                ComboBoxNMAOutcomeType.SelectedIndex = 0;
            }

            if (ma.IsNew)
            {
                ma.Title = "New meta-analysis";
                //2023 change: we don't save a new MA upon creation, we wait for users to save it, given we have a new "onclose" check
                //SaveMetaAnalysis(false, true);
                GetOutcomesList(true);
            }
            else
            {
                GetOutcomesList(false);
            }
            switch (ma.AnalysisType)
            {
                case 0: TabControlAnalyses.SelectedIndex = 0; break;
                case 1: TabControlAnalyses.SelectedIndex = 1; break;
                case 2: TabControlAnalyses.SelectedIndex = 2; break;
                default: break;
            }
            SettingUp = false;
            dialogMetaAnalysisSetupWindow.ShowDialog();
        }

        private void GetOutcomesList(bool SetSelectSelectable)
        {
            //returnValue.Outcomes = OutcomeList.GetOutcomeList(returnValue.SetId, returnValue.AttributeIdIntervention, returnValue.AttributeIdControl,
            //    returnValue.AttributeIdOutcome, returnValue.AttributeId, returnValue.MetaAnalysisId, returnValue.AttributeIdQuestion, returnValue.AttributeIdAnswer);

            MetaAnalysis ma = this.DataContext as MetaAnalysis;
            DataPortal<OutcomeList> dp = new DataPortal<OutcomeList>();
            dp.FetchCompleted += (o, e2) =>
            {
                radBusyEditMAIndicator.IsBusy = false;
                if (e2.Error != null)
                {
                    MessageBox.Show(e2.Error.Message);
                }
                else
                {
                    ma = this.DataContext as MetaAnalysis;
                    //bool IsDirtyCheck = ma.IsDirty;
                    //if (IsDirtyCheck == true)
                    //{
                    //    RadWindow.Alert("AHA0");
                    //    IsDirtyCheck = ma.IsDirty;
                    //}
                    //ma.Outcomes = e2.Object as OutcomeList;

                    ma.SetOutcomesList(e2.Object as OutcomeList);
                    //if (IsDirtyCheck != ma.IsDirty)
                    //{
                    //    RadWindow.Alert("AHA1");
                    //    if (e2.Object.FirstOrDefault(f=> f.IsDirty == true) != null)
                    //    {
                    //        RadWindow.Alert("AHA1.2!");
                    //    }
                    //    IsDirtyCheck = ma.IsDirty;
                    //}

                    cbModel.SelectedIndex = ma.StatisticalModel;
                    //if (IsDirtyCheck != ma.IsDirty)
                    //{
                    //    RadWindow.Alert("AHA2");
                    //    IsDirtyCheck = ma.IsDirty;
                    //}

                    cbOutputType.SelectedIndex = ma.Verbose;
                    //if (IsDirtyCheck != ma.IsDirty)
                    //{
                    //    RadWindow.Alert("AHA3");
                    //    IsDirtyCheck = ma.IsDirty;
                    //}

                    SetUpOutcomesGrid(SetSelectSelectable);
                    //if (IsDirtyCheck != ma.IsDirty)
                    //{
                    //    RadWindow.Alert("AHA4");
                    //    IsDirtyCheck = ma.IsDirty;
                    //}
                    EnableDisableKNHA();
                }
            };
            radBusyEditMAIndicator.IsBusy = true;
            dp.BeginFetch(new BusinessLibrary.BusinessClasses.OutcomeList.OutcomeListSelectionCriteria(typeof(OutcomeList), ma.SetId, ma.AttributeIdIntervention,
                ma.AttributeIdControl, ma.AttributeIdOutcome, 0, ma.MetaAnalysisId, ma.AttributeIdQuestion, ma.AttributeIdAnswer));
        }

        private void SetUpOutcomesGrid(bool SetSelectSelectable)
        {
            if (this.DataContext != null)
            {
                MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
                //bool IsDirtyCheck = _currentSelectedMetaAnalysis.IsDirty;
                
                _currentSelectedMetaAnalysis.Outcomes.SetMetaAnalysisType(ComboBoxMetaOutcomeType.SelectedIndex);

                //if (IsDirtyCheck != _currentSelectedMetaAnalysis.IsDirty)
                //{
                //    IsDirtyCheck = _currentSelectedMetaAnalysis.IsDirty;
                //    RadWindow.Alert("BABA1");
                //}

                _currentSelectedMetaAnalysis.SetupModeratorList();

                //if (IsDirtyCheck != _currentSelectedMetaAnalysis.IsDirty)
                //{
                //    IsDirtyCheck = _currentSelectedMetaAnalysis.IsDirty;
                //    RadWindow.Alert("BABA2");
                //}

                GridViewMetaStudies.ItemsSource = _currentSelectedMetaAnalysis.Outcomes;
                GridViewModerators.ItemsSource = _currentSelectedMetaAnalysis.MetaAnalysisModerators;
                foreach (MetaAnalysisModerator mam in _currentSelectedMetaAnalysis.MetaAnalysisModerators)
                {
                    if (mam.FieldName == "InterventionText")
                    {
                        cbNMAReferences.ItemsSource = mam.References;
                    }
                }
                SetOutcomeClassificationCodeFilters();
                SetAttributeAnswerFilters();
                SetAttributeQuestionFilters();
                SetGridViewMetaStudiesFilters();
                if (SetSelectSelectable)
                {
                    SelectSelectable();
                }
                EnableDisableKNHA();
            }
        }

        private void UpdateCurrentMetaAnalysis()
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            _currentSelectedMetaAnalysis.MetaAnalysisTypeId = ComboBoxMetaOutcomeType.SelectedIndex;
            _currentSelectedMetaAnalysis.StatisticalModel = cbModel.SelectedIndex;
            _currentSelectedMetaAnalysis.Verbose = cbOutputType.SelectedIndex;
            _currentSelectedMetaAnalysis.NMAStatisticalModel = ComboBoxNetMetaOutcomeType.SelectedIndex;

            //PersistenceManager manager = new PersistenceManager();
            //System.IO.StreamReader sr = new System.IO.StreamReader(manager.Save(GridViewMetaStudies));
            //_currentSelectedMetaAnalysis.GridSettings = sr.ReadToEnd();
        }

        private void cmdMetaSaveMetaAnalysis_Click(object sender, RoutedEventArgs e)
        {
            SaveMetaAnalysis(false, false);
        }

        private void SaveMetaAnalysis(bool CloseWindow, bool SetSelectSelectable)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = (MetaAnalysis)this.DataContext;
            if (_currentSelectedMetaAnalysis.Title != "")
            {
                UpdateCurrentMetaAnalysis();
                if (_currentSelectedMetaAnalysis.IsDirty == false) _currentSelectedMetaAnalysis.DoMarkDirty();

                _currentSelectedMetaAnalysis.Saved += (o, e2) =>
                {
                    radBusyEditMAIndicator.IsBusy = false;
                    MetaAnalysis returnedMA = (MetaAnalysis)e2.NewObject;
                    if (e2.Error != null)
                        MessageBox.Show(e2.Error.Message);
                    //we also replace the new (saved) obj into the global list of MAs (new: April 2023)
                    CslaDataProvider maldProvider = ((CslaDataProvider)App.Current.Resources["MetaAnalysisListData"]);
                    MetaAnalysisList mald = maldProvider.Data as MetaAnalysisList;
                    MetaAnalysis toSwap = mald.FirstOrDefault(f => f.MetaAnalysisId == _currentSelectedMetaAnalysis.MetaAnalysisId);
                    int index = mald.IndexOf(toSwap);
                    if (CloseWindow == true)
                    {
                        if (maldProvider != null)
                            maldProvider.Refresh();
                        dialogMetaAnalysisSetupWindow.Close();
                    }
                    else
                    {//changed on April 2023
                        if (index == -1) mald.Add(returnedMA);
                        else mald[index] = returnedMA;
                        this.DataContext = returnedMA;
                        maldProvider.Rebind();
                        //if (maldProvider != null)
                        //    maldProvider.Refresh();
                        SetUpOutcomesGrid(SetSelectSelectable);
                    }

                };
                radBusyEditMAIndicator.Visibility = System.Windows.Visibility.Visible;
                radBusyEditMAIndicator.IsBusy = true;
                _currentSelectedMetaAnalysis.BeginSave(true);
            }
            else
            {
                MessageBox.Show("Please give your meta-analysis a title first");
            }
        }

        private void SetOutcomeClassificationCodeFilters()
        {
            // set all outcome classification code columns to invisible
            for (int i = 1; i < 31; i++)
            {
                GridViewMetaStudies.Columns["occ" + i.ToString()].IsVisible = false;
            }

            if (ComboBoxMetaOutcomeType != null)
            {
                OutcomeList ol = GridViewMetaStudies.ItemsSource as OutcomeList;
                if (ol != null)
                {
                    Dictionary<long, string> fieldNames = ol.GetOutcomeClassificationFieldList();
                    int c = 1;
                    foreach (KeyValuePair<long, string> currField in fieldNames)
                    {
                        Telerik.Windows.Controls.GridViewColumn col = GridViewMetaStudies.Columns["occ" + c.ToString()];
                        if (col != null)
                        {
                            col.Header = Regex.Replace(currField.Value, @"[^A-Za-z0-9]+", "");
                            col.IsVisible = true;
                        }
                        c++;
                    }
                    
                    GridViewMetaStudies.Rebind();
                }
            }
        }

        private void SetAttributeAnswerFilters()
        {
            OutcomeList ol = GridViewMetaStudies.ItemsSource as OutcomeList;
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            if (ol != null && _currentSelectedMetaAnalysis != null)
            {
                string[] AttributeNames = _currentSelectedMetaAnalysis.AttributeAnswerText.Split('¬');

                for (int i = 0; i < 20; i++)
                {
                    Telerik.Windows.Controls.GridViewColumn col = GridViewMetaStudies.Columns["aa" + (i + 1).ToString()];
                    if (col != null && i < AttributeNames.Length && AttributeNames[i] != "")
                    {
                        col.Header = Regex.Replace(AttributeNames[i], @"[^A-Za-z0-9]+", "");
                        col.IsVisible = true;
                    }
                    else
                    {
                        col.IsVisible = false;
                    }
                }
            }
        }

        private void SetAttributeQuestionFilters()
        {
            OutcomeList ol = GridViewMetaStudies.ItemsSource as OutcomeList;
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            if (ol != null && _currentSelectedMetaAnalysis != null)
            {
                string[] QuestionNames = _currentSelectedMetaAnalysis.AttributeQuestionText.Split('¬');

                for (int i = 0; i < 20; i++)
                {
                    Telerik.Windows.Controls.GridViewColumn col = GridViewMetaStudies.Columns["aq" + (i + 1).ToString()];
                    if (col != null && i < QuestionNames.Length && QuestionNames[i] != "")
                    {
                        col.Header = Regex.Replace(QuestionNames[i], @"[^A-Za-z0-9]+", "");
                        col.IsVisible = true;
                    }
                    else
                    {
                        col.IsVisible = false;
                    }
                }
            }
        }

        private void SetGridViewMetaStudiesFilters()
        {
            //see: https://docs.telerik.com/devtools/silverlight/controls/radgridview/filtering/programmatic
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            GridViewMetaStudies.SortDescriptors.Clear();
            GridViewMetaStudies.FilterDescriptors.Clear();
            if (_currentSelectedMetaAnalysis != null)
            {
                //first the easy bit: sorting
                if (_currentSelectedMetaAnalysis.SortedBy != "")
                {
                    ColumnSortDescriptor csd = new ColumnSortDescriptor();
                    string colname = "";
                    if (_currentSelectedMetaAnalysis.SortedBy.StartsWith("aa")
                        || _currentSelectedMetaAnalysis.SortedBy.StartsWith("aq")
                        || _currentSelectedMetaAnalysis.SortedBy.StartsWith("occ")
                        )
                        colname = _currentSelectedMetaAnalysis.SortedBy;
                    else if (_currentSelectedMetaAnalysis.SortedBy == "ES") colname = "ESColumn";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "SEES") colname = "SEESColumn";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "ShortTitle") colname = "titleColumn";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "Title") colname = "DescColumn";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "TimepointDisplayValue") colname = "TimepointColumn";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "OutcomeTypeName") colname = "OutcomeTypeName";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "OutcomeText") colname = "OutcomeColumn";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "InterventionText") colname = "InterventionColumn";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "ControlText") colname = "ComparisonColumn";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "grp1ArmName") colname = "Arm1Column";
                    else if (_currentSelectedMetaAnalysis.SortedBy == "grp2ArmName") colname = "Arm2Column";
                    csd.Column = GridViewMetaStudies.Columns[colname];
                    csd.SortDirection = _currentSelectedMetaAnalysis.SortDirection == "Ascending" ? ListSortDirection.Ascending : ListSortDirection.Descending;
                    GridViewMetaStudies.SortDescriptors.Add(csd);
                    //GridViewMetaStudies.InvalidateArrange();
                    //GridViewMetaStudies.Rebind();
                    //GridViewMetaStudies.UpdateLayout();
                }

                //then the tricky filters
                if (_currentSelectedMetaAnalysis.FilterSettingsList != null
                && _currentSelectedMetaAnalysis.FilterSettingsList.Count > 0)
                {
                    GridViewMetaStudies.FilterDescriptors.SuspendNotifications();
                    foreach (MetaAnalysisFilterSetting setting in _currentSelectedMetaAnalysis.FilterSettingsList)
                    {
                        GridViewColumn col = GridViewMetaStudies.Columns[setting.ColumnName];
                        if (col != null)
                        {
                            IColumnFilterDescriptor colFilter = col.ColumnFilterDescriptor;
                            colFilter.Clear();//necessary despite having already called GridViewMetaStudies.FilterDescriptors.Clear(); - go figure!
                            if (setting.SelectedValues != null && setting.SelectedValues != "")
                            {
                                string[] vals = setting.SelectedValues.Split(new string[] { "{¬}" }, StringSplitOptions.RemoveEmptyEntries);
                                if (vals != null)
                                {
                                    foreach (string selval in vals)
                                    {
                                        if (col.UniqueName.StartsWith("aa"))
                                        {//for our purposes, these columns have numeric values (actually 0 or 1), bound fields are of type long/Int64
                                            Int64 intVal;
                                            if (Int64.TryParse(selval, out intVal)) colFilter.DistinctFilter.AddDistinctValue(intVal);
                                        }
                                        else if (col.UniqueName.StartsWith("occ"))
                                        {//these cols refer to int values
                                            int intVal;
                                            if (int.TryParse(selval, out intVal)) colFilter.DistinctFilter.AddDistinctValue(intVal);
                                        }
                                        else colFilter.DistinctFilter.AddDistinctValue(selval);

                                    }
                                }
                            }
                            if (setting.Filter1 != null)
                            {
                                if (setting.Filter1 == "") colFilter.FieldFilter.Filter1.Value = Telerik.Windows.Data.OperatorValueFilterDescriptorBase.UnsetValue;
                                else colFilter.FieldFilter.Filter1.Value = setting.Filter1;
                                colFilter.FieldFilter.Filter1.IsCaseSensitive = setting.Filter1CaseSensitive;
                                switch (setting.Filter1Operator)
                                {
                                    case "IsLessThan":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsLessThan;
                                        break;
                                    case "IsLessThanOrEqualTo":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsLessThanOrEqualTo;
                                        break;
                                    case "IsEqualTo":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsEqualTo;
                                        break;
                                    case "IsNotEqualTo":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsNotEqualTo;
                                        break;
                                    case "IsGreaterThanOrEqualTo":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsGreaterThanOrEqualTo;
                                        break;
                                    case "IsGreaterThan":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsGreaterThan;
                                        break;
                                    case "StartsWith":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.StartsWith;
                                        break;
                                    case "EndsWith":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.EndsWith;
                                        break;
                                    case "Contains":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.Contains;
                                        break;
                                    case "DoesNotContain":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.DoesNotContain;
                                        break;
                                    case "IsContainedIn":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsContainedIn;
                                        break;
                                    case "IsNotContainedIn":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsNotContainedIn;
                                        break;
                                    case "IsNull":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsNull;
                                        break;
                                    case "IsNotNull":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsNotNull;
                                        break;
                                    case "IsEmpty":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsEmpty;
                                        break;
                                    case "IsNotEmpty":
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsNotEmpty;
                                        break;
                                    default:
                                        colFilter.FieldFilter.Filter1.Operator = FilterOperator.IsEqualTo;
                                        break;
                                }


                                if (setting.FiltersLogicalOperator == "And")
                                {
                                    colFilter.FieldFilter.LogicalOperator = Telerik.Windows.Data.FilterCompositionLogicalOperator.And;
                                }
                                else
                                {
                                    colFilter.FieldFilter.LogicalOperator = Telerik.Windows.Data.FilterCompositionLogicalOperator.Or;
                                }
                            }
                            else
                            {
                                colFilter.FieldFilter.Filter1.Value = Telerik.Windows.Data.OperatorValueFilterDescriptorBase.UnsetValue;
                            }

                            if (setting.Filter2 != null)
                            {
                                if (setting.Filter2 == "") colFilter.FieldFilter.Filter2.Value = Telerik.Windows.Data.OperatorValueFilterDescriptorBase.UnsetValue;
                                else colFilter.FieldFilter.Filter2.Value = setting.Filter2;
                                colFilter.FieldFilter.Filter2.IsCaseSensitive = setting.Filter2CaseSensitive;
                                switch (setting.Filter2Operator)
                                {
                                    case "IsLessThan":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsLessThan;
                                        break;
                                    case "IsLessThanOrEqualTo":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsLessThanOrEqualTo;
                                        break;
                                    case "IsEqualTo":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsEqualTo;
                                        break;
                                    case "IsNotEqualTo":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsNotEqualTo;
                                        break;
                                    case "IsGreaterThanOrEqualTo":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsGreaterThanOrEqualTo;
                                        break;
                                    case "IsGreaterThan":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsGreaterThan;
                                        break;
                                    case "StartsWith":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.StartsWith;
                                        break;
                                    case "EndsWith":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.EndsWith;
                                        break;
                                    case "Contains":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.Contains;
                                        break;
                                    case "DoesNotContain":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.DoesNotContain;
                                        break;
                                    case "IsContainedIn":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsContainedIn;
                                        break;
                                    case "IsNotContainedIn":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsNotContainedIn;
                                        break;
                                    case "IsNull":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsNull;
                                        break;
                                    case "IsNotNull":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsNotNull;
                                        break;
                                    case "IsEmpty":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsEmpty;
                                        break;
                                    case "IsNotEmpty":
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsNotEmpty;
                                        break;
                                    default:
                                        colFilter.FieldFilter.Filter2.Operator = FilterOperator.IsEqualTo;
                                        break;
                                }
                            }
                            else
                            {
                                colFilter.FieldFilter.Filter2.Value = Telerik.Windows.Data.OperatorValueFilterDescriptorBase.UnsetValue;
                            }
                            //bool tb = colFilter.IsActive;
                            //colFilter.Refresh();
                            //tb = colFilter.IsActive;
                        }
                    }
                    GridViewMetaStudies.FilterDescriptors.ResumeNotifications();
                }
                //else
                //{
                //    GridViewMetaStudies.FilterDescriptors.SuspendNotifications();
                //    GridViewMetaStudies.FilterDescriptors.ResumeNotifications();
                //}
                
            }

            //MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            //GridViewMetaStudies.SortDescriptors.Clear();
            //GridViewMetaStudies.FilterDescriptors.Clear();
            //if (_currentSelectedMetaAnalysis != null && _currentSelectedMetaAnalysis.GridSettings != "")
            //{
            //    byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(_currentSelectedMetaAnalysis.GridSettings);
            //    using (System.IO.Stream stream = new System.IO.MemoryStream(byteArray))
            //    {
            //        stream.Position = 0L;
            //        PersistenceManager manager = new PersistenceManager();
            //        manager.Load(GridViewMetaStudies, stream);
            //    }
            //    Telerik.Windows.Data.SortDescriptorCollection sdc = GridViewMetaStudies.SortDescriptors;
            //    _currentSelectedMetaAnalysis.SortedBy = "";
            //    _currentSelectedMetaAnalysis.SortDirection = "";
            //    foreach (Telerik.Windows.Controls.GridView.ColumnSortDescriptor sd in sdc) // there's actually only 1!
            //    {
            //        _currentSelectedMetaAnalysis.SortedBy = (sd.Column as GridViewDataColumn).DataMemberBinding.Path.Path;
            //        if (sd.SortDirection == ListSortDirection.Ascending)
            //        {
            //            _currentSelectedMetaAnalysis.SortDirection = "Ascending";
            //        }
            //        else
            //        {
            //            _currentSelectedMetaAnalysis.SortDirection = "Descending";
            //        }
            //    }

            //    //
            //}
        }

        //private void ReapplyFilterButton_Click(object sender, RoutedEventArgs e)
        //{
        //    SetGridViewMetaStudiesFilters();
        //}

        private void ComboBoxMetaOutcomeType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            ComboBoxItem selected = ComboBoxMetaOutcomeType.SelectedItem as ComboBoxItem;
            if (selected != null && selected.Content != null)
            {
                _currentSelectedMetaAnalysis.MetaAnalysisTypeId = ComboBoxMetaOutcomeType.SelectedIndex;
                _currentSelectedMetaAnalysis.MetaAnalysisTypeTitle = selected.Content.ToString();
            }
            SelectSelectable();
        }

        // (and unselect what's not selectable - i.e. not available for meta-analysis for this outcome type and/or has zero variance)
        // Should only be called for NEW outcomes and when the drop-down to change meta-analysis type is changed
        private void SelectSelectable()
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            if (_currentSelectedMetaAnalysis.Outcomes != null)
            {
                _currentSelectedMetaAnalysis.Outcomes.SetMetaAnalysisType(ComboBoxMetaOutcomeType.SelectedIndex);
                _currentSelectedMetaAnalysis.Outcomes.UnSelectAll();
                foreach (Outcome o in GridViewMetaStudies.Items)
                {
                    o.IsSelected = true; // IsSelected won't select what can't be selected so this is safe
                }
            }
        }
        
        
        //private CheckBox cbQuestionOrAnswer = null;
        //private codesSelectControl cscSelectedCode = null;

        private void cmdAddColumn_Click(object sender, RoutedEventArgs e)
        {
            
            //FrameworkElement fe = (sender as FrameworkElement);
            //if (fe != null)
            //{
            //    Outcome outC = fe.DataContext as Outcome;
            //    if (outC != null)
            //    {
            //        cscSelectedCode.showOnlyOneSetID(0);
            //    }
            //}
            windowAddColumn.ShowDialog();
        }

        void windowAddColumn_cmdOk_Clicked(object sender, RoutedEventArgs e)
        {
            MetaAnalysis ma = this.DataContext as MetaAnalysis;
            if (windowAddColumn.cscSelectedCode.SelectedAttributeSet() != null && ma != null)
            {
                AttributeSet selectedAttribute = windowAddColumn.cscSelectedCode.SelectedAttributeSet();
                if (windowAddColumn.cbQuestionOrAnswer.IsChecked == true)
                {
                    if (ma.AttributeIdAnswer.IndexOf(selectedAttribute.AttributeId.ToString()) == -1)
                    {
                        if (ma.AttributeIdAnswer == "")
                        {
                            ma.AttributeIdAnswer = selectedAttribute.AttributeId.ToString();
                        }
                        else
                        {
                            ma.AttributeIdAnswer += "," + selectedAttribute.AttributeId.ToString();
                        }
                    }
                    else
                    {
                        RadWindow.Alert("Error: you already have added that column");
                        return;
                    }
                }
                else
                {
                    if (ma.AttributeIdQuestion.IndexOf(selectedAttribute.AttributeId.ToString()) == -1)
                    {
                        if (ma.AttributeIdQuestion == "")
                        {
                            ma.AttributeIdQuestion = selectedAttribute.AttributeId.ToString();
                        }
                        else
                        {
                            ma.AttributeIdQuestion += "," + selectedAttribute.AttributeId.ToString();
                        }
                    }
                    else
                    {
                        RadWindow.Alert("Error: you already have added that column");
                        return;
                    }
                }
            }
            windowAddColumn.Close();
            SaveMetaAnalysis(false, false);
        }

        private string columnToDelete = "";
        private void MouseDownOnHeaderCell(object sender, MouseButtonEventArgs args)
        {
            GridViewHeaderCell cellClicked = ((FrameworkElement)args.OriginalSource).ParentOfType<GridViewHeaderCell>();
            if (cellClicked == null) return;
            args.Handled = true;
            if (cellClicked.Column.UniqueName.StartsWith("aa") || cellClicked.Column.UniqueName.StartsWith("aq"))
            {
                columnToDelete = cellClicked.Column.UniqueName;
                RadWindow.Confirm("Are you sure you want to delete this column?", this.OnDeleteColumnDlgClosed);
            }
        }
        
        private void OnDeleteColumnDlgClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == false)
                return;
            int index = 0;
            MetaAnalysis ma = this.DataContext as MetaAnalysis;
            string ColPrefix = "";
            if (columnToDelete.StartsWith("aq"))
            {
                ColPrefix = "aq";
                index = Convert.ToInt32(columnToDelete.Replace("aq", ""));
                ma.AttributeIdQuestion = removeAttributeIndex(ma.AttributeIdQuestion.Split(','), index -1);
            }
            else
            {
                ColPrefix = "aa";
                index = Convert.ToInt32(columnToDelete.Replace("aa", ""));
                ma.AttributeIdAnswer = removeAttributeIndex(ma.AttributeIdAnswer.Split(','), index -1);
            }

            //new 2023: check that the column isn't filtered, get rid of the filter if it is!
            //step 1: clear and update filter/sorting in the UI
            GridViewColumn col = GridViewMetaStudies.Columns[columnToDelete];
            if (col != null)
            {
                GridViewMetaStudies.SortDescriptors.SuspendNotifications();

                //2: if we're sorting by This column, stop doing it!
                if (ma.SortedBy == columnToDelete) ma.SortedBy = "";
                else
                {
                    //problem: if we had more optional colums after the one we're deleting and if we were sorting by one of them,
                    //then we need to update the sorting as we are changing the uniquename of the column we want to sort-by...
                    Telerik.Windows.Data.SortDescriptorCollection sortDescriptors = GridViewMetaStudies.SortDescriptors;
                    if (sortDescriptors != null)
                    {
                        foreach (ColumnSortDescriptor sort in sortDescriptors)//only one (or none), but we have an IEnumerable object...
                        {
                            if (sort.Column.UniqueName.StartsWith(ColPrefix))
                            {
                                int index2 = Convert.ToInt32(sort.Column.UniqueName.Replace(ColPrefix, ""));
                                if (index2 > index)
                                {//OK, we were sorting by a column that is being moved, we need to change the unique name of the "sortBy" field in current MA
                                 //saving it then makes it re-bind to the UI, so changes will update in the UI at that point
                                    ma.SortedBy = ColPrefix + (index2 - 1).ToString();
                                }
                            }
                        }
                    }
                }
                col.SortingState = SortingState.None;
                GridViewMetaStudies.SortDescriptors.ResumeNotifications();

                IColumnFilterDescriptor colFilter = col.ColumnFilterDescriptor;
                if (colFilter != null && colFilter.IsActive)
                {
                    GridViewMetaStudies.FilterDescriptors.SuspendNotifications();
                    colFilter.Clear();
                    GridViewMetaStudies.FilterDescriptors.ResumeNotifications();
                }
            }
            if (ma.FilterSettingsList != null)
                {

                //3: if there is a filter on this col within the MA object, remove it.                
                if (ma.FilterSettingsList.Count > 0)
                {
                    MetaAnalysisFilterSetting toRemove = ma.FilterSettingsList.FirstOrDefault(f => f.ColumnName == columnToDelete);
                    if (toRemove != null) toRemove.Delete();
                }
            }
            SaveMetaAnalysis(false, false);
        }

        private string removeAttributeIndex(string[] attrs, int index)
        {
            string retVal = "";
            var attrlist = attrs.ToList();
            attrlist.RemoveAt(index);
            if (attrlist.Count > 0)
            {
                for (int i = 0; i < attrlist.Count; i++)
                {
                    if (retVal == "")
                        retVal = attrlist[i];
                    else
                        retVal += "," + attrlist[i];
                }
            }
            return retVal;
        }

        private void cmdRunit_Click(object sender, RoutedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            if (!_currentSelectedMetaAnalysis.CheckValidModerators())
            {
                RadWindow.Alert("Error: please check you have valid moderators and data");
                return;
            }
            
            if (_currentSelectedMetaAnalysis.Outcomes.CountSelected() < 1)
            {
                RadWindow.Alert("You have no outcomes selected");
                return;
            }
            UpdateCurrentMetaAnalysis();
            MetaAnalysisRunInRCommand RrunCmd = new MetaAnalysisRunInRCommand();
            RrunCmd.MetaAnalaysisObject = _currentSelectedMetaAnalysis;
            DataPortal<MetaAnalysisRunInRCommand> dp = new DataPortal<MetaAnalysisRunInRCommand>();
            dp.ExecuteCompleted += (o, e2) =>
            {
                cmdRunit.IsEnabled = true;
                radBusyEditMAIndicator.IsBusy = false;
                if (e2.Error != null)
                {
                    RadWindow.Alert(e2.Error.Message);
                }
                else
                {
                    reports.DisplayRMetaAnalysis(e2.Object as MetaAnalysisRunInRCommand);
                    windowReportsDocuments.ShowDialog();
                }
            };
            radBusyEditMAIndicator.Visibility = System.Windows.Visibility.Visible;
            radBusyEditMAIndicator.IsBusy = true;
            cmdRunit.IsEnabled = false;
            dp.BeginExecute(RrunCmd);
        }

       

        private void GridViewMetaStudies_Filtered(object sender, GridViewFilteredEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            
            if (_currentSelectedMetaAnalysis != null)
            {
                if (_currentSelectedMetaAnalysis.FilterSettingsList == null)
                    _currentSelectedMetaAnalysis.FilterSettingsList = new MetaAnalysisFilterSettingList();

                string colName = e.ColumnFilterDescriptor.Column.UniqueName;

                MetaAnalysisFilterSetting setting = _currentSelectedMetaAnalysis.FilterSettingsList.FirstOrDefault(f => f.ColumnName == colName);
                if (setting == null)
                {//we're adding a new filter to a column that didn't have one, so we create the new object and add it to the current MA
                    setting = new MetaAnalysisFilterSetting(_currentSelectedMetaAnalysis.MetaAnalysisId);
                    setting.ColumnName = colName;
                    _currentSelectedMetaAnalysis.FilterSettingsList.Add(setting);
                }
                if (setting != null)
                {
                    setting.Clear();
                    string aggregateVals = "";
                    foreach (object distinctValueObj in e.ColumnFilterDescriptor.DistinctFilter.DistinctValues)
                    {
                        string valStr = distinctValueObj.ToString();
                        aggregateVals += valStr + "{¬}";
                    }
                    if (aggregateVals != "") aggregateVals = aggregateVals.Substring(0, aggregateVals.Length - 3);
                    setting.SelectedValues = aggregateVals;

                    if (e.ColumnFilterDescriptor.FieldFilter.Filter1 != null)
                    {
                        OperatorValueFilterDescriptorBase fVal = e.ColumnFilterDescriptor.FieldFilter.Filter1;
                        if (fVal.Value == null) setting.Filter1 = "";
                        else setting.Filter1 = fVal.Value.ToString();
                        setting.Filter1CaseSensitive = fVal.IsCaseSensitive;
                        switch (fVal.Operator)
                        {
                            case FilterOperator.IsLessThan:
                                setting.Filter1Operator = "IsLessThan";
                                break;
                            case FilterOperator.IsLessThanOrEqualTo:
                                setting.Filter1Operator = "IsLessThanOrEqualTo";
                                break;
                            case FilterOperator.IsEqualTo:
                                setting.Filter1Operator = "IsEqualTo";
                                break;
                            case FilterOperator.IsNotEqualTo:
                                setting.Filter1Operator = "IsNotEqualTo";
                                break;
                            case FilterOperator.IsGreaterThanOrEqualTo:
                                setting.Filter1Operator = "IsGreaterThanOrEqualTo";
                                break;
                            case FilterOperator.IsGreaterThan:
                                setting.Filter1Operator = "IsGreaterThan";
                                break;
                            case FilterOperator.StartsWith:
                                setting.Filter1Operator = "StartsWith";
                                break;
                            case FilterOperator.EndsWith:
                                setting.Filter1Operator = "EndsWith";
                                break;
                            case FilterOperator.Contains:
                                setting.Filter1Operator = "Contains";
                                break;
                            case FilterOperator.DoesNotContain:
                                setting.Filter1Operator = "DoesNotContain";
                                break;
                            case FilterOperator.IsContainedIn:
                                setting.Filter1Operator = "IsContainedIn";
                                break;
                            case FilterOperator.IsNotContainedIn:
                                setting.Filter1Operator = "IsNotContainedIn";
                                break;
                            case FilterOperator.IsNull:
                                setting.Filter1Operator = "IsNull";
                                break;
                            case FilterOperator.IsNotNull:
                                setting.Filter1Operator = "IsNotNull";
                                break;
                            case FilterOperator.IsEmpty:
                                setting.Filter1Operator = "IsEmpty";
                                break;
                            case FilterOperator.IsNotEmpty:
                                setting.Filter1Operator = "IsNotEmpty";
                                break;
                            default:
                                setting.Filter1Operator = "IsNotNull";
                                break;
                        }
                    }

                    if (e.ColumnFilterDescriptor.FieldFilter.LogicalOperator == FilterCompositionLogicalOperator.And)
                    {
                        setting.FiltersLogicalOperator = "And";
                    }
                    else
                    {
                        setting.FiltersLogicalOperator = "Or";
                    }
                    if (e.ColumnFilterDescriptor.FieldFilter.Filter2 != null)
                    {
                        OperatorValueFilterDescriptorBase fVal = e.ColumnFilterDescriptor.FieldFilter.Filter2;
                        if (fVal.Value == null) setting.Filter2 = "";
                        else setting.Filter2 = fVal.Value.ToString();
                        setting.Filter2CaseSensitive = fVal.IsCaseSensitive;
                        switch (fVal.Operator)
                        {
                            case FilterOperator.IsLessThan:
                                setting.Filter2Operator = "IsLessThan";
                                break;
                            case FilterOperator.IsLessThanOrEqualTo:
                                setting.Filter2Operator = "IsLessThanOrEqualTo";
                                break;
                            case FilterOperator.IsEqualTo:
                                setting.Filter2Operator = "IsEqualTo";
                                break;
                            case FilterOperator.IsNotEqualTo:
                                setting.Filter2Operator = "IsNotEqualTo";
                                break;
                            case FilterOperator.IsGreaterThanOrEqualTo:
                                setting.Filter2Operator = "IsGreaterThanOrEqualTo";
                                break;
                            case FilterOperator.IsGreaterThan:
                                setting.Filter2Operator = "IsGreaterThan";
                                break;
                            case FilterOperator.StartsWith:
                                setting.Filter2Operator = "StartsWith";
                                break;
                            case FilterOperator.EndsWith:
                                setting.Filter2Operator = "EndsWith";
                                break;
                            case FilterOperator.Contains:
                                setting.Filter2Operator = "Contains";
                                break;
                            case FilterOperator.DoesNotContain:
                                setting.Filter2Operator = "DoesNotContain";
                                break;
                            case FilterOperator.IsContainedIn:
                                setting.Filter2Operator = "IsContainedIn";
                                break;
                            case FilterOperator.IsNotContainedIn:
                                setting.Filter2Operator = "IsNotContainedIn";
                                break;
                            case FilterOperator.IsNull:
                                setting.Filter2Operator = "IsNull";
                                break;
                            case FilterOperator.IsNotNull:
                                setting.Filter2Operator = "IsNotNull";
                                break;
                            case FilterOperator.IsEmpty:
                                setting.Filter2Operator = "IsEmpty";
                                break;
                            case FilterOperator.IsNotEmpty:
                                setting.Filter2Operator = "IsNotEmpty";
                                break;
                            default:
                                setting.Filter2Operator = "IsNotNull";
                                break;
                        }
                    }
                    if (setting.IsClear)
                    {//there is no meaningful setting: delete.
                        setting.Delete();
                    }
                    _currentSelectedMetaAnalysis.DoMarkDirty();
                }
            }
            SelectSelectable();
        }

        private void cbModel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            if (_currentSelectedMetaAnalysis != null && cbModel.SelectedIndex >= 0) _currentSelectedMetaAnalysis.StatisticalModel = cbModel.SelectedIndex;
            EnableDisableKNHA();
        }
        private void cbOutputType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            if (_currentSelectedMetaAnalysis != null && cbOutputType.SelectedIndex >= 0) _currentSelectedMetaAnalysis.Verbose = cbOutputType.SelectedIndex;
        }
        private void ComboBoxNetMetaOutcomeType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            if (_currentSelectedMetaAnalysis != null && ComboBoxNetMetaOutcomeType.SelectedIndex >= 0) _currentSelectedMetaAnalysis.NMAStatisticalModel = ComboBoxNetMetaOutcomeType.SelectedIndex;
        }
        private void EnableDisableKNHA()
        {
            if (cbModel != null && cbModel.SelectedIndex == 0)
            {
                cbKNHA.IsChecked = false;
                cbKNHA.IsEnabled = false;
            }
            else
            {
                if (cbKNHA != null)
                    cbKNHA.IsEnabled = true;
            }
        }

        private void cmdExportGrid_Click(object sender, RoutedEventArgs e)
        {
            string extension = "";
            ExportFormat format = ExportFormat.Html;

            RadComboBoxItem comboItem = ComboBoxExportOutcomes.SelectedItem as RadComboBoxItem;
            string selectedItem = comboItem.Content.ToString();

            switch (selectedItem)
            {
                case "Excel": extension = "xls";
                    format = ExportFormat.Html;
                    break;
                case "ExcelML": extension = "xml";
                    format = ExportFormat.ExcelML;
                    break;
                case "Word": extension = "doc";
                    format = ExportFormat.Html;
                    break;
                case "Csv": extension = "csv";
                    format = ExportFormat.Csv;
                    break;
            }    

            SaveFileDialog dialog = new SaveFileDialog();
			dialog.DefaultExt = extension;
            dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, selectedItem);
			dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                using (Stream stream = dialog.OpenFile())
                {
                    // a bit of a hack to make the IsSelected field export properly!!
                    GridViewMetaStudies.Columns[0].IsVisible = true;
                    GridViewMetaStudies.Columns[1].IsVisible = false;
                    (GridViewMetaStudies.Columns["ESColumn"] as GridViewDataColumn).DataFormatString = "";
                    (GridViewMetaStudies.Columns["SEESColumn"] as GridViewDataColumn).DataFormatString = "";
                    GridViewExportOptions exportOptions = new GridViewExportOptions();
                    exportOptions.Format = format;
                    exportOptions.ShowColumnHeaders = true;
                    GridViewMetaStudies.Export(stream, exportOptions);
                    (GridViewMetaStudies.Columns["ESColumn"] as GridViewDataColumn).DataFormatString = "{0:F2}";
                    (GridViewMetaStudies.Columns["SEESColumn"] as GridViewDataColumn).DataFormatString = "{0:F2}";
                    GridViewMetaStudies.Columns[0].IsVisible = false;
                    GridViewMetaStudies.Columns[1].IsVisible = true;
                }
            }
        }

        private void GridViewMetaStudies_Sorted(object sender, GridViewSortedEventArgs e)
        {
            GridViewDataColumn col = e.Column as GridViewDataColumn;
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            switch (col.SortingState)
            {
                case SortingState.Ascending:
                    _currentSelectedMetaAnalysis.SortDirection = "Ascending";
                    _currentSelectedMetaAnalysis.SortedBy = col.DataMemberBinding.Path.Path;
                    break;

                case SortingState.Descending:
                    _currentSelectedMetaAnalysis.SortDirection = "Descending";
                    _currentSelectedMetaAnalysis.SortedBy = col.DataMemberBinding.Path.Path;
                    break;

                default:
                    _currentSelectedMetaAnalysis.SortDirection = "";
                    _currentSelectedMetaAnalysis.SortedBy = "";
                    break;
            }
        }

        private void cmdShowModerators_Click(object sender, RoutedEventArgs e)
        {
            if (GridViewModerators.Visibility == System.Windows.Visibility.Collapsed)
            {
                OutcomesAndModeratorsGrid.ColumnDefinitions[1].Width = new GridLength(400);
                GridViewModerators.Visibility = System.Windows.Visibility.Visible;
                tbShowModerators.Text = "Hide moderators";
            }
            else
            {
                OutcomesAndModeratorsGrid.ColumnDefinitions[1].Width = new GridLength(0);
                GridViewModerators.Visibility = System.Windows.Visibility.Collapsed;
                tbShowModerators.Text = "Show moderators";
                MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
                foreach (MetaAnalysisModerator mam in _currentSelectedMetaAnalysis.MetaAnalysisModerators)
                {
                    mam.IsSelected = false;
                }
            }
        }

        private void cbRefValue_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MetaAnalysisModerator mam = (sender as ComboBox).DataContext as MetaAnalysisModerator;
            if (mam != null)
            {
                mam.Reference = ((sender as ComboBox).SelectedItem as MetaAnalysisModeratorReference).Name;
            }
        }

        private void TabControlAnalyses_SelectionChanged(object sender, RadSelectionChangedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            _currentSelectedMetaAnalysis.AnalysisType = TabControlAnalyses.SelectedIndex;
            if (TabControlAnalyses.SelectedIndex == 3)
            {
                DetailsGridRow.Height = new GridLength(1, GridUnitType.Star);
                OutcomesGridRow.Height = new GridLength(30);
                cmdPreviewGRADE.Visibility = Visibility.Visible;
                cmdRunit.Visibility = Visibility.Collapsed;
                cmdExportGrid.Visibility = Visibility.Collapsed;
                ComboBoxExportOutcomes.Visibility = Visibility.Collapsed;
                cmdShowModerators.Visibility = Visibility.Collapsed;
                cmdAddColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                cmdPreviewGRADE.Visibility = Visibility.Collapsed;
                DetailsGridRow.Height = new GridLength(300);
                OutcomesGridRow.Height = new GridLength(1, GridUnitType.Star);
                cmdRunit.Visibility = Visibility.Visible;
                cmdExportGrid.Visibility = Visibility.Visible;
                ComboBoxExportOutcomes.Visibility = Visibility.Visible;
                cmdShowModerators.Visibility = Visibility.Visible;
                cmdAddColumn.Visibility = Visibility.Visible;
            }
        }

        private void ComboBoxNMAOutcomeType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBoxMetaOutcomeType.SelectedIndex = ComboBoxNMAOutcomeType.SelectedIndex;
            if (ComboBoxNMAOutcomeType.SelectedIndex == 2)
            {
                cbExponentiated.Visibility = Visibility.Visible;
            }
            else
            {
                cbExponentiated.Visibility = Visibility.Collapsed;
            }
            SelectSelectable();
        }

        private void cbNMAReferences_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbNMAReferences.SelectedItem != null)
            {
                MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
                _currentSelectedMetaAnalysis.NMAReference = cbNMAReferences.SelectedValue.ToString();
            }
        }

        private void cbModeratorSelected_Checked(object sender, RoutedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            bool OneSelected = false;
            if ((e.OriginalSource as CheckBox).IsChecked == true)
            {
                OneSelected = true;
            }
            else
            {
                OneSelected = false;
            }
            foreach (MetaAnalysisModerator mam in _currentSelectedMetaAnalysis.MetaAnalysisModerators)
            {
                if (mam.IsSelected && mam != (e.OriginalSource as CheckBox).DataContext) OneSelected = true;
            }
            
            if (OneSelected)
            {
                cbTrimFill.IsChecked = false;
                cbTrimFill.IsEnabled = false;
            }
            else
            {
                cbTrimFill.IsEnabled = true;
            }
        }

        private void cmdPreviewGRADE_Click(object sender, RoutedEventArgs e)
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            if (_currentSelectedMetaAnalysis != null)
            {
                reports.SetContent(_currentSelectedMetaAnalysis.GRADEReport());
                windowReportsDocuments.ShowDialog();
            }
        }

        private void RadWindow_ActuallyClose()
        {
            MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            if (_currentSelectedMetaAnalysis != null)
            {
                if (_currentSelectedMetaAnalysis.IsDirty)
                {
                    if (_currentSelectedMetaAnalysis.IsNew)
                    {
                        RadWindow.Confirm("You have unsaved Changes! (Current MA is New)"
                            + Environment.NewLine + "Closing the window will discard all changes. Continue?"
                            , OnConfrimClosing_MAisNew);
                    }
                    else
                    {
                        RadWindow.Confirm("You have unsaved Changes!"
                            + Environment.NewLine + "Do you want to discard all changes?"
                            , OnConfrimClosing_MAisDirty);
                    }
                }
            }
            else
            {
                //nothing we can do: no MA to look into, can't make any decision. We'll just close the window
            }

            CslaDataProvider maldProvider = ((CslaDataProvider)App.Current.Resources["MetaAnalysisListData"]);
            if (maldProvider != null) maldProvider.Rebind();
            //we close the MA window here, otherwise the confirm dialog might appear beneath it
            this.Close();
        }
        private void OnConfrimClosing_MAisNew(object sender, WindowClosedEventArgs e)
        {
            //MA is new and not present in the underlying list. We either NOT close the window or close it and lose the new MA
            var result = e.DialogResult;
            if (result == true)
            {// user wants to discard changes to a new MA
                //MA was NOT saved and NOT added to the list of MAs, so we don't need to do anything, it will be lost to the GC
            }
            else if (result == false)
            {
                //we simply cancel the closing by re-opening
                dialogMetaAnalysisSetupWindow.ShowDialog();
            }
        }
        private void OnConfrimClosing_MAisDirty(object sender, WindowClosedEventArgs e)
        {
            //MA is NOT new. We either refresh the underlying list, losing the changes, or we close without refreshing, which allows to save the changes later
            var result = e.DialogResult;
            if (result == true)
            {
                //the changed object is in the main list, and we don't know how to undo the changes.
                //so we need to refresh the list of MAs
                CslaDataProvider maldProvider = ((CslaDataProvider)App.Current.Resources["MetaAnalysisListData"]);
                maldProvider.Refresh();
            }
            else if (result == false)
            {
                //we close without re-fetching the list of MAs
                //dialogMetaAnalysisSetupWindow.ShowDialog();
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            //MetaAnalysis _currentSelectedMetaAnalysis = this.DataContext as MetaAnalysis;
            //bool check = _currentSelectedMetaAnalysis.IsSavable;
            RadWindow_ActuallyClose();
        }
        private void OutcomeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            MetaAnalysis MA = this.DataContext as MetaAnalysis;
            if (MA != null) MA.DoMarkDirty();
        }

        private void dialogMetaAnalysisSetupWindow_WindowStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Maximized) this.WindowState = WindowState.Maximized;

        }
    }





    // ****************************************************************************************************
    // Derived from Telerik examples for saving the filters etc of the gridview
    // **************************************************************************************

    public class ColumnProxy
    {
        public string UniqueName { get; set; }
        public int DisplayOrder { get; set; }
        public string Header { get; set; }
        public GridViewLength Width { get; set; }
    }

    public class SortDescriptorProxy
    {
        public string ColumnUniqueName { get; set; }
        public ListSortDirection SortDirection { get; set; }
    }

    public class GroupDescriptorProxy
    {
        public string ColumnUniqueName { get; set; }
        public ListSortDirection? SortDirection { get; set; }
    }

    public class FilterDescriptorProxy
    {
        public Telerik.Windows.Data.FilterOperator Operator { get; set; }
        public object Value { get; set; }
        public bool IsCaseSensitive { get; set; }
    }

    public class FilterSetting
    {
        public string ColumnUniqueName { get; set; }

        private List<object> selectedDistinctValue;
        public List<object> SelectedDistinctValues
        {
            get
            {
                if (this.selectedDistinctValue == null)
                {
                    this.selectedDistinctValue = new List<object>();
                }

                return this.selectedDistinctValue;
            }
        }

        public FilterDescriptorProxy Filter1 { get; set; }
        public Telerik.Windows.Data.FilterCompositionLogicalOperator FieldFilterLogicalOperator { get; set; }
        public FilterDescriptorProxy Filter2 { get; set; }
    }

    // **************************************************************************************

    public class GridViewCustomPropertyProvider : ICustomPropertyProvider
    {
        public CustomPropertyInfo[] GetCustomProperties()
        {
            // Create three custom properties to persist the Columns, Sorting and Group descriptors using proxy objects
            return new CustomPropertyInfo[]
            {
                //new CustomPropertyInfo("Columns", typeof(List<ColumnProxy>)),
                new CustomPropertyInfo("SortDescriptors", typeof(List<SortDescriptorProxy>)),
                //new CustomPropertyInfo("GroupDescriptors", typeof(List<GroupDescriptorProxy>)),
                new CustomPropertyInfo("FilterDescriptors", typeof(List<FilterSetting>)),
            };
        }

        public void InitializeObject(object context)
        {
            if (context is RadGridView)
            {
                RadGridView gridView = context as RadGridView;
                gridView.SortDescriptors.Clear();
                gridView.GroupDescriptors.Clear();
                gridView.Columns
                    .OfType<GridViewColumn>()
                    .Where(c => c.ColumnFilterDescriptor.IsActive)
                    .ToList().ForEach(c => c.ClearFilters());
            }
        }

        public object InitializeValue(CustomPropertyInfo customPropertyInfo, object context)
        {
            return null;
        }

        public object ProvideValue(CustomPropertyInfo customPropertyInfo, object context)
        {
            RadGridView gridView = context as RadGridView;

            switch (customPropertyInfo.Name)
            {
                case "Columns":
                    {
                        List<ColumnProxy> columnProxies = new List<ColumnProxy>();

                        foreach (GridViewColumn column in gridView.Columns)
                        {
                            columnProxies.Add(new ColumnProxy()
                            {
                                UniqueName = column.UniqueName,
                                Header = column.Header.ToString(),
                                DisplayOrder = column.DisplayIndex,
                                Width = column.Width,
                            });
                        }

                        return columnProxies;
                    }

                case "SortDescriptors":
                    {
                        List<SortDescriptorProxy> sortDescriptorProxies = new List<SortDescriptorProxy>();

                        foreach (ColumnSortDescriptor descriptor in gridView.SortDescriptors)
                        {
                            sortDescriptorProxies.Add(new SortDescriptorProxy()
                            {
                                ColumnUniqueName = descriptor.Column.UniqueName,
                                SortDirection = descriptor.SortDirection,
                            });
                        }

                        return sortDescriptorProxies;
                    }

                case "GroupDescriptors":
                    {
                        List<GroupDescriptorProxy> groupDescriptorProxies = new List<GroupDescriptorProxy>();

                        foreach (ColumnGroupDescriptor descriotor in gridView.GroupDescriptors)
                        {
                            groupDescriptorProxies.Add(new GroupDescriptorProxy()
                            {
                                ColumnUniqueName = descriotor.Column.UniqueName,
                                SortDirection = descriotor.SortDirection,
                            });
                        }

                        return groupDescriptorProxies;
                    }

                case "FilterDescriptors":
                    {
                        List<FilterSetting> filterSettings = new List<FilterSetting>();

                        foreach (IColumnFilterDescriptor columnFilter in gridView.FilterDescriptors)
                        {
                            FilterSetting columnFilterSetting = new FilterSetting();

                            columnFilterSetting.ColumnUniqueName = columnFilter.Column.UniqueName;

                            columnFilterSetting.SelectedDistinctValues.AddRange(columnFilter.DistinctFilter.DistinctValues);

                            if (columnFilter.FieldFilter.Filter1.IsActive)
                            {
                                columnFilterSetting.Filter1 = new FilterDescriptorProxy();
                                columnFilterSetting.Filter1.Operator = columnFilter.FieldFilter.Filter1.Operator;
                                columnFilterSetting.Filter1.Value = columnFilter.FieldFilter.Filter1.Value;
                                columnFilterSetting.Filter1.IsCaseSensitive = columnFilter.FieldFilter.Filter1.IsCaseSensitive;
                            }

                            columnFilterSetting.FieldFilterLogicalOperator = columnFilter.FieldFilter.LogicalOperator;

                            if (columnFilter.FieldFilter.Filter2.IsActive)
                            {
                                columnFilterSetting.Filter2 = new FilterDescriptorProxy();
                                columnFilterSetting.Filter2.Operator = columnFilter.FieldFilter.Filter2.Operator;
                                columnFilterSetting.Filter2.Value = columnFilter.FieldFilter.Filter2.Value;
                                columnFilterSetting.Filter2.IsCaseSensitive = columnFilter.FieldFilter.Filter2.IsCaseSensitive;
                            }

                            filterSettings.Add(columnFilterSetting);
                        }

                        return filterSettings;
                    }
            }

            return null;
        }

        public void RestoreValue(CustomPropertyInfo customPropertyInfo, object context, object value)
        {
            RadGridView gridView = context as RadGridView;

            switch (customPropertyInfo.Name)
            {
                    /*
                case "Columns":
                    {
                        List<ColumnProxy> columnProxies = value as List<ColumnProxy>;

                        foreach (ColumnProxy proxy in columnProxies)
                        {
                            GridViewColumn column = gridView.Columns[proxy.UniqueName];
                            column.DisplayIndex = proxy.DisplayOrder;
                            column.Header = proxy.Header;
                            column.Width = proxy.Width;
                        }
                    }
                    break;
                */
                case "SortDescriptors":
                    {
                        gridView.SortDescriptors.SuspendNotifications();

                        gridView.SortDescriptors.Clear();

                        List<SortDescriptorProxy> sortDescriptoProxies = value as List<SortDescriptorProxy>;

                        foreach (SortDescriptorProxy proxy in sortDescriptoProxies)
                        {
                            GridViewColumn column = gridView.Columns[proxy.ColumnUniqueName];
                            gridView.SortDescriptors.Add(new ColumnSortDescriptor() { Column = column, SortDirection = proxy.SortDirection });
                        }

                        gridView.SortDescriptors.ResumeNotifications();
                    }
                    break;
                    /*
                case "GroupDescriptors":
                    {
                        gridView.GroupDescriptors.SuspendNotifications();

                        gridView.GroupDescriptors.Clear();

                        List<GroupDescriptorProxy> groupDescriptorProxies = value as List<GroupDescriptorProxy>;

                        foreach (GroupDescriptorProxy proxy in groupDescriptorProxies)
                        {
                            GridViewColumn column = gridView.Columns[proxy.ColumnUniqueName];
                            gridView.GroupDescriptors.Add(new ColumnGroupDescriptor() { Column = column, SortDirection = proxy.SortDirection });
                        }

                        gridView.GroupDescriptors.ResumeNotifications();
                    }
                    break;
                    */
                case "FilterDescriptors":
                    {
                        gridView.FilterDescriptors.SuspendNotifications();

                        foreach (var c in gridView.Columns)
                        {
                            if (c.ColumnFilterDescriptor.IsActive)
                            {
                                c.ClearFilters();
                            }
                        }

                        List<FilterSetting> filterSettings = value as List<FilterSetting>;

                        foreach (FilterSetting setting in filterSettings)
                        {
                            Telerik.Windows.Controls.GridViewColumn column = gridView.Columns[setting.ColumnUniqueName];

                            Telerik.Windows.Controls.GridView.IColumnFilterDescriptor columnFilter = column.ColumnFilterDescriptor;

                            foreach (object distinctValue in setting.SelectedDistinctValues)
                            {
                                columnFilter.DistinctFilter.AddDistinctValue(distinctValue);
                            }

                            if (setting.Filter1 != null)
                            {
                                columnFilter.FieldFilter.Filter1.Operator = setting.Filter1.Operator;
                                columnFilter.FieldFilter.Filter1.Value = setting.Filter1.Value;
                                columnFilter.FieldFilter.Filter1.IsCaseSensitive = setting.Filter1.IsCaseSensitive;
                            }

                            columnFilter.FieldFilter.LogicalOperator = setting.FieldFilterLogicalOperator;

                            if (setting.Filter2 != null)
                            {
                                columnFilter.FieldFilter.Filter2.Operator = setting.Filter2.Operator;
                                columnFilter.FieldFilter.Filter2.Value = setting.Filter2.Value;
                                columnFilter.FieldFilter.Filter2.IsCaseSensitive = setting.Filter2.IsCaseSensitive;
                            }
                        }

                        gridView.FilterDescriptors.ResumeNotifications();
                    }
                    break;
            }
        }
    }
    
}
