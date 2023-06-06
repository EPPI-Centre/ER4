import { Component, OnInit,Input, ViewChild, OnDestroy, EventEmitter, Output } from '@angular/core';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { iReference, MetaAnalysis, MetaAnalysisService, Moderator } from '../services/MetaAnalysis.service';
import { FilterOutcomesFormComp } from './FilterOutcomesForm.component';

@Component({
  selector: 'MAmoderatorsComp',
  templateUrl: './MAmoderators.component.html',
  providers: [],
  styles: [
`
.ModeratorsTableContainer {border-top: 1px solid DarkBlue; border-bottom: 1px solid DarkBlue; max-height: 40vh; overflow:auto; max-width:90vw;}
.ModeratorsTable thead th {background-color: #dff0df; box-shadow: inset 0px -0.8px #222222, 0 0 #000; }
`
  ]
})
export class MAmoderatorsComp implements OnInit, OnDestroy {

  constructor(
    private ReviewerIdentityServ: ReviewerIdentityService,
    private MetaAnalysisService: MetaAnalysisService
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();
  ngOnInit() { }
  

  public get CurrentMA(): MetaAnalysis | null {
    return this.MetaAnalysisService.CurrentMetaAnalysis;
  }
  public get Moderators(): Moderator[] {
    if (this.MetaAnalysisService.CurrentMetaAnalysis) return this.MetaAnalysisService.CurrentMetaAnalysis.metaAnalysisModerators;
    else return [];
  }
  public ChangingReference(event: Event, Mod: Moderator) {
    const val: string = (event.target as HTMLOptionElement).value;
    Mod.reference = val;
  }
  
  public CloseMe() {
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() { }
}






