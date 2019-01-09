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

	Create(title: string, attrOn: string, attrNotOn: string) {

		let MVCcmd: ClassifierCommand = new ClassifierCommand();

		MVCcmd._attributeIdClassifyTo = 0;
		MVCcmd._attributeIdNotOn = attrNotOn;
		MVCcmd._attributeIdOn = attrOn;
		MVCcmd._sourceId = -1;
		MVCcmd._title = title;
		MVCcmd.revInfo = this.reviewInfoService.ReviewInfo;

		this._BusyMethods.push("Fetch");

		
		const headers = new HttpHeaders().set('Content-Type', 'application/json; charset=utf-8');

		this._httpC.post<ClassifierCommand>(this._baseUrl + 'api/Classifier/ClassifierAsync',
			MVCcmd, { headers: headers }
		)
				 .subscribe(result => {
				
					 //alert('what the hell: ' + result);
					 
					 //console.log(result)
				 },
				 error => {
					 this.modalService.GenericError(error);
				 }
				 , () => {
					 this.RemoveBusy("Fetch");
				 }
			 );
	}
	
	Apply(modeltitle: string, AttributeId: number, ModelId: number, SourceId: number) {

		let MVCcmd: ClassifierCommand = new ClassifierCommand();
		
		MVCcmd._title = modeltitle;
		MVCcmd._attributeIdOn = '-1';
		MVCcmd._attributeIdNotOn = '-1';
		MVCcmd._attributeIdClassifyTo = AttributeId;
		MVCcmd._classifierId = ModelId;
		MVCcmd._sourceId = SourceId;
		MVCcmd.revInfo = this.reviewInfoService.ReviewInfo;

		this._BusyMethods.push("Fetch");

		const headers = new HttpHeaders().set('Content-Type', 'application/json; charset=utf-8');

		this._httpC.post<ClassifierCommand>(this._baseUrl + 'api/Classifier/ApplyClassifierAsync',
			MVCcmd, { headers: headers }
		)
			.subscribe(result => {
				
				console.log(result)
			},
				error => {
					this.modalService.GenericError(error);
				}
				, () => {
					this.RemoveBusy("Fetch");
				}
			);
	}

}

export class ClassifierCommand {

	public _title: string = '';
	public _attributeIdOn: string = '0';
	public _attributeIdNotOn: string = '0';
	public _attributeIdClassifyTo: number = 0;
	public _sourceId: number = 0;
	public _classifierId: number = 0;
	public revInfo: ReviewInfo = new ReviewInfo();

}
