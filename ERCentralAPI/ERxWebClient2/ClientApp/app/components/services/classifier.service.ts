import {  Inject, Injectable} from '@angular/core';
import { HttpClient, HttpHeaders   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewInfo, ReviewInfoService } from './ReviewInfo.service';

@Injectable({

    providedIn: 'root',

})

export class ClassifierService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private reviewInfoService: ReviewInfoService,
        @Inject('BASE_URL') private _baseUrl: string
        ) {
        super();
    }

	//public cmdSearches: SearchCodeCommand = new SearchCodeCommand();
	public ConnectionId: string = '';

	Create(title: string, attrOn: string, attrNotOn: string, ConnId: string) {

		let MVCcmd: ClassifierCommand = new ClassifierCommand();

		MVCcmd.ConnectionId = ConnId;
		MVCcmd._attributeIdClassifyTo = 0;
		MVCcmd._attributeIdNotOn = attrNotOn;
		MVCcmd._attributeIdOn = attrOn;
		MVCcmd._sourceId = -1;
		MVCcmd._title = title;
		MVCcmd.revInfo = this.reviewInfoService.ReviewInfo;

		this._BusyMethods.push("Fetch");

		let body = JSON.stringify({ MVCClassifierCommand: MVCcmd });

		alert('press F12 check log');
		console.log('is ti null? ==> ' + MVCcmd.revInfo.reviewName);

		const headers = new HttpHeaders().set('Content-Type', 'application/json; charset=utf-8');

		this._httpC.post<ClassifierCommand>(this._baseUrl + 'api/Classifier/GetClassifierAsync',
			MVCcmd, { headers: headers }
		)
				 .subscribe(result => {
				
					 alert('what the hell: ' + result);
					 
					 console.log(result)
				 },
				 error => {
					 this.modalService.GenericError(error);
					 this.Clear();
				 }
				 , () => {
					 this.RemoveBusy("Fetch");
				 }
			 );
	}

    private Clear() {

    }

}

export class ClassifierCommand {

	public ConnectionId: string = '';
	public _title: string = '';
	public _attributeIdOn: string = '0';
	public _attributeIdNotOn: string = '0';
	public _attributeIdClassifyTo: number = 0;
	public _sourceId: number = 0;
	public revInfo: ReviewInfo = new ReviewInfo();

}
