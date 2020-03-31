import { Component, OnInit } from '@angular/core';
import { searchService } from '../services/search.service';
import { MagRelatedPapersRun, MagRelatedPaperListSelectionCriteria, BasicMAGService } from '../services/BasicMAG.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { MAGAdvancedService, MVCMagPaperListSelectionCriteria, MagPaper, MvcMagFieldOfStudyListSelectionCriteria, MagFieldOfStudy } from '../services/magAdvanced.service';
import { MAGListService } from '../services/MagList.service';
import { Subscription } from 'rxjs';
import { EventEmitterService } from '../services/EventEmitter.service';


@Component({
    selector: 'MAGBrowser',
    templateUrl: './MAGBrowser.component.html',
    providers: []
})

export class MAGBrowser implements OnInit {

    constructor(private ConfirmationDialogService: ConfirmationDialogService,
        private _magAdvancedService: MAGAdvancedService,
        private _magListService: MAGListService,
        private _eventEmitterService: EventEmitterService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _mAGListService: MAGListService,
        private router: Router

    ) {

    }

    ngOnInit() {

        console.log('paperIds are: ',this._magListService.ListCriteria.paperIds);

        //if (true) {

        //}
        //this._magAdvancedService.FetchMagFieldOfStudyList(this._magListService.ListCriteria.paperIds);



        // need a better check for list type later
        //the basic page check first 
        if (this._magListService.MAGList.papers && this._magListService.MAGList.papers.length > 0) {
            
            // do something change conditionbal here

        } else if (this._magAdvancedService.ReviewMatchedPapersList.length > 0) {

            //let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            //crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            //crit.listType = 'ReviewMatchedPapers';
            //crit.included = 'Included';
            //crit.pageSize = 20;
            console.log(this._magAdvancedService.CurrentCriteria);
            this._magAdvancedService.FetchMagPaperList(this._magAdvancedService.CurrentCriteria);
            this._mAGListService.MAGList.papers = this._magAdvancedService.ReviewMatchedPapersList;

            //conditional below is wrong   100000 =====================================================
        } else if (this._magListService.MAGList.papers.length > 100000) {

            let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit.listType = 'CitationsList';
            this._magAdvancedService.FetchMagPaperList(this._magAdvancedService.CurrentCriteria);

        } else if (this._magListService.MAGList.papers.length > 100000) {
            let crit1: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();

            crit1.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit1.listType = 'CitedByList';
            this._magAdvancedService.FetchMagPaperList(crit1);
            //all wrong
        } else if (this._magListService.MAGList.papers.length > 100000) {

            let crit2: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            crit2.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit2.listType = 'Recommendations';
            this._magAdvancedService.FetchMagPaperList(crit2);
        }
    }

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
    private RemoveFromSelectedList(paperId: number): any {

        let pos: number = this.SelectedPaperIds.indexOf(paperId);
        if (pos > -1)
            this.SelectedPaperIds.splice(pos, 1);
        this.UpdateSelectedCount();
    }
    private _maxFieldOfStudyPaperCount : number = 1000000;
    public WPParentTopics: MagFieldOfStudy[] = [];
    public WPChildTopics: MagFieldOfStudy[] = [];
    public GetParentAndChildRelatedPapers(item: MagFieldOfStudy) {

        let FieldOfStudyId: number = item.fieldOfStudyId;
        //let FieldOfStudy: string = '';

        this.GetParentAndChildFieldsOfStudy("FieldOfStudyParentsList", FieldOfStudyId, "Parent topics").then(
            () => {
                this.GetParentAndChildFieldsOfStudy("FieldOfStudyChildrenList", FieldOfStudyId, "Child topics").then(
                    () => {
                        this.GetPaperListForTopic(FieldOfStudyId);
                    });
            });
    }
    GetPaperListForTopic(FieldOfStudyId: number): any {

        let selectionCriteria: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
        selectionCriteria.pageSize = 20;
        selectionCriteria.pageNumber = 0;
        selectionCriteria.listType = "PaperFieldsOfStudyList";
        selectionCriteria.fieldOfStudyId = FieldOfStudyId;
        
        this._mAGListService.FetchWithCrit(selectionCriteria, "PaperFieldsOfStudyList");

    }
    GetParentAndChildFieldsOfStudy(FieldOfStudy: string, FieldOfStudyId: number, ParentOrChild: string): Promise<void> {

        let selectionCriteria: MvcMagFieldOfStudyListSelectionCriteria = new MvcMagFieldOfStudyListSelectionCriteria();
        selectionCriteria.listType = FieldOfStudy;
        selectionCriteria.fieldOfStudyId = FieldOfStudyId;
        return this._magListService.FetchMagFieldOfStudyList(selectionCriteria).then(

            (result: MagFieldOfStudy[] | void) => {

                if (result != null) {
                    for (var i = 0; i < result.length; i++) {

                        let newHl: MagFieldOfStudy = result[i];
                        //if (result[i].paperCount > _maxFieldOfStudyPaperCount) {
                        //        newHl.NavigateUri = new Uri("https://academic.microsoft.com/topic/" +
                        //            result[i].fieldOfStudyId.ToString());
                       
                        //}
                        //else {
                        //        newHl.Click += HlNavigateToTopic_Click;
                        //}
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
            //change to notification
            console.log("This paper is already in your review");
        }

        //NEED TO UPDATE PAPER TO API AT SMART POINT MAYNE HERE===================================****
        //this._magAdvancedService.UpdateCurrentPaper(paper.paperId);

    }


    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public GetItems(item: MagRelatedPapersRun) {

        let selectionCriteria: MagRelatedPaperListSelectionCriteria = new MagRelatedPaperListSelectionCriteria();

        selectionCriteria.pageSize = 20;

        selectionCriteria.pageNumber = 0;

        selectionCriteria.listType = "MagRelatedPapersRunList";

        selectionCriteria.magRelatedRunId = item.magRelatedRunId;




    }
    public ImportMagSearchPapers(item: MagRelatedPapersRun) {

        //this._magService.ImportMagPapers(item);

    }
    public IsServiceBusy(): boolean {

        return false;
    }
    public Selected(): void {

    }
    Clear() {


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

        this.router.navigate(['AdvancedMAGFeatures']);

    }

    public AutoUpdateHome() {

        this.router.navigate(['BasicMAGFeatures']);
    }

    public Back() {
        this.router.navigate(['AdvancedMAGFeatures']);
    }


}
