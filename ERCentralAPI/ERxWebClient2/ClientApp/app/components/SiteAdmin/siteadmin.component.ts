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
    selector: 'siteadmin',
    templateUrl: './siteadmin.component.html',
    providers: []
})

export class SiteAdminComponent implements OnInit {

    @ViewChild('content') private content: any;

    constructor(private router: Router,
                private _httpC: HttpClient,
                @Inject('BASE_URL') private _baseUrl: string,
                public ReviewerIdentityServ: ReviewerIdentityService
    ) {    }

   
    public Uname: string = "";
    public Pw: string = "";
    public revId: string = "";
     
    

   

    ngOnInit() {
        if (!this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) this.router.navigate(['home']);
    }

    public get IsSiteAdmin(): boolean {
        //console.log("Is it?", this.ReviewerIdentityServ.reviewerIdentity
        //    , this.ReviewerIdentityServ.reviewerIdentity.userId > 0
        //    , this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated
        //    , this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin);
        if (this.ReviewerIdentityServ
            && this.ReviewerIdentityServ.reviewerIdentity
            && this.ReviewerIdentityServ.reviewerIdentity.userId > 0
            && this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated
            && this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) return true;
        else return false;
    }
    public get CanOpenRev(): boolean {
        if (this.Uname.trim().length < 2) return false;
        else if (this.Pw.trim().length < 6) return false;
        else if (this.revId.trim().length > 0) {
            let rid = parseInt(this.revId, 10)
            if (!isNaN(rid) && rid > 0) return true;
            else return false;
        }
        else return false;
    }
    OpenRev() {
        let rid = parseInt(this.revId, 10)
        if (!isNaN(rid) && rid > 0) {
            this.ReviewerIdentityServ.LoginReqSA(this.Uname, this.Pw, rid);
        }
    }

    ngOnDestroy() {
    }
}




