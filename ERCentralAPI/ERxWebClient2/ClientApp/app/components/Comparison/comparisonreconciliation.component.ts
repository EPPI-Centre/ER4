import { Component,  OnInit, Output, EventEmitter } from '@angular/core';
import { ReviewSetsService, ReviewSet } from '../services/ReviewSets.service';
import { ComparisonsService, Comparison } from '../services/comparisons.service';
import { Router } from '@angular/router';
import { ItemListService, Item } from '../services/ItemList.service';
import { ItemSet } from '../services/ItemCoding.service';
import { ReconciliationService, ReconcilingItemList, ReconcilingItem } from '../services/reconciliation.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { EventEmitterService } from '../services/EventEmitter.service';
import { PagerService } from '../services/pagination.service';

@Component({
	selector: 'ComparisonReconciliationComp',
	templateUrl: './comparisonReconciliation.component.html',
    providers: []
})

export class ComparisonReconciliationComp implements OnInit {

	constructor(
		private router: Router, 
		private _reviewSetsService: ReviewSetsService,
		private _ItemListService: ItemListService,
		private _comparisonsService: ComparisonsService,
		private _reconciliationService: ReconciliationService,
		private _ItemDocsService: ItemDocsService,
		private pagerService: PagerService,
		private _eventEmitterService: EventEmitterService

	) { }

	@Output() criteriaChange = new EventEmitter();
	private localList: ReconcilingItemList = new ReconcilingItemList(new ReviewSet(),
		new Comparison(), ""
	);
	private ReviewSet: ReviewSet = new ReviewSet();
	private item: Item = new Item();
	public CurrentComparison: Comparison = new Comparison();
	public panelItem: Item | undefined = new Item();
	public hideme = [];
	public hidemeOne = [];
	public hidemeTwo = [];
	public hidemeThree = [];
	public hidemearms = [];
    public get CodeSets(): ReviewSet[] {
		return this._reviewSetsService.ReviewSets.filter(x => x.setType.allowComparison != false);
    }

	getReconciliations() {

		if (this.item != null && this.item != undefined) {
			this.CurrentComparison = this._comparisonsService.currentComparison;
			if (this.CurrentComparison) {
				this.ReviewSet = this._reviewSetsService.GetReviewSets().filter(
					x => x.set_id == this.CurrentComparison.setId)[0];
				this.localList = new ReconcilingItemList(this.ReviewSet,
					this.CurrentComparison, "");
				let i: number = 0;
				this.recursiveItemList(i);

			}
		}
	}
	
	OpenItem(itemId: number) {

		if (itemId > 0) {
		
			//if (this.Context == 'FullUI') this.router.navigate(['itemcoding', itemId]);
			//else if (this.Context == 'CodingOnly') this.router.navigate(['itemcodingOnly', itemId]);
			this.router.navigate(['itemcoding', itemId]);
		}
	}

	recursiveItemList(i: number) {
		let ItemSetlst: ItemSet[] = [];
		this._reconciliationService.FetchItemSetList(this._ItemListService.ItemList.items[i].itemId)

			.then(
				(res: ItemSet[]) => {

					ItemSetlst = res;
					this.localList.AddItem(this._ItemListService.ItemList.items[i], ItemSetlst);
					
					if (i < this._ItemListService.ItemList.items.length-1) {
						i = i + 1;
						this.recursiveItemList(i);
					} else {

						this._eventEmitterService.reconDataChanged.emit();
						this._eventEmitterService.reconcilingArr = this.localList.Items;

						return;
					}
				}
			);
	}
	RefreshData() {

		this.panelItem = this._ItemListService.ItemList.items[0];
		this.getReconciliations();
		
		if (this.panelItem && this.panelItem != undefined) {
			let itemID: number = this.panelItem.itemId;
			if (itemID) {
				this.getItemDocuments(this.panelItem.itemId);
				this._reconciliationService.FetchArmsForReconItems(
					this._ItemListService.ItemList.items);
			}
		}
	}
	
	getItemDocuments(itemid: number) {
		this._ItemDocsService.FetchDocList(itemid);
	}
	public getReconSplitArray(fullPath: string): string[] {
		if (fullPath != '') {
			return fullPath.split(',');
		} else {
			return [];
		}
	}
	public UnComplete(recon: ReconcilingItem) {

		if (recon && this.CurrentComparison) {
			this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, 0, false)
				.then(
					() => {
						this.RefreshData();
					}
				);
		}
	}
	public Complete(recon: ReconcilingItem, contactID: number) {
		if (recon && this.CurrentComparison) {
			this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, contactID, true)
				.then(
					() => {
						this.RefreshData();
		
					}
				);
		}
	}
	public reconcilingCodeArrayLength(len: number): any {

		return Array.from({ length: len }, (v, k) => k + 1);
	}
	public ChangePanelItem(itemid: number) {

		let tempItemList = this._ItemListService.ItemList.items;
		this.panelItem = tempItemList.find(x => x.itemId == itemid);
		this.getItemDocuments(itemid);
	}
	ngOnInit() {
		this.item = this._ItemListService.ItemList.items[0];
		this.panelItem = this._ItemListService.ItemList.items[0];
		this.RefreshData();

		this._eventEmitterService.reconDataChanged.subscribe(
			() => {

				this._eventEmitterService.reconcilingArr = this.localList.Items;
			}
		);
	}
	BackToMain() {
		this.router.navigate(['Main']);
	}
	Clear() {

		 this.CurrentComparison = new Comparison();
		 this.panelItem  = new Item();
		 this.hideme = [];
	}
}

