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
using System.Xml.Linq;
using CircularRelationshipGraph.Data;

namespace CircularRelationshipGraph
{
  public partial class StackOverflow : UserControl
  {
    public StackOverflow()
    {
      InitializeComponent();

      var doc = XDocument.Parse(_xml);
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

     
    }



    private static string _xml = @"
<tags>
  <tag name='android' count='286'>
    <rel name='java' count='48' />
    <rel name='android-layout' count='22' />
    <rel name='eclipse' count='16' />
    <rel name='listview' count='16' />
    <rel name='image' count='12' />
    <rel name='google-maps' count='10' />
    <rel name='xml' count='6' />
    <rel name='string' count='6' />
    <rel name='database' count='6' />
    <rel name='iphone' count='6' />
    <rel name='web-services' count='6' />
    <rel name='file' count='4' />
    <rel name='api' count='4' />
    <rel name='multithreading' count='4' />
    <rel name='facebook' count='4' />
    <rel name='linux' count='2' />
    <rel name='wcf' count='2' />
    <rel name='php' count='2' />
    <rel name='c' count='2' />
    <rel name='css' count='2' />
    <rel name='ios' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='parsing' count='2' />
    <rel name='mysql' count='2' />
    <rel name='html5' count='2' />
    <rel name='c#' count='2' />
    <rel name='jquery' count='2' />
    <rel name='arrays' count='2' />
    <rel name='javascript' count='2' />
    <rel name='google' count='2' />
  </tag>
  <tag name='java' count='255'>
    <rel name='swing' count='48' />
    <rel name='android' count='48' />
    <rel name='spring' count='30' />
    <rel name='eclipse' count='22' />
    <rel name='jboss7.x' count='22' />
    <rel name='java-ee' count='20' />
    <rel name='jboss' count='16' />
    <rel name='xml' count='14' />
    <rel name='maven' count='14' />
    <rel name='hibernate' count='10' />
    <rel name='web-services' count='10' />
    <rel name='javascript' count='10' />
    <rel name='homework' count='10' />
    <rel name='string' count='8' />
    <rel name='regex' count='8' />
    <rel name='mongodb' count='6' />
    <rel name='mysql' count='6' />
    <rel name='json' count='6' />
    <rel name='web-development' count='6' />
    <rel name='php' count='6' />
    <rel name='arrays' count='6' />
    <rel name='multithreading' count='6' />
    <rel name='database' count='6' />
    <rel name='google-app-engine' count='6' />
    <rel name='oracle' count='4' />
    <rel name='html' count='4' />
    <rel name='c' count='4' />
    <rel name='list' count='4' />
    <rel name='jquery' count='4' />
    <rel name='file' count='4' />
    <rel name='events' count='4' />
    <rel name='linux' count='4' />
    <rel name='google' count='4' />
    <rel name='algorithm' count='4' />
    <rel name='c++' count='4' />
    <rel name='server' count='4' />
    <rel name='python' count='2' />
    <rel name='apache' count='2' />
    <rel name='excel' count='2' />
    <rel name='sql' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='parsing' count='2' />
    <rel name='google-chrome' count='2' />
    <rel name='android-layout' count='2' />
    <rel name='windows' count='2' />
    <rel name='internet-explorer' count='2' />
    <rel name='ios' count='2' />
    <rel name='c#' count='2' />
    <rel name='sql-server-2008' count='2' />
    <rel name='api' count='2' />
    <rel name='ajax' count='2' />
  </tag>
  <tag name='javascript' count='229'>
    <rel name='jquery' count='144' />
    <rel name='html' count='58' />
    <rel name='php' count='42' />
    <rel name='html5' count='26' />
    <rel name='ajax' count='22' />
    <rel name='css' count='22' />
    <rel name='asp.net' count='14' />
    <rel name='json' count='14' />
    <rel name='regex' count='14' />
    <rel name='backbone.js' count='12' />
    <rel name='java' count='10' />
    <rel name='google-chrome' count='10' />
    <rel name='events' count='8' />
    <rel name='ipad' count='8' />
    <rel name='web-development' count='8' />
    <rel name='c#' count='8' />
    <rel name='mysql' count='8' />
    <rel name='image' count='6' />
    <rel name='ios' count='6' />
    <rel name='internet-explorer' count='6' />
    <rel name='arrays' count='6' />
    <rel name='string' count='6' />
    <rel name='wordpress' count='4' />
    <rel name='parsing' count='4' />
    <rel name='google-maps' count='4' />
    <rel name='python' count='4' />
    <rel name='api' count='4' />
    <rel name='flash' count='4' />
    <rel name='jquery-plugins' count='4' />
    <rel name='excel' count='2' />
    <rel name='.net' count='2' />
    <rel name='winforms' count='2' />
    <rel name='google' count='2' />
    <rel name='ruby-on-rails-3' count='2' />
    <rel name='iphone' count='2' />
    <rel name='facebook' count='2' />
    <rel name='joomla' count='2' />
    <rel name='file' count='2' />
    <rel name='mongodb' count='2' />
    <rel name='xml' count='2' />
    <rel name='eclipse' count='2' />
    <rel name='android' count='2' />
    <rel name='mvc' count='2' />
    <rel name='asp.net-mvc-3' count='2' />
    <rel name='ruby' count='2' />
    <rel name='web-services' count='2' />
    <rel name='database' count='2' />
  </tag>
  <tag name='c#' count='211'>
    <rel name='.net' count='80' />
    <rel name='asp.net' count='56' />
    <rel name='winforms' count='28' />
    <rel name='wpf' count='20' />
    <rel name='linq' count='18' />
    <rel name='windows-phone-7' count='16' />
    <rel name='arrays' count='14' />
    <rel name='silverlight' count='14' />
    <rel name='wcf' count='12' />
    <rel name='xml' count='12' />
    <rel name='c++' count='10' />
    <rel name='multithreading' count='10' />
    <rel name='sql-server-2008' count='10' />
    <rel name='asp.net-mvc-3' count='8' />
    <rel name='events' count='8' />
    <rel name='c#-4.0' count='8' />
    <rel name='javascript' count='8' />
    <rel name='jquery' count='8' />
    <rel name='visual-studio' count='8' />
    <rel name='sql-server' count='8' />
    <rel name='visual-studio-2010' count='6' />
    <rel name='vb.net' count='6' />
    <rel name='sql' count='6' />
    <rel name='json' count='6' />
    <rel name='excel' count='6' />
    <rel name='asp.net-mvc' count='6' />
    <rel name='web-services' count='6' />
    <rel name='c' count='4' />
    <rel name='ajax' count='4' />
    <rel name='list' count='4' />
    <rel name='google-chrome' count='4' />
    <rel name='mvc' count='4' />
    <rel name='homework' count='4' />
    <rel name='iphone' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='web-development' count='2' />
    <rel name='ios' count='2' />
    <rel name='regex' count='2' />
    <rel name='facebook' count='2' />
    <rel name='flash' count='2' />
    <rel name='listview' count='2' />
    <rel name='internet-explorer' count='2' />
    <rel name='hibernate' count='2' />
    <rel name='java' count='2' />
    <rel name='android' count='2' />
    <rel name='image' count='2' />
    <rel name='database' count='2' />
    <rel name='windows' count='2' />
    <rel name='api' count='2' />
  </tag>
  <tag name='php' count='193'>
    <rel name='mysql' count='52' />
    <rel name='javascript' count='42' />
    <rel name='jquery' count='36' />
    <rel name='codeigniter' count='20' />
    <rel name='html' count='18' />
    <rel name='sql' count='16' />
    <rel name='arrays' count='14' />
    <rel name='database' count='12' />
    <rel name='magento' count='12' />
    <rel name='regex' count='12' />
    <rel name='wordpress' count='10' />
    <rel name='ajax' count='10' />
    <rel name='xml' count='10' />
    <rel name='joomla' count='10' />
    <rel name='facebook' count='6' />
    <rel name='api' count='6' />
    <rel name='java' count='6' />
    <rel name='web-development' count='4' />
    <rel name='css' count='4' />
    <rel name='file' count='4' />
    <rel name='python' count='4' />
    <rel name='apache' count='4' />
    <rel name='parsing' count='2' />
    <rel name='json' count='2' />
    <rel name='eclipse' count='2' />
    <rel name='google-maps' count='2' />
    <rel name='oracle' count='2' />
    <rel name='android' count='2' />
    <rel name='.htaccess' count='2' />
    <rel name='c++' count='2' />
    <rel name='symfony-2.0' count='2' />
    <rel name='excel' count='2' />
    <rel name='git' count='2' />
    <rel name='mvc' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='server' count='2' />
    <rel name='ios' count='2' />
    <rel name='string' count='2' />
    <rel name='linux' count='2' />
    <rel name='ruby' count='2' />
    <rel name='image' count='2' />
    <rel name='mod-rewrite' count='2' />
    <rel name='facebook-graph-api' count='2' />
    <rel name='web-services' count='2' />
    <rel name='c#-4.0' count='2' />
  </tag>
  <tag name='jquery' count='164'>
    <rel name='javascript' count='144' />
    <rel name='php' count='36' />
    <rel name='html' count='30' />
    <rel name='ajax' count='26' />
    <rel name='asp.net' count='24' />
    <rel name='jquery-plugins' count='20' />
    <rel name='css' count='18' />
    <rel name='wordpress' count='10' />
    <rel name='c#' count='8' />
    <rel name='html5' count='8' />
    <rel name='asp.net-mvc-3' count='6' />
    <rel name='internet-explorer' count='4' />
    <rel name='java' count='4' />
    <rel name='mysql' count='4' />
    <rel name='json' count='4' />
    <rel name='mvc' count='4' />
    <rel name='events' count='4' />
    <rel name='wcf' count='2' />
    <rel name='xml' count='2' />
    <rel name='backbone.js' count='2' />
    <rel name='asp.net-mvc' count='2' />
    <rel name='web-development' count='2' />
    <rel name='iphone' count='2' />
    <rel name='ipad' count='2' />
    <rel name='parsing' count='2' />
    <rel name='google-chrome' count='2' />
    <rel name='joomla' count='2' />
    <rel name='google' count='2' />
    <rel name='list' count='2' />
    <rel name='flash' count='2' />
    <rel name='file' count='2' />
    <rel name='regex' count='2' />
    <rel name='android' count='2' />
    <rel name='web-services' count='2' />
    <rel name='arrays' count='2' />
    <rel name='database' count='2' />
  </tag>
  <tag name='ios' count='120'>
    <rel name='iphone' count='90' />
    <rel name='objective-c' count='80' />
    <rel name='xcode' count='30' />
    <rel name='cocoa-touch' count='28' />
    <rel name='ipad' count='16' />
    <rel name='ios5' count='12' />
    <rel name='javascript' count='6' />
    <rel name='api' count='6' />
    <rel name='cocoa' count='4' />
    <rel name='image' count='4' />
    <rel name='database' count='2' />
    <rel name='facebook' count='2' />
    <rel name='facebook-graph-api' count='2' />
    <rel name='c#' count='2' />
    <rel name='web-development' count='2' />
    <rel name='html' count='2' />
    <rel name='xml' count='2' />
    <rel name='parsing' count='2' />
    <rel name='query' count='2' />
    <rel name='json' count='2' />
    <rel name='osx' count='2' />
    <rel name='events' count='2' />
    <rel name='regex' count='2' />
    <rel name='android' count='2' />
    <rel name='server' count='2' />
    <rel name='php' count='2' />
    <rel name='google-maps' count='2' />
    <rel name='java' count='2' />
  </tag>
  <tag name='c++' count='107'>
    <rel name='c' count='30' />
    <rel name='visual-studio-2010' count='10' />
    <rel name='c#' count='10' />
    <rel name='algorithm' count='8' />
    <rel name='linux' count='6' />
    <rel name='multithreading' count='6' />
    <rel name='homework' count='6' />
    <rel name='string' count='6' />
    <rel name='windows' count='4' />
    <rel name='.net' count='4' />
    <rel name='xcode' count='4' />
    <rel name='java' count='4' />
    <rel name='apache' count='2' />
    <rel name='events' count='2' />
    <rel name='google' count='2' />
    <rel name='json' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='cocoa' count='2' />
    <rel name='eclipse' count='2' />
    <rel name='mysql' count='2' />
    <rel name='php' count='2' />
    <rel name='python' count='2' />
    <rel name='list' count='2' />
    <rel name='winforms' count='2' />
    <rel name='osx' count='2' />
    <rel name='parsing' count='2' />
    <rel name='oracle' count='2' />
    <rel name='visual-studio' count='2' />
  </tag>
  <tag name='iphone' count='107'>
    <rel name='ios' count='90' />
    <rel name='objective-c' count='64' />
    <rel name='ipad' count='20' />
    <rel name='cocoa-touch' count='18' />
    <rel name='xcode' count='14' />
    <rel name='ios5' count='6' />
    <rel name='android' count='6' />
    <rel name='cocoa' count='4' />
    <rel name='google-maps' count='4' />
    <rel name='database' count='2' />
    <rel name='facebook-graph-api' count='2' />
    <rel name='c#' count='2' />
    <rel name='web-development' count='2' />
    <rel name='jquery' count='2' />
    <rel name='query' count='2' />
    <rel name='javascript' count='2' />
    <rel name='events' count='2' />
    <rel name='windows-phone-7' count='2' />
    <rel name='html5' count='2' />
    <rel name='c' count='2' />
    <rel name='web-services' count='2' />
  </tag>
  <tag name='objective-c' count='105'>
    <rel name='ios' count='80' />
    <rel name='iphone' count='64' />
    <rel name='xcode' count='30' />
    <rel name='cocoa-touch' count='28' />
    <rel name='cocoa' count='24' />
    <rel name='osx' count='16' />
    <rel name='ios5' count='8' />
    <rel name='ipad' count='4' />
    <rel name='api' count='4' />
    <rel name='database' count='2' />
    <rel name='c#' count='2' />
    <rel name='web-development' count='2' />
    <rel name='xml' count='2' />
    <rel name='parsing' count='2' />
    <rel name='c++' count='2' />
    <rel name='c' count='2' />
    <rel name='file' count='2' />
    <rel name='server' count='2' />
    <rel name='ruby' count='2' />
  </tag>
  <tag name='asp.net' count='99'>
    <rel name='c#' count='56' />
    <rel name='jquery' count='24' />
    <rel name='asp.net-mvc' count='18' />
    <rel name='asp.net-mvc-3' count='18' />
    <rel name='.net' count='16' />
    <rel name='javascript' count='14' />
    <rel name='vb.net' count='8' />
    <rel name='html' count='8' />
    <rel name='ajax' count='8' />
    <rel name='mvc' count='6' />
    <rel name='wcf' count='6' />
    <rel name='server' count='4' />
    <rel name='web-services' count='4' />
    <rel name='json' count='4' />
    <rel name='events' count='4' />
    <rel name='arrays' count='4' />
    <rel name='c#-4.0' count='4' />
    <rel name='sql' count='4' />
    <rel name='listview' count='4' />
    <rel name='sql-server-2008' count='4' />
    <rel name='mysql' count='2' />
    <rel name='web-development' count='2' />
    <rel name='winforms' count='2' />
    <rel name='sql-server' count='2' />
    <rel name='linq' count='2' />
    <rel name='flash' count='2' />
    <rel name='regex' count='2' />
    <rel name='java' count='2' />
    <rel name='android' count='2' />
    <rel name='parsing' count='2' />
    <rel name='jquery-plugins' count='2' />
    <rel name='php' count='2' />
    <rel name='image' count='2' />
    <rel name='css' count='2' />
    <rel name='linux' count='2' />
    <rel name='apache' count='2' />
  </tag>
  <tag name='html' count='92'>
    <rel name='css' count='60' />
    <rel name='javascript' count='58' />
    <rel name='jquery' count='30' />
    <rel name='php' count='18' />
    <rel name='html5' count='12' />
    <rel name='asp.net' count='8' />
    <rel name='web-development' count='6' />
    <rel name='image' count='6' />
    <rel name='python' count='6' />
    <rel name='ipad' count='4' />
    <rel name='xml' count='4' />
    <rel name='.htaccess' count='4' />
    <rel name='java' count='4' />
    <rel name='.net' count='4' />
    <rel name='google' count='4' />
    <rel name='file' count='2' />
    <rel name='ios' count='2' />
    <rel name='google-app-engine' count='2' />
    <rel name='vb.net' count='2' />
    <rel name='perl' count='2' />
    <rel name='json' count='2' />
    <rel name='wpf' count='2' />
    <rel name='silverlight' count='2' />
    <rel name='google-chrome' count='2' />
    <rel name='internet-explorer' count='2' />
    <rel name='flash' count='2' />
    <rel name='parsing' count='2' />
    <rel name='ajax' count='2' />
    <rel name='apache' count='2' />
    <rel name='wordpress' count='2' />
    <rel name='osx' count='2' />
    <rel name='ruby' count='2' />
    <rel name='bash' count='2' />
    <rel name='ruby-on-rails' count='2' />
  </tag>
  <tag name='python' count='86'>
    <rel name='django' count='24' />
    <rel name='google-app-engine' count='10' />
    <rel name='linux' count='6' />
    <rel name='html' count='6' />
    <rel name='regex' count='4' />
    <rel name='multithreading' count='4' />
    <rel name='javascript' count='4' />
    <rel name='php' count='4' />
    <rel name='api' count='4' />
    <rel name='osx' count='4' />
    <rel name='json' count='2' />
    <rel name='java' count='2' />
    <rel name='windows' count='2' />
    <rel name='database' count='2' />
    <rel name='c++' count='2' />
    <rel name='string' count='2' />
    <rel name='excel' count='2' />
    <rel name='server' count='2' />
    <rel name='mongodb' count='2' />
    <rel name='perl' count='2' />
    <rel name='list' count='2' />
    <rel name='algorithm' count='2' />
    <rel name='image' count='2' />
    <rel name='apache' count='2' />
    <rel name='parsing' count='2' />
    <rel name='ruby' count='2' />
    <rel name='bash' count='2' />
    <rel name='c' count='2' />
  </tag>
  <tag name='mysql' count='85'>
    <rel name='php' count='52' />
    <rel name='sql' count='34' />
    <rel name='database' count='12' />
    <rel name='javascript' count='8' />
    <rel name='java' count='6' />
    <rel name='query' count='6' />
    <rel name='oracle' count='4' />
    <rel name='regex' count='4' />
    <rel name='ruby-on-rails' count='4' />
    <rel name='excel' count='4' />
    <rel name='jquery' count='4' />
    <rel name='ruby' count='4' />
    <rel name='swing' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='ruby-on-rails-3' count='2' />
    <rel name='c++' count='2' />
    <rel name='ajax' count='2' />
    <rel name='git' count='2' />
    <rel name='sql-server' count='2' />
    <rel name='spring' count='2' />
    <rel name='web-development' count='2' />
    <rel name='file' count='2' />
    <rel name='android' count='2' />
    <rel name='hibernate' count='2' />
    <rel name='jboss7.x' count='2' />
    <rel name='wordpress' count='2' />
    <rel name='codeigniter' count='2' />
    <rel name='magento' count='2' />
  </tag>
  <tag name='.net' count='75'>
    <rel name='c#' count='80' />
    <rel name='asp.net' count='16' />
    <rel name='winforms' count='16' />
    <rel name='wcf' count='12' />
    <rel name='asp.net-mvc' count='10' />
    <rel name='asp.net-mvc-3' count='8' />
    <rel name='wpf' count='8' />
    <rel name='multithreading' count='8' />
    <rel name='visual-studio-2010' count='6' />
    <rel name='xml' count='6' />
    <rel name='silverlight' count='6' />
    <rel name='windows' count='6' />
    <rel name='mvc' count='4' />
    <rel name='vb.net' count='4' />
    <rel name='web-services' count='4' />
    <rel name='c++' count='4' />
    <rel name='html' count='4' />
    <rel name='sql-server-2008' count='4' />
    <rel name='windows-phone-7' count='4' />
    <rel name='javascript' count='2' />
    <rel name='events' count='2' />
    <rel name='image' count='2' />
    <rel name='json' count='2' />
    <rel name='sql-server' count='2' />
    <rel name='file' count='2' />
    <rel name='database' count='2' />
    <rel name='linq' count='2' />
    <rel name='visual-studio' count='2' />
    <rel name='c#-4.0' count='2' />
    <rel name='sql' count='2' />
  </tag>
  <tag name='sql' count='69'>
    <rel name='mysql' count='34' />
    <rel name='sql-server' count='18' />
    <rel name='sql-server-2008' count='16' />
    <rel name='php' count='16' />
    <rel name='oracle' count='12' />
    <rel name='query' count='10' />
    <rel name='database' count='8' />
    <rel name='c#' count='6' />
    <rel name='asp.net' count='4' />
    <rel name='web-services' count='2' />
    <rel name='arrays' count='2' />
    <rel name='json' count='2' />
    <rel name='java' count='2' />
    <rel name='spring' count='2' />
    <rel name='linq' count='2' />
    <rel name='homework' count='2' />
    <rel name='wordpress' count='2' />
    <rel name='.net' count='2' />
  </tag>
  <tag name='css' count='65'>
    <rel name='html' count='60' />
    <rel name='javascript' count='22' />
    <rel name='jquery' count='18' />
    <rel name='internet-explorer' count='8' />
    <rel name='php' count='4' />
    <rel name='wordpress' count='4' />
    <rel name='file' count='2' />
    <rel name='html5' count='2' />
    <rel name='parsing' count='2' />
    <rel name='image' count='2' />
    <rel name='android' count='2' />
    <rel name='web-development' count='2' />
    <rel name='google-chrome' count='2' />
    <rel name='google' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='ipad' count='2' />
  </tag>
  <tag name='c' count='62'>
    <rel name='c++' count='30' />
    <rel name='linux' count='18' />
    <rel name='homework' count='8' />
    <rel name='multithreading' count='6' />
    <rel name='java' count='4' />
    <rel name='c#' count='4' />
    <rel name='algorithm' count='4' />
    <rel name='arrays' count='4' />
    <rel name='apache' count='2' />
    <rel name='android' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='xcode' count='2' />
    <rel name='cocoa' count='2' />
    <rel name='osx' count='2' />
    <rel name='linq' count='2' />
    <rel name='bash' count='2' />
    <rel name='iphone' count='2' />
    <rel name='list' count='2' />
    <rel name='windows' count='2' />
    <rel name='file' count='2' />
    <rel name='python' count='2' />
  </tag>
  <tag name='ruby-on-rails' count='53'>
    <rel name='ruby' count='46' />
    <rel name='ruby-on-rails-3' count='20' />
    <rel name='mysql' count='4' />
    <rel name='backbone.js' count='2' />
    <rel name='arrays' count='2' />
    <rel name='mvc' count='2' />
    <rel name='facebook' count='2' />
    <rel name='database' count='2' />
    <rel name='css' count='2' />
    <rel name='mongodb' count='2' />
    <rel name='windows' count='2' />
    <rel name='git' count='2' />
    <rel name='html' count='2' />
  </tag>
  <tag name='ruby' count='47'>
    <rel name='ruby-on-rails' count='46' />
    <rel name='ruby-on-rails-3' count='16' />
    <rel name='mysql' count='4' />
    <rel name='arrays' count='2' />
    <rel name='algorithm' count='2' />
    <rel name='facebook' count='2' />
    <rel name='database' count='2' />
    <rel name='php' count='2' />
    <rel name='python' count='2' />
    <rel name='html' count='2' />
    <rel name='javascript' count='2' />
    <rel name='ajax' count='2' />
    <rel name='mongodb' count='2' />
    <rel name='windows' count='2' />
    <rel name='git' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='server' count='2' />
  </tag>
  <tag name='linux' count='38'>
    <rel name='c' count='18' />
    <rel name='bash' count='8' />
    <rel name='c++' count='6' />
    <rel name='python' count='6' />
    <rel name='eclipse' count='4' />
    <rel name='java' count='4' />
    <rel name='osx' count='4' />
    <rel name='apache' count='4' />
    <rel name='jboss7.x' count='4' />
    <rel name='database' count='2' />
    <rel name='perl' count='2' />
    <rel name='android' count='2' />
    <rel name='windows' count='2' />
    <rel name='server' count='2' />
    <rel name='multithreading' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='php' count='2' />
    <rel name='java-ee' count='2' />
    <rel name='jboss' count='2' />
  </tag>
  <tag name='facebook' count='37'>
    <rel name='facebook-graph-api' count='12' />
    <rel name='php' count='6' />
    <rel name='android' count='4' />
    <rel name='ios' count='2' />
    <rel name='api' count='2' />
    <rel name='javascript' count='2' />
    <rel name='backbone.js' count='2' />
    <rel name='c#' count='2' />
    <rel name='web-development' count='2' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='ruby' count='2' />
    <rel name='ruby-on-rails-3' count='2' />
    <rel name='wordpress' count='2' />
  </tag>
  <tag name='database' count='37'>
    <rel name='php' count='12' />
    <rel name='mysql' count='12' />
    <rel name='sql' count='8' />
    <rel name='sql-server' count='8' />
    <rel name='android' count='6' />
    <rel name='java' count='6' />
    <rel name='query' count='4' />
    <rel name='excel' count='4' />
    <rel name='sql-server-2008' count='4' />
    <rel name='linux' count='2' />
    <rel name='iphone' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='ios' count='2' />
    <rel name='django' count='2' />
    <rel name='python' count='2' />
    <rel name='xml' count='2' />
    <rel name='c#' count='2' />
    <rel name='.net' count='2' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='ruby' count='2' />
    <rel name='swing' count='2' />
    <rel name='listview' count='2' />
    <rel name='codeigniter' count='2' />
    <rel name='javascript' count='2' />
    <rel name='jquery' count='2' />
  </tag>
  <tag name='xcode' count='35'>
    <rel name='objective-c' count='30' />
    <rel name='ios' count='30' />
    <rel name='iphone' count='14' />
    <rel name='osx' count='6' />
    <rel name='ios5' count='4' />
    <rel name='c++' count='4' />
    <rel name='c' count='2' />
    <rel name='cocoa' count='2' />
    <rel name='file' count='2' />
    <rel name='api' count='2' />
  </tag>
  <tag name='xml' count='35'>
    <rel name='java' count='14' />
    <rel name='c#' count='12' />
    <rel name='php' count='10' />
    <rel name='android' count='6' />
    <rel name='.net' count='6' />
    <rel name='html' count='4' />
    <rel name='json' count='4' />
    <rel name='spring' count='4' />
    <rel name='web-services' count='4' />
    <rel name='jquery' count='2' />
    <rel name='winforms' count='2' />
    <rel name='api' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='ios' count='2' />
    <rel name='parsing' count='2' />
    <rel name='list' count='2' />
    <rel name='codeigniter' count='2' />
    <rel name='database' count='2' />
    <rel name='sql-server-2008' count='2' />
    <rel name='javascript' count='2' />
    <rel name='ajax' count='2' />
    <rel name='string' count='2' />
    <rel name='mod-rewrite' count='2' />
  </tag>
  <tag name='spring' count='32'>
    <rel name='java' count='30' />
    <rel name='hibernate' count='8' />
    <rel name='jboss' count='6' />
    <rel name='xml' count='4' />
    <rel name='maven' count='4' />
    <rel name='mvc' count='4' />
    <rel name='ajax' count='2' />
    <rel name='google-app-engine' count='2' />
    <rel name='mysql' count='2' />
    <rel name='sql' count='2' />
    <rel name='eclipse' count='2' />
    <rel name='jboss7.x' count='2' />
  </tag>
  <tag name='wpf' count='31'>
    <rel name='c#' count='20' />
    <rel name='silverlight' count='8' />
    <rel name='.net' count='8' />
    <rel name='c#-4.0' count='4' />
    <rel name='html' count='2' />
    <rel name='multithreading' count='2' />
    <rel name='visual-studio' count='2' />
    <rel name='windows-phone-7' count='2' />
  </tag>
  <tag name='eclipse' count='31'>
    <rel name='java' count='22' />
    <rel name='android' count='16' />
    <rel name='linux' count='4' />
    <rel name='git' count='4' />
    <rel name='maven' count='4' />
    <rel name='jboss7.x' count='4' />
    <rel name='php' count='2' />
    <rel name='c++' count='2' />
    <rel name='parsing' count='2' />
    <rel name='windows' count='2' />
    <rel name='spring' count='2' />
    <rel name='javascript' count='2' />
    <rel name='java-ee' count='2' />
    <rel name='jboss' count='2' />
    <rel name='google-chrome' count='2' />
  </tag>
  <tag name='arrays' count='29'>
    <rel name='c#' count='14' />
    <rel name='php' count='14' />
    <rel name='java' count='6' />
    <rel name='javascript' count='6' />
    <rel name='asp.net' count='4' />
    <rel name='homework' count='4' />
    <rel name='c' count='4' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='ruby' count='2' />
    <rel name='linq' count='2' />
    <rel name='sql' count='2' />
    <rel name='json' count='2' />
    <rel name='list' count='2' />
    <rel name='string' count='2' />
    <rel name='algorithm' count='2' />
    <rel name='jquery-plugins' count='2' />
    <rel name='query' count='2' />
    <rel name='mongodb' count='2' />
    <rel name='c#-4.0' count='2' />
    <rel name='api' count='2' />
    <rel name='android' count='2' />
    <rel name='jquery' count='2' />
    <rel name='ajax' count='2' />
  </tag>
  <tag name='asp.net-mvc-3' count='29'>
    <rel name='asp.net-mvc' count='20' />
    <rel name='asp.net' count='18' />
    <rel name='.net' count='8' />
    <rel name='c#' count='8' />
    <rel name='mvc' count='6' />
    <rel name='jquery' count='6' />
    <rel name='wcf' count='2' />
    <rel name='javascript' count='2' />
    <rel name='backbone.js' count='2' />
  </tag>
  <tag name='regex' count='27'>
    <rel name='javascript' count='14' />
    <rel name='php' count='12' />
    <rel name='java' count='8' />
    <rel name='python' count='4' />
    <rel name='mysql' count='4' />
    <rel name='perl' count='2' />
    <rel name='c#' count='2' />
    <rel name='ios' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='string' count='2' />
    <rel name='jquery' count='2' />
  </tag>
  <tag name='ajax' count='26'>
    <rel name='jquery' count='26' />
    <rel name='javascript' count='22' />
    <rel name='php' count='10' />
    <rel name='asp.net' count='8' />
    <rel name='c#' count='4' />
    <rel name='wcf' count='2' />
    <rel name='json' count='2' />
    <rel name='spring' count='2' />
    <rel name='mysql' count='2' />
    <rel name='codeigniter' count='2' />
    <rel name='html' count='2' />
    <rel name='xml' count='2' />
    <rel name='ruby' count='2' />
    <rel name='arrays' count='2' />
    <rel name='java' count='2' />
    <rel name='wordpress' count='2' />
  </tag>
  <tag name='windows' count='26'>
    <rel name='.net' count='6' />
    <rel name='c++' count='4' />
    <rel name='python' count='2' />
    <rel name='linux' count='2' />
    <rel name='eclipse' count='2' />
    <rel name='java' count='2' />
    <rel name='c#' count='2' />
    <rel name='windows-phone-7' count='2' />
    <rel name='c' count='2' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='ruby' count='2' />
    <rel name='git' count='2' />
  </tag>
  <tag name='sql-server' count='26'>
    <rel name='sql' count='18' />
    <rel name='sql-server-2008' count='12' />
    <rel name='c#' count='8' />
    <rel name='database' count='8' />
    <rel name='asp.net' count='2' />
    <rel name='mysql' count='2' />
    <rel name='web-development' count='2' />
    <rel name='.net' count='2' />
    <rel name='query' count='2' />
    <rel name='asp.net-mvc' count='2' />
  </tag>
  <tag name='swing' count='25'>
    <rel name='java' count='48' />
    <rel name='homework' count='4' />
    <rel name='mysql' count='2' />
    <rel name='database' count='2' />
  </tag>
  <tag name='image' count='25'>
    <rel name='android' count='12' />
    <rel name='javascript' count='6' />
    <rel name='html' count='6' />
    <rel name='ios' count='4' />
    <rel name='.net' count='2' />
    <rel name='css' count='2' />
    <rel name='joomla' count='2' />
    <rel name='html5' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='python' count='2' />
    <rel name='c#' count='2' />
    <rel name='php' count='2' />
  </tag>
  <tag name='ruby-on-rails-3' count='25'>
    <rel name='ruby-on-rails' count='20' />
    <rel name='ruby' count='16' />
    <rel name='mysql' count='2' />
    <rel name='javascript' count='2' />
    <rel name='facebook' count='2' />
  </tag>
  <tag name='sql-server-2008' count='25'>
    <rel name='sql' count='16' />
    <rel name='sql-server' count='12' />
    <rel name='c#' count='10' />
    <rel name='visual-studio-2010' count='4' />
    <rel name='database' count='4' />
    <rel name='asp.net' count='4' />
    <rel name='.net' count='4' />
    <rel name='xml' count='2' />
    <rel name='hibernate' count='2' />
    <rel name='java' count='2' />
  </tag>
  <tag name='apache' count='24'>
    <rel name='mod-rewrite' count='14' />
    <rel name='.htaccess' count='8' />
    <rel name='linux' count='4' />
    <rel name='php' count='4' />
    <rel name='c++' count='2' />
    <rel name='c' count='2' />
    <rel name='parsing' count='2' />
    <rel name='wordpress' count='2' />
    <rel name='java' count='2' />
    <rel name='web-development' count='2' />
    <rel name='html' count='2' />
    <rel name='python' count='2' />
    <rel name='django' count='2' />
    <rel name='asp.net' count='2' />
  </tag>
  <tag name='jboss7.x' count='24'>
    <rel name='java' count='22' />
    <rel name='jboss' count='22' />
    <rel name='java-ee' count='8' />
    <rel name='maven' count='4' />
    <rel name='eclipse' count='4' />
    <rel name='linux' count='4' />
    <rel name='hibernate' count='4' />
    <rel name='web-services' count='2' />
    <rel name='spring' count='2' />
    <rel name='mysql' count='2' />
  </tag>
  <tag name='visual-studio-2010' count='23'>
    <rel name='c++' count='10' />
    <rel name='visual-studio' count='10' />
    <rel name='vb.net' count='6' />
    <rel name='c#' count='6' />
    <rel name='.net' count='6' />
    <rel name='sql-server-2008' count='4' />
    <rel name='c#-4.0' count='2' />
  </tag>
  <tag name='wcf' count='23'>
    <rel name='c#' count='12' />
    <rel name='.net' count='12' />
    <rel name='asp.net' count='6' />
    <rel name='web-services' count='6' />
    <rel name='silverlight' count='4' />
    <rel name='asp.net-mvc' count='4' />
    <rel name='jquery' count='2' />
    <rel name='ajax' count='2' />
    <rel name='android' count='2' />
    <rel name='winforms' count='2' />
    <rel name='multithreading' count='2' />
    <rel name='c#-4.0' count='2' />
    <rel name='asp.net-mvc-3' count='2' />
    <rel name='linq' count='2' />
  </tag>
  <tag name='asp.net-mvc' count='23'>
    <rel name='asp.net-mvc-3' count='20' />
    <rel name='asp.net' count='18' />
    <rel name='.net' count='10' />
    <rel name='c#' count='6' />
    <rel name='mvc' count='4' />
    <rel name='wcf' count='4' />
    <rel name='jquery' count='2' />
    <rel name='sql-server' count='2' />
  </tag>
  <tag name='django' count='22'>
    <rel name='python' count='24' />
    <rel name='database' count='2' />
    <rel name='homework' count='2' />
    <rel name='apache' count='2' />
    <rel name='parsing' count='2' />
  </tag>
  <tag name='multithreading' count='22'>
    <rel name='c#' count='10' />
    <rel name='.net' count='8' />
    <rel name='c++' count='6' />
    <rel name='java' count='6' />
    <rel name='c' count='6' />
    <rel name='python' count='4' />
    <rel name='android' count='4' />
    <rel name='silverlight' count='2' />
    <rel name='windows-phone-7' count='2' />
    <rel name='wpf' count='2' />
    <rel name='wcf' count='2' />
    <rel name='linux' count='2' />
    <rel name='file' count='2' />
  </tag>
  <tag name='ipad' count='22'>
    <rel name='iphone' count='20' />
    <rel name='ios' count='16' />
    <rel name='javascript' count='8' />
    <rel name='html' count='4' />
    <rel name='objective-c' count='4' />
    <rel name='jquery' count='2' />
    <rel name='ios5' count='2' />
    <rel name='html5' count='2' />
    <rel name='cocoa-touch' count='2' />
    <rel name='css' count='2' />
  </tag>
  <tag name='algorithm' count='22'>
    <rel name='c++' count='8' />
    <rel name='c' count='4' />
    <rel name='java' count='4' />
    <rel name='string' count='4' />
    <rel name='arrays' count='2' />
    <rel name='python' count='2' />
    <rel name='ruby' count='2' />
    <rel name='homework' count='2' />
  </tag>
  <tag name='cocoa' count='21'>
    <rel name='objective-c' count='24' />
    <rel name='osx' count='12' />
    <rel name='iphone' count='4' />
    <rel name='ios' count='4' />
    <rel name='cocoa-touch' count='4' />
    <rel name='c++' count='2' />
    <rel name='c' count='2' />
    <rel name='xcode' count='2' />
  </tag>
  <tag name='html5' count='21'>
    <rel name='javascript' count='26' />
    <rel name='html' count='12' />
    <rel name='jquery' count='8' />
    <rel name='css' count='2' />
    <rel name='web-development' count='2' />
    <rel name='image' count='2' />
    <rel name='ipad' count='2' />
    <rel name='backbone.js' count='2' />
    <rel name='android' count='2' />
    <rel name='iphone' count='2' />
    <rel name='server' count='2' />
  </tag>
  <tag name='osx' count='21'>
    <rel name='objective-c' count='16' />
    <rel name='cocoa' count='12' />
    <rel name='xcode' count='6' />
    <rel name='python' count='4' />
    <rel name='linux' count='4' />
    <rel name='ios' count='2' />
    <rel name='cocoa-touch' count='2' />
    <rel name='c' count='2' />
    <rel name='c++' count='2' />
    <rel name='html' count='2' />
    <rel name='bash' count='2' />
  </tag>
  <tag name='excel' count='20'>
    <rel name='c#' count='6' />
    <rel name='mysql' count='4' />
    <rel name='database' count='4' />
    <rel name='javascript' count='2' />
    <rel name='php' count='2' />
    <rel name='java-ee' count='2' />
    <rel name='internet-explorer' count='2' />
    <rel name='python' count='2' />
    <rel name='server' count='2' />
    <rel name='java' count='2' />
  </tag>
  <tag name='winforms' count='20'>
    <rel name='c#' count='28' />
    <rel name='.net' count='16' />
    <rel name='xml' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='wcf' count='2' />
    <rel name='javascript' count='2' />
    <rel name='c++' count='2' />
    <rel name='events' count='2' />
    <rel name='vb.net' count='2' />
    <rel name='visual-studio' count='2' />
  </tag>
  <tag name='cocoa-touch' count='20'>
    <rel name='objective-c' count='28' />
    <rel name='ios' count='28' />
    <rel name='iphone' count='18' />
    <rel name='cocoa' count='4' />
    <rel name='osx' count='2' />
    <rel name='api' count='2' />
    <rel name='ipad' count='2' />
    <rel name='ios5' count='2' />
  </tag>
  <tag name='api' count='20'>
    <rel name='php' count='6' />
    <rel name='ios' count='6' />
    <rel name='android' count='4' />
    <rel name='javascript' count='4' />
    <rel name='objective-c' count='4' />
    <rel name='python' count='4' />
    <rel name='xml' count='2' />
    <rel name='json' count='2' />
    <rel name='facebook' count='2' />
    <rel name='xcode' count='2' />
    <rel name='facebook-graph-api' count='2' />
    <rel name='cocoa-touch' count='2' />
    <rel name='arrays' count='2' />
    <rel name='c#' count='2' />
    <rel name='java' count='2' />
  </tag>
  <tag name='silverlight' count='19'>
    <rel name='c#' count='14' />
    <rel name='wpf' count='8' />
    <rel name='windows-phone-7' count='8' />
    <rel name='.net' count='6' />
    <rel name='wcf' count='4' />
    <rel name='multithreading' count='2' />
    <rel name='html' count='2' />
    <rel name='file' count='2' />
  </tag>
  <tag name='vb.net' count='19'>
    <rel name='asp.net' count='8' />
    <rel name='visual-studio-2010' count='6' />
    <rel name='c#' count='6' />
    <rel name='.net' count='4' />
    <rel name='html' count='2' />
    <rel name='visual-studio' count='2' />
    <rel name='winforms' count='2' />
  </tag>
  <tag name='wordpress' count='19'>
    <rel name='php' count='10' />
    <rel name='jquery' count='10' />
    <rel name='javascript' count='4' />
    <rel name='css' count='4' />
    <rel name='apache' count='2' />
    <rel name='.htaccess' count='2' />
    <rel name='mod-rewrite' count='2' />
    <rel name='html' count='2' />
    <rel name='facebook' count='2' />
    <rel name='mysql' count='2' />
    <rel name='sql' count='2' />
    <rel name='ajax' count='2' />
  </tag>
  <tag name='json' count='19'>
    <rel name='javascript' count='14' />
    <rel name='java' count='6' />
    <rel name='c#' count='6' />
    <rel name='asp.net' count='4' />
    <rel name='xml' count='4' />
    <rel name='jquery' count='4' />
    <rel name='web-services' count='4' />
    <rel name='python' count='2' />
    <rel name='ajax' count='2' />
    <rel name='php' count='2' />
    <rel name='api' count='2' />
    <rel name='c++' count='2' />
    <rel name='ios' count='2' />
    <rel name='html' count='2' />
    <rel name='sql' count='2' />
    <rel name='arrays' count='2' />
    <rel name='.net' count='2' />
  </tag>
  <tag name='web-services' count='19'>
    <rel name='java' count='10' />
    <rel name='c#' count='6' />
    <rel name='android' count='6' />
    <rel name='wcf' count='6' />
    <rel name='asp.net' count='4' />
    <rel name='.net' count='4' />
    <rel name='java-ee' count='4' />
    <rel name='xml' count='4' />
    <rel name='json' count='4' />
    <rel name='list' count='2' />
    <rel name='sql' count='2' />
    <rel name='jboss7.x' count='2' />
    <rel name='iphone' count='2' />
    <rel name='javascript' count='2' />
    <rel name='jquery' count='2' />
    <rel name='php' count='2' />
    <rel name='c#-4.0' count='2' />
  </tag>
  <tag name='windows-phone-7' count='19'>
    <rel name='c#' count='16' />
    <rel name='silverlight' count='8' />
    <rel name='.net' count='4' />
    <rel name='multithreading' count='2' />
    <rel name='list' count='2' />
    <rel name='iphone' count='2' />
    <rel name='windows' count='2' />
    <rel name='wpf' count='2' />
    <rel name='c#-4.0' count='2' />
  </tag>
  <tag name='git' count='18'>
    <rel name='eclipse' count='4' />
    <rel name='mysql' count='2' />
    <rel name='php' count='2' />
    <rel name='jquery-plugins' count='2' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='ruby' count='2' />
    <rel name='windows' count='2' />
  </tag>
  <tag name='oracle' count='17'>
    <rel name='sql' count='12' />
    <rel name='java' count='4' />
    <rel name='mysql' count='4' />
    <rel name='php' count='2' />
    <rel name='query' count='2' />
    <rel name='c++' count='2' />
    <rel name='magento' count='2' />
  </tag>
  <tag name='java-ee' count='17'>
    <rel name='java' count='20' />
    <rel name='jboss7.x' count='8' />
    <rel name='web-services' count='4' />
    <rel name='web-development' count='2' />
    <rel name='excel' count='2' />
    <rel name='internet-explorer' count='2' />
    <rel name='eclipse' count='2' />
    <rel name='linux' count='2' />
    <rel name='jboss' count='2' />
  </tag>
  <tag name='hibernate' count='17'>
    <rel name='java' count='10' />
    <rel name='spring' count='8' />
    <rel name='jboss7.x' count='4' />
    <rel name='jboss' count='2' />
    <rel name='c#' count='2' />
    <rel name='sql-server-2008' count='2' />
    <rel name='mysql' count='2' />
  </tag>
  <tag name='homework' count='17'>
    <rel name='java' count='10' />
    <rel name='c' count='8' />
    <rel name='c++' count='6' />
    <rel name='arrays' count='4' />
    <rel name='swing' count='4' />
    <rel name='c#' count='4' />
    <rel name='django' count='2' />
    <rel name='parsing' count='2' />
    <rel name='string' count='2' />
    <rel name='algorithm' count='2' />
    <rel name='sql' count='2' />
    <rel name='list' count='2' />
  </tag>
  <tag name='web-development' count='16'>
    <rel name='javascript' count='8' />
    <rel name='java' count='6' />
    <rel name='html' count='6' />
    <rel name='php' count='4' />
    <rel name='asp.net' count='2' />
    <rel name='jquery' count='2' />
    <rel name='c#' count='2' />
    <rel name='iphone' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='ios' count='2' />
    <rel name='java-ee' count='2' />
    <rel name='html5' count='2' />
    <rel name='css' count='2' />
    <rel name='google-chrome' count='2' />
    <rel name='sql-server' count='2' />
    <rel name='apache' count='2' />
    <rel name='mod-rewrite' count='2' />
    <rel name='mysql' count='2' />
    <rel name='google-app-engine' count='2' />
    <rel name='facebook' count='2' />
  </tag>
  <tag name='codeigniter' count='16'>
    <rel name='php' count='20' />
    <rel name='xml' count='2' />
    <rel name='ajax' count='2' />
    <rel name='mysql' count='2' />
    <rel name='database' count='2' />
  </tag>
  <tag name='google-app-engine' count='16'>
    <rel name='python' count='10' />
    <rel name='java' count='6' />
    <rel name='html' count='2' />
    <rel name='spring' count='2' />
    <rel name='web-development' count='2' />
  </tag>
  <tag name='string' count='16'>
    <rel name='java' count='8' />
    <rel name='android' count='6' />
    <rel name='javascript' count='6' />
    <rel name='c++' count='6' />
    <rel name='algorithm' count='4' />
    <rel name='perl' count='2' />
    <rel name='arrays' count='2' />
    <rel name='python' count='2' />
    <rel name='query' count='2' />
    <rel name='parsing' count='2' />
    <rel name='php' count='2' />
    <rel name='xml' count='2' />
    <rel name='regex' count='2' />
    <rel name='homework' count='2' />
  </tag>
  <tag name='perl' count='15'>
    <rel name='parsing' count='4' />
    <rel name='linux' count='2' />
    <rel name='regex' count='2' />
    <rel name='html' count='2' />
    <rel name='string' count='2' />
    <rel name='python' count='2' />
  </tag>
  <tag name='mvc' count='15'>
    <rel name='asp.net' count='6' />
    <rel name='asp.net-mvc-3' count='6' />
    <rel name='.net' count='4' />
    <rel name='asp.net-mvc' count='4' />
    <rel name='c#' count='4' />
    <rel name='jquery' count='4' />
    <rel name='spring' count='4' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='php' count='2' />
    <rel name='javascript' count='2' />
  </tag>
  <tag name='mod-rewrite' count='15'>
    <rel name='apache' count='14' />
    <rel name='.htaccess' count='14' />
    <rel name='wordpress' count='2' />
    <rel name='symfony-2.0' count='2' />
    <rel name='web-development' count='2' />
    <rel name='php' count='2' />
    <rel name='xml' count='2' />
  </tag>
  <tag name='.htaccess' count='15'>
    <rel name='mod-rewrite' count='14' />
    <rel name='apache' count='8' />
    <rel name='html' count='4' />
    <rel name='wordpress' count='2' />
    <rel name='symfony-2.0' count='2' />
    <rel name='php' count='2' />
  </tag>
  <tag name='google-chrome' count='15'>
    <rel name='javascript' count='10' />
    <rel name='c#' count='4' />
    <rel name='internet-explorer' count='4' />
    <rel name='jquery' count='2' />
    <rel name='html' count='2' />
    <rel name='css' count='2' />
    <rel name='web-development' count='2' />
    <rel name='java' count='2' />
    <rel name='eclipse' count='2' />
  </tag>
  <tag name='jboss' count='15'>
    <rel name='jboss7.x' count='22' />
    <rel name='java' count='16' />
    <rel name='spring' count='6' />
    <rel name='hibernate' count='2' />
    <rel name='maven' count='2' />
    <rel name='eclipse' count='2' />
    <rel name='linux' count='2' />
    <rel name='java-ee' count='2' />
  </tag>
  <tag name='events' count='14'>
    <rel name='javascript' count='8' />
    <rel name='c#' count='8' />
    <rel name='asp.net' count='4' />
    <rel name='java' count='4' />
    <rel name='jquery' count='4' />
    <rel name='c++' count='2' />
    <rel name='.net' count='2' />
    <rel name='server' count='2' />
    <rel name='iphone' count='2' />
    <rel name='ios' count='2' />
    <rel name='winforms' count='2' />
  </tag>
  <tag name='android-layout' count='14'>
    <rel name='android' count='22' />
    <rel name='java' count='2' />
  </tag>
  <tag name='jquery-plugins' count='14'>
    <rel name='jquery' count='20' />
    <rel name='javascript' count='4' />
    <rel name='arrays' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='git' count='2' />
  </tag>
  <tag name='bash' count='14'>
    <rel name='linux' count='8' />
    <rel name='mongodb' count='2' />
    <rel name='c' count='2' />
    <rel name='osx' count='2' />
    <rel name='python' count='2' />
    <rel name='html' count='2' />
  </tag>
  <tag name='magento' count='14'>
    <rel name='php' count='12' />
    <rel name='mysql' count='2' />
    <rel name='oracle' count='2' />
  </tag>
  <tag name='maven' count='14'>
    <rel name='java' count='14' />
    <rel name='spring' count='4' />
    <rel name='eclipse' count='4' />
    <rel name='jboss7.x' count='4' />
    <rel name='jboss' count='2' />
  </tag>
  <tag name='ios5' count='14'>
    <rel name='ios' count='12' />
    <rel name='objective-c' count='8' />
    <rel name='iphone' count='6' />
    <rel name='xcode' count='4' />
    <rel name='ipad' count='2' />
    <rel name='cocoa-touch' count='2' />
  </tag>
  <tag name='linq' count='13'>
    <rel name='c#' count='18' />
    <rel name='c#-4.0' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='arrays' count='2' />
    <rel name='c' count='2' />
    <rel name='sql' count='2' />
    <rel name='.net' count='2' />
    <rel name='wcf' count='2' />
  </tag>
  <tag name='c#-4.0' count='13'>
    <rel name='c#' count='8' />
    <rel name='wpf' count='4' />
    <rel name='asp.net' count='4' />
    <rel name='linq' count='2' />
    <rel name='wcf' count='2' />
    <rel name='visual-studio-2010' count='2' />
    <rel name='arrays' count='2' />
    <rel name='.net' count='2' />
    <rel name='windows-phone-7' count='2' />
    <rel name='php' count='2' />
    <rel name='web-services' count='2' />
  </tag>
  <tag name='list' count='13'>
    <rel name='java' count='4' />
    <rel name='c#' count='4' />
    <rel name='web-services' count='2' />
    <rel name='windows-phone-7' count='2' />
    <rel name='xml' count='2' />
    <rel name='arrays' count='2' />
    <rel name='c++' count='2' />
    <rel name='jquery' count='2' />
    <rel name='python' count='2' />
    <rel name='c' count='2' />
    <rel name='homework' count='2' />
  </tag>
  <tag name='facebook-graph-api' count='13'>
    <rel name='facebook' count='12' />
    <rel name='ios' count='2' />
    <rel name='iphone' count='2' />
    <rel name='api' count='2' />
    <rel name='php' count='2' />
  </tag>
  <tag name='listview' count='13'>
    <rel name='android' count='16' />
    <rel name='asp.net' count='4' />
    <rel name='c#' count='2' />
    <rel name='database' count='2' />
  </tag>
  <tag name='file' count='12'>
    <rel name='android' count='4' />
    <rel name='java' count='4' />
    <rel name='php' count='4' />
    <rel name='html' count='2' />
    <rel name='css' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='xcode' count='2' />
    <rel name='javascript' count='2' />
    <rel name='jquery' count='2' />
    <rel name='mysql' count='2' />
    <rel name='.net' count='2' />
    <rel name='silverlight' count='2' />
    <rel name='c' count='2' />
    <rel name='multithreading' count='2' />
  </tag>
  <tag name='server' count='12'>
    <rel name='asp.net' count='4' />
    <rel name='java' count='4' />
    <rel name='events' count='2' />
    <rel name='python' count='2' />
    <rel name='excel' count='2' />
    <rel name='linux' count='2' />
    <rel name='php' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='ios' count='2' />
    <rel name='html5' count='2' />
    <rel name='ruby' count='2' />
  </tag>
  <tag name='backbone.js' count='12'>
    <rel name='javascript' count='12' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='jquery' count='2' />
    <rel name='facebook' count='2' />
    <rel name='html5' count='2' />
    <rel name='asp.net-mvc-3' count='2' />
  </tag>
  <tag name='query' count='12'>
    <rel name='sql' count='10' />
    <rel name='mysql' count='6' />
    <rel name='database' count='4' />
    <rel name='iphone' count='2' />
    <rel name='ios' count='2' />
    <rel name='oracle' count='2' />
    <rel name='string' count='2' />
    <rel name='sql-server' count='2' />
    <rel name='arrays' count='2' />
    <rel name='mongodb' count='2' />
  </tag>
  <tag name='parsing' count='12'>
    <rel name='perl' count='4' />
    <rel name='javascript' count='4' />
    <rel name='php' count='2' />
    <rel name='apache' count='2' />
    <rel name='objective-c' count='2' />
    <rel name='ios' count='2' />
    <rel name='xml' count='2' />
    <rel name='jquery' count='2' />
    <rel name='css' count='2' />
    <rel name='homework' count='2' />
    <rel name='eclipse' count='2' />
    <rel name='java' count='2' />
    <rel name='android' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='c++' count='2' />
    <rel name='string' count='2' />
    <rel name='html' count='2' />
    <rel name='python' count='2' />
    <rel name='django' count='2' />
  </tag>
  <tag name='google-maps' count='12'>
    <rel name='android' count='10' />
    <rel name='javascript' count='4' />
    <rel name='iphone' count='4' />
    <rel name='google' count='2' />
    <rel name='php' count='2' />
    <rel name='ios' count='2' />
  </tag>
  <tag name='internet-explorer' count='12'>
    <rel name='css' count='8' />
    <rel name='javascript' count='6' />
    <rel name='jquery' count='4' />
    <rel name='google-chrome' count='4' />
    <rel name='excel' count='2' />
    <rel name='java-ee' count='2' />
    <rel name='html' count='2' />
    <rel name='c#' count='2' />
    <rel name='java' count='2' />
    <rel name='flash' count='2' />
  </tag>
  <tag name='visual-studio' count='12'>
    <rel name='visual-studio-2010' count='10' />
    <rel name='c#' count='8' />
    <rel name='wpf' count='2' />
    <rel name='vb.net' count='2' />
    <rel name='winforms' count='2' />
    <rel name='.net' count='2' />
    <rel name='c++' count='2' />
  </tag>
  <tag name='flash' count='12'>
    <rel name='javascript' count='4' />
    <rel name='c#' count='2' />
    <rel name='asp.net' count='2' />
    <rel name='jquery' count='2' />
    <rel name='html' count='2' />
    <rel name='internet-explorer' count='2' />
  </tag>
  <tag name='joomla' count='12'>
    <rel name='php' count='10' />
    <rel name='jquery' count='2' />
    <rel name='javascript' count='2' />
    <rel name='image' count='2' />
  </tag>
  <tag name='mongodb' count='11'>
    <rel name='java' count='6' />
    <rel name='bash' count='2' />
    <rel name='python' count='2' />
    <rel name='javascript' count='2' />
    <rel name='arrays' count='2' />
    <rel name='query' count='2' />
    <rel name='ruby-on-rails' count='2' />
    <rel name='ruby' count='2' />
  </tag>
  <tag name='google' count='11'>
    <rel name='html' count='4' />
    <rel name='java' count='4' />
    <rel name='javascript' count='2' />
    <rel name='google-maps' count='2' />
    <rel name='c++' count='2' />
    <rel name='jquery' count='2' />
    <rel name='css' count='2' />
    <rel name='android' count='2' />
  </tag>
  <tag name='symfony-2.0' count='11'>
    <rel name='.htaccess' count='2' />
    <rel name='mod-rewrite' count='2' />
    <rel name='php' count='2' />
  </tag>
</tags>";
  }
}
