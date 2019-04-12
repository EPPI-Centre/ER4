import { Component, ViewChild, ElementRef, AfterViewInit, OnInit } from '@angular/core';
import { WebViewerComponent } from './webviewer.component';
import { Helpers } from '../helpers/HelperMethods';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ItemCodingService, InPageSelection } from '../services/ItemCoding.service';
import { Console } from '@angular/core/src/console';

declare const PDFTron: any;
//declare const PDFNet: any; 
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
    private docViewer: any;
    annotManager: any;
    //PDFNet: any;
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
            items.splice(6, 13);
            //items.splice(2, 1);
            header.update(items);
            console.log('items: ', header.getItems());
        });
        this.viewerInstance.disableElements([
            'textUnderlineToolButton',
            'textSquigglyToolButton',
            'textStrikeoutToolButton',
            'annotationCommentButton',
            'annotationStyleEditButton'
        ]);

        this.viewerInstance.disableTools([
            //'AnnotationEdit',
            'AnnotationCreateSticky',
            'AnnotationCreateFreeHand',
            'AnnotationCreateTextHighlight',
            //'AnnotationCreateTextUnderline',
            //'AnnotationCreateTextSquiggly',
            //'AnnotationCreateTextStrikeout',
            //'AnnotationCreateFreeText',
            //'AnnotationCreateCallout',
            //'AnnotationCreateSignature',
            //'AnnotationCreateLine',
            //'AnnotationCreateArrow',
            //'AnnotationCreatePolyline',
            //'AnnotationCreateStamp',
            //'AnnotationCreateRectangle',
            //'AnnotationCreateEllipse',
            //'AnnotationCreatePolygon',
            //'AnnotationCreatePolygonCloud'
        ]); // hides DOM element + disables shortcut
        //this.PDFNet = this.webviewer.myWebViewer;
        //console.log(this.PDFNet);
        
        //this.PDFNet.initialize().then(()=> alert("initiated!"));
        // or listen to events from the viewer element
        this.webviewer.getElement().addEventListener('pageChanged', (e: any) => {
            const [pageNumber] = e.detail;
            console.log(`Current page is ${pageNumber}`);
        });

        // or from the docViewer instance
        this.viewerInstance.docViewer.on('annotationsLoaded', () => {
            console.log('annotations loaded');
        });

        

        this.docViewer = this.viewerInstance.docViewer;
        this.annotManager = this.docViewer.getAnnotationManager();
        this.annotManager.on('annotationChanged', function (event: any, annotations: any, action: any ) {
            if (action === 'add') {
                console.log('this is a change that added annotations', annotations, action);
                if (annotations[0].Quads) console.log("Quads: ", JSON.stringify(annotations[0].Quads));
                else console.log("No Quads");
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
        // you can access docViewer object for low-level APIs
        const { docViewer } = this.webviewer.getInstance();
        const all = this.webviewer.getInstance();
        //const annotManager = docViewer.getAnnotationManager();
        // and access classes defined in the WebViewer iframe
        const { Annotations } = this.webviewer.getWindow();
        const rectangle = new Annotations.RectangleAnnotation();
        this.viewerInstance.setFitMode("FitWidth");
        rectangle.PageNumber = 2;
        rectangle.X = 100;
        rectangle.Y = 100;
        rectangle.Width = 250;
        rectangle.Height = 250;
        rectangle.StrokeThickness = 5;
        rectangle.Author = this.annotManager.getCurrentUser();
        this.annotManager.addAnnotation(rectangle);
        this.annotManager.drawAnnotations(rectangle.PageNumber);
        // see https://www.pdftron.com/api/web/PDFTron.WebViewer.html for the full list of low-level APIs
    }

    buildHighlightsByGeometry(): void {
        console.log("buildHighlights", this.ItemCodingService.CurrentItemAttPDFCoding);
        //var annotManager = this.docViewer.getAnnotationManager();
        if (this.annotManager) {
            this.deleteAnnotations(this.annotManager);
        }
        if (this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding && this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding.length > 0) {
           
            var iFrame = document.querySelector('iframe');

            //try: https://www.pdftron.com/documentation/samples/js/text-position


            if (iFrame && iFrame.contentWindow) {
                //const Annotations = this.webviewer.getWindow();
                const { Annotations } = this.webviewer.getWindow();
                //annotManager.deleteAnnotations(Annotations);

                //(iFrame.contentWindow as any).Annotations = null;
                for (let sel of this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding) {
                    const Highl = new Annotations.TextHighlightAnnotation();
                    //console.log("created:", Highl);
                    let aQuads: any[] = [];
                    let rectSt0: string = sel.shapeTxt;
                    rectSt0 = rectSt0.substr(3);
                    let RectsA = rectSt0.split("zM");
                    for (let rectSt of RectsA) {
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
                        i = rectSt.length-1;
                        let y4 = +rectSt.substr(0, i) * 0.75;
                        rectSt = rectSt.substr(i + 1);
                        let quad = {
                            "x1": x1,
                            "x2": x2,
                            "x3": x3,
                            "x4": x4,
                            "y1": y1,
                            "y2": y2,
                            "y3": y3,
                            "y4": y4
                        }
                        aQuads.push(quad);
                        let rec = new Annotations.Rect(x1, y1, x3, y3);
                        console.log("Rect: ", rec, rec.getHeight(), rec.getWidth());
                        Highl.setContents(sel.inPageSelections[0].selTxt);
                        Highl.setRect(rec);
                        Highl.NoView = false;
                        Highl.Hidden = false;
                        //Highl.setWidth(rec.getWidth());
                        //Highl.setHeight(rec.getHeight());
                        //console.log("ann X, y", Highl.getX(), Highl.getY());
                        Highl.Author = this.annotManager.getCurrentUser();
                        Highl.StrokeColor = new Annotations.Color(136, 39, 31);
                        
                        //console.log("redrawAnnotation", Highl);
                        //annotManager.redrawAnnotation(Highl);

                        //const rectangle = new Annotations.RectangleAnnotation();
                        //rectangle.PageNumber = sel.page;
                        //rectangle.X = x1;
                        //rectangle.Y = y1;
                        //rectangle.Width = rec.getWidth();
                        //rectangle.Height = rec.getHeight();
                        //rectangle.StrokeThickness = 0.5;
                        //rectangle.Author = annotManager.getCurrentUser();
                        //annotManager.addAnnotation(rectangle);


                        
                    }
                    Highl.Quads = aQuads;
                    Highl.setPageNumber(sel.page);
                    //Highl.FillColor = new Annotations.Color(255, 234, 12);
                    Highl.StrokeColor = new Annotations.Color(250, 224, 6);
                    //Highl.$i = new Annotations.Color(247, 247, 227);
                    console.log("addAnnotation", Highl);
                    this.annotManager.addAnnotation(Highl);
                    this.annotManager.drawAnnotations(sel.page);
                    //annotManager.redrawAnnotation(Highl);
                    //console.log("AQQQ", (iFrame.contentWindow as any).Annotations);
                }
            }
        }
    }
    
    async buildHighlights() {
        if (this.annotManager) {
            this.deleteAnnotations(this.annotManager);
        }

        //what we need is:
        //create highlights based on the text, so that shapes "know" what text they belong to.
        //First bit is to cycle through let RectsA = rectSt0.split("zM");
        //for (let rectSt of RectsA) {...}
        //for each rectangle, find the corresponding string, get an arr of "selectedTxt" with its shape strings and a (now) empty field for the annotation.
        //create one annotation per element in Arr, put it into the element, show the annotation.

        //let's start by creating one TextWithShapes obj for each "sel.inPageSelections[0].selTxt"
        let AllContinuousSelections: TextWithShapes[] = [];
        let AllInputShapes: string[] = []; //will receive one string per page...

        var docViewer = this.webviewer.getInstance().docViewer;
        var GenDoc = docViewer.getDocument();
        let doc = await GenDoc.getPDFDoc();
        const PDFnet = this.webviewer.getPDFNet();
        try { await PDFnet.initialize(); }
        catch (error) { console.log("Error initialising PDFnet: ", error); }
        const reader = await this.webviewer.getPDFNet().ElementReader.create();
        const { Annotations } = this.webviewer.getWindow();
        console.log("PDFNet", PDFnet);

        if (this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding && this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding.length > 0) {
            for (let sel of this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding) {
                //from the DB, we get one ItemAttPDFCoding per page!
                AllInputShapes = sel.shapeTxt.substring(3).split("zM");
                AllContinuousSelections = [];
                for (let inPsel of sel.inPageSelections) {
                    let inPageTwS: TextWithShapes = new TextWithShapes();
                    inPageTwS.Page = sel.page;
                    inPageTwS.FullSelectedText = inPsel.selTxt.trim();
                    AllContinuousSelections.push(inPageTwS);
                    
                }
                //now we want to see which InputShapes belong to what TextWithShapes
                for (let i = 0; i < AllInputShapes.length; i++) {
                    let quad = TextWithShapes.BuildQuad(AllInputShapes[i]);
                    console.log("quad s:", quad);
                    let PDFpoint1 = GenDoc.getPDFCoordinates(sel.page - 1, quad.x1, quad.y1);
                    let PDFpoint2 = GenDoc.getPDFCoordinates(sel.page - 1, quad.x3, quad.y3);
                    let ydiff = (PDFpoint1.y - PDFpoint2.y) / 4;
                    PDFpoint1.y = PDFpoint1.y - ydiff;
                    PDFpoint2.y = PDFpoint2.y + ydiff;
                    console.log(PDFpoint1, PDFpoint2);
                    let rec = await PDFnet.Rect.init(PDFpoint2.x, PDFpoint2.y, PDFpoint1.x, PDFpoint1.y);
                    let textinSel = (await this.readTextFromRect(sel.page, rec, reader, doc, PDFnet)).trim();
                    console.log("looking for:", textinSel)
                    let myCurrentSelectIndex = AllContinuousSelections.findIndex(found =>  found.FullSelectedText.indexOf(textinSel) != -1);
                    console.log("my curr sel ind:", AllContinuousSelections.findIndex(found => found.FullSelectedText.indexOf(textinSel) != -1));
                    if (myCurrentSelectIndex != -1) {
                        //yay! the text underneath the current shape belongs to AllContinuousSelections[myCurrentSelectIndex]
                        AllContinuousSelections[myCurrentSelectIndex].ShapesStrings.push(AllInputShapes[i]);
                        //we found where the current shape belongs, so let's remove it
                        AllInputShapes.splice(i, 1);//this also means we don't need to increase i:
                        i--;
                        continue;
                    }
                    else {
                        console.log("did not find this string!!!!!!!\n\n Try again...\n\n", textinSel, AllInputShapes[i]);
                        //replace(/\r\n/g, '<br />')
                        if (textinSel.length > 6) textinSel = textinSel.substring(1, textinSel.length - 1);
                        textinSel = textinSel.replace(/[ \t]/g, '');
                        myCurrentSelectIndex = AllContinuousSelections.findIndex(
                            found => found.FullSelectedText.replace(/[ \t]/g, '')
                                .replace(/ﬀ/g, 'f‌f')
                                .replace(/ﬃ/g, 'f‌f‌i')
                                .replace(/ﬄ/g, 'f‌f‌l')
                                .replace(/ﬁ/g, 'f‌i')
                                .replace(/ﬂ/g, 'fl')
                                .replace(/ﬅ/g, 'ſt')
                                .replace(/Ꝡ/g, 'VY')
                                .replace(/ꝡ/g, 'vy')
                                .indexOf(textinSel) != -1
                        );
                        if (myCurrentSelectIndex != -1) {
                            //yay! the text underneath the current shape belongs to AllContinuousSelections[myCurrentSelectIndex]
                            AllContinuousSelections[myCurrentSelectIndex].ShapesStrings.push(AllInputShapes[i]);
                            //we found where the current shape belongs, so let's remove it
                            AllInputShapes.splice(i, 1);//this also means we don't need to increase i:
                            i--;
                            continue;
                        }
                    }
                }
                console.log("done one page, results of step 1:", AllContinuousSelections, AllInputShapes);
                if (AllInputShapes.length > 0) {
                    //something didn't work, if we have only one in AllInputShapes, find the corresponding empty AllContinuousSelections, fill it in and be done with it.
                    if (AllInputShapes.length == 1) {
                        for (let csel of AllContinuousSelections) {
                            if (csel.ShapesStrings.length == 0) {
                                //found it!
                                csel.ShapesStrings.push(AllInputShapes[0]);
                            }
                        }
                    }
                    else {
                        //find the empty AllContinuousSelections, get rid of them, create a blank new AllContinuousSelection with no text but the shapes.
                        for (let i = 0; i < AllContinuousSelections.length;) {
                            if (AllContinuousSelections[i].ShapesStrings.length == 0) {
                                AllContinuousSelections.splice(i, 1);
                            }
                            else i++;
                        }
                        let inPageTwS: TextWithShapes = new TextWithShapes();
                        inPageTwS.Page = sel.page;
                        inPageTwS.FullSelectedText = "";
                        for (let shape of AllInputShapes) {
                            inPageTwS.ShapesStrings.push(shape);
                        }
                        AllContinuousSelections.push(inPageTwS);
                    }
                }
                for (let pSel of AllContinuousSelections) {
                    
                    const Highl = new Annotations.TextHighlightAnnotation();
                    Highl.setContents(pSel.FullSelectedText);
                    Highl.NoView = false;
                    Highl.Hidden = false;
                    Highl.Author = this.annotManager.getCurrentUser();
                    Highl.setPageNumber(sel.page);
                    //Highl.FillColor = new Annotations.Color(255, 234, 12);
                    if (pSel.FullSelectedText.length > 0) {
                        Highl.StrokeColor = new Annotations.Color(250, 224, 6);
                    }
                    else {
                        Highl.StrokeColor = new Annotations.Color(255, 124, 6);
                    }

                    //Highl.$i = new Annotations.Color(247, 247, 227);
                    
                    if (pSel.ShapesStrings.length > 0) {
                        Highl.Quads = pSel.Quads;
                        console.log("addAnnotation", Highl);
                        this.annotManager.addAnnotation(Highl);
                    }
                    else console.log("broken ann:", Highl);
                }
                this.annotManager.drawAnnotations(sel.page);
            }
            //AllContinuousSelections now needs to receive one or more string description for a rectangle.
            //var iFrame = document.querySelector('iframe');
            return;
        }


        //console.log("buildHighlights", this.ItemCodingService.CurrentItemAttPDFCoding);
        //var annotManager = this.docViewer.getAnnotationManager();
        
        if (this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding && this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding.length > 0) {
            
            
            
            

            //const reader = await PDFnet.ElementReader.create();
            //try: https://www.pdftron.com/documentation/samples/js/text-position


            //if (iFrame && iFrame.contentWindow) {
                //const Annotations = this.webviewer.getWindow();
                const { Annotations } = this.webviewer.getWindow();
                
                console.log("Doc:", GenDoc);
                let Txt: string = "";
                
                let doc = await GenDoc.getPDFDoc();
                await GenDoc.loadPageText(0, (pageTxt: string) => { Txt = pageTxt });
                console.log("What doc?", doc);
                console.log("\n\n\n\n\n\n\nPage txt:\n", Txt, "\n\nEnd PAGE txt\n\n\n\n\n");
                doc.initSecurityHandler();
                // Locks all PNaCl operations on the document
                doc.lock();

                // insert user code after this point
                var pgnum = await doc.getPageCount();
                //alert("Test Complete! Your file has " + pgnum + " pages");


                //annotManager.deleteAnnotations(Annotations);
                //let elCnt = await PDFnet.runWithCleanup(async () => await this.TestAPICall(1, "", doc));
                //console.log("page has " + elCnt + " elements!\n\n\n\n\n");
                //(iFrame.contentWindow as any).Annotations = null;
                for (let sel of this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding) {
                    


                    const Highl = new Annotations.TextHighlightAnnotation();
                    //console.log("created:", Highl);
                    let aQuads: any[] = [];
                    let rectSt0: string = sel.shapeTxt;
                    rectSt0 = rectSt0.substr(3);
                    let RectsA = rectSt0.split("zM");
                    for (let rectSt of RectsA) {
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
                        i = rectSt.length - 1;
                        let y4 = +rectSt.substr(0, i) * 0.75;
                        rectSt = rectSt.substr(i + 1);
                        let quad = {
                            "x1": x1,
                            "x2": x2,
                            "x3": x3,
                            "x4": x4,
                            "y1": y1,
                            "y2": y2,
                            "y3": y3,
                            "y4": y4
                        }
                        aQuads.push(quad);
                        //let rec = new Annotations.Rect(x1, y1, x3, y3);
                        let PDFpoint1 = GenDoc.getPDFCoordinates(sel.page - 1, x1, y1);
                        let PDFpoint2 = GenDoc.getPDFCoordinates(sel.page - 1, x3, y3);
                        let ydiff = (PDFpoint1.y - PDFpoint2.y)/4;
                        PDFpoint1.y = PDFpoint1.y - ydiff;
                        PDFpoint2.y = PDFpoint2.y + ydiff;
                        console.log(PDFpoint1, PDFpoint2);
                        let rec = await PDFnet.Rect.init(PDFpoint2.x, PDFpoint2.y, PDFpoint1.x, PDFpoint1.y);
                        let textinSel = await this.readTextFromRect(sel.page, rec, reader, doc, PDFnet);
                        console.log("Got some text:\n" + textinSel);
                        //console.log("Rect: ", rec, rec.getHeight(), rec.getWidth());
                        Highl.setContents(sel.inPageSelections[0].selTxt);
                        Highl.setRect(rec);
                        Highl.NoView = false;
                        Highl.Hidden = false;
                        //Highl.setWidth(rec.getWidth());
                        //Highl.setHeight(rec.getHeight());
                        //console.log("ann X, y", Highl.getX(), Highl.getY());
                        Highl.Author = this.annotManager.getCurrentUser();
                        Highl.StrokeColor = new Annotations.Color(136, 39, 31);

                        //console.log("redrawAnnotation", Highl);
                        //annotManager.redrawAnnotation(Highl);

                        //const rectangle = new Annotations.RectangleAnnotation();
                        //rectangle.PageNumber = sel.page;
                        //rectangle.X = x1;
                        //rectangle.Y = y1;
                        //rectangle.Width = rec.getWidth();
                        //rectangle.Height = rec.getHeight();
                        //rectangle.StrokeThickness = 0.5;
                        //rectangle.Author = annotManager.getCurrentUser();
                        //annotManager.addAnnotation(rectangle);



                    }
                    for (let InPgSel of sel.inPageSelections) {
                        //this.findShapeFromText(InPgSel, sel.page, doc, aQuads,
                        //    Annotations, this.webviewer.getInstance(), this.annotManager);
                    }


                    ////Highl.Quads = aQuads;
                    //Highl.setPageNumber(sel.page);
                    ////Highl.FillColor = new Annotations.Color(255, 234, 12);
                    //Highl.StrokeColor = new Annotations.Color(250, 224, 6);
                    ////Highl.$i = new Annotations.Color(247, 247, 227);
                    //console.log("addAnnotation", Highl);
                    //annotManager.addAnnotation(Highl);
                    this.annotManager.drawAnnotations(sel.page);
                    //annotManager.redrawAnnotation(Highl);
                    //console.log("AQQQ", (iFrame.contentWindow as any).Annotations);
                }
            //}
        }
    }

    findShapeFromText(InPgSel: InPageSelection, page:number, doc: any, shapesToMatch: any[],
        Annotations: any, ViewerInstance: any, annotationManager: any): any {
        //this is a failed attempt: page indexes are just different :-((((
        //to make things worse, doc.getTextPosition returns quads on a per-char basis, so we can't compare like for li
        doc.loadPageText(page-1, function (text: string) {
            var textStartIndex = 0;
            var textIndex;
            var annotations: any[] = [];
            console.log("Searching...", shapesToMatch, "InPgSel: " + InPgSel.start, InPgSel.end, InPgSel.selTxt);// , "page: " + page, "fullpage: " + text);
            
                //console.log("Found text", InPgSel.selTxt, textIndex);
                
                // Get text position
            doc.getTextPosition(Number(page - 1), InPgSel.start, InPgSel.start + InPgSel.selTxt.length, function (quads: any) {
                    console.log("got the pos:", quads);
                for (let shapeToMatch of shapesToMatch) {
                    let minx = Math.min(shapeToMatch.x1, shapeToMatch.x2, shapeToMatch.x3, shapeToMatch.x4);
                    let maxx = Math.max(shapeToMatch.x1, shapeToMatch.x2, shapeToMatch.x3, shapeToMatch.x4);
                    let miny = Math.min(shapeToMatch.y1, shapeToMatch.y2, shapeToMatch.y3, shapeToMatch.y4);
                    let maxy = Math.max(shapeToMatch.y1, shapeToMatch.y2, shapeToMatch.y3, shapeToMatch.y4);
                    console.log("minMaxes:", minx, miny, maxx, maxy);
                    let minPindex = quads.findIndex((found: any) => (Math.abs(minx - found.x1) < 0.95 && Math.abs(miny - found.y3) < 1.5));
                    let maxPindex = quads.findIndex((found: any) => (Math.abs(maxx - found.x1) < 0.95 && Math.abs(maxy - found.y3) < 1.5));
                    if (minPindex > -1 && maxPindex > -1)
                        //if (Math.abs(shapeToMatch.x1 - quads[0].x1) < 0.95
                        //    //&& Math.abs(shapeToMatch.x3 - quads[quads.length - 1].x3) < 0.95
                        //    //&& Math.abs(shapeToMatch.y1 - quads[0].y3) < 1.5
                        //    //&& Math.abs(shapeToMatch.y3 - quads[quads.length - 1].y1) < 1.5
                        //)
                        {
                            console.log("We found a match for ", shapeToMatch);
                            var annotation = new Annotations.TextHighlightAnnotation();
                            annotation.Author = ViewerInstance.getAnnotationUser();
                            annotation.PageNumber = page;
                            annotation.Quads = [shapeToMatch];
                            annotation.StrokeColor = new Annotations.Color(250, 224, 6);
                            annotations.push(annotation);
                            break;
                        }
                    }


                    //this doesn't work, the quads are one per letter, not the whole selection :-(
                    //for (let foundQ of quads) {
                    //    let foundIndex = shapesToMatch.findIndex((found) => (Math.abs(found.x1 - foundQ.x1) < 0.1 && Math.abs(found.y3 - foundQ.y3) < 0.1)) ;
                    //    if (foundIndex > -1) {
                    //        console.log("Quad was found:", foundQ);
                    //        var annotation = new Annotations.TextHighlightAnnotation();
                    //        annotation.Author = ViewerInstance.getAnnotationUser();
                    //        annotation.PageNumber = page;
                    //        annotation.Quads = quads;
                    //        annotation.StrokeColor = new Annotations.Color(250, 224, 6);
                    //        annotations.push(annotation);
                    //    }
                    //}
                    
                });
            
            annotationManager.addAnnotations(annotations);
            //annotationManager.selectAnnotations(annotations);
        });
    }
    private deleteAnnotations(annotManager: any) {
        //console.log("deleteAnnotations");
        let Annots: any[] = annotManager.getAnnotationsList();
        //if (Annots) console.log("Annots to delete:", Annots, Annots.length);
        let count = Annots.length;
        let i = 0;
        for (i = 0; i < count; i++) {
            let annot = Annots[0];
            //console.log("deleting annot: " + (i+1) + " of " + count, annot);
            try {
                annotManager.deleteAnnotation(annot, true, true);
            }
            catch (e) {
                console.log("error deleting annot:", e);
            }
        }
        console.log("Annots after deleting:", annotManager.getAnnotationsList(), annotManager.getAnnotationsList().length);
        console.log(""); console.log("");
    }

    async TestAPICall(page: number, pos: any, doc: any) {
        const reader = await this.webviewer.getPDFNet().ElementReader.create();
        let count = 0;
        let element;
        let PDFpage = await doc.getPage(page );
        console.log("page: ", page, PDFpage);
        
        reader.beginOnPage(PDFpage);
        while ((element = await reader.next()) !== null) {
            count++;
        }
        reader.end();
        return count;
    }

    private async readTextFromRect(page: number, pos: any, reader: any, doc: any, PDFnet: any) {
        //let elCnt = await PDFnet.runWithCleanup(async () => await this.TestAPICall(1, "", doc));
        let srchStr = '';
        //reader.beginOnPage(doc.getPage(page -1)); // uses default parameters.
        console.log("starting readTextFromRect");
        //srchStr += await PDFnet.runWithCleanup(async () => this.rectTextSearch(page, true, reader, pos, srchStr, doc, PDFnet));
        srchStr += await PDFnet.runWithCleanup(async () => this.IntersectByLines(page, true, reader, pos, srchStr, doc, PDFnet));

        //reader.end();
        console.log("found some text!", srchStr);
        return srchStr;
    }
    private async IntersectByLines(page: number, startReader: boolean, reader: any, pos: any, srchStr: string, doc: any, PDFnet: any) {
        const TextExtractor = await PDFnet.TextExtractor.create();
        let intersectLinesText = "";
        TextExtractor.begin(await doc.getPage(page), pos);
        //if (startReader) reader.beginOnPage(await doc.getPage(page));
        for (let line = await TextExtractor.getFirstLine(); await line.isValid(); line = await line.getNextLine()) {
            let bbox0;
            bbox0 = await line.getBBox();
            if (bbox0 && await bbox0.intersectRect(bbox0, pos)) {
                //current line intersects with our rectangle.
                for (let word = await line.getFirstWord(); (await word.isValid()); word = (await word.getNextWord())) {
                    
                    //console.log(intersectLinesText);
                    let bbox = await word.getBBox();
                    if (bbox) {
                        let intersectBB = await bbox.intersectRect(bbox, pos);
                        //console.log("intersectBB:", bbox.y1, bbox.y2, pos.y1, pos.y2);
                        bbox.normalize();
                        pos.normalize();
                        //console.log("intersectBB Norm:", bbox.y1, bbox.y2, bbox.x1, bbox.x2, "\n", pos.y1, pos.y2, pos.x1, pos.x2);
                        if (intersectBB)
                        {
                            //the word in this line intersects with our rectangle!
                            //we now check if the intersect is substantial.
                            //1. get the intersection "y coords"

                            let ItrsY1 = Math.max(bbox.y1, pos.y1);
                            let ItrsY2 = Math.min(bbox.y2, pos.y2);
                            if (ItrsY2 - ItrsY1 > (bbox.y2 - bbox.y1) / 2.1) {
                                //intersection is 50% or more, it's substantial!
                                //qwertyuiopasdfghjklzxcvbnm
                                //qypgj
                                //tidfhjkl
                                //console.log("found word:", word);
                                intersectLinesText += await word.getString() + " ";
                            }
                        }
                    }
                }
            }
        }
        return intersectLinesText;
    }
    private async rectTextSearch(page: number, startReader: boolean, reader: any, pos: any, srchStr: string, doc: any, PDFnet: any) {
        console.log("starting rectTextSearch \n", page, startReader, reader, pos, srchStr, doc, PDFnet);
        if (startReader) reader.beginOnPage(await doc.getPage(page));
        let element, arr;
        while ((element = await reader.next()) !== null) {
            let bbox;
            let elType = await element.getType();
            //console.log("type =", elType);
            switch (elType) {
                case PDFnet.Element.Type.e_text:
                    bbox = await element.getBBox();
                    //console.log("Rectangles:", bbox, pos);
                    if (bbox && await bbox.intersectRect(bbox, pos)) {
                        arr = await element.getTextString();
                        //if (arr && arr.length > 0) console.log("el text:", arr);
                        srchStr += arr + '\n';
                    }
                    break;
                case PDFnet.Element.Type.e_text_new_line:
                    break;
                case PDFnet.Element.Type.e_form:
                    reader.formBegin();
                    srchStr += await this.rectTextSearch(page, false, reader, pos, srchStr, doc, PDFnet); // possibly need srchStr = ...
                    reader.end();
                    break;
            }
        }
        reader.end();
        return srchStr;
    }
}
export class TextWithShapes {
    FullSelectedText: string = "";
    ShapesStrings: string[] = [];
    Annotation: any = null;
    Page: number = -1;
    private _Quads: any[] = [];
    public get Quads(): any[] {
        if (this._Quads.length == 0 && this.ShapesStrings.length > 0) {
            //we don't have the quads, let's build them.
            for (let Qst of this.ShapesStrings) {
                this._Quads.push(TextWithShapes.BuildQuad(Qst));
            }
        }
        return this._Quads;
    }
    //public static BuildQuads(shapeTxt: string): any {
    //    let aQuads: any[] = [];
    //    let rectSt0 = shapeTxt.substr(3);
    //    let RectsA = rectSt0.split("zM");
    //    for (let rectSt of RectsA) {
    //        aQuads.push(TextWithShapes.BuildQuad(rectSt));
    //    }
    //    return aQuads;
    //}
    public static BuildQuad(SingleRectStr: string): any {
        let i = SingleRectStr.indexOf(',');
        let x1 = +SingleRectStr.substr(0, i) * 0.75;
        SingleRectStr = SingleRectStr.substr(i + 1);
        i = SingleRectStr.indexOf('L');
        let y1 = +SingleRectStr.substr(0, i) * 0.75;
        SingleRectStr = SingleRectStr.substr(i + 1);

        i = SingleRectStr.indexOf(',');
        let x2 = +SingleRectStr.substr(0, i) * 0.75;
        SingleRectStr = SingleRectStr.substr(i + 1);
        i = SingleRectStr.indexOf('L');
        let y2 = +SingleRectStr.substr(0, i) * 0.75;
        SingleRectStr = SingleRectStr.substr(i + 1);

        i = SingleRectStr.indexOf(',');
        let x3 = +SingleRectStr.substr(0, i) * 0.75;
        SingleRectStr = SingleRectStr.substr(i + 1);
        i = SingleRectStr.indexOf('L');
        let y3 = +SingleRectStr.substr(0, i) * 0.75;
        SingleRectStr = SingleRectStr.substr(i + 1);

        i = SingleRectStr.indexOf(',');
        let x4 = +SingleRectStr.substr(0, i) * 0.75;
        SingleRectStr = SingleRectStr.substr(i + 1);
        i = SingleRectStr.length - 1;
        let y4 = +SingleRectStr.substr(0, i) * 0.75;
        //SingleRectStr = SingleRectStr.substr(i + 1);
        let quad = {
            "x1": x1,
            "x2": x2,
            "x3": x3,
            "x4": x4,
            "y1": y1,
            "y2": y2,
            "y3": y3,
            "y4": y4
        };
        return quad;
    }
}
