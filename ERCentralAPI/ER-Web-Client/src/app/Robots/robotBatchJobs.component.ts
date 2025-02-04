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
  selector: 'RobotBatchJobs',
  templateUrl: './robotBatchJobs.component.html',
  providers: []
})

export class RobotBatchJobs implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private _reviewService: ReviewService,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private robotsService: RobotsService,
    private itemListService: ItemListService,
    private reviewInfoService: ReviewInfoService,
    private reviewSetsService: ReviewSetsService,
    private notificationService: NotificationService,
		private confirmationDialogService: ConfirmationDialogService
  ) { }

  //@Output() onCloseClick = new EventEmitter();
  public ShowSettings: boolean = false;
  public ShowQueue: boolean = true;
  public DetailsJobId: number = -1;
  @Output() PleaseCloseMe = new EventEmitter();

  ngOnInit() {
    this.robotsService.GetCurrentQueue().then(() => {
      if (this.robotsService.CurrentQueue.length > 0) this.ShowQueue = true;
      else this.ShowQueue = false;
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

  public get SelectedItems(): Item[] {
    return this.itemListService.SelectedItems;
  }
  public get SelectedItemsCount(): number {
    return this.itemListService.SelectedItems.length;
  }
  public get SelectedNodeName(): string {
    if (this.reviewSetsService.selectedNode) return this.reviewSetsService.selectedNode.name;
    else return "";
  }
  public get CanSubmitBatch(): boolean {
    if (!this.HasWriteRights) return false;
    else if (!this.reviewInfoService.ReviewInfo.hasCreditForRobots) return false;
    else {
      let node = this.reviewSetsService.selectedNode;
      if (node != null && node.nodeType == 'ReviewSet' && (node.subTypeName == "Standard" || node.subTypeName == "Screening")) {
        if (this.SelectedItemsCount > 0 && this.SelectedItemsCount <= 1000) return true;
        else return false;
      }
      else return false;
    }
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

  public SubmitBatch() {
    if (!this.CanSubmitBatch) return;
    const encoded = Helpers.htmlEncode(this.reviewSetsService.selectedNode  ? this.reviewSetsService.selectedNode.name : "[undefined]");
    let msg: string = "This will queue a batch-coding request.<br />"
    if (this.RobotSettings.useFullTextDocument) {
      msg += "The batch contains <strong>" + this.SelectedItemsCount.toString() + (this.SelectedItemsCount > 1 ? " Items" : " Item") + "</strong>.<br />"
        + "The coding tool to use is: <br />"
        + "<div class='w-100 p-0 mx-0 my-1 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
        + encoded + "</strong></div>"
        + "<div class='my-1 px-1 alert-warning'>The job will submit <strong>full-text documents</strong> to GPT. This means that:"
        + "<ol><li>The PDFs will be parsed for processing, which can take minutes (per document)</li>"
        + "<li>Cost per item is higher (possibly about <strong>£0.10 per document</strong>)</li>"
        + "<li>Process is much slower, as each item might take more than one minute to process</li></ol>"
        + "Are you <strong>sure</strong> you want to proceed?"
        + "</div>"
        + "<span class='small'>The job will be queued on a 1st-come, 1st-served basis and might take a while to start and/or run.</span>";
    }
    else {
      msg += "The batch contains <strong>" + this.SelectedItemsCount.toString() + (this.SelectedItemsCount > 1 ? " Items" : " Item") + "</strong>.<br />"
        + "The coding tool to use is: <br />"
        + "<div class='w-100 p-0 mx-0 my-1 text-center'><strong class='border mx-auto px-1 rounded border-success d-inline-block'>"
        + encoded + "</strong></div>"
        + "<span class='small'>The job will be queued on a 1st-come, 1st-served basis and might take a while to start and/or run.</span>";
    }
    this.confirmationDialogService.confirm("Submit Robot-Coding batch?", msg, false, "", "Submit", "Cancel"
      , this.RobotSettings.useFullTextDocument ? "lg" : "sm").then((confirm: any) => {
      if (confirm) {
        this.ActuallySubmitBach();
      }
    });
  }
  private ActuallySubmitBach() {
    let crit: string = "ItemIds: ";
    for (let itm of this.SelectedItems) {
      crit += itm.itemId + ",";
    }
    crit = crit.substring(0, crit.length - 1);
    const node = this.reviewSetsService.selectedNode as ReviewSet;
    let data: iRobotOpenAiQueueBatchJobCommand = {
      reviewSetId: node.reviewSetId,
      criteria: crit,
      onlyCodeInTheRobotName: this.RobotSettings.onlyCodeInTheRobotName,
      lockTheCoding: this.RobotSettings.lockTheCoding,
      useFullTextDocument: this.RobotSettings.useFullTextDocument,
      returnMessage: ""
    };
    this.robotsService.EnqueueRobotOpenAIBatchCommand(data).then(
      (res:boolean) => {
        if (res) {
          this.notificationService.show({
            content: "Your Batch Coding request has been Queued.",
            animation: { type: 'slide', duration: 400 },
            position: { horizontal: 'center', vertical: 'top' },
            type: { style: "info", icon: true },
            hideAfter: 4000
          });
          this.robotsService.GetCurrentQueue();
          this.ShowQueue = true;
        }
      }

    );
  }
  Close() {
    this.PleaseCloseMe.emit();
  }
  ngOnDestroy() {

  }
}
