import { Component, OnInit, OnDestroy, Input, AfterViewInit, EventEmitter, Output } from '@angular/core';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { OutcomesService, OutcomeType, Outcome } from '../services/outcomes.service';
import { Item } from '../services/ItemList.service';
import { iTimePoint } from '../services/ArmTimepointLinkList.service';
import { iArm } from '../services/ArmTimepointLinkList.service';
import { ItemCodingService, ItemSet } from '../services/ItemCoding.service';


@Component({
  selector: 'OutcomesComp',
  templateUrl: './outcomes.component.html',
  providers: []
})

export class OutcomesComponent implements OnInit, OnDestroy, AfterViewInit {
  DataSource: any;
  constructor(
    private reviewSetsService: ReviewSetsService,
    private _ReviewerIdentityServ: ReviewerIdentityService,
    private ItemCodingService: ItemCodingService,
    public _OutcomesService: OutcomesService
  ) { }

  @Output() PleaseCloseMe = new EventEmitter<void>();

  //private ItemSetId: number = 0;
  public CurrentItemSet: ItemSet | null = null;
  public ShowOutcomesList: boolean = true;
  @Input() item: Item | undefined;
  // Correction for unit of analysis error
  public ShowCFUOAEBool: boolean = false;
  public OutcomeTypeList: OutcomeType[] = [
    { "outcomeTypeId": 0, "outcomeTypeName": "Manual entry" },
    { "outcomeTypeId": 1, "outcomeTypeName": "Continuous: Ns, means, and SD" },
    { "outcomeTypeId": 2, "outcomeTypeName": "Binary: 2 x 2 table" },
    { "outcomeTypeId": 3, "outcomeTypeName": "Continuous: N, Mean, and SE" },
    { "outcomeTypeId": 4, "outcomeTypeName": "Continuous: N, Mean, and CI" },
    { "outcomeTypeId": 5, "outcomeTypeName": "Continuous: N, t- or p-value" },
    { "outcomeTypeId": 6, "outcomeTypeName": "Diagnostic test: 2 x 2 table" },
    { "outcomeTypeId": 7, "outcomeTypeName": "Correlation coefficient r" }
  ];
  private UnchangedOutcome: Outcome = new Outcome();


  async ngOnInit() {
    await this.SelfSetup();
    await this.FetchData();
  }
  private async SelfSetup() {
    this.CurrentItemSet = this.ItemCodingService.FindItemSetByItemSetId(this._OutcomesService.ItemSetId);
    var outcomeTimePoint = <iTimePoint>{};
    if (this.item) {
      outcomeTimePoint.itemId = this.item.itemId;
    }
    outcomeTimePoint.itemTimepointId = this._OutcomesService.currentOutcome.itemTimepointId;
    outcomeTimePoint.timepointMetric = this._OutcomesService.currentOutcome.itemTimepointMetric;
    outcomeTimePoint.timepointValue = this._OutcomesService.currentOutcome.itemTimepointValue;
    this._OutcomesService.currentOutcome.outcomeTimePoint = outcomeTimePoint;
  }
  private async FetchData() {
    if (this.CurrentItemSet) {
      //this.GetReviewSetOutcomeList(this.CurrentItemSet.itemSetId);
      await this.GetReviewSetInterventionList(this.CurrentItemSet.itemSetId);
      await this.GetReviewSetControlList(this.CurrentItemSet.itemSetId);
      //this.GetItemArmList();
    }
  }

  public get OutcomesBySet() {
    if (!this.CurrentItemSet) return [];
    else {
      return this.CurrentItemSet.OutcomeList;
    }
  }

  //public GetReviewSetOutcomeList(ItemSetId: number) {

  //  this._OutcomesService.FetchReviewSetOutcomeList(ItemSetId, 0);
  //}
  public GetReviewSetInterventionList(ItemSetId: number) {

    this._OutcomesService.FetchReviewSetInterventionList(ItemSetId, 0);
  }
  public GetReviewSetControlList(ItemSetId: number) {

    this._OutcomesService.FetchReviewSetControlList(ItemSetId, 0);
  }

