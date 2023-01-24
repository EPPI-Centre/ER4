import { Component, Inject, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ActivatedRoute, Params } from '@angular/router';
import { SourcesService, ReadOnlySource, Source } from '../services/sources.service';
import { Subscription } from 'rxjs';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ItemListService } from '../services/ItemList.service';
import { CodesetStatisticsService } from '../services/codesetstatistics.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { TabStripComponent, SelectEvent } from '@progress/kendo-angular-layout';
import { Helpers } from '../helpers/HelperMethods';
import { saveAs, encodeBase64 } from '@progress/kendo-file-saver';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';



@Component({
    selector: 'SourcesComp',
    templateUrl: './sources.component.html',
    providers: [],
    styles: [
        `@keyframes oscillate {
          0%   {transform:rotate(35deg);}
          50% {transform:rotate(-35deg);}
          100% {transform:rotate(35deg);}
        }`
    ]
})

export class SourcesComponent implements OnInit, OnDestroy {
  constructor(
        private route: ActivatedRoute,
        private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
        private SourcesService: SourcesService,
        private notificationService: NotificationService,
        private ItemListService: ItemListService,
        private CodesetStatisticsService: CodesetStatisticsService,
        private ReviewerIdentityServ: ReviewerIdentityService,
        private ReviewerIdentityService: ReviewerIdentityService,
        private ConfirmationDialogService: ConfirmationDialogService,
    ) {    }

