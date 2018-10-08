import { Component, Inject, Injectable, ChangeDetectorRef } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of, Subject } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ModalService } from './modal.service';


@Injectable({
    providedIn: 'root',
})

export class readonlyreviewsService {

    constructor(
        private _httpC: HttpClient,
        private modalService: ModalService,
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
                //console.log("Got workAllocations from LS");
                this._ReviewList = ReadOnlyReviews;
            }
        }
        return this._ReviewList;

    }
    
    public set ReadOnlyReviews(ror: ReadOnlyReview[]) {


        //for (var i = 0; i < ror.length; i++) {

        //    var temp = new ReadOnlyReview(ror[i].reviewId,
        //        ror[i].reviewName,
        //        ror[i].reviewOwner,
        //        ror[i].contactReviewRoles,
        //        ror[i].lastAccess);

        //    this.ReadOnlyReviews[i] = temp;

        //}
        this._ReviewList = ror;

        this.Save();
    }
    
        
    public Fetch(dtTrigger: Subject<any>) {

        return this._httpC.get<ReadOnlyReview[]>(this._baseUrl + 'api/review/readonlyreviews')
           
            .subscribe(result => {

                //for (var i = 0; i < result.length; i++) {

                //    var temp = new ReadOnlyReview(result[i].reviewId,
                //        result[i].reviewName,
                //        result[i].reviewOwner,
                //         result[i].contactReviewRoles,
                //        result[i].lastAccess);

                //    this.ReadOnlyReviews[i] = temp;
                  
                //}
                this.ReadOnlyReviews = result;
                dtTrigger.next();
                
                console.log(this.ReadOnlyReviews);
          
        }, error => { this.modalService.GenericError(error); }
          
        );
    }


    public Save() {
        if (this._ReviewList.length > 0)
            localStorage.setItem('ReadOnlyReviews', JSON.stringify(this._ReviewList));
        else if (localStorage.getItem('ReadOnlyReviews'))//to be confirmed!! 
            localStorage.removeItem('ReadOnlyReviews');
    }
}

export class ReadOnlyReview {

    reviewId: string = "0";
    reviewName: string = "";
    contactReviewRoles: string="";
    reviewOwner: string="";
    lastAccess: string = "";

    //constructor(ReviewId: string,
    //    ReviewName: string,
    //    ContactReviewRoles: string,
    //    ReviewOwner: string,
    //    LastAccess: string, )
    //{

    //    this.reviewId = ReviewId.toString().trim();
    //    this.reviewName = ReviewName;
    //    this.contactReviewRoles = ContactReviewRoles;
    //    this.reviewOwner = ReviewOwner;
    //    this.lastAccess = LastAccess;
    //}
}
