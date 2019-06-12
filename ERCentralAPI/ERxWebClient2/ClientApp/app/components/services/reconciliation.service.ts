import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BusyAwareService } from '../helpers/BusyAwareService';
import {  Comparison } from './comparisons.service';
import { ReviewSet, SetAttribute, ItemSetCompleteCommand, ReviewSetsService } from './ReviewSets.service';
import { Item, Criteria } from './ItemList.service';
import { ItemSet } from './ItemCoding.service';
import { ArmsService } from './arms.service';
import { ModalService } from './modal.service';
import { EventEmitterService } from './EventEmitter.service';

@Injectable({

	providedIn: 'root',

})

export class ReconciliationService extends BusyAwareService {

	constructor(
		private _httpC: HttpClient,
		private _armsService: ArmsService,
		private _modalService: ModalService,
		private _ReviewSetsService: ReviewSetsService,
		@Inject('BASE_URL') private _baseUrl: string
	) {
		super();
	}

	//public localList: ReconcilingItemList = new ReconcilingItemList(new ReviewSet(),
	//	new Comparison(), ""
	//);
	public reconcilingArr: any[] = [];

	FetchItemSetList(ItemIDCrit: number): Promise<ItemSet[]> {

		this._BusyMethods.push("FetchItemSetList");
		let body = JSON.stringify({ Value: ItemIDCrit });

		return this._httpC.post<ItemSet[]>(this._baseUrl + 'api/ItemSetList/Fetch', body
			)
			.toPromise().then(

			(res: ItemSet[]) => {
			
				this.RemoveBusy('FetchItemSetList');
				return res;
			},
			(error) => {
				this.RemoveBusy("FetchItemSetList");
				this._modalService.GenericError(error);
				return error;
			}
		);
	}

	async FetchArmsForReconItems(items: Item[]) {

		for (var i = 0; i < items.length; i++) {
			await this._armsService.FetchPromiseArms(items[i]);
		}
		//return items;
	}

	ItemSetCompleteComparison(recon: ReconcilingItem, comp: Comparison,
		contactID: number, completeOrNot: boolean, LockOrNot: boolean): Promise<ReviewSet> {

		this._BusyMethods.push("ItemSetCompleteComparison");

		let bt: number = contactID;
		let isi: number = -1;
		let completor: string = "";
		let cmd: ItemSetCompleteCommand = new ItemSetCompleteCommand();

		if (completeOrNot) {
			if (comp.contactId1 == bt) {
				isi = recon.ItemSetR1;
				completor = comp.contactName1;
			}
			else if (comp.contactId2 == bt) {
				isi = recon.ItemSetR2;
				completor = comp.contactName2;
			}
			else if (comp.contactId3 == bt) {
				isi = recon.ItemSetR3;
				completor = comp.contactName3;
			}
			cmd.itemSetId = isi;
			cmd.isLocked = LockOrNot;
			cmd.complete = true;
		}
		else {

			let completedByID: number = recon.CompletedByID;
			if (comp.contactId1 == completedByID) {
				isi = recon.ItemSetR1;
			}
			else if (comp.contactId2 == completedByID) {
				isi = recon.ItemSetR2;
			}
			else if (comp.contactId3 == completedByID) {
				isi = recon.ItemSetR3;
			}
			else {
				isi = recon.CompletedItemSetID;
			}
			cmd.itemSetId = isi;
			cmd.isLocked = LockOrNot;
			cmd.complete = false;
		}
		alert('testing...' + cmd.itemSetId);

		return this._httpC.post<ItemSetCompleteCommand>(this._baseUrl + 'api/ItemSetList/ExcecuteItemSetCompleteCommand', cmd
		)
			.toPromise().then(
			(res) => {
				let rSet = this._ReviewSetsService.ReviewSets.find(found => found.ItemSetId == cmd.itemSetId);
					this.RemoveBusy('ItemSetCompleteComparison');
					if (res.successful != null && res.successful) {
						
						if (rSet) {
							rSet.codingComplete = cmd.complete;
							rSet.itemSetIsLocked = cmd.isLocked;
						} else {
							return Error;
						}
					}
					return rSet;
				},
				(error) => {
					this.RemoveBusy("ItemSetCompleteComparison");
					this._modalService.GenericError(error);
					return error;
				}
			);
	}

	ngOnInit() {

	}
}

enum Visibility {

	Visible = 0,
	Collapsed = 1

}
export class ReconcilingItemList {

	private _Attributes: ReconcilingCode[] = [];
	get Attributes(): ReconcilingCode[] { return this._Attributes; }
	private _Items: ReconcilingItem[] = [];
	get Items(): ReconcilingItem[] { return this._Items }
	public Description: string = '';
	get Comparison(): Comparison {
		return this._Comparison;
	}
	private _Comparison: Comparison = new Comparison();
	public ShowReviewer3: boolean = false;
	public Reviewer3Visibility: Visibility = Visibility.Collapsed;
	public Reviewer1: string = '';

	public GetReconcilingCodeFromID(AttributeID: number): ReconcilingCode | null {
		if (this._Attributes == null) return null;

		for (let rcc of this._Attributes) {
			if (rcc.ID == AttributeID) return rcc;
		}
		return null;
	}

	constructor(Set: ReviewSet, comp: Comparison, Descr: string) {
		this._Items = [];
		this._Attributes = [];
		this._Comparison = comp;
		this.Description = Descr;

		if (Set != null && Set != undefined ) {
			if (Set.attributes != null && Set.attributes.length > 0) {
				for (let CaSet of Set.attributes) {
					this.buildToPasteFlatUnsortedList(CaSet, "");
				}
			}

		}

	}

