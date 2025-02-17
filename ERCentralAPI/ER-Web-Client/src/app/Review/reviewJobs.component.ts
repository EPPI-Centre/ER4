import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { iRobotOpenAiTaskReadOnly, iRobotSettings, RobotsService } from '../services/Robots.service';
import { process, State } from '@progress/kendo-data-query';
import { GridDataResult, PageChangeEvent, RowClassArgs, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { Helpers } from '../helpers/HelperMethods';
import { iReviewJob, ReviewInfoService } from '../services/ReviewInfo.service';

@Component({
  selector: 'ReviewJobs',
  templateUrl: './ReviewJobs.component.html',
  styles: [],
  providers: []
})

export class ReviewJobs implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private _reviewerIdentityServ: ReviewerIdentityService,
    public reviewInfoService: ReviewInfoService,
    private modalService: ModalService
  ) { }

  ngOnInit() {
    console.log("ReviewJobs ngOnInit");
    if (this.reviewInfoService.ReviewJobs.length == 0) this.Refresh();
  }
  public get DataSourceJobs(): GridDataResult {
    return process(this.reviewInfoService.ReviewJobs, this.state);
  }
  public get Jobs(): iReviewJob[] {
    return this.reviewInfoService.ReviewJobs;
  }
  public pageSizes: number[] = [10, 20, 50, 100, 200, 500];
  public state: State = {
    skip: 0,
    take: 50
  };
  public dataStateChange(state: DataStateChangeEvent): void {
    this.state = state;
    this.DataSourceJobs; //makes sure it's "processed"
  }

  FormatDate(DateSt: string): string {
    return Helpers.FormatDateTime(DateSt);
  }
  public get IsSiteAdmin(): boolean {
    return this._reviewerIdentityServ.reviewerIdentity.isSiteAdmin;
  }
  public JobType(job: iRobotOpenAiTaskReadOnly): string {
    if (job.rawCriteria == "Robot investigate single query") return "Investigate";
    else if (job.useFullTextDocument == true) return "Coding (full text)";
    else return "Coding";
  }
  public ItemsCount(job: iRobotOpenAiTaskReadOnly): string {
    if (this.JobType(job).startsWith("Coding")) return job.itemIDsList.length.toString();
    else return "N/A";
  }

  public rowCallback = (context: RowClassArgs) => {
    const row: iRobotOpenAiTaskReadOnly = context.dataItem;
    if (row.success == false
      || row.status.indexOf("Failed") == 0
    ) {
      //console.log("error row", row.success, row.status.indexOf("Failed"));
      return { GPTJobsErrorRow: true };
    }
    else return { };
  };
  public JobDuration(line: iRobotOpenAiTaskReadOnly): string {
    const d1: any = new Date(line.created);
    const d2: any = new Date(line.updated);
    const diff = d2 - d1;
    if (diff > 60e3) return Math.floor(diff / 60e3) + " minutes";
    else return Math.floor(diff / 1e3) + " seconds";
  }

  Refresh() {
    this.reviewInfoService.FetchJobs();
  }

  BackToMain() {
    this.router.navigate(['Main']);
  }

  ngOnDestroy() {

  }
}
