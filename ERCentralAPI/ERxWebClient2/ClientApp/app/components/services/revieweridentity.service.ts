import { Component, Inject, Injectable, EventEmitter, Output, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router, RouteReuseStrategy } from '@angular/router';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { Observable, throwError, of } from 'rxjs';
import { catchError, retry, map } from 'rxjs/operators';
import { NgbModal, ModalDismissReasons } from '@ng-bootstrap/ng-bootstrap';
import { ReviewSetsService } from './ReviewSets.service';
import { error } from '@angular/compiler/src/util';
import { ReviewerTermsService } from './ReviewerTerms.service';
import { ModalService } from './modal.service';
import { take } from 'lodash';
import { CustomRouteReuseStrategy } from '../helpers/CustomRouteReuseStrategy';


@Injectable({
    providedIn: 'root',
})

export class ReviewerIdentityService {

    constructor(private router: Router,
        private _httpC: HttpClient,
        private ReviewInfoService: ReviewInfoService,
        @Inject('BASE_URL') private _baseUrl: string,
        private customRouteReuseStrategy: RouteReuseStrategy,
        private ReviewerTermsService: ReviewerTermsService,
        private modalService: ModalService
    ) { }

    private _reviewerIdentity: ReviewerIdentity = new ReviewerIdentity;
    public currentStatus: string = 'No message yet.';
    public exLgtCheck: LogonTicketCheck = new LogonTicketCheck("", "");
    public modalMsg: string = '';

    public get reviewerIdentity(): ReviewerIdentity {

        if (this._reviewerIdentity.userId == 0) {
            const userJson = localStorage.getItem('currentErUser');
            let currentUser: ReviewerIdentity = userJson !== null ? JSON.parse(userJson) : new ReviewerIdentity();

            if (currentUser == undefined || currentUser == null || currentUser.userId == 0) {

                return this._reviewerIdentity;
            }
            else {

                this._reviewerIdentity = currentUser;
            }
        }

        return this._reviewerIdentity;
    }

    public get HasWriteRights(): boolean {
        if (!this.reviewerIdentity || !this.reviewerIdentity.reviewId || this.reviewerIdentity.reviewId == 0 || !this.reviewerIdentity.roles) return false;
        else if (this.reviewerIdentity.roles.indexOf('ReadOnlyUser') == -1) {
            return true;
        }
        else return false;
    }
    public set reviewerIdentity(ri: ReviewerIdentity) {
        this._reviewerIdentity = ri;
    }
    public Report() {
        console.log('Reporting Cid: ' + this.reviewerIdentity.userId);
        console.log('NAME: ' + this.reviewerIdentity.name);
        console.log('Token: ' + this.reviewerIdentity.token);
        console.log('Ticket: ' + this.reviewerIdentity.ticket);
        console.log('Expires on: ' + this.reviewerIdentity.accountExpiration);
    }
    @Output() LoginFailed = new EventEmitter();
    public LoginReq(u: string, p: string) {
        (this.customRouteReuseStrategy as CustomRouteReuseStrategy).Clear();
        let reqpar = new LoginCreds(u, p);
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/Login',
            reqpar).subscribe(ri => {

                this.reviewerIdentity = ri;

                if (this.reviewerIdentity.userId > 0) {
                    this.Save();
                    this.router.navigate(['intropage']);
                }
            }, error => {
                ////check error is 401, if it is show modal and on modal close, go home
                //if (error = 401) this.SendBackHome();

                this.LoginFailed.emit();
            }
            );

    }

    public UpdateStatus(msg: string) {

        this.currentStatus = msg;

    }

    public FetchCurrentTicket() {

        return this._reviewerIdentity.ticket;

    }

    public LogonTicketCheckAPI(u: string, g: string) {

        // Have to override here; ask Sergio...
        g = this._reviewerIdentity.ticket;

        let LgtC = new LogonTicketCheck(u, g);

        return this._httpC.post<any>(this._baseUrl + 'api/LogonTicketCheck/ExcecuteCheckTicketExpirationCommand',
            LgtC).toPromise();
    }


    @Output() OpeningNewReview = new EventEmitter();
    public LoginToReview(RevId: number) {
        (this.customRouteReuseStrategy as CustomRouteReuseStrategy).Clear();
        let body = JSON.stringify({ Value: RevId });
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body).subscribe(ri => {

                this.reviewerIdentity = ri;

                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === RevId) {

                    this.Save();
                    this.ReviewInfoService.Fetch();
                    this.ReviewerTermsService.Fetch();
                    this.router.onSameUrlNavigation = "reload";
                    this.OpeningNewReview.emit();
                    this.router.navigate(['main']);
                }
            }
                , error => {
                    console.log(error);
                    this.modalService.SendBackHomeWithError(error);
                }
            );

    }


    public LoginToFullReview(RevId: number) {

        (this.customRouteReuseStrategy as CustomRouteReuseStrategy).Clear();
        let body = JSON.stringify({ Value: RevId });
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body).subscribe(ri => {

                this.reviewerIdentity = ri;

                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === RevId) {

                    this.Save();
                    this.ReviewInfoService.Fetch();
                    this.ReviewerTermsService.Fetch();
                    this.router.onSameUrlNavigation = "reload";
                    this.OpeningNewReview.emit();

                    this.router.navigate(['mainFullReview']);
                }
            }
                , error => {
                    console.log(error);
                    this.modalService.SendBackHomeWithError(error);
                }
            );

    }

    public Save() {
        //if (isPlatformBrowser(this._platformId)) {

        if (this._reviewerIdentity.userId != 0) {
            localStorage.setItem('currentErUser', JSON.stringify(this._reviewerIdentity));
        }
        else if (localStorage.getItem('currentErUser'))//to be confirmed!! 
        {
            localStorage.removeItem('currentErUser');
        }
    }
    getDaysLeftAccount() {
        return this.reviewerIdentity.daysLeftAccount;
    }
    getDaysLeftReview() {
        if (this.reviewerIdentity && this.reviewerIdentity.reviewExpiration
            && this.reviewerIdentity.reviewExpiration.length >= 4
            && this.reviewerIdentity.reviewExpiration.substring(0, 4) == '3000') return -999999;
        else if (this.reviewerIdentity.daysLeftReview) return this.reviewerIdentity.daysLeftReview;
        else return 100;
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

class LogonTicketCheck {
    constructor(u: string, g: string) {
        this.userId = u;
        this.gUID = g;
    }
    public userId: string = "";
    public gUID: string = "";
    public result: string = "";
    public serverMessage: string = "";
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
    public daysLeftAccount: number = 0;
    public daysLeftReview: number = 0;
}


