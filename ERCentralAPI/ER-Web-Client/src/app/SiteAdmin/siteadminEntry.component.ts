import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { FeedbackAndClientError, OnlineHelpService } from '../services/onlinehelp.service';
import { GridDataResult, PageChangeEvent, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, process, CompositeFilterDescriptor, State } from '@progress/kendo-data-query';
import { Helpers } from '../helpers/HelperMethods';


@Component({
    selector: 'siteadminEntry',
    templateUrl: './siteadminEntry.component.html',
    providers: []
})

export class SiteAdminEntryComponent implements OnInit {

    @ViewChild('content') private content: any;

    constructor(private router: Router,
        private _httpC: HttpClient,
        private OnlineHelpService: OnlineHelpService,
        @Inject('BASE_URL') private _baseUrl: string,
        public ReviewerIdentityServ: ReviewerIdentityService
    ) {    }

    ngOnInit() {
        if (this.IsSiteAdmin) this.OnlineHelpService.GetFeedbackMessageList();
    }
    public get LatestFeedbackDate(): string {
        if (this.OnlineHelpService.FeedbackMessageList.length == 0) return "";
        else return Helpers.FormatDate( this.OnlineHelpService.FeedbackMessageList[0].dateCreated);
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
    GoToSiteAdmin() {
        if (!this.IsSiteAdmin) return;
        else {
            this.router.navigate(['SiteAdmin']);
        }
    }
    ngOnDestroy() {
    }
}




