import { Component, Inject, OnInit, OnDestroy, ViewChild, Input } from '@angular/core';
import { Router } from '@angular/router';
import { ClassifierService } from '../services/classifier.service';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute } from '../services/ReviewSets.service';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { InfoBoxModalContent } from '../CodesetTrees/codesetTreeCoding.component';
import { OutcomesService } from '../services/outcomes.service';
import { Item, iTimePoint, iArm } from '../services/ItemList.service';
import { ItemCodingService, Outcome, OutcomeItemList, ItemSet } from '../services/ItemCoding.service';



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
	//@Input() itemSet: ItemSet = new ItemSet();
	private ItemSetId: number = 0;
	public ShowOutcomesStatistics: boolean = false;
	public ShowOutcomesList: boolean = true;
	public outcomeItemList: OutcomeItemList = new OutcomeItemList();
	@Input() item!: Item | undefined;
	public OutcomeTypeList: string[] = [];

	ngOnInit() {

		console.log('============initiating outcome component');
		this.OutcomeTypeList[0] = "Continuous: d (Hedges g)";
		this.OutcomeTypeList[1] = "Continuous: r";
		this.OutcomeTypeList[2] = "Binary: odds ratio";
		this.OutcomeTypeList[3] = "Binary: risk ratio";
		this.OutcomeTypeList[4] = "Binary: risk difference";
		this.OutcomeTypeList[5] = "Binary: diagnostic test OR";
		this.OutcomeTypeList[6] = "Binary: Peto OR";
		this.OutcomeTypeList[7] = "Continuous: mean difference";


		this.GetReviewSetOutcomeList();

		this.outcomeItemList.outcomesList = this._OutcomesService.outcomesList;
		for (var i = 0; i < this._OutcomesService.outcomesList.length; i++) {
			console.log('==============================');
			console.log(this._OutcomesService.outcomesList[i].outcomeDescription + '\n');
			console.log(this._OutcomesService.outcomesList[i].outcomeText + '\n');
			console.log(this._OutcomesService.outcomesList[i].outcomeTypeName + '\n');
		}
		console.log('=====Finished initiating outcome component');
			
	}
	public GetReviewSetOutcomeList() {

		this._OutcomesService.FetchReviewSetOutcomeList(3514232, 0);

	}
	public get timePointsList(): iTimePoint[] {

		if (!this.item || !this.item.timepoints) {
			//console.log('empty:!!! ');
			return [];
		}
		else {
			//console.log('timepoints: ' + this.item.timepoints.length);
			return this.item.timepoints;
		}
	}
	public get armsList(): iArm[] {

		if (!this.item || !this.item.arms) return [];
		else return this.item.arms;
	}
	
	public outcomeTypeModel: string = '';
	public GroupOneArmModel: string = '';
	public GroupOneArm: string = '';
	public GroupTwoArmModel: string = '';
	public GroupTwoArm: string = '';
	public timePoint: string = '';
	public timePointModel: string = '';
	public title: string = '';
	public titleModel: string = '';
	public group1N: string = '0';
	public group1NModel: string = '';
	public group2N: string = '0';
	public group2NModel: string = '';
	public group1Mean: string = '0';
	public group1MeanModel: string = '';
	public group2Mean: string = '0';
	public group2MeanModel: string = '';
	public group1SD: string = '0';
	public group1SDModel: string = '';
	public group2SD: string = '0';
	public group2SDModel: string = '';

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
	public setOutcome(outcome: Outcome, key: number) {

		this.ShowOutcomesStatistics = true;
		this.ShowOutcomesList = false;


	}
	removeWarning(outcome: Outcome) {

		//var outcome = this.outcomeItemList.outcomesList[key];

		//alert(JSON.stringify(outcome));
		this._OutcomesService.DeleteOutcome(outcome.outcomeId, outcome.itemSetId);
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


    ngAfterViewInit() {

	}
	CreateNewOutcome() {


	}
	ClearAndCancelEdit() {


	}
	Clear() {
		
	}

}
