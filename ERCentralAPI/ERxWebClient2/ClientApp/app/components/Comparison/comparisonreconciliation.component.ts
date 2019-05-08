import { Component,  OnInit,  Output, EventEmitter } from '@angular/core';
import { ReviewSetsService, SetAttribute, ReviewSet } from '../services/ReviewSets.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ComparisonsService, Comparison } from '../services/comparisons.service';
import { Router } from '@angular/router';
import { ItemListService, Item, Criteria } from '../services/ItemList.service';
import { ItemSet } from '../services/ItemCoding.service';
import { ReconciliationService, ReconcilingItemList, ReconcilingItem } from '../services/reconciliation.service';
import { ItemDocsService } from '../services/itemdocs.service';

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
		private _reconciliationService: ReconciliationService,
		private _ItemDocsService: ItemDocsService

	) { }

	private localList: ReconcilingItemList = new ReconcilingItemList(new ReviewSet(),
		new Comparison(), ""
		);
	public CurrentComparison: Comparison = new Comparison();
	private ReviewSet: ReviewSet = new ReviewSet();
	public item: Item = new Item();
	public panelItem: Item | undefined = new Item();
	public PanelName: string = '';
	public chosenFilter: SetAttribute | null = null;
	public hideme = [];
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
		this._ItemListService.FetchWithCrit(crit, listDescription);
		this.item = this._ItemListService.ItemList.items[0];
	}
	getReconciliations() {

		if (this.item != null && this.item != undefined) {
			this.CurrentComparison = this._comparisonsService.currentComparison;
			this.ReviewSet = this._reviewSetsService.GetReviewSets().filter(
				x => x.set_id == this.CurrentComparison.setId)[0];
			this.localList = new ReconcilingItemList(this.ReviewSet,
				this.CurrentComparison, "testing right now");
			let i: number = 0;
			this.recursiveItemList(i);
		}
	}
	public reconcilingCodeArrayLength(len: number): any {

		return	Array.from({ length: len }, (v, k) => k + 1);
	}
	public recursiveItemList(i: number) {
		let ItemSetListTest: ItemSet[] = [];
		this._reconciliationService.FetchItemSetList(this._ItemListService.ItemList.items[i].itemId)

			.then(
				(res: ItemSet[]) => {

					ItemSetListTest = res;
					this.localList.AddItem(this._ItemListService.ItemList.items[i], ItemSetListTest);
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

		let tempItemList = this._ItemListService.ItemList.items;
		this.panelItem = tempItemList.find(x => x.itemId == itemid);
		this.getItemDocuments(itemid);
	}
	public RefreshData() {

		this.panelItem = this._ItemListService.ItemList.items[0];
		this.getReconciliations();
		if (this.panelItem) {

			this.getItemDocuments(this.panelItem.itemId);
		}

		this._reconciliationService.FetchArmsForReconItems(this._ItemListService.ItemList.items);
	}
	getItemDocuments(itemid: number) {
		this._ItemDocsService.FetchDocList(itemid);
	}
	getReconSplitArray(fullPath: string): string[] {
		if (fullPath != '') {
			return fullPath.split(',');
		} else {
			return [];
		}
	}
	ShowFullPath(): boolean {

		if (true) {

		}
		return false;
	}
	UnComplete(recon: ReconcilingItem) {

		this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, 0, false)
			.then(
				() => {
					this.RefreshData();
				}
			);
	}
	Complete(recon: ReconcilingItem, contactID: number) {

		this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, contactID, true)
			.then(
				() => {
					this.RefreshData();
				}
			);
	}
	ngOnInit() {
		this.item = this._ItemListService.ItemList.items[0];
		this.panelItem = this._ItemListService.ItemList.items[0];
		this.RefreshData();
	}
	BackToMain() {
		this.router.navigate(['Main']);
	}
	Clear() {
		this.selectedCodeSet = new ReviewSet();
		// NEED TO ADD CODE FOR THIS...!!!!!!!!!!!!!!!!!!!!!!!!!!
	}
}

