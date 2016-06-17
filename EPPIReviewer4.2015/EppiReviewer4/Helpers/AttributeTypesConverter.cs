﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Globalization;
using BusinessLibrary.BusinessClasses;

namespace EppiReviewer4
{
    public class AttributeTypesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Csla.Xaml.CslaDataProvider provider = parameter as Csla.Xaml.CslaDataProvider;

            if (value != null && provider != null && provider.Data != null)
            {
                AttributeTypes attributeTypeList = provider.Data as AttributeTypes;
                return attributeTypeList.GetItemByKey((int)value);
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Csla.Xaml.CslaDataProvider provider = parameter as Csla.Xaml.CslaDataProvider;

            if (value != null && provider != null && provider.Data != null)
            {
                var returnValue = ((AttributeTypes.NameValuePair)value);
                if (returnValue != null)
                    return returnValue.Key;
                else
                    return 0;
            }
            else
                return 0;
        }
    }
}
