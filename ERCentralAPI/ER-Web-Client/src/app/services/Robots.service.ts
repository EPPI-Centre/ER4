import { Inject, Injectable, EventEmitter, Output, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ConfigService } from './config.service';
import { lastValueFrom, Subscription } from 'rxjs';
import { EventEmitterService } from './EventEmitter.service';
import { ReviewSetsService } from './ReviewSets.service';

@Injectable({
    providedIn: 'root',
})

export class RobotsService extends BusyAwareService implements OnDestroy {
	   
    constructor(
        private _httpC: HttpClient,
      private modalService: ModalService,
      private _reviewSetsService: ReviewSetsService,
      private EventEmitterService: EventEmitterService,
      configService: ConfigService
    ) {
      super(configService);
      this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
  }
  private clearSub: Subscription | null = null;
  public RobotSetting: iRobotSettings = {
    onlyCodeInTheRobotName: true,
    lockTheCoding: true,
    useFullTextDocument: false,
    rememberTheseChoices: false
  };

  public RobotInvestigateResults: iRobotInvestigate[] = [];

  public CurrentQueue: iRobotOpenAiTaskReadOnly[] = [];
  public PastJobs: iRobotOpenAiTaskReadOnly[] = [];

  public GetCurrentQueue(): Promise<void> {
    this.CurrentQueue = [];
    this._BusyMethods.push("GetCurrentQueue");
    return lastValueFrom(this._httpC.get<iRobotOpenAiTaskReadOnly[]>(this._baseUrl + 'api/Robots/GetCurrentJobsQueue'))
      .then((res) => {
        this.CurrentQueue = res;
        this.RemoveBusy("GetCurrentQueue");
      },
        (err) => {
          this.RemoveBusy("GetCurrentQueue");
          this.modalService.GenericError(err);
        })
      .catch((err) => {
        this.RemoveBusy("GetCurrentQueue");
        this.modalService.GenericError(err);
      });
  }
  public GetPastJobs(): Promise<void> {
    this.PastJobs = [];
    this._BusyMethods.push("GetPastJobs");
    return lastValueFrom(this._httpC.get<iRobotOpenAiTaskReadOnly[]>(this._baseUrl + 'api/Robots/GetPastJobs'))
      .then((res) => {
        this.PastJobs = res;
        this.RemoveBusy("GetPastJobs");
      },
        (err) => {
          this.RemoveBusy("GetPastJobs");
          this.modalService.GenericError(err);
        })
      .catch((err) => {
        this.RemoveBusy("GetPastJobs");
        this.modalService.GenericError(err);
      });
  }

  public RunRobotOpenAICommand(cmd: iRobotOpenAICommand): Promise<iRobotOpenAICommand> {

    this._BusyMethods.push("RunRobotOpenAICommand");
    return lastValueFrom(this._httpC.post<iRobotOpenAICommand>(this._baseUrl + 'api/Robots/RunRobotOpenAICommand', cmd))
      .then((res) => {
        this.RemoveBusy("RunRobotOpenAICommand");
        if (res.returnMessage.toLowerCase().indexOf('error') == 0) {
          this.modalService.GenericErrorMessage(res.returnMessage);
          res.returnMessage = "Error";
        }
        return res;
      },
        (err) => {
          this.RemoveBusy("RunRobotOpenAICommand");
          this.modalService.GenericError(err);
          cmd.returnMessage = "Error";
          return cmd;
        })
      .catch((err) => {
        this.RemoveBusy("RunRobotOpenAICommand");
        this.modalService.GenericError(err);
        cmd.returnMessage = "Error";
        return cmd;
      });
  }

  public RunRobotInvestigateCommand(cmd: iRobotInvestigate): Promise<iRobotInvestigate> {
    this._BusyMethods.push("RunRobotOpenAICommand");
    return lastValueFrom(this._httpC.post<iRobotInvestigate>(this._baseUrl + 'api/Robots/RunRobotInvestigateCommand', cmd))
      .then((res) => {
        this.RemoveBusy("RunRobotOpenAICommand");
        if (res.returnMessage.toLowerCase().indexOf('error') == 0) {
          this.modalService.GenericErrorMessage(res.returnMessage);
          res.returnMessage = "Error";
        } else {
          this.RobotInvestigateResults.push(res);
        }
        return res;
      },
        (err) => {
          this.RemoveBusy("RunRobotOpenAICommand");
          this.modalService.GenericError(err);
          cmd.returnMessage = "Error";
          return cmd;
        })
      .catch((err) => {
        this.RemoveBusy("RunRobotOpenAICommand");
        this.modalService.GenericError(err);
        cmd.returnMessage = "Error";
        return cmd;
      });
  }

