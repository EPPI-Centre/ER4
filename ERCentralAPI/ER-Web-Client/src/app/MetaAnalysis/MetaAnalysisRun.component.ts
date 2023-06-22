import { Component, OnInit,Input, ViewChild, OnDestroy, EventEmitter, Output } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { iReference, MetaAnalysis, MetaAnalysisService, Moderator } from '../services/MetaAnalysis.service';
import { FilterOutcomesFormComp } from './FilterOutcomesForm.component';

@Component({
  selector: 'MetaAnalysisRunComp',
  templateUrl: './MetaAnalysisRun.component.html',
  providers: [],
  styles: []
})
export class MetaAnalysisRunComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private MetaAnalysisService: MetaAnalysisService
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();
  ngOnInit() { }
  

  public get CurrentMA(): MetaAnalysis | null {
    return this.MetaAnalysisService.CurrentMetaAnalysis;
  }

  ChangingModel(event: Event) {
    this.EnableDisableKNHA();
  }
   EnableDisableKNHA() {
    //if (cbModel != null && cbModel.SelectedIndex == 0) {
    //  cbKNHA.IsChecked = false;
    //  cbKNHA.IsEnabled = false;
    //}
    //else {
    //  if (cbKNHA != null)
    //    cbKNHA.IsEnabled = true;
    //}
  }
  public get knhaIsDisabled(): boolean {
    if (!this.CurrentMA) return true;
    else if (this.CurrentMA.statisticalModel == 0) {
      this.CurrentMA.knha = false;
      return true;
    }
    else return false;
  }

  public CloseMe() {
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() { }
}






