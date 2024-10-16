import { Component, Inject, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Criteria } from '../services/ItemList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource, IncomingItemAuthor } from '../services/sources.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Subscription } from 'rxjs';
import { ModalService } from '../services/modal.service';



@Component({
    selector: 'ImportReferencesFile',
    templateUrl: './importreferencesfile.component.html',
    providers: []
})

export class ImportReferencesFileComponent implements OnInit, OnDestroy {
    
    //some inspiration taken from: https://malcoded.com/posts/angular-file-upload-component-with-express
    constructor(private router: Router,
                @Inject('BASE_URL') private _baseUrl: string,
        private ReviewerIdentityServ: ReviewerIdentityService,
        private CodesetStatisticsService: CodesetStatisticsService,
        private ItemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private SourcesService: SourcesService,
        private notificationService: NotificationService,
        private ModalService: ModalService
    ) {    }
    ngOnInit() {
        this.reader.onload = (e) => this.fileRead(e);
        this.gotItems4CheckingSbus = this.SourcesService.gotItems4Checking.subscribe(() => {
            this.gotItems4Checking();
        });
        //this.SourcesService.FetchSources();
        this.SrcUploadedSbus = this.SourcesService.SourceUploaded.subscribe(() => {
            this.SourceUploaded();
        })
    }
    @ViewChild('file') file: any;
    private SrcUploadedSbus: Subscription = new Subscription();
    private gotItems4CheckingSbus: Subscription = new Subscription();

    public DateOfSearch: Date = new Date();
    public WizPhase: number = 1
    public ShowPreviewTable: boolean = false;
    public Source4upload: SourceForUpload | null = null;
  private _currentFilter: ImportFilter | null = null;
  public get currentFilter(): ImportFilter {
    if (this._currentFilter == null) {
      if (this.SourcesService.ImportFilters.length > 0) {
        this._currentFilter = this.SourcesService.ImportFilters[0];
      }
      else {
        return {
          filterID: -1,
          ruleName: "Loading...",
          description: 'List of filters is loading',
          startOfNewRec: '',
        };
      }
    }
    return this._currentFilter;
  }
    private currentFileName: string = "";

    addFile() {
        //console.log('oo');
        this.file.nativeElement.click();
    }
    private reader = new FileReader();
    
    onFilesAdded() {
        const files: { [key: string]: File } = this.file.nativeElement.files;
        const file: File = files[0];
        console.log("onFilesAdded", file.size, file.name);
        if (file) {
            //if (file.size > 31457280 && 1 !== 1) {
            if (file.size >   52428800 ) {
                //console.log("onFilesAdded", file.size, file.name);
                this.ModalService.GenericErrorMessage("Sorry, the <strong>maximum</strong> file size is <strong>50MB</strong>. Please select a smaller file.<br /><br />"
                    + "Alternatively, you can edit this file with a text editor in order to produce two or more smaller files and upload them separately. "
                    + "You should ensure that you'll divide the files in the right spot, so to avoid splitting a single reference across two files.");
            }
            else {
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
    }
    
    private fileRead(e: ProgressEvent) {
        if (this.reader.result) {
            let fileContent: string = this.reader.result as string;
            //console.log("fileRead: " + fileContent.length);
            let filename = "Please update";
            if (this.currentFileName) filename = this.currentFileName.trim();
            this.Source4upload = this.SourcesService.newSourceForUpload(fileContent, filename, this.currentFilter.ruleName);
            this.SourcesService.CheckUpload(this.Source4upload).then(
                (result) => {
                    if (!result) {
                        this.back();
                    }
                }
            );
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
    get DefaultFilter(): ImportFilter | null {
        if (this.SourcesService.ImportFilters && this.SourcesService.ImportFilters.length > 0) return this.SourcesService.ImportFilters[0];
        else return null;
    }
    get ReviewSources(): ReadOnlySource[] {
        return this.SourcesService.ReviewSources;
    }
    DisplaySourcename(ROS: ReadOnlySource): string {
        if (ROS.source_Name == 'NN_SOURCELESS_NN' && ROS.source_ID == -1) return "Manually Created Items";
        else return ROS.source_Name;
    }
  FilterChanged(event: Event) {
    let selection = (event.target as HTMLOptionElement).value;
    const index = this.SourcesService.ImportFilters.findIndex(f => f.filterID.toString() == selection);
    if (index != -1) this._currentFilter = this.SourcesService.ImportFilters[index];
  }
    Upload() {
        if (!this.Source4upload) return;
        this.Source4upload.dateOfSerach = this.DateOfSearch.toJSON().slice(0, 10);
        this.SourcesService.Upload(this.Source4upload);
    }
    SourceUploaded() {
        if (this.Source4upload) {
            this.showUploadedNotification(this.Source4upload.source_Name, this.SourcesService.LastUploadOrUpdateStatus);
            this.ItemListService.Refresh();
            this.CodesetStatisticsService.GetReviewStatisticsCountsCommand();
        }
        this.back();
    }
    public showUploadedNotification(sourcename: string, status: string): void {
        let typeElement: "success" | "error" | "none" | "warning" | "info" | undefined = undefined;
        let contentSt: string = "";
        if (status == "Success") {
            typeElement = "success";
            contentSt = 'Upload of "' + sourcename +'" completed successfully.';
        }//type: { style: 'error', icon: true }
        else {
            typeElement = "error";
            contentSt = 'Upload of "' + sourcename + '" failed, if the problem persists, please contact EPPISupport.';
        }
        this.notificationService.show({
            content: contentSt,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: typeElement, icon: true },
            closable: true
        });
        this.SourcesService.ClearIncomingItems4Checking();
    }
    
    ToggleDelSource(ros: ReadOnlySource) {
        //we should really show a "are you sure?" dialog...
        if ((ros.source_Name == "NN_SOURCELESS_NN" && ros.source_ID == -1) || ros.source_ID > 0) this.SourcesService.DeleteUndeleteSource(ros);
    }
    IsSourceNameValid(): number {
        // zero if it's fine, 1 if empty, 2 if name-clash (we don't want 2 sources with the same name)
        //if (this.WizPhase != 2) return 1;
        if (this.Source4upload == null) return 1;
        else {
            return this.SourcesService.IsSourceNameValid(this.Source4upload.source_Name);
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
        return SourcesService.LimitedAuthorsString(IncomingItemAuthors);        
    }
    public CanWrite(): boolean {
        //console.log('CanWrite? is busy: ', this.SourcesService.IsBusy);
        if (this.ReviewerIdentityServ.HasWriteRights && !this.SourcesService.IsBusy) return true;
        else return false;
    }
    ngOnDestroy(): void {
        if (this.SrcUploadedSbus) this.SrcUploadedSbus.unsubscribe(); 
        if (this.gotItems4CheckingSbus) this.gotItems4CheckingSbus.unsubscribe();
    }
}