	private buildToPasteFlatUnsortedList(aSet: SetAttribute, path: string): any {//this is recursive!!

		let astp: ReconcilingCode = new ReconcilingCode(aSet.attribute_id,
			aSet.attributeSetId, aSet.attribute_name, path);

		this._Attributes.push(astp);

		for (let CaSet of aSet.attributes) {
			this.buildToPasteFlatUnsortedList(CaSet, path + "," + aSet.attribute_name);
		}
	}

	public AddItem(item: Item, itemSetList: ItemSet[]): any {

		if (this._Comparison == null || itemSetList == null || itemSetList.length == 0) return;

		let isCompleted: boolean = false;
		let CompletedBy: string = "";
		let CompletedByID: number = 0;
		let CompletedItemSetID: number = 0;
		let r1: ReconcilingCode[] = [];
		let r2: ReconcilingCode[] = [];
		let r3: ReconcilingCode[] = [];

		let itSetR1: number = -1, itSetR2: number = -1, itSetR3: number = -1;

		for (let iSet of itemSetList) {
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
					for (let roia of iSet.itemAttributesList) {
						let r0: ReconcilingCode | null = this.GetReconcilingCodeFromID(roia.attributeId);
						if (r0 != null)
						{
							let r: ReconcilingCode = r0.Clone();

							r.InfoBox = roia.additionalText;
							r.ArmID = roia.armId;
							r.ArmName = roia.armTitle;
							r1.push(r);
							
						}
					}
				}
				else if (iSet.contactId == this._Comparison.contactId2) {
					itSetR2 = iSet.itemSetId;
					for (let roia of iSet.itemAttributesList) {
						let r0: ReconcilingCode | null = this.GetReconcilingCodeFromID(roia.attributeId);
						if (r0 != null)//this is necessary to avoid trying to add a code that belongs to the item, but is coming from a dead branch (a code for wich one of the parents got deleted)!
						{//in such situations r0 is null
							let r: ReconcilingCode = r0.Clone();
							r.InfoBox = roia.additionalText;
							r.ArmID = roia.armId;
							r.ArmName = roia.armTitle;
							r2.push(r);
						}
					}
				}
				else if (iSet.contactId == this._Comparison.contactId3) {
					itSetR3 = iSet.itemSetId;
					for (let roia of iSet.itemAttributesList) {
						let r0: ReconcilingCode | null = this.GetReconcilingCodeFromID(roia.attributeId);
						if (r0 != null)//this is necessary to avoid trying to add a code that belongs to the item, but is coming from a dead branch (a code for wich one of the parents got deleted)!
						{
							let r = r0.Clone();
							r.InfoBox = roia.additionalText;
							r.ArmID = roia.armId;
							r.ArmName = roia.armTitle;
							r3.push(r);
						}
					}
				}

			}
		}
		this._Items.push(new ReconcilingItem(item, isCompleted, r1, r2, r3,
			CompletedBy, CompletedByID, CompletedItemSetID, itSetR1, itSetR2, itSetR3));
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

	get InfoBox(): string {
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

	public Clone(): ReconcilingCode {

		let res: ReconcilingCode =
			new ReconcilingCode(this.ID, this.AttributeSetID, this.Name, this.Fullpath);

		return res;
	}
}
export class ReconcilingItem {
	private _Item: Item = new Item();
	get Item(): Item { return this._Item; }

	private _IsCompleted: boolean = false;
	get IsCompleted(): boolean { return this._IsCompleted; }

	private _CodesReviewer1: ReconcilingCode[] = [];
	get CodesReviewer1(): ReconcilingCode[] { return this._CodesReviewer1; }

	private _CodesReviewer2: ReconcilingCode[] = [];
	get CodesReviewer2(): ReconcilingCode[] { return this._CodesReviewer2; }

	private _CodesReviewer3: ReconcilingCode[] = [];
	get CodesReviewer3(): ReconcilingCode[] { return this._CodesReviewer3; }

	private _CompletedByName: string = '';
	get CompletedByName(): string { return this._CompletedByName }
	private _Completedby: string = '';
	get Completedby(): string { return this._Completedby; }
	private _CompletedByID: number = 0;
	get CompletedByID(): number { return this._CompletedByID; }
	set CompletedByID(value: number) { this._CompletedByID = value; }
	private _CompletedItemSetID: number = 0;
	get CompletedItemSetID(): number { return this._CompletedItemSetID; }
	private _ItemSetR1: number = 0;
	get ItemSetR1(): number { return this._ItemSetR1; }
	private _ItemSetR2: number = 0;
	get ItemSetR2(): number { return this._ItemSetR2; }
	private _ItemSetR3: number = 0;
	get ItemSetR3(): number { return this._ItemSetR3; }

	constructor(item: Item, isCompleted: boolean, codesReviewer1: ReconcilingCode[],
		codesReviewer2: ReconcilingCode[], codesReviewer3: ReconcilingCode[]
		, completedby: string, completedbyID: number, completedItemSetID: number
		, itemsetR1: number, itemsetR2: number, itemsetR3: number) {

		this._Item = item;
		this._CodesReviewer1 = codesReviewer1;
		this._CodesReviewer2 = codesReviewer2;
		this._CodesReviewer3 = codesReviewer3;
		this._IsCompleted = isCompleted;
		this._CompletedByName = completedby;
		this._CompletedByID = completedbyID;
		this._CompletedItemSetID = completedItemSetID;
		this._ItemSetR1 = itemsetR1;
		this._ItemSetR2 = itemsetR2;
		this._ItemSetR3 = itemsetR3;
	}
}