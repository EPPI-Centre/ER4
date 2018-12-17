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
using System.ComponentModel;
using Snippets;
using CircularRelationshipGraph.Data;

namespace CircularRelationshipGraph
{
  [SnippetINotifyPropertyChanged]
  [SnippetDependencyProperty(property = "StartAngle", defaultValue = "0.0",
                             type = "double", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "SweepAngle", defaultValue = "0.0",
                             type = "double", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "InnerRadius", defaultValue = "0.0",
                             type = "double", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "OuterRadius", defaultValue = "0.0",
                             type = "double", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "Center", defaultValue = "new Point()",
                             type = "Point", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "ConnectorPoint", defaultValue = "new Point()",
                             type = "Point", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "MidPointAngle", defaultValue = "0.0",
                             type = "double", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "IsHighlighted", defaultValue = "false",
                             type = "bool", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "Stroke", defaultValue = "null",
                             type = "Brush", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "StrokeThickness", defaultValue = "1.0",
                             type = "double", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "LabelText", defaultValue = "\"\"",
                             type = "string", containerType = "NodeSegment")]
  [SnippetDependencyProperty(property = "LabelRadius", defaultValue = "0.0",
                             type = "double", containerType = "NodeSegment")]
  public partial class NodeSegment : Control, INotifyPropertyChanged
  {
    partial void OnIsHighlightedPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      VisualStateManager.GoToState(this, IsHighlighted ? "Highlighted" : "Normal", false);
      Canvas.SetZIndex(this, IsHighlighted ? 100 : 99);

      if (IsHighlighted && ParentGraph != null)
      {
        ParentGraph.HighlightedNode = this.DataContext as INode;
      }
    }

    partial void OnCenterPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      UpdateViewModel();
    }

    partial void OnInnerRadiusPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      UpdateViewModel();
    }

    partial void OnOuterRadiusPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      UpdateViewModel();
    }

    partial void OnStartAnglePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      UpdateViewModel();
    }

    partial void OnSweepAnglePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
      UpdateViewModel();
    }

    private NodeSegmentViewModel _viewmodel = new NodeSegmentViewModel();

    public RelationshipGraph ParentGraph { get; set; }

    public NodeSegmentViewModel ViewModel
    {
      get
      {
        return _viewmodel;
      }
    }

    private void UpdateViewModel()
    {
      double startAngle = StartAngle;
      double endAngle = StartAngle + SweepAngle;

      // compute the properties that the segment exposes to support other UI elements
      MidPointAngle = startAngle + (SweepAngle / 2);
      ConnectorPoint = Util.RadialToCartesian(MidPointAngle, InnerRadius, Center);

      // compute the path control points
      ViewModel.S1 = Util.RadialToCartesian(startAngle, OuterRadius, Center);
      ViewModel.S2 = Util.RadialToCartesian(endAngle, OuterRadius, Center);
      ViewModel.S3 = Util.RadialToCartesian(endAngle, InnerRadius, Center);
      ViewModel.S4 = Util.RadialToCartesian(startAngle, InnerRadius, Center);

      // create sizes from radius values
      ViewModel.InnerSize = new Size(InnerRadius, InnerRadius);
      ViewModel.OuterSize = new Size(OuterRadius, OuterRadius);

      ViewModel.LabelLocation = Util.RadialToCartesian(MidPointAngle, LabelRadius, Center);

      ViewModel.IsLargeArc = SweepAngle > 180;

    }

    public NodeSegment()
    {
      this.DefaultStyleKey = typeof(NodeSegment);
    }

    public override void OnApplyTemplate()
    {
      Panel root = this.GetTemplateChild("rootElement") as Panel;
      UIElement shape = this.GetTemplateChild("segmentShape") as UIElement;

      shape.MouseEnter += (s, e) => IsHighlighted = true;
      shape.MouseLeave += (s, e) => IsHighlighted = false;
    }

  }
}
