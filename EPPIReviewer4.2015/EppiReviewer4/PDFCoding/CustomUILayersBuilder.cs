using System;
using Telerik.Windows.Documents.Fixed.UI.Layers;

namespace EppiReviewer4
{
    public class CustomUILayersBuilder : UILayersBuilder
    {
        public CustomUILayersBuilder(Highlights highlights, NotesCs ntCs, dialogCoding father)
        {
            this.highlights = highlights;
            this.ntCs = ntCs;
            this.dg = father;
        }
        private readonly dialogCoding dg;
        private readonly Highlights highlights;
        private readonly NotesCs ntCs;
        protected override void BuildUILayersOverride(IUILayerContainer uiLayerContainer)
        {
            HighlightUILayer hui = new HighlightUILayer(this.highlights,  dg);
            uiLayerContainer.UILayers.AddAfter(DefaultUILayers.ContentElementsUILayer, hui);

            uiLayerContainer.UILayers.AddLast(new AnnotationsUILayer(this.ntCs));
            
        }
    }
}
