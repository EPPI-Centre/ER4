import { Component, OnInit, EventEmitter, Output, OnDestroy, Input, AfterViewInit } from '@angular/core';
import { forEach } from '@angular/router/src/utils/collection';
import { Router } from '@angular/router';
import { Subscription} from 'rxjs';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { ItemListService, Item } from '../services/ItemList.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReviewSetsService } from '../services/ReviewSets.service';
import { ReviewInfoService } from '../services/ReviewInfo.service';
import { PriorityScreeningService } from '../services/PriorityScreening.service';
import { ReviewerTermsService } from '../services/ReviewerTerms.service';
import { ItemDocsService } from '../services/itemdocs.service';

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
		, private ItemCodingService: ItemCodingService,
		private ReviewSetsService: ReviewSetsService,
		private reviewInfoService: ReviewInfoService,
		public PriorityScreeningService: PriorityScreeningService
		, private ReviewerTermsService: ReviewerTermsService,
		public ItemDocsService: ItemDocsService
	) { }
	
	//public item?: Item;
	//public itemId = new Subject<number>();
    public get itemID(): number {
        if (this.item) return this.item.itemId;
        else return -1;
    }
    
    private subGotScreeningItem: Subscription | null = null;
    @Output() ItemChanged = new EventEmitter();
    @Output() GoToNextScreeningItemClicked = new EventEmitter();
    @Input() IsScreening: boolean = false;
    @Input() item: Item | undefined;
    @Input() Context: string = "CodingFull";
	

	ngOnInit() {
        //if (this.item) this.itemID = this.item.itemId;
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
		this.ItemDocsService.Clear();
		this.PriorityScreeningService.PreviousItem();
	}
	
    public GotScreeningItem() {
        //this.item = this.PriorityScreeningService.CurrentItem;
        //this.itemID = this.item.itemId;
        //this.GetItemCoding();
    }
    public GetScreeningItem() {
        this.GoToNextScreeningItemClicked.emit();
    }
	private _hasPrevious: boolean | null = null;
	hasPrevious(): boolean {
		
		return this.ItemListService.hasPrevious(this.itemID);
	}
    public get MyHumanIndex(): number {
        //console.log('MyHumanIndex called', this.itemID);
		if (this.ItemListService.ItemList.items.findIndex(found => found.itemId == this.itemID) == -1) {
			return 1;
		} else {
			return this.ItemListService.ItemList.items.findIndex(found => found.itemId == this.itemID) + 1;
		}
	}
	private _hasNext: boolean | null = null;
	hasNext(): boolean {

		return this.ItemListService.hasNext(this.itemID);
	}
	firstItem() {
		//this.WipeHighlights();

		if (this.item) this.goToItem(this.ItemListService.getFirst());

	}
	prevItem() {

		//this.WipeHighlights();
		if (this.item) {

			this.goToItem(this.ItemListService.getPrevious(this.item.itemId));
		}
	}
	nextItem() {

		//this.WipeHighlights();
		if (this.item) {

			this.goToItem(this.ItemListService.getNext(this.item.itemId));
			//this.valueChange.next(this.item.itemId);
		}
	}
	lastItem() {
		//this.WipeHighlights();
		if (this.item) this.goToItem(this.ItemListService.getLast());
	}

	lastPage() {
		
		this.ItemListService.FetchLastPage();
	}
	
	goToItem(item: Item) {
		//this.WipeHighlights();
        if (this.Context == 'FullUI') this.router.navigate(['itemcoding', item.itemId]);
        else if (this.Context == 'CodingOnly') this.router.navigate(['itemcodingOnly', item.itemId]);
		this.item = item;
		//if (this.item.itemId != this.itemID) {

		//	this.itemID = this.item.itemId;
  //      }
  
        this.ItemChanged.emit();
		//this.GetItemCoding();
	}
	BackToMain() {
		this.router.navigate(['Main']);
	}
	
	ngOnDestroy() {
		//console.log('killing coding comp');
		
	}
}







