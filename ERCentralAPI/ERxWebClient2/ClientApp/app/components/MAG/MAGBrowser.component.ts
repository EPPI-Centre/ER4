import { Component, OnInit, ViewChild} from '@angular/core';
import { searchService } from '../services/search.service';
import { MAGService, MagRelatedPapersRun, MagPaperListSelectionCriteria } from '../services/mag.service';
import { singleNode, SetAttribute } from '../services/ReviewSets.service';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import {  ItemListService } from '../services/ItemList.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { MAGAdvancedService, MVCMagPaperListSelectionCriteria } from '../services/magAdvanced.service';


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

        this.Clear();
       
    }

	
	public desc: string = '';
	public value: Date = new Date(2000, 2, 10);
	public searchAll: string = 'true';
	public magDate: string = 'true';
	public magSearchCheck: boolean = false;
	public magDateRadio: string = 'true';
    public magRCTRadio: string = 'NoFilter';
	public magMode: string = '';

    public onTabSelect(e: any) {

        if (e.index == 0) {
            //refactor again below once working criteria defined twice
            //this._magAdvancedService.FetchMagFieldOfStudyList(this._magAdvancedService.currentMagPaper.paperId.toString());

        } else if (e.index == 1) {
            let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit.listType = 'CitationsList';
           // this._magAdvancedService.FetchMagPaperList(crit);

        } else if (e.index == 2) {
            let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit.listType = 'CitedByList';
            //this._magAdvancedService.FetchMagPaperList(crit);

        } else if (e.index == 3) {
            let crit: MVCMagPaperListSelectionCriteria = new MVCMagPaperListSelectionCriteria();
            crit.magPaperId = this._magAdvancedService.currentMagPaper.paperId;
            crit.listType = 'Recommendations';
            //this._magAdvancedService.FetchMagPaperList(crit);

        }
        console.log('testing tab: ', e.index );
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

        this.router.navigate(['MAGFeatures']);
    }

    public Back() {
        this.router.navigate(['Main']);
    }
	

}
	