import { Component,  OnInit, Output, EventEmitter, OnDestroy } from '@angular/core';
import { ReviewSetsService, ReviewSet } from '../services/ReviewSets.service';
import { ComparisonsService, Comparison } from '../services/comparisons.service';
import { Router } from '@angular/router';
import { ItemListService, Item, Criteria } from '../services/ItemList.service';
import { ItemSet } from '../services/ItemCoding.service';
import { ReconciliationService, ReconcilingItemList, ReconcilingItem, ReconcilingReviewSet, ReconcilingCode } from '../services/reconciliation.service';
import { ItemDocsService } from '../services/itemdocs.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { Subscription } from 'rxjs';


@Component({
	selector: 'ComparisonReconciliationComp',
	templateUrl: './comparisonReconciliation.component.html',
    styles: [`
               .bg-comp {    
                    background-color: #C2F3C0;
                }
               .bg-comp-sel {    
                    background-color: #95E292;
                }
               .bg-incomp {    
                    background-color: #EEEEEE;
                }
               .bg-incomp-sel {    
                    background-color: #DCDCDC;
                }
                .table-bordered th, .table-bordered td {
                    border: 1px solid #888888;
                } 
            `],
    providers: []
})

export class ComparisonReconciliationComp extends BusyAwareService implements OnInit, OnDestroy {

	constructor(
		private router: Router, 
		private _reviewSetsService: ReviewSetsService,
		private _ItemListService: ItemListService,
		private _comparisonsService: ComparisonsService,
		private _reconciliationService: ReconciliationService,
		private _ItemDocsService: ItemDocsService

	) {
		super();
	}

	@Output() criteriaChange = new EventEmitter();
	private localList: ReconcilingItemList = new ReconcilingItemList(new ReviewSet(),
		new Comparison(), ""
	);
	private ReviewSet: ReviewSet = new ReviewSet();
	private item: Item = new Item();
	public CurrentComparison: Comparison = new Comparison();
	public DetailsView: boolean = false;
	public panelItem: Item | undefined = new Item();
	public hideme = [];
	public hidemeOne = [];
	public hidemeTwo = [];
	public hidemeThree = [];
	public hidemearms = [];
	public hidemearmsTwo = [];
	public hidemearmsThree = [];
    public get CodeSets(): ReviewSet[] {
		return this._reviewSetsService.ReviewSets.filter(x => x.setType.allowComparison != false);
	}
	public get FlatAttributes(): ReconcilingCode[] {
		return this.localList.Attributes;
    }
	public allItems: ReconcilingItem[] = [];
	public testBool: boolean = false;
	public selectedRow: number = 0;
	private subscription: Subscription | null = null;

