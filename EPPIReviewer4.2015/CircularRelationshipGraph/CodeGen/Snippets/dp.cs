
using System;

namespace Snippets
{
    
    /// <summary>
    /// Defines a DependencyProperty
    /// </summary>
    [AttributeUsage(AttributeTargets.Class , AllowMultiple = true)]
    public class SnippetDependencyProperty  : Attribute
    {
    
        /// <summary>
        /// Property Type
        /// </summary>
        public string type = "string";
  
        /// <summary>
        /// Summary Documentation
        /// </summary>
        public string summary = "Gets / sets the property value";
  
        /// <summary>
        /// Property Name
        /// </summary>
        public string property = "MyProperty";
  
        /// <summary>
        /// Containing type
        /// </summary>
        public string containerType = "Control";
  
        /// <summary>
        /// Property default value
        /// </summary>
        public string defaultValue = "null";
  
    /// <summary>
    /// Gets the code snippet
    /// </summary>
    public string GetSnippet()
    {
    return @"
    /// <summary>
    /// $summary$ This is a dependency property
    /// </summary>
    public $type$ $property$
    {
        get { return ($type$)GetValue($property$Property); }
        set { SetValue($property$Property, value); }
    }
    
    /// <summary>
    /// Defines the $property$ dependnecy property.
    /// </summary>
    public static readonly DependencyProperty $property$Property =
        DependencyProperty.Register(""$property$"", typeof($type$), typeof($containerType$),
            new PropertyMetadata($defaultValue$, new PropertyChangedCallback(On$property$PropertyChanged)));
            
    /// <summary>
    /// Invoked when the $property$ property changes
    /// </summary>
    partial void On$property$PropertyChanged(DependencyPropertyChangedEventArgs e);

    private static void On$property$PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        $containerType$ control = d as $containerType$;
        control.On$property$PropertyChanged(e);
    }
    
    $end$";
    }
  
    }
  
}
  
