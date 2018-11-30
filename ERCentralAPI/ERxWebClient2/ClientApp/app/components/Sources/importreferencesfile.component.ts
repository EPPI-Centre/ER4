import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { timer, Subject, Subscription, Observable } from 'rxjs';
import { take, map, takeUntil } from 'rxjs/operators';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource } from '../services/sources.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { EventEmitterService } from '../services/EventEmitter.service';


@Component({
    selector: 'ImportReferencesFile',
    templateUrl: './importreferencesfile.component.html',
    providers: []
})

export class ImportReferencesFileComponent implements OnInit {
    //some inspiration taken from: https://malcoded.com/posts/angular-file-upload-component-with-express
    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
        private CodesetStatisticsService: CodesetStatisticsService,
        private ItemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private SourcesService: SourcesService
    ) {    }
    ngOnInit() {
        this.reader.onload = (e) => this.fileRead(e);
        this.SourcesService.gotItems4Checking.subscribe(() => {
            this.gotItems4Checking();
        });
        this.SourcesService.FetchImportFilters();
        //this.SourcesService.FetchSources();
        this.SourcesService.SourceUploaded.subscribe(() => {
            this.SourceUploaded();
        })
    }
    @ViewChild('file') file: any;
    public WizPhase: number = 1
    addFile() {
        console.log('oo');
        this.file.nativeElement.click();
    }
    private reader = new FileReader();
    
    onFilesAdded() {
        const files: { [key: string]: File } = this.file.nativeElement.files;
        const file: File = files[0];
        if (file) {
            this.WizPhase = 1.5;
            this.currentFileName = file.name;
            //reader.onload = function (e) {
            //    if (reader.result) {
            //        fileContent = reader.result as string;
            //        console.log(fileContent.length);
            //    }
            //}
            this.reader.readAsText(file);
            
        }
    }
    private Source4upload: SourceForUpload | null = null;
    private fileRead(e: ProgressEvent) {
        if (this.reader.result) {
            let fileContent: string = this.reader.result as string;
            console.log("fileRead: " + fileContent.length);
            let filename = "Please update";
            if (this.currentFileName) filename = this.currentFileName;
            this.Source4upload = this.SourcesService.newSourceForUpload(fileContent, filename, this.currentFilterName);
            this.SourcesService.CheckUpload(this.Source4upload);
        }
    }
    private gotItems4Checking() {
        this.WizPhase = 2;
    }
    back() {
        this.currentFileName = "";
        this.Source4upload = null;
        this.WizPhase = 1;
    }
    get DataToCheck(): IncomingItemsList | null {
        if (this.WizPhase == 2 && this.SourcesService.IncomingItems4Checking) return this.SourcesService.IncomingItems4Checking;
        else return null;
    }
    get ImportFilters(): ImportFilter[] | null {
        if (this.SourcesService.ImportFilters && this.SourcesService.ImportFilters.length > 0) return this.SourcesService.ImportFilters;
        else return null;
    }
    get ReviewSources(): ReadOnlySource[] {
        return this.SourcesService.ReviewSources;
    }
    DisplaySourcename(ROS: ReadOnlySource): string {
        if (ROS.source_Name == 'NN_SOURCELESS_NN' && ROS.source_ID == -1) return "Manually Created Items";
        else return ROS.source_Name;
    }
    public currentFilterName: string = "RIS";
    private currentFileName: string = "";
    FilterChanged(ruleName: string) {
        this.currentFilterName = ruleName;
    }
    Upload() {
        if (!this.Source4upload) return;
        this.SourcesService.Upload(this.Source4upload);
    }
    SourceUploaded() {
        alert("Source was uploaded succesfully. :-)");
        this.ItemListService.FetchWithCrit(this.ItemListService.ListCriteria, this.ItemListService.ListDescription);
        this.CodesetStatisticsService.GetReviewStatisticsCountsCommand();
    }
    ListSource(ros: ReadOnlySource) {
        let cr = new Criteria();
        //cr.onlyIncluded = false;// included ignore for sources
        //cr.showDeleted = true; // deleted ignore for sources
        cr.attributeSetIdList = "";
        cr.sourceId = ros.source_ID;
        cr.listType = "StandardItemList";
        let ListDescription: string = "Showing: " +  ((ros.source_Name == "NN_SOURCELESS_NN" && ros.source_ID == -1) ? "Manually Created (Sourceless) Items." : ros.source_Name);
        this.ItemListService.FetchWithCrit(cr, ListDescription);
        this._eventEmitter.PleaseSelectItemsListTab.emit();
    }
    ToggleDelSource(ros: ReadOnlySource) {
        //we should really show a "are you sure?" dialog...
        if ((ros.source_Name == "NN_SOURCELESS_NN" && ros.source_ID == -1) || ros.source_ID > 0) this.SourcesService.DeleteUndeleteSource(ros);
    }
}
