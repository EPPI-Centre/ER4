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
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewSetsService } from '../services/ReviewSets.service';


@Component({
    selector: 'itemcoding',
    templateUrl: './coding.component.html',
    //providers: [ReviewerIdentityService]
    providers: []
})
export class ItemCodingComp implements OnInit, OnDestroy {

    constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService, private ItemListService: ItemListService
        , private route: ActivatedRoute, private ItemCodingService: ItemCodingService, private ReviewSetsService: ReviewSetsService
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
        this.item = this.ItemListService.getItem(this.itemID);
        this.GetItemCoding();
    }
    private GetItemCoding() {
        this.ItemCodingService.Fetch(this.itemID).subscribe(result => {
            this.ItemCodingService.ItemCodingList = result;

            //this.ReviewSetsService.AddItemData(result);
            this.ItemCodingService.Save();
        })
    }
    SetCoding() {
        this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList);
    }
    ngOnDestroy() {
        this.sub.unsubscribe();
    }
}





