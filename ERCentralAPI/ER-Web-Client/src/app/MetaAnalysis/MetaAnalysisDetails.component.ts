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
  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }
  public get CurrentMA(): MetaAnalysis | null {
    return this.MetaAnalysisService.CurrentMetaAnalysis;
  }
  public get CanSave(): boolean {
    return this.HasWriteRights && this.MetaAnalysisService.CurrentMAhasChanges;
  }

  public Save() {

  }


  ngOnDestroy() { }
}






