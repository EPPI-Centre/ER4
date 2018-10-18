import { Component, Inject, OnInit, EventEmitter, Output, OnDestroy, Input, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { Observable, Subscription, Subject, Subscribable, } from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ReviewerIdentity } from '../services/revieweridentity.service';
import { WorkAllocation } from '../services/WorkAllocationContactList.service';
import { ItemListService, Criteria, Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet, ReadOnlyItemAttribute } from '../services/ItemCoding.service';
import { ReviewSetsService, ItemAttributeSaveCommand, SetAttribute } from '../services/ReviewSets.service';
import { ReviewSetsComponent, CheckBoxClickedEventData } from '../reviewsets/reviewsets.component';
import { ReviewInfo, ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { ArmsService } from '../services/arms.service';

@Component({
	selector: 'itemDetailsPaginator',
	templateUrl: './itemDetailsPaginator.component.html',
	providers: [],
	styles: [`
                button.disabled {
                    color:black; 
                    }
            `]
})

export class itemDetailsPaginatorComp implements OnInit, OnDestroy, AfterViewInit {

	constructor(private router: Router, private ReviewerIdentityServ: ReviewerIdentityService,
		public ItemListService: ItemListService
		, private route: ActivatedRoute, private ItemCodingService: ItemCodingService,
		private ReviewSetsService: ReviewSetsService,
		private reviewInfoService: ReviewInfoService,
		public PriorityScreeningService: PriorityScreeningService
		, private ReviewerTermsService: ReviewerTermsService,
		public ItemDocsService: ItemDocsService,
		private armservice: ArmsService
	) { }
	
	//public item?: Item;
	//public itemId = new Subject<number>();
	public itemID: number = 0;
	private subGotScreeningItem: Subscription | null = null;
    @Output() ItemChanged = new EventEmitter();
    @Input() IsScreening: boolean = false;
    @Input() item: Item | undefined;
    @Input() Context: string = "CodingFull";
	

	ngOnInit() {
        console.log('testing if item is passed: ' + this.item);
        if (this.item) this.itemID = this.item.itemId;
	}
	
	
	ngAfterViewInit() {
		// child is set
	}

	
	public HasPreviousScreening(): boolean {

		if (this.PriorityScreeningService.CurrentItemIndex > 0) return true;
		return false;
	}
	public CanMoveToNextInScreening(): boolean {

		let ItemSetIndex = this.ItemCodingService.ItemCodingList.findIndex(cset =>
			cset.setId == this.reviewInfoService.ReviewInfo.screeningCodeSetId
			&& (cset.contactId == this.ReviewerIdentityServ.reviewerIdentity.userId || cset.isCompleted));
		if (ItemSetIndex == -1) return false;
		else return true;
	}
	public prevScreeningItem() {
		//this.WipeHighlights();
		if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
		this.IsScreening = true;
		this.PriorityScreeningService.PreviousItem();
	}
	
    public GotScreeningItem() {

        //this.item = this.PriorityScreeningService.CurrentItem;
        //this.itemID = this.item.itemId;
        //this.GetItemCoding();
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
	MyHumanIndex(): number {
		return this.ItemListService.ItemList.items.findIndex(found => found.itemId == this.itemID) + 1;
	}
	private _hasNext: boolean | null = null;
	hasNext(): boolean {
		if (!this.item || !this.ItemListService || !this.ItemListService.ItemList || !this.ItemListService.ItemList.items || this.ItemListService.ItemList.items.length < 1) {
			return false;
		}
		else if (this._hasNext === null) {
			this._hasNext = this.ItemListService.hasNext(this.item.itemId);
		}
		return this._hasNext;
	}
	firstItem() {
		//this.WipeHighlights();

		if (this.item) this.goToItem(this.ItemListService.getFirst());

	}
	prevItem() {

		//this.WipeHighlights();
		if (this.item) {
			console.log('inside previous coding item component part' + this.item.itemId);

			this.goToItem(this.ItemListService.getPrevious(this.item.itemId));
		}
	}
	nextItem() {

		//this.WipeHighlights();
		if (this.item) {

			this.goToItem(this.ItemListService.getNext(this.item.itemId));
			console.log('inside next coding item component part' + this.item.itemId);
			//this.valueChange.next(this.item.itemId);
		}
	}
	lastItem() {
		//this.WipeHighlights();
		if (this.item) this.goToItem(this.ItemListService.getLast());
	}
	
	goToItem(item: Item) {
		//this.WipeHighlights();
        console.log('what do you need me to do?' + item.itemId);
        if (this.Context == 'FullUI') this.router.navigate(['itemcoding', item.itemId]);
        else if (this.Context == 'CodingOnly') this.router.navigate(['itemcodingOnly', item.itemId]);
		this.item = item;
		if (this.item.itemId != this.itemID) {

			this.itemID = this.item.itemId;
        }
        console.log('emitting');
        this.ItemChanged.emit();
		//this.GetItemCoding();
	}
	BackToMain() {
		this.router.navigate(['mainFullReview']);
	}
	
	ngOnDestroy() {
		//console.log('killing coding comp');
		
	}
}







