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

  public get currentOutcomeHasChanges(): boolean {
    return this._OutcomesService.currentOutcomeHasChanges;
  }

  private get UnchangedOutcome(): Outcome {
    return this._OutcomesService.UnchangedOutcome;
  }
  private set UnchangedOutcome(val: Outcome){
    this._OutcomesService.UnchangedOutcome = val;
  }

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
      await this._OutcomesService.FetchReviewSetOutcomeList(this.CurrentItemSet.itemSetId, 0);
      await this._OutcomesService.FetchReviewSetInterventionList(this.CurrentItemSet.itemSetId, 0);
      await this._OutcomesService.FetchReviewSetControlList(this.CurrentItemSet.itemSetId, 0);
      //this.GetItemArmList();
    }
  }

  public get OutcomesBySet() {
    if (!this.CurrentItemSet) return [];
    else {
      return this.CurrentItemSet.OutcomeList;
    }
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

  public get ArmsCheckIsFailing(): boolean {
    if (!this._OutcomesService.currentOutcome || !this.item) return true;
    else {
      const outc = this._OutcomesService.currentOutcome;
      if (outc.itemArmIdGrp1 > 0) {
        const index = this.item.arms.findIndex(f => f.itemArmId == outc.itemArmIdGrp1);
        if (index == -1) return true;
      }
      if (outc.itemArmIdGrp2 > 0) {
        const index = this.item.arms.findIndex(f => f.itemArmId == outc.itemArmIdGrp2);
        if (index == -1) return true;
      }
    }
    return false;
  }

  public get TimepointsCheckIsFailing(): boolean {
    if (!this._OutcomesService.currentOutcome || !this.item) return true;
    else {
      const outc = this._OutcomesService.currentOutcome;
      if (outc.itemTimepointId > 0) {
        const index = this.item.timepoints.findIndex(f => f.itemTimepointId == outc.itemTimepointId);
        if (index == -1) return true;
      }
    }
    return false;
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
          this._OutcomesService.UnchangedOutcome = new Outcome(Result);
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
            this._OutcomesService.UnchangedOutcome = new Outcome(Result);
            this.CurrentItemSet.OutcomeList.splice(key, 1, Result);
          }
        }
      }
    } 
    this.ShowOutcomesList = true;
  }
  
  //public IsThisArmSelected(armId: number, slotNumber: number): boolean {
  //  if (slotNumber == 1) {
  //    console.log("IsThisArmSelected1", armId, slotNumber, this._OutcomesService.currentOutcome.itemArmIdGrp1);
  //    if (armId == this._OutcomesService.currentOutcome.itemArmIdGrp2) return true;
  //  }
  //  else if (slotNumber == 2) {
  //    console.log("IsThisArmSelected2", armId, slotNumber, this._OutcomesService.currentOutcome.itemArmIdGrp2);
  //    if (armId == this._OutcomesService.currentOutcome.itemArmIdGrp2) return true;
  //  }
  //  console.log("IsThisArmSelected - false!");
  //  return false;
  //}
  //== _OutcomesService.currentOutcome.itemArmIdGrp1
  public CopyOutcome(toCopy: Outcome) {
    this._OutcomesService.currentOutcome = new Outcome(toCopy);
    this._OutcomesService.currentOutcome.outcomeId = 0;
    if (this._OutcomesService.currentOutcome.title.endsWith("(copy)")) {
      this._OutcomesService.currentOutcome.title = this._OutcomesService.currentOutcome.title.substring(0, this._OutcomesService.currentOutcome.title.length - 6) + "(copy2)";
    }
    else if (this._OutcomesService.currentOutcome.title.endsWith("(copy2)")) {
      this._OutcomesService.currentOutcome.title = this._OutcomesService.currentOutcome.title.substring(0, this._OutcomesService.currentOutcome.title.length - 7) + "(copy3)";
    }
    else if (this._OutcomesService.currentOutcome.title.endsWith("(copy3)")) {
      this._OutcomesService.currentOutcome.title = this._OutcomesService.currentOutcome.title.substring(0, this._OutcomesService.currentOutcome.title.length - 7) + "(copy4)";
    }
    else if (this._OutcomesService.currentOutcome.title.endsWith("(copy4)")) {
      this._OutcomesService.currentOutcome.title = this._OutcomesService.currentOutcome.title.substring(0, this._OutcomesService.currentOutcome.title.length - 7) + "(copy5)";
    }
    else if (this._OutcomesService.currentOutcome.title.endsWith("(copy5)")) {
      this._OutcomesService.currentOutcome.title = this._OutcomesService.currentOutcome.title.substring(0, this._OutcomesService.currentOutcome.title.length - 7) + "(copy6)";
    }
    else if (this._OutcomesService.currentOutcome.title.endsWith("(copy6)")) {
      this._OutcomesService.currentOutcome.title = this._OutcomesService.currentOutcome.title.substring(0, this._OutcomesService.currentOutcome.title.length - 7) + "(copy7)";
    }
    else if (this._OutcomesService.currentOutcome.title.endsWith("(copy7)")) {
      this._OutcomesService.currentOutcome.title = this._OutcomesService.currentOutcome.title.substring(0, this._OutcomesService.currentOutcome.title.length - 7) + "(copy8)";
    }
    else if (this._OutcomesService.currentOutcome.title.endsWith("(copy8)")) {
      this._OutcomesService.currentOutcome.title = this._OutcomesService.currentOutcome.title.substring(0, this._OutcomesService.currentOutcome.title.length - 7) + "(copy9)";
    }
    else if (this._OutcomesService.currentOutcome.title.endsWith("(copy9)")) {
      this._OutcomesService.currentOutcome.title = this._OutcomesService.currentOutcome.title.substring(0, this._OutcomesService.currentOutcome.title.length - 7) + "(copy10)";
    }
    else {
      this._OutcomesService.currentOutcome.title += " (copy)";
    }
    this.UnchangedOutcome = new Outcome();//outcome we're editing is NOT saved to DB, so we want it to be always different from the "unchanged" one, making it always salveable
    this._OutcomesService.currentOutcome.SetCalculatedValues();
    this.ShowOutcomesList = false;
  }


  CreateNewOutcome() {

    this._OutcomesService.currentOutcome = new Outcome();
    this.UnchangedOutcome = new Outcome();
    //console.log(this._OutcomesService.currentOutcome);
    this._OutcomesService.currentOutcome.SetCalculatedValues();
    this.ShowOutcomesList = false;
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


  ngOnDestroy() {
  }
  ngAfterViewInit() {

  }
}
