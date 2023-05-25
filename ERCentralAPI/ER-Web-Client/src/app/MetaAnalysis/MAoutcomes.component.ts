import { Component, OnInit, OnDestroy, EventEmitter, Output } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { Item, ItemListService, StringKeyValue } from '../services/ItemList.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { DynamicColumnsOutcomes, MetaAnalysis, MetaAnalysisSelectionCrit, MetaAnalysisService } from '../services/MetaAnalysis.service';
import { ExtendedOutcome } from '../services/outcomes.service';
import { CustomSorting } from '../helpers/CustomSorting';



@Component({
  selector: 'MAoutcomes',
  templateUrl: './MAoutcomes.component.html',
  providers: [],
  styles: [
`
.OutcomesTableContainer {border-top: 1px solid DarkBlue; border-bottom: 1px solid DarkBlue;}
.OutcomesTable th {border: 1px dotted Silver; min-width:3vw;}
.OutcomesTable td {border: 1px dotted Silver;}
.sortableTH { cursor:pointer;}
.QuestionCol { background: Khaki;  cursor:pointer;}
.AnswerCol { background: LemonChiffon; cursor:pointer;}
.ClassifCol { background: LightGray; border: 1px dotted white !important; cursor:pointer;}
.FirstQuestion, .FirstAnswer, .FirstClassif {border-left:1px solid DarkBlue !important;}
.filterIcon {padding: 6px 8px 8px 8px ; border: 1px solid #00000000; border-radius: 3px;}
.filterIcon:hover {border: 1px solid blue; border-radius: 3px; color:blue;}
`
  ]
})
export class MAoutcomesComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private router: Router,
    private MetaAnalysisService: MetaAnalysisService
  ) { }
  ngOnInit() {
    
  }

  @Output() PleaseEditThisFilter = new EventEmitter<string>();

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

  public SortingSymbol(fieldName: string): string {
    return CustomSorting.SortingSymbol(fieldName, this.MetaAnalysisService.LocalSort);
  }
  public Sort(fieldname: string) {
    this.MetaAnalysisService.SortOutcomesBy(fieldname);
  }
  public IsFilteringOnThisCol(ER4Colname: string): boolean {
    if (this.MetaAnalysisService.CurrentMetaAnalysis == null) return false;
    if (this.MetaAnalysisService.CurrentMetaAnalysis.filterSettingsList.findIndex(f => f.columnName == ER4Colname) > -1) return true;
    return false;
  }

  public DoEditThisFilter(fieldName: string, event: Event) {
    event.stopPropagation();
    this.PleaseEditThisFilter.emit(fieldName);
  }

  
  ngOnDestroy() { }
}
