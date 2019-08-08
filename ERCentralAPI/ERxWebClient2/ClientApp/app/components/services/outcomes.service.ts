import { Inject, Injectable, EventEmitter, Output, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { SetAttribute } from './ReviewSets.service';
import { iTimePoint } from './timePoints.service';

@Injectable({
    providedIn: 'root',
})

export class OutcomesService extends BusyAwareService implements OnInit, OnDestroy  {

    constructor(
		private _http: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
	ngOnInit() {

	}
	ngOnDestroy() {

		this.outcomesChangedEE.unsubscribe();
	}
	private _currentItemSetId: number = 0;
	public _Outcomes: Outcome[] = [];
	public ItemSetId: number = 0;
	public ShowOutComeList: EventEmitter<SetAttribute> = new EventEmitter();
	public get outcomesList(): Outcome[] {
        if (this._Outcomes) return this._Outcomes;
        else {
            this._Outcomes = [];
            return this._Outcomes;
        }
	}
	
	
	public set outcomesList(Outcomes: Outcome[]) {
        this._Outcomes = Outcomes;
    }
	@Output() outcomesChangedEE = new EventEmitter();

	public ReviewSetOutcomeList: ReviewSetDropDownResult[] = [];
	public ReviewSetControlList: ReviewSetDropDownResult[] = [];
	public ReviewSetInterventionList: ReviewSetDropDownResult[] = [];
	public ReviewSetItemArmList: ItemArm[] = [];
	public IsServiceBusy(): boolean {

		if (this._BusyMethods.length > 0) {
			return true;
		} else {
			return false;
		}
	}

    public FetchOutcomes(ItemSetId: number): Outcome[] {

		this._BusyMethods.push("FetchOutcomes");
		let body = JSON.stringify({ Value: ItemSetId });

		this._http.post<Outcome[]>(this._baseUrl + 'api/OutcomeList/Fetch', body).subscribe(result => {

				console.log('can see the new outcome in here: ' + JSON.stringify(result));
				//this.outcomesList = result;
				this.RemoveBusy("FetchOutcomes");

		}, error => {
				
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchOutcomes");
			}
		);
		return this.outcomesList;
	}

	public FetchReviewSetOutcomeList(itemSetId: number, setId: number) {

		this._BusyMethods.push("FetchReviewSetOutcomeList");
		let body = JSON.stringify({ itemSetId: itemSetId, setId: setId });

		this._http.post<ReviewSetDropDownResult[]>(this._baseUrl + 'api/OutcomeList/FetchReviewSetOutcomeList',
			body)
			.subscribe(result => {

				this.ReviewSetOutcomeList = result;
				
				//this.outcomesChangedEE.emit(this.outcomesList);
				this.RemoveBusy("FetchReviewSetOutcomeList");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchReviewSetOutcomeList");
		}
		);

	}

	public FetchReviewSetInterventionList(itemSetId: number, setId: number) {

		this._BusyMethods.push("FetchReviewSetInterventionList");
		let body = JSON.stringify({ itemSetId: itemSetId, setId: setId });

		this._http.post<ReviewSetDropDownResult[]>(this._baseUrl + 'api/OutcomeList/FetchReviewSetInterventionList',
			body)
			.subscribe(result => {

				this.ReviewSetInterventionList = result;
			
				this.RemoveBusy("FetchReviewSetInterventionList");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchReviewSetInterventionList");
			}
			);

	}

	public FetchReviewSetControlList(itemSetId: number, setId: number) {

		this._BusyMethods.push("FetchReviewSetControlList");
		let body = JSON.stringify({ itemSetId: itemSetId, setId: setId });

		this._http.post<ReviewSetDropDownResult[]>(this._baseUrl + 'api/OutcomeList/FetchReviewSetControlList',
			body)
			.subscribe(result => {

				this.ReviewSetControlList = result;
				
				this.RemoveBusy("FetchReviewSetControlList");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchReviewSetControlList");
			}
			);


	}

	public FetchItemArmList(itemId: number) {

		this._BusyMethods.push("FetchItemArmList");
		let body = JSON.stringify({ Value: itemId});

		this._http.post<ItemArm[]>(this._baseUrl + 'api/OutcomeList/FetchItemArmList',
			body)
			.subscribe(result => {

				this.ReviewSetItemArmList = result;
				
				this.RemoveBusy("FetchItemArmList");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchItemArmList");
			}
			);
	}

	public Createoutcome(currentoutcome: Outcome): Promise<Outcome> {

		this._BusyMethods.push("CreateOutcome");
		let ErrMsg = "Something went wrong when creating an outcome. \r\n If the problem persists, please contact EPPISupport.";

		return this._http.post<Outcome>(this._baseUrl + 'api/OutcomeList/Createoutcome',

			currentoutcome).toPromise()
						.then(
						(result) => {

							var testObj = result as Outcome;
							alert(JSON.stringify(testObj));
							this._Outcomes.push(testObj);							
							//if (!result) this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("CreateOutcome");
							
						}
						, (error) => {

							//this.FetchOutcomes(this._currentItemSetId);		
							this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("CreateOutcome");
							return error;
						}
					)
					.catch(
						(error) => {
							this.FetchOutcomes(this._currentItemSetId);		
							this.modalService.GenericErrorMessage(ErrMsg);
							this.RemoveBusy("CreateOutcome");
							return error;
					}
		);
	}
	
	public Updateoutcome(currentOutcome: Outcome) {

		this._BusyMethods.push("UpdateOutcome");
		let ErrMsg = "Something went wrong when updating an outcome. \r\n If the problem persists, please contact EPPISupport.";

		this._http.post<Outcome[]>(this._baseUrl + 'api/OutcomeList/UpdateOutcome',

			currentOutcome).subscribe(

				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.FetchOutcomes(this._currentItemSetId);	
					this.RemoveBusy("UpdateOutcome");
					return result;
				}
				, (error) => {
					this.FetchOutcomes(this._currentItemSetId);		
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("UpdateOutcome");
					return error;
				});
	}
	
	DeleteOutcome(outcomeId: number, itemSetId: number) {

		this._BusyMethods.push("DeleteOutcome");
			let ErrMsg = "Something went wrong when deleting an outcome. \r\n If the problem persists, please contact EPPISupport.";

		console.log('outcome deleting: ' + JSON.stringify(outcomeId) + 'asdfsadf: ==== ' + JSON.stringify(itemSetId) );
		let body = JSON.stringify({ outcomeId: outcomeId, itemSetId: itemSetId  });		
		this._http.post<any>(this._baseUrl + 'api/OutcomeList/DeleteOutcome',

			body).subscribe(
				(result) => {

					//console.log('Checking if error returned anything: ' + JSON.stringify(result));
					//this.FetchOutcomes(itemSetId);	
					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteOutcome");
					return result;
				}
				, (error) => {

					//console.log(JSON.stringify(error));
					//this.FetchOutcomes(itemSetId);					
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteOutcome");
					return error;
				}
				);

	}

}

export class ItemArm {

	itemArmId: number = 0;
	title: string = '';

}


export class OutcomeItemList {
	outcomesList: Outcome[] = [];
}
export class OutcomeType {

	outcomeTypeId: number = 0;
	outcomeTypeName: string = '';

}
export class ReviewSetDropDownResult {

	public attributeId: number = 0;
	public attributeName: string = '';

}
export class Outcome {


	outcomeId: number = 0;
	itemSetId: number = 0;
	outcomeTypeName: string = "";
	outcomeTypeId: number = 0;
	outcomeCodes: OutcomeItemAttributesList = new OutcomeItemAttributesList();//OutcomeItemAttribute[] = [];
	itemAttributeIdIntervention: number = 0;
	itemAttributeIdControl: number = 0;
	itemAttributeIdOutcome: number = 0;
	itemArmIdGrp1: number = 0;
	itemArmIdGrp2: number = 0;
	itemId: number = 0;
	itemTimepointValue: string = '';
	itemTimepointMetric: string = '';
	itemTimepointId: number = 0;
	outcomeTimePoint = {} as iTimePoint;
	title: string = "";
	shortTitle: string = "";
	outcomeDescription: string = "";
	data1: number = 0;
	data2: number = 0;
	data3: number = 0;
	data4: number = 0;
	data5: number = 0;
	data6: number = 0;
	data7: number = 0;
	data8: number = 0;
	data9: number = 0;
	data10: number = 0;
	data11: number = 0;
	data12: number = 0;
	data13: number = 0;
	data14: number = 0;
	interventionText: string = "";
	controlText: string = "";
	outcomeText: string = "";
	feWeight: number = 0;
	reWeight: number = 0;
	smd: number = 0;
	sesmd: number = 0;
	r: number = 0;
	ser: number = 0;
	oddsRatio: number = 0;
	seOddsRatio: number = 0;
	riskRatio: number = 0;
	seRiskRatio: number = 0;
	ciUpperSMD: number = 0;
	ciLowerSMD: number = 0;
	ciUpperR: number = 0;
	ciLowerR: number = 0;
	ciUpperOddsRatio: number = 0;
	ciLowerOddsRatio: number = 0;
	ciUpperRiskRatio: number = 0;
	ciLowerRiskRatio: number = 0;
	ciUpperRiskDifference: number = 0;
	ciLowerRiskDifference: number = 0;
	ciUpperPetoOddsRatio: number = 0;
	ciLowerPetoOddsRatio: number = 0;
	ciUpperMeanDifference: number = 0;
	ciLowerMeanDifference: number = 0;
	riskDifference: number = 0;
	seRiskDifference: number = 0;
	meanDifference: number = 0;
	seMeanDifference: number = 0;
	petoOR: number = 0;
	sePetoOR: number = 0;
	es: number = 0;
	sees: number = 0;
	nRows: number = 0;
	ciLower: number = 0;
	ciUpper: number = 0;
	esDesc: string = "";
	seDesc: string = "";
	data1Desc: string = "";
	data2Desc: string = "";
	data3Desc: string = "";
	data4Desc: string = "";
	data5Desc: string = "";
	data6Desc: string = "";
	data7Desc: string = "";
	data8Desc: string = "";
	data9Desc: string = "";
	data10Desc: string = "";
	data11Desc: string = "";
	data12Desc: string = "";
	data13Desc: string = "";
	data14Desc: string = "";
}
export class OutcomeItemAttributesList {
	outcomeItemAttributesList: OutcomeItemAttribute[] = [];
}
export interface OutcomeItemAttribute {
	outcomeItemAttributeId: number;
	outcomeId: number;
	attributeId: number;
	additionalText: string;
	attributeName: string;
}
