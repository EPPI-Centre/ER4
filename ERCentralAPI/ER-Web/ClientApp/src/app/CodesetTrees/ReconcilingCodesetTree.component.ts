import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, AfterViewInit, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode } from '../services/ReviewSets.service';
import { ITreeOptions,  TreeComponent, ITreeState } from '@circlon/angular-tree-component';
import {  Outcome } from '../services/outcomes.service';
import {  ReconcilingReviewSet, ReconcilingSetAttribute, ReconcilingCode, ReconcilingItem, ReconciliationService } from '../services/reconciliation.service';
import { Comparison } from '../services/comparisons.service';
import { Helpers } from '../helpers/HelperMethods';
import { ItemCodingService, ItemAttPDFCodingCrit } from '../services/ItemCoding.service';
import { ItemDocsService, ItemDocument } from '../services/itemdocs.service';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

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
		private ReconciliationService: ReconciliationService,
		private ConfirmationDialogService: ConfirmationDialogService,
		private ReviewerIdentityServ: ReviewerIdentityService,
		private ReviewSetsService: ReviewSetsService,
		private ItemCodingService: ItemCodingService,
		private ItemDocsService: ItemDocsService
	) { }

	@ViewChild('SingleCodeSetTree1') treeComponent1!: TreeComponent; 
	@ViewChild('SingleCodeSetTree2') treeComponent2!: TreeComponent;
	@ViewChild('SingleCodeSetTree3') treeComponent3!: TreeComponent;
	private _reconcilingReviewSet: ReconcilingReviewSet | null = null;
	private UpdatingSingleItem: boolean = false;
	@Input() public set reconcilingReviewSet(rrs: ReconcilingReviewSet | null) {
		this._reconcilingReviewSet = rrs;
		if (this.UpdatingSingleItem) {
			console.log("set reconcilingReviewSet2!");
			this.FindDisagreements();
			if (this.reconcilingReviewSet && this.SelectedNode) this.SelectedNode = this.reconcilingReviewSet.FindByIdNumber(this.SelectedNode.attribute_id);
			this.UpdatingSingleItem = false;
        }
    }

	public get reconcilingReviewSet(): ReconcilingReviewSet | null {
		return this._reconcilingReviewSet;
    }
	@Input() CurrentComparison: Comparison = new Comparison();
	@Input() HasWriteRights: boolean = false;//to avoid having to use the ReviewerIdentityService
	@Input() HasAdminRights: boolean = false;//ditto. If not, any "on the fly" editing of coding is disabled.
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
	@Output() PleaseUpdateCurrentItem = new EventEmitter<void>();

	NodesState: ITreeState = {};// see https://angular2-tree.readme.io/docs/save-restore
	SelectedNode: ReconcilingSetAttribute | null = null;
	LoadPDFCoding: boolean = false;
	ShowOutcomes: boolean = false; 
	FindingDisagreements: boolean = false;
	ShowingTransferPanelForCoding: ReconcilingCode | null = null;
	TransferringToReviewer: string = "reviewer2";//this is the key that drives who we are transferring coding to. All logic should refer to this value...
	public get TransferringToReviewerName(): string {
		if (!this.CurrentComparison) return "N/A";
		if (this.TransferringToReviewer == "reviewer1") return this.CurrentComparison.contactName1;
		else if (this.TransferringToReviewer == "reviewer2") return this.CurrentComparison.contactName2;
		else if (this.TransferringToReviewer == "reviewer3") return this.CurrentComparison.contactName3;
		else return "N/A";
    }
	public get TransferringToReviewerId(): number | null {
		if (!this.CurrentComparison) return null;
		if (this.TransferringToReviewer == "reviewer1") return this.CurrentComparison.contactId1;
		else if (this.TransferringToReviewer == "reviewer2") return this.CurrentComparison.contactId2;
		else if (this.TransferringToReviewer == "reviewer3") return this.CurrentComparison.contactId3;
		else return null;
	}
	private _Disagreements: ReconcilingSetAttribute[] = [];
	public get Disagreements(): ReconcilingSetAttribute[] {
		return this._Disagreements;
	}
	private _EditAcess: number = -1;
	public get EditAcess(): number {
		//0 = read only, 1 = accept other people's coding, 2 move coding freely
		if (this._EditAcess == -1) {
			if (!this.HasWriteRights) this._EditAcess = 0;
			else if (this.HasAdminRights) this._EditAcess = 2;
			else this._EditAcess = 1;
		}
		return this._EditAcess;
    }
	public get CurrentContactId(): number {
		return this.ReviewerIdentityServ.reviewerIdentity.userId;
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
	CanMoveCodesFromHere(reviewerSlot: number):boolean {
		if (this.EditAcess == 0) return false;
		else if (this.EditAcess == 1) {
			//user can "accept" someone else's codes (doesn't have admin rights) so can't transfer their own to anyone else...
			if (reviewerSlot == 1) {
				if (this.ReviewerIdentityServ.reviewerIdentity.userId == this.CurrentComparison.contactId1) return false;
				else return (this.ReviewerIdentityServ.reviewerIdentity.userId == this.CurrentComparison.contactId2
					|| this.ReviewerIdentityServ.reviewerIdentity.userId == this.CurrentComparison.contactId3);//we also check the current user is in the comparison!
			}
			else if (reviewerSlot == 2) {
				if (this.ReviewerIdentityServ.reviewerIdentity.userId == this.CurrentComparison.contactId2) return false;
				else return (this.ReviewerIdentityServ.reviewerIdentity.userId == this.CurrentComparison.contactId1
					|| this.ReviewerIdentityServ.reviewerIdentity.userId == this.CurrentComparison.contactId3);//we also check the current user is in the comparison!
			}
			else {//must be slot #3...
				if (this.ReviewerIdentityServ.reviewerIdentity.userId == this.CurrentComparison.contactId3) return false;
				else return (this.ReviewerIdentityServ.reviewerIdentity.userId == this.CurrentComparison.contactId2
					|| this.ReviewerIdentityServ.reviewerIdentity.userId == this.CurrentComparison.contactId1);//we also check the current user is in the comparison!
			} 
		}
		return true;//must have admin rights...
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
  Complete(recItem: ReconcilingItem | undefined, contactId: number) {
    if (recItem) {
      this.ShowingTransferPanelForCoding = null;
      this.CompleteEvent.emit({ item: recItem, contactId: contactId });
    }
  }
  CompleteAndLock(recItem: ReconcilingItem | undefined, contactId: number) {
    if (recItem) {
      this.ShowingTransferPanelForCoding = null;
      this.CompleteAndLockEvent.emit({ item: recItem, contactId: contactId });
    }
  }
  UnComplete(recItem: ReconcilingItem | undefined) {
    if (recItem) {
      this.UnCompleteEvent.emit(recItem);
    }
  }
	public PreviousDisagreement() {
		//console.log("PreviousDisagreement");
		if (this.treeComponent1) {
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
		if (this.treeComponent1) {
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
				res += "<i class='fa small text-primary fa-arrow-right pt-1 mt-2 mx-1'></i><span class='font-italic'>" + parents[i].attribute_name + "</span>";
            }
		}
		res += "<i class='fa small text-primary fa-arrow-right pt-1 mt-2 mx-1'></i>" + sa.attribute_name;
		return res;
	}

	OutcomeHTMLtable(data: Outcome[]): string {
		return this.ItemCodingService.OutcomesTable(data, true);
	}
	
	public DisagreementPosition(): number {
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
	}
	OpenTransferDialog(coding: ReconcilingCode, from: string) {
		if (this.EditAcess == 0 || this.CurrentComparison.comparisonId == 0) return;
		else if (this.EditAcess == 1) {//user can only transfer to herself...
			if (this.CurrentComparison.contactId3 < 1) {//2 people in comparison
				if (this.CurrentContactId == this.CurrentComparison.contactId1 && from != "reviewer1") {
					this.TransferringToReviewer = "Reviewer1";
				}
				else if (this.CurrentContactId == this.CurrentComparison.contactId2 && from != "reviewer2") {
					this.TransferringToReviewer = "reviewer2";
				}
				else return;//we can't transfer to anyone
			}
			else {//3 people in comparison
				if (this.CurrentContactId == this.CurrentComparison.contactId1 && from != "reviewer1") {
					this.TransferringToReviewer = "reviewer1";
				}
				else if (this.CurrentContactId == this.CurrentComparison.contactId2 && from != "reviewer2") {
					this.TransferringToReviewer = "reviewer2";
				}
				else if (this.CurrentContactId == this.CurrentComparison.contactId3 && from != "reviewer3") {
					this.TransferringToReviewer = "reviewer3";
				}
				else return;//we can't transfer to anyone
			}
		}
		else if (this.EditAcess == 2) {//user can transfer to anybody...
			if (this.CurrentComparison.contactId3 < 1) {//2 people in comparison
				if (from == "reviewer1") {
					this.TransferringToReviewer = "reviewer2";
				}
				else if (from == "reviewer2") {
					this.TransferringToReviewer = "reviewer1";
				}
				else return;//we can't transfer to anyone
			}
			else {//3 people in comparison
				if (from == "reviewer1") {
					this.TransferringToReviewer = "reviewer2";
				}
				else if (from == "reviewer2") {
					this.TransferringToReviewer = "reviewer1";
				}
				else if (from == "reviewer3") {
					this.TransferringToReviewer = "reviewer2";
				}
				else return;//we can't transfer to anyone
			}
		}
		else return;
		this.ShowingTransferPanelForCoding = coding;
    }
	ConfirmTransferACode(node: ReconcilingSetAttribute | null, rc: ReconcilingCode, from:string) {
		
		if (!this.HasWriteRights || node == null) return;
		else {
			let title = "Copy coding to: ";
			let destName = "";
			let msg: string = "";
			//let msg: string = "Are you sure? <br />";
			let destId: number = 0;
			if (this.TransferringToReviewer == "reviewer1") {
				destName = this.CurrentComparison.contactName1;
				title += destName;
				destId = this.CurrentComparison.contactId1;
			}
			else if (this.TransferringToReviewer == "reviewer2") {
				destName = this.CurrentComparison.contactName2;
				title += destName;
				destId = this.CurrentComparison.contactId2;
			}
			else if (this.TransferringToReviewer == "reviewer3") {
				destName = this.CurrentComparison.contactName3;
				title += destName;
				destId = this.CurrentComparison.contactId3;
			}
			else return;
			if (this.LoadPDFCoding) {
				msg += "This will merge also the PDF coding (overwriting text selections only if they clash on a <strong class='font-danger'>per-page basis</strong>).<br />";
			} else {
				msg += "This will <strong class='font-danger'>not copy the PDF coding</strong> (please tick \"Show PDF Coding?\" first, if you want to see and transfer PDF coding).<br />";
			}
			msg += "You will copy the coding for the <strong class='border border-success text-success px-1 mb-1 rounded d-inline-block'>" + rc.Name + (rc.ArmID == 0 ? "" : " (Arm: " + rc.ArmName + ")")
				+ "</strong> code, to the coding of <strong class='border border-info text-info px-1 mb-1 rounded d-inline-block'>"
				+ destName + "</strong>. <br />";
			msg += "<div class='m-1 p-1 alert-danger rounded'><i class='fa fa-warning'></i> This will (irreversibly) overwrite the coding (including Info box text) on the destination version (if any).</div>"
			this.ConfirmationDialogService.confirm(title, msg, false, "", "Copy coding", "Cancel", "lg").then(
				(res: any) => { if (res == true) this.DoTheTransfer(destId, node, rc, from); }
			);
        }
	}
	private DoTheTransfer(destId: number, node: ReconcilingSetAttribute, rc: ReconcilingCode, from: string) {
		let srcContId: number = this.CurrentComparison.contactId1;
		if (from == "reviewer2") {
			srcContId = this.CurrentComparison.contactId2;
		} else if (from == "reviewer3") {
			srcContId = this.CurrentComparison.contactId3;
		}
		let cmd = {
			destinationContactId: destId,
			sourceContactId: srcContId,
			attributeSetId: node.attributeSetId,
			comparisonId: this.CurrentComparison.comparisonId,
			includePDFcoding: this.LoadPDFCoding,
			setId: node.set_id,
			itemId: this.ReconcilingItem != undefined ? this.ReconcilingItem.Item.itemId : 0,
			itemArmId: rc.ArmID,

			result: "",
			itemAttributeId: 0,
			itemSetId: 0
		}
		this.ReconciliationService.TransferSingleCoding(cmd).then(
			(res) => {
				if (res == true) {
					//alert("this worked");
					//this._lastReconcilingItemId = -1;//this ensures the page will react reloading all data when the current item gets updated.
					//if (this._ReconcilingItem) this._ReconcilingItem.Item.itemId = this._lastReconcilingItemId;//as above, we need both!
					this.UpdatingSingleItem = true;
					this.PleaseUpdateCurrentItem.emit();
				}
				//else alert("nope");
            }
		)
	}
	public TextForTransferringCoding(rc: ReconcilingCode): string {
		//if (this.SelectedNode) console.log("don't understand...", this.SelectedNode.Reviewer1Coding);
		//if (rc == undefined) return "wtf?";
		return rc.ArmID == 0 ? "Whole study" : rc.ArmName;
    }
	CanTransferTheCurrentCodingTo(reviewerN: number): boolean {
		return true;
    }

	ngOnDestroy() {
		
	}
}





