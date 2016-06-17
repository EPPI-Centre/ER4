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

namespace CircularRelationshipGraph.Primitives
{
  public class DoubleToVisibilityConverter : IValueConverter
  {

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      double collapsedBound = double.Parse((string)parameter);
      double doubleValue = (double)value;
      return doubleValue > collapsedBound ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
