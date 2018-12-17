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
  /// A sort order provider that doesn't actually perform any sorting.
  /// </summary>
  public class NaturalSortOrderProvider : ISortOrderProvider
  {
    public INodeList Sort(INodeList nodes)
    {
      return nodes;
    }
  }
}
