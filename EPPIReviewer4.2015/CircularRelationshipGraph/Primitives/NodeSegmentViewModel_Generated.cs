





using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using CircularRelationshipGraph.Data;
using System.Windows.Media;
using System.Windows.Controls.DataVisualization;

namespace CircularRelationshipGraph
{
  public partial class NodeSegmentViewModel  
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
    /// Field which backs the S1 property
    /// </summary>
    private Point _s1 = EMPTY_POINT;

    public static readonly string S1Property = "S1";
				
    /// <summary>
    /// Gets / sets the S1 value
    /// </summary>
    public Point S1
    {
	    get { return _s1; }
	    set
	    {
		    if (_s1 == value)
			    return;
			
		    _s1 = value;
        
        OnS1Changed(value);
		
		    OnPropertyChanged(S1Property);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of S1 changes
    /// </summary>
    partial void OnS1Changed(Point value);
    

    /// <summary>
    /// Field which backs the S2 property
    /// </summary>
    private Point _s2 = EMPTY_POINT;

    public static readonly string S2Property = "S2";
				
    /// <summary>
    /// Gets / sets the S2 value
    /// </summary>
    public Point S2
    {
	    get { return _s2; }
	    set
	    {
		    if (_s2 == value)
			    return;
			
		    _s2 = value;
        
        OnS2Changed(value);
		
		    OnPropertyChanged(S2Property);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of S2 changes
    /// </summary>
    partial void OnS2Changed(Point value);
    

    /// <summary>
    /// Field which backs the S3 property
    /// </summary>
    private Point _s3 = EMPTY_POINT;

    public static readonly string S3Property = "S3";
				
    /// <summary>
    /// Gets / sets the S3 value
    /// </summary>
    public Point S3
    {
	    get { return _s3; }
	    set
	    {
		    if (_s3 == value)
			    return;
			
		    _s3 = value;
        
        OnS3Changed(value);
		
		    OnPropertyChanged(S3Property);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of S3 changes
    /// </summary>
    partial void OnS3Changed(Point value);
    

    /// <summary>
    /// Field which backs the S4 property
    /// </summary>
    private Point _s4 = EMPTY_POINT;

    public static readonly string S4Property = "S4";
				
    /// <summary>
    /// Gets / sets the S4 value
    /// </summary>
    public Point S4
    {
	    get { return _s4; }
	    set
	    {
		    if (_s4 == value)
			    return;
			
		    _s4 = value;
        
        OnS4Changed(value);
		
		    OnPropertyChanged(S4Property);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of S4 changes
    /// </summary>
    partial void OnS4Changed(Point value);
    

    /// <summary>
    /// Field which backs the LabelLocation property
    /// </summary>
    private Point _labelLocation = EMPTY_POINT;

    public static readonly string LabelLocationProperty = "LabelLocation";
				
    /// <summary>
    /// Gets / sets the LabelLocation value
    /// </summary>
    public Point LabelLocation
    {
	    get { return _labelLocation; }
	    set
	    {
		    if (_labelLocation == value)
			    return;
			
		    _labelLocation = value;
        
        OnLabelLocationChanged(value);
		
		    OnPropertyChanged(LabelLocationProperty);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of LabelLocation changes
    /// </summary>
    partial void OnLabelLocationChanged(Point value);
    

    /// <summary>
    /// Field which backs the InnerSize property
    /// </summary>
    private Size _innerSize = EMPTY_SIZE;

    public static readonly string InnerSizeProperty = "InnerSize";
				
    /// <summary>
    /// Gets / sets the InnerSize value
    /// </summary>
    public Size InnerSize
    {
	    get { return _innerSize; }
	    set
	    {
		    if (_innerSize == value)
			    return;
			
		    _innerSize = value;
        
        OnInnerSizeChanged(value);
		
		    OnPropertyChanged(InnerSizeProperty);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of InnerSize changes
    /// </summary>
    partial void OnInnerSizeChanged(Size value);
    

    /// <summary>
    /// Field which backs the OuterSize property
    /// </summary>
    private Size _outerSize = EMPTY_SIZE;

    public static readonly string OuterSizeProperty = "OuterSize";
				
    /// <summary>
    /// Gets / sets the OuterSize value
    /// </summary>
    public Size OuterSize
    {
	    get { return _outerSize; }
	    set
	    {
		    if (_outerSize == value)
			    return;
			
		    _outerSize = value;
        
        OnOuterSizeChanged(value);
		
		    OnPropertyChanged(OuterSizeProperty);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of OuterSize changes
    /// </summary>
    partial void OnOuterSizeChanged(Size value);
    

    /// <summary>
    /// Field which backs the IsLargeArc property
    /// </summary>
    private bool _isLargeArc = false;

    public static readonly string IsLargeArcProperty = "IsLargeArc";
				
    /// <summary>
    /// Gets / sets the IsLargeArc value
    /// </summary>
    public bool IsLargeArc
    {
	    get { return _isLargeArc; }
	    set
	    {
		    if (_isLargeArc == value)
			    return;
			
		    _isLargeArc = value;
        
        OnIsLargeArcChanged(value);
		
		    OnPropertyChanged(IsLargeArcProperty);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of IsLargeArc changes
    /// </summary>
    partial void OnIsLargeArcChanged(bool value);
    	}
}
	