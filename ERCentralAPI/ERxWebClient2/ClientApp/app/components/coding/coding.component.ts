import { Component, Inject, OnInit, EventEmitter, Output, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item } from '../services/ItemList.service';


@Component({
    selector: 'itemcoding',
    templateUrl: './coding.component.html',
    //providers: [ReviewerIdentityService]
    providers: []
})
export class ItemCodingComp implements OnInit, OnDestroy {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService, private ItemListService: ItemListService
        , private route: ActivatedRoute
    ) { }
    private sub: any;
    private itemID: number = 0;
    private item?: Item;
    onSubmit(f: string) {
    }
    //@Output() criteriaChange = new EventEmitter();
    //public ListSubType: string = "";

    ngOnInit() {

        if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0) {
            this.router.navigate(['home']);
        }
        else {
            this.sub = this.route.params.subscribe(params => {
                this.itemID = +params['itemId'];
                this.GetItem();
            })
        }

    }
    private GetItem() {
        this.item = this.ItemListService.getItem(this.itemID)
    }
    ngOnDestroy() {
        this.sub.unsubscribe();
    }
}