    ngOnInit() {
        console.log("init sources");
        this.SourceDeletedSubs = this.SourcesService.SourceDeleted.subscribe((value: number) => {
            this.SourceDeletedForever(value);
        })
        
        //see: https://blog.angularindepth.com/everything-you-need-to-know-about-the-expressionchangedafterithasbeencheckederror-error-e3fd9ce7dbb4
        //timeout might be needed, but apparently not!
        this.SourcesService.FetchSources();
        this.SourcesService.FetchImportFilters();
        this.SrcUpdatedSbus = this.SourcesService.SourceUpdated.subscribe(() => {
            this.SourceUpdated();
        })

      this.tabFromQueryString = this.route.queryParams.subscribe(params => {
          if (params['tabby']) {
            this.goToTab = params['tabby'];
          }
        });
    }
  async ngAfterViewInit() {
      if (this.goToTab == 'ManageSources') {
        if (this.SourcesService.ReviewSources.length > 0) await this.SourcesService.FetchSource(this.SourcesService.ReviewSources[0].source_ID);
        setTimeout(()=> { this.SelectTab(0); }, 50);
        // select the first source
      }
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
    private SourceDeletedSubs: Subscription = new Subscription();
    private SrcUpdatedSbus: Subscription = new Subscription();
    //we are going to use a clone of the selected source, cached here
    //this is to avoid dangerous recursion problems.
    private _CurrentSource: Source | null = null;
    private tabFromQueryString: Subscription | null = null;
    private goToTab: string = "";

    public get HasWriteRights(): boolean {
        return this.ReviewerIdentityService.HasWriteRights;
    }

    ToggleDelSource(ros: ReadOnlySource) {
        if ((ros.source_Name == "NN_SOURCELESS_NN" && ros.source_ID == -1) || ros.source_ID > 0) {
            let msg: string;
            if (ros.isDeleted) {
                msg = "Are you sure you want to undelete the<br> <b>\"" + ros.source_Name + "\"</b> source?<br/>Items within the source <b> will be marked as 'Included' </b>, with the exception of duplicates."
            }
            else {
                msg = "Are you sure you want to delete the<br> <b>\"" + ros.source_Name + "\"</b> source?<br/>Information about items state (<b>Included, Exluded or Deleted</b>) will be lost."
            }
            this.openConfirmationDialogDeleteUnDeleteSource(ros, msg);
        }
    }

    public openConfirmationDialogDeleteUnDeleteSource(ros: ReadOnlySource, msg: string) {

        this.ConfirmationDialogService.confirm('Please confirm', msg, false, '')
            .then(
                (confirmed: any) => {
                    //console.log('User confirmed source (un/)delete:', confirmed);
                    if (confirmed) {
                        this.ActuallyDeleteUndeleteSource(ros);
                    } else {
                        //alert('did not confirm');
                    }
                }
            )
            .catch(() => {
                //console.log('User dismissed the dialog (e.g., by using ESC, clicking the cross icon, or clicking outside the dialog)');
            });
    }

    ActuallyDeleteUndeleteSource(ros: ReadOnlySource) {
        this.SourcesService.DeleteUndeleteSource(ros);
    }

    get CurrentSource(): Source | null {
        if (this.SourcesService.CurrentSourceDetail == null && this.SourcesService.ReviewSources.length > 0) {
            this._CurrentSource = null;
            //setTimeout(() => { this.SourcesService.FetchSource(this.SourcesService.ReviewSources[0].source_ID); });//this might go wrong!!
        }
        else if (this._CurrentSource == null
          || (this.SourcesService.CurrentSourceDetail && this._CurrentSource.source_ID != this.SourcesService.CurrentSourceDetail.source_ID)) {
          this._CurrentSourceDateofSearch = null;
          let CS = JSON.parse(JSON.stringify(this.SourcesService.CurrentSourceDetail));//silly cloning: we want a new reference here!
          if (CS) this._CurrentSourceDateofSearch = new Date(CS.dateOfSerach);
          this._CurrentSource = CS;
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

    public get CurrentSourceIsEdited(): boolean {
      if (!this._CurrentSource || !this.SourcesService.CurrentSourceDetail || !this.CurrentSourceDateofSearch || !this.CanWrite()
        || this._CurrentSource.source_ID != this.SourcesService.CurrentSourceDetail.source_ID //only happens while we're changing source and busy animation is "on"
      ) return false;
        else if (
        this._CurrentSource.source_Name != this.SourcesService.CurrentSourceDetail.source_Name ||
            this.CurrentSourceDateofSearch.toISOString() != new Date(this._CurrentSource.dateOfSerach).toISOString() ||
            this._CurrentSource.sourceDataBase != this.SourcesService.CurrentSourceDetail.sourceDataBase ||
            this._CurrentSource.searchDescription != this.SourcesService.CurrentSourceDetail.searchDescription ||
            this._CurrentSource.searchString != this.SourcesService.CurrentSourceDetail.searchString ||
            this._CurrentSource.notes != this.SourcesService.CurrentSourceDetail.notes
        ) {
            //console.log("S is edited? "
            //    , this._CurrentSource.source_Name != this.SourcesService.CurrentSourceDetail.source_Name
            //    , this.CurrentSourceDateofSearch.toISOString()
            //    , this.CurrentSourceDateofSearch.toISOString() != new Date(this._CurrentSource.dateOfSerach).toISOString()
            //    , this._CurrentSource.sourceDataBase != this.SourcesService.CurrentSourceDetail.sourceDataBase
            //    , this._CurrentSource.searchDescription != this.SourcesService.CurrentSourceDetail.searchDescription
            //    , this._CurrentSource.searchString != this.SourcesService.CurrentSourceDetail.searchString
            //    , this._CurrentSource.notes != this.SourcesService.CurrentSourceDetail.notes
            //);
            return true;
        }
        else return false;
    }

    CancelEditSource() {
        this._CurrentSource = null;//will be re-populated in "get CurrentSource()"
    }

    BackToMain() {
        this.router.navigate(['Main']);
    }
    HideManuallyCreatedItems(ROS: ReadOnlySource): boolean {
        if (ROS.source_Name == 'NN_SOURCELESS_NN' && ROS.source_ID == -1) return true;
        else return false;
    }
    async SelectSource(ROS: ReadOnlySource) {
       await this.SourcesService.FetchSource(ROS.source_ID);
      setTimeout(() => { this.SelectTab(0); }, 50);//wait a tiny bit to ensure SourcesService is busy (which prevents loading the first source automatically).
    }
    IsSourceNameValid(): number {
        // zero if it's fine, 1 if empty, 2 if name-clash (we don't want 2 sources with the same name)
        //if (this.WizPhase != 2) return 1;
        if (this._CurrentSource == null) return 1;
        else {
            return this.SourcesService.IsSourceNameValid(this._CurrentSource.source_Name, this._CurrentSource.source_ID);
        };
    }
    public get SomeSourceIsBeingDeleted(): boolean {
        return this.SourcesService.SomeSourceIsBeingDeleted;
    }
    CanDeleteSourceForever(): boolean {
        if (this._CurrentSource == null || !this.CanWrite() || this.SourcesService.SomeSourceIsBeingDeleted) return false;
        else if (this._CurrentSource.isFlagDeleted && this._CurrentSource.isMasterOf == 0) return true;
        else return false;
    }
    confirmSourceDeletionClose(status: string) {
        this.confirmSourceDeletionOpen = false;
        if (status == 'yes' && this._CurrentSource) {
            //console.log("I'll delete the source");
            this.SourcesService.DeleteSourceForever(this._CurrentSource.source_ID);
        }
    }
    SourceDeletedForever(sourceId: number) {
        //console.log("SourceDeletedForever", sourceId);
        if (this._CurrentSource && this._CurrentSource.source_ID == sourceId) {
            this.showDeletedForeverNotification(this._CurrentSource.source_Name, this.SourcesService.LastDeleteForeverStatus, sourceId);
            this._CurrentSource = null;
        }
        else {//user might have changed source!!!
            this.showDeletedForeverNotification("*missing name*", this.SourcesService.LastDeleteForeverStatus, sourceId);
        }
        this.ItemListService.Refresh();
        this.CodesetStatisticsService.GetReviewStatisticsCountsCommand();
    }
    public showDeletedForeverNotification(sourcename: string, status: string, sourceId: number): void {
        console.log('got into showDeletedForeverNotification');
        let typeElement: "success" | "error" | "none" | "warning" | "info" | undefined = undefined;
        let contentSt: string = "";
        if (status == "No deletion is running") {
            typeElement = "success";
            contentSt = 'Permanent deletion of source "' + sourcename + '" completed successfully.';
        }
        else if (status == "Deletion running for SourceId: " + sourceId.toString()) {
            typeElement = "success";
            contentSt = 'Permanent deletion of source "' + sourcename + '" is running (it can take some time!)';
        }
        else if (status.indexOf("Deletion is  already running for a different source") > -1) {
            typeElement = "error";
            contentSt = 'Permanent deletion of source "' + sourcename + '" did not start, because another source is being deleted.';
        }
        else {//this is moot. We're now handling errors in the service...
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
            while (this.SourcesService.IsBusy && counter < 4*120) {
                counter++;
                await Helpers.Sleep(200);
                console.log("waiting, cycle n: " + counter);
            }
        //will remain here for up to 96s (200ms*4*120)... counter ensures we won't have an endless loop.
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
  SelectTab(i: number) {
    if (!this.tabstrip) return;
    else {
      let t = this.tabstrip.tabs.get(i);
      if (!t) return;
      let e = new SelectEvent(i, t.title);
      this.tabstrip.selectTab(i);
      this.onTabSelect(e);
    }
  }
    onTabSelect($event: SelectEvent) {
        if ($event.title == 'Manage Sources'
            && this._CurrentSource == null && this.SourcesService.ReviewSources
            && this.SourcesService.ReviewSources.length > 0 && !this.SourcesService.IsBusy) {
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
      if (this.tabFromQueryString) this.tabFromQueryString.unsubscribe();
    }


    public async CreateSourceReport() {
        if (this._CurrentSource != null) {

            let ReportParameter: Source = this._CurrentSource;
            let report: string = await this.SourcesService.GetSourceReport(ReportParameter);

            //const dataURI = "data:text/plain;base64," + encodeBase64(Helpers.AddHTMLFrame(report, this._baseUrl, "Source Table"));
            //saveAs(dataURI, "Source table.html");
            if (report != "") {//report could be made to come back empty if there was an error - error messages will be shown by the service
                Helpers.OpenInNewWindow(report, this._baseUrl);
            }
        }
    }

}
