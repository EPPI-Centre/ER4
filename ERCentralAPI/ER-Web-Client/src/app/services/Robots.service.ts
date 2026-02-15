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
      this.subOpeningReview = this.EventEmitterService.OpeningNewReview.subscribe(() => this.Clear());
  }
  private clearSub: Subscription | null = null;
  private subOpeningReview: Subscription | null = null;
  public RobotSetting: iRobotSettings = this.DefaultRobotSetting;
  private get DefaultRobotSetting(): iRobotSettings {
    return {
      robotName: "",
      onlyCodeInTheRobotName: true,
      lockTheCoding: true,
      useFullTextDocument: false,
      rememberTheseChoices: false,
    };
  }
  public ShowSettingsInBatchPanel: boolean = true;
  public RobotInvestigateResults: iRobotInvestigate[] = [];
  public RobotsList: iRobotCoderReadOnly[] = [];

  public CurrentQueue: iRobotOpenAiTaskReadOnly[] = [];
  public PastJobs: RobotOpenAiTaskReadOnly[] = [];

  public GetRobotsList(): Promise<void> {
    this.CurrentQueue = [];
    this._BusyMethods.push("GetRobotsList");
    return lastValueFrom(this._httpC.get<iRobotCoderReadOnly[]>(this._baseUrl + 'api/Robots/GetRobotsList'))
      .then((res) => {
        this.RobotsList = res;
        this.RemoveBusy("GetRobotsList");
      },
        (err) => {
          this.RemoveBusy("GetRobotsList");
          this.modalService.GenericError(err);
        })
      .catch((err) => {
        this.RemoveBusy("GetRobotsList");
        this.modalService.GenericError(err);
      });
  }

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
  public GetPastJobs(AllJobs: boolean): Promise<void> {
    let endpoint = 'api/Robots/GetPastJobs';
    if (AllJobs) endpoint = 'api/Robots/GetAllPastJobs';
    this.PastJobs = [];
    this._BusyMethods.push("GetPastJobs");
    return lastValueFrom(this._httpC.get<iRobotOpenAiTaskReadOnly[]>(this._baseUrl + endpoint))
      .then((res) => {
        for (let iJob of res) {
          this.PastJobs.push(new RobotOpenAiTaskReadOnly(iJob));
        }
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

  public CancelRobotOpenAIBatch(JobId: number): Promise<boolean> {
    this._BusyMethods.push("CancelRobotOpenAIBatch");
    let body = JSON.stringify({ Value: JobId });
    return lastValueFrom(this._httpC.post<iRobotOpenAiCancelQueuedBatchJobCommand>(this._baseUrl + 'api/Robots/CancelRobotOpenAIBatch', body))
      .then((res) => {
        this.RemoveBusy("CancelRobotOpenAIBatch");
        if (res.success == false) {
          this.modalService.GenericErrorMessage("The job could <strong>not be Cancelled</strong><br>This usually happens because the job had already started or ended.");
        }
        this.GetCurrentQueue();
        return true;
      },
        (err) => {
          this.RemoveBusy("CancelRobotOpenAIBatch");
          this.modalService.GenericError(err);
          this.GetCurrentQueue();
          return false;
        })
      .catch((err) => {
        this.RemoveBusy("CancelRobotOpenAIBatch");
        this.modalService.GenericError(err);
        this.GetCurrentQueue();
        return false;
      });
  }

  public EnqueueRobotOpenAIBatchJobEvaluationCommand(cmd: iRobotOpenAiQueueBatchJobEvaluationCommand): Promise<boolean> {

    this._BusyMethods.push("EnqueueRobotOpenAIBatchJobEvaluationCommand");
    return lastValueFrom(this._httpC.post<iRobotOpenAiQueueBatchJobEvaluationCommand>(this._baseUrl + 'api/Robots/EnqueueRobotOpenAIBatchJobEvaluation', cmd))
      .then((res) => {
        this.RemoveBusy("EnqueueRobotOpenAIBatchJobEvaluationCommand");
        if (res.returnMessage.includes("Error")) {
          this.modalService.GenericErrorMessage("The batch submission failed with message: " + res.returnMessage);
          return false;
        }
        return true;
      },
        (err) => {
          this.RemoveBusy("EnqueueRobotOpenAIBatchJobEvaluationCommand");
          this.modalService.GenericError(err);
          cmd.returnMessage = "Error";
          return false;
        })
      .catch((err) => {
        this.RemoveBusy("EnqueueRobotOpenAIBatchJobEvaluationCommand");
        this.modalService.GenericError(err);
        cmd.returnMessage = "Error";
        return false;
      });
  }

  public CancelRobotOpenAIBatchEvaluation(JobId: number): Promise<boolean> {
    this._BusyMethods.push("CancelRobotOpenAIBatchEvaluation");
    let body = JSON.stringify({ Value: JobId });
    return lastValueFrom(this._httpC.post<iRobotOpenAiCancelQueuedBatchJobEvaluationCommand>(this._baseUrl + 'api/Robots/CancelRobotOpenAIBatchEvaluation', body))
      .then((res) => {
        this.RemoveBusy("CancelRobotOpenAIBatchEvaluation");
        if (res.success == false) {
          this.modalService.GenericErrorMessage("The job could <strong>not be Cancelled</strong><br>This usually happens because the job had already started or ended.");
        }
        this.GetCurrentQueue();
        return true;
      },
        (err) => {
          this.RemoveBusy("CancelRobotOpenAIBatchEvaluation");
          this.modalService.GenericError(err);
          this.GetCurrentQueue();
          return false;
        })
      .catch((err) => {
        this.RemoveBusy("CancelRobotOpenAIBatchEvaluation");
        this.modalService.GenericError(err);
        this.GetCurrentQueue();
        return false;
      });
  }

  private _RobotOpenAiPromptEvaluationList: RobotOpenAiPromptEvaluation[] = [];
  public get RobotOpenAiPromptEvaluationList(): RobotOpenAiPromptEvaluation[] {
    return this._RobotOpenAiPromptEvaluationList;
  }
  public set RobotOpenAiPromptEvaluationList(robotOpenAiPromptEvaluationList: RobotOpenAiPromptEvaluation[]) {
    this._RobotOpenAiPromptEvaluationList = robotOpenAiPromptEvaluationList;
  }
  private _currentRobotOpenAiPromptEvaluationDataList: RobotOpenAiPromptEvaluationData[] = [];
  public get CurrentRobotOpenAiPromptEvaluationDataList(): RobotOpenAiPromptEvaluationData[] {
    return this._currentRobotOpenAiPromptEvaluationDataList;
  }
  public set CurrentRobotOpenAiPromptEvaluationDataList(robotOpenAiPromptEvaluationDataList: RobotOpenAiPromptEvaluationData[]) {
    this._currentRobotOpenAiPromptEvaluationDataList = robotOpenAiPromptEvaluationDataList;
  }

  public FetchRobotOpenAiPromptEvaluationList() {
    this._BusyMethods.push("FetchRobotOpenAiPromptEvaluationList");
    lastValueFrom(this._httpC.get<RobotOpenAiPromptEvaluation[]>(this._baseUrl + 'api/Robots/FetchRobotOpenAiPromptEvaluationList'))
      .then(result => {
        this.RemoveBusy("FetchRobotOpenAiPromptEvaluationList");
        if (result != null) {
          this.RobotOpenAiPromptEvaluationList = result;

          console.log('this.FetchRobotOpenAiPromptEvaluationList', this.RobotOpenAiPromptEvaluationList);
        }
      },
        error => {
          this.RemoveBusy("FetchRobotOpenAiPromptEvaluationList");
          this.modalService.GenericError(error);
        }).catch(
          (caught) => {
            this.modalService.GenericError(caught);
            this.RemoveBusy("FetchRobotOpenAiPromptEvaluationList");
          });
  }
  public FetchRobotOpenAiPromptEvaluationDataList(openAiPromptEvaluationId: string) {
    this._BusyMethods.push("FetchRobotOpenAiPromptEvaluationDataList");
    //let body = JSON.stringify(OpenAiPromptEvaluationId);

    const params = { openAiPromptEvaluationId };

    //lastValueFrom(this._httpC.get<RobotOpenAiPromptEvaluationData[]>(this._baseUrl + 'api/Robots/FetchRobotOpenAiPromptEvaluationDataList', {params}))
    lastValueFrom(this._httpC.post<RobotOpenAiPromptEvaluationData[]>(this._baseUrl + 'api/Robots/FetchRobotOpenAiPromptEvaluationDataList', openAiPromptEvaluationId,
      { headers: { 'Content-Type': 'application/json' } }))
      .then((result) => {
        this.RemoveBusy("FetchRobotOpenAiPromptEvaluationDataList");
        if (result != null) {
          this._currentRobotOpenAiPromptEvaluationDataList = result;

          console.log('this.FetchRobotOpenAiPromptEvaluationDataList', this._currentRobotOpenAiPromptEvaluationDataList);
        }
      },
        error => {
          this.RemoveBusy("FetchRobotOpenAiPromptEvaluationDataList");
          this.modalService.GenericError(error);
        }).catch(
          (caught) => {
            this.modalService.GenericError(caught);
            this.RemoveBusy("FetchRobotOpenAiPromptEvaluationDataList");
          });
  }

  public DeleteRobotOpenAiPromptEvaluation(OpenAiPromptEvaluationId: string) {
    this._BusyMethods.push("DeleteRobotOpenAiPromptEvaluation");
    let body = JSON.stringify(OpenAiPromptEvaluationId);
    return lastValueFrom(this._httpC.post<RobotOpenAiPromptEvaluation>(this._baseUrl + 'api/Robots/DeleteRobotOpenAiPromptEvaluation',
      body))
      .then(
        (result) => {
          this.RemoveBusy("DeleteRobotOpenAiPromptEvaluation");
          this.FetchRobotOpenAiPromptEvaluationList();
          return true;
        }, error => {
          this.modalService.GenericError(error);
          //this.modalService.GenericErrorMessage("There was an error getting the RobotOpenAiPromptEvaluationList. Please contact eppisupport@ucl.ac.uk");
          this.RemoveBusy("DeleteRobotOpenAiPromptEvaluation");
          return false;
        }
      );
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
    let res = "<H2>Investigate (" + InvComm.robotName + ") Report</H2>";
    res += "\r\n" + "<TABLE class='ItemsTable'><tr><th>Query:</th><td colspan='5'>" + InvComm.queryForRobot + "</td></tr>";
    res += "\r\n" + "<tr><th>Item IDs used:</th><td colspan='3'>" + InvComm.returnItemIdList + "</td><td colspan='2'><strong>Robot:</strong> " + InvComm.robotName + "</td></tr>";
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
    this.RobotsList = [];
    this.PastJobs = [];
    this.RobotInvestigateResults = [];
    this.RobotSetting = this.DefaultRobotSetting;
    this.ShowSettingsInBatchPanel = true;
  }
  ngOnDestroy() {
    this.Clear();
    //console.log("Destroy RobotsService");
    if (this.clearSub != null) this.clearSub.unsubscribe();
    if (this.subOpeningReview != null) this.subOpeningReview.unsubscribe();
    
  }
}
export interface iRobotOpenAICommand {
  robotName: string;
  reviewSetId: number;
  itemDocumentId: number;
  itemId: number;
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  useFullTextDocument: boolean;
  returnMessage: string;
}
export interface iRobotOpenAiQueueBatchJobCommand {
  robotName: string;
  reviewSetId: number;
  criteria: string;
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  useFullTextDocument: boolean;
  returnMessage: string;
}

export interface iRobotOpenAiQueueBatchJobEvaluationCommand {
  evaluationName: string;
  robotName: string;
  reviewSetId: number;
  reviewSetHtml: string;
  goldStandardAttributeId: number;
  useFullTextDocument: boolean;
  returnMessage: string;
  nIterations: number;
}

export interface iRobotSettings {
  robotName: string;
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  useFullTextDocument: boolean;
  rememberTheseChoices: boolean;
}
export interface iRobotInvestigate {
  robotName: string;
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
  robotName: string;
  jobOwnerId: number;
  jobOwner: string;
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
  robot: iRobotCoderReadOnly;
}

export class RobotOpenAiTaskReadOnly {
  constructor(data?: iRobotOpenAiTaskReadOnly) {
    if (data) {
      this.robotApiCallId = data.robotApiCallId;
      this.creditPurchaseId = data.creditPurchaseId;
      this.reviewId = data.reviewId;
      this.robotId = data.robotId;
      this.robotName = data.robotName;
      this.jobOwnerId = data.jobOwnerId;
      this.jobOwner = data.jobOwner;
      this.reviewSetId = data.reviewSetId;
      this.rawCriteria = data.rawCriteria;
      this.itemIDsList = data.itemIDsList;
      this.status = data.status;
      this.currentItemId = data.currentItemId;
      this.created = new Date(data.created);
      this.updated = new Date(data.updated);
      this.success = data.success;
      this.inputTokens = data.inputTokens;
      this.outputTokens = data.outputTokens;
      this.cost = data.cost;
      this.onlyCodeInTheRobotName = data.onlyCodeInTheRobotName;
      this.lockTheCoding = data.lockTheCoding;
      this.useFullTextDocument = data.useFullTextDocument;
      this.robotContactId = data.robotContactId;
      this.errors = data.errors;
      this.robot = data.robot;

      const d1: any = this.created;
      const d2: any = this.updated;
      this.JobDurationMs = d2 - d1;
      if (data.rawCriteria == "Robot investigate single query") this.JobType = "Investigate";
      else if (data.useFullTextDocument == true) this.JobType = "Coding (full text)";
      else this.JobType = "Coding";

      if (this.JobType.startsWith("Coding")) this.ItemsCount = this.itemIDsList.length;
      else this.ItemsCount = -1;

    } else {
      this.robotApiCallId = 0;
      this.creditPurchaseId = 0;
      this.reviewId = 0;
      this.robotId = 0;
      this.robotName = "";
      this.jobOwnerId = 0;
      this.jobOwner = "";
      this.reviewSetId = 0;
      this.rawCriteria = "";
      this.itemIDsList = [];
      this.status = "";
      this.currentItemId = 0;
      this.created = new Date();
      this.updated = this.created;
      this.success = false;
      this.inputTokens = 0;
      this.outputTokens = 0;
      this.cost = 0;
      this.onlyCodeInTheRobotName = true;
      this.lockTheCoding = true;
      this.useFullTextDocument = false;
      this.robotContactId = 0;
      this.errors = [];
      this.robot = {
        robotId: -1,
        robotContactId: -1,
        robotName: "",
        isPublic: false,
        topP: 0,
        temperature: 0,
        frequencyPenalty:0,
        presencePenalty: 0,
        description: "",
        retirementDate: "",
        isRetired: true,
        inputTokenCostPerMillion: 0,
        outputTokenCostPerMillion: 0
      };

      this.JobDurationMs = 0;
      this.JobType = "N/A";
      this.ItemsCount = -1;
    }
  }
  robotApiCallId: number;
  creditPurchaseId: number;
  reviewId: number;
  robotId: number;
  robotName: string;
  jobOwnerId: number;
  jobOwner: string;
  reviewSetId: number;
  rawCriteria: string;
  itemIDsList: number[];
  status: string;
  currentItemId: number;
  created: Date;
  updated: Date;
  success: boolean;
  inputTokens: number;
  outputTokens: number;
  cost: number;
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  useFullTextDocument: boolean;
  robotContactId: number;
  errors: iRobotOpenAiTaskError[];
  robot: iRobotCoderReadOnly;

  JobDurationMs: number;
  JobType: string;
  ItemsCount: number;
}
export interface iRobotCoderReadOnly {
  robotId: number;
  robotContactId: number;
  robotName: string;
  isPublic: boolean;
  topP: number;
  temperature: number;
  frequencyPenalty: number;
  presencePenalty: number;
  description: string;
  retirementDate: string;
  isRetired: boolean;
  inputTokenCostPerMillion: number;
  outputTokenCostPerMillion: number;
}

export interface iRobotOpenAiTaskError {
  affectedItemId: number;
  errorMessage: string;
  stackTrace: string;
}
export interface iRobotOpenAiCancelQueuedBatchJobCommand {
  jobId: number;
  success: boolean;
}

export interface iRobotOpenAiCancelQueuedBatchJobEvaluationCommand {
  jobId: number;
  success: boolean;
}

export interface RobotOpenAiPromptEvaluation {
  openAiPromptEvaluationId: string;
  title: string;
  robotName: string;
  reviewSetHtml: string;
  usePdfs: boolean;
  nIterations: number;
  nRecords: number;
  nCodes: number;
  tp: number;
  tn: number;
  fp: number;
  fn: number;
  whenRun: Date;
}
export interface RobotOpenAiPromptEvaluationData {
  openaiPromptEvaluationId: number;
  iteration: number;
  itemId: number;
  attributeId: number;
  additionalText: string;
  goldStandard: boolean;
}
