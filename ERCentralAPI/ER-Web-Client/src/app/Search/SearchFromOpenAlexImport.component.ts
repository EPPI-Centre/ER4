import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { faArrowsRotate, faSpinner } from '@fortawesome/free-solid-svg-icons';
import { ClassifierService } from '../services/classifier.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { SetAttribute, singleNode } from '../services/ReviewSets.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { MAGRelatedRunsService } from '../services/MAGRelatedRuns.service';
import { MagAutoUpdateRun } from '../services/MAGClasses.service';
import { ReadOnlySource, Source, SourcesService } from '../services/sources.service';
import { Helpers } from '../helpers/HelperMethods';

@Component({
  selector: 'SearchFromOpenAlexImport',
  templateUrl: './SearchFromOpenAlexImport.component.html',
  providers: []
})

export class SearchFromOpenAlexImport implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private MagRelatedRunsService: MAGRelatedRunsService,
    private sourcesService: SourcesService,
    private confirmationDialogService: ConfirmationDialogService,
    private notificationService: NotificationService,
    private modalService: ModalService
  ) { }

  @Output() PleaseCloseMe = new EventEmitter();
  ngOnInit() {
    this.Refresh();
    //setTimeout(() => {
    //  let res = this.MagRelatedRunsService.MagAutoUpdateRunList;
    //  for (let i = -100; i < -1; i++) {
    //    let p = this.MakeEmptyAutoUpdateRun();
    //    p.magAutoUpdateRunId = i;
    //    p.userDescription = "Fake N." + (-1 * i).toString();
    //    res.push(p);
    //  }
    //}, 500);
  }
  faArrowsRotate = faArrowsRotate;
  faSpinner = faSpinner;

  public SelectedMagAutoUpdateRun: MagAutoUpdateRun = this.MakeEmptyAutoUpdateRun();
  public IncEx: string = 'bothIandE';
  public ScoresToUse: string = "A";

  private MakeEmptyAutoUpdateRun(): MagAutoUpdateRun {
    return {
      magAutoUpdateRunId: -1,
      reviewIdId: this._reviewerIdentityServ.reviewerIdentity.reviewId,
      userDescription: "N/A",
      attributeId: -1,
      attributeName: "N/A",
      allIncluded: true,
      studyTypeClassifier: "",
      userClassifierDescription: "",
      userClassifierModelId: -1,
      userClassifierModelReviewId: -1,
      dateRun: "N/A",
      nPapers: 0,
      magAutoUpdateId: -1,
      magVersion: "N/A",
    };
  }
  public Refresh() {
    this.MagRelatedRunsService.GetMagAutoUpdateRunList();
  }
  public get HasWriteRights(): boolean {
    return this._reviewerIdentityServ.HasWriteRights;
  }
  public get MagAutoUpdateRunList(): MagAutoUpdateRun[] {
    return this.MagRelatedRunsService.MagAutoUpdateRunList;
  }
  
  public get SourceList(): ReadOnlySource[] {
    return this.sourcesService.ReviewSources;
  }
  public get AutoUpdateSourceList(): ReadOnlySource[] {
    return this.sourcesService.ReviewSources.filter(f => f.source_Name.startsWith("Automated search:"));
  }
  public FormatDate(date: string): string {
    return Helpers.FormatDate(date);
  }

  public SelectThisRun(run: MagAutoUpdateRun) {
    if (this.SelectedMagAutoUpdateRun == run) {
      this.SelectedMagAutoUpdateRun = this.MakeEmptyAutoUpdateRun();
      return;
    }
    this.SelectedMagAutoUpdateRun = run;
    this.ScoresToUse = "A";//reset to using only the auto-update score whenever we changed the "run"
  }
  public UseTheseScores(flag: string, event: Event) {
    
    //allowed: "", A, P, U, AP, AU, APU, PU
    if (this.ScoresToUse.includes(flag)) {
      this.ScoresToUse = this.ScoresToUse.replace(flag, "");
      if (this.ScoresToUse == "") {
        if (flag == "A") event.preventDefault();//do not untick as we're re-ticking it!
        this.ScoresToUse = "A";
      }
      return;
    }
    if (flag == "A") {
      this.ScoresToUse = "A" + this.ScoresToUse;
    } else if (flag == "U") {
      this.ScoresToUse = this.ScoresToUse + "U";
    }
    else if (flag == "P") {
      if (this.ScoresToUse == "AU") this.ScoresToUse = "APU";
      else if (this.ScoresToUse == "A") this.ScoresToUse = "AP";
      else this.ScoresToUse = "P" + this.ScoresToUse;
    }
  }

  //public openConfirmationDialogCheckScreening() {
  //  this.confirmationDialogService.confirm('Please confirm', 'Are you sure you wish to check screening with these codes ?', false, '')
  //    .then(
  //      (confirmed: any) => {
  //        //console.log('User confirmed:', confirmed);
  //        if (confirmed) {
            
  //        }
  //        else {
  //          //alert('pressed cancel close dialog');
  //        };
  //      }
  //    )
  //    .catch(() => { });
  //}

  
  Close() {
    this.PleaseCloseMe.emit();
  }


  ngOnDestroy() {
    this.MagRelatedRunsService.Clear();
  }
}
