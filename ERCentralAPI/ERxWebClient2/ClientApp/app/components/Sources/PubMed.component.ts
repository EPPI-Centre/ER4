import { Component, Inject, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, ItemList, Criteria } from '../services/ItemList.service';
import { SourcesService, IncomingItemsList, ImportFilter, SourceForUpload, Source, ReadOnlySource, PubMedSearch, IncomingItemAuthor } from '../services/sources.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { Subscription } from 'rxjs';
import { NotificationService } from '@progress/kendo-angular-notification';
import { Helpers } from '../helpers/HelperMethods';


@Component({
    selector: 'PubMedComp',
    templateUrl: './PubMed.component.html',
    providers: []
})

export class PubMedComponent implements OnInit, OnDestroy {
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
        this.gotPmSearchToCheckSubs = this.SourcesService.gotPmSearchToCheck.subscribe(() => {
            this.gotPmSearchToCheck();
        });
        this.PubMedSearchImportedSubs = this.SourcesService.PubMedSearchImported.subscribe(() => {
            this.PubMedSearchImported();
        });}
    public WizPhase: number = 1
    public ShowPreviewTable: boolean = false;
    public NewSearchString: string = "";

    private gotPmSearchToCheckSubs: Subscription = new Subscription();
    private PubMedSearchImportedSubs: Subscription = new Subscription();

    private _DataToCheck: PubMedSearch | null = null;
    get DataToCheck(): PubMedSearch | null {
        if (this.WizPhase == 2 && this._DataToCheck) return this._DataToCheck;
        else return null;
    }
    set AdjustedShowStart(value: number) {
        if (this._DataToCheck && value > 0) {
            this._DataToCheck.showStart = value;
            if (this._DataToCheck.showStart > this._DataToCheck.showEnd) {
                //adjust showEnd as it's too low!
                this._DataToCheck.showEnd = this._DataToCheck.showStart;
            }
        }
    }
    get AdjustedShowStart(): number {
        if (this._DataToCheck) return this._DataToCheck.showStart;
        else return 1;
    }
    set AdjustedShowEnd(value: number) {
        if (this._DataToCheck && value > 0) {
            this._DataToCheck.showEnd = value;
            //if (this._DataToCheck.showEnd < this._DataToCheck.showStart) {
            //    //adjust showStart as it's too high!
            //    //this._DataToCheck.showStart = this._DataToCheck.showEnd;
            //}
        }
    }
    get AdjustedShowEnd(): number {
        if (this._DataToCheck) return this._DataToCheck.showEnd;
        else return 1;
    }
    set AdjustedSaveStart(value: number) {
        if (this._DataToCheck && value > 0) {
            this._DataToCheck.saveStart = value;
            if (this._DataToCheck.saveStart > this._DataToCheck.saveEnd) {
                //adjust saveEnd as it's too low!
                this._DataToCheck.saveEnd = this._DataToCheck.saveStart;
            }
        }
    }
    get AdjustedSaveStart(): number {
        if (this._DataToCheck) {
            if (this._DataToCheck.saveStart <= 0) this._DataToCheck.saveStart = 1;
            return this._DataToCheck.saveStart;
        }
        else return 1;
    }
    set AdjustedSaveEnd(value: number) {
        if (this._DataToCheck && value > 0) {
            this._DataToCheck.saveEnd = value;
            //if (this._DataToCheck.saveEnd < this._DataToCheck.saveStart) {
            //    //adjust saveStart as it's too high!
            //    //this._DataToCheck.saveStart = this._DataToCheck.saveEnd;
            //}
        }
    }
    get AdjustedSaveEnd(): number {
        if (this._DataToCheck) {
            if (this._DataToCheck.saveEnd == 0) {
                this._DataToCheck.saveEnd = this._DataToCheck.queMax > 10000 ? 10000 : this._DataToCheck.queMax;
            }
            return this._DataToCheck.saveEnd;
        }
        else return 1;
    }
    AdjustedMax(): number {
        if (this._DataToCheck) {
            return this._DataToCheck.queMax > 10000 ? 10000 : this._DataToCheck.queMax ;
        }
        else return 1;
    }
    public togglePreviewPanel() {
        this.ShowPreviewTable = !this.ShowPreviewTable;
    }
    DoNewSearch() {
        if (this.NewSearchString.trim().length < 2) return;
        this.SourcesService.FetchNewPubMedSearch(this.NewSearchString);
    }
    GetSearchPreview() {
        if (this._DataToCheck) {
            this._DataToCheck.saveEnd = 0;
            this._DataToCheck.saveStart = 0;
            this.SourcesService.ActOnPubMedSearch(this._DataToCheck);
        }
    }
    ImportPmSearch() {
        if (this._DataToCheck && this._DataToCheck.saveEnd != 0
            && this._DataToCheck.saveStart > 0
            && this._DataToCheck.saveStart <= this._DataToCheck.saveEnd
        ) {
            this._DataToCheck.showEnd = 0;
            this._DataToCheck.showStart = 0;
            this.SourcesService.ActOnPubMedSearch(this._DataToCheck);
        }
    }
    gotPmSearchToCheck() {
        this._DataToCheck = this.SourcesService.CurrentPMsearch;
        this.WizPhase = 2;
    }
    PubMedSearchImported() {
        if (this.DataToCheck) {
            this.showUploadedNotification(this.DataToCheck.itemsList.sourceName, this.SourcesService.LastUploadOrUpdateStatus);
            this.ItemListService.Refresh();
            this.CodesetStatisticsService.GetReviewStatisticsCountsCommand();
        }
        this.back();
    }
    public showUploadedNotification(sourcename: string, status: string): void {
        console.log("show(PM)UploadedNotification " + sourcename + " " + status, this.DataToCheck, this.SourcesService);
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
        this.SourcesService.ClearPMsearchState();
    }
    back() {
        //this.Source4upload = null;
        //this.NewSearchString = "";
        this._DataToCheck = null;
        this.ShowPreviewTable = false;
        this.WizPhase = 1;
    }
    onSubmit(): boolean {
        console.log("PubMed Search onSubmit");
        return false;
    }
    public get togglePreviewPanelButtonText(): string {
        if (this.ShowPreviewTable) return 'Hide Preview';
        else return 'Show Preview';
    }
    public CanWrite(): boolean {
        //console.log('CanWrite? is busy: ', this.SourcesService.IsBusy);
        if (this.ReviewerIdentityServ.HasWriteRights && !this.SourcesService.IsBusy) return true;
        else return false;
    }
    public AuthorsString(IncomingItemAuthors: IncomingItemAuthor[]): string {
        return SourcesService.LimitedAuthorsString(IncomingItemAuthors);
    }
    IsSourceNameValid(): number {
        // zero if it's fine, 1 if empty, 2 if name-clash (we don't want 2 sources with the same name)
        //if (this.WizPhase != 2) return 1;
        if (this._DataToCheck == null) return 1;
        else {
            return this.SourcesService.IsSourceNameValid(this._DataToCheck.itemsList.sourceName);
        };
    }
    FormatDate(DateSt: string): string {
        return Helpers.FormatDate2(DateSt);
    }
    ngOnDestroy(): void {
        if (this.gotPmSearchToCheckSubs) this.gotPmSearchToCheckSubs.unsubscribe();
        //if (this.gotItems4CheckingSbus) this.gotItems4CheckingSbus.unsubscribe();
    }
}