	ngOnInit() {
		if (this.subscription) {
			this.subscription.unsubscribe();
		}
		this.subscription = this._ItemListService.ListChanged.subscribe(

			() => {
				this.item = this._ItemListService.ItemList.items[0];
				this.panelItem = this._ItemListService.ItemList.items[0];
				if (this.panelItem) {
					this.RefreshDataItems(this.panelItem);
					this.testBool = true;
				}
			}
		);
		if (this.testBool) {
			this.item = this._ItemListService.ItemList.items[0];
			this.panelItem = this._ItemListService.ItemList.items[0];
			if (this.panelItem) {
				this.RefreshDataItems(this.panelItem);
				this.testBool = false;
			}
		}
	}
	public IsServiceBusy(): boolean {
		if (this._BusyMethods.length > 0
			|| this._reviewSetsService.IsBusy
			|| this._ItemListService.IsBusy
			|| this._comparisonsService.IsBusy
			|| this._reconciliationService.IsBusy
			|| this._ItemDocsService.IsBusy) return true;
		else {
			return false;
		}
	}
	getReconciliations() {
		if (this.item != null && this.item != undefined) {
			this.CurrentComparison = this._comparisonsService.currentComparison;
			if (this.CurrentComparison) {
                this.ReviewSet = this._reviewSetsService.ReviewSets.filter(
					x => x.set_id == this.CurrentComparison.setId)[0];
				this.localList = new ReconcilingItemList(this.ReviewSet,
					this.CurrentComparison, "");
				this._BusyMethods.push("recursiveItemList");
				let i: number = 0;
				this.recursiveItemList(i);
			}
		}
	}
	ToDetailsView() {
		this.PrepareDetailsViewData();
		this.DetailsView = true;
    }
	OpenItem(itemId: number) {
		if (itemId > 0) {
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

						this.RemoveBusy("recursiveItemList");
						this.allItems  = this.localList.Items;
						return;
					}
				}
			);
    }
    UpdateItem(item: Item) {
        let ItemSetlst: ItemSet[] = [];
        this._reconciliationService.FetchItemSetList(item.itemId)
            .then(
                (res: ItemSet[]) => {
                    ItemSetlst = res;
                    let tmp = this.localList.GenerateItem(item, ItemSetlst);
                    if (tmp) {
                        //we want to substitute the new reconciling item in the current list
                        let index = this.localList.Items.findIndex(found => found.Item.itemId == item.itemId);
                        if (index > -1) {
                            this.localList.Items[index] = tmp;
                        }
                    }
                    
                }
            );
    }
	RefreshDataItems(item: Item) {
		this.panelItem = this._ItemListService.ItemList.items[0];
		this.getReconciliations();
		if (this.panelItem && this.panelItem != undefined) {
			if (this.panelItem) {
				this.getItemDocuments(this.panelItem.itemId);
				this._reconciliationService.FetchArmsForReconItems(
					this._ItemListService.ItemList.items);

			}
		}
	}
	RefreshDataItem(item: Item) {
		
		if (item == null) {
			this.panelItem = this._ItemListService.ItemList.items[0];
		} else {
			this.panelItem = item;
		}
		//this.getReconciliations();
        this.UpdateItem(item);
		let tempItems: Item[] = [];
		tempItems[0] = this.panelItem;
		if (this.panelItem && this.panelItem != undefined) {
			if (item.itemId) {
				//this.getItemDocuments(this.panelItem.itemId);
				this._reconciliationService.FetchArmsForReconItems(
					tempItems);
			}
		}
	}
    getItemDocuments(itemid: number) {
        if (this._ItemDocsService.CurrentItemId != itemid) this._ItemDocsService.FetchDocList(itemid);
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
			//alert(recon.Item.shortTitle);
			this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, 0, false, false)
				.then(
				() => {
					this.RefreshDataItem(recon.Item);
					}
				);
		}
	}
	public Complete(recon: ReconcilingItem, contactID: number) {
		if (recon && this.CurrentComparison) {
			//alert(contactID);
			this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, contactID, true, false)
				.then(
					() => {
						this.RefreshDataItem(recon.Item);
					}
				);
		}
	}
	public CompleteAndLock(recon: ReconcilingItem, contactID: number) {
		if (recon && this.CurrentComparison) {
			this._reconciliationService.ItemSetCompleteComparison(recon, this.CurrentComparison, contactID, true, true)
				.then(
					() => {
						this.RefreshDataItem(recon.Item);
					}
				);
		}
	}
	public reconcilingCodeArrayLength(len: number): any {

		return Array.from({ length: len }, (v, k) => k + 1);
	}
	public ChangePanelItem(itemid: number, index: number) {

		this.selectedRow = index;
		let tempItemList = this._ItemListService.ItemList.items;
        this.panelItem = tempItemList.find(x => x.itemId == itemid);
		this.getItemDocuments(itemid);
		if (this.DetailsView == true) this.PrepareDetailsViewData();
	}
	public ReconcilingReviewSet: ReconcilingReviewSet | null = null;
	private PrepareDetailsViewData() {
		if (this.panelItem) {
			let rrs: ReconcilingReviewSet = new ReconcilingReviewSet(this.ReviewSet
				, this.allItems[this.selectedRow].CodesReviewer1
				, this.allItems[this.selectedRow].CodesReviewer2
				, this.allItems[this.selectedRow].CodesReviewer3);
			//console.log("justaTest", rrs);
			this.ReconcilingReviewSet = rrs;
        }
    }
	BackToMain() {
		this.router.navigate(['Main']);
	}
	Clear() {
		 this.CurrentComparison = new Comparison();
		 this.panelItem  = new Item();
		 this.hideme = [];
    }
    ngOnDestroy() {
        console.log("Destroy reconcile Page");
        if (this.subscription) {
            this.subscription.unsubscribe();
            this.subscription = null;
        }
    }
}

