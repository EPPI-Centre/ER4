import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource, IncomingItemAuthor } from '../services/sources.service';
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
    public DateOfSearch: Date = new Date();
    public WizPhase: number = 1
    public ShowPreviewTable: boolean = false;
    public Source4upload: SourceForUpload | null = null;
    public currentFilterName: string = "RIS";
    private currentFileName: string = "";

    addFile() {
        //console.log('oo');
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
    
    private fileRead(e: ProgressEvent) {
        if (this.reader.result) {
            let fileContent: string = this.reader.result as string;
            //console.log("fileRead: " + fileContent.length);
            let filename = "Please update";
            if (this.currentFileName) filename = this.currentFileName.trim();
            this.Source4upload = this.SourcesService.newSourceForUpload(fileContent, filename, this.currentFilterName);
            this.SourcesService.CheckUpload(this.Source4upload);
        }
    }
    private gotItems4Checking() {
        console.log('gotItems4Checking')
        this.WizPhase = 2;
        if (!this.PreviewResultsAreGood()) this.ShowPreviewTable = true;
    }
    public PreviewResultsAreGood(): boolean {
        //true if all is well, false if we think user might have picked the wrong filter
        //console.log("PreviewResultsAreGood?");
        //if (this.DataToCheck) console.log("DataToCheck.totalReferences", this.DataToCheck.totalReferences)
        //else console.log("WTF?");
        if (this.DataToCheck && this.DataToCheck.totalReferences > 1) return true;
        return false;
    }
    back() {
        this.currentFileName = "";
        this.Source4upload = null;
        this.ShowPreviewTable = false;
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
    FilterChanged(ruleName: string) {
        this.currentFilterName = ruleName;
    }
    Upload() {
        if (!this.Source4upload) return;
        this.Source4upload.dateOfSerach = this.DateOfSearch.toJSON().slice(0, 10);
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
    IsSourceNameValid(): number {
        // zero if it's fine, 1 if empty, 2 if name-clash (we don't want 2 sources with the same name)
        //if (this.WizPhase != 2) return 1;
        if (this.Source4upload == null || this.Source4upload.source_Name.trim() == "") return 1;
        else {
            const s4u = this.Source4upload;
            if (
                this.SourcesService.ReviewSources.findIndex(found => found.source_Name == s4u.source_Name) == -1
            ) return 0;
            else return 2;
        };
    }
    public get togglePreviewPanelButtonText(): string {
        if (this.ShowPreviewTable) return 'Hide Preview';
        else return 'Show Preview';
    }
    public togglePreviewPanel() {        
        this.ShowPreviewTable = !this.ShowPreviewTable;        
    }
    public AuthorsString(IncomingItemAuthors: IncomingItemAuthor[]): string {
        //[LAST] + ' ' + [FIRST] + ' ' + [SECOND]
        let res: string = "";
        if (IncomingItemAuthors) {
            for (let IncomingItemAuthor of IncomingItemAuthors) {
                res += IncomingItemAuthor.lastName + ' ' + IncomingItemAuthor.firstName +
                    (IncomingItemAuthor.middleName.length > 0 ? ' ' + IncomingItemAuthor.middleName : '') + '; ';
                if (res.length > 60) {
                    res += " [et al.]";
                    break;
                }
            }
        }
        return res.trim();
    }
}
