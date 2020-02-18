import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Helpers } from '../helpers/HelperMethods';

@Injectable({
    providedIn: 'root',
})

export class ReviewInfoService extends BusyAwareService{

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        super();
    }

	private _ReviewContacts: Contact[] = [];
    private _ReviewInfo: ReviewInfo = new ReviewInfo();
    public get ReviewInfo(): ReviewInfo {
        //if (this._ReviewInfo.reviewId && this._ReviewInfo.reviewId != 0) {
        //    return this._ReviewInfo;
        //}
        //else {
        //    const RevInfoJson = localStorage.getItem('ReviewInfo');
        //    let rev_Info: ReviewInfo = RevInfoJson !== null ? JSON.parse(RevInfoJson) : new ReviewInfo();
  
        //    if (rev_Info == undefined || rev_Info == null || rev_Info.reviewId == 0) {

        //        return this._ReviewInfo;
        //    }
        //    else {
        //        this._ReviewInfo = rev_Info;
        //    }
        //}
        return this._ReviewInfo;
	}
	public get Contacts(): Contact[] {

		if (this._ReviewContacts) return this._ReviewContacts;
		else {
			this._ReviewContacts = [];
			return this._ReviewContacts;
		}
	}

    public async FetchAll() {
        this.Fetch();
        await Helpers.Sleep(40);//just avoiding to send two requests exactly at the same time...
        this.FetchReviewMembers();
    }
	public Fetch() {
        this._BusyMethods.push("Fetch");
		this._httpC.get<ReviewInfo>(this._baseUrl + 'api/ReviewInfo/ReviewInfo').subscribe(
			rI => {
            this._ReviewInfo = rI;
            this.RemoveBusy("Fetch");
            //this.Save();
        }, error => {
            this.RemoveBusy("Fetch");
            this.modalService.SendBackHomeWithError(error);
        }
		);
	}

	public FetchReviewMembers() {
		
		let ErrMsg = "Something went wrong when fetching review members \r\n If the problem persists, please contact EPPISupport.";

		this._BusyMethods.push("FetchReviewMembers");
		this._httpC.get<Contact[]>(this._baseUrl + 'api/ReviewInfo/ReviewMembers').subscribe(

			(result) => {
				this._ReviewContacts = result;
				if (!result) this.modalService.GenericErrorMessage(ErrMsg);
				this.RemoveBusy("FetchReviewMembers");
				return result;
			}
			, (error) => {
				this.RemoveBusy("FetchReviewMembers");
				this.modalService.GenericErrorMessage(ErrMsg);
				return error;
			});

    }
    public ContactNameById(ContactID: number): string {
        if (ContactID <= 0) return "N/A";
        let ind = this._ReviewContacts.findIndex(found => found.contactId == ContactID);
        if (ind != -1) return this._ReviewContacts[ind].contactName;
        else return "N/A [Id:" + ContactID.toString() +"]";
    }
    public Clear() {
        this._ReviewInfo = new ReviewInfo();
        this._ReviewContacts = [];
    }
}

export class ReviewInfo {

    reviewId: number = 0;
    reviewName: string = "";
    showScreening: boolean = false;
    screeningCodeSetId: number = 0;
    screeningMode: string = "";
    screeningReconcilliation: string = "";
    screeningWhatAttributeId: number = 0;
    screeningNPeople: number = 0;
    screeningAutoExclude: boolean = false;
    screeningModelRunning: boolean = false;
    screeningIndexed: boolean = false;
    screeningListIsGood: boolean = false;
    bL_ACCOUNT_CODE: string = "";
    bL_AUTH_CODE: string = "";
    bL_TX: string = "";
    bL_CC_ACCOUNT_CODE: string = "";
    bL_CC_AUTH_CODE: string = "";
    bL_CC_TX: string = "";

}

export class Contact {

	 contactName: string = '';
	 contactId: number = 0;
}

