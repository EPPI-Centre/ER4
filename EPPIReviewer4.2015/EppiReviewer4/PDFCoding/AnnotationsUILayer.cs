using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Documents.Fixed.UI.Layers;
using Telerik.Windows.Documents.Fixed.Model;

namespace EppiReviewer4
{
    public class AnnotationsUILayer : IUILayer
    {
        #region Constructors

        public AnnotationsUILayer(NotesCs ntCs)
        {
            this.ntCs = ntCs;
            this.container = new Canvas();
        }

        #endregion


        #region Fields
        private RadFixedPage page;
        private readonly Canvas container;
        private readonly NotesCs ntCs;

        #endregion


        #region Properties

        public string Name
        {
            get { return "NotesLayer"; }
        }

        public Canvas UIElement
        {
            get { return this.container; }
        }

        #endregion


        #region Methods
        private bool done = false;
        public void Clear()
        {
            //this.annotations = null;
            this.container.Children.Clear();
        }

        public void Initialize(UILayerInitializeContext context)
        {
            this.page = context.Page;
        }

        public void Update(UILayerUpdateContext context)
        {
            if (!done)
            {
                doit();
            }
            else
            {
                done = false;
            }
            //doit();
        }
        private void doit()
        {
            if (this.ntCs != null && this.ntCs.ContainsKey(this.page))
            {
                if (UIElement.Children != null
                    &&
                    (
                        (UIElement.Children.Count > 0 && UIElement.Children[0] != ntCs[this.page])
                        ||
                        (UIElement.Children.Count == 0)
                    )
                    )
                {
                    UIElement.Children.Clear();
                    UIElement.Children.Add(ntCs[this.page]);
                    Canvas.SetLeft(ntCs[this.page], page.ActualWidth - 95);
                }
                    ntCs[this.page].RedrawMe -= ann_dragger;
                ntCs[this.page].RedrawMe += new EventHandler<EventArgs>(ann_dragger);
                ntCs[this.page].Height = page.ActualHeight;
                //ntCs[this.page].Width = 95;
                foreach (xAnnotation ann in ntCs[this.page].ColumnContent.Children)
                {
                    Canvas.SetTop(ann, ann.Y);
                    ann.dragger -= ann_dragger;
                    ann.dragger += new EventHandler<EventArgs>(ann_dragger);
                }
                done = true;
            }
        }
        void ann_dragger(object sender, EventArgs e)
        {
            doit();
        }

        #endregion
    }
}
