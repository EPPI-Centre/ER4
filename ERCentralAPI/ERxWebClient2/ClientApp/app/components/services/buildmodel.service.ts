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
		private reviewInfoService: ReviewInfoService,
        @Inject('BASE_URL') private _baseUrl: string
        ) {
        super();
    }

	private _ClassifierModelList: ClassifierModel[] = [];
	//@Output() searchesChanged = new EventEmitter();
	//public crit: CriteriaSearch = new CriteriaSearch();
	public searchToBeDeleted: string = '';//WHY string?

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
