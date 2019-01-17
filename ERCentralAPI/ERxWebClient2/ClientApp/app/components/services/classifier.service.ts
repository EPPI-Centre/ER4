import {  Inject, Injectable} from '@angular/core';
import { HttpClient, HttpHeaders   } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ReviewInfo, ReviewInfoService } from './ReviewInfo.service';
import { Observable, Subscription } from 'rxjs';
import { BuildModelService } from './buildmodel.service';
import { NotificationService } from '@progress/kendo-angular-notification';

@Injectable({

    providedIn: 'root',

})

export class ClassifierService extends BusyAwareService {

    asyncResult: ClassifierCommand = new ClassifierCommand();

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private reviewInfoService: ReviewInfoService,
		public _buildModelService: BuildModelService,
		private notificationService: NotificationService,
        @Inject('BASE_URL') private _baseUrl: string
        ) {
        super();
	}

	IamVerySorryRefresh() {

		this._buildModelService.Fetch();

	}

	showBuildModelMessage(notifyMsg: string) {

		this.notificationService.show({
			content: notifyMsg,
			animation: { type: 'slide', duration: 400 },
			position: { horizontal: 'center', vertical: 'top' },
			type: { style: "info", icon: true },
			closable: true
		});

	}

	CreateAsync(title: string, attrOn: string, attrNotOn: string): Subscription {

		let MVCcmd: ClassifierCommand = new ClassifierCommand();

		MVCcmd._attributeIdClassifyTo = 0;
		MVCcmd._attributeIdNotOn = attrNotOn;
		MVCcmd._attributeIdOn = attrOn;
		MVCcmd._sourceId = -1;
		MVCcmd._title = title;
		MVCcmd.revInfo = this.reviewInfoService.ReviewInfo;

		this._BusyMethods.push("Fetch");

		const headers = new HttpHeaders().set('Content-Type', 'application/json; charset=utf-8');

		//alert('about to send to controller');

		return this._httpC.post<ClassifierCommand>(this._baseUrl + 'api/Classifier/Classifier',
			MVCcmd, { headers: headers }
		).subscribe(

			result => {

				if (result.returnMessage == '') {

					this.showBuildModelMessage('request was submitted');

				} else {

					this.showBuildModelMessage(result.returnMessage);
				}

				this.IamVerySorryRefresh();
			},
			error => {
				this.modalService.GenericError(error);

			}, () => {
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
	public returnMessage: string = '';

}
