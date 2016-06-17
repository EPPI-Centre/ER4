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
using Snippets;
using System.ComponentModel;

namespace CircularRelationshipGraph
{
  [SnippetINotifyPropertyChanged]
  [SnippetPropertyINPC(property = "S1", field = "_s1", type = "Point", defaultValue = "EMPTY_POINT")]
  [SnippetPropertyINPC(property = "S2", field = "_s2", type = "Point", defaultValue = "EMPTY_POINT")]
  [SnippetPropertyINPC(property = "S3", field = "_s3", type = "Point", defaultValue = "EMPTY_POINT")]
  [SnippetPropertyINPC(property = "S4", field = "_s4", type = "Point", defaultValue = "EMPTY_POINT")]
  [SnippetPropertyINPC(property = "LabelLocation", field = "_labelLocation", type = "Point", defaultValue = "EMPTY_POINT")]
  [SnippetPropertyINPC(property = "InnerSize", field = "_innerSize", type = "Size", defaultValue = "EMPTY_SIZE")]
  [SnippetPropertyINPC(property = "OuterSize", field = "_outerSize", type = "Size", defaultValue = "EMPTY_SIZE")]
  [SnippetPropertyINPC(property = "IsLargeArc", field = "_isLargeArc", type = "bool", defaultValue = "false")]
  public partial class NodeSegmentViewModel : INotifyPropertyChanged
  {
    private static readonly Point EMPTY_POINT = new Point();

    private static readonly Size EMPTY_SIZE = new Size();
  }
}
