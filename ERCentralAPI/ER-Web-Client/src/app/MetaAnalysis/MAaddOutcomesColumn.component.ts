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

  ngOnInit() {
    if (this.ReviewSetsService.ReviewSets.length == 0) this.ReviewSetsService.GetReviewSets();
  }

  public ColumnMode: string = "Answer";//or "Question"...
  public MaxOptionalColumns = 3;
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
      //this.getErWebObjects();//we directly get sync data for "Items with this code".
    }
  }

  public CloseMe() {
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() { }
}






