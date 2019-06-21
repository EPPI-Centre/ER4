import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { NgbModal, ModalDismissReasons } from '@ng-bootstrap/ng-bootstrap';
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
                public ReviewInfoService: ReviewInfoService,
        private modalService: NgbModal,
        public sanitizer: DomSanitizer
    ) {    }

    public count: number = 60;
    public modalMsg: string = '';
    public testlgtC: any;
    //public statusClass: string = 'bg-light';
    public IsAdmin: boolean = false;
    public isMoreButtonVisible = false;
    private fullMsg: string = '';
     
    

    ngOnDestroy() {
    }

    ngOnInit() {

        this.IsAdmin = this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin;

        //localStorage.getItem('currentErUser');

        //let guid = this.ReviewerIdentityServ.reviewerIdentity.ticket;
        //let uu = String(this.ReviewerIdentityServ.reviewerIdentity.userId);
        
        //if (guid != undefined && uu != '') {

        //    this.timerObj = timer(15000, 45000).pipe(
        //        takeUntil(this.killTrigger));

        //    this.timerObj.subscribe(() => this.LogonTicketCheckTimer(uu, guid));

        //}

    }
    public get CurrentMessageIsWarning(): boolean {
        if (this.ReviewerIdentityServ.currentStatus.length == 0) return false;
        else if (this.ReviewerIdentityServ.currentStatus.substr(0, 1) == "!") {
            return true;
        }
        else return false;
    }
    public get CurrentStatus(): string {
        //console.log("Getting status...", this.ReviewerIdentityServ.currentStatus);
        if (this.ReviewerIdentityServ.currentStatus.length < 1) return "No message yet.";
        else {
            let msgSt: string = this.ReviewerIdentityServ.currentStatus;
            let msg = msgSt;
            if (msg.substr(0, 1) == "!") {
                //this.statusClass = "bg-warning";
                msgSt = msg.substr(1).trim();
            }
            let truncateIndex: number = msgSt.indexOf("<br />");
            if (msgSt.length <= 40 && truncateIndex == -1) {
                //no new lines, short message: just show it!
                this.isMoreButtonVisible = false;
                return msgSt;
            }
            else if (msgSt.length > 40 || truncateIndex != -1) {
                //long message, or a message with new lines, we want to shorten it, stopping at the 8th word or a new line, whichever comes first.
                //Thus, this is the case when we will truncate the message
                let st: string[] = msgSt.replace("<br />", " ").replace("  ", " ").split(" ");
                if (st.length > 8) {
                    if (st[0].length + 1
                        + st[1].length + 1
                        + st[2].length + 1
                        + st[3].length + 1
                        + st[4].length + 1
                        + st[5].length + 1
                        + st[6].length + 1
                        + st[7].length + 1 < truncateIndex
                        || truncateIndex == -1
                    ) {
                        //first new line happens after the first 8 words. OR there is no new line.
                        //we truncate at the 8th word.
                        truncateIndex = st[0].length + 1
                            + st[1].length + 1
                            + st[2].length + 1
                            + st[3].length + 1
                            + st[4].length + 1
                            + st[5].length + 1
                            + st[6].length + 1
                            + st[7].length + 1;
                    }
                }
                //this.fullMsg = msgSt;
                //this.ReviewerIdentityServ.currentStatus = msgSt.substr(0, 80);
                //this.ReviewerIdentityServ.currentStatus = msgSt.substr(0, truncateIndex);
                this.isMoreButtonVisible = true;
                //console.log("will return:", msgSt, truncateIndex);
                return msgSt.substr(0, truncateIndex);
            } else {
                //I don't trust my boolean logic, but I don't think this case can happen
                this.isMoreButtonVisible = false;
                return msgSt;
                //this.ReviewerIdentityServ.currentStatus = msgSt;
            }
        }
    }

    public UpdateStatus(msg: string) {
        console.log("UpdateStatus:", msg);
        let msgSt: string = msg;
        if (msg.substr(0, 1) == "!") {

            //this.statusClass = "bg-warning";
            msgSt = msg.substr(1).trim();

        }
        let truncateIndex: number = msgSt.indexOf("<br />");
        if (msgSt.length > 40 || truncateIndex != -1) {
            let st: string[] = msgSt.replace("<br />", " ").replace("  ", " ").split(" ");
            if (st.length > 8) {
                if (st[0].length + 1
                    + st[1].length + 1
                    + st[2].length + 1
                    + st[3].length + 1
                    + st[4].length + 1
                    + st[5].length + 1
                    + st[6].length + 1
                    + st[7].length + 1 < truncateIndex) {
                    //first new line happens after the first 8 words.
                    truncateIndex = st[0].length + 1
                        + st[1].length + 1
                        + st[2].length + 1
                        + st[3].length + 1
                        + st[4].length + 1
                        + st[5].length + 1
                        + st[6].length + 1
                        + st[7].length + 1;
                }
            }
            this.fullMsg = msgSt;
            //this.ReviewerIdentityServ.currentStatus = msgSt.substr(0, 80);
            //this.ReviewerIdentityServ.currentStatus = msgSt.substr(0, truncateIndex);
            this.isMoreButtonVisible = true;

        } else {

            //this.ReviewerIdentityServ.currentStatus = msgSt;
        }
    }

    pressedMore() {
        if (this.ReviewerIdentityServ.currentStatus.indexOf('!') == 0) {
            this.modalMsg = this.ReviewerIdentityServ.currentStatus.substr(1);
        } else {
            this.modalMsg = this.ReviewerIdentityServ.currentStatus;
        }
        this.openMsg(this.content);

    }

    
    

    //https://ng-bootstrap.github.io/#/components/modal/examples
    openMsg(content : any) {
        this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' }).result.then((res) => {

        }, (reject) => { }
        ).catch(() => { });
    }
    //openMsgAndSendHome(content: any) {
    //    this.modalService.open(content, { ariaLabelledBy: 'modal-basic-title' }).result.then((res) => {

    //    },
    //        (res) => {
    //            this.router.navigate(['home']);
    //        }
    //    );
    //}
}




