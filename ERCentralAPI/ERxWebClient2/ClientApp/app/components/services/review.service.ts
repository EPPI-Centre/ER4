import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewInfo, ReviewInfoService } from './ReviewInfo.service';

@Injectable({

	providedIn: 'root',

})

export class ReviewService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
        ) {
        super();
    }

	//private _ClassifierModelList: ClassifierModel[] = [];
	////@Output() searchesChanged = new EventEmitter();
	////public crit: CriteriaSearch = new CriteriaSearch();
	//public searchToBeDeleted: string = '';//WHY string?

	//public get ClassifierModelList(): ClassifierModel[] {

	//	return this._ClassifierModelList;

	//}

	//public set ClassifierModelList(models: ClassifierModel[]) {

	//	this._ClassifierModelList = models;
	//}

	CreateReview(RevName: string, ContactId: string): Promise<number> {

		//hardcode until this works

		this._BusyMethods.push("CreateReview");

		let body = JSON.stringify({ reviewName: RevName, userId: ContactId });
        return this._httpC.post<number>(this._baseUrl + 'api/Review/CreateReview', body
        ).toPromise<number>().then(
            (result) => {
                this.RemoveBusy("CreateReview");
                return result;
            },
            (rejected) => {
                this.RemoveBusy("CreateReview");
                this.modalService.GenericErrorMessage("Sorry could not create new review. If the problem persists, please contact EPPI-Support.");
                return 0;
            }
        ).catch((error) => {
            this.RemoveBusy("CreateReview");
            this.modalService.GenericErrorMessage("Sorry could not create new review. If the problem persists, please contact EPPI-Support.");
            return 0;
        });

	
	}

//	,
//	error => {
//	this.modalService.GenericError(error);

//}
//				, () => {
//	this.RemoveBusy("CreateReview");
//}

	ngOnInit() {

	}
}


export class Review {

	contactId: number = 0;
	reviewId: number = 0;
	reviewName: string = '';
}

