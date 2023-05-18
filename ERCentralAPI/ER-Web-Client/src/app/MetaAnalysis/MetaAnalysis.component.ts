import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { Item, ItemListService, KeyValue } from '../services/ItemList.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { MetaAnalysis, MetaAnalysisSelectionCrit, MetaAnalysisService } from '../services/MetaAnalysis.service';



@Component({
  selector: 'MetaAnalysis',
  templateUrl: './MetaAnalysis.component.html',
  providers: [],
  styles: [`
@keyframes hiding {
  0%   {max-height:70vh;}
  33%   {max-height:60vh;}
  100% {max-height:0vh;}
}
@keyframes showing {
  0%   {max-height:0vh;}
  33%   {max-height:10vh;}
  100% {max-height:70vh;}
}
.HideAnim { }
.HideAnim.hide {
animation-name: hiding;
animation-duration: 0.5s;
animation-timing-function: linear;
animation-delay: 0s;
animation-iteration-count: 1;
overflow:clip;
max-height:0vh;
z-index:-1500;
}
.HideAnim.show {
z-index:auto;
max-height:70vh;
animation-name: showing;
animation-duration: 0.5s;
animation-timing-function: linear;
animation-delay: 0s;
animation-iteration-count: 1;
overflow:auto;
}
  `]
})
export class MetaAnalysisComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private router: Router,
    private MetaAnalysisService: MetaAnalysisService
  ) { }
  ngOnInit() {
    if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
      this.router.navigate(['home']);
    }
    else {
      this.MetaAnalysisService.FetchMAsList();
    }
  }

  public TopIsExpanded: boolean = true;
  public BottomIsExpanded: boolean = false;

  public get IsServiceBusy(): boolean {
    return (
      this.MetaAnalysisService.IsBusy 
      //|| this.SomeotherService.IsBusy
    );
  }
  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }
  public get MaList(): MetaAnalysis[] {
    return this.MetaAnalysisService.MetaAnalysisList;
  }

  public get CurrentMA(): MetaAnalysis | null {
    return this.MetaAnalysisService.CurrentMetaAnalysis;
  }
  public get ExpandCollapseTopTxt(): string {
    if (this.TopIsExpanded) return "Collapse MAs list";
    else return "Expand MAs list";
  }


  public EditMA(ma: MetaAnalysis) {
    const crit: MetaAnalysisSelectionCrit = { MetaAnalysisId: ma.metaAnalysisId, GetAllDetails: true };
    this.MetaAnalysisService.FetchMetaAnalysis(crit);
    this.BottomIsExpanded = true;
    this.TopIsExpanded = false;
  }

  BackHome() {
    this.router.navigate(['Main']);
  }
  ngOnDestroy() {
    this.MetaAnalysisService.Clear();//maybe we should keep the MAs list?
  }
}
