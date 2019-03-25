import { Component, ViewChild, ElementRef, AfterViewInit, OnInit } from '@angular/core';
import { WebViewerComponent } from './webviewer.component';
import { Helpers } from '../helpers/HelperMethods';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ItemCodingService } from '../services/ItemCoding.service';

declare const PDFTron: any;
declare let licenseKey: string;

@Component({
    selector: 'pdftroncontainer',
    template: '<app-webviewer style="height:100%;" ></app-webviewer>'
})
export class PdfTronContainer implements OnInit, AfterViewInit {
    constructor(private ReviewerIdentityServ: ReviewerIdentityService,
        private ItemDocsService: ItemDocsService,
        private ItemCodingService: ItemCodingService
    ) { }
    @ViewChild(WebViewerComponent) private webviewer!: WebViewerComponent;
    private viewerInstance: any;
    ngOnInit() {
        this.wvReadyHandler = this.wvReadyHandler.bind(this);
        this.wvDocumentLoadedHandler = this.wvDocumentLoadedHandler.bind(this);
    }

    ngAfterViewInit() {
        this.webviewer.getElement().addEventListener('ready', this.wvReadyHandler);
        this.webviewer.getElement().addEventListener('documentLoaded', this.wvDocumentLoadedHandler);
        this.ItemCodingService.ItemAttPDFCodingChanged.subscribe(() => this.buildHighlights());
    }
    async loadDoc() {
        if (!this.viewerInstance) {
            //return?
        }
        let counter: number = 0;
        
        console.log("viewerInstance ", this.viewerInstance);
        while ((!this.viewerInstance ||!this.ItemDocsService.CurrentDoc) && counter < 3 * 120) {
            counter++;
            await Helpers.Sleep(200);
            console.log("waiting, cycle n: " + counter);
        }
        if (this.ItemDocsService.CurrentDoc) this.viewerInstance.loadDocument(this.ItemDocsService.CurrentDoc);
        else console.log("I'm giving up :-(");
    }
    wvReadyHandler(): void {
        // now you can access APIs through this.webviewer.getInstance()
        //this.webviewer.getInstance().openElement('notesPanel');
        // see https://www.pdftron.com/documentation/web/guides/ui/apis for the full list of APIs
        this.viewerInstance = this.webviewer.getInstance();
        this.viewerInstance.setHeaderItems(function (header: any) {
            var items = header.getItems();
            //console.log('items: ', items);
            items.splice(0, 3);
            items.splice(19, 1);
            items.splice(5, 13);
            items.splice(2, 1);
            header.update(items);
            console.log('items: ', header.getItems());
        });
        this.viewerInstance.disableTools(['AnnotationEdit',
        //    'AnnotationCreateSticky',
        //    'AnnotationCreateFreeHand',
        //    'AnnotationCreateTextHighlight',
        //    'AnnotationCreateTextUnderline',
        //    'AnnotationCreateTextSquiggly',
        //    'AnnotationCreateTextStrikeout',
        //    'AnnotationCreateFreeText',
        //    'AnnotationCreateCallout',
        //    'AnnotationCreateSignature',
        //    'AnnotationCreateLine',
        //    'AnnotationCreateArrow',
        //    'AnnotationCreatePolyline',
        //    'AnnotationCreateStamp',
        //    'AnnotationCreateRectangle',
        //    'AnnotationCreateEllipse',
        //    'AnnotationCreatePolygon',
            'AnnotationCreatePolygonCloud']); // hides DOM element + disables shortcut

        // or listen to events from the viewer element
        this.webviewer.getElement().addEventListener('pageChanged', (e: any) => {
            const [pageNumber] = e.detail;
            console.log(`Current page is ${pageNumber}`);
        });

        // or from the docViewer instance
        this.viewerInstance.docViewer.on('annotationsLoaded', () => {
            console.log('annotations loaded');
        });

        

        var docViewer = this.viewerInstance.docViewer;
        var annotManager = docViewer.getAnnotationManager();
        annotManager.on('annotationChanged', function (event: any, annotations: any, action: any ) {
            if (action === 'add') {
                console.log('this is a change that added annotations', annotations, action);
            } else if (action === 'modify') {
                console.log('this change modified annotations');
            } else if (action === 'delete') {
                console.log('there were annotations deleted');
            }

            annotations.forEach(function (annot: any) {
                console.log('annotation page number', annot.PageNumber);
            });
        });
        //this.loadDoc();
    }

