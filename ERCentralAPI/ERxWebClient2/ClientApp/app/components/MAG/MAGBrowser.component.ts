import { Component, OnInit, ViewChild} from '@angular/core';
import { searchService } from '../services/search.service';
import { BasicMAGService, MagRelatedPapersRun, MagPaperListSelectionCriteria } from '../services/BasicMAG.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import {  ItemListService } from '../services/ItemList.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGAdvancedService, MVCMagPaperListSelectionCriteria, MagPaper } from '../services/magAdvanced.service';


@Component({
	selector: 'MAGBrowser',
	templateUrl: './MAGBrowser.component.html',
	providers: []
})

export class MAGBrowser implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
		private _magAdvancedService: MAGAdvancedService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _itemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private router: Router

	) {

	}

	ngOnInit() {

   
            this._magAdvancedService.FetchMagFieldOfStudyList(this._magAdvancedService.PaperIds);

            if (this._magAdvancedService.ReviewMatchedPapersList.length > 0) {

                //let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
                //crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
                //crit.listType = 'ReviewMatchedPapers';
                //crit.included = 'Included';
                //crit.pageSize = 20;
                console.log(this._magAdvancedService.CurrentCriteria);
                this._magAdvancedService.FetchMagPaperList(this._magAdvancedService.CurrentCriteria);

            } else {

                let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
                crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
                crit.listType = 'CitationsList';
                this._magAdvancedService.FetchMagPaperList(this._magAdvancedService.CurrentCriteria);
            }

            let crit1: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            crit1.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit1.listType = 'CitedByList';
            this._magAdvancedService.FetchMagPaperList(crit1);

            let crit2: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            crit2.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit2.listType = 'Recommendations';
            this._magAdvancedService.FetchMagPaperList(crit2);

    }
       
	public desc: string = '';
	public value: Date = new Date(2000, 2, 10);
	public searchAll: string = 'true';
	public magDate: string = 'true';
	public magSearchCheck: boolean = false;
	public magDateRadio: string = 'true';
    public magRCTRadio: string = 'NoFilter';
	public magMode: string = '';

    //public onTabSelect(e: any) {

    //    if (e.index == 0) {
    //        //refactor again below once working criteria defined twice
    //        this._magAdvancedService.FetchMagFieldOfStudyList(this._magAdvancedService.PaperIds);

    //    } else if (e.index == 1) {

    //        if (this._magAdvancedService.ReviewMatchedPapersList.length > 0) {

    //            //let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //            //crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
    //            //crit.listType = 'ReviewMatchedPapers';
    //            //crit.included = 'Included';
    //            //crit.pageSize = 20;
    //            console.log(this._magAdvancedService.CurrentCriteria);
    //            this._magAdvancedService.FetchMagPaperList(this._magAdvancedService.CurrentCriteria);

    //        } else {

    //            let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //            crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
    //            crit.listType = 'CitationsList';
    //            this._magAdvancedService.FetchMagPaperList(this._magAdvancedService.CurrentCriteria);
    //        }
            

    //    } else if (e.index == 2) {
    //        let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //        crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
    //        crit.listType = 'CitedByList';
    //        this._magAdvancedService.FetchMagPaperList(crit);

    //    } else if (e.index == 3) {
    //        let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
    //        crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
    //        crit.listType = 'Recommendations';
    //        this._magAdvancedService.FetchMagPaperList(crit);

    //    }
    //    console.log('testing tab: ', e.index );
    //}

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
        // this._magAdvancedService.UpdateCurrentPaper();

    }


    public get HasWriteRights(): boolean {
        return this._ReviewerIdentityServ.HasWriteRights;
    }
    public GetItems(item: MagRelatedPapersRun) {
                
        let selectionCriteria: MagPaperListSelectionCriteria = new MagPaperListSelectionCriteria();

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
    public CanDeleteMAGRun() : boolean {

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
	