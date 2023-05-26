import { Component, OnInit,Input, ViewChild, OnDestroy } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { MetaAnalysis, MetaAnalysisService } from '../services/MetaAnalysis.service';

@Component({
  selector: 'MetaAnalysisDetailsComp',
  templateUrl: './MetaAnalysisDetails.component.html',
    providers: [],
    styles: []
})
export class MetaAnalysisDetailsComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private MetaAnalysisService: MetaAnalysisService
  ) { }


  ngOnInit() { }
  public ActivePanel: string = "";
  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }
  public FilterToBeEdited: string = "";

  public get CurrentMA(): MetaAnalysis | null {
    return this.MetaAnalysisService.CurrentMetaAnalysis;
  }
  public get CanSave(): boolean {
    return this.HasWriteRights && this.MetaAnalysisService.CurrentMAhasChanges;
  }
  public CloseActivePanel() {
    this.FilterToBeEdited = "";
    this.ActivePanel = "";
  }
  public EditFilters() {
    this.FilterToBeEdited = "";
    this.ActivePanel = "EditFilters";
  }

  public PleaseEditThisFilter(fieldName: string) {
    if (this.ActivePanel != "EditFilters") {
      this.FilterToBeEdited = fieldName;
      this.ActivePanel = "EditFilters";
    } else {
      //what to do when panel is open and user clicks on the "filter" icon for a column in the outcomes table? For now: nothing!
    }
  }

  public Save() {
    if (this.CurrentMA) {
      this.MetaAnalysisService.SaveMetaAnalysis(this.CurrentMA)
    }
  }


  ngOnDestroy() { }
}






