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
    providers: [],
    styles: ["button.disabled {color:black; }"]
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
        console.log('init!');
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

            this.ReviewSetsService.AddItemData(result);
            this.ItemCodingService.Save();
        })
    }
    SetCoding() {
        this.ReviewSetsService.clearItemData();
        this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList);
    }
    ngOnDestroy() {
        this.sub.unsubscribe();
    }
    private _hasPrevious: boolean | null = null;
    hasPrevious(): boolean {
        
        if (!this.item || !this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.items || this.ItemListService.ItemList.items.length < 1) {
            //console.log('NO!');
            return false;
        }
        else if (this._hasPrevious === null) {
            this._hasPrevious = this.ItemListService.hasPrevious(this.item.itemId);
        }
        //console.log('has prev? ' + this._hasPrevious);
        return this._hasPrevious;
    }
    MyHumanIndex():number {
        return this.ItemListService.ItemList.items.findIndex(found => found.itemId == this.itemID) + 1;
    }
    private _hasNext: boolean | null = null;
    hasNext(): boolean {
        if (!this.item || !this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.items || this.ItemListService.ItemList.items.length < 1) {
            return false;
        }
        else if (this._hasNext === null)
        {
            this._hasNext = this.ItemListService.hasNext(this.item.itemId);
        }
        return this._hasNext;
    }
    firstItem() {
        console.log('First');
        
        if (this.item) this.goToItem(this.ItemListService.getFirst());
    }
    prevItem() {
        console.log('Prev');
        if (this.item) this.goToItem(this.ItemListService.getPrevious(this.item.itemId));
    }
    nextItem() {
        console.log('Next');
        if (this.item) this.goToItem(this.ItemListService.getNext(this.item.itemId));
    }
    lastItem() {
        console.log('Last');
        if (this.item) this.goToItem(this.ItemListService.getLast());
    }
    clearItemData() {
        this._hasNext = null;
        this._hasPrevious = null;
        this.item = undefined;
        this.itemID = -1;
        this.ItemCodingService.ItemCodingList = [];
        if (this.ReviewSetsService) this.ReviewSetsService.clearItemData();
    }
    goToItem(item: Item) {
        //console.log('gti');
        this.clearItemData();
        this.router.navigate(['itemcoding', item.itemId]);
        this.item = item;
        //console.log(this.item.title);
        if (this.item.itemId != this.itemID) {
            //console.log('ouch');
            this.itemID = this.item.itemId;
        }
        this.GetItemCoding();
    }
    BackToMain() {
        this.clearItemData();
        this.router.navigate(['main']);
    }
}





