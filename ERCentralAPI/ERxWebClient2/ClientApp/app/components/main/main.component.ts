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
import { timer, Subject } from 'rxjs'; 
import { take, map, takeUntil } from 'rxjs/operators';

@Component({
    selector: 'main',
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
        @Inject('BASE_URL') private _baseUrl: string,
        private _httpC: HttpClient,
        private ItemListService: ItemListService
    ) {
        
    }

    @ViewChild(WorkAllocationContactListComp)
    private workAllocationsComp!: WorkAllocationContactListComp;
    @ViewChild(ItemListComp)
    private itemListComp!: ItemListComp;
    @ViewChild(FetchReadOnlyReviewsComponent)
    private ReadOnlyReviewsComponent!: FetchReadOnlyReviewsComponent;

    private killTrigger: Subject<void> = new Subject();
    public countDown: any | undefined;
    public count: number = 60;
    ngAfterViewInit() {


    }
    tootlTipText() {

        return "HELLO";
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
    ngOnInit() {

        this.ReviewInfoService.Fetch();
    }
    Reload() {
        this.Clear();
        this.workAllocationsComp.getWorkAllocationContactList();

    }

    Clear() {
        this.ItemListService.SaveItems(new ItemList(), new Criteria());
        this.workAllocationsComp.Clear();
    }
    LoadWorkAllocList(workAlloc: WorkAllocation) {

        this.itemListComp.LoadWorkAllocList(workAlloc, this.workAllocationsComp.ListSubType);

    }
    ngOnDestroy() {


    }
}
