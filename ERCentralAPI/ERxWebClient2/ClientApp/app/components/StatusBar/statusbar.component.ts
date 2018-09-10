import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationContactList.service'
import { Criteria, ItemList } from '../services/ItemList.service'
import { WorkAllocationContactListComp } from '../WorkAllocationContactList/workAllocationContactListComp.component';
import { ItemListService } from '../services/ItemList.service'
import { ItemListComp } from '../ItemList/itemListComp.component';
import { FetchReadOnlyReviewsComponent } from '../readonlyreviews/readonlyreviews.component';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { timer, Subject, Subscription, Observable } from 'rxjs';
import { take, map, takeUntil } from 'rxjs/operators';
import { NgbModal, ModalDismissReasons } from '@ng-bootstrap/ng-bootstrap';
import { Response } from '@angular/http';
import { ErrorHandler } from "@angular/core";
import { UNAUTHORIZED, BAD_REQUEST, FORBIDDEN, NOT_FOUND } from "http-status-codes/index";
import { DomSanitizer } from '@angular/platform-browser';


@Component({
    selector: 'statusbar',
    templateUrl: './statusbar.component.html',
    providers: []
})

export class StatusBarComponent implements OnInit {

    @ViewChild('content') private content: any;

    constructor(private router: Router,
                private _httpC: HttpClient,
                @Inject('BASE_URL') private _baseUrl: string,
                public ReviewerIdentityServ: ReviewerIdentityService,
                private ReviewInfoService: ReviewInfoService,
        private modalService: NgbModal,
        public sanitizer: DomSanitizer
    ) {    }

    private killTrigger: Subject<void> = new Subject();
    public timerObj: any | undefined;
    public count: number = 60;
    public modalMsg: string = '';
    public testlgtC: any;
    public statusClass: string = 'bg-light';
    public IsAdmin: boolean = false;
    private isMoreButtonVisible = false;
    private fullMsg: string = '';
     
    getDaysLeftAccount() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftAccount;
    }
    getDaysLeftReview() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftReview;
    }

    ngOnDestroy() {

        if (this.timerObj) this.killTrigger.next();
    }

    ngOnInit() {

        this.IsAdmin = this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin;

        //localStorage.getItem('currentErUser');

        let guid = this.ReviewerIdentityServ.reviewerIdentity.ticket;
        let uu = String(this.ReviewerIdentityServ.reviewerIdentity.userId);
        
        if (guid != undefined && uu != '') {

            this.timerObj = timer(3000, 3000).pipe(
                takeUntil(this.killTrigger));

            this.timerObj.subscribe(() => this.LogonTicketCheckTimer(uu, guid));

        }

    }

    public UpdateStatus(msg: string) {

        let msgSt: string = msg;
        if (msg.substr(0, 1) == "!") {

            this.statusClass = "bg-warning";
            msgSt = "Status: " + msg.substr(1).trim();

        }
        if (msgSt.length > 80) {

            this.fullMsg = msgSt;
            this.ReviewerIdentityServ.currentStatus = msgSt.substr(0, 80);
            this.isMoreButtonVisible = true;

        } else {

            this.ReviewerIdentityServ.currentStatus = msgSt;
        }
        

    }

    pressedMore() {

        this.modalMsg = this.fullMsg;
        this.openMsg(this.content);

    }

    public handleError(error: any) {

        console.error(error);
        let httpErrorCode = error.httpErrorCode;
        // For now we set all the error messages to the
        // same message.
        this.modalMsg = "We are sorry, you have lost communication with the server. To avoid data corruption, the page will now reload.\n" +
                    "This message may appear if you didn't log out during a software update.\n" +
                     "Note that Eppi-Reviewer might fail to load until the update is completed, please wait a couple of minutes and try again.";

        switch (httpErrorCode) {
            case UNAUTHORIZED:
               
                this.openMsg(this.content);
                break;
            case FORBIDDEN:
             
                this.openMsg(this.content);
                break;
            case BAD_REQUEST:
               
                this.openMsg(this.content);
                break;
            case NOT_FOUND:
               
                this.openMsg(this.content);
                break;
            default:
             
                this.openMsg(this.content);

        }
    }

    LogonTicketCheckTimer(user: string, guid: string) {

        this.ReviewerIdentityServ.LogonTicketCheckAPI(user, guid).then(

            success => {
                if (success.result == "Valid") {
                    this.UpdateStatus(success.serverMessage);

                }else {

                    if (this.timerObj) this.killTrigger.next();

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
                    this.modalMsg = msg;
                    this.openMsg(this.content);
                }


            },
            error => {

                console.log('An unauthorized worked correctly...' + error);

                if (this.timerObj) this.killTrigger.next();

                this.handleError(error);

          });
    }

    //https://ng-bootstrap.github.io/#/components/modal/examples
    openMsg(content : any) {
        this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' }).result.then((res) => {

                    //alert('Simulate application returning to logon page: ' + res);

        },
        (res) => {
                    //alert('Continue for debugging purposes: ' + res)
                    if (!this.isMoreButtonVisible == true) {
                        this.router.navigate(['home']);
                    }
                }
        );
    }

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


