import { Component, OnInit, OnDestroy, EventEmitter, Output } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { Item, ItemListService, StringKeyValue } from '../services/ItemList.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { DynamicColumnsOutcomes, IdAndNamePair, MetaAnalysis, MetaAnalysisSelectionCrit, MetaAnalysisService } from '../services/MetaAnalysis.service';
import { ExtendedOutcome } from '../services/outcomes.service';
import { CustomSorting } from '../helpers/CustomSorting';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';



@Component({
  selector: 'MAoutcomes',
  templateUrl: './MAoutcomes.component.html',
  providers: [],
  styles: [
`
.OutcomesTableContainer {border-top: 1px solid DarkBlue; border-bottom: 1px solid DarkBlue; max-height: 55vh; overflow:auto; max-width:95vw;}
.OutcomesTable table { max-height: 50vh; max-width: 90vm; }
.OutcomesTable th {border: 1px dotted Silver; min-width:3vw;}
.OutcomesTable thead th {background-color: #fbfbfb; box-shadow: inset 0px -0.8px #222222, 0 0 #000; }
.OutcomesTable td {border: 1px dotted Silver;}
.sortableTH { cursor:pointer;}
.QuestionCol { background: Khaki !important;  cursor:pointer;}
.AnswerCol { background: LemonChiffon !important; cursor:pointer;}
.ClassifCol { background: LightGray !important; border: 1px dotted white !important; cursor:pointer;}
.FirstQuestion, .FirstAnswer, .FirstClassif {border-left:1px solid DarkBlue !important;}
.clickableIcon {padding: 6px 8px 8px 8px ; border: 1px solid #00000000; border-radius: 3px;}
.clickableIcon:hover {border: 1px solid blue; border-radius: 3px; color:blue;}
.text-danger.clickableIcon:hover {border: 1px solid red; border-radius: 3px; color:red;}
.DisabledClickableIcon { color:Gray !important;}
.DisabledClickableIcon:hover {border: 1px solid Gray !important; color:Gray !important;}

`]
})
  //see https://stackoverflow.com/a/47923622 for how the "ticky" thing works for tableFixHead!!

