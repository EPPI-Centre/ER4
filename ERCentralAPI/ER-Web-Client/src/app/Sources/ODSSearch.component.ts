import { Component, Inject, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource, ODSRetrieval, IncomingItemAuthor } from '../services/sources.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { Subscription } from 'rxjs';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Helpers } from '../helpers/HelperMethods';


@Component({
    selector: 'ODSSearchComp',
    templateUrl: './ODSSearch.component.html',
    providers: []
})

export class ODSSearchComponent implements OnInit, OnDestroy {
    //some inspiration taken from: https://malcoded.com/posts/angular-file-upload-component-with-express
    constructor( 
        private ItemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private CodesetStatisticsService: CodesetStatisticsService,
        private SourcesService: SourcesService,
        private ReviewerIdentityServ: ReviewerIdentityService,
        private notificationService: NotificationService
    ) {    }

    ngOnInit() {
      this.gotOdsRetrievalToCheckSubs = this.SourcesService.gotOdsRetrievalToCheck.subscribe(() => {
            this.gotOdsRetrievalToCheck();
        });
        this.ODSRetrievalImportedSubs = this.SourcesService.ODSRetrievalImported.subscribe(() => {
            this.ODSRetrievalImported();
        });
     }
        ngOnDestroy(): void {
          if (this.gotOdsRetrievalToCheckSubs) this.gotOdsRetrievalToCheckSubs.unsubscribe();
          if (this.ODSRetrievalImportedSubs) this.ODSRetrievalImportedSubs.unsubscribe();
    }

    public WizPhase: number = 1
    public ShowPreviewTable: boolean = false;
    public NewURLString: string = "";
    private _DataToCheck: ODSRetrieval | null = null;
    private gotOdsRetrievalToCheckSubs: Subscription = new Subscription();
    private ODSRetrievalImportedSubs: Subscription = new Subscription();
    private readonly PageSize: number = 20;
    private readonly AbsoluteMax: number = 10000;
    private _adjustedShowStart: number = 1;
    private _adjustedShowEnd: number = 21;


    get DataToCheck(): ODSRetrieval | null {
        if (this.WizPhase == 2 && this._DataToCheck) return this._DataToCheck;
        else return null;
    }

    public CanWrite(): boolean {
        //console.log('CanWrite? is busy: ', this.SourcesService.IsBusy);
        if (this.ReviewerIdentityServ.HasWriteRights && !this.SourcesService.IsBusy) return true;
        else return false;
    }

    DoRetrieval() {
        if (this.NewURLString.trim().length < 2) return;
        this.SourcesService.FetchNewODSRetrieval(this.NewURLString);
    }
    public togglePreviewPanel() {
        this.ShowPreviewTable = !this.ShowPreviewTable;
    }
    back() {
        this._DataToCheck = null;
        this.ShowPreviewTable = false;
        this.WizPhase = 1;
    }
    public get togglePreviewPanelButtonText(): string {
        if (this.ShowPreviewTable) return 'Hide Preview';
        else return 'Show Preview';
    }
    GetSearchPreview() {
        if (this._DataToCheck) {
            this._DataToCheck.saveEnd = 0;
            this._DataToCheck.saveStart = 0;
            this.SourcesService.ActOnODSRetrieval(this._DataToCheck);
        }
    }
    onSubmit(): boolean {
        console.log("ODS Retrieval onSubmit");
        return false;
    }
    IsSourceNameValid(): number {
        // zero if it's fine, 1 if empty, 2 if name-clash (we don't want 2 sources with the same name)
        //if (this.WizPhase != 2) return 1;
        if (this._DataToCheck == null) return 1;
        else {
            return this.SourcesService.IsSourceNameValid(this._DataToCheck.itemsList.sourceName);
        };
    }

    // Highest value either control may take (end-of-range limit)
    AdjustedMax(): number {
       if (this._DataToCheck) {
        const max = Math.min(this._DataToCheck.queMax ?? this.AbsoluteMax, this.AbsoluteMax);
        return Math.max(1, max);
        }
        else return 1;
    }
    // Highest valid start: the 1 + n*20 value of the page that contains the max
    public MaxStart(): number {
        return 1 + Math.floor((this.AdjustedMax() - 1) / this.PageSize) * this.PageSize;
    }
    // Snaps any number to a valid 1 + n*20 value within [1, MaxStart()]
    private SnapStart(value: number): number {
        if (value == null || isNaN(value)) return 1;
        const n = Math.round((value - 1) / this.PageSize);
        const snapped = 1 + Math.max(0, n) * this.PageSize;
        return Math.min(snapped, this.MaxStart());
    }
    private ClampEnd(value: number, start: number): number {
        if (this._DataToCheck) {
            const max = this.AdjustedMax();
            const min = this.MinEndFor(start);
            if (value == null || isNaN(value) || value <= 0) return min;
            if (value >= max) return max;
            const snapped = Math.round(value / this.PageSize) * this.PageSize;
            return Math.min(Math.max(snapped, min), max);
        }
        else return 1;
    }
    // End must be at least start + 20, but never more than the max
    public MinEndFor(start: number): number {
        return Math.min(start + this.PageSize - 1, this.AdjustedMax());
    }

