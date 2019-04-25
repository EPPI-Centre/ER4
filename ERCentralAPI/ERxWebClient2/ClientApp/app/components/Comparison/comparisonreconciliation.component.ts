import { Component,  OnInit,  Output, EventEmitter } from '@angular/core';
import { ReviewSetsService, SetAttribute, ReviewSet } from '../services/ReviewSets.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ComparisonsService, Comparison } from '../services/comparisons.service';
import { Router } from '@angular/router';
import { ItemListService, Item } from '../services/ItemList.service';
import { ItemCodingService } from '../services/ItemCoding.service';
import { ReconciliationService, ReconcilingItemList } from '../services/reconciliation.service';


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

	private localList: ReconcilingItemList = new ReconcilingItemList();
	public CurrentComparison: Comparison = new Comparison();
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

		// Fill with dummy reference data for viewing the reference information
		// in the page
		this.item = this.ItemListService.ItemList.items[0];
		this.CurrentComparison = this.comparisonsService.currentComparison;

	}
	ngOnInit() {

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

