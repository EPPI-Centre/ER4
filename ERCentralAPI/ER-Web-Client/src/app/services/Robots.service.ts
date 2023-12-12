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
}
export interface iRobotOpenAICommand {
  reviewSetId: number;
  itemDocumentId: number;
  itemId: number;
  returnMessage: string;
} 
