using System;
using System.Net;
using System.Linq;
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
  /// A sort provider that orders the nodes via the given delegate
  /// </summary>
  public class DelegateSortOrderProvider : ISortOrderProvider
  {
    private Func<IList<INode>, IEnumerable<INode>> _func;

    public DelegateSortOrderProvider(Func<IList<INode>, IEnumerable<INode>> func)
    {
      _func = func;
    }

    public INodeList Sort(INodeList nodes)
    {
      return new NodeList(_func(nodes));
    }
  }
}
