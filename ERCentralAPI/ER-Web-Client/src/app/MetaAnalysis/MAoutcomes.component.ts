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
import { Outcome } from '../services/outcomes.service';



@Component({
  selector: 'MAoutcomes',
  templateUrl: './MAoutcomes.component.html',
  providers: [],
  styles: []
})
export class MAoutcomesComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private router: Router,
    private MetaAnalysisService: MetaAnalysisService
  ) { }
  ngOnInit() {
    
  }



  
  public get HasWriteRights(): boolean {
    return this.ReviewerIdentityServ.HasWriteRights;
  }
  public get Outcomes(): Outcome[] {
    if (this.MetaAnalysisService.CurrentMetaAnalysis == null) return [];
    else return this.MetaAnalysisService.CurrentMetaAnalysis.outcomes;
  }

  //public get CurrentMA(): MetaAnalysis | null {
  //  return this.MetaAnalysisService.CurrentMetaAnalysis;
  //}

  
  ngOnDestroy() { }
}
