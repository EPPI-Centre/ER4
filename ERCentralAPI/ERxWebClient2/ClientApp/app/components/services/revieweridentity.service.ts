import { Component, Inject, Injectable, EventEmitter, Output, ViewChild, OnDestroy } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router, RouteReuseStrategy } from '@angular/router';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { Observable, throwError, of, Subject, timer, Subscription } from 'rxjs';
import { catchError, retry, map, takeUntil } from 'rxjs/operators';
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

export class ReviewerIdentityService implements OnDestroy {

    constructor(private router: Router,
        private _httpC: HttpClient,
        private ReviewInfoService: ReviewInfoService,
        @Inject('BASE_URL') private _baseUrl: string,
        private customRouteReuseStrategy: RouteReuseStrategy,
        private ReviewerTermsService: ReviewerTermsService,
        private modalService: ModalService
    ) {
        console.log("Creating RI.");
    }

    private ID: number = Math.random();
    @Output() OpeningNewReview = new EventEmitter();
    private _reviewerIdentity: ReviewerIdentity = new ReviewerIdentity;
    public exLgtCheck: LogonTicketCheck = new LogonTicketCheck("", "");
    public modalMsg: string = '';
    public timerObj: any | undefined;
    private killTrigger: Subject<void> = new Subject();
    private LogonTicketTimerSubscription: Subscription | null = null;
    private _currentStatus: string = 'No message yet.';
    public get currentStatus(): string {
        if (!this.timerObj && this.reviewerIdentity.userId != 0
            && this.reviewerIdentity.reviewId != 0
            && this.reviewerIdentity.ticket
            && this.reviewerIdentity.ticket.length > 0) {
            //Strange/Wrong: user appears to be logged on a review, logonticket is set (user , but the timer to check status isn't...
            this.KillLogonTicketTimer();
            this.StartLogonTicketTimer();
        }
        return this._currentStatus;
    }

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
        this.Save();
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
                    //this.Save();
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
        this._currentStatus = msg;
    }

    public FetchCurrentTicket() {

        return this._reviewerIdentity.ticket;

    }





    public LogonTicketCheckAPI(u: string, g: string) {

        // Have to override here; ask Sergio...
        //Sergio Says: this happens when:
        //1. we logout (this._reviewerIdentity.ticket == ""), timer still ticks, comes here, but we shouldn't check the ticket...
        //2. we change review, ticket changes, we should not check the old ticket...
        //special "return" value below tells the timer to kill itself!
        if (this._reviewerIdentity.ticket == ""
            || this._reviewerIdentity.ticket != g
        ) {
            let res = new Promise<any>((resolve, reject) => { resolve({ result: 'no (local) user' }); });
            return res;
        }
        g = this._reviewerIdentity.ticket;

        let LgtC = new LogonTicketCheck(u, g);

        return this._httpC.post<any>(this._baseUrl + 'api/LogonTicketCheck/ExcecuteCheckTicketExpirationCommand',
            LgtC).toPromise();
    }

    //@Output() OpeningNewReviewCoding = new EventEmitter();
    public LoginToReview(RevId: number) {
        (this.customRouteReuseStrategy as CustomRouteReuseStrategy).Clear();
        this.KillLogonTicketTimer();//kills the timer
        let body = JSON.stringify({ Value: RevId });
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body).subscribe(ri => {

                this.reviewerIdentity = ri;

                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === RevId) {
                    this.StartLogonTicketTimer();
                    //this.Save();
                    this.ReviewInfoService.Fetch();
                    this.ReviewerTermsService.Fetch();
                    this.router.onSameUrlNavigation = "reload";
                    this.OpeningNewReview.emit();
                    this.router.navigate(['MainCodingOnly']);
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
        this.KillLogonTicketTimer();
        let body = JSON.stringify({ Value: RevId });
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body).subscribe(ri => {

                this.reviewerIdentity = ri;

                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === RevId) {

                    //this.Save();
                    this.StartLogonTicketTimer();
                    this.ReviewInfoService.Fetch();
                    this.ReviewerTermsService.Fetch();
                    this.router.onSameUrlNavigation = "reload";
                    this.OpeningNewReview.emit();

                    this.router.navigate(['Main']);
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
    private StartLogonTicketTimer() {
        this.timerObj = timer(15000, 45000).pipe(
            takeUntil(this.killTrigger));
        if (this.LogonTicketTimerSubscription) this.LogonTicketTimerSubscription.unsubscribe();
        this.LogonTicketTimerSubscription = this.timerObj.subscribe(() => this.LogonTicketCheckTimer());
    }
    private KillLogonTicketTimer() {
        if (this.LogonTicketTimerSubscription) this.LogonTicketTimerSubscription.unsubscribe();//make extra sure we don't oversubscribe!
        if (this.killTrigger) this.killTrigger.next();//kills the timer
        this.timerObj = undefined;
    }
    private LogonTicketCheckTimer() {

        let user: string = String(this.reviewerIdentity.userId);
        let guid: string = this.reviewerIdentity.ticket;
        console.log("check timer:", this.ID, user, guid);
        this.LogonTicketCheckAPI(user, guid).then(
            success => {
                if (success.result == "Valid") {
                    this.UpdateStatus(success.serverMessage);
                }
                else if (success.result == "no (local) user") {
                    console.log('Silently killing the timer, user is out (or changed review)!')
                    if (this.timerObj) this.KillLogonTicketTimer();
                }
                else {
                    if (this.timerObj) this.KillLogonTicketTimer();
                    let msg: string = "Sorry, you have been logged off automatically.\n" + "<br/>";
                    switch (success.result) {
                        case "Expired":
                            msg += "Your session has been inactive for too long.\n" + "<br/>";
                            break;
                        case "Invalid":
                            msg += "Someone has logged on with the same credentials you are using.\n" + "<br/>";
                            msg += "This is not allowed in EPPI-Reviewer. If you believe that someone is using your credentials without permission, " + "<br/>";
                            msg += "you should contact EPPI-Reviewer support.\n" + "<br/>";
                            break;
                        case "None":
                            msg += "Your session has become invalid for unrecognised reasons (Return code = NONE).\n" + "<br/>";
                            msg += "Please contact EPPI-Reviewer support.\n" + "<br/>";
                            break;
                        case "Multiple":
                            //we need to add Archie-specific cases in here.
                            msg += "Your session has become invalid for unrecognised reasons (Return code = NONE).\n";
                            msg += "Please contact EPPI-Reviewer support.\n";
                            break;
                    }
                    msg += "You will be asked to logon again when you close this message."

                    //this.modalMsg = msg;
                    this.modalService.SendBackHomeWithError(msg);
                    //this.openMsgAndSendHome(this.content);
                }
            },
            error => {
                if (this.timerObj) this.KillLogonTicketTimer();
                this.handleError(error.status);
            });
    }
    //used by the logonTicket Check...
    private handleError(error: any) {
        let httpErrorCode = error;
        let msg: string = "";
        switch (httpErrorCode) {
            case 401:

                msg = 'Sorry, your session expired, please log-in again.';
                break;
            case 403:
                msg = 'Sorry, you lost your connection with the server, system will log you off to prevent losing your changes. Error code is 403.';

                break;
            case 400:
                msg = 'Sorry, you lost your connection with the server, system will log you off to prevent losing your changes. Error code is 400.';

                break;
            case 404:
                msg = 'Sorry, you lost your connection with the server, system will log you off to prevent losing your changes. Error code is 404.';

                break;
            default:
                this.modalMsg = 'Sorry, you lost your connection with the server, system will log you off to prevent losing your changes.';

        }
        this.modalService.SendBackHomeWithError(msg);
        //this.openMsgAndSendHome(this.content);
    }
    ngOnDestroy() {
        console.log("destroying RI ! ! ! ! ! !ngOnDestroy");
        if (this.timerObj) this.killTrigger.next();
        if (this.LogonTicketTimerSubscription) this.LogonTicketTimerSubscription.unsubscribe();
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


