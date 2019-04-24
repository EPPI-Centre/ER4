import { Component,  OnInit,  Output, EventEmitter } from '@angular/core';
import { ReviewSetsService, SetAttribute, ReviewSet } from '../services/ReviewSets.service';
import { ReviewInfoService, Contact } from '../services/ReviewInfo.service';
import { ComparisonsService, Comparison } from '../services/comparisons.service';
import { Router } from '@angular/router';
import { ItemListService, Item } from '../services/ItemList.service';
import { ItemCodingService, ItemSet } from '../services/ItemCoding.service';


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
		private itemCodingService: ItemCodingService
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

enum Visibility 
{
	Visible = 0,
	Collapsed = 1
}

export class ReconcilingItemList 
{
		private _Attributes: ReconcilingCode[] = [];
		public Attributes: ReconcilingCode[] = [];
		public Items: ReconcilingItem[] = [];
		public Description: string = '';
		get Comparison(): Comparison {
			return this._Comparison;
		}
		private _Comparison: Comparison = new Comparison();
		public ShowReviewer3: boolean = false;
		public Reviewer3Visibility: Visibility = Visibility.Collapsed;
		public Reviewer1: string = '';
	
		public GetReconcilingCodeFromID(AttributeID: number): ReconcilingCode | null
		{
			if (this._Attributes == null) return null;

			for(let rcc of this._Attributes)
			{
				if (rcc.ID == AttributeID) return rcc;
			}
			return null;
		}

	public ReconcilingItemList(Set: ReviewSet, comp: Comparison, Descr: string)
	{
		this.Items = [];
		this.Attributes = [];
		this._Comparison = comp;
		this.Description = Descr;

		for(let CaSet of Set.attributes)
		{
			this.buildToPasteFlatUnsortedList(CaSet, "");
		}
	}

	private buildToPasteFlatUnsortedList(aSet: SetAttribute, path: string): any {//this is recursive!!

		let astp: ReconcilingCode = new ReconcilingCode(aSet.attribute_id,
			aSet.attributeSetId, aSet.attribute_name, path);

		this._Attributes.push(astp);
		for (let CaSet of aSet.attributes) {
			this.buildToPasteFlatUnsortedList(CaSet, path + "<¬sep¬>" + aSet.attribute_name);
		}
	}


	public AddItem(item: Item, itemSetList: ItemSet[]): any
	{
		if (this._Comparison == null || itemSetList == null || itemSetList.length == 0) return;

		let isCompleted: boolean = false;
		let CompletedBy: string = "";
		let CompletedByID: number = 0;
		let CompletedItemSetID: number = 0;
		let r1: ReconcilingCode[] = [];
		let r2: ReconcilingCode[] = [];
		let r3: ReconcilingCode[] = [];

		let itSetR1: number = -1, itSetR2: number = -1, itSetR3: number  = -1;


		for(let iSet of itemSetList)
		{
			if (iSet.setId != this.Comparison.setId) continue;
			else {

				if (iSet.isCompleted) {
					isCompleted = iSet.isCompleted;
					CompletedBy = iSet.contactName;
					CompletedByID = iSet.contactId;
					CompletedItemSetID = iSet.itemSetId;
				}

				if (iSet.contactId == this._Comparison.contactId1) {
					itSetR1 = iSet.itemSetId;

					for(let roia of iSet.itemAttributesList)
					{
						let r0: ReconcilingCode | null = this.GetReconcilingCodeFromID(roia.attributeId);
						if (r0 != null)
						//this is necessary to avoid trying to add a code that 
						//belongs to the item, but is coming from a dead branch(a
						//code for wich one of the parents got deleted) !
						{//in such situations r0 is null
							let r: ReconcilingCode = r0.Clone();

							r.InfoBox = roia.additionalText;
							r.ArmID = roia.armId;
							r.ArmName = roia.armTitle;
							r1.push(r);
						}
					}
				}
				//else if (iSet.ContactId == _Comparison.ContactId2) {
				//	itSetR2 = iSet.ItemSetId;
				//	foreach(ReadOnlyItemAttribute roia in iSet.ItemAttributes)
				//	{
				//		ReconcilingCode r0 = GetReconcilingCodeFromID(roia.AttributeId);
				//		if (r0 != null)//this is necessary to avoid trying to add a code that belongs to the item, but is coming from a dead branch (a code for wich one of the parents got deleted)!
				//		{//in such situations r0 is null
				//			ReconcilingCode r = r0.Clone();
				//			r.InfoBox = roia.AdditionalText;
				//			r.ArmID = roia.ArmId;
				//			r.ArmName = roia.ArmTitle;
				//			r2.Add(r);
				//		}
				//	}
				//}
				//else if (iSet.ContactId == _Comparison.ContactId3) {
				//	itSetR3 = iSet.ItemSetId;
				//	foreach(ReadOnlyItemAttribute roia in iSet.ItemAttributes)
				//	{
				//		ReconcilingCode r0 = GetReconcilingCodeFromID(roia.AttributeId);
				//		if (r0 != null)//this is necessary to avoid trying to add a code that belongs to the item, but is coming from a dead branch (a code for wich one of the parents got deleted)!
				//		{//in such situations r0 is null
				//			ReconcilingCode r = r0.Clone();
				//			r.InfoBox = roia.AdditionalText;
				//			r.ArmID = roia.ArmId;
				//			r.ArmName = roia.ArmTitle;
				//			r3.Add(r);
				//		}
				//	}
				//}

			}
		}
		//this._Items.Add(new ReconcilingItem(item, isCompleted, r1, r2, r3, 
		//CompletedBy, CompletedByID, CompletedItemSetID, itSetR1, itSetR2, itSetR3));

	}
	
}


export class ReconcilingCode {

	private _ID: number = 0;
	private _AttributeSetID: number = 0;
	private _ArmID: number = 0;
	private _Name: string = '';
	private _ArmName: string = '';
	private _Fullpath: string = '';
	private _InfoBox: string = '';


	get ID(): number {
		return this._ID;
	}

	get AttributeSetID(): number {
		return this._AttributeSetID;
	}

	get ArmID(): number {
		return this._ArmID;
	}

	set ArmID(value: number) {
		this._ArmID = value;
	}


	get ArmName(): string {
		return this._ArmName;
	}

	set ArmName(value: string) {
		this._ArmName = value;
	}

	get Name(): string {
		return this._Name;
	}

	get Fullpath(): string {
		return this._Fullpath;
	}

	get InfoBox(): string	{
		return this._InfoBox;
	}

	set InfoBox(value: string) {
		this._InfoBox = value;
	}


	constructor(AttributeID: number, attributeSetID: number,
		name: string, fullpath: string) {

		this._ID = AttributeID;
		this._Name = name;
		this._Fullpath = fullpath;
		this._AttributeSetID = attributeSetID;

	}
}

export class ReconcilingItem {

	public item: Item = new Item();
	public isCompleted: boolean = false;

	public codesReviewer1: ReconcilingCode[] = [];
	public codesReviewer2: ReconcilingCode[] = [];
	public codesReviewer3: ReconcilingCode[] = [];
    
	public completedby: string = '';
	public completedbyID: number = 0;
	public completedItemSetID: number = 0;
	public itemsetR1: number = 0;
	public itemsetR2: number = 0;
	public itemsetR3: number = 0;

}