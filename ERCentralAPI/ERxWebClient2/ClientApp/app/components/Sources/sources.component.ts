import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { SourcesService, ReadOnlySource, Source } from '../services/sources.service';
import { Subscription } from 'rxjs';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ItemListService } from '../services/ItemList.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { TabStripComponent, SelectEvent } from '@progress/kendo-angular-layout';
import { Helpers } from '../helpers/HelperMethods';
import { ReviewInfoService } from '../services/ReviewInfo.service';


@Component({
    selector: 'SourcesComp',
    templateUrl: './sources.component.html',
    providers: []
})

export class SourcesComponent implements OnInit, OnDestroy {
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private SourcesService: SourcesService,
        private notificationService: NotificationService,
        private ItemListService: ItemListService,
        private CodesetStatisticsService: CodesetStatisticsService,
        private ReviewerIdentityServ: ReviewerIdentityService,
        private reviewInfoService: ReviewInfoService
    ) {    }

    ngOnInit() {
        console.log("init sources");
        this.SourceDeletedSubs = this.SourcesService.SourceDeleted.subscribe((value: number) => {
            this.SourceDeletedForever(value);
        })
        this.GotSourcesSubs = this.SourcesService.gotSource.subscribe(() => {
            if (this.SourcesService.CurrentSourceDetail && this._CurrentSource == null) {
                this._CurrentSource = this.SourcesService.CurrentSourceDetail;
                this._CurrentSourceDateofSearch = new Date(this._CurrentSource.dateOfSerach);
            }
            this.GotSourcesSubs.unsubscribe();
        })
        //see: https://blog.angularindepth.com/everything-you-need-to-know-about-the-expressionchangedafterithasbeencheckederror-error-e3fd9ce7dbb4
        //timeout might be needed, but apparently not!
        this.SourcesService.FetchSources();
        this.SourcesService.FetchImportFilters();
        this.SrcUpdatedSbus = this.SourcesService.SourceUpdated.subscribe(() => {
            this.SourceUpdated();
        })
    }
    public get ReviewIsMagEnabled(): boolean {
        if (this.reviewInfoService.ReviewInfo.magEnabled
            //&& this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin
        ) return true;
        return false;
    }
    ngAfterViewInit() {
       
        //if (this.SourcesService.ReviewSources && this.SourcesService.ReviewSources.length == 0) {
        //     this.SourcesService.FetchSources();
        //}
        //this.SourcesService.FetchImportFilters();
    }
    onSubmit(): boolean {
        console.log("Sources onSubmit");
        return false;
    }
    @ViewChild('tabstrip') public tabstrip!: TabStripComponent;
    get ReviewSources(): ReadOnlySource[] {
        //console.log("rev srcs:", this.SourcesService.ReviewSources.length);
        return this.SourcesService.ReviewSources;
    }
    private GotSourcesSubs: Subscription = new Subscription();
    private SourceDeletedSubs: Subscription = new Subscription();
    private SrcUpdatedSbus: Subscription = new Subscription();
    //we are going to use a clone of the selected source, cached here
    //this is to avoid dangerous recursion problems.
    private _CurrentSource: Source | null = null;
    
    get CurrentSource(): Source | null {
        if (this.SourcesService.CurrentSourceDetail == null && this.SourcesService.ReviewSources.length > 0) {
            this._CurrentSource = null;
            //setTimeout(() => { this.SourcesService.FetchSource(this.SourcesService.ReviewSources[0].source_ID); });//this might go wrong!!
        }
        else if (this._CurrentSource == null
            || (this.SourcesService.CurrentSourceDetail && this._CurrentSource.source_ID != this.SourcesService.CurrentSourceDetail.source_ID)) {
            this._CurrentSource = JSON.parse(JSON.stringify(this.SourcesService.CurrentSourceDetail));//silly cloning: we want a new reference here!
            if (this._CurrentSource) this._CurrentSourceDateofSearch = new Date(this._CurrentSource.dateOfSerach);
        }
        return this._CurrentSource;
    }
    private _CurrentSourceDateofSearch: Date | null = null;
    public get CurrentSourceDateofSearch(): Date | null{
        if (this._CurrentSource == null) {
            return null;
        }
        else if (this._CurrentSourceDateofSearch == null) {
            try {
                this._CurrentSourceDateofSearch = new Date(this._CurrentSource.dateOfSerach);
            } catch { this._CurrentSourceDateofSearch = null;}
        }
        return this._CurrentSourceDateofSearch;
    }
    public confirmSourceDeletionOpen: boolean = false;
    public HelpAndFeebackContext: string = "sources\\file";

    HasCurrentSourceDateofSearch(): boolean {
        if (this._CurrentSource == null) return false;
        else return true;
    }
    public set CurrentSourceDateofSearch(newDate: Date | null) {
        this._CurrentSourceDateofSearch = newDate;
    }
    BackToMain() {
        this.router.navigate(['Main']);
    }
    HideManuallyCreatedItems(ROS: ReadOnlySource): boolean {
        if (ROS.source_Name == 'NN_SOURCELESS_NN' && ROS.source_ID == -1) return true;
        else return false;
    }
    SelectSource(ROS: ReadOnlySource) {
        this.tabstrip.selectTab(0);
        this.SourcesService.FetchSource(ROS.source_ID);
    }
    IsSourceNameValid(): number {
        // zero if it's fine, 1 if empty, 2 if name-clash (we don't want 2 sources with the same name)
        //if (this.WizPhase != 2) return 1;
        if (this._CurrentSource == null) return 1;
        else {
            return this.SourcesService.IsSourceNameValid(this._CurrentSource.source_Name, this._CurrentSource.source_ID);
        };
    }
    CanDeleteSourceForever(): boolean {
        if (this._CurrentSource == null) return false;
        else if (this._CurrentSource.isFlagDeleted && this._CurrentSource.isMasterOf == 0) return true;
        else return false;
    }
    confirmSourceDeletionClose(status: string) {
        this.confirmSourceDeletionOpen = false;
        if (status == 'yes' && this._CurrentSource) {
            console.log("I'll delete the source");
            this.SourcesService.DeleteSourceForever(this._CurrentSource.source_ID);
        }
    }
    SourceDeletedForever(sourceId: Number) {
        if (this._CurrentSource && this._CurrentSource.source_ID == sourceId) {
            this.showDeletedForeverNotification(this._CurrentSource.source_Name, this.SourcesService.LastDeleteForeverStatus);
            this._CurrentSource = null;
        }
        else {
            this.showDeletedForeverNotification("*missing name*", this.SourcesService.LastDeleteForeverStatus);
            //user might have changed source!!!
        }
        this.ItemListService.Refresh();
        this.CodesetStatisticsService.GetReviewStatisticsCountsCommand();
    }
    public showDeletedForeverNotification(sourcename: string, status: string): void {
        console.log('got into showDeletedForeverNotification');
        let typeElement: "success" | "error" | "none" | "warning" | "info" | undefined = undefined;
        let contentSt: string = "";
        if (status == "Success") {
            typeElement = "success";
            contentSt = 'Permanent deletion of source "' + sourcename + '" completed successfully.';
        }//type: { style: 'error', icon: true }
        else {
            typeElement = "error";
            contentSt = 'Permanent deletion of source "' + sourcename + '" failed, if the problem persists, please contact EPPISupport.';
        }
        this.notificationService.show({
            content: contentSt,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: typeElement, icon: true },
            closable: true
        });
    }

    SaveSource() {
        //alert('Not yet ;-)');
        if (this._CurrentSource) {
            if (this._CurrentSourceDateofSearch) this._CurrentSource.dateOfSerach = this._CurrentSourceDateofSearch.toJSON().slice(0, 10);
            this.SourcesService.UpdateSource(this._CurrentSource);
        }
    }
    async SourceUpdated() {
        let counter: number = 0;
        //setTimeout(() => {
            while (this.SourcesService.IsBusy && counter < 3*120) {
                counter++;
                await Helpers.Sleep(200);
                console.log("waiting, cycle n: " + counter);
            }
        //will remain here for up to 72s (200ms*3*120)... counter ensures we won't have an endless loop.
        this.showUploadedNotification(this.SourcesService.LastUploadOrUpdateStatus);
    }
    
    private showUploadedNotification(status: string): void {

        let typeElement: "success" | "error" | "none" | "warning" | "info" | undefined = undefined;
        let contentSt: string = "";
        if (status == "Success") {
            typeElement = "success";
            contentSt = 'Source updated succesfully.';
        }//type: { style: 'error', icon: true }
        else {
            typeElement = "error";
            contentSt = 'The source update failed, if the problem persists, please contact EPPISupport.';
        }
        this.notificationService.show({
            content: contentSt,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: typeElement, icon: true },
            closable: true
        });
    }

    ConfirmDeleteSourceForever() {
        //alert('Not yet ;-)');
        this.confirmSourceDeletionOpen = true;
    }
    onTabSelect($event: SelectEvent) {
        if ($event.title == 'Manage Sources'
            && this._CurrentSource == null && this.SourcesService.ReviewSources
            && this.SourcesService.ReviewSources.length > 0) {
            //let's go and get the first source:
            this.SourcesService.FetchSource(this.SourcesService.ReviewSources[0].source_ID);
        }
        if ($event.title == 'Import Items') {
            this.HelpAndFeebackContext = "sources\\file";
        }
        else if ($event.title == 'Manage Sources') {
            this.HelpAndFeebackContext = "sources\\managesources";
        }
        else if ($event.title == 'PubMed') {
            this.HelpAndFeebackContext = "sources\\pubmed";
        }
        else {
            this.HelpAndFeebackContext = "sources\\file";
        }
    }
    FormatDate(DateSt: string): string {
        return Helpers.FormatDate2(DateSt);
    }
    public CanWrite(): boolean {
        //console.log('CanWrite? is busy: ', this.SourcesService.IsBusy);
        if (this.ReviewerIdentityServ.HasWriteRights && !this.SourcesService.IsBusy) return true;
        else return false;
    }
    public IsServiceBusy(): boolean {
        //console.log('CanWrite? is busy: ', this.SourcesService.IsBusy);
        return this.SourcesService.IsBusy;
    }
    ngOnDestroy() {
        if (this.SourceDeletedSubs) this.SourceDeletedSubs.unsubscribe();
        if (this.SrcUpdatedSbus) this.SrcUpdatedSbus.unsubscribe();
        if (this.GotSourcesSubs) this.GotSourcesSubs.unsubscribe();
    }

}
