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
import { OutcomesService } from '../services/outcomes.service';
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

	public showManualModal: boolean = false;

	sub: Subscription = new Subscription();

	public smallTree: string = '';

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
	private DeletingEvent: any;
	private DeletingData: singleNode | null = null;
	DeleteCodingConfirmed() {
		if (this.DeletingData) {
			this.DeletingData.isSelected = false;
			this.CheckBoxClickedAfterCheck(this.DeletingEvent, this.DeletingData);
		}
		this.DeletingEvent = undefined;
		this.DeletingData = null;
		this.showManualModal = false;
	}
	DeleteCodingCancelled() {
		//console.log('trying to close...')
		if (this.DeletingData) this.DeletingData.isSelected = true;
		this.DeletingEvent = undefined;
		this.DeletingData = null;
		this.showManualModal = false;
	}
	CheckBoxClicked(event: any, data: singleNode, ) {

		let checkPassed: boolean = true;
		if (event.target) checkPassed = event.target.checked;//if we ticked the checkbox, it's OK to carry on, otherwise we need to check
		if (!checkPassed) {
			//event.srcElement.blur();
			console.log('checking...');
			//deleting the codeset: need to confirm
			this.DeletingData = data;
			this.DeletingEvent = event;
			//all this seems necessary because I could not suppress the error discussed here:
			//https://blog.angularindepth.com/everything-you-need-to-know-about-the-expressionchangedafterithasbeencheckederror-error-e3fd9ce7dbb4
			this.showManualModal = true;
		}
		else this.CheckBoxClickedAfterCheck(event, data);
	}
	CheckBoxClickedAfterCheck(event: any, data: singleNode) {
		//let evdata: CheckBoxClickedEventData = new CheckBoxClickedEventData();
		//evdata.event = event;
		//evdata.armId = this._armsService.SelectedArm == null ? 0 : this._armsService.SelectedArm.itemArmId;
		//evdata.AttId = +data.id.replace('A', '');
		//console.log('AttID: ' + evdata.AttId + ' armid = ' + evdata.armId);
		//evdata.additionalText = data.additionalText;
		//this.ReviewSetsService.PassItemCodingCeckboxChangedEvent(evdata);
	}
	//CheckIfNodeIsSelected(data: singleNode) {

	//	// this needs to be for the current outcome !!!!!!!!!!!!!!!!!
	//	for (var i = 0; i < data.attributes.length; i++) {

	//		if (this._outcomeService.currentOutcome) {
	//			this._outcomeService.currentOutcome.outcomeCodes.outcomeItemAttributesList.forEach(function (value) {
	//			console.log(value);
	//			let attribute: SetAttribute = data.attributes[i] as SetAttribute;
	//				if (value.attributeId == attribute.attribute_id) {

	//					data.isSelected = true;
	//				} else {
	//					data.isSelected = false;
	//				}
	//			});
	//		}
	//	}		
	//}
	//Potentially need to change below with logic from above
	get nodes(): singleNode[] | null {

		if (this.ReviewSetsService && this.ReviewSetsService.ReviewSets && this.ReviewSetsService.ReviewSets.length > 0) {


			var RelevantCodingTool = this.ReviewSetsService.ReviewSets.filter(x => x.ItemSetId == this._outcomeService.ItemSetId);
			// this needs to be for the current outcome !!!!!!!!!!!!!!!!!
			for (var i = 0; i < RelevantCodingTool[0].attributes.length; i++) {
				console.log('got in here: ' + this._outcomeService.outcomesList[0].outcomeCodes.outcomeItemAttributesList.length);
				if (this._outcomeService.outcomesList[0]) {
					this._outcomeService.outcomesList[0].outcomeCodes.outcomeItemAttributesList.forEach(function (value) {
						console.log(value);
						let attribute: SetAttribute = RelevantCodingTool[0].attributes[i] as SetAttribute;
						console.log('got in here: ' + value.attributeId + ' : ' + attribute.attribute_id);
						if (value.attributeId == attribute.attribute_id) {

							
							RelevantCodingTool[0].attributes[i].isSelected = true;
						} else {
							RelevantCodingTool[0].attributes[i].isSelected = false;
						}
					});
				}
			}
			//console.log(RelevantCodingTool);
			return RelevantCodingTool;

		}//return this.ReviewSetsService.ReviewSets.filter(x => x.ItemSetId == this._outcomeService.ItemSetId);
		else {
			return null;
		}
	}

	rootsCollect() {

		const treeModel: TreeModel = this.treeComponent.treeModel;
		const firstNode: any = treeModel.getFirstRoot();

		var rootsArr: Array<ITreeNode> = [];

		for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

			rootsArr[i] = this.treeComponent.treeModel.roots[i];
			console.log(rootsArr[i]);
		}

	}

	nodesNotRootsCollect(node: ITreeNode) {

		const treeModel: TreeModel = this.treeComponent.treeModel;
		const firstNode: any = treeModel.getFirstRoot();

		var childrenArr: Array<any> = [];

		for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

			var test = this.treeComponent.treeModel.roots[i];

			childrenArr[i] = test.getVisibleChildren();
			console.log(childrenArr[i]);

		}
	}

	onEvent($event: any) {

		alert($event);

	}

	selectAllRoots() {

		const treeModel: TreeModel = this.treeComponent.treeModel;

		const firstNode: any = treeModel.getFirstRoot();

		for (var i = 0; i < this.treeComponent.treeModel.roots.length; i++) {

			this.treeComponent.treeModel.roots[i].setIsActive(false, true);

		}
	}

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





