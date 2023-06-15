import { Component, OnInit,Input, ViewChild, OnDestroy, EventEmitter, Output } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { iReference, MetaAnalysis, MetaAnalysisService, Moderator } from '../services/MetaAnalysis.service';
import { FilterOutcomesFormComp } from './FilterOutcomesForm.component';
import { codesetSelectorComponent } from '../CodesetTrees/codesetSelector.component';
import { ReviewSetsService, SetAttribute } from '../services/ReviewSets.service';

@Component({
  selector: 'MAaddOutcomesColumnComp',
  templateUrl: './MAaddOutcomesColumn.component.html',
  providers: [],
  styles: [
`
.ModeratorsTableContainer {border-top: 1px solid DarkBlue; border-bottom: 1px solid DarkBlue; max-height: 40vh; overflow:auto; max-width:90vw;}
.ModeratorsTable thead th {background-color: #dff0df; box-shadow: inset 0px -0.8px #222222, 0 0 #000; }
`
  ]
})
export class MAaddOutcomesColumnComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private ReviewSetsService: ReviewSetsService,
    private MetaAnalysisService: MetaAnalysisService
  ) { }

  @ViewChild('ColCodeSelector') ColCodeSelector!: codesetSelectorComponent;
  @Output() PleaseCloseMe = new EventEmitter();
  @Output() PleaseSaveTheMA = new EventEmitter();
  @Input() CanSave: boolean = false;  

  ngOnInit() {
    if (this.ReviewSetsService.ReviewSets.length == 0) this.ReviewSetsService.GetReviewSets();
  }

  public ColumnMode: string = "Answer";//or "Question"...
  public MaxOptionalColumns: number = 20;
  public SelectionState: number = 0; // 0 means "no code"; 1 "valid selection"; 2 "column already present"; 3 "invalid selection" (question mode, but code has no children)
  public ColumnCode: SetAttribute | null = null;
  public ShowingCodes: boolean = false;

  public get CurrentMA(): MetaAnalysis | null {
    return this.MetaAnalysisService.CurrentMetaAnalysis;
  }

  public get HasMaxQuestionCols(): boolean {
    if (!this.MetaAnalysisService) return true;
    else if (this.MetaAnalysisService.ColumnVisibility.QuestionHeaders.length >= this.MaxOptionalColumns) return true;
    else return false;
  }
  public get HasMaxAnswerCols(): boolean {
    if (!this.MetaAnalysisService) return true;
    else if (this.MetaAnalysisService.ColumnVisibility.AnswerHeaders.length >= this.MaxOptionalColumns) return true;
    else return false;
  }
  public CloseCodeDropDown() {
    if (this.ColCodeSelector !== null) {
      this.ColumnCode = this.ColCodeSelector.SelectedNodeData as SetAttribute;
      this.ShowingCodes = false;
    }
    this.CheckSelectionState();
  }
  public ToggleDropDown(ColType: string) {
    if (this.ShowingCodes == false) {
      //we'll show the dropdown, some work to do...
      this.ColumnCode = null;
      this.SelectionState = 0;
      this.ShowingCodes = true;
    } else {
      this.CheckSelectionState();
      this.ShowingCodes = false;
    }
  }

  public DelayedCheckSelectionState() {
    setTimeout(() => { this.CheckSelectionState(); }, 50);
  }

  public CheckSelectionState() {
    console.log("CheckSelectionState");
    if (this.CurrentMA == null) {
      this.ColumnCode = null;
      this.SelectionState = 0;
      return;
    }
    if (this.ColumnCode == null) {
      this.SelectionState = 0;
      return;
    }
    const idSt = this.ColumnCode.attribute_id.toString();
    let currentIds: string[] = [];
    if (this.ColumnMode == "Answer") {
      currentIds = this.CurrentMA.attributeIdAnswer.split(',');
    } else {// has to be "Question"
      if (this.ColumnCode.attributes.length == 0) {
        this.SelectionState = 3;
        return;
      }
      currentIds = this.CurrentMA.attributeIdQuestion.split(',');
    }
    if (currentIds.findIndex(f => f == idSt) == -1) {
      this.SelectionState = 1;
    }
    else this.SelectionState = 2;
  }
  public Add() {
    this.CheckSelectionState();
    if (this.CurrentMA == null || this.ColumnCode == null) return;
    if (this.SelectionState == 1) {
      if (this.ColumnMode == "Answer") {
        if (this.CurrentMA.attributeIdAnswer == '') this.CurrentMA.attributeIdAnswer += this.ColumnCode.attribute_id.toString();
        else this.CurrentMA.attributeIdAnswer += ',' + this.ColumnCode.attribute_id.toString();
      } else {
        if (this.CurrentMA.attributeIdQuestion == '') this.CurrentMA.attributeIdQuestion += this.ColumnCode.attribute_id.toString();
        else this.CurrentMA.attributeIdQuestion += ',' + this.ColumnCode.attribute_id.toString();
      }
      setTimeout(() => { if (this.CanSave) this.PleaseSaveTheMA.emit(); }, 10);
    }
  }
  public CloseMe() {
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() { }
}






