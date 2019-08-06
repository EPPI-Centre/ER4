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
import { ItemCodingService, Outcome, OutcomeItemList, ItemSet, OutcomeType } from '../services/ItemCoding.service';



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
	@Input() item: Item | undefined;
	public OutcomeTypeList: OutcomeType[] = [];

	ngOnInit() {

		alert('the item being passed in is: ' + JSON.stringify(this.item));
		console.log('============initiating outcome component');
		this.OutcomeTypeList = [
			{ "outcomeTypeId": 0, "outcomeTypeName": "Continuous: d (Hedges g)" },
			{ "outcomeTypeId": 1, "outcomeTypeName": "Continuous: r" },
			{ "outcomeTypeId": 2, "outcomeTypeName": "Binary: odds ratio" },
			{ "outcomeTypeId": 3, "outcomeTypeName": "Binary: risk ratio" },
			{ "outcomeTypeId": 4, "outcomeTypeName": "Binary: risk difference" },
			{ "outcomeTypeId": 5, "outcomeTypeName": "Binary: diagnostic test OR" },
			{ "outcomeTypeId": 6, "outcomeTypeName": "Binary: Peto OR" },
			{ "outcomeTypeId": 7, "outcomeTypeName": "Continuous: mean difference" }
		];

		this.outcomeItemList.outcomesList = this._OutcomesService.outcomesList;
		for (var i = 0; i < this._OutcomesService.outcomesList.length; i++) {
			//console.log('==============================outcome title etc');
			//console.log(this._OutcomesService.outcomesList[i].title + '\n');
			//console.log(this._OutcomesService.outcomesList[i].outcomeTypeName + '\n');
			//console.log(this._OutcomesService.outcomesList[i].outcomeDescription + '\n');
		}
		this.currentOutcome = this.outcomeItemList.outcomesList[0];
		var testTimePoint = <iTimePoint>{};

		if (this.item) {
			testTimePoint.itemId = this.item.itemId;
		}
		testTimePoint.itemTimepointId = this.currentOutcome.itemTimepointId;
		testTimePoint.timepointMetric = this.currentOutcome.itemTimepointMetric;
		testTimePoint.timepointValue = this.currentOutcome.itemTimepointValue;
		this, this.currentOutcome.outcomeTimePoint = testTimePoint;
		console.log('This is the current outcome timepoint is: ');
		console.log(this.currentOutcome.outcomeTimePoint);
		console.log('=====Finished initiating outcome component');

		this.ItemSetId = this._OutcomesService.ItemSetId;
		if (this.ItemSetId != 0) {

			this.GetReviewSetOutcomeList(this.ItemSetId);
			this.GetReviewSetInterventionList(this.ItemSetId);
			this.GetReviewSetControlList(this.ItemSetId);
			this.GetItemArmList();
		}
	}
	public GetReviewSetOutcomeList(ItemSetId: number ) {

		this._OutcomesService.FetchReviewSetOutcomeList(ItemSetId, 0);

	}
	public GetReviewSetInterventionList(ItemSetId: number ) {

		this._OutcomesService.FetchReviewSetInterventionList(ItemSetId, 0);

	}
	public GetReviewSetControlList(ItemSetId: number ) {

		this._OutcomesService.FetchReviewSetControlList(ItemSetId, 0);

	}
	public GetItemArmList() {

		if (this.item) {
			this._OutcomesService.FetchItemArmList(this.item.itemId);
		}

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
	public currentOutcome: Outcome = new Outcome();
	public outcomeDescription: string = '';
	public outcomeDescriptionModel: string = '';
	public interventionDD: string = '';
	public interventionDDModel: string = '';
	public controlDD: string = '';
	public controlDDModel: string = '';
	public outcomeDD: string = '';
	public outcomeDDModel: string = '';
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
		this.currentOutcome = outcome;
		console.log('la la and po: ' + JSON.stringify(this.currentOutcome));
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
	public selected: string = '';
	public outcome: string = '';
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
	public outcomeType: string = '';
	public control: any;
	public Intervention: any;
	public Timepoint!: iTimePoint;
	public armOne!: iArm;
	public armTwo!: iArm;
	public setGroupTwoArm(arm: iArm) {
		this.armTwo = arm;
	}
	public setGroupOneArm(arm: iArm) {
		this.armOne = arm;
	}
	public setOutcomeType(outcomeType: string) {

		this.outcomeType = outcomeType;
	}
	public setControl(control: any) {

		this.control = control;
	}
	public setIntervention(Intervention: any) {

		this.Intervention = Intervention;
	}
	public setTimepoint(Timepoint: iTimePoint) {

		this.Timepoint = Timepoint;
	}
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
	public SaveOutcome() {

		console.log('title' + this.title);
		console.log('GroupOneArmModel' + JSON.stringify(this.GroupOneArmModel));
		console.log('GroupTwoArmModel' + JSON.stringify(this.GroupTwoArmModel));
		console.log('controlDDModel' + JSON.stringify(this.controlDDModel));
		console.log('interventionDDModel' + JSON.stringify(this.interventionDDModel));
		console.log('outcomeDDModel' + JSON.stringify(this.outcomeDDModel));
		console.log('Timepoint' + JSON.stringify(this.timePointModel));
		console.log('outcomeTypeModel' + JSON.stringify(this.outcomeTypeModel));

		console.log('group2N' + this.group2N);
		console.log('group1N' + this.group1N);
		console.log('group1Mean' + this.group1Mean);
		console.log('group2Mean' + this.group2Mean);
		console.log('group1SD' + this.group1SD);
		console.log('group2SD' + this.group2SD);

		alert('not implemented yet but useful values printed in console.log');
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
	ClearAndCancelSave() {

		this.ShowOutcomesStatistics = false;
		this.ShowOutcomesList = true;
		// need to clear fields
	}
	ClearAndCancelEdit() {

		// !!!!!!!!!Need to clear what is relevant here...
		this.ShowOutcomesList = false;

	}
	Clear() {
		
	}
}
