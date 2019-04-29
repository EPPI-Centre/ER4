import { Component,  OnInit,  Output, EventEmitter } from '@angular/core';
import { ReviewSetsService, SetAttribute, ReviewSet } from '../services/ReviewSets.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ComparisonsService, Comparison } from '../services/comparisons.service';
import { Router } from '@angular/router';
import { ItemListService, Item, Criteria } from '../services/ItemList.service';
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
		private _ItemListService: ItemListService,
		private _comparisonsService: ComparisonsService,
		private _itemCodingService: ItemCodingService,
		private _reconciliationService: ReconciliationService
	) { }

	private ComparisonDescription: string = '';
	private localList: ReconcilingItemList = new ReconcilingItemList(new ReviewSet(),
		new Comparison(), ""
		);
	public CurrentComparison: Comparison = new Comparison();
	private ReviewSet: ReviewSet = new ReviewSet();
	public item: Item = new Item();
	public panelItem: Item | undefined = new Item();
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
	public LoadComparisons(comparison: Comparison, ListSubType: string) {

		let crit = new Criteria();
		crit.listType = ListSubType;
		let typeMsg: string = '';
		if (ListSubType.indexOf('Disagree') != -1) {
			typeMsg = 'disagreements between';
		} else {
			typeMsg = 'agreements between';
		}
		let middleDescr: string = ' ' + comparison.contactName3 != '' ? ' and ' + comparison.contactName3 : '';
		let listDescription: string = typeMsg + '  ' + comparison.contactName1 + ' and ' + comparison.contactName2 + middleDescr + ' using ' + comparison.setName;
		crit.description = listDescription;
		crit.listType = ListSubType;
		crit.comparisonId = comparison.comparisonId;

		console.log('checking: ' + JSON.stringify(crit) + '\n' + ListSubType);
		console.log('checking: ' + listDescription);

		this._ItemListService.FetchWithCrit(crit, listDescription);

		console.log('length of item list for this page: ' + this._ItemListService.ItemList.items.length);
		this.item = this._ItemListService.ItemList.items[0];
	}
	getReconciliations() {

		if (this.item != null && this.item != undefined) {

			// Fill with dummy reference data for viewing the reference information
			//if (itemid = 0) {
			//	this.item = this._ItemListService.ItemList.items[0];
			//} else {
			//	this.item = this._ItemListService.ItemList.items.filter(
			//		x => x.itemId = itemid)[0];
			//}
			
			this.CurrentComparison = this._comparisonsService.currentComparison;
			
			this.ReviewSet = this._reviewSetsService.GetReviewSets().filter(
				x => x.set_id == this.CurrentComparison.setId)[0];
			//console.log(' current review Set: ' + JSON.stringify(this.ReviewSet));
			
			// fill the reconciliation list accordingly
			this.localList = new ReconcilingItemList(this.ReviewSet,
				this.CurrentComparison, "testing right now");

			console.log(' current comparison inside localist: ' + JSON.stringify(this.localList.Comparison));

			//1 -> So i need to do a command to call an ItemSetList thingy
			let i: number = 0;

			this.recursiveItemList(i);

			console.log('going through each item and calling the below');

			console.log('We have here: ' + this.localList.Items.length);

			//3 -> Then I need to get the arm list
			//GetItemArmList(items[CurrentItem]);

			//console.log('number of locallist items are: ', testLocalList.length)

		}
	}
	public setsAmount(len: number): any {

		return	Array.from({ length: len }, (v, k) => k + 1);
	}

	public recursiveItemList(i: number) {

		let ItemSetListTest: ItemSet[] = [];
		this._reconciliationService.FetchItemSetList(this._ItemListService.ItemList.items[i].itemId)

			.then(
				(res: ItemSet[]) => {

					ItemSetListTest = res;

					// this needs to be a new itemSetList each time by calling the arms below
					// needs to loop and be async await....
					this.localList.AddItem(this._ItemListService.ItemList.items[i], ItemSetListTest);
					console.log('calling recursive part ======== ' + i);
					if (i < this._ItemListService.ItemList.items.length-1) {

						i = i + 1;
						this.recursiveItemList(i);
					} else {
						return;
					}
				}
			);
	}
	public ReconRowClick(itemid: number) {

		//alert(itemid);
		let tempItemList = this._ItemListService.ItemList.items;
		this.panelItem = tempItemList.find(x => x.itemId == itemid);

	}

	public RefreshData() {


		//alert('got here without rerouting' );
		this.panelItem = this._ItemListService.ItemList.items[0];
		this.getReconciliations();

		//console.log('About to refresh the data...' + JSON.stringify(this._ItemListService.ItemList.items[0]));

	}
	UnComplete(recon: ReconcilingItem) {

		console.log(recon);
		console.log(this.CurrentComparison);
		this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, 0, false)
			.then(

				() => {

					this.RefreshData();
				}

			);
	}
	Complete(recon: ReconcilingItem, contactID: number) {

		console.log(recon);
		console.log(this.CurrentComparison);
		this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, contactID, true)
			.then(

			() => {

				this.RefreshData();
			}
			
			);
	}

	ngOnInit() {
		console.log('Initialising...');
		this.item = this._ItemListService.ItemList.items[0];
		this.panelItem = this._ItemListService.ItemList.items[0];
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

