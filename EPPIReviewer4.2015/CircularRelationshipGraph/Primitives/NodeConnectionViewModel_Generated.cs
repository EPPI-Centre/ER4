





using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using CircularRelationshipGraph.Data;
using System.Windows.Media;
using System.Windows.Controls.DataVisualization;

namespace CircularRelationshipGraph
{
  public partial class NodeConnectionViewModel  
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
    /// Field which backs the Size property
    /// </summary>
    private Size _size = EMPTY_SIZE;

    public static readonly string SizeProperty = "Size";
				
    /// <summary>
    /// Gets / sets the Size value
    /// </summary>
    public Size Size
    {
	    get { return _size; }
	    set
	    {
		    if (_size == value)
			    return;
			
		    _size = value;
        
        OnSizeChanged(value);
		
		    OnPropertyChanged(SizeProperty);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of Size changes
    /// </summary>
    partial void OnSizeChanged(Size value);
    

    /// <summary>
    /// Field which backs the SweepDirection property
    /// </summary>
    private SweepDirection _dir = SweepDirection.Clockwise;

    public static readonly string SweepDirectionProperty = "SweepDirection";
				
    /// <summary>
    /// Gets / sets the SweepDirection value
    /// </summary>
    public SweepDirection SweepDirection
    {
	    get { return _dir; }
	    set
	    {
		    if (_dir == value)
			    return;
			
		    _dir = value;
        
        OnSweepDirectionChanged(value);
		
		    OnPropertyChanged(SweepDirectionProperty);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of SweepDirection changes
    /// </summary>
    partial void OnSweepDirectionChanged(SweepDirection value);
    	}
}
	