    get AdjustedShowStart(): number {
        if (this._DataToCheck) {
            this._DataToCheck.showStart = this.SnapStart(this._DataToCheck.showStart);
            return this._DataToCheck.showStart;
        }
        else return 1;
    }
    set AdjustedShowStart(value: number) {
        if (this._DataToCheck) {
            this._DataToCheck.showStart = this.SnapStart(value);
            this._DataToCheck.showEnd = this.ClampEnd(this._DataToCheck.showEnd, this._DataToCheck.showStart);
        }
    }
    get AdjustedShowEnd(): number {
        if (this._DataToCheck) {
            this._DataToCheck.showEnd = this.ClampEnd(this._DataToCheck.showEnd, this.AdjustedShowStart);
            return this._DataToCheck.showEnd;
        }
        else return 1;
    }
    set AdjustedShowEnd(value: number) {
        if (this._DataToCheck)
            this._DataToCheck.showEnd = this.ClampEnd(value, this.AdjustedShowStart);
    }

    get AdjustedSaveStart(): number {
        if (this._DataToCheck) {
            this._DataToCheck.saveStart = this.SnapStart(this._DataToCheck.saveStart);
            return this._DataToCheck.saveStart;
        }
        else return 1;
    }
    set AdjustedSaveStart(value: number) {
        if (this._DataToCheck) {
            this._DataToCheck.saveStart = this.SnapStart(value);
            this._DataToCheck.saveEnd = this.ClampEnd(this._DataToCheck.saveEnd, this._DataToCheck.saveStart);
        }
    }
    get AdjustedSaveEnd(): number {
        if (this._DataToCheck) {
            this._DataToCheck.saveEnd = this.ClampEnd(this._DataToCheck.saveEnd, this.AdjustedSaveStart);
            return this._DataToCheck.saveEnd;
        }
        else return 1;
    }
    set AdjustedSaveEnd(value: number) {
        if (this._DataToCheck)
            this._DataToCheck.saveEnd = this.ClampEnd(value, this.AdjustedSaveStart);
    }

    get ImportRangeOK(): boolean {
        if ((this.AdjustedSaveEnd - this.AdjustedSaveStart) < 10000) {
            return true
        }
        else {
            return false
        }
    }
    ImportODSRetrieval() {
        if (this._DataToCheck && this._DataToCheck.saveEnd != 0
            && this._DataToCheck.saveStart > 0
            && this._DataToCheck.saveStart <= this._DataToCheck.saveEnd
        ) {
            this._DataToCheck.showEnd = 0;
            this._DataToCheck.showStart = 0;
            this.SourcesService.ActOnODSRetrieval(this._DataToCheck); 
        }
    }
    public AuthorsString(IncomingItemAuthors: IncomingItemAuthor[]): string {
        return SourcesService.LimitedAuthorsString(IncomingItemAuthors);
    }
    FormatDate(DateSt: string): string {
        return Helpers.FormatDate2(DateSt);
    }
    ODSRetrievalImported() {
        if (this.DataToCheck) {
            this.showUploadedNotification(this.DataToCheck.itemsList.sourceName, this.SourcesService.LastUploadOrUpdateStatus);
            this.ItemListService.Refresh();
            this.CodesetStatisticsService.GetReviewStatisticsCountsCommand();
        }
        this.back();
    }
    public showUploadedNotification(sourcename: string, status: string): void {
        console.log("show(ODSRetrieval)UploadedNotification " + sourcename + " " + status, this.DataToCheck, this.SourcesService);
        let typeElement: "success" | "error" | "none" | "warning" | "info" | undefined = undefined;
        let contentSt: string = "";
        if (status == "Success") {
            typeElement = "success";
            contentSt = 'Upload of "' + sourcename + '" completed successfully.';
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
        this.SourcesService.ClearOdsRetrievalState();
    }
    gotOdsRetrievalToCheck() {
        this._DataToCheck = this.SourcesService.CurrentODSRetrieval;
        if (this._DataToCheck){
          this.WizPhase = 2;
        }
    }
    isEvidenceRepositoryUrl(input: string): boolean {
  const trimmed = (input ?? '').trim();
  if (!trimmed || /\s/.test(trimmed)) return false; // reject any internal whitespace
  try {
    const { protocol, hostname } = new URL(trimmed);
    return (protocol === 'https:' || protocol === 'http:') &&
      (hostname === 'evidence-repository.org' || hostname.endsWith('.evidence-repository.org'));
  } catch {
    return false;
  }
}





}
