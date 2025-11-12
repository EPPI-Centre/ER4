import { Component, ViewChild, ElementRef, AfterViewInit, OnInit, Input, OnDestroy, NgZone } from '@angular/core';
import { WebViewerComponent } from './webviewer.component';
import { Helpers } from '../helpers/HelperMethods';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ItemCodingService, InPageSelection, ItemAttPDFCodingCrit, ItemSet } from '../services/ItemCoding.service';
import { ArmTimepointLinkListService } from '../services/ArmTimepointLinkList.service';
import { ReviewSetsService, ItemAttributeSaveCommand } from '../services/ReviewSets.service';
import { Item } from '../services/ItemList.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { Subscription } from 'rxjs';
import { ModalService } from '../services/modal.service';

declare const PDFTron: any;
//declare const PDFNet: any; 
declare let licenseKey: string;

@Component({
    selector: 'pdftroncontainer',
    templateUrl: './pdftroncontainer.component.html'
})
export class PdfTronContainer implements OnInit, AfterViewInit, OnDestroy {
    constructor(private ReviewerIdentityServ: ReviewerIdentityService,
        private ItemDocsService: ItemDocsService,
        private ItemCodingService: ItemCodingService,
        private armsService: ArmTimepointLinkListService,
        private ReviewSetsService: ReviewSetsService,
        private ReviewInfoService: ReviewInfoService,
        private ngZone: NgZone,
        private modalService: ModalService
    ) { }
    
    @ViewChild(WebViewerComponent) private webviewer!: WebViewerComponent;
    @Input() ItemID: number = 0;
    private viewerInstance: any;
    private docViewer: any;
    private annotManager: any;
    public AvoidHandlingAnnotationChanges: boolean = false;
    private LoadingNewDoc = false;
    private AlsoBuildHighlights: boolean = false;//used to instruct wvAnnotationsLoaded
    private AttachwvAnnotationsLoaded = true;
    private PDFnet: any;
    private subBuildHighlights: Subscription | null = null;
    private _currentDocId: number = 0;
    //used by the parent, to figure out if loading the doc is needed or if it's already here.
    public get currentDocId(): number {
        return this._currentDocId;
    }
    //PDFNet: any;
    ngOnInit() {
        this.wvReadyHandler = this.wvReadyHandler.bind(this);
        this.wvDocumentLoadedHandler = this.wvDocumentLoadedHandler.bind(this);
        this.wvAnnotationsLoaded = this.wvAnnotationsLoaded.bind(this);
    }
    public get CurrentSelectedCode(): string {
        if (this.ItemCodingService.SelectedSetAttribute) {
            return this.ItemCodingService.SelectedSetAttribute.attribute_name;
        }
        else return "No valid selection";
    }
    public get CanWritePDFCoding(): string {
        if (!this.ItemCodingService.SelectedSetAttribute) return "No Code";
        if (this.ReviewSetsService.IsBusy) return "Busy";
        else if (!this.ReviewSetsService.CanWriteCoding(this.ItemCodingService.SelectedSetAttribute) && this.ReviewerIdentityServ.HasWriteRights) return "Coding locked";
        else if (!this.ReviewerIdentityServ.HasWriteRights) return "Read Only";
        else return "yes";
    }

