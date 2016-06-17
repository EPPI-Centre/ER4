using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using Snippets;

namespace CircularRelationshipGraph
{
  [SnippetINotifyPropertyChanged]
  [SnippetPropertyINPC(property = "Size", field = "_size", type = "Size", defaultValue = "EMPTY_SIZE")]
  [SnippetPropertyINPC(property = "SweepDirection", field = "_dir", type = "SweepDirection", defaultValue = "SweepDirection.Clockwise")]
  public partial class NodeConnectionViewModel : INotifyPropertyChanged
  {
    private static readonly Size EMPTY_SIZE = new Size();

    private static readonly Point EMPTY_POINT = new Point();
  }
}