export class MAoutcomesComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private router: Router,
    private MetaAnalysisService: MetaAnalysisService,
    private ConfirmationDialogService: ConfirmationDialogService
  ) { }
  ngOnInit() {
    
  }

  @Output() PleaseEditThisFilter = new EventEmitter<string>();
  @Output() PleaseSaveTheCurrentMA = new EventEmitter<void>();

  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }
  public get Outcomes(): ExtendedOutcome[] {
    if (this.MetaAnalysisService.CurrentMetaAnalysis == null) return [];
    else return this.MetaAnalysisService.FilteredOutcomes;
  }
  public get ColumnVisibility(): DynamicColumnsOutcomes {
    return this.MetaAnalysisService.ColumnVisibility;
  }

  public get HasSelections(): number {
    //console.log("HasSelections o1", this.Outcomes.length, this.Outcomes.filter(f => f.isSelected == true).length);
    const selectedCount = this.Outcomes.filter(f => f.isSelected == true).length;
    if (selectedCount == 0) return 0;
    const selectableCount = this.Outcomes.filter(f => f.canSelect == true).length;
    //console.log("HasSelections o2", this.Outcomes.length, this.Outcomes.filter(f => f.canSelect == true).length);
    if (selectedCount != selectableCount) return 1; //partial selection
    else return 2;
  }
  public get SelectedCount(): number {
    return this.Outcomes.filter(f => f.isSelected == true).length;
  }

  public SortingSymbol(fieldName: string): string {
    return CustomSorting.SortingSymbol(fieldName, this.MetaAnalysisService.LocalSort);
  }
  public Sort(fieldname: string) {
    this.MetaAnalysisService.SortOutcomesBy(fieldname);
  }
  public IsFilteringOnThisCol(ER4Colname: string): boolean {
    if (this.MetaAnalysisService.CurrentMetaAnalysis == null) return false;
    if (this.MetaAnalysisService.CurrentMetaAnalysis.filterSettingsList.findIndex(f => !f.isClear && f.columnName == ER4Colname) > -1) return true;
    return false;
  }

  public DoEditThisFilter(fieldName: string, event: Event) {
    event.stopPropagation();
    this.PleaseEditThisFilter.emit(fieldName);
  }
  public SelectAll() {
    for (let o of this.Outcomes) {
      if (o.canSelect == true) o.isSelected = true;
    }
  }
  public UnSelectAll() {
    for (let o of this.Outcomes) {
      o.isSelected = false;
    }
  }
  public DeleteColumn(colToDelete: IdAndNamePair, event: Event) {
    event.stopPropagation();
    if (!this.HasWriteRights) return;
    this.ConfirmationDialogService.confirm("Delete column?"
      , "Are you sure you want to delete this column? "
      + "<div class='w-100 p-0 mx-0 my-2 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
      + colToDelete.Name + "</strong></div>"
      + "Removing this column will <strong>save</strong> the whole Meta Analysis."
      , false, '')
      .then((confirm: any) => {
        if (confirm) {
          this.DoDeleteColumn(colToDelete);
        }
      });
  }
  private DoDeleteColumn(colToDelete: IdAndNamePair) {
    //need to:
    //1. remove name/id from 2 fields in CurrentMetaAnalysis
    //2. remove column from this.ColumnVisibility
    //3. check "sortBy", react if we're sorting by the column that's about to disappear.
    //4. save all changes! (this is to match the behaviour of "add column" where we HAVE to save the whole MA)

    if (this.MetaAnalysisService.CurrentMetaAnalysis == null) return;
    const separator = String.fromCharCode(0x00AC); //the "not" simbol, or inverted pipe
    let ind: number = this.ColumnVisibility.AnswerHeaders.indexOf(colToDelete);
    let colname = "";
    if (ind != -1) {
      //answer col
      //attributeIdAnswer only has commas between elements, so there is some discerning to do...
      if (this.ColumnVisibility.AnswerHeaders.length == 1) {//only element
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeAnswerText = "";
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer = "";
      } else {
        if (ind == 0) {//first element of many
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer.replace(colToDelete.Id.toString() + ',', '');
        } else {
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdAnswer.replace(',' + colToDelete.Id.toString(), '');
        }
        this.MetaAnalysisService.CurrentMetaAnalysis.attributeAnswerText =
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeAnswerText.replace(colToDelete.Name + separator, '');
      }
      colname = "aa" + (ind + 1).toString();
      this.ColumnVisibility.AnswerHeaders.splice(ind, 1);
    } else {
      ind = this.ColumnVisibility.QuestionHeaders.indexOf(colToDelete);
      if (ind != -1) {
        //question col
        //attributeIdQuestion only has commas between elements, so there is some discerning to do...
        if (this.ColumnVisibility.QuestionHeaders.length == 1) {//only element
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeQuestionText = "";
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion = "";
        } else {
          if (ind == 0) {//first element of many 
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion =
              this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion.replace(colToDelete.Id.toString() + ',', '');
          } else {
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion =
              this.MetaAnalysisService.CurrentMetaAnalysis.attributeIdQuestion.replace(',' + colToDelete.Id.toString(), '');
          }
          this.MetaAnalysisService.CurrentMetaAnalysis.attributeQuestionText =
            this.MetaAnalysisService.CurrentMetaAnalysis.attributeQuestionText.replace(colToDelete.Name + separator, '');
        }
        colname = "aq" + (ind + 1).toString();
        this.ColumnVisibility.QuestionHeaders.splice(ind, 1);
      }
    }
    if (this.MetaAnalysisService.CurrentMetaAnalysis.sortedBy == colname) {
      this.MetaAnalysisService.UnSortOutcomes();
    }
    this.PleaseSaveTheCurrentMA.emit();
  }
  ngOnDestroy() { }
}
