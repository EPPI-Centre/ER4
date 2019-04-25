import { Component,  OnInit,  Output, EventEmitter } from '@angular/core';
import { ReviewSetsService, SetAttribute, ReviewSet } from '../services/ReviewSets.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ComparisonsService, Comparison } from '../services/comparisons.service';
import { Router } from '@angular/router';
import { ItemListService, Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet } from '../services/ItemCoding.service';
import { ReconciliationService, ReconcilingItemList, ReconcilingItem } from '../services/reconciliation.service';
import { Review } from '../services/review.service';


@Component({
	selector: 'ComparisonReconciliationComp',
	templateUrl: './comparisonReconciliation.component.html',
    providers: []
})

export class ComparisonReconciliationComp implements OnInit {

	constructor(
		private router: Router, 
		private _reviewSetsService: ReviewSetsService,
		private _reviewInfoService: ReviewInfoService,
		private ItemListService: ItemListService,
		private comparisonsService: ComparisonsService,
		private itemCodingService: ItemCodingService,
		private reconciliationService: ReconciliationService
	) { }

	private ComparisonDescription: string = '';
	private localList: ReconcilingItemList = new ReconcilingItemList(new ReviewSet(),
		new Comparison(), ""
		);
	public CurrentComparison: Comparison = new Comparison();
	private ReviewSet: ReviewSet = new ReviewSet();
	public item: Item = new Item();
	public PanelName: string = '';
	public chosenFilter: SetAttribute | null = null;
    public get CodeSets(): ReviewSet[] {
		return this._reviewSetsService.ReviewSets.filter(x => x.setType.allowComparison != false);
    }
	public selectedReviewer1: Contact = new Contact();
	public selectedReviewer2: Contact = new Contact();
	public selectedReviewer3: Contact = new Contact();
	public selectedCodeSet: ReviewSet = new ReviewSet();
	@Output() emitterCancel = new EventEmitter();

	public get Contacts(): Contact[] {
		return this._reviewInfoService.Contacts;
	}
	canSetFilter(): boolean {
		if (this._reviewSetsService.selectedNode
			&& this._reviewSetsService.selectedNode.nodeType == "SetAttribute") return true;
		return false;
	}
	clearChosenFilter() {
		this.chosenFilter = null;
	}

	public RefreshData() {

		console.log('About to refresh the data...');

		if (this.item != null && this.item != undefined) {

			// Fill with dummy reference data for viewing the reference information
			this.item = this.ItemListService.ItemList.items[0];
			this.CurrentComparison = this.comparisonsService.currentComparison;
			//console.log(' current comparison: ' + JSON.stringify(this.CurrentComparison));
			this.ReviewSet = this._reviewSetsService.GetReviewSets().filter(
				x => x.set_id == this.CurrentComparison.setId)[0];
			//console.log(' current review Set: ' + JSON.stringify(this.ReviewSet));


			// fill the reconciliation list accordingly
			let testLocalList: any = new ReconcilingItemList(this.ReviewSet,
				this.CurrentComparison, "testing right now");

		
			//1 -> So i need to do a command to call an ItemSetList thingy
			let ItemSetListTest: ItemSet[] = [];

			this.reconciliationService.FetchItemSetList(this.item.itemId)

				.then(
						(res: ItemSet[]) => {

							ItemSetListTest = res;
							//console.log('test item set list: '
							//	+ ItemSetListTest.length);
						}
				);

			//2 -> So I then need to fill the above locallist.
			for (var i = 0; i < this.ItemListService.ItemList.items.length; i++) {
			
				this.localList.AddItem(this.ItemListService.ItemList.items[i], ItemSetListTest);
			}	


			console.log('We have here: ' + this.localList.Items.length );

			//3 -> Then I need to get the arm list
			//GetItemArmList(items[CurrentItem]);

			console.log('number of locallist items are: ', testLocalList.length)

		}

	}

	ngOnInit() {
		console.log('Initialising...');
		this.RefreshData();
	}
	BackToMain() {
		//this.clearItemData();
		this.router.navigate(['Main']);
	}
	Clear() {
		this.selectedCodeSet = new ReviewSet();
	}
}