  public EnqueueRobotOpenAIBatchCommand(cmd: iRobotOpenAiQueueBatchJobCommand): Promise<boolean> {

    this._BusyMethods.push("EnqueueRobotOpenAIBatchCommand");
    return lastValueFrom(this._httpC.post<iRobotOpenAiQueueBatchJobCommand>(this._baseUrl + 'api/Robots/EnqueueRobotOpenAIBatch', cmd))
      .then((res) => {
        this.RemoveBusy("EnqueueRobotOpenAIBatchCommand");
        if (res.returnMessage.includes("Error")) {
          this.modalService.GenericErrorMessage("The batch submission failed with message: " + res.returnMessage);
          return false;
        }
        return true;
      },
        (err) => {
          this.RemoveBusy("EnqueueRobotOpenAIBatchCommand");
          this.modalService.GenericError(err);
          cmd.returnMessage = "Error";
          return false;
        })
      .catch((err) => {
        this.RemoveBusy("EnqueueRobotOpenAIBatchCommand");
        this.modalService.GenericError(err);
        cmd.returnMessage = "Error";
        return false;
      });
  }
  public TextFromAttributeId(AttId: number): string {
    const res = this._reviewSetsService.FindAttributeById(AttId);
    if (res) {
      return res.attribute_name + " (ID: " + AttId.toString() + ")";
    }
    return "N/A";
  }
  public TextFromInvestigateTextOption(OptionVal: string): string {
    if (OptionVal == "title") return "Title and Abstract";
    else if (OptionVal == "info") return "'Info' boxes";
    else if (OptionVal == "highlighted") return "Highlighted text from documents";
    else return "N/A";
  }
  public InvestigateReportHTML(InvComm: iRobotInvestigate) :string {
    let res = "<H2>Investigate (using GPT) Report</H2>";
    res += "\r\n" + "<TABLE class='ItemsTable'><tr><th>Query:</th><td colspan='5'>" + InvComm.queryForRobot + "</td></tr>";
    res += "\r\n" + "<tr><th>Item IDs used:</th><td colspan='5'>" + InvComm.returnItemIdList + "</td></tr>";
    if (InvComm.getTextFrom != 'title') {
      res += "\r\n" + "<tr><td colspan='2'><strong>Grab text from: </strong>" + this.TextFromInvestigateTextOption(InvComm.getTextFrom) + "</td>";
      res += "\r\n" + "<td colspan='2'><strong>Using this code: </strong>" + this.TextFromAttributeId(InvComm.textFromThisAttribute) + "</td>";
      res += "\r\n" + "<td colspan='2'><strong>Sample Size: </strong>" + InvComm.sampleSize + "</td></tr></TABLE>";
    }
    else {
      res += "\r\n" + "<tr><td colspan='3'><strong>Grab text from: </strong>" + this.TextFromInvestigateTextOption(InvComm.getTextFrom) + "</td>";
      res += "\r\n" + "<td colspan='3'><strong>Sample Size: </strong>" + InvComm.sampleSize + "</td></tr></TABLE>\r\n";
    }
    res += "<H4>Result:</H4>"
    res += "<div style='border:1px solid black; margin:5px; padding:5px;'>" + InvComm.returnResultText + "</div>";
    return res;
  }
  public Clear() {
    this.CurrentQueue = [];
    this.PastJobs = [];
    this.RobotInvestigateResults = [];
  }
  ngOnDestroy() {
    this.Clear();
    //console.log("Destroy RobotsService");
    if (this.clearSub != null) this.clearSub.unsubscribe();
  }
}
export interface iRobotOpenAICommand {
  reviewSetId: number;
  itemDocumentId: number;
  itemId: number;
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  useFullTextDocument: boolean;
  returnMessage: string;
}
export interface iRobotOpenAiQueueBatchJobCommand {
  reviewSetId: number;
  criteria: string;
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  useFullTextDocument: boolean;
  returnMessage: string;
}

export interface iRobotSettings {
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  useFullTextDocument: boolean;
  rememberTheseChoices: boolean;
}
export interface iRobotInvestigate {
  queryForRobot: string;
  getTextFrom: string;
  itemsWithThisAttribute: number;
  textFromThisAttribute: number;
  sampleSize: number;
  returnMessage: string;
  returnResultText: string;
  returnItemIdList: string;
}
export interface iRobotOpenAiTaskReadOnly {
  robotApiCallId: number;
  creditPurchaseId: number;
  reviewId: number;
  robotId: number;
  jobOwnerId: number;
  reviewSetId: number;
  rawCriteria: string;
  itemIDsList: number[];
  status: string;
  currentItemId: number;
  created: string;
  updated: string;
  success: boolean;
  inputTokens: number;
  outputTokens: number;
  cost: number;
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  useFullTextDocument: boolean;
  robotContactId: number;
  errors: iRobotOpenAiTaskError[];
}
export interface iRobotOpenAiTaskError {
  affectedItemId: number;
  errorMessage: string;
  stackTrace: string;
}
