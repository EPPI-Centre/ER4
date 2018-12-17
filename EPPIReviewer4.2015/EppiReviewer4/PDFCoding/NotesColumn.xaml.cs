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

namespace EppiReviewer4
{
    public partial class NotesColumn : UserControl
    {
        long timer = 0;
        public event EventHandler<MouseEventArgs> DoubleClick;
        public event EventHandler<EventArgs> RedrawMe;
        private int _Page;
        public NotesColumn(int page)
        {
            InitializeComponent();
            _Page = page;
            
        }
        public UIElementCollection Children
        {
            get
            {
                return ColumnContent.Children;
            }
        }
        public int Page
        {
            get {return _Page;}
        }

        private void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            long timer2 = DateTime.Now.Ticks;
            if (timer != 0 && timer2 - timer < 6000000)
            {//double click
                if (DoubleClick != null)
                {
                    DoubleClick.Invoke(this, e);
                }
            }
            timer = timer2;
            e.Handled = true;
        }
        public void Redraw()
        {
            if (RedrawMe != null) RedrawMe.Invoke(this, new EventArgs());
        }

    }
}
