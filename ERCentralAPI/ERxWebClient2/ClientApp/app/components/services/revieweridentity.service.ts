import { Component, Inject, Injectable, EventEmitter, Output } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { AppComponent } from '../app/app.component'
import { HttpClient, HttpHeaders, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { isPlatformServer, isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { Observable, throwError, of } from 'rxjs';
import { catchError, retry, map } from 'rxjs/operators';
import { NgbModal, ModalDismissReasons } from '@ng-bootstrap/ng-bootstrap';
import { ReviewSetsService } from './ReviewSets.service';

@Injectable({
    providedIn: 'root',
    }
)

export class ReviewerIdentityService {
    constructor(private router: Router, //private _http: Http, 
        private _httpC: HttpClient,
        private ReviewInfoService: ReviewInfoService,
        @Inject('BASE_URL') private _baseUrl: string
        , @Inject(PLATFORM_ID) private _platformId: Object,
        private modalService: NgbModal) { }

    private _reviewerIdentity: ReviewerIdentity = new ReviewerIdentity;
    public currentStatus: string = '';
    public exLgtCheck: LogonTicketCheck = new LogonTicketCheck("", "");

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

                if (this.reviewerIdentity.userId > 0) {
                    this.Save();
                    this.router.navigate(['intropage']);
                }
            });

    }

    public FetchCurrentRI() {

        let reqpar = new LoginCreds('1799', 'CrapBirkbeck1');
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/Login',
            reqpar).subscribe(ri => {
                this.reviewerIdentity = ri;

                if (this.reviewerIdentity.userId > 0) {
                    this.Save();
                }
            });
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

        return this._httpC.post<LogonTicketCheck>(this._baseUrl + 'api/LogonTicketCheck/ExcecuteCheckTicketExpirationCommand',
            LgtC).toPromise();
    }
       

    openMsg(content: any) {
        this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' }).result.then(() => {
            //alert('closed');
        },
            () => {
                //alert('dismissed')
            }
        );
    }

    @Output() OpeningNewReview = new EventEmitter();
    public LoginToReview(RevId: number) {
        //data: JSON.stringify({ FilterName: "Dirty Deeds" })
        //this.ReviewSetsService.Clear();
        //console.log('Opening a review');
        
        let body = JSON.stringify({ Value: RevId });
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body).subscribe(ri => {

                this.reviewerIdentity = ri;
      
                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === RevId) {
                    console.log('got into it');
                    this.Save();
                    this.router.onSameUrlNavigation = "reload";
                    this.OpeningNewReview.emit();
                    this.router.navigate(['main']);
                }
            });
      
    }
    public Save() {
        //if (isPlatformBrowser(this._platformId)) {

        if (this._reviewerIdentity.userId != 0) {
            localStorage.setItem('currentErUser', JSON.stringify(this._reviewerIdentity));
            
        }
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


