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
using CircularRelationshipGraph.Data;
using System.Xml.Linq;

namespace CircularRelationshipGraph
{
  public partial class MainPage : UserControl
  {
    public MainPage()
    {
      InitializeComponent();

      var doc = XDocument.Parse(_xml2);
      var data = doc.Descendants("tag")
                 .Select(el => new Node()
                 {
                   Name = el.Attribute("name").Value,
                   Count = int.Parse(el.Attribute("count").Value),
                   Relationships = el.Descendants("rel")
                                  .Select(rel => new NodeRelationship()
                                  {
                                    To = rel.Attribute("name").Value,
                                    Strength = int.Parse(rel.Attribute("count").Value)
                                  }).Cast<INodeRelationship>().ToList()
                 }).Cast<INode>();

      graph.Data = new NodeList(data);

      List<SortOrder> sort = new List<SortOrder>()
      {
        new SortOrder()
        {
          Text = "Tag popularity",
          Provider = new DelegateSortOrderProvider(nodes => nodes.OrderBy(node => node.Count))
        },
        new SortOrder()
        {
          Text = "Tag name",
          Provider = new DelegateSortOrderProvider(nodes => nodes.OrderBy(node => node.Name))
        },
        new SortOrder()
        {
          Text = "Cluster related tags",
          Provider = new MinimisedConnectionLengthSort(true)
        },
        new SortOrder()
        {
          Text = "Number of relationships",
          Provider = new DelegateSortOrderProvider(nodes => nodes.OrderBy(node => node.Relationships.Count))
        }
      };
      sortCombo.ItemsSource = sort;
    }


    public class SortOrder
    {
      public ISortOrderProvider Provider { get; set; }
      public string Text { get; set; }
    }

    private static string _xml2 = @"
<tags>
  <tag name='android' count='107'>
    <rel name='java' count='34' />
    <rel name='javascript' count='8' />
    <rel name='c++' count='2' />
    <rel name='html' count='2' />
    <rel name='ios' count='2' />
  </tag>
  <tag name='java' count='103'>
    <rel name='android' count='34' />
    <rel name='c++' count='2' />
  </tag>
  <tag name='javascript' count='90'>
    <rel name='jquery' count='60' />
    <rel name='php' count='22' />
    <rel name='html' count='20' />
    <rel name='css' count='14' />
    <rel name='android' count='8' />
    <rel name='ruby-on-rails' count='4' />
    <rel name='asp.net' count='2' />
    <rel name='c#' count='2' />
    <rel name='.net' count='2' />
  </tag>
  <tag name='php' count='84'>
    <rel name='javascript' count='22' />
    <rel name='mysql' count='20' />
    <rel name='jquery' count='14' />
    <rel name='html' count='8' />
    <rel name='css' count='6' />
    <rel name='c#' count='2' />
    <rel name='python' count='2' />
  </tag>
  <tag name='c#' count='82'>
    <rel name='.net' count='28' />
    <rel name='asp.net' count='22' />
    <rel name='vb.net' count='6' />
    <rel name='c++' count='4' />
    <rel name='php' count='2' />
    <rel name='javascript' count='2' />
    <rel name='ios' count='2' />
    <rel name='iphone' count='2' />
    <rel name='objective-c' count='2' />
  </tag>
  <tag name='jquery' count='62'>
    <rel name='javascript' count='60' />
    <rel name='css' count='16' />
    <rel name='php' count='14' />
    <rel name='html' count='8' />
    <rel name='asp.net' count='2' />
    <rel name='ios' count='2' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='mysql' count='2' />
  </tag>
  <tag name='c++' count='53'>
    <rel name='c' count='20' />
    <rel name='c#' count='4' />
    <rel name='objective-c' count='2' />
    <rel name='java' count='2' />
    <rel name='android' count='2' />
    <rel name='ios' count='2' />
  </tag>
  <tag name='objective-c' count='48'>
    <rel name='ios' count='34' />
    <rel name='iphone' count='24' />
    <rel name='c++' count='2' />
    <rel name='c' count='2' />
    <rel name='c#' count='2' />
  </tag>
  <tag name='ios' count='47'>
    <rel name='objective-c' count='34' />
    <rel name='iphone' count='34' />
    <rel name='html' count='2' />
    <rel name='c' count='2' />
    <rel name='android' count='2' />
    <rel name='jquery' count='2' />
    <rel name='c#' count='2' />
    <rel name='c++' count='2' />
  </tag>
  <tag name='python' count='46'>
    <rel name='php' count='2' />
  </tag>
  <tag name='ruby-on-rails' count='41'>
    <rel name='ruby' count='26' />
    <rel name='javascript' count='4' />
    <rel name='jquery' count='2' />
  </tag>
  <tag name='iphone' count='36'>
    <rel name='ios' count='34' />
    <rel name='objective-c' count='24' />
    <rel name='html' count='4' />
    <rel name='c' count='2' />
    <rel name='c#' count='2' />
  </tag>
  <tag name='css' count='35'>
    <rel name='html' count='26' />
    <rel name='jquery' count='16' />
    <rel name='javascript' count='14' />
    <rel name='php' count='6' />
    <rel name='asp.net' count='2' />
    <rel name='vb.net' count='2' />
  </tag>
  <tag name='c' count='30'>
    <rel name='c++' count='20' />
    <rel name='objective-c' count='2' />
    <rel name='iphone' count='2' />
    <rel name='html' count='2' />
    <rel name='ios' count='2' />
  </tag>
  <tag name='mysql' count='29'>
    <rel name='php' count='20' />
    <rel name='jquery' count='2' />
    <rel name='vb.net' count='2' />
  </tag>
  <tag name='vb.net' count='29'>
    <rel name='.net' count='8' />
    <rel name='c#' count='6' />
    <rel name='asp.net' count='6' />
    <rel name='mysql' count='2' />
    <rel name='css' count='2' />
  </tag>
  <tag name='ruby' count='28'>
    <rel name='ruby-on-rails' count='26' />
  </tag>
  <tag name='asp.net' count='28'>
    <rel name='c#' count='22' />
    <rel name='vb.net' count='6' />
    <rel name='javascript' count='2' />
    <rel name='jquery' count='2' />
    <rel name='.net' count='2' />
    <rel name='css' count='2' />
  </tag>
  <tag name='html' count='28'>
    <rel name='css' count='26' />
    <rel name='javascript' count='20' />
    <rel name='jquery' count='8' />
    <rel name='php' count='8' />
    <rel name='iphone' count='4' />
    <rel name='android' count='2' />
    <rel name='c' count='2' />
    <rel name='ios' count='2' />
  </tag>
  <tag name='.net' count='25'>
    <rel name='c#' count='28' />
    <rel name='vb.net' count='8' />
    <rel name='javascript' count='2' />
    <rel name='asp.net' count='2' />
  </tag>
</tags>";

    

  }
}
