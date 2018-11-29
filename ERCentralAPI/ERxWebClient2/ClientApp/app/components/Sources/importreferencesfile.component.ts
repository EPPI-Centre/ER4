import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { timer, Subject, Subscription, Observable } from 'rxjs';
import { take, map, takeUntil } from 'rxjs/operators';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { SourcesService, IncomingItemsList } from '../services/sources.service';


@Component({
    selector: 'ImportReferencesFile',
    templateUrl: './importreferencesfile.component.html',
    providers: []
})

export class ImportReferencesFileComponent implements OnInit {
    //some inspiration taken from: https://malcoded.com/posts/angular-file-upload-component-with-express
    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService,
        private ItemListService: ItemListService,
        private SourcesService: SourcesService
    ) {    }
    ngOnInit() {
        this.reader.onload = (e) => this.fileRead(e);
        this.SourcesService.gotItems4Checking.subscribe(() => {
            this.gotItems4Checking();
        });
    }
    @ViewChild('file') file: any;
    public WizPhase: number = 1
    addFile() {
        this.file.nativeElement.click();
    }
    private reader = new FileReader();
    private fileContent: string = "";
    onFilesAdded() {
        const files: { [key: string]: File } = this.file.nativeElement.files;
        const file: File = files[0];
        if (file) {
            this.WizPhase = 1.5;
            //reader.onload = function (e) {
            //    if (reader.result) {
            //        fileContent = reader.result as string;
            //        console.log(fileContent.length);
            //    }
            //}
            this.reader.readAsText(file);
            
        }
    }
    private fileRead(e: ProgressEvent) {
        if (this.reader.result) {
            this.fileContent = this.reader.result as string;
            console.log(this.fileContent.length);
            this.SourcesService.CheckUpload("RIS", this.fileContent);
        }
    }
    private gotItems4Checking() {
        this.WizPhase = 2;
    }
    back() {
        this.WizPhase = 1;
    }
    get DataToCheck(): IncomingItemsList | null {
        if (this.WizPhase == 2 && this.SourcesService.IncomingItems4Checking) return this.SourcesService.IncomingItems4Checking;
        else return null;
    }
}
