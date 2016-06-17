





using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using CircularRelationshipGraph.Data;
using System.Windows.Media;
using System.Windows.Controls.DataVisualization;

namespace CircularRelationshipGraph
{
  public partial class NodeConnection  
  {
	
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public Point From
    {
        get { return (Point)GetValue(FromProperty); }
        set { SetValue(FromProperty, value); }
    }
    
    /// <summary>
    /// Defines the From dependnecy property.
    /// </summary>
    public static readonly DependencyProperty FromProperty =
        DependencyProperty.Register("From", typeof(Point), typeof(NodeConnection),
            new PropertyMetadata(new Point(), new PropertyChangedCallback(OnFromPropertyChanged)));
            
    /// <summary>
    /// Invoked when the From property changes
    /// </summary>
    partial void OnFromPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnFromPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeConnection control = d as NodeConnection;
        control.OnFromPropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public Point To
    {
        get { return (Point)GetValue(ToProperty); }
        set { SetValue(ToProperty, value); }
    }
    
    /// <summary>
    /// Defines the To dependnecy property.
    /// </summary>
    public static readonly DependencyProperty ToProperty =
        DependencyProperty.Register("To", typeof(Point), typeof(NodeConnection),
            new PropertyMetadata(new Point(), new PropertyChangedCallback(OnToPropertyChanged)));
            
    /// <summary>
    /// Invoked when the To property changes
    /// </summary>
    partial void OnToPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnToPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeConnection control = d as NodeConnection;
        control.OnToPropertyChanged(e);
    }
    
    
    /// <summary>
    /// Gets / sets the property value This is a dependency property
    /// </summary>
    public Point Via
    {
        get { return (Point)GetValue(ViaProperty); }
        set { SetValue(ViaProperty, value); }
    }
    
    /// <summary>
    /// Defines the Via dependnecy property.
    /// </summary>
    public static readonly DependencyProperty ViaProperty =
        DependencyProperty.Register("Via", typeof(Point), typeof(NodeConnection),
            new PropertyMetadata(new Point(), new PropertyChangedCallback(OnViaPropertyChanged)));
            
    /// <summary>
    /// Invoked when the Via property changes
    /// </summary>
    partial void OnViaPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnViaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeConnection control = d as NodeConnection;
        control.OnViaPropertyChanged(e);
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
        DependencyProperty.Register("Stroke", typeof(Brush), typeof(NodeConnection),
            new PropertyMetadata(null, new PropertyChangedCallback(OnStrokePropertyChanged)));
            
    /// <summary>
    /// Invoked when the Stroke property changes
    /// </summary>
    partial void OnStrokePropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnStrokePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeConnection control = d as NodeConnection;
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
        DependencyProperty.Register("StrokeThickness", typeof(double), typeof(NodeConnection),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnStrokeThicknessPropertyChanged)));
            
    /// <summary>
    /// Invoked when the StrokeThickness property changes
    /// </summary>
    partial void OnStrokeThicknessPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnStrokeThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeConnection control = d as NodeConnection;
        control.OnStrokeThicknessPropertyChanged(e);
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
        DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(NodeConnection),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsHighlightedPropertyChanged)));
            
    /// <summary>
    /// Invoked when the IsHighlighted property changes
    /// </summary>
    partial void OnIsHighlightedPropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void OnIsHighlightedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NodeConnection control = d as NodeConnection;
        control.OnIsHighlightedPropertyChanged(e);
    }
    
    	}
}
	