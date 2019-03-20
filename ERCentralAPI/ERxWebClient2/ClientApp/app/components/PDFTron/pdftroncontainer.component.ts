import { Component, ViewChild, ElementRef, AfterViewInit, OnInit } from '@angular/core';
import { WebViewerComponent } from './webviewer.component';
import { Helpers } from '../helpers/HelperMethods';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemDocsService } from '../services/itemdocs.service';

declare const PDFTron: any;
declare let licenseKey: string;

@Component({
    selector: 'pdftroncontainer',
    template: '<app-webviewer style="height:100%;" ></app-webviewer>'
})
export class PdfTronContainer implements OnInit, AfterViewInit {
    constructor(private ReviewerIdentityServ: ReviewerIdentityService,
        private ItemDocsService: ItemDocsService
    ) { }
    @ViewChild(WebViewerComponent) private webviewer!: WebViewerComponent;

    ngOnInit() {
        this.wvReadyHandler = this.wvReadyHandler.bind(this);
        this.wvDocumentLoadedHandler = this.wvDocumentLoadedHandler.bind(this);
    }

    ngAfterViewInit() {
        this.webviewer.getElement().addEventListener('ready', this.wvReadyHandler);
        this.webviewer.getElement().addEventListener('documentLoaded', this.wvDocumentLoadedHandler);
        
    }
    async loadDoc(viewerInstance: any) {
        let counter: number = 0;
        
        console.log("viewerInstance ", viewerInstance);
        while (!viewerInstance == undefined && counter < 3 * 120) {
            counter++;
            await Helpers.Sleep(200);
            console.log("waiting, cycle n: " + counter);
        }
        if (this.ItemDocsService.CurrentDoc) viewerInstance.loadDocument(this.ItemDocsService.CurrentDoc, { filename: 'myfile.pdf' });
        else console.log("I'm giving up :-(");
    }
    wvReadyHandler(): void {
        // now you can access APIs through this.webviewer.getInstance()
        //this.webviewer.getInstance().openElement('notesPanel');
        // see https://www.pdftron.com/documentation/web/guides/ui/apis for the full list of APIs
        var viewerInstance = this.webviewer.getInstance();
        viewerInstance.setHeaderItems(function (header: any) {
            var items = header.getItems();
            console.log('items: ', items);
            items.splice(0, 3);
            items.splice(19, 1);
            items.splice(5, 13);
            items.splice(2, 1);
            header.update(items);
            console.log('items: ', header.getItems());
        });
        viewerInstance.disableTools(['AnnotationEdit',
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
        viewerInstance.docViewer.on('annotationsLoaded', () => {
            console.log('annotations loaded');
        });

        

        var docViewer = viewerInstance.docViewer;
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
        this.loadDoc(viewerInstance);
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
}
