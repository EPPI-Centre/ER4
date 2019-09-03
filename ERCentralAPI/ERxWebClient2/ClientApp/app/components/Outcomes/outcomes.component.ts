import { Component,  OnInit, OnDestroy, Input, AfterViewInit, EventEmitter, Output } from '@angular/core';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { OutcomesService, OutcomeType, Outcome } from '../services/outcomes.service';
import { Item } from '../services/ItemList.service';
import { iTimePoint } from '../services/timePoints.service';
import { iArm } from '../services/arms.service';
import { ItemSet } from '../services/ItemCoding.service';


@Component({
    selector: 'OutcomesComp',
    templateUrl: './outcomes.component.html',
    providers: []
})

export class OutcomesComponent implements OnInit, OnDestroy, AfterViewInit {
    DataSource: any;
    constructor(
		public _reviewSetsService: ReviewSetsService,
		public _eventEmitterService: EventEmitterService,
		private _ReviewerIdentityServ: ReviewerIdentityService,
		private _OutcomesService: OutcomesService
	) { }


	private ItemSetId: number = 0;
	public ShowOutcomesStatistics: boolean = false;
	public ShowOutcomesList: boolean = true;
	@Input() item: Item | undefined;
	public ShowCFUOAEBool: boolean = false;
	public OutcomeTypeList: OutcomeType[] = [];

	ngOnInit() {
		
		this.OutcomeTypeList = [
			{ "outcomeTypeId": 0, "outcomeTypeName": "Manual entry" },
			{ "outcomeTypeId": 1, "outcomeTypeName": "Continuous: Ns, means, and SD" },
			{ "outcomeTypeId": 2, "outcomeTypeName": "Binary: 2 x 2 table" },
			{ "outcomeTypeId": 3, "outcomeTypeName": "Continuous: N, Mean, and SE" },
			{ "outcomeTypeId": 4, "outcomeTypeName": "Continuous: N, Mean, and CI" },
			{ "outcomeTypeId": 5, "outcomeTypeName": "Continuous: N, t- or p-value" },
			{ "outcomeTypeId": 6, "outcomeTypeName": "Diagnostic test: 2 x 2 table" },
			{ "outcomeTypeId": 7, "outcomeTypeName": "Correlation coefficient r" }
		];

		var outcomeTimePoint = <iTimePoint>{};
		if (this.item) {
			outcomeTimePoint.itemId = this.item.itemId;
		}
		outcomeTimePoint.itemTimepointId = this.currentOutcome.itemTimepointId;
		outcomeTimePoint.timepointMetric = this.currentOutcome.itemTimepointMetric;
		outcomeTimePoint.timepointValue = this.currentOutcome.itemTimepointValue;
		this.currentOutcome.outcomeTimePoint = outcomeTimePoint;
		this._OutcomesService.currentOutcome = this.currentOutcome;
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
	public ShowCFUOAE(){

		this.ShowCFUOAEBool = !this.ShowCFUOAEBool;
	}
	public get SMD(): string {

		return this.currentOutcome.smd.toFixed(15);
	}
	public get SEES(): string {

		return this.currentOutcome.sees.toFixed(15);
	}
	public get timePointsList(): iTimePoint[] {

		if (!this.item || !this.item.timepoints) {
			return [];
		}
		else {
			return this.item.timepoints;
		}
	}
	private _calculatedEffectSize: number = 0;

	public CalculatedEffectSize(): number {

		if (this.currentOutcome.esDesc == 'Effect size') {
			return this.currentOutcome.es;
		}
		if (this.currentOutcome.esDesc == 'SMD') {
			return this.currentOutcome.smd;
		}
		if (this.currentOutcome.esDesc == 'Diagnostic OR') {
			return this.currentOutcome.petoOR;
		}
		if (this.currentOutcome.esDesc == 'r') {

			return this.currentOutcome.r;
		}

		return this._calculatedEffectSize;

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

	public get HasWriteRights(): boolean {
		return this._ReviewerIdentityServ.HasWriteRights;
	}
	public printstuff() {

		var ans = this.currentOutcome;//.OutcomeCodes.outcomeItemAttributesList;
		console.log(JSON.stringify(ans));
	}
	public editOutcome(outcome: Outcome, key: number) {

		this.ShowOutcomesStatistics = true;
		this.ShowOutcomesList = false;
		this.currentOutcome = outcome;
		this._OutcomesService.ItemSetId = outcome.itemSetId;
		this._OutcomesService.ItemSetChanged.emit(outcome.itemSetId);
		console.log('emitting the following number: ' + outcome.itemSetId);
	}
	removeWarning(outcome: Outcome, key: number) {

		if (outcome != null) {
			this._OutcomesService.DeleteOutcome(outcome.outcomeId, outcome.itemSetId, key);
			
		}
	}
	public selected: string = '';
	public outcome: string = '';
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
	public SaveOutcome() {

		if (this.currentOutcome &&
			this.ItemSetId != 0) {
			if (this.currentOutcome.outcomeId == 0 ) {
				this.currentOutcome.itemSetId = this.ItemSetId;
				this._OutcomesService.Createoutcome(this.currentOutcome).then(
					() => {
						this._OutcomesService.outcomesList;
						}
					);
			} else {
				this._OutcomesService.Updateoutcome(this.currentOutcome);
			}
		}
		this.ShowOutcomesStatistics = false;
		this.ShowOutcomesList = true;
	}
	ngOnDestroy() {

		this._reviewSetsService.selectedNode = null;
	}
    ngAfterViewInit() {

	}
	CreateNewOutcome() {

		this.currentOutcome = new Outcome();
		this.ShowOutcomesStatistics = true;
		this.ShowOutcomesList = false;
		this.currentOutcome.SetCalculatedValues();
	}
	ClearAndCancelSave() {
		this.ShowOutcomesStatistics = false;
		this.ShowOutcomesList = true;
	}
	ClearAndCancelEdit() {
		this.ShowOutcomesList = false;
		this._OutcomesService.outcomesChangedEE.emit();
	}
	Clear() {

	}
}
