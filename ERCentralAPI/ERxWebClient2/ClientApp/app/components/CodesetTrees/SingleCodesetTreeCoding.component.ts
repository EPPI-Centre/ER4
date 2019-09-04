import { Component, Inject, OnInit, Output, Input, ViewChild, OnDestroy, ElementRef, AfterViewInit, ViewContainerRef } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { forEach } from '@angular/router/src/utils/collection';
import { ReviewerIdentityService } from '../services/revieweridentity.service';
import { Router } from '@angular/router';
import { ReviewSetsService, singleNode, ReviewSet, SetAttribute, iAttributeSet } from '../services/ReviewSets.service';
import { ITreeOptions, TreeModel, TreeComponent } from 'angular-tree-component';
import { NgbModal, NgbActiveModal, NgbTabset } from '@ng-bootstrap/ng-bootstrap';
import { ArmsService } from '../services/arms.service';
import { ITreeNode } from 'angular-tree-component/dist/defs/api';
import { frequenciesService } from '../services/frequencies.service';
import { Injectable, EventEmitter } from '@angular/core';
import { Subscription } from 'rxjs';
import { EventEmitterService } from '../services/EventEmitter.service';
import { OutcomesService, OutcomeItemAttribute, Outcome } from '../services/outcomes.service';
import { CheckBoxClickedEventData } from './codesetTreeCoding.component';

@Component({
	selector: 'SingleCodesetTreeCoding',
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
        `],
	templateUrl: './SingleCodesetTreeCoding.component.html'
})

export class SingleCodesetTreeCodingComponent implements OnInit, OnDestroy, AfterViewInit {
	constructor(private router: Router,
		private _httpC: HttpClient,
		@Inject('BASE_URL') private _baseUrl: string,
		private ReviewerIdentityServ: ReviewerIdentityService,
		private ReviewSetsService: ReviewSetsService,
		private _outcomeService: OutcomesService,
		private _armsService: ArmsService

	) { }

	@ViewChild('SingleCodeSetTree') treeComponent!: TreeComponent;

	@Input() tabSelected: string = '';
	@Input() MaxHeight: number = 800;
	@Input() currentOutcome: Outcome = new Outcome();

	public showManualModal: boolean = false;

	sub: Subscription = new Subscription();
	
	ngOnInit() {

		if (this.ReviewerIdentityServ.reviewerIdentity.userId == 0 || this.ReviewerIdentityServ.reviewerIdentity.reviewId == 0) {
			this.router.navigate(['home']);
		}
	}
	public get IsServiceBusy(): boolean {
		return this.ReviewSetsService.IsBusy;
	}
	options: ITreeOptions = {
		childrenField: 'attributes',
		displayField: 'name',
		allowDrag: false,

	}

	ngAfterViewInit() {

	}

	CheckBoxClicked(checked: boolean, data: singleNode, ) {
		if (data.nodeType != "SetAttribute" || this.currentOutcome.itemSetId < 1) return;

		let Att = data as SetAttribute;
		let index = this.currentOutcome.outcomeCodes.outcomeItemAttributesList.findIndex(found => found.attributeId == Att.attribute_id);
		if (checked) {
			if (index == -1) {
				//add it to outcomeCodes.outcomeItemAttributesList
				let outcomeItemAttribute: OutcomeItemAttribute = {
					outcomeItemAttributeId: 0,
					outcomeId: this.currentOutcome.outcomeId,
					attributeId: Att.attribute_id,
					additionalText: "",
					attributeName: Att.attribute_name
				};
				this.currentOutcome.outcomeCodes.outcomeItemAttributesList.push(outcomeItemAttribute);
			}
			else {
				//uh? It's there already!
				console.log("didn't add attribute to outcome - was already there.", Att);
			}
		}
		else {//splice
			if (index == -1) {
				//uh? It's not there already!
				console.log("didn't remove attribute to outcome - wasn't already there.", Att);
			}
			else {
				this.currentOutcome.outcomeCodes.outcomeItemAttributesList.splice(index, 1);
			}
		}
	}
	
	IsAttributeInOutcome(data: singleNode): boolean {
		if (data.nodeType != "SetAttribute") return false; //check this! || this._outcomeService.currentOutcome.itemSetId < 1
		let Att = data as SetAttribute;
		let index = this.currentOutcome.outcomeCodes.outcomeItemAttributesList.findIndex(found => found.attributeId == Att.attribute_id);
		if (index < 0) return false;
		else return true;
	}
	
	get nodes(): singleNode[] | null {

		if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) {

			return this.ReviewSetsService.ReviewSets.filter(x => x.ItemSetId == this._outcomeService.ItemSetId);
			//return this.ReviewSetsService.ReviewSets.filter(x => x.ItemSetId == this._outcomeService.ItemSetId);
		}
		else {
			return null;
		}
	}

	//rootsCollect() {

	//	const treeModel: TreeModel = this.treeComponent.treeModel;
	//	const firstNode: any = treeModel.getFirstRoot();

	//	var rootsArr: Array<ITreeNode> = [];

	//	for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

	//		rootsArr[i] = this.treeComponent.treeModel.roots[i];
	//		console.log(rootsArr[i]);
	//	}
	//}

	//nodesNotRootsCollect(node: ITreeNode) {

	//	const treeModel: TreeModel = this.treeComponent.treeModel;
	//	const firstNode: any = treeModel.getFirstRoot();

	//	var childrenArr: Array<any> = [];

	//	for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

	//		var test = this.treeComponent.treeModel.roots[i];

	//		childrenArr[i] = test.getVisibleChildren();
	//		console.log(childrenArr[i]);

	//	}
	//}

	//onEvent($event: any) {

	//	alert($event);

	//}

	//selectAllRoots() {

	//	const treeModel: TreeModel = this.treeComponent.treeModel;

	//	const firstNode: any = treeModel.getFirstRoot();

	//	for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

	//		this.treeComponent.treeModel.roots[i].setIsActive(false, true);

	//	}
	//}

	public SelectedNodeData: singleNode | null = null;
	public get SelectedCodeDescription(): string {
		return this.ReviewSetsService.SelectedCodeDescription;
	}

	NodeSelected(node: singleNode) {

			this.ReviewSetsService.selectedNode = node;
	}

	ngOnDestroy() {

		this.sub.unsubscribe();

	}
}





