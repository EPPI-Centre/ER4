import {  Inject, Injectable, OnDestroy} from '@angular/core';
import { HttpClient, HttpHeaders   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewInfo, ReviewInfoService } from './ReviewInfo.service';
import {  lastValueFrom, Subscription } from 'rxjs';
import { NotificationService } from '@progress/kendo-angular-notification';
import { EventEmitterService } from './EventEmitter.service';
import { ReviewerIdentityService } from './revieweridentity.service';
import { ConfigService } from './config.service';

@Injectable({

    providedIn: 'root',

})

export class ClassifierService extends BusyAwareService implements OnDestroy {


    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private _reviewInfoService: ReviewInfoService,
		private notificationService: NotificationService,
		private EventEmitterService: EventEmitterService,
		private _ReviewerIdentityServ: ReviewerIdentityService,
      configService: ConfigService
        ) {
      super(configService);
		//console.log("On create ClassifierService");
		this.clearSub = this.EventEmitterService.PleaseClearYourDataAndState.subscribe(() => { this.Clear(); });
	}
	ngOnDestroy() {
		console.log("Destroy MAGRelatedRunsService");
		if (this.clearSub != null) this.clearSub.unsubscribe();
	}
  private clearSub: Subscription | null = null;

  private _PriorityScreeningSimulationList: PriorityScreeningSimulation[] = [];
  public get PriorityScreeningSimulationList(): PriorityScreeningSimulation[] {
    return this._PriorityScreeningSimulationList;
  }
  public set PriorityScreeningSimulationList(priorityScreeningSimulationList: PriorityScreeningSimulation[]) {
    this._PriorityScreeningSimulationList = priorityScreeningSimulationList;
  }

  //private _PriorityScreeningSimulation: PriorityScreeningSimulation | null = null;
  //public get PriorityScreeningSimulation(): PriorityScreeningSimulation | null {
  //  return this._PriorityScreeningSimulation;
  //}
  //public set PriorityScreeningSimulation(priorityScreeningSimulation: PriorityScreeningSimulation | null) {
  //  this._PriorityScreeningSimulation = priorityScreeningSimulation;
  //}

  private _currentSimulation: PriorityScreeningSimulation | null = null;
  
  public get PriorityScreeningSimulationResults(): string | null {
    if (this._currentSimulation == null) return null;
    return this._currentSimulation.blob;
  } public get PriorityScreeningSimulationName(): string {
    if (this._currentSimulation == null) return "";
    return this._currentSimulation.simulationName;
  }

	private _CurrentUserId4ClassifierContactModelList: number = 0;
	private _ClassifierContactModelList: ClassifierModel[] = [];
	public get ClassifierContactAllModelList(): ClassifierModel[] {
		return this._ClassifierContactModelList;
	}
	public get CurrentUserId4ClassifierContactModelList(): number {
		return this._CurrentUserId4ClassifierContactModelList;
	}
	public set ClassifierContactAllModelList(classifierContactModelList: ClassifierModel[]) {
		this._ClassifierContactModelList = classifierContactModelList;
	}
	private _ClassifierModelList: ClassifierModel[] = [];
	//@Output() searchesChanged = new EventEmitter();
	//public crit: CriteriaSearch = new CriteriaSearch();
	public modelToBeDeleted: number = 0;

	public get ClassifierModelCurrentReviewList(): ClassifierModel[] {
		return this._ClassifierModelList;
	}

	public set ClassifierModelCurrentReviewList(models: ClassifierModel[]) {
		this._ClassifierModelList = models;
	}
	public GetClassifierContactModelList(): void {
		if ((this.ClassifierContactAllModelList.length == 0
			&& (
				this.CurrentUserId4ClassifierContactModelList < 1
				|| this.CurrentUserId4ClassifierContactModelList != this._ReviewerIdentityServ.reviewerIdentity.userId
			)) || (this.CurrentUserId4ClassifierContactModelList < 1
				|| this.CurrentUserId4ClassifierContactModelList != this._ReviewerIdentityServ.reviewerIdentity.userId)) {
			//only fetch this if it's empty or if it contains a list of models that belongs to someone else. 
			//the second checks on userId prevent leaking when one user logs off, another logs in and finds the list belonging to another user, very ugly, but should work.
			//wait 100ms and then get this list, I don't like sending many server requests all concurrent
			this.FetchClassifierContactModelList(this._ReviewerIdentityServ.reviewerIdentity.userId);
		}
	}
	public FetchClassifierContactModelList(UserId: number) {
		this._BusyMethods.push("FetchClassifierContactModelList");
		this._CurrentUserId4ClassifierContactModelList = UserId;
		lastValueFrom(this._httpC.get<ClassifierModel[]>(this._baseUrl + 'api/MagClassifierContact/FetchClassifierContactList')).then(
			result => {
				this.RemoveBusy("FetchClassifierContactModelList");
				if (result != null) {			
					this.ClassifierContactAllModelList = result; 
					
					this.ClassifierModelCurrentReviewList = this.ClassifierContactAllModelList.filter(x => x.reviewId == this._reviewInfoService.ReviewInfo.reviewId);
					
					console.log('this.ClassifierModelCurrentReviewList', this.ClassifierModelCurrentReviewList);
				}
			},
				error => {
					this.RemoveBusy("FetchClassifierContactModelList");
					this.modalService.GenericError(error);
				}).catch (
          (caught) => {
            this.modalService.GenericError(caught);
            this.RemoveBusy("FetchClassifierContactModelList");
				});
	}
	public Delete(modelId: number): Promise<boolean> {

		let MVCcmd: MVCClassifierCommand = new MVCClassifierCommand();

		MVCcmd._title = '';
		MVCcmd._attributeIdOn = -1;
		MVCcmd._attributeIdNotOn = -1;
		MVCcmd._attributeIdClassifyTo = -1;
		MVCcmd._modelId = modelId;
		MVCcmd.revInfo = this._reviewInfoService.ReviewInfo;

		this._BusyMethods.push("DeleteModel");

    return lastValueFrom(this._httpC.post<MVCClassifierCommand>(this._baseUrl + 'api/Classifier/DeleteModel',
			MVCcmd)
			).then(
				(result: MVCClassifierCommand) => {
					this.RemoveBusy("DeleteModel");
					if (result != null && result.returnMessage == 'Success') {
						//all is well!
						//we'll let the component decide when to refresh data...
						//this.Fetch(); 
						return true;
					}
					else {
						this.modalService.GenericErrorMessage("Deletion of model failed. Model id:" + MVCcmd._modelId + ". Failure message: " + result.returnMessage
							+ ". If the problem persists, please contact EPPISupport");
						return false;
					}
				}, error => {
					this.RemoveBusy("DeleteModel");
					console.log("Delete model Error: " + error);
					this.modalService.GenericError(error);
					return false;
				}
			).catch(
				(caught) => {
					this.RemoveBusy("DeleteModel");
					this.modalService.GenericErrorMessage("Deletion of model failed. Model id:" + MVCcmd._modelId
						+ ". If the problem persists, please contact EPPISupport");
					console.log("Catch in DeleteModel", caught);
					return false;
				}
			);
	}

	IamVerySorryRefresh() {

		this.FetchClassifierContactModelList(this._ReviewerIdentityServ.reviewerIdentity.userId);

	}

	showBuildModelMessage(notifyMsg: string) {
    if (notifyMsg.startsWith("Already running")) {
      this.modalService.GenericErrorMessage("The task did not start, as another job of this type is already running in this review.\r\nPlease check the Review Jobs log and try again.");
    }
    else if (notifyMsg.startsWith("Insufficient Data")) {
      this.modalService.GenericErrorMessage(notifyMsg);
    }
    else {
      this.notificationService.show({
        content: notifyMsg,
        animation: { type: 'slide', duration: 400 },
        position: { horizontal: 'center', vertical: 'top' },
        type: { style: "info", icon: true },
        closable: true
      });
    }
	}

	CreateAsync(title: string, attrOn: number, attrNotOn: number, classifierId: number): Promise<void> {

    let MVCcmd: MVCClassifierCommand = new MVCClassifierCommand();

    MVCcmd._attributeIdClassifyTo = 0;
    MVCcmd._attributeIdNotOn = attrNotOn;
    MVCcmd._attributeIdOn = attrOn;
    MVCcmd._sourceId = -1;
    MVCcmd._title = title;
    MVCcmd.revInfo = this._reviewInfoService.ReviewInfo;
    MVCcmd._classifierId = classifierId;

    this._BusyMethods.push("CreateAsync");
    return lastValueFrom(this._httpC.post<MVCClassifierCommand>(this._baseUrl + 'api/Classifier/Classifier',
      MVCcmd)).then(
        result => {
          if (result.returnMessage == '') {
            this.showBuildModelMessage('request was submitted');
          } else {
            this.showBuildModelMessage(result.returnMessage);
          }
          this.IamVerySorryRefresh();
          this.RemoveBusy("CreateAsync");
        },
        error => {
          this.RemoveBusy("CreateAsync");
          this.modalService.GenericError(error);
        }).catch((caught) => {
          this.modalService.GenericError(caught);
          this.RemoveBusy("CreateAsync");
        });
	}
	
  Apply(modeltitle: string, AttributeId: number, ModelId: number, SourceId: number): Promise<string | boolean> {
    let MVCcmd: MVCClassifierCommand = new MVCClassifierCommand();
    MVCcmd._title = modeltitle;
    if (ModelId > 0) {//the model is a custom one
      let ind = this.ClassifierContactAllModelList.findIndex(f => f.modelId == ModelId);
      if (ind == -1) return new Promise(() => { return false; });//no point trying to apply a model we can't find the model list...

      if (this._reviewInfoService.ReviewInfo.reviewId != this.ClassifierContactAllModelList[ind].reviewId) {
        //we're recording the ReviewID only if the model was built in another review
        MVCcmd._title = modeltitle + " (Review ID: " + this.ClassifierContactAllModelList[ind].reviewId + ")";
      }
    }
    MVCcmd._attributeIdOn = -1;
    MVCcmd._attributeIdNotOn = -1;
    MVCcmd._attributeIdClassifyTo = AttributeId;
    MVCcmd._classifierId = ModelId;
    MVCcmd._sourceId = SourceId;
    MVCcmd.revInfo = this._reviewInfoService.ReviewInfo;

    this._BusyMethods.push("Apply");

    //const headers = new HttpHeaders().set('Content-Type', 'application/json; charset=utf-8');

    return lastValueFrom(this._httpC.post<MVCClassifierCommand>(this._baseUrl + 'api/Classifier/ApplyClassifier'//'api/Classifier/ApplyClassifierAsync'
      , MVCcmd
    )).then(result => {
      //console.log(result);
      this.RemoveBusy("Apply");
      return result.returnMessage;
    },
      error => {
        this.modalService.GenericError(error);
        this.RemoveBusy("Apply");
        return false;
      }
    ).catch(caught =>  {
      this.RemoveBusy("Apply");
      this.modalService.GenericError(caught);
      return false;
    });
  }

  CheckScreening(title: string, attrOn: number, attrNotOn: number): Promise<string | boolean> {
    let MVCcmd: MVCClassifierCommand = new MVCClassifierCommand();

    MVCcmd._title = title;
    MVCcmd._attributeIdOn = attrOn;
    MVCcmd._attributeIdNotOn = attrNotOn;
    MVCcmd._attributeIdClassifyTo = -1;
    MVCcmd._classifierId = -1;
    MVCcmd._sourceId = -1;
    MVCcmd.revInfo = this._reviewInfoService.ReviewInfo;

    this._BusyMethods.push("CheckScreening");

    //const headers = new HttpHeaders().set('Content-Type', 'application/json; charset=utf-8');

    return lastValueFrom(this._httpC.post<MVCClassifierCommand>(this._baseUrl + 'api/Classifier/CheckScreening'
      , MVCcmd
    )).then(result => {
      //console.log(result);
      this.RemoveBusy("CheckScreening");
      return result.returnMessage;
    },
      error => {
        this.modalService.GenericError(error);
        this.RemoveBusy("CheckScreening");
        return false;
      }
    ).catch(caught =>  {
      this.RemoveBusy("CheckScreening");
      this.modalService.GenericError(caught);
      return false;
    });
  }

  RunPriorityScreeningSimulation(title: string, attrOn: number, attrNotOn: number): Promise<string | boolean> {
    let MVCcmd: MVCClassifierCommand = new MVCClassifierCommand();

    MVCcmd._title = title;
    MVCcmd._attributeIdOn = attrOn;
    MVCcmd._attributeIdNotOn = attrNotOn;
    MVCcmd._attributeIdClassifyTo = -1;
    MVCcmd._classifierId = -1;
    MVCcmd._sourceId = -1;
    MVCcmd.revInfo = this._reviewInfoService.ReviewInfo;

    this._BusyMethods.push("RunPriorityScreeningSimulation");

    //const headers = new HttpHeaders().set('Content-Type', 'application/json; charset=utf-8');

    return lastValueFrom(this._httpC.post<MVCClassifierCommand>(this._baseUrl + 'api/Classifier/RunPriorityScreeningSimulation'
      , MVCcmd
    )).then(result => {
      //console.log(result);
      this.RemoveBusy("RunPriorityScreeningSimulation");
      return result.returnMessage;
    },
      error => {
        this.modalService.GenericError(error);
        this.RemoveBusy("RunPriorityScreeningSimulation");
        return false;
      }
    ).catch(caught => {
      this.RemoveBusy("RunPriorityScreeningSimulation");
      this.modalService.GenericError(caught);
      return false;
    });
  }

  public FetchPriorityScreeningSimulationList() {
    this._BusyMethods.push("FetchPriorityScreeningSimulationList");
    lastValueFrom( this._httpC.get<PriorityScreeningSimulation[]>(this._baseUrl + 'api/PriorirtyScreening/FetchPriorityScreeningSimulationList'))
      .then(result => {
        this.RemoveBusy("FetchPriorityScreeningSimulationList");
        if (result != null) {
          this.PriorityScreeningSimulationList = result;

          console.log('this.PriorityScreeningSimulationList', this.PriorityScreeningSimulationList);
        }
      },
        error => {
          this.RemoveBusy("FetchPriorityScreeningSimulationList");
          this.modalService.GenericError(error);
        }).catch(
          (caught) => {
            this.modalService.GenericError(caught);
          this.RemoveBusy("FetchPriorityScreeningSimulationList");
        });
  }

  public FetchPriorityScreeningSimulation(simulationName: string): Promise<boolean> {
    this._BusyMethods.push("FetchPriorityScreeningSimulation");
    let body = JSON.stringify(simulationName);
    return lastValueFrom(this._httpC.post<PriorityScreeningSimulation>(this._baseUrl + 'api/PriorirtyScreening/FetchPriorityScreeningSimulation',
      body))
      .then(
        (result) => {
          this.RemoveBusy("FetchPriorityScreeningSimulation");
          this._currentSimulation = result;

          return true;
        }, error => {
          this.modalService.GenericError(error);
          //this.modalService.GenericErrorMessage("There was an error getting the priority screening simulation. Please contact eppisupport@ucl.ac.uk");
          this.RemoveBusy("FetchPriorityScreeningSimulation");
          return false;
        }
      );
  }

  public DeletePriorityScreeningSimulation(simulationName: string) {
    this._BusyMethods.push("DeletePriorityScreeningSimulation");
    let body = JSON.stringify(simulationName);
    return lastValueFrom(this._httpC.post<PriorityScreeningSimulation>(this._baseUrl + 'api/PriorirtyScreening/DeletePriorityScreeningSimulation',
      body))
      .then(
        (result) => {
          this.RemoveBusy("DeletePriorityScreeningSimulation");
          this.FetchPriorityScreeningSimulationList();
          return true;
        }, error => {
          this.modalService.GenericError(error);
          //this.modalService.GenericErrorMessage("There was an error getting the priority screening simulation. Please contact eppisupport@ucl.ac.uk");
          this.RemoveBusy("DeletePriorityScreeningSimulation");
          return false;
        }
      );
  }


	public async UpdateModelName(modelName: string, modelNumber: string): Promise<boolean> {
		this._BusyMethods.push("UpdateModelName");
		let _ModelName: ModelNameUpdate = { ModelId: modelNumber, ModelName: modelName };
		let body = JSON.stringify(_ModelName);

    return lastValueFrom(this._httpC.post<boolean>(this._baseUrl + 'api/Classifier/UpdateModelName',
			body))
			.then(
				(result) => {
					this.RemoveBusy("UpdateModelName");

					// just update that line (rather than reloading all searches).
					let parsedInt: number = parseInt(modelNumber);
					let tmpIndex: number = this.ClassifierModelCurrentReviewList.findIndex(x => x.modelId == parsedInt);
					if (tmpIndex > -1) this.ClassifierModelCurrentReviewList[tmpIndex].modelTitle = _ModelName.ModelName;
					return true;
				}, error => {
					this.modalService.GenericError(error);
					//this.modalService.GenericErrorMessage("There was an error updating the model name. Please contact eppisupport@ucl.ac.uk");
					this.RemoveBusy("UpdateModelName");
					return false;
				}
			);
	}





	public Clear() {
		this.modelToBeDeleted = 0;
		this._ClassifierModelList = [];
		this._ClassifierContactModelList = [];
    this._CurrentUserId4ClassifierContactModelList = 0;
    this._PriorityScreeningSimulationList = [];
    this._currentSimulation = null;
    }
}

