import { Component, Inject, Injectable, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { ReviewInfoService } from '../services/ReviewInfo.service'

@Injectable({
    providedIn: 'root',
    }
)

export class ReviewerIdentityService {

    constructor(private router: Router, //private _http: Http, 
        private _httpC: HttpClient,
        private ReviewInfoService: ReviewInfoService,
        @Inject('BASE_URL') private _baseUrl: string
        , @Inject(PLATFORM_ID) private _platformId: Object) { }

    private _reviewerIdentity: ReviewerIdentity = new ReviewerIdentity;

    public get reviewerIdentity(): ReviewerIdentity {

        if (isPlatformBrowser(this._platformId)) {

            if (this._reviewerIdentity.userId == 0) {

                //console.log("before LS: " + this._platformId);
                const userJson = localStorage.getItem('currentErUser');
                let currentUser: ReviewerIdentity = userJson !== null ? JSON.parse(userJson) : new ReviewerIdentity();
                //let tmp: any = localStorage.getItem('currentErUser');
                //console.log("after LS: " + this._platformId);
                    //let tmp2: ReviewerIdentity = tmp;
                if (currentUser == undefined || currentUser == null || currentUser.userId == 0) {

                    return this._reviewerIdentity;
                }
                else {
                    //console.log("Got User from LS");
                    this._reviewerIdentity = currentUser;
                }
            }
        }
            
        return this._reviewerIdentity;

    }

    public get HasWriteRights(): boolean {
        if (!this.reviewerIdentity || !this.reviewerIdentity.reviewId || this.reviewerIdentity.reviewId == 0 || !this.reviewerIdentity.roles) return false;
        else if (this.reviewerIdentity.roles.indexOf('ReadOnly') == -1) return true;
        else return false;
    }
    public set reviewerIdentity(ri: ReviewerIdentity) {
        this._reviewerIdentity = ri;
    }
    public Report()  {
        console.log('Reporting Cid: ' + this.reviewerIdentity.userId);
        console.log('NAME: ' + this.reviewerIdentity.name);
        console.log('Token: ' + this.reviewerIdentity.token);
        console.log('Ticket: ' + this.reviewerIdentity.ticket);
        console.log('Expires on: ' + this.reviewerIdentity.accountExpiration);
    }
      
    
    
    public LoginReq(u: string, p: string) {
        let reqpar = new LoginCreds(u, p);
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/Login',
            reqpar).subscribe(ri => {
                this.reviewerIdentity = ri;
                //console.log('home login: ' + this.reviewerIdentity.userId);
                if (this.reviewerIdentity.userId > 0) {
                    this.Save();
                    this.router.navigate(['readonlyreviews']);
                }
            });
            //body);
    }
    public LoginToReview(RevId: number, OpeningNewReview: EventEmitter<any>) {
        //data: JSON.stringify({ FilterName: "Dirty Deeds" })
        let body = JSON.stringify({ Value: RevId });
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body).subscribe(ri => {

                this.reviewerIdentity = ri;
                //console.log('login to Review: ' + this.reviewerIdentity.userId);
                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === RevId) {
                    this.Save();
                    this.router.onSameUrlNavigation = "reload";
                    OpeningNewReview.emit();
                    this.router.navigate(['main']);
                }
            });
      
    }
    public Save() {
        //if (isPlatformBrowser(this._platformId)) {
            if (this._reviewerIdentity.userId != 0)
                localStorage.setItem('currentErUser', JSON.stringify(this._reviewerIdentity));
            else if (localStorage.getItem('currentErUser'))//to be confirmed!! 
                localStorage.removeItem('currentErUser');
        //}
    }
}

class LoginCreds {
    constructor(u: string, p: string) {
        this.Username = u;
        this.Password = p;
    }
    public Username: string = "";
    public Password: string = "";
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



