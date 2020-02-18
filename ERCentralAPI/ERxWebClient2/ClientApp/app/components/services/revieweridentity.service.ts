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
import { stack } from '@progress/kendo-drawing';


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
    private _IsCodingOnly: boolean = false;
    public get IsCodingOnly(): boolean {
        return this._IsCodingOnly;
    }
    public get currentStatus(): string {
        //console.log("getting status: ")
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
    private _userOptions: UserOptions = new UserOptions();
    public get userOptions(): UserOptions {
        if (this._reviewerIdentity.userId == 0) {
            this._userOptions = new UserOptions();
        }
        else if (this._userOptions.persistingOptions == null) {
            const userJson = localStorage.getItem('currentErUserOptions'+ this._reviewerIdentity.userId);
            let PuserOptions: PersistingOptions = userJson !== null ? JSON.parse(userJson) : new PersistingOptions();
            if (PuserOptions == undefined || PuserOptions == null ) {
                this._userOptions.persistingOptions = new PersistingOptions();
            }
            else {
                this._userOptions.persistingOptions = PuserOptions;
            }
        }
        return this._userOptions;
    }
    //always set the options to persist values by calling 
        //this.ReviewerIdentityServ.userOptions = this.ReviewerIdentityServ.userOptions;
    public set userOptions(options: UserOptions) {
        this._userOptions = options;
        if (this._userOptions.persistingOptions == null) {
            const userJson = localStorage.getItem('currentErUserOptions' + this._reviewerIdentity.userId);
            let PuserOptions: PersistingOptions = userJson !== null ? JSON.parse(userJson) : new PersistingOptions();
            if (PuserOptions == undefined || PuserOptions == null) {
                this._userOptions.persistingOptions = new PersistingOptions();
            }
            else {
                this._userOptions.persistingOptions = PuserOptions;
            }
        }
        this.SaveOptions();
    }
    public SaveOptions() {
        console.log("Saving options: ", this._userOptions.persistingOptions);
        if (this._reviewerIdentity.userId == 0) return;
        if (this._userOptions.persistingOptions != null) {
            localStorage.setItem('currentErUserOptions' + this.reviewerIdentity.userId, JSON.stringify(this._userOptions.persistingOptions));
        }
        else if (localStorage.getItem('currentErUserOptions' + this.reviewerIdentity.userId))//to be confirmed!! 
        {
            localStorage.removeItem('currentErUserOptions' + this.reviewerIdentity.userId);
        }
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
    public get HasAdminRights(): boolean {
        if (!this.reviewerIdentity || !this.reviewerIdentity.reviewId || this.reviewerIdentity.reviewId == 0 || !this.reviewerIdentity.roles) return false;
        return this.reviewerIdentity.roles.indexOf('AdminUser') != -1;
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
        //console.log('Logon ticket API call, myID', this.ID);
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

    public LoginReq(u: string, p: string) {
        //(this.customRouteReuseStrategy as CustomRouteReuseStrategy).Clear();
        this.userOptions = new UserOptions();
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

    public LoginViaArchieReq(code: string, state: string): Promise<ReviewerIdentity | undefined > {
        //(this.customRouteReuseStrategy as CustomRouteReuseStrategy).Clear();
        this.userOptions = new UserOptions();
        let reqpar = new ArchieLoginCreds(code, state);
        if (this.reviewerIdentity.userId == 0 && this.reviewerIdentity.isAuthenticated
            && this.reviewerIdentity.ticket == ""
            && this.reviewerIdentity.roles.indexOf('CochraneUser') != -1
            && this.reviewerIdentity.token && this.reviewerIdentity.token.length > 20
        ) {
            //very special case! 
            //User logged on via Archie and created the ER Account on the fly,
            //so we'll send also the RI, this will tell the controller that
            //it needs to use the special (one-off) authentication route (get RI via code & state, but from local DB!)
            reqpar.reviewerIdentity = this.reviewerIdentity;
        }
            //this.reviewerIdentity.roles.indexOf('ReadOnlyUser') == -1
        return this._httpC.post<ArchieLoginCreds>(this._baseUrl + 'api/Login/LoginFromArchie',
            reqpar).toPromise().then((res) => {
                console.log("LoginViaArchieReq: ", res);
                if (res.error !== "") {
                    //to be confirmed
                    console.log("LoginViaArchieReq: ", 1);
                    let fakeResult = new ReviewerIdentity();
                    fakeResult.userId = -1;
                    fakeResult.reviewId = -1;
                    fakeResult.name = '{ERROR: In Result}';
                    fakeResult.ticket = res.error;
                    return fakeResult;
                }
                else if (res.reviewerIdentity && res.reviewerIdentity.name == "{UnidentifiedArchieUser}") {
                    this.reviewerIdentity = res.reviewerIdentity;//in this way current token is used and user will have access to needed API endpoints, which require "CochraneUser" role.
                    console.log("LoginViaArchieReq: ", 2);
                    return res.reviewerIdentity;
                }
                else if (res.reviewerIdentity && res.reviewerIdentity.userId > 0) {
                    //good, things worked, we know who this is, let's let them in.
                    console.log("LoginViaArchieReq: ", 3);
                    this.reviewerIdentity = res.reviewerIdentity;
                    this.router.navigate(['intropage']);
                    return this.reviewerIdentity;
                }
            }, error => {
                console.log("LoginViaArchieReq: ", 4, error);
                let fakeResult = new ReviewerIdentity();
                fakeResult.userId = -1;
                fakeResult.reviewId = -1;
                fakeResult.name = '{ERROR: In API Call}';
                if (error && error.status) {
                    if (error.status && error.status == 403) {
                        fakeResult.ticket = "Login Failed";
                    }
                    else {
                        fakeResult.ticket = "Unexpected error" + (error.status ? " (" + error.status + ")" : "")
                            + (error.statusText ? ", full details are: '" + error.statusText + "." : ".");
                    }
                }
                else {
                    fakeResult.ticket = "Unexpected error (no error details).";
                }
                return fakeResult;
            }
        ).catch((err) => {
            console.log("LoginViaArchieReq: ", 5, err);
            let fakeResult = new ReviewerIdentity();
            fakeResult.userId = -1;
            fakeResult.reviewId = -1;
            fakeResult.name = '{ERROR: In Catch}';
            fakeResult.ticket = "Unexpected error (catch on Angular side).";
            return this.reviewerIdentity;
        });

    }
    public LinkToArchieAccount(code: string, state: string, un: string, pw: string): Promise<ArchieLoginCreds> {
        //(this.customRouteReuseStrategy as CustomRouteReuseStrategy).Clear();
        let reqpar = new ArchieLoginCreds(code, state);
        reqpar.loginCreds = new LoginCreds(un, pw);
        return this._httpC.post<ArchieLoginCreds>(this._baseUrl + 'api/Login/LinkToExistingAccount',
            reqpar).toPromise().then((res) => {
                if (res) {
                    if (res.error != "") {
                        console.log("LinkToArchieAccount: ", 1);
                        return res;
                    }
                    else {
                        console.log("LinkToArchieAccount: ", 2);
                        if (res.reviewerIdentity) this.reviewerIdentity = res.reviewerIdentity;
                        return res;
                    }
                }
                else {
                    console.log("LinkToArchieAccount: ", 3);
                    if (reqpar.reviewerIdentity) reqpar.reviewerIdentity = null;
                    reqpar.error = "No response from 'Link to Archie' API call.";
                    return reqpar;
                }
            }, (error: Response) => {
                console.log("LinkToArchieAccount error: ", 4, error);
                if (error && error.status) {
                    if (error.status && error.status == 403) {
                        reqpar.error = "Login Failed";
                    }
                    else {
                        reqpar.error = "Unexpected error" + (error.status ? " ("+ error.status +")" : "")
                            + (error.statusText ? ", full details are: '" + error.statusText + "." : ".");
                    }
                }
                else {
                    reqpar.error = "Unexpected error (no error details).";
                }
                return reqpar;
            }
        ).catch(caught => {
            console.log("LinkToArchieAccount catch: ", 3, caught);
            reqpar.error = "Unexpected error (catch on Angular side).";
            return reqpar;
        });

    }

    public CreateERAccountFromArchie(reqpar: iCreateER4ContactViaArchieCommandJSON): Promise<iCreateER4ContactViaArchieCommandJSON> {
        //(this.customRouteReuseStrategy as CustomRouteReuseStrategy).Clear();
        return this._httpC.post<iCreateER4ContactViaArchieCommandJSON>(this._baseUrl + 'api/Login/CreateER4ContactViaArchie',
            reqpar).toPromise().then((res) => {
                if (res) {
                    console.log("CreateERAccountFromArchie: ", 1, res);
                    return res;
                }
                else {
                    console.log("CreateERAccountFromArchie: ", 2);
                    reqpar.result = "No response from 'CreateERAccountFromArchie' API call.";
                    return reqpar;
                }
            }, (error: Response) => {
                console.log("CreateERAccountFromArchie error: ", 3, error);
                if (error && error.status) {
                    if (error.status && error.status == 403) {
                        reqpar.result = "Login Failed";
                    }
                    else if (error.status && error.status == 401) {
                        reqpar.result = "Not Authorised";
                    }
                    else {
                        reqpar.result = "Unexpected error" + (error.status ? " (" + error.status + ")" : "")
                            + (error.statusText ? ", full details are: '" + error.statusText + "." : ".");
                    }
                }
                else {
                    reqpar.result = "Unexpected error (no error details)."
                }
                return reqpar;
            }
        ).catch(caught => {
            console.log("CreateERAccountFromArchie catch: ", 3, caught);
            reqpar.result = "Unexpected error (catch on Angular side)."
            return reqpar;
        });

    }

    private CommonPreLoginToReview() {
        this.userOptions = new UserOptions();
        //(this.customRouteReuseStrategy as CustomRouteReuseStrategy).Clear();
        this.KillLogonTicketTimer();//kills the timer
        this.ReviewInfoService.Clear();
    }
    private CommonPostLoginToReview(isCodingOnly: boolean = false) {
        this.StartLogonTicketTimer();
        //this.Save();
        this.ReviewInfoService.FetchAll();
        this.ReviewerTermsService.Fetch();
        this._IsCodingOnly = isCodingOnly;
    }

    public LoginToReview(RevId: number) {
        this.CommonPreLoginToReview();
        let body = JSON.stringify({ Value: RevId });
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body).subscribe(ri => {

                this.reviewerIdentity = ri;

                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === RevId) {
                    this.router.onSameUrlNavigation = "reload";
                    this.CommonPostLoginToReview(true);
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
        this.CommonPreLoginToReview();
        let body = JSON.stringify({ Value: RevId });
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body).subscribe(ri => {

                this.reviewerIdentity = ri;

                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === RevId) {

                    //this.Save();
                    this.CommonPostLoginToReview(false);
                    this.router.onSameUrlNavigation = "reload";
                    this.router.navigate(['Main']);
                    this.OpeningNewReview.emit();
                }
            }
                , error => {
                    console.log(error);
                    this.modalService.SendBackHomeWithError(error);
                }
            );
    }
    public LoginReqSA(u: string, p: string, rid: number) {
        this.CommonPreLoginToReview();
        let cred = new LoginCredsSA(u, p, rid);//
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReviewSA',
            cred).subscribe(ri => {

                this.reviewerIdentity = ri;
                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === rid) {
                    this.CommonPostLoginToReview(false);
                    this.OpeningNewReview.emit();
                }
            }, error => {
                ////check error is 401, if it is show modal and on modal close, go home
                //if (error = 401) this.SendBackHome();
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
    public LogOut() {
        this.reviewerIdentity = new ReviewerIdentity();
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
        //console.log("StartLogonTicketTimer");
        this.timerObj = timer(15000, 45000).pipe(
            takeUntil(this.killTrigger));
        if (this.LogonTicketTimerSubscription) this.LogonTicketTimerSubscription.unsubscribe();
        this.LogonTicketTimerSubscription = this.timerObj.subscribe(() => this.LogonTicketCheckTimer());
    }
    private KillLogonTicketTimer() {
        //console.log("KillLogonTicketTimer");//, this.LogonTicketTimerSubscription, this.killTrigger);
        if (this.LogonTicketTimerSubscription) this.LogonTicketTimerSubscription.unsubscribe();//make extra sure we don't oversubscribe!
        if (this.killTrigger) {
            //console.log("this.killTrigger.next();");
            this.killTrigger.next();//kills the timer
        }
        this.timerObj = undefined;
    }
    private LogonTicketCheckTimer() {

        let user: string = String(this.reviewerIdentity.userId);
        let guid: string = this.reviewerIdentity.ticket;
        //console.log("check timer:", this.ID, user, guid);
        this.LogonTicketCheckAPI(user, guid).then(
            success => {
                //console.log("LogonTicketCheckAPI success:", success)
                if (success.result == "Valid") {
                    this.UpdateStatus(success.serverMessage);
                }
                else if (success.result == "no (local) user") {
                    console.log('Silently killing the timer, user is out (or changed review)!')
                    if (this.timerObj) {
                        this.reviewerIdentity.ticket = "";
                        this.reviewerIdentity.token = "";
                        this.Save();
                        this.KillLogonTicketTimer();
                    }
                }
                else {
                    console.log('Silently killing the timer, logon ticket is invalid!')
                    if (this.timerObj) {
                        this.reviewerIdentity.ticket = "";
                        this.reviewerIdentity.token = "";
                        this.Save();
                        this.KillLogonTicketTimer();
                    }
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
                    this.modalService.SendBackHome(msg);
                    //this.openMsgAndSendHome(this.content);
                }
            },
            error => {
                //console.log("LogonTicketCheckAPI error:", error);
                if (this.timerObj) {
                    this.reviewerIdentity.ticket = "";
                    this.reviewerIdentity.token = "";
                    this.Save();
                    this.KillLogonTicketTimer();
                }
                this.handleError(error.status);
            });
    }
    //used by the logonTicket Check...
    private handleError(error: any) {
        //console.log("handling LogonTicket check Error:", error);
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
                msg = 'Sorry, you lost your connection with the server, system will log you off to prevent losing your changes.';

        }
        this.modalService.SendBackHome(msg);
        //this.openMsgAndSendHome(this.content);
    }

    public GoToArchie() {
        //
        let url = "";
        let redirectUri = this._baseUrl.toLowerCase() + "ArchieCallBack";
        //redirectUri = "https://ssru38.ioe.ac.uk/WcfHostPortal/ArchieCallBack.aspx";//temporary!!!!!!!
        if (this._baseUrl.indexOf("://eppi.ioe.ac.uk") != -1) {
            //this is the production environment, go there
            url = "https://login.cochrane.org/auth/realms/cochrane/protocol/openid-connect/auth?client_id=eppi&response_type=code&redirect_uri=";
            //redirectUri = redirectUri.toLowerCase();//necessary because we don't know the case of _baseUrl when actual users are accessing it, but Cochrane system is likely to be case sensitive.
        }
        else {
            //go to test env
            url = "https://test-login.cochrane.org/auth/realms/cochrane/protocol/openid-connect/auth?client_id=eppi&response_type=code&redirect_uri=";
        }
        url += redirectUri + "&scope=document person&state=";
        var state = '';
        const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        const charactersLength = characters.length;
        for (var i = 0; i < 12; i++) {//generate a random string of 12 chars...
            state += characters.charAt(Math.floor(Math.random() * charactersLength));
        }
        url += state + "&access_type=offline";
        url = encodeURI(url);
        console.log("Trying this URL:", url);
        window.location.href = url;
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
class LoginCredsSA {
    constructor(u: string, p: string, rid:number) {
        this.Username = u;
        this.Password = p;
        this.RevId = rid;
    }
    public Username: string = "";
    public Password: string = "";
    public RevId: number;
}
class ArchieLoginCreds {
    constructor(Code: string, State: string) {
        this.code = Code;
        this.state = State;
    }
    public code: string = "";
    public state: string = "";
    public error: string = "";
    public loginCreds: LoginCreds | null = null;
    public reviewerIdentity: ReviewerIdentity | null = null;
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
export interface ArchieIdentity {
    archieID: string;
    error: string;
    errorReason: string;
    isAuthenticated: boolean;
}
export interface iCreateER4ContactViaArchieCommandJSON {
    code: string;
    status: string;
    username: string;
    email: string;
    fullname: string;
    password: string;
    sendNewsletter: boolean;
    createExampleReview: boolean;
    result: string;
}
export class UserOptions {
    //these persist across sessions - saved to local-storage - if the set method in revieweridentityservice is called...
    persistingOptions: PersistingOptions | null = null;
    //all options below here persist in one session, disappear after it
    AutoAdvance: boolean = false;
    get ShowHighlight() : boolean {
        if (this.persistingOptions == null) {
            this.persistingOptions = new PersistingOptions();
        }
        return this.persistingOptions.ShowHighlight;
    }
    set ShowHighlight(val: boolean) {
        if (this.persistingOptions == null) {
            this.persistingOptions = new PersistingOptions();
        }
        this.persistingOptions.ShowHighlight = val;
    }

    public get RelevantTermClass(): string {
        if (this.persistingOptions) {
            if (this.persistingOptions.HighlightsStyle == 'EPPI-Reviewer 4') {
                return 'RelevantTermER4';
            }
            else if (this.persistingOptions.HighlightsStyle == 'Black & White') {
                return 'RelevantTermBW';
            }
            else if (this.persistingOptions.HighlightsStyle == 'Subtle') {
                return 'RelevantTermFainter';
            }
            else { //should be: (this.ReviewerIdentityServ.userOptions.HighlightsStyle == 'Default')
                return 'RelevantTerm';
            }
        }
        else { //should be: (this.ReviewerIdentityServ.userOptions.HighlightsStyle == 'Default')
            return 'RelevantTerm';
        }
    }


    public get IrrelevantTermClass(): string {
        if (this.persistingOptions) {
            if (this.persistingOptions.HighlightsStyle == 'EPPI-Reviewer 4') {
                return 'IrrelevantTermER4';
            }
            else if (this.persistingOptions.HighlightsStyle == 'Black & White') {
                return 'IrrelevantTermBW';
            }
            else if (this.persistingOptions.HighlightsStyle == 'Subtle') {
                return 'IrrelevantTermFainter';
            }
            else { //(style == 'Default')
                return 'IrrelevantTerm';
            }
        }
        else { //(style == 'Default')
            return 'IrrelevantTerm';
        }
    }
}
export class PersistingOptions {
    HighlightsStyle: string = "Default";
    ShowHighlight: boolean = false;
}


