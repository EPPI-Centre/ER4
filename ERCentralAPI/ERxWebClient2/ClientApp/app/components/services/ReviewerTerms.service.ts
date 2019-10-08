import { Component, Inject, Injectable, EventEmitter } from '@angular/core';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';

@Injectable({
    providedIn: 'root',
})

export class ReviewerTermsService extends BusyAwareService {



    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
		super();
    }


    private _TermsList: ReviewerTerm[] = [];
    public get TermsList(): ReviewerTerm[] {
        //if (this._TermsList && this._TermsList.length > 0 ) {
        //    return this._TermsList;
        //}
        //else {
        //    const TermsListJson = localStorage.getItem('TermsList');
        //    let terms_Info: ReviewerTerm[] = TermsListJson !== null ? JSON.parse(TermsListJson) : [];

        //    if (terms_Info == undefined || terms_Info == null ) {

        //        return this._TermsList;
        //    }
        //    else {
        //        this._TermsList = terms_Info;
        //    }
        //}
        return this._TermsList;
	}

	public ShowHideTermsListEvent: EventEmitter<boolean> = new EventEmitter();
	public _ShowHideTermsList: boolean = false;

    public Fetch() {

		this._BusyMethods.push("Fetch");
        return this._httpC.get<ReviewerTerm[]>(this._baseUrl + 'api/ReviewerTermList/Fetch').subscribe(result => {
			this._TermsList = result;
			this.RemoveBusy("Fetch");
            //this.Save();
        },
			error => {
				this.modalService.GenericError(error);
				this.RemoveBusy("Fetch");
			}
        );
	}

	public CreateTerm(trt: ReviewerTerm): any {


		this._BusyMethods.push("CreateTerm");

		let body = JSON.stringify( trt );
		return this._httpC.post<ReviewerTerm>(this._baseUrl + 'api/ReviewerTermList/CreateReviewerTerm',
		body)
			.subscribe(result => {

				this._TermsList.push(result)
				console.log('Testing here: ' , result);
				this.Fetch();
				this.RemoveBusy("CreateTerm");

		},
			error => {
				this.modalService.GenericError(error);
				this.RemoveBusy("CreateTerm");
			}
		);
	}

	public DeleteTerm(termId: number): any {

		this._BusyMethods.push("DeleteTerm");

		let body = JSON.stringify({ Value: termId });
		return this._httpC.post<ReviewerTerm>(this._baseUrl + 'api/ReviewerTermList/DeleteReviewerTerm',
			body)
			.subscribe(result => {

//				this._TermsList
				this.Fetch();
				this.RemoveBusy("DeleteTerm");

			},
			error => {

					this.modalService.GenericError(error);
					this.RemoveBusy("DeleteTerm");
				}
			);
	}

	public UpdateTerm(term: ReviewerTerm): any {

		this._BusyMethods.push("UpdateTerm");

		let body = JSON.stringify(term);
		return this._httpC.post<ReviewerTerm>(this._baseUrl + 'api/ReviewerTermList/UpdateReviewerTerm',
			body)
			.subscribe(result => {

				//this._TermsList
				this.Fetch();
				this.RemoveBusy("UpdateTerm");

			},
				error => {

					this.modalService.GenericError(error);
					this.RemoveBusy("UpdateTerm");
				}
			);
	}



}
export interface ReviewerTerm {
    trainingReviewerTermId: number;
    itemTermDictionaryId: number;
    reviewerTerm: string;
    included: boolean;
    term: string;
}