    ngAfterViewInit() {
        this.webviewer.getElement().addEventListener('ready', this.wvReadyHandler);
        this.webviewer.getElement().addEventListener('documentLoaded', this.wvDocumentLoadedHandler);
        
        if (this.subBuildHighlights === null) {
            this.subBuildHighlights = this.ItemCodingService.ItemAttPDFCodingChanged.subscribe(() => {
                console.log("sub buildH");
                if (!this.AlsoBuildHighlights && !this.LoadingNewDoc) {
                    //buildHighlights will be called in wvAnnotationsLoaded:
                    //AlsoBuildHighlights tells it to call it, LoadingNewDoc means it will execute...
                    this.buildHighlights();
                }
            });
        }
    }
    async loadDoc() {
        let counter: number = 0;
        
        //console.log("viewerInstance ", this.viewerInstance);
        while ((!this.viewerInstance ||!this.ItemDocsService.CurrentDoc) && counter < 1 * 60) {
            counter++;
            await Helpers.Sleep(200);
            //console.log("waiting, cycle n: " + counter);
        }
        if (this.ItemDocsService.CurrentDoc) {
            this.AvoidHandlingAnnotationChanges = true;

            //this.webviewer.getElement().removeEventListener("annotationsLoaded", this.wvAnnotationsLoaded);
            if (this.AttachwvAnnotationsLoaded) {
                if (!this.docViewer) {
                    //user got here super fast, PDFtron is still initialising...
                    counter = 0;
                    while (!this.docViewer && counter < 1 * 60) {
                        counter++;
                        await Helpers.Sleep(200);
                        //console.log("waiting, cycle2 n: " + counter);
                    }
                }
                this.docViewer.on('annotationsLoaded', () => this.wvAnnotationsLoaded());
                this.AttachwvAnnotationsLoaded = false;
            }
            this.LoadingNewDoc = true;

            //console.log("asking for the doc- - viewerInstance.loadDocument...", this.LoadingNewDoc);
            this.viewerInstance.loadDocument(this.ItemDocsService.CurrentDoc);
        }
        else console.log("I'm giving up :-(");
    }
    async wvAnnotationsLoaded(): Promise<void> {
        console.log("wvAnnotationsLoaded");
        this.webviewer.getElement().removeEventListener("annotationsLoaded", this.wvAnnotationsLoaded);
        //this.webviewer.getElement().removeEventListener("annotationsLoaded", this.wvAnnotationsLoaded);
        let annotations: any[] = this.annotManager.getAnnotationsList();
        for (let i = 0; i < annotations.length; i++) {
            let annot = annotations[i];
            if (annot.Subject && annot.Subject == "Highlight") {
                i--;
                console.log("Imported annotation (from PDF binary): delete!", annot, i);
                let currentAvoidState: boolean = this.AvoidHandlingAnnotationChanges;
                this.AvoidHandlingAnnotationChanges = true;
                await this.annotManager.deleteAnnotation(annot, true, true);//we just delete it, consider showing an explanation...
                this.AvoidHandlingAnnotationChanges = currentAvoidState;
            }
        }
        this.LoadingNewDoc = false;
        if (this.AlsoBuildHighlights) {
            this.buildHighlights();
            this.AlsoBuildHighlights = false;
        }
        console.log("wvAnnotationsLoaded", this.LoadingNewDoc);
        //this.AvoidHandlingAnnotationChanges = false;
    }
    async wvReadyHandler() {
        console.log("wvReadyHandler");
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
            //console.log('items: ', header.getItems());
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
            'AnnotationCreateFreeText',
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
        
        //this.webviewer.getElement().addEventListener('pageChanged', (e: any) => {
        //    const [pageNumber] = e.detail;
        //    console.log(`Current page is ${pageNumber}`);
        //});

        // or from the docViewer instance
        //this.viewerInstance.docViewer.on('annotationsLoaded', () => {
        //    console.log('annotations loaded');
        //});

        
        this.PDFnet = this.webviewer.getPDFNet();
        try { await this.PDFnet.initialize(); }
        catch (error) { console.log("Error initialising PDFnet: ", error); }
        this.docViewer = this.viewerInstance.docViewer;
        this.annotManager = this.docViewer.getAnnotationManager();
        this.annotManager.on('annotationChanged', (event: any, annotations: any, action: any) => this.AnnotationChangedHandler(event, annotations, action));
    }

