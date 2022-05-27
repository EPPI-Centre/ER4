import { Component, Inject, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationList.service'
import { Criteria, ItemList } from '../services/ItemList.service'
import { WorkAllocationContactListComp } from '../WorkAllocations/WorkAllocationContactListComp.component';
import { ItemListService } from '../services/ItemList.service'
import { ItemListComp } from '../ItemList/itemListComp.component';
import { FetchReadOnlyReviewsComponent } from '../readonlyreviews/readonlyreviews.component';
import { ReviewInfoService } from '../services/ReviewInfo.service'
import { Subject, Subscription } from 'rxjs'; 
import { ReviewSetsService } from '../services/ReviewSets.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { Helpers } from '../helpers/HelperMethods';

@Component({
    selector: 'mainCodingOnly',
    templateUrl: './main.component.html'
    ,styles: [`
                
               .ReviewsBg {
                    background-color:#f1f1f8 !important; 
                }
        `]
     ,providers: []

})
export class MainComponent implements OnInit, OnDestroy, AfterViewInit {
    constructor(private router: Router,
        public ReviewerIdentityServ: ReviewerIdentityService,
        private ReviewInfoService: ReviewInfoService,
        private ReviewSetsService: ReviewSetsService,
        @Inject('BASE_URL') private _baseUrl: string,
        private _httpC: HttpClient,
        private ItemListService: ItemListService,
        private eventEmitters: EventEmitterService
    ) {
        
    }

    @ViewChild(WorkAllocationContactListComp)
    private workAllocationsComp!: WorkAllocationContactListComp;
    @ViewChild(ItemListComp)
    private itemListComp!: ItemListComp;
    @ViewChild(FetchReadOnlyReviewsComponent)
    private ReadOnlyReviewsComponent!: FetchReadOnlyReviewsComponent;
    private InstanceId: number = Math.random();
    private killTrigger: Subject<void> = new Subject();
    public countDown: any | undefined;
    public count: number = 60;
    public isReviewPanelCollapsed = false;
    public get ReviewPanelTogglingSymbol(): string {
        if (this.isReviewPanelCollapsed) return '&uarr;';
        else return '&darr;';
    }
    ngAfterViewInit() {


    }
    public get IsServiceBusy(): boolean {
        return (this.ReviewInfoService.IsBusy ||
            this.ReviewSetsService.IsBusy ||
            this.ItemListService.IsBusy);
    }
    toggleReviewPanel() {
        this.isReviewPanelCollapsed = !this.isReviewPanelCollapsed;
        if (this.isReviewPanelCollapsed && this.ReadOnlyReviewsComponent) this.ReadOnlyReviewsComponent.getReviews();
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

        console.log("MainCodingOnly init:", this.InstanceId);
        this.subOpeningReview = this.eventEmitters.OpeningNewReview.subscribe(() => this.Reload());
        //this.ReviewInfoService.Fetch();
        this.ReviewSetsService.GetReviewSets();
    }

    Reload() {
        this.Clear();
        if (this.ReviewerIdentityServ.IsCodingOnly) {//might not be, if we're going to the full UI and this component isn't dead yet.
            this.workAllocationsComp.getWorkAllocationContactList();
            this.ReviewSetsService.GetReviewSets();
        }
        this.isReviewPanelCollapsed = false;
    }

    FormatDate(DateSt: string): string {
        return Helpers.FormatDate2(DateSt);
    }

    Clear() {
        this.ItemListService.SaveItems(new ItemList(), new Criteria());
        this.ReviewSetsService.Clear();
        this.workAllocationsComp.Clear();
        if (this.ReadOnlyReviewsComponent) this.ReadOnlyReviewsComponent.Clear();
    }
    LoadWorkAllocList(workAlloc: WorkAllocation) {

        this.itemListComp.LoadWorkAllocList(workAlloc, this.workAllocationsComp.ListSubType);

    }
    public get MyAccountMessage(): string {
        let msg: string = "Your account expires on: ";
        let AccExp: string = this.FormatDate(this.ReviewerIdentityServ.reviewerIdentity.accountExpiration);
        msg += AccExp;
        return msg;
    }
    public get MyReviewMessage(): string {
        let revPart: string = "";
        if (this.ReviewerIdentityServ.getDaysLeftReview() == -999999) {//review is private
            revPart = "Current review is private (does not expire).";
        }
        else {
            let RevExp: string = this.FormatDate(this.ReviewerIdentityServ.reviewerIdentity.reviewExpiration);
            revPart = "Current(shared) review expires on: " + RevExp + ".";
        }
        return revPart;
    }
    ngOnDestroy() {
        if (this.subOpeningReview) {
            this.subOpeningReview.unsubscribe();
            //this.ReviewerIdentityServ.OpeningNewReview = null;
        }
    }
}
