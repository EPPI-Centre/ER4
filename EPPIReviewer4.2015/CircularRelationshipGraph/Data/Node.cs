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

namespace CircularRelationshipGraph.Data
{
  public class Node : INode
  {
    private double _count;

    private string _text;

    private object _tag;

    private List<INodeRelationship> _relationships;

    public Node()
    {
    }

    public Node(double count, string text, List<INodeRelationship> relationships)
    {
      _count = count;
      _text = text;
      _relationships = relationships;
    }

    public double Count
    {
      get { return _count; }
      set { _count = value; }
    }

    public object Tag
    {
      get { return _tag; }
      set { _tag = value; }
    }

    public string Name
    {
      get { return _text; }
      set { _text = value; }
    }

    public List<INodeRelationship> Relationships
    {
      get { return _relationships; }
      set { _relationships = value; }
    }
  }
}
