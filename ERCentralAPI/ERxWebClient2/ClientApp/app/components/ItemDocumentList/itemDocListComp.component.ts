import { Component, Inject, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscribable, Subscription } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation, WorkAllocationContactListService } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item, ItemList } from '../services/ItemList.service';
import { FormGroup, FormControl, FormBuilder, Validators, ReactiveFormsModule  } from '@angular/forms';
import { Subject } from 'rxjs/index';
import { debounceTime, startWith, merge, switchMap, share  } from 'rxjs/operators/index';
import { pipe } from 'rxjs'
import { style } from '@angular/animations';
import { ItemCodingService } from '../services/ItemCoding.service'
import { ItemDocsService } from '../services/itemdocs.service'

@Component({
    selector: 'ItemDocListComp',
    templateUrl: './ItemDocListComp.component.html',
    providers: []
})
export class ItemDocListComp implements OnInit {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
        public ItemListService: ItemListService,
        private _WorkAllocationService: WorkAllocationContactListService,
        private route: ActivatedRoute,
        private ItemCodingService: ItemCodingService,
        private ItemDocsService: ItemDocsService

    ) {
        this.sub = new Subscription();
    }

    public sub: Subscription;

    // testing
    @Input() itemID: number = 0;

    ngOnInit() {

      
        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {

            this.sub = this.ItemCodingService.itemID.subscribe(
                
                (itemID) => {
                    console.log('inside the component doc stuff: ' + this.itemID);
                    this.itemID = itemID;
                    this.ItemDocsService.FetchDocList(this.itemID);
                }
            );
            this.ItemDocsService.FetchDocList(this.itemID);

        }
    }

    ngOnDestroy() {

        this.sub.unsubscribe();
    }

}






