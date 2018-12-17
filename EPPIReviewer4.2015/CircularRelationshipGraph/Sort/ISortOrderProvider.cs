
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
using System.Collections.Generic;
using CircularRelationshipGraph.Data;

namespace CircularRelationshipGraph
{
  /// <summary>
  /// Takes a list of nodes and sorts them.
  /// </summary>
  public interface ISortOrderProvider
  {
    INodeList Sort(INodeList nodes);
  }
}
