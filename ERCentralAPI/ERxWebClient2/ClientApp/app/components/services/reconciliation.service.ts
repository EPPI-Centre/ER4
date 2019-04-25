import { Inject, Injectable} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ModalService } from './modal.service';
import { BusyAwareService } from '../helpers/BusyAwareService';
import { ComparisonsService, Comparison } from './comparisons.service';
import { ReviewSet, SetAttribute } from './ReviewSets.service';
import { Item } from './ItemList.service';
import { ItemSet } from './ItemCoding.service';

@Injectable({

	providedIn: 'root',

})

export class ReconciliationService extends BusyAwareService {

    constructor(
        private _httpC: HttpClient,
		private modalService: ModalService,
		private comparisonsService: ComparisonsService,
        @Inject('BASE_URL') private _baseUrl: string
        ) {
        super();
    }

	//private _ClassifierModelList: ClassifierModel[] = [];
	////@Output() searchesChanged = new EventEmitter();
	////public crit: CriteriaSearch = new CriteriaSearch();
	//public searchToBeDeleted: string = '';//WHY string?

	//public get ClassifierModelList(): ClassifierModel[] {

	//	return this._ClassifierModelList;

	//}

	//public set ClassifierModelList(models: ClassifierModel[]) {

	//	this._ClassifierModelList = models;
	//}

	//Fetch() {

	//	this._BusyMethods.push("Fetch");

	//	this._httpC.get<any>(this._baseUrl + 'api/Classifier/GetClassifierModelList',
	//	)
	//		.subscribe(result => {

	//			this.ClassifierModelList = result;
	//			console.log(result)
	//		},
	//			error => {
	//				this.modalService.GenericError(error);
 //                   this.RemoveBusy("Fetch");
	//			}
	//			, () => {
	//				this.RemoveBusy("Fetch");
	//			}
	//		);

	//}

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

	public ReconcilingItemList(Set: ReviewSet, comp: Comparison, Descr: string) {
		this._Items = [];
		this._Attributes = [];
		this._Comparison = comp;
		this.Description = Descr;

		for (let CaSet of Set.attributes) {
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
				else if (iSet.contactId == this._Comparison.contactId2) {

					itSetR2 = iSet.itemSetId;

					for (let roia of iSet.itemAttributesList) {
						let r0: ReconcilingCode | null = this.GetReconcilingCodeFromID(roia.itemAttributeId);

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
						{//in such situations r0 is null

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
	get item(): Item { return this._Item; }

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