import { Component, Inject, Injectable } from '@angular/core';
//import { Http, Response, Headers, RequestOptions, RequestMethod } from '@angular/http';
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
        @Inject('BASE_URL') private _baseUrl: string) {

        //this._baseUrl = baseUrl;
    }
    public reviewerIdentity: ReviewerIdentity = new ReviewerIdentity;
    //private headers: Headers = new Headers({ "Content-Type": 'application/x-www-form-urlencoded' });
    //private headers2 = new HttpHeaders().set('content-type', 'application/json');
    //private headers2: Headers = new Headers({ "Content-Type": 'application/json' });
    private header = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });
    public Report()  {
        console.log('Reporting Cid: ' + this.reviewerIdentity.userId);
        console.log('NAME: ' + this.reviewerIdentity.name);
        console.log('Token: ' + this.reviewerIdentity.token);
        console.log('Ticket: ' + this.reviewerIdentity.ticket);
        console.log('Expires on: ' + this.reviewerIdentity.accountExpiration);
    }
    public Login(u: string, p: string) {
        this.LoginReq2(u, p).subscribe(ri => this.reviewerIdentity = ri);
        this.Report();
    }
    
    public LoginReq2(u: string, p: string) {
        let body2 = "Username=" + u + "&Password=" + p;
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/x-www-form-urlencoded'
            })
        };
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/Login',
            body2, httpOptions
        );
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
