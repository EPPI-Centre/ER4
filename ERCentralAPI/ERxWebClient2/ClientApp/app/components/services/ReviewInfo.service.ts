import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

@Injectable({
    providedIn: 'root',
})

export class ReviewInfoService {

    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string
    ) {
        this._ReviewInfo = new ReviewInfo();
    }


    private _ReviewInfo: ReviewInfo;
    public get ReviewInfo(): ReviewInfo {
        //console.log('Revinfo GET ' + this._ReviewInfo.screeningCodeSetId + " " + this._ReviewInfo.screeningListIsGood);
        if (this._ReviewInfo.reviewId && this._ReviewInfo.reviewId != 0) {
            return this._ReviewInfo;
        }
        else {
            const RevInfoJson = localStorage.getItem('ReviewInfo');
            let rev_Info: ReviewInfo = RevInfoJson !== null ? JSON.parse(RevInfoJson) : new ReviewInfo();
            //let tmp: any = localStorage.getItem('currentErUser');
            //console.log("after LS: " + this._platformId);
            //let tmp2: ReviewerIdentity = tmp;
            if (rev_Info == undefined || rev_Info == null || rev_Info.reviewId == 0) {

                return this._ReviewInfo;
            }
            else {
                //console.log("Got User from LS");
                this._ReviewInfo = rev_Info;
            }
        }
        return this._ReviewInfo;
    }

    public Fetch() {
        console.log('fetching revInfo');
        return this._httpC.get<ReviewInfo>(this._baseUrl + 'api/ReviewInfo/ReviewInfo').subscribe(rI => {
            this._ReviewInfo = rI;
            this.Save();
            //console.log('This is the review name: ' + rI.reviewId + ' ' + this.ReviewInfo.reviewName);
        });
    }

    public Save() {
        if (this.ReviewInfo != null)
            localStorage.setItem('ReviewInfo', JSON.stringify(this.ReviewInfo));
        else if (localStorage.getItem('ReviewInfo'))//to be confirmed!! 
            localStorage.removeItem('ReviewInfo');
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

