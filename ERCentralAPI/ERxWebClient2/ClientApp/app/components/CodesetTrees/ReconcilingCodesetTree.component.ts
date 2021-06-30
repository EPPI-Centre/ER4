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
		private ReviewSetsService: ReviewSetsService

	) { }

	@ViewChild('SingleCodeSetTree1') treeComponent!: TreeComponent;

	@Input() reconcilingReviewSet: ReconcilingReviewSet | null = null;
	@Input() CurrentComparison: Comparison = new Comparison();
	@Input() ReconcilingItem: ReconcilingItem | null = null;

	@Output() CompleteEvent = new EventEmitter<{ item: ReconcilingItem, contactId: number }>();
	@Output() CompleteAndLockEvent = new EventEmitter<{ item: ReconcilingItem, contactId: number }>();
	@Output() UnCompleteEvent = new EventEmitter<ReconcilingItem>();

	NodesState: ITreeState = {};// see https://angular2-tree.readme.io/docs/save-restore

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

	NodeSelected(node: singleNode) {
		//this.ReviewSetsService.selectedNode = node;
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
		console.log("PreviousDisagreement");
		if (this.treeComponent) {
			let sel = this.SelectedNodeId();
			console.log("PreviousDisagreement", sel);
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
		if (this.treeComponent) {
			let Id: number | null = null;
			if (this.reconcilingReviewSet == null || this.reconcilingReviewSet.attributes.length == 0) return;
			else {
				Id = this.reconcilingReviewSet.attributes[0].attribute_id;
			}
			if (Id != null) {
				this.NextPreviousDisagreementFromAttributeId(Id, true, true);
			}
		}
	}
	public LastDisagreement() {
		if (this.treeComponent) {
			let Id: number | null = null;
			if (this.reconcilingReviewSet == null || this.reconcilingReviewSet.attributes.length == 0) return;
			else {//got to do some crawling to find the last attribute!
				let LastAtt = this.reconcilingReviewSet.attributes[this.reconcilingReviewSet.attributes.length - 1];//last of first level codes
				let counter: number = 0;
				while (Id == null && counter < 10000) {
					counter++;
					if (LastAtt.attributes.length == 0) {
						//no children to crawl, this is the att we want
						Id = LastAtt.attribute_id;
					} else {
						//this LastAtt is not the actual last, 'cause it has children...
						LastAtt = LastAtt.attributes[LastAtt.attributes.length - 1];//replace with the last child of the current "LastAtt", repeat...
                    }
                }
			}
			if (Id != null) {
				this.NextPreviousDisagreementFromAttributeId(Id, false, true);
			}
		}
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
			//console.log("NextDisagreement2", att);
			if (att != null) {
				const activeNodeIds: any = {};
				activeNodeIds[att.id] = true;
				this.NodesState = {
					...this.NodesState,
					activeNodeIds
				};
				let expandTheseNodes = this.reconcilingReviewSet.ParentsListByAttId(att.attribute_id);
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
			}
		}
    }

	private SelectedNodeId(): string | null {
		//console.log("SelectedNodeId", this.NodesState.selectedNodeIds, this.NodesState.activeNodeIds);
		const Id = this.NodesState.activeNodeIds;
		//console.log("SelectedNodeId", Id);
		let i: number = 0;
		let Key: string = "";
		if (Id) {
			for (let key in Id) {
				Key = key;
				break;
			}
		}
		if (Key != "") return Key;
		else return null;
    }
	ngOnDestroy() {
		
	}
}





