import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode,  SetAttribute } from '../services/ReviewSets.service';
import { ITreeOptions,  TreeComponent, ITreeState } from 'angular-tree-component';
import { OutcomesService, OutcomeItemAttribute, Outcome } from '../services/outcomes.service';
import {  ReconcilingReviewSet, ReconcilingSetAttribute, ReconcilingCode, ReconcilingItem } from '../services/reconciliation.service';
import { IDTypeDictionary } from 'angular-tree-component/dist/defs/api';
import { Comparison } from '../services/comparisons.service';
import { Helpers } from '../helpers/HelperMethods';
import { forEach } from '@angular/router/src/utils/collection';
import { ItemCodingService, ItemAttPDFCodingCrit } from '../services/ItemCoding.service';
import { ItemDocsService, ItemDocument } from '../services/itemdocs.service';

//see: https://angular2-tree.readme.io/docs

@Component({
	selector: 'ReconcilingCodesetTree',
	styles: [`.bt-infoBox {    
                    padding: .08rem .12rem .12rem .12rem;
                    margin-bottom: .12rem;
                    font-size: .875rem;
                    line-height: 1.2;
                    border-radius: .2rem;
                }
			.no-select{    
				-webkit-user-select: none;
				cursor:not-allowed; /*makes it even more obvious*/
				}
			.bg-comp {    
                    background-color: #C2F3C0;
                }
        `],
	templateUrl: './ReconcilingCodesetTree.component.html'
})

export class ReconcilingCodesetTreeComponent implements OnInit, OnDestroy, AfterViewInit  {
	constructor(private router: Router,
		private _httpC: HttpClient,
		@Inject('BASE_URL') private _baseUrl: string,
		private ReviewerIdentityServ: ReviewerIdentityService,
		private ReviewSetsService: ReviewSetsService,
		private ItemCodingService: ItemCodingService,
		private ItemDocsService: ItemDocsService
	) { }

	@ViewChild('SingleCodeSetTree1') treeComponent!: TreeComponent;

	@Input() reconcilingReviewSet: ReconcilingReviewSet | null = null;
	@Input() CurrentComparison: Comparison = new Comparison();
	private _ReconcilingItem: ReconcilingItem | undefined = undefined;
	private _lastReconcilingItemId: number = 0;
	@Input() public set ReconcilingItem(it: ReconcilingItem | undefined) {
		console.log("set ReconcilingItem", it, this._ReconcilingItem);
		if (it && (this._ReconcilingItem == undefined || this._ReconcilingItem.Item.itemId != it.Item.itemId)) {
			console.log("set ReconcilingItem2", this.reconcilingReviewSet, this.SelectedNode, this._lastReconcilingItemId, it.Item.itemId);
			if (this.reconcilingReviewSet && this.SelectedNode) {
				this.SelectedNode = this.reconcilingReviewSet.FindByIdNumber(this.SelectedNode.attribute_id);
			}
			else if (this.SelectedNode && !this.reconcilingReviewSet) {
				setTimeout(() => {
					console.log("Waiting for the recRevSet to arrive...");
					if (this.reconcilingReviewSet && this.SelectedNode) {
						this.SelectedNode = this.reconcilingReviewSet.FindByIdNumber(this.SelectedNode.attribute_id);
					}
					//if (this._lastReconcilingItemId != it.Item.itemId) {
					//	console.log("set ReconcilingItem3.1!");
					//	this.FindDisagreements();
					//	this._lastReconcilingItemId = it.Item.itemId;
					//}
				}, 200);
			}
			if (this._lastReconcilingItemId != it.Item.itemId) {
				console.log("set ReconcilingItem3!");
				this.FindDisagreements();
				this._lastReconcilingItemId = it.Item.itemId;
			}
        }
		this._ReconcilingItem = it;
	}
	public get ReconcilingItem(): ReconcilingItem | undefined {
		return this._ReconcilingItem;
	}



