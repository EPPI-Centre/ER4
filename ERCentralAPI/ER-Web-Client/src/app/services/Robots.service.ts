import { Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ConfigService } from './config.service';
import { lastValueFrom } from 'rxjs';

@Injectable({
    providedIn: 'root',
})

export class RobotsService extends BusyAwareService {
	   
    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
      configService: ConfigService
    ) {
      super(configService);
    }

  public RobotSetting: iRobotSettings = {
    onlyCodeInTheRobotName: true,
    lockTheCoding: true,
    rememberTheseChoices: false
  };

  public CurrentQueue: iRobotOpenAiTaskReadOnly[] = [];

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

  public RunRobotOpenAICommand(cmd: iRobotOpenAICommand): Promise<iRobotOpenAICommand> {

    this._BusyMethods.push("RunRobotOpenAICommand");
    return lastValueFrom(this._httpC.post<iRobotOpenAICommand>(this._baseUrl + 'api/Robots/RunRobotOpenAICommand', cmd))
      .then((res) => {
        this.RemoveBusy("RunRobotOpenAICommand");
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
}
export interface iRobotOpenAICommand {
  reviewSetId: number;
  itemDocumentId: number;
  itemId: number;
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  returnMessage: string;
}
export interface iRobotOpenAiQueueBatchJobCommand {
  reviewSetId: number;
  criteria: string;
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  returnMessage: string;
}

export interface iRobotSettings {
  onlyCodeInTheRobotName: boolean;
  lockTheCoding: boolean;
  rememberTheseChoices: boolean;
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
  robotContactId: number;
}
