using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Documents.UI;
using Telerik.Windows.Documents.Fixed.UI.Extensibility;
using Telerik.Windows.Controls;

namespace EppiReviewer4
{
    public partial class xAnnotation : UserControl
    {
        private int _page;
        bool isMouseCaptured;
        double mouseVerticalPosition;
        double mouseHorizontalPosition;
        double maxdown;
        long timer;
        long _ID;
        public bool isDeleted = false; 
        private bool  hasChanged = false;
        public event EventHandler<EventArgs> dragged;
        public event EventHandler<EventArgs> dragger;
        public event EventHandler<MouseEventArgs> DoubleClick;
        public xAnnotation()
        {
            InitializeComponent();

        }
        public xAnnotation(int page, double Y, string Content, long ID)
        {
            InitializeComponent();
            _page = page;
            _ID = ID;
            text.Text = Content;
            Canvas.SetTop(element, Y);
        }
        public xAnnotation(string XML, long ID)
        {
            InitializeComponent();
            XElement me = XElement.Parse(XML);
            ParseXML(me, ID) ;
        }
        public xAnnotation(XElement me, long ID)
        {
            InitializeComponent();
            ParseXML(me, ID);
        }
        private void ParseXML(XElement me, long ID)
        {
            _page = (int)me.Element("PageNumber");
            text.Text = (string)me.Element("PopupContents");
            double h = (double)me.Element("Y");
            if ((string)me.Element("Subject") != null)
            {//it is still in the old format, need to adjust height
                int len = text.Text.Length;
                h = h - 14 * (len / 9);//9 = avg # of char per line, 14 = line height
                if (h < 0) h = 0;
            }
            Canvas.SetTop(element, h);
            _ID = ID;
        }
        public XElement ToXML
        {
            get
            {
                XElement me =
                    new XElement("Annotation",
                                 new XElement("PageNumber", _page.ToString())
                                 , new XElement("PopupContents", text.Text)
                                 , new XElement("Y", Y.ToString())
                                );
                return me;
            }
        }
        public double Y
        {
            get
            {
                return Canvas.GetTop(element);
            }
            set
            {
                Canvas.SetTop(element, Y);
            }
        }
        public int page
        {
            get { return _page; }
        }
        public string Text
        {
            get { return text.Text; }
            set { text.Text = value; }
        }


        private void Rectangle_MouseMove(object sender, MouseEventArgs args)
        {
            if (isMouseCaptured)
            {
                
                Grid item = sender as Grid;
                Grid uc = (((item.Parent as UserControl).Parent as Canvas).Parent as Border).Parent as Grid;
                // Calculate the current position of the object.
                double deltaV = args.GetPosition(null).Y - mouseVerticalPosition;
                double deltaH = args.GetPosition(null).X - mouseHorizontalPosition;
                double newTop = deltaV + (double)item.GetValue(Canvas.TopProperty);
                hasChanged = true;
                if (newTop < 0) newTop = 2;
                else if (newTop > maxdown) newTop = maxdown-2;

                // Set new position of object.
                item.SetValue(Canvas.TopProperty, newTop);
                
                // Update position global variables.
                mouseVerticalPosition = args.GetPosition(null).Y;
                mouseHorizontalPosition = args.GetPosition(null).X;
                if (dragger != null) dragger.Invoke(sender, args);
            }
        }
        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            long timer2 = DateTime.Now.Ticks;
            if (timer != 0 && timer2 - timer < 10000000) //6000000
            {//double click
                timer = timer2;
                if (DoubleClick != null)
                {
                    DoubleClick.Invoke(this, e);
                }
            }
            else
            {
                timer = timer2;
            }
            if (hasChanged)
            {
                hasChanged = false;
                if (dragged != null) dragged.Invoke(sender, new EventArgs());
            }
            if (isMouseCaptured)
            {
                isMouseCaptured = false;
                Grid item = sender as Grid;
                item.ReleaseMouseCapture();
                mouseVerticalPosition = -1;
                mouseHorizontalPosition = -1;
            }
            e.Handled = true;
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            Grid item = sender as Grid;
            Grid uc = (((item.Parent as UserControl).Parent as Canvas).Parent as Border).Parent as Grid;
            maxdown = uc.ActualHeight - item.ActualHeight;
            mouseVerticalPosition = args.GetPosition(null).Y;
            mouseHorizontalPosition = args.GetPosition(null).X;
            isMouseCaptured = true;
            item.CaptureMouse();
            args.Handled = true;
        }

        private void element_LostMouseCapture(object sender, MouseEventArgs e)
        {
            long timer2 = DateTime.Now.Ticks;
            Grid item = sender as Grid;
            if (hasChanged)
            {
                hasChanged = false;
                if (dragged != null)  dragged.Invoke(sender, new EventArgs());
            }
            isMouseCaptured = false;

            item.ReleaseMouseCapture();
            mouseVerticalPosition = -1;
            mouseHorizontalPosition = -1;
            timer = timer2 - 6000001;
        }
    }
}
