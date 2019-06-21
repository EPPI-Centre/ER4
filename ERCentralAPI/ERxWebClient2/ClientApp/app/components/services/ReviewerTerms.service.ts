import { Component, Inject, Injectable } from '@angular/core';
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

    //public Save() {
    //    if (this._TermsList != null)
    //        localStorage.setItem('TermsList', JSON.stringify(this._TermsList));
    //    else if (localStorage.getItem('TermsList'))//to be confirmed!! 
    //        localStorage.removeItem('TermsList');
    //}
}
export interface ReviewerTerm {
    trainingReviewerTermId: number;
    itemTermDictionaryId: number;
    reviewerTerm: string;
    included: boolean;
    term: string;
}