	@Output() CompleteEvent = new EventEmitter<{ item: ReconcilingItem, contactId: number }>();
	@Output() CompleteAndLockEvent = new EventEmitter<{ item: ReconcilingItem, contactId: number }>();
	@Output() UnCompleteEvent = new EventEmitter<ReconcilingItem>();

	NodesState: ITreeState = {};// see https://angular2-tree.readme.io/docs/save-restore
	SelectedNode: ReconcilingSetAttribute | null = null;
	LoadPDFCoding: boolean = false;
	ShowOutcomes: boolean = false; 
	FindingDisagreements: boolean = false;
	private _Disagreements: ReconcilingSetAttribute[] = [];
	public get Disagreements(): ReconcilingSetAttribute[] {
		return this._Disagreements;
	}
	ngOnInit() {
		if (this.reconcilingReviewSet) {
			//console.log("ngOnInit Will try to expand the root!");
			const expandedNodeIds: any = {};
			expandedNodeIds[this.reconcilingReviewSet.id] = true;
			this.NodesState = {
				...this.NodesState,
				expandedNodeIds
			};
		}
	}
	ngAfterViewInit() {
		
    }
	public get IsServiceBusy(): boolean {
		return this.ReviewSetsService.IsBusy;
	}
	options: ITreeOptions = {
		childrenField: 'attributes',
		displayField: 'name',
		allowDrag: false,

	}
	get nodes(): singleNode[] | null {
		if (this.reconcilingReviewSet == null) return null;
		else return [this.reconcilingReviewSet];
	}
	CheckBoxClicked(checked: boolean, data: singleNode, ) {
	}
	AgreementClass(node: singleNode | ReconcilingSetAttribute): string {
		switch (this.IsNodeAgreement(node)) {
			case "":
				return "";
			case "Agr":
				return "alert-success";
			default:
				return "alert-danger"; 
		}
	}
	IsNodeAgreement(node: singleNode | ReconcilingSetAttribute): string {
		if (node.nodeType == "ReviewSet") return "";
		const data = node as ReconcilingSetAttribute;
		//console.log("IsNodeAgreement", data);
		if (!data.attribute_id) return "";
		if (data.Reviewer1Coding.length == 0 && data.Reviewer2Coding.length == 0 && data.Reviewer3Coding.length == 0) return "";
		if (data.Reviewer1Coding.length != data.Reviewer2Coding.length
			|| (this.CurrentComparison.contactId3 > 0 && data.Reviewer1Coding.length != data.Reviewer3Coding.length)
			|| (this.CurrentComparison.contactId3 > 0 && data.Reviewer2Coding.length != data.Reviewer3Coding.length)
		) return "Dis";
		for (let coding of data.Reviewer1Coding) {
			if (data.Reviewer2Coding.findIndex(f => f.ID == coding.ID && f.ArmID == coding.ArmID) == -1) return "Dis";
			if (this.CurrentComparison.contactId3 > 0) {
				if (data.Reviewer3Coding.findIndex(f => f.ID == coding.ID && f.ArmID == coding.ArmID) == -1) return "Dis";
            }
		}
		for (let coding of data.Reviewer2Coding) {
			if (data.Reviewer1Coding.findIndex(f => f.ID == coding.ID && f.ArmID == coding.ArmID) == -1) return "Dis";
			if (this.CurrentComparison.contactId3 > 0) {
				if (data.Reviewer3Coding.findIndex(f => f.ID == coding.ID && f.ArmID == coding.ArmID) == -1) return "Dis";
			}
		}
		if (this.CurrentComparison.contactId3 > 0) {
			for (let coding of data.Reviewer2Coding) {
				if (data.Reviewer1Coding.findIndex(f => f.ID == coding.ID && f.ArmID == coding.ArmID) == -1) return "Dis";
				if (data.Reviewer2Coding.findIndex(f => f.ID == coding.ID && f.ArmID == coding.ArmID) == -1) return "Dis";
			}
		}
		return "Agr";
    }

