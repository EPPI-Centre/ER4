
using System;

namespace Snippets
{
    
    /// <summary>
    /// Code snippet for a property which raises INotifyPropertyChanged
    /// </summary>
    [AttributeUsage(AttributeTargets.Class , AllowMultiple = true)]
    public class SnippetPropertyINPC  : Attribute
    {
    
        /// <summary>
        /// Property Type
        /// </summary>
        public string type = "string";
  
        /// <summary>
        /// Property Name
        /// </summary>
        public string property = "MyProperty";
  
        /// <summary>
        /// Backing Field
        /// </summary>
        public string field = "_myproperty";
  
        /// <summary>
        /// Field default value
        /// </summary>
        public string defaultValue = "null";
  
    /// <summary>
    /// Gets the code snippet
    /// </summary>
    public string GetSnippet()
    {
    return @"

    /// <summary>
    /// Field which backs the $property$ property
    /// </summary>
    private $type$ $field$ = $defaultValue$;

    public static readonly string $property$Property = ""$property$"";
				
    /// <summary>
    /// Gets / sets the $property$ value
    /// </summary>
    public $type$ $property$
    {
	    get { return $field$; }
	    set
	    {
		    if ($field$ == value)
			    return;
			
		    $field$ = value;
        
        On$property$Changed(value);
		
		    OnPropertyChanged($property$Property);
	    }
    }
    
    /// <summary>
    /// Invoked when the value of $property$ changes
    /// </summary>
    partial void On$property$Changed($type$ value);
    $end$";
    }
  
    }
  
}
  
