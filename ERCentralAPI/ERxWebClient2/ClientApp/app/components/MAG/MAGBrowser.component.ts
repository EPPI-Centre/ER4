import { Component, OnInit, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { searchService } from '../services/search.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router, NavigationEnd } from '@angular/router';
import { MagPaper,  MagFieldOfStudy } from '../services/MAGClasses.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';
import { NotificationService } from '@progress/kendo-angular-notification';


@Component({
    selector: 'MAGBrowser',
    templateUrl: './MAGBrowser.component.html',
    providers: []
})

export class MAGBrowser implements OnInit, OnDestroy {

    constructor(
        public _magAdvancedService: MAGAdvancedService,
        public _magBrowserService: MAGBrowserService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _routingStateService: MAGBrowserHistoryService,
        private _location: Location,
        public _notificationService: NotificationService,
        private router: Router
    ) {

    }

    public browsingHistory: NavigationEnd[] = [];
    public MAGPapers: MagPaper[] = [];
    public description: string = '';
    public ShowSelectedPapers: string = '';
    public isShowDivIf = false;

    ngOnInit() {

        this.browsingHistory = this._routingStateService.getHistory();
        
    }
    ngOnDestroy() {

        this._magBrowserService.Clear();
        this.Clear();
    }
    public UpdatePageSize(pageSize: number) {

        if (pageSize != null && pageSize > 0) {
            this._magBrowserService.pageSize = pageSize;
            this.GetParentAndChildRelatedPapers(this._magBrowserService.currentFieldOfStudy);
        }
    }
    public toggleDisplayDivIf() {
        this.isShowDivIf = !this.isShowDivIf;
    }
    public GetPDFLinks() : string[] {
        let links: string = this._magAdvancedService.currentMagPaper.pdfLinks;
        if (links != null && links != '') {
            var pdfLinks = links.split(';');
            return pdfLinks;
        } else {
            return [];      
        }
    }
    showMAGRunMessage(notifyMsg: string) {

        this._notificationService.show({
            content: notifyMsg,
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            closable: true
        });
    }
    public GetMagPaperRef(magPaperRefId: number) {

        this._magAdvancedService.FetchMagPaperId(magPaperRefId).then(

            (result: MagPaper) => {

                this._magAdvancedService.PostFetchMagPaperCalls(result);
            });
     
    }
    public Back() {
        this._location.back();
    }
    private AddPaperToSelectedList(paperId: number) {

        if (!this.IsInSelectedList(paperId)) {
            this._magBrowserService.SelectedPaperIds.push(paperId);
            this.UpdateSelectedCount();
            this.AddToSelectedList();
        }
    }
    public AddToSelectedList() {

        for (var i = 0; i < this._magBrowserService.SelectedPaperIds.length; i++) {
            var item = this._magBrowserService.MAGList.papers.filter(x => x.paperId == this._magBrowserService.SelectedPaperIds[i])[0];
            if (item != null && this._magBrowserService.selectedPapers.findIndex(x => x.paperId == this._magBrowserService.SelectedPaperIds[i]) == -1) {
                this._magBrowserService.selectedPapers.push(item);
            }
        }
    }
    private RemovePaperFromSelectedList(paperId: number): any {

        let pos: number = this._magBrowserService.SelectedPaperIds.indexOf(paperId);
        if (pos > -1)
            this._magBrowserService.SelectedPaperIds.splice(pos, 1);
        this.UpdateSelectedCount();
    }
    public GetParentAndChildRelatedPapers(item: MagFieldOfStudy) {
        this._magBrowserService.currentFieldOfStudy = item;
        let FieldOfStudyId: number = item.fieldOfStudyId;
        this._magBrowserService.ParentTopic = item.displayName;
        this._magBrowserService.WPChildTopics = [];
        this._magBrowserService.WPParentTopics = [];
        this._magBrowserService.Clear();
        this._magAdvancedService.currentMagPaper = new MagPaper();

        this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", FieldOfStudyId).then(
            () => {
                this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", FieldOfStudyId).then(
                    () => {
                        this._magBrowserService.GetPaperListForTopic(FieldOfStudyId);
                    });
            });
    }
    public HideCitatedBy(): boolean {

        let len: number = this._magBrowserService.MagCitationsByPaperList.papers.length;
        if (len > 0) {
            return false;
        } else {
            return true;
        }
    }
    public HideSelectedPapers(): boolean {

        let len: number = this._magBrowserService.SelectedPaperIds.length;
        if (len > 0) {
            return false;
        } else {
            return true;
        }

    }
    private IsInSelectedList(paperId: number): boolean {

        if (this._magBrowserService.SelectedPaperIds.indexOf(paperId) > -1)
            return true;
        else
            return false;
    }
    private UpdateSelectedCount(): any {
        
        this.ShowSelectedPapers = "Selected (" + this._magBrowserService.SelectedPaperIds.length.toString() + ")";
    }
    public InOutReview(paper: MagPaper) {

        if (paper.linkedITEM_ID == 0) {

            if (paper.isSelected) {

                this.RemovePaperFromSelectedList(paper.paperId);
                paper.isSelected = false;
            }
            else {

                this.AddPaperToSelectedList(paper.paperId);
                paper.isSelected = true;
            }
        }
        else {

            this.showMAGRunMessage("This paper is already in your review");
        }

    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {

        return this._magBrowserService.IsBusy || this._magAdvancedService.IsBusy;
    }
    public Clear() {

        this._magAdvancedService.currentMagPaper = new MagPaper();
        this.MAGPapers = [];

    }
    public CanDeleteMAGRun(): boolean {

        return this.HasWriteRights;
    }
    public CanAddNewMAGSearch(): boolean {

        if (this.description != '' && this.description != null && this.HasWriteRights
        ) {
            return true;
        } else {
            return false;
        }
    }
}
