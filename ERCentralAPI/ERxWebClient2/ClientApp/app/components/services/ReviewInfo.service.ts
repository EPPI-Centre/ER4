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
        this.ReviewInfo = new ReviewInfo();
    }


    private ReviewInfo: ReviewInfo;
    

    public Fetch() {

        return this._httpC.get<ReviewInfo>(this._baseUrl + 'api/ReviewInfo/ReviewInfo').subscribe(rI => {
            this.ReviewInfo = rI;
            this.Save();
            console.log('This is the review name: ' + rI.reviewId + ' ' + this.ReviewInfo.reviewName);
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

    public reviewId: number = 0;
    public reviewName: string = "";

}

