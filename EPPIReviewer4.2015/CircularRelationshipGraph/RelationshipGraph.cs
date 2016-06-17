using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Snippets;
using CircularRelationshipGraph.Data;
using System.Windows.Data;
using System.Diagnostics;

namespace CircularRelationshipGraph
{
  /// <summary>
  /// A graph that renders the relationships between a list of nodes.
  /// </summary>
  [SnippetDependencyProperty(property = "Data", defaultValue = "null",
                             type = "INodeList", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "SegmentFillInterpolator", defaultValue = "null",
                             type = "Interpolator", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "ConnectorFillInterpolator", defaultValue = "null",
                             type = "Interpolator", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "NodeSegmentStyle", defaultValue = "null",
                             type = "Style", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "NodeConnectorStyle", defaultValue = "null",
                             type = "Style", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "ConnectorThickness", defaultValue = "null",
                             type = "DoubleRange", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "InnerRadius", defaultValue = "0.7",
                             type = "double", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "OuterRadius", defaultValue = "0.8",
                             type = "double", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "LabelRadius", defaultValue = "0.9",
                             type = "double", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "HighlightedNode", 
                             type = "INode", containerType = "RelationshipGraph")]
  [SnippetDependencyProperty(property = "SortOrderProvider", defaultValue = "new NaturalSortOrderProvider()",
                             type = "ISortOrderProvider", containerType = "RelationshipGraph")]
  public partial class RelationshipGraph : Control
  {

    #region fields

    private Panel _graphContainer;

    private Dictionary<INode, NodeSegment> _segmentForNode = new Dictionary<INode, NodeSegment>();

    #endregion

    #region DP change handlers

    partial void OnDataPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Render();
    }

