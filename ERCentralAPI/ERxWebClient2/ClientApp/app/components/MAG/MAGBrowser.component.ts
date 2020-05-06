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

    history: NavigationEnd [] = [];

    constructor(
        public _magAdvancedService: MAGAdvancedService,
        public _magBrowserService: MAGBrowserService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _routingStateService: MAGBrowserHistoryService,
        private _location: Location,
        private router: Router
    ) {

        this.history = this._routingStateService.getHistory();
        console.log('testing URL: ', this.history);
    }

    public ClearSelected() {

    }
    public ImportSelected() {

    }

    public ShowHistory() {

        this.router.navigate(['MAGBrowserHistory']);
    }
    public Admin() {

    }
    public AutoUpdateHome() {
        this.router.navigate(['BasicMAGFeatures']);
    }
    public Forward() {
        this._location.forward();
    }
    public Back() {
        this._location.back();
    }
    ngOnInit() {

        this.section['first'] = true;

        if (this._magBrowserService.MAGList.papers && this._magBrowserService.MAGList.papers.length > 0) {

        } else if (this._magAdvancedService.ReviewMatchedPapersList.length > 0) {

            console.log(this._magAdvancedService.CurrentCriteria);
            this._magAdvancedService.FetchMagPaperList(this._magAdvancedService.CurrentCriteria);
            this._magBrowserService.MAGList.papers = this._magAdvancedService.ReviewMatchedPapersList;

        } else if (this._magBrowserService.MAGList.papers.length > 100000) {

            let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit.listType = 'CitationsList';
            this._magAdvancedService.FetchMagPaperList(this._magAdvancedService.CurrentCriteria);

        } else if (this._magBrowserService.MAGList.papers.length > 100000) {
            let crit1: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();

            crit1.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit1.listType = 'CitedByList';
            this._magAdvancedService.FetchMagPaperList(crit1);

        } else if (this._magBrowserService.MAGList.papers.length > 100000) {

            let crit2: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            crit2.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit2.listType = 'Recommendations';
            this._magAdvancedService.FetchMagPaperList(crit2);
        }
    }
    public Papers: MagPaper[] = [];
    public desc: string = '';
    public value: Date = new Date(2000, 2, 10);
    public searchAll: string = 'true';
    public magDate: string = 'true';
    public magSearchCheck: boolean = false;
    public magDateRadio: string = 'true';
    public magRCTRadio: string = 'NoFilter';
    public magMode: string = '';
    public SelectedPaperIds: number[] = [];
    public ShowSelectedPapers: string = '';
    private AddToSelectedList(paperId: number) {

        if (!this.IsInSelectedList(paperId)) {
            this.SelectedPaperIds.push(paperId);
            this.UpdateSelectedCount();
        }
    }
    section: any = [];
    public SetCriteria(listType: string) {

        console.log(listType);
        this._magBrowserService.ListCriteria.listType = listType;

    }
    private RemoveFromSelectedList(paperId: number): any {

        let pos: number = this.SelectedPaperIds.indexOf(paperId);
        if (pos > -1)
            this.SelectedPaperIds.splice(pos, 1);
        this.UpdateSelectedCount();
    }
    private _maxFieldOfStudyPaperCount: number = 1000000;
    public ParentTopic: string = '';
    public WPParentTopics: MagFieldOfStudy[] = [];
    public WPChildTopics: MagFieldOfStudy[] = [];
    public GetParentAndChildRelatedPapers(item: MagFieldOfStudy) {

        let FieldOfStudyId: number = item.fieldOfStudyId;
        this.ParentTopic =  item.displayName;

        this.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", FieldOfStudyId, "Parent topics").then(
            () => {
                this.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", FieldOfStudyId, "Child topics").then(
                    () => {
                        this.GetPaperListForTopic(FieldOfStudyId);
                    });
            });
    }
   
    GetPaperListForTopic(FieldOfStudyId: number): any {

        let id = this._magBrowserService.ListCriteria.magRelatedRunId;
        this._magBrowserService.ListCriteria = new MVCMagPaperListSelectionCriteria();
        this._magBrowserService.ListCriteria.magRelatedRunId = id;
        this._magBrowserService.ListCriteria.fieldOfStudyId = FieldOfStudyId;
        this._magBrowserService.ListCriteria.listType = "PaperFieldsOfStudyList";
        this._magBrowserService.ListCriteria.pageNumber = 0;
        this._magBrowserService.ListCriteria.pageSize = 20;
        this._magBrowserService.FetchWithCrit(this._magBrowserService.ListCriteria, "PaperFieldsOfStudyList");

    }
    GetParentAndChildFieldsOfStudy(FieldOfStudy: string, FieldOfStudyId: number, ParentOrChild: string): Promise<void> {

        let selectionCriteria: MvcMagFieldOfStudyListSelectionCriteria = new MvcMagFieldOfStudyListSelectionCriteria();
        selectionCriteria.listType = FieldOfStudy;
        selectionCriteria.fieldOfStudyId = FieldOfStudyId;
        return this._magBrowserService.FetchMagFieldOfStudyList(selectionCriteria).then(

            (result: MagFieldOfStudy[] | void) => {

                if (result != null) {
                    for (var i = 0; i < result.length; i++) {

                        let newHl: MagFieldOfStudy = result[i];
                          if (ParentOrChild == 'Parent topics') {
                            this.WPParentTopics.push(newHl);

                        } else {
                            this.WPChildTopics.push(newHl);
                        }
                    }
                }
            }
        );
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

                this.RemoveFromSelectedList(paper.paperId);
                paper.isSelected = false;
            }
            else {

                this.AddToSelectedList(paper.paperId);
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
    public Selected(): void {

    }
    Clear() {

        this.Papers = [];
        this._magAdvancedService.currentMagPaper = new MagPaper();
        this.magDate = '';
        this.magMode = '';

    }
    public CanDeleteMAGRun(): boolean {

        return this.HasWriteRights;
    }

    public CanAddNewMAGSearch(): boolean {

        if (this.desc != '' && this.desc != null && this.HasWriteRights
        ) {
            return true;
        } else {
            return false;
        }
    }
    public AdvancedFeatures() {
        this.Clear();
        this.router.navigate(['AdvancedMAGFeatures']);

    }
         
}
