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
	

	@ViewChild('ArmsCmp')
	private ArmsCompRef!: any;
	@ViewChild('ItemDetailsCmp')
	private ItemDetailsCompRef!: any;

	private subItemIDinPath: Subscription | null = null;
	private subCodingCheckBoxClickedEvent: Subscription | null = null;
	private ItemCodingServiceDataChanged: Subscription | null = null;
	private ItemArmsDataChanged: Subscription | null = null;
	public itemID: number = 0;
	private itemString: string = '0';
	//public item?: Item;

	public itemId = new Subject<number>();

	private subGotScreeningItem: Subscription | null = null;
	public IsScreening: boolean = false;
	//public ShowHighlights: boolean = false;

    @Input() item: Item | undefined;
    @Input() ShowHighlights: boolean = false;

	public HAbstract: string = "";
    public HTitle: string = "";

	ngOnInit() {
		console.log('testing if item is passed: ' + this.item);
	}
	

	public get HasTermList(): boolean {
		if (!this.ReviewerTermsService || !this.ReviewerTermsService.TermsList || !(this.ReviewerTermsService.TermsList.length > 0)) return false;
		else return true;
	}
	ngAfterViewInit() {
		// child is set
	}

	public CheckBoxAutoAdvanceVal: boolean = false;
	onSubmit(f: string) {
	}

	private GetItem() {

		this.WipeHighlights();
		if (this.itemString == 'PriorityScreening') {
			if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
			this.IsScreening = true;
			this.PriorityScreeningService.NextItem();
		}
		else {
			this.itemID = +this.itemString;
			this.item = this.ItemListService.getItem(this.itemID);

			this.IsScreening = false;
			this.GetItemCoding();
			//this.ItemListService.eventChange(this.itemID);
			console.log('fill in arms here teseroo1');

		}
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
		this.WipeHighlights();
		if (this.subGotScreeningItem == null) this.subGotScreeningItem = this.PriorityScreeningService.gotItem.subscribe(() => this.GotScreeningItem());
		this.IsScreening = true;
		this.PriorityScreeningService.PreviousItem();
	}
	public GotScreeningItem() {

		this.item = this.PriorityScreeningService.CurrentItem;
		this.itemID = this.item.itemId;
		this.GetItemCoding();
	}
	private GetItemCoding() {

		if (this.item) {
			this.ArmsCompRef.CurrentItem = this.item;
			this.armservice.FetchArms(this.item);
		}
		this.ItemCodingService.Fetch(this.itemID);

	}
	SetCoding() {
		console.log('change something');
		if (this.ItemCodingService.ItemCodingList.length == 0) {
			this.ReviewSetsService.clearItemData();
			console.log('change: clearonly');
			return;
		}
		this.SetHighlights();
		this.ReviewSetsService.clearItemData();
		if (this.armservice.SelectedArm) this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, this.armservice.SelectedArm.itemArmId);
		else this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, 0);
	}
	SetArmCoding(armId: number) {
		console.log('change Arm');
		this.ReviewSetsService.clearItemData();
		this.ReviewSetsService.AddItemData(this.ItemCodingService.ItemCodingList, armId);
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
		this.WipeHighlights();

		if (this.item) this.goToItem(this.ItemListService.getFirst());

	}
	prevItem() {

		this.WipeHighlights();
		if (this.item) {
			console.log('inside previous coding item component part' + this.item.itemId);

			this.goToItem(this.ItemListService.getPrevious(this.item.itemId));
		}
	}
	nextItem() {

		this.WipeHighlights();
		if (this.item) {

			this.goToItem(this.ItemListService.getNext(this.item.itemId));
			console.log('inside next coding item component part' + this.item.itemId);
			//this.valueChange.next(this.item.itemId);
		}
	}
	lastItem() {
		this.WipeHighlights();
		if (this.item) this.goToItem(this.ItemListService.getLast());
	}
	clearItemData() {
		this._hasNext = null;
		this._hasPrevious = null;
		this.item = undefined;
		this.itemID = -1;
		this.ItemCodingService.ItemCodingList = [];
		if (this.ReviewSetsService) {
			this.ReviewSetsService.clearItemData();
		}
	}
	goToItem(item: Item) {
		this.WipeHighlights();
		this.clearItemData();
		console.log('what do you need me to do?' + item.itemId);
		this.router.navigate(['itemcoding', item.itemId]);
		this.item = item;
		if (this.item.itemId != this.itemID) {

			this.itemID = this.item.itemId;
		}
		//this.GetItemCoding();
	}
	BackToMain() {
		this.clearItemData();
		this.router.navigate(['mainFullReview']);
	}
	
	ngOnDestroy() {
		//console.log('killing coding comp');
		if (this.subItemIDinPath) this.subItemIDinPath.unsubscribe();
		if (this.ItemCodingServiceDataChanged) this.ItemCodingServiceDataChanged.unsubscribe();
		if (this.subCodingCheckBoxClickedEvent) this.subCodingCheckBoxClickedEvent.unsubscribe();
		if (this.subGotScreeningItem) this.subGotScreeningItem.unsubscribe();
	}
	WipeHighlights() {
		if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.WipeHighlights();
	}
	SetHighlights() {
		if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.SetHighlights();
	}
	ShowHighlightsClicked() {
		if (this.ItemDetailsCompRef) this.ItemDetailsCompRef.ShowHighlightsClicked();
	}
}







