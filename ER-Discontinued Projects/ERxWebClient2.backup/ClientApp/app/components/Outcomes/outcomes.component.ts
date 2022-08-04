import { Component,  OnInit, OnDestroy, Input, AfterViewInit } from '@angular/core';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { OutcomesService, OutcomeType, Outcome } from '../services/outcomes.service';
import { Item } from '../services/ItemList.service';
import { iTimePoint } from '../services/ArmTimepointLinkList.service';
import { iArm } from '../services/ArmTimepointLinkList.service';


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
	// Correction for unit of analysis error
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
		outcomeTimePoint.itemTimepointId = this._OutcomesService.currentOutcome.itemTimepointId;
		outcomeTimePoint.timepointMetric = this._OutcomesService.currentOutcome.itemTimepointMetric;
		outcomeTimePoint.timepointValue = this._OutcomesService.currentOutcome.itemTimepointValue;
		this._OutcomesService.currentOutcome.outcomeTimePoint = outcomeTimePoint;

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
	public ShowCFUOAEBoolCheck() : boolean {
		if (this._OutcomesService.currentOutcome.data9 > 0 ||
            this._OutcomesService.currentOutcome.data10 > 0) {
            this.ShowCFUOAEBool = true;
			return true;
		} else {
            return this.ShowCFUOAEBool;
		}
    }
    public get ShowCFUOAEtext(): string {
        if (this.ShowCFUOAEBoolCheck()) return "Stop correcting for unit of analysis error (resets current values)";
        else return "Correct for unit of analysis error";
    }
	public ShowCFUOAE() {
        if (this._OutcomesService.currentOutcome.data9 > 0 ||
            this._OutcomesService.currentOutcome.data10 > 0) {
            //we are currently correcting for unit of analysis...
            //we'll wipe the data and set the backing field to false
            this._OutcomesService.currentOutcome.data9 = 0;
            this._OutcomesService.currentOutcome.data10 = 0;
            this.ShowCFUOAEBool = false;
        }
        else {
            //we currently are not "correcting", or we are, but no values have been entered, so flip backing field
            this.ShowCFUOAEBool = !this.ShowCFUOAEBool;
        }
		//currentOutcome.isSelected = !currentOutcome.isSelected;
		//this.ShowCFUOAEBool = !this.ShowCFUOAEBool;

	}
	public get SMD(): string {

		return this._OutcomesService.currentOutcome.smd.toFixed(15);
	}
	public get SEES(): string {

		return this._OutcomesService.currentOutcome.sees.toFixed(15);
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

		if (this._OutcomesService.currentOutcome.esDesc == 'Effect size') {
			return this._OutcomesService.currentOutcome.es;
		}
		if (this._OutcomesService.currentOutcome.esDesc == 'SMD') {
			return this._OutcomesService.currentOutcome.smd;
		}
		if (this._OutcomesService.currentOutcome.esDesc == 'Diagnostic OR') {
			return this._OutcomesService.currentOutcome.petoOR;
		}
		if (this._OutcomesService.currentOutcome.esDesc == 'r') {

			return this._OutcomesService.currentOutcome.r;
		}

		return this._calculatedEffectSize;
	}
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
	
	public editOutcome(outcome: Outcome) {

		if (outcome != null) {
			this.ShowOutcomesStatistics = true;
			this.ShowOutcomesList = false;
			this._OutcomesService.currentOutcome = outcome;
            this._OutcomesService.ItemSetId = outcome.itemSetId;
            if (this._OutcomesService.currentOutcome.data9 > 0 ||
                this._OutcomesService.currentOutcome.data10 > 0) this.ShowCFUOAEBool = true;
            else this.ShowCFUOAEBool = false;
		}
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

		if (Timepoint != null) {
			this.Timepoint = Timepoint;
		}
	}
	public SaveOutcome() {

		if (this._OutcomesService.currentOutcome &&
			this.ItemSetId != 0) {
			if (this._OutcomesService.currentOutcome.outcomeId == 0 ) {
				this._OutcomesService.currentOutcome.itemSetId = this.ItemSetId;
				//console.log('Just before creating outcome we have: ', this._OutcomesService.currentOutcome.outcomeCodes);
				this._OutcomesService.Createoutcome(this._OutcomesService.currentOutcome).then(
					() => {
						console.log(JSON.stringify(this._OutcomesService.currentOutcome));

							this._OutcomesService.FetchOutcomes(this._OutcomesService.currentOutcome.itemSetId);
						}
					);
			} else {
				console.log(JSON.stringify(this._OutcomesService.currentOutcome));
				this._OutcomesService.Updateoutcome(this._OutcomesService.currentOutcome);
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

		this._OutcomesService.currentOutcome = new Outcome();
		//console.log(this._OutcomesService.currentOutcome);
		this.ShowOutcomesStatistics = true;
		this.ShowOutcomesList = false;
		this._OutcomesService.currentOutcome.SetCalculatedValues();
	}
	ClearAndCancelSave() {
		this._OutcomesService.FetchOutcomes(this.ItemSetId);
		this.ShowOutcomesStatistics = false;
		this.ShowOutcomesList = true;
	}
	ClearAndCancelEdit() {
		this.ShowOutcomesList = false;
		this._OutcomesService.outcomesChangedEE.emit();
	}
}
