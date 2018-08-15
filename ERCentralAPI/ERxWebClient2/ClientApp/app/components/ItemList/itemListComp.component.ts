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
   
})
export class ItemListComp implements OnInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService, private ItemListService: ItemListService
        , private pagerService: PagerService) {

    }

    onSubmit(f: string) {

    }

    // array of all items to be paged
    allItems: any[] = [];

    // pager object
    pager: any = {};

    // paged items
    pagedItems: any[] = [];

    public LoadWorkAllocList(workAlloc: WorkAllocation, ListSubType: string) {
        let crit = new Criteria();
        crit.listType = ListSubType;
        crit.workAllocationId = workAlloc.workAllocationId;
        this.ItemListService.FetchWithCrit(crit)
            .subscribe(list => {

                this.allItems = list.items;
                this.setPage(1);
                console.log("Got ItemList, length = " + list.items.length);
                this.ItemListService.SaveItems(list, crit);

            })
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
            this.allItems = this.ItemListService.ItemList.items;

            this.setPage(1);

            console.log('Got in here...1');
        }
    }

    setPage(page: number) {
        // get pager object from service
        this.pager = this.pagerService.getPager(this.allItems.length, page);

        console.log('Indexes here are:{0}, {1} ' , this.pager.startIndex , this.pager.endIndex + 1);

        // get current page of items
        this.pagedItems = this.allItems.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }
}






