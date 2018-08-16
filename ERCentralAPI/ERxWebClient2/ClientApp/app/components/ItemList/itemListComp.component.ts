import { Component, Inject, OnInit, EventEmitter, Output } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { FormGroup, FormControl, FormBuilder, Validators, ReactiveFormsModule  } from '@angular/forms';
import { Subject } from 'rxjs/index';
import { debounceTime, startWith, merge, switchMap, share  } from 'rxjs/operators/index';
import { pipe } from 'rxjs'
import { PagerService } from '../services/pager.service'
import { style } from '@angular/animations';

@Component({
    selector: 'ItemListComp',
    templateUrl: './ItemListComp.component.html',
    providers: [],
    styles: ["button.disabled {color:black; }"]
})
export class ItemListComp implements OnInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService, private ItemListService: ItemListService
        , private pagerService: PagerService) {

    }

    onSubmit(f: string) {

    }
    private sub: any;
    

    public LoadWorkAllocList(workAlloc: WorkAllocation, ListSubType: string) {
        let crit = new Criteria();
        crit.listType = ListSubType;
        crit.workAllocationId = workAlloc.workAllocationId;
        this.sub = this.ItemListService.FetchWithCrit(crit);
            //.subscribe(list => {

            //    //this.allItems = list.items;
            //    //this.setPage(1);
            //    console.log("Got ItemList, length = " + list.items.length);
            //    this.ItemListService.SaveItems(list, crit);

            //})
    }

    OpenItem(itemId: number) {
        if (itemId > 0) {
            this.router.navigate(['itemcoding', itemId]);
        }
    }
    

    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            // initialize to page 1
            //this.allItems = this.ItemListService.ItemList.items;

            //this.setPage(1);

            console.log('Got in here...1');
        }
    }
    nextPage() {
        this.ItemListService.FetchNextPage();
            //.subscribe(list => {

            //    //this.allItems = list.items;
            //    //this.setPage(1);
            //    console.log("Got ItemList, length = " + list.items.length);
            //    this.ItemListService.SaveItems(list, this.ItemListService.ListCriteria);

            //})
    }
    prevPage() {
        this.ItemListService.FetchPrevPage();
            //.subscribe(list => {

            //    //this.allItems = list.items;
            //    //this.setPage(1);
            //    console.log("Got ItemList, length = " + list.items.length);
            //    this.ItemListService.SaveItems(list, this.ItemListService.ListCriteria);

            //});
    }
    firstPage() {
        this.ItemListService.FetchFirstPage();
            //.subscribe(list => {
            //    //this.allItems = list.items;
            //    //this.setPage(1);
            //    console.log("Got ItemList, length = " + list.items.length);
            //    this.ItemListService.SaveItems(list, this.ItemListService.ListCriteria);
            //})
    }
    lastPage() {
        this.ItemListService.FetchLastPage();
            //.subscribe(list => {
            //    //this.allItems = list.items;
            //    //this.setPage(1);
            //    console.log("Got ItemList, length = " + list.items.length);
            //    this.ItemListService.SaveItems(list, this.ItemListService.ListCriteria);
            //})
    }

    
}