export class ClassifierModel {

	modelId: number = 0;
	contactId: number = 0;
	contactName: string = '';
	accuracy: number = 0;
	auc: number = 0;
	precision: number = 0;
	recall: number = 0;
	modelTitle: string = '';
	attributeOn: string = '';
	attributeNotOn: string = '';
	attributeIdOn: number = -1;
	attributeIdNotOn: number = -1;
	reviewId: number = 0;
	reviewName: string = '';

}

export class BuildModelCommand {

	public _title: string = '';
	public _attributeIdOn: number = 0;
	public _attributeIdNotOn: number = 0;
	public _attributeIdClassifyTo: number = 0;
	public _sourceId: number = 0;
	public revInfo: ReviewInfo = new ReviewInfo();
}

export class MVCClassifierCommand {

	public _title: string = '';
	public _attributeIdOn: number = 0;
	public _attributeIdNotOn: number = 0;
	public _attributeIdClassifyTo: number = 0;
	public _sourceId: number = 0;
	public _modelId: number = 0;
	public _attributeId: number = 0;
	public _classifierId: number = 0;
	public returnMessage: string = '';
	public revInfo: ReviewInfo = new ReviewInfo();
}


export class ClassifierCommandDeprecated {

	public _title: string = '';
	public _attributeIdOn: string = '0';
	public _attributeIdNotOn: string = '0';
	public _attributeIdClassifyTo: number = 0;
	public _sourceId: number = 0;
	public _classifierId: number = 0;
	public revInfo: ReviewInfo = new ReviewInfo();
	public returnMessage: string = '';

}

export interface ModelNameUpdate {
	ModelId: string;
	ModelName: string;
}

export class PriorityScreeningSimulation {
  public simulationName: string = '';
  public blob: string = '';
}