	NodeSelected(node: ReconcilingSetAttribute | ReconcilingReviewSet) {
		if (node.nodeType == "ReviewSet") {
			this.SelectedNode = null;
		}
		else {
			let renode = node as ReconcilingSetAttribute;
			if (renode) {
				this.SelectedNode = renode;
            }
        }
		//console.log("NodeSelected", node);
	}
	Complete(recItem: ReconcilingItem, contactId: number) {
		this.CompleteEvent.emit({ item: recItem, contactId: contactId });
	}
	CompleteAndLock(recItem: ReconcilingItem, contactId: number) {
		this.CompleteAndLockEvent.emit({ item: recItem, contactId: contactId });
	}
	UnComplete(recItem: ReconcilingItem) {
		this.UnCompleteEvent.emit(recItem);
    }
	public PreviousDisagreement() {
		//console.log("PreviousDisagreement");
		if (this.treeComponent) {
			let sel = this.SelectedNodeId();
			//console.log("PreviousDisagreement", sel);
			let Id: number | null = null;
			if (this.reconcilingReviewSet == null || this.reconcilingReviewSet.attributes.length == 0) return;
			else if (sel == null || sel.toString().startsWith("C_")) {
				Id = this.reconcilingReviewSet.attributes[0].attribute_id;
			}
			else if (sel != null && sel.toString().startsWith("A")) {
				Id = Helpers.SafeParseInt(sel.toString().substr(1));
			}
			if (Id != null) {
				this.NextPreviousDisagreementFromAttributeId(Id, false);
			}
		}
	}

	public FirstDisagreement() {
		if (this.reconcilingReviewSet && this._Disagreements.length > 0) {
			this.UpdateTreeSelection(this._Disagreements[0], this.reconcilingReviewSet);
		}

		//old code below, from when we did not have the list of disagreements, this code works, keeping it in case the list of disagreements is unreliable
		//if (this.treeComponent) {
		//	let Id: number | null = null;
		//	if (this.reconcilingReviewSet == null || this.reconcilingReviewSet.attributes.length == 0) return;
		//	else {
		//		Id = this.reconcilingReviewSet.attributes[0].attribute_id;
		//	}
		//	if (Id != null) {
		//		this.NextPreviousDisagreementFromAttributeId(Id, true, true);
		//	}
		//}
	}
	public LastDisagreement() {
		if (this.reconcilingReviewSet && this._Disagreements.length > 0) {
			this.UpdateTreeSelection(this._Disagreements[this._Disagreements.length - 1], this.reconcilingReviewSet);
        }

		//old code below, from when we did not have the list of disagreements, this code works, keeping it in case the list of disagreements is unreliable
		//if (this.treeComponent) {
		//	let Id: number | null = null;
		//	if (this.reconcilingReviewSet == null || this.reconcilingReviewSet.attributes.length == 0) return;
		//	else {//got to do some crawling to find the last attribute!
		//		let LastAtt = this.reconcilingReviewSet.attributes[this.reconcilingReviewSet.attributes.length - 1];//last of first level codes
		//		let counter: number = 0;
		//		while (Id == null && counter < 10000) {
		//			counter++;
		//			if (LastAtt.attributes.length == 0) {
		//				//no children to crawl, this is the att we want
		//				Id = LastAtt.attribute_id;
		//			} else {
		//				//this LastAtt is not the actual last, 'cause it has children...
		//				LastAtt = LastAtt.attributes[LastAtt.attributes.length - 1];//replace with the last child of the current "LastAtt", repeat...
  //                  }
  //              }
		//	}
		//	if (Id != null) {
		//		this.NextPreviousDisagreementFromAttributeId(Id, false, true);
		//	}
		//}
	}
	public NextDisagreement() {
		//console.log("NextDisagreement");
		if (this.treeComponent) {
			let sel = this.SelectedNodeId();
			//console.log("NextDisagreement", sel);
			let Id: number | null = null;
			let includeFirst: boolean = false;
			if (this.reconcilingReviewSet == null || this.reconcilingReviewSet.attributes.length == 0) return;
			else if (sel == null || sel.toString().startsWith("C_")) {
				Id = this.reconcilingReviewSet.attributes[0].attribute_id;
				includeFirst = true;
            }
			else if (sel != null && sel.toString().startsWith("A")) {
				Id = Helpers.SafeParseInt(sel.toString().substr(1));
			}
			if (Id != null) {
				this.NextPreviousDisagreementFromAttributeId(Id, true, includeFirst);
			}
        }
	}

