
using System;

namespace Snippets
{
    
    /// <summary>
    /// Implementation of INotifyPropertyChanged
    /// </summary>
    [AttributeUsage(AttributeTargets.Class , AllowMultiple = true)]
    public class SnippetINotifyPropertyChanged  : Attribute
    {
    
    /// <summary>
    /// Gets the code snippet
    /// </summary>
    public string GetSnippet()
    {
    return @"
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
    $end$";
    }
  
    }
  
}
  
