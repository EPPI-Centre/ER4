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
using Csla.Xaml;

namespace EppiReviewer4
{
    public partial class dialogLinkedItemDetail : UserControl
    {
        public dialogLinkedItemDetail()
        {
            InitializeComponent();
        }

        public void BindItem(ItemLink i)
        {
            CslaDataProvider provider = this.Resources["ItemData"] as CslaDataProvider;
            provider.FactoryParameters.Clear();
            provider.FactoryParameters.Add(i.ItemIdSecondary);
            provider.FactoryMethod = "GetItem";
            provider.Refresh();
        }

        private void CslaDataProvider_DataChanged(object sender, EventArgs e)
        {
            CslaDataProvider provider = ((CslaDataProvider)this.Resources["ItemData"]);
            if (provider.Error != null)
                Telerik.Windows.Controls.RadWindow.Alert(((Csla.Xaml.CslaDataProvider)sender).Error.Message);
            else
            {
                Item i = provider.Data as Item;
                if (i != null)
                {
                    this.DataContext = provider.Data;
                    dialogLinkedItemDetailsControl.BindTree(i);
                }
            }
        }
    }
}