	private NextPreviousDisagreementFromAttributeId(Id: number, searchForward: boolean, includeStartId: boolean = false) {
		if (this.reconcilingReviewSet) {
			let att: ReconcilingSetAttribute | null = null;
			const indOfDis = this._Disagreements.findIndex(f => f.attribute_id == Id);
			if (indOfDis == -1) {
				//this is the old code that does work, written before we started listing disagreements, we need it when a random code (not a disagreement) is selected...
				//it can be slow, so we use this _only_ when we have to
				if (includeStartId) att = this.reconcilingReviewSet.FindByIdNumber(Id);
				else if (searchForward) att = this.reconcilingReviewSet.FindNextByIdNumber(Id);
				else att = this.reconcilingReviewSet.FindPreviousByIdNumber(Id);
				let counter: number = 0;
				let compRes: string = "";
				while (att != null && compRes != "Dis" && counter < 10000) {//
					counter++;
					compRes = this.IsNodeAgreement(att);
					if (compRes != "Dis") {
						if (searchForward) att = this.reconcilingReviewSet.FindNextByIdNumber(att.attribute_id);
						else att = this.reconcilingReviewSet.FindPreviousByIdNumber(att.attribute_id);
					}
				}
			}
			else {
				if (includeStartId) att = this._Disagreements[indOfDis];//?? why would we? (go to first and last used this option, but now rely on the list of disagreements...)
				else if (searchForward) {
					if (indOfDis == this._Disagreements.length - 1) {//shouldn't happen, we already are on the last, no change
						att = this._Disagreements[indOfDis];
					}
					else att = this._Disagreements[indOfDis+1];
				}
				else {//looking for previous...
					if (indOfDis == 0) {//shouldn't happen, we already are on the first, no change
						att = this._Disagreements[indOfDis];
					}
					else att = this._Disagreements[indOfDis - 1];
                }
            }
			//console.log("NextDisagreement2", att);
			if (att != null) {
				this.UpdateTreeSelection(att, this.reconcilingReviewSet);
			}
		}
    }
	private UpdateTreeSelection(att: ReconcilingSetAttribute, rrs: ReconcilingReviewSet) {		
		const activeNodeIds: any = {};
		activeNodeIds[att.id] = true;
		this.NodesState = {
			...this.NodesState,
			activeNodeIds
		};
		this.NodeSelected(att);
		let expandTheseNodes = rrs.ParentsListByAttId(att.attribute_id);
		if (expandTheseNodes.length > 0) {
			let alreadyExpanded: any = this.NodesState.expandedNodeIds;
			if (alreadyExpanded != undefined) {
				for (let toExpand of expandTheseNodes) {
					let done: boolean = false;
					for (let key in alreadyExpanded) {
						if (toExpand.id == key) {
							//add this key to the list of expanded nodes
							alreadyExpanded[key] = true;
							done = true;
							break;
						}
					}
					if (done == false) {
						alreadyExpanded[toExpand.id] = true;
					}
				}
			}
			else {
				alreadyExpanded = {};
				for (let toExpand of expandTheseNodes) {
					alreadyExpanded[toExpand.id] = true;
				}
			}
			this.NodesState.expandedNodeIds = alreadyExpanded;
		}
		//scroll the node in view
		if (document) {
			const attId = att.id;
			setTimeout(() => {
				const element = document.getElementById(attId);
				if (element) element.scrollIntoView(false);
			}, 250);
		}		
    }
	public CodingWholeStudy(coding: ReconcilingCode[]): ReconcilingCode[] {
		if (this.LoadPDFCoding && this.ReconcilingItem && this.ItemDocsService._itemDocs.length > 0 && this.ReconcilingItem.Item.itemId == this.ItemDocsService.CurrentItemId) {
			for (let singleCoding of coding) {
				if (singleCoding.PDFCoding == null) {//if it's an empty array, it means we already asked for the data, and there was no data. Null means we never asked for this data.
					this.getPDFCoding(singleCoding, this.ItemDocsService._itemDocs);
				}
            }
        }
		return coding.filter(f => f.ArmID == 0);
	}
	private async getPDFCoding(singleCoding: ReconcilingCode, docs: ItemDocument[]) {
		singleCoding.PDFCoding = [];//no matter what, this ensures we ask for this data only once!
		for (let doc of this.ItemDocsService._itemDocs) {
			let req: ItemAttPDFCodingCrit = new ItemAttPDFCodingCrit(doc.itemDocumentId, singleCoding.ItemAttributeID);
			let res = await this.ItemCodingService.StandaloneFetchItemAttPDFCoding(req);
			if (typeof res != "boolean") {
				singleCoding.PDFCoding = res;// as ReconcilingCode[];
			} else singleCoding.PDFCoding = [];//something went wrong, but we can't keep asking!!
		}
	}
	PDFNameFromId(id: number): string {
		let ind = this.ItemDocsService._itemDocs.findIndex(f => f.itemDocumentId == id);
		if (ind > -1) return this.ItemDocsService._itemDocs[ind].title;
		else return "*Unknown*";
    }
	public CodingByArmId(coding: ReconcilingCode[], armId: number): ReconcilingCode[] {
		return coding.filter(f => f.ArmID == armId);
	}
	public HtmlPathForReconcilingSetAttribute(sa: ReconcilingSetAttribute): string {
		let res: string = "";
		if (this.reconcilingReviewSet) {
			let parents = this.reconcilingReviewSet.ParentsListByAttId(sa.attribute_id);
			for (let i = parents.length - 1; i > -1; i-- ) {//parents come from immediate parent towards the root
				res += "<i class='fa fa-arrow-right pt-1 mt-2 mx-1'></i>" + parents[i].attribute_name;
            }
		}
		res += "<i class='fa fa-arrow-right pt-1 mt-2 mx-1'></i>" + sa.attribute_name;
		return res;
	}

