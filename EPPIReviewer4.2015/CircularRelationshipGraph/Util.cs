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

namespace CircularRelationshipGraph
{
  public static class Util
  {

    private static readonly double factor = 2 * Math.PI / 360;


    public static Point RadialToCartesian(double angle, double radius, Point centre)
    {
      return new Point()
      {
        X = Math.Sin(angle * factor) * radius + centre.X,
        Y = Math.Cos(angle * factor) * radius + centre.Y
      };
    }


    /// <summary>
    /// Swaps the items at the given indices
    /// </summary>
    public static void Swap<T>(this List<T> items, int from, int to)
    {
      T itemOne = items[from];
      T itemTwo = items[to];
      items[from] = itemTwo;
      items[to] = itemOne;
    }

  }
}
