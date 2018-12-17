





using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using CircularRelationshipGraph.Data;
using System.Windows.Media;
using System.Windows.Controls.DataVisualization;

namespace CircularRelationshipGraph
{
  public partial class NodeSegment  
  {
	
    #region INotifyPropertyChanged Members

    /// <summary>
    /// Occurs when a property changes
    /// </summary>
    public event PropertyChangedEventHandler  PropertyChanged;

    /// <summary>
    /// Raises a PropertyChanged event
    /// </summary>
    protected void OnPropertyChanged(string property)
    {
	    if (PropertyChanged != null)
	    {
		    PropertyChanged(this, new PropertyChangedEventArgs(property));
	    }
    }

    #endregion
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public double StartAngle
    {
        get { return (double)GetValue(StartAngleProperty); }
        set { SetValue(StartAngleProperty, value); }
    }
    
    /// <summary>
    /// Defines the StartAngle dependnecy property.
    /// </summary>
    public static readonly DependencyProperty StartAngleProperty =
        DependencyProperty.Register("StartAngle", typeof(double), typeof(NodeSegment),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnStartAnglePropertyChanged)));
            
    /// <summary>
    /// Invoked when the StartAngle property changes
    /// </summary>
    partial void OnStartAnglePropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnStartAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnStartAnglePropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public double SweepAngle
    {
        get { return (double)GetValue(SweepAngleProperty); }
        set { SetValue(SweepAngleProperty, value); }
    }
    
    /// <summary>
    /// Defines the SweepAngle dependnecy property.
    /// </summary>
    public static readonly DependencyProperty SweepAngleProperty =
        DependencyProperty.Register("SweepAngle", typeof(double), typeof(NodeSegment),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnSweepAnglePropertyChanged)));
            
    /// <summary>
    /// Invoked when the SweepAngle property changes
    /// </summary>
    partial void OnSweepAnglePropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnSweepAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnSweepAnglePropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public double InnerRadius
    {
        get { return (double)GetValue(InnerRadiusProperty); }
        set { SetValue(InnerRadiusProperty, value); }
    }
    
    /// <summary>
    /// Defines the InnerRadius dependnecy property.
    /// </summary>
    public static readonly DependencyProperty InnerRadiusProperty =
        DependencyProperty.Register("InnerRadius", typeof(double), typeof(NodeSegment),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnInnerRadiusPropertyChanged)));
            
    /// <summary>
    /// Invoked when the InnerRadius property changes
    /// </summary>
    partial void OnInnerRadiusPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnInnerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnInnerRadiusPropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public double OuterRadius
    {
        get { return (double)GetValue(OuterRadiusProperty); }
        set { SetValue(OuterRadiusProperty, value); }
    }
    
    /// <summary>
    /// Defines the OuterRadius dependnecy property.
    /// </summary>
    public static readonly DependencyProperty OuterRadiusProperty =
        DependencyProperty.Register("OuterRadius", typeof(double), typeof(NodeSegment),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnOuterRadiusPropertyChanged)));
            
    /// <summary>
    /// Invoked when the OuterRadius property changes
    /// </summary>
    partial void OnOuterRadiusPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnOuterRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnOuterRadiusPropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public Point Center
    {
        get { return (Point)GetValue(CenterProperty); }
        set { SetValue(CenterProperty, value); }
    }
    
    /// <summary>
    /// Defines the Center dependnecy property.
    /// </summary>
    public static readonly DependencyProperty CenterProperty =
        DependencyProperty.Register("Center", typeof(Point), typeof(NodeSegment),
            new PropertyMetadata(new Point(), new PropertyChangedCallback(OnCenterPropertyChanged)));
            
    /// <summary>
    /// Invoked when the Center property changes
    /// </summary>
    partial void OnCenterPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnCenterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnCenterPropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public Point ConnectorPoint
    {
        get { return (Point)GetValue(ConnectorPointProperty); }
        set { SetValue(ConnectorPointProperty, value); }
    }
    
    /// <summary>
    /// Defines the ConnectorPoint dependnecy property.
    /// </summary>
    public static readonly DependencyProperty ConnectorPointProperty =
        DependencyProperty.Register("ConnectorPoint", typeof(Point), typeof(NodeSegment),
            new PropertyMetadata(new Point(), new PropertyChangedCallback(OnConnectorPointPropertyChanged)));
            
    /// <summary>
    /// Invoked when the ConnectorPoint property changes
    /// </summary>
    partial void OnConnectorPointPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnConnectorPointPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnConnectorPointPropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public double MidPointAngle
    {
        get { return (double)GetValue(MidPointAngleProperty); }
        set { SetValue(MidPointAngleProperty, value); }
    }
    
    /// <summary>
    /// Defines the MidPointAngle dependnecy property.
    /// </summary>
    public static readonly DependencyProperty MidPointAngleProperty =
        DependencyProperty.Register("MidPointAngle", typeof(double), typeof(NodeSegment),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnMidPointAnglePropertyChanged)));
            
    /// <summary>
    /// Invoked when the MidPointAngle property changes
    /// </summary>
    partial void OnMidPointAnglePropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnMidPointAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnMidPointAnglePropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public bool IsHighlighted
    {
        get { return (bool)GetValue(IsHighlightedProperty); }
        set { SetValue(IsHighlightedProperty, value); }
    }
    
    /// <summary>
    /// Defines the IsHighlighted dependnecy property.
    /// </summary>
    public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(NodeSegment),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsHighlightedPropertyChanged)));
            
    /// <summary>
    /// Invoked when the IsHighlighted property changes
    /// </summary>
    partial void OnIsHighlightedPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnIsHighlightedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnIsHighlightedPropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public Brush Stroke
    {
        get { return (Brush)GetValue(StrokeProperty); }
        set { SetValue(StrokeProperty, value); }
    }
    
    /// <summary>
    /// Defines the Stroke dependnecy property.
    /// </summary>
    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register("Stroke", typeof(Brush), typeof(NodeSegment),
            new PropertyMetadata(null, new PropertyChangedCallback(OnStrokePropertyChanged)));
            
    /// <summary>
    /// Invoked when the Stroke property changes
    /// </summary>
    partial void OnStrokePropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnStrokePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnStrokePropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public double StrokeThickness
    {
        get { return (double)GetValue(StrokeThicknessProperty); }
        set { SetValue(StrokeThicknessProperty, value); }
    }
    
    /// <summary>
    /// Defines the StrokeThickness dependnecy property.
    /// </summary>
    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register("StrokeThickness", typeof(double), typeof(NodeSegment),
            new PropertyMetadata(1.0, new PropertyChangedCallback(OnStrokeThicknessPropertyChanged)));
            
    /// <summary>
    /// Invoked when the StrokeThickness property changes
    /// </summary>
    partial void OnStrokeThicknessPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnStrokeThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnStrokeThicknessPropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public string LabelText
    {
        get { return (string)GetValue(LabelTextProperty); }
        set { SetValue(LabelTextProperty, value); }
    }
    
    /// <summary>
    /// Defines the LabelText dependnecy property.
    /// </summary>
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register("LabelText", typeof(string), typeof(NodeSegment),
            new PropertyMetadata("", new PropertyChangedCallback(OnLabelTextPropertyChanged)));
            
    /// <summary>
    /// Invoked when the LabelText property changes
    /// </summary>
    partial void OnLabelTextPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnLabelTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnLabelTextPropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public double LabelRadius
    {
        get { return (double)GetValue(LabelRadiusProperty); }
        set { SetValue(LabelRadiusProperty, value); }
    }
    
    /// <summary>
    /// Defines the LabelRadius dependnecy property.
    /// </summary>
    public static readonly DependencyProperty LabelRadiusProperty =
        DependencyProperty.Register("LabelRadius", typeof(double), typeof(NodeSegment),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnLabelRadiusPropertyChanged)));
            
    /// <summary>
    /// Invoked when the LabelRadius property changes
    /// </summary>
    partial void OnLabelRadiusPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnLabelRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeSegment control = d as NodeSegment;
        control.OnLabelRadiusPropertyChanged(e);
    }
    
    	}
}
	