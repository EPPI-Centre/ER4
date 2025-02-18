import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { iRobotOpenAiTaskReadOnly, iRobotSettings, RobotsService } from '../services/Robots.service';
import { process, State } from '@progress/kendo-data-query';
import { GridDataResult, PageChangeEvent, RowClassArgs, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { Helpers } from '../helpers/HelperMethods';
import { SVGIcon, fileExcelIcon } from "@progress/kendo-svg-icons";
import { iReviewJob, ReviewInfoService, ReviewJob } from '../services/ReviewInfo.service';
import { ExcelService } from '../services/excel.service';

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
    private excelService: ExcelService,
    private modalService: ModalService
  ) { }

  ngOnInit() {
     this.Refresh();
  }
  public fileExcelIcon: SVGIcon = fileExcelIcon;

  public get DataSourceJobs(): GridDataResult {
    return process(this.reviewInfoService.ReviewJobs, this.state);
  }
  public get Jobs(): ReviewJob[] {
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

  public get IsServiceBusy(): boolean {
    if (this.reviewInfoService.IsBusy) return true;
    else return false;
  }
  public DateTimeOptions: Intl.DateTimeFormatOptions = {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false
  };
  public JobDuration(line: ReviewJob): string {
    line.created.toLocaleDateString()
    const diff = line.JobDurationMs;
    if (diff > 60e3) return Math.floor(diff / 60e3) + " minutes";
    else return Math.floor(diff / 1e3) + " seconds";
  }
  
  public get IsSiteAdmin(): boolean {
    return this._reviewerIdentityServ.reviewerIdentity.isSiteAdmin;
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

  public AllToExcel() {
    this.SaveToExcel(this.reviewInfoService.ReviewJobs, 'Review Jobs (Id ' + this.reviewInfoService.ReviewInfo.reviewId + ')');
  }
  
  public PageToExcel() {
    this.SaveToExcel(this.DataSourceJobs.data, 'Review Jobs Page (Id ' + this.reviewInfoService.ReviewInfo.reviewId + ')');
  }

  SaveToExcel(data: ReviewJob[], filename:string) {
    let res: any[] = [];
    for (let row of data) {
      let rrow = {
        "Review Job Id": row.reviewJobId,
        "Job Type": row.jobType,
        "Job Owner Id": row.jobOwnerId,
        "Owner Name": row.ownerName,
        Created: row.created.toLocaleDateString(undefined, {
          year: 'numeric', month: 'short', day: 'numeric',
          hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false
        }),
        Updated: row.updated.toLocaleDateString(undefined, {
          year: 'numeric', month: 'short', day: 'numeric',
          hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false
        }),
        Duration: this.JobDuration(row),
        Status: row.status,
        Success: row.success,
        "Job Message": row.jobMessage
      };
      res.push(rrow);
    }
    this.excelService.exportAsExcelFile(res, filename);
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
