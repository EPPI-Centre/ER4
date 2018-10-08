import { Component, Inject, OnInit, EventEmitter, Output } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { FormGroup, FormControl, FormBuilder, Validators, ReactiveFormsModule  } from '@angular/forms';
import { Subject } from 'rxjs/index';
import { debounceTime, startWith, merge, switchMap, share  } from 'rxjs/operators/index';
import { pipe } from 'rxjs'
import { style } from '@angular/animations';

@Component({
    selector: 'ItemListComp',
    templateUrl: './ItemListComp.component.html',
    providers: []
})
export class ItemListComp implements OnInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
        private _WorkAllocationService: WorkAllocationContactListService
    ) {

    }
    onSubmit(f: string) {

    }
    //private sub: any;
    //@Output() loadDefault = new EventEmitter();

    value = 1;
    onEnter(value: number) {
        this.value = value ;
        this.ItemListService.FetchParticularPage(value-1);
    }
    
    public LoadWorkAllocList(workAlloc: WorkAllocation, ListSubType: string) {

        let crit = new Criteria();
        crit.listType = ListSubType;
        crit.workAllocationId = workAlloc.workAllocationId;
        let ListDescr: string = "Showing ";
        if (ListSubType == 'GetItemWorkAllocationListRemaining') {
            ListDescr = "work allocation remaining: " + workAlloc.attributeName;
        }
        else if (ListSubType == 'GetItemWorkAllocationListStarted') {
            ListDescr = "work allocation started: " + workAlloc.attributeName;
        }
        else if (ListSubType == 'GetItemWorkAllocationList') {
            ListDescr = "total work allocation: " + workAlloc.attributeName;
        }
        else {
            ListDescr = "work allocation (unknown)";
        }      
        this.ItemListService.FetchWithCrit(crit, ListDescr);
    }

    OpenItem(itemId: number) {
       
        console.log('opening one: ' + itemId);
        if (itemId > 0) {

            this.router.navigate(['itemcoding', itemId]);
        } 
    }
    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            //this.loadDefault.emit();
        }
    }
    nextPage() {
        console.log('tester');
        this.ItemListService.FetchNextPage();
    }
    prevPage() {
        this.ItemListService.FetchPrevPage();

    }
    firstPage() {
        this.ItemListService.FetchFirstPage();
    }
    lastPage() {
        this.ItemListService.FetchLastPage();
    }
        
}






