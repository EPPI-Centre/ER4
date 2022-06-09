import { Component, ViewChild, ElementRef, AfterViewInit, Input, Inject } from '@angular/core';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';

declare const PDFTron: any;
declare let licenseKey: string;
declare const PDFNet: any; 

@Component({
    selector: 'app-webviewer',
    templateUrl: './webviewer.component.html'
    
})

//template: `<div style="">
//        <div #viewer class= "iframe-container" style="display: flex; width: 100%; height: 100%; flex-direction: column; background-color: blue; overflow: hidden;" >
//        </div></div > `,
//    styles: [``]

    //`<div style="position:absolute; top:7em; left:1px; padding-bottom:7.2em; height:100%; width:99.8%">
    //    <div #viewer class= "iframe-container" style="width: 100%; height:100%; position:relative" >
    //    </div></div > `
//template: '<div #viewer style="width: 100%; height: 600px; min-height:300px;"></div>',
//    styles: ['div { width: 100%; height: 100%; min-height:300px;}'
export class WebViewerComponent implements AfterViewInit {
    constructor(
        @Inject('BASE_URL') private _baseUrl: string
    ) { }
    @ViewChild('viewer') viewer!: ElementRef;
    myWebViewer: any;
    @Input() DocId: number = 0;
    ngAfterViewInit(): void {
        this.myWebViewer = new PDFTron.WebViewer({
            path: this._baseUrl + 'assets/lib',
            documentType: 'pdf',
            //config: '../lib/CustomPDFtron.js',
            fullAPI: true,
            l: "University College London(ucl.ac.uk):OEM:EPPI-Reviewer::B+:AMS(20200429):D6A5281D04B7480A3360B13AC9A2737860613F9D0644EDA8CD046541120C6EA02ACA31F5C7"
        }, this.viewer.nativeElement);
        //try {
        //    console.log("try PDFNet.initialize");
        //    PDFNet.initialize().then(console.log(" PDFNet.initialize"));
        //}
        //catch (e) { console.log(e); }
    }
    public async GetReader() {
        let aaa = this.viewer.nativeElement.querySelector('iframe');
        let bb = this.viewer.nativeElement;
        let ccc = this.viewer.nativeElement.querySelector('iframe').contentWindow;
        let ddd = this.myWebViewer.getInstance();
        console.log(aaa, bb, ccc);
        return await this.viewer.nativeElement.querySelector('iframe').PDFNet.ElementReader.create();
    }
    getPDFNet() {
        let something = this.getWindow();
        //let counter: number = 0;
        //while ((!something) && counter < 3 * 20) {
        //    counter++;
        //    await Helpers.Sleep(200);
        //    console.log("waiting, cycle n (window 4 PDFNet): " + counter);
        //    something = this.getWindow();
        //}
        if (something) {
            let { PDFNet } = something;
            //while ((!PDFNet) && counter < 3 * 20) {
            //    counter++;
            //    await Helpers.Sleep(200);
            //    console.log("waiting, cycle n (PDFNet): " + counter);
            //    PDFNet = something.PDFNet;
            //}
            
            if (PDFNet) {
                console.log("got PDFNet :-)\n\n\n\n\n");
                //await PDFNet.initialize();
                
                return PDFNet;
            }
            else {
                console.log("did not get PDFNet :-(");
                return null;
            }
        }
        else {
            console.log("did not get window, for PDFNet :-(");
            return null;
        }
    }
    getInstance(): any {
        return this.myWebViewer.getInstance();
    }

    getWindow(): any {
        return this.viewer.nativeElement.querySelector('iframe').contentWindow;
    }

    getElement(): any {
        return this.viewer.nativeElement;
    }
}
