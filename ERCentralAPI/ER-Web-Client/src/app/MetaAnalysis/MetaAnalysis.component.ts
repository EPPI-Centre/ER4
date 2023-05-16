import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { Item, ItemListService, KeyValue } from '../services/ItemList.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Helpers } from '../helpers/HelperMethods';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { MetaAnalysis, MetaAnalysisService } from '../services/MetaAnalysis.service';



@Component({
  selector: 'MetaAnalysis',
  templateUrl: './MetaAnalysis.component.html',
  providers: [],
  styles: []
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



  public get IsServiceBusy(): boolean {
    return (
      this.MetaAnalysisService.IsBusy 
      //|| this.SomeotherService.IsBusy
    );
  }

  public get MaList(): MetaAnalysis[] {
    return this.MetaAnalysisService.MetaAnalysisList;
  }



  BackHome() {
    this.router.navigate(['Main']);
  }
  ngOnDestroy() { }
}