    wvDocumentLoadedHandler(): void {
        //// you can access docViewer object for low-level APIs
        //const { docViewer } = this.webviewer.getInstance();
        //const annotManager = docViewer.getAnnotationManager();
        //// and access classes defined in the WebViewer iframe
        //const { Annotations } = this.webviewer.getWindow();
        //const rectangle = new Annotations.RectangleAnnotation();
        //rectangle.PageNumber = 2;
        //rectangle.X = 100;
        //rectangle.Y = 100;
        //rectangle.Width = 250;
        //rectangle.Height = 250;
        //rectangle.StrokeThickness = 5;
        //rectangle.Author = annotManager.getCurrentUser();
        //annotManager.addAnnotation(rectangle);
        //annotManager.drawAnnotations(rectangle.PageNumber);
        // see https://www.pdftron.com/api/web/PDFTron.WebViewer.html for the full list of low-level APIs
    }

    buildHighlights(): void {
        console.log("buildHighlights", this.ItemCodingService.CurrentItemAttPDFCoding);
        if (this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding) {
            var docViewer = this.viewerInstance.docViewer;
            var annotManager = docViewer.getAnnotationManager();
            var iFrame = document.querySelector('iframe');
            if (iFrame && iFrame.contentWindow) {
                var Annotations = (iFrame.contentWindow as any).Annotations;
                (iFrame.contentWindow as any).Annotations = null;
                for (let sel of this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding) {
                    var Highl = new Annotations.TextHighlightAnnotation();
                    
                    Highl.setPageNumber(sel.page);
                    console.log("created:", Highl);
                    //"F1M66.8976,140.856L368.306,140.856L368.306,160.47L66.8976,160.47z"
                    let rectSt: string = sel.shapeTxt;
                    rectSt = rectSt.substr(3);
                    let i = rectSt.indexOf(',');
                    let x1 = +rectSt.substr(0, i) * 0.75;
                    rectSt = rectSt.substr(i + 1);
                    i = rectSt.indexOf('L');
                    let y1 = +rectSt.substr(0, i) * 0.75;
                    rectSt = rectSt.substr(i + 1);

                    i = rectSt.indexOf(',');
                    let x2 = +rectSt.substr(0, i) * 0.75;
                    rectSt = rectSt.substr(i + 1);
                    i = rectSt.indexOf('L');
                    let y2 = +rectSt.substr(0, i) * 0.75;
                    rectSt = rectSt.substr(i + 1);

                    i = rectSt.indexOf(',');
                    let x3 = +rectSt.substr(0, i) * 0.75;
                    rectSt = rectSt.substr(i + 1);
                    i = rectSt.indexOf('L');
                    let y3 = +rectSt.substr(0, i) * 0.75;
                    rectSt = rectSt.substr(i + 1);

                    i = rectSt.indexOf(',');
                    let x4 = +rectSt.substr(0, i) * 0.75;
                    rectSt = rectSt.substr(i + 1);
                    i = rectSt.indexOf('L');
                    let y4 = +rectSt.substr(0, i) * 0.75;
                    rectSt = rectSt.substr(i + 1);
                    let rec = new Annotations.Rect(x1, y1, x3, y3);
                    console.log("Rect: ", rec);
                    Highl.setRect(rec);
                    Highl.setContents(sel.inPageSelections[0].selTxt);
                    console.log("addAnnotation", Highl);
                    annotManager.addAnnotation(Highl);
                    console.log("redrawAnnotation");
                    annotManager.redrawAnnotation(Highl);
                    console.log("AQQQ", (iFrame.contentWindow as any).Annotations);
                }
            }
        }
    }
}
