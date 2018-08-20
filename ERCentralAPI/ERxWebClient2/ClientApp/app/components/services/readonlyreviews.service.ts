import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';



@Injectable({
    providedIn: 'root',
})

export class readonlyreviewsService {

    constructor(
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string
        ) { }


    private _ReviewList: ReadOnlyReview[] = [];

    public get ReadOnlyReviews(): ReadOnlyReview[] {
        if (this._ReviewList.length == 0) {

            const ReadOnlyReviewsJson = localStorage.getItem('ReadOnlyReviews');
            let ReadOnlyReviews: ReadOnlyReview[] = ReadOnlyReviewsJson !== null ? JSON.parse(ReadOnlyReviewsJson) : [];
            if (ReadOnlyReviews == undefined || ReadOnlyReviews == null || ReadOnlyReviews.length == 0) {
                return this._ReviewList;
            }
            else {
                console.log("Got workAllocations from LS");
                this._ReviewList = ReadOnlyReviews;
            }
        }
        return this._ReviewList;

    }
    
    public set ReadOnlyReviews(ror: ReadOnlyReview[]) {
        this._ReviewList = ror;
        this.Save();
    }
        
    public Fetch() {

        return this._httpC.get<ReadOnlyReview[]>(this._baseUrl + 'api/review/readonlyreviews').subscribe(result => {
            this.ReadOnlyReviews = result;
        });
    }

    public Save() {
        if (this._ReviewList.length > 0)
            localStorage.setItem('ReadOnlyReviews', JSON.stringify(this._ReviewList));
        else if (localStorage.getItem('ReadOnlyReviews'))//to be confirmed!! 
            localStorage.removeItem('ReadOnlyReviews');
    }
}

export class ReadOnlyReview {
    reviewId: number = 0;
    reviewName: string = "";
    contactReviewRoles: string="";
    reviewOwner: string="";
    lastAccess: string = "";
}
