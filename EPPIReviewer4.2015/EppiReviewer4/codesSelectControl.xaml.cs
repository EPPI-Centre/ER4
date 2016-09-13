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
using Telerik.Windows.Controls;

namespace EppiReviewer4
{
    public partial class codesSelectControl : UserControl
    {
        private bool IsForCodesOnly = true;
        private bool IsForSetsOnly = false;
        private bool IsMultipleSelect = false;
        private Int64 lastID = -1;
        public event EventHandler<Telerik.Windows.Controls.SelectionChangedEventArgs> SelectCode_SelectionChanged;
        public AttributeSetList selectedAttributes;

        public codesSelectControl()
        {
            InitializeComponent();
            selectedAttributes = new AttributeSetList();
        }

        public void SetMode(bool AllowSelectingSets, bool AllowSelectingCodes, bool isRandomAllocationTarget)
        {
            if (AllowSelectingCodes && !AllowSelectingSets)
            {
                IsForSetsOnly = false;
                IsForCodesOnly = true;
            }
            else if (AllowSelectingSets && !AllowSelectingCodes)
            {
                IsForSetsOnly = true;
                IsForCodesOnly = false;
            } 
            else //could be that it doesn't allow to select anything, but in that case, why show the control??
            {
                IsForCodesOnly = false;
                IsForSetsOnly = false;
            }
            if (isRandomAllocationTarget)
            {
                Csla.Xaml.CslaDataProvider Provider = App.Current.Resources["SetTypes"] as Csla.Xaml.CslaDataProvider;
                ReadOnlySetTypeList rostl = (Provider.Data as ReadOnlySetTypeList);
                if (Provider != null && rostl != null && rostl.Count > 0)
                {//ListOfSetTypes is used to bind the visibility of Sets in the random allocation code, we need access to the list of code-sets type from within XAML 
                    Provider_DataChanged(null, null);
                }
                else
                {
                    Provider.DataChanged += new EventHandler(Provider_DataChanged);
                }
                
                //Csla.Xaml.CslaDataProvider Provider = App.Current.Resources["SetTypes"] as Csla.Xaml.CslaDataProvider;
                //ReadOnlySetTypeList rostl = (Provider.Data as ReadOnlySetTypeList);
                //if (Provider != null && rostl != null && rostl.Count > 0)
                //{//ListOfSetTypes is used to bind the visibility of Sets in the random allocation code, we need access to the list of code-sets type from within XAML 
                //    Provider_DataChanged(null, null);
                //}
                //else
                //{
                //    Provider.DataChanged += new EventHandler(Provider_DataChanged);
                //}
            }
        }

        void Provider_DataChanged(object sender, EventArgs e)
        {
            Csla.Xaml.CslaDataProvider Provider = App.Current.Resources["SetTypes"] as Csla.Xaml.CslaDataProvider;
            ReadOnlySetTypeList rostl = (Provider.Data as ReadOnlySetTypeList);
            ListOfSetTypes.ItemsSource = rostl;
            if (ListOfSetTypes.Items.Count > 0) Provider.DataChanged -= Provider_DataChanged;
            TreeViewSelectCode.ItemContainerStyle = TreeViewSelectCode.Resources["4RandomAlloc"] as Style;
        }

        public void SelectionMode(string mode)
        {
            if (mode == "Multiple")
            {
                TreeViewSelectCode.SelectionMode = Telerik.Windows.Controls.SelectionMode.Multiple;
                IsMultipleSelect = true;
            }
            else
            {
                TreeViewSelectCode.SelectionMode = Telerik.Windows.Controls.SelectionMode.Single;
                IsMultipleSelect = false;
            }
        }

        private void TreeViewSelectCode_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TreeViewSelectCode.SelectedItem == null) return;
            selectedAttributes.Clear();

