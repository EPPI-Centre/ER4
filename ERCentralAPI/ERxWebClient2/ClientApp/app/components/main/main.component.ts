import { Component, Inject, OnInit, ViewChild, AfterViewInit } from '@angular/core';
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

@Component({
    selector: 'main',
    templateUrl: './main.component.html'
     ,providers: []

    //providers: [ReviewerIdentityService]
})
export class MainComponent implements OnInit, AfterViewInit {
    constructor(private router: Router,
        private ReviewerIdentityServ: ReviewerIdentityService,
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
    private ReadOnlyReviewsComponent!: FetchReadOnlyReviewsComponent
    ngAfterViewInit() {
    }

    onLogin(u: string, p:string) {
        //this.ReviewerIdentityServ.Login(u, p);

        this.ReviewerIdentityServ.LoginReq(u, p).subscribe(ri => {
            this.ReviewerIdentityServ.reviewerIdentity = ri;
            console.log('home login: ' + this.ReviewerIdentityServ.reviewerIdentity.userId);
            if (this.ReviewerIdentityServ.reviewerIdentity.userId > 0) {
                this.ReviewerIdentityServ.Save();
                this.router.navigate(['readonlyreviews']);
            }
        })
        
    };
    ngOnInit() {
       
    }
    Reload() {
        this.Clear();
        this.workAllocationsComp.getWorkAllocationContactList();
        //this.itemListComp.
    }
    Clear() {
        this.ItemListService.SaveItems(new ItemList());
        this.workAllocationsComp.Clear();
    }
    LoadWorkAllocList(workAlloc: WorkAllocation) {
        this.itemListComp.LoadWorkAllocList(workAlloc, this.workAllocationsComp.ListSubType);
        //this.ItemListService.FetchWorkAlloc(workAlloc.workAllocationId, this.workAllocations.ListSubType, 100, 0)
        //    .subscribe(list => {
        //        console.log("Got ItemList, lenght = " + list.items.length);
        //    })
    }
   

}
