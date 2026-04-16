import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { iRobotOpenAiQueueBatchJobCommand, iRobotOpenAiTaskReadOnly, iRobotSettings, RobotsService } from '../services/Robots.service';
import { Item, ItemListService } from '../services/ItemList.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { ReviewSet, ReviewSetsService } from '../services/ReviewSets.service';
import { NotificationService } from '@progress/kendo-angular-notification';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { Helpers } from '../helpers/HelperMethods';

@Component({
  selector: 'robotJobsQueue',
  templateUrl: './robotJobsQueue.component.html',
  providers: []
})

export class robotJobsQueue implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private _reviewService: ReviewService,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private robotsService: RobotsService,
    private notificationService: NotificationService,
		private confirmationDialogService: ConfirmationDialogService
  ) { }


  @Input() ShowQueue: boolean = true;
  @Output() PleaseCloseMe = new EventEmitter();
  public DetailsJobId: number = -1;

  ngOnInit() {
    this.robotsService.GetCurrentQueue().then(() => {
      if (this.robotsService.CurrentQueue.length > 0) this.ShowQueue = true;
      else {
        this.PleaseCloseMe.emit();
      }
    });
  }
  HasWriteRights(): boolean {
    return this._reviewerIdentityServ.HasWriteRights;
  }

  public get RobotSettings(): iRobotSettings {
    return this.robotsService.RobotSetting;
  }

  public get CurrentQueue(): iRobotOpenAiTaskReadOnly[] {
    return this.robotsService.CurrentQueue;
  }
  public CloseJobsQueue() {
    this.PleaseCloseMe.emit();
  }
  public JobCSSclass(Job: iRobotOpenAiTaskReadOnly): string {
    let res:string = "";
    if (this._reviewerIdentityServ.reviewerIdentity.isSiteAdmin) {
      if (Job.reviewId == this._reviewerIdentityServ.reviewerIdentity.reviewId
        || Job.jobOwnerId == this._reviewerIdentityServ.reviewerIdentity.userId) res = "alert-primary";
      else res = "";
    } else {
      if (Job.reviewId > 0) res = "alert-primary";
      else res = "";
    }
    if (Job.robotApiCallId == this.DetailsJobId) {
      res += (res == "" ? "" : " ") + "font-weight-bold";
    }
    return res;
  }
  public get DetailedJob(): iRobotOpenAiTaskReadOnly | undefined {
    if (this.DetailsJobId > 0) {
      const index = this.robotsService.CurrentQueue.findIndex(f => f.robotApiCallId == this.DetailsJobId);
      if (index == -1) this.DetailsJobId = -1;
      else if (this.robotsService.CurrentQueue[index].reviewId > 0) {
        return this.robotsService.CurrentQueue[index];
      }
    }
    return undefined;
  }

  public JobDescription(Job: iRobotOpenAiTaskReadOnly): string {
    if (Job.status == 'Running') {
      let index = Job.itemIDsList.findIndex(f => f == Job.currentItemId) + 1;
      return "Done Item " + index.toString() + " of " + Job.itemIDsList.length.toString();
    }
    else return 'Queued';
  }
  public JobDetails(robotApiCallId: number) {
    if (this.DetailsJobId == robotApiCallId) {
      this.DetailsJobId = -1;
      return;
    }
    const index = this.robotsService.CurrentQueue.findIndex(f => f.robotApiCallId == robotApiCallId);
    if (index == -1) this.DetailsJobId = -1;
    else this.DetailsJobId = robotApiCallId;
  }
  public RefreshQueue() {
    this.robotsService.GetCurrentQueue();
  }
  public GoToPastJobs() {
    this.router.navigate(['JobsRecord']);
  }


  public CancelJob(job: iRobotOpenAiTaskReadOnly) {
    this.robotsService.CancelRobotOpenAIBatch(job.robotApiCallId);
  }
  ngOnDestroy() {

  }
}