    partial void OnInnerRadiusPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Render();
    }

    partial void OnConnectorFillInterpolatorPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Render();
    }

    partial void OnConnectorThicknessPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Render();
    }

    partial void OnLabelRadiusPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Render();
    }

    partial void OnNodeConnectorStylePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Render();
    }

    partial void OnNodeSegmentStylePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Render();
    }

    partial void OnOuterRadiusPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Render();
    }

    partial void OnSegmentFillInterpolatorPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      Render();
    }

    partial void OnSortOrderProviderPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      var sortedData = SortOrderProvider.Sort(Data);
      AnimateToOrder(sortedData);
    }

    #endregion

    public RelationshipGraph()
    {
      this.DefaultStyleKey = typeof(RelationshipGraph);

      this.SizeChanged += new SizeChangedEventHandler(RelationshipGraph_SizeChanged);
    }

    private void RelationshipGraph_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      Render();
    }

    public override void OnApplyTemplate()
    {
      _graphContainer = this.GetTemplateChild("graphContainer") as Panel;

      Render();
    }

    /// <summary>
    /// Renders the relationship graph
    /// </summary>
    private void Render()
    {
      if (_graphContainer == null ||
          double.IsNaN(ActualWidth) || double.IsNaN(ActualHeight) ||
          ActualHeight == 0.0 || ActualWidth == 0.0)
        return;

      // clear the UI
      _segmentForNode.Clear();
      _graphContainer.Children.Clear();

      if (Data == null || Data.Count == 0)
        return;

      // sort the nodes
      var sortedData = SortOrderProvider.Sort(Data);

      // compute the various radii
      double minDimension = Math.Min(ActualWidth, ActualHeight) / 2;
      Point center = new Point(ActualWidth / 2, ActualHeight / 2);
      double innerRadius = minDimension * InnerRadius;
      double outerRadius = minDimension * OuterRadius;
      double labelRadius = minDimension * LabelRadius;

      // set the fill colour interpolator bounds
      double totalCount = Data.Sum(t => t.Count);
      SegmentFillInterpolator.ActualDataMaximum = Data.Max(t => t.Relationships.Count);
      SegmentFillInterpolator.ActualDataMinimum = Data.Min(t => t.Relationships.Count);

      // render the segments
      double currentAngle = 0;
      foreach (INode node in sortedData)
      {
        double sweepAngle = ((double)node.Count) * 360.0 / totalCount;
        var segment = new NodeSegment()
        {
          SweepAngle = sweepAngle,
          StartAngle = currentAngle,
          InnerRadius = innerRadius,
          OuterRadius = outerRadius,
          LabelRadius = labelRadius,
          LabelText = node.Name,
          Center = center,
          Style = NodeSegmentStyle,
          Background = SegmentFillInterpolator.Interpolate(node.Relationships.Count) as SolidColorBrush,
          DataContext = node,
          ParentGraph = this
        };
        _graphContainer.Children.Add(segment);
        _segmentForNode.Add(node, segment);
        currentAngle += sweepAngle;
      }

      
      double maxRelation = Data.SelectMany(d => d.Relationships).Max(d => d.Strength);
      double minRelation = Data.SelectMany(d => d.Relationships).Min(d => d.Strength);

      // set the interpolator bounds
      ConnectorFillInterpolator.ActualDataMaximum = maxRelation;
      ConnectorFillInterpolator.ActualDataMinimum = minRelation;

      // render the connections
      foreach (INode fromNode in sortedData)
      {
        foreach (var rel in fromNode.Relationships)
        {
          // locate the other end of this connection
          INode toNode = Data.SingleOrDefault(n => n.Name == rel.To);

          if (toNode == null)
          {
            Debug.WriteLine("A relationship to a node that does not exist was found [" + rel.To + "]");
            continue;
          }

          // locate the segment for each node
          var fromSegment = _segmentForNode[fromNode];
          var toSegment = _segmentForNode[toNode];

          // create a connector
          var conn = new NodeConnection()
          {
            Via = center,
            StrokeThickness = Interpolate(minRelation, maxRelation, ConnectorThickness.Minimum,
                                            ConnectorThickness.Maximum, rel.Strength),
            Stroke = ConnectorFillInterpolator.Interpolate(rel.Strength) as SolidColorBrush,
            Style = NodeConnectorStyle
          };

          // bind the connector from / to points to the respective segments
          conn.SetBinding(NodeConnection.FromProperty, new Binding("ConnectorPoint")
          {
            Source = fromSegment
          });
          conn.SetBinding(NodeConnection.ToProperty, new Binding("ConnectorPoint")
          {
            Source = toSegment
          });

          // bind the highlighted state to the highlight state of the source segment
          conn.SetBinding(NodeConnection.IsHighlightedProperty, new Binding("IsHighlighted")
          {
            Source = fromSegment
          });

          _graphContainer.Children.Add(conn);
        }
      }
    }

    private double Interpolate(double fromMin, double fromMax, double toMin, double toMax, double value)
    {
      double factor = (value - fromMin) / (fromMax - fromMin);
      return (toMax - toMin) * factor + toMin;
    }
    
    /// <summary>
    /// Launch a storyboard to animate each segment into place
    /// </summary>
    private void AnimateToOrder(IList<INode> data)
    {
      var sb = new Storyboard();

      double currentAngle = 0;
      foreach (INode node in data)
      {
        NodeSegment segment = _segmentForNode[node];

        double toAngle = currentAngle;
        double fromAngle = segment.StartAngle;

        // find the shortest route between the from / to angles        
        if (Math.Abs(fromAngle - (toAngle - 360)) < Math.Abs(fromAngle - toAngle))
          toAngle -= 360;
        if (Math.Abs(fromAngle - (toAngle + 360)) < Math.Abs(fromAngle - toAngle))
          toAngle += 360;
        
        // animate the segment
        var db = CreateDoubleAnimation(fromAngle, toAngle,
            new SineEase(),
            segment, NodeSegment.StartAngleProperty, TimeSpan.FromMilliseconds(1500));
        sb.Children.Add(db);
        
        currentAngle += segment.SweepAngle;
      }
      sb.Begin();
    }


    private static DoubleAnimation CreateDoubleAnimation(double from, double to, IEasingFunction easing,
                                              DependencyObject target, object propertyPath, TimeSpan duration)
    {
      var db = new DoubleAnimation();
      db.To = to;
      db.From = from;
      db.EasingFunction = easing;
      db.Duration = duration;
      Storyboard.SetTarget(db, target);
      Storyboard.SetTargetProperty(db, new PropertyPath(propertyPath));
      return db;
    }
  }
}
