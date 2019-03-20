import { Component, ViewChild, ElementRef, AfterViewInit, Input } from '@angular/core';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';

declare const PDFTron: any;
declare let licenseKey: string;

@Component({
  selector: 'app-webviewer',
    template: '<div #viewer style="width: 100%; height: 600px; min-height:300px;"></div>',
    styles: ['div { width: 100%; height: 100%; min-height:300px;}'
    ]
})
export class WebViewerComponent implements AfterViewInit {
    constructor(private ReviewerIdentityServ: ReviewerIdentityService,
        private ItemDocsService: ItemDocsService
    ) { }
    @ViewChild('viewer') viewer!: ElementRef;
    myWebViewer: any;
    @Input() DocId: number = 0;
    ngAfterViewInit(): void {
        this.myWebViewer = new PDFTron.WebViewer({
            path: '../lib',
            documentType: 'pdf',
            l: atob(licenseKey)
        }, this.viewer.nativeElement);
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
