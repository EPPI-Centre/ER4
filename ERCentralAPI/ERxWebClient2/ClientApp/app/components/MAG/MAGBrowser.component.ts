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
    public SelectedPaperIds: number[] = [];
    public ShowSelectedPapers: string = '';
    public selectedPapers: MagPaper[] = [];
    public isShowDivIf = false;


    ngOnInit() {

        this.browsingHistory = this._routingStateService.getHistory();
        
    }
    ngOnDestroy() {

        this._magBrowserService.Clear();
        this.Clear();
    }
    public toggleDisplayDivIf() {
        this.isShowDivIf = !this.isShowDivIf;
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

            //() => { this.router.navigate(['MAGBrowser']); }

        );
    }
    public Back() {
        this._location.back();
    }
    private AddPaperToSelectedList(paperId: number) {

        if (!this.IsInSelectedList(paperId)) {
            this.SelectedPaperIds.push(paperId);
            this.UpdateSelectedCount();
            this.AddToSelectedList();
        }
    }
    public AddToSelectedList() {

        for (var i = 0; i < this.SelectedPaperIds.length; i++) {
            var item = this._magBrowserService.MAGList.papers.filter(x => x.paperId == this.SelectedPaperIds[i])[0];
            if (item != null && this.selectedPapers.findIndex(x => x.paperId == this.SelectedPaperIds[i]) == -1) {
                this.selectedPapers.push(item);
            }
        }
    }
    public ClearSelected() {
        for (var i = 0; i < this._magBrowserService.MAGList.papers.length; i++) {
           this._magBrowserService.MAGList.papers[i].isSelected = false;
        }
        this.SelectedPaperIds = [];
        this.selectedPapers = [];
    }
    private RemovePaperFromSelectedList(paperId: number): any {

        let pos: number = this.SelectedPaperIds.indexOf(paperId);
        if (pos > -1)
            this.SelectedPaperIds.splice(pos, 1);
        this.UpdateSelectedCount();
    }
    public GetParentAndChildRelatedPapers(item: MagFieldOfStudy) {

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

        let len: number = this.SelectedPaperIds.length;
        if (len > 0) {
            return false;
        } else {
            return true;
        }

    }
    private IsInSelectedList(paperId: number): boolean {

        if (this.SelectedPaperIds.indexOf(paperId) > -1)
            return true;
        else
            return false;
    }
    private UpdateSelectedCount(): any {
        
        this.ShowSelectedPapers = "Selected (" + this.SelectedPaperIds.length.toString() + ")";
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

        console.log('called clear on for Magpapers in component ');
        //this.MAGPapers = [];

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
