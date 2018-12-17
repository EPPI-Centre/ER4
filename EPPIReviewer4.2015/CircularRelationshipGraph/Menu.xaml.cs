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

namespace CircularRelationshipGraph
{
  public partial class Menu : UserControl
  {
    public Menu()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      content.Content = new MainPage();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
      content.Content = new StackOverflow();
    }

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {
      content.Content = new WorldDebt();
    }
  }
}