    wvDocumentLoadedHandler(): void {
        //TODO: if a code is selected, get the PDFCoding...
        this._currentDocId = this.ItemDocsService.CurrentDocId;
        console.log("wvDocumentLoadedHandler", this.AvoidHandlingAnnotationChanges);

        if (this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding
            && this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding.length > 0
            && this.ItemDocsService.CurrentDoc
            && this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemDocumentId == this.ItemDocsService.CurrentDocId
        ) {
            console.log("wvDocumentLoadedHandler1", this.AvoidHandlingAnnotationChanges, this.LoadingNewDoc);
            if (this.docViewer) {
                this.AlsoBuildHighlights = true; //buildHighlights is called by wvAnnotationsLoaded, we want to do it after this event!
                //this.docViewer.on('annotationsLoaded', () => this.wvAnnotationsLoaded());//wvAnnotationsLoaded will call buildHighlights when done
            }
            //the above makes sure we can distinguish between imported highlights that come from within the PDF binary from those that are "coding highlights"...
            //this.buildHighlights();
        }
        else if (
            this.ItemDocsService.CurrentDoc
            && this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemDocumentId !== this.ItemDocsService.CurrentDocId
            && this.ItemCodingService.SelectedSetAttribute
        ) {
            //we have a doc, but doc coding isn't right, go fetch it.
            const ROatt = this.ItemCodingService.FindROItemAttributeByAttribute(this.ItemCodingService.SelectedSetAttribute);
            console.log("wvDocumentLoadedHandler2 fetch PDF coding", ROatt, this.AvoidHandlingAnnotationChanges, this.LoadingNewDoc);
            if (this.docViewer) {
                this.AlsoBuildHighlights = true; //buildHighlights is called by wvAnnotationsLoaded, we want to do it after this event!
                //this.docViewer.on('annotationsLoaded', () => this.wvAnnotationsLoaded());
            }
            if (ROatt) this.ItemCodingService.FetchItemAttPDFCoding(new ItemAttPDFCodingCrit(this.ItemDocsService.CurrentDocId, ROatt.itemAttributeId));
            else this.ItemCodingService.ClearItemAttPDFCoding();
        }
        else {
            console.log("wvDocumentLoadedHandler3 nothing to do (docId, crit.docId, selected attId, AvoidHandlingAnnotationChanges, LoadingNewDoc):",
                this.ItemDocsService.CurrentDocId,
                this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemDocumentId,
                this.ItemCodingService.SelectedSetAttribute,
                this.AvoidHandlingAnnotationChanges, this.LoadingNewDoc
            );
            if (this.docViewer) {
                this.AlsoBuildHighlights = false; //buildHighlights does not need to be called.
                //this.docViewer.on('annotationsLoaded', () => this.wvAnnotationsLoaded());//buildHighlights is called otherwise, no need to do it in wvAnnotationsLoaded
            }
        }


        this.viewerInstance.setFitMode("FitWidth");
        
        // you can access docViewer object for low-level APIs
        //const annotManager = docViewer.getAnnotationManager();
        // and access classes defined in the WebViewer iframe
        //const { Annotations } = this.webviewer.getWindow();
        //const rectangle = new Annotations.RectangleAnnotation();
        //rectangle.PageNumber = 2;
        //rectangle.X = 100;
        //rectangle.Y = 100;
        //rectangle.Width = 250;
        //rectangle.Height = 250;
        //rectangle.StrokeThickness = 5;
        //rectangle.Author = this.annotManager.getCurrentUser();
        //this.annotManager.addAnnotation(rectangle);
        //this.annotManager.drawAnnotations(rectangle.PageNumber);
        // see https://www.pdftron.com/api/web/PDFTron.WebViewer.html for the full list of low-level APIs
    }
    
