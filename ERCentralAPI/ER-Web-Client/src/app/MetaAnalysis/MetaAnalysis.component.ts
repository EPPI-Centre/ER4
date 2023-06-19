import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { Item, ItemListService, StringKeyValue } from '../services/ItemList.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { MetaAnalysis, MetaAnalysisSelectionCrit, MetaAnalysisService } from '../services/MetaAnalysis.service';
import { MetaAnalysisDetailsComp } from './MetaAnalysisDetails.component';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';



@Component({
  selector: 'MetaAnalysis',
  templateUrl: './MetaAnalysis.component.html',
  providers: [],
  styles: [`
@keyframes hiding {
  0%   {max-height:70vh; padding:0.5rem;}
  33%   {max-height:60vh; padding:0.2rem;}
  100% {max-height:0vh; padding:0;}
}
@keyframes showing {
  0%   {max-height:0vh; padding:0;}
  33%   {max-height:10vh; padding:0.3rem;}
  100% {max-height:70vh; padding:0.5rem;}
}
.HideAnim { padding:0.5rem;}
.HideAnim.hide {
animation-name: hiding;
animation-duration: 0.5s;
animation-timing-function: linear;
animation-delay: 0s;
animation-iteration-count: 1;
overflow:clip;
padding:0;
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
padding:0.5rem;
}
.MAsTableContainer { max-height: 60vh; overflow:auto; max-width:98vw;}
.MAsTable thead th {background-color: #f8f9fa; box-shadow: inset 0px -0.8px #222222, 0 0 #000; }

.MAsTable td {padding:0;}
.MAsTable div {padding:0.3rem;}
.selectedMA td {background-color: #cce5ff;}
.selectedMA div {font-weight:bold;}
  `]
})
export class MetaAnalysisComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private router: Router,
    private MetaAnalysisService: MetaAnalysisService,
    private ConfirmationDialogService: ConfirmationDialogService
  ) { }

  @ViewChild('MetaAnalysisDetailsComp') MetaAnalysisDetailsComp!: MetaAnalysisDetailsComp;

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
    if (this.MetaAnalysisDetailsComp) this.MetaAnalysisDetailsComp.CloseActivePanel();
    this.MetaAnalysisService.FetchMetaAnalysis(crit);
    this.BottomIsExpanded = true;
    this.TopIsExpanded = false;
  }
  public NewMA() {
    if (this.MetaAnalysisDetailsComp) this.MetaAnalysisDetailsComp.CloseActivePanel();
    this.MetaAnalysisService.FetchEmptyMetaAnalysis();
    this.BottomIsExpanded = true;
    this.TopIsExpanded = false;
  }

  public DeleteMA(ma: MetaAnalysis) {
    if (!this.HasWriteRights) return;
    this.ConfirmationDialogService.confirm("Delete Meta Analysis?"
      , "Are you sure you want to delete this Meta Analysis? "
      + "<div class='w-100 p-0 mx-0 my-2 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
      + ma.title + "</strong></div>"
      + "This deletion would be <strong>permanent</strong>."
      , false, '')
      .then((confirm: any) => {
        if (confirm) {
          this.DoDeleteMA(ma);
        }
      });
  }
  public DoDeleteMA(ma: MetaAnalysis) {
    if (!this.HasWriteRights) return;
    if (this.BottomIsExpanded && this.CurrentMA != null && this.CurrentMA.metaAnalysisId == ma.metaAnalysisId) {
      this.BottomIsExpanded = false;
      this.TopIsExpanded = true;
    }
    this.MetaAnalysisService.DeleteMetaAnalysis(ma.metaAnalysisId);
  }

  BackHome() {
    this.router.navigate(['Main']);
  }
  ngOnDestroy() {
    this.MetaAnalysisService.Clear();//maybe we should keep the MAs list?
  }
}
