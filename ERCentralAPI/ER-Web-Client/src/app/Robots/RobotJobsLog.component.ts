import { Component, Inject, OnInit, OnDestroy, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewService } from '../services/review.service';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ModalService } from '../services/modal.service';
import { iRobotCoderReadOnly, iRobotOpenAiTaskReadOnly, iRobotSettings, RobotOpenAiTaskReadOnly, RobotsService } from '../services/Robots.service';
import { process, State } from '@progress/kendo-data-query';
import { GridDataResult, PageChangeEvent, RowClassArgs, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { DataForMultiSheetExcel, ExcelService } from '../services/excel.service';
import { Helpers } from '../helpers/HelperMethods';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { indexOf } from 'lodash';

@Component({
  selector: 'RobotJobsLog',
  templateUrl: './RobotJobsLog.component.html',
  styles: [],
  providers: []
})

export class RobotJobsLog implements OnInit, OnDestroy {
  constructor(private router: Router,
    @Inject('BASE_URL') private _baseUrl: string,
    private _reviewerIdentityServ: ReviewerIdentityService,
    private reviewSetsService: ReviewSetsService,
    private robotsService: RobotsService,
    private excelService: ExcelService,
    private modalService: ModalService
  ) { }

  ngOnInit() {
    if (this.reviewSetsService.ReviewSets.length == 0) this.reviewSetsService.GetReviewSets(false);
    if (this.robotsService.PastJobs.length == 0) setTimeout(() => { this.Refresh(); }, 80);
  }
  public GetAllJobsOption:boolean = false;
  CanWrite(): boolean {
    return this._reviewerIdentityServ.HasWriteRights;
  }
  public get DataSourceJobs(): GridDataResult {
    return process(this.robotsService.PastJobs, this.state);
  }
  public get Jobs(): RobotOpenAiTaskReadOnly[] {
    return this.robotsService.PastJobs;
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

  public get RobotsList() {
    // we want to only display the robots that aren't expired
    let robotsListTmp: iRobotCoderReadOnly[] = [];
    let robotsListFinal: iRobotCoderReadOnly[] = [];
    if (this.robotsService.RobotsList) {
      robotsListTmp = this.robotsService.RobotsList;
      let counter = 0;
      for (let i = 0; i < robotsListTmp.length; i++) {
        if (!robotsListTmp[i].isRetired) {
          robotsListFinal[counter] = robotsListTmp[i];
          counter += 1;
        }
      }
    }
    return robotsListFinal;
  }
  
  public get RobotsExpiredList() {
    // we want to only display the robots that aren't expired
    let robotsListTmp: iRobotCoderReadOnly[] = [];
    let robotsListExpiredFinal: iRobotCoderReadOnly[] = [];
    if (this.robotsService.RobotsExpiredList) {
      robotsListTmp = this.robotsService.RobotsExpiredList;
      let counter = 0;
      for (let i = 0; i < robotsListTmp.length; i++) {
        if (robotsListTmp[i].isRetired) {
          robotsListExpiredFinal[counter] = robotsListTmp[i];
          counter += 1;
        }
      }
    }
    return robotsListExpiredFinal;
  }
  
  public get IsSiteAdmin(): boolean {
    return this._reviewerIdentityServ.reviewerIdentity.isSiteAdmin;
  }
  public ChangingGetAllJobsOption() {
    this.GetAllJobsOption = !this.GetAllJobsOption;
    this.Refresh();
  }

  public get RobotSettings(): iRobotSettings {
    return this.robotsService.RobotSetting;
  }

  private _robotDescription: string = "";
  private _robotExpiredDescription: string = "";
  private _expiredRobotExpiryDate: string = "";
  private _robotExpiryDate: string = "";
  private _robotSelected: boolean = false;
  private _expiredRobotSelected: boolean = false;

  RobotChanged(event: Event) {
    let name = (event.target as HTMLOptionElement).value;
    if (name == "") {
      this._robotDescription = "";
      this._robotSelected = false;
    }
    else {
      for (var i = 0; i < this.RobotsList.length; i++) {
        if (this.RobotsList[i].robotName == name) {
          this._robotDescription = this.RobotsList[i].description;
          this._robotExpiryDate = this.RobotsList[i].retirementDate;
          this._robotSelected = true;
          i = this.RobotsList.length;
          //const description = this.RobotsList.find(f => f.robotName == name);
        }
      }
    }
  }
  RobotExpiredChanged(event: Event) {
    let name = (event.target as HTMLOptionElement).value;
    if (name == "") {
      this._robotExpiredDescription = "";
      this._expiredRobotSelected = false;
    }
    else {
      for (var i = 0; i < this.RobotsExpiredList.length; i++) {
        if (this.RobotsExpiredList[i].robotName == name) {
          this._robotExpiredDescription = this.RobotsExpiredList[i].description;
          this._expiredRobotExpiryDate = this.RobotsExpiredList[i].retirementDate;
          this._expiredRobotSelected = true;
          i = this.RobotsExpiredList.length;
          //const description = this.RobotsList.find(f => f.robotName == name);
        }
      }
    }
  }

  ExpiredRobotDescription() {
    return this._robotExpiredDescription;
  }
  RobotDescription() {
    return this._robotDescription;
  }
  ExpiredRobotExpiryDate() {
    return this._expiredRobotExpiryDate.substring(0, this._expiredRobotExpiryDate.indexOf("T"));
  }
  RobotExpiryDate() {
    return this._robotExpiryDate.substring(0, this._robotExpiryDate.indexOf("T"));;
  }
  RobotSelected() {
    return this._robotSelected;
  }
  ExpiredRobotSelected() {
    return this._expiredRobotSelected;
  }

  public basicMAGPanel: boolean = false;
  public ShowMAGPanel() {

    this.basicMAGPanel = !this.basicMAGPanel;
  }


  public rowCallback = (context: RowClassArgs) => {
    const row: RobotOpenAiTaskReadOnly = context.dataItem;
    if (row.success == false
      || row.status.indexOf("Failed") == 0
    ) {
      //console.log("error row", row.success, row.status.indexOf("Failed"));
      return { GPTJobsErrorRow: true };
    }
    else return { };
  };

  public DateTimeOptions: Intl.DateTimeFormatOptions = {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false
  };
 
  public JobDuration(line: RobotOpenAiTaskReadOnly): string {
    line.created.toLocaleDateString()
    const diff = line.JobDurationMs;
    if (diff > 60e3) return Math.floor(diff / 60e3) + " minutes";
    else return Math.floor(diff / 1e3) + " seconds";
  }

  public CodingToolText(line: RobotOpenAiTaskReadOnly): string {
    let res: string;
    const ind = this.reviewSetsService.ReviewSets.findIndex(f => f.reviewSetId == line.reviewSetId);
    if (ind == -1) {
      res = "[Unknown/deleted coding tool.]";
    }
    else {
      const tool = this.reviewSetsService.ReviewSets[ind];
      res = "Name: \"" + tool.set_name + "\". Id: <span class='alert-dark px-1 py-0'>" + tool.set_id.toString() + "</span>";
    }
    return res;
  }

  public AllToExcel() {
    this.SaveToExcel(this.robotsService.PastJobs, 'Robot Jobs');
  }

  public PageToExcel() {
    this.SaveToExcel(this.DataSourceJobs.data, 'Robot Jobs Page');
  }

  SaveToExcel(data: RobotOpenAiTaskReadOnly[], filename: string) {
    let res: DataForMultiSheetExcel[] = [];
    let Sheet1: any[] = [];
    let Sheet2: any[] = [];
    for (let row of data) {
      let rrow = {
        "Robot Job Id": row.robotApiCallId,
        "Credit Id": row.creditPurchaseId,
        "Review Id": row.reviewId,
        "Robot Name": row.robotName,
        "Job Type": row.JobType,
        "Job Owner Id": row.jobOwnerId,
        "Owner Name": row.jobOwner,
        "Cost": row.cost,
        "ReviewSet Id": row.reviewSetId,
        "Raw job details": row.rawCriteria,
        "Item IDs": row.itemIDsList.join(","),
        "Items Count": row.ItemsCount,
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
        "Current Item": row.currentItemId,
        "Input Tokens": row.inputTokens,
        "Output Tokens": row.outputTokens,
        "Always Code In The Robot Name": row.onlyCodeInTheRobotName,
        "Lock The Coding": row.lockTheCoding,
        "Use FullText Documents": row.useFullTextDocument        
      };
      Sheet1.push(rrow);
      for (const err of row.errors) {
        let ErrRow = {
          "Robot Job Id": row.robotApiCallId,
          "Affected ItemId": err.affectedItemId,
          "Error Message": err.errorMessage,
          "Stack Trace": err.stackTrace
        };
        Sheet2.push(ErrRow);
      }
    }
    res.push({ SheetName: "Jobs List", SheetData: Sheet1 });
    if (Sheet2.length > 0) res.push({ SheetName: "Errors List", SheetData: Sheet2 });
    this.excelService.exportAsMultiSheetExcelFile(res, filename);
  }


  Refresh() {
    this.robotsService.GetPastJobs(this.GetAllJobsOption);
  }
  public get PastJobs(): RobotOpenAiTaskReadOnly[] {
    return this.robotsService.PastJobs;
  }
  BackToMain() {
    this.router.navigate(['Main']);
  }

  ngOnDestroy() {

  }
}
