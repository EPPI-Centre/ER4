import { Component, Inject, Injectable } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

@Injectable({
    providedIn: 'root',
})
export class ReviewerIdentityService {
    constructor(private router: Router, //private _http: Http, 
        private _httpC: HttpClient,
        @Inject('BASE_URL') private _baseUrl: string) {}
    private _reviewerIdentity: ReviewerIdentity = new ReviewerIdentity;
    public get reviewerIdentity(): ReviewerIdentity {
         
        if (this._reviewerIdentity.userId == 0) {
            let tmp: any = localStorage.getItem('currentErUser');
            let tmp2: ReviewerIdentity = tmp;
            if (tmp2 == undefined || tmp2 == null || tmp2.userId == 0) {
                return this._reviewerIdentity;
            }
            else {
                this._reviewerIdentity = tmp2;
            }
        }
        return this._reviewerIdentity;
    }
    public set reviewerIdentity(ri: ReviewerIdentity) {
        this._reviewerIdentity = ri;
    }
    public Report()  {
        console.log('Reporting Cid: ' + this._reviewerIdentity.userId);
        console.log('NAME: ' + this._reviewerIdentity.name);
        console.log('Token: ' + this._reviewerIdentity.token);
        console.log('Ticket: ' + this._reviewerIdentity.ticket);
        console.log('Expires on: ' + this._reviewerIdentity.accountExpiration);
    }
    
    
    public LoginReq(u: string, p: string) {
        let body = "Username=" + u + "&Password=" + p;
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/Login',
            body);
    }
    public LoginToReview(RevId: number) {
        let body = "ReviewId=" + RevId;
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body);
    }
    public Save() {
        if (this._reviewerIdentity.userId != 0)
            localStorage.setItem('currentErUser', JSON.stringify(this._reviewerIdentity));
        else if (localStorage.getItem('currentErUser'))//to be confirmed!! 
            localStorage.removeItem('currentErUser');
    }
}
export class ReviewerIdentity {
    public reviewId: number = 0;
    public ticket: string = "";
    public userId: number = 0;
    public name: string = "";
    public accountExpiration: string = "";
    public reviewExpiration: string = "";
    public isSiteAdmin: boolean = false;
    public loginMode: string = "";
    public roles: string[] = [];
    public token: string = "";
    public isAuthenticated: boolean = false;
}
