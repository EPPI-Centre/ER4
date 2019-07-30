import { Inject, Injectable, EventEmitter, Output, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Outcome } from '../services/ItemCoding.service';
import { COMPOSITION_BUFFER_MODE } from '@angular/forms';
import { Item } from './ItemList.service';

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
	private _Outcomes: Outcome[] | null = null;

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
   @Output() gotNewOutcomes = new EventEmitter();
   // @Output() outcomeChangedEE = new EventEmitter();
	public get Selectedoutcome(): Outcome | null {

        return this._selectedoutcome;
    }
	private _selectedoutcome: Outcome | null = null;

	public SetSelectedoutcome(outcome: Outcome) {

		this._selectedoutcome = outcome;
		//if (this._selectedoutcome != outcome) {
			this.gotNewOutcomes.emit();
		//}

	}

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
			console.log(JSON.stringify(result));
			console.log('=============================');
			console.log(JSON.stringify(this.outcomesList));
			console.log('got inside the Outcomes service: ' + this.outcomesList.length);

			this.gotNewOutcomes.emit(this.outcomesList);
				   this.RemoveBusy("FetchOutcomes");
			}, error => {
				this.modalService.SendBackHomeWithError(error);
				this.RemoveBusy("FetchOutcomes");
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


	DeleteOutcome(OutcomeJSON: Outcome) {

		this._BusyMethods.push("DeleteOutcome");
			let ErrMsg = "Something went wrong when deleting an outcome. \r\n If the problem persists, please contact EPPISupport.";

		console.log('outcome deleting: ' + JSON.stringify(OutcomeJSON));
		let body = JSON.stringify({ OutcomeJSON: OutcomeJSON });		
		this._http.post<Outcome>(this._baseUrl + 'api/OutcomeList/DeleteOutcome',

			body).subscribe(
				(result) => {

					if (!result) this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteOutcome");
					return result;
				}
				, (error) => {

					this.FetchOutcomes(this._currentItemSetId);					
					this.modalService.GenericErrorMessage(ErrMsg);
					this.RemoveBusy("DeleteOutcome");
					return error;
				}
				);

	}

}


export class ItemOutcomeDeleteWarningCommandJSON {

	numOutcomes: number = 0;
    itemId: number = 0;
	itemoutcomeId: number = 0;
}


