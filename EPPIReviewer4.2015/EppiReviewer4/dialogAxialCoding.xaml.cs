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
using Csla.Silverlight;
using Csla;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using Telerik.Windows.Input;
using Telerik.Windows;
using Telerik.Windows.Documents.FormatProviders.Txt;


namespace EppiReviewer4
{
    public partial class dialogAxialCoding : UserControl
    {
        public dialogAxialCoding()
        {
            InitializeComponent();
            this.Loaded += new System.Windows.RoutedEventHandler(ThisControl_Loaded);
        }

        private ReadOnlyAttributeTextAllItemsList list = null;
        private ItemDocument CurrentTextDocument;

        private void ThisControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TreeControl.BindCodes();
            TreeControl.rich = rich;
            list = null;
            CurrentTextDocument = null;
            TreeControl.ShowCodeTxtOptions = true;
            //if (rich.RichControl != null)
            //    rich.RichControl.Text = "";
            if (rich.Document == null) rich.Document = new Telerik.Windows.Documents.Model.RadDocument();
            AttributeSet attributeSet = this.DataContext as AttributeSet;
            if (attributeSet != null)
            {
                DataPortal<ReadOnlyAttributeTextAllItemsList> dp = new DataPortal<ReadOnlyAttributeTextAllItemsList>();
                dp.FetchCompleted += (o, e2) =>
                {
                    BusyLoading.IsRunning = false;
                    if (e2.Error != null)
                    {
                        MessageBox.Show(e2.Error.Message);
                    }
                    else
                    {
                        TextBlockAttributeName.Text = "Text coded with: " + attributeSet.AttributeName;
                        list = e2.Object;
                        string report = "<html><body><p><h3>Items coded with: <i>" + attributeSet.AttributeName + "</i></h3>";
                        Int64 CurrentItemDocumentID = 0;
                        Int64 CurrentItemId = 0;
                        RadTreeViewItem CurrentItem = null;
                        RadTreeViewItem CurrentDocument = null;
                        TreeViewDocuments.Items.Clear();
                        foreach (ReadOnlyAttributeTextAllItems textItem in list)
                        {
                            if (textItem.ItemId != CurrentItemId)
                            {
                                CurrentItem = new RadTreeViewItem();
                                CurrentItem.Header = textItem.ItemTitle;
                                //CurrentItem.Foreground = new SolidColorBrush(Colors.Blue);
                                TreeViewDocuments.Items.Add(CurrentItem);
                                CurrentItemId = textItem.ItemId;
                            }
                            if (textItem.ItemDocumentId != CurrentItemDocumentID)
                            {
                                CurrentDocument = new RadTreeViewItem();
                                CurrentDocument.Header = textItem.ItemDocumentTitle;
                                //CurrentDocument.Foreground = new SolidColorBrush(Colors.LightGray);
                                CurrentItem.Items.Add(CurrentDocument);
                                CurrentItemDocumentID = textItem.ItemDocumentId;
                            }
                            RadTreeViewItem tvi = new RadTreeViewItem();
                            
                            tvi.Header = textItem.Snippet;
                            //tvi.Foreground = new SolidColorBrush(Colors.DarkGray);
                            tvi.Tag = textItem;
                            CurrentDocument.Items.Add(tvi);
                        }
                    }
                };
                BusyLoading.IsRunning = true;
                dp.BeginFetch(new SingleCriteria<ReadOnlyAttributeTextAllItemsList, Int64>(attributeSet.AttributeSetId));
            }
        }

        private void TreeViewDocuments_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            Telerik.Windows.Controls.TreeView.EditableHeaderedItemsControl SelI = TreeViewDocuments.SelectedItem as Telerik.Windows.Controls.TreeView.EditableHeaderedItemsControl;
            //ReadOnlyAttributeTextAllItems o = TreeViewDocuments.SelectedItem as ReadOnlyAttributeTextAllItems;
            if (SelI != null)
            {
                ReadOnlyAttributeTextAllItems textItem = SelI.Tag as ReadOnlyAttributeTextAllItems;
                if (textItem != null)
                {
                    bool LoadItemData = false;
                    if (CurrentTextDocument == null)
                    {
                        CurrentTextDocument = new ItemDocument();
                        LoadItemData = true;
                    }
                    else
                    {
                        if (textItem.ItemId != CurrentTextDocument.ItemId)
                        {
                            LoadItemData = true;
                        }
                    }
                    CurrentTextDocument.ItemDocumentId = textItem.ItemDocumentId;
                    CurrentTextDocument.ItemId = textItem.ItemId;
                    CurrentTextDocument.Text = textItem.CodedText;
                    CurrentTextDocument.TextFrom = textItem.TextFrom;
                    CurrentTextDocument.TextTo = textItem.TextTo;
                    CurrentTextDocument.Title = textItem.ItemDocumentTitle;
                    //rich.RichControl.Text = CurrentTextDocument.Text;
                    TxtFormatProvider provider = new TxtFormatProvider();
                    rich.Document = provider.Import(CurrentTextDocument == null ? "" : CurrentTextDocument.Text);
                    TextBlockDocDetails.Text = "Document: " + textItem.ItemTitle;
                    if (LoadItemData == true)
                    {
                        TreeControl.BindItemIdAndDocument(textItem.ItemId, CurrentTextDocument);
                    }
                    else
                    {
                        TreeControl.CurrentTextDocument = CurrentTextDocument;
                    }
                }
            }
        }

        

       
    }
}
