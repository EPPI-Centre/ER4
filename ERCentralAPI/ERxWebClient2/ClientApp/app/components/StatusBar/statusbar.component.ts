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
                private modalService: NgbModal
    ) {    }

    private killTrigger: Subject<void> = new Subject();
    public timerTest: any | undefined;
    public count: number = 60;
    public modalMsg: string = '';
    public testlgtC: any;
    public statusClass: string = 'bg-light';

    //timerServerCheck(u: string, g: string): Observable<LogonTicketCheck> {

    //    this.countDown = timer(0, 8000).pipe(
    //        takeUntil(this.killTrigger),
    //        map(() => {

    //            this.ReviewerIdentityServ.LogonTicketCheckExpiration(u, g);
    //        })
    //    );
    //    return this.countDown.subscribe();
    //}

    getDaysLeftAccount() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftAccount;
    }
    getDaysLeftReview() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftReview;
    }

    ngOnDestroy() {

        if (this.timerTest) this.killTrigger.next();
    }

    ngOnInit() {

        this.ReviewInfoService.Fetch();
        let guid = this.ReviewerIdentityServ.reviewerIdentity.ticket;
        let uu = String(this.ReviewerIdentityServ.reviewerIdentity.userId);
        
        if (guid != undefined && uu != '') {

            this.timerTest = timer(0, 5000).pipe(
                takeUntil(this.killTrigger));

            this.timerTest.subscribe(() => this.LogonTicketCheckTimer(uu, guid));

        }
    }
    public UpdateStatus(msg: string) {

        // NEED TO ADD THE FOLLOWING LOGIC TO THE BELOW
        let msgSt: string = "";
        if (msg.substr(0, 1) == "!") {

            //StatusContainer.Background = new SolidColorBrush(Colors.Yellow);
            this.statusClass = "bg-warning";
            msgSt = "Status: " + msg.substr(1).trim();

        }
        //if (msgSt.Length > 80) {

        //    int ii = msgSt.LastIndexOf(" ", 80, 79);
        //    string tmpStr = msgSt.Substring(0, msgSt.LastIndexOf(" ", 80, 79)) + "...";
        //    windowMOTD.MOTDtextBlock.Text = msgSt.Replace(@"\n", Environment.NewLine);
        //    textBlockMsg.Text = tmpStr.Replace(@"\n", "");
        //    viewFullMOTD_hlink.Visibility = System.Windows.Visibility.Visible;

        //}

        //else {

        //    viewFullMOTD_hlink.Visibility = System.Windows.Visibility.Collapsed;
        //    textBlockMsg.Text = msgSt.Replace(@"\n", "");
        //}

        this.ReviewerIdentityServ.currentStatus = msg;

    }
    public handleError(error: any) {
        console.error(error);
        let httpErrorCode = error.httpErrorCode;
        switch (httpErrorCode) {
            case UNAUTHORIZED:
                //this.router.navigateByUrl("/home");
                break;
            case FORBIDDEN:
                // NB: we do not have this page yet.
                //this.router.navigateByUrl("/unauthorized");
                //this.router.navigateByUrl("/home");
                break;
            case BAD_REQUEST:
                this.modalMsg = error.message;
                this.openMsg(this.content);
                  // NB: we do not have this page yet.
                //this.router.navigateByUrl("/home");
                break;
            case NOT_FOUND:
                this.modalMsg = error.message;
                this.openMsg(this.content);
                // NB: we do not have this page yet.
                //this.router.navigateByUrl("/home");
                break;
            default:
                // NB: THIS ALSO NEEDS TO BE SET
                this.modalMsg = "error here";
                this.openMsg(this.content);
                  // NB: we do not have this page yet.
                //this.router.navigateByUrl("/home");
        }
    }
    LogonTicketCheckTimer(user: string, guid: string) {

        this.ReviewerIdentityServ.LogonTicketCheckAPI(user, guid).then(

            success => {
                if (success.result == "Valid") {
                    this.UpdateStatus(success.serverMessage);

                }else {

                    if (this.timerTest) this.killTrigger.next();

                    let msg: string = "Sorry, you have been logged off automatically.\n";
                    switch (success.result) {
                            case "Expired":
                                msg += "Your session has been inactive for too long.\n"
                                break;
                            case "Invalid":
                                msg += "Someone has logged on with the same credentials you are using.\n";
                                msg += "This is not allowed in EPPI-Reviewer. If you believe that someone is using your credentials without permission, ";
                                msg += "you should contact the ER4 support.\n";
                                break;
                            case "None":
                                msg += "Your session has become invalid for unrecognised reasons (Return code = NONE).\n";
                            msg += "Please contact EPPI-Reviewer support.\n";
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


               
                console.log('Got inside error here: (meaning unauthorized worked correctly...)' + error);

                // NEED TO ASK SERGIO: not sure how to capture the error in the way defined here

                //if (e.Error.GetType() == (new System.Reflection.TargetInvocationException(new Exception()).GetType())) {

                this.UpdateStatus("!You have lost the connection with our server, please check your Internet connection.\n" +
                         "This message will revert to normal when the connection will be re-established:\n" +
                         "Please keep in mind that data changes made while disconnected cannot be saved.\n " +
                         "If your Internet connection is working, we might be experiencing some technical problems,\n " +
                         "We apologise for the inconvenience.");

                if (this.timerTest) this.killTrigger.next();

                this.handleError(error);
                 //}
                //windowMOTD.Tag = "failure";
                // windowMOTD.MOTDtextBlock.Text = "We are sorry, you have lost communication with the server. To avoid data corruption, the page will now reload.\n" +
                    // "This message may appear if you didn't log out during a software update.\n" +
                     //"Note that Eppi-Reviewer might fail to load until the update is completed, please wait a couple of minutes and try again.";
                 //windowMOTD.Show();

               // POSSIBLY, NEED TO CHECK: ANGULAR SHOULD REFRESH ITSELF; NO NEED TO REFRESH COMMAND ON WINDOW
            });
    }

    //https://ng-bootstrap.github.io/#/components/modal/examples
    openMsg(content : any) {
        this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' }).result.then((res) => {

                alert('Simulate application returning to logon page: ' + res);
               // this.router.navigate(['home']);
        },
        (res) => {
                    alert('Continue for debugging purposes: ' + res)
                    this.router.navigate(['home']);
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
