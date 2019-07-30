import { Component, Inject, OnInit, OnDestroy, ViewChild, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ClassifierService } from '../services/classifier.service';
import { ReviewSetsService, singleNode, ReviewSet } from '../services/ReviewSets.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { InfoBoxModalContent } from '../CodesetTrees/codesetTreeCoding.component';
import { OutcomesService } from '../services/outcomes.service';
import { Item } from '../services/ItemList.service';
import { ItemCodingService, Outcome, OutcomeItemList } from '../services/ItemCoding.service';


@Component({
    selector: 'OutcomesComp',
    templateUrl: './outcomes.component.html',
    providers: []
})

export class OutcomesComponent implements OnInit, OnDestroy {
    DataSource: any;
    constructor(private router: Router,
        @Inject('BASE_URL') private _baseUrl: string,
		private _classifierService: ClassifierService,
		public _reviewSetsService: ReviewSetsService,
		public _eventEmitterService: EventEmitterService,
		private _confirmationDialogService: ConfirmationDialogService,
		private _ReviewerIdentityServ: ReviewerIdentityService,
		private _notificationService: NotificationService,
		private _OutcomesService: OutcomesService,
		private _ItemCodingService: ItemCodingService
	) { }


	//private currentItem: Item = new Item();
	@Input() item: Item = new Item();
	private ItemSetId: number = 0;
	public outcomeItemList: OutcomeItemList = new OutcomeItemList();
	ngOnInit() {

		console.log('============initiating outcome component');
		this._reviewSetsService.GetReviewSets();
		// hardcode for first draft
		//if (this._reviewSetsService.selectedNode != null) {

			var selectedNode = this._reviewSetsService.selectedNode as ReviewSet;
			//alert('selected NOde is: ' + JSON.stringify(selectedNode));

			if (selectedNode != null) {

				//alert('Please: ' + selectedNode.ItemSetId);
				var itemSet = this._ItemCodingService.FindItemSetByItemSetId(selectedNode.ItemSetId);
				if (itemSet != null) {
					this.outcomeItemList = itemSet.outcomeItemList;
					console.log('=================================');
					console.log(JSON.stringify(this.outcomeItemList.outcomesList));
					console.log('=================================');
				}
			
			//this._OutcomesService.FetchOutcomes(this.ItemSetId);
			}
		//}
	}

	public get OutcomeList(): Outcome[] {

		if (!this.ItemSetId || !this._OutcomesService.outcomesList) return [];
		else return this._OutcomesService.outcomesList;
	}
	CanOnlySelectRoots() {

		return true;

	}
	public get HasWriteRights(): boolean {
		return this._ReviewerIdentityServ.HasWriteRights;
	}

	private canDelete: boolean = false;
	
	removeWarning(outcome: Outcome) {

		//var outcome = this.outcomeItemList.outcomesList[key];

		//alert(JSON.stringify(outcome));
		this._OutcomesService.DeleteOutcome(outcome);
	}

	public sort: SortDescriptor[] = [{
		field: 'modelId',
		dir: 'desc'
	}];

	public modelsToBeDeleted: number[] = [];
	
	public checkBoxSelected: boolean = false;
	public checkboxClicked(dataItem: any) {

		if (dataItem.add == undefined || dataItem.add == null) {
			dataItem.add = true;
		} else {
			dataItem.add = !dataItem.add;
		}
	
		if (dataItem.add == true) {
			this.checkBoxSelected = true;

		} else {

		}

	};
	public allModelsSelected: boolean = false;
	public selectAllModelsChange() {

		if (this.allModelsSelected == true) {
			for (var i = 0; i < this.DataSource.data.length; i++) {
				this.DataSource.data[i].add = false;
			}
			this.allModelsSelected = false;
			return;
		} else {
			for (var i = 0; i < this.DataSource.data.length; i++) {
				this.DataSource.data[i].add = true;
			}
		}
		this.allModelsSelected = true;

	}
	public sortChange(sort: SortDescriptor[]): void {
		this.sort = sort;
		console.log('sorting?' + this.sort[0].field + " ");
	}
	BackToMain() {
		this.router.navigate(['Main']);
	}
	ngOnDestroy() {

		this._reviewSetsService.selectedNode = null;
	}
	public isCollapsed: boolean = false;
	public isCollapsed2: boolean = false;
	CloseBMDropDown1() {

		this.isCollapsed = false;
	}
	CloseBMDropDown2() {

		this.isCollapsed2 = false;
	}

    ngAfterViewInit() {

	}
	CreateNewOutcome() {


	}
	ClearAndCancelEdit() {


	}
	Clear() {
		
	}

}
