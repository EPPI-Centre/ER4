import { Inject, Injectable, EventEmitter, Output, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Outcome, ReviewSetDropDownDResult } from '../services/ItemCoding.service';
import { COMPOSITION_BUFFER_MODE } from '@angular/forms';
import { Item } from './ItemList.service';
import { SetAttribute } from './ReviewSets.service';

@Injectable({
    providedIn: 'root',
})

export class OutcomesService extends BusyAwareService implements OnInit  {

    constructor(
		private _http: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }
	ngOnInit() {

	}
	private _currentItemSetId: number = 0;
	private _Outcomes: Outcome[] = [];
	public ItemSetId: number = 0;

	public get outcomesList(): Outcome[] {
        if (this._Outcomes) return this._Outcomes;
        else {
            this._Outcomes = [];
            return this._Outcomes;
        }
	}
	public ShowOutComeList: EventEmitter<SetAttribute> = new EventEmitter();
	
	public set outcomesList(Outcomes: Outcome[]) {
        this._Outcomes = Outcomes;
    }
    @Output() gotNewOutcomes = new EventEmitter();
    @Output() outcomesChangedEE = new EventEmitter();
	public get Selectedoutcome(): Outcome | null {

        return this._selectedoutcome;
    }
	private _selectedoutcome: Outcome | null = null;
	public SetSelectedoutcome(outcome: Outcome) {

		this._selectedoutcome = outcome;
		//if (this._selectedoutcome != outcome) {
		this.outcomesChangedEE.emit();
		//}

	}
	public ReviewSetOutcomeList: ReviewSetDropDownDResult[] = [];
	public ReviewSetControlList: ReviewSetDropDownDResult[] = [];
	public ReviewSetInterventionList: ReviewSetDropDownDResult[] = [];
	public ReviewSetItemArmList: ItemArm[] = [];

	public IsServiceBusy(): boolean {

		if (this._BusyMethods.length > 0) {
			return true;
		} else {
			return false;
		}
	}

    public FetchOutcomes(ItemSetId: number) {

		this._BusyMethods.push("FetchOutcomes");
		let body = JSON.stringify({ Value: ItemSetId });

		this._http.post<Outcome[]>(this._baseUrl + 'api/OutcomeList/Fetch', body).subscribe(result => {


			this.outcomesList = result;
			//console.log(JSON.stringify(result));
			//console.log('=============================');
			//console.log(JSON.stringify(this.outcomesList));
			//console.log('got inside the Outcomes service: ' + this.outcomesList.length);

			//this.outcomesChangedEE.emit(this.outcomesList[0].itemSetId);
				   this.RemoveBusy("FetchOutcomes");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchOutcomes");
			}
        );
	}
	public FetchReviewSetOutcomeList(itemSetId: number, setId: number) {

		this._BusyMethods.push("FetchReviewSetOutcomeList");
		let body = JSON.stringify({ itemSetId: itemSetId, setId: setId });

		this._http.post<ReviewSetDropDownDResult[]>(this._baseUrl + 'api/OutcomeList/FetchReviewSetOutcomeList',
			body)
			.subscribe(result => {

				this.ReviewSetOutcomeList = result;
				console.log('Outcome' + JSON.stringify(result));
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

		this._http.post<ReviewSetDropDownDResult[]>(this._baseUrl + 'api/OutcomeList/FetchReviewSetInterventionList',
			body)
			.subscribe(result => {

				this.ReviewSetInterventionList = result;
				console.log('Intervention' + JSON.stringify(result));
				//this.gotNewOutcomes.emit(this.outcomesList);
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

		this._http.post<ReviewSetDropDownDResult[]>(this._baseUrl + 'api/OutcomeList/FetchReviewSetControlList',
			body)
			.subscribe(result => {

				this.ReviewSetControlList = result;
				console.log('Control' + JSON.stringify(result));
				//this.gotNewOutcomes.emit(this.outcomesList);
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
				console.log('Arm' + JSON.stringify(result));
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
	
							if (!result) this.modalService.GenericErrorMessage(ErrMsg);
							this.FetchOutcomes(this._currentItemSetId);
							this.RemoveBusy("CreateOutcome");
							return result;
						}
						, (error) => {
							this.FetchOutcomes(this._currentItemSetId);		
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
		this._http.post<Outcome>(this._baseUrl + 'api/OutcomeList/DeleteOutcome',

			body).subscribe(
				(result) => {

					console.log('Checking if error returned anything: ' + JSON.stringify(result));
					this.FetchOutcomes(this._currentItemSetId);	
					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteOutcome");
					return result;
				}
				, (error) => {

					//console.log(JSON.stringify(error));
					this.FetchOutcomes(this._currentItemSetId);					
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

export class ItemOutcomeDeleteWarningCommandJSON {

	numOutcomes: number = 0;
    itemId: number = 0;
	itemoutcomeId: number = 0;
}