	OutcomeHTMLtable(data: Outcome[]): string {
		return this.ItemCodingService.OutcomesTable(data, true);
	}
	
	public DisagreementPosition(item: ReconcilingSetAttribute): number {
		if (this.SelectedNode !== null) {
			const curr = this.SelectedNode;
			return this._Disagreements.findIndex(f => f.id == curr.id);
		}
		else return -1
		
    }
	FindDisagreements() {
		console.log("FindDisagreements");
		if (!this.reconcilingReviewSet || this.reconcilingReviewSet.attributes.length < 1) return;
		else {
			console.log("FindDisagreements2");
			this.FindingDisagreements = true;
			this._Disagreements = [];
			let att: ReconcilingSetAttribute | null = this.reconcilingReviewSet.attributes[0];
			if (this.IsNodeAgreement(att) == "Dis") this._Disagreements.push(att);
			let counter: number = 0;
			while (counter < 50000 && att != null) {
				att = this.reconcilingReviewSet.FindNextByIdNumber(att.attribute_id);
				counter++;
				if (att == null) break;
				else {
					const cAtt = att;
					if (this.IsNodeAgreement(att) == "Dis" && this._Disagreements.findIndex(f => f.id == cAtt.id) == -1) this._Disagreements.push(att);
				}
			}
			this.FindingDisagreements = false;
        }
    }

	private SelectedNodeId(): string | null {
		if (this.SelectedNode == null) return null;
		else {
			return this.SelectedNode.id;
		}
		////console.log("SelectedNodeId", this.NodesState.selectedNodeIds, this.NodesState.activeNodeIds);
		//const Id = this.NodesState.activeNodeIds;
		////console.log("SelectedNodeId", Id);
		//let i: number = 0;
		//let Key: string = "";
		//if (Id) {
		//	for (let key in Id) {
		//		Key = key;
		//		break;
		//	}
		//}
		//if (Key != "") return Key;
		//else return null;
    }
	ngOnDestroy() {
		
	}
}





