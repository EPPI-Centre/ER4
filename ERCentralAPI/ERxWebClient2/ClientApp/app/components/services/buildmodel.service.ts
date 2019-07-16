import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewInfo, ReviewInfoService } from './ReviewInfo.service';

@Injectable({

	providedIn: 'root',

})

export class BuildModelService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private _reviewInfoService: ReviewInfoService,
        @Inject('BASE_URL') private _baseUrl: string
        ) {
        super();
    }

	private _ClassifierModelList: ClassifierModel[] = [];
	//@Output() searchesChanged = new EventEmitter();
	//public crit: CriteriaSearch = new CriteriaSearch();
	public modelToBeDeleted: number = 0;

	public get ClassifierModelList(): ClassifierModel[] {

		return this._ClassifierModelList;

	}

	public set ClassifierModelList(models: ClassifierModel[]) {

		this._ClassifierModelList = models;
	}

	Fetch() {

		this._BusyMethods.push("Fetch");

		this._httpC.get<any>(this._baseUrl + 'api/Classifier/GetClassifierModelList',
		)
			.subscribe(result => {

				this.ClassifierModelList = result;
				console.log(result)
			},
				error => {
					this.modalService.GenericError(error);
                    this.RemoveBusy("Fetch");
				}
				, () => {
					this.RemoveBusy("Fetch");
				}
			);

	}

	public Delete(modelId: number): Promise<boolean>{

		let MVCcmd: MVCClassifierCommand = new MVCClassifierCommand();

		MVCcmd._title = '';
		MVCcmd._attributeIdOn = -1;
		MVCcmd._attributeIdNotOn = -1;
		MVCcmd._attributeIdClassifyTo = -1;
		MVCcmd._modelId = modelId;
		MVCcmd.revInfo = this._reviewInfoService.ReviewInfo;

		this._BusyMethods.push("DeleteModel");

		return this._httpC.post<MVCClassifierCommand>(this._baseUrl + 'api/Classifier/DeleteModel',
			MVCcmd)
			.toPromise().then(
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


	ngOnInit() {

		
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