  public ShowCFUOAEBoolCheck(): boolean {
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
  public UpdateInterventionName(event: Event) {
    if (this.CurrentItemSet && this._OutcomesService.currentOutcome) {
      let Id: number = parseInt((event.target as HTMLOptionElement).value);
      if (!isNaN(Id)) {
        const index = this._OutcomesService.ReviewSetInterventionList.findIndex(f => f.attributeId == Id);
        if (index != -1) this._OutcomesService.currentOutcome.interventionText = this._OutcomesService.ReviewSetInterventionList[index].attributeName;
      }
    }
  }
  public UpdateComparisonName(event: Event) {
    if (this.CurrentItemSet && this._OutcomesService.currentOutcome) {
      let Id: number = parseInt((event.target as HTMLOptionElement).value);
      if (!isNaN(Id)) {
        const index = this._OutcomesService.ReviewSetControlList.findIndex(f => f.attributeId == Id);
        if (index != -1) this._OutcomesService.currentOutcome.controlText = this._OutcomesService.ReviewSetControlList[index].attributeName;
      }
    }
  }
  


  public get HasWriteRights(): boolean {
    return this._ReviewerIdentityServ.HasWriteRights;
  }

  public editOutcome(outcome: Outcome) {
    if (outcome != null) {
      this.ShowOutcomesList = false;
      this.UnchangedOutcome = new Outcome(outcome);
      this._OutcomesService.currentOutcome = outcome;
      this._OutcomesService.ItemSetId = outcome.itemSetId;
      if (this._OutcomesService.currentOutcome.data9 > 0 ||
        this._OutcomesService.currentOutcome.data10 > 0) this.ShowCFUOAEBool = true;
      else this.ShowCFUOAEBool = false;
    }
  }

  async removeWarning(outcome: Outcome, key: number) {
    if (outcome != null && this.CurrentItemSet != null) {
      let res = await this._OutcomesService.DeleteOutcome(outcome.outcomeId, outcome.itemSetId, key);
      if (res == true) this.CurrentItemSet.OutcomeList.splice(key, 1);
      else (this.ItemCodingService.Fetch(this.CurrentItemSet.itemId));
    }
  }

  
  public async SaveOutcome() {
    if (this._OutcomesService.currentOutcome && this.CurrentItemSet) {
      let Result: Outcome | boolean = false;
      if (this._OutcomesService.currentOutcome.outcomeId == 0) {
        this._OutcomesService.currentOutcome.itemSetId = this.CurrentItemSet.itemSetId;
        //console.log('Just before creating outcome we have: ', this._OutcomesService.currentOutcome.outcomeCodes);
        Result = await this._OutcomesService.Createoutcome(this._OutcomesService.currentOutcome);
        if (Result == false) {//didn't work!!
        } else if (Result != true) {//true case doesn't happen
          this._OutcomesService.currentOutcome = Result;
          this.CurrentItemSet.OutcomeList.push(Result);
        }
      } else {
        //console.log(JSON.stringify(this._OutcomesService.currentOutcome));
        Result = await this._OutcomesService.Updateoutcome(this._OutcomesService.currentOutcome);
        if (Result == false) {//didn't work!!
        } else if (Result != true) {//true case doesn't happen
          let key = this.CurrentItemSet.OutcomeList.findIndex(f => f.outcomeId == this._OutcomesService.currentOutcome.outcomeId);
          if (key != -1) {
            this._OutcomesService.currentOutcome = Result;
            this.CurrentItemSet.OutcomeList.splice(key, 1, Result);
          }
        }
      }
    }
    this.ShowOutcomesList = true;
  }
  ngOnDestroy() {
  }
  ngAfterViewInit() {

  }
  CreateNewOutcome() {

    this._OutcomesService.currentOutcome = new Outcome();
    this.UnchangedOutcome = new Outcome();
    //console.log(this._OutcomesService.currentOutcome);
    this.ShowOutcomesList = false;
    this._OutcomesService.currentOutcome.SetCalculatedValues();
  }
  ClearAndCancelSave() {
    //this._OutcomesService.FetchOutcomes(this.ItemSetId);
    if (this.UnchangedOutcome.outcomeId > 0) {
      this._OutcomesService.currentOutcome = this.UnchangedOutcome;
      if (this.CurrentItemSet) {
        const index = this.CurrentItemSet.OutcomeList.findIndex(f => f.outcomeId == this.UnchangedOutcome.outcomeId)
        if (index != -1) this.CurrentItemSet.OutcomeList.splice(index, 1, this.UnchangedOutcome);
      }
    }
    this.ShowOutcomesList = true;
  }
  ClearAndCancelEdit() {
    this.ShowOutcomesList = false;
    this.PleaseCloseMe.emit();
  }
}
