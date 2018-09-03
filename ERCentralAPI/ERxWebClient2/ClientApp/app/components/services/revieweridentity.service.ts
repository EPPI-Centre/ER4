import { Component, Inject, Injectable, EventEmitter } from '@angular/core';
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

        if (isPlatformBrowser(this._platformId)) {

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

                if (this.reviewerIdentity.userId > 0) {
                    this.Save();
                    this.router.navigate(['readonlyreviews']);
                }
            });

    }

    public UpdateStatus(msg: string) {

        this.currentStatus = msg;

    }


    public LogonTicketCheckAPI(u: string, g: string) {

        let LgtC = new LogonTicketCheck(u, g);

        return this._httpC.post<LogonTicketCheck>(this._baseUrl + 'api/LogonTicketCheck/ExcecuteCheckTicketExpirationCommand',
            LgtC).toPromise();
    }

    // Make a call to the stored proc in the CSLA BO
    //public LogonTicketCheckExpiration(u: string, g: string) {
       
    //    let LgtC = new LogonTicketCheck(u, g);

    //    return this._httpC.post<LogonTicketCheck>(this._baseUrl + 'api/LogonTicketCheck/ExcecuteCheckTicketExpirationCommand',
    //        LgtC).subscribe(lgtC => {

    //            if (lgtC != null) {

    //                this.exLgtCheck = lgtC;
    //                console.log('inside subscription: ' + this.exLgtCheck.result);
    //                if (lgtC.result == "Valid") {

    //                    this.UpdateStatus(lgtC.serverMessage);
    //                }
    //                else {

    //                    let msg: string = "Sorry, you have been logged off automatically.\n";
    //                    switch (lgtC.result) {
    //                        case "Expired":
    //                            msg += "Your session has been inactive for too long.\n"
    //                            break;
    //                        case "Invalid":
    //                            msg += "Someone has logged on with the same credentials you are using.\n";
    //                            msg += "This is not allowed in ER4. If you believe that someone is using your credentials without permission, ";
    //                            msg += "you should contact the ER4 support.\n";
    //                            break;
    //                        case "None":
    //                            msg += "Your session has become invalid for unrecognised reasons (Return code = NONE).\n";
    //                            msg += "Please contact the ER4 support team.\n";
    //                            break;
    //                        case "Multiple":
    //                        // CHECK WITH SERGIO
    //                        //if (this.reviewerIdentity.IsCochraneUser) {
    //                        //    msg += "Your session has become invalid.\n";
    //                        //    msg += "Most likely, the Cochrane review you have open has become 'Read-Only'.\n";
    //                        //    msg += "This would happen if the review got Checked-In in Archie\n";
    //                        //    msg += "(or someone undid the check-out).\n";
    //                        //    msg += "If you think this wasn't the case, please contact EPPISupport.\n";
    //                        //}
    //                        //else {
    //                        //    msg += "Your session has become invalid for unrecognised reasons (Return code = MULTIPLE).\n";
    //                        //    msg += "Please contact the ER4 support team.\n";
    //                        //}
    //                        //break;
    //                    }
        
    //                    //this.openMsg("content");
    //                    //this.openMsg("You will be asked to logon again when you close this message.");
    //                    //string res = MessageBox.Show(msg + "You will be asked to logon again when you close this message.").ToString();
    //                    //System.Windows.Browser.HtmlPage.Window.Invoke("Refresh");
    //                }
    //            }
    //            else {

    //                // if (e.Error.GetType() == (new System.Reflection.TargetInvocationException(new Exception()).GetType())) {

    //                //     UpdateStatus("!You have lost the connection with our server, please check your Internet connection.\n"  +
    //                //         "This message will revert to normal when the connection will be re-established:\n" +
    //                //         "Please keep in mind that data changes made while disconnected cannot be saved.\n" +
    //                //         "If your Internet connection is working, we might be experiencing some technical problems,\n" +
    //                //         "We apologise for the inconvenience.");
    //                //     return;
    //                // }
    //                ////windowMOTD.Tag = "failure";
    //                //// windowMOTD.MOTDtextBlock.Text = "We are sorry, you have lost communication with the server. To avoid data corruption, the page will now reload.\n" +
    //                //     "This message may appear if you didn't log out during a software update.\n" +
    //                //     "Note that Eppi-Reviewer might fail to load until the update is completed, please wait a couple of minutes and try again.";
    //                // //windowMOTD.Show();
    //                this.router.navigate(['home']);
    //            }
    //        }
    //            , err => {
    //                this.handleError(err);
    //                console.log('Something went wrong!' + err);
    //                this.router.navigate(['home']);
    //        })
    //}

    openMsg(content: any) {
        this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' }).result.then(() => {
            alert('closed');
        },
            () => { alert('dismissed') }
        );
    }


    private handleError(error: HttpErrorResponse) {
        if (error.error instanceof ErrorEvent) {
            // A client-side or network error occurred. Handle it accordingly.
            console.error('An error occurred:', error.error.message);
        } else {
            // The backend returned an unsuccessful response code.
            // The response body may contain clues as to what went wrong,
            console.error(
                `Backend returned code ${error.status}, ` +
                `body was: ${error.error}`);
        }
        // return an observable with a user-facing error message
        //return throwError(
        //    'Something bad happened; please try again later.');
    };

    public LoginToReview(RevId: number, OpeningNewReview: EventEmitter<any>) {
        //data: JSON.stringify({ FilterName: "Dirty Deeds" })
        let body = JSON.stringify({ Value: RevId });
        return this._httpC.post<ReviewerIdentity>(this._baseUrl + 'api/Login/LoginToReview',
            body).subscribe(ri => {

                this.reviewerIdentity = ri;
                //console.log('login to Review: ' + this.reviewerIdentity.userId);
                if (this.reviewerIdentity.userId > 0 && this.reviewerIdentity.reviewId === RevId) {
                    //console.log('sdfhaskjdf sakdjfhkasjdfhwuewjhdf' + this.reviewerIdentity.userId);
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



