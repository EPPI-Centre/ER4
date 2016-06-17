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

namespace CircularRelationshipGraph.Data
{
  public class NodeRelationship : INodeRelationship
  {
    private string _to;

    private double _strength;
    
    public string To
    {
      get { return _to; }
      set { _to = value; }
    }

    public double Strength
    {
      get { return _strength; }
      set { _strength = value; }
    }
  }
}
