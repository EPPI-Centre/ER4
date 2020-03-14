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


@Component({
	selector: 'MAGBrowser',
	templateUrl: './MAGBrowser.component.html',
	providers: []
})

export class MAGBrowser implements OnInit {

	constructor(private ConfirmationDialogService: ConfirmationDialogService,
		private _magService: MAGService,
        public _searchService: searchService,
        private _ReviewerIdentityServ: ReviewerIdentityService,
        private _itemListService: ItemListService,
        private _eventEmitter: EventEmitterService,
        private router: Router

	) {

	}

	ngOnInit() {

        this.Clear();
        this._magService.Fetch();

    }

	
	public desc: string = '';
	public value: Date = new Date(2000, 2, 10);
	public searchAll: string = 'true';
	public magDate: string = 'true';
	public magSearchCheck: boolean = false;
	public magDateRadio: string = 'true';
    public magRCTRadio: string = 'NoFilter';
	public magMode: string = '';


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

        this._magService.ImportMagPapers(item);

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


	

}
	