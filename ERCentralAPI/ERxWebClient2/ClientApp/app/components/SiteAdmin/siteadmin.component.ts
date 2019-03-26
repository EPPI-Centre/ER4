import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { FeedbackAndClientError, OnlineHelpService } from '../services/onlinehelp.service';
import { GridDataResult, PageChangeEvent, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, process, CompositeFilterDescriptor, State } from '@progress/kendo-data-query';
import { Subscription } from 'rxjs';


@Component({
    selector: 'siteadmin',
    templateUrl: './siteadmin.component.html',
    providers: []
})

export class SiteAdminComponent implements OnInit {

    @ViewChild('content') private content: any;

    constructor(private router: Router,
        private _httpC: HttpClient,
        private OnlineHelpService: OnlineHelpService,
        @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService
    ) {    }

    ngOnInit() {
        this.subOpeningReview = this.ReviewerIdentityServ.OpeningNewReview.subscribe(() => this.BackToMain());
        if (!this.ReviewerIdentityServ.reviewerIdentity.isSiteAdmin) this.router.navigate(['home']);
        else this.OnlineHelpService.GetFeedbackMessageList();
    }
    public Uname: string = "";
    public Pw: string = "";
    public revId: string = "";
    //public LoggedStatus: string = "";
    subOpeningReview: Subscription | null = null;
    public get FeedbackMessageList(): FeedbackAndClientError[] {
        return this.OnlineHelpService.FeedbackMessageList;
    }
    
    public get DataSource(): GridDataResult {
        return process(this.OnlineHelpService.FeedbackMessageList, this.state);
        //return {
        //    data: orderBy(this.OnlineHelpService.FeedbackMessageList.slice(this.skip, this.skip + this.pageSize), this.sort),
        //    total: this.OnlineHelpService.FeedbackMessageList.length,
        //};
    }
    public state: State = {
        skip: 0,
        take: 10,
        
    };
    public dataStateChange(state: DataStateChangeEvent): void {
        this.state = state;
        this.DataSource; //= process(sampleProducts, this.state);
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
        //this.LoggedStatus == "";
        let rid = parseInt(this.revId, 10)
        if (!isNaN(rid) && rid > 0) {
            //this.BackToMain();
            this.ReviewerIdentityServ.LoginReqSA(this.Uname, this.Pw, rid);
        }
    }
    //public  CheckLoggedStatus() {
    //    if (this.ReviewerIdentityServ.reviewerIdentity && this.ReviewerIdentityServ.reviewerIdentity.reviewId != 0) {
    //        this.LoggedStatus =  "Rid=" + this.ReviewerIdentityServ.reviewerIdentity.reviewId + " " + this.ReviewerIdentityServ.reviewerIdentity.isAuthenticated;
    //    }
    //    else this.LoggedStatus = "Nope: " + this.ReviewerIdentityServ.reviewerIdentity.reviewId;
    //}
    //protected pageChange({ skip, take }: PageChangeEvent): void {
    //    this.skip = skip;
    //    this.pageSize = take;
    //    this.DataSource;
    //}//
    //protected filterChange($event: CompositeFilterDescriptor) {
    //    console.log($event);
    //    this.filter = $event;
    //}
    //public sortChange(sort: SortDescriptor[]): void {
    //    this.sort = sort;
    //    this.DataSource;
    //}
    BackToMain() {
        this.router.navigate(['Main']);
    }
    ngOnDestroy() {
    }
}




