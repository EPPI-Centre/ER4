using System;
using Telerik.Windows.Documents.Fixed.UI.Layers;
using Telerik.Windows.Documents.Fixed.Model;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace EppiReviewer4
{
    public class HighlightUILayer : IUILayer
    {
        #region Constructors

        public HighlightUILayer(Highlights highlights, dialogCoding father)
        {
            this.highlights = highlights;
            this.container = new Canvas();
            father.highlightsChanged += new EventHandler<EventArgs>(Redraw);
        }

        #endregion


        #region Fields

        private RadFixedPage page;
        private readonly Canvas container;
        private readonly Highlights highlights;
        private Path path;
        
        #endregion


        #region Properties

        public string Name
        {
            get { return "HighlightsLayer"; }
        }

        public Canvas UIElement
        {
            get { return this.container; }
        }

        #endregion


        #region Methods

        public void Clear()
        {
            this.path.Data = null;
            this.path = null;
            this.container.Children.Clear();
        }

        public void Initialize(UILayerInitializeContext context)
        {
            this.page = context.Page;
            this.path = new Path { Fill = new SolidColorBrush(Color.FromArgb(99, 255, 255, 0)) };
            this.UIElement.Children.Add(this.path);
        }

        public void Update(UILayerUpdateContext context)
        {
            if (this.highlights.ContainsKey(this.page) && this.path != null && this.path.Data != this.highlights[this.page])
            {
                this.path.Data = this.highlights[this.page];                
            }
        }
        private void Redraw(object sender, EventArgs e)
        {
            if (this.highlights.ContainsKey(this.page) && this.path != null && this.path.Data != this.highlights[this.page])
            {
                this.path.Data = this.highlights[this.page];
            }
            else if (!this.highlights.ContainsKey(this.page) && this.path != null)
            {
                this.path.Data = null;
            }
        }
        #endregion
    }
}