    async buildHighlights() {
        console.log("buildHighlights (pdf)");
        if (this.ItemCodingService.IsBusy) {
            //we try to avoid doing this if the service is busy. 
            let counter: number = 0;
            //console.log("viewerInstance ", this.viewerInstance);
            while (this.ItemCodingService.IsBusy && counter < 4 * 60) {//wait up to 1m...
                counter++;
                await Helpers.Sleep(200);
                //console.log("waiting, cycle n: " + counter);
            }
            if (this.ItemCodingService.IsBusy) {
                alert("Sorry.\n We could not load the current highlights in the allocated time.\n Please try again after a few seconds.");
                return;//Don't even try...
            }
        }
        
        let AllInputShapes: string[] = []; //will receive one string per page...
        let AllContinuousSelections: TextWithShapes[] = [];
        
        if (!this.docViewer) return;
        var GenDoc = this.docViewer.getDocument();
        if (!GenDoc) return;
        let doc = await GenDoc.getPDFDoc();
        this.ItemCodingService.markBusyBuildingHighlights();
        
        const reader = await this.PDFnet.ElementReader.create();
        const { Annotations } = this.webviewer.getWindow();
        //console.log("PDFNet", this.PDFnet);

        if (this.annotManager) {
            await this.deleteAnnotations(this.annotManager, Annotations);//deletes Highlight annotations only. Handles AvoidHandlingAnnotationChanges on its own.
        }
        this.AvoidHandlingAnnotationChanges = true;

        //what we need is:
        //if we have an XML description of the annotations, use that, otherwise
        //create highlights based on the text, so that shapes "know" what text they belong to.
        
        
        
        //if we have data, show it. Otherwise we've already deleted any old highlight...
        if (this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding && this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding.length > 0) {
            for (let sel of this.ItemCodingService.CurrentItemAttPDFCoding.ItemAttPDFCoding) {
                let HasDataToSave: boolean = false;//this is used to figure out whether we're creating new annotations when only ER4 is present
                
                if (sel.pdfTronXml !== "") {//if we have the XML for this page, we build the annotations from that...
                    //build the annotations from XML, we'll use the XML itself to create the ER-proper data when saving changes.
                    //console.log("We have the XML annotation");
                    this.annotManager.importAnnotations(sel.pdfTronXml);
                }
                //else try to rebuild it...
                else {
                    //First bit is to cycle through let RectsA = rectSt0.split("zM");
                    //for (let rectSt of RectsA) {...}
                    //for each rectangle, find the corresponding string, get an arr of "selectedTxt" with its shape strings and a (now) empty field for the annotation.
                    //create one annotation per element in Arr, put it into the element, show the annotation.
                    //let's start by creating one TextWithShapes obj for each "sel.inPageSelections[0].selTxt"

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
                        //console.log("quad s:", quad);
                        let PDFpoint1 = GenDoc.getPDFCoordinates(sel.page - 1, quad.x1, quad.y1);
                        let PDFpoint2 = GenDoc.getPDFCoordinates(sel.page - 1, quad.x3, quad.y3);
                        //we are creating a very short (reduced height) rectangule to avoid finding letters from the rows above and below, hence the /4 operation and +-ydiff.
                        let ydiff = (PDFpoint1.y - PDFpoint2.y) / 4;
                        PDFpoint1.y = PDFpoint1.y - ydiff;
                        PDFpoint2.y = PDFpoint2.y + ydiff;
                        //console.log(PDFpoint1, PDFpoint2);
                        let rec = await this.PDFnet.Rect.init(PDFpoint2.x, PDFpoint2.y, PDFpoint1.x, PDFpoint1.y);

                        //readTextFromRect(...) uses PDFTron full API to extract text from the shape we've created.
                        let textinSel = (await this.readTextFromRect(sel.page, rec, reader, doc, this.PDFnet)).trim();
                        //console.log("looking for:", textinSel)
                        let myCurrentSelectIndex = AllContinuousSelections.findIndex(found => found.FullSelectedText.indexOf(textinSel) != -1);
                        //console.log("my curr sel ind:", AllContinuousSelections.findIndex(found => found.FullSelectedText.indexOf(textinSel) != -1));
                        if (myCurrentSelectIndex != -1) {
                            //yay! the text underneath the current shape belongs to AllContinuousSelections[myCurrentSelectIndex]
                            AllContinuousSelections[myCurrentSelectIndex].ShapesStrings.push(AllInputShapes[i]);
                            //we found where the current shape belongs, so let's remove it
                            AllInputShapes.splice(i, 1);//this also means we don't need to increase i:
                            i--;
                            continue;
                        }
                        else {
                            //console.log("did not find this string!!!!!!!\n\n Try again...\n\n", textinSel, AllInputShapes[i]);
                            //we try again, but this time, full blooded (slower)
                            //we remove spaces and tabs, and also replace the horrible "double chars" that frequently appear in PDFs.
                            //if the string is longish, remove first and last char, they sometimes fall without our visual boundaries...
                            if (textinSel.length > 9) textinSel = textinSel.substring(1, textinSel.length - 1);
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
                                    .replace(/Ð/g, '–')
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
                    //console.log("done one page, results of step 1:", AllContinuousSelections, AllInputShapes);
                    //AllInputShapes contains ER4 selections that we couldn't match :-(
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
                            //can't save this one, page has more than one unrecognised selection.
                            //find the empty AllContinuousSelections, get rid of all of them, create a blank new AllContinuousSelection with no text but the shapes.
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
                    //AllContinuousSelections can now be used to create new PDF-native annotations.
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
                            //special colour to signal this page-wide annotation wasn't recognised from ER4 data.
                            Highl.StrokeColor = new Annotations.Color(255, 124, 6);
                        }

                        //Highl.$i = new Annotations.Color(247, 247, 227);

                        if (pSel.ShapesStrings.length > 0) {//in the odd case that we don't have ShapesStrings, attempting the below would crash...
                            Highl.Quads = pSel.Quads;
                            //console.log("addAnnotation", Highl);
                            //Highl.Custom = "ERx";
                            this.annotManager.addAnnotation(Highl);
                            //we are adding an annotation based on ER4 selections, so we'll save the resulting XML, to avoid re-doing the above each time a user fetches this data.
                            HasDataToSave = true;
                        }
                        else console.log("broken ann:", Highl);
                    }
                    await this.annotManager.drawAnnotations(sel.page);//we can only draw annotations on a per page basis :-(
                }
                if (HasDataToSave && this.ReviewerIdentityServ.HasWriteRights) {
                    //we have rebuilt the XML from the ER4 annots, let's save it...
                    let fAnnots = this.annotManager.getAnnotationsList().filter((found: any) => found.PageNumber == sel.page);
                    const xfdfString: string = this.annotManager.exportAnnotations({ annotList: fAnnots, links: false, widgets: false });
                    //console.log("We'll try to save some changes:", xfdfString, this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemAttributeId);
                    this.ItemCodingService.SaveItemAttPDFCoding(xfdfString, this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemAttributeId);
                }
            }           
        }
        this.ItemCodingService.removeBusyBuildingHighlights();
        this.AvoidHandlingAnnotationChanges = false;//all done! Changes to annotations will now be due to user actions.
    }
    
    //this method is called often: any time we need to show a different set of highlights...
    //despite the name, it only deletes annotations of type "Highlight":
    //it will remove any (PDF-native) Highlight that was present in the uploaded PDF (these should have been removed in wvAnnotationsLoaded).
    //such highlights will be shown if the user "Downloads" it, though.
    private async deleteAnnotations(annotManager: any, Annotations: any) {
        console.log("deleteAnnotations");
        this.AvoidHandlingAnnotationChanges = true;
        
        let Annots: any[] = annotManager.getAnnotationsList();
        
        //if (Annots) console.log("Annots to delete:", Annots, Annots.length);
        
        for (let i = 0; i < Annots.length; i++) {
            let annot = Annots[i];
            
            //if (annot && annot !== undefined && (annot.Subject === "Highlight" || annot.Subject === null)) {
            if (annot && annot !== undefined ) { 
                if (annot.Subject === "Highlight" || annot instanceof Annotations.TextHighlightAnnotation) {
                    try {
                        //console.log("how many?", Annots.length, annot.Custom, annot.Subject, annot instanceof Annotations.TextHighlightAnnotation);
                        console.log("deleting annot: " + (i + 1), annot);
                        annotManager.deleteAnnotation(annot, true, true);
                        i--;
                        //Annots.splice(i, 1);
                        //console.log("2 how many?", Annots.length);
                    }
                    catch (e) {
                        console.log("error deleting annot:", e);
                    }
                }
            }
            else {
                console.log("Not deleting this annot (null or undefined):", i);
                //Annots = annotManager.getAnnotationsList();
                //i = -1;
                //retryCount++;
            }
        }
        //console.log("Annots after deleting:", annotManager.getAnnotationsList(), annotManager.getAnnotationsList().length);
        //console.log(""); console.log("");
        this.AvoidHandlingAnnotationChanges = false;
    }

    //this executes whenever annotations are changed, even if done programmatically,
    //we use this.AvoidHandlingAnnotationChanges to figure if the change is user-initiated or not.
    private async AnnotationChangedHandler(event: any, annotations: any, action: any) {
        console.log("AnnotationChangedHandler", this.AvoidHandlingAnnotationChanges, this.LoadingNewDoc);
        if (event && event.imported && this.LoadingNewDoc) {
            //if imported (or from within the PDF or from database), but we haven't finished loading, do nothing. Rationale:
            //when we finish loading, we're deleting all annots coming from inside the PDF, before loading the highlights (for the selected code)
            return;
        }
        //console.log("AnnotationChangedHandler, action:", action);
        const { Annotations } = this.webviewer.getWindow();
        if (this.AvoidHandlingAnnotationChanges) {
            //when an annot is added programmatically, make sure we don't allow editing it...
            if (action === 'add') {
                for (let ann of annotations) {
                    console.log("checking if we need to make annotation not editable, annot:", ann);
                    //this is where we might get the "TypeError: Cannot read property 'sink' of null"  error.
                    //when this happens, the page after the annotation that failed would not render!
                    if ((ann.Subject && ann.Subject === "Highlight") || ann instanceof Annotations.TextHighlightAnnotation) {
                        console.log("trying to make annotation not editable, annot:", ann);
                        ann.NoResize = true;
                        //ann.Custom = "ERx";
                        this.annotManager.redrawAnnotation(ann);
                    }
                }
            }
            return; //we do nothing else when we're handling annotations from code-behind...
        }
        //we have nothing to do if we could not get the annotations that are being edited
        //we do it in two distinct checks so that from now on, we know annotations is not null/undefined AND that it contains at least one annotation.
        if (!annotations) return;
        else if (annotations.length == 0) return;
        let cmd: ItemAttributeSaveCommand = new ItemAttributeSaveCommand();//we'll fill this one in if needed
        if (this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemAttributeId == 0
            && this.ItemCodingService.SelectedSetAttribute
            && this.ReviewSetsService.CanWriteCoding(this.ItemCodingService.SelectedSetAttribute)
        ) {
            //this is crucial: need to make sure that this happens ONLY when it's the user 
            //who has generated a new selection for a code that hasn't been applied to current item
            //in testing as of 26/04/2019, it appears to be the case, but might need more work. (i.e. the AvoidHandlingAnnotationChanges flag is sufficient)
            //in case, we can consider using event.imported, see: https://www.pdftron.com/documentation/web/guides/annotation-events/
            //see also the next IF
            
            ///are we sure this is a user generated insert? (can add 1 highlight per event only)
            if (annotations[0] instanceof Annotations.TextHighlightAnnotation && action === 'add') {
                //we fill-in the ItemAttributeSaveCommand object, so that the API controller will create the ItemAttribute record, as well as the ItemSet one, if needed.
                //by sending the cmd object in the request, we avoid having to orchestrate 2 API calls in strict succession (can't save PDF coding without an ItemAttributeId)
                if (annotations.length == 1) {
                    cmd.additionalText = "";
                    cmd.attributeId = this.ItemCodingService.SelectedSetAttribute.attribute_id;
                    cmd.itemArmId = this.armsService.SelectedArm == null ? 0 : this.armsService.SelectedArm.itemArmId;
                    cmd.itemId = this.ItemID;
                    cmd.revInfo = this.ReviewInfoService.ReviewInfo;
                    cmd.saveType = "Insert";
                    cmd.setId = this.ItemCodingService.SelectedSetAttribute.set_id;
                    let itemSet: ItemSet | null = this.ItemCodingService.FindItemSetBySetId(this.ItemCodingService.SelectedSetAttribute.set_id);
                    if (itemSet) {
                        //we have an item set to use, so put the relevant data in cmd. Otherwise, default value is fine.
                        cmd.itemSetId = itemSet.itemSetId;
                    }
                    console.log("create coding from PDF tab:", cmd);
                }
            }
            //return;
        }
        else if (!this.ItemCodingService.SelectedSetAttribute || !this.ReviewSetsService.CanWriteCoding(this.ItemCodingService.SelectedSetAttribute)) {
            //we don't know what code we're supposed to attach the PDF text to :-(
            //OR: coding is locked, OR we don't have write rights.
            //most likely user has not selected a code from the tree (left side) or has selected a ReviewSet, or user does not have the right to edit...
            //if we're here we know that AvoidHandlingAnnotationChanges == false,
            //we can call delete annotations (will end by setting it to false)
            let annot = annotations[0];
            console.log("Delete ann on this basis: (selected, can write)", this.ItemCodingService.SelectedSetAttribute, this.ReviewerIdentityServ.HasWriteRights);
            if (action === 'add') {
                let currentAvoidState = this.AvoidHandlingAnnotationChanges;
                this.AvoidHandlingAnnotationChanges = true;
                this.annotManager.deleteAnnotation(annot, true, true);//we just delete it, consider showing an explanation...
                this.AvoidHandlingAnnotationChanges = currentAvoidState;
            }
            else if (action === 'delete' && (!this.ReviewerIdentityServ.HasWriteRights || (this.ItemCodingService.SelectedSetAttribute && !this.ReviewSetsService.CanWriteCoding(this.ItemCodingService.SelectedSetAttribute))))
            {
                //user can't delete this (read only, OR locked coding), so we'll put it back in...
                console.log("trying to re-add annotation (v0)");
                let currentAvoidState = this.AvoidHandlingAnnotationChanges;
                this.AvoidHandlingAnnotationChanges = true;
                this.annotManager.addAnnotation(annot);
                this.AvoidHandlingAnnotationChanges = currentAvoidState;
            }
            return;
        }
        if (annotations.length > 1) {
            //We have more than one annotation! 
            //So far, we think this is only possible when the user selected text across 2 or more pages.
            //we can't support this scenario, because the annotations returned by PDFTron report the whole text as "selected text" for all annotations.
            //this is a problem for reports, where the same text will appear 2 or more times and claim to be on a given page (where only part of the text is)
            //it is also a problem for backward compatibility with ER4, as in there ER-Web selections are "reconstructed" based on their "selected text"
            //but the selected text won't be found as it does not belong to any single page!
            //thus, inform the user that ER-Web can't do this AND delete the selection instead.
            this.ngZone.run(() =>
                this.modalService.GenericErrorMessage("Sorry, adding one selection that spans 2 or more pages <strong>is not supported</strong>.<br />"
                    + "To add your intended selection, please add it as <strong>two separate selections, one per page</strong>.")
            );
            //delete the annotations...
            let currentAvoidState = this.AvoidHandlingAnnotationChanges;
            for (let annot of annotations) {
                this.AvoidHandlingAnnotationChanges = true;
                this.annotManager.deleteAnnotation(annot, true, true);
                //this.annotManager.redrawAnnotation(annot);
            }
            this.AvoidHandlingAnnotationChanges = currentAvoidState;
            return;//
        }
        //at this point we know we have only one annotation to handle
        if (annotations[0] instanceof Annotations.TextHighlightAnnotation
            && this.ReviewSetsService.CanWriteCoding(this.ItemCodingService.SelectedSetAttribute)
        ) {
            let highlightAnnotation = annotations[0];
            
            
            if (action === 'add') {
                console.log('this is a change that added annotations', annotations, action);
                highlightAnnotation.NoResize = true;
                //highlightAnnotation.Custom = "ERx";
                this.annotManager.redrawAnnotation(highlightAnnotation);
                let fAnnots = this.annotManager.getAnnotationsList().filter((found: any) => found.PageNumber == highlightAnnotation.PageNumber);
                const xfdfString: string = this.annotManager.exportAnnotations({ annotList: fAnnots, links: false, widgets: false });
                //API call!!! save changes.
                if (this.ReviewerIdentityServ.HasWriteRights)
                    this.ItemCodingService.SaveItemAttPDFCoding(xfdfString, this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemAttributeId, cmd);
                //console.log("xfdfString (add):", xfdfString);

            } else if (action === 'modify') {
                //uh? this should not be allowed...
                console.log('this change modified annotations', annotations);
            } else if (action === 'delete') {
                //do we delete just one annotation in current page but some other annotation remain? In which case, it's an update...
                let fAnnots = this.annotManager.getAnnotationsList().filter((found: any) => 
                    found.PageNumber == highlightAnnotation.PageNumber && found instanceof Annotations.TextHighlightAnnotation
                );
                const xfdfString: string = this.annotManager.exportAnnotations({ annotList: fAnnots, links: false, widgets: false });
                let ind = xfdfString.indexOf("</highlight>");
                //if the exported XML has highlights and we have an item attribute record associated with it, save what's left.
                if (
                    ind > -1
                    && this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemAttributeId > 0
                ) {
                    if (this.ReviewerIdentityServ.HasWriteRights)
                        this.ItemCodingService.SaveItemAttPDFCoding(xfdfString, this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemAttributeId);
                    else {
                        console.log("trying to re-add annotation (v1)");
                        let currentAvoidState = this.AvoidHandlingAnnotationChanges;
                        this.AvoidHandlingAnnotationChanges = true;
                        this.annotManager.addAnnotation(highlightAnnotation);
                        this.AvoidHandlingAnnotationChanges = currentAvoidState;
                    }
                }
                else {
                    //do the delete thing... removes the whole record for a page in the PDF ('cause it's empty, now)
                    if (this.ReviewerIdentityServ.HasWriteRights)
                        this.ItemCodingService.DeleteItemAttPDFCodingPage(highlightAnnotation.PageNumber, this.ItemCodingService.CurrentItemAttPDFCoding.Criteria.itemAttributeId);
                    else {
                        console.log("trying to re-add annotation (v2)");
                        let currentAvoidState = this.AvoidHandlingAnnotationChanges;
                        this.AvoidHandlingAnnotationChanges = true;
                        this.annotManager.addAnnotation(highlightAnnotation);
                        this.AvoidHandlingAnnotationChanges = currentAvoidState;
                    }
                }
                //console.log('there were annotations deleted', annotations, xfdfString);
                
            }
        }

        //annotations.forEach(function (annot: any) {
        //    console.log('annotation page number', annot.PageNumber);
        //});
    }

    
    //method is called by buildHighlights when trying to rebuild annotations from ER4 data
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
        //pos is key: since we know where we're looking we start from here, makes the whole thing much faster!
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
                            let ItrsY1 = Math.max(bbox.y1, pos.y1);
                            let ItrsY2 = Math.min(bbox.y2, pos.y2);
                            if (ItrsY2 - ItrsY1 > (bbox.y2 - bbox.y1) / 2.1) {
                                //intersection is  substantial!
                                //console.log("found word:", word);
                                intersectLinesText += await word.getString() + " ";
                            }
                        }
                    }
                }
            }
        }
        return intersectLinesText;//this string will be used to match with text from ER4 data...
    }
    ngOnDestroy() {
        console.log('killing pdftroncontainer comp');
        if (this.subBuildHighlights) this.subBuildHighlights.unsubscribe();
        this.ItemCodingService.Clear();
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

    //important method: given a (rectangle) shape as described by ER4, build the "Quad" used in PDFTron
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


//minimal test example to use the full PDFTron API
//to call it: 
//let elCnt = await PDFnet.runWithCleanup(async () => await this.TestAPICall(1, "", doc));
//PDFnet.runWithCleanup(...) is KEY. You NEED to use it in order to access the full API.
//async TestAPICall(page: number, pos: any, doc: any) {
//    const reader = await this.webviewer.getPDFNet().ElementReader.create();
//    let count = 0;
//    let element;
//    let PDFpage = await doc.getPage(page);
//    console.log("page: ", page, PDFpage);

//    reader.beginOnPage(PDFpage);
//    while ((element = await reader.next()) !== null) {
//        count++;
//    }
//    reader.end();
//    return count;
//}


//OLD code that works, but isn't used...
//findShapeFromText(InPgSel: InPageSelection, page: number, doc: any, shapesToMatch: any[],
//    Annotations: any, ViewerInstance: any, annotationManager: any): any {
//    //this is a failed attempt: page indexes are just different :-((((
//    //to make things worse, doc.getTextPosition returns quads on a per-char basis, so we can't compare like for li
//    doc.loadPageText(page - 1, function (text: string) {
//        var textStartIndex = 0;
//        var textIndex;
//        var annotations: any[] = [];
//        console.log("Searching...", shapesToMatch, "InPgSel: " + InPgSel.start, InPgSel.end, InPgSel.selTxt);// , "page: " + page, "fullpage: " + text);

//        //console.log("Found text", InPgSel.selTxt, textIndex);

//        // Get text position
//        doc.getTextPosition(Number(page - 1), InPgSel.start, InPgSel.start + InPgSel.selTxt.length, function (quads: any) {
//            console.log("got the pos:", quads);
//            for (let shapeToMatch of shapesToMatch) {
//                let minx = Math.min(shapeToMatch.x1, shapeToMatch.x2, shapeToMatch.x3, shapeToMatch.x4);
//                let maxx = Math.max(shapeToMatch.x1, shapeToMatch.x2, shapeToMatch.x3, shapeToMatch.x4);
//                let miny = Math.min(shapeToMatch.y1, shapeToMatch.y2, shapeToMatch.y3, shapeToMatch.y4);
//                let maxy = Math.max(shapeToMatch.y1, shapeToMatch.y2, shapeToMatch.y3, shapeToMatch.y4);
//                console.log("minMaxes:", minx, miny, maxx, maxy);
//                let minPindex = quads.findIndex((found: any) => (Math.abs(minx - found.x1) < 0.95 && Math.abs(miny - found.y3) < 1.5));
//                let maxPindex = quads.findIndex((found: any) => (Math.abs(maxx - found.x1) < 0.95 && Math.abs(maxy - found.y3) < 1.5));
//                if (minPindex > -1 && maxPindex > -1)
//                //if (Math.abs(shapeToMatch.x1 - quads[0].x1) < 0.95
//                //    //&& Math.abs(shapeToMatch.x3 - quads[quads.length - 1].x3) < 0.95
//                //    //&& Math.abs(shapeToMatch.y1 - quads[0].y3) < 1.5
//                //    //&& Math.abs(shapeToMatch.y3 - quads[quads.length - 1].y1) < 1.5
//                //)
//                {
//                    console.log("We found a match for ", shapeToMatch);
//                    var annotation = new Annotations.TextHighlightAnnotation();
//                    annotation.Author = ViewerInstance.getAnnotationUser();
//                    annotation.PageNumber = page;
//                    annotation.Quads = [shapeToMatch];
//                    annotation.StrokeColor = new Annotations.Color(250, 224, 6);
//                    annotations.push(annotation);
//                    break;
//                }
//            }


//            //this doesn't work, the quads are one per letter, not the whole selection :-(
//            //for (let foundQ of quads) {
//            //    let foundIndex = shapesToMatch.findIndex((found) => (Math.abs(found.x1 - foundQ.x1) < 0.1 && Math.abs(found.y3 - foundQ.y3) < 0.1)) ;
//            //    if (foundIndex > -1) {
//            //        console.log("Quad was found:", foundQ);
//            //        var annotation = new Annotations.TextHighlightAnnotation();
//            //        annotation.Author = ViewerInstance.getAnnotationUser();
//            //        annotation.PageNumber = page;
//            //        annotation.Quads = quads;
//            //        annotation.StrokeColor = new Annotations.Color(250, 224, 6);
//            //        annotations.push(annotation);
//            //    }
//            //}

//        });

//        annotationManager.addAnnotations(annotations);
//        //annotationManager.selectAnnotations(annotations);
//    });
//}

//OLD code that works, but is too slow!
//private async rectTextSearch(page: number, startReader: boolean, reader: any, pos: any, srchStr: string, doc: any, PDFnet: any) {
//    console.log("starting rectTextSearch \n", page, startReader, reader, pos, srchStr, doc, PDFnet);
//    if (startReader) reader.beginOnPage(await doc.getPage(page));
//    let element, arr;
//    while ((element = await reader.next()) !== null) {
//        let bbox;
//        let elType = await element.getType();
//        //console.log("type =", elType);
//        switch (elType) {
//            case PDFnet.Element.Type.e_text:
//                bbox = await element.getBBox();
//                //console.log("Rectangles:", bbox, pos);
//                if (bbox && await bbox.intersectRect(bbox, pos)) {
//                    arr = await element.getTextString();
//                    //if (arr && arr.length > 0) console.log("el text:", arr);
//                    srchStr += arr + '\n';
//                }
//                break;
//            case PDFnet.Element.Type.e_text_new_line:
//                break;
//            case PDFnet.Element.Type.e_form:
//                reader.formBegin();
//                srchStr += await this.rectTextSearch(page, false, reader, pos, srchStr, doc, PDFnet); // possibly need srchStr = ...
//                reader.end();
//                break;
//        }
//    }
//    reader.end();
//    return srchStr;
//}



