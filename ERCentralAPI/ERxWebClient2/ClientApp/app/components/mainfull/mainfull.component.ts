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
import { timer, Subject, Subscription, Subscribable } from 'rxjs'; 
import { take, map, takeUntil } from 'rxjs/operators';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewSetsService, ReviewSet } from '../services/ReviewSets.service';
import { CodesetStatisticsService, ReviewStatisticsCountsCommand, ReviewStatisticsCodeSet } from '../services/codesetstatistics.service';

@Component({
    selector: 'mainfull',
    templateUrl: './mainfull.component.html'
    ,styles: [`
                
               .ReviewsBg {
                    background-color:#f1f1f8 !important; 
                }
        `]
     ,providers: []

})
export class MainFullReviewComponent implements OnInit, OnDestroy, AfterViewInit {
    constructor(private router: Router,
        public ReviewerIdentityServ: ReviewerIdentityService,
        //private ReviewInfoService: ReviewInfoService,
        private reviewSetsService: ReviewSetsService,
        @Inject('BASE_URL') private _baseUrl: string,
        private _httpC: HttpClient,
        private ItemListService: ItemListService,
        private codesetStatsServ: CodesetStatisticsService
    ) {
        
    }

    @ViewChild(WorkAllocationContactListComp)
    private workAllocationsComp!: WorkAllocationContactListComp;

    public stats: ReviewStatisticsCountsCommand | null = null;
    public countDown: any | undefined;
    public count: number = 60;
    public isReviewPanelCollapsed = false;
    private statsSub: Subscription = new Subscription();

    public get ReviewPanelTogglingSymbol(): string {
        if (this.isReviewPanelCollapsed) return '&uarr;';
        else return '&darr;';
    }
    ngAfterViewInit() {


    }
    toggleReviewPanel() {
        this.isReviewPanelCollapsed = !this.isReviewPanelCollapsed;
    }
    getDaysLeftAccount() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftAccount;
    }
    getDaysLeftReview() {

        return this.ReviewerIdentityServ.reviewerIdentity.daysLeftReview;
    }
    onLogin(u: string, p:string) {

        this.ReviewerIdentityServ.LoginReq(u, p);
        
    };
    subOpeningReview: Subscription | null = null;

    ngOnInit() {

        this.subOpeningReview = this.ReviewerIdentityServ.OpeningNewReview.subscribe(() => this.Reload());

        this.reviewSetsService.GetReviewSets();

        this.statsSub = this.reviewSetsService.GetReviewStatsEmit.subscribe(

            () => {

                this.codesetStatsServ.GetReviewStatisticsCountsCommand();

                this.codesetStatsServ.GetReviewSetsCodingCounts(true);
            }
        );
    }

    Reload() {
        this.Clear();
        this.workAllocationsComp.getWorkAllocationContactList();
    }

    Clear() {
        this.ItemListService.SaveItems(new ItemList(), new Criteria());
        this.reviewSetsService.Clear();
        this.statsSub.unsubscribe();
    }
   
    MyInfoMessage(): string {
        let msg: string  = "Your account expires on: ";
        let revPart: string = "";
        let AccExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.accountExpiration).toLocaleDateString();
        if (this.ReviewerIdentityServ.getDaysLeftReview() == -999999) {//review is private
            revPart = " | Current review is private (does not expire).";
        }
        else {
            let RevExp: string = new Date(this.ReviewerIdentityServ.reviewerIdentity.reviewExpiration).toLocaleDateString();
            revPart = " | Current(shared) review expires on " + RevExp + ".";
        }
        msg += AccExp + revPart;
        return msg;
       
    }

    ngOnDestroy() {
        if (this.subOpeningReview) {
            this.subOpeningReview.unsubscribe();
        }
    }
}