            if (((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) &&
                IsMultipleSelect == true)
            {
                foreach (object o in TreeViewSelectCode.SelectedItems)
                {
                    if (o is AttributeSet)
                    {
                        selectedAttributes.Add(o as AttributeSet);
                    }
                }
            }
            else
            {
                if (IsForCodesOnly && TreeViewSelectCode.SelectedItem is AttributeSet)
                {
                    codeSelectControlDropDown.Content = TreeViewSelectCode.SelectedItem as AttributeSet;
                    codeSelectControlDropDown.IsOpen = false;
                }
                else if (IsForSetsOnly && TreeViewSelectCode.SelectedItem is ReviewSet)
                {
                    codeSelectControlDropDown.Content = TreeViewSelectCode.SelectedItem as ReviewSet;
                    codeSelectControlDropDown.IsOpen = false;
                }

                if (SelectCode_SelectionChanged != null) SelectCode_SelectionChanged.Invoke(sender, e);
            }
        }

        public AttributeSet SelectedAttributeSet()
        {
            return codeSelectControlDropDown.Content as AttributeSet;
        }

        public AttributeSetList SelectedAttributes()
        {
            return selectedAttributes;
        }

        public void ClearSelection()
        {
            codeSelectControlDropDown.Content = null;
            selectedAttributes.Clear();
        }

        public void SelectAttributeSet(Int64 attributeSetId)
        {
            AttributeSet selected = (TreeViewSelectCode.ItemsSource as ReviewSetsList).GetAttributeSet(attributeSetId);
            if (selected != null)
            {
                TreeViewSelectCode.SelectedItem = selected;
            }
        }
        //public void showOnlyOneSetID(int setID)
        //{
        //    foreach (ReviewSet rSet in TreeViewSelectCode.Items)
        //    {
        //        if (rSet.SetId != setID)
        //        {
        //            Telerik.Windows.Controls.RadTreeViewItem rtvi = TreeViewSelectCode.ContainerFromItemRecursive(rSet);
        //            if (rtvi != null) rtvi.Visibility = System.Windows.Visibility.Collapsed;
        //        }
        //    }
        //}
        public void SelectAttributeSetFromAttributeId(Int64 attributeId)
        {
            if ((TreeViewSelectCode.ItemsSource as ReviewSetsList) == null) return;
            AttributeSet selected = (TreeViewSelectCode.ItemsSource as ReviewSetsList).GetAttributeSetFromAttributeId(attributeId);
            if (selected != null)
            {
                TreeViewSelectCode.SelectedItem = selected;
            }
        }
        private void TreeViewSelectCode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectedAttributes.Clear();
            if (((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) &&
                IsMultipleSelect == true)// i.e. we're doing a multiple selection
            {
                foreach (object o in TreeViewSelectCode.SelectedItems)
                {
                    if (o is AttributeSet)
                    {
                        selectedAttributes.Add(o as AttributeSet);
                    }
                }
            }
            else
            {
                if (TreeViewSelectCode.SelectedItem == null || IsForSetsOnly || IsForCodesOnly) return;
                //what follows is only useful when we can select both sets and codes
                Int64 current;
                if (TreeViewSelectCode.SelectedItem is AttributeSet)
                {
                    current = (TreeViewSelectCode.SelectedItem as AttributeSet).AttributeId;
                }
                else
                {
                    current = (TreeViewSelectCode.SelectedItem as ReviewSet).SetId;
                }
                if (current == lastID)
                {//clicked again on the same item, we'll guess that's the one you want and close the dialog
                    if (TreeViewSelectCode.SelectedItem is AttributeSet)
                    {
                        codeSelectControlDropDown.Content = TreeViewSelectCode.SelectedItem as AttributeSet;
                    }
                    else
                    {
                        codeSelectControlDropDown.Content = TreeViewSelectCode.SelectedItem as ReviewSet;
                    }
                    codeSelectControlDropDown.IsOpen = false;
                }
                else lastID = current;
            }
        }
        public void UnhookMe()
        {
            Csla.Xaml.CslaDataProvider Provider = App.Current.Resources["SetTypes"] as Csla.Xaml.CslaDataProvider;
            if (Provider != null)
            {
                Provider.DataChanged -= Provider_DataChanged;
                Provider.DataChanged -= Provider_DataChanged;
            }
        }
    }
}
