import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { searchService } from '../services/search.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router, NavigationEnd } from '@angular/router';
import { MVCMagPaperListSelectionCriteria, MagPaper, MvcMagFieldOfStudyListSelectionCriteria, MagFieldOfStudy } from '../services/MAGClasses.service';
import { MAGBrowserService } from '../services/MAGBrowser.service';
import { MAGAdvancedService } from '../services/magAdvanced.service';
import { MAGBrowserHistoryService } from '../services/MAGBrowserHistory.service';


@Component({
    selector: 'MAGBrowser',
    templateUrl: './MAGBrowser.component.html',
    providers: []
})

export class MAGBrowser implements OnInit {

    constructor(
        public _magAdvancedService: MAGAdvancedService,
        public _magBrowserService: MAGBrowserService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _routingStateService: MAGBrowserHistoryService,
        private _location: Location,
        private router: Router
    ) {

    }

    public browsingHistory: NavigationEnd[] = [];
    public MAGPapers: MagPaper[] = [];
    public description: string = '';
    public SelectedPaperIds: number[] = [];
    public ShowSelectedPapers: string = '';
    public selectedPapers: MagPaper[] = [];

    ngOnInit() {

        this.browsingHistory = this._routingStateService.getHistory();
    }

    public ImportSelected() {

        alert('not implemented yet!');
    }
    public ShowMAGBrowserHistory() {
        this.router.navigate(['MAGBrowserHistory']);
    }
    public ShowAdminPage() {
        this.router.navigate(['MAGAdmin']);
    }
    public ShowAutoUpdateHome() {
        this.router.navigate(['BasicMAGFeatures']);
    }
    public Forward() {
        this._location.forward();
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
        //this is not efficient but may not matter on such a small list
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

        this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", FieldOfStudyId, "Parent topics").then(
            () => {
                this._magBrowserService.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", FieldOfStudyId, "Child topics").then(
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

            console.log("This paper is already in your review");
        }

    }
    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public get IsServiceBusy(): boolean {

        return this._magBrowserService.IsBusy || this._magAdvancedService.IsBusy;
    }
    public Clear() {

        this.MAGPapers = [];
        this._magAdvancedService.currentMagPaper = new MagPaper();

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
    public AdvancedFeatures() {
        this.router.navigate(['AdvancedMAGFeatures']);
    }
         
}
