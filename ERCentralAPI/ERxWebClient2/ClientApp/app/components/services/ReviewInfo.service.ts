import { Component, Inject, Injectable, EventEmitter, Output } from '@angular/core';
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
    @Output() ReviewInfoChanged = new EventEmitter<void>();
	private _ReviewContacts: Contact[] = [];
    private _ReviewInfo: ReviewInfo = new ReviewInfo();
    public get ReviewInfo(): ReviewInfo {
        return this._ReviewInfo;
    }
    public set ReviewInfo(ri: ReviewInfo) {
        this.ReviewInfoChanged.emit();
        this._ReviewInfo = ri;
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
		this._httpC.get<iReviewInfo>(this._baseUrl + 'api/ReviewInfo/ReviewInfo').subscribe(
			rI => {
            this.ReviewInfo = new ReviewInfo(rI);
                this.RemoveBusy("Fetch");
                //console.log("fetched revinfo:", this._ReviewInfo);
            //this.Save();
        }, error => {
            this.RemoveBusy("Fetch");
            this.modalService.SendBackHomeWithError(error);
        }
		);
    }
    public Update(rInfo: ReviewInfo) {
        this._BusyMethods.push("Update");
        return this._httpC.post<iReviewInfo>(this._baseUrl +
            'api/ReviewInfo/UpdateReviewInfo', rInfo)
            .subscribe(
            (result: iReviewInfo) => {
                this.RemoveBusy("Update");
                this.ReviewInfo = new ReviewInfo(result);
            },
            error => {
                this.RemoveBusy("Update");
                this.modalService.SendBackHomeWithError(error);
            });
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
    constructor(iRnfo?: iReviewInfo) {
        if (iRnfo) {
            this.reviewId = iRnfo.reviewId;
            this.reviewName = iRnfo.reviewName;
            this.showScreening = iRnfo.showScreening;
            this.screeningCodeSetId = iRnfo.screeningCodeSetId;
            this.screeningMode = iRnfo.screeningMode;
            this.screeningReconcilliation = iRnfo.screeningReconcilliation;
            this.screeningWhatAttributeId = iRnfo.screeningWhatAttributeId;
            this.screeningNPeople = iRnfo.screeningNPeople;
            this.screeningAutoExclude = iRnfo.screeningAutoExclude;
            this.screeningModelRunning = iRnfo.screeningModelRunning;
            this.screeningIndexed = iRnfo.screeningIndexed;
            this.screeningListIsGood = iRnfo.screeningListIsGood;
            this.bL_ACCOUNT_CODE = iRnfo.bL_ACCOUNT_CODE;
            this.bL_AUTH_CODE = iRnfo.bL_AUTH_CODE;
            this.bL_TX = iRnfo.bL_TX;
            this.bL_CC_ACCOUNT_CODE = iRnfo.bL_CC_ACCOUNT_CODE;
            this.bL_CC_AUTH_CODE = iRnfo.bL_CC_AUTH_CODE;
            this.bL_CC_TX = iRnfo.bL_CC_TX;
            this.magEnabled = iRnfo.magEnabled;
        }
    }
    public Clone(): ReviewInfo {
        let res: ReviewInfo = new ReviewInfo();
        res.reviewId = this.reviewId;
        res.reviewName = this.reviewName;
        res.showScreening = this.showScreening;
        res.screeningCodeSetId = this.screeningCodeSetId;
        res.screeningMode = this.screeningMode;
        res.screeningReconcilliation = this.screeningReconcilliation;
        res.screeningWhatAttributeId = this.screeningWhatAttributeId;
        res.screeningNPeople = this.screeningNPeople;
        res.screeningAutoExclude = this.screeningAutoExclude;
        res.screeningModelRunning = this.screeningModelRunning;
        res.screeningIndexed = this.screeningIndexed;
        res.screeningListIsGood = this.screeningListIsGood;
        res.bL_ACCOUNT_CODE = this.bL_ACCOUNT_CODE;
        res.bL_AUTH_CODE = this.bL_AUTH_CODE;
        res.bL_TX = this.bL_TX;
        res.bL_CC_ACCOUNT_CODE = this.bL_CC_ACCOUNT_CODE;
        res.bL_CC_AUTH_CODE = this.bL_CC_AUTH_CODE;
        res.bL_CC_TX = this.bL_CC_TX;
        res.magEnabled = this.magEnabled;
        return res;
    }
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
    magEnabled: number = 0;

}
export interface iReviewInfo {
    reviewId: number;
    reviewName: string;
    showScreening: boolean;
    screeningCodeSetId: number;
    screeningMode: string;
    screeningReconcilliation: string;
    screeningWhatAttributeId: number;
    screeningNPeople: number;
    screeningAutoExclude: boolean;
    screeningModelRunning: boolean;
    screeningIndexed: boolean;
    screeningListIsGood: boolean;
    bL_ACCOUNT_CODE: string;
    bL_AUTH_CODE: string;
    bL_TX: string;
    bL_CC_ACCOUNT_CODE: string;
    bL_CC_AUTH_CODE: string;
    bL_CC_TX: string;
    magEnabled: number;

}
export class Contact {

	 contactName: string = '';
	 contactId: number = 0;
}

